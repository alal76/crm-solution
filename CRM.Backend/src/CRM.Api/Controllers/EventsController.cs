using CRM.Api.Infrastructure;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/events")]
[Authorize]
public class EventsController : CrmControllerBase
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<EventsController> _logger;

    public EventsController(ICrmDbContext db, ILogger<EventsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? eventType,
        CancellationToken ct)
    {
        var query = _db.Events.AsNoTracking()
            .Where(e => !e.IsDeleted);

        if (from.HasValue)
            query = query.Where(e => e.StartDate >= from.Value);
        if (to.HasValue)
            query = query.Where(e => e.StartDate <= to.Value);
        if (!string.IsNullOrWhiteSpace(eventType))
            query = query.Where(e => e.EventType == eventType);

        var events = await query.OrderBy(e => e.StartDate).ToListAsync(ct);
        return Ok(events);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var ev = await _db.Events.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);
        return ev == null ? NotFound(new { message = "Event not found" }) : Ok(ev);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] Event dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "Title is required." });

        dto.CreatedAt = DateTime.UtcNow;
        dto.UpdatedAt = DateTime.UtcNow;
        _db.Events.Add(dto);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] Event dto, CancellationToken ct)
    {
        var existing = await _db.Events
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);
        if (existing == null)
            return NotFound(new { message = "Event not found" });

        existing.Title = dto.Title;
        existing.Description = dto.Description;
        existing.EventType = dto.EventType;
        existing.StartDate = dto.StartDate;
        existing.EndDate = dto.EndDate;
        existing.IsAllDay = dto.IsAllDay;
        existing.Location = dto.Location;
        existing.OrganizerId = dto.OrganizerId;
        existing.RelatedEntityType = dto.RelatedEntityType;
        existing.RelatedEntityId = dto.RelatedEntityId;
        existing.Status = dto.Status;
        existing.RecurrenceRule = dto.RecurrenceRule;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var existing = await _db.Events
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);
        if (existing == null)
            return NotFound(new { message = "Event not found" });

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
