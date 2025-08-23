using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Shared;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service for repair job operations and workflow management
/// </summary>
public class RepairJobService : IRepairJobService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;
    private readonly ILogger<RepairJobService> _logger;

    public RepairJobService(
        ApplicationDbContext context,
        IMapper mapper,
        IAuditService auditService,
        ILogger<RepairJobService> logger)
    {
        _context = context;
        _mapper = mapper;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<(bool IsSuccess, string? ErrorMessage, RepairJobDto? RepairJob)> CreateRepairJobAsync(CreateRepairJobRequestDto request, string userId)
    {
        try
        {
            // Verify financial transaction exists and is a repair transaction
            var financialTransaction = await _context.FinancialTransactions
                .Include(ft => ft.Branch)
                .FirstOrDefaultAsync(ft => ft.Id == request.FinancialTransactionId);

            if (financialTransaction == null)
            {
                return (false, "Financial transaction not found", null);
            }

            // Check if repair job already exists for this financial transaction
            var existingRepairJob = await _context.RepairJobs
                .FirstOrDefaultAsync(rj => rj.FinancialTransactionId == request.FinancialTransactionId);

            if (existingRepairJob != null)
            {
                return (false, "Repair job already exists for this financial transaction", null);
            }

            // Verify technician exists if specified
            if (request.AssignedTechnicianId.HasValue)
            {
                var technician = await _context.Technicians
                    .FirstOrDefaultAsync(t => t.Id == request.AssignedTechnicianId.Value && t.IsActive);

                if (technician == null)
                {
                    return (false, "Assigned technician not found", null);
                }
            }

            // Create repair job
            var repairJob = new RepairJob
            {
                FinancialTransactionId = request.FinancialTransactionId,
                StatusId = LookupTableConstants.RepairStatusPending,
                PriorityId = request.PriorityId,
                AssignedTechnicianId = request.AssignedTechnicianId,
                TechnicianNotes = request.TechnicianNotes,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.RepairJobs.AddAsync(repairJob);
            await _context.SaveChangesAsync();

            // Log audit trail
            await _auditService.LogAsync(
                userId,
                "CREATE_REPAIR_JOB",
                "RepairJob",
                repairJob.Id.ToString(),
                $"Created repair job for financial transaction {financialTransaction.TransactionNumber}",
                branchId: financialTransaction.BranchId
            );

            // Get the created repair job with navigation properties
            var createdRepairJob = await GetRepairJobAsync(repairJob.Id);

            _logger.LogInformation("Repair job {RepairJobId} created successfully for financial transaction {FinancialTransactionId} by user {UserId}",
                repairJob.Id, request.FinancialTransactionId, userId);

            return (true, null, createdRepairJob);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating repair job for financial transaction {FinancialTransactionId}", request.FinancialTransactionId);
            return (false, "An error occurred while creating the repair job", null);
        }
    }

    public async Task<RepairJobDto?> GetRepairJobAsync(int id)
    {
        try
        {
            var repairJob = await _context.RepairJobs
                .Include(rj => rj.FinancialTransaction)
                    .ThenInclude(ft => ft!.Branch)
                .Include(rj => rj.AssignedTechnician)
                .Include(rj => rj.QualityChecker)
                .Include(rj => rj.Status)
                .Include(rj => rj.Priority)
                .FirstOrDefaultAsync(rj => rj.Id == id);

            if (repairJob == null)
                return null;

            return await MapRepairJobToDto(repairJob);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving repair job {RepairJobId}", id);
            throw;
        }
    }

    public async Task<RepairJobDto?> GetRepairJobByFinancialTransactionIdAsync(int financialTransactionId)
    {
        try
        {
            var repairJob = await _context.RepairJobs
                .Include(rj => rj.FinancialTransaction)
                    .ThenInclude(ft => ft!.Branch)
                .Include(rj => rj.AssignedTechnician)
                .Include(rj => rj.QualityChecker)
                .Include(rj => rj.Status)
                .Include(rj => rj.Priority)
                .FirstOrDefaultAsync(rj => rj.FinancialTransactionId == financialTransactionId);

            if (repairJob == null)
                return null;

            return await MapRepairJobToDto(repairJob);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving repair job by financial transaction {FinancialTransactionId}", financialTransactionId);
            throw;
        }
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> UpdateRepairJobStatusAsync(int id, UpdateRepairJobStatusRequestDto request, string userId)
    {
        try
        {
            var repairJob = await _context.RepairJobs
                .Include(rj => rj.FinancialTransaction)
                .FirstOrDefaultAsync(rj => rj.Id == id);

            if (repairJob == null)
            {
                return (false, "Repair job not found");
            }

            // Validate status transition
            if (!IsValidStatusTransition(repairJob.StatusId, request.StatusId))
            {
                return (false, $"Invalid status transition from status ID {repairJob.StatusId} to {request.StatusId}");
            }

            // Update repair job
            repairJob.StatusId = request.StatusId;
            repairJob.TechnicianNotes = request.TechnicianNotes ?? repairJob.TechnicianNotes;
            repairJob.ActualCost = request.ActualCost ?? repairJob.ActualCost;
            repairJob.MaterialsUsed = request.MaterialsUsed ?? repairJob.MaterialsUsed;
            repairJob.HoursSpent = request.HoursSpent ?? repairJob.HoursSpent;
            repairJob.ModifiedBy = userId;
            repairJob.ModifiedAt = DateTime.UtcNow;

            // Set status-specific dates
            switch (request.StatusId)
            {
                case LookupTableConstants.RepairStatusInProgress:
                    if (repairJob.StartedDate == null)
                        repairJob.StartedDate = DateTime.UtcNow;
                    break;
                case LookupTableConstants.RepairStatusCompleted:
                    if (repairJob.CompletedDate == null)
                        repairJob.CompletedDate = DateTime.UtcNow;
                    break;
                case LookupTableConstants.RepairStatusReadyForPickup:
                    if (repairJob.ReadyForPickupDate == null)
                        repairJob.ReadyForPickupDate = DateTime.UtcNow;
                    break;
                case LookupTableConstants.RepairStatusDelivered:
                    if (repairJob.DeliveredDate == null)
                        repairJob.DeliveredDate = DateTime.UtcNow;
                    break;
            }

            await _context.SaveChangesAsync();

            // Log audit trail
            await _auditService.LogAsync(
                userId,
                "UPDATE_REPAIR_STATUS",
                "RepairJob",
                repairJob.Id.ToString(),
                $"Updated repair job status to status ID {request.StatusId}",
                branchId: repairJob.FinancialTransaction?.BranchId
            );

            _logger.LogInformation("Repair job {RepairJobId} status updated to status ID {StatusId} by user {UserId}",
                id, request.StatusId, userId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating repair job {RepairJobId} status", id);
            return (false, "An error occurred while updating the repair job status");
        }
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> AssignTechnicianAsync(int id, AssignTechnicianRequestDto request, string userId)
    {
        try
        {
            var repairJob = await _context.RepairJobs
                .Include(rj => rj.FinancialTransaction)
                .FirstOrDefaultAsync(rj => rj.Id == id);

            if (repairJob == null)
            {
                return (false, "Repair job not found");
            }

            // Verify technician exists
            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.Id == request.TechnicianId && t.IsActive);

            if (technician == null)
            {
                return (false, "Technician not found");
            }

            // Update repair job
            repairJob.AssignedTechnicianId = request.TechnicianId;
            repairJob.TechnicianNotes = request.TechnicianNotes ?? repairJob.TechnicianNotes;
            repairJob.ModifiedBy = userId;
            repairJob.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log audit trail
            await _auditService.LogAsync(
                userId,
                "ASSIGN_TECHNICIAN",
                "RepairJob",
                repairJob.Id.ToString(),
                $"Assigned technician {technician.FullName} to repair job",
                branchId: repairJob.FinancialTransaction?.BranchId
            );

            _logger.LogInformation("Technician {TechnicianId} assigned to repair job {RepairJobId} by user {UserId}",
                request.TechnicianId, id, userId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning technician to repair job {RepairJobId}", id);
            return (false, "An error occurred while assigning the technician");
        }
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> CompleteRepairAsync(int id, CompleteRepairRequestDto request, string userId)
    {
        try
        {
            var repairJob = await _context.RepairJobs
                .Include(rj => rj.FinancialTransaction)
                .FirstOrDefaultAsync(rj => rj.Id == id);

            if (repairJob == null)
            {
                return (false, "Repair job not found");
            }

            if (repairJob.StatusId != LookupTableConstants.RepairStatusInProgress)
            {
                return (false, "Repair job must be in progress to be completed");
            }

            // Update repair job
            repairJob.StatusId = LookupTableConstants.RepairStatusCompleted;
            repairJob.CompletedDate = DateTime.UtcNow;
            repairJob.ActualCost = request.ActualCost;
            repairJob.TechnicianNotes = request.TechnicianNotes ?? repairJob.TechnicianNotes;
            repairJob.MaterialsUsed = request.MaterialsUsed ?? repairJob.MaterialsUsed;
            repairJob.HoursSpent = request.HoursSpent ?? repairJob.HoursSpent;
            repairJob.ModifiedBy = userId;
            repairJob.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log audit trail
            await _auditService.LogAsync(
                userId,
                "COMPLETE_REPAIR",
                "RepairJob",
                repairJob.Id.ToString(),
                $"Completed repair job with actual cost {request.ActualCost:C}",
                branchId: repairJob.FinancialTransaction?.BranchId
            );

            _logger.LogInformation("Repair job {RepairJobId} completed by user {UserId} with actual cost {ActualCost}",
                id, userId, request.ActualCost);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing repair job {RepairJobId}", id);
            return (false, "An error occurred while completing the repair job");
        }
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> MarkReadyForPickupAsync(int id, string userId)
    {
        try
        {
            var repairJob = await _context.RepairJobs
                .Include(rj => rj.FinancialTransaction)
                .FirstOrDefaultAsync(rj => rj.Id == id);

            if (repairJob == null)
            {
                return (false, "Repair job not found");
            }

            if (repairJob.StatusId != LookupTableConstants.RepairStatusCompleted)
            {
                return (false, "Repair job must be completed before marking ready for pickup");
            }

            // Update repair job
            repairJob.StatusId = LookupTableConstants.RepairStatusReadyForPickup;
            repairJob.ReadyForPickupDate = DateTime.UtcNow;
            repairJob.QualityCheckedBy = int.Parse(userId); // Assuming quality check by current user
            repairJob.QualityCheckDate = DateTime.UtcNow;
            repairJob.ModifiedBy = userId;
            repairJob.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log audit trail
            await _auditService.LogAsync(
                userId,
                "READY_FOR_PICKUP",
                "RepairJob",
                repairJob.Id.ToString(),
                "Marked repair job ready for pickup",
                branchId: repairJob.FinancialTransaction?.BranchId
            );

            _logger.LogInformation("Repair job {RepairJobId} marked ready for pickup by user {UserId}",
                id, userId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking repair job {RepairJobId} ready for pickup", id);
            return (false, "An error occurred while marking the repair job ready for pickup");
        }
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> DeliverRepairAsync(int id, DeliverRepairRequestDto request, string userId)
    {
        try
        {
            var repairJob = await _context.RepairJobs
                .Include(rj => rj.FinancialTransaction)
                .FirstOrDefaultAsync(rj => rj.Id == id);

            if (repairJob == null)
            {
                return (false, "Repair job not found");
            }

            if (repairJob.StatusId != LookupTableConstants.RepairStatusReadyForPickup)
            {
                return (false, "Repair job must be ready for pickup before delivery");
            }

            // Update repair job
            repairJob.StatusId = LookupTableConstants.RepairStatusDelivered;
            repairJob.DeliveredDate = DateTime.UtcNow;
            repairJob.CustomerNotified = request.CustomerNotified;
            repairJob.CustomerNotificationDate = request.CustomerNotified ? DateTime.UtcNow : null;
            repairJob.ModifiedBy = userId;
            repairJob.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log audit trail
            await _auditService.LogAsync(
                userId,
                "DELIVER_REPAIR",
                "RepairJob",
                repairJob.Id.ToString(),
                "Delivered repair job to customer",
                branchId: repairJob.FinancialTransaction?.BranchId
            );

            _logger.LogInformation("Repair job {RepairJobId} delivered to customer by user {UserId}",
                id, userId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delivering repair job {RepairJobId}", id);
            return (false, "An error occurred while delivering the repair job");
        }
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> CancelRepairAsync(int id, string reason, string userId)
    {
        try
        {
            var repairJob = await _context.RepairJobs
                .Include(rj => rj.FinancialTransaction)
                .FirstOrDefaultAsync(rj => rj.Id == id);

            if (repairJob == null)
            {
                return (false, "Repair job not found");
            }

            if (repairJob.StatusId == LookupTableConstants.RepairStatusDelivered)
            {
                return (false, "Cannot cancel a delivered repair job");
            }

            // Update repair job
            repairJob.StatusId = LookupTableConstants.RepairStatusCancelled;
            repairJob.TechnicianNotes = $"{repairJob.TechnicianNotes}\n\nCancelled: {reason}".Trim();
            repairJob.ModifiedBy = userId;
            repairJob.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log audit trail
            await _auditService.LogAsync(
                userId,
                "CANCEL_REPAIR",
                "RepairJob",
                repairJob.Id.ToString(),
                $"Cancelled repair job: {reason}",
                branchId: repairJob.FinancialTransaction?.BranchId
            );

            _logger.LogInformation("Repair job {RepairJobId} cancelled by user {UserId}: {Reason}",
                id, userId, reason);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling repair job {RepairJobId}", id);
            return (false, "An error occurred while cancelling the repair job");
        }
    }

    public async Task<(List<RepairJobDto> Items, int TotalCount)> SearchRepairJobsAsync(RepairJobSearchRequestDto request)
    {
        try
        {
            var query = _context.RepairJobs
                .Include(rj => rj.FinancialTransaction)
                    .ThenInclude(ft => ft!.Branch)
                .Include(rj => rj.AssignedTechnician)
                .Include(rj => rj.QualityChecker)
                .Include(rj => rj.Status)
                .Include(rj => rj.Priority)
                .AsQueryable();

            // Apply filters
            if (request.BranchId.HasValue)
            {
                query = query.Where(rj => rj.FinancialTransaction!.BranchId == request.BranchId.Value);
            }

            if (request.StatusId.HasValue)
            {
                query = query.Where(rj => rj.StatusId == request.StatusId.Value);
            }

            if (request.PriorityId.HasValue)
            {
                query = query.Where(rj => rj.PriorityId == request.PriorityId.Value);
            }

            if (request.AssignedTechnicianId.HasValue)
            {
                query = query.Where(rj => rj.AssignedTechnicianId == request.AssignedTechnicianId.Value);
            }

            if (request.CustomerId.HasValue)
            {
                query = query.Where(rj => rj.FinancialTransaction!.BusinessEntityId == request.CustomerId.Value &&
                                         rj.FinancialTransaction.BusinessEntityTypeId == LookupTableConstants.BusinessEntityTypeCustomer);
            }

            if (!string.IsNullOrEmpty(request.TransactionNumber))
            {
                query = query.Where(rj => rj.FinancialTransaction!.TransactionNumber.Contains(request.TransactionNumber));
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(rj => rj.CreatedAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(rj => rj.CreatedAt <= request.ToDate.Value.AddDays(1));
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var repairJobs = await query
                .OrderByDescending(rj => rj.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            // Map to DTOs
            var repairJobDtos = new List<RepairJobDto>();
            foreach (var repairJob in repairJobs)
            {
                var dto = await MapRepairJobToDto(repairJob);
                repairJobDtos.Add(dto);
            }

            return (repairJobDtos, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching repair jobs");
            throw;
        }
    }

    public async Task<RepairJobStatisticsDto> GetRepairJobStatisticsAsync(int? branchId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.RepairJobs
                .Include(rj => rj.FinancialTransaction)
                .Include(rj => rj.AssignedTechnician)
                .AsQueryable();

            // Apply filters
            if (branchId.HasValue)
            {
                query = query.Where(rj => rj.FinancialTransaction!.BranchId == branchId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(rj => rj.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(rj => rj.CreatedAt <= toDate.Value.AddDays(1));
            }

            var repairJobs = await query.ToListAsync();

            // Calculate statistics
            var statistics = new RepairJobStatisticsDto
            {
                TotalJobs = repairJobs.Count,
                PendingJobs = repairJobs.Count(rj => rj.StatusId == LookupTableConstants.RepairStatusPending),
                InProgressJobs = repairJobs.Count(rj => rj.StatusId == LookupTableConstants.RepairStatusInProgress),
                CompletedJobs = repairJobs.Count(rj => rj.StatusId == LookupTableConstants.RepairStatusCompleted),
                ReadyForPickupJobs = repairJobs.Count(rj => rj.StatusId == LookupTableConstants.RepairStatusReadyForPickup),
                DeliveredJobs = repairJobs.Count(rj => rj.StatusId == LookupTableConstants.RepairStatusDelivered),
                CancelledJobs = repairJobs.Count(rj => rj.StatusId == LookupTableConstants.RepairStatusCancelled),
                TotalRevenue = repairJobs.Where(rj => rj.StatusId == LookupTableConstants.RepairStatusDelivered)
                                       .Sum(rj => rj.ActualCost ?? 0),
                AverageCompletionTime = CalculateAverageCompletionTime(repairJobs),
                JobsByPriority = repairJobs.GroupBy(rj => rj.Priority?.Name ?? "Unknown")
                                          .ToDictionary(g => g.Key, g => g.Count()),
                JobsByTechnician = repairJobs.Where(rj => rj.AssignedTechnician != null)
                                           .GroupBy(rj => rj.AssignedTechnician!.FullName)
                                           .ToDictionary(g => g.Key, g => g.Count())
            };

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving repair job statistics");
            throw;
        }
    }

    public async Task<List<RepairJobDto>> GetRepairJobsByStatusAsync(int statusId, int? branchId = null)
    {
        try
        {
            var query = _context.RepairJobs
                .Include(rj => rj.FinancialTransaction)
                    .ThenInclude(ft => ft!.Branch)
                .Include(rj => rj.AssignedTechnician)
                .Include(rj => rj.QualityChecker)
                .Include(rj => rj.Status)
                .Include(rj => rj.Priority)
                .Where(rj => rj.StatusId == statusId);

            if (branchId.HasValue)
            {
                query = query.Where(rj => rj.FinancialTransaction!.BranchId == branchId.Value);
            }

            var repairJobs = await query
                .OrderByDescending(rj => rj.CreatedAt)
                .ToListAsync();

            var repairJobDtos = new List<RepairJobDto>();
            foreach (var repairJob in repairJobs)
            {
                var dto = await MapRepairJobToDto(repairJob);
                repairJobDtos.Add(dto);
            }

            return repairJobDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving repair jobs by status {StatusId}", statusId);
            throw;
        }
    }

    public async Task<List<RepairJobDto>> GetRepairJobsByTechnicianAsync(int technicianId, int? branchId = null)
    {
        try
        {
            var query = _context.RepairJobs
                .Include(rj => rj.FinancialTransaction)
                    .ThenInclude(ft => ft!.Branch)
                .Include(rj => rj.AssignedTechnician)
                .Include(rj => rj.QualityChecker)
                .Include(rj => rj.Status)
                .Include(rj => rj.Priority)
                .Where(rj => rj.AssignedTechnicianId == technicianId);

            if (branchId.HasValue)
            {
                query = query.Where(rj => rj.FinancialTransaction!.BranchId == branchId.Value);
            }

            var repairJobs = await query
                .OrderByDescending(rj => rj.CreatedAt)
                .ToListAsync();

            var repairJobDtos = new List<RepairJobDto>();
            foreach (var repairJob in repairJobs)
            {
                var dto = await MapRepairJobToDto(repairJob);
                repairJobDtos.Add(dto);
            }

            return repairJobDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving repair jobs by technician {TechnicianId}", technicianId);
            throw;
        }
    }

    public async Task<List<RepairJobDto>> GetOverdueRepairJobsAsync(int? branchId = null)
    {
        try
        {
            var currentDate = DateTime.UtcNow.Date;

            var query = _context.RepairJobs
                .Include(rj => rj.FinancialTransaction)
                    .ThenInclude(ft => ft!.Branch)
                .Include(rj => rj.AssignedTechnician)
                .Include(rj => rj.QualityChecker)
                .Include(rj => rj.Status)
                .Include(rj => rj.Priority)
                .Where(rj => rj.StatusId != LookupTableConstants.RepairStatusDelivered &&
                           rj.StatusId != LookupTableConstants.RepairStatusCancelled);

            if (branchId.HasValue)
            {
                query = query.Where(rj => rj.FinancialTransaction!.BranchId == branchId.Value);
            }

            var repairJobs = await query.ToListAsync();

            // Filter overdue jobs (where estimated completion date has passed)
            var overdueJobs = repairJobs
                .Where(rj => GetEstimatedCompletionDate(rj) < currentDate)
                .OrderByDescending(rj => rj.CreatedAt)
                .ToList();

            var repairJobDtos = new List<RepairJobDto>();
            foreach (var repairJob in overdueJobs)
            {
                var dto = await MapRepairJobToDto(repairJob);
                repairJobDtos.Add(dto);
            }

            return repairJobDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue repair jobs");
            throw;
        }
    }

    public async Task<List<RepairJobDto>> GetRepairJobsDueTodayAsync(int? branchId = null)
    {
        try
        {
            var currentDate = DateTime.UtcNow.Date;

            var query = _context.RepairJobs
                .Include(rj => rj.FinancialTransaction)
                    .ThenInclude(ft => ft!.Branch)
                .Include(rj => rj.AssignedTechnician)
                .Include(rj => rj.QualityChecker)
                .Include(rj => rj.Status)
                .Include(rj => rj.Priority)
                .Where(rj => rj.StatusId != LookupTableConstants.RepairStatusDelivered &&
                           rj.StatusId != LookupTableConstants.RepairStatusCancelled);

            if (branchId.HasValue)
            {
                query = query.Where(rj => rj.FinancialTransaction!.BranchId == branchId.Value);
            }

            var repairJobs = await query.ToListAsync();

            // Filter jobs due today
            var jobsDueToday = repairJobs
                .Where(rj => GetEstimatedCompletionDate(rj).Date == currentDate)
                .OrderByDescending(rj => rj.CreatedAt)
                .ToList();

            var repairJobDtos = new List<RepairJobDto>();
            foreach (var repairJob in jobsDueToday)
            {
                var dto = await MapRepairJobToDto(repairJob);
                repairJobDtos.Add(dto);
            }

            return repairJobDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving repair jobs due today");
            throw;
        }
    }

    #region Private Helper Methods

    private async Task<RepairJobDto> MapRepairJobToDto(RepairJob repairJob)
    {
        // Get customer information from the financial transaction
        Customer? customer = null;
        if (repairJob.FinancialTransaction?.BusinessEntityTypeId == LookupTableConstants.BusinessEntityTypeCustomer)
        {
            customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == repairJob.FinancialTransaction.BusinessEntityId);
        }

        // Get order information if exists
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.FinancialTransactionId == repairJob.FinancialTransactionId);

        return new RepairJobDto
        {
            Id = repairJob.Id,
            FinancialTransactionId = repairJob.FinancialTransactionId,
            FinancialTransactionNumber = repairJob.FinancialTransaction?.TransactionNumber,
            StatusId = repairJob.StatusId,
            StatusDisplayName = repairJob.Status?.Name ?? string.Empty,
            PriorityId = repairJob.PriorityId,
            PriorityDisplayName = repairJob.Priority?.Name ?? string.Empty,
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
            CreatedByName = repairJob.CreatedBy, // TODO: Get actual user name
            RepairDescription = order?.Notes ?? "Repair job",
            RepairAmount = repairJob.FinancialTransaction?.TotalAmount ?? 0,
            AmountPaid = repairJob.FinancialTransaction?.AmountPaid ?? 0,
            EstimatedCompletionDate = GetEstimatedCompletionDate(repairJob),
            CustomerId = customer?.Id,
            CustomerName = customer?.FullName,
            CustomerPhone = customer?.MobileNumber,
            BranchId = repairJob.FinancialTransaction?.BranchId ?? 0,
            BranchName = repairJob.FinancialTransaction?.Branch?.Name ?? string.Empty
        };
    }

    private static bool IsValidStatusTransition(int currentStatusId, int newStatusId)
    {
        return currentStatusId switch
        {
            LookupTableConstants.RepairStatusPending => newStatusId is LookupTableConstants.RepairStatusInProgress or LookupTableConstants.RepairStatusCancelled,
            LookupTableConstants.RepairStatusInProgress => newStatusId is LookupTableConstants.RepairStatusCompleted or LookupTableConstants.RepairStatusCancelled,
            LookupTableConstants.RepairStatusCompleted => newStatusId is LookupTableConstants.RepairStatusReadyForPickup or LookupTableConstants.RepairStatusCancelled,
            LookupTableConstants.RepairStatusReadyForPickup => newStatusId is LookupTableConstants.RepairStatusDelivered or LookupTableConstants.RepairStatusCancelled,
            LookupTableConstants.RepairStatusDelivered => false, // Cannot change from delivered
            LookupTableConstants.RepairStatusCancelled => false, // Cannot change from cancelled
            _ => false
        };
    }

    private static DateTime GetEstimatedCompletionDate(RepairJob repairJob)
    {
        // Try to get estimated completion date from related order
        // For now, default to 7 days from creation if not specified
        return repairJob.CreatedAt.AddDays(7);
    }

    private static decimal CalculateAverageCompletionTime(List<RepairJob> repairJobs)
    {
        var completedJobs = repairJobs
            .Where(rj => rj.CompletedDate.HasValue && rj.StartedDate.HasValue)
            .ToList();

        if (!completedJobs.Any())
            return 0;

        var totalHours = completedJobs
            .Sum(rj => (rj.CompletedDate!.Value - rj.StartedDate!.Value).TotalHours);

        return (decimal)(totalHours / completedJobs.Count);
    }

    #endregion
}
