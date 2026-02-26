// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>Service for managing threaded record comments on CRM entities.</summary>
public interface IRecordCommentService
{
    /// <summary>Get all top-level comments (with replies) for a given entity.</summary>
    Task<IEnumerable<RecordCommentDto>> GetByEntityAsync(string entityType, int entityId, CancellationToken ct = default);

    /// <summary>Get a single comment by ID.</summary>
    Task<RecordCommentDto?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Create a new comment.</summary>
    Task<RecordCommentDto> CreateAsync(CreateRecordCommentDto dto, int authorId, CancellationToken ct = default);

    /// <summary>Update the content of a comment (only the owner can edit).</summary>
    Task<RecordCommentDto?> UpdateAsync(int id, UpdateRecordCommentDto dto, int userId, CancellationToken ct = default);

    /// <summary>Soft-delete a comment (owner or admin).</summary>
    Task<bool> DeleteAsync(int id, int userId, CancellationToken ct = default);

    /// <summary>Get all replies under a parent comment.</summary>
    Task<IEnumerable<RecordCommentDto>> GetThreadAsync(int parentCommentId, CancellationToken ct = default);
}
