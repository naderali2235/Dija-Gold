using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DijaGoldPOS.API.Models.LookupTables;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents an individual raw gold item within a raw gold purchase order
/// </summary>
public class RawGoldPurchaseOrderItem : BaseEntity
{
    /// <summary>
    /// Raw gold purchase order this item belongs to
    /// </summary>
    public int RawGoldPurchaseOrderId { get; set; }

    /// <summary>
    /// Karat type of the raw gold
    /// </summary>
    public int KaratTypeId { get; set; }

    /// <summary>
    /// Description of the raw gold (e.g., "24K Gold Bars", "22K Gold Scrap")
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Weight ordered in grams
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal WeightOrdered { get; set; }
    
    /// <summary>
    /// Weight received in grams
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal WeightReceived { get; set; } = 0;

    /// <summary>
    /// Weight consumed in manufacturing
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal WeightConsumedInManufacturing { get; set; } = 0;

    /// <summary>
    /// Available weight for manufacturing (WeightReceived - WeightConsumedInManufacturing)
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    private decimal _availableWeightForManufacturing;
    
    public decimal AvailableWeightForManufacturing 
    {
        get => _availableWeightForManufacturing > 0 ? _availableWeightForManufacturing : WeightReceived - WeightConsumedInManufacturing;
        set => _availableWeightForManufacturing = value;
    }
    
    /// <summary>
    /// Unit cost per gram
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitCostPerGram { get; set; }
    
    /// <summary>
    /// Total line amount (WeightOrdered * UnitCostPerGram)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }
    
    /// <summary>
    /// Item status
    /// </summary>
    public string Status { get; set; } = "Pending"; // Pending, Received, Cancelled

    /// <summary>
    /// Purity percentage (e.g., 99.9 for 24K)
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal? PurityPercentage { get; set; }

    /// <summary>
    /// Certificate number if applicable
    /// </summary>
    public string? CertificateNumber { get; set; }

    /// <summary>
    /// Source or origin of the raw gold
    /// </summary>
    public string? Source { get; set; }
    
    /// <summary>
    /// Notes about this raw gold item
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Navigation property to raw gold purchase order
    /// </summary>
    [JsonIgnore]
    public virtual RawGoldPurchaseOrder RawGoldPurchaseOrder { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to karat type
    /// </summary>
    [JsonIgnore]
    public virtual KaratTypeLookup KaratType { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to manufacturing records where this raw gold was used as source material
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<ProductManufacture> SourceManufacturingRecords { get; set; } = new List<ProductManufacture>();
}
