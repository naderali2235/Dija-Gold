using DijaGoldPOS.API.Models;


namespace DijaGoldPOS.API.IRepositories;

/// <summary>
/// Repository interface for Product entity with specific business methods
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    /// <summary>
    /// Get product by product code
    /// </summary>
    /// <param name="productCode">Product code/SKU</param>
    /// <returns>Product or null if not found</returns>
    Task<Product?> GetByProductCodeAsync(string productCode);

    /// <summary>
    /// Get products by category type
    /// </summary>
    /// <param name="categoryTypeId">Product category type ID</param>
    /// <returns>List of products</returns>
    Task<List<Product>> GetByCategoryAsync(int categoryTypeId);

    /// <summary>
    /// Get products by karat type
    /// </summary>
    /// <param name="karatTypeId">Karat type ID</param>
    /// <returns>List of products</returns>
    Task<List<Product>> GetByKaratTypeAsync(int karatTypeId);

    /// <summary>
    /// Get products by supplier
    /// </summary>
    /// <param name="supplierId">Supplier ID</param>
    /// <returns>List of products</returns>
    Task<List<Product>> GetBySupplierAsync(int supplierId);

    /// <summary>
    /// Search products by name or product code
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>List of matching products</returns>
    Task<List<Product>> SearchAsync(string searchTerm);

    /// <summary>
    /// Get products with low stock across all branches
    /// </summary>
    /// <returns>List of products with low stock</returns>
    Task<List<Product>> GetLowStockProductsAsync();

    /// <summary>
    /// Get products by weight range
    /// </summary>
    /// <param name="minWeight">Minimum weight</param>
    /// <param name="maxWeight">Maximum weight</param>
    /// <returns>List of products within weight range</returns>
    Task<List<Product>> GetByWeightRangeAsync(decimal minWeight, decimal maxWeight);

    /// <summary>
    /// Check if product code exists
    /// </summary>
    /// <param name="productCode">Product code to check</param>
    /// <param name="excludeId">Product ID to exclude (for updates)</param>
    /// <returns>True if product code exists</returns>
    Task<bool> ProductCodeExistsAsync(string productCode, int? excludeId = null);
}
