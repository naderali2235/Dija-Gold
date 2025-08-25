using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Interface for repair job operations and workflow management
/// </summary>
public interface IRepairJobService
{
    /// <summary>
    /// Create a new repair job from a financial transaction
    /// </summary>
    /// <param name="request">Create repair job request</param>
    /// <param name="userId">User creating the repair job</param>
    /// <returns>Success status, error message, and created repair job</returns>
    Task<(bool IsSuccess, string? ErrorMessage, RepairJobDto? RepairJob)> CreateRepairJobAsync(CreateRepairJobRequestDto request, string userId);

    /// <summary>
    /// Get repair job by ID
    /// </summary>
    /// <param name="id">Repair job ID</param>
    /// <returns>Repair job DTO or null if not found</returns>
    Task<RepairJobDto?> GetRepairJobAsync(int id);

    /// <summary>
    /// Get repair job by financial transaction ID
    /// </summary>
    /// <param name="financialTransactionId">Financial transaction ID</param>
    /// <returns>Repair job DTO or null if not found</returns>
    Task<RepairJobDto?> GetRepairJobByFinancialTransactionIdAsync(int financialTransactionId);

    /// <summary>
    /// Update repair job status
    /// </summary>
    /// <param name="id">Repair job ID</param>
    /// <param name="request">Status update request</param>
    /// <param name="userId">User updating the status</param>
    /// <returns>Success status and error message</returns>
    Task<(bool IsSuccess, string? ErrorMessage)> UpdateRepairJobStatusAsync(int id, UpdateRepairJobStatusRequestDto request, string userId);

    /// <summary>
    /// Assign technician to repair job
    /// </summary>
    /// <param name="id">Repair job ID</param>
    /// <param name="request">Technician assignment request</param>
    /// <param name="userId">User making the assignment</param>
    /// <returns>Success status and error message</returns>
    Task<(bool IsSuccess, string? ErrorMessage)> AssignTechnicianAsync(int id, AssignTechnicianRequestDto request, string userId);

    /// <summary>
    /// Complete repair job
    /// </summary>
    /// <param name="id">Repair job ID</param>
    /// <param name="request">Complete repair request</param>
    /// <param name="userId">User completing the repair</param>
    /// <returns>Success status and error message</returns>
    Task<(bool IsSuccess, string? ErrorMessage)> CompleteRepairAsync(int id, CompleteRepairRequestDto request, string userId);

    /// <summary>
    /// Mark repair job as ready for pickup
    /// </summary>
    /// <param name="id">Repair job ID</param>
    /// <param name="userId">User marking as ready</param>
    /// <returns>Success status and error message</returns>
    Task<(bool IsSuccess, string? ErrorMessage)> MarkReadyForPickupAsync(int id, string userId);

    /// <summary>
    /// Deliver repair job to customer
    /// </summary>
    /// <param name="id">Repair job ID</param>
    /// <param name="request">Delivery request</param>
    /// <param name="userId">User processing delivery</param>
    /// <returns>Success status and error message</returns>
    Task<(bool IsSuccess, string? ErrorMessage)> DeliverRepairAsync(int id, DeliverRepairRequestDto request, string userId);

    /// <summary>
    /// Cancel repair job
    /// </summary>
    /// <param name="id">Repair job ID</param>
    /// <param name="reason">Cancellation reason</param>
    /// <param name="userId">User cancelling the repair</param>
    /// <returns>Success status and error message</returns>
    Task<(bool IsSuccess, string? ErrorMessage)> CancelRepairAsync(int id, string reason, string userId);

    /// <summary>
    /// Search repair jobs with pagination
    /// </summary>
    /// <param name="request">Search criteria</param>
    /// <returns>Paginated repair jobs and total count</returns>
    Task<(List<RepairJobDto> Items, int TotalCount)> SearchRepairJobsAsync(RepairJobSearchRequestDto request);

    /// <summary>
    /// Get repair job statistics
    /// </summary>
    /// <param name="branchId">Branch ID filter</param>
    /// <param name="fromDate">From date filter</param>
    /// <param name="toDate">To date filter</param>
    /// <returns>Repair job statistics</returns>
    Task<RepairJobStatisticsDto> GetRepairJobStatisticsAsync(int? branchId = null, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Get repair jobs by status
    /// </summary>
    /// <param name="statusId">Repair status ID</param>
    /// <param name="branchId">Branch ID filter</param>
    /// <returns>List of repair jobs</returns>
    Task<List<RepairJobDto>> GetRepairJobsByStatusAsync(int statusId, int? branchId = null);

    /// <summary>
    /// Get repair jobs by technician
    /// </summary>
    /// <param name="technicianId">Technician ID</param>
    /// <param name="branchId">Branch ID filter</param>
    /// <returns>List of repair jobs</returns>
    Task<List<RepairJobDto>> GetRepairJobsByTechnicianAsync(int technicianId, int? branchId = null);

    /// <summary>
    /// Get overdue repair jobs
    /// </summary>
    /// <param name="branchId">Branch ID filter</param>
    /// <returns>List of overdue repair jobs</returns>
    Task<List<RepairJobDto>> GetOverdueRepairJobsAsync(int? branchId = null);

    /// <summary>
    /// Get repair jobs due today
    /// </summary>
    /// <param name="branchId">Branch ID filter</param>
    /// <returns>List of repair jobs due today</returns>
    Task<List<RepairJobDto>> GetRepairJobsDueTodayAsync(int? branchId = null);
}
