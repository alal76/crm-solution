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
[Route("api/tags")]
[Authorize]
public class TagsController : CrmControllerBase
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<TagsController> _logger;

    public TagsController(ICrmDbContext db, ILogger<TagsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tags = await _db.Tags.AsNoTracking()
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
        return Ok(tags);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var tag = await _db.Tags.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct);
        return tag == null ? NotFound(new { message = "Tag not found" }) : Ok(tag);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] Tag dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Name is required." });

        var duplicate = await _db.Tags.AnyAsync(t => t.Name == dto.Name && !t.IsDeleted, ct);
        if (duplicate)
            return BadRequest(new { message = $"A tag with name '{dto.Name}' already exists." });

        dto.CreatedAt = DateTime.UtcNow;
        dto.UpdatedAt = DateTime.UtcNow;
        _db.Tags.Add(dto);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] Tag dto, CancellationToken ct)
    {
        var existing = await _db.Tags.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct);
        if (existing == null)
            return NotFound(new { message = "Tag not found" });

        existing.Name = dto.Name;
        existing.Color = dto.Color;
        existing.Description = dto.Description;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var existing = await _db.Tags.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct);
        if (existing == null)
            return NotFound(new { message = "Tag not found" });

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
