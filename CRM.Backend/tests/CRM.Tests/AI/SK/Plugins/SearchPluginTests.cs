// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.AI.SK.Plugins;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.AI.SK.Plugins;

#nullable enable

/// <summary>
/// Unit tests for <see cref="SearchPlugin"/>.
/// Validates global search and entity-type-scoped search kernel functions.
/// Note: SearchPlugin depends on ISearchPort only — no ICrmDbContext.
/// </summary>
public class SearchPluginTests
{
    #region Fields & Setup

    private readonly Mock<ISearchPort> _searchPortMock = new();
    private readonly Mock<ILogger<SearchPlugin>> _loggerMock = new();
    private readonly SearchPlugin _plugin;

    public SearchPluginTests()
    {
        _plugin = new SearchPlugin(_searchPortMock.Object, _loggerMock.Object);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void PluginName_ShouldReturnSearch()
    {
        _plugin.PluginName.Should().Be("Search");
    }

    [Fact]
    public void Description_ShouldNotBeEmpty()
    {
        _plugin.Description.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Constructor Validation

    [Fact]
    public void Constructor_NullSearchPort_ShouldThrow()
    {
        var act = () => new SearchPlugin(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var act = () => new SearchPlugin(_searchPortMock.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region GlobalSearchAsync Tests

    [Fact]
    public async Task GlobalSearchAsync_ValidQuery_ShouldReturnResults()
    {
        // Arrange
        var searchResult = new SearchResult
        {
            Query = "test",
            TotalCount = 2,
            ProcessingTimeMs = 15,
            Hits = new List<SearchHit>
            {
                new() { EntityType = "Account", Id = "1", Title = "Acme Corp", Score = 0.95 },
                new() { EntityType = "Contact", Id = "2", Title = "John Doe", Score = 0.85 }
            }
        };
        _searchPortMock.Setup(s => s.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult);

        // Act
        var result = await _plugin.GlobalSearchAsync("test");

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GlobalSearchAsync_EmptyResults_ShouldReturnSuccessWithEmptyData()
    {
        // Arrange
        var searchResult = new SearchResult
        {
            Query = "nonexistent",
            TotalCount = 0,
            Hits = new List<SearchHit>()
        };
        _searchPortMock.Setup(s => s.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult);

        // Act
        var result = await _plugin.GlobalSearchAsync("nonexistent");

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GlobalSearchAsync_DefaultMaxResults_ShouldBe20()
    {
        // Arrange
        SearchRequest? capturedRequest = null;
        _searchPortMock.Setup(s => s.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .Callback<SearchRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new SearchResult { Query = "test", TotalCount = 0, Hits = new List<SearchHit>() });

        // Act
        await _plugin.GlobalSearchAsync("test");

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Take.Should().Be(20);
    }

    #endregion

    #region SearchByTypeAsync Tests

    [Fact]
    public async Task SearchByTypeAsync_ValidQuery_ShouldReturnFilteredResults()
    {
        // Arrange
        var searchResult = new SearchResult
        {
            Query = "test",
            TotalCount = 1,
            Hits = new List<SearchHit>
            {
                new() { EntityType = "Account", Id = "1", Title = "Acme Corp" }
            }
        };
        _searchPortMock.Setup(s => s.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult);

        // Act
        var result = await _plugin.SearchByTypeAsync("test", "Account");

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task GlobalSearchAsync_SearchPortThrows_ShouldReturnError()
    {
        // Arrange
        _searchPortMock.Setup(s => s.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Search service unavailable"));

        // Act
        var result = await _plugin.GlobalSearchAsync("test");

        // Assert
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion
}
