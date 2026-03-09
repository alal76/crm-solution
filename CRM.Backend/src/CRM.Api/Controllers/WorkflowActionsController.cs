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
[Route("api/workflow-actions")]
[Authorize]
public class WorkflowActionsController : CrmControllerBase
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<WorkflowActionsController> _logger;

    public WorkflowActionsController(ICrmDbContext db, ILogger<WorkflowActionsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? category, CancellationToken ct)
    {
        var query = _db.WorkflowActions.AsNoTracking()
            .Where(a => !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(a => a.Category == category);

        var actions = await query.OrderBy(a => a.Name).ToListAsync(ct);
        return Ok(actions);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var action = await _db.WorkflowActions.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, ct);
        return action == null ? NotFound(new { message = "Workflow action not found" }) : Ok(action);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] WorkflowAction dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.ActionType))
            return BadRequest(new { message = "Name and ActionType are required." });

        dto.CreatedAt = DateTime.UtcNow;
        dto.UpdatedAt = DateTime.UtcNow;
        _db.WorkflowActions.Add(dto);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] WorkflowAction dto, CancellationToken ct)
    {
        var existing = await _db.WorkflowActions
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, ct);
        if (existing == null)
            return NotFound(new { message = "Workflow action not found" });

        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.ActionType = dto.ActionType;
        existing.ConfigurationJson = dto.ConfigurationJson;
        existing.IsActive = dto.IsActive;
        existing.Category = dto.Category;
        existing.Icon = dto.Icon;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var existing = await _db.WorkflowActions
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, ct);
        if (existing == null)
            return NotFound(new { message = "Workflow action not found" });

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
