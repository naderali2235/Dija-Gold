using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models.Enums;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service interface for repair job operations
/// </summary>
public interface IRepairJobService
{
    /// <summary>
    /// Create a new repair job from a repair transaction
    /// </summary>
    Task<(bool IsSuccess, string? ErrorMessage, RepairJobDto? RepairJob)> CreateRepairJobAsync(CreateRepairJobRequestDto request, string userId);

    /// <summary>
    /// Get repair job by ID
    /// </summary>
    Task<RepairJobDto?> GetRepairJobAsync(int repairJobId);

    /// <summary>
    /// Get repair job by transaction ID
    /// </summary>
    Task<RepairJobDto?> GetRepairJobByTransactionIdAsync(int transactionId);

    /// <summary>
    /// Update repair job status
    /// </summary>
    Task<(bool IsSuccess, string? ErrorMessage)> UpdateRepairJobStatusAsync(int repairJobId, UpdateRepairJobStatusRequestDto request, string userId);

    /// <summary>
    /// Assign technician to repair job
    /// </summary>
    Task<(bool IsSuccess, string? ErrorMessage)> AssignTechnicianAsync(int repairJobId, AssignTechnicianRequestDto request, string userId);

    /// <summary>
    /// Complete repair job
    /// </summary>
    Task<(bool IsSuccess, string? ErrorMessage)> CompleteRepairAsync(int repairJobId, CompleteRepairRequestDto request, string userId);

    /// <summary>
    /// Mark repair as ready for pickup
    /// </summary>
    Task<(bool IsSuccess, string? ErrorMessage)> MarkReadyForPickupAsync(int repairJobId, string userId);

    /// <summary>
    /// Deliver repair to customer
    /// </summary>
    Task<(bool IsSuccess, string? ErrorMessage)> DeliverRepairAsync(int repairJobId, DeliverRepairRequestDto request, string userId);

    /// <summary>
    /// Cancel repair job
    /// </summary>
    Task<(bool IsSuccess, string? ErrorMessage)> CancelRepairAsync(int repairJobId, string reason, string userId);

    /// <summary>
    /// Search repair jobs
    /// </summary>
    Task<(List<RepairJobDto> Items, int TotalCount)> SearchRepairJobsAsync(RepairJobSearchRequestDto request);

    /// <summary>
    /// Get repair job statistics
    /// </summary>
    Task<RepairJobStatisticsDto> GetRepairJobStatisticsAsync(int? branchId = null, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Get repair jobs by status
    /// </summary>
    Task<List<RepairJobDto>> GetRepairJobsByStatusAsync(RepairStatus status, int? branchId = null);

    /// <summary>
    /// Get repair jobs assigned to technician
    /// </summary>
    Task<List<RepairJobDto>> GetRepairJobsByTechnicianAsync(string technicianId, int? branchId = null);

    /// <summary>
    /// Check if repair job can be updated to specified status
    /// </summary>
    Task<(bool CanUpdate, string? ErrorMessage)> CanUpdateRepairJobStatusAsync(int repairJobId, RepairStatus newStatus);

    /// <summary>
    /// Get overdue repair jobs
    /// </summary>
    Task<List<RepairJobDto>> GetOverdueRepairJobsAsync(int? branchId = null);

    /// <summary>
    /// Get repair jobs due today
    /// </summary>
    Task<List<RepairJobDto>> GetRepairJobsDueTodayAsync(int? branchId = null);
}
