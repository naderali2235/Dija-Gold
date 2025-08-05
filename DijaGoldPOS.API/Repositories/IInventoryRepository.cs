using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository interface for Inventory entity with specific business methods
/// </summary>
public interface IInventoryRepository : IRepository<Inventory>
{
    /// <summary>
    /// Get inventory by product and branch
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="branchId">Branch ID</param>
    /// <returns>Inventory record or null if not found</returns>
    Task<Inventory?> GetByProductAndBranchAsync(int productId, int branchId);

    /// <summary>
    /// Get all inventory records for a branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="includeZeroStock">Include items with zero stock</param>
    /// <returns>List of inventory records</returns>
    Task<List<Inventory>> GetByBranchAsync(int branchId, bool includeZeroStock = false);

    /// <summary>
    /// Get all inventory records for a product across all branches
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of inventory records</returns>
    Task<List<Inventory>> GetByProductAsync(int productId);

    /// <summary>
    /// Get low stock items for a branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <returns>List of low stock inventory records</returns>
    Task<List<Inventory>> GetLowStockItemsAsync(int branchId);

    /// <summary>
    /// Get out of stock items for a branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <returns>List of out of stock inventory records</returns>
    Task<List<Inventory>> GetOutOfStockItemsAsync(int branchId);

    /// <summary>
    /// Get items at or above maximum stock level for a branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <returns>List of overstocked inventory records</returns>
    Task<List<Inventory>> GetOverstockedItemsAsync(int branchId);

    /// <summary>
    /// Get total stock value for a branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="goldRateId">Current gold rate ID for valuation</param>
    /// <returns>Total stock value</returns>
    Task<decimal> GetBranchStockValueAsync(int branchId, int goldRateId);

    /// <summary>
    /// Get inventory summary by karat type for a branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <returns>Dictionary with karat type as key and total weight as value</returns>
    Task<Dictionary<string, decimal>> GetInventorySummaryByKaratAsync(int branchId);

    /// <summary>
    /// Check if sufficient stock is available for a transaction
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="branchId">Branch ID</param>
    /// <param name="requiredQuantity">Required quantity</param>
    /// <returns>True if sufficient stock available</returns>
    Task<bool> CheckStockAvailabilityAsync(int productId, int branchId, decimal requiredQuantity);

    /// <summary>
    /// Get inventory movements for a specific inventory item
    /// </summary>
    /// <param name="inventoryId">Inventory ID</param>
    /// <param name="fromDate">From date (optional)</param>
    /// <param name="toDate">To date (optional)</param>
    /// <returns>List of inventory movements</returns>
    Task<List<InventoryMovement>> GetMovementsAsync(int inventoryId, DateTime? fromDate = null, DateTime? toDate = null);
}