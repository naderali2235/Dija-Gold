using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Interface for inventory management service
/// </summary>
public interface IInventoryService
{
    /// <summary>
    /// Get inventory for a specific product at a branch
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="branchId">Branch ID</param>
    /// <returns>Inventory record</returns>
    Task<Inventory?> GetInventoryAsync(int productId, int branchId);

    /// <summary>
    /// Get all inventory for a branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="includeZeroStock">Include products with zero stock</param>
    /// <returns>List of inventory records</returns>
    Task<List<Inventory>> GetBranchInventoryAsync(int branchId, bool includeZeroStock = false);

    /// <summary>
    /// Get low stock items for a branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <returns>List of low stock inventory records</returns>
    Task<List<Inventory>> GetLowStockItemsAsync(int branchId);

    /// <summary>
    /// Check if sufficient stock is available for sale
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="branchId">Branch ID</param>
    /// <param name="requestedQuantity">Requested quantity</param>
    /// <returns>True if sufficient stock available</returns>
    Task<bool> CheckStockAvailabilityAsync(int productId, int branchId, decimal requestedQuantity);

    /// <summary>
    /// Reserve inventory for an order (decrease stock)
    /// </summary>
    /// <param name="orderItems">List of order items</param>
    /// <param name="branchId">Branch ID</param>
    /// <param name="userId">User performing the action</param>
    /// <returns>Success status</returns>
    Task<bool> ReserveInventoryAsync(List<OrderItem> orderItems, int branchId, string userId);

    /// <summary>
    /// Reserve inventory for an order (decrease stock) - internal method that works within existing transaction
    /// </summary>
    /// <param name="orderItems">List of order items</param>
    /// <param name="branchId">Branch ID</param>
    /// <param name="userId">User performing the action</param>
    /// <returns>Success status</returns>
    Task<bool> ReserveInventoryInternalAsync(List<OrderItem> orderItems, int branchId, string userId);

    /// <summary>
    /// Release reserved inventory (return to stock)
    /// </summary>
    /// <param name="orderItems">List of order items</param>
    /// <param name="branchId">Branch ID</param>
    /// <param name="userId">User performing the action</param>
    /// <returns>Success status</returns>
    Task<bool> ReleaseInventoryAsync(List<OrderItem> orderItems, int branchId, string userId);

    /// <summary>
    /// Add inventory (from purchases or adjustments)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="branchId">Branch ID</param>
    /// <param name="quantity">Quantity to add</param>
    /// <param name="weight">Weight to add</param>
    /// <param name="movementType">Type of movement</param>
    /// <param name="referenceNumber">Reference number</param>
    /// <param name="unitCost">Unit cost (for purchases)</param>
    /// <param name="userId">User performing the action</param>
    /// <param name="notes">Additional notes</param>
    /// <returns>Success status</returns>
    Task<bool> AddInventoryAsync(
        int productId,
        int branchId,
        decimal quantity,
        decimal weight,
        string movementType,
        string? referenceNumber,
        decimal? unitCost,
        string userId,
        string? notes = null);

    /// <summary>
    /// Adjust inventory (manual adjustment)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="branchId">Branch ID</param>
    /// <param name="newQuantity">New quantity</param>
    /// <param name="newWeight">New weight</param>
    /// <param name="reason">Reason for adjustment</param>
    /// <param name="userId">User performing the action</param>
    /// <returns>Success status</returns>
    Task<bool> AdjustInventoryAsync(
        int productId,
        int branchId,
        decimal newQuantity,
        decimal newWeight,
        string reason,
        string userId);

    /// <summary>
    /// Transfer inventory between branches
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="fromBranchId">Source branch ID</param>
    /// <param name="toBranchId">Destination branch ID</param>
    /// <param name="quantity">Quantity to transfer</param>
    /// <param name="weight">Weight to transfer</param>
    /// <param name="transferNumber">Transfer reference number</param>
    /// <param name="userId">User performing the action</param>
    /// <param name="notes">Additional notes</param>
    /// <returns>Success status</returns>
    Task<bool> TransferInventoryAsync(
        int productId,
        int fromBranchId,
        int toBranchId,
        decimal quantity,
        decimal weight,
        string transferNumber,
        string userId,
        string? notes = null);

    /// <summary>
    /// Get inventory movement history
    /// </summary>
    /// <param name="productId">Product ID (optional)</param>
    /// <param name="branchId">Branch ID (optional)</param>
    /// <param name="fromDate">From date (optional)</param>
    /// <param name="toDate">To date (optional)</param>
    /// <param name="movementType">Movement type filter (optional)</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of inventory movements</returns>
    Task<(List<InventoryMovement> Movements, int TotalCount)> GetInventoryMovementsAsync(
        int? productId = null,
        int? branchId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? movementType = null,
        int pageNumber = 1,
        int pageSize = 50);
}