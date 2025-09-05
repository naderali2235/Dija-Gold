using DijaGoldPOS.API.Models.InventoryModels;
using DijaGoldPOS.API.Models.LookupModels;
using DijaGoldPOS.API.Models.ManfacturingModels;
using DijaGoldPOS.API.Models.SalesModels;
using DijaGoldPOS.API.Models.Shared;
using DijaGoldPOS.API.Models.SupplierModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models.ProductModels;

/// <summary>
/// Represents a product in the catalog (Gold Jewelry, Bullion, or Coins)
/// </summary>
[Table("Products", Schema = "Product")]
public class Product : BaseEntity
{
    /// <summary>
    /// Product code/SKU for identification (unique among active products only)
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
    /// Year of manufacture/minting
    /// </summary>
    public int? ManufactureYear { get; set; }

    /// <summary>
    /// Supplier who provided this product
    /// </summary>
    public int? SupplierId { get; set; }

    /// <summary>
    /// Minimum stock level for reorder alerts
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal? MinimumStockLevel { get; set; }

    /// <summary>
    /// Maximum stock level for inventory management
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal? MaximumStockLevel { get; set; }

    /// <summary>
    /// Reorder point for automatic purchase orders
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal? ReorderPoint { get; set; }

    /// <summary>
    /// Standard cost per unit for costing purposes
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? StandardCost { get; set; }

    /// <summary>
    /// Current selling price per unit
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? CurrentPrice { get; set; }

    /// <summary>
    /// Product images (JSON array of image URLs/paths)
    /// </summary>
    public string? Images { get; set; }

    /// <summary>
    /// Additional product specifications (JSON format)
    /// </summary>
    public string? Specifications { get; set; }

    /// <summary>
    /// Internal notes about the product
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Barcode for the product
    /// </summary>
    public string? Barcode { get; set; }

    /// <summary>
    /// Whether this product is available for sale
    /// </summary>
    public bool IsAvailableForSale { get; set; } = true;

    /// <summary>
    /// Whether this product is featured/highlighted
    /// </summary>
    public bool IsFeatured { get; set; } = false;

    /// <summary>
    /// Unit price for this product (alias for CurrentPrice)
    /// </summary>
    [NotMapped]
    public decimal? UnitPrice => CurrentPrice;

    /// <summary>
    /// Cost price of the product (alias for StandardCost)
    /// </summary>
    [NotMapped]
    public decimal? CostPrice => StandardCost;

    /// <summary>
    /// Row version for optimistic concurrency control
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    /// <summary>
    /// Whether making charges are applicable to this product
    /// </summary>
    public bool MakingChargesApplicable { get; set; } = true;

    /// <summary>
    /// Whether to use product-specific making charges instead of category-based
    /// </summary>
    public bool UseProductMakingCharges { get; set; } = false;

    /// <summary>
    /// Product-specific making charges type ID
    /// </summary>
    public int? ProductMakingChargesTypeId { get; set; }

    /// <summary>
    /// Product-specific making charges value
    /// </summary>
    [Column(TypeName = "decimal(10,4)")]
    public decimal? ProductMakingChargesValue { get; set; }

    /// <summary>
    /// Sub-category lookup navigation property (alias for SubCategoryLookup)
    /// </summary>
    [NotMapped]
    public SubCategoryLookup? SubCategory => SubCategoryLookup;

    /// <summary>
    /// Year of minting (for coins)
    /// </summary>
    public int? YearOfMinting { get; set; }

    /// <summary>
    /// Face value (for coins)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? FaceValue { get; set; }

    /// <summary>
    /// Whether this item has numismatic value beyond gold content
    /// </summary>
    public bool HasNumismaticValue { get; set; } = false;

    // Navigation Properties
    /// <summary>
    /// Navigation property to supplier
    /// </summary>
    [JsonIgnore]
    public virtual Supplier? Supplier { get; set; }

    /// <summary>
    /// Navigation property to category type lookup
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
    /// Navigation property to inventory records
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<Inventory> InventoryRecords { get; set; } = new List<Inventory>();

    /// <summary>
    /// Navigation property to manufacturing records
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<ProductManufacture> ManufacturingRecords { get; set; } = new List<ProductManufacture>();

    /// <summary>
    /// Navigation property to product ownerships
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<ProductOwnership> ProductOwnerships { get; set; } = new List<ProductOwnership>();
}
