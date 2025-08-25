
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents an item in a sales order
/// </summary>
public class OrderItem : BaseEntity
{
    /// <summary>
    /// Order ID this item belongs to
    /// </summary>

    public int OrderId { get; set; }
    
    /// <summary>
    /// Product ID
    /// </summary>

    public int ProductId { get; set; }
    
    /// <summary>
    /// Quantity ordered
    /// </summary>


    public decimal Quantity { get; set; }
    
    /// <summary>
    /// Unit price at time of order
    /// </summary>

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// Total price for this item (UnitPrice * Quantity)
    /// </summary>

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }
    
    /// <summary>
    /// Discount percentage applied to this item
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal DiscountPercentage { get; set; }
    
    /// <summary>
    /// Discount amount applied to this item
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; }
    
    /// <summary>
    /// Final price after discount
    /// </summary>

    [Column(TypeName = "decimal(18,2)")]
    public decimal FinalPrice { get; set; }
    
    /// <summary>
    /// Making charges for this item
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal MakingCharges { get; set; }
    
    /// <summary>
    /// Tax amount for this item
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }
    
    /// <summary>
    /// Total amount for this item (FinalPrice + MakingCharges + TaxAmount)
    /// </summary>

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Additional notes for this item
    /// </summary>

    public string? Notes { get; set; }
    
    /// <summary>
    /// Navigation property to order
    /// </summary>
    [JsonIgnore]
    public virtual Order Order { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to product
    /// </summary>
    [JsonIgnore]
    public virtual Product Product { get; set; } = null!;
}
