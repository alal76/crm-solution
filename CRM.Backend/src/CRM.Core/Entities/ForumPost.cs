// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

/// <summary>
/// A community forum post that can be public, internal, or partner-only.
/// </summary>
public class ForumPost : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    /// <summary>Markdown body (nullable – stored separately for large posts).</summary>
    public string? BodyMarkdown { get; set; }

    public int AuthorId { get; set; }

    /// <summary>Forum category: General, TechnicalSupport, ProductFeedback, Announcements.</summary>
    public string Category { get; set; } = "General";

    /// <summary>JSON array of tag strings.</summary>
    public string? TagsJson { get; set; }

    public bool IsApproved { get; set; }
    public bool IsPinned { get; set; }

    /// <summary>Visibility: Public, Internal, Partners.</summary>
    public string Visibility { get; set; } = "Public";

    public int ViewCount { get; set; }
    public int ReplyCount { get; set; }
}
