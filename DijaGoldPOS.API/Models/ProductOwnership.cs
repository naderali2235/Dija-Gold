
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents ownership tracking for products/inventory with partial payment scenarios
/// </summary>
public class ProductOwnership : BaseEntity
{
    /// <summary>
    /// Product being tracked
    /// </summary>

    public int ProductId { get; set; }
    
    /// <summary>
    /// Branch where inventory is held
    /// </summary>

    public int BranchId { get; set; }
    
    /// <summary>
    /// Supplier ID (null for merchant-owned products or customer purchases)
    /// </summary>
    public int? SupplierId { get; set; }
    
    /// <summary>
    /// Purchase order ID if this ownership is from a purchase order
    /// </summary>
    public int? PurchaseOrderId { get; set; }
    
    /// <summary>
    /// Customer purchase ID if this ownership is from buying from customers
    /// </summary>
    public int? CustomerPurchaseId { get; set; }
    
    /// <summary>
    /// Total quantity received/purchased
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal TotalQuantity { get; set; }
    
    /// <summary>
    /// Total weight received/purchased in grams
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal TotalWeight { get; set; }
    
    /// <summary>
    /// Quantity that the merchant actually owns (based on payments made)
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal OwnedQuantity { get; set; }
    
    /// <summary>
    /// Weight that the merchant actually owns (based on payments made)
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal OwnedWeight { get; set; }
    
    /// <summary>
    /// Calculated ownership percentage (OwnedQuantity / TotalQuantity)
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal OwnershipPercentage { get; set; }
    
    /// <summary>
    /// Total cost of the inventory
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCost { get; set; }
    
    /// <summary>
    /// Amount paid to supplier/customer
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; }
    
    /// <summary>
    /// Outstanding amount owed
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingAmount { get; set; }

    /// <summary>
    /// Additional notes about this ownership record
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Whether this ownership record is active
    /// </summary>
    public new bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Navigation property to product
    /// </summary>
    [JsonIgnore]
    public virtual Product Product { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to branch
    /// </summary>
    [JsonIgnore]
    public virtual Branch Branch { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to supplier
    /// </summary>
    [JsonIgnore]
    public virtual Supplier? Supplier { get; set; }
    
    /// <summary>
    /// Navigation property to purchase order
    /// </summary>
    [JsonIgnore]
    public virtual PurchaseOrder? PurchaseOrder { get; set; }
    
    /// <summary>
    /// Navigation property to customer purchase
    /// </summary>
    [JsonIgnore]
    public virtual CustomerPurchase? CustomerPurchase { get; set; }
    
    /// <summary>
    /// Navigation property to ownership movements
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<OwnershipMovement> OwnershipMovements { get; set; } = new List<OwnershipMovement>();
}
