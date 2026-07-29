namespace APIHealthMonitoring.Application.Exceptions;

/// <summary>
/// Thrown when a create/update operation conflicts with existing state
/// (e.g., duplicate resource, optimistic concurrency violation).
/// Maps to HTTP 409 Conflict.
/// </summary>
public sealed class ConflictException : ApplicationException
{
    public ConflictException(string message)
        : base(message) { }
}
