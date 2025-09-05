

using DijaGoldPOS.API.Models.Shared;

namespace DijaGoldPOS.API.Models.LookupTables;

/// <summary>
/// Lookup table for karat types
/// </summary>
public class KaratTypeLookup : BaseEntity, ILookupEntity
{


    public string Name { get; set; } = string.Empty;
    

    public string? Description { get; set; }
    

    public int SortOrder { get; set; }
    

}
