using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DijaGoldPOS.API.Models.BranchModels;
using DijaGoldPOS.API.Models.CustomerModels;
using DijaGoldPOS.API.Models.LookupTables;
using DijaGoldPOS.API.Models.Shared;
using DijaGoldPOS.API.Models.SupplierModels;

namespace DijaGoldPOS.API.Models.OwneShipModels;

/// <summary>
/// Represents transfer/waiving of raw gold between merchant and suppliers
/// </summary>
public class RawGoldTransfer : BaseEntity
{
    /// <summary>
    /// Transfer number for tracking
    /// </summary>
    public string TransferNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Branch where transfer occurred
    /// </summary>
    public int BranchId { get; set; }
    
    /// <summary>
    /// Source supplier ID (null if from merchant's own gold)
    /// </summary>
    public int? FromSupplierId { get; set; }
    
    /// <summary>
    /// Target supplier ID (null if to merchant's own gold)
    /// </summary>
    public int? ToSupplierId { get; set; }
    
    /// <summary>
    /// Source karat type
    /// </summary>
    public int FromKaratTypeId { get; set; }
    
    /// <summary>
    /// Target karat type (can be different for conversions)
    /// </summary>
    public int ToKaratTypeId { get; set; }
    
    /// <summary>
    /// Weight being transferred from source (in grams)
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal FromWeight { get; set; }
    
    /// <summary>
    /// Weight being transferred to target (in grams) - may differ due to karat conversion
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal ToWeight { get; set; }
    
    /// <summary>
    /// Gold rate per gram for source karat at time of transfer
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal FromGoldRate { get; set; }
    
    /// <summary>
    /// Gold rate per gram for target karat at time of transfer
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ToGoldRate { get; set; }
    
    /// <summary>
    /// Conversion factor used (ToWeight / FromWeight)
    /// </summary>
    [Column(TypeName = "decimal(8,6)")]
    public decimal ConversionFactor { get; set; }
    
    /// <summary>
    /// Total value of the transfer
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TransferValue { get; set; }
    
    /// <summary>
    /// Transfer date
    /// </summary>
    public DateTime TransferDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Transfer type: Waive (reduce debt), Credit (create balance), Convert (karat change)
    /// </summary>
    public string TransferType { get; set; } = "Waive"; // Waive, Credit, Convert
    
    /// <summary>
    /// Reference to customer purchase if this transfer is from customer gold
    /// </summary>
    public int? CustomerPurchaseId { get; set; }
    
    /// <summary>
    /// Notes about the transfer
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// User who created this transfer
    /// </summary>
    public string CreatedByUserId { get; set; } = string.Empty;
    
    // Navigation properties
    [JsonIgnore]
    public virtual Branch Branch { get; set; } = null!;
    
    [JsonIgnore]
    public virtual Supplier? FromSupplier { get; set; }
    
    [JsonIgnore]
    public virtual Supplier? ToSupplier { get; set; }
    
    [JsonIgnore]
    public virtual KaratTypeLookup FromKaratType { get; set; } = null!;
    
    [JsonIgnore]
    public virtual KaratTypeLookup ToKaratType { get; set; } = null!;
    
    [JsonIgnore]
    public virtual CustomerPurchase? CustomerPurchase { get; set; }
}
