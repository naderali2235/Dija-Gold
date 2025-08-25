
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents individual items within a customer purchase
/// </summary>
public class CustomerPurchaseItem : BaseEntity
{
    /// <summary>
    /// Customer purchase this item belongs to
    /// </summary>

    public int CustomerPurchaseId { get; set; }
    
    /// <summary>
    /// Product being purchased from customer
    /// </summary>

    public int ProductId { get; set; }
    
    /// <summary>
    /// Quantity purchased
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// Weight purchased in grams
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal Weight { get; set; }
    
    /// <summary>
    /// Unit price paid to customer
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// Total amount for this item
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Notes about this item
    /// </summary>

    public string? Notes { get; set; }
    
    /// <summary>
    /// Navigation property to customer purchase
    /// </summary>
    [JsonIgnore]
    public virtual CustomerPurchase CustomerPurchase { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to product
    /// </summary>
    [JsonIgnore]
    public virtual Product Product { get; set; } = null!;
}
