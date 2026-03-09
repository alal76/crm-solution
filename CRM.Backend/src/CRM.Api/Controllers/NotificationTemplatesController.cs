using CRM.Api.Infrastructure;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/notification-templates")]
[Authorize]
public class NotificationTemplatesController : CrmControllerBase
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<NotificationTemplatesController> _logger;

    public NotificationTemplatesController(ICrmDbContext db, ILogger<NotificationTemplatesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? channel, CancellationToken ct)
    {
        var query = _db.NotificationTemplates.AsNoTracking()
            .Where(t => !t.IsDeleted);

        if (!string.IsNullOrWhiteSpace(channel))
            query = query.Where(t => t.Channel == channel);

        var templates = await query.OrderBy(t => t.Name).ToListAsync(ct);
        return Ok(templates);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var template = await _db.NotificationTemplates.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct);
        return template == null ? NotFound(new { message = "Notification template not found" }) : Ok(template);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] NotificationTemplate dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Name is required." });

        dto.CreatedAt = DateTime.UtcNow;
        dto.UpdatedAt = DateTime.UtcNow;
        _db.NotificationTemplates.Add(dto);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] NotificationTemplate dto, CancellationToken ct)
    {
        var existing = await _db.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct);
        if (existing == null)
            return NotFound(new { message = "Notification template not found" });

        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.Channel = dto.Channel;
        existing.Subject = dto.Subject;
        existing.Body = dto.Body;
        existing.EventTrigger = dto.EventTrigger;
        existing.IsActive = dto.IsActive;
        existing.Variables = dto.Variables;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var existing = await _db.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct);
        if (existing == null)
            return NotFound(new { message = "Notification template not found" });

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
