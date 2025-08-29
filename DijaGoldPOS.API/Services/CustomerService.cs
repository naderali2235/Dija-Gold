using AutoMapper;
using AutoMapper.QueryableExtensions;
using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Shared;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service implementation for customer business operations
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        IUnitOfWork unitOfWork,
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<CustomerService> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated customers with search filters
    /// </summary>
    public async Task<PagedResult<CustomerDto>> GetCustomersAsync(CustomerSearchRequestDto searchRequest)
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

        return new PagedResult<CustomerDto>
        {
            Items = customers,
            TotalCount = totalCount,
            PageNumber = searchRequest.PageNumber,
            PageSize = searchRequest.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / searchRequest.PageSize)
        };
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        return await _context.Customers
            .Where(c => c.Id == id)
            .ProjectTo<CustomerDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequestDto request)
    {
        // Check for duplicates
        if (!string.IsNullOrWhiteSpace(request.NationalId))
        {
            if (await _unitOfWork.Customers.NationalIdExistsAsync(request.NationalId))
            {
                throw new InvalidOperationException("A customer with this National ID already exists");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.MobileNumber))
        {
            if (await _unitOfWork.Customers.MobileNumberExistsAsync(request.MobileNumber))
            {
                throw new InvalidOperationException("A customer with this mobile number already exists");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (await _unitOfWork.Customers.EmailExistsAsync(request.Email))
            {
                throw new InvalidOperationException("A customer with this email already exists");
            }
        }

        var customer = _mapper.Map<Customer>(request);
        customer.RegistrationDate = DateTime.UtcNow;

        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CustomerDto>(customer);
    }

    /// <summary>
    /// Update an existing customer
    /// </summary>
    public async Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerRequestDto request)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        if (customer == null)
        {
            throw new InvalidOperationException("Customer not found");
        }

        // Check for duplicates (excluding current customer)
        if (!string.IsNullOrWhiteSpace(request.NationalId) && request.NationalId != customer.NationalId)
        {
            if (await _unitOfWork.Customers.NationalIdExistsAsync(request.NationalId, id))
            {
                throw new InvalidOperationException("A customer with this National ID already exists");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.MobileNumber) && request.MobileNumber != customer.MobileNumber)
        {
            if (await _unitOfWork.Customers.MobileNumberExistsAsync(request.MobileNumber, id))
            {
                throw new InvalidOperationException("A customer with this mobile number already exists");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != customer.Email)
        {
            if (await _unitOfWork.Customers.EmailExistsAsync(request.Email, id))
            {
                throw new InvalidOperationException("A customer with this email already exists");
            }
        }

        // Update customer properties via AutoMapper
        _mapper.Map(request, customer);
        customer.ModifiedAt = DateTime.UtcNow;

        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CustomerDto>(customer);
    }

    /// <summary>
    /// Soft delete a customer
    /// </summary>
    public async Task<bool> DeleteCustomerAsync(int id)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        if (customer == null)
        {
            return false;
        }

        // Check if customer has active Orders
        var hasActiveOrders = await _context.Orders
            .AnyAsync(o => o.CustomerId == id && o.StatusId != LookupTableConstants.OrderStatusCancelled);

        if (hasActiveOrders)
        {
            throw new InvalidOperationException("Cannot delete customer with active Orders");
        }

        customer.IsActive = false;
        customer.ModifiedAt = DateTime.UtcNow;

        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Get customer orders history
    /// </summary>
    public async Task<CustomerOrdersHistoryDto> GetCustomerOrdersAsync(int customerId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
        if (customer == null)
        {
            throw new InvalidOperationException("Customer not found");
        }

        var query = _context.Orders
            .Include(o => o.Branch)
            .Include(o => o.Cashier)
            .Where(o => o.CustomerId == customerId && o.OrderTypeId == LookupTableConstants.OrderTypeSale);

        if (fromDate.HasValue)
        {
            query = query.Where(o => o.OrderDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(o => o.OrderDate <= toDate.Value);
        }

        var orders = await query
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .Take(100) // Limit to last 100 orders
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

        // Fix: Calculate totals separately to avoid nested aggregation issues
        var orderIds = await query.Select(o => o.Id).ToListAsync();
        var totalAmount = orderIds.Any() ? await _context.OrderItems
            .Where(oi => orderIds.Contains(oi.OrderId))
            .SumAsync(oi => oi.TotalAmount) : 0;
        var totalOrdersCount = await query.CountAsync();

        return new CustomerOrdersHistoryDto
        {
            CustomerId = customer.Id,
            CustomerName = customer.FullName,
            Orders = orders,
            TotalAmount = totalAmount,
            TotalOrderCount = totalOrdersCount
        };
    }

    /// <summary>
    /// Get customer loyalty information
    /// </summary>
    public async Task<CustomerLoyaltyDto> GetCustomerLoyaltyAsync(int customerId)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
        if (customer == null)
        {
            throw new InvalidOperationException("Customer not found");
        }

        // Calculate points to next tier
        var pointsToNextTier = customer.LoyaltyTier < 5 ? 
            (customer.LoyaltyTier * 1000) - customer.LoyaltyPoints : 0;

        return new CustomerLoyaltyDto
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
    }

    /// <summary>
    /// Update customer loyalty status
    /// </summary>
    public async Task<CustomerLoyaltyDto> UpdateCustomerLoyaltyAsync(int customerId, UpdateCustomerLoyaltyRequestDto request)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
        if (customer == null)
        {
            throw new InvalidOperationException("Customer not found");
        }

        // Update loyalty properties
        customer.LoyaltyTier = request.LoyaltyTier;
        customer.LoyaltyPoints = request.LoyaltyPoints;
        customer.DefaultDiscountPercentage = request.DefaultDiscountPercentage;
        customer.MakingChargesWaived = request.MakingChargesWaived;
        customer.ModifiedAt = DateTime.UtcNow;

        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync();

        return await GetCustomerLoyaltyAsync(customerId);
    }

    /// <summary>
    /// Search customers by term
    /// </summary>
    public async Task<List<CustomerDto>> SearchCustomersAsync(string searchTerm, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<CustomerDto>();
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

        return await query
            .ProjectTo<CustomerDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    /// <summary>
    /// Check if customer exists by national ID
    /// </summary>
    public async Task<bool> CustomerExistsByNationalIdAsync(string nationalId, int? excludeId = null)
    {
        return await _unitOfWork.Customers.NationalIdExistsAsync(nationalId, excludeId);
    }

    /// <summary>
    /// Check if customer exists by mobile number
    /// </summary>
    public async Task<bool> CustomerExistsByMobileNumberAsync(string mobileNumber, int? excludeId = null)
    {
        return await _unitOfWork.Customers.MobileNumberExistsAsync(mobileNumber, excludeId);
    }

    /// <summary>
    /// Check if customer exists by email
    /// </summary>
    public async Task<bool> CustomerExistsByEmailAsync(string email, int? excludeId = null)
    {
        return await _unitOfWork.Customers.EmailExistsAsync(email, excludeId);
    }
}
