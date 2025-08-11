using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents an individual item within a transaction
/// </summary>
public class TransactionItem : BaseEntity
{
    /// <summary>
    /// Transaction this item belongs to
    /// </summary>
    [Required]
    public int TransactionId { get; set; }
    
    /// <summary>
    /// Product being sold/returned
    /// </summary>
    [Required]
    public int ProductId { get; set; }
    
    /// <summary>
    /// Quantity of the product
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal Quantity { get; set; } = 1;
    
    /// <summary>
    /// Weight per unit in grams
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal UnitWeight { get; set; }
    
    /// <summary>
    /// Total weight (Quantity × UnitWeight)
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal TotalWeight { get; set; }
    
    /// <summary>
    /// Gold rate per gram at time of transaction
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal GoldRatePerGram { get; set; }
    
    /// <summary>
    /// Unit price (calculated from gold rate and weight)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// Making charges ID applied to this item
    /// </summary>
    public int? MakingChargesId { get; set; }
    
    /// <summary>
    /// Making charges amount for this item
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal MakingChargesAmount { get; set; }
    
    /// <summary>
    /// Discount percentage applied to this item
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal DiscountPercentage { get; set; } = 0;
    
    /// <summary>
    /// Discount amount applied to this item
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; } = 0;
    
    /// <summary>
    /// Line total before taxes (UnitPrice × Quantity + MakingCharges - Discount)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }
    
    /// <summary>
    /// Weight (alias for TotalWeight)
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal Weight 
    { 
        get => TotalWeight; 
        set => TotalWeight = value; 
    }
    
    /// <summary>
    /// Total price (alias for LineTotal)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice 
    { 
        get => LineTotal; 
        set => LineTotal = value; 
    }
    
    /// <summary>
    /// Navigation property to transaction
    /// </summary>
    public virtual Transaction Transaction { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to product
    /// </summary>
    public virtual Product Product { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to making charges
    /// </summary>
    public virtual MakingCharges? MakingCharges { get; set; }
}