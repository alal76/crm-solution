// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CRM.Api.Controllers;

/// <summary>
/// Saved search preset CRUD endpoints.
/// Implements TODO-PORTAL-06.
/// </summary>
[ApiController]
[Route("api/saved-searches")]
[Authorize]
public class SavedSearchesController : ControllerBase
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<SavedSearchesController> _logger;

    public SavedSearchesController(ICrmDbContext db, ILogger<SavedSearchesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }

    /// <summary>Returns all saved searches for the current user (plus public ones).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SavedFilter>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? entityType, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var query = _db.SavedFilters
            .AsNoTracking()
            .Where(f => !f.IsDeleted && (f.UserId == userId || f.IsPublic));

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(f => f.EntityType == entityType);

        var results = await query
            .OrderByDescending(f => f.IsPinned)
            .ThenByDescending(f => f.LastUsedAt)
            .ThenBy(f => f.Name)
            .ToListAsync(ct);

        return Ok(results);
    }

    /// <summary>Returns a specific saved search by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SavedFilter), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var filter = await _db.SavedFilters
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted && (f.UserId == userId || f.IsPublic), ct);

        return filter is null ? NotFound() : Ok(filter);
    }

    /// <summary>Creates a new saved search for the current user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(SavedFilter), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] SavedFilterCreateDto dto, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var filter = new SavedFilter
        {
            Name = dto.Name,
            EntityType = dto.EntityType,
            FilterCriteriaJson = dto.FilterCriteriaJson,
            SortConfigJson = dto.SortConfigJson,
            IsPublic = dto.IsPublic,
            IsPinned = dto.IsPinned,
            Description = dto.Description,
            UserId = userId.Value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.SavedFilters.Add(filter);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Saved search created: {Id} by user {UserId}", filter.Id, userId);
        return CreatedAtAction(nameof(GetById), new { id = filter.Id }, filter);
    }

    /// <summary>Updates an existing saved search (owner only).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] SavedFilterCreateDto dto, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var filter = await _db.SavedFilters
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, ct);

        if (filter is null) return NotFound();
        if (filter.UserId != userId) return Forbid();

        filter.Name = dto.Name;
        filter.EntityType = dto.EntityType;
        filter.FilterCriteriaJson = dto.FilterCriteriaJson;
        filter.SortConfigJson = dto.SortConfigJson;
        filter.IsPublic = dto.IsPublic;
        filter.IsPinned = dto.IsPinned;
        filter.Description = dto.Description;
        filter.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return Ok(filter);
    }

    /// <summary>Increments the usage count when a filter is applied.</summary>
    [HttpPost("{id:int}/use")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RecordUsage(int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var filter = await _db.SavedFilters
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted && (f.UserId == userId || f.IsPublic), ct);

        if (filter is null) return NotFound();

        filter.UsageCount++;
        filter.LastUsedAt = DateTime.UtcNow;
        filter.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Soft-deletes the specified saved search (owner only).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var filter = await _db.SavedFilters
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, ct);

        if (filter is null) return NotFound();
        if (filter.UserId != userId) return Forbid();

        filter.IsDeleted = true;
        filter.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

/// <summary>DTO for creating/updating a saved filter.</summary>
public class SavedFilterCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string FilterCriteriaJson { get; set; } = "[]";
    public string? SortConfigJson { get; set; }
    public bool IsPublic { get; set; }
    public bool IsPinned { get; set; }
    public string? Description { get; set; }
}
