using DijaGoldPOS.API.Models.ProductModels;

namespace DijaGoldPOS.API.IRepositories;

public interface ITaxConfigurationRepository : IRepository<TaxConfiguration>
{
    Task<IEnumerable<TaxConfiguration>> GetCurrentAsync();
    Task<IEnumerable<TaxConfiguration>> GetByProductCategoryAsync(int? productCategoryId);
    Task<TaxConfiguration?> GetByTaxCodeAsync(string taxCode);
}
