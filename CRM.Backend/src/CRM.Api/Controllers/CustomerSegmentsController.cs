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
[Route("api/customer-segments")]
[Authorize]
public class CustomerSegmentsController : CrmControllerBase
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<CustomerSegmentsController> _logger;

    public CustomerSegmentsController(ICrmDbContext db, ILogger<CustomerSegmentsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var segments = await _db.CustomerSegments.AsNoTracking()
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
        return Ok(segments);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var segment = await _db.CustomerSegments.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, ct);
        return segment == null ? NotFound(new { message = "Customer segment not found" }) : Ok(segment);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CustomerSegment dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Name is required." });

        dto.CreatedAt = DateTime.UtcNow;
        dto.UpdatedAt = DateTime.UtcNow;
        _db.CustomerSegments.Add(dto);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CustomerSegment dto, CancellationToken ct)
    {
        var existing = await _db.CustomerSegments
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, ct);
        if (existing == null)
            return NotFound(new { message = "Customer segment not found" });

        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.CriteriaJson = dto.CriteriaJson;
        existing.SegmentType = dto.SegmentType;
        existing.IsDynamic = dto.IsDynamic;
        existing.IsActive = dto.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var existing = await _db.CustomerSegments
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, ct);
        if (existing == null)
            return NotFound(new { message = "Customer segment not found" });

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
