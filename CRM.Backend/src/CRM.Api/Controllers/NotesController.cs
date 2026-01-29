using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CRM.Api.Controllers;

/// <summary>
/// DTO for creating/updating notes
/// </summary>
public class NoteDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public NoteType NoteType { get; set; } = NoteType.General;
    public NoteVisibility Visibility { get; set; } = NoteVisibility.Team;
    public bool IsPinned { get; set; } = false;
    public bool IsImportant { get; set; } = false;
    
    // Polymorphic entity attachment
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    
    // Legacy FK fields
    public int? CustomerId { get; set; }
    public int? ContactId { get; set; }
    public int? OpportunityId { get; set; }
    public int? CampaignId { get; set; }
    public int? ProductId { get; set; }
    public int? LeadId { get; set; }
    public int? ServiceRequestId { get; set; }
    public int? QuoteId { get; set; }
    
    public string? Tags { get; set; }
    public string? Category { get; set; }
    public string? Attachments { get; set; }
    public string? ContextPath { get; set; }
}

/// <summary>
/// Response DTO with author info and permissions
/// </summary>
public class NoteResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public NoteType NoteType { get; set; }
    public NoteVisibility Visibility { get; set; }
    public bool IsPinned { get; set; }
    public bool IsImportant { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public int? CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public int? LastModifiedByUserId { get; set; }
    public string? LastModifiedByUserName { get; set; }
    public string? Tags { get; set; }
    public string? Category { get; set; }
    public string? Attachments { get; set; }
    public string? ContextPath { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Permissions for current user
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

/// <summary>
/// DTO for quick note creation from context
/// </summary>
public class QuickNoteDto
{
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? ContextPath { get; set; }
}

/// <summary>
/// API endpoints for managing notes with RBAC
/// Notes can be attached to any entity type (Customer, Contact, Lead, Opportunity, Campaign, Quote, ServiceRequest, Product, etc.)
/// Edit/Delete permissions: Creator or users with NotesAdmin/SystemAdmin roles
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILogger<NotesController> _logger;
    private readonly NormalizationService _normalization;
    
    // Roles that can edit/delete any note
    private static readonly string[] AdminRoles = { "SystemAdmin", "Admin", "NotesAdmin", "Manager" };

    public NotesController(CrmDbContext context, ILogger<NotesController> logger, NormalizationService normalization)
    {
        _context = context;
        _logger = logger;
        _normalization = normalization;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("userId")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private async Task<bool> HasAdminRole(int userId)
    {
        if (userId == 0) return false;
        
        var user = await _context.Users
            .Include(u => u.PrimaryGroup)
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user == null) return false;
        
        // Check user group name
        var groupName = user.PrimaryGroup?.Name ?? "";
        if (AdminRoles.Any(r => groupName.Contains(r, StringComparison.OrdinalIgnoreCase)))
            return true;
        
        // Check user's role claim
        var roleClaims = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        return roleClaims.Any(r => AdminRoles.Contains(r, StringComparer.OrdinalIgnoreCase));
    }

    private async Task<(bool CanEdit, bool CanDelete)> GetPermissions(Note note, int currentUserId)
    {
        var isCreator = note.CreatedByUserId == currentUserId;
        var isAdmin = await HasAdminRole(currentUserId);
        
        return (CanEdit: isCreator || isAdmin, CanDelete: isCreator || isAdmin);
    }

    private async Task<NoteResponseDto> MapToResponseDto(Note note, int currentUserId)
    {
        var (canEdit, canDelete) = await GetPermissions(note, currentUserId);
        
        return new NoteResponseDto
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            Summary = note.Summary,
            NoteType = note.NoteType,
            Visibility = note.Visibility,
            IsPinned = note.IsPinned,
            IsImportant = note.IsImportant,
            EntityType = note.EntityType,
            EntityId = note.EntityId,
            CreatedByUserId = note.CreatedByUserId,
            CreatedByUserName = note.CreatedByUser != null 
                ? $"{note.CreatedByUser.FirstName} {note.CreatedByUser.LastName}".Trim() 
                : null,
            LastModifiedByUserId = note.LastModifiedByUserId,
            LastModifiedByUserName = note.LastModifiedByUser != null 
                ? $"{note.LastModifiedByUser.FirstName} {note.LastModifiedByUser.LastName}".Trim() 
                : null,
            Tags = note.Tags,
            Category = note.Category,
            Attachments = note.Attachments,
            ContextPath = note.ContextPath,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt,
            CanEdit = canEdit,
            CanDelete = canDelete
        };
    }

    /// <summary>
    /// Get all notes with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NoteResponseDto>>> GetNotes(
        [FromQuery] int? customerId = null,
        [FromQuery] int? contactId = null,
        [FromQuery] int? opportunityId = null,
        [FromQuery] int? leadId = null,
        [FromQuery] int? campaignId = null,
        [FromQuery] int? quoteId = null,
        [FromQuery] int? serviceRequestId = null,
        [FromQuery] int? productId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] int? entityId = null,
        [FromQuery] NoteType? noteType = null,
        [FromQuery] bool? pinned = null)
    {
        var currentUserId = GetCurrentUserId();
        
        var query = _context.Notes
            .Include(n => n.CreatedByUser)
            .Include(n => n.LastModifiedByUser)
            .Where(n => !n.IsDeleted)
            .AsQueryable();

        // Filter by polymorphic EntityType + EntityId
        if (!string.IsNullOrEmpty(entityType) && entityId.HasValue)
        {
            query = query.Where(n => 
                (n.EntityType == entityType && n.EntityId == entityId) ||
                // Also check legacy FK fields
                (entityType.ToLower() == "customer" && n.CustomerId == entityId) ||
                (entityType.ToLower() == "contact" && n.ContactId == entityId) ||
                (entityType.ToLower() == "opportunity" && n.OpportunityId == entityId) ||
                (entityType.ToLower() == "lead" && n.LeadId == entityId) ||
                (entityType.ToLower() == "campaign" && n.CampaignId == entityId) ||
                (entityType.ToLower() == "quote" && n.QuoteId == entityId) ||
                (entityType.ToLower() == "servicerequest" && n.ServiceRequestId == entityId) ||
                (entityType.ToLower() == "product" && n.ProductId == entityId)
            );
        }
        else
        {
            // Legacy filters
            if (customerId.HasValue)
                query = query.Where(n => n.CustomerId == customerId || (n.EntityType == "Customer" && n.EntityId == customerId));
            if (contactId.HasValue)
                query = query.Where(n => n.ContactId == contactId || (n.EntityType == "Contact" && n.EntityId == contactId));
            if (opportunityId.HasValue)
                query = query.Where(n => n.OpportunityId == opportunityId || (n.EntityType == "Opportunity" && n.EntityId == opportunityId));
            if (leadId.HasValue)
                query = query.Where(n => n.LeadId == leadId || (n.EntityType == "Lead" && n.EntityId == leadId));
            if (campaignId.HasValue)
                query = query.Where(n => n.CampaignId == campaignId || (n.EntityType == "Campaign" && n.EntityId == campaignId));
            if (quoteId.HasValue)
                query = query.Where(n => n.QuoteId == quoteId || (n.EntityType == "Quote" && n.EntityId == quoteId));
            if (serviceRequestId.HasValue)
                query = query.Where(n => n.ServiceRequestId == serviceRequestId || (n.EntityType == "ServiceRequest" && n.EntityId == serviceRequestId));
            if (productId.HasValue)
                query = query.Where(n => n.ProductId == productId || (n.EntityType == "Product" && n.EntityId == productId));
        }
        
        if (noteType.HasValue)
            query = query.Where(n => n.NoteType == noteType);
        
        if (pinned == true)
            query = query.Where(n => n.IsPinned);

        var notes = await query
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.CreatedAt)
            .ToListAsync();

        var results = new List<NoteResponseDto>();
        foreach (var note in notes)
        {
            var nt = await _normalization.GetTagsAsync("Note", note.Id);
            if (!string.IsNullOrWhiteSpace(nt)) note.Tags = nt;
            var cf = await _normalization.GetCustomFieldsAsync("Note", note.Id);
            if (!string.IsNullOrWhiteSpace(cf)) note.CustomFields = cf;
            
            results.Add(await MapToResponseDto(note, currentUserId));
        }

        return Ok(results);
    }

    /// <summary>
    /// Get a note by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<NoteResponseDto>> GetNote(int id)
    {
        var currentUserId = GetCurrentUserId();
        
        var note = await _context.Notes
            .Include(n => n.CreatedByUser)
            .Include(n => n.LastModifiedByUser)
            .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);

        if (note == null)
            return NotFound();

        var nt = await _normalization.GetTagsAsync("Note", note.Id);
        if (!string.IsNullOrWhiteSpace(nt)) note.Tags = nt;
        var cf = await _normalization.GetCustomFieldsAsync("Note", note.Id);
        if (!string.IsNullOrWhiteSpace(cf)) note.CustomFields = cf;

        return Ok(await MapToResponseDto(note, currentUserId));
    }

    /// <summary>
    /// Create a new note
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<NoteResponseDto>> CreateNote(NoteDto dto)
    {
        var currentUserId = GetCurrentUserId();
        
        var note = new Note
        {
            Title = dto.Title,
            Content = dto.Content,
            Summary = dto.Summary,
            NoteType = dto.NoteType,
            Visibility = dto.Visibility,
            IsPinned = dto.IsPinned,
            IsImportant = dto.IsImportant,
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            CustomerId = dto.CustomerId,
            ContactId = dto.ContactId,
            OpportunityId = dto.OpportunityId,
            CampaignId = dto.CampaignId,
            ProductId = dto.ProductId,
            LeadId = dto.LeadId,
            ServiceRequestId = dto.ServiceRequestId,
            QuoteId = dto.QuoteId,
            Tags = dto.Tags,
            Category = dto.Category,
            Attachments = dto.Attachments,
            ContextPath = dto.ContextPath,
            CreatedByUserId = currentUserId > 0 ? currentUserId : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // If EntityType/EntityId provided, also set the corresponding legacy FK
        if (!string.IsNullOrEmpty(dto.EntityType) && dto.EntityId.HasValue)
        {
            switch (dto.EntityType.ToLower())
            {
                case "customer": note.CustomerId = dto.EntityId; break;
                case "contact": note.ContactId = dto.EntityId; break;
                case "opportunity": note.OpportunityId = dto.EntityId; break;
                case "lead": note.LeadId = dto.EntityId; break;
                case "campaign": note.CampaignId = dto.EntityId; break;
                case "quote": note.QuoteId = dto.EntityId; break;
                case "servicerequest": note.ServiceRequestId = dto.EntityId; break;
                case "product": note.ProductId = dto.EntityId; break;
            }
        }

        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        // Reload with includes
        note = await _context.Notes
            .Include(n => n.CreatedByUser)
            .FirstOrDefaultAsync(n => n.Id == note.Id);

        _logger.LogInformation("Note {NoteId} created by user {UserId}: {Title}", note!.Id, currentUserId, note.Title);
        
        return CreatedAtAction(nameof(GetNote), new { id = note.Id }, await MapToResponseDto(note, currentUserId));
    }

    /// <summary>
    /// Update an existing note (RBAC: Creator or Admin roles only)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNote(int id, NoteDto dto)
    {
        if (id != dto.Id)
            return BadRequest("ID mismatch");

        var currentUserId = GetCurrentUserId();
        var existingNote = await _context.Notes.FindAsync(id);
        
        if (existingNote == null || existingNote.IsDeleted)
            return NotFound();

        // RBAC check: only creator or admin can edit
        var (canEdit, _) = await GetPermissions(existingNote, currentUserId);
        if (!canEdit)
        {
            _logger.LogWarning("User {UserId} attempted to edit note {NoteId} without permission", currentUserId, id);
            return Forbid();
        }

        existingNote.Title = dto.Title;
        existingNote.Content = dto.Content;
        existingNote.Summary = dto.Summary;
        existingNote.NoteType = dto.NoteType;
        existingNote.Visibility = dto.Visibility;
        existingNote.IsPinned = dto.IsPinned;
        existingNote.IsImportant = dto.IsImportant;
        existingNote.EntityType = dto.EntityType;
        existingNote.EntityId = dto.EntityId;
        existingNote.Tags = dto.Tags;
        existingNote.Category = dto.Category;
        existingNote.Attachments = dto.Attachments;
        existingNote.LastModifiedByUserId = currentUserId > 0 ? currentUserId : null;
        existingNote.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Note {NoteId} updated by user {UserId}", id, currentUserId);
        return NoContent();
    }

    /// <summary>
    /// Delete a note (soft delete, RBAC: Creator or Admin roles only)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNote(int id)
    {
        var currentUserId = GetCurrentUserId();
        var note = await _context.Notes.FindAsync(id);
        
        if (note == null || note.IsDeleted)
            return NotFound();

        // RBAC check: only creator or admin can delete
        var (_, canDelete) = await GetPermissions(note, currentUserId);
        if (!canDelete)
        {
            _logger.LogWarning("User {UserId} attempted to delete note {NoteId} without permission", currentUserId, id);
            return Forbid();
        }

        // Soft delete
        note.IsDeleted = true;
        note.UpdatedAt = DateTime.UtcNow;
        note.LastModifiedByUserId = currentUserId > 0 ? currentUserId : null;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Note {NoteId} deleted by user {UserId}", id, currentUserId);
        return NoContent();
    }

    /// <summary>
    /// Toggle pin status of a note
    /// </summary>
    [HttpPost("{id}/toggle-pin")]
    public async Task<IActionResult> TogglePin(int id)
    {
        var currentUserId = GetCurrentUserId();
        var note = await _context.Notes
            .Include(n => n.CreatedByUser)
            .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);
            
        if (note == null)
            return NotFound();

        note.IsPinned = !note.IsPinned;
        note.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(await MapToResponseDto(note, currentUserId));
    }

    /// <summary>
    /// Toggle important status of a note
    /// </summary>
    [HttpPost("{id}/toggle-important")]
    public async Task<IActionResult> ToggleImportant(int id)
    {
        var currentUserId = GetCurrentUserId();
        var note = await _context.Notes
            .Include(n => n.CreatedByUser)
            .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);
            
        if (note == null)
            return NotFound();

        note.IsImportant = !note.IsImportant;
        note.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(await MapToResponseDto(note, currentUserId));
    }

    /// <summary>
    /// Get notes for a specific entity (supports all entity types)
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<ActionResult<IEnumerable<NoteResponseDto>>> GetNotesByEntity(string entityType, int entityId)
    {
        var currentUserId = GetCurrentUserId();
        var normalizedType = entityType.ToLower();
        
        var query = _context.Notes
            .Include(n => n.CreatedByUser)
            .Include(n => n.LastModifiedByUser)
            .Where(n => !n.IsDeleted)
            .AsQueryable();

        // Query both polymorphic and legacy FK fields
        query = normalizedType switch
        {
            "customer" => query.Where(n => n.CustomerId == entityId || (n.EntityType == "Customer" && n.EntityId == entityId)),
            "contact" => query.Where(n => n.ContactId == entityId || (n.EntityType == "Contact" && n.EntityId == entityId)),
            "opportunity" => query.Where(n => n.OpportunityId == entityId || (n.EntityType == "Opportunity" && n.EntityId == entityId)),
            "lead" => query.Where(n => n.LeadId == entityId || (n.EntityType == "Lead" && n.EntityId == entityId)),
            "campaign" => query.Where(n => n.CampaignId == entityId || (n.EntityType == "Campaign" && n.EntityId == entityId)),
            "quote" => query.Where(n => n.QuoteId == entityId || (n.EntityType == "Quote" && n.EntityId == entityId)),
            "servicerequest" => query.Where(n => n.ServiceRequestId == entityId || (n.EntityType == "ServiceRequest" && n.EntityId == entityId)),
            "product" => query.Where(n => n.ProductId == entityId || (n.EntityType == "Product" && n.EntityId == entityId)),
            "task" => query.Where(n => n.TaskId == entityId || (n.EntityType == "Task" && n.EntityId == entityId)),
            "interaction" => query.Where(n => n.InteractionId == entityId || (n.EntityType == "Interaction" && n.EntityId == entityId)),
            _ => query.Where(n => n.EntityType == entityType && n.EntityId == entityId)
        };

        var notes = await query
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.CreatedAt)
            .ToListAsync();

        var results = new List<NoteResponseDto>();
        foreach (var note in notes)
        {
            var nt = await _normalization.GetTagsAsync("Note", note.Id);
            if (!string.IsNullOrWhiteSpace(nt)) note.Tags = nt;
            var cf = await _normalization.GetCustomFieldsAsync("Note", note.Id);
            if (!string.IsNullOrWhiteSpace(cf)) note.CustomFields = cf;
            
            results.Add(await MapToResponseDto(note, currentUserId));
        }

        return Ok(results);
    }

    /// <summary>
    /// Get note count for an entity
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId}/count")]
    public async Task<ActionResult<int>> GetNoteCountByEntity(string entityType, int entityId)
    {
        var normalizedType = entityType.ToLower();
        
        var query = _context.Notes.Where(n => !n.IsDeleted).AsQueryable();

        var count = normalizedType switch
        {
            "customer" => await query.CountAsync(n => n.CustomerId == entityId || (n.EntityType == "Customer" && n.EntityId == entityId)),
            "contact" => await query.CountAsync(n => n.ContactId == entityId || (n.EntityType == "Contact" && n.EntityId == entityId)),
            "opportunity" => await query.CountAsync(n => n.OpportunityId == entityId || (n.EntityType == "Opportunity" && n.EntityId == entityId)),
            "lead" => await query.CountAsync(n => n.LeadId == entityId || (n.EntityType == "Lead" && n.EntityId == entityId)),
            "campaign" => await query.CountAsync(n => n.CampaignId == entityId || (n.EntityType == "Campaign" && n.EntityId == entityId)),
            "quote" => await query.CountAsync(n => n.QuoteId == entityId || (n.EntityType == "Quote" && n.EntityId == entityId)),
            "servicerequest" => await query.CountAsync(n => n.ServiceRequestId == entityId || (n.EntityType == "ServiceRequest" && n.EntityId == entityId)),
            "product" => await query.CountAsync(n => n.ProductId == entityId || (n.EntityType == "Product" && n.EntityId == entityId)),
            _ => await query.CountAsync(n => n.EntityType == entityType && n.EntityId == entityId)
        };

        return Ok(count);
    }
    
    /// <summary>
    /// Quick add note from context (e.g., from chatbot flyout)
    /// </summary>
    [HttpPost("quick-add")]
    public async Task<ActionResult<NoteResponseDto>> QuickAddNote([FromBody] QuickNoteDto dto)
    {
        var currentUserId = GetCurrentUserId();
        
        var note = new Note
        {
            Title = dto.Title ?? $"Note - {DateTime.UtcNow:g}",
            Content = dto.Content,
            NoteType = NoteType.General,
            Visibility = NoteVisibility.Team,
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            ContextPath = dto.ContextPath,
            CreatedByUserId = currentUserId > 0 ? currentUserId : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Set legacy FK if applicable
        if (!string.IsNullOrEmpty(dto.EntityType) && dto.EntityId.HasValue)
        {
            switch (dto.EntityType.ToLower())
            {
                case "customer": note.CustomerId = dto.EntityId; break;
                case "contact": note.ContactId = dto.EntityId; break;
                case "opportunity": note.OpportunityId = dto.EntityId; break;
                case "lead": note.LeadId = dto.EntityId; break;
                case "campaign": note.CampaignId = dto.EntityId; break;
                case "quote": note.QuoteId = dto.EntityId; break;
                case "servicerequest": note.ServiceRequestId = dto.EntityId; break;
                case "product": note.ProductId = dto.EntityId; break;
            }
        }

        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        note = await _context.Notes
            .Include(n => n.CreatedByUser)
            .FirstOrDefaultAsync(n => n.Id == note.Id);

        _logger.LogInformation("Quick note {NoteId} created by user {UserId} for {EntityType} {EntityId}", 
            note!.Id, currentUserId, dto.EntityType, dto.EntityId);
        
        return CreatedAtAction(nameof(GetNote), new { id = note.Id }, await MapToResponseDto(note, currentUserId));
    }
}
