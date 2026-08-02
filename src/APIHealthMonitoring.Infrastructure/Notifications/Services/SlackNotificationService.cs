using System.Net.Http.Json;
using System.Text.Json;
using APIHealthMonitoring.Application.Interfaces.Notifications;
using APIHealthMonitoring.Domain.Enums;
using APIHealthMonitoring.Infrastructure.Notifications.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace APIHealthMonitoring.Infrastructure.Notifications.Services;

/// <summary>
/// Sends Slack Block Kit messages via an incoming webhook URL.
/// Failures are caught and logged — a Slack outage must never interrupt monitoring.
/// </summary>
public sealed class SlackNotificationService : ISlackNotificationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SlackSettings _settings;
    private readonly ILogger<SlackNotificationService> _logger;

    public SlackNotificationService(
        IHttpClientFactory httpClientFactory,
        IOptions<SlackSettings> settings,
        ILogger<SlackNotificationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendAlertNotificationAsync(
        string endpointName,
        AlertSeverity severity,
        string message,
        bool isResolved,
        CancellationToken ct = default)
    {
        if (!_settings.Enabled)
        {
            _logger.LogDebug("Slack notifications are disabled. Skipping message for '{Endpoint}'.", endpointName);
            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.WebhookUrl))
        {
            _logger.LogWarning("SlackSettings.WebhookUrl is not configured. Skipping Slack notification.");
            return;
        }

        try
        {
            var payload = BuildPayload(endpointName, severity, message, isResolved);
            using var client = _httpClientFactory.CreateClient("SlackWebhook");
            using var response = await client.PostAsJsonAsync(_settings.WebhookUrl, payload, ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning(
                    "Slack webhook returned non-success status {Status} for endpoint '{Endpoint}'. Body: {Body}",
                    response.StatusCode, endpointName, body);
            }
            else
            {
                _logger.LogInformation(
                    "Slack notification sent for endpoint '{Endpoint}' (resolved={IsResolved}, severity={Severity}).",
                    endpointName, isResolved, severity);
            }
        }
        catch (Exception ex)
        {
            // Never let a Slack failure propagate and break the monitoring pipeline.
            _logger.LogError(ex,
                "Failed to send Slack notification for endpoint '{Endpoint}'.", endpointName);
        }
    }

    // -------------------------------------------------------------------------
    // Slack Block Kit payload builder
    // -------------------------------------------------------------------------

    private static object BuildPayload(
        string endpointName,
        AlertSeverity severity,
        string message,
        bool isResolved)
    {
        string emoji, statusText, color;

        if (isResolved)
        {
            emoji = "✅";
            statusText = "Resolved";
            color = "#2eb886"; // green
        }
        else
        {
            (emoji, statusText, color) = severity switch
            {
                AlertSeverity.Critical => ("🔴", "CRITICAL", "#e01e5a"),
                AlertSeverity.High => ("🟠", "HIGH", "#e06b1e"),
                AlertSeverity.Medium => ("🟡", "MEDIUM", "#ecb22e"),
                _ => ("🟡", "WARNING", "#ecb22e"),
            };
        }

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC";

        return new
        {
            attachments = new[]
            {
                new
                {
                    color,
                    blocks = new object[]
                    {
                        new
                        {
                            type = "header",
                            text = new
                            {
                                type  = "plain_text",
                                text  = $"{emoji} API Health Alert — {statusText}",
                                emoji = true
                            }
                        },
                        new
                        {
                            type   = "section",
                            fields = new[]
                            {
                                new { type = "mrkdwn", text = $"*Endpoint:*\n{endpointName}" },
                                new { type = "mrkdwn", text = $"*Status:*\n{statusText}" }
                            }
                        },
                        new
                        {
                            type = "section",
                            text = new { type = "mrkdwn", text = $"*Details:*\n{message}" }
                        },
                        new
                        {
                            type     = "context",
                            elements = new[]
                            {
                                new { type = "mrkdwn", text = $"🕒 {timestamp} | API Health Monitoring System" }
                            }
                        },
                        new { type = "divider" }
                    }
                }
            }
        };
    }
}
