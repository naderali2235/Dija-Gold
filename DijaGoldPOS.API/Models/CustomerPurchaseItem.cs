
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DijaGoldPOS.API.Models.LookupTables;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents individual gold amounts within a customer purchase
/// </summary>
public class CustomerPurchaseItem : BaseEntity
{
    /// <summary>
    /// Customer purchase this item belongs to
    /// </summary>

    public int CustomerPurchaseId { get; set; }

    /// <summary>
    /// Karat type of the gold being purchased
    /// </summary>

    public int KaratTypeId { get; set; }

    /// <summary>
    /// Weight purchased in grams
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal Weight { get; set; }

    /// <summary>
    /// Unit price paid to customer per gram
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Total amount for this gold item
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Notes about this gold item
    /// </summary>

    public string? Notes { get; set; }

    /// <summary>
    /// Navigation property to customer purchase
    /// </summary>
    [JsonIgnore]
    public virtual CustomerPurchase CustomerPurchase { get; set; } = null!;

    /// <summary>
    /// Navigation property to karat type lookup
    /// </summary>
    [JsonIgnore]
    public virtual KaratTypeLookup KaratType { get; set; } = null!;
}
