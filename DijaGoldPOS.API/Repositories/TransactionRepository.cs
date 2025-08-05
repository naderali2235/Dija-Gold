using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository implementation for Transaction entity with specific business methods
/// </summary>
public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get transaction by transaction number
    /// </summary>
    public async Task<Transaction?> GetByTransactionNumberAsync(string transactionNumber, int? branchId = null)
    {
        var query = _dbSet
            .Include(t => t.TransactionItems)
                .ThenInclude(ti => ti.Product)
            .Include(t => t.TransactionTaxes)
                .ThenInclude(tt => tt.TaxConfiguration)
            .Include(t => t.Customer)
            .Include(t => t.Branch)
            .Include(t => t.Cashier)
            .Where(t => t.TransactionNumber == transactionNumber);

        if (branchId.HasValue)
        {
            query = query.Where(t => t.BranchId == branchId.Value);
        }

        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get transactions for a specific branch
    /// </summary>
    public async Task<List<Transaction>> GetByBranchAsync(int branchId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _dbSet
            .Include(t => t.Customer)
            .Include(t => t.Cashier)
            .Where(t => t.BranchId == branchId);

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= toDate.Value);
        }

        return await query
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get transactions for a specific customer
    /// </summary>
    public async Task<List<Transaction>> GetByCustomerAsync(int customerId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _dbSet
            .Include(t => t.Branch)
            .Include(t => t.Cashier)
            .Include(t => t.TransactionItems)
                .ThenInclude(ti => ti.Product)
            .Where(t => t.CustomerId == customerId);

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= toDate.Value);
        }

        return await query
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get transactions by cashier
    /// </summary>
    public async Task<List<Transaction>> GetByCashierAsync(string cashierId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _dbSet
            .Include(t => t.Branch)
            .Include(t => t.Customer)
            .Where(t => t.CashierId == cashierId);

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= toDate.Value);
        }

        return await query
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get transactions by type
    /// </summary>
    public async Task<List<Transaction>> GetByTypeAsync(TransactionType transactionType, int? branchId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _dbSet
            .Include(t => t.Branch)
            .Include(t => t.Customer)
            .Include(t => t.Cashier)
            .Where(t => t.TransactionType == transactionType);

        if (branchId.HasValue)
        {
            query = query.Where(t => t.BranchId == branchId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= toDate.Value);
        }

        return await query
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get daily sales summary for a branch
    /// </summary>
    public async Task<(decimal TotalSales, decimal TotalMakingCharges, decimal TotalTax, int TransactionCount)> GetDailySalesSummaryAsync(int branchId, DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var summary = await _dbSet
            .Where(t => t.BranchId == branchId && 
                       t.TransactionDate >= startOfDay && 
                       t.TransactionDate < endOfDay &&
                       t.TransactionType == TransactionType.Sale)
            .GroupBy(t => 1)
            .Select(g => new
            {
                TotalSales = g.Sum(t => t.TotalAmount),
                TotalMakingCharges = g.Sum(t => t.TotalMakingCharges),
                TotalTax = g.Sum(t => t.TotalTaxAmount),
                TransactionCount = g.Count()
            })
            .FirstOrDefaultAsync();

        return summary != null 
            ? (summary.TotalSales, summary.TotalMakingCharges, summary.TotalTax, summary.TransactionCount)
            : (0, 0, 0, 0);
    }

    /// <summary>
    /// Get sales summary by date range
    /// </summary>
    public async Task<(decimal TotalSales, decimal TotalMakingCharges, decimal TotalTax, int TransactionCount)> GetSalesSummaryAsync(int? branchId, DateTime fromDate, DateTime toDate)
    {
        var query = _dbSet
            .Where(t => t.TransactionDate >= fromDate && 
                       t.TransactionDate <= toDate &&
                       t.TransactionType == TransactionType.Sale);

        if (branchId.HasValue)
        {
            query = query.Where(t => t.BranchId == branchId.Value);
        }

        var summary = await query
            .GroupBy(t => 1)
            .Select(g => new
            {
                TotalSales = g.Sum(t => t.TotalAmount),
                TotalMakingCharges = g.Sum(t => t.TotalMakingCharges),
                TotalTax = g.Sum(t => t.TotalTaxAmount),
                TransactionCount = g.Count()
            })
            .FirstOrDefaultAsync();

        return summary != null 
            ? (summary.TotalSales, summary.TotalMakingCharges, summary.TotalTax, summary.TransactionCount)
            : (0, 0, 0, 0);
    }

    /// <summary>
    /// Get return transactions that can be processed (within return period)
    /// </summary>
    public async Task<List<Transaction>> GetEligibleReturnTransactionsAsync(int branchId, int returnPeriodDays = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-returnPeriodDays);

        return await _dbSet
            .Include(t => t.Customer)
            .Include(t => t.TransactionItems)
                .ThenInclude(ti => ti.Product)
            .Where(t => t.BranchId == branchId &&
                       t.TransactionType == TransactionType.Sale &&
                       t.TransactionDate >= cutoffDate &&
                       !t.ReturnTransactions.Any()) // Not already returned
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get top customers by purchase amount
    /// </summary>
    public async Task<List<(Customer Customer, decimal TotalPurchases, int TransactionCount)>> GetTopCustomersAsync(int? branchId, DateTime fromDate, DateTime toDate, int topCount = 10)
    {
        var query = _dbSet
            .Include(t => t.Customer)
            .Where(t => t.CustomerId.HasValue &&
                       t.TransactionDate >= fromDate &&
                       t.TransactionDate <= toDate &&
                       t.TransactionType == TransactionType.Sale);

        if (branchId.HasValue)
        {
            query = query.Where(t => t.BranchId == branchId.Value);
        }

        var results = await query
            .GroupBy(t => t.Customer)
            .Select(g => new
            {
                Customer = g.Key,
                TotalPurchases = g.Sum(t => t.TotalAmount),
                TransactionCount = g.Count()
            })
            .OrderByDescending(x => x.TotalPurchases)
            .Take(topCount)
            .ToListAsync();

        return results.Select(r => (r.Customer!, r.TotalPurchases, r.TransactionCount)).ToList();
    }

    /// <summary>
    /// Get next transaction number for a branch
    /// </summary>
    public async Task<string> GetNextTransactionNumberAsync(int branchId, TransactionType transactionType)
    {
        var today = DateTime.Today;
        var branchCode = await _context.Branches
            .Where(b => b.Id == branchId)
            .Select(b => b.Code)
            .FirstOrDefaultAsync() ?? "001";

        var typePrefix = transactionType switch
        {
            TransactionType.Sale => "SAL",
            TransactionType.Return => "RET",
            TransactionType.Repair => "REP",
            _ => "TRN"
        };

        var datePrefix = today.ToString("yyyyMMdd");
        var lastNumber = await _dbSet
            .Where(t => t.BranchId == branchId && 
                       t.TransactionType == transactionType &&
                       t.TransactionDate.Date == today)
            .CountAsync();

        return $"{branchCode}-{typePrefix}-{datePrefix}-{(lastNumber + 1):0000}";
    }

    /// <summary>
    /// Check if transaction number exists
    /// </summary>
    public async Task<bool> TransactionNumberExistsAsync(string transactionNumber, int branchId)
    {
        return await _dbSet.AnyAsync(t => t.TransactionNumber == transactionNumber && t.BranchId == branchId);
    }
}