using DijaGoldPOS.API.Models.LookupModels;
using DijaGoldPOS.API.Models.Shared;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models.ProductModels;

/// <summary>
/// Represents tax configuration for different tax types
/// </summary>
[Table("TaxConfigurations", Schema = "Product")]
public class TaxConfiguration : BaseEntity
{
    /// <summary>
    /// Unique tax code for identification
    /// </summary>
    public string TaxCode { get; set; } = string.Empty;

    /// <summary>
    /// Tax name/description
    /// </summary>
    public string TaxName { get; set; } = string.Empty;

    /// <summary>
    /// Tax type ID (VAT, Sales Tax, etc.)
    /// </summary>
    public int TaxTypeId { get; set; }

    /// <summary>
    /// Tax rate (as decimal, e.g., 0.14 for 14%)
    /// </summary>
    [Column(TypeName = "decimal(10,4)")]
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Effective start date for this tax configuration
    /// </summary>
    public DateTime EffectiveFrom { get; set; }

    /// <summary>
    /// Effective end date for this tax configuration (null if current)
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>
    /// Whether this is the current active tax configuration
    /// </summary>
    public bool IsCurrent { get; set; } = true;

    /// <summary>
    /// Product category this tax applies to (null for all categories)
    /// </summary>
    public int? ProductCategoryId { get; set; }

    /// <summary>
    /// Minimum transaction amount for this tax to apply
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinimumTransactionAmount { get; set; }

    /// <summary>
    /// Maximum transaction amount for this tax to apply
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaximumTransactionAmount { get; set; }

    /// <summary>
    /// Whether this tax is inclusive (included in price) or exclusive (added to price)
    /// </summary>
    public bool IsInclusive { get; set; } = false;

    /// <summary>
    /// Whether this tax is mandatory or optional
    /// </summary>
    public bool IsMandatory { get; set; } = true;

    /// <summary>
    /// Tax authority or jurisdiction
    /// </summary>
    public string? TaxAuthority { get; set; }

    /// <summary>
    /// Tax registration number with authority
    /// </summary>
    public string? TaxRegistrationNumber { get; set; }

    /// <summary>
    /// Description of this tax configuration
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Display order for UI sorting
    /// </summary>
    public int? DisplayOrder { get; set; }

    /// <summary>
    /// Row version for optimistic concurrency control
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation Properties
    /// <summary>
    /// Navigation property to tax type lookup
    /// </summary>
    [JsonIgnore]
    public virtual TransactionTypeLookup TaxType { get; set; } = null!;

    /// <summary>
    /// Navigation property to product category (if specific)
    /// </summary>
    [JsonIgnore]
    public virtual ProductCategoryTypeLookup? ProductCategory { get; set; }
}
