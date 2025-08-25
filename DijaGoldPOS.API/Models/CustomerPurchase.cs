
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DijaGoldPOS.API.Models.LookupTables;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents purchases from customers (individuals selling their gold to the merchant)
/// </summary>
public class CustomerPurchase : BaseEntity
{
    /// <summary>
    /// Unique purchase number
    /// </summary>


    public string PurchaseNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Customer selling the products
    /// </summary>

    public int CustomerId { get; set; }
    
    /// <summary>
    /// Branch where purchase occurred
    /// </summary>

    public int BranchId { get; set; }
    
    /// <summary>
    /// Purchase date
    /// </summary>

    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Total purchase amount
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Amount paid to customer
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; }
    
    /// <summary>
    /// Payment method used
    /// </summary>

    public int PaymentMethodId { get; set; }
    
    /// <summary>
    /// Notes about the purchase
    /// </summary>

    public string? Notes { get; set; }
    
    /// <summary>
    /// User who created this purchase
    /// </summary>


    public string CreatedByUserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Navigation property to customer
    /// </summary>
    [JsonIgnore]
    public virtual Customer Customer { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to branch
    /// </summary>
    [JsonIgnore]
    public virtual Branch Branch { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to payment method lookup
    /// </summary>
    [JsonIgnore]
    public virtual PaymentMethodLookup PaymentMethod { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to purchase items
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<CustomerPurchaseItem> CustomerPurchaseItems { get; set; } = new List<CustomerPurchaseItem>();
    
    /// <summary>
    /// Navigation property to product ownership records
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<ProductOwnership> ProductOwnerships { get; set; } = new List<ProductOwnership>();
}
