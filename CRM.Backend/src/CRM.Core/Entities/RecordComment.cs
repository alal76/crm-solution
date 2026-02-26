// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Entities;

/// <summary>
/// Represents a threaded comment on any CRM entity (Account, Contact, Lead, Opportunity, ServiceRequest, etc.)
/// Supports @mention of users via MentionedUserIds JSON array.
/// </summary>
public class RecordComment : BaseEntity
{
    /// <summary>
    /// The type of entity this comment is attached to (e.g. "Account", "Contact", "Lead").
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>The ID of the entity this comment is attached to.</summary>
    public int EntityId { get; set; }

    /// <summary>Comment content, max 2000 chars.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>FK to the User who authored this comment.</summary>
    public int AuthorId { get; set; }

    /// <summary>FK to parent comment for threaded replies. Null = top-level comment.</summary>
    public int? ParentCommentId { get; set; }

    /// <summary>JSON array of user IDs who are @mentioned in this comment. Max 500 chars.</summary>
    public string? MentionedUserIds { get; set; }

    // ── Navigation properties ──────────────────────────────────────────────
    public virtual User? Author { get; set; }
    public virtual RecordComment? ParentComment { get; set; }
    public virtual ICollection<RecordComment> Replies { get; set; } = new List<RecordComment>();
}
