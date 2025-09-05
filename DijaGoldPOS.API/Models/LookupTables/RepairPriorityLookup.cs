

using DijaGoldPOS.API.Models.Shared;

namespace DijaGoldPOS.API.Models.LookupTables;

/// <summary>
/// Lookup table for repair priorities
/// </summary>
public class RepairPriorityLookup : BaseEntity, ILookupEntity
{


    public string Name { get; set; } = string.Empty;
    

    public string? Description { get; set; }
    

    public int SortOrder { get; set; }
    

}
