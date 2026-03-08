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
        var article = await context.ITSMKnowledgeArticles
            .FirstOrDefaultAsync(a => a.ArticleId == articleId && !a.IsDeleted, ct);

        if (article == null)
        {
            _logger.LogWarning("Article {ArticleId} not found for indexing", articleId);
            return;
        }

        try
        {
            var text = BuildArticleText(article);
            var embeddingResponse = await aiPort.GetEmbeddingAsync(text, cancellationToken: ct);

            if (embeddingResponse.Embedding.Length > 0)
            {
                _embeddingCache[articleId] = embeddingResponse.Embedding;
                _metadataCache[articleId] = new ArticleMetadata
                {
                    Title = article.Title,
                    Snippet = GetSnippet(article.ArticleBody),
                    PublishingState = article.PublishingState
                };
                _logger.LogInformation("Indexed article {ArticleId} with {Dimensions}-dimensional embedding", articleId, embeddingResponse.Embedding.Length);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index article {ArticleId}", articleId);
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

        var context = _dbContext;
        var articles = await context.ITSMKnowledgeArticles
            .Where(a => !a.IsDeleted && a.PublishingState == PublishingState.Published)
            .ToListAsync(ct);

        if (articles.Count == 0)
        {
            _logger.LogInformation("No published articles to index");
            return;
        }

        _logger.LogInformation("Reindexing {Count} published articles", articles.Count);

        var texts = articles.Select(BuildArticleText).ToList();

        try
        {
            var batchResponse = await aiPort.GetEmbeddingsAsync(texts, cancellationToken: ct);

            for (var i = 0; i < articles.Count && i < batchResponse.Embeddings.Count; i++)
            {
                var article = articles[i];
                var embedding = batchResponse.Embeddings[i];

                if (embedding.Length > 0)
                {
                    _embeddingCache[article.ArticleId] = embedding;
                    _metadataCache[article.ArticleId] = new ArticleMetadata
                    {
                        Title = article.Title,
                        Snippet = GetSnippet(article.ArticleBody),
                        PublishingState = article.PublishingState
                    };
                }
            }

            _logger.LogInformation("Successfully reindexed {Count} articles", _embeddingCache.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Batch reindexing failed, attempting individual indexing");

            // Fallback to individual indexing
            foreach (var article in articles)
            {
                try
                {
                    var text = BuildArticleText(article);
                    var response = await aiPort.GetEmbeddingAsync(text, cancellationToken: ct);
                    if (response.Embedding.Length > 0)
                    {
                        _embeddingCache[article.ArticleId] = response.Embedding;
                        _metadataCache[article.ArticleId] = new ArticleMetadata
                        {
                            Title = article.Title,
                            Snippet = GetSnippet(article.ArticleBody),
                            PublishingState = article.PublishingState
                        };
                    }
                }
                catch (Exception innerEx)
                {
                    _logger.LogWarning(innerEx, "Failed to index article {ArticleId}", article.ArticleId);
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

        var articles = await context.ITSMKnowledgeArticles
            .Where(a => !a.IsDeleted && a.PublishingState == PublishingState.Published)
            .Where(a => a.Title.Contains(query) ||
                        (a.ShortDescription != null && a.ShortDescription.Contains(query)) ||
                        a.ArticleBody.Contains(query))
            .OrderByDescending(a => a.ViewCount)
            .Take(maxResults)
            .ToListAsync(ct);

        return articles.Select(a => new SemanticSearchResult
        {
            ArticleId = a.ArticleId,
            Title = a.Title,
            Snippet = GetSnippet(a.ArticleBody),
            RelevanceScore = 0.5 // Default score for keyword matches
        }).ToList();
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
