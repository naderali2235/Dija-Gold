
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents ownership movement history for audit trail and tracking ownership changes
/// </summary>
public class OwnershipMovement : BaseEntity
{
    /// <summary>
    /// Product ownership record this movement affects
    /// </summary>

    public int ProductOwnershipId { get; set; }
    
    /// <summary>
    /// Type of movement (Purchase, Payment, Sale, Adjustment, Conversion)
    /// </summary>


    public string MovementType { get; set; } = string.Empty;
    
    /// <summary>
    /// Date and time when the movement occurred
    /// </summary>

    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Reference to source document (TransactionId, PurchaseOrderId, etc.)
    /// </summary>

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
    /// Financial amount change (positive for payments, negative for purchases)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountChange { get; set; }
    
    /// <summary>
    /// Owned quantity after this movement
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal OwnedQuantityAfter { get; set; }
    
    /// <summary>
    /// Owned weight after this movement
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal OwnedWeightAfter { get; set; }
    
    /// <summary>
    /// Amount paid after this movement
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaidAfter { get; set; }
    
    /// <summary>
    /// Ownership percentage after this movement
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal OwnershipPercentageAfter { get; set; }
    
    /// <summary>
    /// Notes about the movement
    /// </summary>

    public string? Notes { get; set; }
    
    /// <summary>
    /// User who created this movement
    /// </summary>


    public string CreatedByUserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Navigation property to product ownership
    /// </summary>
    [JsonIgnore]
    public virtual ProductOwnership ProductOwnership { get; set; } = null!;
}
