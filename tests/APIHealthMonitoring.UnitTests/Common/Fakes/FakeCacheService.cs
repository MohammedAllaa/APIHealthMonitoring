using APIHealthMonitoring.Application.Interfaces;

namespace APIHealthMonitoring.UnitTests.Common.Fakes;

/// <summary>
/// Simple in-memory dictionary-backed cache service for unit tests.
/// </summary>
public class FakeCacheService : ICacheService
{
    private readonly Dictionary<string, object> _cache = new();

    public bool TryGetValue<T>(string key, out T? value)
    {
        if (_cache.TryGetValue(key, out var cachedValue) && cachedValue is T typedValue)
        {
            value = typedValue;
            return true;
        }
        
        value = default;
        return false;
    }

    public void Set<T>(string key, T value, TimeSpan absoluteExpiration)
    {
        if (value is not null)
        {
            _cache[key] = value;
        }
        else
        {
            _cache.Remove(key);
        }
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }

    public void RemoveByPrefix(string keyPrefix)
    {
        var keysToRemove = _cache.Keys
            .Where(k => k.StartsWith(keyPrefix, StringComparison.Ordinal))
            .ToList();
            
        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
        }
    }

    // Diagnostic method to check if an entry exists
    public bool ContainsKey(string key) => _cache.ContainsKey(key);
}
