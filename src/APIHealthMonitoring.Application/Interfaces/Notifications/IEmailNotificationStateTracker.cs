namespace APIHealthMonitoring.Application.Interfaces.Notifications;

/// <summary>
/// Thread-safe, singleton-scoped tracker that remembers whether a Critical email
/// notification has already been dispatched for a given API endpoint.
///
/// <para>
/// The tracker is intentionally in-memory only — state resets on application restart,
/// which ensures the first alert after a restart is always sent.
/// </para>
/// </summary>
public interface IEmailNotificationStateTracker
{
    /// <summary>
    /// Returns <see langword="true"/> if a Critical notification email has already
    /// been sent for the specified <paramref name="endpointId"/> and has not yet been reset.
    /// </summary>
    /// <param name="endpointId">The unique identifier of the API endpoint.</param>
    bool HasNotificationBeenSent(int endpointId);

    /// <summary>
    /// Records that a notification email was successfully dispatched for the
    /// specified <paramref name="endpointId"/>.
    /// </summary>
    /// <param name="endpointId">The unique identifier of the API endpoint.</param>
    void MarkAsSent(int endpointId);

    /// <summary>
    /// Clears the notification state for the specified <paramref name="endpointId"/>,
    /// allowing the next Critical transition to trigger a fresh email.
    /// Should be called when the endpoint returns to a <c>Healthy</c> status.
    /// </summary>
    /// <param name="endpointId">The unique identifier of the API endpoint.</param>
    void Reset(int endpointId);
}
