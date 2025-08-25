using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Models;

using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository implementation for Product entity with specific business methods
/// </summary>
public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get product by product code
    /// </summary>
    public async Task<Product?> GetByProductCodeAsync(string productCode)
    {
        return await _dbSet
            .Include(p => p.Supplier)
            .Include(p => p.InventoryRecords)
            .FirstOrDefaultAsync(p => p.ProductCode == productCode);
    }

    /// <summary>
    /// Get products by category type
    /// </summary>
    public async Task<List<Product>> GetByCategoryAsync(int categoryTypeId)
    {
        return await _dbSet
            .Include(p => p.Supplier)
            .Where(p => p.CategoryTypeId == categoryTypeId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Get products by karat type
    /// </summary>
    public async Task<List<Product>> GetByKaratTypeAsync(int karatTypeId)
    {
        return await _dbSet
            .Include(p => p.Supplier)
            .Where(p => p.KaratTypeId == karatTypeId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Get products by supplier
    /// </summary>
    public async Task<List<Product>> GetBySupplierAsync(int supplierId)
    {
        return await _dbSet
            .Include(p => p.Supplier)
            .Where(p => p.SupplierId == supplierId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Search products by name or product code
    /// </summary>
    public async Task<List<Product>> SearchAsync(string searchTerm)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        
        return await _dbSet
            .Include(p => p.Supplier)
            .Where(p => p.Name.ToLower().Contains(lowerSearchTerm) || 
                       p.ProductCode.ToLower().Contains(lowerSearchTerm) ||
                       (p.Brand != null && p.Brand.ToLower().Contains(lowerSearchTerm)))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Get products with low stock across all branches
    /// </summary>
    public async Task<List<Product>> GetLowStockProductsAsync()
    {
        return await _dbSet
            .Include(p => p.Supplier)
            .Include(p => p.InventoryRecords)
            .Where(p => p.InventoryRecords.Any(i => 
                i.QuantityOnHand <= i.ReorderPoint && 
                i.QuantityOnHand > 0))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Get products by weight range
    /// </summary>
    public async Task<List<Product>> GetByWeightRangeAsync(decimal minWeight, decimal maxWeight)
    {
        return await _dbSet
            .Include(p => p.Supplier)
            .Where(p => p.Weight >= minWeight && p.Weight <= maxWeight)
            .OrderBy(p => p.Weight)
            .ToListAsync();
    }

    /// <summary>
    /// Check if product code exists
    /// </summary>
    public async Task<bool> ProductCodeExistsAsync(string productCode, int? excludeId = null)
    {
        var query = _dbSet.Where(p => p.ProductCode == productCode);
        
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
