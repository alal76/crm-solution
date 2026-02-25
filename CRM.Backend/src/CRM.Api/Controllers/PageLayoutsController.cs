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
/// Manages page layout configurations for list and detail views (CUST-03/04).
/// </summary>
[ApiController]
[Route("api/page-layouts")]
[Authorize]
public class PageLayoutsController : ControllerBase
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<PageLayoutsController> _logger;

    public PageLayoutsController(ICrmDbContext db, ILogger<PageLayoutsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }

    /// <summary>Gets all page layouts, optionally filtered by entity type.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? entityType, CancellationToken ct)
    {
        var query = _db.PageLayouts.AsNoTracking().Where(p => !p.IsDeleted);
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(p => p.EntityType == entityType);
        var results = await query.OrderBy(p => p.EntityType).ThenBy(p => p.Name).ToListAsync(ct);
        return Ok(results);
    }

    /// <summary>Gets a single page layout by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var layout = await _db.PageLayouts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        return layout == null ? NotFound(new { message = "Page layout not found" }) : Ok(layout);
    }

    /// <summary>Gets the default layout for an entity type.</summary>
    [HttpGet("default")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDefault([FromQuery] string entityType, CancellationToken ct)
    {
        var layout = await _db.PageLayouts.AsNoTracking()
            .FirstOrDefaultAsync(p => p.EntityType == entityType && p.IsDefault && !p.IsDeleted, ct);
        return layout == null ? NotFound(new { message = "No default layout found for entity type" }) : Ok(layout);
    }

    /// <summary>Gets the column preferences for the current user.</summary>
    [HttpGet("user-preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserPreferences([FromQuery] string entityType, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var pref = await _db.UserListViewPreferences.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && p.EntityType == entityType && !p.IsDeleted, ct);
        return Ok(pref);
    }

    /// <summary>Saves user-specific column preferences.</summary>
    [HttpPut("user-preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SaveUserPreferences([FromBody] SavePreferencesRequest req, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var pref = await _db.UserListViewPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.EntityType == req.EntityType && !p.IsDeleted, ct);

        if (pref == null)
        {
            pref = new CRM.Core.Entities.UserListViewPreference
            {
                UserId = userId,
                EntityType = req.EntityType,
                ColumnsJson = req.ColumnsJson,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.UserListViewPreferences.Add(pref);
        }
        else
        {
            pref.ColumnsJson = req.ColumnsJson;
            pref.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        return Ok(pref);
    }

    /// <summary>Creates a new page layout.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CRM.Core.Entities.PageLayout dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.EntityType))
            return BadRequest(new { message = "EntityType is required." });

        // If this is marked default, un-default existing ones
        if (dto.IsDefault)
        {
            var existing = await _db.PageLayouts
                .Where(p => p.EntityType == dto.EntityType && p.IsDefault && !p.IsDeleted)
                .ToListAsync(ct);
            foreach (var e in existing) e.IsDefault = false;
        }

        dto.CreatedAt = DateTime.UtcNow;
        dto.UpdatedAt = DateTime.UtcNow;
        _db.PageLayouts.Add(dto);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    /// <summary>Updates an existing page layout.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CRM.Core.Entities.PageLayout dto, CancellationToken ct)
    {
        var layout = await _db.PageLayouts.FindAsync(new object[] { id }, ct);
        if (layout == null || layout.IsDeleted)
            return NotFound(new { message = "Page layout not found" });

        if (dto.IsDefault && !layout.IsDefault)
        {
            var existing = await _db.PageLayouts
                .Where(p => p.EntityType == layout.EntityType && p.IsDefault && !p.IsDeleted && p.Id != id)
                .ToListAsync(ct);
            foreach (var e in existing) e.IsDefault = false;
        }

        layout.Name = dto.Name;
        layout.LayoutJson = dto.LayoutJson;
        layout.IsDefault = dto.IsDefault;
        layout.UserGroupId = dto.UserGroupId;
        layout.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(layout);
    }

    /// <summary>Soft-deletes a page layout.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var layout = await _db.PageLayouts.FindAsync(new object[] { id }, ct);
        if (layout == null || layout.IsDeleted)
            return NotFound(new { message = "Page layout not found" });

        layout.IsDeleted = true;
        layout.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record SavePreferencesRequest(string EntityType, string ColumnsJson);
