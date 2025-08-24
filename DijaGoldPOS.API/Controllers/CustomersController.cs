using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Services;
using DijaGoldPOS.API.Shared;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<CustomersController> _logger;
    private readonly IMapper _mapper;

    public CustomersController(
        ApplicationDbContext context,
        IAuditService auditService,
        ILogger<CustomersController> logger,
        IMapper mapper)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
        _mapper = mapper;
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
            var query = _context.Customers.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchRequest.SearchTerm))
            {
                var searchTerm = searchRequest.SearchTerm.ToLower();
                query = query.Where(c => 
                    c.FullName.ToLower().Contains(searchTerm) ||
                    (c.NationalId != null && c.NationalId.ToLower().Contains(searchTerm)) ||
                    (c.MobileNumber != null && c.MobileNumber.Contains(searchTerm)) ||
                    (c.Email != null && c.Email.ToLower().Contains(searchTerm))
                );
            }

            if (!string.IsNullOrWhiteSpace(searchRequest.NationalId))
            {
                query = query.Where(c => c.NationalId == searchRequest.NationalId);
            }

            if (!string.IsNullOrWhiteSpace(searchRequest.MobileNumber))
            {
                query = query.Where(c => c.MobileNumber == searchRequest.MobileNumber);
            }

            if (!string.IsNullOrWhiteSpace(searchRequest.Email))
            {
                query = query.Where(c => c.Email == searchRequest.Email);
            }

            if (searchRequest.LoyaltyTier.HasValue)
            {
                query = query.Where(c => c.LoyaltyTier == searchRequest.LoyaltyTier.Value);
            }

            if (searchRequest.IsActive.HasValue)
            {
                query = query.Where(c => c.IsActive == searchRequest.IsActive.Value);
            }

            // Apply sorting
            query = query.OrderByDescending(c => c.CreatedAt);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var customers = await query
                .Skip((searchRequest.PageNumber - 1) * searchRequest.PageSize)
                .Take(searchRequest.PageSize)
                .ProjectTo<CustomerDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var result = new PagedResult<CustomerDto>
            {
                Items = customers,
                TotalCount = totalCount,
                PageNumber = searchRequest.PageNumber,
                PageSize = searchRequest.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / searchRequest.PageSize)
            };

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
            var customerDto = await _context.Customers
                .Where(c => c.Id == id)
                .ProjectTo<CustomerDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

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
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid input", ModelState));
            }

            // Check for duplicate National ID
            if (!string.IsNullOrWhiteSpace(request.NationalId))
            {
                var existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.NationalId == request.NationalId);
                if (existingCustomer != null)
                {
                    return BadRequest(ApiResponse.ErrorResponse("A customer with this National ID already exists"));
                }
            }

            // Check for duplicate mobile number
            if (!string.IsNullOrWhiteSpace(request.MobileNumber))
            {
                var existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.MobileNumber == request.MobileNumber);
                if (existingCustomer != null)
                {
                    return BadRequest(ApiResponse.ErrorResponse("A customer with this mobile number already exists"));
                }
            }

            // Check for duplicate email
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == request.Email);
                if (existingCustomer != null)
                {
                    return BadRequest(ApiResponse.ErrorResponse("A customer with this email already exists"));
                }
            }

            var customer = _mapper.Map<Customer>(request);
            customer.RegistrationDate = DateTime.UtcNow;

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "CREATE",
                "Customer",
                customer.Id.ToString(),
                $"Created customer: {customer.FullName}"
            );

            var customerDto = _mapper.Map<CustomerDto>(customer);

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, 
                ApiResponse<CustomerDto>.SuccessResponse(customerDto));
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid input", ModelState));
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Customer not found"));
            }

            // Check for duplicate National ID (excluding current customer)
            if (!string.IsNullOrWhiteSpace(request.NationalId) && request.NationalId != customer.NationalId)
            {
                var existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.NationalId == request.NationalId && c.Id != id);
                if (existingCustomer != null)
                {
                    return BadRequest(ApiResponse.ErrorResponse("A customer with this National ID already exists"));
                }
            }

            // Check for duplicate mobile number (excluding current customer)
            if (!string.IsNullOrWhiteSpace(request.MobileNumber) && request.MobileNumber != customer.MobileNumber)
            {
                var existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.MobileNumber == request.MobileNumber && c.Id != id);
                if (existingCustomer != null)
                {
                    return BadRequest(ApiResponse.ErrorResponse("A customer with this mobile number already exists"));
                }
            }

            // Check for duplicate email (excluding current customer)
            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != customer.Email)
            {
                var existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == request.Email && c.Id != id);
                if (existingCustomer != null)
                {
                    return BadRequest(ApiResponse.ErrorResponse("A customer with this email already exists"));
                }
            }

            // Update customer properties via AutoMapper
            _mapper.Map(request, customer);
            customer.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "UPDATE",
                "Customer",
                customer.Id.ToString(),
                $"Updated customer: {customer.FullName}"
            );

            var customerDto = _mapper.Map<CustomerDto>(customer);

            return Ok(ApiResponse<CustomerDto>.SuccessResponse(customerDto));
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
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Customer not found"));
            }

            // Check if customer has active Orders
            var hasActiveOrders = await _context.Orders
                .AnyAsync(o => o.CustomerId == id && o.StatusId != LookupTableConstants.OrderStatusCancelled);

            if (hasActiveOrders)
            {
                return BadRequest(ApiResponse.ErrorResponse("Cannot delete customer with active Orders"));
            }

            customer.IsActive = false;
            customer.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "DELETE",
                "Customer",
                customer.Id.ToString(),
                $"Soft deleted customer: {customer.FullName}"
            );

            return Ok(ApiResponse.SuccessResponse("Customer deleted successfully"));
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
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Customer not found"));
            }

            var query = _context.Orders
                .Include(o => o.Branch)
                .Include(o => o.Cashier)
                .Where(o => o.CustomerId == id && o.OrderTypeId == LookupTableConstants.OrderTypeSale);

            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= toDate.Value);
            }

            var Orders = await query
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .Take(100) // Limit to last 100 Orders
                .Select(o => new CustomerOrderDto
                {
                    OrderId = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    OrderType = "Sale", // OrderTypeSale
                    TotalAmount = o.OrderItems.Sum(oi => oi.TotalAmount),
                    BranchName = o.Branch != null ? o.Branch.Name : string.Empty,
                    CashierName = o.Cashier != null ? o.Cashier.FullName : string.Empty
                })
                .ToListAsync();

            var totalAmount = await query
                .Include(o => o.OrderItems)
                .SumAsync(o => o.OrderItems.Sum(oi => oi.TotalAmount));
            var totalOrdersCount = await query.CountAsync();

            var result = new CustomerOrdersHistoryDto
            {
                CustomerId = customer.Id,
                CustomerName = customer.FullName,
                Orders = Orders,
                TotalAmount = totalAmount,
                TotalOrderCount = totalOrdersCount
            };

            return Ok(ApiResponse<CustomerOrdersHistoryDto>.SuccessResponse(result));
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
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Customer not found"));
            }

            // Calculate points to next tier
            var pointsToNextTier = customer.LoyaltyTier < 5 ? 
                (customer.LoyaltyTier * 1000) - customer.LoyaltyPoints : 0;

            var loyaltyDto = new CustomerLoyaltyDto
            {
                CustomerId = customer.Id,
                CustomerName = customer.FullName,
                CurrentTier = customer.LoyaltyTier,
                CurrentPoints = customer.LoyaltyPoints,
                PointsToNextTier = pointsToNextTier,
                TotalPurchaseAmount = customer.TotalPurchaseAmount,
                DefaultDiscountPercentage = customer.DefaultDiscountPercentage,
                MakingChargesWaived = customer.MakingChargesWaived,
                LastPurchaseDate = customer.LastPurchaseDate,
                TotalOrders = customer.TotalOrders
            };

            return Ok(ApiResponse<CustomerLoyaltyDto>.SuccessResponse(loyaltyDto));
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.ErrorResponse("Invalid input", ModelState));
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Customer not found"));
            }

            // Update loyalty properties
            customer.LoyaltyTier = request.LoyaltyTier;
            customer.LoyaltyPoints = request.LoyaltyPoints;
            customer.DefaultDiscountPercentage = request.DefaultDiscountPercentage;
            customer.MakingChargesWaived = request.MakingChargesWaived;
            customer.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "UPDATE",
                "CustomerLoyalty",
                customer.Id.ToString(),
                $"Updated loyalty for customer: {customer.FullName} - Tier: {customer.LoyaltyTier}, Points: {customer.LoyaltyPoints}"
            );

            // Calculate points to next tier
            var pointsToNextTier = customer.LoyaltyTier < 5 ? 
                (customer.LoyaltyTier * 1000) - customer.LoyaltyPoints : 0;

            var loyaltyDto = new CustomerLoyaltyDto
            {
                CustomerId = customer.Id,
                CustomerName = customer.FullName,
                CurrentTier = customer.LoyaltyTier,
                CurrentPoints = customer.LoyaltyPoints,
                PointsToNextTier = pointsToNextTier,
                TotalPurchaseAmount = customer.TotalPurchaseAmount,
                DefaultDiscountPercentage = customer.DefaultDiscountPercentage,
                MakingChargesWaived = customer.MakingChargesWaived,
                LastPurchaseDate = customer.LastPurchaseDate,
                TotalOrders = customer.TotalOrders
            };

            return Ok(ApiResponse<CustomerLoyaltyDto>.SuccessResponse(loyaltyDto));
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
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Ok(ApiResponse<List<CustomerDto>>.SuccessResponse(new List<CustomerDto>()));
            }

            var query = _context.Customers
                .Where(c => c.IsActive)
                .Where(c => 
                    c.FullName.ToLower().Contains(searchTerm.ToLower()) ||
                    (c.NationalId != null && c.NationalId.Contains(searchTerm)) ||
                    (c.MobileNumber != null && c.MobileNumber.Contains(searchTerm)) ||
                    (c.Email != null && c.Email.ToLower().Contains(searchTerm.ToLower()))
                )
                .OrderBy(c => c.FullName)
                .Take(limit);

            var customers = await query
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    FullName = c.FullName,
                    NationalId = c.NationalId,
                    MobileNumber = c.MobileNumber,
                    Email = c.Email,
                    Address = c.Address,
                    RegistrationDate = c.RegistrationDate,
                    LoyaltyTier = c.LoyaltyTier,
                    LoyaltyPoints = c.LoyaltyPoints,
                    TotalPurchaseAmount = c.TotalPurchaseAmount,
                    DefaultDiscountPercentage = c.DefaultDiscountPercentage,
                    MakingChargesWaived = c.MakingChargesWaived,
                    Notes = c.Notes,
                    LastPurchaseDate = c.LastPurchaseDate,
                    TotalOrders = c.TotalOrders,
                    CreatedAt = c.CreatedAt,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            return Ok(ApiResponse<List<CustomerDto>>.SuccessResponse(customers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching customers with term: {SearchTerm}", searchTerm);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while searching customers"));
        }
    }
}