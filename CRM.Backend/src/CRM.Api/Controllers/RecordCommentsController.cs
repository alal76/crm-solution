// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// CRUD endpoints for threaded record comments on any CRM entity.
/// </summary>
[ApiController]
[Route("api/comments")]
[Authorize]
[Produces("application/json")]
public class RecordCommentsController : CrmControllerBase
{
    private readonly IRecordCommentService _service;

    public RecordCommentsController(IRecordCommentService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value
                 ?? User.FindFirst("userId")?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }

    /// <summary>
    /// Get all top-level comments (with threaded replies) for an entity.
    /// </summary>
    /// <param name="entityType">Entity type, e.g. "Account", "Lead".</param>
    /// <param name="entityId">Entity ID.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RecordCommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<RecordCommentDto>>> GetByEntity(
        [FromQuery] string entityType,
        [FromQuery] int entityId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(entityType) || entityId <= 0)
            return BadRequest("entityType and entityId are required.");

        var comments = await _service.GetByEntityAsync(entityType, entityId, ct);
        return Ok(comments);
    }

    /// <summary>Get a single comment by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RecordCommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RecordCommentDto>> GetById(int id, CancellationToken ct = default)
    {
        var comment = await _service.GetByIdAsync(id, ct);
        if (comment == null)
            return NotFound();
        return Ok(comment);
    }

    /// <summary>Get all replies to a parent comment.</summary>
    [HttpGet("{id:int}/thread")]
    [ProducesResponseType(typeof(IEnumerable<RecordCommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<RecordCommentDto>>> GetThread(int id, CancellationToken ct = default)
    {
        var replies = await _service.GetThreadAsync(id, ct);
        return Ok(replies);
    }

    /// <summary>Create a new comment on an entity.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(RecordCommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RecordCommentDto>> Create(
        [FromBody] CreateRecordCommentDto dto, CancellationToken ct = default)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest("Content is required.");

        var authorId = GetCurrentUserId();
        if (authorId == 0)
            return Unauthorized();

        var created = await _service.CreateAsync(dto, authorId, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Update an existing comment (owner only).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(RecordCommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RecordCommentDto>> Update(
        int id, [FromBody] UpdateRecordCommentDto dto, CancellationToken ct = default)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest("Content is required.");

        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized();

        var updated = await _service.UpdateAsync(id, dto, userId, ct);
        if (updated == null)
            return NotFound();

        return Ok(updated);
    }

    /// <summary>Soft-delete a comment (owner or admin).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized();

        var deleted = await _service.DeleteAsync(id, userId, ct);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
