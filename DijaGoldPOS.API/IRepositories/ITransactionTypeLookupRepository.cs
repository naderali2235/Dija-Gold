using DijaGoldPOS.API.Models.LookupTables;

namespace DijaGoldPOS.API.IRepositories;

public interface ITransactionTypeLookupRepository : IRepository<TransactionTypeLookup>
{
    Task<TransactionTypeLookup?> GetByNameAsync(string name);
    Task<IEnumerable<TransactionTypeLookup>> GetActiveAsync();
}
