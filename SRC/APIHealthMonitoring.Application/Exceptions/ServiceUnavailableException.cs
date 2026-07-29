namespace APIHealthMonitoring.Application.Exceptions;

/// <summary>
/// Thrown when a downstream service or dependency is temporarily unavailable.
/// Maps to HTTP 503 Service Unavailable.
/// </summary>
public sealed class ServiceUnavailableException : ApplicationException
{
    public ServiceUnavailableException(string message)
        : base(message) { }

    public ServiceUnavailableException(string message, Exception innerException)
        : base(message, innerException) { }
}
