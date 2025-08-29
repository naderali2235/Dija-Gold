
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DijaGoldPOS.API.Models.LookupTables;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents an individual item within a purchase order
/// </summary>
public class PurchaseOrderItem : BaseEntity
{
    /// <summary>
    /// Purchase order this item belongs to
    /// </summary>

    public int PurchaseOrderId { get; set; }

    /// <summary>
    /// Product being purchased
    /// </summary>
    //[Required]
    public int ProductId { get; set; } = 1;

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
    public string Status { get; set; } = "Pending"; // Pending, Received, Cancelled

    /// <summary>
    /// Status ID for the purchase order item
    /// </summary>
    public int? StatusId { get; set; }
    
    /// <summary>
    /// Notes about this item
    /// </summary>

    public string? Notes { get; set; }
    
    /// <summary>
    /// Navigation property to purchase order
    /// </summary>
    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to product
    /// </summary>
    public virtual Product Product { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to manufacturing records where this item was referenced as additional material
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<ProductManufacture> AdditionalManufacturingRecords { get; set; } = new List<ProductManufacture>();
}
