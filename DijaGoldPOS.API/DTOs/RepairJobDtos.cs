using DijaGoldPOS.API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DijaGoldPOS.API.DTOs;

/// <summary>
/// Repair job DTO for display operations
/// </summary>
public class RepairJobDto
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public RepairStatus Status { get; set; }
    public string StatusDisplayName => Status.ToString().Replace("_", " ");
    public RepairPriority Priority { get; set; }
    public string PriorityDisplayName => Priority.ToString();
    public string? AssignedTechnicianId { get; set; }
    public string? AssignedTechnicianName { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? ReadyForPickupDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public string? TechnicianNotes { get; set; }
    public decimal? ActualCost { get; set; }
    public string? MaterialsUsed { get; set; }
    public decimal? HoursSpent { get; set; }
    public string? QualityCheckedBy { get; set; }
    public string? QualityCheckerName { get; set; }
    public DateTime? QualityCheckDate { get; set; }
    public bool CustomerNotified { get; set; }
    public DateTime? CustomerNotificationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    
    // Transaction details
    public string RepairDescription { get; set; } = string.Empty;
    public decimal RepairAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
}

/// <summary>
/// Create repair job request DTO
/// </summary>
public class CreateRepairJobRequestDto
{
    [Required(ErrorMessage = "Transaction ID is required")]
    public int TransactionId { get; set; }

    [Required(ErrorMessage = "Priority is required")]
    public RepairPriority Priority { get; set; } = RepairPriority.Medium;

    public string? AssignedTechnicianId { get; set; }
    public string? TechnicianNotes { get; set; }
}

/// <summary>
/// Update repair job status request DTO
/// </summary>
public class UpdateRepairJobStatusRequestDto
{
    [Required(ErrorMessage = "Status is required")]
    public RepairStatus Status { get; set; }

    public string? TechnicianNotes { get; set; }
    public decimal? ActualCost { get; set; }
    public string? MaterialsUsed { get; set; }
    public decimal? HoursSpent { get; set; }
}

/// <summary>
/// Assign technician request DTO
/// </summary>
public class AssignTechnicianRequestDto
{
    [Required(ErrorMessage = "Technician ID is required")]
    public string TechnicianId { get; set; } = string.Empty;

    public string? TechnicianNotes { get; set; }
}

/// <summary>
/// Complete repair request DTO
/// </summary>
public class CompleteRepairRequestDto
{
    [Required(ErrorMessage = "Actual cost is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Actual cost must be greater than 0")]
    public decimal ActualCost { get; set; }

    public string? TechnicianNotes { get; set; }
    public string? MaterialsUsed { get; set; }
    public decimal? HoursSpent { get; set; }
}

/// <summary>
/// Quality check request DTO
/// </summary>
public class QualityCheckRequestDto
{
    [Required(ErrorMessage = "Quality check notes are required")]
    [StringLength(500, ErrorMessage = "Quality check notes cannot exceed 500 characters")]
    public string QualityCheckNotes { get; set; } = string.Empty;

    public bool Passed { get; set; } = true;
}

/// <summary>
/// Deliver repair request DTO
/// </summary>
public class DeliverRepairRequestDto
{
    public string? DeliveryNotes { get; set; }
    public bool CustomerNotified { get; set; } = true;
}

/// <summary>
/// Repair job search request DTO
/// </summary>
public class RepairJobSearchRequestDto
{
    public int? BranchId { get; set; }
    public RepairStatus? Status { get; set; }
    public RepairPriority? Priority { get; set; }
    public string? AssignedTechnicianId { get; set; }
    public int? CustomerId { get; set; }
    public string? TransactionNumber { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Repair job statistics DTO
/// </summary>
public class RepairJobStatisticsDto
{
    public int TotalJobs { get; set; }
    public int PendingJobs { get; set; }
    public int InProgressJobs { get; set; }
    public int CompletedJobs { get; set; }
    public int ReadyForPickupJobs { get; set; }
    public int DeliveredJobs { get; set; }
    public int CancelledJobs { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageCompletionTime { get; set; } // in hours
    public Dictionary<string, int> JobsByPriority { get; set; } = new();
    public Dictionary<string, int> JobsByTechnician { get; set; } = new();
}
