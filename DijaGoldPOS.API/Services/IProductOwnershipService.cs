using DijaGoldPOS.API.DTOs;

namespace DijaGoldPOS.API.Services;

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
    
    /// <summary>
    /// Convert raw gold to products (Scenario 2)
    /// </summary>
    Task<(bool Success, string Message)> ConvertRawGoldToProductsAsync(ConvertRawGoldRequest request, string userId);
    
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
    Task<List<ProductOwnershipDto>> GetLowOwnershipProductsAsync(decimal threshold = 0.5m);
    
    /// <summary>
    /// Get products with outstanding payments
    /// </summary>
    Task<List<ProductOwnershipDto>> GetProductsWithOutstandingPaymentsAsync();
}
