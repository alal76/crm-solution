// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.Api.Infrastructure;

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
    public int? AccountId { get; set; }
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
/// API endpoints for managing notes with RBAC.
/// </summary>
/// <remarks>
/// Notes can be attached to any entity type (Account, Contact, Lead, Opportunity, Campaign, Quote, ServiceRequest, Product, etc.).
/// Edit/Delete permissions: Creator or users with NotesAdmin/SystemAdmin roles.
/// Supports both polymorphic (EntityType/EntityId) and legacy FK attachment methods.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class NotesController : CrmControllerBase
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
        if (userId == 0)
            return false;

        var user = await _context.Users
            .Include(u => u.PrimaryGroup)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return false;

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

    /// <summary>
    /// Sets the legacy foreign key field on a note based on entity type.
    /// Reduces cognitive complexity by consolidating switch logic.
    /// </summary>
    private static void SetLegacyEntityForeignKey(Note note, string? entityType, int? entityId)
    {
        if (string.IsNullOrEmpty(entityType) || !entityId.HasValue)
            return;

        switch (entityType.ToLower())
        {
            case "account": note.AccountId = entityId; break;
            case "contact": note.ContactId = entityId; break;
            case "opportunity": note.OpportunityId = entityId; break;
            case "lead": note.LeadId = entityId; break;
            case "campaign": note.CampaignId = entityId; break;
            case "quote": note.QuoteId = entityId; break;
            case "servicerequest": note.ServiceRequestId = entityId; break;
            case "product": note.ProductId = entityId; break;
        }
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
    /// Get all notes with optional filtering.
    /// </summary>
    /// <param name="accountId">Filter by account ID.</param>
    /// <param name="contactId">Filter by contact ID.</param>
    /// <param name="opportunityId">Filter by opportunity ID.</param>
    /// <param name="leadId">Filter by lead ID.</param>
    /// <param name="campaignId">Filter by campaign ID.</param>
    /// <param name="quoteId">Filter by quote ID.</param>
    /// <param name="serviceRequestId">Filter by service request ID.</param>
    /// <param name="productId">Filter by product ID.</param>
    /// <param name="entityType">Filter by entity type (polymorphic).</param>
    /// <param name="entityId">Filter by entity ID (polymorphic).</param>
    /// <param name="noteType">Filter by note type.</param>
    /// <param name="pinned">Filter to only pinned notes.</param>
    /// <returns>List of notes matching the filter criteria.</returns>
    /// <response code="200">Returns the list of notes.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NoteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<NoteResponseDto>>> GetNotes(
        [FromQuery] int? accountId = null,
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
                (entityType.ToLower() == "account" && n.AccountId == entityId) ||
                (entityType.ToLower() == "contact" && n.ContactId == entityId) ||
                (entityType.ToLower() == "opportunity" && n.OpportunityId == entityId) ||
                (entityType.ToLower() == "lead" && n.LeadId == entityId) ||
                (entityType.ToLower() == "campaign" && n.CampaignId == entityId) ||
                (entityType.ToLower() == "quote" && n.QuoteId == entityId) ||
                (entityType.ToLower() == "servicerequest" && n.ServiceRequestId == entityId) ||
                (entityType.ToLower() == "product" && n.ProductId == entityId));
        }
        else
        {
            // Legacy filters
            if (accountId.HasValue)
                query = query.Where(n => n.AccountId == accountId || (n.EntityType == "Account" && n.EntityId == accountId));
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
            if (!string.IsNullOrWhiteSpace(nt))
                note.Tags = nt;
            var cf = await _normalization.GetCustomFieldsAsync("Note", note.Id);
            if (!string.IsNullOrWhiteSpace(cf))
                note.CustomFields = cf;

            results.Add(await MapToResponseDto(note, currentUserId));
        }

        return Ok(results);
    }

    /// <summary>
    /// Get a note by ID.
    /// </summary>
    /// <param name="id">The note ID.</param>
    /// <returns>The note with the specified ID.</returns>
    /// <response code="200">Returns the note.</response>
    /// <response code="404">Note not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(NoteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        if (!string.IsNullOrWhiteSpace(nt))
            note.Tags = nt;
        var cf = await _normalization.GetCustomFieldsAsync("Note", note.Id);
        if (!string.IsNullOrWhiteSpace(cf))
            note.CustomFields = cf;

        return Ok(await MapToResponseDto(note, currentUserId));
    }

    /// <summary>
    /// Create a new note.
    /// </summary>
    /// <param name="dto">The note data to create.</param>
    /// <returns>The created note with response metadata.</returns>
    /// <response code="201">Returns the newly created note.</response>
    /// <response code="400">Invalid note data provided.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(NoteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            AccountId = dto.AccountId,
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
        SetLegacyEntityForeignKey(note, dto.EntityType, dto.EntityId);

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
    /// Update an existing note.
    /// </summary>
    /// <remarks>
    /// RBAC: Only the creator or users with Admin role can update notes.
    /// </remarks>
    /// <param name="id">The note ID to update.</param>
    /// <param name="dto">The updated note data.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Note was successfully updated.</response>
    /// <response code="400">Invalid data or ID mismatch.</response>
    /// <response code="403">Forbidden - User is not the creator or admin.</response>
    /// <response code="404">Note not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Delete a note (soft delete).
    /// </summary>
    /// <remarks>
    /// RBAC: Only the creator or users with Admin role can delete notes.
    /// </remarks>
    /// <param name="id">The note ID to delete.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Note was successfully deleted.</response>
    /// <response code="403">Forbidden - User is not the creator or admin.</response>
    /// <response code="404">Note not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Toggle the pinned status of a note.
    /// </summary>
    /// <param name="id">The note ID to toggle.</param>
    /// <returns>The updated note with new pinned status.</returns>
    /// <response code="200">Returns the note with updated pinned status.</response>
    /// <response code="404">Note not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{id}/toggle-pin")]
    [ProducesResponseType(typeof(NoteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Toggle the important flag of a note.
    /// </summary>
    /// <param name="id">The note ID to toggle.</param>
    /// <returns>The updated note with new important status.</returns>
    /// <response code="200">Returns the note with updated important status.</response>
    /// <response code="404">Note not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{id}/toggle-important")]
    [ProducesResponseType(typeof(NoteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Get notes for a specific entity.
    /// </summary>
    /// <remarks>
    /// Supports all entity types via polymorphic lookup (Account, Contact, Opportunity, Lead, Campaign, Quote, ServiceRequest, Product, Task, Interaction).
    /// </remarks>
    /// <param name="entityType">The type of entity (e.g., "account", "contact", "opportunity").</param>
    /// <param name="entityId">The entity ID.</param>
    /// <returns>List of notes attached to the specified entity.</returns>
    /// <response code="200">Returns the list of notes for the entity.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("entity/{entityType}/{entityId}")]
    [ProducesResponseType(typeof(IEnumerable<NoteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            "account" => query.Where(n => n.AccountId == entityId || (n.EntityType == "Account" && n.EntityId == entityId)),
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
            if (!string.IsNullOrWhiteSpace(nt))
                note.Tags = nt;
            var cf = await _normalization.GetCustomFieldsAsync("Note", note.Id);
            if (!string.IsNullOrWhiteSpace(cf))
                note.CustomFields = cf;

            results.Add(await MapToResponseDto(note, currentUserId));
        }

        return Ok(results);
    }

    /// <summary>
    /// Get the note count for an entity.
    /// </summary>
    /// <param name="entityType">The type of entity (e.g., "account", "contact", "opportunity").</param>
    /// <param name="entityId">The entity ID.</param>
    /// <returns>The count of notes attached to the specified entity.</returns>
    /// <response code="200">Returns the note count.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("entity/{entityType}/{entityId}/count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> GetNoteCountByEntity(string entityType, int entityId)
    {
        var normalizedType = entityType.ToLower();

        var query = _context.Notes.Where(n => !n.IsDeleted).AsQueryable();

        var count = normalizedType switch
        {
            "account" => await query.CountAsync(n => n.AccountId == entityId || (n.EntityType == "Account" && n.EntityId == entityId)),
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
    /// Quick add a note from context.
    /// </summary>
    /// <remarks>
    /// Simplified note creation for use from chatbot flyout or quick-entry interfaces.
    /// Creates a note with default NoteType.General and NoteVisibility.Team.
    /// </remarks>
    /// <param name="dto">The quick note data to create.</param>
    /// <returns>The created note with response metadata.</returns>
    /// <response code="201">Returns the newly created note.</response>
    /// <response code="400">Invalid note data provided.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("quick-add")]
    [ProducesResponseType(typeof(NoteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        SetLegacyEntityForeignKey(note, dto.EntityType, dto.EntityId);

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
