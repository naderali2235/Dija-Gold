using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Models.LookupModels;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

public class TransactionStatusLookupRepository : Repository<TransactionStatusLookup>, ITransactionStatusLookupRepository
{
    public TransactionStatusLookupRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<TransactionStatusLookup?> GetByNameAsync(string name)
    {
        return await _context.TransactionStatusLookups
            .FirstOrDefaultAsync(t => t.Name == name && t.IsActive);
    }

    public async Task<IEnumerable<TransactionStatusLookup>> GetActiveAsync()
    {
        return await _context.TransactionStatusLookups
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }
}
