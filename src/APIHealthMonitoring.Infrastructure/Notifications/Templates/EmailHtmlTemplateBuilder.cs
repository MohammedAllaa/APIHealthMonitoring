namespace APIHealthMonitoring.Infrastructure.Notifications.Templates;

/// <summary>
/// Builds a modern, responsive HTML email template for Critical API alert notifications.
/// All styles are inlined for maximum email client compatibility.
/// </summary>
public static class EmailHtmlTemplateBuilder
{
    /// <summary>
    /// Generates a fully-formatted HTML email body for a Critical API alert.
    /// </summary>
    /// <param name="apiName">Human-readable name of the monitored API.</param>
    /// <param name="endpointUrl">The URL that was checked (HealthEndpoint).</param>
    /// <param name="status">The current health status string (e.g. "Critical").</param>
    /// <param name="httpStatusCode">The HTTP status code returned, or null if unreachable.</param>
    /// <param name="errorMessage">The error description, or null if none.</param>
    /// <param name="timestampUtc">The UTC timestamp of the health check that triggered this alert.</param>
    /// <returns>A complete HTML document string suitable for use as an email body.</returns>
    public static string Build(
        string   apiName,
        string   endpointUrl,
        string   status,
        int?     httpStatusCode,
        string?  errorMessage,
        DateTime timestampUtc)
    {
        var statusCodeDisplay = httpStatusCode.HasValue
            ? httpStatusCode.Value.ToString()
            : "N/A (Unreachable)";

        var errorDisplay = string.IsNullOrWhiteSpace(errorMessage)
            ? "No additional error details available."
            : errorMessage;

        var timestamp = timestampUtc.ToString("yyyy-MM-dd HH:mm:ss") + " UTC";

        return $"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="UTF-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1.0" />
          <title>Critical API Alert</title>
        </head>
        <body style="margin:0;padding:0;background-color:#0f1117;font-family:'Segoe UI',Arial,sans-serif;">
          <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="background-color:#0f1117;padding:32px 16px;">
            <tr>
              <td align="center">
                <table role="presentation" cellpadding="0" cellspacing="0" width="600" style="max-width:600px;background-color:#1a1d27;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.4);">

                  <!-- HEADER -->
                  <tr>
                    <td style="background:linear-gradient(135deg,#dc2626,#991b1b);padding:32px 40px;text-align:center;">
                      <div style="font-size:40px;margin-bottom:12px;">🚨</div>
                      <h1 style="margin:0;color:#ffffff;font-size:22px;font-weight:700;letter-spacing:0.5px;">
                        Critical API Alert
                      </h1>
                      <p style="margin:8px 0 0;color:#fca5a5;font-size:14px;">
                        Immediate attention required
                      </p>
                    </td>
                  </tr>

                  <!-- ALERT BADGE -->
                  <tr>
                    <td style="padding:28px 40px 0;text-align:center;">
                      <span style="display:inline-block;background-color:#450a0a;color:#f87171;border:1px solid #dc2626;border-radius:6px;padding:6px 20px;font-size:13px;font-weight:600;letter-spacing:1px;text-transform:uppercase;">
                        ● {status}
                      </span>
                    </td>
                  </tr>

                  <!-- API NAME -->
                  <tr>
                    <td style="padding:16px 40px 0;text-align:center;">
                      <h2 style="margin:0;color:#f1f5f9;font-size:20px;font-weight:700;">
                        {EscapeHtml(apiName)}
                      </h2>
                    </td>
                  </tr>

                  <!-- DIVIDER -->
                  <tr>
                    <td style="padding:24px 40px 0;">
                      <div style="height:1px;background-color:#2d3149;"></div>
                    </td>
                  </tr>

                  <!-- DETAILS TABLE -->
                  <tr>
                    <td style="padding:24px 40px;">
                      <table role="presentation" cellpadding="0" cellspacing="0" width="100%">

                        <!-- Endpoint URL -->
                        <tr>
                          <td style="padding:10px 0;border-bottom:1px solid #1e2235;">
                            <table role="presentation" cellpadding="0" cellspacing="0" width="100%">
                              <tr>
                                <td style="color:#94a3b8;font-size:12px;font-weight:600;text-transform:uppercase;letter-spacing:0.8px;width:140px;vertical-align:top;padding-top:2px;">
                                  Endpoint URL
                                </td>
                                <td style="color:#e2e8f0;font-size:14px;word-break:break-all;">
                                  {EscapeHtml(endpointUrl)}
                                </td>
                              </tr>
                            </table>
                          </td>
                        </tr>

                        <!-- Status -->
                        <tr>
                          <td style="padding:10px 0;border-bottom:1px solid #1e2235;">
                            <table role="presentation" cellpadding="0" cellspacing="0" width="100%">
                              <tr>
                                <td style="color:#94a3b8;font-size:12px;font-weight:600;text-transform:uppercase;letter-spacing:0.8px;width:140px;">
                                  Current Status
                                </td>
                                <td style="color:#f87171;font-size:14px;font-weight:600;">
                                  {EscapeHtml(status)}
                                </td>
                              </tr>
                            </table>
                          </td>
                        </tr>

                        <!-- HTTP Status Code -->
                        <tr>
                          <td style="padding:10px 0;border-bottom:1px solid #1e2235;">
                            <table role="presentation" cellpadding="0" cellspacing="0" width="100%">
                              <tr>
                                <td style="color:#94a3b8;font-size:12px;font-weight:600;text-transform:uppercase;letter-spacing:0.8px;width:140px;">
                                  HTTP Status Code
                                </td>
                                <td style="color:#e2e8f0;font-size:14px;">
                                  {EscapeHtml(statusCodeDisplay)}
                                </td>
                              </tr>
                            </table>
                          </td>
                        </tr>

                        <!-- Error Message -->
                        <tr>
                          <td style="padding:10px 0;border-bottom:1px solid #1e2235;">
                            <table role="presentation" cellpadding="0" cellspacing="0" width="100%">
                              <tr>
                                <td style="color:#94a3b8;font-size:12px;font-weight:600;text-transform:uppercase;letter-spacing:0.8px;width:140px;vertical-align:top;padding-top:2px;">
                                  Error Details
                                </td>
                                <td style="color:#fca5a5;font-size:14px;word-break:break-word;">
                                  {EscapeHtml(errorDisplay)}
                                </td>
                              </tr>
                            </table>
                          </td>
                        </tr>

                        <!-- Timestamp -->
                        <tr>
                          <td style="padding:10px 0;">
                            <table role="presentation" cellpadding="0" cellspacing="0" width="100%">
                              <tr>
                                <td style="color:#94a3b8;font-size:12px;font-weight:600;text-transform:uppercase;letter-spacing:0.8px;width:140px;">
                                  Detected At
                                </td>
                                <td style="color:#e2e8f0;font-size:14px;">
                                  {EscapeHtml(timestamp)}
                                </td>
                              </tr>
                            </table>
                          </td>
                        </tr>

                      </table>
                    </td>
                  </tr>

                  <!-- FOOTER -->
                  <tr>
                    <td style="background-color:#12151f;padding:20px 40px;text-align:center;border-top:1px solid #1e2235;">
                      <p style="margin:0;color:#475569;font-size:12px;line-height:1.6;">
                        This alert was generated automatically by the
                        <strong style="color:#64748b;">API Health Monitoring System</strong>.
                        <br/>Please do not reply to this email.
                      </p>
                      <p style="margin:8px 0 0;color:#334155;font-size:11px;">
                        © {DateTime.UtcNow.Year} Tanta University — API Health Monitoring
                      </p>
                    </td>
                  </tr>

                </table>
              </td>
            </tr>
          </table>
        </body>
        </html>
        """;
    }

    /// <summary>Escapes characters that have special meaning in HTML.</summary>
    private static string EscapeHtml(string input) =>
        input
            .Replace("&",  "&amp;")
            .Replace("<",  "&lt;")
            .Replace(">",  "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'",  "&#39;");
}
