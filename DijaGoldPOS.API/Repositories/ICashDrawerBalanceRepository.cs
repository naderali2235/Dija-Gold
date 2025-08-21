using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository interface for cash drawer balance operations
/// </summary>
public interface ICashDrawerBalanceRepository : IRepository<CashDrawerBalance>
{
    /// <summary>
    /// Get cash drawer balance for a specific branch and date
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="date">Balance date</param>
    /// <returns>Cash drawer balance or null if not found</returns>
    Task<CashDrawerBalance?> GetByBranchAndDateAsync(int branchId, DateTime date);

    /// <summary>
    /// Get the most recent closing balance for a branch (previous day's closing)
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="asOfDate">Date to get balance as of</param>
    /// <returns>Most recent closing balance or null if not found</returns>
    Task<CashDrawerBalance?> GetLastClosingBalanceAsync(int branchId, DateTime asOfDate);

    /// <summary>
    /// Get cash drawer balances for a date range
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>List of cash drawer balances</returns>
    Task<List<CashDrawerBalance>> GetByDateRangeAsync(int branchId, DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Check if a cash drawer balance exists for a specific branch and date
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="date">Balance date</param>
    /// <returns>True if balance exists, false otherwise</returns>
    Task<bool> ExistsAsync(int branchId, DateTime date);

    /// <summary>
    /// Get all open cash drawer balances for a branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <returns>List of open cash drawer balances</returns>
    Task<List<CashDrawerBalance>> GetOpenBalancesAsync(int branchId);
}
