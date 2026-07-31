using APIHealthMonitoring.Application.DTOs.Dashboard;
using APIHealthMonitoring.Application.Interfaces;
using APIHealthMonitoring.Application.Interfaces.Dashboard;
using APIHealthMonitoring.Application.Interfaces.Reporting;
using APIHealthMonitoring.Application.Specifications;
using APIHealthMonitoring.Application.Specifications.Alerts;
using APIHealthMonitoring.Application.Specifications.Endpoints;
using APIHealthMonitoring.Application.Specifications.HealthChecks;
using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Infrastructure.Dashboard.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAvailabilityCalculator _calculator;

    public DashboardService(IUnitOfWork unitOfWork, IAvailabilityCalculator calculator)
    {
        _unitOfWork = unitOfWork;
        _calculator = calculator;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var allEndpoints = await _unitOfWork.Repository<ApiEndpoint>().GetAllAsync(ct);
        var activeEndpoints = allEndpoints.Where(e => e.IsActive).ToList();

        int totalApis = allEndpoints.Count;
        int healthy = activeEndpoints.Count(e => e.CurrentStatus == ApiHealthStatus.Healthy);
        int warning = activeEndpoints.Count(e => e.CurrentStatus == ApiHealthStatus.Warning);
        int critical = activeEndpoints.Count(e => e.CurrentStatus == ApiHealthStatus.Critical);
        int unknown = activeEndpoints.Count(e => e.CurrentStatus == ApiHealthStatus.Unknown);

        // Fetch today's health checks for all active endpoints
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        var todayChecks = new List<HealthCheck>();
        foreach (var endpoint in activeEndpoints)
        {
            var spec = new HealthChecksInDateRangeSpec(endpoint.Id, todayStart, todayEnd);
            var checks = await _unitOfWork.Repository<HealthCheck>().GetAllWithSpecAsync(spec, ct);
            todayChecks.AddRange(checks);
        }

        var (overallAvailability, avgResponseTime, _, _, _) = _calculator.Calculate(todayChecks);

        // Fetch open alerts count
        var openAlerts = await _unitOfWork.Repository<Alert>().GetAllWithSpecAsync(
            new AlertsByApiPaginatedSpec(new Application.DTOs.Alerts.AlertPagedRequestDto { Status = AlertStatus.Open, PageSize = 100 }), ct);

        return new DashboardSummaryDto
        {
            TotalApis = totalApis,
            HealthyCount = healthy,
            WarningCount = warning,
            CriticalCount = critical,
            UnknownCount = unknown,
            OverallAvailability = overallAvailability,
            AvgResponseTimeMs = avgResponseTime,
            OpenAlertsCount = openAlerts.Count
        };
    }

    public async Task<PaginatedResult<ApiDashboardCardDto>> GetApiCardsAsync(int pageIndex, int pageSize, CancellationToken ct = default)
    {
        var activeSpec = new ActiveApiEndpointsSpec();
        var allActive = await _unitOfWork.Repository<ApiEndpoint>().GetAllWithSpecAsync(activeSpec, ct);

        int totalItems = allActive.Count;
        var pagedEndpoints = allActive
            .Skip((Math.Max(pageIndex, 1) - 1) * Math.Min(pageSize, 100))
            .Take(Math.Min(pageSize, 100))
            .ToList();

        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        var cards = new List<ApiDashboardCardDto>();

        foreach (var endpoint in pagedEndpoints)
        {
            // Fetch today's checks
            var todaySpec = new HealthChecksInDateRangeSpec(endpoint.Id, todayStart, todayEnd);
            var todayChecks = await _unitOfWork.Repository<HealthCheck>().GetAllWithSpecAsync(todaySpec, ct);

            var (todaySuccessRate, _, _, _, _) = _calculator.Calculate(todayChecks);

            // Last check & last response time
            var lastCheckSpec = new LastNHealthChecksSpec(endpoint.Id, 1);
            var lastCheckList = await _unitOfWork.Repository<HealthCheck>().GetAllWithSpecAsync(lastCheckSpec, ct);
            var lastCheck = lastCheckList.FirstOrDefault();

            // Last failure
            var failedChecksSpec = new HealthChecksByApiSpec(new Application.DTOs.HealthChecks.HealthCheckPagedRequestDto
            {
                ApiEndpointId = endpoint.Id,
                IsSuccessful = false,
                PageSize = 1
            });
            var failedChecks = await _unitOfWork.Repository<HealthCheck>().GetAllWithSpecAsync(failedChecksSpec, ct);
            var lastFailure = failedChecks.FirstOrDefault()?.CheckedAt;

            // Open alerts
            var openAlertsSpec = new OpenAlertsByEndpointSpec(endpoint.Id);
            var openAlerts = await _unitOfWork.Repository<Alert>().GetAllWithSpecAsync(openAlertsSpec, ct);

            cards.Add(new ApiDashboardCardDto
            {
                Id = endpoint.Id,
                Name = endpoint.Name,
                BaseUrl = endpoint.BaseUrl,
                Environment = endpoint.Environment.ToString(),
                CurrentStatus = endpoint.CurrentStatus,
                LastCheckedAt = endpoint.LastCheckedAt,
                LastResponseTimeMs = lastCheck?.ResponseTimeMs,
                TodaySuccessRate = todaySuccessRate,
                LastFailureAt = lastFailure,
                OpenAlertCount = openAlerts.Count,
                ConsecutiveFailures = endpoint.ConsecutiveFailures
            });
        }

        return new PaginatedResult<ApiDashboardCardDto>(cards, totalItems, pageIndex, pageSize);
    }

    public async Task<ApiHistoricalStatsDto> GetEndpointStatsAsync(int apiEndpointId, CancellationToken ct = default)
    {
        var endpoint = await _unitOfWork.Repository<ApiEndpoint>().GetByIdAsync(apiEndpointId, ct)
            ?? throw new KeyNotFoundException($"API Endpoint {apiEndpointId} not found.");

        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var weekStart = now.AddDays(-7);
        var monthStart = now.AddDays(-30);

        // Daily
        var dailyChecks = await _unitOfWork.Repository<HealthCheck>().GetAllWithSpecAsync(
            new HealthChecksInDateRangeSpec(apiEndpointId, todayStart, now), ct);
        var (dailyAvail, _, _, _, _) = _calculator.Calculate(dailyChecks);

        // Weekly
        var weeklyChecks = await _unitOfWork.Repository<HealthCheck>().GetAllWithSpecAsync(
            new HealthChecksInDateRangeSpec(apiEndpointId, weekStart, now), ct);
        var (weeklyAvail, _, _, _, _) = _calculator.Calculate(weeklyChecks);

        // Monthly (30 days)
        var monthlyChecks = await _unitOfWork.Repository<HealthCheck>().GetAllWithSpecAsync(
            new HealthChecksInDateRangeSpec(apiEndpointId, monthStart, now), ct);
        var (monthlyAvail, avgResponse, totalChecks, _, failedChecks) = _calculator.Calculate(monthlyChecks);

        var successfulMonthly = monthlyChecks.Where(c => c.IsSuccessful).ToList();
        int fastest = successfulMonthly.Any() ? successfulMonthly.Min(c => c.ResponseTimeMs) : 0;
        int slowest = monthlyChecks.Any() ? monthlyChecks.Max(c => c.ResponseTimeMs) : 0;

        return new ApiHistoricalStatsDto
        {
            ApiEndpointId = endpoint.Id,
            ApiName = endpoint.Name,
            LastCheckedAt = endpoint.LastCheckedAt,
            DailyAvailability = dailyAvail,
            WeeklyAvailability = weeklyAvail,
            MonthlyAvailability = monthlyAvail,
            AvgResponseTimeMs = avgResponse,
            FastestResponseMs = fastest,
            SlowestResponseMs = slowest,
            FailureCount = failedChecks,
            TotalChecks = totalChecks
        };
    }
}
