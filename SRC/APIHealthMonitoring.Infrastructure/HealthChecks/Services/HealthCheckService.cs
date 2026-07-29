using APIHealthMonitoring.Application.Constants;
using APIHealthMonitoring.Application.DTOs.HealthChecks;
using APIHealthMonitoring.Application.Interfaces;
using APIHealthMonitoring.Application.Interfaces.Alerts;
using APIHealthMonitoring.Application.Interfaces.HealthChecks;
using APIHealthMonitoring.Application.Specifications;
using APIHealthMonitoring.Application.Specifications.HealthChecks;
using APIHealthMonitoring.Application.Specifications.MonitoringConfig;
using APIHealthMonitoring.Domain.Entities;
using APIHealthMonitoring.Domain.Enums;

namespace APIHealthMonitoring.Infrastructure.HealthChecks.Services;

/// <summary>
/// Orchestrates health check result persistence, status cache updates, and availability calculation.
/// </summary>
public class HealthCheckService : IHealthCheckService
{
    private readonly IUnitOfWork             _unitOfWork;
    private readonly IHealthCheckExecutor    _executor;
    private readonly IHealthStatusEvaluator  _evaluator;
    private readonly IAlertEvaluator         _alertEvaluator;
    private readonly ICacheService           _cache;

    // Rate-limit: last manual trigger time per API endpoint
    private static readonly Dictionary<int, DateTime> _lastManualTrigger = new();
    private static readonly object _triggerLock = new();

    public HealthCheckService(
        IUnitOfWork            unitOfWork,
        IHealthCheckExecutor   executor,
        IHealthStatusEvaluator evaluator,
        IAlertEvaluator        alertEvaluator,
        ICacheService          cache)
    {
        _unitOfWork      = unitOfWork;
        _executor        = executor;
        _evaluator       = evaluator;
        _alertEvaluator  = alertEvaluator;
        _cache           = cache;
    }

    // -------------------------------------------------------------------------
    // Save Result + Update Cached Status
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task SaveResultAsync(HealthCheck result, CancellationToken ct = default)
    {
        // 1. Fetch the endpoint with its config (using tracking since we will update it)
        var endpoint = await _unitOfWork.Repository<ApiEndpoint>().GetByIdAsync(result.ApiEndpointId, ct)
            ?? throw new KeyNotFoundException($"ApiEndpoint {result.ApiEndpointId} not found.");

        var configSpec = new ConfigByApiEndpointIdSpec(endpoint.Id);
        var config = await _unitOfWork.Repository<MonitoringConfiguration>()
            .GetEntityWithSpecAsync(configSpec, ct);

        // 2. Update consecutive failure counter
        if (result.IsSuccessful)
            endpoint.ConsecutiveFailures = 0;
        else
            endpoint.ConsecutiveFailures += 1;

        // 3. Evaluate new status
        endpoint.CurrentStatus  = config is not null
            ? _evaluator.Evaluate(result, config, endpoint.ConsecutiveFailures)
            : (result.IsSuccessful ? ApiHealthStatus.Healthy : ApiHealthStatus.Warning);

        endpoint.LastCheckedAt = result.CheckedAt;

        // 4. Persist health check record
        _unitOfWork.Repository<HealthCheck>().Add(result);

        // 5. Update endpoint cached fields
        _unitOfWork.Repository<ApiEndpoint>().Update(endpoint);

        await _unitOfWork.SaveChangesAsync(ct);

        // 6. Trigger alert evaluation (Module 5)
        await _alertEvaluator.EvaluateAndAlertAsync(endpoint, result, config, ct);

        // 7. Module 10 — Invalidate cached dashboard data that reflects this endpoint
        _cache.Remove(CacheKeys.DashboardSummary);
        _cache.RemoveByPrefix(CacheKeys.DashboardApiCards);
        _cache.RemoveByPrefix($"{CacheKeys.ApiStatsPrefix}{result.ApiEndpointId}");
    }

    // -------------------------------------------------------------------------
    // Read — Paged List
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<PaginatedResult<HealthCheckResultDto>> GetPagedAsync(
        HealthCheckPagedRequestDto request, CancellationToken ct = default)
    {
        // Validate date range
        if (request.FromDate.HasValue && request.ToDate.HasValue &&
            request.FromDate.Value > request.ToDate.Value)
        {
            throw new InvalidOperationException("FromDate cannot be after ToDate.");
        }

        var dataSpec  = new HealthCheckSearchSpec(request);
        var countSpec = new HealthCheckSearchCountSpec(request);

        var data  = await _unitOfWork.Repository<HealthCheck>().GetAllWithSpecAsync(dataSpec, ct);
        var total = await _unitOfWork.Repository<HealthCheck>().CountAsync(countSpec, ct);

        var dtos = data.Select(h => MapToDto(h, null)).ToList();

        // Populate API names in batch
        if (data.Any())
        {
            var endpointIds = data.Select(h => h.ApiEndpointId).Distinct().ToList();
            var endpoints   = (await _unitOfWork.Repository<ApiEndpoint>().GetAllAsync(ct))
                               .Where(e => endpointIds.Contains(e.Id))
                               .ToDictionary(e => e.Id, e => e.Name);

            dtos = data.Select(h =>
                MapToDto(h, endpoints.TryGetValue(h.ApiEndpointId, out var n) ? n : null)).ToList();
        }

        return new PaginatedResult<HealthCheckResultDto>(
            dtos, total, request.PageIndex, Math.Min(request.PageSize, 100));
    }

    // -------------------------------------------------------------------------
    // Read — Single
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<HealthCheckResultDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var check = await _unitOfWork.Repository<HealthCheck>().GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Health check record with ID {id} not found.");

        var endpoint = await _unitOfWork.Repository<ApiEndpoint>().GetByIdAsync(check.ApiEndpointId, ct);

        return MapToDto(check, endpoint?.Name);
    }

    // -------------------------------------------------------------------------
    // Endpoint Status Summary
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<ApiHealthSummaryDto> GetEndpointSummaryAsync(
        int apiEndpointId, CancellationToken ct = default)
    {
        var endpoint = await _unitOfWork.Repository<ApiEndpoint>().GetByIdAsync(apiEndpointId, ct)
            ?? throw new KeyNotFoundException($"API endpoint with ID {apiEndpointId} not found.");

        // Today availability: success count / total count for today
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd   = todayStart.AddDays(1);

        var todaySpec   = new HealthChecksInDateRangeSpec(apiEndpointId, todayStart, todayEnd);
        var todayChecks = await _unitOfWork.Repository<HealthCheck>().GetAllWithSpecAsync(todaySpec, ct);

        decimal todayAvailability = 0m;
        if (todayChecks.Any())
        {
            var successCount = todayChecks.Count(h => h.IsSuccessful);
            todayAvailability = Math.Round((decimal)successCount / todayChecks.Count * 100, 2);
        }

        return new ApiHealthSummaryDto
        {
            ApiEndpointId      = endpoint.Id,
            ApiName            = endpoint.Name,
            CurrentStatus      = endpoint.CurrentStatus,
            LastCheckedAt      = endpoint.LastCheckedAt,
            ConsecutiveFailures = endpoint.ConsecutiveFailures,
            TodayAvailability  = todayAvailability,
        };
    }

    // -------------------------------------------------------------------------
    // Manual Trigger
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<HealthCheckResultDto> TriggerManualCheckAsync(
        int apiEndpointId, CancellationToken ct = default)
    {
        // Rate-limit: one manual trigger per API per 10 seconds
        lock (_triggerLock)
        {
            if (_lastManualTrigger.TryGetValue(apiEndpointId, out var lastTrigger) &&
                (DateTime.UtcNow - lastTrigger).TotalSeconds < 10)
            {
                throw new InvalidOperationException(
                    "Manual trigger rate-limited. Please wait at least 10 seconds between manual checks.");
            }
            _lastManualTrigger[apiEndpointId] = DateTime.UtcNow;
        }

        var endpoint = await _unitOfWork.Repository<ApiEndpoint>().GetByIdAsync(apiEndpointId, ct)
            ?? throw new KeyNotFoundException($"API endpoint with ID {apiEndpointId} not found.");

        if (!endpoint.IsActive)
            throw new InvalidOperationException("Cannot manually check an inactive endpoint.");

        var result = await _executor.ExecuteAsync(endpoint, ct);
        await SaveResultAsync(result, ct);

        return MapToDto(result, endpoint.Name);
    }

    // -------------------------------------------------------------------------
    // Mapping
    // -------------------------------------------------------------------------

    private static HealthCheckResultDto MapToDto(HealthCheck h, string? apiName) => new()
    {
        Id                = h.Id,
        ApiEndpointId     = h.ApiEndpointId,
        ApiName           = h.ApiEndpoint?.Name ?? apiName ?? string.Empty,
        CheckedAt         = h.CheckedAt,
        ResponseTimeMs    = h.ResponseTimeMs,
        StatusCode        = h.StatusCode,
        IsSuccessful      = h.IsSuccessful,
        ErrorMessage      = h.ErrorMessage,
        ResponseSizeBytes = h.ResponseSizeBytes,
    };
}
