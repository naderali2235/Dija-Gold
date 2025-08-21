using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents a supplier transaction for tracking financial movements
/// </summary>
public class SupplierTransaction : BaseEntity
{
    /// <summary>
    /// Unique transaction number
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string TransactionNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Supplier ID
    /// </summary>
    [Required]
    public int SupplierId { get; set; }
    
    /// <summary>
    /// Transaction date and time
    /// </summary>
    [Required]
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Type of transaction (payment, credit, adjustment, purchase_order)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string TransactionType { get; set; } = string.Empty;
    
    /// <summary>
    /// Transaction amount
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Balance after this transaction
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal BalanceAfterTransaction { get; set; }
    
    /// <summary>
    /// Reference to related entity (e.g., PurchaseOrder ID, Payment reference)
    /// </summary>
    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }
    
    /// <summary>
    /// Additional notes about the transaction
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// User who created this transaction
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string CreatedByUserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Branch where transaction occurred
    /// </summary>
    [Required]
    public int BranchId { get; set; }
    
    /// <summary>
    /// Navigation property to supplier
    /// </summary>
    [ForeignKey(nameof(SupplierId))]
    public virtual Supplier Supplier { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to branch
    /// </summary>
    [ForeignKey(nameof(BranchId))]
    public virtual Branch Branch { get; set; } = null!;
}
