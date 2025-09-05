using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Models.InventoryModels;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository implementation for InventoryMovement entities
/// </summary>
public class InventoryMovementRepository : Repository<InventoryMovement>, IInventoryMovementRepository
{
    public InventoryMovementRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get inventory movements for a specific inventory item
    /// </summary>
    public async Task<List<InventoryMovement>> GetByInventoryIdAsync(int inventoryId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _dbSet
            .Include(im => im.Inventory)
                .ThenInclude(i => i.Product)
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

    /// <summary>
    /// Get inventory movements by movement type
    /// </summary>
    public async Task<List<InventoryMovement>> GetByMovementTypeAsync(string movementType, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _dbSet
            .Include(im => im.Inventory)
                .ThenInclude(i => i.Product)
            .Where(im => im.MovementType == movementType);

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

    /// <summary>
    /// Get inventory movements by reference number
    /// </summary>
    public async Task<List<InventoryMovement>> GetByReferenceNumberAsync(string referenceNumber)
    {
        return await _dbSet
            .Include(im => im.Inventory)
                .ThenInclude(i => i.Product)
            .Where(im => im.ReferenceNumber == referenceNumber)
            .OrderByDescending(im => im.MovementDate)
            .ToListAsync();
    }
}
