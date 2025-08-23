using DijaGoldPOS.API.Models.LookupTables;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Represents daily gold rates with versioning for different karat types
/// </summary>
public class GoldRate : BaseEntity
{
    /// <summary>
    /// Karat type for this rate
    /// </summary>
    [Required]
    public int KaratTypeId { get; set; }
    
    /// <summary>
    /// Rate per gram in Egyptian Pounds
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal RatePerGram { get; set; }
    
    /// <summary>
    /// Effective start date and time for this rate
    /// </summary>
    [Required]
    public DateTime EffectiveFrom { get; set; }
    
    /// <summary>
    /// Effective end date and time for this rate (null if current)
    /// </summary>
    public DateTime? EffectiveTo { get; set; }
    
    /// <summary>
    /// Whether this is the current active rate for the karat type
    /// </summary>
    public bool IsCurrent { get; set; } = true;
    
    /// <summary>
    /// Navigation property to karat type lookup
    /// </summary>
    public virtual KaratTypeLookup KaratType { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to orders using this rate
    /// </summary>
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}