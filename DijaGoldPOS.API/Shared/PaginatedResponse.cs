namespace DijaGoldPOS.API.Shared;

/// <summary>
/// Paginated response wrapper
/// </summary>
/// <typeparam name="T">Data type</typeparam>
public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
