namespace APIHealthMonitoring.Infrastructure.Notifications.Settings;

/// <summary>
/// Strongly-typed settings bound from the <c>SlackSettings</c> section of appsettings.json.
/// </summary>
public sealed class SlackSettings
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "SlackSettings";

    /// <summary>Slack incoming webhook URL.</summary>
    public string WebhookUrl { get; init; } = string.Empty;

    /// <summary>
    /// Master switch. When <c>false</c> no HTTP calls are made to Slack.
    /// Useful for disabling notifications in Development without removing config.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
