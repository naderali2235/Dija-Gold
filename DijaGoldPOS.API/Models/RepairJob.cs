using DijaGoldPOS.API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents a repair job with workflow tracking
/// </summary>
public class RepairJob : BaseEntity
{
    /// <summary>
    /// Reference to the original transaction
    /// </summary>
    [Required]
    public int TransactionId { get; set; }
    
    /// <summary>
    /// Current status of the repair job
    /// </summary>
    [Required]
    public RepairStatus Status { get; set; } = RepairStatus.Pending;
    
    /// <summary>
    /// Priority level of the repair
    /// </summary>
    [Required]
    public RepairPriority Priority { get; set; } = RepairPriority.Medium;
    
    /// <summary>
    /// ID of the technician assigned to this repair
    /// </summary>
    [MaxLength(450)]
    public string? AssignedTechnicianId { get; set; }
    
    /// <summary>
    /// Date when repair work started
    /// </summary>
    public DateTime? StartedDate { get; set; }
    
    /// <summary>
    /// Date when repair work was completed
    /// </summary>
    public DateTime? CompletedDate { get; set; }
    
    /// <summary>
    /// Date when repair was ready for pickup
    /// </summary>
    public DateTime? ReadyForPickupDate { get; set; }
    
    /// <summary>
    /// Date when repair was delivered to customer
    /// </summary>
    public DateTime? DeliveredDate { get; set; }
    
    /// <summary>
    /// Notes from the technician about the repair work
    /// </summary>
    [MaxLength(2000)]
    public string? TechnicianNotes { get; set; }
    
    /// <summary>
    /// Actual cost of the repair (may differ from estimated)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? ActualCost { get; set; }
    
    /// <summary>
    /// Additional materials or parts used in the repair
    /// </summary>
    [MaxLength(1000)]
    public string? MaterialsUsed { get; set; }
    
    /// <summary>
    /// Time spent on the repair in hours
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal? HoursSpent { get; set; }
    
    /// <summary>
    /// Quality check performed by
    /// </summary>
    [MaxLength(450)]
    public string? QualityCheckedBy { get; set; }
    
    /// <summary>
    /// Date of quality check
    /// </summary>
    public DateTime? QualityCheckDate { get; set; }
    
    /// <summary>
    /// Customer notification sent
    /// </summary>
    public bool CustomerNotified { get; set; } = false;
    
    /// <summary>
    /// Date when customer was notified
    /// </summary>
    public DateTime? CustomerNotificationDate { get; set; }
    
    /// <summary>
    /// Navigation property to the transaction
    /// </summary>
    [JsonIgnore]
    public virtual Transaction Transaction { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to the assigned technician
    /// </summary>
    [JsonIgnore]
    public virtual ApplicationUser? AssignedTechnician { get; set; }
    
    /// <summary>
    /// Navigation property to the quality checker
    /// </summary>
    [JsonIgnore]
    public virtual ApplicationUser? QualityChecker { get; set; }
}
