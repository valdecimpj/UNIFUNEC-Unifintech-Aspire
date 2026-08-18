namespace Unifintech.Application.Common.Interfaces;

public interface ICacheService
{
    Task SetAsync(string key, string value, DateTimeOffset? absoluteExpiration = null);
    Task<string?> GetAsync(string key);
    Task RemoveAsync(string key);
}
