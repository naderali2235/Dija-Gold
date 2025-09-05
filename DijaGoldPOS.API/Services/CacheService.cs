using DijaGoldPOS.API.IServices;
using Microsoft.Extensions.Caching.Memory;

namespace DijaGoldPOS.API.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;

    public CacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        return await Task.FromResult(_memoryCache.Get<T>(key));
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.SetAbsoluteExpiration(expiration.Value);
        }
        _memoryCache.Set(key, value, options);
        await Task.CompletedTask;
    }

    public async Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        await Task.CompletedTask;
    }

    public async Task RemovePatternAsync(string pattern)
    {
        // Memory cache doesn't support pattern removal - this is a stub
        await Task.CompletedTask;
    }
}
