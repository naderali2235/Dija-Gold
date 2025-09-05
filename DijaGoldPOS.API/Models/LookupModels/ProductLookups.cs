using DijaGoldPOS.API.Models.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijaGoldPOS.API.Models.LookupModels;

/// <summary>
/// Lookup table for karat types (18K, 21K, 22K, 24K, etc.)
/// </summary>
[Table("KaratTypes", Schema = "Lookup")]
public class KaratTypeLookup : BaseEntity
{
    /// <summary>
    /// Karat value (e.g., 18, 21, 22, 24)
    /// </summary>
    [Column(TypeName = "decimal(4,1)")]
    public decimal KaratValue { get; set; }

    /// <summary>
    /// Purity percentage (e.g., 0.750 for 18K, 0.916 for 22K)
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal PurityPercentage { get; set; }

    /// <summary>
    /// Standard abbreviation (e.g., "18K", "22K")
    /// </summary>
    public string Abbreviation { get; set; } = string.Empty;

    /// <summary>
    /// Whether this karat type is commonly used
    /// </summary>
    public bool IsCommon { get; set; } = true;
    public string Description { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; } = 0;
}

/// <summary>
/// Lookup table for product category types (Jewelry, Bullion, Coins)
/// </summary>
[Table("ProductCategoryTypes", Schema = "Lookup")]
public class ProductCategoryTypeLookup : BaseEntity
{
    /// <summary>
    /// Category code for internal use
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Whether making charges apply to this category
    /// </summary>
    public bool HasMakingCharges { get; set; } = true;

    /// <summary>
    /// Default tax rate for this category
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal? DefaultTaxRate { get; set; }

    /// <summary>
    /// Whether this category requires weight measurement
    /// </summary>
    public bool RequiresWeight { get; set; } = true;
    public string Description { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; } = 0;
}

/// <summary>
/// Lookup table for product sub-categories (Rings, Necklaces, Bracelets, etc.)
/// </summary>
[Table("SubCategories", Schema = "Lookup")]
public class SubCategoryLookup : BaseEntity
{
    /// <summary>
    /// Parent category type ID
    /// </summary>
    public int CategoryTypeId { get; set; }

    /// <summary>
    /// Sub-category code for internal use
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Default making charge rate for this sub-category
    /// </summary>
    [Column(TypeName = "decimal(10,4)")]
    public decimal? DefaultMakingChargeRate { get; set; }

    /// <summary>
    /// Whether this sub-category is for men's items
    /// </summary>
    public bool IsMens { get; set; } = false;

    /// <summary>
    /// Whether this sub-category is for women's items
    /// </summary>
    public bool IsWomens { get; set; } = false;

    /// <summary>
    /// Whether this sub-category is unisex
    /// </summary>
    public bool IsUnisex { get; set; } = true;

    // Navigation Properties
    /// <summary>
    /// Navigation property to parent category type
    /// </summary>
    public virtual ProductCategoryTypeLookup CategoryType { get; set; } = null!;
    public string Description { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; } = 0;
}

/// <summary>
/// Lookup table for charge types (Fixed, Percentage, Per Gram)
/// </summary>
[Table("ChargeTypes", Schema = "Lookup")]
public class ChargeTypeLookup : BaseEntity
{
    /// <summary>
    /// Charge type code (FIXED, PERCENTAGE, PER_GRAM)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Unit of measurement for this charge type
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Whether this charge type is percentage-based
    /// </summary>
    public bool IsPercentage { get; set; } = false;

    /// <summary>
    /// Whether this charge type is weight-based
    /// </summary>
    public bool IsWeightBased { get; set; } = false;
    public string Description { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; } = 0;
}
