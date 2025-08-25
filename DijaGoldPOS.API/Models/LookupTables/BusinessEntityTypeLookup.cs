

namespace DijaGoldPOS.API.Models.LookupTables;

/// <summary>
/// Lookup table for business entity types
/// </summary>
public class BusinessEntityTypeLookup : BaseEntity, ILookupEntity
{


    public string Name { get; set; } = string.Empty;
    

    public string? Description { get; set; }
    

    public int SortOrder { get; set; }
    

}
