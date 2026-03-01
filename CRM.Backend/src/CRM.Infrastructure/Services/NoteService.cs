// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing notes attached to various entities
/// </summary>
public class NoteService : INoteService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<NoteService> _logger;

    public NoteService(ICrmDbContext context, ILogger<NoteService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Note>> GetNotesAsync(
        int? accountId = null,
        int? opportunityId = null,
        int? productId = null,
        NoteType? noteType = null,
        bool? pinned = null)
    {
        _logger.LogDebug(
            "Getting notes with filters: AccountId={AccountId}, OpportunityId={OpportunityId}, ProductId={ProductId}, NoteType={NoteType}, Pinned={Pinned}",
            accountId, opportunityId, productId, noteType, pinned);

        var query = _context.Notes.AsNoTracking().Where(n => !n.IsDeleted);

        if (accountId.HasValue)
        {
            query = query.Where(n => n.AccountId == accountId);
        }

        if (opportunityId.HasValue)
        {
            query = query.Where(n => n.OpportunityId == opportunityId);
        }

        if (productId.HasValue)
        {
            query = query.Where(n => n.ProductId == productId);
        }

        if (noteType.HasValue)
        {
            query = query.Where(n => n.NoteType == noteType);
        }

        if (pinned.HasValue)
        {
            query = query.Where(n => n.IsPinned == pinned);
        }

        var notes = await query
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.CreatedAt)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} notes", notes.Count);
        return notes;
    }

    /// <inheritdoc />
    public async Task<Note?> GetByIdAsync(int id)
    {
        _logger.LogDebug("Getting note by ID: {NoteId}", id);

        var note = await _context.Notes
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);

        if (note == null)
        {
            _logger.LogWarning("Note not found: {NoteId}", id);
        }

        return note;
    }

    /// <inheritdoc />
    public async Task<Note> CreateAsync(Note note)
    {
        ArgumentNullException.ThrowIfNull(note);

        _logger.LogDebug("Creating note: {Title}", note.Title);

        note.CreatedAt = DateTime.UtcNow;
        note.IsDeleted = false;

        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created note with ID: {NoteId}, Title: {Title}", note.Id, note.Title);
        return note;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(int id, Note note)
    {
        ArgumentNullException.ThrowIfNull(note);

        _logger.LogDebug("Updating note: {NoteId}", id);

        var existingNote = await _context.Notes
            .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);

        if (existingNote == null)
        {
            _logger.LogWarning("Note not found for update: {NoteId}", id);
            return false;
        }

        // Update properties
        existingNote.Title = note.Title;
        existingNote.Content = note.Content;
        existingNote.NoteType = note.NoteType;
        existingNote.Visibility = note.Visibility;
        existingNote.IsPinned = note.IsPinned;
        existingNote.IsImportant = note.IsImportant;

        // Update entity associations if provided
        if (note.AccountId.HasValue)
        {
            existingNote.AccountId = note.AccountId;
        }
        if (note.ContactId.HasValue)
        {
            existingNote.ContactId = note.ContactId;
        }
        if (note.OpportunityId.HasValue)
        {
            existingNote.OpportunityId = note.OpportunityId;
        }
        if (note.ProductId.HasValue)
        {
            existingNote.ProductId = note.ProductId;
        }
        if (note.LeadId.HasValue)
        {
            existingNote.LeadId = note.LeadId;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated note: {NoteId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogDebug("Deleting note: {NoteId}", id);

        var note = await _context.Notes
            .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);

        if (note == null)
        {
            _logger.LogWarning("Note not found for deletion: {NoteId}", id);
            return false;
        }

        // Soft delete
        note.IsDeleted = true;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted note: {NoteId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> TogglePinAsync(int id)
    {
        _logger.LogDebug("Toggling pin for note: {NoteId}", id);

        var note = await _context.Notes
            .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);

        if (note == null)
        {
            _logger.LogWarning("Note not found for pin toggle: {NoteId}", id);
            return false;
        }

        note.IsPinned = !note.IsPinned;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Toggled pin for note: {NoteId}, IsPinned: {IsPinned}", id, note.IsPinned);
        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Note>> GetByEntityAsync(string entityType, int entityId)
    {
        _logger.LogDebug("Getting notes for entity: {EntityType}, ID: {EntityId}", entityType, entityId);

        var query = _context.Notes.AsNoTracking().Where(n => !n.IsDeleted);

        // Filter by polymorphic entity relationship
        query = entityType.ToLowerInvariant() switch
        {
            "account" or "customer" => query.Where(n => n.AccountId == entityId || (n.EntityId == entityId && n.EntityType == entityType)),
            "contact" => query.Where(n => n.ContactId == entityId || (n.EntityId == entityId && n.EntityType == entityType)),
            "opportunity" => query.Where(n => n.OpportunityId == entityId || (n.EntityId == entityId && n.EntityType == entityType)),
            "product" => query.Where(n => n.ProductId == entityId || (n.EntityId == entityId && n.EntityType == entityType)),
            "lead" => query.Where(n => n.LeadId == entityId || (n.EntityId == entityId && n.EntityType == entityType)),
            "servicerequest" or "ticket" => query.Where(n => n.ServiceRequestId == entityId || (n.EntityId == entityId && n.EntityType == entityType)),
            "quote" => query.Where(n => n.QuoteId == entityId || (n.EntityId == entityId && n.EntityType == entityType)),
            _ => query.Where(n => n.EntityId == entityId && n.EntityType == entityType)
        };

        var notes = await query
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.CreatedAt)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} notes for {EntityType} ID {EntityId}", notes.Count, entityType, entityId);
        return notes;
    }
}
