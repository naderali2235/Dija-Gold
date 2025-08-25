using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Services;
using DijaGoldPOS.API.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Controller for repair job operations and workflow management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RepairJobsController : ControllerBase
{
    private readonly IRepairJobService _repairJobService;
    private readonly ILogger<RepairJobsController> _logger;

    public RepairJobsController(
        IRepairJobService repairJobService,
        ILogger<RepairJobsController> logger)
    {
        _repairJobService = repairJobService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new repair job from a financial transaction
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<RepairJobDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRepairJob([FromBody] CreateRepairJobRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var (isSuccess, errorMessage, repairJob) = await _repairJobService.CreateRepairJobAsync(request, userId);

            if (!isSuccess)
            {
                return BadRequest(ApiResponse.ErrorResponse(errorMessage ?? "Failed to create repair job"));
            }

            if (repairJob == null)
            {
                return BadRequest(ApiResponse.ErrorResponse("Repair job creation failed"));
            }

            _logger.LogInformation("Repair job {RepairJobId} created successfully by user {UserId}", 
                repairJob.Id, userId);

            return CreatedAtAction(nameof(GetRepairJob), new { id = repairJob.Id }, 
                ApiResponse<RepairJobDto>.SuccessResponse(repairJob, "Repair job created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating repair job");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while creating the repair job"));
        }
    }

    /// <summary>
    /// Get repair job by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<RepairJobDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRepairJob(int id)
    {
        try
        {
            var repairJob = await _repairJobService.GetRepairJobAsync(id);

            if (repairJob == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Repair job not found"));
            }

            return Ok(ApiResponse<RepairJobDto>.SuccessResponse(repairJob));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving repair job {RepairJobId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving the repair job"));
        }
    }

    /// <summary>
    /// Get repair job by financial transaction ID
    /// </summary>
    [HttpGet("by-financial-transaction/{financialTransactionId}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<RepairJobDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRepairJobByFinancialTransactionId(int financialTransactionId)
    {
        try
        {
            var repairJob = await _repairJobService.GetRepairJobByFinancialTransactionIdAsync(financialTransactionId);

            if (repairJob == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Repair job not found for this financial transaction"));
            }

            return Ok(ApiResponse<RepairJobDto>.SuccessResponse(repairJob));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving repair job for financial transaction {FinancialTransactionId}", financialTransactionId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving the repair job"));
        }
    }

    /// <summary>
    /// Update repair job status
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRepairJobStatus(int id, [FromBody] UpdateRepairJobStatusRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var (isSuccess, errorMessage) = await _repairJobService.UpdateRepairJobStatusAsync(id, request, userId);

            if (!isSuccess)
            {
                return BadRequest(ApiResponse.ErrorResponse(errorMessage ?? "Failed to update repair job status"));
            }

            _logger.LogInformation("Repair job {RepairJobId} status updated to {StatusId} by user {UserId}", 
                id, request.StatusId, userId);

            return Ok(ApiResponse.SuccessResponse("Repair job status updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating repair job {RepairJobId} status", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating the repair job status"));
        }
    }

    /// <summary>
    /// Assign technician to repair job
    /// </summary>
    [HttpPut("{id}/assign")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignTechnician(int id, [FromBody] AssignTechnicianRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var (isSuccess, errorMessage) = await _repairJobService.AssignTechnicianAsync(id, request, userId);

            if (!isSuccess)
            {
                return BadRequest(ApiResponse.ErrorResponse(errorMessage ?? "Failed to assign technician"));
            }

            _logger.LogInformation("Technician {TechnicianId} assigned to repair job {RepairJobId} by user {UserId}", 
                request.TechnicianId, id, userId);

            return Ok(ApiResponse.SuccessResponse("Technician assigned successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning technician to repair job {RepairJobId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while assigning the technician"));
        }
    }

    /// <summary>
    /// Complete repair job
    /// </summary>
    [HttpPut("{id}/complete")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteRepair(int id, [FromBody] CompleteRepairRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var (isSuccess, errorMessage) = await _repairJobService.CompleteRepairAsync(id, request, userId);

            if (!isSuccess)
            {
                return BadRequest(ApiResponse.ErrorResponse(errorMessage ?? "Failed to complete repair"));
            }

            _logger.LogInformation("Repair job {RepairJobId} completed by user {UserId} with actual cost {ActualCost}", 
                id, userId, request.ActualCost);

            return Ok(ApiResponse.SuccessResponse("Repair completed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing repair job {RepairJobId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while completing the repair"));
        }
    }

    /// <summary>
    /// Mark repair as ready for pickup
    /// </summary>
    [HttpPut("{id}/ready-for-pickup")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarkReadyForPickup(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var (isSuccess, errorMessage) = await _repairJobService.MarkReadyForPickupAsync(id, userId);

            if (!isSuccess)
            {
                return BadRequest(ApiResponse.ErrorResponse(errorMessage ?? "Failed to mark repair ready for pickup"));
            }

            _logger.LogInformation("Repair job {RepairJobId} marked ready for pickup by user {UserId}", 
                id, userId);

            return Ok(ApiResponse.SuccessResponse("Repair marked ready for pickup"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking repair job {RepairJobId} ready for pickup", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while marking the repair ready for pickup"));
        }
    }

    /// <summary>
    /// Deliver repair to customer
    /// </summary>
    [HttpPut("{id}/deliver")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeliverRepair(int id, [FromBody] DeliverRepairRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var (isSuccess, errorMessage) = await _repairJobService.DeliverRepairAsync(id, request, userId);

            if (!isSuccess)
            {
                return BadRequest(ApiResponse.ErrorResponse(errorMessage ?? "Failed to deliver repair"));
            }

            _logger.LogInformation("Repair job {RepairJobId} delivered to customer by user {UserId}", 
                id, userId);

            return Ok(ApiResponse.SuccessResponse("Repair delivered successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delivering repair job {RepairJobId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while delivering the repair"));
        }
    }

    /// <summary>
    /// Cancel repair job
    /// </summary>
    [HttpPut("{id}/cancel")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelRepair(int id, [FromBody] CancelRepairRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var (isSuccess, errorMessage) = await _repairJobService.CancelRepairAsync(id, request.Reason, userId);

            if (!isSuccess)
            {
                return BadRequest(ApiResponse.ErrorResponse(errorMessage ?? "Failed to cancel repair"));
            }

            _logger.LogInformation("Repair job {RepairJobId} cancelled by user {UserId}: {Reason}", 
                id, userId, request.Reason);

            return Ok(ApiResponse.SuccessResponse("Repair cancelled successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling repair job {RepairJobId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while cancelling the repair"));
        }
    }

    /// <summary>
    /// Search repair jobs
    /// </summary>
    [HttpGet("search")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<RepairJobDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchRepairJobs([FromQuery] RepairJobSearchRequestDto request)
    {
        try
        {
            var (items, totalCount) = await _repairJobService.SearchRepairJobsAsync(request);

            var response = new PaginatedResponse<RepairJobDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };

            return Ok(ApiResponse<PaginatedResponse<RepairJobDto>>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching repair jobs");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while searching repair jobs"));
        }
    }

    /// <summary>
    /// Get repair job statistics
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<RepairJobStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRepairJobStatistics(
        [FromQuery] int? branchId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var statistics = await _repairJobService.GetRepairJobStatisticsAsync(branchId, fromDate, toDate);

            return Ok(ApiResponse<RepairJobStatisticsDto>.SuccessResponse(statistics));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving repair job statistics");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving repair job statistics"));
        }
    }

    /// <summary>
    /// Get repair jobs by status
    /// </summary>
    [HttpGet("by-status/{statusId}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<List<RepairJobDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRepairJobsByStatus(
        int statusId,
        [FromQuery] int? branchId = null)
    {
        try
        {
            var repairJobs = await _repairJobService.GetRepairJobsByStatusAsync(statusId, branchId);

            return Ok(ApiResponse<List<RepairJobDto>>.SuccessResponse(repairJobs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving repair jobs by status {StatusId}", statusId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving repair jobs"));
        }
    }

    /// <summary>
    /// Get repair jobs assigned to technician
    /// </summary>
    [HttpGet("by-technician/{technicianId}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<List<RepairJobDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRepairJobsByTechnician(
        int technicianId,
        [FromQuery] int? branchId = null)
    {
        try
        {
            var repairJobs = await _repairJobService.GetRepairJobsByTechnicianAsync(technicianId, branchId);

            return Ok(ApiResponse<List<RepairJobDto>>.SuccessResponse(repairJobs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving repair jobs for technician {TechnicianId}", technicianId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving repair jobs"));
        }
    }

    /// <summary>
    /// Get overdue repair jobs
    /// </summary>
    [HttpGet("overdue")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<List<RepairJobDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverdueRepairJobs([FromQuery] int? branchId = null)
    {
        try
        {
            var repairJobs = await _repairJobService.GetOverdueRepairJobsAsync(branchId);

            return Ok(ApiResponse<List<RepairJobDto>>.SuccessResponse(repairJobs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue repair jobs");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving overdue repair jobs"));
        }
    }

    /// <summary>
    /// Get repair jobs due today
    /// </summary>
    [HttpGet("due-today")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<List<RepairJobDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRepairJobsDueToday([FromQuery] int? branchId = null)
    {
        try
        {
            var repairJobs = await _repairJobService.GetRepairJobsDueTodayAsync(branchId);

            return Ok(ApiResponse<List<RepairJobDto>>.SuccessResponse(repairJobs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving repair jobs due today");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving repair jobs due today"));
        }
    }
}

/// <summary>
/// Cancel repair request DTO
/// </summary>
public class CancelRepairRequestDto
{
    [Required(ErrorMessage = "Cancellation reason is required")]
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string Reason { get; set; } = string.Empty;
}
