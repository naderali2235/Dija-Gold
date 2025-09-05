using DijaGoldPOS.API.Models.FinancialModels;
using DijaGoldPOS.API.Services;


namespace DijaGoldPOS.API.IServices;

/// <summary>
/// Interface for financial transaction processing service
/// </summary>
public interface IFinancialTransactionService
{
    /// <summary>
    /// Create a new financial transaction
    /// </summary>
    /// <param name="request">Financial transaction request</param>
    /// <param name="userId">User creating the transaction</param>
    /// <returns>Created financial transaction</returns>
    Task<FinancialTransaction> CreateFinancialTransactionAsync(CreateFinancialTransactionRequest request, string userId);

    /// <summary>
    /// Get financial transaction by ID
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <returns>Financial transaction with details</returns>
    Task<FinancialTransaction?> GetFinancialTransactionAsync(int transactionId);

    /// <summary>
    /// Get financial transaction by transaction number
    /// </summary>
    /// <param name="transactionNumber">Transaction number</param>
    /// <param name="branchId">Branch ID</param>
    /// <returns>Financial transaction with details</returns>
    Task<FinancialTransaction?> GetFinancialTransactionByNumberAsync(string transactionNumber, int branchId);

    /// <summary>
    /// Search financial transactions
    /// </summary>
    /// <param name="searchRequest">Search criteria</param>
    /// <returns>List of financial transactions</returns>
    Task<(List<FinancialTransaction> Transactions, int TotalCount)> SearchFinancialTransactionsAsync(FinancialTransactionSearchRequest searchRequest);

    /// <summary>
    /// Update financial transaction
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="request">Update request</param>
    /// <param name="userId">User performing update</param>
    /// <returns>Updated financial transaction</returns>
    Task<FinancialTransaction?> UpdateFinancialTransactionAsync(int transactionId, UpdateFinancialTransactionRequest request, string userId);

    /// <summary>
    /// Void a financial transaction
    /// </summary>
    /// <param name="transactionId">Transaction ID to void</param>
    /// <param name="reason">Reason for voiding</param>
    /// <param name="userId">User performing void</param>
    /// <returns>Void result</returns>
    Task<FinancialTransactionResult> VoidFinancialTransactionAsync(int transactionId, string reason, string userId);

    /// <summary>
    /// Create a reversal transaction
    /// </summary>
    /// <param name="originalTransactionId">Original transaction to reverse</param>
    /// <param name="reason">Reason for reversal</param>
    /// <param name="userId">User performing reversal</param>
    /// <param name="managerId">Manager approving reversal</param>
    /// <returns>Reversal transaction result</returns>
    Task<FinancialTransactionResult> CreateReversalTransactionAsync(int originalTransactionId, string reason, string userId, string managerId);

    /// <summary>
    /// Validate if a transaction can be voided
    /// </summary>
    /// <param name="transactionId">Transaction ID to check</param>
    /// <returns>Validation result</returns>
    Task<(bool CanVoid, string? ErrorMessage)> CanVoidFinancialTransactionAsync(int transactionId);

    /// <summary>
    /// Validate if a transaction can be reversed
    /// </summary>
    /// <param name="transactionId">Transaction ID to check</param>
    /// <returns>Validation result</returns>
    Task<(bool CanReverse, string? ErrorMessage)> CanReverseFinancialTransactionAsync(int transactionId);

    /// <summary>
    /// Get financial transaction summary
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <returns>Financial transaction summary</returns>
    Task<FinancialTransactionSummary> GetFinancialTransactionSummaryAsync(int? branchId, DateTime? fromDate, DateTime? toDate);

    /// <summary>
    /// Generate next transaction number for a branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <returns>Next transaction number</returns>
    Task<string> GenerateNextTransactionNumberAsync(int branchId);

    /// <summary>
    /// Mark transaction as receipt printed
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="userId">User marking as printed</param>
    /// <returns>Success status</returns>
    Task<bool> MarkReceiptPrintedAsync(int transactionId, string userId);

    /// <summary>
    /// Mark transaction as general ledger posted
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="userId">User marking as posted</param>
    /// <returns>Success status</returns>
    Task<bool> MarkGeneralLedgerPostedAsync(int transactionId, string userId);
}

// Service request classes moved to FinancialTransactionServiceRequests.cs
