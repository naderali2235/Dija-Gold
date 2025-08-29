using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Controller for customer purchase operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomerPurchaseController : ControllerBase
{
    private readonly ICustomerPurchaseService _customerPurchaseService;
    private readonly ILogger<CustomerPurchaseController> _logger;

    public CustomerPurchaseController(
        ICustomerPurchaseService customerPurchaseService,
        ILogger<CustomerPurchaseController> logger)
    {
        _customerPurchaseService = customerPurchaseService ?? throw new ArgumentNullException(nameof(customerPurchaseService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Create a new customer purchase
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<CustomerPurchaseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CustomerPurchaseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePurchase([FromBody] CreateCustomerPurchaseRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<CustomerPurchaseDto>.ErrorResponse("Invalid request data"));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<CustomerPurchaseDto>.ErrorResponse("User not authenticated"));
            }

            var result = await _customerPurchaseService.CreatePurchaseAsync(request, userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetPurchaseById), new { id = result.Data?.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer purchase");
            return StatusCode(500, ApiResponse<CustomerPurchaseDto>.ErrorResponse("An error occurred while creating the purchase"));
        }
    }

    /// <summary>
    /// Get customer purchase by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<CustomerPurchaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerPurchaseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPurchaseById(int id)
    {
        try
        {
            var result = await _customerPurchaseService.GetPurchaseByIdAsync(id);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer purchase {PurchaseId}", id);
            return StatusCode(500, ApiResponse<CustomerPurchaseDto>.ErrorResponse("An error occurred while retrieving the purchase"));
        }
    }

    /// <summary>
    /// Get customer purchase by purchase number
    /// </summary>
    [HttpGet("by-number/{purchaseNumber}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<CustomerPurchaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerPurchaseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPurchaseByNumber(string purchaseNumber)
    {
        try
        {
            var result = await _customerPurchaseService.GetPurchaseByNumberAsync(purchaseNumber);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer purchase {PurchaseNumber}", purchaseNumber);
            return StatusCode(500, ApiResponse<CustomerPurchaseDto>.ErrorResponse("An error occurred while retrieving the purchase"));
        }
    }

    /// <summary>
    /// Get customer purchases with pagination and filtering
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CustomerPurchaseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPurchases([FromQuery] CustomerPurchaseSearchRequest searchRequest)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<PagedResult<CustomerPurchaseDto>>.ErrorResponse("Invalid search parameters"));
            }

            var result = await _customerPurchaseService.GetPurchasesAsync(searchRequest);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer purchases");
            return StatusCode(500, ApiResponse<PagedResult<CustomerPurchaseDto>>.ErrorResponse("An error occurred while retrieving purchases"));
        }
    }

    /// <summary>
    /// Get customer purchases by customer ID
    /// </summary>
    [HttpGet("by-customer/{customerId}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<List<CustomerPurchaseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPurchasesByCustomer(int customerId)
    {
        try
        {
            var result = await _customerPurchaseService.GetPurchasesByCustomerAsync(customerId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer purchases for customer {CustomerId}", customerId);
            return StatusCode(500, ApiResponse<List<CustomerPurchaseDto>>.ErrorResponse("An error occurred while retrieving purchases"));
        }
    }

    /// <summary>
    /// Get customer purchases by branch ID
    /// </summary>
    [HttpGet("by-branch/{branchId}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<List<CustomerPurchaseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPurchasesByBranch(int branchId)
    {
        try
        {
            var result = await _customerPurchaseService.GetPurchasesByBranchAsync(branchId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer purchases for branch {BranchId}", branchId);
            return StatusCode(500, ApiResponse<List<CustomerPurchaseDto>>.ErrorResponse("An error occurred while retrieving purchases"));
        }
    }

    /// <summary>
    /// Get customer purchases within date range
    /// </summary>
    [HttpGet("by-date-range")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<List<CustomerPurchaseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPurchasesByDateRange([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        try
        {
            var result = await _customerPurchaseService.GetPurchasesByDateRangeAsync(fromDate, toDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer purchases for date range {FromDate} to {ToDate}", fromDate, toDate);
            return StatusCode(500, ApiResponse<List<CustomerPurchaseDto>>.ErrorResponse("An error occurred while retrieving purchases"));
        }
    }

    /// <summary>
    /// Update customer purchase payment status
    /// </summary>
    [HttpPut("{id}/payment")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<CustomerPurchaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerPurchaseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] UpdatePaymentRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<CustomerPurchaseDto>.ErrorResponse("User not authenticated"));
            }

            var result = await _customerPurchaseService.UpdatePaymentStatusAsync(id, request.AmountPaid, userId);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment status for purchase {PurchaseId}", id);
            return StatusCode(500, ApiResponse<CustomerPurchaseDto>.ErrorResponse("An error occurred while updating payment status"));
        }
    }

    /// <summary>
    /// Cancel customer purchase
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelPurchase(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));
            }

            var result = await _customerPurchaseService.CancelPurchaseAsync(id, userId);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling purchase {PurchaseId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while cancelling the purchase"));
        }
    }

    /// <summary>
    /// Get customer purchase summary for a date range
    /// </summary>
    [HttpGet("summary")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<CustomerPurchaseSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPurchaseSummary([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] int? branchId = null)
    {
        try
        {
            var result = await _customerPurchaseService.GetPurchaseSummaryAsync(fromDate, toDate, branchId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving purchase summary for date range {FromDate} to {ToDate}", fromDate, toDate);
            return StatusCode(500, ApiResponse<CustomerPurchaseSummaryDto>.ErrorResponse("An error occurred while retrieving purchase summary"));
        }
    }
}

/// <summary>
/// Update payment request DTO
/// </summary>
public class UpdatePaymentRequest
{
    public decimal AmountPaid { get; set; }
}
