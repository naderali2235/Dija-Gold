
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents a purchase order from suppliers
/// </summary>
public class PurchaseOrder : BaseEntity
{
    /// <summary>
    /// Purchase order number
    /// </summary>


    public string PurchaseOrderNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Supplier for this purchase order
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
    /// Status ID for the purchase order
    /// </summary>
    public int? StatusId { get; set; }
    
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
    /// Navigation property to purchase order items
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();

    /// <summary>
    /// Navigation property to product ownerships related to this purchase order
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<ProductOwnership> ProductOwnerships { get; set; } = new List<ProductOwnership>();
}
