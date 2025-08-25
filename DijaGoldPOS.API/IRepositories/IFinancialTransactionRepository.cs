using DijaGoldPOS.API.Models;


namespace DijaGoldPOS.API.IRepositories;

/// <summary>
/// Repository interface for financial transaction operations
/// </summary>
public interface IFinancialTransactionRepository : IRepository<FinancialTransaction>
{
    /// <summary>
    /// Get financial transaction by transaction number and branch
    /// </summary>
    /// <param name="transactionNumber">Transaction number</param>
    /// <param name="branchId">Branch ID</param>
    /// <returns>Financial transaction</returns>
    Task<FinancialTransaction?> GetByTransactionNumberAsync(string transactionNumber, int branchId);

    /// <summary>
    /// Get financial transactions by business entity
    /// </summary>
    /// <param name="businessEntityId">Business entity ID</param>
    /// <param name="businessEntityTypeId">Business entity type ID</param>
    /// <returns>List of financial transactions</returns>
    Task<List<FinancialTransaction>> GetByBusinessEntityAsync(int businessEntityId, int businessEntityTypeId);

    /// <summary>
    /// Search financial transactions with filters
    /// </summary>
    /// <param name="branchId">Branch ID filter</param>
    /// <param name="transactionTypeId">Transaction type ID filter</param>
    /// <param name="statusId">Status ID filter</param>
    /// <param name="fromDate">From date filter</param>
    /// <param name="toDate">To date filter</param>
    /// <param name="transactionNumber">Transaction number filter</param>
    /// <param name="processedByUserId">Processed by user filter</param>
    /// <param name="businessEntityId">Business entity ID filter</param>
    /// <param name="businessEntityTypeId">Business entity type ID filter</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated financial transactions</returns>
    Task<(List<FinancialTransaction> Transactions, int TotalCount)> SearchAsync(
        int? branchId = null,
        int? transactionTypeId = null,
        int? statusId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? transactionNumber = null,
        string? processedByUserId = null,
        int? businessEntityId = null,
        int? businessEntityTypeId = null,
        int page = 1,
        int pageSize = 20);

    /// <summary>
    /// Get financial transaction summary
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>Financial transaction summary</returns>
    Task<FinancialTransactionSummary> GetSummaryAsync(int? branchId = null, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Get next transaction number for a branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <returns>Next transaction number</returns>
    Task<string> GetNextTransactionNumberAsync(int branchId);

    /// <summary>
    /// Get financial transactions by original transaction (for reversals)
    /// </summary>
    /// <param name="originalTransactionId">Original transaction ID</param>
    /// <returns>List of reversal transactions</returns>
    Task<List<FinancialTransaction>> GetReversalTransactionsAsync(int originalTransactionId);

    /// <summary>
    /// Get financial transactions by date range
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>List of financial transactions</returns>
    Task<List<FinancialTransaction>> GetByDateRangeAsync(int branchId, DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Get financial transactions by user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>List of financial transactions</returns>
    Task<List<FinancialTransaction>> GetByUserAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null);
}

/// <summary>
/// Financial transaction summary for reporting
/// </summary>
public class FinancialTransactionSummary
{
    public int TotalTransactions { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public Dictionary<int, int> TransactionTypeCounts { get; set; } = new();
    public Dictionary<int, int> StatusCounts { get; set; } = new();
}
