using APIHealthMonitoring.Application.DTOs.Reports;
using APIHealthMonitoring.Application.Interfaces;
using APIHealthMonitoring.Application.Interfaces.Reporting;
using APIHealthMonitoring.Application.Specifications.Endpoints;
using APIHealthMonitoring.Application.Specifications.HealthChecks;
using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Infrastructure.Reporting.Services;

public class ReportingService : IReportingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAvailabilityCalculator _calculator;

    public ReportingService(IUnitOfWork unitOfWork, IAvailabilityCalculator calculator)
    {
        _unitOfWork = unitOfWork;
        _calculator = calculator;
    }

    public async Task<List<DailyHealthReportDto>> GetDailyReportAsync(DateTime date, int? apiEndpointId = null, CancellationToken ct = default)
    {
        var targetDate = date.Date;
        var dayEnd = targetDate.AddDays(1);

        List<ApiEndpoint> endpoints;
        if (apiEndpointId.HasValue)
        {
            var ep = await _unitOfWork.Repository<ApiEndpoint>().GetByIdAsync(apiEndpointId.Value, ct);
            endpoints = ep != null ? new List<ApiEndpoint> { ep } : new List<ApiEndpoint>();
        }
        else
        {
            var spec = new ActiveApiEndpointsSpec();
            endpoints = (await _unitOfWork.Repository<ApiEndpoint>().GetAllWithSpecAsync(spec, ct)).ToList();
        }

        var reports = new List<DailyHealthReportDto>();

        foreach (var endpoint in endpoints)
        {
            var spec = new HealthChecksInDateRangeSpec(endpoint.Id, targetDate, dayEnd);
            var checks = await _unitOfWork.Repository<HealthCheck>().GetAllWithSpecAsync(spec, ct);

            var (availability, avgResponseTime, total, success, failed) = _calculator.Calculate(checks);

            reports.Add(new DailyHealthReportDto
            {
                ApiId = endpoint.Id,
                ApiName = endpoint.Name,
                ReportDate = targetDate,
                TotalChecks = total,
                SuccessfulChecks = success,
                FailedChecks = failed,
                AvailabilityPercentage = availability,
                AvgResponseTimeMs = avgResponseTime
            });
        }

        return reports;
    }

    public async Task<List<WeeklyTrendReportDto>> GetWeeklyTrendReportAsync(DateTime weekStart, CancellationToken ct = default)
    {
        // Adjust weekStart to nearest preceding Monday if needed
        int diff = (7 + (weekStart.DayOfWeek - DayOfWeek.Monday)) % 7;
        var monday = weekStart.Date.AddDays(-1 * diff);
        var sundayEnd = monday.AddDays(7);

        var spec = new ActiveApiEndpointsSpec();
        var endpoints = await _unitOfWork.Repository<ApiEndpoint>().GetAllWithSpecAsync(spec, ct);

        var reports = new List<WeeklyTrendReportDto>();

        foreach (var endpoint in endpoints)
        {
            var dailyPoints = new List<DailyAvailabilityPointDto>();

            for (int i = 0; i < 7; i++)
            {
                var day = monday.AddDays(i);
                var dayNext = day.AddDays(1);

                var daySpec = new HealthChecksInDateRangeSpec(endpoint.Id, day, dayNext);
                var checks = await _unitOfWork.Repository<HealthCheck>().GetAllWithSpecAsync(daySpec, ct);

                var (avail, avgMs, total, _, _) = _calculator.Calculate(checks);

                dailyPoints.Add(new DailyAvailabilityPointDto
                {
                    Date = day,
                    AvailabilityPercentage = avail,
                    AvgResponseTimeMs = avgMs,
                    TotalChecks = total
                });
            }

            reports.Add(new WeeklyTrendReportDto
            {
                ApiId = endpoint.Id,
                ApiName = endpoint.Name,
                WeekStartDate = monday,
                WeekEndDate = sundayEnd.AddDays(-1),
                DailyAvailability = dailyPoints
            });
        }

        return reports;
    }

    public async Task<MonthlyPerformanceReportDto> GetMonthlyPerformanceReportAsync(int year, int month, CancellationToken ct = default)
    {
        int currentYear = DateTime.UtcNow.Year;
        if (year < 2020 || year > currentYear)
        {
            throw new InvalidOperationException($"Year must be between 2020 and {currentYear}.");
        }
        if (month < 1 || month > 12)
        {
            throw new InvalidOperationException("Month must be between 1 and 12.");
        }

        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var spec = new ActiveApiEndpointsSpec();
        var endpoints = await _unitOfWork.Repository<ApiEndpoint>().GetAllWithSpecAsync(spec, ct);

        var items = new List<ApiPerformanceSummaryItemDto>();

        foreach (var endpoint in endpoints)
        {
            var monthSpec = new HealthChecksInDateRangeSpec(endpoint.Id, monthStart, monthEnd);
            var checks = await _unitOfWork.Repository<HealthCheck>().GetAllWithSpecAsync(monthSpec, ct);

            var (avail, avgMs, _, _, failed) = _calculator.Calculate(checks);

            items.Add(new ApiPerformanceSummaryItemDto
            {
                ApiId = endpoint.Id,
                ApiName = endpoint.Name,
                AvailabilityPercentage = avail,
                AvgResponseTimeMs = avgMs,
                TotalFailures = failed
            });
        }

        return new MonthlyPerformanceReportDto
        {
            Year = year,
            Month = month,
            Top10SlowestApis = items.OrderByDescending(i => i.AvgResponseTimeMs).Take(10).ToList(),
            MostFailedApis = items.OrderByDescending(i => i.TotalFailures).Take(10).ToList(),
            HighestAvailabilityApis = items.OrderByDescending(i => i.AvailabilityPercentage).Take(10).ToList(),
            LowestAvailabilityApis = items.OrderBy(i => i.AvailabilityPercentage).Take(10).ToList()
        };
    }
}
