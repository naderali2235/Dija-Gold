using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Repositories;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Inventory management service implementation using Unit of Work pattern
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InventoryService> _logger;
    private readonly IAuditService _auditService;

    public InventoryService(
        IUnitOfWork unitOfWork,
        ILogger<InventoryService> logger,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _auditService = auditService;
    }

    /// <summary>
    /// Get inventory for a specific product at a branch
    /// </summary>
    public async Task<Inventory?> GetInventoryAsync(int productId, int branchId)
    {
        try
        {
            return await _unitOfWork.Inventory.GetByProductAndBranchAsync(productId, branchId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory for product {ProductId} at branch {BranchId}", productId, branchId);
            throw;
        }
    }

    /// <summary>
    /// Get all inventory for a branch
    /// </summary>
    public async Task<List<Inventory>> GetBranchInventoryAsync(int branchId, bool includeZeroStock = false)
    {
        try
        {
            return await _unitOfWork.Inventory.GetByBranchAsync(branchId, includeZeroStock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch inventory for branch {BranchId}", branchId);
            throw;
        }
    }

    /// <summary>
    /// Get low stock items for a branch
    /// </summary>
    public async Task<List<Inventory>> GetLowStockItemsAsync(int branchId)
    {
        try
        {
            return await _unitOfWork.Inventory.GetLowStockItemsAsync(branchId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low stock items for branch {BranchId}", branchId);
            throw;
        }
    }

    /// <summary>
    /// Check if sufficient stock is available for sale
    /// </summary>
    public async Task<bool> CheckStockAvailabilityAsync(int productId, int branchId, decimal requestedQuantity)
    {
        try
        {
            return await _unitOfWork.Inventory.CheckStockAvailabilityAsync(productId, branchId, requestedQuantity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking stock availability for product {ProductId} at branch {BranchId}", productId, branchId);
            throw;
        }
    }

    /// <summary>
    /// Reserve inventory for a transaction (decrease stock)
    /// </summary>
    public async Task<bool> ReserveInventoryAsync(List<TransactionItem> transactionItems, int branchId, string userId)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                return await ReserveInventoryInternalAsync(transactionItems, branchId, userId);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving inventory for branch {BranchId}", branchId);
            throw;
        }
    }

    /// <summary>
    /// Reserve inventory for a transaction (decrease stock) - internal method that works within existing transaction
    /// </summary>
    public async Task<bool> ReserveInventoryInternalAsync(List<TransactionItem> transactionItems, int branchId, string userId)
    {
        try
        {
            foreach (var item in transactionItems)
            {
                var inventory = await _unitOfWork.Inventory.GetByProductAndBranchAsync(item.ProductId, branchId);
                if (inventory == null)
                {
                    _logger.LogError("Inventory not found for product {ProductId} at branch {BranchId}", 
                        item.ProductId, branchId);
                    return false;
                }

                if (inventory.QuantityOnHand < item.Quantity)
                {
                    _logger.LogError("Insufficient stock for product {ProductId}. Available: {Available}, Required: {Required}", 
                        item.ProductId, inventory.QuantityOnHand, item.Quantity);
                    return false;
                }

                // Update inventory
                inventory.QuantityOnHand -= item.Quantity;
                inventory.WeightOnHand -= item.TotalWeight;
                _unitOfWork.Inventory.Update(inventory);

                // Create inventory movement record
                var movement = new InventoryMovement
                {
                    InventoryId = inventory.Id,
                    MovementType = "Sale",
                    QuantityChange = -item.Quantity,
                    WeightChange = -item.TotalWeight,
                    QuantityBalance = inventory.QuantityOnHand,
                    WeightBalance = inventory.WeightOnHand,
                    MovementDate = DateTime.UtcNow,
                    ReferenceNumber = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    Notes = $"Reserved for transaction by user {userId}",
                    CreatedBy = userId
                };

                await _unitOfWork.InventoryMovements.AddAsync(movement);

                // Log audit trail
                await _auditService.LogAsync(userId, "Reserve", "Inventory", inventory.Id.ToString(), 
                    $"Reserved {item.Quantity} units, {item.TotalWeight}g for transaction", null, null, null, null, branchId);
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in internal inventory reservation for branch {BranchId}", branchId);
            throw;
        }
    }

    /// <summary>
    /// Release reserved inventory (return to stock)
    /// </summary>
    public async Task<bool> ReleaseInventoryAsync(List<TransactionItem> transactionItems, int branchId, string userId)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                foreach (var item in transactionItems)
                {
                    var inventory = await _unitOfWork.Inventory.GetByProductAndBranchAsync(item.ProductId, branchId);
                    if (inventory == null)
                    {
                        _logger.LogError("Inventory not found for product {ProductId} at branch {BranchId}", 
                            item.ProductId, branchId);
                        return false;
                    }

                    // Update inventory
                    inventory.QuantityOnHand += item.Quantity;
                    inventory.WeightOnHand += item.TotalWeight;
                    _unitOfWork.Inventory.Update(inventory);

                    // Create inventory movement record
                    var movement = new InventoryMovement
                    {
                        InventoryId = inventory.Id,
                        MovementType = "Return",
                        QuantityChange = item.Quantity,
                        WeightChange = item.TotalWeight,
                        QuantityBalance = inventory.QuantityOnHand,
                        WeightBalance = inventory.WeightOnHand,
                        MovementDate = DateTime.UtcNow,
                        ReferenceNumber = $"RET-{DateTime.UtcNow:yyyyMMddHHmmss}",
                        Notes = $"Released from reserved inventory by user {userId}",
                        CreatedBy = userId
                    };

                    await _unitOfWork.InventoryMovements.AddAsync(movement);

                    // Log audit trail
                    await _auditService.LogAsync(userId, "Release", "Inventory", inventory.Id.ToString(), 
                        $"Released {item.Quantity} units, {item.TotalWeight}g from reservation", null, null, null, null, branchId);
                }

                await _unitOfWork.SaveChangesAsync();
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing inventory for branch {BranchId}", branchId);
            throw;
        }
    }

    /// <summary>
    /// Add inventory (from purchases or adjustments)
    /// </summary>
    public async Task<bool> AddInventoryAsync(
        int productId,
        int branchId,
        decimal quantity,
        decimal weight,
        string movementType,
        string? referenceNumber,
        decimal? unitCost,
        string userId,
        string? notes = null)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var inventory = await _unitOfWork.Inventory.GetByProductAndBranchAsync(productId, branchId);
                
                if (inventory == null)
                {
                    // Create new inventory record
                    inventory = new Inventory
                    {
                        ProductId = productId,
                        BranchId = branchId,
                        QuantityOnHand = quantity,
                        WeightOnHand = weight,
                        ReorderPoint = 10, // Default reorder point
                        MinimumStockLevel = 5, // Default minimum stock
                        MaximumStockLevel = 1000, // Default maximum stock
                        CreatedBy = userId
                    };
                    
                    await _unitOfWork.Inventory.AddAsync(inventory);
                    await _unitOfWork.SaveChangesAsync(); // Save to get the ID
                }
                else
                {
                    // Update existing inventory
                    inventory.QuantityOnHand += quantity;
                    inventory.WeightOnHand += weight;
                    _unitOfWork.Inventory.Update(inventory);
                }

                // Create inventory movement record
                var movement = new InventoryMovement
                {
                    InventoryId = inventory.Id,
                    MovementType = movementType,
                    QuantityChange = quantity,
                    WeightChange = weight,
                    QuantityBalance = inventory.QuantityOnHand,
                    WeightBalance = inventory.WeightOnHand,
                    MovementDate = DateTime.UtcNow,
                    ReferenceNumber = referenceNumber ?? $"ADD-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    UnitCost = unitCost,
                    Notes = notes ?? $"Inventory added by user {userId}",
                    CreatedBy = userId
                };

                await _unitOfWork.InventoryMovements.AddAsync(movement);

                // Log audit trail
                await _auditService.LogAsync(userId, "Add", "Inventory", inventory.Id.ToString(), 
                    $"Added {quantity} units, {weight}g via {movementType}", null, null, null, null, branchId);

                await _unitOfWork.SaveChangesAsync();
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding inventory for product {ProductId} at branch {BranchId}", productId, branchId);
            throw;
        }
    }

    /// <summary>
    /// Adjust inventory (manual adjustment)
    /// </summary>
    public async Task<bool> AdjustInventoryAsync(
        int productId,
        int branchId,
        decimal newQuantity,
        decimal newWeight,
        string reason,
        string userId)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var inventory = await _unitOfWork.Inventory.GetByProductAndBranchAsync(productId, branchId);
                if (inventory == null)
                {
                    _logger.LogError("Inventory not found for product {ProductId} at branch {BranchId}", productId, branchId);
                    return false;
                }

                var quantityChange = newQuantity - inventory.QuantityOnHand;
                var weightChange = newWeight - inventory.WeightOnHand;

                // Update inventory
                inventory.QuantityOnHand = newQuantity;
                inventory.WeightOnHand = newWeight;
                _unitOfWork.Inventory.Update(inventory);

                // Create inventory movement record
                var movement = new InventoryMovement
                {
                    InventoryId = inventory.Id,
                    MovementType = "Adjustment",
                    QuantityChange = quantityChange,
                    WeightChange = weightChange,
                    QuantityBalance = inventory.QuantityOnHand,
                    WeightBalance = inventory.WeightOnHand,
                    MovementDate = DateTime.UtcNow,
                    ReferenceNumber = $"ADJ-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    Notes = $"Manual adjustment: {reason}",
                    CreatedBy = userId
                };

                await _unitOfWork.InventoryMovements.AddAsync(movement);

                // Log audit trail
                await _auditService.LogAsync(userId, "Adjust", "Inventory", inventory.Id.ToString(), 
                    $"Adjusted from {inventory.QuantityOnHand - quantityChange} to {newQuantity} units. Reason: {reason}", null, null, null, null, branchId);

                await _unitOfWork.SaveChangesAsync();
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting inventory for product {ProductId} at branch {BranchId}", productId, branchId);
            throw;
        }
    }

    /// <summary>
    /// Transfer inventory between branches
    /// </summary>
    public async Task<bool> TransferInventoryAsync(
        int productId,
        int fromBranchId,
        int toBranchId,
        decimal quantity,
        decimal weight,
        string transferNumber,
        string userId,
        string? notes = null)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // Get source inventory
                var sourceInventory = await _unitOfWork.Inventory.GetByProductAndBranchAsync(productId, fromBranchId);
                if (sourceInventory == null || sourceInventory.QuantityOnHand < quantity)
                {
                    _logger.LogError("Insufficient inventory for transfer. Product: {ProductId}, From Branch: {FromBranchId}", 
                        productId, fromBranchId);
                    return false;
                }

                // Update source inventory
                sourceInventory.QuantityOnHand -= quantity;
                sourceInventory.WeightOnHand -= weight;
                _unitOfWork.Inventory.Update(sourceInventory);

                // Get or create destination inventory
                var destInventory = await _unitOfWork.Inventory.GetByProductAndBranchAsync(productId, toBranchId);
                if (destInventory == null)
                {
                    destInventory = new Inventory
                    {
                        ProductId = productId,
                        BranchId = toBranchId,
                        QuantityOnHand = quantity,
                        WeightOnHand = weight,
                        ReorderPoint = 10,
                        MinimumStockLevel = 5,
                        MaximumStockLevel = 1000,
                        CreatedBy = userId
                    };
                    await _unitOfWork.Inventory.AddAsync(destInventory);
                }
                else
                {
                    destInventory.QuantityOnHand += quantity;
                    destInventory.WeightOnHand += weight;
                    _unitOfWork.Inventory.Update(destInventory);
                }

                await _unitOfWork.SaveChangesAsync();

                // Create movement records for both inventories
                var sourceMovement = new InventoryMovement
                {
                    InventoryId = sourceInventory.Id,
                    MovementType = "Transfer Out",
                    QuantityChange = -quantity,
                    WeightChange = -weight,
                    QuantityBalance = sourceInventory.QuantityOnHand,
                    WeightBalance = sourceInventory.WeightOnHand,
                    MovementDate = DateTime.UtcNow,
                    ReferenceNumber = transferNumber,
                    Notes = notes ?? $"Transfer to branch {toBranchId}",
                    CreatedBy = userId
                };

                var destMovement = new InventoryMovement
                {
                    InventoryId = destInventory.Id,
                    MovementType = "Transfer In",
                    QuantityChange = quantity,
                    WeightChange = weight,
                    QuantityBalance = destInventory.QuantityOnHand,
                    WeightBalance = destInventory.WeightOnHand,
                    MovementDate = DateTime.UtcNow,
                    ReferenceNumber = transferNumber,
                    Notes = notes ?? $"Transfer from branch {fromBranchId}",
                    CreatedBy = userId
                };

                await _unitOfWork.InventoryMovements.AddAsync(sourceMovement);
                await _unitOfWork.InventoryMovements.AddAsync(destMovement);

                // Log audit trail
                await _auditService.LogAsync(userId, "Transfer Out", "Inventory", sourceInventory.Id.ToString(), 
                    $"Transferred {quantity} units to branch {toBranchId}", null, null, null, null, fromBranchId);
                await _auditService.LogAsync(userId, "Transfer In", "Inventory", destInventory.Id.ToString(), 
                    $"Received {quantity} units from branch {fromBranchId}", null, null, null, null, toBranchId);

                await _unitOfWork.SaveChangesAsync();
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring inventory for product {ProductId} from branch {FromBranchId} to {ToBranchId}", 
                productId, fromBranchId, toBranchId);
            throw;
        }
    }

    /// <summary>
    /// Get inventory movement history
    /// </summary>
    public async Task<(List<InventoryMovement> Movements, int TotalCount)> GetInventoryMovementsAsync(
        int? productId = null,
        int? branchId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? movementType = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        try
        {
            var query = _unitOfWork.Inventory.GetQueryable()
                .OfType<InventoryMovement>()
                .AsQueryable();

            if (productId.HasValue && branchId.HasValue)
            { 
                var inventory = await _unitOfWork.Inventory.GetByProductAndBranchAsync(productId.Value, branchId.Value);
                if (inventory != null)
                {
                    return (await _unitOfWork.Inventory.GetMovementsAsync(inventory.Id, fromDate, toDate), 
                           await _unitOfWork.Inventory.GetMovementsAsync(inventory.Id, fromDate, toDate).ConfigureAwait(false) is var movements ? movements.Count : 0);
                }
            }

            // For now, return empty results - in a real implementation, you'd need a proper InventoryMovement repository
            return (new List<InventoryMovement>(), 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory movements");
            throw;
        }
    }
}