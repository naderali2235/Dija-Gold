using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Models.Enums;

namespace DijaGoldPOS.API.Repositories;

/// <summary>
/// Repository interface for Transaction entity with specific business methods
/// </summary>
public interface ITransactionRepository : IRepository<Transaction>
{
    /// <summary>
    /// Get transaction by transaction number
    /// </summary>
    /// <param name="transactionNumber">Transaction number</param>
    /// <param name="branchId">Branch ID (optional)</param>
    /// <returns>Transaction or null if not found</returns>
    Task<Transaction?> GetByTransactionNumberAsync(string transactionNumber, int? branchId = null);

    /// <summary>
    /// Get transactions for a specific branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="fromDate">From date (optional)</param>
    /// <param name="toDate">To date (optional)</param>
    /// <returns>List of transactions</returns>
    Task<List<Transaction>> GetByBranchAsync(int branchId, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Get transactions for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="fromDate">From date (optional)</param>
    /// <param name="toDate">To date (optional)</param>
    /// <returns>List of transactions</returns>
    Task<List<Transaction>> GetByCustomerAsync(int customerId, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Get transactions by cashier
    /// </summary>
    /// <param name="cashierId">Cashier user ID</param>
    /// <param name="fromDate">From date (optional)</param>
    /// <param name="toDate">To date (optional)</param>
    /// <returns>List of transactions</returns>
    Task<List<Transaction>> GetByCashierAsync(string cashierId, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Get transactions by type
    /// </summary>
    /// <param name="transactionType">Transaction type</param>
    /// <param name="branchId">Branch ID (optional)</param>
    /// <param name="fromDate">From date (optional)</param>
    /// <param name="toDate">To date (optional)</param>
    /// <returns>List of transactions</returns>
    Task<List<Transaction>> GetByTypeAsync(TransactionType transactionType, int? branchId = null, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Get daily sales summary for a branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="date">Date</param>
    /// <returns>Daily sales summary</returns>
    Task<(decimal TotalSales, decimal TotalMakingCharges, decimal TotalTax, int TransactionCount)> GetDailySalesSummaryAsync(int branchId, DateTime date);

    /// <summary>
    /// Get sales summary by date range
    /// </summary>
    /// <param name="branchId">Branch ID (optional)</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>Sales summary</returns>
    Task<(decimal TotalSales, decimal TotalMakingCharges, decimal TotalTax, int TransactionCount)> GetSalesSummaryAsync(int? branchId, DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Get return transactions that can be processed (within return period)
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="returnPeriodDays">Return period in days</param>
    /// <returns>List of eligible return transactions</returns>
    Task<List<Transaction>> GetEligibleReturnTransactionsAsync(int branchId, int returnPeriodDays = 30);

    /// <summary>
    /// Get top customers by purchase amount
    /// </summary>
    /// <param name="branchId">Branch ID (optional)</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <param name="topCount">Number of top customers to return</param>
    /// <returns>List of top customers with purchase amounts</returns>
    Task<List<(Customer Customer, decimal TotalPurchases, int TransactionCount)>> GetTopCustomersAsync(int? branchId, DateTime fromDate, DateTime toDate, int topCount = 10);

    /// <summary>
    /// Get next transaction number for a branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="transactionType">Transaction type</param>
    /// <returns>Next transaction number</returns>
    Task<string> GetNextTransactionNumberAsync(int branchId, TransactionType transactionType);

    /// <summary>
    /// Check if transaction number exists
    /// </summary>
    /// <param name="transactionNumber">Transaction number</param>
    /// <param name="branchId">Branch ID</param>
    /// <returns>True if transaction number exists</returns>
    Task<bool> TransactionNumberExistsAsync(string transactionNumber, int branchId);
}