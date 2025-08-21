using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository implementation for cash drawer balance operations
/// </summary>
public class CashDrawerBalanceRepository : Repository<CashDrawerBalance>, ICashDrawerBalanceRepository
{
    public CashDrawerBalanceRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get cash drawer balance for a specific branch and date
    /// </summary>
    public async Task<CashDrawerBalance?> GetByBranchAndDateAsync(int branchId, DateTime date)
    {
        var balanceDate = date.Date;
        return await _dbSet
            .Include(cdb => cdb.Branch)
            .FirstOrDefaultAsync(cdb => cdb.BranchId == branchId && 
                                       cdb.BalanceDate == balanceDate);
    }

    /// <summary>
    /// Get the most recent closing balance for a branch (previous day's closing)
    /// </summary>
    public async Task<CashDrawerBalance?> GetLastClosingBalanceAsync(int branchId, DateTime asOfDate)
    {
        var cutoffDate = asOfDate.Date;
        return await _dbSet
            .Include(cdb => cdb.Branch)
            .Where(cdb => cdb.BranchId == branchId && 
                         cdb.BalanceDate < cutoffDate &&
                         cdb.Status == CashDrawerStatus.Closed &&
                         cdb.ActualClosingBalance.HasValue)
            .OrderByDescending(cdb => cdb.BalanceDate)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get cash drawer balances for a date range
    /// </summary>
    public async Task<List<CashDrawerBalance>> GetByDateRangeAsync(int branchId, DateTime fromDate, DateTime toDate)
    {
        var startDate = fromDate.Date;
        var endDate = toDate.Date;
        
        return await _dbSet
            .Include(cdb => cdb.Branch)
            .Where(cdb => cdb.BranchId == branchId && 
                         cdb.BalanceDate >= startDate && 
                         cdb.BalanceDate <= endDate)
            .OrderBy(cdb => cdb.BalanceDate)
            .ToListAsync();
    }

    /// <summary>
    /// Check if a cash drawer balance exists for a specific branch and date
    /// </summary>
    public async Task<bool> ExistsAsync(int branchId, DateTime date)
    {
        var balanceDate = date.Date;
        return await _dbSet
            .AnyAsync(cdb => cdb.BranchId == branchId && cdb.BalanceDate == balanceDate);
    }

    /// <summary>
    /// Get all open cash drawer balances for a branch
    /// </summary>
    public async Task<List<CashDrawerBalance>> GetOpenBalancesAsync(int branchId)
    {
        return await _dbSet
            .Include(cdb => cdb.Branch)
            .Where(cdb => cdb.BranchId == branchId && 
                         cdb.Status == CashDrawerStatus.Open)
            .OrderByDescending(cdb => cdb.BalanceDate)
            .ToListAsync();
    }
}
