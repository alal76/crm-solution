// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Infrastructure;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/views")]
[Authorize]
public class ViewsController : CrmControllerBase
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<ViewsController> _logger;

    public ViewsController(ICrmDbContext db, ILogger<ViewsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? entityType, CancellationToken ct)
    {
        var query = _db.SavedViews.AsNoTracking()
            .Where(v => !v.IsDeleted);

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(v => v.EntityType == entityType);

        var views = await query.OrderBy(v => v.Name).ToListAsync(ct);
        return Ok(views);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var view = await _db.SavedViews.AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted, ct);
        return view == null ? NotFound(new { message = "View not found" }) : Ok(view);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] SavedView dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.EntityType))
            return BadRequest(new { message = "Name and EntityType are required." });

        dto.CreatedAt = DateTime.UtcNow;
        dto.UpdatedAt = DateTime.UtcNow;
        _db.SavedViews.Add(dto);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] SavedView dto, CancellationToken ct)
    {
        var existing = await _db.SavedViews
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted, ct);
        if (existing == null)
            return NotFound(new { message = "View not found" });

        existing.Name = dto.Name;
        existing.EntityType = dto.EntityType;
        existing.IsDefault = dto.IsDefault;
        existing.IsShared = dto.IsShared;
        existing.ColumnsJson = dto.ColumnsJson;
        existing.FiltersJson = dto.FiltersJson;
        existing.SortJson = dto.SortJson;
        existing.PageSize = dto.PageSize;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var existing = await _db.SavedViews
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted, ct);
        if (existing == null)
            return NotFound(new { message = "View not found" });

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
