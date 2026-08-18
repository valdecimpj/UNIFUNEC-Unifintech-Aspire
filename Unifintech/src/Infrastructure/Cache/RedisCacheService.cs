using Microsoft.Extensions.Caching.Distributed;
using Unifintech.Application.Common.Interfaces;

namespace Unifintech.Infrastructure.Cache;

public class RedisCacheService(IDistributedCache cache) : ICacheService
{
    public async Task SetAsync(string key, string value, DateTimeOffset? absoluteExpiration = null)
    {
        await cache.SetStringAsync(
            key,
            value,
            new DistributedCacheEntryOptions { AbsoluteExpiration = absoluteExpiration }
        );
    }

    public async Task<string?> GetAsync(string key) => await cache.GetStringAsync(key);

    public async Task RemoveAsync(string key) => await cache.RemoveAsync(key);
}
