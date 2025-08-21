using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service for repair job operations and workflow management
/// </summary>
public class RepairJobService : IRepairJobService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<RepairJobService> _logger;

    public RepairJobService(
        ApplicationDbContext context,
        IAuditService auditService,
        ILogger<RepairJobService> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new repair job from a repair transaction
    /// </summary>
    public async Task<(bool IsSuccess, string? ErrorMessage, RepairJobDto? RepairJob)> CreateRepairJobAsync(CreateRepairJobRequestDto request, string userId)
    {
        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Verify transaction exists and is a repair transaction
            var transaction = await _context.Transactions
                .Include(t => t.Customer)
                .Include(t => t.Branch)
                .FirstOrDefaultAsync(t => t.Id == request.TransactionId && t.TransactionType == TransactionType.Repair);

            if (transaction == null)
            {
                return (false, "Repair transaction not found", null);
            }

            // Check if repair job already exists for this transaction
            var existingJob = await _context.RepairJobs
                .FirstOrDefaultAsync(rj => rj.TransactionId == request.TransactionId);

            if (existingJob != null)
            {
                return (false, "Repair job already exists for this transaction", null);
            }

            // Create repair job
            var repairJob = new RepairJob
            {
                TransactionId = request.TransactionId,
                Status = RepairStatus.Pending,
                Priority = request.Priority,
                AssignedTechnicianId = request.AssignedTechnicianId,
                TechnicianNotes = request.TechnicianNotes,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.RepairJobs.AddAsync(repairJob);
            await _context.SaveChangesAsync();

            await dbTransaction.CommitAsync();

            // Log audit
            await _auditService.LogAsync(
                userId,
                "CREATE_REPAIR_JOB",
                "RepairJob",
                repairJob.Id.ToString(),
                $"Created repair job for transaction {transaction.TransactionNumber}",
                newValues: System.Text.Json.JsonSerializer.Serialize(new { 
                    TransactionId = repairJob.TransactionId,
                    Priority = repairJob.Priority,
                    AssignedTechnicianId = repairJob.AssignedTechnicianId
                }),
                branchId: transaction.BranchId,
                transactionId: transaction.Id
            );

            // Map to DTO
            var repairJobDto = await MapToRepairJobDtoAsync(repairJob);

            _logger.LogInformation("Repair job {RepairJobId} created for transaction {TransactionNumber} by user {UserId}", 
                repairJob.Id, transaction.TransactionNumber, userId);

            return (true, null, repairJobDto);
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            _logger.LogError(ex, "Error creating repair job for transaction {TransactionId}", request.TransactionId);
            return (false, "An error occurred while creating the repair job", null);
        }
    }

    /// <summary>
    /// Get repair job by ID
    /// </summary>
    public async Task<RepairJobDto?> GetRepairJobAsync(int repairJobId)
    {
        var repairJob = await _context.RepairJobs
            .Include(rj => rj.Transaction)
                .ThenInclude(t => t.Customer)
            .Include(rj => rj.Transaction)
                .ThenInclude(t => t.Branch)
            .Include(rj => rj.AssignedTechnician)
            .Include(rj => rj.QualityChecker)
            .FirstOrDefaultAsync(rj => rj.Id == repairJobId);

        return repairJob != null ? await MapToRepairJobDtoAsync(repairJob) : null;
    }

    /// <summary>
    /// Get repair job by transaction ID
    /// </summary>
    public async Task<RepairJobDto?> GetRepairJobByTransactionIdAsync(int transactionId)
    {
        var repairJob = await _context.RepairJobs
            .Include(rj => rj.Transaction)
                .ThenInclude(t => t.Customer)
            .Include(rj => rj.Transaction)
                .ThenInclude(t => t.Branch)
            .Include(rj => rj.AssignedTechnician)
            .Include(rj => rj.QualityChecker)
            .FirstOrDefaultAsync(rj => rj.TransactionId == transactionId);

        return repairJob != null ? await MapToRepairJobDtoAsync(repairJob) : null;
    }

    /// <summary>
    /// Update repair job status
    /// </summary>
    public async Task<(bool IsSuccess, string? ErrorMessage)> UpdateRepairJobStatusAsync(int repairJobId, UpdateRepairJobStatusRequestDto request, string userId)
    {
        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var repairJob = await _context.RepairJobs
                .Include(rj => rj.Transaction)
                .FirstOrDefaultAsync(rj => rj.Id == repairJobId);

            if (repairJob == null)
            {
                return (false, "Repair job not found");
            }

            // Validate status transition
            var (canUpdate, errorMessage) = await CanUpdateRepairJobStatusAsync(repairJobId, request.Status);
            if (!canUpdate)
            {
                return (false, errorMessage);
            }

            var oldStatus = repairJob.Status;
            repairJob.Status = request.Status;

            // Update status-specific fields
            switch (request.Status)
            {
                case RepairStatus.InProgress:
                    repairJob.StartedDate = DateTime.UtcNow;
                    break;
                case RepairStatus.Completed:
                    repairJob.CompletedDate = DateTime.UtcNow;
                    break;
                case RepairStatus.ReadyForPickup:
                    repairJob.ReadyForPickupDate = DateTime.UtcNow;
                    break;
                case RepairStatus.Delivered:
                    repairJob.DeliveredDate = DateTime.UtcNow;
                    break;
            }

            // Update optional fields
            if (!string.IsNullOrEmpty(request.TechnicianNotes))
            {
                repairJob.TechnicianNotes = string.IsNullOrEmpty(repairJob.TechnicianNotes) 
                    ? request.TechnicianNotes 
                    : $"{repairJob.TechnicianNotes}\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: {request.TechnicianNotes}";
            }

            if (request.ActualCost.HasValue)
            {
                repairJob.ActualCost = request.ActualCost.Value;
            }

            if (!string.IsNullOrEmpty(request.MaterialsUsed))
            {
                repairJob.MaterialsUsed = request.MaterialsUsed;
            }

            if (request.HoursSpent.HasValue)
            {
                repairJob.HoursSpent = request.HoursSpent.Value;
            }

            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            // Log audit
            await _auditService.LogAsync(
                userId,
                "UPDATE_REPAIR_STATUS",
                "RepairJob",
                repairJob.Id.ToString(),
                $"Updated repair job status from {oldStatus} to {request.Status}",
                newValues: System.Text.Json.JsonSerializer.Serialize(new { 
                    OldStatus = oldStatus,
                    NewStatus = request.Status,
                    ActualCost = request.ActualCost,
                    HoursSpent = request.HoursSpent
                }),
                branchId: repairJob.Transaction.BranchId,
                transactionId: repairJob.TransactionId
            );

            _logger.LogInformation("Repair job {RepairJobId} status updated from {OldStatus} to {NewStatus} by user {UserId}", 
                repairJob.Id, oldStatus, request.Status, userId);

            return (true, null);
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            _logger.LogError(ex, "Error updating repair job {RepairJobId} status", repairJobId);
            return (false, "An error occurred while updating the repair job status");
        }
    }

    /// <summary>
    /// Assign technician to repair job
    /// </summary>
    public async Task<(bool IsSuccess, string? ErrorMessage)> AssignTechnicianAsync(int repairJobId, AssignTechnicianRequestDto request, string userId)
    {
        try
        {
            var repairJob = await _context.RepairJobs
                .Include(rj => rj.Transaction)
                .FirstOrDefaultAsync(rj => rj.Id == repairJobId);

            if (repairJob == null)
            {
                return (false, "Repair job not found");
            }

            // Verify technician exists
            var technician = await _context.Users.FindAsync(request.TechnicianId);
            if (technician == null)
            {
                return (false, "Technician not found");
            }

            var oldTechnicianId = repairJob.AssignedTechnicianId;
            repairJob.AssignedTechnicianId = request.TechnicianId;

            if (!string.IsNullOrEmpty(request.TechnicianNotes))
            {
                repairJob.TechnicianNotes = string.IsNullOrEmpty(repairJob.TechnicianNotes) 
                    ? request.TechnicianNotes 
                    : $"{repairJob.TechnicianNotes}\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: {request.TechnicianNotes}";
            }

            await _context.SaveChangesAsync();

            // Log audit
            await _auditService.LogAsync(
                userId,
                "ASSIGN_TECHNICIAN",
                "RepairJob",
                repairJob.Id.ToString(),
                $"Assigned technician {request.TechnicianId} to repair job",
                newValues: System.Text.Json.JsonSerializer.Serialize(new { 
                    OldTechnicianId = oldTechnicianId,
                    NewTechnicianId = request.TechnicianId
                }),
                branchId: repairJob.Transaction.BranchId,
                transactionId: repairJob.TransactionId
            );

            _logger.LogInformation("Technician {TechnicianId} assigned to repair job {RepairJobId} by user {UserId}", 
                request.TechnicianId, repairJob.Id, userId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning technician to repair job {RepairJobId}", repairJobId);
            return (false, "An error occurred while assigning the technician");
        }
    }

    /// <summary>
    /// Complete repair job
    /// </summary>
    public async Task<(bool IsSuccess, string? ErrorMessage)> CompleteRepairAsync(int repairJobId, CompleteRepairRequestDto request, string userId)
    {
        try
        {
            var repairJob = await _context.RepairJobs
                .Include(rj => rj.Transaction)
                .FirstOrDefaultAsync(rj => rj.Id == repairJobId);

            if (repairJob == null)
            {
                return (false, "Repair job not found");
            }

            if (repairJob.Status != RepairStatus.InProgress)
            {
                return (false, "Repair job must be in progress to be completed");
            }

            repairJob.Status = RepairStatus.Completed;
            repairJob.CompletedDate = DateTime.UtcNow;
            repairJob.ActualCost = request.ActualCost;

            if (!string.IsNullOrEmpty(request.TechnicianNotes))
            {
                repairJob.TechnicianNotes = string.IsNullOrEmpty(repairJob.TechnicianNotes) 
                    ? request.TechnicianNotes 
                    : $"{repairJob.TechnicianNotes}\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: {request.TechnicianNotes}";
            }

            if (!string.IsNullOrEmpty(request.MaterialsUsed))
            {
                repairJob.MaterialsUsed = request.MaterialsUsed;
            }

            if (request.HoursSpent.HasValue)
            {
                repairJob.HoursSpent = request.HoursSpent.Value;
            }

            await _context.SaveChangesAsync();

            // Log audit
            await _auditService.LogAsync(
                userId,
                "COMPLETE_REPAIR",
                "RepairJob",
                repairJob.Id.ToString(),
                $"Completed repair job with actual cost {request.ActualCost}",
                newValues: System.Text.Json.JsonSerializer.Serialize(new { 
                    ActualCost = request.ActualCost,
                    HoursSpent = request.HoursSpent,
                    MaterialsUsed = request.MaterialsUsed
                }),
                branchId: repairJob.Transaction.BranchId,
                transactionId: repairJob.TransactionId
            );

            _logger.LogInformation("Repair job {RepairJobId} completed by user {UserId} with actual cost {ActualCost}", 
                repairJob.Id, userId, request.ActualCost);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing repair job {RepairJobId}", repairJobId);
            return (false, "An error occurred while completing the repair");
        }
    }

    /// <summary>
    /// Mark repair as ready for pickup
    /// </summary>
    public async Task<(bool IsSuccess, string? ErrorMessage)> MarkReadyForPickupAsync(int repairJobId, string userId)
    {
        try
        {
            var repairJob = await _context.RepairJobs
                .Include(rj => rj.Transaction)
                .FirstOrDefaultAsync(rj => rj.Id == repairJobId);

            if (repairJob == null)
            {
                return (false, "Repair job not found");
            }

            if (repairJob.Status != RepairStatus.Completed)
            {
                return (false, "Repair job must be completed to be marked ready for pickup");
            }

            repairJob.Status = RepairStatus.ReadyForPickup;
            repairJob.ReadyForPickupDate = DateTime.UtcNow;
            repairJob.CustomerNotified = true;
            repairJob.CustomerNotificationDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log audit
            await _auditService.LogAsync(
                userId,
                "READY_FOR_PICKUP",
                "RepairJob",
                repairJob.Id.ToString(),
                $"Marked repair job ready for pickup",
                branchId: repairJob.Transaction.BranchId,
                transactionId: repairJob.TransactionId
            );

            _logger.LogInformation("Repair job {RepairJobId} marked ready for pickup by user {UserId}", 
                repairJob.Id, userId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking repair job {RepairJobId} ready for pickup", repairJobId);
            return (false, "An error occurred while marking the repair ready for pickup");
        }
    }

    /// <summary>
    /// Deliver repair to customer
    /// </summary>
    public async Task<(bool IsSuccess, string? ErrorMessage)> DeliverRepairAsync(int repairJobId, DeliverRepairRequestDto request, string userId)
    {
        try
        {
            var repairJob = await _context.RepairJobs
                .Include(rj => rj.Transaction)
                .FirstOrDefaultAsync(rj => rj.Id == repairJobId);

            if (repairJob == null)
            {
                return (false, "Repair job not found");
            }

            if (repairJob.Status != RepairStatus.ReadyForPickup)
            {
                return (false, "Repair job must be ready for pickup to be delivered");
            }

            repairJob.Status = RepairStatus.Delivered;
            repairJob.DeliveredDate = DateTime.UtcNow;
            repairJob.CustomerNotified = request.CustomerNotified;
            repairJob.CustomerNotificationDate = request.CustomerNotified ? DateTime.UtcNow : null;

            if (!string.IsNullOrEmpty(request.DeliveryNotes))
            {
                repairJob.TechnicianNotes = string.IsNullOrEmpty(repairJob.TechnicianNotes) 
                    ? $"Delivery: {request.DeliveryNotes}" 
                    : $"{repairJob.TechnicianNotes}\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: Delivery: {request.DeliveryNotes}";
            }

            await _context.SaveChangesAsync();

            // Log audit
            await _auditService.LogAsync(
                userId,
                "DELIVER_REPAIR",
                "RepairJob",
                repairJob.Id.ToString(),
                $"Delivered repair to customer",
                newValues: System.Text.Json.JsonSerializer.Serialize(new { 
                    DeliveryNotes = request.DeliveryNotes,
                    CustomerNotified = request.CustomerNotified
                }),
                branchId: repairJob.Transaction.BranchId,
                transactionId: repairJob.TransactionId
            );

            _logger.LogInformation("Repair job {RepairJobId} delivered to customer by user {UserId}", 
                repairJob.Id, userId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delivering repair job {RepairJobId}", repairJobId);
            return (false, "An error occurred while delivering the repair");
        }
    }

    /// <summary>
    /// Cancel repair job
    /// </summary>
    public async Task<(bool IsSuccess, string? ErrorMessage)> CancelRepairAsync(int repairJobId, string reason, string userId)
    {
        try
        {
            var repairJob = await _context.RepairJobs
                .Include(rj => rj.Transaction)
                .FirstOrDefaultAsync(rj => rj.Id == repairJobId);

            if (repairJob == null)
            {
                return (false, "Repair job not found");
            }

            if (repairJob.Status == RepairStatus.Delivered)
            {
                return (false, "Cannot cancel a delivered repair job");
            }

            var oldStatus = repairJob.Status;
            repairJob.Status = RepairStatus.Cancelled;

            if (!string.IsNullOrEmpty(reason))
            {
                repairJob.TechnicianNotes = string.IsNullOrEmpty(repairJob.TechnicianNotes) 
                    ? $"Cancelled: {reason}" 
                    : $"{repairJob.TechnicianNotes}\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: Cancelled: {reason}";
            }

            await _context.SaveChangesAsync();

            // Log audit
            await _auditService.LogAsync(
                userId,
                "CANCEL_REPAIR",
                "RepairJob",
                repairJob.Id.ToString(),
                $"Cancelled repair job: {reason}",
                newValues: System.Text.Json.JsonSerializer.Serialize(new { 
                    OldStatus = oldStatus,
                    CancellationReason = reason
                }),
                branchId: repairJob.Transaction.BranchId,
                transactionId: repairJob.TransactionId
            );

            _logger.LogInformation("Repair job {RepairJobId} cancelled by user {UserId}: {Reason}", 
                repairJob.Id, userId, reason);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling repair job {RepairJobId}", repairJobId);
            return (false, "An error occurred while cancelling the repair");
        }
    }

    /// <summary>
    /// Search repair jobs
    /// </summary>
    public async Task<(List<RepairJobDto> Items, int TotalCount)> SearchRepairJobsAsync(RepairJobSearchRequestDto request)
    {
        var query = _context.RepairJobs
            .Include(rj => rj.Transaction)
                .ThenInclude(t => t.Customer)
            .Include(rj => rj.Transaction)
                .ThenInclude(t => t.Branch)
            .Include(rj => rj.AssignedTechnician)
            .Include(rj => rj.QualityChecker)
            .AsQueryable();

        // Apply filters
        if (request.BranchId.HasValue)
        {
            query = query.Where(rj => rj.Transaction.BranchId == request.BranchId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(rj => rj.Status == request.Status.Value);
        }

        if (request.Priority.HasValue)
        {
            query = query.Where(rj => rj.Priority == request.Priority.Value);
        }

        if (!string.IsNullOrEmpty(request.AssignedTechnicianId))
        {
            query = query.Where(rj => rj.AssignedTechnicianId == request.AssignedTechnicianId);
        }

        if (request.CustomerId.HasValue)
        {
            query = query.Where(rj => rj.Transaction.CustomerId == request.CustomerId.Value);
        }

        if (!string.IsNullOrEmpty(request.TransactionNumber))
        {
            query = query.Where(rj => rj.Transaction.TransactionNumber.Contains(request.TransactionNumber));
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(rj => rj.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(rj => rj.CreatedAt <= request.ToDate.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(rj => rj.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var dtos = new List<RepairJobDto>();
        foreach (var item in items)
        {
            dtos.Add(await MapToRepairJobDtoAsync(item));
        }

        return (dtos, totalCount);
    }

    /// <summary>
    /// Get repair job statistics
    /// </summary>
    public async Task<RepairJobStatisticsDto> GetRepairJobStatisticsAsync(int? branchId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.RepairJobs
            .Include(rj => rj.Transaction)
            .AsQueryable();

        if (branchId.HasValue)
        {
            query = query.Where(rj => rj.Transaction.BranchId == branchId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(rj => rj.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(rj => rj.CreatedAt <= toDate.Value);
        }

        var statistics = new RepairJobStatisticsDto
        {
            TotalJobs = await query.CountAsync(),
            PendingJobs = await query.CountAsync(rj => rj.Status == RepairStatus.Pending),
            InProgressJobs = await query.CountAsync(rj => rj.Status == RepairStatus.InProgress),
            CompletedJobs = await query.CountAsync(rj => rj.Status == RepairStatus.Completed),
            ReadyForPickupJobs = await query.CountAsync(rj => rj.Status == RepairStatus.ReadyForPickup),
            DeliveredJobs = await query.CountAsync(rj => rj.Status == RepairStatus.Delivered),
            CancelledJobs = await query.CountAsync(rj => rj.Status == RepairStatus.Cancelled),
            TotalRevenue = await query.Where(rj => rj.ActualCost.HasValue).SumAsync(rj => rj.ActualCost.Value)
        };

        // Calculate average completion time
        var completedJobs = await query
            .Where(rj => rj.Status == RepairStatus.Delivered && rj.StartedDate.HasValue && rj.DeliveredDate.HasValue)
            .ToListAsync();

        if (completedJobs.Any())
        {
            var totalHours = completedJobs.Sum(rj => (rj.DeliveredDate.Value - rj.StartedDate.Value).TotalHours);
            statistics.AverageCompletionTime = (decimal)(totalHours / completedJobs.Count);
        }

        // Jobs by priority
        var priorityStats = await query
            .GroupBy(rj => rj.Priority)
            .Select(g => new { Priority = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        foreach (var stat in priorityStats)
        {
            statistics.JobsByPriority[stat.Priority] = stat.Count;
        }

        // Jobs by technician
        var technicianStats = await query
            .Where(rj => rj.AssignedTechnicianId != null)
            .GroupBy(rj => rj.AssignedTechnicianId)
            .Select(g => new { TechnicianId = g.Key, Count = g.Count() })
            .ToListAsync();

        foreach (var stat in technicianStats)
        {
            statistics.JobsByTechnician[stat.TechnicianId!] = stat.Count;
        }

        return statistics;
    }

    /// <summary>
    /// Get repair jobs by status
    /// </summary>
    public async Task<List<RepairJobDto>> GetRepairJobsByStatusAsync(RepairStatus status, int? branchId = null)
    {
        var query = _context.RepairJobs
            .Include(rj => rj.Transaction)
                .ThenInclude(t => t.Customer)
            .Include(rj => rj.Transaction)
                .ThenInclude(t => t.Branch)
            .Include(rj => rj.AssignedTechnician)
            .Include(rj => rj.QualityChecker)
            .Where(rj => rj.Status == status);

        if (branchId.HasValue)
        {
            query = query.Where(rj => rj.Transaction.BranchId == branchId.Value);
        }

        var items = await query
            .OrderByDescending(rj => rj.CreatedAt)
            .ToListAsync();

        var dtos = new List<RepairJobDto>();
        foreach (var item in items)
        {
            dtos.Add(await MapToRepairJobDtoAsync(item));
        }

        return dtos;
    }

    /// <summary>
    /// Get repair jobs assigned to technician
    /// </summary>
    public async Task<List<RepairJobDto>> GetRepairJobsByTechnicianAsync(string technicianId, int? branchId = null)
    {
        var query = _context.RepairJobs
            .Include(rj => rj.Transaction)
                .ThenInclude(t => t.Customer)
            .Include(rj => rj.Transaction)
                .ThenInclude(t => t.Branch)
            .Include(rj => rj.AssignedTechnician)
            .Include(rj => rj.QualityChecker)
            .Where(rj => rj.AssignedTechnicianId == technicianId);

        if (branchId.HasValue)
        {
            query = query.Where(rj => rj.Transaction.BranchId == branchId.Value);
        }

        var items = await query
            .OrderByDescending(rj => rj.CreatedAt)
            .ToListAsync();

        var dtos = new List<RepairJobDto>();
        foreach (var item in items)
        {
            dtos.Add(await MapToRepairJobDtoAsync(item));
        }

        return dtos;
    }

    /// <summary>
    /// Check if repair job can be updated to specified status
    /// </summary>
    public async Task<(bool CanUpdate, string? ErrorMessage)> CanUpdateRepairJobStatusAsync(int repairJobId, RepairStatus newStatus)
    {
        var repairJob = await _context.RepairJobs.FindAsync(repairJobId);
        if (repairJob == null)
        {
            return (false, "Repair job not found");
        }

        // Define valid status transitions
        var validTransitions = new Dictionary<RepairStatus, RepairStatus[]>
        {
            { RepairStatus.Pending, new[] { RepairStatus.InProgress, RepairStatus.Cancelled } },
            { RepairStatus.InProgress, new[] { RepairStatus.Completed, RepairStatus.Cancelled } },
            { RepairStatus.Completed, new[] { RepairStatus.ReadyForPickup, RepairStatus.Cancelled } },
            { RepairStatus.ReadyForPickup, new[] { RepairStatus.Delivered, RepairStatus.Cancelled } },
            { RepairStatus.Delivered, new RepairStatus[] { } }, // No further transitions
            { RepairStatus.Cancelled, new RepairStatus[] { } }  // No further transitions
        };

        if (!validTransitions.ContainsKey(repairJob.Status))
        {
            return (false, "Invalid current status");
        }

        if (!validTransitions[repairJob.Status].Contains(newStatus))
        {
            return (false, $"Cannot transition from {repairJob.Status} to {newStatus}");
        }

        return (true, null);
    }

    /// <summary>
    /// Get overdue repair jobs
    /// </summary>
    public async Task<List<RepairJobDto>> GetOverdueRepairJobsAsync(int? branchId = null)
    {
        var today = DateTime.UtcNow.Date;
        var query = _context.RepairJobs
            .Include(rj => rj.Transaction)
                .ThenInclude(t => t.Customer)
            .Include(rj => rj.Transaction)
                .ThenInclude(t => t.Branch)
            .Include(rj => rj.AssignedTechnician)
            .Include(rj => rj.QualityChecker)
            .Where(rj => rj.Transaction.EstimatedCompletionDate.HasValue &&
                        rj.Transaction.EstimatedCompletionDate.Value.Date < today &&
                        rj.Status != RepairStatus.Delivered &&
                        rj.Status != RepairStatus.Cancelled);

        if (branchId.HasValue)
        {
            query = query.Where(rj => rj.Transaction.BranchId == branchId.Value);
        }

        var items = await query
            .OrderBy(rj => rj.Transaction.EstimatedCompletionDate)
            .ToListAsync();

        var dtos = new List<RepairJobDto>();
        foreach (var item in items)
        {
            dtos.Add(await MapToRepairJobDtoAsync(item));
        }

        return dtos;
    }

    /// <summary>
    /// Get repair jobs due today
    /// </summary>
    public async Task<List<RepairJobDto>> GetRepairJobsDueTodayAsync(int? branchId = null)
    {
        var today = DateTime.UtcNow.Date;
        var query = _context.RepairJobs
            .Include(rj => rj.Transaction)
                .ThenInclude(t => t.Customer)
            .Include(rj => rj.Transaction)
                .ThenInclude(t => t.Branch)
            .Include(rj => rj.AssignedTechnician)
            .Include(rj => rj.QualityChecker)
            .Where(rj => rj.Transaction.EstimatedCompletionDate.HasValue &&
                        rj.Transaction.EstimatedCompletionDate.Value.Date == today &&
                        rj.Status != RepairStatus.Delivered &&
                        rj.Status != RepairStatus.Cancelled);

        if (branchId.HasValue)
        {
            query = query.Where(rj => rj.Transaction.BranchId == branchId.Value);
        }

        var items = await query
            .OrderBy(rj => rj.Transaction.EstimatedCompletionDate)
            .ToListAsync();

        var dtos = new List<RepairJobDto>();
        foreach (var item in items)
        {
            dtos.Add(await MapToRepairJobDtoAsync(item));
        }

        return dtos;
    }

    /// <summary>
    /// Map RepairJob entity to RepairJobDto
    /// </summary>
    private async Task<RepairJobDto> MapToRepairJobDtoAsync(RepairJob repairJob)
    {
        return new RepairJobDto
        {
            Id = repairJob.Id,
            TransactionId = repairJob.TransactionId,
            TransactionNumber = repairJob.Transaction.TransactionNumber,
            Status = repairJob.Status,
            Priority = repairJob.Priority,
            AssignedTechnicianId = repairJob.AssignedTechnicianId,
            AssignedTechnicianName = repairJob.AssignedTechnician?.FullName,
            StartedDate = repairJob.StartedDate,
            CompletedDate = repairJob.CompletedDate,
            ReadyForPickupDate = repairJob.ReadyForPickupDate,
            DeliveredDate = repairJob.DeliveredDate,
            TechnicianNotes = repairJob.TechnicianNotes,
            ActualCost = repairJob.ActualCost,
            MaterialsUsed = repairJob.MaterialsUsed,
            HoursSpent = repairJob.HoursSpent,
            QualityCheckedBy = repairJob.QualityCheckedBy,
            QualityCheckerName = repairJob.QualityChecker?.FullName,
            QualityCheckDate = repairJob.QualityCheckDate,
            CustomerNotified = repairJob.CustomerNotified,
            CustomerNotificationDate = repairJob.CustomerNotificationDate,
            CreatedAt = repairJob.CreatedAt,
            CreatedBy = repairJob.CreatedBy,
            CreatedByName = await GetUserNameAsync(repairJob.CreatedBy),
            
            // Transaction details
            RepairDescription = repairJob.Transaction.RepairDescription ?? "",
            RepairAmount = repairJob.Transaction.TotalAmount,
            AmountPaid = repairJob.Transaction.AmountPaid,
            EstimatedCompletionDate = repairJob.Transaction.EstimatedCompletionDate,
            CustomerId = repairJob.Transaction.CustomerId,
            CustomerName = repairJob.Transaction.Customer?.FullName,
            CustomerPhone = repairJob.Transaction.Customer?.MobileNumber,
            BranchId = repairJob.Transaction.BranchId,
            BranchName = repairJob.Transaction.Branch.Name
        };
    }

    /// <summary>
    /// Get user name by user ID
    /// </summary>
    private async Task<string> GetUserNameAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user?.FullName ?? userId;
    }
}
