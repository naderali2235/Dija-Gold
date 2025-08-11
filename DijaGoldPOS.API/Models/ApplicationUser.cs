using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Application user extending IdentityUser with additional properties
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Full name of the user
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>
    /// Employee ID or code
    /// </summary>
    [MaxLength(50)]
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
    /// Navigation property to user roles
    /// </summary>
    public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; } = new List<IdentityUserRole<string>>();
}