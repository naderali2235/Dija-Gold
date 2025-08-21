using DijaGoldPOS.API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// Transaction DTO for display operations
/// </summary>
public class TransactionDto
{
    public int Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    public DateTime TransactionDate { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public string? ApprovedByName { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TotalMakingCharges { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal ChangeGiven { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public TransactionStatus Status { get; set; }
    public string StatusDisplayName => Status.ToString();
    public string? ReturnReason { get; set; }
    public string? RepairDescription { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public bool ReceiptPrinted { get; set; }
    public List<TransactionItemDto> Items { get; set; } = new();
    public List<TransactionTaxDto> Taxes { get; set; } = new();
}

/// <summary>
/// Transaction item DTO
/// </summary>
public class TransactionItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public KaratType KaratType { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitWeight { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal GoldRatePerGram { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal MakingChargesAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
}

/// <summary>
/// Transaction tax DTO
/// </summary>
public class TransactionTaxDto
{
    public int Id { get; set; }
    public string TaxName { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
}

/// <summary>
/// Sale transaction request DTO
/// </summary>
public class SaleTransactionRequestDto
{
    [Required(ErrorMessage = "Branch ID is required")]
    public int BranchId { get; set; }

    public int? CustomerId { get; set; }

    [Required(ErrorMessage = "At least one item is required")]
    [MinLength(1, ErrorMessage = "At least one item is required")]
    public List<SaleItemRequestDto> Items { get; set; } = new();

    [Required(ErrorMessage = "Amount paid is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount paid must be greater than 0")]
    public decimal AmountPaid { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
}

/// <summary>
/// Sale item request DTO
/// </summary>
public class SaleItemRequestDto
{
    [Required(ErrorMessage = "Product ID is required")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [Range(0, 100, ErrorMessage = "Discount percentage must be between 0 and 100")]
    public decimal? CustomDiscountPercentage { get; set; }
}

/// <summary>
/// Return transaction request DTO
/// </summary>
public class ReturnTransactionRequestDto
{
    [Required(ErrorMessage = "Original transaction ID is required")]
    public int OriginalTransactionId { get; set; }

    [Required(ErrorMessage = "Return reason is required")]
    [StringLength(500, ErrorMessage = "Return reason cannot exceed 500 characters")]
    public string ReturnReason { get; set; } = string.Empty;

    [Required(ErrorMessage = "Return amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Return amount must be greater than 0")]
    public decimal ReturnAmount { get; set; }

    [Required(ErrorMessage = "At least one return item is required")]
    [MinLength(1, ErrorMessage = "At least one return item is required")]
    public List<ReturnItemRequestDto> Items { get; set; } = new();
}

/// <summary>
/// Return item request DTO
/// </summary>
public class ReturnItemRequestDto
{
    [Required(ErrorMessage = "Original transaction item ID is required")]
    public int OriginalTransactionItemId { get; set; }

    [Required(ErrorMessage = "Return quantity is required")]
    [Range(0.001, double.MaxValue, ErrorMessage = "Return quantity must be greater than 0")]
    public decimal ReturnQuantity { get; set; }
}

/// <summary>
/// Repair transaction request DTO
/// </summary>
public class RepairTransactionRequestDto
{
    [Required(ErrorMessage = "Branch ID is required")]
    public int BranchId { get; set; }

    public int? CustomerId { get; set; }

    [Required(ErrorMessage = "Repair description is required")]
    [StringLength(1000, ErrorMessage = "Repair description cannot exceed 1000 characters")]
    public string RepairDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "Repair amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Repair amount must be greater than 0")]
    public decimal RepairAmount { get; set; }

    public DateTime? EstimatedCompletionDate { get; set; }

    [Required(ErrorMessage = "Amount paid is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Amount paid cannot be negative")]
    public decimal AmountPaid { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
}

/// <summary>
/// Transaction search request DTO
/// </summary>
public class TransactionSearchRequestDto
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
/// Cancel transaction request DTO
/// </summary>
public class CancelTransactionRequestDto
{
    [Required(ErrorMessage = "Transaction ID is required")]
    public int TransactionId { get; set; }

    [Required(ErrorMessage = "Cancellation reason is required")]
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Transaction summary DTO
/// </summary>
public class TransactionSummaryDto
{
    public int TransactionCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalTax { get; set; }
    public decimal AverageTransactionValue { get; set; }
    public Dictionary<TransactionType, int> TransactionsByType { get; set; } = new();
    public Dictionary<PaymentMethod, decimal> AmountsByPaymentMethod { get; set; } = new();
}

/// <summary>
/// Reprint receipt request DTO
/// </summary>
public class ReprintReceiptRequestDto
{
    [Required(ErrorMessage = "Transaction ID is required")]
    public int TransactionId { get; set; }

    [Range(1, 10, ErrorMessage = "Number of copies must be between 1 and 10")]
    public int Copies { get; set; } = 1;
}

/// <summary>
/// Transaction receipt DTO
/// </summary>
public class TransactionReceiptDto
{
    public string ReceiptContent { get; set; } = string.Empty;
    public bool PrintedSuccessfully { get; set; }
    public string? PrintError { get; set; }
}

/// <summary>
/// Void transaction request DTO
/// </summary>
public class VoidTransactionRequestDto
{
    [Required(ErrorMessage = "Void reason is required")]
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Refund transaction request DTO
/// </summary>
public class RefundTransactionRequestDto
{
    [Required(ErrorMessage = "Original transaction ID is required")]
    public int OriginalTransactionId { get; set; }

    [Required(ErrorMessage = "Refund amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Refund amount must be greater than 0")]
    public decimal RefundAmount { get; set; }

    [Required(ErrorMessage = "Refund reason is required")]
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Can void response DTO
/// </summary>
public class CanVoidResponseDto
{
    public bool CanVoid { get; set; }
    public string? ErrorMessage { get; set; }
}