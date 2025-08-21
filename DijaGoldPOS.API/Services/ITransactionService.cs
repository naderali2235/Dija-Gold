using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Models.Enums;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Interface for transaction processing service
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Process a sale transaction
    /// </summary>
    /// <param name="saleRequest">Sale transaction request</param>
    /// <param name="userId">User processing the sale</param>
    /// <returns>Transaction result</returns>
    Task<TransactionResult> ProcessSaleAsync(SaleTransactionRequest saleRequest, string userId);

    /// <summary>
    /// Process a return transaction
    /// </summary>
    /// <param name="returnRequest">Return transaction request</param>
    /// <param name="userId">User processing the return</param>
    /// <param name="managerId">Manager approving the return</param>
    /// <returns>Transaction result</returns>
    Task<TransactionResult> ProcessReturnAsync(ReturnTransactionRequest returnRequest, string userId, string managerId);

    /// <summary>
    /// Process a repair transaction
    /// </summary>
    /// <param name="repairRequest">Repair transaction request</param>
    /// <param name="userId">User processing the repair</param>
    /// <returns>Transaction result</returns>
    Task<TransactionResult> ProcessRepairAsync(RepairTransactionRequest repairRequest, string userId);

    /// <summary>
    /// Get transaction by ID
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <returns>Transaction with details</returns>
    Task<Transaction?> GetTransactionAsync(int transactionId);

    /// <summary>
    /// Get transaction by transaction number
    /// </summary>
    /// <param name="transactionNumber">Transaction number</param>
    /// <param name="branchId">Branch ID</param>
    /// <returns>Transaction with details</returns>
    Task<Transaction?> GetTransactionByNumberAsync(string transactionNumber, int branchId);

    /// <summary>
    /// Search transactions
    /// </summary>
    /// <param name="searchRequest">Search criteria</param>
    /// <returns>List of transactions</returns>
    Task<(List<Transaction> Transactions, int TotalCount)> SearchTransactionsAsync(TransactionSearchRequest searchRequest);

    /// <summary>
    /// Cancel/void a transaction
    /// </summary>
    /// <param name="transactionId">Transaction ID to cancel</param>
    /// <param name="reason">Reason for cancellation</param>
    /// <param name="userId">User performing cancellation</param>
    /// <param name="managerId">Manager approving cancellation</param>
    /// <returns>Success status</returns>
    Task<bool> CancelTransactionAsync(int transactionId, string reason, string userId, string managerId);

    /// <summary>
    /// Void a pending transaction
    /// </summary>
    /// <param name="transactionId">Transaction ID to void</param>
    /// <param name="reason">Reason for voiding</param>
    /// <param name="userId">User performing void</param>
    /// <returns>Void result</returns>
    Task<TransactionResult> VoidTransactionAsync(int transactionId, string reason, string userId);

    /// <summary>
    /// Create a reverse transaction for full transaction reversal
    /// </summary>
    /// <param name="originalTransactionId">Original transaction to reverse</param>
    /// <param name="reason">Reason for reversal</param>
    /// <param name="userId">User performing reversal</param>
    /// <param name="managerId">Manager approving reversal</param>
    /// <returns>Reverse transaction result</returns>
    Task<TransactionResult> CreateReverseTransactionAsync(int originalTransactionId, string reason, string userId, string managerId);

    /// <summary>
    /// Validate if a transaction can be voided
    /// </summary>
    /// <param name="transactionId">Transaction ID to check</param>
    /// <returns>Validation result</returns>
    Task<(bool CanVoid, string? ErrorMessage)> CanVoidTransactionAsync(int transactionId);

    /// <summary>
    /// Validate if a transaction can be reversed
    /// </summary>
    /// <param name="transactionId">Transaction ID to check</param>
    /// <returns>Validation result</returns>
    Task<(bool CanReverse, string? ErrorMessage)> CanReverseTransactionAsync(int transactionId);

    /// <summary>
    /// Generate next transaction number for branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="transactionType">Transaction type</param>
    /// <returns>Next transaction number</returns>
    Task<string> GenerateTransactionNumberAsync(int branchId, TransactionType transactionType);

    /// <summary>
    /// Debug method to calculate transaction totals without saving
    /// </summary>
    /// <param name="saleRequest">Sale transaction request</param>
    /// <param name="userId">User ID</param>
    /// <returns>Debug calculation information</returns>
    Task<object> DebugCalculateTransactionTotalsAsync(SaleTransactionRequest saleRequest, string userId);
}

/// <summary>
/// Sale transaction request
/// </summary>
public class SaleTransactionRequest
{
    public int BranchId { get; set; }
    public int? CustomerId { get; set; }
    public List<SaleItemRequest> Items { get; set; } = new();
    public decimal AmountPaid { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
}

/// <summary>
/// Sale item request
/// </summary>
public class SaleItemRequest
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal? CustomDiscountPercentage { get; set; }
}

/// <summary>
/// Return transaction request
/// </summary>
public class ReturnTransactionRequest
{
    public int OriginalTransactionId { get; set; }
    public string ReturnReason { get; set; } = string.Empty;
    public decimal ReturnAmount { get; set; }
    public List<ReturnItemRequest> Items { get; set; } = new();
}

/// <summary>
/// Return item request
/// </summary>
public class ReturnItemRequest
{
    public int OriginalTransactionItemId { get; set; }
    public decimal ReturnQuantity { get; set; }
}

/// <summary>
/// Repair transaction request
/// </summary>
public class RepairTransactionRequest
{
    public int BranchId { get; set; }
    public int? CustomerId { get; set; }
    public string RepairDescription { get; set; } = string.Empty;
    public decimal RepairAmount { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public decimal AmountPaid { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
}

/// <summary>
/// Transaction search request
/// </summary>
public class TransactionSearchRequest
{
    public int? BranchId { get; set; }
    public string? TransactionNumber { get; set; }
    public TransactionType? TransactionType { get; set; }
    public TransactionStatus? Status { get; set; }
    public int? CustomerId { get; set; }
    public string? CashierId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Transaction processing result
/// </summary>
public class TransactionResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public Transaction? Transaction { get; set; }
    public string? ReceiptContent { get; set; }
    
    /// <summary>
    /// Success status (alias for IsSuccess)
    /// </summary>
    public bool Success 
    { 
        get => IsSuccess; 
        set => IsSuccess = value; 
    }
    
    /// <summary>
    /// Result message (alias for ErrorMessage)
    /// </summary>
    public string? Message 
    { 
        get => ErrorMessage; 
        set => ErrorMessage = value; 
    }
    
    /// <summary>
    /// Transaction ID
    /// </summary>
    public int? TransactionId { get; set; }
    
    /// <summary>
    /// Transaction number
    /// </summary>
    public string? TransactionNumber { get; set; }
}