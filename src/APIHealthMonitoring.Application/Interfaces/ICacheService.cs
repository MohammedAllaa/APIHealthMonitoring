namespace APIHealthMonitoring.Application.Interfaces;

/// <summary>
/// Abstraction over IMemoryCache that adds prefix-based bulk invalidation
/// and a consistent API for the cache-aside pattern.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Try to retrieve a cached value. Returns <c>true</c> when the key is found.
    /// </summary>
    bool TryGetValue<T>(string key, out T? value);

    /// <summary>
    /// Store a value with an absolute expiration relative to now.
    /// </summary>
    void Set<T>(string key, T value, TimeSpan absoluteExpiration);

    /// <summary>Remove a single cache entry by exact key.</summary>
    void Remove(string key);

    /// <summary>
    /// Remove all entries whose key starts with <paramref name="keyPrefix"/>.
    /// Used to invalidate a family of per-entity entries at once.
    /// </summary>
    void RemoveByPrefix(string keyPrefix);
}
