using DijaGoldPOS.API.Models.FinancialModels;


namespace DijaGoldPOS.API.Services;

/// <summary>
/// Request for creating a financial transaction
/// </summary>
public class CreateFinancialTransactionRequest
{
    public int BranchId { get; set; }
    public int TransactionTypeId { get; set; } // Changed from FinancialTransactionType to int
    public int? BusinessEntityId { get; set; }
    public int BusinessEntityTypeId { get; set; } // Changed from string to int
    public decimal Subtotal { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal ChangeGiven { get; set; }
    public int PaymentMethodId { get; set; } // Changed from PaymentMethod to int
    public string? Notes { get; set; }
    public string? ApprovedByUserId { get; set; }
}

/// <summary>
/// Request for updating a financial transaction
/// </summary>
public class UpdateFinancialTransactionRequest
{
    public decimal? Subtotal { get; set; }
    public decimal? TotalTaxAmount { get; set; }
    public decimal? TotalDiscountAmount { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? AmountPaid { get; set; }
    public decimal? ChangeGiven { get; set; }
    public int? PaymentMethodId { get; set; } // Changed from PaymentMethod to int
    public int? StatusId { get; set; } // Changed from FinancialTransactionStatus to int
    public string? Notes { get; set; }
}

/// <summary>
/// Request for searching financial transactions
/// </summary>
public class FinancialTransactionSearchRequest
{
    public int? BranchId { get; set; }
    public int? TransactionTypeId { get; set; } // Changed from FinancialTransactionType to int
    public int? StatusId { get; set; } // Changed from FinancialTransactionStatus to int
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? TransactionNumber { get; set; }
    public string? ProcessedByUserId { get; set; }
    public int? BusinessEntityId { get; set; }
    public int? BusinessEntityTypeId { get; set; } // Changed from string to int
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Request for voiding a financial transaction
/// </summary>
public class VoidFinancialTransactionRequest
{
    public int TransactionId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Request for creating a reversal transaction
/// </summary>
public class CreateReversalTransactionRequest
{
    public int OriginalTransactionId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string ManagerId { get; set; } = string.Empty;
}

/// <summary>
/// Request for marking receipt as printed
/// </summary>
public class MarkReceiptPrintedRequest
{
    public int TransactionId { get; set; }
}

/// <summary>
/// Request for marking general ledger as posted
/// </summary>
public class MarkGeneralLedgerPostedRequest
{
    public int TransactionId { get; set; }
}

/// <summary>
/// Result of financial transaction operations
/// </summary>
public class FinancialTransactionResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public FinancialTransaction? Transaction { get; set; }
}

/// <summary>
/// Financial transaction summary
/// </summary>
public class FinancialTransactionSummary
{
    public int TotalTransactions { get; set; }
    public decimal TotalAmount { get; set; }
    public Dictionary<int, int> TransactionTypeCounts { get; set; } = new(); // Changed from FinancialTransactionType to int
    public Dictionary<int, int> StatusCounts { get; set; } = new(); // Changed from FinancialTransactionStatus to int
}
