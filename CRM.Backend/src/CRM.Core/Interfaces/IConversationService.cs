// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing conversation threads
/// </summary>
public interface IConversationService
{
    /// <summary>
    /// Get all conversations with optional filtering
    /// </summary>
    Task<IEnumerable<Conversation>> GetAllAsync(
        int? accountId = null,
        int? contactId = null,
        ConversationStatus? status = null,
        int? assignedToUserId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a conversation by ID
    /// </summary>
    Task<Conversation?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a conversation by its unique conversation identifier
    /// </summary>
    Task<Conversation?> GetByConversationIdAsync(string conversationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new conversation
    /// </summary>
    Task<Conversation> CreateAsync(Conversation conversation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing conversation
    /// </summary>
    Task<bool> UpdateAsync(int id, Conversation conversation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a conversation (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update the status of a conversation
    /// </summary>
    Task<bool> UpdateStatusAsync(int id, ConversationStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assign a conversation to a user
    /// </summary>
    Task<bool> AssignAsync(int id, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get conversations for a specific entity (account, contact, or lead)
    /// </summary>
    Task<IEnumerable<Conversation>> GetByEntityAsync(string entityType, int entityId, CancellationToken cancellationToken = default);
}
