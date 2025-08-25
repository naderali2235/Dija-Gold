

namespace DijaGoldPOS.API.Models.LookupTables;

/// <summary>
/// Lookup table for order statuses
/// </summary>
public class OrderStatusLookup : BaseEntity, ILookupEntity
{


    public string Name { get; set; } = string.Empty;
    

    public string? Description { get; set; }
    

    public int SortOrder { get; set; }
    

}
