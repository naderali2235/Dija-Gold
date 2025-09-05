

using DijaGoldPOS.API.Models.BranchModels;
using DijaGoldPOS.API.Models.SalesModels;
using DijaGoldPOS.API.Models.Shared;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents a technician who can perform repair work
/// </summary>
public class Technician : BaseEntity
{
    /// <summary>
    /// Full name of the technician
    /// </summary>


    public string FullName { get; set; } = string.Empty;
    
    /// <summary>
    /// Phone number for contacting the technician
    /// </summary>


    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Email address (optional)
    /// </summary>

    public string? Email { get; set; }
    
    /// <summary>
    /// Specialization or skills (optional)
    /// </summary>

    public string? Specialization { get; set; }
    

    
    /// <summary>
    /// Branch where the technician primarily works
    /// </summary>
    public int? BranchId { get; set; }
    
    /// <summary>
    /// Navigation property to the branch
    /// </summary>
    public virtual Branch? Branch { get; set; }

    /// <summary>
    /// Navigation property to repair jobs assigned to this technician
    /// </summary>
    public virtual ICollection<RepairJob> RepairJobs { get; set; } = new List<RepairJob>();

    /// <summary>
    /// Navigation property to repair jobs quality checked by this technician
    /// </summary>
    public virtual ICollection<RepairJob> QualityCheckedRepairJobs { get; set; } = new List<RepairJob>();
}
