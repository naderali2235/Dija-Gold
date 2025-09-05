using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Models.ProductModels;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

public class MakingChargesRepository : Repository<MakingCharges>, IMakingChargesRepository
{
    public MakingChargesRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<MakingCharges?> GetCurrentByProductCategoryAsync(int productCategoryId, int? subCategoryId = null)
    {
        return await _context.MakingCharges
            .Include(mc => mc.ProductCategory)
            .Include(mc => mc.SubCategoryLookup)
            .Include(mc => mc.ChargeType)
            .Where(mc => mc.ProductCategoryId == productCategoryId && 
                        mc.SubCategoryId == subCategoryId && 
                        mc.IsCurrent && mc.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<MakingCharges>> GetByProductCategoryAsync(int productCategoryId)
    {
        return await _context.MakingCharges
            .Include(mc => mc.ProductCategory)
            .Include(mc => mc.SubCategoryLookup)
            .Include(mc => mc.ChargeType)
            .Where(mc => mc.ProductCategoryId == productCategoryId && mc.IsActive)
            .OrderByDescending(mc => mc.EffectiveFrom)
            .ToListAsync();
    }

    public async Task<IEnumerable<MakingCharges>> GetActiveAsync()
    {
        return await _context.MakingCharges
            .Include(mc => mc.ProductCategory)
            .Include(mc => mc.SubCategoryLookup)
            .Include(mc => mc.ChargeType)
            .Where(mc => mc.IsActive)
            .OrderBy(mc => mc.ProductCategoryId)
            .ThenByDescending(mc => mc.EffectiveFrom)
            .ToListAsync();
    }
}
