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
/// Service interface for managing notes
/// </summary>
public interface INoteService
{
    /// <summary>
    /// Get notes with optional filtering
    /// </summary>
    Task<IEnumerable<Note>> GetNotesAsync(
        int? accountId = null,
        int? opportunityId = null,
        int? productId = null,
        NoteType? noteType = null,
        bool? pinned = null);

    /// <summary>
    /// Get a note by ID
    /// </summary>
    Task<Note?> GetByIdAsync(int id);

    /// <summary>
    /// Create a new note
    /// </summary>
    Task<Note> CreateAsync(Note note);

    /// <summary>
    /// Update an existing note
    /// </summary>
    Task<bool> UpdateAsync(int id, Note note);

    /// <summary>
    /// Delete a note
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Toggle pin status of a note
    /// </summary>
    Task<bool> TogglePinAsync(int id);

    /// <summary>
    /// Get notes for a specific entity
    /// </summary>
    Task<IEnumerable<Note>> GetByEntityAsync(string entityType, int entityId);
}
