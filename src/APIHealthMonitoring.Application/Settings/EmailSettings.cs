namespace APIHealthMonitoring.Application.Settings;

/// <summary>
/// Holds all SMTP and sender configuration for outgoing email notifications.
/// Bind from <c>appsettings.json</c> under the key <c>"EmailSettings"</c>.
/// </summary>
public class EmailSettings
{
    /// <summary>Section key used for configuration binding.</summary>
    public const string SectionName = "EmailSettings";

    /// <summary>Display name that appears in the email "From" field.</summary>
    public string SenderName { get; init; } = string.Empty;

    /// <summary>Email address used as the "From" address.</summary>
    public string SenderEmail { get; init; } = string.Empty;

    /// <summary>Hostname of the SMTP relay server (e.g. smtp-relay.brevo.com).</summary>
    public string SmtpServer { get; init; } = string.Empty;

    /// <summary>TCP port for the SMTP connection (typically 587 for STARTTLS).</summary>
    public int Port { get; init; } = 587;

    /// <summary>SMTP authentication username.</summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>SMTP authentication password or API key.</summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// List of recipient email addresses that receive critical alert notifications.
    /// Supports multiple recipients.
    /// </summary>
    public List<string> Recipients { get; init; } = new();
}
