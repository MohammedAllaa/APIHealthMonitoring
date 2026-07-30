using System.Collections.Concurrent;
using APIHealthMonitoring.Application.Interfaces.Notifications;

namespace APIHealthMonitoring.Infrastructure.Notifications.Services;

/// <summary>
/// Thread-safe, in-memory singleton implementation of <see cref="IEmailNotificationStateTracker"/>.
/// Uses a <see cref="ConcurrentDictionary{TKey,TValue}"/> to track whether a Critical alert
/// email has already been sent for each monitored API endpoint.
///
/// <para>
/// Registered as a <b>Singleton</b> so the state survives across the short-lived scopes
/// created by <c>MonitoringBackgroundService</c>.
/// </para>
/// </summary>
public sealed class EmailNotificationStateTracker : IEmailNotificationStateTracker
{
    // key: ApiEndpointId, value: true if a notification email has been sent
    private readonly ConcurrentDictionary<int, bool> _sentFlags = new();

    /// <inheritdoc />
    public bool HasNotificationBeenSent(int endpointId) =>
        _sentFlags.TryGetValue(endpointId, out var sent) && sent;

    /// <inheritdoc />
    public void MarkAsSent(int endpointId) =>
        _sentFlags[endpointId] = true;

    /// <inheritdoc />
    public void Reset(int endpointId) =>
        _sentFlags[endpointId] = false;
}
