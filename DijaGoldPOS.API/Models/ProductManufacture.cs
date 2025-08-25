
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents the manufacturing relationship between finished products and raw gold from purchase orders
/// </summary>
public class ProductManufacture : BaseEntity
{
    /// <summary>
    /// The finished product that was manufactured
    /// </summary>

    public int ProductId { get; set; }

    /// <summary>
    /// Branch where the manufacturing is performed
    /// </summary>

    public int BranchId { get; set; }

    /// <summary>
    /// Technician performing the manufacturing
    /// </summary>

    public int TechnicianId { get; set; }
    
    /// <summary>
    /// The purchase order item (raw gold) that was used to manufacture this product
    /// </summary>

    public int SourcePurchaseOrderItemId { get; set; }

    /// <summary>
    /// Additional purchase order item reference (nullable)
    /// </summary>
    public int? PurchaseOrderItemId { get; set; }
    
    /// <summary>
    /// Weight of raw gold consumed to manufacture this product (in grams)
    /// </summary>

    [Column(TypeName = "decimal(10,3)")]
    public decimal ConsumedWeight { get; set; }
    
    /// <summary>
    /// Weight lost during manufacturing process (wastage) in grams
    /// </summary>
    [Column(TypeName = "decimal(10,3)")]
    public decimal WastageWeight { get; set; } = 0;
    
    /// <summary>
    /// Date when the product was manufactured
    /// </summary>

    public DateTime ManufactureDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Manufacturing cost per gram (includes making charges, labor, etc.)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ManufacturingCostPerGram { get; set; }
    
    /// <summary>
    /// Total manufacturing cost for this product
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalManufacturingCost { get; set; }
    
    /// <summary>
    /// Manufacturing batch number for grouping related products
    /// </summary>

    public string? BatchNumber { get; set; }
    
    /// <summary>
    /// Notes about the manufacturing process
    /// </summary>

    public string? ManufacturingNotes { get; set; }
    
    /// <summary>
    /// Manufacturing status - supports workflow transitions
    /// </summary>


    public string Status { get; set; } = "Draft"; // Draft, InProgress, QualityCheck, Approved, Completed, Rejected, Cancelled

    /// <summary>
    /// Current workflow step
    /// </summary>


    public string WorkflowStep { get; set; } = "Draft"; // Draft, Manufacturing, QualityControl, FinalApproval, Complete

    /// <summary>
    /// Quality check status
    /// </summary>

    public string? QualityCheckStatus { get; set; } // Pending, Passed, Failed

    /// <summary>
    /// Quality checked by user ID
    /// </summary>
    public string? QualityCheckedByUserId { get; set; }

    /// <summary>
    /// Quality check date
    /// </summary>
    public DateTime? QualityCheckDate { get; set; }

    /// <summary>
    /// Quality check notes
    /// </summary>

    public string? QualityCheckNotes { get; set; }

    /// <summary>
    /// Final approval status
    /// </summary>

    public string? FinalApprovalStatus { get; set; } // Pending, Approved, Rejected

    /// <summary>
    /// Final approved by user ID
    /// </summary>
    public string? FinalApprovedByUserId { get; set; }

    /// <summary>
    /// Final approval date
    /// </summary>
    public DateTime? FinalApprovalDate { get; set; }

    /// <summary>
    /// Final approval notes
    /// </summary>

    public string? FinalApprovalNotes { get; set; }

    /// <summary>
    /// Rejection reason if rejected at any stage
    /// </summary>

    public string? RejectionReason { get; set; }

    /// <summary>
    /// Priority level for manufacturing
    /// </summary>


    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent

    /// <summary>
    /// Estimated completion date
    /// </summary>
    public DateTime? EstimatedCompletionDate { get; set; }

    /// <summary>
    /// Actual completion date
    /// </summary>
    public DateTime? ActualCompletionDate { get; set; }

    /// <summary>
    /// Manufacturing efficiency rating (0-100)
    /// </summary>

    public int? EfficiencyRating { get; set; }

    /// <summary>
    /// Workflow history tracking
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<ManufacturingWorkflowHistory> WorkflowHistory { get; set; } = new List<ManufacturingWorkflowHistory>();
    
    /// <summary>
    /// Navigation property to the finished product
    /// </summary>
    [JsonIgnore]
    public virtual Product Product { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to the source purchase order item (raw gold)
    /// </summary>
    [JsonIgnore]
    public virtual PurchaseOrderItem SourcePurchaseOrderItem { get; set; } = null!;

    /// <summary>
    /// Navigation property to the additional purchase order item reference
    /// </summary>
    [JsonIgnore]
    public virtual PurchaseOrderItem? PurchaseOrderItem { get; set; }

    /// <summary>
    /// Navigation property to the branch
    /// </summary>
    [JsonIgnore]
    public virtual Branch Branch { get; set; } = null!;

    /// <summary>
    /// Navigation property to the technician
    /// </summary>
    [JsonIgnore]
    public virtual Technician Technician { get; set; } = null!;
}
