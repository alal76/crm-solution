// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Concurrent;
using CRM.Core.Entities.ITSM;
using CRM.Core.Features;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
// KB-014: alias to disambiguate General KB ArticleStatus (Published = 2) from ITSM PublishingState
using GeneralKbArticleStatus = CRM.Core.Entities.KnowledgeBase.ArticleStatus;

namespace CRM.Infrastructure.Services.AI;

#region Interfaces and DTOs

/// <summary>
/// Service for AI-powered semantic search over knowledge base articles.
/// Uses embeddings from IAIPort for cosine similarity matching.
/// Falls back to keyword search when AI is unavailable.
/// </summary>
public interface IAIKnowledgeSearchService
{
    /// <summary>
    /// Performs semantic search over knowledge base articles using embeddings.
    /// Falls back to keyword search if AI is unavailable.
    /// </summary>
    /// <param name="query">Search query text.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Ranked list of matching articles with relevance scores.</returns>
    Task<IEnumerable<SemanticSearchResult>> SemanticSearchAsync(string query, int maxResults = 5, CancellationToken ct = default);

    /// <summary>
    /// Indexes a single article by generating and caching its embedding.
    /// </summary>
    /// <param name="articleId">ID of the article to index.</param>
    /// <param name="ct">Cancellation token.</param>
    Task IndexArticleAsync(int articleId, CancellationToken ct = default);

    /// <summary>
    /// Reindexes all published articles by generating embeddings for each.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task ReindexAllAsync(CancellationToken ct = default);
}

/// <summary>
/// Result of a semantic search query against the knowledge base.
/// </summary>
public class SemanticSearchResult
{
    /// <summary>Knowledge article ID.</summary>
    public int ArticleId { get; set; }

    /// <summary>Article title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Relevant snippet from the article body.</summary>
    public string Snippet { get; set; } = string.Empty;

    /// <summary>Relevance score (0.0 to 1.0, higher = more relevant).</summary>
    public double RelevanceScore { get; set; }
}

#endregion

/// <summary>
/// AI-powered knowledge base search service.
/// Uses embeddings for semantic similarity when AI is available,
/// falls back to keyword-based search otherwise.
/// </summary>
public class AIKnowledgeSearchService : IAIKnowledgeSearchService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICrmDbContext _dbContext;
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<AIKnowledgeSearchService> _logger;

    // In-memory cache of article embeddings: ArticleId → embedding vector
    private readonly ConcurrentDictionary<int, float[]> _embeddingCache = new();

    // Cache of article metadata for search results
    private readonly ConcurrentDictionary<int, ArticleMetadata> _metadataCache = new();

    /// <summary>
    /// Initializes a new instance of AIKnowledgeSearchService.
    /// </summary>
    public AIKnowledgeSearchService(
        IServiceProvider serviceProvider,
        ICrmDbContext dbContext,
        IFeatureManager featureManager,
        ILogger<AIKnowledgeSearchService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SemanticSearchResult>> SemanticSearchAsync(string query, int maxResults = 5, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Enumerable.Empty<SemanticSearchResult>();
        }

        // Check if AI is enabled and available
        var aiPort = await GetAIPortAsync(ct);
        if (aiPort != null && _embeddingCache.Count > 0)
        {
            try
            {
                return await SemanticSearchWithEmbeddingsAsync(aiPort, query, maxResults, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Semantic search failed, falling back to keyword search");
            }
        }

        // Fallback to keyword-based search
        return await KeywordSearchAsync(query, maxResults, ct);
    }

    /// <inheritdoc />
    public async Task IndexArticleAsync(int articleId, CancellationToken ct = default)
    {
        var aiPort = await GetAIPortAsync(ct);
        if (aiPort == null)
        {
            _logger.LogDebug("AI port unavailable, skipping article indexing for {ArticleId}", articleId);
            return;
        }

        var context = _dbContext;
        var itsmArticle = await context.ITSMKnowledgeArticles
            .FirstOrDefaultAsync(a => a.ArticleId == articleId && !a.IsDeleted, ct);

        if (itsmArticle != null)
        {
            try
            {
                var text = BuildArticleText(itsmArticle);
                var embeddingResponse = await aiPort.GetEmbeddingAsync(text, cancellationToken: ct);

                if (embeddingResponse.Embedding.Length > 0)
                {
                    _embeddingCache[articleId] = embeddingResponse.Embedding;
                    _metadataCache[articleId] = new ArticleMetadata
                    {
                        Title = itsmArticle.Title,
                        Snippet = GetSnippet(itsmArticle.ArticleBody),
                        PublishingState = itsmArticle.PublishingState
                    };
                    _logger.LogInformation("Indexed ITSM article {ArticleId} with {Dimensions}-dimensional embedding", articleId, embeddingResponse.Embedding.Length);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to index ITSM article {ArticleId}", articleId);
            }
            return;
        }

        // KB-014: also index General KB articles when ITSM article not found
        var generalArticle = await context.KnowledgeArticles
            .FirstOrDefaultAsync(a => a.Id == articleId && !a.IsDeleted, ct);

        if (generalArticle == null)
        {
            _logger.LogWarning("Article {ArticleId} not found in either ITSM or General KB", articleId);
            return;
        }

        try
        {
            var generalText = BuildGeneralArticleText(generalArticle);
            var generalEmbeddingResponse = await aiPort.GetEmbeddingAsync(generalText, cancellationToken: ct);

            if (generalEmbeddingResponse.Embedding.Length > 0)
            {
                var cacheId = articleId + 100_000; // KB-014: offset to avoid ID collision with ITSM articles in the cache
                _embeddingCache[cacheId] = generalEmbeddingResponse.Embedding;
                _metadataCache[cacheId] = new ArticleMetadata
                {
                    Title = generalArticle.Title,
                    Snippet = GetSnippet(generalArticle.Content),
                    PublishingState = PublishingState.Published
                };
                _logger.LogInformation("Indexed General KB article {ArticleId} (cacheId={CacheId}) with {Dimensions}-dimensional embedding", articleId, cacheId, generalEmbeddingResponse.Embedding.Length);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index General KB article {ArticleId}", articleId);
        }
    }

    /// <inheritdoc />
    public async Task ReindexAllAsync(CancellationToken ct = default)
    {
        var aiPort = await GetAIPortAsync(ct);
        if (aiPort == null)
        {
            _logger.LogWarning("AI port unavailable, cannot reindex articles");
            return;
        }

        // KB-014: index both ITSM KB and General KB articles
        var itsmArticles = await _dbContext.ITSMKnowledgeArticles
            .Where(a => !a.IsDeleted && a.PublishingState == PublishingState.Published)
            .ToListAsync(ct);

        var generalArticles = await _dbContext.KnowledgeArticles
            .Where(a => !a.IsDeleted && a.Status == GeneralKbArticleStatus.Published)
            .ToListAsync(ct);

        var totalCount = itsmArticles.Count + generalArticles.Count;
        if (totalCount == 0)
        {
            _logger.LogInformation("No published articles to index");
            return;
        }

        _logger.LogInformation(
            "Reindexing {TotalCount} published articles (ITSM: {ITSMCount}, General: {GeneralCount})",
            totalCount, itsmArticles.Count, generalArticles.Count);

        // ---- Index ITSM articles ----
        await IndexArticleListAsync(
            aiPort,
            itsmArticles.Select(a => (a.ArticleId, BuildArticleText(a), a.Title, GetSnippet(a.ArticleBody), (object)a.PublishingState)),
            ct);

        // ---- Index General KB articles (KB-014) ----
        await IndexArticleListAsync(
            aiPort,
            generalArticles.Select(a => (
                a.Id + 100_000, // offset to avoid ID collision with ITSM articles in the cache
                BuildGeneralArticleText(a),
                a.Title,
                GetSnippet(a.Content),
                (object)a.Status)),
            ct);

        _logger.LogInformation("Successfully reindexed {Count} articles", _embeddingCache.Count);
    }

    /// <summary>Indexes a list of articles represented as (id, text, title, snippet, state) tuples.</summary>
    private async Task IndexArticleListAsync(
        IAIPort aiPort,
        IEnumerable<(int Id, string Text, string Title, string Snippet, object State)> articles,
        CancellationToken ct)
    {
        var articleList = articles.ToList();
        if (articleList.Count == 0)
        {
            return;
        }

        var texts = articleList.Select(a => a.Text).ToList();

        try
        {
            var batchResponse = await aiPort.GetEmbeddingsAsync(texts, cancellationToken: ct);

            for (var i = 0; i < articleList.Count && i < batchResponse.Embeddings.Count; i++)
            {
                var (id, _, title, snippet, _) = articleList[i];
                var embedding = batchResponse.Embeddings[i];

                if (embedding.Length > 0)
                {
                    _embeddingCache[id] = embedding;
                    _metadataCache[id] = new ArticleMetadata
                    {
                        Title = title,
                        Snippet = snippet,
                        // PublishingState is ITSM-specific; use a sentinel for General KB
                        PublishingState = PublishingState.Published
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Batch reindexing failed, attempting individual indexing");

            foreach (var (id, text, title, snippet, _) in articleList)
            {
                try
                {
                    var response = await aiPort.GetEmbeddingAsync(text, cancellationToken: ct);
                    if (response.Embedding.Length > 0)
                    {
                        _embeddingCache[id] = response.Embedding;
                        _metadataCache[id] = new ArticleMetadata
                        {
                            Title = title,
                            Snippet = snippet,
                            PublishingState = PublishingState.Published
                        };
                    }
                }
                catch (Exception innerEx)
                {
                    _logger.LogWarning(innerEx, "Failed to index article {ArticleId}", id);
                }
            }
        }
    }

    #region Private Methods

    private async Task<IAIPort?> GetAIPortAsync(CancellationToken ct)
    {
        try
        {
            var isAIEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalAI);
            if (!isAIEnabled)
            {
                return null;
            }

            var aiPort = _serviceProvider.GetService<IAIPort>();
            if (aiPort == null)
            {
                return null;
            }

            var isAvailable = await aiPort.IsAvailableAsync(ct);
            return isAvailable ? aiPort : null;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "AI port availability check failed");
            return null;
        }
    }

    private async Task<IEnumerable<SemanticSearchResult>> SemanticSearchWithEmbeddingsAsync(
        IAIPort aiPort, string query, int maxResults, CancellationToken ct)
    {
        // Embed the query
        var queryEmbedding = await aiPort.GetEmbeddingAsync(query, cancellationToken: ct);
        if (queryEmbedding.Embedding.Length == 0)
        {
            return await KeywordSearchAsync(query, maxResults, ct);
        }

        // Compute cosine similarity against all cached embeddings
        var results = new List<SemanticSearchResult>();

        foreach (var kvp in _embeddingCache)
        {
            var articleId = kvp.Key;
            var articleEmbedding = kvp.Value;

            // Skip if embedding dimensions don't match
            if (articleEmbedding.Length != queryEmbedding.Embedding.Length)
            {
                continue;
            }

            var similarity = CosineSimilarity(queryEmbedding.Embedding, articleEmbedding);

            if (_metadataCache.TryGetValue(articleId, out var metadata))
            {
                results.Add(new SemanticSearchResult
                {
                    ArticleId = articleId,
                    Title = metadata.Title,
                    Snippet = metadata.Snippet,
                    RelevanceScore = Math.Max(0, similarity) // Clamp to non-negative
                });
            }
        }

        return results
            .OrderByDescending(r => r.RelevanceScore)
            .Take(maxResults)
            .ToList();
    }

    private async Task<IEnumerable<SemanticSearchResult>> KeywordSearchAsync(string query, int maxResults, CancellationToken ct)
    {
        var context = _dbContext;

        var itsmArticles = await context.ITSMKnowledgeArticles
            .Where(a => !a.IsDeleted && a.PublishingState == PublishingState.Published)
            .Where(a => a.Title.Contains(query) ||
                        (a.ShortDescription != null && a.ShortDescription.Contains(query)) ||
                        a.ArticleBody.Contains(query))
            .OrderByDescending(a => a.ViewCount)
            .Take(maxResults)
            .ToListAsync(ct);

        var itsmResults = itsmArticles.Select(a => new SemanticSearchResult
        {
            ArticleId = a.ArticleId,
            Title = a.Title,
            Snippet = GetSnippet(a.ArticleBody),
            RelevanceScore = 0.5 // Default score for keyword matches
        });

        // KB-014: also search General KB articles in keyword fallback
        var generalArticles = await context.KnowledgeArticles
            .Where(a => !a.IsDeleted && a.Status == GeneralKbArticleStatus.Published)
            .Where(a => a.Title.Contains(query) ||
                        (a.Summary != null && a.Summary.Contains(query)) ||
                        a.Content.Contains(query))
            .OrderByDescending(a => a.ViewCount)
            .Take(maxResults)
            .ToListAsync(ct);

        var generalResults = generalArticles.Select(a => new SemanticSearchResult
        {
            ArticleId = a.Id + 100_000, // KB-014: offset to match ReindexAllAsync cache key convention
            Title = a.Title,
            Snippet = GetSnippet(a.Content),
            RelevanceScore = 0.5
        });

        return itsmResults.Concat(generalResults)
            .OrderByDescending(r => r.RelevanceScore)
            .Take(maxResults)
            .ToList();
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length || a.Length == 0)
        {
            return 0;
        }

        double dotProduct = 0;
        double normA = 0;
        double normB = 0;

        for (int i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        var denominator = Math.Sqrt(normA) * Math.Sqrt(normB);
        return Math.Abs(denominator) < 1e-10 ? 0 : dotProduct / denominator;
    }

    private static string BuildArticleText(KnowledgeArticle article)
    {
        var parts = new List<string> { article.Title };
        if (!string.IsNullOrEmpty(article.ShortDescription))
        {
            parts.Add(article.ShortDescription);
        }
        parts.Add(article.ArticleBody);
        return string.Join(". ", parts);
    }

    // KB-014: builds indexable text for General KB articles
    private static string BuildGeneralArticleText(CRM.Core.Entities.KnowledgeBase.KnowledgeArticle article)
    {
        var parts = new List<string> { article.Title };
        if (!string.IsNullOrEmpty(article.Summary))
        {
            parts.Add(article.Summary);
        }
        parts.Add(article.Content);
        return string.Join(". ", parts);
    }

    private static string GetSnippet(string body, int maxLength = 200)
    {
        if (string.IsNullOrEmpty(body))
        {
            return string.Empty;
        }

        return body.Length <= maxLength
            ? body
            : body[..maxLength] + "...";
    }

    #endregion

    #region Internal Types

    private class ArticleMetadata
    {
        public string Title { get; set; } = string.Empty;
        public string Snippet { get; set; } = string.Empty;
        public PublishingState PublishingState { get; set; }
    }

    #endregion
}
