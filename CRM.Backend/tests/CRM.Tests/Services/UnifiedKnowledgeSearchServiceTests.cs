// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.ITSM;
using CRM.Core.Ports.Input;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Services;
using CRM.Infrastructure.Services.Knowledge;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using GeneralKb = CRM.Core.Entities.KnowledgeBase;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for <see cref="UnifiedKnowledgeSearchService"/>.
/// KB-011 / KB-015.
/// </summary>
public class UnifiedKnowledgeSearchServiceTests : ServiceTestFixtureBase<UnifiedKnowledgeSearchService>
{
    private readonly UnifiedKnowledgeSearchService _service;

    // KB-015: mock search port used in Meilisearch-enabled tests.
    private readonly Mock<ISearchPort> _mockSearchPort;
    private readonly UnifiedKnowledgeSearchService _meilisearchService;

    public UnifiedKnowledgeSearchServiceTests()
    {
        // EF Core path (no external search port)
        _service = new UnifiedKnowledgeSearchService(MockContext.Object, MockLogger.Object);

        // KB-015: Meilisearch path — provider name is "Meilisearch" (non-BuiltIn)
        _mockSearchPort = new Mock<ISearchPort>();
        _mockSearchPort.Setup(p => p.ProviderName).Returns("Meilisearch");
        _meilisearchService = new UnifiedKnowledgeSearchService(
            MockContext.Object,
            MockLogger.Object,
            _mockSearchPort.Object);
    }

    // =========================================================================
    // SearchAsync — empty / null guard
    // =========================================================================

    [Fact]
    public async Task SearchAsync_ShouldReturnEmpty_WhenQueryIsEmpty()
    {
        // Act
        var results = await _service.SearchAsync(string.Empty);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnEmpty_WhenQueryIsWhitespace()
    {
        // Act
        var results = await _service.SearchAsync("   ");

        // Assert
        results.Should().BeEmpty();
    }

    // =========================================================================
    // SearchAsync — KnowledgeSource.General only
    // =========================================================================

    [Fact]
    public async Task SearchAsync_ShouldOnlySearchGeneralKb_WhenSourceIsGeneral()
    {
        // Arrange
        var generalArticles = new List<GeneralKb.KnowledgeArticle>
        {
            CreateGeneralArticle(1, "Password Reset Guide", "Reset your password easily"),
        };
        var itsmArticles = new List<KnowledgeArticle>
        {
            CreateItsmArticle(1, "Password Troubleshooting", "ITSM password guide"),
        };

        MockContext.Setup(c => c.KnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(generalArticles).Object);
        MockContext.Setup(c => c.ITSMKnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(itsmArticles).Object);

        // Act
        var results = (await _service.SearchAsync("password", source: KnowledgeSource.General)).ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].Source.Should().Be(KnowledgeSource.General);
        results[0].Title.Should().Be("Password Reset Guide");
    }

    // =========================================================================
    // SearchAsync — KnowledgeSource.ITSM only
    // =========================================================================

    [Fact]
    public async Task SearchAsync_ShouldOnlySearchItsmKb_WhenSourceIsITSM()
    {
        // Arrange
        var generalArticles = new List<GeneralKb.KnowledgeArticle>
        {
            CreateGeneralArticle(1, "VPN Setup Guide General", "General VPN setup"),
        };
        var itsmArticles = new List<KnowledgeArticle>
        {
            CreateItsmArticle(5, "VPN Troubleshooting", "ITSM VPN diagnosis"),
        };

        MockContext.Setup(c => c.KnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(generalArticles).Object);
        MockContext.Setup(c => c.ITSMKnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(itsmArticles).Object);

        // Act
        var results = (await _service.SearchAsync("VPN", source: KnowledgeSource.ITSM)).ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].Source.Should().Be(KnowledgeSource.ITSM);
        results[0].Title.Should().Be("VPN Troubleshooting");
    }

    // =========================================================================
    // SearchAsync — KnowledgeSource.All (merged results)
    // =========================================================================

    [Fact]
    public async Task SearchAsync_ShouldMergeResults_WhenSourceIsAll()
    {
        // Arrange
        var generalArticles = new List<GeneralKb.KnowledgeArticle>
        {
            CreateGeneralArticle(1, "Password FAQ", "Frequently asked password questions"),
        };
        var itsmArticles = new List<KnowledgeArticle>
        {
            CreateItsmArticle(1, "Password Reset ITSM", "ITSM password reset procedure"),
        };

        MockContext.Setup(c => c.KnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(generalArticles).Object);
        MockContext.Setup(c => c.ITSMKnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(itsmArticles).Object);

        // Act
        var results = (await _service.SearchAsync("password", source: KnowledgeSource.All)).ToList();

        // Assert
        results.Should().HaveCount(2);
        results.Select(r => r.Source).Should().Contain(KnowledgeSource.General);
        results.Select(r => r.Source).Should().Contain(KnowledgeSource.ITSM);
    }

    // =========================================================================
    // SearchAsync — filter by status
    // =========================================================================

    [Fact]
    public async Task SearchAsync_ShouldExcludeDraftGeneralArticles()
    {
        // Arrange
        var generalArticles = new List<GeneralKb.KnowledgeArticle>
        {
            CreateGeneralArticle(1, "Published VPN Guide", "VPN setup", GeneralKb.ArticleStatus.Published),
            CreateGeneralArticle(2, "Draft VPN Article", "VPN draft", GeneralKb.ArticleStatus.Draft),
        };

        MockContext.Setup(c => c.KnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(generalArticles).Object);
        MockContext.Setup(c => c.ITSMKnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<KnowledgeArticle>()).Object);

        // Act
        var results = (await _service.SearchAsync("VPN", source: KnowledgeSource.General)).ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].Title.Should().Be("Published VPN Guide");
    }

    [Fact]
    public async Task SearchAsync_ShouldExcludeDraftItsmArticles()
    {
        // Arrange
        var itsmArticles = new List<KnowledgeArticle>
        {
            CreateItsmArticle(1, "Published Network Guide", "Network troubleshooting", PublishingState.Published),
            CreateItsmArticle(2, "Draft Network Article", "Network draft", PublishingState.Draft),
        };

        MockContext.Setup(c => c.KnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<GeneralKb.KnowledgeArticle>()).Object);
        MockContext.Setup(c => c.ITSMKnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(itsmArticles).Object);

        // Act
        var results = (await _service.SearchAsync("Network", source: KnowledgeSource.ITSM)).ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].Title.Should().Be("Published Network Guide");
    }

    // =========================================================================
    // SearchAsync — relevance + ordering
    // =========================================================================

    [Fact]
    public async Task SearchAsync_ShouldAssignHigherScore_WhenQueryMatchesTitle()
    {
        // Arrange — summary deliberately does NOT contain the query word so score = 0.7 (title-only)
        var generalArticles = new List<GeneralKb.KnowledgeArticle>
        {
            CreateGeneralArticle(1, "Printer Setup Guide", "Detailed instructions for configuring local output hardware"),
        };

        MockContext.Setup(c => c.KnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(generalArticles).Object);
        MockContext.Setup(c => c.ITSMKnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<KnowledgeArticle>()).Object);

        // Act — query matches title
        var results = (await _service.SearchAsync("Printer", source: KnowledgeSource.General)).ToList();

        // Assert
        results.Should().HaveCount(1);
        // Title match adds 0.7; summary doesn't contain keyword, so score = 0.7
        results[0].RelevanceScore.Should().BeApproximately(0.7, 0.01);
    }

    [Fact]
    public async Task SearchAsync_ShouldRespectMaxResults()
    {
        // Arrange
        var generalArticles = Enumerable.Range(1, 10)
            .Select(i => CreateGeneralArticle(i, $"Article {i} about networking", "Networking guide"))
            .ToList();

        MockContext.Setup(c => c.KnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(generalArticles).Object);
        MockContext.Setup(c => c.ITSMKnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<KnowledgeArticle>()).Object);

        // Act
        var results = (await _service.SearchAsync("networking", maxResults: 3, source: KnowledgeSource.General)).ToList();

        // Assert
        results.Should().HaveCount(3);
    }

    // =========================================================================
    // SearchAsync — DTO field mapping
    // =========================================================================

    [Fact]
    public async Task SearchAsync_ShouldMapSlug_ForGeneralKbArticles()
    {
        // Arrange
        var generalArticles = new List<GeneralKb.KnowledgeArticle>
        {
            CreateGeneralArticle(1, "VPN Guide", "VPN setup guide", slug: "vpn-guide"),
        };

        MockContext.Setup(c => c.KnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(generalArticles).Object);
        MockContext.Setup(c => c.ITSMKnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<KnowledgeArticle>()).Object);

        // Act
        var results = (await _service.SearchAsync("VPN", source: KnowledgeSource.General)).ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].Slug.Should().Be("vpn-guide");
    }

    [Fact]
    public async Task SearchAsync_ShouldHaveEmptySlug_ForItsmKbArticles()
    {
        // Arrange
        var itsmArticles = new List<KnowledgeArticle>
        {
            CreateItsmArticle(1, "ITSM VPN Guide", "ITSM VPN troubleshooting"),
        };

        MockContext.Setup(c => c.KnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<GeneralKb.KnowledgeArticle>()).Object);
        MockContext.Setup(c => c.ITSMKnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(itsmArticles).Object);

        // Act
        var results = (await _service.SearchAsync("ITSM VPN", source: KnowledgeSource.ITSM)).ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].Slug.Should().BeEmpty();
    }

    // =========================================================================
    // Private helpers
    // =========================================================================

    private static GeneralKb.KnowledgeArticle CreateGeneralArticle(
        int id,
        string title,
        string summary,
        GeneralKb.ArticleStatus status = GeneralKb.ArticleStatus.Published,
        string? slug = null)
    {
        return new GeneralKb.KnowledgeArticle
        {
            Id = id,
            Title = title,
            Summary = summary,
            Content = summary,
            Slug = slug ?? title.ToLowerInvariant().Replace(' ', '-'),
            Status = status,
            IsDeleted = false,
            ViewCount = 0,
            CreatedAt = DateTime.UtcNow,
        };
    }

    private static KnowledgeArticle CreateItsmArticle(
        int id,
        string title,
        string shortDescription,
        PublishingState state = PublishingState.Published)
    {
        return new KnowledgeArticle
        {
            ArticleId = id,
            Number = $"KB{id:D5}",
            Title = title,
            ShortDescription = shortDescription,
            ArticleBody = shortDescription,
            PublishingState = state,
            IsDeleted = false,
            ViewCount = 0,
            CreatedAt = DateTime.UtcNow,
        };
    }

    // =========================================================================
    // KB-015 — Meilisearch path: SearchAsync delegates to ISearchPort
    // =========================================================================

    [Fact]
    public async Task SearchAsync_ShouldUseMeilisearch_WhenExternalSearchEnabled()
    {
        // Arrange — Meilisearch returns one KnowledgeIndexDocument hit
        var msDoc = new KnowledgeIndexDocument
        {
            Id = "general-1",
            SourceId = 1,
            Title = "Meilisearch VPN Result",
            Summary = "From Meilisearch",
            Source = 0, // General
            ViewCount = 5,
            Slug = "meilisearch-vpn-result",
            UpdatedAt = DateTime.UtcNow
        };

        _mockSearchPort
            .Setup(p => p.SearchAsync<KnowledgeIndexDocument>(
                It.IsAny<string>(),
                It.IsAny<SearchOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult<KnowledgeIndexDocument>
            {
                Items = new List<KnowledgeIndexDocument> { msDoc },
                TotalCount = 1
            });

        // Act
        var results = (await _meilisearchService.SearchAsync("VPN")).ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].Title.Should().Be("Meilisearch VPN Result");
        results[0].Source.Should().Be(KnowledgeSource.General);
        results[0].Slug.Should().Be("meilisearch-vpn-result");
        results[0].RelevanceScore.Should().Be(1.0); // KB-015: Meilisearch sets score to 1.0

        // Verify Meilisearch was called and EF Core was NOT queried
        _mockSearchPort.Verify(
            p => p.SearchAsync<KnowledgeIndexDocument>(
                "VPN",
                It.IsAny<SearchOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        MockContext.Verify(c => c.KnowledgeArticles, Times.Never);
        MockContext.Verify(c => c.ITSMKnowledgeArticles, Times.Never);
    }

    [Fact]
    public async Task SearchAsync_ShouldPassSourceFilter_WhenSourceIsGeneral_MeilisearchPath()
    {
        // Arrange
        _mockSearchPort
            .Setup(p => p.SearchAsync<KnowledgeIndexDocument>(
                It.IsAny<string>(),
                It.Is<SearchOptions>(o => o.Filters != null && o.Filters["source"] == "0"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult<KnowledgeIndexDocument>
            {
                Items = Enumerable.Empty<KnowledgeIndexDocument>(),
                TotalCount = 0
            });

        // Act
        await _meilisearchService.SearchAsync("test", source: KnowledgeSource.General);

        // Assert — provider was called with source = "0" filter
        _mockSearchPort.Verify(
            p => p.SearchAsync<KnowledgeIndexDocument>(
                "test",
                It.Is<SearchOptions>(o =>
                    o.Filters != null &&
                    o.Filters.ContainsKey("source") &&
                    o.Filters["source"] == "0"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_ShouldPassSourceFilter_WhenSourceIsITSM_MeilisearchPath()
    {
        // Arrange
        _mockSearchPort
            .Setup(p => p.SearchAsync<KnowledgeIndexDocument>(
                It.IsAny<string>(),
                It.IsAny<SearchOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult<KnowledgeIndexDocument>
            {
                Items = Enumerable.Empty<KnowledgeIndexDocument>(),
                TotalCount = 0
            });

        // Act
        await _meilisearchService.SearchAsync("incident", source: KnowledgeSource.ITSM);

        // Assert — provider was called with source = "1" filter
        _mockSearchPort.Verify(
            p => p.SearchAsync<KnowledgeIndexDocument>(
                "incident",
                It.Is<SearchOptions>(o =>
                    o.Filters != null &&
                    o.Filters.ContainsKey("source") &&
                    o.Filters["source"] == "1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_ShouldNotPassSourceFilter_WhenSourceIsAll_MeilisearchPath()
    {
        // Arrange
        _mockSearchPort
            .Setup(p => p.SearchAsync<KnowledgeIndexDocument>(
                It.IsAny<string>(),
                It.IsAny<SearchOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult<KnowledgeIndexDocument>
            {
                Items = Enumerable.Empty<KnowledgeIndexDocument>(),
                TotalCount = 0
            });

        // Act
        await _meilisearchService.SearchAsync("network", source: KnowledgeSource.All);

        // Assert — provider was called WITHOUT a source filter
        _mockSearchPort.Verify(
            p => p.SearchAsync<KnowledgeIndexDocument>(
                "network",
                It.Is<SearchOptions>(o => o.Filters == null || !o.Filters.ContainsKey("source")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_ShouldFallBackToEfCore_WhenSearchPortIsBuiltIn()
    {
        // Arrange — provider name is "BuiltIn" — should trigger EF Core path
        var builtInMock = new Mock<ISearchPort>();
        builtInMock.Setup(p => p.ProviderName).Returns("BuiltIn");

        var service = new UnifiedKnowledgeSearchService(
            MockContext.Object,
            MockLogger.Object,
            builtInMock.Object);

        var generalArticles = new List<GeneralKb.KnowledgeArticle>
        {
            CreateGeneralArticle(1, "BuiltIn Path Result", "Found via EF Core")
        };

        MockContext.Setup(c => c.KnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(generalArticles).Object);
        MockContext.Setup(c => c.ITSMKnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<KnowledgeArticle>()).Object);

        // Act
        var results = (await service.SearchAsync("BuiltIn", source: KnowledgeSource.General)).ToList();

        // Assert — EF Core was queried; Meilisearch was not
        results.Should().HaveCount(1);
        results[0].Title.Should().Be("BuiltIn Path Result");
        builtInMock.Verify(
            p => p.SearchAsync<KnowledgeIndexDocument>(
                It.IsAny<string>(),
                It.IsAny<SearchOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // =========================================================================
    // KB-015 — IndexAllAsync: Meilisearch batch push
    // =========================================================================

    [Fact]
    public async Task IndexAllKnowledgeAsync_ShouldIndexBothSources_WhenMeilisearchIsActive()
    {
        // Arrange
        var generalArticles = new List<GeneralKb.KnowledgeArticle>
        {
            CreateGeneralArticle(10, "General Article A", "Summary A"),
            CreateGeneralArticle(11, "General Article B", "Summary B"),
        };
        var itsmArticles = new List<KnowledgeArticle>
        {
            CreateItsmArticle(20, "ITSM Article X", "ITSM short A"),
        };

        MockContext.Setup(c => c.KnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(generalArticles).Object);
        MockContext.Setup(c => c.ITSMKnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(itsmArticles).Object);

        List<KnowledgeIndexDocument>? capturedDocs = null;
        _mockSearchPort
            .Setup(p => p.IndexBatchAsync(
                It.IsAny<IEnumerable<KnowledgeIndexDocument>>(),
                It.IsAny<Func<KnowledgeIndexDocument, string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<KnowledgeIndexDocument>, Func<KnowledgeIndexDocument, string>, CancellationToken>(
                (docs, _, __) => capturedDocs = docs.ToList())
            .Returns(Task.CompletedTask);

        // Act
        await _meilisearchService.IndexAllAsync(CancellationToken.None);

        // Assert — IndexBatchAsync was called once with 3 documents
        _mockSearchPort.Verify(
            p => p.IndexBatchAsync(
                It.IsAny<IEnumerable<KnowledgeIndexDocument>>(),
                It.IsAny<Func<KnowledgeIndexDocument, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        capturedDocs.Should().NotBeNull();
        capturedDocs!.Should().HaveCount(3);
        capturedDocs.Where(d => d.Source == 0).Should().HaveCount(2, "two General KB articles");
        capturedDocs.Where(d => d.Source == 1).Should().HaveCount(1, "one ITSM KB article");
    }

    [Fact]
    public async Task IndexAllKnowledgeAsync_ShouldNotCallMeilisearch_WhenNoSearchPortInjected()
    {
        // Arrange — service without any search port (pure EF Core)
        var serviceWithoutPort = new UnifiedKnowledgeSearchService(
            MockContext.Object,
            MockLogger.Object);

        MockContext.Setup(c => c.KnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<GeneralKb.KnowledgeArticle>()).Object);
        MockContext.Setup(c => c.ITSMKnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<KnowledgeArticle>()).Object);

        // Act & Assert — should complete without throwing, no search port calls
        await serviceWithoutPort.Invoking(s => s.IndexAllAsync()).Should().NotThrowAsync();
        _mockSearchPort.Verify(
            p => p.IndexBatchAsync(
                It.IsAny<IEnumerable<KnowledgeIndexDocument>>(),
                It.IsAny<Func<KnowledgeIndexDocument, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task IndexAllKnowledgeAsync_DocumentIds_ShouldUseCompositeFormat()
    {
        // Arrange
        var generalArticles = new List<GeneralKb.KnowledgeArticle>
        {
            CreateGeneralArticle(5, "General Doc", "Summary")
        };
        var itsmArticles = new List<KnowledgeArticle>
        {
            CreateItsmArticle(7, "ITSM Doc", "Body")
        };

        MockContext.Setup(c => c.KnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(generalArticles).Object);
        MockContext.Setup(c => c.ITSMKnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(itsmArticles).Object);

        List<KnowledgeIndexDocument>? capturedDocs = null;
        _mockSearchPort
            .Setup(p => p.IndexBatchAsync(
                It.IsAny<IEnumerable<KnowledgeIndexDocument>>(),
                It.IsAny<Func<KnowledgeIndexDocument, string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<KnowledgeIndexDocument>, Func<KnowledgeIndexDocument, string>, CancellationToken>(
                (docs, _, __) => capturedDocs = docs.ToList())
            .Returns(Task.CompletedTask);

        // Act
        await _meilisearchService.IndexAllAsync(CancellationToken.None);

        // Assert — IDs follow the composite format
        capturedDocs.Should().NotBeNull();
        capturedDocs!.Should().ContainSingle(d => d.Id == "general-5");
        capturedDocs!.Should().ContainSingle(d => d.Id == "itsm-7");
    }

    [Fact]
    public async Task SearchAsync_ShouldMapItsmDocumentSource_Correctly_MeilisearchPath()
    {
        // Arrange — Meilisearch returns an ITSM document (source = 1)
        var itsmDoc = new KnowledgeIndexDocument
        {
            Id = "itsm-42",
            SourceId = 42,
            Title = "Network Outage Procedure",
            Summary = "Steps to handle outage",
            Source = 1, // ITSM
            ViewCount = 12,
            Slug = string.Empty,
            UpdatedAt = DateTime.UtcNow
        };

        _mockSearchPort
            .Setup(p => p.SearchAsync<KnowledgeIndexDocument>(
                It.IsAny<string>(),
                It.IsAny<SearchOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult<KnowledgeIndexDocument>
            {
                Items = new List<KnowledgeIndexDocument> { itsmDoc },
                TotalCount = 1
            });

        // Act
        var results = (await _meilisearchService.SearchAsync("outage")).ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].Id.Should().Be(42);
        results[0].Source.Should().Be(KnowledgeSource.ITSM);
        results[0].Slug.Should().BeEmpty();
        results[0].ViewCount.Should().Be(12);
    }
}
