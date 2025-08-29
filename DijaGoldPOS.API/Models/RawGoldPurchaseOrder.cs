using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents a purchase order specifically for raw gold materials
/// </summary>
public class RawGoldPurchaseOrder : BaseEntity
{
    /// <summary>
    /// Raw gold purchase order number
    /// </summary>
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Supplier for this raw gold purchase order
    /// </summary>
    public int SupplierId { get; set; }
    
    /// <summary>
    /// Branch requesting the purchase
    /// </summary>
    public int BranchId { get; set; }
    
    /// <summary>
    /// Purchase order date
    /// </summary>
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Expected delivery date
    /// </summary>
    public DateTime? ExpectedDeliveryDate { get; set; }
    
    /// <summary>
    /// Actual delivery date
    /// </summary>
    public DateTime? ActualDeliveryDate { get; set; }
    
    /// <summary>
    /// Total order amount
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Amount paid so far
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; } = 0;
    
    /// <summary>
    /// Outstanding balance
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingBalance { get; set; }
    
    /// <summary>
    /// Order status
    /// </summary>
    public string Status { get; set; } = "Pending"; // Pending, Received, Cancelled
    
    /// <summary>
    /// Payment status
    /// </summary>
    public string PaymentStatus { get; set; } = "Unpaid"; // Unpaid, Partial, Paid
    
    /// <summary>
    /// Special terms and conditions
    /// </summary>
    public string? Terms { get; set; }
    
    /// <summary>
    /// Notes about the purchase order
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Total weight of raw gold ordered in grams
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal TotalWeightOrdered { get; set; }

    /// <summary>
    /// Total weight of raw gold received in grams
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal TotalWeightReceived { get; set; }
    
    /// <summary>
    /// Navigation property to supplier
    /// </summary>
    [JsonIgnore]
    public virtual Supplier Supplier { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to branch
    /// </summary>
    [JsonIgnore]
    public virtual Branch Branch { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to raw gold purchase order items
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<RawGoldPurchaseOrderItem> RawGoldPurchaseOrderItems { get; set; } = new List<RawGoldPurchaseOrderItem>();
}
