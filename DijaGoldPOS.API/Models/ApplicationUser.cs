using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Application user extending IdentityUser with additional properties
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Full name of the user
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Employee ID or code
    /// </summary>
    public string? EmployeeCode { get; set; }

    /// <summary>
    /// Branch ID where the user is assigned
    /// </summary>
    public int? BranchId { get; set; }

    /// <summary>
    /// Navigation property to Branch
    /// </summary>
    public virtual Branch? Branch { get; set; }

    /// <summary>
    /// Indicates if the user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date when the user was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last login date time
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// User who created this record
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Last modified date time
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// User who last modified this record
    /// </summary>
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Navigation property to user roles
    /// </summary>
    public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; } = new List<IdentityUserRole<string>>();

    /// <summary>
    /// Navigation property to orders created by this user
    /// </summary>
    [InverseProperty("Cashier")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    /// <summary>
    /// Navigation property to financial transactions created by this user
    /// </summary>
    [InverseProperty("ProcessedByUser")]
    public virtual ICollection<FinancialTransaction> FinancialTransactions { get; set; } = new List<FinancialTransaction>();

    /// <summary>
    /// Navigation property to financial transactions approved by this user
    /// </summary>
    [InverseProperty("ApprovedByUser")]
    public virtual ICollection<FinancialTransaction> ApprovedFinancialTransactions { get; set; } = new List<FinancialTransaction>();

    /// <summary>
    /// Navigation property to orders approved by this user
    /// </summary>
    [InverseProperty("ApprovedByUser")]
    public virtual ICollection<Order> ApprovedOrders { get; set; } = new List<Order>();

    /// <summary>
    /// Navigation property to audit logs for this user
    /// </summary>
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
