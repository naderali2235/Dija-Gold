using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents inventory movement history for audit trail
/// </summary>
public class InventoryMovement : BaseEntity
{
    /// <summary>
    /// Inventory record this movement affects
    /// </summary>
    [Required]
    public int InventoryId { get; set; }
    
    /// <summary>
    /// Type of movement (Sale, Purchase, Return, Transfer, Adjustment)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string MovementType { get; set; } = string.Empty;
    
    /// <summary>
    /// Date and time when the movement occurred
    /// </summary>
    [Required]
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Reference to source document (TransactionId, PurchaseOrderId, etc.)
    /// </summary>
    [MaxLength(50)]
    public string? ReferenceNumber { get; set; }
    
    /// <summary>
    /// Quantity change (positive for inbound, negative for outbound)
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal QuantityChange { get; set; }
    
    /// <summary>
    /// Weight change in grams (positive for inbound, negative for outbound)
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal WeightChange { get; set; }
    
    /// <summary>
    /// Quantity balance after this movement
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal QuantityBalance { get; set; }
    
    /// <summary>
    /// Weight balance after this movement
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal WeightBalance { get; set; }
    
    /// <summary>
    /// Unit cost at time of movement (for purchases)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? UnitCost { get; set; }
    
    /// <summary>
    /// Notes about the movement
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Navigation property to inventory
    /// </summary>
    public virtual Inventory Inventory { get; set; } = null!;
}