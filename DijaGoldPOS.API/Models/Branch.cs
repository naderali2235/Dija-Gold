using System.ComponentModel.DataAnnotations;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents a branch/store location
/// </summary>
public class Branch : BaseEntity
{
    /// <summary>
    /// Branch name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Branch code for identification
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Branch address
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }
    
    /// <summary>
    /// Branch phone number
    /// </summary>
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    /// <summary>
    /// Manager name
    /// </summary>
    [MaxLength(100)]
    public string? ManagerName { get; set; }
    
    /// <summary>
    /// Whether this is the main branch/headquarters
    /// </summary>
    public bool IsHeadquarters { get; set; } = false;
    
    /// <summary>
    /// Navigation property to users assigned to this branch
    /// </summary>
    public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    
    /// <summary>
    /// Navigation property to inventory items at this branch
    /// </summary>
    public virtual ICollection<Inventory> InventoryItems { get; set; } = new List<Inventory>();
    
    /// <summary>
    /// Navigation property to financial transactions at this branch
    /// </summary>
    public virtual ICollection<FinancialTransaction> FinancialTransactions { get; set; } = new List<FinancialTransaction>();
    
    /// <summary>
    /// Navigation property to orders at this branch
    /// </summary>
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}