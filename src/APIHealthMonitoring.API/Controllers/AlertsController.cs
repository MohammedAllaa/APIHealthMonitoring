using APIHealthMonitoring.Application.DTOs.Alerts;
using APIHealthMonitoring.Application.Interfaces.Alerts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIHealthMonitoring.API.Controllers;

/// <summary>
/// Exposes alert history queries and manual resolution.
/// Alert creation is fully automated by the Health Check Engine.
/// </summary>
[ApiController]
[Authorize]
[Produces("application/json")]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _service;

    public AlertsController(IAlertService service)
    {
        _service = service;
    }

    // -------------------------------------------------------------------------
    // GET /api/alerts   [Admin, Viewer]
    // -------------------------------------------------------------------------

    /// <summary>Returns a paged, filterable list of all alerts.</summary>
    [HttpGet("api/alerts")]
    [Authorize(Roles = "Administrator,Viewer")]
    [ProducesResponseType(typeof(Application.Specifications.PaginatedResult<AlertResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] AlertPagedRequestDto request,
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
    // GET /api/alerts/{id}   [Admin, Viewer]
    // -------------------------------------------------------------------------

    /// <summary>Returns a single alert by ID.</summary>
    [HttpGet("api/alerts/{id:int}")]
    [Authorize(Roles = "Administrator,Viewer")]
    [ProducesResponseType(typeof(AlertResponseDto), StatusCodes.Status200OK)]
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
    // GET /api/endpoints/{id}/alerts   [Admin, Viewer]
    // -------------------------------------------------------------------------

    /// <summary>Returns all alerts for a specific API endpoint (paged).</summary>
    [HttpGet("api/endpoints/{id:int}/alerts")]
    [Authorize(Roles = "Administrator,Viewer")]
    [ProducesResponseType(typeof(Application.Specifications.PaginatedResult<AlertResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByEndpoint(
        [FromRoute] int id,
        [FromQuery] AlertPagedRequestDto request,
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
    // PUT /api/alerts/{id}/resolve   [Admin]
    // -------------------------------------------------------------------------

    /// <summary>Manually resolves an open alert, marking it as Closed.</summary>
    [HttpPut("api/alerts/{id:int}/resolve")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(AlertResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Resolve(
        [FromRoute] int id,
        [FromBody] ResolveAlertDto dto,
        CancellationToken ct)
    {
        try
        {
            var result = await _service.ResolveAsync(id, dto, ct);
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
