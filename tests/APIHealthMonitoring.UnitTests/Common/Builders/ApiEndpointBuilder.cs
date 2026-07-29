using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.UnitTests.Common.Builders;

public class ApiEndpointBuilder
{
    private int _id = 1;
    private string _name = "Test API";
    private string _baseUrl = "https://api.test.com";
    private string _healthEndpoint = "/health";
    private APIHealthMonitoring.Domain.Enums.HttpMethod _httpMethod = APIHealthMonitoring.Domain.Enums.HttpMethod.GET;
    private int _expectedStatusCode = 200;
    private int _timeoutSeconds = 30;
    private int _intervalSeconds = 60;
    private string _serviceOwner = "Test Team";
    private Domain.Enums.Environment _environment = Domain.Enums.Environment.Development;
    private bool _isActive = true;
    private ApiHealthStatus _currentStatus = ApiHealthStatus.Unknown;
    private int _consecutiveFailures = 0;
    private MonitoringConfiguration? _monitoringConfig;

    public ApiEndpointBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public ApiEndpointBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ApiEndpointBuilder WithBaseUrl(string baseUrl)
    {
        _baseUrl = baseUrl;
        return this;
    }

    public ApiEndpointBuilder WithHealthEndpoint(string healthEndpoint)
    {
        _healthEndpoint = healthEndpoint;
        return this;
    }

    public ApiEndpointBuilder WithHttpMethod(APIHealthMonitoring.Domain.Enums.HttpMethod httpMethod)
    {
        _httpMethod = httpMethod;
        return this;
    }

    public ApiEndpointBuilder WithExpectedStatusCode(int expectedStatusCode)
    {
        _expectedStatusCode = expectedStatusCode;
        return this;
    }

    public ApiEndpointBuilder WithTimeoutSeconds(int timeoutSeconds)
    {
        _timeoutSeconds = timeoutSeconds;
        return this;
    }

    public ApiEndpointBuilder WithIntervalSeconds(int intervalSeconds)
    {
        _intervalSeconds = intervalSeconds;
        return this;
    }

    public ApiEndpointBuilder WithServiceOwner(string serviceOwner)
    {
        _serviceOwner = serviceOwner;
        return this;
    }

    public ApiEndpointBuilder WithEnvironment(Domain.Enums.Environment environment)
    {
        _environment = environment;
        return this;
    }

    public ApiEndpointBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public ApiEndpointBuilder WithCurrentStatus(ApiHealthStatus currentStatus)
    {
        _currentStatus = currentStatus;
        return this;
    }

    public ApiEndpointBuilder WithConsecutiveFailures(int consecutiveFailures)
    {
        _consecutiveFailures = consecutiveFailures;
        return this;
    }

    public ApiEndpointBuilder WithMonitoringConfig(MonitoringConfiguration config)
    {
        _monitoringConfig = config;
        return this;
    }

    public ApiEndpoint Build()
    {
        var endpoint = new ApiEndpoint
        {
            Id = _id,
            Name = _name,
            BaseUrl = _baseUrl,
            HealthEndpoint = _healthEndpoint,
            HttpMethod = _httpMethod,
            ExpectedStatusCode = _expectedStatusCode,
            TimeoutSeconds = _timeoutSeconds,
            IntervalSeconds = _intervalSeconds,
            ServiceOwner = _serviceOwner,
            Environment = _environment,
            IsActive = _isActive,
            CurrentStatus = _currentStatus,
            ConsecutiveFailures = _consecutiveFailures,
            MonitoringConfig = _monitoringConfig,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        if (endpoint.MonitoringConfig is not null)
        {
            endpoint.MonitoringConfig.ApiEndpointId = endpoint.Id;
            endpoint.MonitoringConfig.ApiEndpoint = endpoint;
        }

        return endpoint;
    }
}
