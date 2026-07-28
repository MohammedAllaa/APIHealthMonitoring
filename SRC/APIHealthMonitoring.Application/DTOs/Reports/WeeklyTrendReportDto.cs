namespace APIHealthMonitoring.Application.DTOs.Reports;

/// <summary>
/// Daily data point within a weekly trend report.
/// </summary>
public class DailyAvailabilityPointDto
{
    public DateTime Date { get; set; }
    public decimal AvailabilityPercentage { get; set; }
    public double AvgResponseTimeMs { get; set; }
    public int TotalChecks { get; set; }
}

/// <summary>
/// Weekly availability and response time trend report per API.
/// </summary>
public class WeeklyTrendReportDto
{
    public int ApiId { get; set; }
    public string ApiName { get; set; } = string.Empty;
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public List<DailyAvailabilityPointDto> DailyAvailability { get; set; } = new();
}
