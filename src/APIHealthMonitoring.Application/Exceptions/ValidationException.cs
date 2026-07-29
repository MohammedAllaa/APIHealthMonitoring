namespace APIHealthMonitoring.Application.Exceptions;

/// <summary>
/// Thrown when request data fails business or model validation.
/// Maps to HTTP 400 Bad Request.
/// Carries one or more human-readable error messages.
/// </summary>
public sealed class ValidationException : ApplicationException
{
    /// <summary>Collection of validation error messages.</summary>
    public IEnumerable<string> Errors { get; }

    public ValidationException(IEnumerable<string> errors)
        : base("One or more validation failures occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string error)
        : base("One or more validation failures occurred.")
    {
        Errors = [error];
    }
}
