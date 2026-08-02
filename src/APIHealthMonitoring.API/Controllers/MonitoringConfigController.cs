using APIHealthMonitoring.Application.DTOs.MonitoringConfig;
using APIHealthMonitoring.Application.Interfaces.Endpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIHealthMonitoring.API.Controllers;

/// <summary>
/// Handles endpoints for managing API Endpoint Monitoring Configurations.
/// </summary>
[ApiController]
[Route("api/endpoints/{id:int}/config")]
[Authorize]
[Produces("application/json")]
public class MonitoringConfigController : ControllerBase
{
    private readonly IMonitoringConfigService _service;

    public MonitoringConfigController(IMonitoringConfigService service)
    {
        _service = service;
    }

    // -------------------------------------------------------------------------
    // POST /api/endpoints/{id}/config   [Admin]
    // -------------------------------------------------------------------------

    /// <summary>Create a monitoring configuration for an API endpoint.</summary>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(MonitoringConfigResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromRoute] int id,
        [FromBody] CreateMonitoringConfigDto request,
        CancellationToken ct)
    {
        request.ApiEndpointId = id;

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetByEndpointId), new { id = result.ApiEndpointId }, result);
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

    // -------------------------------------------------------------------------
    // GET /api/endpoints/{id}/config   [Admin, Viewer]
    // -------------------------------------------------------------------------

    /// <summary>Get the monitoring configuration for an API endpoint.</summary>
    [HttpGet]
    [Authorize(Roles = "Administrator,Viewer")]
    [ProducesResponseType(typeof(MonitoringConfigResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByEndpointId(
        [FromRoute] int id,
        CancellationToken ct)
    {
        try
        {
            var result = await _service.GetByEndpointIdAsync(id, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // PUT /api/endpoints/{id}/config   [Admin]
    // -------------------------------------------------------------------------

    /// <summary>Update the monitoring configuration for an API endpoint.</summary>
    [HttpPut]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(MonitoringConfigResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] UpdateMonitoringConfigDto request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _service.UpdateAsync(id, request, ct);
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

    // -------------------------------------------------------------------------
    // DELETE /api/endpoints/{id}/config   [Admin]
    // -------------------------------------------------------------------------

    /// <summary>Reset the monitoring configuration for an API endpoint back to defaults.</summary>
    [HttpDelete]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(MonitoringConfigResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResetToDefaults(
        [FromRoute] int id,
        CancellationToken ct)
    {
        try
        {
            var result = await _service.ResetToDefaultsAsync(id, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
