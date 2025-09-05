using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Customers controller for customer management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IAuditService _auditService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(
        ICustomerService customerService,
        IAuditService auditService,
        ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Get all customers with optional filtering and pagination
    /// </summary>
    /// <param name="searchRequest">Search parameters</param>
    /// <returns>List of customers</returns>
    [HttpGet]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CustomerDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomers([FromQuery] CustomerSearchRequestDto searchRequest)
    {
        try
        {
            var result = await _customerService.GetCustomersAsync(searchRequest);
            return Ok(ApiResponse<PagedResult<CustomerDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving customers"));
        }
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer details</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomer(int id)
    {
        try
        {
            var customerDto = await _customerService.GetCustomerByIdAsync(id);

            if (customerDto == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Customer not found"));
            }

            return Ok(ApiResponse<CustomerDto>.SuccessResponse(customerDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer with ID {CustomerId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving the customer"));
        }
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    /// <param name="request">Customer creation request</param>
    /// <returns>Created customer</returns>
    [HttpPost]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequestDto request)
    {
        try
        {
            var customerDto = await _customerService.CreateCustomerAsync(request);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "CREATE",
                "Customer",
                customerDto.Id.ToString(),
                $"Created customer: {customerDto.FullName}"
            );

            return CreatedAtAction(nameof(GetCustomer), new { id = customerDto.Id }, 
                ApiResponse<CustomerDto>.SuccessResponse(customerDto));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while creating the customer"));
        }
    }

    /// <summary>
    /// Update an existing customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="request">Customer update request</param>
    /// <returns>Updated customer</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerRequestDto request)
    {
        try
        {
            var customerDto = await _customerService.UpdateCustomerAsync(id, request);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "UPDATE",
                "Customer",
                customerDto.Id.ToString(),
                $"Updated customer: {customerDto.FullName}"
            );

            return Ok(ApiResponse<CustomerDto>.SuccessResponse(customerDto));
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(ApiResponse.ErrorResponse(ex.Message));
            }
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer with ID {CustomerId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating the customer"));
        }
    }

    /// <summary>
    /// Soft delete a customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        try
        {
            var success = await _customerService.DeleteCustomerAsync(id);
            
            if (!success)
            {
                return NotFound(ApiResponse.ErrorResponse("Customer not found"));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "DELETE",
                "Customer",
                id.ToString(),
                $"Soft deleted customer with ID: {id}"
            );

            return Ok(ApiResponse.SuccessResponse("Customer deleted successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer with ID {CustomerId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting the customer"));
        }
    }

    /// <summary>
    /// Get customer Orders history
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="fromDate">From date (optional)</param>
    /// <param name="toDate">To date (optional)</param>
    /// <returns>Customer Orders history</returns>
    [HttpGet("{id}/Orders")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<CustomerOrdersHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerOrders(
        int id,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var result = await _customerService.GetCustomerOrdersAsync(id, fromDate, toDate);
            return Ok(ApiResponse<CustomerOrdersHistoryDto>.SuccessResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Order history for customer {CustomerId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving Orders history"));
        }
    }

    /// <summary>
    /// Get customer loyalty status
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer loyalty information</returns>
    [HttpGet("{id}/loyalty")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<CustomerLoyaltyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerLoyalty(int id)
    {
        try
        {
            var loyaltyDto = await _customerService.GetCustomerLoyaltyAsync(id);
            return Ok(ApiResponse<CustomerLoyaltyDto>.SuccessResponse(loyaltyDto));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving loyalty status for customer {CustomerId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving loyalty status"));
        }
    }

    /// <summary>
    /// Update customer loyalty status
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="request">Loyalty update request</param>
    /// <returns>Updated loyalty information</returns>
    [HttpPut("{id}/loyalty")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<CustomerLoyaltyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCustomerLoyalty(int id, [FromBody] UpdateCustomerLoyaltyRequestDto request)
    {
        try
        {
            var loyaltyDto = await _customerService.UpdateCustomerLoyaltyAsync(id, request);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "UPDATE",
                "CustomerLoyalty",
                loyaltyDto.CustomerId.ToString(),
                $"Updated loyalty for customer: {loyaltyDto.CustomerName} - Tier: {loyaltyDto.CurrentTier}, Points: {loyaltyDto.CurrentPoints}"
            );

            return Ok(ApiResponse<CustomerLoyaltyDto>.SuccessResponse(loyaltyDto));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating loyalty status for customer {CustomerId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating loyalty status"));
        }
    }

    /// <summary>
    /// Search customers by various criteria
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="limit">Maximum number of results</param>
    /// <returns>List of matching customers</returns>
    [HttpGet("search")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<List<CustomerDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchCustomers(
        [FromQuery] string searchTerm,
        [FromQuery] int limit = 10)
    {
        try
        {
            var customers = await _customerService.SearchCustomersAsync(searchTerm, limit);
            return Ok(ApiResponse<List<CustomerDto>>.SuccessResponse(customers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching customers with term: {SearchTerm}", searchTerm);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while searching customers"));
        }
    }
}
