

using DijaGoldPOS.API.Models.LookupModels;

namespace DijaGoldPOS.API.IRepositories;

public interface ITransactionStatusLookupRepository : IRepository<TransactionStatusLookup>
{
    Task<TransactionStatusLookup?> GetByNameAsync(string name);
    Task<IEnumerable<TransactionStatusLookup>> GetActiveAsync();
}
