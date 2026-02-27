// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for event attendee management.
/// </summary>
[ApiController]
[Route("api/event-attendees")]
[Authorize]
[Produces("application/json")]
public class EventAttendeesController : ControllerBase
{
    private const string AttendeeNotFoundMessage = "Event attendee {0} not found";
    private readonly IEventAttendeeService _service;
    private readonly ILogger<EventAttendeesController> _logger;

    public EventAttendeesController(IEventAttendeeService service, ILogger<EventAttendeesController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region CRUD Operations

    /// <summary>Gets all event attendees with optional filters.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<EventAttendee>>> GetAll(
        [FromQuery] int? activityId = null,
        [FromQuery] AttendeeType? attendeeType = null,
        [FromQuery] AttendeeResponseStatus? responseStatus = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var attendees = await _service.GetAllAsync(activityId, attendeeType, responseStatus, cancellationToken);
            return Ok(attendees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event attendees");
            return Problem("An error occurred while retrieving event attendees.");
        }
    }

    /// <summary>Gets an event attendee by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EventAttendee>> GetById(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var attendee = await _service.GetByIdAsync(id, cancellationToken);
            if (attendee == null)
                return NotFound(string.Format(AttendeeNotFoundMessage, id));
            return Ok(attendee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event attendee {AttendeeId}", id);
            return Problem("An error occurred while retrieving the event attendee.");
        }
    }

    /// <summary>Creates a new event attendee.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EventAttendee>> Create([FromBody] EventAttendee attendee, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var created = await _service.CreateAsync(attendee, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event attendee");
            return Problem("An error occurred while creating the event attendee.");
        }
    }

    /// <summary>Updates an existing event attendee.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(int id, [FromBody] EventAttendee attendee, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var updated = await _service.UpdateAsync(id, attendee, cancellationToken);
            if (!updated)
                return NotFound(string.Format(AttendeeNotFoundMessage, id));
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event attendee {AttendeeId}", id);
            return Problem("An error occurred while updating the event attendee.");
        }
    }

    /// <summary>Deletes an event attendee (soft delete).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _service.DeleteAsync(id, cancellationToken);
            if (!deleted)
                return NotFound(string.Format(AttendeeNotFoundMessage, id));
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event attendee {AttendeeId}", id);
            return Problem("An error occurred while deleting the event attendee.");
        }
    }

    #endregion

    #region Attendee-Specific Operations

    /// <summary>Gets all attendees for a specific activity/event.</summary>
    [HttpGet("by-activity/{activityId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<EventAttendee>>> GetByActivity(int activityId, CancellationToken cancellationToken = default)
    {
        try
        {
            var attendees = await _service.GetByActivityAsync(activityId, cancellationToken);
            return Ok(attendees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attendees for activity {ActivityId}", activityId);
            return Problem("An error occurred while retrieving attendees by activity.");
        }
    }

    /// <summary>Updates the response status for an attendee (accept, decline, tentative).</summary>
    [HttpPatch("{id}/response")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateResponse(int id, [FromBody] UpdateResponseRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var updated = await _service.UpdateResponseAsync(id, request.Status, request.Comment, cancellationToken);
            if (!updated)
                return NotFound(string.Format(AttendeeNotFoundMessage, id));
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating response for attendee {AttendeeId}", id);
            return Problem("An error occurred while updating the attendee response.");
        }
    }

    /// <summary>Records attendance after an event.</summary>
    [HttpPost("{id}/record-attendance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RecordAttendance(int id, [FromBody] RecordAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var recorded = await _service.RecordAttendanceAsync(id, request.DidAttend, request.DurationMinutes, request.Notes, cancellationToken);
            if (!recorded)
                return NotFound(string.Format(AttendeeNotFoundMessage, id));
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording attendance for attendee {AttendeeId}", id);
            return Problem("An error occurred while recording attendance.");
        }
    }

    #endregion

    #region Request DTOs

    public class UpdateResponseRequest
    {
        [Required]
        public AttendeeResponseStatus Status { get; set; }

        public string? Comment { get; set; }
    }

    public class RecordAttendanceRequest
    {
        [Required]
        public bool DidAttend { get; set; }

        public int? DurationMinutes { get; set; }

        public string? Notes { get; set; }
    }

    #endregion
}
