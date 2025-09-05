using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Models.ProductModels;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

public class TaxConfigurationRepository : Repository<TaxConfiguration>, ITaxConfigurationRepository
{
    public TaxConfigurationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TaxConfiguration>> GetCurrentAsync()
    {
        return await _context.TaxConfigurations
            .Include(tc => tc.TaxType)
            .Include(tc => tc.ProductCategory)
            .Where(tc => tc.IsCurrent && tc.IsActive)
            .OrderBy(tc => tc.DisplayOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaxConfiguration>> GetByProductCategoryAsync(int? productCategoryId)
    {
        return await _context.TaxConfigurations
            .Include(tc => tc.TaxType)
            .Include(tc => tc.ProductCategory)
            .Where(tc => (tc.ProductCategoryId == productCategoryId || tc.ProductCategoryId == null) && 
                        tc.IsCurrent && tc.IsActive)
            .OrderBy(tc => tc.DisplayOrder)
            .ToListAsync();
    }

    public async Task<TaxConfiguration?> GetByTaxCodeAsync(string taxCode)
    {
        return await _context.TaxConfigurations
            .Include(tc => tc.TaxType)
            .Include(tc => tc.ProductCategory)
            .FirstOrDefaultAsync(tc => tc.TaxCode == taxCode && tc.IsActive);
    }
}
