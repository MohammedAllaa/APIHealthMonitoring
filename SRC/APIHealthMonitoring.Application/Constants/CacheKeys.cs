namespace APIHealthMonitoring.Application.Constants;

/// <summary>
/// Centralised cache key constants used across the application.
/// All keys are prefixed to avoid collisions when the cache is shared.
/// </summary>
public static class CacheKeys
{
    /// <summary>Top-level dashboard summary (total counts, availability, open alerts).</summary>
    public const string DashboardSummary = "cache:dashboard:summary";

    /// <summary>Paged list of API dashboard cards.</summary>
    public const string DashboardApiCards = "cache:dashboard:api-cards";

    /// <summary>
    /// Prefix for per-endpoint historical stats.
    /// Append the endpoint ID to form the full key, e.g. "cache:api:42".
    /// </summary>
    public const string ApiStatsPrefix = "cache:api:";

    /// <summary>All currently active API endpoints.</summary>
    public const string ActiveEndpoints = "cache:endpoints:active";
}
