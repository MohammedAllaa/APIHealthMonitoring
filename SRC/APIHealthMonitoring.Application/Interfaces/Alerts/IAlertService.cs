using APIHealthMonitoring.Application.DTOs.Alerts;
using APIHealthMonitoring.Application.Specifications;

namespace APIHealthMonitoring.Application.Interfaces.Alerts;

/// <summary>
/// Service contract for querying, resolving, and managing alert lifecycle.
/// Alert creation is handled internally via <see cref="IAlertEvaluator"/>.
/// </summary>
public interface IAlertService
{
    /// <summary>Returns a paged, filtered list of alerts.</summary>
    Task<PaginatedResult<AlertResponseDto>> GetPagedAsync(AlertPagedRequestDto request, CancellationToken ct = default);

    /// <summary>Returns a single alert by ID.</summary>
    Task<AlertResponseDto> GetByIdAsync(int alertId, CancellationToken ct = default);

    /// <summary>Resolves an open alert, marking it as Closed.</summary>
    Task<AlertResponseDto> ResolveAsync(int alertId, ResolveAlertDto dto, CancellationToken ct = default);

    /// <summary>
    /// Creates a new alert for an API endpoint.
    /// Called only by the Health Check Engine — checks for duplicate open alerts first.
    /// </summary>
    Task CreateIfNotDuplicateAsync(int apiEndpointId, Domain.Enums.AlertSeverity severity, string message, CancellationToken ct = default);

    /// <summary>Auto-resolves all open alerts for an API endpoint (called when API becomes Healthy).</summary>
    Task AutoResolveForEndpointAsync(int apiEndpointId, CancellationToken ct = default);
}
