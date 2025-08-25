using DijaGoldPOS.API.DTOs;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Shared;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service interface for product management operations
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Get paginated products with filtering and search
    /// </summary>
    Task<PagedResult<ProductDto>> GetProductsAsync(ProductSearchRequestDto searchRequest);

    /// <summary>
    /// Get product by ID
    /// </summary>
    Task<ProductDto?> GetProductByIdAsync(int id);

    /// <summary>
    /// Get product with inventory information
    /// </summary>
    Task<ProductWithInventoryDto?> GetProductWithInventoryAsync(int id);

    /// <summary>
    /// Create a new product
    /// </summary>
    Task<ProductDto> CreateProductAsync(CreateProductRequestDto request, string userId);

    /// <summary>
    /// Update an existing product
    /// </summary>
    Task<ProductDto> UpdateProductAsync(UpdateProductRequestDto request, string userId);

    /// <summary>
    /// Deactivate a product (soft delete)
    /// </summary>
    Task DeactivateProductAsync(int id, string userId);

    /// <summary>
    /// Get product pricing information
    /// </summary>
    Task<ProductPricingDto> GetProductPricingAsync(int id, decimal quantity = 1, int? customerId = null);

    /// <summary>
    /// Check if product code exists
    /// </summary>
    Task<bool> ProductCodeExistsAsync(string productCode, int? excludeId = null);
}
