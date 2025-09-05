using DijaGoldPOS.API.Models.FinancialModels;
using DijaGoldPOS.API.Models.SupplierModels;

namespace DijaGoldPOS.API.IServices;

public interface ITreasuryService
{
    Task<TreasuryAccount> GetOrCreateAccountAsync(int branchId, string? userId = null);
    Task<decimal> GetBalanceAsync(int branchId);

    Task<TreasuryTransaction> AdjustAsync(int branchId, decimal amount, TreasuryTransactionDirection direction, string? reason, string userId);

    Task<TreasuryTransaction> FeedFromCashDrawerAsync(int branchId, DateTime date, string userId, string? notes = null);

    Task<IReadOnlyList<TreasuryTransaction>> GetTransactionsAsync(int branchId, DateTime? from = null, DateTime? to = null, TreasuryTransactionType? type = null);

    Task<(TreasuryTransaction treasuryTxn, SupplierTransaction supplierTxn)> PaySupplierAsync(int branchId, int supplierId, decimal amount, string userId, string? notes = null);
}
