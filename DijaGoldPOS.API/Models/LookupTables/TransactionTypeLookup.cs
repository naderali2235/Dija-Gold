

namespace DijaGoldPOS.API.Models.LookupTables;

/// <summary>
/// Lookup table for transaction types
/// </summary>
public class TransactionTypeLookup : BaseEntity
{


    public string Name { get; set; } = string.Empty;
    

    public string? Description { get; set; }
    

    public int SortOrder { get; set; }
    

}
