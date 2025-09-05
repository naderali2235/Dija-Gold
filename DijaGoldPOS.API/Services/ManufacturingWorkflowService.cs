using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Repositories;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IServices;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using DijaGoldPOS.API.Models.ManfacturingModels;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service for managing manufacturing workflow transitions and approvals
/// </summary>
public class ManufacturingWorkflowService : IManufacturingWorkflowService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ManufacturingWorkflowService> _logger;
    private readonly IProductOwnershipService _productOwnershipService;

    public ManufacturingWorkflowService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<ManufacturingWorkflowService> logger,
        IProductOwnershipService productOwnershipService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
        _productOwnershipService = productOwnershipService;
    }

    /// <summary>
    /// Transitions a manufacturing record to the next workflow step
    /// </summary>
    public async Task<bool> TransitionWorkflowAsync(int productManufactureId, string targetStatus, string? notes = null)
    {
        try
        {
            var manufacture = await _unitOfWork.Repository<ProductManufacture>().GetByIdAsync(productManufactureId);
            if (manufacture == null)
            {
                throw new InvalidOperationException($"Manufacturing record {productManufactureId} not found");
            }

            var currentUserId = _currentUserService.UserId ?? "system";
            var currentUserName = _currentUserService.UserName ?? "System";

            var fromStatus = manufacture.Status;
            var fromWorkflowStep = manufacture.WorkflowStep;

            // Validate transition
            if (!IsValidTransition(fromStatus, targetStatus))
            {
                throw new InvalidOperationException($"Invalid transition from {fromStatus} to {targetStatus}");
            }

            // Update status and workflow step
            manufacture.Status = targetStatus;
            manufacture.WorkflowStep = GetWorkflowStepForStatus(targetStatus);

            // Handle specific status transitions
            switch (targetStatus)
            {
                case "InProgress":
                    manufacture.ActualCompletionDate = null; // Reset completion date
                    break;

                case "QualityCheck":
                    manufacture.QualityCheckStatus = "Pending";
                    manufacture.QualityCheckedByUserId = null;
                    manufacture.QualityCheckDate = null;
                    break;

                case "Approved":
                    manufacture.FinalApprovalStatus = "Approved";
                    manufacture.FinalApprovedByUserId = currentUserId;
                    manufacture.FinalApprovalDate = DateTime.UtcNow;
                    manufacture.FinalApprovalNotes = notes;
                    break;

                case "Completed":
                    manufacture.ActualCompletionDate = DateTime.UtcNow;
                    manufacture.Status = "Completed";
                    manufacture.WorkflowStep = "Complete";

                    // Automatically create full ownership for manufactured goods
                    try
                    {
                        var manufacturedWeight = manufacture.ConsumedWeight - manufacture.WastageWeight;
                        if (manufacturedWeight < 0) manufacturedWeight = 0;
                        var ownershipRequest = new ProductOwnershipRequest
                        {
                            ProductId = manufacture.ProductId,
                            BranchId = manufacture.BranchId,
                            SupplierId = null, // Manufactured internally
                            PurchaseOrderId = null,
                            CustomerPurchaseId = null,
                            TotalQuantity = manufacture.QuantityProduced,
                            TotalWeight = manufacturedWeight,
                            OwnedQuantity = manufacture.QuantityProduced,
                            OwnedWeight = manufacturedWeight,
                            TotalCost = manufacture.TotalManufacturingCost,
                            AmountPaid = manufacture.TotalManufacturingCost // fully owned by company
                        };

                        await _productOwnershipService.CreateOrUpdateOwnershipAsync(ownershipRequest, currentUserId);
                        _logger.LogInformation("Created ownership for manufactured product {ProductId} at branch {BranchId}. Qty: {Qty}, Cost: {Cost}",
                            manufacture.ProductId, manufacture.BranchId, manufacture.QuantityProduced, manufacture.TotalManufacturingCost);
                    }
                    catch (Exception ownEx)
                    {
                        // Log but do not fail the workflow transition
                        _logger.LogError(ownEx, "Failed to create ownership on manufacturing completion for ProductManufacture {Id}", productManufactureId);
                    }
                    break;

                case "Rejected":
                    manufacture.RejectionReason = notes;
                    break;
            }

            manufacture.ModifiedAt = DateTime.UtcNow;

            // Record workflow history
            var history = new ManufacturingWorkflowHistory
            {
                ProductManufactureId = productManufactureId,
                FromStatus = fromStatus,
                ToStatus = targetStatus,
                Action = GetActionForTransition(fromStatus, targetStatus),
                ActionByUserId = currentUserId,
                ActionByUserName = currentUserName,
                Notes = notes
            };

            await _unitOfWork.Repository<ManufacturingWorkflowHistory>().AddAsync(history);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Workflow transitioned for manufacturing {Id}: {From} -> {To} by {User}",
                productManufactureId, fromStatus, targetStatus, currentUserName);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning workflow for manufacturing {Id}", productManufactureId);
            throw;
        }
    }

    /// <summary>
    /// Performs quality check on a manufacturing record
    /// </summary>
    public async Task<bool> PerformQualityCheckAsync(int productManufactureId, bool passed, string? notes = null)
    {
        try
        {
            var manufacture = await _unitOfWork.Repository<ProductManufacture>().GetByIdAsync(productManufactureId);
            if (manufacture == null)
            {
                throw new InvalidOperationException($"Manufacturing record {productManufactureId} not found");
            }

            if (manufacture.Status != "QualityCheck")
            {
                throw new InvalidOperationException("Manufacturing record is not in quality check status");
            }

            var currentUserId = _currentUserService.UserId ?? "system";
            var currentUserName = _currentUserService.UserName ?? "System";

            manufacture.QualityCheckStatus = passed ? "Passed" : "Failed";
            manufacture.QualityCheckedByUserId = currentUserId;
            manufacture.QualityCheckDate = DateTime.UtcNow;
            manufacture.QualityCheckNotes = notes;

            // If quality check passed, move to approved status
            if (passed)
            {
                await TransitionWorkflowAsync(productManufactureId, "Approved", notes);
            }
            else
            {
                await TransitionWorkflowAsync(productManufactureId, "Rejected", notes);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing quality check for manufacturing {Id}", productManufactureId);
            throw;
        }
    }

    /// <summary>
    /// Performs final approval on a manufacturing record
    /// </summary>
    public async Task<bool> PerformFinalApprovalAsync(int productManufactureId, bool approved, string? notes = null)
    {
        try
        {
            var manufacture = await _unitOfWork.Repository<ProductManufacture>().GetByIdAsync(productManufactureId);
            if (manufacture == null)
            {
                throw new InvalidOperationException($"Manufacturing record {productManufactureId} not found");
            }

            if (manufacture.Status != "Approved")
            {
                throw new InvalidOperationException("Manufacturing record is not in approved status");
            }

            var targetStatus = approved ? "Completed" : "Rejected";
            return await TransitionWorkflowAsync(productManufactureId, targetStatus, notes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing final approval for manufacturing {Id}", productManufactureId);
            throw;
        }
    }

    /// <summary>
    /// Gets workflow history for a manufacturing record
    /// </summary>
    public async Task<IEnumerable<ManufacturingWorkflowHistory>> GetWorkflowHistoryAsync(int productManufactureId)
    {
        try
        {
            return await _unitOfWork.Repository<ManufacturingWorkflowHistory>()
                .FindAsync(h => h.ProductManufactureId == productManufactureId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow history for manufacturing {Id}", productManufactureId);
            throw;
        }
    }

    /// <summary>
    /// Gets available transitions for current status
    /// </summary>
    public IEnumerable<string> GetAvailableTransitions(string currentStatus)
    {
        return currentStatus switch
        {
            "Draft" => new[] { "InProgress", "Cancelled" },
            "InProgress" => new[] { "QualityCheck", "Rejected", "Cancelled" },
            "QualityCheck" => new[] { "Approved", "Rejected" },
            "Approved" => new[] { "Completed", "Rejected" },
            "Rejected" => new[] { "InProgress" }, // Can restart from rejected
            "Cancelled" => Array.Empty<string>(),
            "Completed" => Array.Empty<string>(),
            _ => Array.Empty<string>()
        };
    }

    /// <summary>
    /// Validates if a transition is allowed
    /// </summary>
    private bool IsValidTransition(string fromStatus, string toStatus)
    {
        var availableTransitions = GetAvailableTransitions(fromStatus);
        return availableTransitions.Contains(toStatus);
    }

    /// <summary>
    /// Gets workflow step for a status
    /// </summary>
    private string GetWorkflowStepForStatus(string status)
    {
        return status switch
        {
            "Draft" => "Draft",
            "InProgress" => "Manufacturing",
            "QualityCheck" => "QualityControl",
            "Approved" => "FinalApproval",
            "Completed" => "Complete",
            "Rejected" => "Rejected",
            "Cancelled" => "Cancelled",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets action description for a transition
    /// </summary>
    private string GetActionForTransition(string fromStatus, string toStatus)
    {
        return $"{fromStatus} -> {toStatus}";
    }
}
