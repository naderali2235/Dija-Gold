using System.ComponentModel.DataAnnotations;

namespace DijaGoldPOS.API.Models.LookupTables;

/// <summary>
/// Lookup table for transaction statuses
/// </summary>
public class TransactionStatusLookup : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    [Required]
    public int SortOrder { get; set; }
    

}
