namespace APIHealthMonitoring.Application.DTOs.Dashboard;

/// <summary>Full historical statistics for a single API endpoint.</summary>
public class ApiHistoricalStatsDto
{
    public int ApiEndpointId { get; set; }
    public string ApiName { get; set; } = string.Empty;
    public DateTime? LastCheckedAt { get; set; }

    /// <summary>Availability % for today (UTC calendar day).</summary>
    public decimal DailyAvailability { get; set; }

    /// <summary>Availability % for the last 7 days.</summary>
    public decimal WeeklyAvailability { get; set; }

    /// <summary>Availability % for the last 30 days.</summary>
    public decimal MonthlyAvailability { get; set; }

    /// <summary>Average response time across all checks in the last 30 days (ms).</summary>
    public double AvgResponseTimeMs { get; set; }

    /// <summary>Fastest successful response time in the last 30 days (ms).</summary>
    public int FastestResponseMs { get; set; }

    /// <summary>Slowest response time in the last 30 days (ms).</summary>
    public int SlowestResponseMs { get; set; }

    /// <summary>Total failure count in the last 30 days.</summary>
    public int FailureCount { get; set; }

    /// <summary>Total checks in the last 30 days.</summary>
    public int TotalChecks { get; set; }
}
