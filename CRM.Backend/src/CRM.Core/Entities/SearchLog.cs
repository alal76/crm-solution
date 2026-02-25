// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

/// <summary>
/// Persisted record of a search query for analytics and tuning.
/// </summary>
public class SearchLog : BaseEntity
{
    public string Query { get; set; } = string.Empty;
    public int ResultCount { get; set; }

    /// <summary>Null for anonymous / portal searches.</summary>
    public int? UserId { get; set; }

    public long DurationMs { get; set; }

    /// <summary>Search provider used: BuiltIn, Meilisearch, Algolia, etc.</summary>
    public string? Provider { get; set; }

    /// <summary>Entity types queried (comma-separated).</summary>
    public string? EntityTypes { get; set; }

    public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
}
