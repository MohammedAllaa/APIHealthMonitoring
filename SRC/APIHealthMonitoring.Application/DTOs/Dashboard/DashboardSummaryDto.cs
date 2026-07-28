using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Application.DTOs.Dashboard;

/// <summary>
/// Overall platform health summary for the main dashboard header.
/// </summary>
public class DashboardSummaryDto
{
    /// <summary>Total registered API endpoints (active + inactive).</summary>
    public int TotalApis { get; set; }

    /// <summary>Number of active APIs currently in Healthy status.</summary>
    public int HealthyCount { get; set; }

    /// <summary>Number of active APIs currently in Warning status.</summary>
    public int WarningCount { get; set; }

    /// <summary>Number of active APIs currently in Critical status.</summary>
    public int CriticalCount { get; set; }

    /// <summary>Number of active APIs with Unknown status (never checked).</summary>
    public int UnknownCount { get; set; }

    /// <summary>Overall availability percentage across all active APIs today (0.0–100.0).</summary>
    public decimal OverallAvailability { get; set; }

    /// <summary>Average response time in milliseconds across all successful checks today.</summary>
    public double AvgResponseTimeMs { get; set; }

    /// <summary>Total number of open alerts across all APIs.</summary>
    public int OpenAlertsCount { get; set; }
}
