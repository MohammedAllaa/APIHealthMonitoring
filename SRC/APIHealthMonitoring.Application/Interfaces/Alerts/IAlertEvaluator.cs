using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Application.Interfaces.Alerts;

/// <summary>
/// Evaluates a health check result against MonitoringConfiguration thresholds
/// and determines which alert (if any) should fire.
/// Called by the Health Check Engine after saving each result.
/// </summary>
public interface IAlertEvaluator
{
    /// <summary>
    /// Inspects the latest health check result and the endpoint's monitoring config,
    /// then creates or auto-resolves alerts accordingly.
    /// </summary>
    Task EvaluateAndAlertAsync(
        ApiEndpoint endpoint,
        HealthCheck result,
        MonitoringConfiguration? config,
        CancellationToken ct = default);
}
