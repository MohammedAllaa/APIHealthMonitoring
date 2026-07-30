using APIHealthMonitoring.Application.DTOs.Notifications;

namespace APIHealthMonitoring.Application.Interfaces.Notifications;

/// <summary>
/// Low-level abstraction for sending a pre-built email message via SMTP.
/// Implementations should handle connection management and graceful error handling.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends the given <paramref name="message"/> via the configured SMTP relay.
    /// Implementations must not propagate SMTP exceptions — errors should be logged and swallowed.
    /// </summary>
    /// <param name="message">The email message to send, including recipients, subject, and HTML body.</param>
    /// <param name="ct">Cancellation token to observe during the async SMTP operation.</param>
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}
