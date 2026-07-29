namespace APIHealthMonitoring.Application.DTOs.Reports;

/// <summary>
/// Daily health report for a specific API or all APIs.
/// </summary>
public class DailyHealthReportDto
{
    public int ApiId { get; set; }
    public string ApiName { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public int TotalChecks { get; set; }
    public int SuccessfulChecks { get; set; }
    public int FailedChecks { get; set; }
    public decimal AvailabilityPercentage { get; set; }
    public double AvgResponseTimeMs { get; set; }
}
