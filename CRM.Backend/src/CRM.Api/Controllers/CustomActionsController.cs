// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

/// <summary>
/// Manages custom action buttons for entity pages (CUST-09).
/// </summary>
[ApiController]
[Route("api/custom-actions")]
[Authorize]
public class CustomActionsController : ControllerBase
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<CustomActionsController> _logger;

    public CustomActionsController(ICrmDbContext db, ILogger<CustomActionsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }

    /// <summary>Gets all custom actions, optionally filtered by entity type.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? entityType, [FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _db.CustomActions.AsNoTracking().Where(a => !a.IsDeleted);
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType == entityType);
        if (!includeInactive)
            query = query.Where(a => a.IsActive);
        var results = await query.OrderBy(a => a.EntityType).ThenBy(a => a.Label).ToListAsync(ct);
        return Ok(results);
    }

    /// <summary>Gets a single custom action by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var action = await _db.CustomActions.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, ct);
        return action == null ? NotFound(new { message = "Custom action not found" }) : Ok(action);
    }

    /// <summary>Creates a new custom action.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CustomAction dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.EntityType) || string.IsNullOrWhiteSpace(dto.Label))
            return BadRequest(new { message = "EntityType and Label are required." });

        dto.CreatedAt = DateTime.UtcNow;
        dto.UpdatedAt = DateTime.UtcNow;
        _db.CustomActions.Add(dto);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Custom action '{Label}' created for entity '{EntityType}' by user {UserId}",
            dto.Label, dto.EntityType, GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    /// <summary>Updates an existing custom action.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CustomAction dto, CancellationToken ct = default)
    {
        var existing = await _db.CustomActions.FindAsync(new object[] { id }, ct);
        if (existing == null || existing.IsDeleted)
            return NotFound(new { message = "Custom action not found" });

        existing.Label = dto.Label;
        existing.ActionType = dto.ActionType;
        existing.TargetUrl = dto.TargetUrl;
        existing.IconName = dto.IconName;
        existing.IsActive = dto.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(existing);
    }

    /// <summary>Soft-deletes a custom action.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var existing = await _db.CustomActions.FindAsync(new object[] { id }, ct);
        if (existing == null || existing.IsDeleted)
            return NotFound(new { message = "Custom action not found" });

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
