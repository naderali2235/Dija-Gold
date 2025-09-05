using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Controller for raw gold balance and supplier-centralized gold management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RawGoldBalanceController : ControllerBase
{
    private readonly IRawGoldBalanceService _rawGoldBalanceService;
    private readonly ILogger<RawGoldBalanceController> _logger;

    public RawGoldBalanceController(
        IRawGoldBalanceService rawGoldBalanceService,
        ILogger<RawGoldBalanceController> logger)
    {
        _rawGoldBalanceService = rawGoldBalanceService ?? throw new ArgumentNullException(nameof(rawGoldBalanceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get supplier gold balances for a branch
    /// </summary>
    [HttpGet("supplier-balances/{branchId}")]
    [Authorize(Policy = "ManagerOrAbove")]
    [ProducesResponseType(typeof(ApiResponse<List<SupplierGoldBalanceDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<SupplierGoldBalanceDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSupplierBalances(int branchId, [FromQuery] int? supplierId = null)
    {
        try
        {
            var result = await _rawGoldBalanceService.GetSupplierBalancesAsync(branchId, supplierId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting supplier balances for branch {BranchId}", branchId);
            return StatusCode(500, ApiResponse<List<SupplierGoldBalanceDto>>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Get merchant's own raw gold balance
    /// </summary>
    [HttpGet("merchant-balance/{branchId}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<List<MerchantRawGoldBalanceDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMerchantBalance(int branchId)
    {
        try
        {
            var result = await _rawGoldBalanceService.GetMerchantRawGoldBalanceAsync(branchId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting merchant balance for branch {BranchId}", branchId);
            return StatusCode(500, ApiResponse<List<MerchantRawGoldBalanceDto>>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Get comprehensive gold balance summary
    /// </summary>
    [HttpGet("summary/{branchId}")]
    [Authorize(Policy = "ManagerOrAbove")]
    [ProducesResponseType(typeof(ApiResponse<GoldBalanceSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGoldBalanceSummary(int branchId)
    {
        try
        {
            var result = await _rawGoldBalanceService.GetGoldBalanceSummaryAsync(branchId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gold balance summary for branch {BranchId}", branchId);
            return StatusCode(500, ApiResponse<GoldBalanceSummaryDto>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Waive customer-purchased gold to a supplier to reduce debt
    /// </summary>
    [HttpPost("waive-to-supplier")]
    [Authorize(Policy = "ManagerOrAbove")]
    [ProducesResponseType(typeof(ApiResponse<RawGoldTransferDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<RawGoldTransferDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> WaiveGoldToSupplier([FromBody] WaiveGoldToSupplierRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<RawGoldTransferDto>.ErrorResponse("Invalid request data"));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<RawGoldTransferDto>.ErrorResponse("User not authenticated"));
            }

            var result = await _rawGoldBalanceService.WaiveGoldToSupplierAsync(request, userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetTransferById), new { id = result.Data!.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error waiving gold to supplier");
            return StatusCode(500, ApiResponse<RawGoldTransferDto>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Convert gold from one karat type to another
    /// </summary>
    [HttpPost("convert-karat")]
    [Authorize(Policy = "ManagerOrAbove")]
    [ProducesResponseType(typeof(ApiResponse<RawGoldTransferDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<RawGoldTransferDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConvertGoldKarat([FromBody] ConvertGoldKaratRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<RawGoldTransferDto>.ErrorResponse("Invalid request data"));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<RawGoldTransferDto>.ErrorResponse("User not authenticated"));
            }

            var result = await _rawGoldBalanceService.ConvertGoldKaratAsync(request, userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetTransferById), new { id = result.Data!.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting gold karat");
            return StatusCode(500, ApiResponse<RawGoldTransferDto>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Get gold transfer history with filtering and pagination
    /// </summary>
    [HttpGet("transfers")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RawGoldTransferDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransferHistory([FromQuery] GoldTransferSearchRequest searchRequest)
    {
        try
        {
            var result = await _rawGoldBalanceService.GetTransferHistoryAsync(searchRequest);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transfer history");
            return StatusCode(500, ApiResponse<PagedResult<RawGoldTransferDto>>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Get a specific gold transfer by ID
    /// </summary>
    [HttpGet("transfers/{id}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<RawGoldTransferDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RawGoldTransferDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransferById(int id)
    {
        try
        {
            // For now, we'll get it from transfer history with a specific filter
            var searchRequest = new GoldTransferSearchRequest
            {
                PageNumber = 1,
                PageSize = 1
            };
            
            var result = await _rawGoldBalanceService.GetTransferHistoryAsync(searchRequest);
            
            if (!result.Success || !result.Data!.Items.Any())
            {
                return NotFound(ApiResponse<RawGoldTransferDto>.ErrorResponse("Transfer not found"));
            }

            var transfer = result.Data.Items.FirstOrDefault(t => t.Id == id);
            if (transfer == null)
            {
                return NotFound(ApiResponse<RawGoldTransferDto>.ErrorResponse("Transfer not found"));
            }

            return Ok(ApiResponse<RawGoldTransferDto>.SuccessResponse(transfer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transfer by ID {TransferId}", id);
            return StatusCode(500, ApiResponse<RawGoldTransferDto>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Calculate karat conversion preview without creating a transfer
    /// </summary>
    [HttpGet("calculate-conversion")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<KaratConversionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<KaratConversionDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CalculateKaratConversion(
        [FromQuery] int fromKaratTypeId, 
        [FromQuery] int toKaratTypeId, 
        [FromQuery] decimal fromWeight)
    {
        try
        {
            if (fromWeight <= 0)
            {
                return BadRequest(ApiResponse<KaratConversionDto>.ErrorResponse("Weight must be greater than zero"));
            }

            var result = await _rawGoldBalanceService.CalculateKaratConversionAsync(fromKaratTypeId, toKaratTypeId, fromWeight);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating karat conversion");
            return StatusCode(500, ApiResponse<KaratConversionDto>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Get available merchant gold for waiving
    /// </summary>
    [HttpGet("available-for-waiving/{branchId}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<List<MerchantRawGoldBalanceDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableGoldForWaiving(int branchId, [FromQuery] int? karatTypeId = null)
    {
        try
        {
            var result = await _rawGoldBalanceService.GetAvailableGoldForWaivingAsync(branchId, karatTypeId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available gold for waiving for branch {BranchId}", branchId);
            return StatusCode(500, ApiResponse<List<MerchantRawGoldBalanceDto>>.ErrorResponse("Internal server error"));
        }
    }
}
