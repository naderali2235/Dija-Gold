
using System.ComponentModel.DataAnnotations.Schema;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents audit trail for all user actions and data changes
/// </summary>
public class AuditLog
{
    /// <summary>
    /// Unique identifier for the audit log entry
    /// </summary>

    public long Id { get; set; }
    
    /// <summary>
    /// User who performed the action
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// Username of the user who performed the action
    /// </summary>
    public string? UserName { get; set; }
    
    /// <summary>
    /// Action performed (Create, Update, Delete, Login, Logout, etc.)
    /// </summary>


    public string Action { get; set; } = string.Empty;
    
    /// <summary>
    /// Entity type affected (Transaction, Product, User, etc.)
    /// </summary>

    public string? EntityType { get; set; }
    
    /// <summary>
    /// Primary key of the affected entity
    /// </summary>

    public string? EntityId { get; set; }
    
    /// <summary>
    /// Description of the action performed
    /// </summary>


    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Old values (JSON format for data changes)
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? OldValues { get; set; }
    
    /// <summary>
    /// New values (JSON format for data changes)
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? NewValues { get; set; }
    
    /// <summary>
    /// IP address of the user
    /// </summary>

    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent/browser information
    /// </summary>

    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Branch ID where the action occurred
    /// </summary>
    public int? BranchId { get; set; }
    
    /// <summary>
    /// Financial Transaction ID if the action is related to a financial transaction
    /// </summary>
    public int? FinancialTransactionId { get; set; }
    
    /// <summary>
    /// Order ID if the action is related to an order
    /// </summary>
    public int? OrderId { get; set; }
    
    /// <summary>
    /// Timestamp when the action occurred
    /// </summary>

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Session ID
    /// </summary>

    public string? SessionId { get; set; }
    
    /// <summary>
    /// Success or failure of the action
    /// </summary>
    public bool IsSuccess { get; set; } = true;
    
    /// <summary>
    /// Error message if action failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional details about the action
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Branch name where the action occurred (for display purposes)
    /// </summary>
    public string? BranchName { get; set; }
    
    /// <summary>
    /// Navigation property to user
    /// </summary>
    public virtual ApplicationUser? User { get; set; }
    
    /// <summary>
    /// Navigation property to branch
    /// </summary>
    public virtual Branch? Branch { get; set; }
    
    /// <summary>
    /// Navigation property to financial transaction
    /// </summary>
    public virtual FinancialTransaction? FinancialTransaction { get; set; }
    
    /// <summary>
    /// Navigation property to order
    /// </summary>
    public virtual Order? Order { get; set; }
}
