using System.ComponentModel.DataAnnotations;

namespace DijaGoldPOS.API.Models.LookupTables;

/// <summary>
/// Lookup table for karat types
/// </summary>
public class KaratTypeLookup : BaseEntity, ILookupEntity
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    [Required]
    public int KaratValue { get; set; }
    
    [Required]
    public int SortOrder { get; set; }
    

}
