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
/// Service for managing conversation threads
/// </summary>
public class ConversationService : IConversationService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<ConversationService> _logger;

    public ConversationService(ICrmDbContext context, ILogger<ConversationService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Conversation>> GetAllAsync(
        int? accountId = null,
        int? contactId = null,
        ConversationStatus? status = null,
        int? assignedToUserId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Getting conversations with filters: AccountId={AccountId}, ContactId={ContactId}, Status={Status}, AssignedToUserId={AssignedToUserId}",
            accountId, contactId, status, assignedToUserId);

        var query = _context.Conversations.AsNoTracking().Where(c => !c.IsDeleted);

        if (accountId.HasValue)
        {
            query = query.Where(c => c.AccountId == accountId.Value);
        }

        if (contactId.HasValue)
        {
            query = query.Where(c => c.ContactId == contactId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (assignedToUserId.HasValue)
        {
            query = query.Where(c => c.AssignedToUserId == assignedToUserId.Value);
        }

        var conversations = await query
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} conversations", conversations.Count);
        return conversations;
    }

    /// <inheritdoc />
    public async Task<Conversation?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting conversation by ID: {ConversationId}", id);

        var conversation = await _context.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        if (conversation == null)
        {
            _logger.LogWarning("Conversation not found: {ConversationId}", id);
        }

        return conversation;
    }

    /// <inheritdoc />
    public async Task<Conversation?> GetByConversationIdAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting conversation by ConversationId: {ConversationId}", conversationId);

        var conversation = await _context.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId && !c.IsDeleted, cancellationToken);

        if (conversation == null)
        {
            _logger.LogWarning("Conversation not found for ConversationId: {ConversationId}", conversationId);
        }

        return conversation;
    }

    /// <inheritdoc />
    public async Task<Conversation> CreateAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        _logger.LogDebug("Creating conversation: {Subject}", conversation.Subject);

        conversation.CreatedAt = DateTime.UtcNow;
        conversation.UpdatedAt = DateTime.UtcNow;
        conversation.IsDeleted = false;

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created conversation with ID: {Id}, ConversationId: {ConversationId}",
            conversation.Id, conversation.ConversationId);
        return conversation;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(int id, Conversation conversation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        _logger.LogDebug("Updating conversation: {ConversationId}", id);

        var existing = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        if (existing == null)
        {
            _logger.LogWarning("Conversation not found for update: {ConversationId}", id);
            return false;
        }

        existing.Subject = conversation.Subject;
        existing.LastMessagePreview = conversation.LastMessagePreview;
        existing.Status = conversation.Status;
        existing.Priority = conversation.Priority;
        existing.ParticipantAddress = conversation.ParticipantAddress;
        existing.ParticipantName = conversation.ParticipantName;
        existing.AccountId = conversation.AccountId;
        existing.ContactId = conversation.ContactId;
        existing.LeadId = conversation.LeadId;
        existing.AssignedToUserId = conversation.AssignedToUserId;
        existing.MessageCount = conversation.MessageCount;
        existing.UnreadCount = conversation.UnreadCount;
        existing.InboundCount = conversation.InboundCount;
        existing.OutboundCount = conversation.OutboundCount;
        existing.LastMessageAt = conversation.LastMessageAt;
        existing.LastInboundAt = conversation.LastInboundAt;
        existing.LastOutboundAt = conversation.LastOutboundAt;
        existing.TagsJson = conversation.TagsJson;
        existing.MetadataJson = conversation.MetadataJson;
        existing.IsStarred = conversation.IsStarred;
        existing.IsMuted = conversation.IsMuted;
        existing.IsPinned = conversation.IsPinned;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated conversation: {ConversationId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting conversation: {ConversationId}", id);

        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        if (conversation == null)
        {
            _logger.LogWarning("Conversation not found for deletion: {ConversationId}", id);
            return false;
        }

        // Soft delete
        conversation.IsDeleted = true;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted conversation: {ConversationId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateStatusAsync(int id, ConversationStatus status, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating status for conversation: {ConversationId} to {Status}", id, status);

        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        if (conversation == null)
        {
            _logger.LogWarning("Conversation not found for status update: {ConversationId}", id);
            return false;
        }

        conversation.Status = status;
        conversation.UpdatedAt = DateTime.UtcNow;

        if (status == ConversationStatus.Resolved || status == ConversationStatus.Closed)
        {
            conversation.ResolvedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated status for conversation: {ConversationId} to {Status}", id, status);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> AssignAsync(int id, int userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Assigning conversation {ConversationId} to user {UserId}", id, userId);

        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        if (conversation == null)
        {
            _logger.LogWarning("Conversation not found for assignment: {ConversationId}", id);
            return false;
        }

        conversation.AssignedToUserId = userId;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Assigned conversation {ConversationId} to user {UserId}", id, userId);
        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Conversation>> GetByEntityAsync(string entityType, int entityId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting conversations for entity: {EntityType}, ID: {EntityId}", entityType, entityId);

        var query = _context.Conversations.AsNoTracking().Where(c => !c.IsDeleted);

        query = entityType.ToLowerInvariant() switch
        {
            "account" or "customer" => query.Where(c => c.AccountId == entityId),
            "contact" => query.Where(c => c.ContactId == entityId),
            "lead" => query.Where(c => c.LeadId == entityId),
            _ => query.Where(c => false) // No match for unknown entity types
        };

        var conversations = await query
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} conversations for {EntityType} ID {EntityId}", conversations.Count, entityType, entityId);
        return conversations;
    }
}
