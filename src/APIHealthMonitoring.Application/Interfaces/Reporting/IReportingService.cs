using APIHealthMonitoring.Application.DTOs.Reports;

namespace APIHealthMonitoring.Application.Interfaces.Reporting;

public interface IReportingService
{
    /// <summary>Generates daily health report for a specific date and optional API endpoint filter.</summary>
    Task<List<DailyHealthReportDto>> GetDailyReportAsync(DateTime date, int? apiEndpointId = null, CancellationToken ct = default);

    /// <summary>Generates weekly trend report starting from a specified week start Monday.</summary>
    Task<List<WeeklyTrendReportDto>> GetWeeklyTrendReportAsync(DateTime weekStart, CancellationToken ct = default);

    /// <summary>Generates monthly performance report ranking APIs for a specified month and year.</summary>
    Task<MonthlyPerformanceReportDto> GetMonthlyPerformanceReportAsync(int year, int month, CancellationToken ct = default);
}
