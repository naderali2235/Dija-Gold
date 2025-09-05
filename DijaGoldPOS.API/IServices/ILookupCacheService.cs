namespace DijaGoldPOS.API.IServices;

public interface ILookupCacheService
{
    Task<T?> GetLookupAsync<T>(string cacheKey) where T : class;
    Task SetLookupAsync<T>(string cacheKey, T value) where T : class;
    Task InvalidateLookupAsync(string cacheKey);
    Task InvalidateAllLookupsAsync();
}
