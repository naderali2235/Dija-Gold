using DijaGoldPOS.API.Models.LookupTables;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents making charges configuration with versioning
/// </summary>
public class MakingCharges : BaseEntity
{
    /// <summary>
    /// Name/description of the making charge
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Product category this charge applies to
    /// </summary>
    [Required]
    public int ProductCategoryId { get; set; }
    
    /// <summary>
    /// Sub-category ID (foreign key to SubCategoryLookup)
    /// </summary>
    public int? SubCategoryId { get; set; }
    
    /// <summary>
    /// Legacy sub-category field (for backward compatibility during migration)
    /// </summary>
    [MaxLength(50)]
    public string? SubCategory { get; set; }
    
    /// <summary>
    /// Type of charge (percentage or fixed amount)
    /// </summary>
    [Required]
    public int ChargeTypeId { get; set; }
    
    /// <summary>
    /// Charge value (percentage or fixed amount in EGP)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10,4)")]
    public decimal ChargeValue { get; set; }
    
    /// <summary>
    /// Effective start date for this charge
    /// </summary>
    [Required]
    public DateTime EffectiveFrom { get; set; }
    
    /// <summary>
    /// Effective end date for this charge (null if current)
    /// </summary>
    public DateTime? EffectiveTo { get; set; }
    
    /// <summary>
    /// Whether this is the current active charge
    /// </summary>
    public bool IsCurrent { get; set; } = true;
    
    /// <summary>
    /// Navigation property to product category type lookup
    /// </summary>
    public virtual ProductCategoryTypeLookup ProductCategory { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to charge type lookup
    /// </summary>
    public virtual ChargeTypeLookup ChargeType { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to sub-category lookup
    /// </summary>
    public virtual SubCategoryLookup? SubCategoryLookup { get; set; }
    
    /// <summary>
    /// Navigation property to order items using this charge
    /// </summary>
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}