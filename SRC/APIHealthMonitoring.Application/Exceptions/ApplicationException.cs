namespace APIHealthMonitoring.Application.Exceptions;

/// <summary>
/// Base class for all application-layer exceptions.
/// Derive from this to create domain-specific, HTTP-mappable error types.
/// </summary>
public abstract class ApplicationException : Exception
{
    protected ApplicationException(string message)
        : base(message) { }

    protected ApplicationException(string message, Exception innerException)
        : base(message, innerException) { }
}
