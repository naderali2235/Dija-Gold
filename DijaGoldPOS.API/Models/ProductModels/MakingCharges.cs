using DijaGoldPOS.API.Models.LookupModels;
using DijaGoldPOS.API.Models.Shared;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models.ProductModels;

/// <summary>
/// Represents making charges configuration for different product categories
/// </summary>
[Table("MakingCharges", Schema = "Product")]
public class MakingCharges : BaseEntity
{
    /// <summary>
    /// Product category this making charge applies to
    /// </summary>
    public int ProductCategoryId { get; set; }

    /// <summary>
    /// Sub-category this making charge applies to (optional)
    /// </summary>
    public int? SubCategoryId { get; set; }

    /// <summary>
    /// Type of charge (Fixed, Percentage, Per Gram)
    /// </summary>
    public int ChargeTypeId { get; set; }

    /// <summary>
    /// Charge value (amount, percentage, or per-gram rate)
    /// </summary>
    [Column(TypeName = "decimal(10,4)")]
    public decimal ChargeValue { get; set; }

    /// <summary>
    /// Minimum charge amount (for percentage-based charges)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinimumCharge { get; set; }

    /// <summary>
    /// Maximum charge amount (for percentage-based charges)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaximumCharge { get; set; }

    /// <summary>
    /// Effective start date for this making charge
    /// </summary>
    public DateTime EffectiveFrom { get; set; }

    /// <summary>
    /// Effective end date for this making charge (null if current)
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>
    /// Whether this is the current active making charge
    /// </summary>
    public bool IsCurrent { get; set; } = true;

    /// <summary>
    /// Minimum weight threshold for this charge to apply
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal? MinimumWeight { get; set; }

    /// <summary>
    /// Maximum weight threshold for this charge to apply
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal? MaximumWeight { get; set; }

    /// <summary>
    /// Description of this making charge
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Whether this charge is mandatory or optional
    /// </summary>
    public bool IsMandatory { get; set; } = true;

    /// <summary>
    /// Whether this charge can be waived for VIP customers
    /// </summary>
    public bool CanBeWaived { get; set; } = false;

    /// <summary>
    /// Name/title for this making charge configuration
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// SubCategory navigation property (alias for SubCategoryLookup)
    /// </summary>
    [NotMapped]
    public LookupModels.SubCategoryLookup? SubCategory => SubCategoryLookup;

    /// <summary>
    /// Row version for optimistic concurrency control
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation Properties
    /// <summary>
    /// Navigation property to product category
    /// </summary>
    [JsonIgnore]
    public virtual ProductCategoryTypeLookup ProductCategory { get; set; } = null!;

    /// <summary>
    /// Navigation property to charge type
    /// </summary>
    [JsonIgnore]
    public virtual ChargeTypeLookup ChargeType { get; set; } = null!;

    /// <summary>
    /// Navigation property to sub-category lookup
    /// </summary>
    [JsonIgnore]
    public virtual SubCategoryLookup? SubCategoryLookup { get; set; }
}
