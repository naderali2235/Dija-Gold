using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Controller for cash drawer operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CashDrawerController : ControllerBase
{
    private readonly ICashDrawerService _cashDrawerService;
    private readonly IAuditService _auditService;
    private readonly ILogger<CashDrawerController> _logger;

    public CashDrawerController(
        ICashDrawerService cashDrawerService,
        IAuditService auditService,
        ILogger<CashDrawerController> logger)
    {
        _cashDrawerService = cashDrawerService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Open cash drawer for a branch
    /// </summary>
    /// <param name="request">Cash drawer open request</param>
    /// <returns>Cash drawer balance</returns>
    [HttpPost("open")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<CashDrawerBalance>), StatusCodes.Status200OK)]
    public async Task<IActionResult> OpenDrawer([FromBody] OpenCashDrawerRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            
            var cashDrawerBalance = await _cashDrawerService.OpenDrawerAsync(
                request.BranchId, 
                request.OpeningBalance, 
                userId, 
                request.Date, 
                request.Notes);

            await _auditService.LogAsync(
                userId,
                "OPEN_CASH_DRAWER",
                "CashDrawerBalance",
                cashDrawerBalance.Id.ToString(),
                $"Opened cash drawer for branch {request.BranchId} with balance {request.OpeningBalance}",
                branchId: request.BranchId
            );

            return Ok(ApiResponse<CashDrawerBalance>.SuccessResponse(cashDrawerBalance));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening cash drawer for branch {BranchId}", request.BranchId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while opening the cash drawer"));
        }
    }

    /// <summary>
    /// Close cash drawer for a branch
    /// </summary>
    /// <param name="request">Cash drawer close request</param>
    /// <returns>Cash drawer balance</returns>
    [HttpPost("close")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<CashDrawerBalance>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CloseDrawer([FromBody] CloseCashDrawerRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            
            var cashDrawerBalance = await _cashDrawerService.CloseDrawerAsync(
                request.BranchId, 
                request.ActualClosingBalance, 
                userId, 
                request.Date, 
                request.Notes);

            await _auditService.LogAsync(
                userId,
                "CLOSE_CASH_DRAWER",
                "CashDrawerBalance",
                cashDrawerBalance.Id.ToString(),
                $"Closed cash drawer for branch {request.BranchId} with actual balance {request.ActualClosingBalance}",
                branchId: request.BranchId
            );

            return Ok(ApiResponse<CashDrawerBalance>.SuccessResponse(cashDrawerBalance));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing cash drawer for branch {BranchId}", request.BranchId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while closing the cash drawer"));
        }
    }

    /// <summary>
    /// Get cash drawer balance for a specific date
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="date">Balance date</param>
    /// <returns>Cash drawer balance</returns>
    [HttpGet("balance")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<CashDrawerBalance>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalance([FromQuery] int branchId, [FromQuery] DateTime date)
    {
        try
        {
            var balance = await _cashDrawerService.GetBalanceAsync(branchId, date);
            
            if (balance == null)
            {
                return NotFound(ApiResponse.ErrorResponse("No cash drawer balance found for the specified date"));
            }

            return Ok(ApiResponse<CashDrawerBalance>.SuccessResponse(balance));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cash drawer balance for branch {BranchId} on {Date}", branchId, date);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving the cash drawer balance"));
        }
    }

    /// <summary>
    /// Get opening balance for a specific date
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="date">Date to get opening balance for</param>
    /// <returns>Opening balance amount</returns>
    [HttpGet("opening-balance")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOpeningBalance([FromQuery] int branchId, [FromQuery] DateTime date)
    {
        try
        {
            var openingBalance = await _cashDrawerService.GetOpeningBalanceAsync(branchId, date);
            return Ok(ApiResponse<decimal>.SuccessResponse(openingBalance));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting opening balance for branch {BranchId} on {Date}", branchId, date);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving the opening balance"));
        }
    }

    /// <summary>
    /// Check if cash drawer is open for a specific date
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="date">Date to check</param>
    /// <returns>True if drawer is open</returns>
    [HttpGet("is-open")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> IsDrawerOpen([FromQuery] int branchId, [FromQuery] DateTime date)
    {
        try
        {
            var isOpen = await _cashDrawerService.IsDrawerOpenAsync(branchId, date);
            return Ok(ApiResponse<bool>.SuccessResponse(isOpen));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if cash drawer is open for branch {BranchId} on {Date}", branchId, date);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while checking cash drawer status"));
        }
    }

    /// <summary>
    /// Get cash drawer balances for a date range
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>List of cash drawer balances</returns>
    [HttpGet("balances")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<List<CashDrawerBalance>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalances([FromQuery] int branchId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        try
        {
            var balances = await _cashDrawerService.GetBalancesByDateRangeAsync(branchId, fromDate, toDate);
            return Ok(ApiResponse<List<CashDrawerBalance>>.SuccessResponse(balances));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cash drawer balances for branch {BranchId} from {FromDate} to {ToDate}", branchId, fromDate, toDate);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving cash drawer balances"));
        }
    }

    /// <summary>
    /// Settle shift by closing current day and carrying forward remaining balance to next day
    /// </summary>
    /// <param name="request">Shift settlement request</param>
    /// <returns>Updated cash drawer balance</returns>
    [HttpPost("settle-shift")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<CashDrawerBalance>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SettleShift([FromBody] SettleShiftRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            
            var cashDrawerBalance = await _cashDrawerService.SettleShiftAsync(
                request.BranchId, 
                request.ActualClosingBalance, 
                request.SettledAmount,
                userId, 
                request.Date, 
                request.SettlementNotes,
                request.Notes);

            await _auditService.LogAsync(
                userId,
                "SETTLE_SHIFT",
                "CashDrawerBalance",
                cashDrawerBalance.Id.ToString(),
                $"Settled shift for branch {request.BranchId}. Actual: {request.ActualClosingBalance}, Settled: {request.SettledAmount}, Carried Forward: {request.ActualClosingBalance - request.SettledAmount}",
                branchId: request.BranchId
            );

            return Ok(ApiResponse<CashDrawerBalance>.SuccessResponse(cashDrawerBalance));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error settling shift for branch {BranchId}", request.BranchId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while settling the shift"));
        }
    }

    /// <summary>
    /// Refresh expected closing balance for an open cash drawer
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="date">Date to refresh balance for</param>
    /// <returns>Updated cash drawer balance</returns>
    [HttpPost("refresh-balance")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<CashDrawerBalance>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshExpectedClosingBalance([FromQuery] int branchId, [FromQuery] DateTime? date = null)
    {
        try
        {
            var refreshedBalance = await _cashDrawerService.RefreshExpectedClosingBalanceAsync(branchId, date);
            
            return Ok(ApiResponse<CashDrawerBalance>.SuccessResponse(refreshedBalance));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing cash drawer balance for branch {BranchId}", branchId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while refreshing the cash drawer balance"));
        }
    }
}

/// <summary>
/// Request model for opening cash drawer
/// </summary>
public class OpenCashDrawerRequest
{
    /// <summary>
    /// Branch ID
    /// </summary>
    public int BranchId { get; set; }
    
    /// <summary>
    /// Opening balance amount
    /// </summary>
    public decimal OpeningBalance { get; set; }
    
    /// <summary>
    /// Date to open drawer for (optional, defaults to today)
    /// </summary>
    public DateTime? Date { get; set; }
    
    /// <summary>
    /// Optional notes
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Request model for closing cash drawer
/// </summary>
public class CloseCashDrawerRequest
{
    /// <summary>
    /// Branch ID
    /// </summary>
    public int BranchId { get; set; }
    
    /// <summary>
    /// Actual closing balance amount
    /// </summary>
    public decimal ActualClosingBalance { get; set; }
    
    /// <summary>
    /// Date to close drawer for (optional, defaults to today)
    /// </summary>
    public DateTime? Date { get; set; }
    
    /// <summary>
    /// Optional notes
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Request model for settling and closing shift with balance carry forward
/// </summary>
public class SettleShiftRequest
{
    /// <summary>
    /// Branch ID
    /// </summary>
    public int BranchId { get; set; }
    
    /// <summary>
    /// Actual closing balance amount (total cash in drawer)
    /// </summary>
    public decimal ActualClosingBalance { get; set; }
    
    /// <summary>
    /// Amount to settle/remove from drawer (e.g., bank deposit)
    /// </summary>
    public decimal SettledAmount { get; set; }
    
    /// <summary>
    /// Date to settle shift for (optional, defaults to today)
    /// </summary>
    public DateTime? Date { get; set; }
    
    /// <summary>
    /// Settlement notes/reason
    /// </summary>
    public string? SettlementNotes { get; set; }
    
    /// <summary>
    /// General notes about the shift
    /// </summary>
    public string? Notes { get; set; }
}