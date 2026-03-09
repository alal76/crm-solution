// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Infrastructure.Services.Knowledge;

// KB-015: Unified Meilisearch document for General + ITSM knowledge articles.

/// <summary>
/// Unified Meilisearch index document that represents an article from either the
/// General Knowledge Base or the ITSM Knowledge Base.
/// Both sources are stored in the same <c>knowledge_articles</c> index with a
/// numeric <see cref="Source"/> discriminator field so that queries can filter
/// by source without an extra index round-trip.
/// KB-015: Meilisearch unified knowledge index.
/// </summary>
public sealed class KnowledgeIndexDocument
{
    // KB-015: Composite id avoids PK collisions between General and ITSM articles.

    /// <summary>
    /// Composite document identifier.
    /// Format: <c>"general-{sourceId}"</c> for General KB or <c>"itsm-{sourceId}"</c> for ITSM KB.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Primary key in the originating source system (table PK).</summary>
    public int SourceId { get; set; }

    /// <summary>Article title — highest weight searchable field.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Full article body / content — used for full-text matching.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Short summary or short description — medium weight searchable field.</summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Source discriminator integer: <c>0</c> = General KB, <c>1</c> = ITSM KB.
    /// Stored as an integer so Meilisearch can filter efficiently.
    /// </summary>
    public int Source { get; set; }

    /// <summary>Category name captured at index time (may lag real-time edits).</summary>
    public string? Category { get; set; }

    /// <summary>Comma-separated tag values.</summary>
    public string? Tags { get; set; }

    /// <summary>View count — used for popularity-based ranking.</summary>
    public int ViewCount { get; set; }

    /// <summary>URL-friendly slug. Non-empty for General KB articles only; empty string for ITSM.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Last-modified UTC timestamp — exposed for sorting.</summary>
    public DateTime UpdatedAt { get; set; }
}
