using DijaGoldPOS.API.Models.Enums;
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
    public ProductCategoryType ProductCategory { get; set; }
    
    /// <summary>
    /// Specific subcategory (optional, e.g., "rings", "necklaces")
    /// </summary>
    [MaxLength(50)]
    public string? SubCategory { get; set; }
    
    /// <summary>
    /// Type of charge (percentage or fixed amount)
    /// </summary>
    [Required]
    public ChargeType ChargeType { get; set; }
    
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
    /// Navigation property to transaction items using this charge
    /// </summary>
    public virtual ICollection<TransactionItem> TransactionItems { get; set; } = new List<TransactionItem>();
}