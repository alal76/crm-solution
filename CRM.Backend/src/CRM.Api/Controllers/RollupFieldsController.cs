// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

/// <summary>
/// Manages rollup field definitions and triggers on-demand recalculations (CUST-07/08).
/// </summary>
[ApiController]
[Route("api/rollup-fields")]
[Authorize]
public class RollupFieldsController : ControllerBase
{
    private readonly IRollupFieldService _rollupSvc;
    private readonly ICrmDbContext _db;

    public RollupFieldsController(IRollupFieldService rollupSvc, ICrmDbContext db)
    {
        _rollupSvc = rollupSvc;
        _db = db;
    }

    /// <summary>Gets all rollup field definitions for a parent entity type.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? parentEntityType, CancellationToken ct)
    {
        var query = _db.RollupFields.AsNoTracking().Where(r => !r.IsDeleted && r.IsActive);
        if (!string.IsNullOrWhiteSpace(parentEntityType))
            query = query.Where(r => r.ParentEntityType == parentEntityType);
        return Ok(await query.ToListAsync(ct));
    }

    /// <summary>Gets all registered rollup definitions for a parent entity type (from in-memory service registry).</summary>
    [HttpGet("definitions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDefinitions([FromQuery] string parentEntityType, CancellationToken ct)
    {
        var definitions = await _rollupSvc.GetDefinitionsAsync(parentEntityType, ct);
        return Ok(definitions);
    }

    /// <summary>
    /// Calculates a rollup value on demand.
    /// </summary>
    [HttpPost("calculate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Calculate([FromBody] RollupRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ParentEntityType))
            return BadRequest(new { message = "ParentEntityType is required." });

        var result = await _rollupSvc.CalculateAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Recalculates all rollup fields for a specific parent entity instance.</summary>
    [HttpPost("{parentEntityType}/{parentId:int}/recalculate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Recalculate(string parentEntityType, int parentId, CancellationToken ct)
    {
        var results = await _rollupSvc.RecalculateAllForEntityAsync(parentEntityType, parentId, ct);
        return Ok(results);
    }

    /// <summary>Saves a rollup field definition to the database.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CRM.Core.Entities.RollupField dto, CancellationToken ct)
    {
        dto.CreatedAt = DateTime.UtcNow;
        dto.UpdatedAt = DateTime.UtcNow;
        _db.RollupFields.Add(dto);
        await _db.SaveChangesAsync(ct);
        return Created($"api/rollup-fields/{dto.Id}", dto);
    }
}
