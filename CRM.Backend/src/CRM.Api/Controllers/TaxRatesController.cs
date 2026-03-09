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
[Route("api/taxrates")]
[Authorize]
public class TaxRatesController : CrmControllerBase
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<TaxRatesController> _logger;

    public TaxRatesController(ICrmDbContext db, ILogger<TaxRatesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? country, CancellationToken ct)
    {
        var query = _db.TaxRates.AsNoTracking()
            .Where(t => !t.IsDeleted);

        if (!string.IsNullOrWhiteSpace(country))
            query = query.Where(t => t.Country == country);

        var rates = await query.OrderBy(t => t.Name).ToListAsync(ct);
        return Ok(rates);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var rate = await _db.TaxRates.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct);
        return rate == null ? NotFound(new { message = "Tax rate not found" }) : Ok(rate);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] TaxRate dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Name is required." });

        dto.CreatedAt = DateTime.UtcNow;
        dto.UpdatedAt = DateTime.UtcNow;
        _db.TaxRates.Add(dto);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] TaxRate dto, CancellationToken ct)
    {
        var existing = await _db.TaxRates
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct);
        if (existing == null)
            return NotFound(new { message = "Tax rate not found" });

        existing.Name = dto.Name;
        existing.Rate = dto.Rate;
        existing.Country = dto.Country;
        existing.Region = dto.Region;
        existing.TaxType = dto.TaxType;
        existing.IsDefault = dto.IsDefault;
        existing.IsActive = dto.IsActive;
        existing.EffectiveFrom = dto.EffectiveFrom;
        existing.EffectiveTo = dto.EffectiveTo;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var existing = await _db.TaxRates
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct);
        if (existing == null)
            return NotFound(new { message = "Tax rate not found" });

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
