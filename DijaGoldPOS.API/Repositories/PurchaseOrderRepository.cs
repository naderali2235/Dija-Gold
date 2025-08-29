using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

public class PurchaseOrderRepository : Repository<PurchaseOrder>, IPurchaseOrderRepository
{
    public PurchaseOrderRepository(ApplicationDbContext context) : base(context) {}

    public async Task<PurchaseOrder?> GetWithItemsAsync(int id)
    {
        return await _context.PurchaseOrders
            .Include(po => po.PurchaseOrderItems)
                .ThenInclude(item => item.Product)
            .Include(po => po.Supplier)
            .Include(po => po.Branch)
            .FirstOrDefaultAsync(po => po.Id == id);
    }

    public async Task<PurchaseOrder?> GetWithItemsAsNoTrackingAsync(int id)
    {
        return await _context.PurchaseOrders
            .AsNoTracking()
            .Include(po => po.PurchaseOrderItems)
                .ThenInclude(item => item.Product)
            .Include(po => po.Supplier)
            .Include(po => po.Branch)
            .FirstOrDefaultAsync(po => po.Id == id);
    }

    public async Task<PurchaseOrder?> GetByNumberAsync(string purchaseOrderNumber)
    {
        return await _context.PurchaseOrders
            .FirstOrDefaultAsync(po => po.PurchaseOrderNumber == purchaseOrderNumber);
    }
}
