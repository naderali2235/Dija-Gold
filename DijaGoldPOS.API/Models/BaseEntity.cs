using System.ComponentModel.DataAnnotations;

namespace DijaGoldPOS.API.Models;

/// <summary>
/// Base entity class with common audit fields for all entities
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Primary key identifier
    /// </summary>
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Date and time when the record was created
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Date and time when the record was last modified
    /// </summary>
    public DateTime? ModifiedAt { get; set; }
    
    /// <summary>
    /// User ID who created the record
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string CreatedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// User ID who last modified the record
    /// </summary>
    [MaxLength(450)]
    public string? ModifiedBy { get; set; }
    
    /// <summary>
    /// Indicates if the record is active (soft delete)
    /// </summary>
    public bool IsActive { get; set; } = true;
}