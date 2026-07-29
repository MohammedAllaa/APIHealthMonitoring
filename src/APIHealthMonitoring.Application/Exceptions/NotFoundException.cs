namespace APIHealthMonitoring.Application.Exceptions;

/// <summary>
/// Thrown when a requested resource cannot be found.
/// Maps to HTTP 404 Not Found.
/// </summary>
public sealed class NotFoundException : ApplicationException
{
    public NotFoundException(string message)
        : base(message) { }

    /// <summary>
    /// Generates a standard formatted message:
    /// "{name} with identifier '{key}' was not found."
    /// </summary>
    public NotFoundException(string name, object key)
        : base($"{name} with identifier '{key}' was not found.") { }
}
