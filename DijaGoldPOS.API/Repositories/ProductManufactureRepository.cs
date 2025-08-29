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
            .Include(pm => pm.SourceRawGoldPurchaseOrderItem)
                .ThenInclude(rgpoi => rgpoi.RawGoldPurchaseOrder)
                    .ThenInclude(rgpo => rgpo.Supplier)
            .Include(pm => pm.SourceRawGoldPurchaseOrderItem)
                .ThenInclude(rgpoi => rgpoi.KaratType)
            .Where(pm => pm.ProductId == productId)
            .OrderByDescending(pm => pm.ManufactureDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductManufacture>> GetByPurchaseOrderItemIdAsync(int purchaseOrderItemId)
    {
        return await _context.ProductManufactures
            .Include(pm => pm.Product)
            .Include(pm => pm.SourceRawGoldPurchaseOrderItem)
                .ThenInclude(rgpoi => rgpoi.RawGoldPurchaseOrder)
                    .ThenInclude(rgpo => rgpo.Supplier)
            .Include(pm => pm.SourceRawGoldPurchaseOrderItem)
                .ThenInclude(rgpoi => rgpoi.KaratType)
            .Where(pm => pm.SourceRawGoldPurchaseOrderItemId == purchaseOrderItemId)
            .OrderByDescending(pm => pm.ManufactureDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductManufacture>> GetByPurchaseOrderIdAsync(int purchaseOrderId)
    {
        return await _context.ProductManufactures
            .Include(pm => pm.Product)
            .Include(pm => pm.SourceRawGoldPurchaseOrderItem)
                .ThenInclude(rgpoi => rgpoi.RawGoldPurchaseOrder)
                    .ThenInclude(rgpo => rgpo.Supplier)
            .Include(pm => pm.SourceRawGoldPurchaseOrderItem)
                .ThenInclude(rgpoi => rgpoi.KaratType)
            .Where(pm => pm.SourceRawGoldPurchaseOrderItem.RawGoldPurchaseOrderId == purchaseOrderId)
            .OrderByDescending(pm => pm.ManufactureDate)
            .ToListAsync();
    }

    public async Task<ManufacturingSummaryByPurchaseOrderDto?> GetManufacturingSummaryByPurchaseOrderAsync(int purchaseOrderId)
    {
        var rawGoldPurchaseOrder = await _context.RawGoldPurchaseOrders
            .Include(rgpo => rgpo.Supplier)
            .Include(rgpo => rgpo.RawGoldPurchaseOrderItems)
            .FirstOrDefaultAsync(rgpo => rgpo.Id == purchaseOrderId);

        if (rawGoldPurchaseOrder == null)
            return null;

        var manufacturingRecords = await GetByPurchaseOrderIdAsync(purchaseOrderId);
        var recordsList = manufacturingRecords.ToList();

        var summary = new ManufacturingSummaryByPurchaseOrderDto
        {
            PurchaseOrderId = rawGoldPurchaseOrder.Id,
            PurchaseOrderNumber = rawGoldPurchaseOrder.PurchaseOrderNumber,
            SupplierName = rawGoldPurchaseOrder.Supplier.CompanyName,
            OrderDate = rawGoldPurchaseOrder.OrderDate,
            TotalRawGoldWeight = rawGoldPurchaseOrder.RawGoldPurchaseOrderItems.Sum(i => i.WeightReceived),
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
                SourceRawGoldPurchaseOrderItemId = r.SourceRawGoldPurchaseOrderItemId,
                PurchaseOrderNumber = r.SourceRawGoldPurchaseOrderItem.RawGoldPurchaseOrder.PurchaseOrderNumber,
                SupplierName = r.SourceRawGoldPurchaseOrderItem.RawGoldPurchaseOrder.Supplier.CompanyName,
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
                SourceRawGoldPurchaseOrderItemId = r.SourceRawGoldPurchaseOrderItemId,
                PurchaseOrderNumber = r.SourceRawGoldPurchaseOrderItem.RawGoldPurchaseOrder.PurchaseOrderNumber,
                SupplierName = r.SourceRawGoldPurchaseOrderItem.RawGoldPurchaseOrder.Supplier.CompanyName,
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
            .Include(pm => pm.SourceRawGoldPurchaseOrderItem)
                .ThenInclude(rgpoi => rgpoi.RawGoldPurchaseOrder)
                    .ThenInclude(rgpo => rgpo.Supplier)
            .Include(pm => pm.SourceRawGoldPurchaseOrderItem)
                .ThenInclude(rgpoi => rgpoi.KaratType)
            .Where(pm => pm.BatchNumber == batchNumber)
            .OrderByDescending(pm => pm.ManufactureDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductManufacture>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.ProductManufactures
            .Include(pm => pm.Product)
            .Include(pm => pm.SourceRawGoldPurchaseOrderItem)
                .ThenInclude(rgpoi => rgpoi.RawGoldPurchaseOrder)
                    .ThenInclude(rgpo => rgpo.Supplier)
            .Include(pm => pm.SourceRawGoldPurchaseOrderItem)
                .ThenInclude(rgpoi => rgpoi.KaratType)
            .Where(pm => pm.ManufactureDate >= startDate && pm.ManufactureDate <= endDate)
            .OrderByDescending(pm => pm.ManufactureDate)
            .ToListAsync();
    }

    public async Task<decimal> GetRemainingRawGoldWeightAsync(int rawGoldPurchaseOrderItemId)
    {
        var rawGoldItem = await _context.RawGoldPurchaseOrderItems
            .FirstOrDefaultAsync(rgpoi => rgpoi.Id == rawGoldPurchaseOrderItemId);

        if (rawGoldItem == null)
            return 0;

        var consumedWeight = await _context.ProductManufactures
            .Where(pm => pm.SourceRawGoldPurchaseOrderItemId == rawGoldPurchaseOrderItemId)
            .SumAsync(pm => pm.ConsumedWeight + pm.WastageWeight);

        return rawGoldItem.WeightReceived - consumedWeight;
    }

    public async Task<bool> IsSufficientRawGoldAvailableAsync(int rawGoldPurchaseOrderItemId, decimal requiredWeight)
    {
        var remainingWeight = await GetRemainingRawGoldWeightAsync(rawGoldPurchaseOrderItemId);
        return remainingWeight >= requiredWeight;
    }

    public async Task<decimal> GetTotalConsumedWeightByPurchaseOrderItemAsync(int purchaseOrderItemId)
    {
        return await _context.ProductManufactures
            .Where(pm => pm.SourceRawGoldPurchaseOrderItemId == purchaseOrderItemId)
            .SumAsync(pm => pm.ConsumedWeight + pm.WastageWeight);
    }

    public async Task<decimal> GetTotalConsumedWeightByRawGoldItemAsync(int rawGoldPurchaseOrderItemId)
    {
        return await _context.ProductManufactures
            .Where(pm => pm.SourceRawGoldPurchaseOrderItemId == rawGoldPurchaseOrderItemId)
            .SumAsync(pm => pm.ConsumedWeight + pm.WastageWeight);
    }
}
