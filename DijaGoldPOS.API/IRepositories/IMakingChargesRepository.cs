using DijaGoldPOS.API.Models.ProductModels;

namespace DijaGoldPOS.API.IRepositories;

public interface IMakingChargesRepository : IRepository<MakingCharges>
{
    Task<MakingCharges?> GetCurrentByProductCategoryAsync(int productCategoryId, int? subCategoryId = null);
    Task<IEnumerable<MakingCharges>> GetByProductCategoryAsync(int productCategoryId);
    Task<IEnumerable<MakingCharges>> GetActiveAsync();
}
