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
[Route("api/automation/rules")]
[Authorize]
public class AutomationRulesController : CrmControllerBase
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<AutomationRulesController> _logger;

    public AutomationRulesController(ICrmDbContext db, ILogger<AutomationRulesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? entityType, CancellationToken ct)
    {
        var query = _db.AutomationRules.AsNoTracking()
            .Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(r => r.EntityType == entityType);

        var rules = await query.OrderBy(r => r.Priority).ThenBy(r => r.Name).ToListAsync(ct);
        return Ok(rules);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var rule = await _db.AutomationRules.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, ct);
        return rule == null ? NotFound(new { message = "Automation rule not found" }) : Ok(rule);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] AutomationRule dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.EntityType))
            return BadRequest(new { message = "Name and EntityType are required." });

        dto.CreatedAt = DateTime.UtcNow;
        dto.UpdatedAt = DateTime.UtcNow;
        _db.AutomationRules.Add(dto);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] AutomationRule dto, CancellationToken ct)
    {
        var existing = await _db.AutomationRules
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, ct);
        if (existing == null)
            return NotFound(new { message = "Automation rule not found" });

        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.EntityType = dto.EntityType;
        existing.TriggerEvent = dto.TriggerEvent;
        existing.ConditionsJson = dto.ConditionsJson;
        existing.ActionsJson = dto.ActionsJson;
        existing.IsActive = dto.IsActive;
        existing.Priority = dto.Priority;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var existing = await _db.AutomationRules
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, ct);
        if (existing == null)
            return NotFound(new { message = "Automation rule not found" });

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
