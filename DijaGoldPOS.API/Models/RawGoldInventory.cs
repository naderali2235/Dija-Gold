using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DijaGoldPOS.API.Models.LookupTables;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents raw gold inventory tracking separate from finished products
/// </summary>
public class RawGoldInventory : BaseEntity
{
    /// <summary>
    /// Branch where raw gold is stored
    /// </summary>
    public int BranchId { get; set; }

    /// <summary>
    /// Karat type of the raw gold
    /// </summary>
    public int KaratTypeId { get; set; }

    /// <summary>
    /// Total weight on hand in grams
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal WeightOnHand { get; set; }

    /// <summary>
    /// Weight reserved for manufacturing orders
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal WeightReserved { get; set; } = 0;

    /// <summary>
    /// Available weight for use (WeightOnHand - WeightReserved)
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal AvailableWeight => WeightOnHand - WeightReserved;

    /// <summary>
    /// Weight available for manufacturing (alias for compatibility)
    /// </summary>
    [NotMapped]
    public decimal WeightAvailable
    {
        get => AvailableWeight;
        set { /* This is a calculated property, setter included for compatibility */ }
    }

    /// <summary>
    /// Minimum stock level for reordering
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal MinimumStockLevel { get; set; } = 0;

    /// <summary>
    /// Maximum stock level
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal MaximumStockLevel { get; set; } = 0;

    /// <summary>
    /// Reorder point
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal ReorderPoint { get; set; } = 0;

    /// <summary>
    /// Average cost per gram (weighted average)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal AverageCostPerGram { get; set; } = 0;

    /// <summary>
    /// Total value of inventory (WeightOnHand * AverageCostPerGram)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    private decimal _totalValue;
    
    public decimal TotalValue 
    {
        get => _totalValue > 0 ? _totalValue : WeightOnHand * AverageCostPerGram;
        set => _totalValue = value;
    }

    /// <summary>
    /// Last count date for physical inventory
    /// </summary>
    public DateTime? LastCountDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last movement date
    /// </summary>
    public DateTime? LastMovementDate { get; set; }

    /// <summary>
    /// Notes about this inventory record
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Navigation property to branch
    /// </summary>
    [JsonIgnore]
    public virtual Branch Branch { get; set; } = null!;

    /// <summary>
    /// Navigation property to karat type
    /// </summary>
    [JsonIgnore]
    public virtual KaratTypeLookup KaratType { get; set; } = null!;

    /// <summary>
    /// Navigation property to raw gold inventory movements
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<RawGoldInventoryMovement> RawGoldInventoryMovements { get; set; } = new List<RawGoldInventoryMovement>();
}
