namespace APIHealthMonitoring.Application.Exceptions;

/// <summary>
/// Thrown when the caller is not authenticated.
/// Maps to HTTP 401 Unauthorized.
/// </summary>
public sealed class UnauthorizedException : ApplicationException
{
    public UnauthorizedException(string message)
        : base(message) { }
}
