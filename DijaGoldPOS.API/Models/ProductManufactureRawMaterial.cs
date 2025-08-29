using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents raw materials used in a manufacturing operation
/// Supports multiple raw material sources for a single product manufacture
/// </summary>
public class ProductManufactureRawMaterial : BaseEntity
{
    /// <summary>
    /// The manufacturing record this raw material belongs to
    /// </summary>
    public int ProductManufactureId { get; set; }

    /// <summary>
    /// The raw gold purchase order item used as source material
    /// </summary>
    public int RawGoldPurchaseOrderItemId { get; set; }

    /// <summary>
    /// Weight of raw gold consumed from this source (in grams)
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal ConsumedWeight { get; set; }

    /// <summary>
    /// Weight lost from this source during manufacturing (wastage) in grams
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal WastageWeight { get; set; } = 0;

    /// <summary>
    /// Cost per gram of this raw material at time of consumption
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal CostPerGram { get; set; }

    /// <summary>
    /// Total cost of raw material consumed from this source
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalRawMaterialCost { get; set; }

    /// <summary>
    /// Percentage of total raw material this source contributed
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal ContributionPercentage { get; set; }

    /// <summary>
    /// Sequence order when multiple sources are used
    /// </summary>
    public int SequenceOrder { get; set; } = 1;

    /// <summary>
    /// Notes about this raw material usage
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Navigation property to the manufacturing record
    /// </summary>
    [JsonIgnore]
    public virtual ProductManufacture ProductManufacture { get; set; } = null!;

    /// <summary>
    /// Navigation property to the raw gold purchase order item
    /// </summary>
    [JsonIgnore]
    public virtual RawGoldPurchaseOrderItem RawGoldPurchaseOrderItem { get; set; } = null!;
}
