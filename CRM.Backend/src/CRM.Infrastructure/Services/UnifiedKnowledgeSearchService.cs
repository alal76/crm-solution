// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Services.Knowledge; // KB-015: KnowledgeIndexDocument
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GeneralKbArticleStatus = CRM.Core.Entities.KnowledgeBase.ArticleStatus;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Provides unified full-text search across the General Knowledge Base and
/// the ITSM Knowledge Base.  Results are normalised to
/// <see cref="UnifiedKnowledgeSearchResultDto"/> and sorted by a simple
/// term-frequency relevance score.
/// When an external search provider (e.g. Meilisearch) is injected and active,
/// <see cref="SearchAsync"/> delegates to Meilisearch for typo-tolerant search
/// and <see cref="IndexAllAsync"/> pushes unified <see cref="KnowledgeIndexDocument"/>
/// records to the provider index.
/// KB-011 / KB-015.
/// </summary>
public class UnifiedKnowledgeSearchService : IUnifiedKnowledgeSearchService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<UnifiedKnowledgeSearchService> _logger;

    // KB-015: optional external search port; null → EF Core fallback path.
    private readonly ISearchPort? _searchPort;

    /// <summary>
    /// Source discriminator value for General KB articles in the Meilisearch index.
    /// </summary>
    private const int MsSourceGeneral = 0; // KB-015

    /// <summary>
    /// Source discriminator value for ITSM KB articles in the Meilisearch index.
    /// </summary>
    private const int MsSourceITSM = 1; // KB-015

    /// <summary>
    /// Initialises a new instance of <see cref="UnifiedKnowledgeSearchService"/>.
    /// </summary>
    /// <param name="context">EF Core DB context.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="searchPort">
    /// Optional pluggable search port (Meilisearch, Algolia, …).
    /// When <c>null</c> or when the provider reports "BuiltIn",
    /// searches fall back to direct EF Core queries.
    /// KB-015.
    /// </param>
    public UnifiedKnowledgeSearchService(
        ICrmDbContext context,
        ILogger<UnifiedKnowledgeSearchService> logger,
        ISearchPort? searchPort = null) // KB-015: optional external search port
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _searchPort = searchPort; // KB-015: null is valid; triggers EF Core fallback
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<UnifiedKnowledgeSearchResultDto>> SearchAsync(
        string query,
        int maxResults = 20,
        KnowledgeSource source = KnowledgeSource.All,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Enumerable.Empty<UnifiedKnowledgeSearchResultDto>();
        }

        _logger.LogDebug(
            "UnifiedKnowledgeSearch: query={Query} source={Source} max={Max}",
            query, source, maxResults);

        // KB-015: delegate to Meilisearch when an external search provider is active.
        if (_searchPort != null && _searchPort.ProviderName != "BuiltIn")
        {
            return await SearchViaMeilisearchAsync(query, maxResults, source, ct);
        }

        var generalTask = source is KnowledgeSource.All or KnowledgeSource.General
            ? SearchGeneralKbAsync(query, maxResults, ct)
            : Task.FromResult(Enumerable.Empty<UnifiedKnowledgeSearchResultDto>());

        var itsmTask = source is KnowledgeSource.All or KnowledgeSource.ITSM
            ? SearchItsmKbAsync(query, maxResults, ct)
            : Task.FromResult(Enumerable.Empty<UnifiedKnowledgeSearchResultDto>());

        await Task.WhenAll(generalTask, itsmTask);

        var merged = generalTask.Result
            .Concat(itsmTask.Result)
            .OrderByDescending(r => r.RelevanceScore)
            .ThenByDescending(r => r.ViewCount)
            .Take(maxResults)
            .ToList();

        _logger.LogDebug(
            "UnifiedKnowledgeSearch returned {Count} results for query={Query}",
            merged.Count, query);

        return merged;
    }

    /// <inheritdoc/>
    public async Task IndexAllAsync(CancellationToken ct = default)
    {
        // This implementation is intentionally lightweight — it validates that
        // both KB sources are reachable.  Semantic indexing is delegated to
        // AIKnowledgeSearchService which can call this service for content.
        var generalCount = await _context.KnowledgeArticles
            .Where(a => !a.IsDeleted && a.Status == GeneralKbArticleStatus.Published)
            .CountAsync(ct);

        var itsmCount = await _context.ITSMKnowledgeArticles
            .Where(a => !a.IsDeleted && a.PublishingState == PublishingState.Published)
            .CountAsync(ct);

        _logger.LogInformation(
            "IndexAllAsync — General KB: {GeneralCount} published articles, ITSM KB: {ITSMCount} published articles",
            generalCount, itsmCount);

        // KB-015: push documents to Meilisearch when an external search provider is active.
        if (_searchPort != null && _searchPort.ProviderName != "BuiltIn")
        {
            await IndexAllToMeilisearchAsync(ct);
        }
    }

    // =========================================================================
    // KB-015: Meilisearch integration helpers
    // =========================================================================

    /// <summary>
    /// Retrieves published articles from both KB sources, converts them to
    /// <see cref="KnowledgeIndexDocument"/> records, and pushes them to the
    /// Meilisearch <c>knowledge_articles</c> index via <see cref="_searchPort"/>.
    /// </summary>
    private async Task IndexAllToMeilisearchAsync(CancellationToken ct)
    {
        _logger.LogInformation("KB-015: Starting Meilisearch unified knowledge re-index.");

        // Load General KB articles
        var generalArticles = await _context.KnowledgeArticles
            .Include(a => a.Category)
            .Where(a => !a.IsDeleted && a.Status == GeneralKbArticleStatus.Published)
            .ToListAsync(ct);

        // Load ITSM KB articles
        var itsmArticles = await _context.ITSMKnowledgeArticles
            .Include(a => a.Category)
            .Where(a => !a.IsDeleted && a.PublishingState == PublishingState.Published)
            .ToListAsync(ct);

        // Build unified documents
        var generalDocs = generalArticles.Select(a => new KnowledgeIndexDocument
        {
            Id = $"general-{a.Id}", // KB-015: composite id prevents PK collision
            SourceId = a.Id,
            Title = a.Title,
            Content = a.Content,
            Summary = a.Summary ?? string.Empty,
            Source = MsSourceGeneral,
            Category = a.Category?.Name,
            Tags = a.TagsJson,
            ViewCount = a.ViewCount,
            Slug = a.Slug,
            UpdatedAt = a.UpdatedAt ?? a.CreatedAt
        });

        var itsmDocs = itsmArticles.Select(a => new KnowledgeIndexDocument
        {
            Id = $"itsm-{a.ArticleId}", // KB-015: composite id prevents PK collision
            SourceId = a.ArticleId,
            Title = a.Title,
            Content = a.ArticleBody,
            Summary = a.ShortDescription ?? string.Empty,
            Source = MsSourceITSM,
            Category = a.Category?.Name,
            Tags = a.Tags,
            ViewCount = a.ViewCount,
            Slug = string.Empty,
            UpdatedAt = a.ModifiedAt ?? a.CreatedAt
        });

        var allDocs = generalDocs.Concat(itsmDocs).ToList();

        await _searchPort!.IndexBatchAsync<KnowledgeIndexDocument>(
            allDocs,
            doc => doc.Id,
            ct);

        _logger.LogInformation(
            "KB-015: Meilisearch knowledge re-index complete — {GeneralCount} General + {ITSMCount} ITSM = {Total} total documents.",
            generalArticles.Count, itsmArticles.Count, allDocs.Count);
    }

    /// <summary>
    /// Executes a knowledge search via Meilisearch and maps the typed results back to
    /// <see cref="UnifiedKnowledgeSearchResultDto"/>.
    /// </summary>
    private async Task<IEnumerable<UnifiedKnowledgeSearchResultDto>> SearchViaMeilisearchAsync(
        string query,
        int maxResults,
        KnowledgeSource source,
        CancellationToken ct)
    {
        // KB-015: build typed search options; optionally filter by source discriminator.
        var searchOptions = new SearchOptions { Take = maxResults };

        if (source == KnowledgeSource.General)
        {
            // KB-015: filter to General KB only (source = 0)
            searchOptions.Filters = new Dictionary<string, string>
            {
                ["source"] = MsSourceGeneral.ToString()
            };
        }
        else if (source == KnowledgeSource.ITSM)
        {
            // KB-015: filter to ITSM KB only (source = 1)
            searchOptions.Filters = new Dictionary<string, string>
            {
                ["source"] = MsSourceITSM.ToString()
            };
        }

        var searchResult = await _searchPort!.SearchAsync<KnowledgeIndexDocument>(
            query,
            searchOptions,
            ct);

        _logger.LogDebug(
            "KB-015: Meilisearch returned {Count} knowledge hits for query={Query}",
            searchResult.TotalCount, query);

        return searchResult.Items.Select(doc => new UnifiedKnowledgeSearchResultDto
        {
            Id = doc.SourceId,
            Title = doc.Title,
            Summary = doc.Summary,
            Source = doc.Source == MsSourceGeneral ? KnowledgeSource.General : KnowledgeSource.ITSM,
            Slug = doc.Slug,
            // KB-015: Meilisearch handles full relevance; assign maximum score so
            // results are treated as equally relevant (order comes from Meilisearch).
            RelevanceScore = 1.0,
            ViewCount = doc.ViewCount,
            Category = doc.Category,
            UpdatedAt = doc.UpdatedAt
        });
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private async Task<IEnumerable<UnifiedKnowledgeSearchResultDto>> SearchGeneralKbAsync(
        string query, int take, CancellationToken ct)
    {
        var articles = await _context.KnowledgeArticles
            .Include(a => a.Category)
            .Where(a =>
                !a.IsDeleted &&
                a.Status == GeneralKbArticleStatus.Published &&
                (a.Title.Contains(query) ||
                 (a.Summary != null && a.Summary.Contains(query)) ||
                 a.Content.Contains(query)))
            .OrderByDescending(a => a.ViewCount)
            .Take(take)
            .ToListAsync(ct);

        return articles.Select(a => new UnifiedKnowledgeSearchResultDto
        {
            Id = a.Id,
            Title = a.Title,
            Summary = a.Summary ?? string.Empty,
            Source = KnowledgeSource.General,
            Slug = a.Slug,
            RelevanceScore = ComputeScore(query, a.Title, a.Summary),
            ViewCount = a.ViewCount,
            Category = a.Category?.Name,
            UpdatedAt = a.UpdatedAt ?? a.CreatedAt
        });
    }

    private async Task<IEnumerable<UnifiedKnowledgeSearchResultDto>> SearchItsmKbAsync(
        string query, int take, CancellationToken ct)
    {
        var articles = await _context.ITSMKnowledgeArticles
            .Include(a => a.Category)
            .Where(a =>
                !a.IsDeleted &&
                a.PublishingState == PublishingState.Published &&
                (a.Title.Contains(query) ||
                 (a.ShortDescription != null && a.ShortDescription.Contains(query)) ||
                 a.ArticleBody.Contains(query)))
            .OrderByDescending(a => a.ViewCount)
            .Take(take)
            .ToListAsync(ct);

        return articles.Select(a => new UnifiedKnowledgeSearchResultDto
        {
            Id = a.ArticleId,
            Title = a.Title,
            Summary = a.ShortDescription ?? string.Empty,
            Source = KnowledgeSource.ITSM,
            Slug = string.Empty, // ITSM articles have no slug
            RelevanceScore = ComputeScore(query, a.Title, a.ShortDescription),
            ViewCount = a.ViewCount,
            Category = a.Category?.Name,
            UpdatedAt = a.ModifiedAt ?? a.CreatedAt
        });
    }

    /// <summary>
    /// Computes a simple [0.0, 1.0] relevance score based on whether the
    /// query appears in the title (higher weight) or description (lower weight).
    /// </summary>
    private static double ComputeScore(string query, string title, string? description)
    {
        var q = query.ToLowerInvariant();
        double score = 0.0;

        if (title.Contains(q, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.7;
        }

        if (description != null && description.Contains(q, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.3;
        }

        return Math.Min(score, 1.0);
    }
}
