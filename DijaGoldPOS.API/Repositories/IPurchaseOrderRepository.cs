using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Repositories;

public interface IPurchaseOrderRepository : IRepository<PurchaseOrder>
{
    Task<PurchaseOrder?> GetWithItemsAsync(int id);
    Task<PurchaseOrder?> GetWithItemsAsNoTrackingAsync(int id);
    Task<PurchaseOrder?> GetByNumberAsync(string purchaseOrderNumber);
}


