// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Diagnostics;
using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Entities.ITSM;
using CRM.Core.Models;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Services.Knowledge; // KB-015: KnowledgeIndexDocument
using Meilisearch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Providers.Meilisearch;

/// <summary>
/// Meilisearch search provider implementing ISearchPort.
/// Provides fast, typo-tolerant full-text search with faceting and filtering.
/// </summary>
public class MeilisearchProvider : ISearchPort
{
    private readonly MeilisearchClient _client;
    private readonly MeilisearchConfiguration _config;
    private readonly ILogger<MeilisearchProvider> _logger;

    // Index name mappings for CRM entities
    private static readonly Dictionary<Type, string> EntityIndexNames = new()
    {
        { typeof(Account), "accounts" },
        { typeof(CRM.Core.Models.Contact), "contacts" },
        { typeof(Opportunity), "opportunities" },
        { typeof(Product), "products" },
        { typeof(KnowledgeArticle), "knowledge_articles" },
        { typeof(Lead), "leads" },
        // KB-015: unified knowledge index document (General KB + ITSM KB with source discriminator)
        { typeof(KnowledgeIndexDocument), "knowledge_articles" }
    };

    public MeilisearchProvider(
        IOptions<MeilisearchConfiguration> config,
        ILogger<MeilisearchProvider> logger)
    {
        _config = config.Value;
        _logger = logger;
        _client = new MeilisearchClient(_config.Url, _config.ApiKey);
    }

    /// <inheritdoc />
    public string ProviderName => "Meilisearch";

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var health = await _client.HealthAsync(cancellationToken);
            return health.Status == "available";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Meilisearch health check failed");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<CRM.Core.Ports.Output.Providers.SearchResult> SearchAsync(
        CRM.Core.Ports.Output.Providers.SearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var query = request.Query?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(query) || query.Length < 2)
        {
            return new CRM.Core.Ports.Output.Providers.SearchResult
            {
                Query = request.Query ?? string.Empty,
                Hits = new List<SearchHit>(),
                TotalCount = 0,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        var allHits = new List<SearchHit>();

        try
        {
            // Determine which indexes to search
            var indexNames = GetIndexNames(request.EntityType);

            // Search each index individually (SDK 0.15.0 approach)
            foreach (var indexName in indexNames)
            {
                try
                {
                    var index = _client.Index(GetFullIndexName(indexName));

                    var searchParams = new SearchQuery
                    {
                        Limit = request.Take,
                        Offset = request.Skip
                    };

                    if (_config.EnableHighlighting)
                    {
                        searchParams.AttributesToHighlight = new[] { "*" };
                        searchParams.HighlightPreTag = "<mark>";
                        searchParams.HighlightPostTag = "</mark>";
                    }

                    var searchResult = await index.SearchAsync<Dictionary<string, object>>(
                        query,
                        searchParams,
                        cancellationToken);

                    var entityType = GetEntityTypeFromIndexName(indexName);
                    foreach (var hit in searchResult.Hits)
                    {
                        var searchHit = MapToSearchHit(hit, entityType, searchResult.Query ?? query);
                        if (searchHit != null)
                        {
                            allHits.Add(searchHit);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Search failed for index {Index}", indexName);
                }
            }

            // Sort by relevance score
            var sortedHits = allHits
                .OrderByDescending(h => h.Score)
                .Take(request.Take)
                .ToList();

            stopwatch.Stop();

            return new CRM.Core.Ports.Output.Providers.SearchResult
            {
                Query = request.Query ?? string.Empty,
                Hits = sortedHits,
                TotalCount = allHits.Count,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Meilisearch query failed for: {Query}", query);
            stopwatch.Stop();

            return new CRM.Core.Ports.Output.Providers.SearchResult
            {
                Query = request.Query ?? string.Empty,
                Hits = new List<SearchHit>(),
                TotalCount = 0,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    /// <inheritdoc />
    public async Task<CRM.Core.Ports.Output.Providers.SearchResult<T>> SearchAsync<T>(
        string query,
        CRM.Core.Ports.Output.Providers.SearchOptions? options = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var stopwatch = Stopwatch.StartNew();
        var trimmedQuery = query?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedQuery) || trimmedQuery.Length < 2)
        {
            return new CRM.Core.Ports.Output.Providers.SearchResult<T>
            {
                Items = new List<T>(),
                TotalCount = 0,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        try
        {
            var indexName = GetIndexNameForType<T>();
            var index = _client.Index(GetFullIndexName(indexName));

            var searchParams = new SearchQuery
            {
                Limit = options?.Take ?? _config.DefaultPageSize,
                Offset = options?.Skip ?? 0
            };

            if (_config.EnableHighlighting)
            {
                searchParams.AttributesToHighlight = new[] { "*" };
            }

            // Apply filters if provided
            if (options?.Filters != null && options.Filters.Any())
            {
                var filterStrings = options.Filters.Select(f => $"{f.Key} = \"{f.Value}\"");
                searchParams.Filter = string.Join(" AND ", filterStrings);
            }

            // Apply sorting if provided
            if (!string.IsNullOrEmpty(options?.SortBy))
            {
                var sortDirection = options.SortDescending ? "desc" : "asc";
                searchParams.Sort = new[] { $"{options.SortBy}:{sortDirection}" };
            }

            var searchResult = await index.SearchAsync<T>(trimmedQuery, searchParams, cancellationToken);

            stopwatch.Stop();

            // Get total hits count from the result
            var totalHits = searchResult.Hits.Count();

            return new CRM.Core.Ports.Output.Providers.SearchResult<T>
            {
                Items = searchResult.Hits.ToList(),
                TotalCount = totalHits,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Meilisearch typed search failed for type {Type}, query: {Query}", typeof(T).Name, query);
            stopwatch.Stop();

            return new CRM.Core.Ports.Output.Providers.SearchResult<T>
            {
                Items = new List<T>(),
                TotalCount = 0,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    /// <inheritdoc />
    public async Task IndexAsync<T>(T document, string id, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var indexName = GetIndexNameForType<T>();
            var index = _client.Index(GetFullIndexName(indexName));

            // Ensure the document has an id field
            var docWithId = AddIdToDocument(document, id);

            var task = await index.AddDocumentsAsync(new[] { docWithId }, "id", cancellationToken);
            _logger.LogDebug("Indexed document {Id} in {Index}, task: {TaskUid}", id, indexName, task.TaskUid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index document {Id} for type {Type}", id, typeof(T).Name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task IndexBatchAsync<T>(IEnumerable<T> documents, Func<T, string> idSelector, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var indexName = GetIndexNameForType<T>();
            var index = _client.Index(GetFullIndexName(indexName));

            var documentList = documents.ToList();
            var totalDocuments = documentList.Count;
            var processedCount = 0;

            // Process in batches
            foreach (var batch in documentList.Chunk(_config.BatchSize))
            {
                var docsWithIds = batch.Select(doc => AddIdToDocument(doc, idSelector(doc))).ToList();
                var task = await index.AddDocumentsAsync(docsWithIds, "id", cancellationToken);

                processedCount += batch.Length;
                _logger.LogDebug("Indexed batch {Processed}/{Total} in {Index}, task: {TaskUid}",
                    processedCount, totalDocuments, indexName, task.TaskUid);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to batch index documents for type {Type}", typeof(T).Name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync<T>(string id, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var indexName = GetIndexNameForType<T>();
            var index = _client.Index(GetFullIndexName(indexName));

            var task = await index.DeleteOneDocumentAsync(id, cancellationToken);
            _logger.LogDebug("Deleted document {Id} from {Index}, task: {TaskUid}", id, indexName, task.TaskUid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete document {Id} for type {Type}", id, typeof(T).Name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SearchSuggestion>> SuggestAsync(
        string prefix,
        string? indexName = null,
        int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prefix) || prefix.Length < 2)
        {
            return Enumerable.Empty<SearchSuggestion>();
        }

        try
        {
            // If no index specified, search the default "accounts" index
            var targetIndex = indexName ?? "accounts";
            var index = _client.Index(GetFullIndexName(targetIndex));

            var searchParams = new SearchQuery
            {
                Limit = maxResults
            };

            var result = await index.SearchAsync<Dictionary<string, object>>(prefix, searchParams, cancellationToken);

            return result.Hits.Select(hit =>
            {
                var title = hit.TryGetValue("name", out var name) ? name?.ToString() :
                           hit.TryGetValue("title", out var t) ? t?.ToString() :
                           hit.TryGetValue("firstName", out var fn) && hit.TryGetValue("lastName", out var ln)
                               ? $"{fn} {ln}" : "Unknown";

                var id = hit.TryGetValue("id", out var idValue) ? idValue?.ToString() ?? "" : "";

                return new SearchSuggestion
                {
                    Text = title ?? "Unknown",
                    Score = 1.0,
                    EntityType = targetIndex,
                    EntityId = id
                };
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Meilisearch suggest failed for prefix: {Prefix}", prefix);
            return Enumerable.Empty<SearchSuggestion>();
        }
    }

    /// <inheritdoc />
    public async Task ClearIndexAsync<T>(CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var indexName = GetIndexNameForType<T>();
            var index = _client.Index(GetFullIndexName(indexName));

            var task = await index.DeleteAllDocumentsAsync(cancellationToken);
            _logger.LogInformation("Cleared index {Index}, task: {TaskUid}", indexName, task.TaskUid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear index for type {Type}", typeof(T).Name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task RebuildIndexAsync<T>(IEnumerable<T> documents, Func<T, string> idSelector, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var indexName = GetIndexNameForType<T>();

            _logger.LogInformation("Starting index rebuild for {Index}", indexName);

            // Clear existing documents
            await ClearIndexAsync<T>(cancellationToken);

            // Wait a moment for the delete to process
            await Task.Delay(500, cancellationToken);

            // Re-index all documents
            await IndexBatchAsync(documents, idSelector, cancellationToken);

            _logger.LogInformation("Completed index rebuild for {Index}", indexName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rebuild index for type {Type}", typeof(T).Name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var health = await _client.HealthAsync(cancellationToken);
            var version = await _client.GetVersionAsync(cancellationToken);
            var stats = await _client.GetStatsAsync(cancellationToken);

            stopwatch.Stop();

            return new ProviderHealthResult
            {
                IsHealthy = health.Status == "available",
                ProviderName = ProviderName,
                Message = $"Meilisearch {version.Version} - {stats.Indexes.Count} indexes",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                Details = new Dictionary<string, object>
                {
                    { "status", health.Status },
                    { "version", version.Version },
                    { "indexCount", stats.Indexes.Count.ToString() },
                    { "databaseSize", stats.DatabaseSize.ToString() }
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            return new ProviderHealthResult
            {
                IsHealthy = false,
                ProviderName = ProviderName,
                Message = $"Health check failed: {ex.Message}",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                Details = new Dictionary<string, object>
                {
                    { "error", ex.Message },
                    { "url", _config.Url }
                }
            };
        }
    }

    #region Index Management

    /// <summary>
    /// Creates or updates index settings for optimal CRM search.
    /// Call this during application startup or when index configuration changes.
    /// </summary>
    public async Task ConfigureIndexesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Configuring Meilisearch indexes for CRM entities");

        // Configure Account index
        await ConfigureIndexAsync("accounts", new MeilisearchIndexConfig
        {
            IndexName = "accounts",
            PrimaryKey = "id",
            SearchableAttributes = new List<string> { "name", "company", "industry", "description", "website" },
            FilterableAttributes = new List<string> { "industry", "status", "ownerId", "createdAt" },
            SortableAttributes = new List<string> { "name", "createdAt", "updatedAt" }
        }, cancellationToken);

        // Configure Contact index
        await ConfigureIndexAsync("contacts", new MeilisearchIndexConfig
        {
            IndexName = "contacts",
            PrimaryKey = "id",
            SearchableAttributes = new List<string> { "firstName", "lastName", "email", "phone", "title", "company" },
            FilterableAttributes = new List<string> { "accountId", "status", "ownerId", "createdAt" },
            SortableAttributes = new List<string> { "lastName", "firstName", "createdAt", "updatedAt" }
        }, cancellationToken);

        // Configure Opportunity index
        await ConfigureIndexAsync("opportunities", new MeilisearchIndexConfig
        {
            IndexName = "opportunities",
            PrimaryKey = "id",
            SearchableAttributes = new List<string> { "name", "description", "accountName" },
            FilterableAttributes = new List<string> { "accountId", "stage", "status", "ownerId", "closeDate" },
            SortableAttributes = new List<string> { "name", "amount", "closeDate", "createdAt" }
        }, cancellationToken);

        // Configure Product index
        await ConfigureIndexAsync("products", new MeilisearchIndexConfig
        {
            IndexName = "products",
            PrimaryKey = "id",
            SearchableAttributes = new List<string> { "name", "description", "sku", "category" },
            FilterableAttributes = new List<string> { "category", "isActive", "price" },
            SortableAttributes = new List<string> { "name", "price", "createdAt" }
        }, cancellationToken);

        // Configure Knowledge Article index
        await ConfigureIndexAsync("knowledge_articles", new MeilisearchIndexConfig
        {
            IndexName = "knowledge_articles",
            PrimaryKey = "id",
            SearchableAttributes = new List<string> { "title", "content", "summary", "keywords" },
            FilterableAttributes = new List<string> { "status", "category", "authorId", "publishedAt" },
            SortableAttributes = new List<string> { "title", "publishedAt", "viewCount" }
        }, cancellationToken);

        _logger.LogInformation("Meilisearch index configuration complete");
    }

    /// <summary>
    /// Configures a single index with the specified settings.
    /// </summary>
    private async Task ConfigureIndexAsync(string indexName, MeilisearchIndexConfig config, CancellationToken cancellationToken)
    {
        try
        {
            var fullIndexName = GetFullIndexName(indexName);

            // Create index if it doesn't exist
            var task = await _client.CreateIndexAsync(fullIndexName, config.PrimaryKey);
            _logger.LogDebug("Created/verified index {Index}, task: {TaskUid}", fullIndexName, task.TaskUid);

            // Wait for index creation
            await Task.Delay(200, cancellationToken);

            var index = _client.Index(fullIndexName);

            // Update searchable attributes
            if (config.SearchableAttributes.Any())
            {
                await index.UpdateSearchableAttributesAsync(config.SearchableAttributes, cancellationToken);
            }

            // Update filterable attributes
            if (config.FilterableAttributes.Any())
            {
                await index.UpdateFilterableAttributesAsync(config.FilterableAttributes, cancellationToken);
            }

            // Update sortable attributes
            if (config.SortableAttributes.Any())
            {
                await index.UpdateSortableAttributesAsync(config.SortableAttributes, cancellationToken);
            }

            _logger.LogDebug("Configured index {Index} with searchable: {Searchable}, filterable: {Filterable}",
                fullIndexName, string.Join(",", config.SearchableAttributes), string.Join(",", config.FilterableAttributes));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to configure index {Index}", indexName);
        }
    }

    #endregion

    #region Private Helpers

    private string GetFullIndexName(string indexName) => $"{_config.IndexPrefix}{indexName}";

    private string GetIndexNameForType<T>() where T : class
    {
        if (EntityIndexNames.TryGetValue(typeof(T), out var indexName))
        {
            return indexName;
        }

        // Fallback to pluralized type name
        return typeof(T).Name.ToLowerInvariant() + "s";
    }

    private List<string> GetIndexNames(string? entityTypeFilter)
    {
        if (string.IsNullOrEmpty(entityTypeFilter))
        {
            // Return all searchable indexes
            return new List<string> { "accounts", "contacts", "opportunities", "products", "knowledge_articles" };
        }

        // Map entity type to index name
        return entityTypeFilter.ToLowerInvariant() switch
        {
            "account" or "accounts" => new List<string> { "accounts" },
            "contact" or "contacts" => new List<string> { "contacts" },
            "opportunity" or "opportunities" => new List<string> { "opportunities" },
            "product" or "products" => new List<string> { "products" },
            "knowledgearticle" or "knowledge_articles" => new List<string> { "knowledge_articles" },
            _ => new List<string> { entityTypeFilter }
        };
    }

    private string GetEntityTypeFromIndexName(string indexName)
    {
        return indexName.ToLowerInvariant() switch
        {
            "accounts" => "Account",
            "contacts" => "Contact",
            "opportunities" => "Opportunity",
            "products" => "Product",
            "knowledge_articles" => "KnowledgeArticle",
            _ => indexName
        };
    }

    private SearchHit? MapToSearchHit(Dictionary<string, object> hit, string entityType, string queryText)
    {
        try
        {
            var id = hit.TryGetValue("id", out var idValue) ? idValue?.ToString() ?? "" : "";

            // Extract title based on entity type
            var title = entityType switch
            {
                "Account" => hit.TryGetValue("name", out var name) ? name?.ToString() :
                            hit.TryGetValue("company", out var company) ? company?.ToString() : "Unknown",
                "Contact" => hit.TryGetValue("firstName", out var fn) && hit.TryGetValue("lastName", out var ln)
                            ? $"{fn} {ln}" : "Unknown",
                "Opportunity" => hit.TryGetValue("name", out var oppName) ? oppName?.ToString() : "Unknown",
                "Product" => hit.TryGetValue("name", out var prodName) ? prodName?.ToString() : "Unknown",
                "KnowledgeArticle" => hit.TryGetValue("title", out var artTitle) ? artTitle?.ToString() : "Unknown",
                _ => hit.TryGetValue("name", out var n) ? n?.ToString() :
                    hit.TryGetValue("title", out var t) ? t?.ToString() : "Unknown"
            };

            var description = hit.TryGetValue("description", out var desc) ? desc?.ToString() :
                             hit.TryGetValue("summary", out var sum) ? sum?.ToString() :
                             hit.TryGetValue("content", out var content) ? TruncateString(content?.ToString(), 200) : null;

            // Extract highlights if available
            Dictionary<string, string>? highlights = null;
            if (hit.TryGetValue("_formatted", out var formatted) && formatted is JsonElement formattedElement)
            {
                var highlightsDict = new Dictionary<string, string>();
                foreach (var prop in formattedElement.EnumerateObject())
                {
                    highlightsDict[prop.Name] = prop.Value.ToString();
                }
                if (highlightsDict.Count > 0) // NOSONAR S2583 - populated by EnumerateObject foreach above
                {
                    highlights = highlightsDict;
                }
            }

            return new SearchHit
            {
                Id = id,
                EntityType = entityType,
                Title = title ?? "Unknown",
                Description = description,
                Score = 1.0, // Meilisearch doesn't expose numeric scores
                Highlights = highlights
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to map search hit for entity type {EntityType}", entityType);
            return null;
        }
    }

    private static string? TruncateString(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }
        return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
    }

    private static object AddIdToDocument<T>(T document, string id) where T : class
    {
        // Convert to dictionary and ensure id field exists
        var json = JsonSerializer.Serialize(document);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
        dict["id"] = id;
        return dict;
    }

    /// <inheritdoc />
    public async Task RebuildAllIndexesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Meilisearch: RebuildAllIndexesAsync - triggering full re-index");
        var indexes = new[] { "accounts", "contacts", "leads", "opportunities", "products" };
        foreach (var indexName in indexes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var indexRef = _client.Index(_config.IndexPrefix + indexName);
                await indexRef.DeleteAllDocumentsAsync();
                _logger.LogDebug("Meilisearch: cleared index {Index}", indexName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Meilisearch: RebuildAllIndexesAsync failed for index {Index}", indexName);
            }
        }
    }

    #endregion
}
