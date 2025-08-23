using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository interface for CustomerPurchase entity
/// </summary>
public interface ICustomerPurchaseRepository : IRepository<CustomerPurchase>
{
    /// <summary>
    /// Get customer purchase by number
    /// </summary>
    Task<CustomerPurchase?> GetByNumberAsync(string purchaseNumber);
    
    /// <summary>
    /// Get customer purchase with items
    /// </summary>
    Task<CustomerPurchase?> GetWithItemsAsync(int id);
    
    /// <summary>
    /// Get customer purchases by customer
    /// </summary>
    Task<List<CustomerPurchase>> GetByCustomerAsync(int customerId);
    
    /// <summary>
    /// Get customer purchases by branch
    /// </summary>
    Task<List<CustomerPurchase>> GetByBranchAsync(int branchId);
    
    /// <summary>
    /// Get customer purchases within date range
    /// </summary>
    Task<List<CustomerPurchase>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
    
    /// <summary>
    /// Get next purchase number
    /// </summary>
    Task<string> GetNextPurchaseNumberAsync();
}
