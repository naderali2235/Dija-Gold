using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository implementation for ProductManufacture entity
/// </summary>
public class ProductManufactureRepository : Repository<ProductManufacture>, IProductManufactureRepository
{
    public ProductManufactureRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProductManufacture>> GetByProductIdAsync(int productId)
    {
        return await _context.ProductManufactures
            .Include(pm => pm.Product)
            .Include(pm => pm.SourcePurchaseOrderItem)
                .ThenInclude(poi => poi.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
            .Where(pm => pm.ProductId == productId)
            .OrderByDescending(pm => pm.ManufactureDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductManufacture>> GetByPurchaseOrderItemIdAsync(int purchaseOrderItemId)
    {
        return await _context.ProductManufactures
            .Include(pm => pm.Product)
            .Include(pm => pm.SourcePurchaseOrderItem)
                .ThenInclude(poi => poi.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
            .Where(pm => pm.SourcePurchaseOrderItemId == purchaseOrderItemId)
            .OrderByDescending(pm => pm.ManufactureDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductManufacture>> GetByPurchaseOrderIdAsync(int purchaseOrderId)
    {
        return await _context.ProductManufactures
            .Include(pm => pm.Product)
            .Include(pm => pm.SourcePurchaseOrderItem)
                .ThenInclude(poi => poi.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
            .Where(pm => pm.SourcePurchaseOrderItem.PurchaseOrderId == purchaseOrderId)
            .OrderByDescending(pm => pm.ManufactureDate)
            .ToListAsync();
    }

    public async Task<ManufacturingSummaryByPurchaseOrderDto?> GetManufacturingSummaryByPurchaseOrderAsync(int purchaseOrderId)
    {
        var purchaseOrder = await _context.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.PurchaseOrderItems)
            .FirstOrDefaultAsync(po => po.Id == purchaseOrderId);

        if (purchaseOrder == null)
            return null;

        var manufacturingRecords = await GetByPurchaseOrderIdAsync(purchaseOrderId);
        var recordsList = manufacturingRecords.ToList();

        var summary = new ManufacturingSummaryByPurchaseOrderDto
        {
            PurchaseOrderId = purchaseOrder.Id,
            PurchaseOrderNumber = purchaseOrder.PurchaseOrderNumber,
            SupplierName = purchaseOrder.Supplier.CompanyName,
            OrderDate = purchaseOrder.OrderDate,
            TotalRawGoldWeight = purchaseOrder.PurchaseOrderItems
                .Where(poi => poi.IsRawGold)
                .Sum(poi => poi.WeightReceived),
            TotalConsumedWeight = recordsList.Sum(r => r.ConsumedWeight),
            TotalWastageWeight = recordsList.Sum(r => r.WastageWeight),
            TotalProductsManufactured = recordsList.Count,
            TotalManufacturingCost = recordsList.Sum(r => r.TotalManufacturingCost),
            ManufacturingRecords = recordsList.Select(r => new ProductManufactureDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product.Name,
                ProductCode = r.Product.ProductCode,
                SourcePurchaseOrderItemId = r.SourcePurchaseOrderItemId,
                PurchaseOrderNumber = r.SourcePurchaseOrderItem.PurchaseOrder.PurchaseOrderNumber,
                SupplierName = r.SourcePurchaseOrderItem.PurchaseOrder.Supplier.CompanyName,
                ConsumedWeight = r.ConsumedWeight,
                WastageWeight = r.WastageWeight,
                ManufactureDate = r.ManufactureDate,
                ManufacturingCostPerGram = r.ManufacturingCostPerGram,
                TotalManufacturingCost = r.TotalManufacturingCost,
                BatchNumber = r.BatchNumber,
                ManufacturingNotes = r.ManufacturingNotes,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.ModifiedAt
            }).ToList()
        };

        summary.RemainingRawGoldWeight = summary.TotalRawGoldWeight - summary.TotalConsumedWeight - summary.TotalWastageWeight;

        return summary;
    }

    public async Task<ManufacturingSummaryByProductDto?> GetManufacturingSummaryByProductAsync(int productId)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
            return null;

        var manufacturingRecords = await GetByProductIdAsync(productId);
        var recordsList = manufacturingRecords.ToList();

        var summary = new ManufacturingSummaryByProductDto
        {
            ProductId = product.Id,
            ProductName = product.Name,
            ProductCode = product.ProductCode,
            TotalConsumedWeight = recordsList.Sum(r => r.ConsumedWeight),
            TotalWastageWeight = recordsList.Sum(r => r.WastageWeight),
            TotalManufacturingCost = recordsList.Sum(r => r.TotalManufacturingCost),
            TotalManufacturingRecords = recordsList.Count,
            ManufacturingRecords = recordsList.Select(r => new ProductManufactureDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product.Name,
                ProductCode = r.Product.ProductCode,
                SourcePurchaseOrderItemId = r.SourcePurchaseOrderItemId,
                PurchaseOrderNumber = r.SourcePurchaseOrderItem.PurchaseOrder.PurchaseOrderNumber,
                SupplierName = r.SourcePurchaseOrderItem.PurchaseOrder.Supplier.CompanyName,
                ConsumedWeight = r.ConsumedWeight,
                WastageWeight = r.WastageWeight,
                ManufactureDate = r.ManufactureDate,
                ManufacturingCostPerGram = r.ManufacturingCostPerGram,
                TotalManufacturingCost = r.TotalManufacturingCost,
                BatchNumber = r.BatchNumber,
                ManufacturingNotes = r.ManufacturingNotes,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.ModifiedAt
            }).ToList()
        };

        return summary;
    }

    public async Task<IEnumerable<ProductManufacture>> GetByBatchNumberAsync(string batchNumber)
    {
        return await _context.ProductManufactures
            .Include(pm => pm.Product)
            .Include(pm => pm.SourcePurchaseOrderItem)
                .ThenInclude(poi => poi.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
            .Where(pm => pm.BatchNumber == batchNumber)
            .OrderByDescending(pm => pm.ManufactureDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductManufacture>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.ProductManufactures
            .Include(pm => pm.Product)
            .Include(pm => pm.SourcePurchaseOrderItem)
                .ThenInclude(poi => poi.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
            .Where(pm => pm.ManufactureDate >= startDate && pm.ManufactureDate <= endDate)
            .OrderByDescending(pm => pm.ManufactureDate)
            .ToListAsync();
    }

    public async Task<decimal> GetRemainingRawGoldWeightAsync(int purchaseOrderItemId)
    {
        var purchaseOrderItem = await _context.PurchaseOrderItems
            .FirstOrDefaultAsync(poi => poi.Id == purchaseOrderItemId);

        if (purchaseOrderItem == null || !purchaseOrderItem.IsRawGold)
            return 0;

        var consumedWeight = await _context.ProductManufactures
            .Where(pm => pm.SourcePurchaseOrderItemId == purchaseOrderItemId)
            .SumAsync(pm => pm.ConsumedWeight + pm.WastageWeight);

        return purchaseOrderItem.WeightReceived - consumedWeight;
    }

    public async Task<bool> IsSufficientRawGoldAvailableAsync(int purchaseOrderItemId, decimal requiredWeight)
    {
        var remainingWeight = await GetRemainingRawGoldWeightAsync(purchaseOrderItemId);
        return remainingWeight >= requiredWeight;
    }

    public async Task<decimal> GetTotalConsumedWeightByPurchaseOrderItemAsync(int purchaseOrderItemId)
    {
        return await _context.ProductManufactures
            .Where(pm => pm.SourcePurchaseOrderItemId == purchaseOrderItemId)
            .SumAsync(pm => pm.ConsumedWeight + pm.WastageWeight);
    }
}
