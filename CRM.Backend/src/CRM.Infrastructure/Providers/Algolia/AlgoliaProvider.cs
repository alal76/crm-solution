// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Diagnostics;
using System.Text.Json;
using Algolia.Search.Clients;
using CRM.Core.Entities;
using CRM.Core.Entities.KnowledgeBase;
using CRM.Core.Models;
using CRM.Core.Ports.Output.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Providers.Algolia;

/// <summary>
/// Algolia search provider implementing ISearchPort.
/// Provides fast, typo-tolerant full-text search with faceting and filtering.
/// </summary>
public class AlgoliaProvider : ISearchPort
{
    private readonly SearchClient? _client;
    private readonly AlgoliaConfiguration _config;
    private readonly ILogger<AlgoliaProvider> _logger;

    // Index name mappings for CRM entities
    private static readonly Dictionary<Type, string> EntityIndexNames = new()
    {
        { typeof(Account), "accounts" },
        { typeof(CRM.Core.Models.Contact), "contacts" },
        { typeof(Opportunity), "opportunities" },
        { typeof(Product), "products" },
        { typeof(KnowledgeArticle), "knowledge_articles" },
        { typeof(Lead), "leads" }
    };

    private static readonly Dictionary<string, string> EntityTypeToIndex = new()
    {
        { "Account", "accounts" },
        { "Contact", "contacts" },
        { "Opportunity", "opportunities" },
        { "Product", "products" },
        { "KnowledgeArticle", "knowledge_articles" },
        { "Lead", "leads" },
        { "Campaign", "campaigns" },
        { "Ticket", "tickets" },
        { "Quote", "quotes" },
        { "Contract", "contracts" }
    };

    public AlgoliaProvider(
        IOptions<AlgoliaConfiguration> config,
        ILogger<AlgoliaProvider> logger)
    {
        _config = config.Value;
        _logger = logger;

        if (!string.IsNullOrEmpty(_config.ApplicationId) && !string.IsNullOrEmpty(_config.ApiKey))
        {
            try
            {
                _client = new SearchClient(_config.ApplicationId, _config.ApiKey);
                _logger.LogInformation("Algolia client initialized for app {AppId}", _config.ApplicationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Algolia client");
            }
        }
        else
        {
            _logger.LogWarning("Algolia credentials not configured");
        }
    }

    /// <inheritdoc />
    public string ProviderName => "Algolia";

    /// <inheritdoc />
    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_client != null);
    }

    /// <inheritdoc />
    public async Task<CRM.Core.Ports.Output.Providers.SearchResult> SearchAsync(
        SearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var query = request.Query?.Trim() ?? string.Empty;

        if (_client == null || string.IsNullOrEmpty(query) || query.Length < 2)
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
        var totalCount = 0;

        try
        {
            var indexNames = GetIndexNames(request.EntityType);

            foreach (var indexName in indexNames)
            {
                try
                {
                    var fullIndexName = GetFullIndexName(indexName);

                    var response = await _client.SearchSingleIndexAsync<Dictionary<string, object>>(
                        fullIndexName,
                        new global::Algolia.Search.Models.Search.SearchParams(
                            new global::Algolia.Search.Models.Search.SearchParamsObject
                            {
                                Query = query,
                                HitsPerPage = request.Take > 0 ? request.Take : _config.DefaultPageSize,
                                Page = request.Skip / Math.Max(request.Take, 1)
                            }),
                        cancellationToken: cancellationToken);

                    if (response.Hits != null)
                    {
                        foreach (var hit in response.Hits)
                        {
                            allHits.Add(MapToSearchHit(hit, indexName));
                        }
                    }

                    totalCount += (int)(response.NbHits ?? 0);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to search Algolia index {Index}", indexName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Algolia search failed for query: {Query}", query);
        }

        stopwatch.Stop();

        return new CRM.Core.Ports.Output.Providers.SearchResult
        {
            Query = request.Query ?? string.Empty,
            Hits = allHits,
            TotalCount = totalCount,
            ProcessingTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

    /// <inheritdoc />
    public async Task<CRM.Core.Ports.Output.Providers.SearchResult<T>> SearchAsync<T>(
        string query,
        SearchOptions? options = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var stopwatch = Stopwatch.StartNew();
        options ??= new SearchOptions();

        if (_client == null || string.IsNullOrEmpty(query?.Trim()))
        {
            return new CRM.Core.Ports.Output.Providers.SearchResult<T>
            {
                Items = Enumerable.Empty<T>(),
                TotalCount = 0,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        try
        {
            var indexName = GetIndexNameForType<T>();
            var fullIndexName = GetFullIndexName(indexName);

            var searchParamsObj = new global::Algolia.Search.Models.Search.SearchParamsObject
            {
                Query = query,
                HitsPerPage = options.Take > 0 ? options.Take : _config.DefaultPageSize,
                Page = options.Skip / Math.Max(options.Take, 1)
            };

            if (options.Filters?.Any() == true)
            {
                searchParamsObj.Filters = string.Join(" AND ",
                    options.Filters.Select(f => $"{f.Key}:{f.Value}"));
            }

            var response = await _client.SearchSingleIndexAsync<T>(
                fullIndexName,
                new global::Algolia.Search.Models.Search.SearchParams(searchParamsObj),
                cancellationToken: cancellationToken);

            stopwatch.Stop();

            return new CRM.Core.Ports.Output.Providers.SearchResult<T>
            {
                Items = response.Hits ?? Enumerable.Empty<T>(),
                TotalCount = (int)(response.NbHits ?? 0),
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Algolia typed search failed for {Type}", typeof(T).Name);
            stopwatch.Stop();

            return new CRM.Core.Ports.Output.Providers.SearchResult<T>
            {
                Items = Enumerable.Empty<T>(),
                TotalCount = 0,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    /// <inheritdoc />
    public async Task IndexAsync<T>(
        T document,
        string id,
        CancellationToken cancellationToken = default) where T : class
    {
        if (_client == null || document == null)
        {
            return;
        }

        try
        {
            var indexName = GetIndexNameForType<T>();
            var fullIndexName = GetFullIndexName(indexName);

            var json = JsonSerializer.Serialize(document);
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new();
            dict["objectID"] = id;

            await _client.SaveObjectsAsync(fullIndexName, new List<Dictionary<string, object>> { dict });

            _logger.LogDebug("Indexed document {Id} to Algolia index {Index}", id, fullIndexName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index document {Id} to Algolia", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task IndexBatchAsync<T>(
        IEnumerable<T> documents,
        Func<T, string> idSelector,
        CancellationToken cancellationToken = default) where T : class
    {
        if (_client == null || documents == null)
        {
            return;
        }

        var docList = documents.ToList();
        if (!docList.Any())
        {
            return;
        }

        try
        {
            var indexName = GetIndexNameForType<T>();
            var fullIndexName = GetFullIndexName(indexName);

            var records = docList.Select(doc =>
            {
                var json = JsonSerializer.Serialize(doc);
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new();
                dict["objectID"] = idSelector(doc);
                return dict;
            }).ToList();

            var batchSize = _config.BatchSize > 0 ? _config.BatchSize : 1000;
            for (var i = 0; i < records.Count; i += batchSize)
            {
                var batch = records.Skip(i).Take(batchSize).ToList();
                await _client.SaveObjectsAsync(fullIndexName, batch);
            }

            _logger.LogInformation("Batch indexed {Count} documents to Algolia index {Index}",
                docList.Count, fullIndexName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to batch index documents to Algolia");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync<T>(
        string id,
        CancellationToken cancellationToken = default) where T : class
    {
        if (_client == null || string.IsNullOrEmpty(id))
        {
            return;
        }

        try
        {
            var indexName = GetIndexNameForType<T>();
            var fullIndexName = GetFullIndexName(indexName);

            await _client.DeleteObjectsAsync(fullIndexName, new List<string> { id });

            _logger.LogDebug("Deleted document {Id} from Algolia index {Index}", id, fullIndexName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete document {Id} from Algolia", id);
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
        if (_client == null || string.IsNullOrWhiteSpace(prefix))
        {
            return Enumerable.Empty<SearchSuggestion>();
        }

        try
        {
            var targetIndex = indexName ?? "accounts";
            var fullIndexName = GetFullIndexName(targetIndex);

            var searchParamsObj = new global::Algolia.Search.Models.Search.SearchParamsObject
            {
                Query = prefix,
                HitsPerPage = maxResults,
                AttributesToRetrieve = new List<string> { "name", "title", "firstName", "lastName" }
            };

            var response = await _client.SearchSingleIndexAsync<Dictionary<string, object>>(
                fullIndexName,
                new global::Algolia.Search.Models.Search.SearchParams(searchParamsObj),
                cancellationToken: cancellationToken);

            return response.Hits?
                .Select(h => new SearchSuggestion
                {
                    Text = GetSuggestionText(h),
                    Score = 1.0
                })
                .Where(s => !string.IsNullOrEmpty(s.Text))
                .Take(maxResults) ?? Enumerable.Empty<SearchSuggestion>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Algolia suggest failed for prefix: {Prefix}", prefix);
            return Enumerable.Empty<SearchSuggestion>();
        }
    }

    /// <inheritdoc />
    public async Task ClearIndexAsync<T>(CancellationToken cancellationToken = default) where T : class
    {
        if (_client == null)
        {
            return;
        }

        try
        {
            var indexName = GetIndexNameForType<T>();
            var fullIndexName = GetFullIndexName(indexName);

            await _client.ClearObjectsAsync(fullIndexName);

            _logger.LogInformation("Cleared Algolia index {Index}", fullIndexName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear Algolia index for {Type}", typeof(T).Name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task RebuildIndexAsync<T>(
        IEnumerable<T> documents,
        Func<T, string> idSelector,
        CancellationToken cancellationToken = default) where T : class
    {
        if (_client == null || documents == null)
        {
            return;
        }

        var docList = documents.ToList();

        try
        {
            var indexName = GetIndexNameForType<T>();
            var fullIndexName = GetFullIndexName(indexName);

            var records = docList.Select(doc =>
            {
                var json = JsonSerializer.Serialize(doc);
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new();
                dict["objectID"] = idSelector(doc);
                return dict;
            }).ToList();

            await _client.ReplaceAllObjectsAsync(fullIndexName, records);

            _logger.LogInformation("Rebuilt Algolia index {Index} with {Count} documents",
                fullIndexName, docList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rebuild Algolia index for {Type}", typeof(T).Name);
            throw;
        }
    }

    /// <inheritdoc />
    public Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var result = new ProviderHealthResult
        {
            ProviderName = ProviderName,
            IsHealthy = _client != null,
            Message = _client != null ? "Algolia client configured" : "Algolia client not configured",
            CheckedAt = DateTime.UtcNow
        };

        if (_client != null)
        {
            result.Details["ApplicationId"] = _config.ApplicationId ?? "Not set";
            result.Details["IndexPrefix"] = _config.IndexPrefix ?? "crm_";
        }

        return Task.FromResult(result);
    }

    #region Private Helper Methods

    private List<string> GetIndexNames(string? entityType)
    {
        if (!string.IsNullOrEmpty(entityType) && EntityTypeToIndex.TryGetValue(entityType, out var indexName))
        {
            return new List<string> { indexName };
        }

        return new List<string> { "accounts", "contacts", "opportunities", "products", "knowledge_articles", "leads" };
    }

    private string GetFullIndexName(string indexName)
    {
        var prefix = _config.IndexPrefix ?? "crm_";
        return prefix + indexName;
    }

    private string GetIndexNameForType<T>()
    {
        if (EntityIndexNames.TryGetValue(typeof(T), out var indexName))
        {
            return indexName;
        }

        return typeof(T).Name.ToLowerInvariant() + "s";
    }

    private SearchHit MapToSearchHit(Dictionary<string, object> hit, string indexName)
    {
        var entityType = EntityTypeToIndex.FirstOrDefault(x => x.Value == indexName).Key ?? indexName;

        return new SearchHit
        {
            Id = hit.TryGetValue("objectID", out var id) ? id?.ToString() ?? "" : "",
            EntityType = entityType,
            Title = GetTitle(hit),
            Description = hit.TryGetValue("description", out var desc) ? desc?.ToString() : null,
            Score = 1.0
        };
    }

    private static string GetTitle(Dictionary<string, object> hit)
    {
        if (hit.TryGetValue("name", out var name) && name != null)
        {
            return name.ToString() ?? "";
        }
        if (hit.TryGetValue("title", out var title) && title != null)
        {
            return title.ToString() ?? "";
        }
        if (hit.TryGetValue("firstName", out var firstName) && hit.TryGetValue("lastName", out var lastName))
        {
            return $"{firstName} {lastName}".Trim();
        }
        return "";
    }

    private static string GetSuggestionText(Dictionary<string, object> hit)
    {
        if (hit.TryGetValue("name", out var name) && name != null)
        {
            return name.ToString() ?? "";
        }
        if (hit.TryGetValue("title", out var title) && title != null)
        {
            return title.ToString() ?? "";
        }
        if (hit.TryGetValue("firstName", out var firstName) && hit.TryGetValue("lastName", out var lastName))
        {
            return $"{firstName} {lastName}".Trim();
        }
        return "";
    }
    /// <inheritdoc />
    public async Task RebuildAllIndexesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Algolia: RebuildAllIndexesAsync - no-op for Algolia provider (use Algolia dashboard to reindex).");
        // Algolia v2 SDK does not support bulk index clearing via the search client.
        // A full rebuild should be triggered via re-indexing all entities through IndexAsync().
        await Task.CompletedTask;
    }

    #endregion
}
