// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net.Http.Json;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Search;

/// <summary>
/// Service for configuring and managing the dedicated Knowledge Base search index in Meilisearch.
/// Defines index schema, filterable/sortable attributes, and ranking rules optimized for KB articles.
/// TODO-SD002-012: Dedicated KB search index schema for Meilisearch.
/// </summary>
public interface IKnowledgeBaseSearchIndexService
{
    /// <summary>
    /// Gets the index configuration for the knowledge base.
    /// </summary>
    KnowledgeBaseIndexConfig GetIndexConfiguration();

    /// <summary>
    /// Configures the Meilisearch index with the KB-specific schema.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ConfigureIndexAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Indexes a single knowledge article.
    /// </summary>
    /// <param name="articleId">Article ID to index</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task IndexArticleAsync(int articleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-indexes all published knowledge articles.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of articles indexed</returns>
    Task<int> ReindexAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an article from the search index.
    /// </summary>
    /// <param name="articleId">Article ID to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveArticleAsync(int articleId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Configuration model for the Knowledge Base search index.
/// </summary>
public class KnowledgeBaseIndexConfig
{
    /// <summary>Index name in Meilisearch.</summary>
    public string IndexName { get; set; } = "crm_knowledge_articles";

    /// <summary>Primary key field.</summary>
    public string PrimaryKey { get; set; } = "id";

    /// <summary>Fields included in full-text search, ordered by relevance.</summary>
    public string[] SearchableAttributes { get; set; } =
    {
        "title",
        "content",
        "shortDescription",
        "tags",
        "category",
        "keywords"
    };

    /// <summary>Fields available for filtering (exact match / faceted).</summary>
    public string[] FilterableAttributes { get; set; } =
    {
        "status",
        "category",
        "categoryId",
        "tags",
        "articleType",
        "isInternal",
        "authorId",
        "publishedDate",
        "language"
    };

    /// <summary>Fields available for sorting.</summary>
    public string[] SortableAttributes { get; set; } =
    {
        "title",
        "publishedDate",
        "updatedAt",
        "viewCount",
        "helpfulCount",
        "createdAt"
    };

    /// <summary>Custom ranking rules for KB articles (Meilisearch format).</summary>
    public string[] RankingRules { get; set; } =
    {
        "words",
        "typo",
        "proximity",
        "attribute",
        "sort",
        "exactness",
        "viewCount:desc",
        "helpfulCount:desc"
    };

    /// <summary>Stop words to exclude from indexing.</summary>
    public string[] StopWords { get; set; } =
    {
        "the", "a", "an", "is", "are", "was", "were",
        "be", "been", "being", "have", "has", "had",
        "do", "does", "did", "will", "would", "could",
        "should", "may", "might", "shall", "can",
        "for", "and", "nor", "but", "or", "yet", "so",
        "in", "on", "at", "to", "from", "by", "with"
    };

    /// <summary>Synonyms for improved search matching.</summary>
    public Dictionary<string, string[]> Synonyms { get; set; } = new()
    {
        ["error"] = new[] { "issue", "problem", "bug", "fault", "defect" },
        ["fix"] = new[] { "solution", "resolve", "repair", "patch", "workaround" },
        ["install"] = new[] { "setup", "configure", "deploy", "provision" },
        ["login"] = new[] { "sign in", "authenticate", "log on", "access" },
        ["password"] = new[] { "credential", "passphrase", "secret" },
        ["reset"] = new[] { "restart", "reboot", "reinitialize" },
        ["slow"] = new[] { "performance", "latency", "lag", "delay" },
        ["crash"] = new[] { "failure", "hang", "freeze", "unresponsive" },
        ["upgrade"] = new[] { "update", "patch", "migrate" },
        ["delete"] = new[] { "remove", "purge", "erase", "uninstall" }
    };

    /// <summary>Fields to display in search results.</summary>
    public string[] DisplayedAttributes { get; set; } =
    {
        "id",
        "title",
        "shortDescription",
        "category",
        "tags",
        "status",
        "articleType",
        "authorName",
        "publishedDate",
        "viewCount",
        "helpfulCount"
    };

    /// <summary>Maximum number of characters in a search highlight.</summary>
    public int CropLength { get; set; } = 200;
}

/// <summary>
/// Document model for indexing in Meilisearch.
/// </summary>
public class KnowledgeArticleSearchDocument
{
    /// <summary>Article ID (primary key).</summary>
    public int Id { get; set; }

    /// <summary>Article title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Full article body/content (HTML stripped for indexing).</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Short description/summary.</summary>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>Article number.</summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>Category name.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Category ID for filtering.</summary>
    public int? CategoryId { get; set; }

    /// <summary>Comma-separated tags.</summary>
    public string Tags { get; set; } = string.Empty;

    /// <summary>Comma-separated keywords.</summary>
    public string Keywords { get; set; } = string.Empty;

    /// <summary>Publishing status (Draft, Published, Archived, etc.).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Article type (HowTo, FAQ, Troubleshooting, etc.).</summary>
    public string ArticleType { get; set; } = string.Empty;

    /// <summary>Whether the article is internal-only.</summary>
    public bool IsInternal { get; set; }

    /// <summary>Author's user ID.</summary>
    public int AuthorId { get; set; }

    /// <summary>Author display name.</summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>Published date (ISO 8601).</summary>
    public string? PublishedDate { get; set; }

    /// <summary>Last updated date (ISO 8601).</summary>
    public string UpdatedAt { get; set; } = string.Empty;

    /// <summary>Created date (ISO 8601).</summary>
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>Total view count.</summary>
    public int ViewCount { get; set; }

    /// <summary>Helpful vote count.</summary>
    public int HelpfulCount { get; set; }

    /// <summary>Language code.</summary>
    public string Language { get; set; } = "en";
}

/// <summary>
/// Implementation of Knowledge Base search index service.
/// Configures Meilisearch index with KB-specific schema, searchable/filterable/sortable fields,
/// custom ranking rules, synonyms, and stop words.
/// </summary>
public class KnowledgeBaseSearchIndexService : IKnowledgeBaseSearchIndexService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<KnowledgeBaseSearchIndexService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly KnowledgeBaseIndexConfig _config;
    private readonly string _meilisearchUrl;
    private readonly string _meilisearchApiKey;

    public KnowledgeBaseSearchIndexService(
        ICrmDbContext dbContext,
        ILogger<KnowledgeBaseSearchIndexService> logger,
        IHttpClientFactory httpClientFactory,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _dbContext = dbContext;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _config = new KnowledgeBaseIndexConfig();

        _meilisearchUrl = configuration["Providers:Search:Meilisearch:Url"] ?? "http://crm-meilisearch:7700";
        _meilisearchApiKey = configuration["Providers:Search:Meilisearch:ApiKey"] ?? "masterKey";
    }

    /// <inheritdoc />
    public KnowledgeBaseIndexConfig GetIndexConfiguration() => _config;

    /// <inheritdoc />
    public async Task ConfigureIndexAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Configuring Knowledge Base search index: {IndexName}", _config.IndexName);

        try
        {
            var client = CreateHttpClient();

            // 1. Create or update the index
            var createPayload = new { uid = _config.IndexName, primaryKey = _config.PrimaryKey };
            var createResponse = await client.PostAsJsonAsync(
                $"{_meilisearchUrl}/indexes",
                createPayload,
                cancellationToken);

            _logger.LogDebug("Index creation response: {StatusCode}", createResponse.StatusCode);

            // 2. Configure searchable attributes (order matters for relevance)
            await client.PutAsJsonAsync(
                $"{_meilisearchUrl}/indexes/{_config.IndexName}/settings/searchable-attributes",
                _config.SearchableAttributes,
                cancellationToken);

            // 3. Configure filterable attributes
            await client.PutAsJsonAsync(
                $"{_meilisearchUrl}/indexes/{_config.IndexName}/settings/filterable-attributes",
                _config.FilterableAttributes,
                cancellationToken);

            // 4. Configure sortable attributes
            await client.PutAsJsonAsync(
                $"{_meilisearchUrl}/indexes/{_config.IndexName}/settings/sortable-attributes",
                _config.SortableAttributes,
                cancellationToken);

            // 5. Configure ranking rules
            await client.PutAsJsonAsync(
                $"{_meilisearchUrl}/indexes/{_config.IndexName}/settings/ranking-rules",
                _config.RankingRules,
                cancellationToken);

            // 6. Configure stop words
            await client.PutAsJsonAsync(
                $"{_meilisearchUrl}/indexes/{_config.IndexName}/settings/stop-words",
                _config.StopWords,
                cancellationToken);

            // 7. Configure synonyms
            await client.PutAsJsonAsync(
                $"{_meilisearchUrl}/indexes/{_config.IndexName}/settings/synonyms",
                _config.Synonyms,
                cancellationToken);

            // 8. Configure displayed attributes
            await client.PutAsJsonAsync(
                $"{_meilisearchUrl}/indexes/{_config.IndexName}/settings/displayed-attributes",
                _config.DisplayedAttributes,
                cancellationToken);

            _logger.LogInformation("Knowledge Base search index configured successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure Knowledge Base search index");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task IndexArticleAsync(int articleId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Indexing knowledge article {ArticleId}", articleId);

        var article = await _dbContext.ITSMKnowledgeArticles
            .AsNoTracking()
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.ArticleId == articleId && !a.IsDeleted, cancellationToken);

        if (article == null)
        {
            _logger.LogWarning("Knowledge article {ArticleId} not found for indexing", articleId);
            return;
        }

        var document = MapToSearchDocument(article);
        var client = CreateHttpClient();

        await client.PostAsJsonAsync(
            $"{_meilisearchUrl}/indexes/{_config.IndexName}/documents",
            new[] { document },
            cancellationToken);

        _logger.LogDebug("Knowledge article {ArticleId} indexed successfully", articleId);
    }

    /// <inheritdoc />
    public async Task<int> ReindexAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Re-indexing all knowledge articles...");

        // First, configure the index schema
        await ConfigureIndexAsync(cancellationToken);

        // Fetch all non-deleted articles
        var articles = await _dbContext.ITSMKnowledgeArticles
            .AsNoTracking()
            .Include(a => a.Author)
            .Where(a => !a.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!articles.Any())
        {
            _logger.LogInformation("No knowledge articles to index");
            return 0;
        }

        var documents = articles.Select(MapToSearchDocument).ToList();
        var client = CreateHttpClient();

        // Batch index in chunks of 500
        const int batchSize = 500;
        for (int i = 0; i < documents.Count; i += batchSize)
        {
            var batch = documents.Skip(i).Take(batchSize).ToList();
            await client.PostAsJsonAsync(
                $"{_meilisearchUrl}/indexes/{_config.IndexName}/documents",
                batch,
                cancellationToken);

            _logger.LogDebug("Indexed batch {Start}-{End} of {Total} articles",
                i + 1, Math.Min(i + batchSize, documents.Count), documents.Count);
        }

        _logger.LogInformation("Re-indexed {Count} knowledge articles", documents.Count);
        return documents.Count;
    }

    /// <inheritdoc />
    public async Task RemoveArticleAsync(int articleId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Removing knowledge article {ArticleId} from search index", articleId);

        var client = CreateHttpClient();
        await client.DeleteAsync(
            $"{_meilisearchUrl}/indexes/{_config.IndexName}/documents/{articleId}",
            cancellationToken);

        _logger.LogDebug("Knowledge article {ArticleId} removed from search index", articleId);
    }

    private HttpClient CreateHttpClient()
    {
        var client = _httpClientFactory.CreateClient("MeilisearchKB");
        client.BaseAddress = new Uri(_meilisearchUrl);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_meilisearchApiKey}");
        return client;
    }

    private static KnowledgeArticleSearchDocument MapToSearchDocument(KnowledgeArticle article)
    {
        return new KnowledgeArticleSearchDocument
        {
            Id = article.ArticleId,
            Title = article.Title ?? string.Empty,
            Content = StripHtml(article.ArticleBody ?? string.Empty),
            ShortDescription = article.ShortDescription ?? string.Empty,
            Number = article.Number ?? string.Empty,
            Category = article.Category?.Name ?? string.Empty,
            CategoryId = article.CategoryId,
            Tags = article.Tags ?? string.Empty,
            Keywords = article.Tags ?? string.Empty,
            Status = article.PublishingState.ToString(),
            ArticleType = article.ArticleType.ToString(),
            IsInternal = article.IsInternal,
            AuthorId = article.AuthorId,
            AuthorName = article.Author != null
                ? $"{article.Author.FirstName} {article.Author.LastName}".Trim()
                : string.Empty,
            PublishedDate = article.PublishedDate?.ToString("o"),
            UpdatedAt = (article.ModifiedAt ?? article.CreatedAt).ToString("o"),
            CreatedAt = article.CreatedAt.ToString("o"),
            ViewCount = article.ViewCount,
            HelpfulCount = article.HelpfulCount,
            Language = "en"
        };
    }

    /// <summary>
    /// Strips HTML tags from content for plain-text indexing.
    /// </summary>
    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
        {
            return string.Empty;
        }

        // Remove HTML tags
            var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", " ", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(1));
        // Decode common HTML entities
        text = text.Replace("&amp;", "&")
                   .Replace("&lt;", "<")
                   .Replace("&gt;", ">")
                   .Replace("&nbsp;", " ")
                   .Replace("&quot;", "\"");
        // Collapse whitespace
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(1)).Trim();
        return text;
    }
}
