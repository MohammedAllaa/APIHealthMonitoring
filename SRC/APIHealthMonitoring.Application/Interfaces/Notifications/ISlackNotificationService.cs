using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Application.Interfaces.Notifications;

/// <summary>
/// Contract for sending Slack webhook notifications when alerts are created or resolved.
/// </summary>
public interface ISlackNotificationService
{
    /// <summary>
    /// Sends a Slack notification for an alert event.
    /// </summary>
    /// <param name="endpointName">Human-readable name of the monitored API endpoint.</param>
    /// <param name="severity">Severity of the alert (Warning / Critical). Ignored when <paramref name="isResolved"/> is true.</param>
    /// <param name="message">The alert message body.</param>
    /// <param name="isResolved">
    /// When <c>true</c>, a green "Resolved" message is sent instead of an alert message.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    Task SendAlertNotificationAsync(
        string        endpointName,
        AlertSeverity severity,
        string        message,
        bool          isResolved,
        CancellationToken ct = default);
}
