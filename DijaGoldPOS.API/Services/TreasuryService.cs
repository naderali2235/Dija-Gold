using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Models.FinancialModels;
using DijaGoldPOS.API.Models.SupplierModels;
using DijaGoldPOS.API.Shared;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Services;

public class TreasuryService : ITreasuryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TreasuryService> _logger;
    private readonly IUnitOfWork _uow;
    private ITreasuryRepository TreasuryRepo => _uow.Treasury;

    public TreasuryService(ApplicationDbContext context, ILogger<TreasuryService> logger, IUnitOfWork uow)
    {
        _context = context;
        _logger = logger;
        _uow = uow;
    }

    public async Task<TreasuryAccount> GetOrCreateAccountAsync(int branchId, string? userId = null)
    {
        var account = await _context.TreasuryAccounts.FirstOrDefaultAsync(t => t.BranchId == branchId && !t.IsDeleted);
        if (account != null) return account;

        // validate branch
        var branch = await _context.Branches.FindAsync(branchId) ?? throw new ArgumentException($"Branch {branchId} not found");

        account = new TreasuryAccount
        {
            BranchId = branchId,
            CurrentBalance = 0m,
            CurrencyCode = "EGP",
            CreatedByUserId = userId
        };
        _context.TreasuryAccounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<decimal> GetBalanceAsync(int branchId)
    {
        var account = await GetOrCreateAccountAsync(branchId);
        return account.CurrentBalance;
    }

    public async Task<TreasuryTransaction> AdjustAsync(int branchId, decimal amount, TreasuryTransactionDirection direction, string? reason, string userId)
    {
        // Delegate to repository; caller manages transaction via UoW if needed
        var account = await TreasuryRepo.GetOrCreateAccountAsync(branchId, userId);
        // compute new balance
        var delta = direction == TreasuryTransactionDirection.Credit ? amount : -amount;
        var newBalance = account.CurrentBalance + delta;
        if (newBalance < 0) throw new InvalidOperationException("Insufficient treasury balance");

        var treTxn = new TreasuryTransaction
        {
            TreasuryAccountId = account.Id,
            Amount = amount,
            Direction = direction,
            Type = TreasuryTransactionType.Adjustment,
            Notes = reason,
            PerformedAt = DateTime.UtcNow,
            PerformedByUserId = userId
        };
        _context.TreasuryTransactions.Add(treTxn);

        account.CurrentBalance = newBalance;
        account.UpdatedAt = DateTime.UtcNow;
        account.UpdatedByUserId = userId;

        await _context.SaveChangesAsync();
        return treTxn;
    }

    public async Task<TreasuryTransaction> FeedFromCashDrawerAsync(int branchId, DateTime date, string userId, string? notes = null)
    {
        var balanceDate = date.Date;
        var cdb = await _context.CashDrawerBalances.FirstOrDefaultAsync(x => x.BranchId == branchId && x.BalanceDate == balanceDate);
        var settled = cdb?.SettledAmount ?? 0m;
        if (settled <= 0)
            throw new InvalidOperationException("No settled cash drawer amount to feed");

        var account = await TreasuryRepo.GetOrCreateAccountAsync(branchId, userId);

        var treTxn = new TreasuryTransaction
        {
            TreasuryAccountId = account.Id,
            Amount = settled,
            Direction = TreasuryTransactionDirection.Credit,
            Type = TreasuryTransactionType.FeedFromCashDrawer,
            ReferenceType = nameof(CashDrawerBalance),
            ReferenceId = cdb!.Id.ToString(),
            Notes = notes ?? $"Feed from cash drawer {balanceDate:yyyy-MM-dd}",
            PerformedAt = DateTime.UtcNow,
            PerformedByUserId = userId
        };
        _context.TreasuryTransactions.Add(treTxn);

        account.CurrentBalance += settled;
        account.UpdatedAt = DateTime.UtcNow;
        account.UpdatedByUserId = userId;

        await _context.SaveChangesAsync();
        return treTxn;
    }

    public async Task<IReadOnlyList<TreasuryTransaction>> GetTransactionsAsync(int branchId, DateTime? from = null, DateTime? to = null, TreasuryTransactionType? type = null)
    {
        var account = await GetOrCreateAccountAsync(branchId);
        return await TreasuryRepo.GetTransactionsAsync(account.Id, from, to, type);
    }

    public async Task<(TreasuryTransaction treasuryTxn, SupplierTransaction supplierTxn)> PaySupplierAsync(int branchId, int supplierId, decimal amount, string userId, string? notes = null)
    {
        // Delegate to repository; repository does pure EF operations without starting its own transaction
        return await TreasuryRepo.PaySupplierAsync(branchId, supplierId, amount, userId, notes);
    }
}
