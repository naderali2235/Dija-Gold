using AutoMapper;
using AutoMapper.QueryableExtensions;
using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service implementation for customer purchase operations
/// </summary>
public class CustomerPurchaseService : ICustomerPurchaseService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerPurchaseService> _logger;
    private readonly IAuditService _auditService;

    public CustomerPurchaseService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<CustomerPurchaseService> logger,
        IAuditService auditService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task<ApiResponse<CustomerPurchaseDto>> CreatePurchaseAsync(CreateCustomerPurchaseRequest request, string userId)
    {
        try
        {
            // Validate request
            if (request == null)
                return ApiResponse<CustomerPurchaseDto>.ErrorResponse("Request cannot be null");

            if (!request.Items.Any())
                return ApiResponse<CustomerPurchaseDto>.ErrorResponse("Purchase must have at least one item");

            // Get next purchase number
            var nextPurchaseNumber = await GenerateNextPurchaseNumberAsync();

            // Calculate total amount from items
            var calculatedTotal = request.Items.Sum(i => i.TotalAmount);
            if (Math.Abs(calculatedTotal - request.TotalAmount) > 0.01m)
            {
                _logger.LogWarning("Total amount mismatch. Request: {RequestTotal}, Calculated: {CalculatedTotal}",
                    request.TotalAmount, calculatedTotal);
            }

            // Create purchase entity
            var purchase = new CustomerPurchase
            {
                PurchaseNumber = nextPurchaseNumber,
                CustomerId = request.CustomerId,
                BranchId = request.BranchId,
                PurchaseDate = DateTime.UtcNow,
                TotalAmount = calculatedTotal,
                AmountPaid = request.AmountPaid,
                PaymentMethodId = request.PaymentMethodId,
                Notes = request.Notes,
                CreatedByUserId = userId,
                CustomerPurchaseItems = new List<CustomerPurchaseItem>()
            };

            // Create purchase items
            foreach (var itemRequest in request.Items)
            {
                var purchaseItem = new CustomerPurchaseItem
                {
                    CustomerPurchaseId = 0, // Will be set by EF
                    KaratTypeId = itemRequest.KaratTypeId,
                    Weight = itemRequest.Weight,
                    UnitPrice = itemRequest.UnitPrice,
                    TotalAmount = itemRequest.TotalAmount,
                    Notes = itemRequest.Notes
                };

                purchase.CustomerPurchaseItems.Add(purchaseItem);
            }

            // Save to database
            await _context.CustomerPurchases.AddAsync(purchase);
            await _context.SaveChangesAsync();

            // Load the complete purchase with navigation properties
            var completePurchase = await _context.CustomerPurchases
                .Include(cp => cp.Customer)
                .Include(cp => cp.Branch)
                .Include(cp => cp.PaymentMethod)
                .Include(cp => cp.CustomerPurchaseItems)
                    .ThenInclude(cpi => cpi.KaratType)
                .FirstOrDefaultAsync(cp => cp.Id == purchase.Id);

            if (completePurchase == null)
                return ApiResponse<CustomerPurchaseDto>.ErrorResponse("Failed to retrieve created purchase");

            var purchaseDto = _mapper.Map<CustomerPurchaseDto>(completePurchase);

            // Map items
            purchaseDto.Items = completePurchase.CustomerPurchaseItems
                .Select(cpi => new CustomerPurchaseItemDto
                {
                    Id = cpi.Id,
                    CustomerPurchaseId = cpi.CustomerPurchaseId,
                    KaratTypeId = cpi.KaratTypeId,
                    KaratTypeName = cpi.KaratType?.Name ?? "Unknown",
                    Weight = cpi.Weight,
                    UnitPrice = cpi.UnitPrice,
                    TotalAmount = cpi.TotalAmount,
                    Notes = cpi.Notes
                }).ToList();

            // Audit log
            await _auditService.LogAsync("CustomerPurchase", "Create",
                $"Created customer purchase {nextPurchaseNumber} for customer {completePurchase.Customer?.FullName}",
                userId);

            _logger.LogInformation("Customer purchase {PurchaseNumber} created successfully by user {UserId}",
                nextPurchaseNumber, userId);

            return ApiResponse<CustomerPurchaseDto>.SuccessResponse(purchaseDto, "Purchase created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer purchase by user {UserId}", userId);
            return ApiResponse<CustomerPurchaseDto>.ErrorResponse("An error occurred while creating the purchase");
        }
    }

    public async Task<ApiResponse<CustomerPurchaseDto>> GetPurchaseByIdAsync(int id)
    {
        try
        {
            var purchase = await _context.CustomerPurchases
                .Include(cp => cp.Customer)
                .Include(cp => cp.Branch)
                .Include(cp => cp.PaymentMethod)
                .Include(cp => cp.CustomerPurchaseItems)
                    .ThenInclude(cpi => cpi.KaratType)
                .FirstOrDefaultAsync(cp => cp.Id == id);

            if (purchase == null)
                return ApiResponse<CustomerPurchaseDto>.ErrorResponse("Purchase not found");

            var purchaseDto = _mapper.Map<CustomerPurchaseDto>(purchase);
            purchaseDto.Items = purchase.CustomerPurchaseItems
                .Select(cpi => new CustomerPurchaseItemDto
                {
                    Id = cpi.Id,
                    CustomerPurchaseId = cpi.CustomerPurchaseId,
                    KaratTypeId = cpi.KaratTypeId,
                    KaratTypeName = cpi.KaratType?.Name ?? "Unknown",
                    Weight = cpi.Weight,
                    UnitPrice = cpi.UnitPrice,
                    TotalAmount = cpi.TotalAmount,
                    Notes = cpi.Notes
                }).ToList();

            return ApiResponse<CustomerPurchaseDto>.SuccessResponse(purchaseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer purchase {PurchaseId}", id);
            return ApiResponse<CustomerPurchaseDto>.ErrorResponse("An error occurred while retrieving the purchase");
        }
    }

    public async Task<ApiResponse<CustomerPurchaseDto>> GetPurchaseByNumberAsync(string purchaseNumber)
    {
        try
        {
            var purchase = await _context.CustomerPurchases
                .Include(cp => cp.Customer)
                .Include(cp => cp.Branch)
                .Include(cp => cp.PaymentMethod)
                .Include(cp => cp.CustomerPurchaseItems)
                    .ThenInclude(cpi => cpi.KaratType)
                .FirstOrDefaultAsync(cp => cp.PurchaseNumber == purchaseNumber);

            if (purchase == null)
                return ApiResponse<CustomerPurchaseDto>.ErrorResponse("Purchase not found");

            var purchaseDto = _mapper.Map<CustomerPurchaseDto>(purchase);
            purchaseDto.Items = purchase.CustomerPurchaseItems
                .Select(cpi => new CustomerPurchaseItemDto
                {
                    Id = cpi.Id,
                    CustomerPurchaseId = cpi.CustomerPurchaseId,
                    KaratTypeId = cpi.KaratTypeId,
                    KaratTypeName = cpi.KaratType?.Name ?? "Unknown",
                    Weight = cpi.Weight,
                    UnitPrice = cpi.UnitPrice,
                    TotalAmount = cpi.TotalAmount,
                    Notes = cpi.Notes
                }).ToList();

            return ApiResponse<CustomerPurchaseDto>.SuccessResponse(purchaseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer purchase {PurchaseNumber}", purchaseNumber);
            return ApiResponse<CustomerPurchaseDto>.ErrorResponse("An error occurred while retrieving the purchase");
        }
    }

    public async Task<ApiResponse<PagedResult<CustomerPurchaseDto>>> GetPurchasesAsync(CustomerPurchaseSearchRequest searchRequest)
    {
        try
        {
            var query = _context.CustomerPurchases
                .Include(cp => cp.Customer)
                .Include(cp => cp.Branch)
                .Include(cp => cp.PaymentMethod)
                .Include(cp => cp.CustomerPurchaseItems)
                    .ThenInclude(cpi => cpi.KaratType)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchRequest.PurchaseNumber))
            {
                query = query.Where(cp => cp.PurchaseNumber.Contains(searchRequest.PurchaseNumber));
            }

            if (searchRequest.CustomerId.HasValue)
            {
                query = query.Where(cp => cp.CustomerId == searchRequest.CustomerId.Value);
            }

            if (searchRequest.BranchId.HasValue)
            {
                query = query.Where(cp => cp.BranchId == searchRequest.BranchId.Value);
            }

            if (searchRequest.FromDate.HasValue)
            {
                query = query.Where(cp => cp.PurchaseDate >= searchRequest.FromDate.Value);
            }

            if (searchRequest.ToDate.HasValue)
            {
                query = query.Where(cp => cp.PurchaseDate <= searchRequest.ToDate.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply sorting and pagination
            var purchases = await query
                .OrderByDescending(cp => cp.PurchaseDate)
                .Skip((searchRequest.PageNumber - 1) * searchRequest.PageSize)
                .Take(searchRequest.PageSize)
                .ToListAsync();

            var purchaseDtos = purchases.Select(cp =>
            {
                var dto = _mapper.Map<CustomerPurchaseDto>(cp);
                dto.Items = cp.CustomerPurchaseItems
                    .Select(cpi => new CustomerPurchaseItemDto
                    {
                        Id = cpi.Id,
                        CustomerPurchaseId = cpi.CustomerPurchaseId,
                        KaratTypeId = cpi.KaratTypeId,
                        KaratTypeName = cpi.KaratType?.Name ?? "Unknown",
                        Weight = cpi.Weight,
                        UnitPrice = cpi.UnitPrice,
                        TotalAmount = cpi.TotalAmount,
                        Notes = cpi.Notes
                    }).ToList();
                return dto;
            }).ToList();

            var result = new PagedResult<CustomerPurchaseDto>
            {
                Items = purchaseDtos,
                TotalCount = totalCount,
                PageNumber = searchRequest.PageNumber,
                PageSize = searchRequest.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)searchRequest.PageSize)
            };

            return ApiResponse<PagedResult<CustomerPurchaseDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer purchases");
            return ApiResponse<PagedResult<CustomerPurchaseDto>>.ErrorResponse("An error occurred while retrieving purchases");
        }
    }

    public async Task<ApiResponse<List<CustomerPurchaseDto>>> GetPurchasesByCustomerAsync(int customerId)
    {
        try
        {
            var purchases = await _context.CustomerPurchases
                .Where(cp => cp.CustomerId == customerId)
                .Include(cp => cp.Branch)
                .Include(cp => cp.PaymentMethod)
                .Include(cp => cp.CustomerPurchaseItems)
                    .ThenInclude(cpi => cpi.KaratType)
                .OrderByDescending(cp => cp.PurchaseDate)
                .ToListAsync();

            var purchaseDtos = purchases.Select(cp =>
            {
                var dto = _mapper.Map<CustomerPurchaseDto>(cp);
                dto.Items = cp.CustomerPurchaseItems
                    .Select(cpi => new CustomerPurchaseItemDto
                    {
                        Id = cpi.Id,
                        CustomerPurchaseId = cpi.CustomerPurchaseId,
                        KaratTypeId = cpi.KaratTypeId,
                        KaratTypeName = cpi.KaratType?.Name ?? "Unknown",
                        Weight = cpi.Weight,
                        UnitPrice = cpi.UnitPrice,
                        TotalAmount = cpi.TotalAmount,
                        Notes = cpi.Notes
                    }).ToList();
                return dto;
            }).ToList();

            return ApiResponse<List<CustomerPurchaseDto>>.SuccessResponse(purchaseDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer purchases for customer {CustomerId}", customerId);
            return ApiResponse<List<CustomerPurchaseDto>>.ErrorResponse("An error occurred while retrieving purchases");
        }
    }

    public async Task<ApiResponse<List<CustomerPurchaseDto>>> GetPurchasesByBranchAsync(int branchId)
    {
        try
        {
            var purchases = await _context.CustomerPurchases
                .Where(cp => cp.BranchId == branchId)
                .Include(cp => cp.Customer)
                .Include(cp => cp.PaymentMethod)
                .Include(cp => cp.CustomerPurchaseItems)
                    .ThenInclude(cpi => cpi.KaratType)
                .OrderByDescending(cp => cp.PurchaseDate)
                .ToListAsync();

            var purchaseDtos = purchases.Select(cp =>
            {
                var dto = _mapper.Map<CustomerPurchaseDto>(cp);
                dto.Items = cp.CustomerPurchaseItems
                    .Select(cpi => new CustomerPurchaseItemDto
                    {
                        Id = cpi.Id,
                        CustomerPurchaseId = cpi.CustomerPurchaseId,
                        KaratTypeId = cpi.KaratTypeId,
                        KaratTypeName = cpi.KaratType?.Name ?? "Unknown",
                        Weight = cpi.Weight,
                        UnitPrice = cpi.UnitPrice,
                        TotalAmount = cpi.TotalAmount,
                        Notes = cpi.Notes
                    }).ToList();
                return dto;
            }).ToList();

            return ApiResponse<List<CustomerPurchaseDto>>.SuccessResponse(purchaseDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer purchases for branch {BranchId}", branchId);
            return ApiResponse<List<CustomerPurchaseDto>>.ErrorResponse("An error occurred while retrieving purchases");
        }
    }

    public async Task<ApiResponse<List<CustomerPurchaseDto>>> GetPurchasesByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            var purchases = await _context.CustomerPurchases
                .Where(cp => cp.PurchaseDate >= fromDate && cp.PurchaseDate <= toDate)
                .Include(cp => cp.Customer)
                .Include(cp => cp.Branch)
                .Include(cp => cp.PaymentMethod)
                .Include(cp => cp.CustomerPurchaseItems)
                    .ThenInclude(cpi => cpi.KaratType)
                .OrderByDescending(cp => cp.PurchaseDate)
                .ToListAsync();

            var purchaseDtos = purchases.Select(cp =>
            {
                var dto = _mapper.Map<CustomerPurchaseDto>(cp);
                dto.Items = cp.CustomerPurchaseItems
                    .Select(cpi => new CustomerPurchaseItemDto
                    {
                        Id = cpi.Id,
                        CustomerPurchaseId = cpi.CustomerPurchaseId,
                        KaratTypeId = cpi.KaratTypeId,
                        KaratTypeName = cpi.KaratType?.Name ?? "Unknown",
                        Weight = cpi.Weight,
                        UnitPrice = cpi.UnitPrice,
                        TotalAmount = cpi.TotalAmount,
                        Notes = cpi.Notes
                    }).ToList();
                return dto;
            }).ToList();

            return ApiResponse<List<CustomerPurchaseDto>>.SuccessResponse(purchaseDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer purchases for date range {FromDate} to {ToDate}", fromDate, toDate);
            return ApiResponse<List<CustomerPurchaseDto>>.ErrorResponse("An error occurred while retrieving purchases");
        }
    }

    public async Task<ApiResponse<CustomerPurchaseDto>> UpdatePaymentStatusAsync(int purchaseId, decimal amountPaid, string userId)
    {
        try
        {
            var purchase = await _context.CustomerPurchases.FindAsync(purchaseId);

            if (purchase == null)
                return ApiResponse<CustomerPurchaseDto>.ErrorResponse("Purchase not found");

            var oldAmountPaid = purchase.AmountPaid;
            purchase.AmountPaid = amountPaid;
            purchase.ModifiedAt = DateTime.UtcNow;
            purchase.ModifiedBy = userId;

            await _context.SaveChangesAsync();

            // Get updated purchase with navigation properties
            var updatedPurchase = await GetPurchaseByIdAsync(purchaseId);

            // Audit log
            await _auditService.LogAsync("CustomerPurchase", "Update",
                $"Updated payment for purchase {purchase.PurchaseNumber}: {oldAmountPaid} -> {amountPaid}",
                userId);

            _logger.LogInformation("Payment status updated for purchase {PurchaseNumber} by user {UserId}",
                purchase.PurchaseNumber, userId);

            return updatedPurchase;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment status for purchase {PurchaseId}", purchaseId);
            return ApiResponse<CustomerPurchaseDto>.ErrorResponse("An error occurred while updating payment status");
        }
    }

    public async Task<ApiResponse<bool>> CancelPurchaseAsync(int purchaseId, string userId)
    {
        try
        {
            var purchase = await _context.CustomerPurchases.FindAsync(purchaseId);

            if (purchase == null)
                return ApiResponse<bool>.ErrorResponse("Purchase not found");

            if (purchase.IsActive == false)
                return ApiResponse<bool>.ErrorResponse("Purchase is already cancelled");

            purchase.IsActive = false;
            purchase.ModifiedAt = DateTime.UtcNow;
            purchase.ModifiedBy = userId;

            await _context.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync("CustomerPurchase", "Cancel",
                $"Cancelled purchase {purchase.PurchaseNumber}",
                userId);

            _logger.LogInformation("Purchase {PurchaseNumber} cancelled by user {UserId}",
                purchase.PurchaseNumber, userId);

            return ApiResponse<bool>.SuccessResponse(true, "Purchase cancelled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling purchase {PurchaseId}", purchaseId);
            return ApiResponse<bool>.ErrorResponse("An error occurred while cancelling the purchase");
        }
    }

    public async Task<ApiResponse<CustomerPurchaseSummaryDto>> GetPurchaseSummaryAsync(DateTime fromDate, DateTime toDate, int? branchId = null)
    {
        try
        {
            var query = _context.CustomerPurchases
                .Where(cp => cp.PurchaseDate >= fromDate && cp.PurchaseDate <= toDate)
                .AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(cp => cp.BranchId == branchId.Value);
            }

            var purchases = await query
                .Include(cp => cp.CustomerPurchaseItems)
                .ToListAsync();

            var summary = new CustomerPurchaseSummaryDto
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalPurchases = purchases.Count,
                TotalCustomers = purchases.Select(cp => cp.CustomerId).Distinct().Count(),
                TotalWeight = purchases.SelectMany(cp => cp.CustomerPurchaseItems).Sum(cpi => cpi.Weight),
                TotalAmount = purchases.Sum(cp => cp.TotalAmount),
                TotalAmountPaid = purchases.Sum(cp => cp.AmountPaid),
                TotalOutstanding = purchases.Sum(cp => cp.TotalAmount - cp.AmountPaid)
            };

            return ApiResponse<CustomerPurchaseSummaryDto>.SuccessResponse(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving purchase summary for date range {FromDate} to {ToDate}", fromDate, toDate);
            return ApiResponse<CustomerPurchaseSummaryDto>.ErrorResponse("An error occurred while retrieving purchase summary");
        }
    }

    private async Task<string> GenerateNextPurchaseNumberAsync()
    {
        var lastPurchase = await _context.CustomerPurchases
            .OrderByDescending(cp => cp.PurchaseNumber)
            .FirstOrDefaultAsync();

        if (lastPurchase == null)
        {
            return "CP-00001";
        }

        var lastNumber = int.Parse(lastPurchase.PurchaseNumber.Split('-')[1]);
        return $"CP-{(lastNumber + 1):D5}";
    }
}
