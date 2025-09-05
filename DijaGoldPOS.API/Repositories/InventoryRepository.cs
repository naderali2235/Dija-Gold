using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Models.InventoryModels;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository implementation for Inventory entity with specific business methods
/// </summary>
public class InventoryRepository : Repository<Inventory>, IInventoryRepository
{
    public InventoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get inventory by product and branch
    /// </summary>
    public async Task<Inventory?> GetByProductAndBranchAsync(int productId, int branchId)
    {
        return await _dbSet
            .Include(i => i.Product)
            .Include(i => i.Branch)
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.BranchId == branchId);
    }

    /// <summary>
    /// Get all inventory records for a branch
    /// </summary>
    public async Task<List<Inventory>> GetByBranchAsync(int branchId, bool includeZeroStock = false)
    {
        var query = _dbSet
            .Include(i => i.Product)
            .Include(i => i.Branch)
            .Where(i => i.BranchId == branchId);

        if (!includeZeroStock)
        {
            query = query.Where(i => i.QuantityOnHand > 0 || i.WeightOnHand > 0);
        }

        return await query
            .OrderBy(i => i.Product.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Get all inventory records for a product across all branches
    /// </summary>
    public async Task<List<Inventory>> GetByProductAsync(int productId)
    {
        return await _dbSet
            .Include(i => i.Product)
            .Include(i => i.Branch)
            .Where(i => i.ProductId == productId)
            .OrderBy(i => i.Branch.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Get low stock items for a branch
    /// </summary>
    public async Task<List<Inventory>> GetLowStockItemsAsync(int branchId)
    {
        return await _dbSet
            .Include(i => i.Product)
            .Include(i => i.Branch)
            .Where(i => i.BranchId == branchId && 
                       i.QuantityOnHand <= i.ReorderPoint && 
                       i.QuantityOnHand > 0)
            .OrderBy(i => i.Product.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Get out of stock items for a branch
    /// </summary>
    public async Task<List<Inventory>> GetOutOfStockItemsAsync(int branchId)
    {
        return await _dbSet
            .Include(i => i.Product)
            .Include(i => i.Branch)
            .Where(i => i.BranchId == branchId && 
                       i.QuantityOnHand == 0 && 
                       i.WeightOnHand == 0)
            .OrderBy(i => i.Product.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Get items at or above maximum stock level for a branch
    /// </summary>
    public async Task<List<Inventory>> GetOverstockedItemsAsync(int branchId)
    {
        return await _dbSet
            .Include(i => i.Product)
            .Include(i => i.Branch)
            .Where(i => i.BranchId == branchId && 
                       i.MaximumStockLevel > 0 &&
                       i.QuantityOnHand >= i.MaximumStockLevel)
            .OrderBy(i => i.Product.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Get total stock value for a branch
    /// </summary>
    public async Task<decimal> GetBranchStockValueAsync(int branchId, int goldRateId)
    {
        // This would require joining with GoldRate and calculating based on current rates
        // For now, return a simple calculation - this would need to be enhanced
        var inventoryItems = await _dbSet
            .Include(i => i.Product)
            .Where(i => i.BranchId == branchId && i.WeightOnHand > 0)
            .ToListAsync();

        // This is a simplified calculation - in reality, you'd join with current gold rates
        return inventoryItems.Sum(i => i.WeightOnHand * 100); // Placeholder calculation
    }

    /// <summary>
    /// Get inventory summary by karat type for a branch
    /// </summary>
    public async Task<Dictionary<string, decimal>> GetInventorySummaryByKaratAsync(int branchId)
    {
        return await _dbSet
            .Include(i => i.Product)
            .Where(i => i.BranchId == branchId && i.WeightOnHand > 0)
            .GroupBy(i => i.Product.KaratType.Name ?? "Unknown")
            .Select(g => new { KaratType = g.Key, TotalWeight = g.Sum(i => i.WeightOnHand) })
            .ToDictionaryAsync(x => x.KaratType, x => x.TotalWeight);
    }

    /// <summary>
    /// Check if sufficient stock is available for a transaction
    /// </summary>
    public async Task<bool> CheckStockAvailabilityAsync(int productId, int branchId, decimal requiredQuantity)
    {
        var inventory = await GetByProductAndBranchAsync(productId, branchId);
        return inventory != null && inventory.QuantityOnHand >= requiredQuantity;
    }

    /// <summary>
    /// Get inventory movements for a specific inventory item
    /// </summary>
    public async Task<List<InventoryMovement>> GetMovementsAsync(int inventoryId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.InventoryMovements
            .Where(im => im.InventoryId == inventoryId);

        if (fromDate.HasValue)
        {
            query = query.Where(im => im.MovementDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(im => im.MovementDate <= toDate.Value);
        }

        return await query
            .OrderByDescending(im => im.MovementDate)
            .ToListAsync();
    }
}
