using APIHealthMonitoring.Application.DTOs.MonitoringConfig;

namespace APIHealthMonitoring.Application.Interfaces.Endpoints;

/// <summary>
/// Service contract for managing monitoring configurations per API endpoint.
/// Enforces the business rule that each API endpoint has exactly one configuration.
/// </summary>
public interface IMonitoringConfigService
{
    /// <summary>
    /// Creates a monitoring configuration for an API endpoint if one does not already exist.
    /// </summary>
    Task<MonitoringConfigResponseDto> CreateAsync(CreateMonitoringConfigDto dto, CancellationToken ct = default);

    /// <summary>
    /// Gets the monitoring configuration for a specific API endpoint.
    /// </summary>
    Task<MonitoringConfigResponseDto> GetByEndpointIdAsync(int apiEndpointId, CancellationToken ct = default);

    /// <summary>
    /// Updates the monitoring configuration thresholds for a specific API endpoint.
    /// </summary>
    Task<MonitoringConfigResponseDto> UpdateAsync(int apiEndpointId, UpdateMonitoringConfigDto dto, CancellationToken ct = default);

    /// <summary>
    /// Resets the monitoring configuration for a specific API endpoint back to system default values
    /// (1000ms slow / 2000ms critical / 3 failures / 99.0% availability).
    /// </summary>
    Task<MonitoringConfigResponseDto> ResetToDefaultsAsync(int apiEndpointId, CancellationToken ct = default);
}
