using DijaGoldPOS.API.IServices;

namespace DijaGoldPOS.API.Services;

public class LookupCacheService : ILookupCacheService
{
    private readonly ICacheService _cacheService;

    public LookupCacheService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<T?> GetLookupAsync<T>(string cacheKey) where T : class
    {
        return await _cacheService.GetAsync<T>($"lookup:{cacheKey}");
    }

    public async Task SetLookupAsync<T>(string cacheKey, T value) where T : class
    {
        await _cacheService.SetAsync($"lookup:{cacheKey}", value, TimeSpan.FromHours(24));
    }

    public async Task InvalidateLookupAsync(string cacheKey)
    {
        await _cacheService.RemoveAsync($"lookup:{cacheKey}");
    }

    public async Task InvalidateAllLookupsAsync()
    {
        await _cacheService.RemovePatternAsync("lookup:*");
    }
}
