using DijaGoldPOS.API.Models.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijaGoldPOS.API.Models.LookupModels;

/// <summary>
/// Lookup table for repair job statuses
/// </summary>
[Table("RepairStatuses", Schema = "Lookup")]
public class RepairStatusLookup : BaseEntity
{
    /// <summary>
    /// Status code (RECEIVED, IN_PROGRESS, COMPLETED, CANCELLED)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Whether this status indicates the repair is in progress
    /// </summary>
    public bool IsInProgress { get; set; } = false;

    /// <summary>
    /// Whether this is a final status
    /// </summary>
    public bool IsFinal { get; set; } = false;

    /// <summary>
    /// Whether this status indicates completion
    /// </summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// Whether this status indicates cancellation
    /// </summary>
    public bool IsCancelled { get; set; } = false;

    /// <summary>
    /// Whether customer notification is required for this status
    /// </summary>
    public bool RequiresCustomerNotification { get; set; } = false;

    /// <summary>
    /// CSS color class for UI display
    /// </summary>
    public string? ColorClass { get; set; }
    public string Description { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; } = 0;
}

/// <summary>
/// Lookup table for repair job priorities
/// </summary>
[Table("RepairPriorities", Schema = "Lookup")]
public class RepairPriorityLookup : BaseEntity
{
    /// <summary>
    /// Priority code (LOW, MEDIUM, HIGH, URGENT)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Numeric priority level (1 = lowest, 5 = highest)
    /// </summary>
    public int PriorityLevel { get; set; } = 1;

    /// <summary>
    /// Expected completion days based on priority
    /// </summary>
    public int? ExpectedCompletionDays { get; set; }

    /// <summary>
    /// Additional cost percentage for this priority level
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal? AdditionalCostPercentage { get; set; }

    /// <summary>
    /// CSS color class for UI display
    /// </summary>
    public string? ColorClass { get; set; }
    public string Description { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; } = 0;
}
