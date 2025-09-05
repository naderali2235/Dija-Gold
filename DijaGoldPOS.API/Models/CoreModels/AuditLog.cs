using DijaGoldPOS.API.Models.FinancialModels;
using DijaGoldPOS.API.Models.SalesModels;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijaGoldPOS.API.Models.CoreModels;

/// <summary>
/// Audit log for tracking all system operations and changes
/// </summary>
[Table("AuditLogs", Schema = "Audit")]
public class AuditLog
{
    /// <summary>
    /// Unique identifier for the audit log entry
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Timestamp when the action occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User ID who performed the action
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Username who performed the action
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Type of action performed (CREATE, UPDATE, DELETE, etc.)
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity that was affected
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// ID of the entity that was affected
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// Description of the action performed
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// JSON representation of old values (for updates)
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// JSON representation of new values (for creates and updates)
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// IP address from where the action was performed
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string from the client
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Session ID for tracking user sessions
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Branch ID where the action was performed
    /// </summary>
    public int? BranchId { get; set; }

    /// <summary>
    /// Branch name for easier querying
    /// </summary>
    public string? BranchName { get; set; }

    /// <summary>
    /// Financial transaction ID if applicable
    /// </summary>
    public int? FinancialTransactionId { get; set; }

    /// <summary>
    /// Order ID if applicable
    /// </summary>
    public int? OrderId { get; set; }

    /// <summary>
    /// Error message if the action failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional details about the action
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Correlation ID for tracking related operations
    /// </summary>
    public string? CorrelationId { get; set; }

    // Navigation Properties
    /// <summary>
    /// Navigation property to the user who performed the action
    /// </summary>
    public virtual ApplicationUser? User { get; set; }

    /// <summary>
    /// Navigation property to the branch where action was performed
    /// </summary>
    public virtual Branch? Branch { get; set; }

    /// <summary>
    /// Navigation property to related financial transaction
    /// </summary>
    public virtual FinancialTransaction? FinancialTransaction { get; set; }

    /// <summary>
    /// Navigation property to related order
    /// </summary>
    public virtual Order? Order { get; set; }
}
