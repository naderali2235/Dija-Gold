

namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// Financial transaction DTO
/// </summary>
public class FinancialTransactionDto
{
    public int Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int TransactionTypeId { get; set; }
    public string TransactionTypeDescription { get; set; } = string.Empty;
    public FinancialTransactionTypeLookupDto? TransactionType { get; set; }
    public int? BusinessEntityId { get; set; }
    public string? BusinessEntityType { get; set; }
    public string ProcessedByUserId { get; set; } = string.Empty;
    public string ProcessedByUserName { get; set; } = string.Empty;
    public string? ApprovedByUserId { get; set; }
    public string? ApprovedByUserName { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal ChangeGiven { get; set; }
    public int PaymentMethodId { get; set; }
    public string PaymentMethodDescription { get; set; } = string.Empty;
    public PaymentMethodLookupDto? PaymentMethod { get; set; }
    public int StatusId { get; set; }
    public string StatusDescription { get; set; } = string.Empty;
    public FinancialTransactionStatusLookupDto? Status { get; set; }
    public int? OriginalTransactionId { get; set; }
    public string? ReversalReason { get; set; }
    public string? Notes { get; set; }
    public bool ReceiptPrinted { get; set; }
    public bool GeneralLedgerPosted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Create financial transaction request DTO
/// </summary>
public class CreateFinancialTransactionRequestDto
{

    public int BranchId { get; set; }
    

    public int TransactionTypeId { get; set; }
    
    public int? BusinessEntityId { get; set; }
    
    public int BusinessEntityTypeId { get; set; }
    

    public decimal Subtotal { get; set; }
    
    public decimal TotalTaxAmount { get; set; }
    
    public decimal TotalDiscountAmount { get; set; }
    

    public decimal TotalAmount { get; set; }
    

    public decimal AmountPaid { get; set; }
    
    public decimal ChangeGiven { get; set; }
    

    public int PaymentMethodId { get; set; }
    
    public string? Notes { get; set; }
    public string? ApprovedByUserId { get; set; }
}

/// <summary>
/// Update financial transaction request DTO
/// </summary>
public class UpdateFinancialTransactionRequestDto
{
    public decimal? Subtotal { get; set; }
    public decimal? TotalTaxAmount { get; set; }
    public decimal? TotalDiscountAmount { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? AmountPaid { get; set; }
    public decimal? ChangeGiven { get; set; }
    public int? PaymentMethodId { get; set; }
    public int? StatusId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Financial transaction search request DTO
/// </summary>
public class FinancialTransactionSearchRequestDto
{
    public int? BranchId { get; set; }
    public int? TransactionTypeId { get; set; }
    public int? StatusId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? TransactionNumber { get; set; }
    public string? ProcessedByUserId { get; set; }
    public int? BusinessEntityId { get; set; }
    public int? BusinessEntityTypeId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Financial transaction summary DTO
/// </summary>
public class FinancialTransactionSummaryDto
{
    public int TotalTransactions { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public Dictionary<int, int> TransactionTypeCounts { get; set; } = new();
    public Dictionary<int, int> StatusCounts { get; set; } = new();
}
