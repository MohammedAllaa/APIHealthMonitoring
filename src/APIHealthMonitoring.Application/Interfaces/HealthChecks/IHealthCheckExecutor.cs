using APIHealthMonitoring.Application.DTOs.HealthChecks;
using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Application.Interfaces.HealthChecks;

/// <summary>
/// Executes a single HTTP health check against an API endpoint, recording all metrics.
/// </summary>
public interface IHealthCheckExecutor
{
    /// <summary>
    /// Performs an HTTP request to the endpoint's health URL,
    /// measures response time, status code, response size, and errors.
    /// Never throws — all errors are captured as unsuccessful results.
    /// </summary>
    Task<HealthCheck> ExecuteAsync(ApiEndpoint endpoint, CancellationToken ct = default);
}
