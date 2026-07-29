namespace APIHealthMonitoring.Application.Exceptions;

/// <summary>
/// Thrown when the caller is authenticated but lacks the required permission.
/// Maps to HTTP 403 Forbidden.
/// </summary>
public sealed class ForbiddenException : ApplicationException
{
    public ForbiddenException()
        : base("You do not have permission to perform this action.") { }

    public ForbiddenException(string message)
        : base(message) { }
}
