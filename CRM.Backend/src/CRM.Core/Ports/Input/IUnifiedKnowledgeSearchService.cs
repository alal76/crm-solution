// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Ports.Input;

/// <summary>
/// Indicates which knowledge source(s) a unified search should target.
/// KB-010: Unified Knowledge Search facade.
/// </summary>
public enum KnowledgeSource
{
    /// <summary>Search both General KB and ITSM KB.</summary>
    All = 0,

    /// <summary>Search General Knowledge Base articles only.</summary>
    General = 1,

    /// <summary>Search ITSM / Service Desk KB articles only.</summary>
    ITSM = 2
}

/// <summary>
/// A single result from the unified knowledge search, normalised across both
/// General KB (<see cref="KnowledgeSource.General"/>) and
/// ITSM KB (<see cref="KnowledgeSource.ITSM"/>).
/// </summary>
public class UnifiedKnowledgeSearchResultDto
{
    /// <summary>Article primary key in its source system.</summary>
    public int Id { get; set; }

    /// <summary>Article title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Short summary / description (may be empty for ITSM articles).</summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>Originating knowledge source.</summary>
    public KnowledgeSource Source { get; set; }

    /// <summary>URL-friendly slug (General KB articles only; empty for ITSM).</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Relevance score in [0.0, 1.0] — higher means more relevant.</summary>
    public double RelevanceScore { get; set; }

    /// <summary>Number of times the article has been viewed.</summary>
    public int ViewCount { get; set; }

    /// <summary>Category name, if assigned.</summary>
    public string? Category { get; set; }

    /// <summary>When the article was last modified.</summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Service that provides a unified full-text search facade over both the
/// General Knowledge Base and the ITSM Knowledge Base.
/// KB-010/KB-011.
/// </summary>
public interface IUnifiedKnowledgeSearchService
{
    /// <summary>
    /// Search for knowledge articles across the specified sources.
    /// Results are sorted by <see cref="UnifiedKnowledgeSearchResultDto.RelevanceScore"/> descending.
    /// </summary>
    /// <param name="query">Free-text search query.</param>
    /// <param name="maxResults">Maximum number of results to return (default 20).</param>
    /// <param name="source">Knowledge source(s) to search (default: All).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IEnumerable<UnifiedKnowledgeSearchResultDto>> SearchAsync(
        string query,
        int maxResults = 20,
        KnowledgeSource source = KnowledgeSource.All,
        CancellationToken ct = default);

    /// <summary>
    /// (Re)indexes all published articles so that downstream AI search services
    /// can consume both General and ITSM content.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task IndexAllAsync(CancellationToken ct = default);
}
