using DijaGoldPOS.API.Models.SupplierModels;

namespace DijaGoldPOS.API.IRepositories;

/// <summary>
/// Repository interface for Supplier entity with specific business methods
/// </summary>
public interface ISupplierRepository : IRepository<Supplier>
{
    /// <summary>
    /// Get suppliers with outstanding balances
    /// </summary>
    /// <returns>List of suppliers with outstanding balances</returns>
    Task<List<Supplier>> GetSuppliersWithOutstandingBalancesAsync();

    /// <summary>
    /// Get suppliers near credit limit
    /// </summary>
    /// <param name="warningPercentage">Warning percentage (default 80%)</param>
    /// <returns>List of suppliers near credit limit</returns>
    Task<List<Supplier>> GetSuppliersNearCreditLimitAsync(decimal warningPercentage = 0.8m);

    /// <summary>
    /// Get suppliers over credit limit
    /// </summary>
    /// <returns>List of suppliers over credit limit</returns>
    Task<List<Supplier>> GetSuppliersOverCreditLimitAsync();

    /// <summary>
    /// Search suppliers by company name or contact info
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>List of matching suppliers</returns>
    Task<List<Supplier>> SearchAsync(string searchTerm);

    /// <summary>
    /// Get supplier purchase history summary
    /// </summary>
    /// <param name="supplierId">Supplier ID</param>
    /// <param name="fromDate">From date (optional)</param>
    /// <param name="toDate">To date (optional)</param>
    /// <returns>Purchase history summary</returns>
    Task<(decimal TotalPurchases, int PurchaseOrderCount, decimal OutstandingBalance)> GetPurchaseHistorySummaryAsync(int supplierId, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Update supplier balance
    /// </summary>
    /// <param name="supplierId">Supplier ID</param>
    /// <param name="balanceChange">Balance change amount (positive for purchases, negative for payments)</param>
    /// <returns>Updated supplier</returns>
    Task<Supplier?> UpdateBalanceAsync(int supplierId, decimal balanceChange);

    /// <summary>
    /// Get top suppliers by purchase volume
    /// </summary>
    /// <param name="topCount">Number of top suppliers to return</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>List of top suppliers</returns>
    Task<List<(Supplier Supplier, decimal TotalPurchases, int PurchaseOrderCount)>> GetTopSuppliersAsync(int topCount, DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Check if supplier can make additional purchases based on credit limit
    /// </summary>
    /// <param name="supplierId">Supplier ID</param>
    /// <param name="additionalAmount">Additional purchase amount</param>
    /// <returns>True if within credit limit</returns>
    Task<bool> CanMakeAdditionalPurchaseAsync(int supplierId, decimal additionalAmount);

    /// <summary>
    /// Get supplier transactions with optional date filtering
    /// </summary>
    /// <param name="supplierId">Supplier ID</param>
    /// <param name="fromDate">From date (optional)</param>
    /// <param name="toDate">To date (optional)</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paged list of supplier transactions</returns>
    Task<(List<SupplierTransaction> Transactions, int TotalCount)> GetTransactionsAsync(
        int supplierId, 
        DateTime? fromDate = null, 
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 20);

    /// <summary>
    /// Create a new supplier transaction
    /// </summary>
    /// <param name="transaction">Supplier transaction to create</param>
    /// <returns>Created transaction</returns>
    Task<SupplierTransaction> CreateTransactionAsync(SupplierTransaction transaction);

    /// <summary>
    /// Get recent transactions for a supplier
    /// </summary>
    /// <param name="supplierId">Supplier ID</param>
    /// <param name="count">Number of recent transactions to return</param>
    /// <returns>List of recent transactions</returns>
    Task<List<SupplierTransaction>> GetRecentTransactionsAsync(int supplierId, int count = 10);
}
