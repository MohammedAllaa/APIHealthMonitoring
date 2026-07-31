namespace APIHealthMonitoring.Application.DTOs.Reports;

/// <summary>
/// Summary item for monthly ranking/top metrics.
/// </summary>
public class ApiPerformanceSummaryItemDto
{
    public int ApiId { get; set; }
    public string ApiName { get; set; } = string.Empty;
    public decimal AvailabilityPercentage { get; set; }
    public double AvgResponseTimeMs { get; set; }
    public int TotalFailures { get; set; }
}

/// <summary>
/// Monthly performance report featuring top/bottom rankings.
/// </summary>
public class MonthlyPerformanceReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public List<ApiPerformanceSummaryItemDto> Top10SlowestApis { get; set; } = new();
    public List<ApiPerformanceSummaryItemDto> MostFailedApis { get; set; } = new();
    public List<ApiPerformanceSummaryItemDto> HighestAvailabilityApis { get; set; } = new();
    public List<ApiPerformanceSummaryItemDto> LowestAvailabilityApis { get; set; } = new();
}
