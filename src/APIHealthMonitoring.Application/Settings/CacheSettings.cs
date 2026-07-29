namespace APIHealthMonitoring.Application.Settings;

/// <summary>
/// Configures TTL and size limits for the in-memory cache.
/// Bind from appsettings.json under "CacheSettings".
/// </summary>
public class CacheSettings
{
    public const string SectionName = "CacheSettings";

    /// <summary>TTL for the dashboard summary in seconds. Default: 30 s.</summary>
    public int DashboardSummaryExpirationSeconds { get; init; } = 30;

    /// <summary>TTL for the API dashboard cards list in seconds. Default: 30 s.</summary>
    public int DashboardApiCardsExpirationSeconds { get; init; } = 30;

    /// <summary>TTL for per-endpoint historical stats in seconds. Default: 60 s.</summary>
    public int ApiStatsExpirationSeconds { get; init; } = 60;

    /// <summary>TTL for the active-endpoints list in seconds. Default: 120 s.</summary>
    public int ActiveEndpointsExpirationSeconds { get; init; } = 120;

    /// <summary>
    /// Maximum number of cache entries (size units).
    /// Each entry counts as 1 unit. Default: 1024 entries.
    /// </summary>
    public long SizeLimit { get; init; } = 1024;
}
