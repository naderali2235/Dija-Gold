using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.IRepositories;

/// <summary>
/// Repository interface for ProductManufacture entity
/// </summary>
public interface IProductManufactureRepository : IRepository<ProductManufacture>
{
    /// <summary>
    /// Get manufacturing records for a specific product
    /// </summary>
    Task<IEnumerable<ProductManufacture>> GetByProductIdAsync(int productId);
    
    /// <summary>
    /// Get manufacturing records for a specific purchase order item
    /// </summary>
    Task<IEnumerable<ProductManufacture>> GetByPurchaseOrderItemIdAsync(int purchaseOrderItemId);
    
    /// <summary>
    /// Get manufacturing records for a specific purchase order
    /// </summary>
    Task<IEnumerable<ProductManufacture>> GetByPurchaseOrderIdAsync(int purchaseOrderId);
    
    /// <summary>
    /// Get manufacturing summary by purchase order
    /// </summary>
    Task<ManufacturingSummaryByPurchaseOrderDto?> GetManufacturingSummaryByPurchaseOrderAsync(int purchaseOrderId);
    
    /// <summary>
    /// Get manufacturing summary by product
    /// </summary>
    Task<ManufacturingSummaryByProductDto?> GetManufacturingSummaryByProductAsync(int productId);
    
    /// <summary>
    /// Get manufacturing records by batch number
    /// </summary>
    Task<IEnumerable<ProductManufacture>> GetByBatchNumberAsync(string batchNumber);
    
    /// <summary>
    /// Get manufacturing records within a date range
    /// </summary>
    Task<IEnumerable<ProductManufacture>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Get remaining raw gold weight for a purchase order item
    /// </summary>
    Task<decimal> GetRemainingRawGoldWeightAsync(int purchaseOrderItemId);
    
    /// <summary>
    /// Check if there's sufficient raw gold available for manufacturing
    /// </summary>
    Task<bool> IsSufficientRawGoldAvailableAsync(int purchaseOrderItemId, decimal requiredWeight);

    /// <summary>
    /// Get total consumed weight by purchase order item
    /// </summary>
    Task<decimal> GetTotalConsumedWeightByPurchaseOrderItemAsync(int purchaseOrderItemId);
}
