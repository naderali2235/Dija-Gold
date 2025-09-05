using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Shared;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using DijaGoldPOS.API.Models.OwneShipModels;
using DijaGoldPOS.API.Models.InventoryModels;
using DijaGoldPOS.API.Models.SupplierModels;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service for managing raw gold balances and supplier relationships
/// </summary>
public class RawGoldBalanceService : IRawGoldBalanceService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<RawGoldBalanceService> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public RawGoldBalanceService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<RawGoldBalanceService> logger,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<ApiResponse<List<SupplierGoldBalanceDto>>> GetSupplierBalancesAsync(int branchId, int? supplierId = null)
    {
        try
        {
            var query = _context.SupplierGoldBalances
                .Where(sgb => sgb.BranchId == branchId)
                .Include(sgb => sgb.Supplier)
                .Include(sgb => sgb.Branch)
                .Include(sgb => sgb.KaratType)
                .AsQueryable();

            if (supplierId.HasValue)
                query = query.Where(sgb => sgb.SupplierId == supplierId.Value);

            var balances = await query.ToListAsync();
            var balanceDtos = balances.Select(balance => new SupplierGoldBalanceDto
            {
                SupplierId = balance.SupplierId,
                SupplierName = balance.Supplier.CompanyName,
                BranchId = balance.BranchId,
                BranchName = balance.Branch.Name,
                KaratTypeId = balance.KaratTypeId,
                KaratTypeName = balance.KaratType.Name,
                KaratPurity = 0, // TODO: Add purity calculation logic
                TotalWeightReceived = balance.TotalWeightReceived,
                TotalWeightPaidFor = balance.TotalWeightPaidFor,
                OutstandingWeightDebt = balance.OutstandingWeightDebt,
                MerchantGoldBalance = balance.MerchantGoldBalance,
                OutstandingMonetaryValue = balance.OutstandingMonetaryValue,
                AverageCostPerGram = balance.AverageCostPerGram,
                LastTransactionDate = balance.LastTransactionDate
            }).ToList();

            return ApiResponse<List<SupplierGoldBalanceDto>>.SuccessResponse(balanceDtos, "Supplier balances retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supplier balances for branch {BranchId}", branchId);
            return ApiResponse<List<SupplierGoldBalanceDto>>.ErrorResponse("Failed to retrieve supplier balances");
        }
    }

    public async Task<ApiResponse<RawGoldTransferDto>> WaiveGoldToSupplierAsync(WaiveGoldToSupplierRequest request, string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // 1. Validate merchant has enough gold of the requested karat type
            var merchantGold = await _context.RawGoldInventories
                .FirstOrDefaultAsync(rgi => rgi.BranchId == request.BranchId && 
                                          rgi.KaratTypeId == request.FromKaratTypeId);

            if (merchantGold == null || merchantGold.AvailableWeight < request.FromWeight)
            {
                var karatTypeName = await GetKaratTypeNameAsync(request.FromKaratTypeId);
                return ApiResponse<RawGoldTransferDto>.ErrorResponse($"Insufficient {karatTypeName} gold available. Available: {merchantGold?.AvailableWeight ?? 0}g, Requested: {request.FromWeight}g");
            }

            // 2. Get current gold rates for conversion
            var fromRate = await GetCurrentGoldRateAsync(request.FromKaratTypeId);
            var toRate = await GetCurrentGoldRateAsync(request.ToKaratTypeId);

            if (fromRate == 0 || toRate == 0)
            {
                return ApiResponse<RawGoldTransferDto>.ErrorResponse("Unable to get current gold rates for conversion");
            }

            // 3. Calculate conversion
            var fromValue = request.FromWeight * fromRate;
            var toWeight = fromValue / toRate; // Convert based on value
            var conversionFactor = toWeight / request.FromWeight;

            // 4. Generate transfer number
            var transferNumber = await GenerateTransferNumberAsync();

            // 5. Create transfer record
            var transfer = new RawGoldTransfer
            {
                TransferNumber = transferNumber,
                BranchId = request.BranchId,
                FromSupplierId = null, // From merchant's own gold
                ToSupplierId = request.ToSupplierId,
                FromKaratTypeId = request.FromKaratTypeId,
                ToKaratTypeId = request.ToKaratTypeId,
                FromWeight = request.FromWeight,
                ToWeight = toWeight,
                FromGoldRate = fromRate,
                ToGoldRate = toRate,
                ConversionFactor = conversionFactor,
                TransferValue = fromValue,
                TransferDate = DateTime.UtcNow,
                TransferType = "Waive",
                CustomerPurchaseId = request.CustomerPurchaseId,
                Notes = request.Notes,
                CreatedByUserId = userId
            };

            _context.RawGoldTransfers.Add(transfer);

            // 6. Update merchant's raw gold inventory (reduce)
            merchantGold.WeightOnHand -= request.FromWeight;
            merchantGold.LastMovementDate = DateTime.UtcNow;
            merchantGold.ModifiedAt = DateTime.UtcNow;
            merchantGold.ModifiedBy = userId;

            // 7. Update or create supplier gold balance
            await UpdateSupplierGoldBalanceAsync(request.ToSupplierId, request.BranchId, request.ToKaratTypeId, toWeight, toRate, userId);

            // 8. Update supplier's financial balance (reduce debt)
            var supplier = await _context.Suppliers.FindAsync(request.ToSupplierId);
            if (supplier != null)
            {
                supplier.CurrentBalance -= fromValue; // Reduce what we owe them
                supplier.LastTransactionDate = DateTime.UtcNow;
                supplier.ModifiedAt = DateTime.UtcNow;
                supplier.ModifiedBy = userId;
            }

            // 9. Create inventory movement records
            await CreateInventoryMovementAsync(request.BranchId, request.FromKaratTypeId, -request.FromWeight, transferNumber, "Gold Transfer Out", userId);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Load transfer with navigation properties for DTO mapping
            var transferWithNav = await _context.RawGoldTransfers
                .Include(t => t.Branch)
                .Include(t => t.FromSupplier)
                .Include(t => t.ToSupplier)
                .Include(t => t.FromKaratType)
                .Include(t => t.ToKaratType)
                .Include(t => t.CustomerPurchase)
                .FirstOrDefaultAsync(t => t.Id == transfer.Id);

            var transferDto = MapToTransferDto(transferWithNav!);

            await _auditService.LogAsync("RawGoldTransfer", "Create",
                $"Gold waived: {request.FromWeight}g {await GetKaratTypeNameAsync(request.FromKaratTypeId)} to Supplier {request.ToSupplierId}: {toWeight}g {await GetKaratTypeNameAsync(request.ToKaratTypeId)}",
                userId);
            
            _logger.LogInformation("Gold waived successfully. Transfer: {TransferNumber}, From: {FromWeight}g {FromKarat} to Supplier {SupplierId}: {ToWeight}g {ToKarat}",
                transferNumber, request.FromWeight, await GetKaratTypeNameAsync(request.FromKaratTypeId), request.ToSupplierId, toWeight, await GetKaratTypeNameAsync(request.ToKaratTypeId));

            return ApiResponse<RawGoldTransferDto>.SuccessResponse(transferDto, "Gold waived to supplier successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error waiving gold to supplier");
            return ApiResponse<RawGoldTransferDto>.ErrorResponse("Failed to waive gold to supplier");
        }
    }

    public async Task<ApiResponse<RawGoldTransferDto>> ConvertGoldKaratAsync(ConvertGoldKaratRequest request, string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Similar logic to waiving but for karat conversion within same supplier or merchant gold
            var fromRate = await GetCurrentGoldRateAsync(request.FromKaratTypeId);
            var toRate = await GetCurrentGoldRateAsync(request.ToKaratTypeId);

            if (fromRate == 0 || toRate == 0)
            {
                return ApiResponse<RawGoldTransferDto>.ErrorResponse("Unable to get current gold rates for conversion");
            }

            var fromValue = request.FromWeight * fromRate;
            var toWeight = fromValue / toRate;
            var conversionFactor = toWeight / request.FromWeight;

            var transferNumber = await GenerateTransferNumberAsync();

            var transfer = new RawGoldTransfer
            {
                TransferNumber = transferNumber,
                BranchId = request.BranchId,
                FromSupplierId = request.SupplierId,
                ToSupplierId = request.SupplierId,
                FromKaratTypeId = request.FromKaratTypeId,
                ToKaratTypeId = request.ToKaratTypeId,
                FromWeight = request.FromWeight,
                ToWeight = toWeight,
                FromGoldRate = fromRate,
                ToGoldRate = toRate,
                ConversionFactor = conversionFactor,
                TransferValue = fromValue,
                TransferDate = DateTime.UtcNow,
                TransferType = "Convert",
                Notes = request.Notes,
                CreatedByUserId = userId
            };

            _context.RawGoldTransfers.Add(transfer);

            // Update balances accordingly
            if (request.SupplierId.HasValue)
            {
                // Update supplier balances
                await UpdateSupplierGoldBalanceAsync(request.SupplierId.Value, request.BranchId, request.FromKaratTypeId, -request.FromWeight, fromRate, userId);
                await UpdateSupplierGoldBalanceAsync(request.SupplierId.Value, request.BranchId, request.ToKaratTypeId, toWeight, toRate, userId);
            }
            else
            {
                // Update merchant's raw gold inventory
                var fromInventory = await _context.RawGoldInventories
                    .FirstOrDefaultAsync(rgi => rgi.BranchId == request.BranchId && rgi.KaratTypeId == request.FromKaratTypeId);
                var toInventory = await _context.RawGoldInventories
                    .FirstOrDefaultAsync(rgi => rgi.BranchId == request.BranchId && rgi.KaratTypeId == request.ToKaratTypeId);

                if (fromInventory == null || fromInventory.AvailableWeight < request.FromWeight)
                {
                    return ApiResponse<RawGoldTransferDto>.ErrorResponse("Insufficient gold available for conversion");
                }

                fromInventory.WeightOnHand -= request.FromWeight;
                fromInventory.LastMovementDate = DateTime.UtcNow;

                if (toInventory == null)
                {
                    toInventory = new RawGoldInventory
                    {
                        BranchId = request.BranchId,
                        KaratTypeId = request.ToKaratTypeId,
                        WeightOnHand = toWeight,
                        AverageCostPerGram = toRate,
                        TotalValue = toWeight * toRate,
                        LastCountDate = DateTime.UtcNow,
                        CreatedBy = userId
                    };
                    _context.RawGoldInventories.Add(toInventory);
                }
                else
                {
                    toInventory.WeightOnHand += toWeight;
                    toInventory.LastMovementDate = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var transferWithNav = await _context.RawGoldTransfers
                .Include(t => t.Branch)
                .Include(t => t.FromSupplier)
                .Include(t => t.ToSupplier)
                .Include(t => t.FromKaratType)
                .Include(t => t.ToKaratType)
                .FirstOrDefaultAsync(t => t.Id == transfer.Id);

            var transferDto = MapToTransferDto(transferWithNav!);

            await _auditService.LogAsync("RawGoldTransfer", "Create",
                $"Gold converted: {request.FromWeight}g {await GetKaratTypeNameAsync(request.FromKaratTypeId)} to {toWeight}g {await GetKaratTypeNameAsync(request.ToKaratTypeId)}",
                userId);

            return ApiResponse<RawGoldTransferDto>.SuccessResponse(transferDto, "Gold karat conversion completed successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error converting gold karat");
            return ApiResponse<RawGoldTransferDto>.ErrorResponse("Failed to convert gold karat");
        }
    }

    public async Task<ApiResponse<List<MerchantRawGoldBalanceDto>>> GetMerchantRawGoldBalanceAsync(int branchId)
    {
        try
        {
            var inventory = await _context.RawGoldInventories
                .Where(rgi => rgi.BranchId == branchId)
                .Include(rgi => rgi.Branch)
                .Include(rgi => rgi.KaratType)
                .ToListAsync();

            var balanceDtos = inventory.Select(inv => new MerchantRawGoldBalanceDto
            {
                BranchId = inv.BranchId,
                BranchName = inv.Branch.Name,
                KaratTypeId = inv.KaratTypeId,
                KaratTypeName = inv.KaratType.Name,
                KaratPurity = 0, // TODO: Add purity calculation logic
                AvailableWeight = inv.AvailableWeight,
                AverageCostPerGram = inv.AverageCostPerGram,
                TotalValue = inv.TotalValue,
                LastMovementDate = inv.LastMovementDate
            }).ToList();

            return ApiResponse<List<MerchantRawGoldBalanceDto>>.SuccessResponse(balanceDtos, "Merchant raw gold balance retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving merchant raw gold balance for branch {BranchId}", branchId);
            return ApiResponse<List<MerchantRawGoldBalanceDto>>.ErrorResponse("Failed to retrieve merchant raw gold balance");
        }
    }

    public async Task<ApiResponse<CustomerPurchaseDto>> ProcessCustomerPurchaseAsync(CreateCustomerPurchaseRequest request, string userId)
    {
        // This method integrates with the existing CustomerPurchaseService
        // For now, we'll delegate to the existing service and enhance it later
        await Task.CompletedTask;
        throw new NotImplementedException("This method will be implemented to integrate with existing CustomerPurchaseService");
    }

    public async Task<ApiResponse<PagedResult<RawGoldTransferDto>>> GetTransferHistoryAsync(GoldTransferSearchRequest searchRequest)
    {
        try
        {
            var query = _context.RawGoldTransfers
                .Include(rgt => rgt.Branch)
                .Include(rgt => rgt.FromSupplier)
                .Include(rgt => rgt.ToSupplier)
                .Include(rgt => rgt.FromKaratType)
                .Include(rgt => rgt.ToKaratType)
                .Include(rgt => rgt.CustomerPurchase)
                .AsQueryable();

            // Apply filters
            if (searchRequest.BranchId.HasValue)
                query = query.Where(rgt => rgt.BranchId == searchRequest.BranchId.Value);

            if (searchRequest.SupplierId.HasValue)
                query = query.Where(rgt => rgt.FromSupplierId == searchRequest.SupplierId.Value || rgt.ToSupplierId == searchRequest.SupplierId.Value);

            if (searchRequest.KaratTypeId.HasValue)
                query = query.Where(rgt => rgt.FromKaratTypeId == searchRequest.KaratTypeId.Value || rgt.ToKaratTypeId == searchRequest.KaratTypeId.Value);

            if (!string.IsNullOrEmpty(searchRequest.TransferType))
                query = query.Where(rgt => rgt.TransferType == searchRequest.TransferType);

            if (searchRequest.FromDate.HasValue)
                query = query.Where(rgt => rgt.TransferDate >= searchRequest.FromDate.Value);

            if (searchRequest.ToDate.HasValue)
                query = query.Where(rgt => rgt.TransferDate <= searchRequest.ToDate.Value);

            var totalCount = await query.CountAsync();
            
            var transfers = await query
                .OrderByDescending(rgt => rgt.TransferDate)
                .Skip((searchRequest.PageNumber - 1) * searchRequest.PageSize)
                .Take(searchRequest.PageSize)
                .ToListAsync();

            var transferDtos = transfers.Select(MapToTransferDto).ToList();

            var result = new PagedResult<RawGoldTransferDto>
            {
                Items = transferDtos,
                TotalCount = totalCount,
                PageNumber = searchRequest.PageNumber,
                PageSize = searchRequest.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)searchRequest.PageSize)
            };

            return ApiResponse<PagedResult<RawGoldTransferDto>>.SuccessResponse(result, "Transfer history retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transfer history");
            return ApiResponse<PagedResult<RawGoldTransferDto>>.ErrorResponse("Failed to retrieve transfer history");
        }
    }

    public async Task<ApiResponse<GoldBalanceSummaryDto>> GetGoldBalanceSummaryAsync(int branchId)
    {
        try
        {
            var supplierBalancesResponse = await GetSupplierBalancesAsync(branchId);
            var merchantBalancesResponse = await GetMerchantRawGoldBalanceAsync(branchId);

            if (!supplierBalancesResponse.Success || !merchantBalancesResponse.Success)
            {
                return ApiResponse<GoldBalanceSummaryDto>.ErrorResponse("Failed to retrieve balance data");
            }

            var branch = await _context.Branches.FindAsync(branchId);

            var summary = new GoldBalanceSummaryDto
            {
                BranchId = branchId,
                BranchName = branch?.Name ?? "Unknown",
                SupplierBalances = supplierBalancesResponse.Data ?? new List<SupplierGoldBalanceDto>(),
                MerchantBalances = merchantBalancesResponse.Data ?? new List<MerchantRawGoldBalanceDto>(),
                TotalDebtValue = supplierBalancesResponse.Data?.Where(sb => sb.OutstandingMonetaryValue > 0).Sum(sb => sb.OutstandingMonetaryValue) ?? 0,
                TotalCreditValue = supplierBalancesResponse.Data?.Where(sb => sb.MerchantGoldBalance > 0).Sum(sb => sb.MerchantGoldBalance * sb.AverageCostPerGram) ?? 0,
                GeneratedAt = DateTime.UtcNow
            };

            summary.NetBalance = summary.TotalCreditValue - summary.TotalDebtValue;

            return ApiResponse<GoldBalanceSummaryDto>.SuccessResponse(summary, "Gold balance summary retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving gold balance summary for branch {BranchId}", branchId);
            return ApiResponse<GoldBalanceSummaryDto>.ErrorResponse("Failed to retrieve gold balance summary");
        }
    }

    public async Task<ApiResponse<KaratConversionDto>> CalculateKaratConversionAsync(int fromKaratTypeId, int toKaratTypeId, decimal fromWeight)
    {
        try
        {
            var fromRate = await GetCurrentGoldRateAsync(fromKaratTypeId);
            var toRate = await GetCurrentGoldRateAsync(toKaratTypeId);

            if (fromRate == 0 || toRate == 0)
            {
                return ApiResponse<KaratConversionDto>.ErrorResponse("Unable to get current gold rates for conversion");
            }

            var fromValue = fromWeight * fromRate;
            var toWeight = fromValue / toRate;
            var conversionFactor = toWeight / fromWeight;

            var fromKaratType = await _context.KaratTypeLookups.FindAsync(fromKaratTypeId);
            var toKaratType = await _context.KaratTypeLookups.FindAsync(toKaratTypeId);

            var conversion = new KaratConversionDto
            {
                FromKaratTypeId = fromKaratTypeId,
                FromKaratTypeName = fromKaratType?.Name ?? "Unknown",
                ToKaratTypeId = toKaratTypeId,
                ToKaratTypeName = toKaratType?.Name ?? "Unknown",
                FromWeight = fromWeight,
                ToWeight = toWeight,
                FromRate = fromRate,
                ToRate = toRate,
                ConversionFactor = conversionFactor,
                TransferValue = fromValue,
                CalculatedAt = DateTime.UtcNow
            };

            return ApiResponse<KaratConversionDto>.SuccessResponse(conversion, "Karat conversion calculated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating karat conversion");
            return ApiResponse<KaratConversionDto>.ErrorResponse("Failed to calculate karat conversion");
        }
    }

    public async Task<ApiResponse<List<MerchantRawGoldBalanceDto>>> GetAvailableGoldForWaivingAsync(int branchId, int? karatTypeId = null)
    {
        try
        {
            var query = _context.RawGoldInventories
                .Where(rgi => rgi.BranchId == branchId && rgi.AvailableWeight > 0)
                .Include(rgi => rgi.Branch)
                .Include(rgi => rgi.KaratType)
                .AsQueryable();

            if (karatTypeId.HasValue)
                query = query.Where(rgi => rgi.KaratTypeId == karatTypeId.Value);

            var inventory = await query.ToListAsync();

            var balanceDtos = inventory.Select(inv => new MerchantRawGoldBalanceDto
            {
                BranchId = inv.BranchId,
                BranchName = inv.Branch.Name,
                KaratTypeId = inv.KaratTypeId,
                KaratTypeName = inv.KaratType.Name,
                KaratPurity = 0, // TODO: Add purity calculation logic
                AvailableWeight = inv.AvailableWeight,
                AverageCostPerGram = inv.AverageCostPerGram,
                TotalValue = inv.AvailableWeight * inv.AverageCostPerGram,
                LastMovementDate = inv.LastMovementDate
            }).ToList();

            return ApiResponse<List<MerchantRawGoldBalanceDto>>.SuccessResponse(balanceDtos, "Available gold for waiving retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available gold for waiving for branch {BranchId}", branchId);
            return ApiResponse<List<MerchantRawGoldBalanceDto>>.ErrorResponse("Failed to retrieve available gold for waiving");
        }
    }

    #region Helper Methods

    private async Task<decimal> GetCurrentGoldRateAsync(int karatTypeId)
    {
        var rate = await _context.GoldRates
            .Where(gr => gr.KaratTypeId == karatTypeId && gr.IsCurrent)
            .OrderByDescending(gr => gr.EffectiveFrom)
            .FirstOrDefaultAsync();
            
        return rate?.RatePerGram ?? 0;
    }
    
    private async Task<string> GetKaratTypeNameAsync(int karatTypeId)
    {
        var karatType = await _context.KaratTypeLookups.FindAsync(karatTypeId);
        return karatType?.Name ?? "Unknown";
    }
    
    private async Task<string> GenerateTransferNumberAsync()
    {
        var today = DateTime.Today;
        var prefix = $"RGT{today:yyyyMMdd}";
        
        var lastTransfer = await _context.RawGoldTransfers
            .Where(rgt => rgt.TransferNumber.StartsWith(prefix))
            .OrderByDescending(rgt => rgt.TransferNumber)
            .FirstOrDefaultAsync();

        if (lastTransfer == null)
            return $"{prefix}001";

        var lastNumber = lastTransfer.TransferNumber.Substring(prefix.Length);
        if (int.TryParse(lastNumber, out var number))
            return $"{prefix}{(number + 1):D3}";

        return $"{prefix}001";
    }
    
    private async Task UpdateSupplierGoldBalanceAsync(int supplierId, int branchId, int karatTypeId, decimal weightChange, decimal rate, string userId)
    {
        var balance = await _context.SupplierGoldBalances
            .FirstOrDefaultAsync(sgb => sgb.SupplierId == supplierId && 
                                       sgb.BranchId == branchId && 
                                       sgb.KaratTypeId == karatTypeId);

        if (balance == null)
        {
            balance = new SupplierGoldBalance
            {
                SupplierId = supplierId,
                BranchId = branchId,
                KaratTypeId = karatTypeId,
                TotalWeightReceived = Math.Max(0, weightChange),
                TotalWeightPaidFor = 0,
                MerchantGoldBalance = weightChange,
                OutstandingMonetaryValue = 0,
                AverageCostPerGram = rate,
                LastTransactionDate = DateTime.UtcNow,
                CreatedBy = userId
            };
            _context.SupplierGoldBalances.Add(balance);
        }
        else
        {
            balance.MerchantGoldBalance += weightChange;
            balance.LastTransactionDate = DateTime.UtcNow;
            balance.ModifiedAt = DateTime.UtcNow;
            balance.ModifiedBy = userId;
        }
    }
    
    private async Task CreateInventoryMovementAsync(int branchId, int karatTypeId, decimal weightChange, string referenceNumber, string movementType, string userId)
    {
        var inventory = await _context.RawGoldInventories
            .FirstOrDefaultAsync(rgi => rgi.BranchId == branchId && rgi.KaratTypeId == karatTypeId);

        if (inventory != null)
        {
            var movement = new RawGoldInventoryMovement
            {
                RawGoldInventoryId = inventory.Id,
                MovementType = movementType,
                WeightChange = weightChange,
                WeightBalance = inventory.WeightOnHand,
                MovementDate = DateTime.UtcNow,
                ReferenceNumber = referenceNumber,
                UnitCost = inventory.AverageCostPerGram,
                CreatedBy = userId
            };

            _context.RawGoldInventoryMovements.Add(movement);
        }
    }

    private RawGoldTransferDto MapToTransferDto(RawGoldTransfer transfer)
    {
        return new RawGoldTransferDto
        {
            Id = transfer.Id,
            TransferNumber = transfer.TransferNumber,
            BranchId = transfer.BranchId,
            BranchName = transfer.Branch?.Name ?? "Unknown",
            FromSupplierId = transfer.FromSupplierId,
            FromSupplierName = transfer.FromSupplier?.CompanyName,
            ToSupplierId = transfer.ToSupplierId,
            ToSupplierName = transfer.ToSupplier?.CompanyName,
            FromKaratTypeId = transfer.FromKaratTypeId,
            FromKaratTypeName = transfer.FromKaratType?.Name ?? "Unknown",
            ToKaratTypeId = transfer.ToKaratTypeId,
            ToKaratTypeName = transfer.ToKaratType?.Name ?? "Unknown",
            FromWeight = transfer.FromWeight,
            ToWeight = transfer.ToWeight,
            FromGoldRate = transfer.FromGoldRate,
            ToGoldRate = transfer.ToGoldRate,
            ConversionFactor = transfer.ConversionFactor,
            TransferValue = transfer.TransferValue,
            TransferDate = transfer.TransferDate,
            TransferType = transfer.TransferType,
            CustomerPurchaseId = transfer.CustomerPurchaseId,
            CustomerPurchaseNumber = transfer.CustomerPurchase?.PurchaseNumber,
            Notes = transfer.Notes,
            CreatedByUserId = transfer.CreatedByUserId,
            CreatedAt = transfer.CreatedAt
        };
    }

    #endregion
}
