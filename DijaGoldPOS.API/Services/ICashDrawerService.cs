using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service interface for cash drawer operations
/// </summary>
public interface ICashDrawerService
{
    /// <summary>
    /// Open cash drawer for a branch on a specific date
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="openingBalance">Opening balance amount</param>
    /// <param name="userId">User ID who is opening the drawer</param>
    /// <param name="date">Date to open drawer for (defaults to today)</param>
    /// <param name="notes">Optional notes</param>
    /// <returns>Created cash drawer balance</returns>
    Task<CashDrawerBalance> OpenDrawerAsync(int branchId, decimal openingBalance, string userId, DateTime? date = null, string? notes = null);

    /// <summary>
    /// Close cash drawer for a branch on a specific date
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="actualClosingBalance">Actual cash count</param>
    /// <param name="userId">User ID who is closing the drawer</param>
    /// <param name="date">Date to close drawer for (defaults to today)</param>
    /// <param name="notes">Optional notes</param>
    /// <returns>Updated cash drawer balance</returns>
    Task<CashDrawerBalance> CloseDrawerAsync(int branchId, decimal actualClosingBalance, string userId, DateTime? date = null, string? notes = null);

    /// <summary>
    /// Get cash drawer balance for a specific branch and date
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="date">Balance date</param>
    /// <returns>Cash drawer balance or null if not found</returns>
    Task<CashDrawerBalance?> GetBalanceAsync(int branchId, DateTime date);

    /// <summary>
    /// Get the opening balance for a specific date (from previous day's closing)
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="date">Date to get opening balance for</param>
    /// <returns>Opening balance amount or 0 if no previous closing balance</returns>
    Task<decimal> GetOpeningBalanceAsync(int branchId, DateTime date);

    /// <summary>
    /// Calculate expected closing balance based on transactions
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="date">Date to calculate for</param>
    /// <returns>Expected closing balance</returns>
    Task<decimal> CalculateExpectedClosingBalanceAsync(int branchId, DateTime date);

    /// <summary>
    /// Check if cash drawer is open for a branch on a specific date
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="date">Date to check</param>
    /// <returns>True if drawer is open, false otherwise</returns>
    Task<bool> IsDrawerOpenAsync(int branchId, DateTime date);

    /// <summary>
    /// Get cash drawer balances for a date range
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>List of cash drawer balances</returns>
    Task<List<CashDrawerBalance>> GetBalancesByDateRangeAsync(int branchId, DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Settle shift by closing current day and setting up next day with carried forward balance
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="actualClosingBalance">Actual cash count before settlement</param>
    /// <param name="settledAmount">Amount to settle/remove from drawer</param>
    /// <param name="userId">User ID who is settling the shift</param>
    /// <param name="date">Date to settle shift for (defaults to today)</param>
    /// <param name="settlementNotes">Notes about the settlement</param>
    /// <param name="notes">General notes about the shift</param>
    /// <returns>Updated cash drawer balance for the settled day</returns>
    Task<CashDrawerBalance> SettleShiftAsync(int branchId, decimal actualClosingBalance, decimal settledAmount, string userId, DateTime? date = null, string? settlementNotes = null, string? notes = null);

    /// <summary>
    /// Refresh the expected closing balance for an open cash drawer to include recent transactions
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="date">Date to refresh balance for (defaults to today)</param>
    /// <returns>Updated cash drawer balance with refreshed expected closing balance</returns>
    Task<CashDrawerBalance> RefreshExpectedClosingBalanceAsync(int branchId, DateTime? date = null);
}
