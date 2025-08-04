using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Inventory management service implementation
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InventoryService> _logger;
    private readonly IAuditService _auditService;

    public InventoryService(
        ApplicationDbContext context,
        ILogger<InventoryService> logger,
        IAuditService auditService)
    {
        _context = context;
        _logger = logger;
        _auditService = auditService;
    }

    /// <summary>
    /// Get inventory for a specific product at a branch
    /// </summary>
    public async Task<Inventory?> GetInventoryAsync(int productId, int branchId)
    {
        return await _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Branch)
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.BranchId == branchId);
    }

    /// <summary>
    /// Get all inventory for a branch
    /// </summary>
    public async Task<List<Inventory>> GetBranchInventoryAsync(int branchId, bool includeZeroStock = false)
    {
        var query = _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Branch)
            .Where(i => i.BranchId == branchId);

        if (!includeZeroStock)
        {
            query = query.Where(i => i.QuantityOnHand > 0 || i.WeightOnHand > 0);
        }

        return await query
            .OrderBy(i => i.Product.CategoryType)
            .ThenBy(i => i.Product.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Get low stock items for a branch
    /// </summary>
    public async Task<List<Inventory>> GetLowStockItemsAsync(int branchId)
    {
        return await _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Branch)
            .Where(i => i.BranchId == branchId && 
                       (i.QuantityOnHand <= i.ReorderPoint || i.WeightOnHand <= i.ReorderPoint))
            .OrderBy(i => i.Product.CategoryType)
            .ThenBy(i => i.Product.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Check if sufficient stock is available for sale
    /// </summary>
    public async Task<bool> CheckStockAvailabilityAsync(int productId, int branchId, decimal requestedQuantity)
    {
        var inventory = await GetInventoryAsync(productId, branchId);
        if (inventory == null)
            return false;

        return inventory.QuantityOnHand >= requestedQuantity;
    }

    /// <summary>
    /// Reserve inventory for a transaction (decrease stock)
    /// </summary>
    public async Task<bool> ReserveInventoryAsync(List<TransactionItem> transactionItems, int branchId, string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in transactionItems)
            {
                var inventory = await GetInventoryAsync(item.ProductId, branchId);
                if (inventory == null)
                {
                    _logger.LogError("Inventory not found for product {ProductId} at branch {BranchId}", 
                        item.ProductId, branchId);
                    return false;
                }

                // Check availability
                if (inventory.QuantityOnHand < item.Quantity)
                {
                    _logger.LogError("Insufficient inventory for product {ProductId}. Available: {Available}, Requested: {Requested}",
                        item.ProductId, inventory.QuantityOnHand, item.Quantity);
                    return false;
                }

                // Update inventory
                var oldQuantity = inventory.QuantityOnHand;
                var oldWeight = inventory.WeightOnHand;

                inventory.QuantityOnHand -= item.Quantity;
                inventory.WeightOnHand -= item.TotalWeight;
                inventory.ModifiedBy = userId;
                inventory.ModifiedAt = DateTime.UtcNow;

                // Create inventory movement record
                var movement = new InventoryMovement
                {
                    InventoryId = inventory.Id,
                    MovementType = "Sale",
                    ReferenceNumber = item.Transaction?.TransactionNumber,
                    QuantityChange = -item.Quantity,
                    WeightChange = -item.TotalWeight,
                    QuantityBalance = inventory.QuantityOnHand,
                    WeightBalance = inventory.WeightOnHand,
                    Notes = $"Sale transaction - {item.Product?.Name}",
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.InventoryMovements.AddAsync(movement);

                // Log audit trail
                await _auditService.LogAsync(
                    userId,
                    "INVENTORY_RESERVED",
                    "Inventory",
                    inventory.Id.ToString(),
                    $"Reserved {item.Quantity} units of {item.Product?.Name}",
                    oldValues: $"Qty: {oldQuantity}, Weight: {oldWeight}",
                    newValues: $"Qty: {inventory.QuantityOnHand}, Weight: {inventory.WeightOnHand}",
                    branchId: branchId
                );
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Inventory reserved successfully for {ItemCount} items by user {UserId}", 
                transactionItems.Count, userId);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error reserving inventory for user {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Release reserved inventory (return to stock)
    /// </summary>
    public async Task<bool> ReleaseInventoryAsync(List<TransactionItem> transactionItems, int branchId, string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in transactionItems)
            {
                var inventory = await GetInventoryAsync(item.ProductId, branchId);
                if (inventory == null)
                {
                    _logger.LogError("Inventory not found for product {ProductId} at branch {BranchId}", 
                        item.ProductId, branchId);
                    return false;
                }

                // Update inventory
                var oldQuantity = inventory.QuantityOnHand;
                var oldWeight = inventory.WeightOnHand;

                inventory.QuantityOnHand += item.Quantity;
                inventory.WeightOnHand += item.TotalWeight;
                inventory.ModifiedBy = userId;
                inventory.ModifiedAt = DateTime.UtcNow;

                // Create inventory movement record
                var movement = new InventoryMovement
                {
                    InventoryId = inventory.Id,
                    MovementType = "Return",
                    ReferenceNumber = item.Transaction?.TransactionNumber,
                    QuantityChange = item.Quantity,
                    WeightChange = item.TotalWeight,
                    QuantityBalance = inventory.QuantityOnHand,
                    WeightBalance = inventory.WeightOnHand,
                    Notes = $"Return transaction - {item.Product?.Name}",
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.InventoryMovements.AddAsync(movement);

                // Log audit trail
                await _auditService.LogAsync(
                    userId,
                    "INVENTORY_RELEASED",
                    "Inventory",
                    inventory.Id.ToString(),
                    $"Released {item.Quantity} units of {item.Product?.Name}",
                    oldValues: $"Qty: {oldQuantity}, Weight: {oldWeight}",
                    newValues: $"Qty: {inventory.QuantityOnHand}, Weight: {inventory.WeightOnHand}",
                    branchId: branchId
                );
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Inventory released successfully for {ItemCount} items by user {UserId}", 
                transactionItems.Count, userId);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error releasing inventory for user {UserId}", userId);
            return false;
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
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var inventory = await GetInventoryAsync(productId, branchId);
            if (inventory == null)
            {
                // Create new inventory record
                inventory = new Inventory
                {
                    ProductId = productId,
                    BranchId = branchId,
                    QuantityOnHand = quantity,
                    WeightOnHand = weight,
                    LastCountDate = DateTime.UtcNow,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Inventories.AddAsync(inventory);
                await _context.SaveChangesAsync(); // Save to get the ID
            }
            else
            {
                // Update existing inventory
                inventory.QuantityOnHand += quantity;
                inventory.WeightOnHand += weight;
                inventory.LastCountDate = DateTime.UtcNow;
                inventory.ModifiedBy = userId;
                inventory.ModifiedAt = DateTime.UtcNow;
            }

            // Create inventory movement record
            var movement = new InventoryMovement
            {
                InventoryId = inventory.Id,
                MovementType = movementType,
                ReferenceNumber = referenceNumber,
                QuantityChange = quantity,
                WeightChange = weight,
                QuantityBalance = inventory.QuantityOnHand,
                WeightBalance = inventory.WeightOnHand,
                UnitCost = unitCost,
                Notes = notes,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.InventoryMovements.AddAsync(movement);

            // Log audit trail
            await _auditService.LogAsync(
                userId,
                "INVENTORY_ADDED",
                "Inventory",
                inventory.Id.ToString(),
                $"Added {quantity} units ({movementType})",
                newValues: $"Qty: {inventory.QuantityOnHand}, Weight: {inventory.WeightOnHand}",
                branchId: branchId
            );

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Inventory added successfully for product {ProductId} by user {UserId}", 
                productId, userId);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error adding inventory for product {ProductId} by user {UserId}", 
                productId, userId);
            return false;
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
        var inventory = await GetInventoryAsync(productId, branchId);
        if (inventory == null)
        {
            _logger.LogError("Inventory not found for adjustment - Product {ProductId} at Branch {BranchId}", 
                productId, branchId);
            return false;
        }

        var oldQuantity = inventory.QuantityOnHand;
        var oldWeight = inventory.WeightOnHand;
        var quantityChange = newQuantity - oldQuantity;
        var weightChange = newWeight - oldWeight;

        inventory.QuantityOnHand = newQuantity;
        inventory.WeightOnHand = newWeight;
        inventory.LastCountDate = DateTime.UtcNow;
        inventory.ModifiedBy = userId;
        inventory.ModifiedAt = DateTime.UtcNow;

        // Create inventory movement record
        var movement = new InventoryMovement
        {
            InventoryId = inventory.Id,
            MovementType = "Adjustment",
            QuantityChange = quantityChange,
            WeightChange = weightChange,
            QuantityBalance = newQuantity,
            WeightBalance = newWeight,
            Notes = $"Manual adjustment: {reason}",
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _context.InventoryMovements.AddAsync(movement);

        // Log audit trail
        await _auditService.LogAsync(
            userId,
            "INVENTORY_ADJUSTED",
            "Inventory",
            inventory.Id.ToString(),
            $"Inventory adjusted: {reason}",
            oldValues: $"Qty: {oldQuantity}, Weight: {oldWeight}",
            newValues: $"Qty: {newQuantity}, Weight: {newWeight}",
            branchId: branchId
        );

        await _context.SaveChangesAsync();

        _logger.LogInformation("Inventory adjusted for product {ProductId} by user {UserId}", 
            productId, userId);
        return true;
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
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Get source inventory
            var fromInventory = await GetInventoryAsync(productId, fromBranchId);
            if (fromInventory == null || fromInventory.QuantityOnHand < quantity)
            {
                _logger.LogError("Insufficient inventory for transfer from branch {FromBranchId}", fromBranchId);
                return false;
            }

            // Update source inventory
            fromInventory.QuantityOnHand -= quantity;
            fromInventory.WeightOnHand -= weight;
            fromInventory.ModifiedBy = userId;
            fromInventory.ModifiedAt = DateTime.UtcNow;

            // Create outbound movement
            var outboundMovement = new InventoryMovement
            {
                InventoryId = fromInventory.Id,
                MovementType = "Transfer Out",
                ReferenceNumber = transferNumber,
                QuantityChange = -quantity,
                WeightChange = -weight,
                QuantityBalance = fromInventory.QuantityOnHand,
                WeightBalance = fromInventory.WeightOnHand,
                Notes = $"Transfer to Branch {toBranchId}: {notes}",
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.InventoryMovements.AddAsync(outboundMovement);

            // Get or create destination inventory
            var toInventory = await GetInventoryAsync(productId, toBranchId);
            if (toInventory == null)
            {
                toInventory = new Inventory
                {
                    ProductId = productId,
                    BranchId = toBranchId,
                    QuantityOnHand = quantity,
                    WeightOnHand = weight,
                    LastCountDate = DateTime.UtcNow,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Inventories.AddAsync(toInventory);
                await _context.SaveChangesAsync(); // Save to get ID
            }
            else
            {
                toInventory.QuantityOnHand += quantity;
                toInventory.WeightOnHand += weight;
                toInventory.LastCountDate = DateTime.UtcNow;
                toInventory.ModifiedBy = userId;
                toInventory.ModifiedAt = DateTime.UtcNow;
            }

            // Create inbound movement
            var inboundMovement = new InventoryMovement
            {
                InventoryId = toInventory.Id,
                MovementType = "Transfer In",
                ReferenceNumber = transferNumber,
                QuantityChange = quantity,
                WeightChange = weight,
                QuantityBalance = toInventory.QuantityOnHand,
                WeightBalance = toInventory.WeightOnHand,
                Notes = $"Transfer from Branch {fromBranchId}: {notes}",
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.InventoryMovements.AddAsync(inboundMovement);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Inventory transferred successfully from branch {FromBranchId} to {ToBranchId} by user {UserId}", 
                fromBranchId, toBranchId, userId);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error transferring inventory from branch {FromBranchId} to {ToBranchId} by user {UserId}", 
                fromBranchId, toBranchId, userId);
            return false;
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
        var query = _context.InventoryMovements
            .Include(im => im.Inventory)
            .ThenInclude(i => i.Product)
            .Include(im => im.Inventory)
            .ThenInclude(i => i.Branch)
            .AsQueryable();

        if (productId.HasValue)
            query = query.Where(im => im.Inventory.ProductId == productId.Value);

        if (branchId.HasValue)
            query = query.Where(im => im.Inventory.BranchId == branchId.Value);

        if (fromDate.HasValue)
            query = query.Where(im => im.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(im => im.CreatedAt <= toDate.Value);

        if (!string.IsNullOrEmpty(movementType))
            query = query.Where(im => im.MovementType == movementType);

        var totalCount = await query.CountAsync();

        var movements = await query
            .OrderByDescending(im => im.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (movements, totalCount);
    }
}