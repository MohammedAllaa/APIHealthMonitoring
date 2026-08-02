using APIHealthMonitoring.Application.DTOs.Dashboard;
using APIHealthMonitoring.Application.Specifications;

namespace APIHealthMonitoring.Application.Interfaces.Dashboard;

public interface IDashboardService
{
    /// <summary>Returns overall platform health summary metrics.</summary>
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default);

    /// <summary>Returns paged dashboard cards for active API endpoints.</summary>
    Task<PaginatedResult<ApiDashboardCardDto>> GetApiCardsAsync(int pageIndex, int pageSize, CancellationToken ct = default);

    /// <summary>Returns historical performance stats for a single API endpoint.</summary>
    Task<ApiHistoricalStatsDto> GetEndpointStatsAsync(int apiEndpointId, CancellationToken ct = default);
}
