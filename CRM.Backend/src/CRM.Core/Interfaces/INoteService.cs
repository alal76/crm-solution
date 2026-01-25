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
        int? customerId = null,
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
