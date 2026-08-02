using APIHealthMonitoring.Application.DTOs.Endpoints;
using APIHealthMonitoring.Application.Interfaces;
using APIHealthMonitoring.Application.Interfaces.Endpoints;
using APIHealthMonitoring.Application.Interfaces.Repositories;
using APIHealthMonitoring.Application.Specifications;
using APIHealthMonitoring.Application.Specifications.Endpoints;
using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Infrastructure.Endpoints.Services;

/// <summary>
/// Implements CRUD and lifecycle operations for registered API endpoints.
/// Uses <see cref="IApiEndpointRepository"/> for custom queries and
/// <see cref="IUnitOfWork"/> for generic operations and persistence.
/// </summary>
public class ApiEndpointService : IApiEndpointService
{
    private readonly IApiEndpointRepository _endpointRepo;
    private readonly IUnitOfWork            _unitOfWork;

    public ApiEndpointService(
        IApiEndpointRepository endpointRepo,
        IUnitOfWork            unitOfWork)
    {
        _endpointRepo = endpointRepo;
        _unitOfWork   = unitOfWork;
    }

    // -------------------------------------------------------------------------
    // Create
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<ApiEndpointResponseDto> CreateAsync(
        CreateApiEndpointDto request, CancellationToken ct = default)
    {
        // Enforce unique name
        if (await _endpointRepo.NameExistsAsync(request.Name, cancellationToken: ct))
            throw new InvalidOperationException(
                $"An endpoint named '{request.Name}' already exists.");

        var endpoint = new ApiEndpoint
        {
            Name               = request.Name,
            BaseUrl            = request.BaseUrl,
            HealthEndpoint     = request.HealthEndpoint,
            HttpMethod         = request.HttpMethod,
            ExpectedStatusCode = request.ExpectedStatusCode,
            TimeoutSeconds     = request.TimeoutSeconds,
            IntervalSeconds    = request.IntervalSeconds,
            ServiceOwner       = request.ServiceOwner,
            Environment        = request.Environment,
            IsActive           = true,
            MonitoringConfig   = new MonitoringConfiguration
            {
                SlowThresholdMs       = 1000,
                CriticalThresholdMs   = 2000,
                FailureCountLimit     = 3,
                AvailabilityThreshold = 99.0m
            }
        };

        _unitOfWork.Repository<ApiEndpoint>().Add(endpoint);
        await _unitOfWork.SaveChangesAsync(ct);

        return MapToResponse(endpoint);
    }

    // -------------------------------------------------------------------------
    // Read — Paged List
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<PaginatedResult<ApiEndpointSummaryDto>> GetPagedAsync(
        ApiEndpointPagedRequestDto request, CancellationToken ct = default)
    {
        var dataSpec  = new ApiEndpointSearchSpec(request);
        var countSpec = new ApiEndpointSearchCountSpec(request);

        var data       = await _endpointRepo.GetAllWithSpecAsync(dataSpec, ct);
        var totalCount = await _endpointRepo.CountAsync(countSpec, ct);

        var summaries = data.Select(MapToSummary).ToList();

        return new PaginatedResult<ApiEndpointSummaryDto>(
            summaries,
            totalCount,
            request.PageIndex,
            Math.Min(request.PageSize, 100));
    }

    // -------------------------------------------------------------------------
    // Read — Single
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<ApiEndpointResponseDto> GetByIdAsync(
        int id, CancellationToken ct = default)
    {
        var spec     = new ApiEndpointByIdWithConfigSpec(id);
        var endpoint = await _endpointRepo.GetEntityWithSpecAsync(spec, ct)
            ?? throw new KeyNotFoundException($"Endpoint with ID {id} not found.");

        return MapToResponse(endpoint);
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<ApiEndpointResponseDto> UpdateAsync(
        int id, UpdateApiEndpointDto request, CancellationToken ct = default)
    {
        var endpoint = await _unitOfWork.Repository<ApiEndpoint>().GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Endpoint with ID {id} not found.");

        // Validate name uniqueness only when name changes
        if (request.Name is not null
            && !string.Equals(request.Name, endpoint.Name, StringComparison.OrdinalIgnoreCase))
        {
            if (await _endpointRepo.NameExistsAsync(request.Name, excludeId: id, cancellationToken: ct))
                throw new InvalidOperationException(
                    $"An endpoint named '{request.Name}' already exists.");

            endpoint.Name = request.Name;
        }

        // Apply only the fields that were provided
        if (request.BaseUrl            is not null) endpoint.BaseUrl            = request.BaseUrl;
        if (request.HealthEndpoint     is not null) endpoint.HealthEndpoint     = request.HealthEndpoint;
        if (request.HttpMethod         is not null) endpoint.HttpMethod         = request.HttpMethod.Value;
        if (request.ExpectedStatusCode is not null) endpoint.ExpectedStatusCode = request.ExpectedStatusCode.Value;
        if (request.TimeoutSeconds     is not null) endpoint.TimeoutSeconds     = request.TimeoutSeconds.Value;
        if (request.IntervalSeconds    is not null) endpoint.IntervalSeconds    = request.IntervalSeconds.Value;
        if (request.ServiceOwner       is not null) endpoint.ServiceOwner       = request.ServiceOwner;
        if (request.Environment        is not null) endpoint.Environment        = request.Environment.Value;

        _unitOfWork.Repository<ApiEndpoint>().Update(endpoint);
        await _unitOfWork.SaveChangesAsync(ct);

        return MapToResponse(endpoint);
    }

    // -------------------------------------------------------------------------
    // Delete
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var endpoint = await _unitOfWork.Repository<ApiEndpoint>().GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Endpoint with ID {id} not found.");

        _unitOfWork.Repository<ApiEndpoint>().Delete(endpoint);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    // -------------------------------------------------------------------------
    // Activate / Deactivate
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task ActivateAsync(int id, CancellationToken ct = default)
    {
        var endpoint = await _unitOfWork.Repository<ApiEndpoint>().GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Endpoint with ID {id} not found.");

        endpoint.IsActive = true;
        _unitOfWork.Repository<ApiEndpoint>().Update(endpoint);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task DeactivateAsync(int id, CancellationToken ct = default)
    {
        var endpoint = await _unitOfWork.Repository<ApiEndpoint>().GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Endpoint with ID {id} not found.");

        endpoint.IsActive = false;
        _unitOfWork.Repository<ApiEndpoint>().Update(endpoint);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    // -------------------------------------------------------------------------
    // Mapping Helpers
    // -------------------------------------------------------------------------

    private static ApiEndpointResponseDto MapToResponse(ApiEndpoint e) => new()
    {
        Id                 = e.Id,
        Name               = e.Name,
        BaseUrl            = e.BaseUrl,
        HealthEndpoint     = e.HealthEndpoint,
        HttpMethod         = e.HttpMethod,
        ExpectedStatusCode = e.ExpectedStatusCode,
        TimeoutSeconds     = e.TimeoutSeconds,
        IntervalSeconds    = e.IntervalSeconds,
        ServiceOwner       = e.ServiceOwner,
        Environment        = e.Environment,
        IsActive           = e.IsActive,
        CreatedAt          = e.CreatedAt,
        UpdatedAt          = e.UpdatedAt,
        CurrentStatus      = e.CurrentStatus.ToString(),
        LastCheckedAt      = e.LastCheckedAt,
    };

    private static ApiEndpointSummaryDto MapToSummary(ApiEndpoint e) => new()
    {
        Id            = e.Id,
        Name          = e.Name,
        Environment   = e.Environment,
        IsActive      = e.IsActive,
        ServiceOwner  = e.ServiceOwner,
        CurrentStatus = e.CurrentStatus.ToString(),
    };

}
