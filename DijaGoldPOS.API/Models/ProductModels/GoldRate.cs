using DijaGoldPOS.API.Models.LookupModels;
using DijaGoldPOS.API.Models.SalesModels;
using DijaGoldPOS.API.Models.Shared;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models.ProductModels;

/// <summary>
/// Represents daily gold rates with versioning for different karat types
/// </summary>
[Table("GoldRates", Schema = "Product")]
public class GoldRate : BaseEntity
{
    /// <summary>
    /// Karat type for this rate
    /// </summary>
    public int KaratTypeId { get; set; }
    
    /// <summary>
    /// Rate per gram in Egyptian Pounds
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal RatePerGram { get; set; }
    
    /// <summary>
    /// Effective start date and time for this rate
    /// </summary>
    public DateTime EffectiveFrom { get; set; }
    
    /// <summary>
    /// Effective end date and time for this rate (null if current)
    /// </summary>
    public DateTime? EffectiveTo { get; set; }
    
    /// <summary>
    /// Whether this is the current active rate for the karat type
    /// </summary>
    public bool IsCurrent { get; set; } = true;

    /// <summary>
    /// Source of the rate (Manual, API, Market)
    /// </summary>
    public string Source { get; set; } = "Manual";

    /// <summary>
    /// Reference number or identifier from the source
    /// </summary>
    public string? SourceReference { get; set; }

    /// <summary>
    /// Additional notes about this rate
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// User who set this rate
    /// </summary>
    public string? SetByUserId { get; set; }

    /// <summary>
    /// User who approved this rate (if approval required)
    /// </summary>
    public string? ApprovedByUserId { get; set; }

    /// <summary>
    /// Date when this rate was approved
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Row version for optimistic concurrency control
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation Properties
    /// <summary>
    /// Navigation property to karat type lookup
    /// </summary>
    [JsonIgnore]
    public virtual KaratTypeLookup KaratType { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to orders using this rate
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    /// <summary>
    /// Navigation property to user who set this rate
    /// </summary>
    [JsonIgnore]
    public virtual ApplicationUser? SetByUser { get; set; }

    /// <summary>
    /// Navigation property to user who approved this rate
    /// </summary>
    [JsonIgnore]
    public virtual ApplicationUser? ApprovedByUser { get; set; }
}
