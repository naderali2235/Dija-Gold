

namespace DijaGoldPOS.API.Models.LookupTables;

/// <summary>
/// Lookup table for charge types
/// </summary>
public class ChargeTypeLookup : BaseEntity, ILookupEntity
{


    public string Name { get; set; } = string.Empty;
    

    public string? Description { get; set; }
    

    public int SortOrder { get; set; }
    

}
