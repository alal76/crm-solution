// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Infrastructure.Services.Search;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for SearchAnalyticsService.
/// Verifies search tracking, popular query listing, and zero-result detection.
/// Uses unique query strings per test to prevent interference with the static in-memory store.
/// </summary>
public class SearchAnalyticsServiceTests
{
    private readonly SearchAnalyticsService _service;

    public SearchAnalyticsServiceTests()
    {
        var mockLogger = new Mock<ILogger<SearchAnalyticsService>>();
        _service = new SearchAnalyticsService(mockLogger.Object);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Test 1 – tracked query appears in popular queries list
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TrackSearchAsync_ShouldAppear_InPopularQueriesAfterTracking()
    {
        // Arrange — unique query to avoid interference with static state
        var uniqueQuery = $"crm-popular-{Guid.NewGuid():N}";

        // Track the query 3 times to ensure it shows up
        for (int i = 0; i < 3; i++)
        {
            await _service.TrackSearchAsync(new SearchAnalyticsEntry
            {
                Query = uniqueQuery,
                ResultCount = 5,
                ProcessingTimeMs = 10
            });
        }

        // Act
        var popular = (await _service.GetPopularQueriesAsync(200)).ToList();

        // Assert
        popular.Should().Contain(q => q.Query == uniqueQuery);
        popular.First(q => q.Query == uniqueQuery).SearchCount.Should().Be(3);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Test 2 – queries with ResultCount=0 appear in zero-result list
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TrackSearchAsync_ShouldAppear_InZeroResultQueries_WhenResultCountIsZero()
    {
        // Arrange — unique query to avoid interference
        var uniqueQuery = $"crm-zero-{Guid.NewGuid():N}";

        await _service.TrackSearchAsync(new SearchAnalyticsEntry
        {
            Query = uniqueQuery,
            ResultCount = 0,
            ProcessingTimeMs = 8
        });

        // Act
        var zeroResults = (await _service.GetZeroResultQueriesAsync(200)).ToList();

        // Assert
        zeroResults.Should().Contain(q => q.Query == uniqueQuery);
        zeroResults.First(q => q.Query == uniqueQuery).TotalResults.Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Test 3 – GetPerformanceMetricsAsync returns non-null metrics after tracking
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPerformanceMetricsAsync_ShouldReturnNonNull_AfterAtLeastOneSearch()
    {
        // Arrange — seed at least one entry
        await _service.TrackSearchAsync(new SearchAnalyticsEntry
        {
            Query = $"crm-perf-{Guid.NewGuid():N}",
            ResultCount = 3,
            ProcessingTimeMs = 25
        });

        // Act
        var metrics = await _service.GetPerformanceMetricsAsync();

        // Assert
        metrics.Should().NotBeNull();
        metrics.TotalSearches.Should().BeGreaterThan(0);
    }
}
