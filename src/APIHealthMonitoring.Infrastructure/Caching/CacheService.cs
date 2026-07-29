using System.Collections.Concurrent;
using APIHealthMonitoring.Application.Interfaces;
using APIHealthMonitoring.Application.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace APIHealthMonitoring.Infrastructure.Caching;

/// <summary>
/// IMemoryCache-backed implementation of <see cref="ICacheService"/>.
/// Maintains a thread-safe key registry to support prefix-based invalidation,
/// which IMemoryCache does not support natively.
/// </summary>
public sealed class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly CacheSettings _settings;

    // Thread-safe registry of every key currently stored in the cache.
    // byte value is always 0 — the dictionary is used as a set.
    private readonly ConcurrentDictionary<string, byte> _keyRegistry = new();

    public CacheService(IMemoryCache cache, IOptions<CacheSettings> settings)
    {
        _cache    = cache;
        _settings = settings.Value;
    }

    /// <inheritdoc />
    public bool TryGetValue<T>(string key, out T? value)
    {
        if (_cache.TryGetValue(key, out object? cached) && cached is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }

    /// <inheritdoc />
    public void Set<T>(string key, T value, TimeSpan absoluteExpiration)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration,
            Size = 1 // each entry counts as 1 unit toward SizeLimit
        };

        // Remove from registry when the entry expires naturally
        options.RegisterPostEvictionCallback((evictedKey, _, _, _) =>
        {
            _keyRegistry.TryRemove(evictedKey.ToString()!, out _);
        });

        _cache.Set(key, value, options);
        _keyRegistry[key] = 0;
    }

    /// <inheritdoc />
    public void Remove(string key)
    {
        _cache.Remove(key);
        _keyRegistry.TryRemove(key, out _);
    }

    /// <inheritdoc />
    public void RemoveByPrefix(string keyPrefix)
    {
        // Snapshot the current keys to avoid modifying the collection while iterating
        var matchedKeys = _keyRegistry.Keys
            .Where(k => k.StartsWith(keyPrefix, StringComparison.Ordinal))
            .ToList();

        foreach (var key in matchedKeys)
        {
            _cache.Remove(key);
            _keyRegistry.TryRemove(key, out _);
        }
    }
}
