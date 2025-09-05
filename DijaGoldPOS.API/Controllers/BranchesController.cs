using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Models.BranchModels;
using DijaGoldPOS.API.Shared;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DijaGoldPOS.API.IServices;

namespace DijaGoldPOS.API.Controllers;

/// <summary>
/// Branches controller for branch management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BranchesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly IPricingService _pricingService;
    private readonly ILogger<BranchesController> _logger;
    private readonly IMapper _mapper;

    public BranchesController(
        ApplicationDbContext context,
        IAuditService auditService,
        IPricingService pricingService,
        ILogger<BranchesController> logger,
        IMapper mapper)
    {
        _context = context;
        _auditService = auditService;
        _pricingService = pricingService;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all branches with optional filtering and pagination
    /// </summary>
    /// <param name="searchRequest">Search parameters</param>
    /// <returns>List of branches</returns>
    [HttpGet]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BranchDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBranches([FromQuery] BranchSearchRequestDto searchRequest)
    {
        try
        {
            var query = _context.Branches.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchRequest.SearchTerm))
            {
                var searchTerm = searchRequest.SearchTerm.ToLower();
                query = query.Where(b => 
                    b.Name.ToLower().Contains(searchTerm) ||
                    b.Code.ToLower().Contains(searchTerm) ||
                    (b.Address != null && b.Address.ToLower().Contains(searchTerm)) ||
                    (b.ManagerName != null && b.ManagerName.ToLower().Contains(searchTerm))
                );
            }

            if (!string.IsNullOrWhiteSpace(searchRequest.Code))
            {
                query = query.Where(b => b.Code == searchRequest.Code);
            }

            if (searchRequest.IsHeadquarters.HasValue)
            {
                query = query.Where(b => b.IsHeadquarters == searchRequest.IsHeadquarters.Value);
            }

            if (searchRequest.IsActive.HasValue)
            {
                query = query.Where(b => b.IsActive == searchRequest.IsActive.Value);
            }

            // Apply sorting - headquarters first, then by name
            query = query.OrderByDescending(b => b.IsHeadquarters).ThenBy(b => b.Name);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination and mapping
            var branches = await query
                .Skip((searchRequest.PageNumber - 1) * searchRequest.PageSize)
                .Take(searchRequest.PageSize)
                .ProjectTo<BranchDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var result = new PagedResult<BranchDto>
            {
                Items = branches,
                TotalCount = totalCount,
                PageNumber = searchRequest.PageNumber,
                PageSize = searchRequest.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / searchRequest.PageSize)
            };

            return Ok(ApiResponse<PagedResult<BranchDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving branches");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving branches"));
        }
    }

    /// <summary>
    /// Get branch by ID
    /// </summary>
    /// <param name="id">Branch ID</param>
    /// <returns>Branch details</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<BranchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBranch(int id)
    {
        try
        {
            var branchDto = await _context.Branches
                .Where(b => b.Id == id)
                .ProjectTo<BranchDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (branchDto == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Branch not found"));
            }

            return Ok(ApiResponse<BranchDto>.SuccessResponse(branchDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving branch with ID {BranchId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving the branch"));
        }
    }

    /// <summary>
    /// Create a new branch
    /// </summary>
    /// <param name="request">Branch creation request</param>
    /// <returns>Created branch</returns>
    [HttpPost]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<BranchDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBranch([FromBody] CreateBranchRequestDto request)
    {
        try
        {
            // Check for duplicate branch code
            var existingBranch = await _context.Branches
                .FirstOrDefaultAsync(b => b.Code == request.Code);
            if (existingBranch != null)
            {
                return BadRequest(ApiResponse.ErrorResponse("A branch with this code already exists"));
            }

            // If setting as headquarters, ensure no other headquarters exists
            if (request.IsHeadquarters)
            {
                var hasHeadquarters = await _context.Branches.AnyAsync(b => b.IsHeadquarters && b.IsActive);
                if (hasHeadquarters)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Only one headquarters branch is allowed. Please update the existing headquarters first."));
                }
            }

            var branch = new Branch
            {
                Name = request.Name,
                Code = request.Code,
                Address = request.Address,
                Phone = request.Phone,
                ManagerName = request.ManagerName,
                IsHeadquarters = request.IsHeadquarters,
                IsActive = true
            };

            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "CREATE",
                "Branch",
                branch.Id.ToString(),
                $"Created branch: {branch.Name} ({branch.Code})"
            );

            var branchDto = new BranchDto
            {
                Id = branch.Id,
                Name = branch.Name,
                Code = branch.Code,
                Address = branch.Address,
                Phone = branch.Phone,
                ManagerName = branch.ManagerName,
                IsHeadquarters = branch.IsHeadquarters,
                CreatedAt = branch.CreatedAt,
                IsActive = branch.IsActive
            };

            return CreatedAtAction(nameof(GetBranch), new { id = branch.Id }, 
                ApiResponse<BranchDto>.SuccessResponse(branchDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating branch");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while creating the branch"));
        }
    }

    /// <summary>
    /// Update an existing branch
    /// </summary>
    /// <param name="id">Branch ID</param>
    /// <param name="request">Branch update request</param>
    /// <returns>Updated branch</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<BranchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBranch(int id, [FromBody] UpdateBranchRequestDto request)
    {
        try
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Branch not found"));
            }

            // Check for duplicate branch code (excluding current branch)
            if (request.Code != branch.Code)
            {
                var existingBranch = await _context.Branches
                    .FirstOrDefaultAsync(b => b.Code == request.Code && b.Id != id);
                if (existingBranch != null)
                {
                    return BadRequest(ApiResponse.ErrorResponse("A branch with this code already exists"));
                }
            }

            // If setting as headquarters, ensure no other headquarters exists
            if (request.IsHeadquarters && !branch.IsHeadquarters)
            {
                var hasHeadquarters = await _context.Branches.AnyAsync(b => b.IsHeadquarters && b.IsActive && b.Id != id);
                if (hasHeadquarters)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Only one headquarters branch is allowed. Please update the existing headquarters first."));
                }
            }

            // Update branch properties
            branch.Name = request.Name;
            branch.Code = request.Code;
            branch.Address = request.Address;
            branch.Phone = request.Phone;
            branch.ManagerName = request.ManagerName;
            branch.IsHeadquarters = request.IsHeadquarters;
            branch.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "UPDATE",
                "Branch",
                branch.Id.ToString(),
                $"Updated branch: {branch.Name} ({branch.Code})"
            );

            var branchDto = new BranchDto
            {
                Id = branch.Id,
                Name = branch.Name,
                Code = branch.Code,
                Address = branch.Address,
                Phone = branch.Phone,
                ManagerName = branch.ManagerName,
                IsHeadquarters = branch.IsHeadquarters,
                CreatedAt = branch.CreatedAt,
                IsActive = branch.IsActive
            };

            return Ok(ApiResponse<BranchDto>.SuccessResponse(branchDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating branch with ID {BranchId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating the branch"));
        }
    }

    /// <summary>
    /// Soft delete a branch
    /// </summary>
    /// <param name="id">Branch ID</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBranch(int id)
    {
        try
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Branch not found"));
            }

            // Check if branch is headquarters
            if (branch.IsHeadquarters)
            {
                return BadRequest(ApiResponse.ErrorResponse("Cannot delete headquarters branch"));
            }

            // Check if branch has inventory
            var hasInventory = await _context.Inventories.AnyAsync(i => i.BranchId == id);
            if (hasInventory)
            {
                return BadRequest(ApiResponse.ErrorResponse("Cannot delete branch with inventory items"));
            }

            // Check if branch has users assigned
            var hasUsers = await _context.Users.AnyAsync(u => u.BranchId == id);
            if (hasUsers)
            {
                return BadRequest(ApiResponse.ErrorResponse("Cannot delete branch with assigned users"));
            }

            branch.IsActive = false;
            branch.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            await _auditService.LogAsync(
                userId,
                "DELETE",
                "Branch",
                branch.Id.ToString(),
                $"Soft deleted branch: {branch.Name} ({branch.Code})"
            );

            return Ok(ApiResponse.SuccessResponse("Branch deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting branch with ID {BranchId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting the branch"));
        }
    }

    /// <summary>
    /// Get branch inventory summary
    /// </summary>
    /// <param name="id">Branch ID</param>
    /// <returns>Branch inventory summary</returns>
    [HttpGet("{id}/inventory")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<BranchInventorySummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBranchInventory(int id)
    {
        try
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Branch not found"));
            }

            var inventoryQuery = _context.Inventories
                .Include(i => i.Product)
                .Where(i => i.BranchId == id);

            var inventoryItems = await inventoryQuery.ToListAsync();

            var totalProducts = inventoryItems.Count;
            var totalWeight = inventoryItems.Sum(i => i.WeightOnHand);
            var lowStockItems = inventoryItems.Count(i => i.MinimumStockLevel > 0 && i.QuantityOnHand <= i.MinimumStockLevel);
            var outOfStockItems = inventoryItems.Count(i => i.QuantityOnHand <= 0);

            // Calculate total value (this is a simplified calculation)
            decimal totalValue = 0;
            foreach (var item in inventoryItems)
            {
                try
                {
                    var pricing = await _pricingService.CalculatePriceAsync(
                        item.Product, 
                        item.WeightOnHand);
                    totalValue += pricing.FinalTotal;
                }
                catch
                {
                    // If pricing fails, skip this item
                    continue;
                }
            }

            // Get top 10 items by quantity
            var topItems = inventoryItems
                .OrderByDescending(i => i.QuantityOnHand)
                .Take(10)
                .Select(i => new BranchInventoryItemDto
                {
                    ProductId = i.ProductId,
                    ProductCode = i.Product.ProductCode,
                    ProductName = i.Product.Name,
                    QuantityOnHand = i.QuantityOnHand,
                    WeightOnHand = i.WeightOnHand,
                    EstimatedValue = 0, // Would calculate this based on current gold rates
                    IsLowStock = i.MinimumStockLevel > 0 && i.QuantityOnHand <= i.MinimumStockLevel,
                    IsOutOfStock = i.QuantityOnHand <= 0
                })
                .ToList();

            var summary = new BranchInventorySummaryDto
            {
                BranchId = branch.Id,
                BranchName = branch.Name,
                BranchCode = branch.Code,
                TotalProducts = totalProducts,
                TotalWeight = totalWeight,
                TotalValue = totalValue,
                LowStockItems = lowStockItems,
                OutOfStockItems = outOfStockItems,
                TopItems = topItems
            };

            return Ok(ApiResponse<BranchInventorySummaryDto>.SuccessResponse(summary));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventory summary for branch {BranchId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving branch inventory"));
        }
    }

    /// <summary>
    /// Get branch staff
    /// </summary>
    /// <param name="id">Branch ID</param>
    /// <returns>Branch staff information</returns>
    [HttpGet("{id}/staff")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<BranchStaffDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBranchStaff(int id)
    {
        try
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Branch not found"));
            }

            var staff = await _context.Users
                .Where(u => u.BranchId == id)
                .Select(u => new BranchStaffMemberDto
                {
                    UserId = u.Id,
                    UserName = u.UserName ?? "",
                    FullName = u.FullName,
                    Email = u.Email ?? "",
                    Role = "User", // TODO: Implement proper role retrieval
                    AssignedDate = u.CreatedAt,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            var staffDto = new BranchStaffDto
            {
                BranchId = branch.Id,
                BranchName = branch.Name,
                Staff = staff,
                TotalStaffCount = staff.Count
            };

            return Ok(ApiResponse<BranchStaffDto>.SuccessResponse(staffDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff for branch {BranchId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving branch staff"));
        }
    }

    /// <summary>
    /// Get branch performance metrics
    /// </summary>
    /// <param name="id">Branch ID</param>
    /// <param name="date">Report date (optional, defaults to today)</param>
    /// <returns>Branch performance metrics</returns>
    [HttpGet("{id}/performance")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(ApiResponse<BranchPerformanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBranchPerformance(int id, [FromQuery] DateTime? date = null)
    {
        try
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Branch not found"));
            }

            var reportDate = date ?? DateTime.Today;
            var startOfDay = reportDate.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            var startOfMonth = new DateTime(reportDate.Year, reportDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

            // Daily metrics - using FinancialTransactions for sales data
            var dailyFinancialTransactions = await _context.FinancialTransactions
                .Where(ft => ft.BranchId == id && ft.TransactionDate >= startOfDay && ft.TransactionDate <= endOfDay 
                    && ft.TransactionTypeId == LookupTableConstants.FinancialTransactionTypeSale
                    && ft.StatusId == LookupTableConstants.FinancialTransactionStatusCompleted)
                .ToListAsync();

            var dailySales = dailyFinancialTransactions.Sum(ft => ft.TotalAmount);
            var dailyTransactionCount = dailyFinancialTransactions.Count;

            // Monthly metrics - using FinancialTransactions for sales data
            var monthlyFinancialTransactions = await _context.FinancialTransactions
                .Where(ft => ft.BranchId == id && ft.TransactionDate >= startOfMonth && ft.TransactionDate <= endOfMonth
                    && ft.TransactionTypeId == LookupTableConstants.FinancialTransactionTypeSale
                    && ft.StatusId == LookupTableConstants.FinancialTransactionStatusCompleted)
                .ToListAsync();

            var monthlySales = monthlyFinancialTransactions.Sum(ft => ft.TotalAmount);
            var monthlyTransactionCount = monthlyFinancialTransactions.Count;

            // Average transaction value
            var averageTransactionValue = monthlyTransactionCount > 0 ? monthlySales / monthlyTransactionCount : 0;

            // Active customers (customers with transactions this month)
            // Note: Customer information is now in the related Order, accessed via BusinessEntityId
            var activeCustomers = await _context.Orders
                .Where(o => o.BranchId == id && o.OrderDate >= startOfMonth && o.OrderDate <= endOfMonth 
                    && o.CustomerId.HasValue
                    && o.OrderTypeId == LookupTableConstants.OrderTypeSale
                    && o.StatusId == LookupTableConstants.OrderStatusCompleted)
                .Select(o => o.CustomerId)
                .Distinct()
                .CountAsync();

            // Recent transactions (last 10) - using FinancialTransactions
            var recentTransactions = await _context.FinancialTransactions
                .Include(ft => ft.ProcessedByUser)
                .Where(ft => ft.BranchId == id && ft.TransactionTypeId == LookupTableConstants.FinancialTransactionTypeSale)
                .OrderByDescending(ft => ft.TransactionDate)
                .Take(10)
                .Select(ft => new BranchTransactionDto
                {
                    TransactionId = ft.Id,
                    TransactionNumber = ft.TransactionNumber,
                    TransactionDate = ft.TransactionDate,
                    TransactionType = "Sale", // FinancialTransactionTypeSale
                    TotalAmount = ft.TotalAmount,
                    CustomerName = "Walk-in", // Customer info is in related Order
                    CashierName = ft.ProcessedByUser != null ? ft.ProcessedByUser.FullName : string.Empty
                })
                .ToListAsync();

            var performance = new BranchPerformanceDto
            {
                BranchId = branch.Id,
                BranchName = branch.Name,
                BranchCode = branch.Code,
                ReportDate = reportDate,
                DailySales = dailySales,
                DailyTransactions = dailyTransactionCount,
                MonthlySales = monthlySales,
                MonthlyTransactions = monthlyTransactionCount,
                AverageTransactionValue = averageTransactionValue,
                ActiveCustomers = activeCustomers,
                InventoryTurnover = 0, // This would require more complex calculation
                RecentTransactions = recentTransactions
            };

            return Ok(ApiResponse<BranchPerformanceDto>.SuccessResponse(performance));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving performance metrics for branch {BranchId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving branch performance"));
        }
    }

    /// <summary>
    /// Get branch transactions
    /// </summary>
    /// <param name="id">Branch ID</param>
    /// <param name="fromDate">From date (optional)</param>
    /// <param name="toDate">To date (optional)</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Branch transactions</returns>
    [HttpGet("{id}/transactions")]
    [Authorize(Policy = "CashierOrManager")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BranchTransactionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBranchTransactions(
        int id,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Branch not found"));
            }

            var query = _context.FinancialTransactions
                .Include(ft => ft.ProcessedByUser)
                .Where(ft => ft.BranchId == id && ft.TransactionTypeId == LookupTableConstants.FinancialTransactionTypeSale);

            if (fromDate.HasValue)
            {
                query = query.Where(ft => ft.TransactionDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(ft => ft.TransactionDate <= toDate.Value);
            }

            query = query.OrderByDescending(ft => ft.TransactionDate);

            var totalCount = await query.CountAsync();

            var transactions = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(ft => new BranchTransactionDto
                {
                    TransactionId = ft.Id,
                    TransactionNumber = ft.TransactionNumber,
                    TransactionDate = ft.TransactionDate,
                    TransactionType = "Sale", // FinancialTransactionTypeSale
                    TotalAmount = ft.TotalAmount,
                    CustomerName = "Walk-in", // Customer info is in related Order
                    CashierName = ft.ProcessedByUser != null ? ft.ProcessedByUser.FullName : string.Empty
                })
                .ToListAsync();

            var result = new PagedResult<BranchTransactionDto>
            {
                Items = transactions,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(ApiResponse<PagedResult<BranchTransactionDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for branch {BranchId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving branch transactions"));
        }
    }
}
