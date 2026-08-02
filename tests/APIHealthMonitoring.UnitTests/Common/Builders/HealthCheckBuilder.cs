using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.UnitTests.Common.Builders;

public class HealthCheckBuilder
{
    private int _id = 1;
    private int _apiEndpointId = 1;
    private DateTime _checkedAt = DateTime.UtcNow;
    private int _responseTimeMs = 150;
    private int? _statusCode = 200;
    private bool _isSuccessful = true;
    private string? _errorMessage;
    private long? _responseSizeBytes = 512;

    public HealthCheckBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public HealthCheckBuilder WithApiEndpointId(int apiEndpointId)
    {
        _apiEndpointId = apiEndpointId;
        return this;
    }

    public HealthCheckBuilder WithCheckedAt(DateTime checkedAt)
    {
        _checkedAt = checkedAt;
        return this;
    }

    public HealthCheckBuilder WithResponseTimeMs(int responseTimeMs)
    {
        _responseTimeMs = responseTimeMs;
        return this;
    }

    public HealthCheckBuilder WithStatusCode(int? statusCode)
    {
        _statusCode = statusCode;
        return this;
    }

    public HealthCheckBuilder WithIsSuccessful(bool isSuccessful)
    {
        _isSuccessful = isSuccessful;
        return this;
    }

    public HealthCheckBuilder WithErrorMessage(string? errorMessage)
    {
        _errorMessage = errorMessage;
        return this;
    }

    public HealthCheckBuilder WithResponseSizeBytes(long? responseSizeBytes)
    {
        _responseSizeBytes = responseSizeBytes;
        return this;
    }

    public HealthCheck Build()
    {
        return new HealthCheck
        {
            Id = _id,
            ApiEndpointId = _apiEndpointId,
            CheckedAt = _checkedAt,
            ResponseTimeMs = _responseTimeMs,
            StatusCode = _statusCode,
            IsSuccessful = _isSuccessful,
            ErrorMessage = _errorMessage,
            ResponseSizeBytes = _responseSizeBytes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
