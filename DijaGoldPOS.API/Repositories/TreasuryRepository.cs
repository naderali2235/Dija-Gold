using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.IRepositories;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Shared;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Repositories;

public class TreasuryRepository : ITreasuryRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TreasuryRepository> _logger;

    public TreasuryRepository(ApplicationDbContext context, ILogger<TreasuryRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TreasuryAccount> GetOrCreateAccountAsync(int branchId, string? userId = null)
    {
        var account = await _context.TreasuryAccounts.FirstOrDefaultAsync(t => t.BranchId == branchId && !t.IsDeleted);
        if (account != null) return account;

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

    public async Task<IReadOnlyList<TreasuryTransaction>> GetTransactionsAsync(int treasuryAccountId, DateTime? from = null, DateTime? to = null, TreasuryTransactionType? type = null)
    {
        var q = _context.TreasuryTransactions.AsQueryable().Where(t => t.TreasuryAccountId == treasuryAccountId);
        if (from.HasValue) q = q.Where(t => t.PerformedAt >= from.Value);
        if (to.HasValue)
        {
            var end = to.Value.Date.AddDays(1);
            q = q.Where(t => t.PerformedAt < end);
        }
        if (type.HasValue) q = q.Where(t => t.Type == type.Value);
        return await q.OrderByDescending(t => t.PerformedAt).ToListAsync();
    }

    public async Task<(TreasuryTransaction treasuryTxn, SupplierTransaction supplierTxn)> PaySupplierAsync(int branchId, int supplierId, decimal amount, string userId, string? notes = null)
    {
        var account = await GetOrCreateAccountAsync(branchId, userId);
        var supplier = await _context.Suppliers.FindAsync(supplierId) ?? throw new InvalidOperationException($"Supplier {supplierId} not found");

        if (account.CurrentBalance < amount) throw new InvalidOperationException("Insufficient treasury balance");
        if (supplier.CurrentBalance < amount) throw new InvalidOperationException("Payment exceeds supplier outstanding balance");

        var treTxn = new TreasuryTransaction
        {
            TreasuryAccountId = account.Id,
            Amount = amount,
            Direction = TreasuryTransactionDirection.Debit,
            Type = TreasuryTransactionType.SupplierPayment,
            ReferenceType = nameof(Supplier),
            ReferenceId = supplierId.ToString(),
            Notes = notes,
            PerformedAt = DateTime.UtcNow,
            PerformedByUserId = userId
        };
        _context.TreasuryTransactions.Add(treTxn);

        account.CurrentBalance -= amount;
        account.UpdatedAt = DateTime.UtcNow;
        account.UpdatedByUserId = userId;

        await _context.SaveChangesAsync();

        var txnNumber = $"SP-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        var newSupplierBalance = supplier.CurrentBalance - amount;

        var supplierTxn = new SupplierTransaction
        {
            TransactionNumber = txnNumber,
            SupplierId = supplierId,
            TransactionDate = DateTime.UtcNow,
            TransactionType = "payment",
            Amount = amount,
            BalanceAfterTransaction = newSupplierBalance,
            ReferenceNumber = treTxn.Id.ToString(),
            Notes = notes,
            CreatedByUserId = userId,
            BranchId = branchId
        };
        _context.Set<SupplierTransaction>().Add(supplierTxn);

        supplier.CurrentBalance = newSupplierBalance;
        await _context.SaveChangesAsync();

        return (treTxn, supplierTxn);
    }
}
