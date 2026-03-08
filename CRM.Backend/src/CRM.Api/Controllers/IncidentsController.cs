// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Incident Management API endpoints for handling service interruptions and issues.
/// </summary>
/// <remarks>
/// Incident Management is the process of managing the lifecycle of all incidents.
/// The primary objective is to return the IT service to users as quickly as possible.
/// Key features include: incident creation, assignment, escalation, resolution, and SLA tracking.
/// </remarks>
[ApiController]
[Route("api/itsm/incidents")]
[Authorize]
[Produces("application/json")]
[Consumes("application/json")]
[Tags("ITSM - Incident Management")]
public class IncidentsController : CrmControllerBase
{
    private readonly IIncidentService _incidentService;

    public IncidentsController(IIncidentService incidentService)
    {
        _incidentService = incidentService;
    }

    /// <summary>
    /// Create a new incident.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IncidentDto>> CreateIncident([FromBody] CreateIncidentDto dto)
    {
        var userId = GetCurrentUserId();
        var incident = await _incidentService.CreateIncidentAsync(dto, userId);
        return CreatedAtAction(nameof(GetIncident), new { id = incident.IncidentId }, incident);
    }

    /// <summary>
    /// Get incident by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentDto>> GetIncident(int id)
    {
        var incident = await _incidentService.GetIncidentByIdAsync(id);
        if (incident == null)
        {
            return NotFound();
        }
        return Ok(incident);
    }

    /// <summary>
    /// Get incidents with filtering and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<IncidentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<IncidentDto>>> GetIncidents(
        [FromQuery] string? searchTerm,
        [FromQuery] int? state,
        [FromQuery] int? priority,
        [FromQuery] int? assignedToId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new IncidentFilterDto
        {
            SearchTerm = searchTerm,
            State = state.HasValue ? (IncidentState)state.Value : null,
            Priority = priority,
            AssignedToId = assignedToId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var (items, totalCount) = await _incidentService.GetIncidentsAsync(filter);
        var result = new PagedResult<IncidentDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(result);
    }

    /// <summary>
    /// Update an incident.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentDto>> UpdateIncident(int id, [FromBody] UpdateIncidentDto dto)
    {
        var userId = GetCurrentUserId();
        var incident = await _incidentService.UpdateIncidentAsync(id, dto, userId);
        return Ok(incident);
    }

    /// <summary>
    /// Assign incident to a user or group.
    /// </summary>
    [HttpPatch("{id}/assign")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentDto>> AssignIncident(int id, [FromBody] AssignIncidentDto dto)
    {
        var userId = GetCurrentUserId();
        var incident = await _incidentService.AssignIncidentAsync(id, dto.AssignedToId, dto.AssignedGroupId, userId);
        return Ok(incident);
    }

    /// <summary>
    /// Escalate an incident.
    /// </summary>
    [HttpPatch("{id}/escalate")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> EscalateIncident(int id, [FromBody] EscalateIncidentDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _incidentService.EscalateIncidentAsync(id, userId);
        return Ok(result);
    }

    /// <summary>
    /// Resolve an incident.
    /// </summary>
    [HttpPatch("{id}/resolve")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentDto>> ResolveIncident(int id, [FromBody] ResolveIncidentDto dto)
    {
        var userId = GetCurrentUserId();
        var incident = await _incidentService.ResolveIncidentAsync(id, dto, userId);
        return Ok(incident);
    }

    /// <summary>
    /// Close an incident.
    /// </summary>
    [HttpPatch("{id}/close")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentDto>> CloseIncident(int id)
    {
        var userId = GetCurrentUserId();
        var incident = await _incidentService.CloseIncidentAsync(id, userId);
        return Ok(incident);
    }

    /// <summary>
    /// Reopen a closed incident.
    /// </summary>
    [HttpPatch("{id}/reopen")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentDto>> ReopenIncident(int id)
    {
        var userId = GetCurrentUserId();
        var incident = await _incidentService.ReopenIncidentAsync(id, userId);
        return Ok(incident);
    }

    /// <summary>
    /// Add a comment to an incident.
    /// </summary>
    [HttpPost("{id}/comments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> AddComment(int id, [FromBody] AddIncidentCommentDto dto)
    {
        var userId = GetCurrentUserId();
        await _incidentService.AddCommentAsync(id, dto.CommentText, dto.IsInternal, userId);
        return Ok();
    }

    /// <summary>
    /// Get comments for an incident.
    /// </summary>
    [HttpGet("{id}/comments")]
    [ProducesResponseType(typeof(IEnumerable<IncidentCommentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IncidentCommentDto>>> GetComments(int id)
    {
        var comments = await _incidentService.GetCommentsAsync(id);
        return Ok(comments);
    }

    private int GetCurrentUserId() // NOSONAR
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim?.Value ?? "1");
    }
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class AssignIncidentDto
{
    public int? AssignedToId { get; set; }
    public int? AssignedGroupId { get; set; }
}

public class EscalateIncidentDto
{
    public int EscalationLevel { get; set; }
}

public class AddIncidentCommentDto
{
    public string CommentText { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
}

public class IncidentCommentDto
{
    public int CommentId { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public int CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; }
}
