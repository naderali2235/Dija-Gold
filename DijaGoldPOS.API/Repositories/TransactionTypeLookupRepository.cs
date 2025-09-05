using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Models.LookupModels;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

public class TransactionTypeLookupRepository : Repository<TransactionTypeLookup>, ITransactionTypeLookupRepository
{
    public TransactionTypeLookupRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<TransactionTypeLookup?> GetByNameAsync(string name)
    {
        return await _context.TransactionTypeLookups
            .FirstOrDefaultAsync(t => t.Name == name && t.IsActive);
    }

    public async Task<IEnumerable<TransactionTypeLookup>> GetActiveAsync()
    {
        return await _context.TransactionTypeLookups
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }
}
