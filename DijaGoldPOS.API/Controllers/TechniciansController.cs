using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Controller for technician operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TechniciansController : ControllerBase
{
    private readonly ITechnicianService _technicianService;
    private readonly ILogger<TechniciansController> _logger;

    public TechniciansController(
        ITechnicianService technicianService,
        ILogger<TechniciansController> logger)
    {
        _technicianService = technicianService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new technician
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTechnician([FromBody] CreateTechnicianRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var (isSuccess, errorMessage, technician) = await _technicianService.CreateTechnicianAsync(request, userId);

            if (!isSuccess)
            {
                return BadRequest(ApiResponse.ErrorResponse(errorMessage ?? "Failed to create technician"));
            }

            if (technician == null)
            {
                return BadRequest(ApiResponse.ErrorResponse("Technician creation failed"));
            }

            _logger.LogInformation("Technician {TechnicianId} created successfully by user {UserId}", 
                technician.Id, userId);

            return CreatedAtAction(nameof(GetTechnician), new { id = technician.Id }, 
                ApiResponse<TechnicianDto>.SuccessResponse(technician, "Technician created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating technician");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while creating the technician"));
        }
    }

    /// <summary>
    /// Get technician by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTechnician(int id)
    {
        try
        {
            var technician = await _technicianService.GetTechnicianAsync(id);

            if (technician == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Technician not found"));
            }

            return Ok(ApiResponse<TechnicianDto>.SuccessResponse(technician));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving technician {TechnicianId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving the technician"));
        }
    }

    /// <summary>
    /// Update technician
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTechnician(int id, [FromBody] UpdateTechnicianRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var (isSuccess, errorMessage) = await _technicianService.UpdateTechnicianAsync(id, request, userId);

            if (!isSuccess)
            {
                return BadRequest(ApiResponse.ErrorResponse(errorMessage ?? "Failed to update technician"));
            }

            _logger.LogInformation("Technician {TechnicianId} updated successfully by user {UserId}", 
                id, userId);

            return Ok(ApiResponse.SuccessResponse("Technician updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating technician {TechnicianId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating the technician"));
        }
    }

    /// <summary>
    /// Delete technician (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteTechnician(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var (isSuccess, errorMessage) = await _technicianService.DeleteTechnicianAsync(id, userId);

            if (!isSuccess)
            {
                return BadRequest(ApiResponse.ErrorResponse(errorMessage ?? "Failed to delete technician"));
            }

            _logger.LogInformation("Technician {TechnicianId} deleted successfully by user {UserId}", 
                id, userId);

            return Ok(ApiResponse.SuccessResponse("Technician deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting technician {TechnicianId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting the technician"));
        }
    }

    /// <summary>
    /// Search technicians
    /// </summary>
    [HttpGet("search")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<TechnicianDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchTechnicians([FromQuery] TechnicianSearchRequestDto request)
    {
        try
        {
            var (items, totalCount) = await _technicianService.SearchTechniciansAsync(request);

            var response = new PaginatedResponse<TechnicianDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };

            return Ok(ApiResponse<PaginatedResponse<TechnicianDto>>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching technicians");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while searching technicians"));
        }
    }

    /// <summary>
    /// Get all active technicians
    /// </summary>
    [HttpGet("active")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<List<TechnicianDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveTechnicians([FromQuery] int? branchId = null)
    {
        try
        {
            var technicians = await _technicianService.GetActiveTechniciansAsync(branchId);

            return Ok(ApiResponse<List<TechnicianDto>>.SuccessResponse(technicians));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active technicians");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving active technicians"));
        }
    }

    /// <summary>
    /// Get technicians by branch
    /// </summary>
    [HttpGet("by-branch/{branchId}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<List<TechnicianDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTechniciansByBranch(int branchId)
    {
        try
        {
            var technicians = await _technicianService.GetTechniciansByBranchAsync(branchId);

            return Ok(ApiResponse<List<TechnicianDto>>.SuccessResponse(technicians));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving technicians for branch {BranchId}", branchId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving technicians"));
        }
    }
}
