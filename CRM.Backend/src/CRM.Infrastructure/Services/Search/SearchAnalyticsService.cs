// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Search;

/// <summary>
/// Interface for search analytics tracking.
/// Tracks popular queries, zero-result queries, and search performance.
/// TODO-INFRA-10
/// </summary>
public interface ISearchAnalyticsService
{
    /// <summary>
    /// Records a search query and its result count.
    /// </summary>
    Task TrackSearchAsync(SearchAnalyticsEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most popular search queries.
    /// </summary>
    Task<IEnumerable<SearchQueryStats>> GetPopularQueriesAsync(int top = 20, DateTime? since = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets queries that returned zero results.
    /// </summary>
    Task<IEnumerable<SearchQueryStats>> GetZeroResultQueriesAsync(int top = 20, DateTime? since = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets search performance metrics.
    /// </summary>
    Task<SearchPerformanceMetrics> GetPerformanceMetricsAsync(DateTime? since = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets search queries by entity type.
    /// </summary>
    Task<Dictionary<string, int>> GetSearchesByEntityTypeAsync(DateTime? since = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets hourly search volume.
    /// </summary>
    Task<IEnumerable<SearchVolumeDataPoint>> GetSearchVolumeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the click-through rate for search results.
    /// </summary>
    Task RecordClickThroughAsync(string query, string entityType, string entityId, CancellationToken cancellationToken = default);
}

/// <summary>
/// A recorded search event.
/// </summary>
public class SearchAnalyticsEntry
{
    public string Query { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int ResultCount { get; set; }
    public long ProcessingTimeMs { get; set; }
    public int? UserId { get; set; }
    public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
    public string? Source { get; set; } // "global", "module", "autocomplete"
}

/// <summary>
/// Aggregated stats for a search query.
/// </summary>
public class SearchQueryStats
{
    public string Query { get; set; } = string.Empty;
    public int SearchCount { get; set; }
    public int TotalResults { get; set; }
    public double AverageResultCount { get; set; }
    public double AverageProcessingTimeMs { get; set; }
    public DateTime FirstSearched { get; set; }
    public DateTime LastSearched { get; set; }
    public int UniqueUsers { get; set; }
}

/// <summary>
/// Overall search performance metrics.
/// </summary>
public class SearchPerformanceMetrics
{
    public long TotalSearches { get; set; }
    public double AverageProcessingTimeMs { get; set; }
    public double P95ProcessingTimeMs { get; set; }
    public double ZeroResultRate { get; set; }
    public int UniqueQueries { get; set; }
    public int UniqueUsers { get; set; }
    public DateTime MeasuredSince { get; set; }
    public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// A data point for search volume over time.
/// </summary>
public class SearchVolumeDataPoint
{
    public DateTime Timestamp { get; set; }
    public int SearchCount { get; set; }
    public double AverageProcessingTimeMs { get; set; }
    public int ZeroResultCount { get; set; }
}

/// <summary>
/// In-memory search analytics service implementation.
/// Tracks search patterns and performance for optimization insights.
/// </summary>
public class SearchAnalyticsService : ISearchAnalyticsService
{
    private readonly ILogger<SearchAnalyticsService> _logger;

    private static readonly List<SearchAnalyticsEntry> _entries = new();
    private static readonly List<(string Query, string EntityType, string EntityId, DateTime ClickedAt)> _clickThroughs = new();
    private static readonly object _lock = new();

    public SearchAnalyticsService(ILogger<SearchAnalyticsService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task TrackSearchAsync(
        SearchAnalyticsEntry entry,
        CancellationToken cancellationToken = default)
    {
        entry.SearchedAt = DateTime.UtcNow;

        lock (_lock)
        {
            _entries.Add(entry);

            // Keep only last 100K entries in memory
            if (_entries.Count > 100_000)
            {
                _entries.RemoveRange(0, _entries.Count - 100_000);
            }
        }

        if (entry.ResultCount == 0)
        {
            _logger.LogDebug("Zero-result search: '{Query}' (entity: {EntityType})",
                entry.Query, entry.EntityType);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IEnumerable<SearchQueryStats>> GetPopularQueriesAsync(
        int top = 20, DateTime? since = null, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var filtered = FilterEntries(since);

            var stats = filtered
                .GroupBy(e => e.Query.ToLowerInvariant())
                .Select(g => new SearchQueryStats
                {
                    Query = g.First().Query,
                    SearchCount = g.Count(),
                    TotalResults = g.Sum(e => e.ResultCount),
                    AverageResultCount = g.Average(e => e.ResultCount),
                    AverageProcessingTimeMs = g.Average(e => e.ProcessingTimeMs),
                    FirstSearched = g.Min(e => e.SearchedAt),
                    LastSearched = g.Max(e => e.SearchedAt),
                    UniqueUsers = g.Where(e => e.UserId.HasValue).Select(e => e.UserId).Distinct().Count()
                })
                .OrderByDescending(s => s.SearchCount)
                .Take(top)
                .ToList();

            return Task.FromResult<IEnumerable<SearchQueryStats>>(stats);
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<SearchQueryStats>> GetZeroResultQueriesAsync(
        int top = 20, DateTime? since = null, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var filtered = FilterEntries(since)
                .Where(e => e.ResultCount == 0);

            var stats = filtered
                .GroupBy(e => e.Query.ToLowerInvariant())
                .Select(g => new SearchQueryStats
                {
                    Query = g.First().Query,
                    SearchCount = g.Count(),
                    TotalResults = 0,
                    AverageResultCount = 0,
                    AverageProcessingTimeMs = g.Average(e => e.ProcessingTimeMs),
                    FirstSearched = g.Min(e => e.SearchedAt),
                    LastSearched = g.Max(e => e.SearchedAt),
                    UniqueUsers = g.Where(e => e.UserId.HasValue).Select(e => e.UserId).Distinct().Count()
                })
                .OrderByDescending(s => s.SearchCount)
                .Take(top)
                .ToList();

            return Task.FromResult<IEnumerable<SearchQueryStats>>(stats);
        }
    }

    /// <inheritdoc />
    public Task<SearchPerformanceMetrics> GetPerformanceMetricsAsync(
        DateTime? since = null, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var filtered = FilterEntries(since).ToList();

            if (!filtered.Any())
            {
                return Task.FromResult(new SearchPerformanceMetrics
                {
                    MeasuredSince = since ?? DateTime.UtcNow.AddDays(-30)
                });
            }

            var orderedTimes = filtered.Select(e => e.ProcessingTimeMs).OrderBy(t => t).ToList();
            var p95Index = (int)(orderedTimes.Count * 0.95);

            var metrics = new SearchPerformanceMetrics
            {
                TotalSearches = filtered.Count,
                AverageProcessingTimeMs = filtered.Average(e => e.ProcessingTimeMs),
                P95ProcessingTimeMs = orderedTimes[Math.Min(p95Index, orderedTimes.Count - 1)],
                ZeroResultRate = filtered.Count > 0
                    ? (double)filtered.Count(e => e.ResultCount == 0) / filtered.Count * 100
                    : 0,
                UniqueQueries = filtered.Select(e => e.Query.ToLowerInvariant()).Distinct().Count(),
                UniqueUsers = filtered.Where(e => e.UserId.HasValue).Select(e => e.UserId).Distinct().Count(),
                MeasuredSince = since ?? filtered.Min(e => e.SearchedAt)
            };

            return Task.FromResult(metrics);
        }
    }

    /// <inheritdoc />
    public Task<Dictionary<string, int>> GetSearchesByEntityTypeAsync(
        DateTime? since = null, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var filtered = FilterEntries(since);

            var byType = filtered
                .GroupBy(e => e.EntityType ?? "All")
                .ToDictionary(g => g.Key, g => g.Count());

            return Task.FromResult(byType);
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<SearchVolumeDataPoint>> GetSearchVolumeAsync(
        DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var filtered = _entries
                .Where(e => e.SearchedAt >= from && e.SearchedAt <= to)
                .ToList();

            var dataPoints = filtered
                .GroupBy(e => new DateTime(e.SearchedAt.Year, e.SearchedAt.Month, e.SearchedAt.Day, e.SearchedAt.Hour, 0, 0))
                .Select(g => new SearchVolumeDataPoint
                {
                    Timestamp = g.Key,
                    SearchCount = g.Count(),
                    AverageProcessingTimeMs = g.Average(e => e.ProcessingTimeMs),
                    ZeroResultCount = g.Count(e => e.ResultCount == 0)
                })
                .OrderBy(d => d.Timestamp)
                .ToList();

            return Task.FromResult<IEnumerable<SearchVolumeDataPoint>>(dataPoints);
        }
    }

    /// <inheritdoc />
    public Task RecordClickThroughAsync(
        string query, string entityType, string entityId,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _clickThroughs.Add((query, entityType, entityId, DateTime.UtcNow));

            if (_clickThroughs.Count > 50_000)
            {
                _clickThroughs.RemoveRange(0, _clickThroughs.Count - 50_000);
            }
        }

        return Task.CompletedTask;
    }

    private static IEnumerable<SearchAnalyticsEntry> FilterEntries(DateTime? since)
    {
        var cutoff = since ?? DateTime.UtcNow.AddDays(-30);
        return _entries.Where(e => e.SearchedAt >= cutoff);
    }
}
