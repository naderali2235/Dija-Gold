using System.ComponentModel.DataAnnotations;

namespace DijaGoldPOS.API.Models.LookupTables;

/// <summary>
/// Sub-category lookup table for product categorization
/// </summary>
public class SubCategoryLookup : BaseEntity
{
    /// <summary>
    /// Name of the sub-category
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the sub-category
    /// </summary>
    [MaxLength(200)]
    public string? Description { get; set; }

    /// <summary>
    /// Sort order for display
    /// </summary>
    public int SortOrder { get; set; } = 1;

    /// <summary>
    /// Whether this sub-category is active
    /// </summary>
    public new bool IsActive { get; set; } = true;
}
