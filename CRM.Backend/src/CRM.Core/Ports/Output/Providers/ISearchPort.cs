// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace CRM.Core.Ports.Output.Providers;

#region Search Port Interface

/// <summary>
/// Output port for search operations supporting pluggable search providers.
/// Implementations can include BuiltIn (SQL-based), Meilisearch, Algolia, Typesense, Elasticsearch.
/// </summary>
public interface ISearchPort
{
    /// <summary>
    /// Gets the unique identifier for this search provider.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Checks if the search provider is properly configured and available.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the provider is available and healthy.</returns>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a unified search across all indexed entity types.
    /// </summary>
    /// <param name="request">The search request with query and filters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search results containing hits from all indexed types.</returns>
    Task<SearchResult> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches within a specific entity type.
    /// </summary>
    /// <typeparam name="T">The entity type to search.</typeparam>
    /// <param name="query">The search query string.</param>
    /// <param name="options">Optional search options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Typed search results for the specified entity.</returns>
    Task<SearchResult<T>> SearchAsync<T>(string query, SearchOptions? options = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Indexes a single document for searching.
    /// </summary>
    /// <typeparam name="T">The document type.</typeparam>
    /// <param name="document">The document to index.</param>
    /// <param name="id">The unique identifier for the document.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task IndexAsync<T>(T document, string id, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Indexes multiple documents in a batch operation.
    /// </summary>
    /// <typeparam name="T">The document type.</typeparam>
    /// <param name="documents">The documents to index.</param>
    /// <param name="idSelector">Function to extract the ID from each document.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task IndexBatchAsync<T>(IEnumerable<T> documents, Func<T, string> idSelector, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Removes a document from the search index.
    /// </summary>
    /// <typeparam name="T">The document type.</typeparam>
    /// <param name="id">The unique identifier of the document to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync<T>(string id, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets autocomplete suggestions based on a prefix.
    /// </summary>
    /// <param name="prefix">The prefix to search for.</param>
    /// <param name="indexName">Optional index name to search in.</param>
    /// <param name="maxResults">Maximum number of suggestions to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of suggested completions.</returns>
    Task<IEnumerable<SearchSuggestion>> SuggestAsync(string prefix, string? indexName = null, int maxResults = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all documents from an index.
    /// </summary>
    /// <typeparam name="T">The document type determining the index.</typeparam>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ClearIndexAsync<T>(CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Rebuilds an entire index from a collection of documents.
    /// </summary>
    /// <typeparam name="T">The document type.</typeparam>
    /// <param name="documents">The complete set of documents for the index.</param>
    /// <param name="idSelector">Function to extract the ID from each document.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RebuildIndexAsync<T>(IEnumerable<T> documents, Func<T, string> idSelector, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets the health status of the search provider.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Health check result with details.</returns>
    Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default);
}

#endregion

#region Search DTOs

/// <summary>
/// Request object for search operations.
/// </summary>
public class SearchRequest
{
    /// <summary>
    /// The search query string.
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Optional entity type to filter search results.
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Filter conditions as key-value pairs (field name → value).
    /// </summary>
    public Dictionary<string, string>? Filters { get; set; }

    /// <summary>
    /// Facet filters for multi-value filtering.
    /// </summary>
    public Dictionary<string, List<string>>? FacetFilters { get; set; }

    /// <summary>
    /// Number of results to skip (for pagination).
    /// </summary>
    public int Skip { get; set; } = 0;

    /// <summary>
    /// Maximum number of results to return.
    /// </summary>
    public int Take { get; set; } = 20;

    /// <summary>
    /// Field to sort results by.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Whether to sort in descending order.
    /// </summary>
    public bool SortDescending { get; set; } = false;

    /// <summary>
    /// Whether to include highlighted matches in results.
    /// </summary>
    public bool IncludeHighlights { get; set; } = true;

    /// <summary>
    /// Fields to include in the result (null = all fields).
    /// </summary>
    public List<string>? AttributesToRetrieve { get; set; }
}

/// <summary>
/// Options for typed search operations.
/// </summary>
public class SearchOptions
{
    /// <summary>
    /// Number of results to skip.
    /// </summary>
    public int Skip { get; set; } = 0;

    /// <summary>
    /// Maximum number of results to return.
    /// </summary>
    public int Take { get; set; } = 20;

    /// <summary>
    /// Filter conditions.
    /// </summary>
    public Dictionary<string, string>? Filters { get; set; }

    /// <summary>
    /// Field to sort by.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Whether to sort descending.
    /// </summary>
    public bool SortDescending { get; set; }

    /// <summary>
    /// Include match highlights.
    /// </summary>
    public bool IncludeHighlights { get; set; } = true;
}

/// <summary>
/// Result of a unified search operation.
/// </summary>
public class SearchResult
{
    /// <summary>
    /// The search hits/matches.
    /// </summary>
    public IEnumerable<SearchHit> Hits { get; set; } = Enumerable.Empty<SearchHit>();

    /// <summary>
    /// Total number of matching documents.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Time taken to process the search in milliseconds.
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// The original query string.
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Facet counts if faceting was requested.
    /// </summary>
    public Dictionary<string, Dictionary<string, int>>? Facets { get; set; }
}

/// <summary>
/// Result of a typed search operation.
/// </summary>
/// <typeparam name="T">The document type.</typeparam>
public class SearchResult<T> where T : class
{
    /// <summary>
    /// The matching items.
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    /// Total number of matching documents.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Time taken to process the search.
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// Current page number (0-indexed).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages { get; set; }
}

/// <summary>
/// A single search result hit.
/// </summary>
public class SearchHit
{
    /// <summary>
    /// Unique identifier of the document.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The entity type (Account, Contact, Opportunity, etc.).
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Display title for the result.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional description or snippet.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Relevance score (higher = more relevant).
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Highlighted text matches by field.
    /// </summary>
    public Dictionary<string, string>? Highlights { get; set; }

    /// <summary>
    /// Additional metadata fields.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// An autocomplete suggestion.
/// </summary>
public class SearchSuggestion
{
    /// <summary>
    /// The suggested text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The entity type if available.
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// The entity ID if this is a specific result.
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// Score/weight of this suggestion.
    /// </summary>
    public double Score { get; set; }
}

#endregion

#region Common Provider DTOs

/// <summary>
/// Result of a provider health check.
/// </summary>
public class ProviderHealthResult
{
    /// <summary>
    /// The provider name.
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the provider is healthy.
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Optional status message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Response time in milliseconds.
    /// </summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>
    /// Time of the health check.
    /// </summary>
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional details.
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = new();
}

#endregion
