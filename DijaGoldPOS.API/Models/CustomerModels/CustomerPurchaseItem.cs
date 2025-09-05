using DijaGoldPOS.API.Models.LookupModels;
using DijaGoldPOS.API.Models.Shared;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models.CustomerModels;

/// <summary>
/// Represents an individual item in a customer purchase transaction
/// </summary>
[Table("CustomerPurchaseItems", Schema = "Customer")]
public class CustomerPurchaseItem : BaseEntity
{
    /// <summary>
    /// Reference to the parent purchase
    /// </summary>
    public int CustomerPurchaseId { get; set; }

    /// <summary>
    /// Item sequence number within the purchase
    /// </summary>
    public int ItemSequence { get; set; }

    /// <summary>
    /// Karat type of this gold item
    /// </summary>
    public int KaratTypeId { get; set; }

    /// <summary>
    /// Weight of this item in grams
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal Weight { get; set; }

    /// <summary>
    /// Actual tested purity of this item
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal ActualPurity { get; set; }

    /// <summary>
    /// Adjusted weight based on purity testing
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal AdjustedWeight { get; set; }

    /// <summary>
    /// Unit price per gram for this item
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Total amount for this item
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Description of the gold item
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Item type (Jewelry, Scrap, Coins, Bars)
    /// </summary>
    public string ItemType { get; set; } = "Jewelry";

    /// <summary>
    /// Condition of the item (Excellent, Good, Fair, Poor)
    /// </summary>
    public string Condition { get; set; } = "Good";

    /// <summary>
    /// Whether stones were removed from jewelry
    /// </summary>
    public bool StonesRemoved { get; set; } = false;

    /// <summary>
    /// Weight of stones removed (if any)
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal? StonesWeight { get; set; }

    /// <summary>
    /// Value of stones removed (if any)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? StonesValue { get; set; }

    /// <summary>
    /// Deductions applied to this item
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Deductions { get; set; } = 0;

    /// <summary>
    /// Reason for deductions
    /// </summary>
    public string? DeductionReason { get; set; }

    /// <summary>
    /// Testing notes for this specific item
    /// </summary>
    public string? TestingNotes { get; set; }

    /// <summary>
    /// Additional notes about this item
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Photos of this specific item (JSON array)
    /// </summary>
    public string? Photos { get; set; }

    /// <summary>
    /// Whether this item was melted down
    /// </summary>
    public bool WasMelted { get; set; } = false;

    /// <summary>
    /// Melting date if applicable
    /// </summary>
    public DateTime? MeltingDate { get; set; }

    /// <summary>
    /// Melting loss percentage
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal? MeltingLoss { get; set; }

    /// <summary>
    /// Final weight after melting
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal? FinalWeight { get; set; }

    // Navigation Properties
    /// <summary>
    /// Parent customer purchase
    /// </summary>
    [JsonIgnore]
    public virtual CustomerPurchase CustomerPurchase { get; set; } = null!;

    /// <summary>
    /// Karat type of this item
    /// </summary>
    [JsonIgnore]
    public virtual KaratTypeLookup KaratType { get; set; } = null!;
}
