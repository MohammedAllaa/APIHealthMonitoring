using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Application.Interfaces.Notifications;

/// <summary>
/// Orchestrates sending a Critical alert email notification for a monitored API endpoint.
/// Applies the "send-once, reset-on-healthy" business rule by delegating state tracking
/// to <see cref="IEmailNotificationStateTracker"/>.
/// </summary>
public interface IEmailNotificationService
{
    /// <summary>
    /// Evaluates whether an email notification should be sent for the given
    /// <paramref name="endpoint"/> and <paramref name="result"/>.
    ///
    /// <para>
    /// An email is sent only when:
    /// <list type="bullet">
    ///   <item>The endpoint's <c>CurrentStatus</c> is <c>Critical</c>.</item>
    ///   <item>No notification has been sent since the last <c>Healthy</c> reset.</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="endpoint">The API endpoint whose status has just been evaluated.</param>
    /// <param name="result">The health check result that produced the current status.</param>
    /// <param name="ct">Cancellation token to observe during the send operation.</param>
    Task NotifyIfCriticalAsync(ApiEndpoint endpoint, HealthCheck result, CancellationToken ct = default);

    /// <summary>
    /// Resets the notification state for the given <paramref name="endpointId"/>.
    /// Should be called when the endpoint transitions back to <c>Healthy</c>.
    /// </summary>
    /// <param name="endpointId">The unique identifier of the API endpoint.</param>
    void ResetNotificationState(int endpointId);
}
