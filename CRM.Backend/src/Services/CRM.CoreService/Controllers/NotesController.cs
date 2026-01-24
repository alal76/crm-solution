using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

/// <summary>
/// API endpoints for managing notes
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILogger<NotesController> _logger;
    private readonly NormalizationService _normalization;

    public NotesController(CrmDbContext context, ILogger<NotesController> logger, NormalizationService normalization)
    {
        _context = context;
        _logger = logger;
        _normalization = normalization;
    }

    /// <summary>
    /// Get all notes with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Note>>> GetNotes(
        [FromQuery] int? customerId = null,
        [FromQuery] int? opportunityId = null,
        [FromQuery] int? productId = null,
        [FromQuery] NoteType? noteType = null,
        [FromQuery] bool? pinned = null)
    {
        var query = _context.Notes
            .Include(n => n.Customer)
            .Include(n => n.Opportunity)
            .Include(n => n.CreatedByUser)
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(n => n.CustomerId == customerId);
        
        if (opportunityId.HasValue)
            query = query.Where(n => n.OpportunityId == opportunityId);
        
        if (productId.HasValue)
            query = query.Where(n => n.ProductId == productId);
        
        if (noteType.HasValue)
            query = query.Where(n => n.NoteType == noteType);
        
        if (pinned == true)
            query = query.Where(n => n.IsPinned);

        var notes = await query
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.CreatedAt)
            .ToListAsync();

        // Prefer normalized tags/custom fields when available
        foreach (var note in notes)
        {
            var nt = await _normalization.GetTagsAsync("Note", note.Id);
            if (!string.IsNullOrWhiteSpace(nt)) note.Tags = nt;
            var cf = await _normalization.GetCustomFieldsAsync("Note", note.Id);
            if (!string.IsNullOrWhiteSpace(cf)) note.CustomFields = cf;
        }

        return Ok(notes);
    }

    /// <summary>
    /// Get a note by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Note>> GetNote(int id)
    {
        var note = await _context.Notes
            .Include(n => n.Customer)
            .Include(n => n.Opportunity)
            .Include(n => n.CreatedByUser)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (note == null)
            return NotFound();

        var nt = await _normalization.GetTagsAsync("Note", note.Id);
        if (!string.IsNullOrWhiteSpace(nt)) note.Tags = nt;
        var cf = await _normalization.GetCustomFieldsAsync("Note", note.Id);
        if (!string.IsNullOrWhiteSpace(cf)) note.CustomFields = cf;

        return Ok(note);
    }

    /// <summary>
    /// Create a new note
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Note>> CreateNote(Note note)
    {
        note.CreatedAt = DateTime.UtcNow;
        note.UpdatedAt = DateTime.UtcNow;

        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Note {NoteId} created: {Title}", note.Id, note.Title);
        return CreatedAtAction(nameof(GetNote), new { id = note.Id }, note);
    }

    /// <summary>
    /// Update an existing note
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNote(int id, Note note)
    {
        if (id != note.Id)
            return BadRequest();

        var existingNote = await _context.Notes.FindAsync(id);
        if (existingNote == null)
            return NotFound();

        _context.Entry(existingNote).CurrentValues.SetValues(note);
        existingNote.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Delete a note
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNote(int id)
    {
        var note = await _context.Notes.FindAsync(id);
        if (note == null)
            return NotFound();

        _context.Notes.Remove(note);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Note {NoteId} deleted", id);
        return NoContent();
    }

    /// <summary>
    /// Toggle pin status of a note
    /// </summary>
    [HttpPost("{id}/toggle-pin")]
    public async Task<IActionResult> TogglePin(int id)
    {
        var note = await _context.Notes.FindAsync(id);
        if (note == null)
            return NotFound();

        note.IsPinned = !note.IsPinned;
        note.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(note);
    }

    /// <summary>
    /// Get notes for a specific entity
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<ActionResult<IEnumerable<Note>>> GetNotesByEntity(string entityType, int entityId)
    {
        var query = _context.Notes.Include(n => n.CreatedByUser).AsQueryable();

        query = entityType.ToLower() switch
        {
            "customer" => query.Where(n => n.CustomerId == entityId),
            "opportunity" => query.Where(n => n.OpportunityId == entityId),
            "product" => query.Where(n => n.ProductId == entityId),
            "campaign" => query.Where(n => n.CampaignId == entityId),
            _ => query.Where(n => false)
        };

        var notes = await query
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.CreatedAt)
            .ToListAsync();

        foreach (var note in notes)
        {
            var nt = await _normalization.GetTagsAsync("Note", note.Id);
            if (!string.IsNullOrWhiteSpace(nt)) note.Tags = nt;
            var cf = await _normalization.GetCustomFieldsAsync("Note", note.Id);
            if (!string.IsNullOrWhiteSpace(cf)) note.CustomFields = cf;
        }

        return Ok(notes);
    }
}
