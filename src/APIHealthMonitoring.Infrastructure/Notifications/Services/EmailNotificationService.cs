using APIHealthMonitoring.Application.DTOs.Notifications;
using APIHealthMonitoring.Application.Interfaces.Notifications;
using APIHealthMonitoring.Application.Settings;
using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Domain.Enums;
using APIHealthMonitoring.Infrastructure.Notifications.Templates;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace APIHealthMonitoring.Infrastructure.Notifications.Services;

/// <summary>
/// Orchestrates sending a Critical alert email for a monitored API endpoint.
/// Enforces the "send-once-per-critical-event, reset-on-healthy" rule by
/// consulting <see cref="IEmailNotificationStateTracker"/> before every send.
/// </summary>
public sealed class EmailNotificationService : IEmailNotificationService
{
    private readonly IEmailSender                       _emailSender;
    private readonly IEmailNotificationStateTracker     _stateTracker;
    private readonly EmailSettings                      _settings;
    private readonly ILogger<EmailNotificationService>  _logger;

    /// <summary>
    /// Initialises a new <see cref="EmailNotificationService"/>.
    /// </summary>
    public EmailNotificationService(
        IEmailSender                        emailSender,
        IEmailNotificationStateTracker      stateTracker,
        IOptions<EmailSettings>             settings,
        ILogger<EmailNotificationService>   logger)
    {
        _emailSender  = emailSender;
        _stateTracker = stateTracker;
        _settings     = settings.Value;
        _logger       = logger;
    }

    /// <inheritdoc />
    public async Task NotifyIfCriticalAsync(
        ApiEndpoint     endpoint,
        HealthCheck     result,
        CancellationToken ct = default)
    {
        // Only act on Critical status
        if (endpoint.CurrentStatus != ApiHealthStatus.Critical)
            return;

        // Suppress duplicate notifications — already sent since last healthy reset
        if (_stateTracker.HasNotificationBeenSent(endpoint.Id))
        {
            _logger.LogDebug(
                "EmailNotificationService: Suppressing duplicate Critical email for endpoint '{Name}' (Id={Id}).",
                endpoint.Name, endpoint.Id);
            return;
        }

        if (_settings.Recipients.Count == 0)
        {
            _logger.LogWarning(
                "EmailNotificationService: No recipients configured in EmailSettings. Cannot send alert for '{Name}'.",
                endpoint.Name);
            return;
        }

        // Build the responsive HTML body
        var htmlBody = EmailHtmlTemplateBuilder.Build(
            apiName:       endpoint.Name,
            endpointUrl:   endpoint.HealthEndpoint,
            status:        endpoint.CurrentStatus.ToString(),
            httpStatusCode: result.StatusCode,
            errorMessage:  result.ErrorMessage,
            timestampUtc:  result.CheckedAt);

        var message = new EmailMessage
        {
            To       = _settings.Recipients.AsReadOnly(),
            Subject  = $"🚨 Critical API Alert - {endpoint.Name}",
            HtmlBody = htmlBody
        };

        _logger.LogInformation(
            "EmailNotificationService: Sending Critical alert email for endpoint '{Name}' (Id={Id}) to {Count} recipient(s).",
            endpoint.Name, endpoint.Id, _settings.Recipients.Count);

        await _emailSender.SendAsync(message, ct);

        // Mark as sent regardless of whether the SMTP call succeeded,
        // to avoid a flood of retries on persistent SMTP failure.
        _stateTracker.MarkAsSent(endpoint.Id);
    }

    /// <inheritdoc />
    public void ResetNotificationState(int endpointId)
    {
        _stateTracker.Reset(endpointId);

        _logger.LogDebug(
            "EmailNotificationService: Notification state reset for endpoint Id={Id} (endpoint returned to Healthy).",
            endpointId);
    }
}
