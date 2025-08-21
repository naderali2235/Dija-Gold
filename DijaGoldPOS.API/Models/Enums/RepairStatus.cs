using System.ComponentModel;

namespace DijaGoldPOS.API.Models.Enums;

/// <summary>
/// Represents the status of a repair job in the workflow
/// </summary>
public enum RepairStatus
{
    /// <summary>Repair job is pending and waiting to be started</summary>
    [Description("Pending")]
    Pending = 1,
    
    /// <summary>Repair work is in progress</summary>
    [Description("In Progress")]
    InProgress = 2,
    
    /// <summary>Repair work is completed</summary>
    [Description("Completed")]
    Completed = 3,
    
    /// <summary>Repair is ready for customer pickup</summary>
    [Description("Ready for Pickup")]
    ReadyForPickup = 4,
    
    /// <summary>Repair has been delivered to customer</summary>
    [Description("Delivered")]
    Delivered = 5,
    
    /// <summary>Repair job has been cancelled</summary>
    [Description("Cancelled")]
    Cancelled = 6
}

/// <summary>
/// Represents the priority level of a repair job
/// </summary>
public enum RepairPriority
{
    /// <summary>Low priority repair</summary>
    [Description("Low")]
    Low = 1,
    
    /// <summary>Medium priority repair</summary>
    [Description("Medium")]
    Medium = 2,
    
    /// <summary>High priority repair</summary>
    [Description("High")]
    High = 3,
    
    /// <summary>Urgent priority repair</summary>
    [Description("Urgent")]
    Urgent = 4
}
