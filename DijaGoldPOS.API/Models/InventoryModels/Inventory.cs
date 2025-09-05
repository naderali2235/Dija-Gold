using DijaGoldPOS.API.Models.BranchModels;
using DijaGoldPOS.API.Models.ProductModels;
using DijaGoldPOS.API.Models.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijaGoldPOS.API.Models.InventoryModels;

/// <summary>
/// Represents inventory levels for products at specific branches
/// </summary>
public class Inventory : BaseEntity
{
    /// <summary>
    /// Product being tracked
    /// </summary>

    public int ProductId { get; set; }
    
    /// <summary>
    /// Branch where inventory is held
    /// </summary>

    public int BranchId { get; set; }
    
    /// <summary>
    /// Current quantity on hand
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal QuantityOnHand { get; set; }
    
    /// <summary>
    /// Current weight on hand in grams (precision to 3 decimal places)
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal WeightOnHand { get; set; }
    
    /// <summary>
    /// Minimum stock level for reorder alerts
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
    /// Additional notes about inventory
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Last updated date for inventory count
    /// </summary>
    public DateTime LastCountDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Navigation property to product
    /// </summary>
    public virtual Product Product { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to branch
    /// </summary>
    public virtual Branch Branch { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to inventory movements
    /// </summary>
    public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
}
