// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// CRUD endpoints for dunning schedule steps.
/// Each step defines an automated email reminder sent when an invoice is
/// overdue by a configurable number of days.
///
/// BACK-010: Dunning Schedule CRUD — /api/dunning-schedules
/// </summary>
[ApiController]
[Route("api/dunning-schedules")]
[Authorize]
public class DunningSchedulesController : CrmControllerBase
{
    private readonly IDunningScheduleService _service;

    /// <summary>Initialises a new instance of <see cref="DunningSchedulesController"/>.</summary>
    public DunningSchedulesController(
        IDunningScheduleService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // GET /api/dunning-schedules
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Returns all dunning schedule steps ordered by StepOrder.</summary>
    /// <param name="activeOnly">Optional filter: true=active only, false=inactive only, omit=all.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of dunning schedule steps.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DunningScheduleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? activeOnly = null,
        CancellationToken cancellationToken = default)
    {
        var items = await _service.GetAllAsync(activeOnly, cancellationToken);
        return Ok(items);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // GET /api/dunning-schedules/{id}
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Returns a single dunning schedule step by ID.</summary>
    /// <param name="id">Step primary key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DunningScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        var dto = await _service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound(new { Message = $"Dunning schedule {id} not found." }) : Ok(dto);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // POST /api/dunning-schedules
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Creates a new dunning schedule step.</summary>
    /// <param name="dto">Creation payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost]
    [ProducesResponseType(typeof(DunningScheduleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateDunningScheduleDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // PUT /api/dunning-schedules/{id}
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Updates an existing dunning schedule step (partial update).</summary>
    /// <param name="id">Step primary key.</param>
    /// <param name="dto">Fields to update; null properties are skipped.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(DunningScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateDunningScheduleDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _service.UpdateAsync(id, dto, cancellationToken);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Message = $"Dunning schedule {id} not found." });
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // DELETE /api/dunning-schedules/{id}
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Soft-deletes a dunning schedule step.</summary>
    /// <param name="id">Step primary key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound(new { Message = $"Dunning schedule {id} not found." });
    }
}
