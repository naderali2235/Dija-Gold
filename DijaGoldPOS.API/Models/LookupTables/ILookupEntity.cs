namespace DijaGoldPOS.API.Models.LookupTables
{
    public interface ILookupEntity
    {
        int Id { get; }
        string Name { get; }
        bool IsActive { get; }
        int SortOrder { get; }
    }
}
