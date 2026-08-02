using APIHealthMonitoring.Application.DTOs.Notifications;
using APIHealthMonitoring.Application.Interfaces.Notifications;
using APIHealthMonitoring.Application.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace APIHealthMonitoring.Infrastructure.Notifications.Services;

/// <summary>
/// SMTP email sender implementation using <b>MailKit</b> connected to the Brevo relay.
/// Reads all connection and authentication settings from <see cref="EmailSettings"/>.
///
/// <para>
/// SMTP failures are caught, logged, and swallowed so that a mail delivery issue
/// never propagates to the monitoring pipeline or crashes the background service.
/// </para>
/// </summary>
public sealed class EmailSender : IEmailSender
{
    private readonly EmailSettings                  _settings;
    private readonly ILogger<EmailSender>           _logger;

    /// <summary>
    /// Initialises a new <see cref="EmailSender"/> with injected settings and logger.
    /// </summary>
    public EmailSender(
        IOptions<EmailSettings>     settings,
        ILogger<EmailSender>        logger)
    {
        _settings = settings.Value;
        _logger   = logger;
    }

    /// <inheritdoc />
    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        if (message.To.Count == 0)
        {
            _logger.LogWarning("EmailSender: No recipients configured. Skipping email send.");
            return;
        }

        try
        {
            var mimeMessage = BuildMimeMessage(message);

            using var client = new SmtpClient();

            // Connect with STARTTLS on port 587
            await client.ConnectAsync(
                _settings.SmtpServer,
                _settings.Port,
                SecureSocketOptions.StartTls,
                ct);

            await client.AuthenticateAsync(
                _settings.Username,
                _settings.Password,
                ct);

            await client.SendAsync(mimeMessage, ct);

            await client.DisconnectAsync(quit: true, ct);

            _logger.LogInformation(
                "EmailSender: Critical alert email sent successfully to {RecipientCount} recipient(s) for subject: {Subject}",
                message.To.Count,
                message.Subject);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("EmailSender: Email send was cancelled.");
        }
        catch (Exception ex)
        {
            // Log and swallow — SMTP failures must never crash the monitoring pipeline
            _logger.LogError(
                ex,
                "EmailSender: Failed to send email with subject '{Subject}'. The monitoring pipeline will continue.",
                message.Subject);
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private MimeMessage BuildMimeMessage(EmailMessage message)
    {
        var mime = new MimeMessage();

        mime.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));

        foreach (var recipient in message.To)
            mime.To.Add(MailboxAddress.Parse(recipient));

        mime.Subject = message.Subject;

        mime.Body = new TextPart("html")
        {
            Text = message.HtmlBody
        };

        return mime;
    }
}
