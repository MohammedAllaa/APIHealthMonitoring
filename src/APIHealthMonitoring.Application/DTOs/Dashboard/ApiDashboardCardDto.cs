using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Application.DTOs.Dashboard;

/// <summary>
/// Per-API card displayed in the dashboard API grid view.
/// </summary>
public class ApiDashboardCardDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public ApiHealthStatus CurrentStatus { get; set; }
    public DateTime? LastCheckedAt { get; set; }

    /// <summary>Response time of the most recent check (ms).</summary>
    public int? LastResponseTimeMs { get; set; }

    /// <summary>Success rate for today as a percentage (0.0–100.0).</summary>
    public decimal TodaySuccessRate { get; set; }

    /// <summary>UTC timestamp of the last failed check. Null if no failures today.</summary>
    public DateTime? LastFailureAt { get; set; }

    /// <summary>Number of currently open alerts for this API.</summary>
    public int OpenAlertCount { get; set; }

    public int ConsecutiveFailures { get; set; }
}
