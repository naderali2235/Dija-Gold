using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DijaGoldPOS.API.Models.Shared;
using DijaGoldPOS.API.Models.PurchaseOrderModels;

namespace DijaGoldPOS.API.Models.InventoryModels;

/// <summary>
/// Represents movements in raw gold inventory (receipts, consumption, transfers)
/// </summary>
public class RawGoldInventoryMovement : BaseEntity
{
    /// <summary>
    /// Raw gold inventory record this movement belongs to
    /// </summary>
    public int RawGoldInventoryId { get; set; }

    /// <summary>
    /// Type of movement (Receipt, Consumption, Transfer, Adjustment)
    /// </summary>
    public string MovementType { get; set; } = string.Empty;

    /// <summary>
    /// Weight change (positive for receipts, negative for consumption)
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal WeightChange { get; set; }

    /// <summary>
    /// Weight balance after this movement
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal WeightBalance { get; set; }

    /// <summary>
    /// Movement date and time
    /// </summary>
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Reference number (PO number, manufacturing order, etc.)
    /// </summary>
    public string? ReferenceNumber { get; set; }

    /// <summary>
    /// Unit cost per gram for this movement
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? UnitCost { get; set; }

    /// <summary>
    /// Unit cost per gram (alias for compatibility)
    /// </summary>
    [NotMapped]
    public decimal? UnitCostPerGram
    {
        get => UnitCost;
        set => UnitCost = value;
    }

    /// <summary>
    /// Cost per gram (alias for compatibility)
    /// </summary>
    [NotMapped]
    public decimal CostPerGram
    {
        get => UnitCost ?? 0;
        set => UnitCost = value;
    }

    /// <summary>
    /// Reference (alias for ReferenceNumber)
    /// </summary>
    [NotMapped]
    public string Reference
    {
        get => ReferenceNumber ?? string.Empty;
        set => ReferenceNumber = value;
    }

    /// <summary>
    /// Weight after this movement (alias for WeightBalance)
    /// </summary>
    [NotMapped]
    public decimal WeightAfter
    {
        get => WeightBalance;
        set => WeightBalance = value;
    }

    /// <summary>
    /// Total cost of this movement (WeightChange * UnitCost)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? TotalCost { get; set; }

    /// <summary>
    /// Raw gold purchase order ID if this movement is from a purchase
    /// </summary>
    public int? RawGoldPurchaseOrderId { get; set; }

    /// <summary>
    /// Raw gold purchase order item ID if this movement is from a purchase
    /// </summary>
    public int? RawGoldPurchaseOrderItemId { get; set; }

    /// <summary>
    /// Manufacturing order ID if this movement is for manufacturing
    /// </summary>
    public int? ManufacturingOrderId { get; set; }

    /// <summary>
    /// Notes about this movement
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Navigation property to raw gold inventory
    /// </summary>
    [JsonIgnore]
    public virtual RawGoldInventory RawGoldInventory { get; set; } = null!;

    /// <summary>
    /// Navigation property to raw gold purchase order (if applicable)
    /// </summary>
    [JsonIgnore]
    public virtual RawGoldPurchaseOrder? RawGoldPurchaseOrder { get; set; }

    /// <summary>
    /// Navigation property to raw gold purchase order item (if applicable)
    /// </summary>
    [JsonIgnore]
    public virtual RawGoldPurchaseOrderItem? RawGoldPurchaseOrderItem { get; set; }
}
