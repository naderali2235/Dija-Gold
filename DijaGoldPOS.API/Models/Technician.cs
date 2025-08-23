using System.ComponentModel.DataAnnotations;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents a technician who can perform repair work
/// </summary>
public class Technician : BaseEntity
{
    /// <summary>
    /// Full name of the technician
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>
    /// Phone number for contacting the technician
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Email address (optional)
    /// </summary>
    [MaxLength(100)]
    public string? Email { get; set; }
    
    /// <summary>
    /// Specialization or skills (optional)
    /// </summary>
    [MaxLength(500)]
    public string? Specialization { get; set; }
    

    
    /// <summary>
    /// Branch where the technician primarily works
    /// </summary>
    public int? BranchId { get; set; }
    
    /// <summary>
    /// Navigation property to the branch
    /// </summary>
    public virtual Branch? Branch { get; set; }
}
