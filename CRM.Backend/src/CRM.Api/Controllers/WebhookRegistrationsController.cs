using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing webhook registrations.
/// Uses the WebhookManagementService for CRUD operations on webhook subscriptions.
/// </summary>
[ApiController]
[Route("api/webhook-registrations")]
[Authorize]
[Produces("application/json")]
public class WebhookRegistrationsController : ControllerBase
{
    private readonly IWebhookManagementService _service;
    private readonly ILogger<WebhookRegistrationsController> _logger;

    public WebhookRegistrationsController(IWebhookManagementService service, ILogger<WebhookRegistrationsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Register a new webhook.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(WebhookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WebhookDto>> Create([FromBody] CreateWebhookDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating webhook registration");
            return StatusCode(500, new { error = "Failed to create webhook registration" });
        }
    }

    /// <summary>
    /// Get a webhook by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(WebhookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WebhookDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Get all registered webhooks.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WebhookDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WebhookDto>>> GetAll([FromQuery] bool? isActive = null, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(isActive, ct);
        return Ok(result);
    }

    /// <summary>
    /// Delete a webhook registration.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _service.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
