using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.IRepositories;

/// <summary>
/// Repository interface for ProductOwnership entity
/// </summary>
public interface IProductOwnershipRepository : IRepository<ProductOwnership>
{
    /// <summary>
    /// Get ownership records for a specific product and branch
    /// </summary>
    Task<List<ProductOwnership>> GetByProductAndBranchAsync(int productId, int branchId);
    
    /// <summary>
    /// Get ownership records for a specific supplier
    /// </summary>
    Task<List<ProductOwnership>> GetBySupplierAsync(int supplierId);
    
    /// <summary>
    /// Get ownership records for a specific purchase order
    /// </summary>
    Task<List<ProductOwnership>> GetByPurchaseOrderAsync(int purchaseOrderId);
    
    /// <summary>
    /// Get ownership records for a specific customer purchase
    /// </summary>
    Task<List<ProductOwnership>> GetByCustomerPurchaseAsync(int customerPurchaseId);
    
    /// <summary>
    /// Get ownership summary for a product across all branches
    /// </summary>
    Task<(decimal TotalOwnedQuantity, decimal TotalOwnedWeight, decimal TotalQuantity, decimal TotalWeight)> GetOwnershipSummaryAsync(int productId);
    
    /// <summary>
    /// Get products with low ownership percentage
    /// </summary>
    Task<List<ProductOwnership>> GetLowOwnershipProductsAsync(decimal threshold = 50m);
    
    /// <summary>
    /// Get products with outstanding payments
    /// </summary>
    Task<List<ProductOwnership>> GetProductsWithOutstandingPaymentsAsync();
    
    /// <summary>
    /// Get ownership records by branch
    /// </summary>
    Task<List<ProductOwnership>> GetByBranchAsync(int branchId);
    
    /// <summary>
    /// Get ownership records with related data
    /// </summary>
    Task<ProductOwnership?> GetWithDetailsAsync(int id);
    
    /// <summary>
    /// Get ownership records with pagination and filtering
    /// </summary>
    Task<(List<ProductOwnership> Items, int TotalCount)> GetWithPaginationAsync(
        int branchId,
        string? searchTerm = null,
        int? supplierId = null,
        int pageNumber = 1,
        int pageSize = 10);
}
