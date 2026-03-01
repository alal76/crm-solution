// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API endpoints for managing analytics events.
/// </summary>
/// <remarks>
/// Provides operations for:
/// - Creating analytics events to track user actions and system events
/// - Querying events by entity, user, or custom filters
/// - Event retrieval for business intelligence and auditing
/// </remarks>
[ApiController]
[Route("api/analytics-events")]
[Authorize]
[Produces("application/json")]
public class AnalyticsEventsController : CrmControllerBase
{
    private readonly IAnalyticsEventService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyticsEventsController"/> class.
    /// </summary>
    /// <param name="service">The analytics event service.</param>
    /// <param name="logger">The logger.</param>
    public AnalyticsEventsController(IAnalyticsEventService service)
    {
        _service = service;
    }

    /// <summary>
    /// Creates a new analytics event.
    /// </summary>
    /// <param name="dto">The event creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created analytics event.</returns>
    /// <response code="201">Event created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    [HttpPost]
    [ProducesResponseType(typeof(AnalyticsEventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AnalyticsEventDto>> Create(
        [FromBody] CreateAnalyticsEventDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

                var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Gets an analytics event by ID.
    /// </summary>
    /// <param name="id">The event ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The analytics event.</returns>
    /// <response code="200">Event found.</response>
    /// <response code="404">Event not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AnalyticsEventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AnalyticsEventDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);

        if (result == null)
        {
            return NotFound(new { message = $"Analytics event with ID {id} not found." });
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets all analytics events with optional filtering.
    /// </summary>
    /// <param name="eventName">Filter by event name.</param>
    /// <param name="entityType">Filter by entity type.</param>
    /// <param name="entityId">Filter by entity ID.</param>
    /// <param name="userId">Filter by user ID.</param>
    /// <param name="fromDate">Filter events from this date.</param>
    /// <param name="toDate">Filter events until this date.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of analytics events.</returns>
    /// <response code="200">Returns the list of events.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AnalyticsEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<AnalyticsEventDto>>> GetAll(
        [FromQuery] string? eventName = null,
        [FromQuery] string? entityType = null,
        [FromQuery] int? entityId = null,
        [FromQuery] int? userId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var filter = new AnalyticsEventFilterDto
        {
            EventName = eventName,
            EntityType = entityType,
            EntityId = entityId,
            UserId = userId,
            FromDate = fromDate,
            ToDate = toDate,
            Page = page,
            PageSize = pageSize,
        };

        var results = await _service.GetAllAsync(filter, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Gets analytics events for a specific entity.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="entityId">The entity ID.</param>
    /// <param name="limit">Maximum number of events to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of analytics events for the entity.</returns>
    /// <response code="200">Returns the list of events.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    [HttpGet("entity/{entityType}/{entityId:int}")]
    [ProducesResponseType(typeof(IEnumerable<AnalyticsEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<AnalyticsEventDto>>> GetByEntity(
        string entityType,
        int entityId,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var results = await _service.GetByEntityAsync(entityType, entityId, limit, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Gets analytics events for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="limit">Maximum number of events to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of analytics events for the user.</returns>
    /// <response code="200">Returns the list of events.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    [HttpGet("user/{userId:int}")]
    [ProducesResponseType(typeof(IEnumerable<AnalyticsEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<AnalyticsEventDto>>> GetByUser(
        int userId,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var results = await _service.GetByUserAsync(userId, limit, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Deletes an analytics event (soft delete).
    /// </summary>
    /// <param name="id">The event ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Event deleted successfully.</response>
    /// <response code="404">Event not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound(new { message = $"Analytics event with ID {id} not found." });
        }

        return NoContent();
    }
}
