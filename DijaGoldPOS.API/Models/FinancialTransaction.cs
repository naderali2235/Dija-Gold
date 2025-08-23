using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DijaGoldPOS.API.Models.LookupTables;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents a financial transaction (credits and debits only)
/// </summary>
public class FinancialTransaction : BaseEntity
{
    /// <summary>
    /// Transaction number (sequential, unique per branch)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string TransactionNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Transaction date and time
    /// </summary>
    [Required]
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Branch where transaction occurred
    /// </summary>
    [Required]
    public int BranchId { get; set; }
    
    /// <summary>
    /// Type of financial transaction
    /// </summary>
    [Required]
    public int TransactionTypeId { get; set; }
    
    /// <summary>
    /// Reference to the business entity (Order, RepairJob, etc.)
    /// </summary>
    public int? BusinessEntityId { get; set; }
    
    /// <summary>
    /// Type of business entity (Order, RepairJob, etc.)
    /// </summary>
    [Required]
    public int BusinessEntityTypeId { get; set; }
    
    /// <summary>
    /// User who processed the transaction
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string ProcessedByUserId { get; set; } = string.Empty;
    
    /// <summary>
    /// User who approved the transaction (if required)
    /// </summary>
    [MaxLength(450)]
    public string? ApprovedByUserId { get; set; }
    
    /// <summary>
    /// Subtotal amount
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }
    
    /// <summary>
    /// Total tax amount
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalTaxAmount { get; set; }
    
    /// <summary>
    /// Total discount amount
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDiscountAmount { get; set; }
    
    /// <summary>
    /// Total amount (final amount)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Amount paid by customer/client
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; }
    
    /// <summary>
    /// Change given to customer/client
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ChangeGiven { get; set; }
    
    /// <summary>
    /// Payment method used
    /// </summary>
    [Required]
    public int PaymentMethodId { get; set; }
    
    /// <summary>
    /// Transaction status
    /// </summary>
    [Required]
    public int StatusId { get; set; }
    
    /// <summary>
    /// Reference to original transaction (for refunds/reversals)
    /// </summary>
    public int? OriginalTransactionId { get; set; }
    
    /// <summary>
    /// Reason for refund/reversal
    /// </summary>
    [MaxLength(500)]
    public string? ReversalReason { get; set; }
    
    /// <summary>
    /// Additional notes for the transaction
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Receipt printed flag
    /// </summary>
    public bool ReceiptPrinted { get; set; } = false;
    
    /// <summary>
    /// General ledger posted flag
    /// </summary>
    public bool GeneralLedgerPosted { get; set; } = false;
    
    /// <summary>
    /// Navigation property to branch
    /// </summary>
    [JsonIgnore]
    public virtual Branch Branch { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to user who processed the transaction
    /// </summary>
    [JsonIgnore]
    public virtual ApplicationUser ProcessedByUser { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to user who approved the transaction
    /// </summary>
    [JsonIgnore]
    public virtual ApplicationUser? ApprovedByUser { get; set; }
    
    /// <summary>
    /// Navigation property to original transaction (for refunds/reversals)
    /// </summary>
    [JsonIgnore]
    public virtual FinancialTransaction? OriginalTransaction { get; set; }
    
    /// <summary>
    /// Navigation property to reversal transactions
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<FinancialTransaction> ReversalTransactions { get; set; } = new List<FinancialTransaction>();
    
    /// <summary>
    /// Navigation property to transaction type lookup
    /// </summary>
    [JsonIgnore]
    public virtual FinancialTransactionTypeLookup TransactionType { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to business entity type lookup
    /// </summary>
    [JsonIgnore]
    public virtual BusinessEntityTypeLookup BusinessEntityType { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to payment method lookup
    /// </summary>
    [JsonIgnore]
    public virtual PaymentMethodLookup PaymentMethod { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to transaction status lookup
    /// </summary>
    [JsonIgnore]
    public virtual FinancialTransactionStatusLookup Status { get; set; } = null!;
    
    // TransactionTaxes navigation property removed - obsolete model
}
