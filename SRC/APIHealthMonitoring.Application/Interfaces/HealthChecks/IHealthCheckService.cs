using APIHealthMonitoring.Application.DTOs.HealthChecks;
using APIHealthMonitoring.Application.Specifications;
using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Application.Interfaces.HealthChecks;

/// <summary>
/// Orchestrates saving health check results, updating cached endpoint status,
/// and triggering alert evaluation after each check.
/// </summary>
public interface IHealthCheckService
{
    /// <summary>Saves a health check result and updates the endpoint's cached status fields.</summary>
    Task SaveResultAsync(HealthCheck result, CancellationToken ct = default);

    /// <summary>Returns paged health check history with optional filters.</summary>
    Task<PaginatedResult<HealthCheckResultDto>> GetPagedAsync(HealthCheckPagedRequestDto request, CancellationToken ct = default);

    /// <summary>Returns a single health check record by ID.</summary>
    Task<HealthCheckResultDto> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Returns the current health summary for one API endpoint.</summary>
    Task<ApiHealthSummaryDto> GetEndpointSummaryAsync(int apiEndpointId, CancellationToken ct = default);

    /// <summary>Immediately triggers a health check for the specified API endpoint.</summary>
    Task<HealthCheckResultDto> TriggerManualCheckAsync(int apiEndpointId, CancellationToken ct = default);
}
