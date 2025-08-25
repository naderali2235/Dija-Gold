using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.IRepositories;

/// <summary>
/// Repository interface for InventoryMovement entities
/// </summary>
public interface IInventoryMovementRepository : IRepository<InventoryMovement>
{
    /// <summary>
    /// Get inventory movements for a specific inventory item
    /// </summary>
    Task<List<InventoryMovement>> GetByInventoryIdAsync(int inventoryId, DateTime? fromDate = null, DateTime? toDate = null);
    
    /// <summary>
    /// Get inventory movements by movement type
    /// </summary>
    Task<List<InventoryMovement>> GetByMovementTypeAsync(string movementType, DateTime? fromDate = null, DateTime? toDate = null);
    
    /// <summary>
    /// Get inventory movements by reference number
    /// </summary>
    Task<List<InventoryMovement>> GetByReferenceNumberAsync(string referenceNumber);
}
