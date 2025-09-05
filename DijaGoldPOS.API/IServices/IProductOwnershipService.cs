using DijaGoldPOS.API.DTOs;

namespace DijaGoldPOS.API.IServices;

/// <summary>
/// Service interface for product ownership management
/// </summary>
public interface IProductOwnershipService
{
    /// <summary>
    /// Create or update product ownership
    /// </summary>
    Task<ProductOwnershipDto> CreateOrUpdateOwnershipAsync(ProductOwnershipRequest request, string userId);
    
    /// <summary>
    /// Validate product ownership for sales
    /// </summary>
    Task<OwnershipValidationResult> ValidateProductOwnershipAsync(int productId, int branchId, decimal requestedQuantity);
    
    /// <summary>
    /// Update ownership after payment
    /// </summary>
    Task<bool> UpdateOwnershipAfterPaymentAsync(int productOwnershipId, decimal paymentAmount, string referenceNumber, string userId);
    
    /// <summary>
    /// Update ownership after sale
    /// </summary>
    Task<bool> UpdateOwnershipAfterSaleAsync(int productId, int branchId, decimal soldQuantity, string referenceNumber, string userId);
    
    ///// <summary>
    ///// Convert raw gold to products (Scenario 2)
    ///// </summary>
    //Task<(bool Success, string Message)> ConvertRawGoldToProductsAsync(ConvertRawGoldRequest request, string userId);
    
    /// <summary>
    /// Get ownership alerts
    /// </summary>
    Task<List<OwnershipAlertDto>> GetOwnershipAlertsAsync(int? branchId = null);
    
    /// <summary>
    /// Get ownership details for a product
    /// </summary>
    Task<List<ProductOwnershipDto>> GetProductOwnershipAsync(int productId, int branchId);
    
    /// <summary>
    /// Get ownership movements
    /// </summary>
    Task<List<OwnershipMovementDto>> GetOwnershipMovementsAsync(int productOwnershipId);
    
    /// <summary>
    /// Get low ownership products
    /// </summary>
    Task<List<ProductOwnershipDto>> GetLowOwnershipProductsAsync(decimal threshold = 50m);
    
    /// <summary>
    /// Get products with outstanding payments
    /// </summary>
    Task<List<ProductOwnershipDto>> GetProductsWithOutstandingPaymentsAsync();
    
    /// <summary>
    /// Get product ownership list with pagination and filtering
    /// </summary>
    Task<(List<ProductOwnershipDto> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetProductOwnershipListAsync(
        int branchId,
        string? searchTerm = null,
        int? supplierId = null,
        int pageNumber = 1,
        int pageSize = 10);

    /// <summary>
    /// Validate product sale and check for unpaid supplier balances
    /// </summary>
    Task<(bool CanSell, string Message, List<string> Warnings)> ValidateProductSaleAsync(int productId, int branchId, decimal requestedQuantity);

    /// <summary>
    /// Get products with unpaid supplier balances (sales risk report)
    /// </summary>
    Task<List<ProductSaleRiskDto>> GetProductsWithUnpaidSupplierBalancesAsync(int? branchId = null);
}
