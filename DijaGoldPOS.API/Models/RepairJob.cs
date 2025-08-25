using DijaGoldPOS.API.Models.LookupTables;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents a repair job with workflow tracking
/// </summary>
public class RepairJob : BaseEntity
{
    /// <summary>
    /// Reference to the financial transaction
    /// </summary>
    public int? FinancialTransactionId { get; set; }
    
    /// <summary>
    /// Current status of the repair job
    /// </summary>

    public int StatusId { get; set; }
    
    /// <summary>
    /// Priority level of the repair
    /// </summary>

    public int PriorityId { get; set; }
    
    /// <summary>
    /// ID of the technician assigned to this repair
    /// </summary>
    [ForeignKey("AssignedTechnician")]
    public int? AssignedTechnicianId { get; set; }
    
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

    public string? TechnicianNotes { get; set; }
    
    /// <summary>
    /// Actual cost of the repair (may differ from estimated)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? ActualCost { get; set; }
    
    /// <summary>
    /// Additional materials or parts used in the repair
    /// </summary>

    public string? MaterialsUsed { get; set; }
    
    /// <summary>
    /// Time spent on the repair in hours
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal? HoursSpent { get; set; }
    
    /// <summary>
    /// Quality check performed by
    /// </summary>
    [ForeignKey("QualityChecker")]
    public int? QualityCheckedBy { get; set; }
    
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
    /// General notes about the repair job
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Estimated cost of the repair
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? EstimatedCost { get; set; }

    /// <summary>
    /// Estimated completion date
    /// </summary>
    public DateTime? EstimatedCompletionDate { get; set; }
    
    /// <summary>
    /// Navigation property to the financial transaction
    /// </summary>
    [JsonIgnore]
    public virtual FinancialTransaction? FinancialTransaction { get; set; }
    
    /// <summary>
    /// Navigation property to the assigned technician
    /// </summary>
    [JsonIgnore]
    public virtual Technician? AssignedTechnician { get; set; }
    
    /// <summary>
    /// Navigation property to repair status lookup
    /// </summary>
    [JsonIgnore]
    public virtual RepairStatusLookup Status { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to repair priority lookup
    /// </summary>
    [JsonIgnore]
    public virtual RepairPriorityLookup Priority { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to the quality checker
    /// </summary>
    [JsonIgnore]
    public virtual Technician? QualityChecker { get; set; }
}
