using DijaGoldPOS.API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents a transaction (Sale, Return, Repair)
/// </summary>
public class Transaction : BaseEntity
{
    /// <summary>
    /// Transaction number (sequential, unique per branch)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string TransactionNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of transaction
    /// </summary>
    [Required]
    public TransactionType TransactionType { get; set; }
    
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
    /// Customer ID (optional)
    /// </summary>
    public int? CustomerId { get; set; }
    
    /// <summary>
    /// Cashier who processed the transaction
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string CashierId { get; set; } = string.Empty;
    
    /// <summary>
    /// Manager who approved (for returns)
    /// </summary>
    [MaxLength(450)]
    public string? ApprovedBy { get; set; }
    
    /// <summary>
    /// Subtotal before taxes and discounts
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }
    
    /// <summary>
    /// Total making charges
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalMakingCharges { get; set; }
    
    /// <summary>
    /// Total tax amount
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalTaxAmount { get; set; }
    
    /// <summary>
    /// Discount amount applied
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; }
    
    /// <summary>
    /// Final total amount
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Amount paid by customer
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; }
    
    /// <summary>
    /// Change given to customer
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ChangeGiven { get; set; }
    
    /// <summary>
    /// Payment method used
    /// </summary>
    [Required]
    public PaymentMethod PaymentMethod { get; set; }
    
    /// <summary>
    /// Transaction status
    /// </summary>
    [Required]
    public TransactionStatus Status { get; set; }
    
    /// <summary>
    /// Reference to original transaction (for returns)
    /// </summary>
    public int? OriginalTransactionId { get; set; }
    
    /// <summary>
    /// Return reason (for return transactions)
    /// </summary>
    [MaxLength(500)]
    public string? ReturnReason { get; set; }
    
    /// <summary>
    /// Repair description (for repair transactions)
    /// </summary>
    [MaxLength(1000)]
    public string? RepairDescription { get; set; }
    
    /// <summary>
    /// Estimated completion date (for repairs)
    /// </summary>
    public DateTime? EstimatedCompletionDate { get; set; }
    
    /// <summary>
    /// Gold rate used for this transaction
    /// </summary>
    public int? GoldRateId { get; set; }
    
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
    public virtual Branch Branch { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to customer
    /// </summary>
    public virtual Customer? Customer { get; set; }
    
    /// <summary>
    /// Navigation property to cashier
    /// </summary>
    public virtual ApplicationUser Cashier { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to approving manager
    /// </summary>
    public virtual ApplicationUser? ApprovedByUser { get; set; }
    
    /// <summary>
    /// Navigation property to gold rate used
    /// </summary>
    public virtual GoldRate? GoldRate { get; set; }
    
    /// <summary>
    /// Navigation property to original transaction (for returns)
    /// </summary>
    public virtual Transaction? OriginalTransaction { get; set; }
    
    /// <summary>
    /// Navigation property to return transactions
    /// </summary>
    public virtual ICollection<Transaction> ReturnTransactions { get; set; } = new List<Transaction>();
    
    /// <summary>
    /// Navigation property to transaction items
    /// </summary>
    public virtual ICollection<TransactionItem> TransactionItems { get; set; } = new List<TransactionItem>();
    
    /// <summary>
    /// Navigation property to transaction taxes
    /// </summary>
    public virtual ICollection<TransactionTax> TransactionTaxes { get; set; } = new List<TransactionTax>();
}