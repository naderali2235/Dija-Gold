using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.IRepositories;

public interface ITreasuryRepository
{
    Task<TreasuryAccount> GetOrCreateAccountAsync(int branchId, string? userId = null);
    Task<IReadOnlyList<TreasuryTransaction>> GetTransactionsAsync(int treasuryAccountId, DateTime? from = null, DateTime? to = null, TreasuryTransactionType? type = null);
    Task<(TreasuryTransaction treasuryTxn, SupplierTransaction supplierTxn)> PaySupplierAsync(int branchId, int supplierId, decimal amount, string userId, string? notes = null);
}
