// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Spec: SK Plugin unit tests — SearchPlugin
// MANDATORY TEST RULE: All method signatures verified against actual source before writing.
// Source files read:
//   SearchPlugin.cs — KernelFunctions: GlobalSearch, SearchByType
//   ISearchPort.cs — SearchAsync(SearchRequest, CT)->Task<SearchResult>
//                    SearchResult: Hits, TotalCount, ProcessingTimeMs, Query
//                    SearchHit: EntityType, Id, Title, Description, Score, Highlights
//   CrmPluginBase.cs — SuccessResult({error:false,data:...}), ErrorResult({error:true,...})

using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.AI.SK.Plugins;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace CRM.Tests.Services.SK;

/// <summary>
/// Unit tests for <see cref="SearchPlugin"/>.
/// KernelFunctions tested: GlobalSearch, SearchByType
/// </summary>
public class SearchPluginTests
{
    private readonly Mock<ISearchPort> _searchPort = new(MockBehavior.Loose);
    private readonly Mock<ILogger<SearchPlugin>> _logger = new();
    private readonly SearchPlugin _sut;

    public SearchPluginTests()
    {
        _sut = new SearchPlugin(_searchPort.Object, _logger.Object);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Property / Constructor tests
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void PluginName_ShouldBe_Search()
    {
        _sut.PluginName.Should().Be("Search");
    }

    [Fact]
    public void Description_ShouldNotBeNullOrEmpty()
    {
        _sut.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenSearchPortIsNull()
    {
        var act = () => new SearchPlugin(null!, _logger.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("searchPort");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        var act = () => new SearchPlugin(_searchPort.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GlobalSearchAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GlobalSearchAsync_ShouldReturnSuccessJson_WithHitsAndMetadata()
    {
        var searchResult = new SearchResult
        {
            Query = "acme",
            TotalCount = 3,
            ProcessingTimeMs = 42,
            Hits = new List<SearchHit>
            {
                new() { Id = "1", EntityType = "Account",  Title = "Acme Corp",    Score = 0.95 },
                new() { Id = "2", EntityType = "Contact",  Title = "Acme Contact", Score = 0.80 },
                new() { Id = "3", EntityType = "Lead",     Title = "Acme Lead",    Score = 0.65 }
            }
        };
        _searchPort
            .Setup(s => s.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult);

        var result = await _sut.GlobalSearchAsync("acme");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        var data = doc.RootElement.GetProperty("data");
        data.GetProperty("totalCount").GetInt32().Should().Be(3);
        data.GetProperty("query").GetString().Should().Be("acme");
    }

    [Fact]
    public async Task GlobalSearchAsync_ShouldReturnSuccessJson_WhenNoHitsFound()
    {
        var emptyResult = new SearchResult
        {
            Query = "notexist",
            TotalCount = 0,
            ProcessingTimeMs = 5,
            Hits = Enumerable.Empty<SearchHit>()
        };
        _searchPort
            .Setup(s => s.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);

        var result = await _sut.GlobalSearchAsync("notexist");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task GlobalSearchAsync_ShouldPassMaxResultsToSearchRequest()
    {
        SearchRequest? capturedRequest = null;
        _searchPort
            .Setup(s => s.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .Callback<SearchRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new SearchResult { Query = "x", TotalCount = 0, Hits = [] });

        await _sut.GlobalSearchAsync("x", maxResults: 5);

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Take.Should().Be(5);
        capturedRequest.Query.Should().Be("x");
    }

    [Fact]
    public async Task GlobalSearchAsync_ShouldReturnErrorJson_WhenSearchPortThrows()
    {
        _searchPort
            .Setup(s => s.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Search engine unavailable"));

        var result = await _sut.GlobalSearchAsync("anything");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        result.Should().Contain("Search engine unavailable");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SearchByTypeAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchByTypeAsync_ShouldReturnSuccessJson_WithEntityTypeFilter()
    {
        var searchResult = new SearchResult
        {
            Query = "alice",
            TotalCount = 1,
            ProcessingTimeMs = 10,
            Hits = new List<SearchHit>
            {
                new() { Id = "10", EntityType = "Contact", Title = "Alice Smith", Score = 1.0 }
            }
        };
        _searchPort
            .Setup(s => s.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult);

        var result = await _sut.SearchByTypeAsync("alice", "Contact");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        var data = doc.RootElement.GetProperty("data");
        data.GetProperty("entityType").GetString().Should().Be("Contact");
        data.GetProperty("totalCount").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task SearchByTypeAsync_ShouldPassEntityTypeToSearchRequest()
    {
        SearchRequest? capturedRequest = null;
        _searchPort
            .Setup(s => s.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .Callback<SearchRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new SearchResult { Query = "test", TotalCount = 0, Hits = [] });

        await _sut.SearchByTypeAsync("test", "Opportunity", maxResults: 10);

        capturedRequest.Should().NotBeNull();
        capturedRequest!.EntityType.Should().Be("Opportunity");
        capturedRequest.Take.Should().Be(10);
    }

    [Fact]
    public async Task SearchByTypeAsync_ShouldReturnSuccessJson_WhenNoHitsFound()
    {
        var emptyResult = new SearchResult
        {
            Query = "nohits",
            TotalCount = 0,
            ProcessingTimeMs = 3,
            Hits = Enumerable.Empty<SearchHit>()
        };
        _searchPort
            .Setup(s => s.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);

        var result = await _sut.SearchByTypeAsync("nohits", "Lead");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task SearchByTypeAsync_ShouldReturnErrorJson_WhenSearchPortThrows()
    {
        _searchPort
            .Setup(s => s.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Provider offline"));

        var result = await _sut.SearchByTypeAsync("query", "Account");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        result.Should().Contain("Provider offline");
    }

    [Fact]
    public async Task SearchByTypeAsync_ShouldIncludeHitFieldsInOutput()
    {
        var searchResult = new SearchResult
        {
            Query = "delta",
            TotalCount = 1,
            ProcessingTimeMs = 8,
            Hits = new List<SearchHit>
            {
                new() { Id = "42", EntityType = "Opportunity", Title = "Delta Deal", Description = "Big deal", Score = 0.90 }
            }
        };
        _searchPort
            .Setup(s => s.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult);

        var result = await _sut.SearchByTypeAsync("delta", "Opportunity");

        result.Should().Contain("Delta Deal");
        result.Should().Contain("Opportunity");
        result.Should().Contain("delta");
    }
}
