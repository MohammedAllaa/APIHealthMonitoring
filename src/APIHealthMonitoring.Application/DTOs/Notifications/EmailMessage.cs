namespace APIHealthMonitoring.Application.DTOs.Notifications;

/// <summary>
/// Data transfer object representing a single outbound email message.
/// Passed from <c>IEmailNotificationService</c> to <c>IEmailSender</c>.
/// </summary>
public class EmailMessage
{
    /// <summary>One or more recipient email addresses.</summary>
    public IReadOnlyList<string> To { get; init; } = Array.Empty<string>();

    /// <summary>The email subject line.</summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>Full HTML body of the email.</summary>
    public string HtmlBody { get; init; } = string.Empty;
}
