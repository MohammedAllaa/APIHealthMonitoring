using APIHealthMonitoring.Application.DTOs.HealthChecks;
using APIHealthMonitoring.Application.Interfaces.HealthChecks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIHealthMonitoring.API.Controllers;

/// <summary>
/// Provides access to health check execution records and triggers manual checks.
/// </summary>
[ApiController]
[Authorize]
[Produces("application/json")]
public class HealthChecksController : ControllerBase
{
    private readonly IHealthCheckService _service;

    public HealthChecksController(IHealthCheckService service)
    {
        _service = service;
    }

    // -------------------------------------------------------------------------
    // GET /api/health-checks   [Admin, Viewer]
    // -------------------------------------------------------------------------

    /// <summary>Returns a paged, filterable list of all health check records.</summary>
    [HttpGet("api/health-checks")]
    [Authorize(Roles = "Administrator,Viewer")]
    [ProducesResponseType(typeof(Application.Specifications.PaginatedResult<HealthCheckResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] HealthCheckPagedRequestDto request,
        CancellationToken ct)
    {
        try
        {
            var result = await _service.GetPagedAsync(request, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // GET /api/health-checks/{id}   [Admin, Viewer]
    // -------------------------------------------------------------------------

    /// <summary>Returns a single health check record by ID.</summary>
    [HttpGet("api/health-checks/{id:int}")]
    [Authorize(Roles = "Administrator,Viewer")]
    [ProducesResponseType(typeof(HealthCheckResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
    {
        try
        {
            var result = await _service.GetByIdAsync(id, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // GET /api/endpoints/{id}/health-checks   [Admin, Viewer]
    // -------------------------------------------------------------------------

    /// <summary>Returns health check history for a specific API endpoint.</summary>
    [HttpGet("api/endpoints/{id:int}/health-checks")]
    [Authorize(Roles = "Administrator,Viewer")]
    [ProducesResponseType(typeof(Application.Specifications.PaginatedResult<HealthCheckResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByEndpoint(
        [FromRoute] int id,
        [FromQuery] HealthCheckPagedRequestDto request,
        CancellationToken ct)
    {
        request.ApiEndpointId = id;

        try
        {
            var result = await _service.GetPagedAsync(request, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // GET /api/endpoints/{id}/status   [Admin, Viewer]
    // -------------------------------------------------------------------------

    /// <summary>Returns the current health status summary for a specific API endpoint.</summary>
    [HttpGet("api/endpoints/{id:int}/status")]
    [Authorize(Roles = "Administrator,Viewer")]
    [ProducesResponseType(typeof(ApiHealthSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStatus([FromRoute] int id, CancellationToken ct)
    {
        try
        {
            var result = await _service.GetEndpointSummaryAsync(id, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // POST /api/endpoints/{id}/check-now   [Admin]
    // -------------------------------------------------------------------------

    /// <summary>Manually triggers an immediate health check for the specified API endpoint.</summary>
    [HttpPost("api/endpoints/{id:int}/check-now")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(HealthCheckResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CheckNow([FromRoute] int id, CancellationToken ct)
    {
        try
        {
            var result = await _service.TriggerManualCheckAsync(id, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
