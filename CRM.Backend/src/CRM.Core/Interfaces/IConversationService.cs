// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
