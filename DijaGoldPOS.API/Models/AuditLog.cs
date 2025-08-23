using System.ComponentModel.DataAnnotations;
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
    [Key]
    public long Id { get; set; }
    
    /// <summary>
    /// User who performed the action
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Username of the user who performed the action
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Action performed (Create, Update, Delete, Login, Logout, etc.)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;
    
    /// <summary>
    /// Entity type affected (Transaction, Product, User, etc.)
    /// </summary>
    [MaxLength(100)]
    public string? EntityType { get; set; }
    
    /// <summary>
    /// Primary key of the affected entity
    /// </summary>
    [MaxLength(50)]
    public string? EntityId { get; set; }
    
    /// <summary>
    /// Description of the action performed
    /// </summary>
    [Required]
    [MaxLength(1000)]
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
    [MaxLength(45)]
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent/browser information
    /// </summary>
    [MaxLength(500)]
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
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Session ID
    /// </summary>
    [MaxLength(100)]
    public string? SessionId { get; set; }
    
    /// <summary>
    /// Success or failure of the action
    /// </summary>
    public bool IsSuccess { get; set; } = true;
    
    /// <summary>
    /// Error message if action failed
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Navigation property to user
    /// </summary>
    public virtual ApplicationUser User { get; set; } = null!;
    
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