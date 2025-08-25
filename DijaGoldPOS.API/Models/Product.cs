using DijaGoldPOS.API.Models.LookupTables;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents a product in the catalog (Gold Jewelry, Bullion, or Coins)
/// </summary>
public class Product : BaseEntity
{
    /// <summary>
    /// Product code/SKU for identification
    /// </summary>
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>
    /// Product name/description
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product category type
    /// </summary>
    public int CategoryTypeId { get; set; }

    /// <summary>
    /// Karat type of the gold
    /// </summary>
    public int KaratTypeId { get; set; }

    /// <summary>
    /// Weight in grams (precision to 3 decimal places)
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal Weight { get; set; }

    /// <summary>
    /// Brand name (if branded, otherwise custom-made by merchant)
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Design or style name (for jewelry)
    /// </summary>
    public string? DesignStyle { get; set; }

    /// <summary>
    /// Sub-category ID (foreign key to SubCategoryLookup)
    /// </summary>
    public int? SubCategoryId { get; set; }

    /// <summary>
    /// Legacy sub-category field (for backward compatibility during migration)
    /// </summary>
    public string? SubCategory { get; set; }

    /// <summary>
    /// Shape or form (for bullion: bars, ingots, etc.)
    /// </summary>
    public string? Shape { get; set; }

    /// <summary>
    /// Purity certificate number (if applicable)
    /// </summary>
    public string? PurityCertificateNumber { get; set; }

    /// <summary>
    /// Country of origin (for coins)
    /// </summary>
    public string? CountryOfOrigin { get; set; }

    /// <summary>
    /// Year of minting (for coins)
    /// </summary>
    public int? YearOfMinting { get; set; }

    /// <summary>
    /// Denomination or face value (for coins)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? FaceValue { get; set; }

    /// <summary>
    /// Whether the coin has numismatic value beyond gold content
    /// </summary>
    public bool? HasNumismaticValue { get; set; }

    /// <summary>
    /// Whether making charges apply to this product
    /// </summary>
    public bool MakingChargesApplicable { get; set; } = true;

    /// <summary>
    /// Product-specific making charges type (null if using pricing-level charges)
    /// </summary>
    public int? ProductMakingChargesTypeId { get; set; }

    /// <summary>
    /// Product-specific making charges value (null if using pricing-level charges)
    /// </summary>
    [Column(TypeName = "decimal(10,4)")]
    public decimal? ProductMakingChargesValue { get; set; }

    /// <summary>
    /// Whether to use product-specific making charges instead of pricing-level charges
    /// </summary>
    public bool UseProductMakingCharges { get; set; } = false;

    /// <summary>
    /// Unit price of the product
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? UnitPrice { get; set; }

    /// <summary>
    /// Supplier ID (if purchased from supplier)
    /// </summary>
    public int? SupplierId { get; set; }

    /// <summary>
    /// Navigation property to supplier
    /// </summary>
    [JsonIgnore]
    public virtual Supplier? Supplier { get; set; }

    /// <summary>
    /// Navigation property to inventory records
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<Inventory> InventoryRecords { get; set; } = new List<Inventory>();

    /// <summary>
    /// Navigation property to product category type lookup
    /// </summary>
    [JsonIgnore]
    public virtual ProductCategoryTypeLookup CategoryType { get; set; } = null!;

    /// <summary>
    /// Navigation property to karat type lookup
    /// </summary>
    [JsonIgnore]
    public virtual KaratTypeLookup KaratType { get; set; } = null!;

    /// <summary>
    /// Navigation property to sub-category lookup
    /// </summary>
    [JsonIgnore]
    public virtual SubCategoryLookup? SubCategoryLookup { get; set; }

    /// <summary>
    /// Navigation property to order items
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    /// <summary>
    /// Navigation property to manufacturing records
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<ProductManufacture> ManufacturingRecords { get; set; } = new List<ProductManufacture>();

    /// <summary>
    /// Navigation property to product ownership records
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<ProductOwnership> ProductOwnerships { get; set; } = new List<ProductOwnership>();
}
