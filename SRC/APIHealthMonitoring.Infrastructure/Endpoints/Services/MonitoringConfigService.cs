using APIHealthMonitoring.Application.DTOs.MonitoringConfig;
using APIHealthMonitoring.Application.Interfaces;
using APIHealthMonitoring.Application.Interfaces.Endpoints;
using APIHealthMonitoring.Application.Specifications.MonitoringConfig;
using APIHealthMonitoring.Domain.Entities;

namespace APIHealthMonitoring.Infrastructure.Endpoints.Services;

/// <summary>
/// Service implementation for managing <see cref="MonitoringConfiguration"/>.
/// </summary>
public class MonitoringConfigService : IMonitoringConfigService
{
    private readonly IUnitOfWork _unitOfWork;

    public MonitoringConfigService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<MonitoringConfigResponseDto> CreateAsync(
        CreateMonitoringConfigDto dto, CancellationToken ct = default)
    {
        // 1. Verify ApiEndpoint exists
        var endpoint = await _unitOfWork.Repository<ApiEndpoint>().GetByIdAsync(dto.ApiEndpointId, ct)
            ?? throw new KeyNotFoundException($"API endpoint with ID {dto.ApiEndpointId} was not found.");

        // 2. Enforce one-per-API rule
        var spec = new ConfigByApiEndpointIdSpec(dto.ApiEndpointId);
        var existing = await _unitOfWork.Repository<MonitoringConfiguration>().GetEntityWithSpecAsync(spec, ct);
        if (existing is not null)
        {
            throw new InvalidOperationException($"API endpoint with ID {dto.ApiEndpointId} already has a monitoring configuration.");
        }

        // 3. Threshold check
        if (dto.SlowThresholdMs >= dto.CriticalThresholdMs)
        {
            throw new InvalidOperationException("SlowThresholdMs must be less than CriticalThresholdMs.");
        }

        var config = new MonitoringConfiguration
        {
            ApiEndpointId         = dto.ApiEndpointId,
            SlowThresholdMs       = dto.SlowThresholdMs,
            CriticalThresholdMs   = dto.CriticalThresholdMs,
            FailureCountLimit     = dto.FailureCountLimit,
            AvailabilityThreshold = dto.AvailabilityThreshold,
        };

        _unitOfWork.Repository<MonitoringConfiguration>().Add(config);
        await _unitOfWork.SaveChangesAsync(ct);

        config.ApiEndpoint = endpoint;
        return MapToDto(config);
    }

    /// <inheritdoc />
    public async Task<MonitoringConfigResponseDto> GetByEndpointIdAsync(
        int apiEndpointId, CancellationToken ct = default)
    {
        // Verify ApiEndpoint exists
        var endpointExists = await _unitOfWork.Repository<ApiEndpoint>().ExistsAsync(apiEndpointId, ct);
        if (!endpointExists)
        {
            throw new KeyNotFoundException($"API endpoint with ID {apiEndpointId} was not found.");
        }

        var spec = new ConfigByApiEndpointIdSpec(apiEndpointId);
        var config = await _unitOfWork.Repository<MonitoringConfiguration>().GetEntityWithSpecAsync(spec, ct)
            ?? throw new KeyNotFoundException($"Monitoring configuration for API endpoint ID {apiEndpointId} was not found.");

        return MapToDto(config);
    }

    /// <inheritdoc />
    public async Task<MonitoringConfigResponseDto> UpdateAsync(
        int apiEndpointId, UpdateMonitoringConfigDto dto, CancellationToken ct = default)
    {
        var spec = new ConfigByApiEndpointIdSpec(apiEndpointId);
        var config = await _unitOfWork.Repository<MonitoringConfiguration>().GetEntityWithSpecAsync(spec, ct);

        if (config is null)
        {
            var endpointExists = await _unitOfWork.Repository<ApiEndpoint>().ExistsAsync(apiEndpointId, ct);
            if (!endpointExists)
            {
                throw new KeyNotFoundException($"API endpoint with ID {apiEndpointId} was not found.");
            }
            throw new KeyNotFoundException($"Monitoring configuration for API endpoint ID {apiEndpointId} was not found.");
        }

        int targetSlow = dto.SlowThresholdMs ?? config.SlowThresholdMs;
        int targetCritical = dto.CriticalThresholdMs ?? config.CriticalThresholdMs;

        if (targetSlow >= targetCritical)
        {
            throw new InvalidOperationException("SlowThresholdMs must be less than CriticalThresholdMs.");
        }

        if (dto.SlowThresholdMs.HasValue)       config.SlowThresholdMs       = dto.SlowThresholdMs.Value;
        if (dto.CriticalThresholdMs.HasValue)   config.CriticalThresholdMs   = dto.CriticalThresholdMs.Value;
        if (dto.FailureCountLimit.HasValue)     config.FailureCountLimit     = dto.FailureCountLimit.Value;
        if (dto.AvailabilityThreshold.HasValue) config.AvailabilityThreshold = dto.AvailabilityThreshold.Value;

        _unitOfWork.Repository<MonitoringConfiguration>().Update(config);
        await _unitOfWork.SaveChangesAsync(ct);

        return MapToDto(config);
    }

    /// <inheritdoc />
    public async Task<MonitoringConfigResponseDto> ResetToDefaultsAsync(
        int apiEndpointId, CancellationToken ct = default)
    {
        var spec = new ConfigByApiEndpointIdSpec(apiEndpointId);
        var config = await _unitOfWork.Repository<MonitoringConfiguration>().GetEntityWithSpecAsync(spec, ct);

        if (config is null)
        {
            var endpoint = await _unitOfWork.Repository<ApiEndpoint>().GetByIdAsync(apiEndpointId, ct)
                ?? throw new KeyNotFoundException($"API endpoint with ID {apiEndpointId} was not found.");

            config = new MonitoringConfiguration
            {
                ApiEndpointId         = apiEndpointId,
                SlowThresholdMs       = 1000,
                CriticalThresholdMs   = 2000,
                FailureCountLimit     = 3,
                AvailabilityThreshold = 99.0m,
                ApiEndpoint           = endpoint
            };
            _unitOfWork.Repository<MonitoringConfiguration>().Add(config);
        }
        else
        {
            config.SlowThresholdMs       = 1000;
            config.CriticalThresholdMs   = 2000;
            config.FailureCountLimit     = 3;
            config.AvailabilityThreshold = 99.0m;
            _unitOfWork.Repository<MonitoringConfiguration>().Update(config);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return MapToDto(config);
    }

    private static MonitoringConfigResponseDto MapToDto(MonitoringConfiguration config)
    {
        return new MonitoringConfigResponseDto
        {
            Id                    = config.Id,
            ApiEndpointId         = config.ApiEndpointId,
            ApiName               = config.ApiEndpoint?.Name ?? string.Empty,
            SlowThresholdMs       = config.SlowThresholdMs,
            CriticalThresholdMs   = config.CriticalThresholdMs,
            FailureCountLimit     = config.FailureCountLimit,
            AvailabilityThreshold = config.AvailabilityThreshold,
            CreatedAt             = config.CreatedAt,
            UpdatedAt             = config.UpdatedAt
        };
    }
}
