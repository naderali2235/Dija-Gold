using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.Models;

using DijaGoldPOS.API.Repositories;
using DijaGoldPOS.API.Shared;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service implementation for cash drawer operations
/// </summary>
public class CashDrawerService : ICashDrawerService
{
    private readonly ICashDrawerBalanceRepository _cashDrawerBalanceRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CashDrawerService> _logger;

    public CashDrawerService(
        ICashDrawerBalanceRepository cashDrawerBalanceRepository,
        ApplicationDbContext context,
        ILogger<CashDrawerService> logger)
    {
        _cashDrawerBalanceRepository = cashDrawerBalanceRepository;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Open cash drawer for a branch on a specific date
    /// </summary>
    public async Task<CashDrawerBalance> OpenDrawerAsync(int branchId, decimal openingBalance, string userId, DateTime? date = null, string? notes = null)
    {
        var balanceDate = (date ?? DateTime.Today).Date;

        // Check if drawer is already open for this date
        var existingBalance = await _cashDrawerBalanceRepository.GetByBranchAndDateAsync(branchId, balanceDate);
        if (existingBalance != null)
        {
            throw new InvalidOperationException($"Cash drawer is already open for branch {branchId} on {balanceDate:yyyy-MM-dd}");
        }

        // Verify branch exists
        var branch = await _context.Branches.FindAsync(branchId);
        if (branch == null)
        {
            throw new ArgumentException($"Branch {branchId} not found");
        }

        // Calculate expected closing balance based on transactions
        var expectedClosingBalance = await CalculateExpectedClosingBalanceAsync(branchId, balanceDate);

        var cashDrawerBalance = new CashDrawerBalance
        {
            BranchId = branchId,
            BalanceDate = balanceDate,
            OpeningBalance = openingBalance,
            ExpectedClosingBalance = expectedClosingBalance,
            OpenedByUserId = userId,
            OpenedAt = DateTime.UtcNow,
            Notes = notes,
            Status = CashDrawerStatus.Open
        };

        await _cashDrawerBalanceRepository.AddAsync(cashDrawerBalance);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cash drawer opened for branch {BranchId} on {Date} with opening balance {OpeningBalance}", 
            branchId, balanceDate, openingBalance);

        return cashDrawerBalance;
    }

    /// <summary>
    /// Close cash drawer for a branch on a specific date
    /// </summary>
    public async Task<CashDrawerBalance> CloseDrawerAsync(int branchId, decimal actualClosingBalance, string userId, DateTime? date = null, string? notes = null)
    {
        var balanceDate = (date ?? DateTime.Today).Date;

        // Get existing balance
        var existingBalance = await _cashDrawerBalanceRepository.GetByBranchAndDateAsync(branchId, balanceDate);
        if (existingBalance == null)
        {
            throw new InvalidOperationException($"No cash drawer balance found for branch {branchId} on {balanceDate:yyyy-MM-dd}");
        }

        if (existingBalance.Status == CashDrawerStatus.Closed)
        {
            throw new InvalidOperationException($"Cash drawer is already closed for branch {branchId} on {balanceDate:yyyy-MM-dd}");
        }

        // Recalculate expected closing balance
        var expectedClosingBalance = await CalculateExpectedClosingBalanceAsync(branchId, balanceDate);

        // Update the balance
        existingBalance.ActualClosingBalance = actualClosingBalance;
        existingBalance.ExpectedClosingBalance = expectedClosingBalance;
        existingBalance.ClosedByUserId = userId;
        existingBalance.ClosedAt = DateTime.UtcNow;
        existingBalance.Status = CashDrawerStatus.Closed;
        
        if (!string.IsNullOrEmpty(notes))
        {
            existingBalance.Notes = string.IsNullOrEmpty(existingBalance.Notes) ? notes : $"{existingBalance.Notes}; {notes}";
        }

        _cashDrawerBalanceRepository.Update(existingBalance);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cash drawer closed for branch {BranchId} on {Date}. Expected: {Expected}, Actual: {Actual}, Over/Short: {OverShort}", 
            branchId, balanceDate, expectedClosingBalance, actualClosingBalance, existingBalance.CashOverShort);

        return existingBalance;
    }

    /// <summary>
    /// Get cash drawer balance for a specific branch and date
    /// </summary>
    public async Task<CashDrawerBalance?> GetBalanceAsync(int branchId, DateTime date)
    {
        return await _cashDrawerBalanceRepository.GetByBranchAndDateAsync(branchId, date);
    }

    /// <summary>
    /// Get the opening balance for a specific date (from previous day's closing)
    /// </summary>
    public async Task<decimal> GetOpeningBalanceAsync(int branchId, DateTime date)
    {
        var previousDayBalance = await _cashDrawerBalanceRepository.GetLastClosingBalanceAsync(branchId, date);
        return previousDayBalance?.ActualClosingBalance ?? 0m;
    }

    /// <summary>
    /// Calculate expected closing balance based on transactions
    /// </summary>
    public async Task<decimal> CalculateExpectedClosingBalanceAsync(int branchId, DateTime date)
    {
        var startDate = date.Date;
        var endDate = startDate.AddDays(1);

        // Get opening balance
        var openingBalance = await GetOpeningBalanceAsync(branchId, date);

        // Get cash transactions for the day - using FinancialTransactions
        var cashTransactions = await _context.FinancialTransactions
            .Where(ft => ft.BranchId == branchId && 
                       ft.TransactionDate >= startDate && 
                       ft.TransactionDate < endDate &&
                       ft.PaymentMethodId == LookupTableConstants.PaymentMethodCash &&
                       ft.StatusId == LookupTableConstants.FinancialTransactionStatusCompleted)
            .ToListAsync();

        var cashSales = cashTransactions
            .Where(ft => ft.TransactionTypeId == LookupTableConstants.FinancialTransactionTypeSale)
            .Sum(ft => ft.AmountPaid);

        var cashReturns = cashTransactions
            .Where(ft => ft.TransactionTypeId == LookupTableConstants.FinancialTransactionTypeReturn)
            .Sum(ft => Math.Abs(ft.ChangeGiven)); // Returns have negative change (refunds)

        var cashRepairs = cashTransactions
            .Where(ft => ft.TransactionTypeId == LookupTableConstants.FinancialTransactionTypeRepair)
            .Sum(ft => ft.AmountPaid);

        return openingBalance + cashSales + cashRepairs - cashReturns;
    }

    /// <summary>
    /// Check if cash drawer is open for a branch on a specific date
    /// </summary>
    public async Task<bool> IsDrawerOpenAsync(int branchId, DateTime date)
    {
        var balance = await _cashDrawerBalanceRepository.GetByBranchAndDateAsync(branchId, date);
        return balance?.Status == CashDrawerStatus.Open;
    }

    /// <summary>
    /// Get cash drawer balances for a date range
    /// </summary>
    public async Task<List<CashDrawerBalance>> GetBalancesByDateRangeAsync(int branchId, DateTime fromDate, DateTime toDate)
    {
        return await _cashDrawerBalanceRepository.GetByDateRangeAsync(branchId, fromDate, toDate);
    }

    /// <summary>
    /// Settle shift by closing current day and setting up next day with carried forward balance
    /// </summary>
    public async Task<CashDrawerBalance> SettleShiftAsync(int branchId, decimal actualClosingBalance, decimal settledAmount, string userId, DateTime? date = null, string? settlementNotes = null, string? notes = null)
    {
        var balanceDate = (date ?? DateTime.Today).Date;
        var nextDay = balanceDate.AddDays(1);

        // Calculate expected closing balance first for validation
        var expectedClosingBalance = await CalculateExpectedClosingBalanceAsync(branchId, balanceDate);
        
        // Validate inputs
        if (settledAmount < 0)
        {
            throw new ArgumentException("Settled amount cannot be negative");
        }

        // Settlement amount must equal expected closing balance
        if (settledAmount != expectedClosingBalance)
        {
            throw new ArgumentException($"Settlement amount must be exactly {expectedClosingBalance:F2} (Expected Closing Balance)");
        }

        if (actualClosingBalance < settledAmount)
        {
            throw new ArgumentException("Actual closing balance must be at least equal to the settlement amount");
        }

        // Get existing balance for current day
        var existingBalance = await _cashDrawerBalanceRepository.GetByBranchAndDateAsync(branchId, balanceDate);
        if (existingBalance == null)
        {
            throw new InvalidOperationException($"No cash drawer balance found for branch {branchId} on {balanceDate:yyyy-MM-dd}");
        }

        if (existingBalance.Status == CashDrawerStatus.Closed)
        {
            throw new InvalidOperationException($"Cash drawer is already closed for branch {branchId} on {balanceDate:yyyy-MM-dd}");
        }

        // Check if next day drawer already exists
        var existingNextDayBalance = await _cashDrawerBalanceRepository.GetByBranchAndDateAsync(branchId, nextDay);
        if (existingNextDayBalance != null)
        {
            throw new InvalidOperationException($"Cash drawer for next day ({nextDay:yyyy-MM-dd}) already exists. Cannot carry forward balance.");
        }

        // Verify branch exists
        var branch = await _context.Branches.FindAsync(branchId);
        if (branch == null)
        {
            throw new ArgumentException($"Branch {branchId} not found");
        }

        // Calculate carried forward amount
        var carriedForwardAmount = actualClosingBalance - settledAmount;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Update current day's balance with settlement info
            existingBalance.ActualClosingBalance = actualClosingBalance;
            existingBalance.ExpectedClosingBalance = expectedClosingBalance;
            existingBalance.SettledAmount = settledAmount;
            existingBalance.CarriedForwardAmount = carriedForwardAmount;
            existingBalance.ClosedByUserId = userId;
            existingBalance.ClosedAt = DateTime.UtcNow;
            existingBalance.Status = CashDrawerStatus.Closed;
            existingBalance.SettlementNotes = settlementNotes;
            
            if (!string.IsNullOrEmpty(notes))
            {
                existingBalance.Notes = string.IsNullOrEmpty(existingBalance.Notes) ? notes : $"{existingBalance.Notes}; {notes}";
            }

            _cashDrawerBalanceRepository.Update(existingBalance);

            // Create next day's balance with carried forward amount as opening balance
            if (carriedForwardAmount > 0)
            {
                var nextDayExpectedClosingBalance = await CalculateExpectedClosingBalanceForDateAsync(branchId, nextDay, carriedForwardAmount);
                
                var nextDayBalance = new CashDrawerBalance
                {
                    BranchId = branchId,
                    BalanceDate = nextDay,
                    OpeningBalance = carriedForwardAmount,
                    ExpectedClosingBalance = nextDayExpectedClosingBalance,
                    OpenedByUserId = userId,
                    OpenedAt = DateTime.UtcNow,
                    Status = CashDrawerStatus.Open,
                    Notes = $"Opening balance carried forward from {balanceDate:yyyy-MM-dd} settlement"
                };

                await _cashDrawerBalanceRepository.AddAsync(nextDayBalance);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Shift settled for branch {BranchId} on {Date}. Actual: {Actual}, Settled: {Settled}, Carried Forward: {CarriedForward}", 
                branchId, balanceDate, actualClosingBalance, settledAmount, carriedForwardAmount);

            return existingBalance;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Calculate expected closing balance for a specific date with a given opening balance
    /// </summary>
    private async Task<decimal> CalculateExpectedClosingBalanceForDateAsync(int branchId, DateTime date, decimal openingBalance)
    {
        var startDate = date.Date;
        var endDate = startDate.AddDays(1);

        // Get cash transactions for the day - using FinancialTransactions
        var cashTransactions = await _context.FinancialTransactions
            .Where(ft => ft.BranchId == branchId && 
                       ft.TransactionDate >= startDate && 
                       ft.TransactionDate < endDate &&
                       ft.PaymentMethodId == LookupTableConstants.PaymentMethodCash &&
                       ft.StatusId == LookupTableConstants.FinancialTransactionStatusCompleted)
            .ToListAsync();

        var cashSales = cashTransactions
            .Where(ft => ft.TransactionTypeId == LookupTableConstants.FinancialTransactionTypeSale)
            .Sum(ft => ft.AmountPaid);

        var cashReturns = cashTransactions
            .Where(ft => ft.TransactionTypeId == LookupTableConstants.FinancialTransactionTypeReturn)
            .Sum(ft => Math.Abs(ft.ChangeGiven)); // Returns have negative change (refunds)

        var cashRepairs = cashTransactions
            .Where(ft => ft.TransactionTypeId == LookupTableConstants.FinancialTransactionTypeRepair)
            .Sum(ft => ft.AmountPaid);

        return openingBalance + cashSales + cashRepairs - cashReturns;
    }

    /// <summary>
    /// Refresh the expected closing balance for an open cash drawer to include recent transactions
    /// </summary>
    public async Task<CashDrawerBalance> RefreshExpectedClosingBalanceAsync(int branchId, DateTime? date = null)
    {
        var balanceDate = (date ?? DateTime.Today).Date;

        // Get existing balance
        var existingBalance = await _cashDrawerBalanceRepository.GetByBranchAndDateAsync(branchId, balanceDate);
        if (existingBalance == null)
        {
            throw new InvalidOperationException($"No cash drawer balance found for branch {branchId} on {balanceDate:yyyy-MM-dd}");
        }

        if (existingBalance.Status != CashDrawerStatus.Open)
        {
            throw new InvalidOperationException($"Cannot refresh expected closing balance - cash drawer is not open for branch {branchId} on {balanceDate:yyyy-MM-dd}");
        }

        // Recalculate expected closing balance with current transactions
        var expectedClosingBalance = await CalculateExpectedClosingBalanceAsync(branchId, balanceDate);

        // Update the balance
        existingBalance.ExpectedClosingBalance = expectedClosingBalance;
        
        _cashDrawerBalanceRepository.Update(existingBalance);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cash drawer expected closing balance refreshed for branch {BranchId} on {Date}. New expected balance: {ExpectedBalance}", 
            branchId, balanceDate, expectedClosingBalance);

        return existingBalance;
    }
}
