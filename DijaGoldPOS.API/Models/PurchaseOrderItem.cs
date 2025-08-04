using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents an individual item within a purchase order
/// </summary>
public class PurchaseOrderItem : BaseEntity
{
    /// <summary>
    /// Purchase order this item belongs to
    /// </summary>
    [Required]
    public int PurchaseOrderId { get; set; }
    
    /// <summary>
    /// Product being purchased
    /// </summary>
    [Required]
    public int ProductId { get; set; }
    
    /// <summary>
    /// Quantity ordered
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal QuantityOrdered { get; set; }
    
    /// <summary>
    /// Quantity received
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal QuantityReceived { get; set; } = 0;
    
    /// <summary>
    /// Weight ordered in grams
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal WeightOrdered { get; set; }
    
    /// <summary>
    /// Weight received in grams
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal WeightReceived { get; set; } = 0;
    
    /// <summary>
    /// Unit cost per gram or per piece
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitCost { get; set; }
    
    /// <summary>
    /// Total line amount
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }
    
    /// <summary>
    /// Item status
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Received, Cancelled
    
    /// <summary>
    /// Notes about this item
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Navigation property to purchase order
    /// </summary>
    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to product
    /// </summary>
    public virtual Product Product { get; set; } = null!;
}