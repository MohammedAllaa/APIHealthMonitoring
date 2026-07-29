using APIHealthMonitoring.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

// Alias to avoid ambiguity with the project's own ApplicationException base class.
using AppEx = APIHealthMonitoring.Application.Exceptions.ApplicationException;

namespace APIHealthMonitoring.API.Middleware;

/// <summary>
/// Centralized exception handler registered with ASP.NET Core's
/// IExceptionHandler pipeline. Converts all thrown exceptions into
/// RFC 7807 ProblemDetails responses, logs appropriately, and ensures
/// no raw stack traces leak to callers.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = MapException(exception);

        // ----------------------------------------------------------------
        // Logging — Warning for expected/client errors, Error for server faults
        // ----------------------------------------------------------------
        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Unhandled exception [{Type}] on {Method} {Path} — {Message}",
                exception.GetType().Name,
                httpContext.Request.Method,
                httpContext.Request.Path,
                exception.Message);
        }
        else
        {
            _logger.LogWarning(
                "Application exception [{Type}] on {Method} {Path} — {Message}",
                exception.GetType().Name,
                httpContext.Request.Method,
                httpContext.Request.Path,
                exception.Message);
        }

        // ----------------------------------------------------------------
        // Build RFC 7807 ProblemDetails
        // ----------------------------------------------------------------
        var problem = new ProblemDetails
        {
            Status   = statusCode,
            Title    = title,
            Detail   = exception.Message,
            Instance = httpContext.Request.Path
        };

        // Attach errors array for validation failures
        if (exception is ValidationException validationEx)
        {
            problem.Extensions["errors"] = validationEx.Errors;
        }

        // ----------------------------------------------------------------
        // Write response
        // ----------------------------------------------------------------
        httpContext.Response.StatusCode  = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true; // signal that we handled the exception
    }

    // ----------------------------------------------------------------
    // Private helper — maps exception type → (HTTP status, title)
    // ----------------------------------------------------------------
    private static (int StatusCode, string Title) MapException(Exception exception) =>
        exception switch
        {
            NotFoundException          => (StatusCodes.Status404NotFound,            "Resource Not Found"),
            UnauthorizedException      => (StatusCodes.Status401Unauthorized,         "Unauthorized"),
            ForbiddenException         => (StatusCodes.Status403Forbidden,            "Forbidden"),
            ConflictException          => (StatusCodes.Status409Conflict,             "Conflict"),
            ValidationException        => (StatusCodes.Status400BadRequest,           "Validation Failed"),
            ServiceUnavailableException=> (StatusCodes.Status503ServiceUnavailable,   "Service Unavailable"),
            OperationCanceledException => (499,                                        "Request Cancelled"),
            _                          => (StatusCodes.Status500InternalServerError,  "An unexpected error occurred")
        };
}
