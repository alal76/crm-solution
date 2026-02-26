// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos;

/// <summary>Response DTO for a record comment (with nested replies).</summary>
public class RecordCommentDto
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAvatarUrl { get; set; }
    public int? ParentCommentId { get; set; }
    public string? MentionedUserIds { get; set; }
    public List<RecordCommentDto> Replies { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

/// <summary>DTO for creating a new comment.</summary>
public class CreateRecordCommentDto
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int? ParentCommentId { get; set; }
    public string? MentionedUserIds { get; set; }
}

/// <summary>DTO for updating an existing comment's content.</summary>
public class UpdateRecordCommentDto
{
    public string Content { get; set; } = string.Empty;
    public string? MentionedUserIds { get; set; }
}
