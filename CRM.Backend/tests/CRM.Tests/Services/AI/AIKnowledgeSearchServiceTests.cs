// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Services.AI;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;
using Xunit;
// KB-014: alias to avoid name clash with ITSM KnowledgeArticle in the same test class
using GeneralKbArticle = CRM.Core.Entities.KnowledgeBase.KnowledgeArticle;
using GeneralKbArticleStatus = CRM.Core.Entities.KnowledgeBase.ArticleStatus;

namespace CRM.Tests.Services.AI;

public class AIKnowledgeSearchServiceTests : ServiceTestFixtureBase<AIKnowledgeSearchService>
{
    private readonly Mock<IFeatureManager> _mockFeatureManager;
    private readonly Mock<IServiceProvider> _mockServiceProvider;    private readonly AIKnowledgeSearchService _service;

    public AIKnowledgeSearchServiceTests()
    {
        _mockFeatureManager = new Mock<IFeatureManager>();
        _mockServiceProvider = new Mock<IServiceProvider>();

        // Default: AI not available (feature flag off)
        _mockFeatureManager.Setup(f => f.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(false);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IFeatureManager))).Returns(_mockFeatureManager.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IAIPort))).Returns((IAIPort?)null);

        // KB-014: default empty DbSets so all service code paths have valid mocks;
        // individual tests override these as needed.
        MockContext.Setup(c => c.ITSMKnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<KnowledgeArticle>()).Object);
        MockContext.Setup(c => c.KnowledgeArticles)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<GeneralKbArticle>()).Object);

        _service = new AIKnowledgeSearchService(
            _mockServiceProvider.Object,
            MockContext.Object,
            _mockFeatureManager.Object,
            MockLogger.Object);
    }

    // ========================================================================
    // SemanticSearchAsync - Keyword Fallback (AI unavailable)
    // ========================================================================

    [Fact]
    public async Task SemanticSearchAsync_ShouldFallbackToKeywordSearch_WhenAIUnavailable()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            CreateArticle(1, "Password Reset Guide", "How to reset your password"),
            CreateArticle(2, "VPN Setup Instructions", "Configure VPN for remote access"),
            CreateArticle(3, "Printer Setup", "How to add a network printer")
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(articles);
        MockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act
        var results = await _service.SemanticSearchAsync("password", 10);

        // Assert
        results.Should().HaveCount(1);
        results.First().Title.Should().Be("Password Reset Guide");
        results.First().RelevanceScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SemanticSearchAsync_ShouldReturnEmpty_WhenNoMatchFound()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            CreateArticle(1, "Password Reset Guide", "How to reset your password")
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(articles);
        MockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act
        var results = await _service.SemanticSearchAsync("kubernetes deployment", 10);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SemanticSearchAsync_ShouldOnlyReturnPublishedArticles()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            CreateArticle(1, "Published VPN Guide", "VPN setup", PublishingState.Published),
            CreateArticle(2, "Draft VPN Article", "VPN draft", PublishingState.Draft)
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(articles);
        MockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act
        var results = await _service.SemanticSearchAsync("VPN", 10);

        // Assert
        results.Should().HaveCount(1);
        results.First().Title.Should().Be("Published VPN Guide");
    }

    [Fact]
    public async Task SemanticSearchAsync_ShouldRespectTopKLimit()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            CreateArticle(1, "Network Issue 1", "network problem one"),
            CreateArticle(2, "Network Issue 2", "network problem two"),
            CreateArticle(3, "Network Issue 3", "network problem three")
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(articles);
        MockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act
        var results = await _service.SemanticSearchAsync("network", 2);

        // Assert
        results.Should().HaveCountLessOrEqualTo(2);
    }

    // ========================================================================
    // IndexArticleAsync
    // ========================================================================

    [Fact]
    public async Task IndexArticleAsync_ShouldComplete_WhenArticleExists()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            CreateArticle(1, "Test Article", "Test body content")
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(articles);
        MockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act
        var ex = await Record.ExceptionAsync(() => _service.IndexArticleAsync(1));

        // Assert
        ex.Should().BeNull();
    }

    [Fact]
    public async Task IndexArticleAsync_ShouldComplete_WhenArticleNotFound()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(articles);
        MockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act
        var ex = await Record.ExceptionAsync(() => _service.IndexArticleAsync(999));

        // Assert
        ex.Should().BeNull();
    }

    // ========================================================================
    // ReindexAllAsync
    // ========================================================================

    [Fact]
    public async Task ReindexAllAsync_ShouldProcessPublishedArticles()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            CreateArticle(1, "Article A", "Body A", PublishingState.Published),
            CreateArticle(2, "Article B", "Body B", PublishingState.Published),
            CreateArticle(3, "Draft Article", "Body C", PublishingState.Draft)
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(articles);
        MockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act
        var ex = await Record.ExceptionAsync(() => _service.ReindexAllAsync());

        // Assert
        ex.Should().BeNull();
    }

    [Fact]
    public async Task ReindexAllAsync_ShouldHandleNoPublishedArticles()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            CreateArticle(1, "Draft", "Body", PublishingState.Draft)
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(articles);
        MockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act
        var ex = await Record.ExceptionAsync(() => _service.ReindexAllAsync());

        // Assert
        ex.Should().BeNull();
    }

    // ========================================================================
    // Semantic Search with AI Provider
    // ========================================================================

    [Fact]
    public async Task SemanticSearchAsync_ShouldUseAI_WhenProviderAvailable()
    {
        // Arrange - enable AI
        var mockAIPort = new Mock<IAIPort>();
        _mockFeatureManager.Setup(f => f.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(true);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IAIPort))).Returns(mockAIPort.Object);

        mockAIPort.Setup(a => a.IsAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        mockAIPort.Setup(a => a.GetEmbeddingAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AIEmbeddingResponse { Embedding = CreateMockEmbedding(384) });

        var articles = new List<KnowledgeArticle>
        {
            CreateArticle(1, "Server Troubleshooting", "How to debug server issues")
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(articles);
        MockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // First index the article
        await _service.ReindexAllAsync();

        // Act
        var results = await _service.SemanticSearchAsync("server problems", 10);

        // Assert - AI path should be attempted
        mockAIPort.Verify(a => a.GetEmbeddingAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    // ========================================================================
    // KB-014: General KB + combined source tests
    // ========================================================================

    [Fact]
    public async Task SemanticSearchAsync_ShouldReturnGeneralKBResults_WhenQueryMatchesGeneralKBOnly()
    {
        // Arrange — ITSM has no matches, General KB has one
        var itsmSet = MockDbSetFactory.CreateMockDbSet(new List<KnowledgeArticle>
        {
            CreateArticle(1, "Unrelated ITSM Article", "irrelevant content")
        });
        MockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(itsmSet.Object);

        var generalSet = MockDbSetFactory.CreateMockDbSet(new List<GeneralKbArticle>
        {
            CreateGeneralArticle(1, "Python Best Practices", "Write clean Python code with best practices")
        });
        MockContext.Setup(c => c.KnowledgeArticles).Returns(generalSet.Object);

        // Act
        var results = await _service.SemanticSearchAsync("Python", 10);

        // Assert
        results.Should().HaveCount(1);
        results.First().Title.Should().Be("Python Best Practices");
        results.First().ArticleId.Should().Be(1 + 100_000); // KB-014: General KB offset
    }

    [Fact]
    public async Task SemanticSearchAsync_ShouldReturnResultsFromBothSources_WhenBothHaveMatches()
    {
        // Arrange — both ITSM and General KB have matching articles
        var itsmSet = MockDbSetFactory.CreateMockDbSet(new List<KnowledgeArticle>
        {
            CreateArticle(1, "ITSM Network Policy", "Network configuration network guide")
        });
        MockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(itsmSet.Object);

        var generalSet = MockDbSetFactory.CreateMockDbSet(new List<GeneralKbArticle>
        {
            CreateGeneralArticle(2, "General Network Setup", "How to set up network connections")
        });
        MockContext.Setup(c => c.KnowledgeArticles).Returns(generalSet.Object);

        // Act
        var results = await _service.SemanticSearchAsync("network", 10);

        // Assert — one ITSM + one General KB result
        results.Should().HaveCount(2);
        results.Should().Contain(r => r.ArticleId == 1);           // ITSM article
        results.Should().Contain(r => r.ArticleId == 2 + 100_000); // General KB article
    }

    [Fact]
    public async Task SemanticSearchAsync_ShouldOnlyReturnPublishedGeneralKBArticles_WhenDraftExists()
    {
        // Arrange
        var itsmSet = MockDbSetFactory.CreateMockDbSet(new List<KnowledgeArticle>());
        MockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(itsmSet.Object);

        var generalSet = MockDbSetFactory.CreateMockDbSet(new List<GeneralKbArticle>
        {
            CreateGeneralArticle(1, "Published Docker Guide", "Docker deployment docker steps", GeneralKbArticleStatus.Published),
            CreateGeneralArticle(2, "Draft Docker Notes",   "Docker notes draft",             GeneralKbArticleStatus.Draft)
        });
        MockContext.Setup(c => c.KnowledgeArticles).Returns(generalSet.Object);

        // Act
        var results = await _service.SemanticSearchAsync("Docker", 10);

        // Assert — only the Published article is returned
        results.Should().HaveCount(1);
        results.First().Title.Should().Be("Published Docker Guide");
    }

    [Fact]
    public async Task SemanticSearchAsync_ShouldReturnEmpty_WhenNeitherITSMNorGeneralKBMatch()
    {
        // Arrange
        var itsmSet = MockDbSetFactory.CreateMockDbSet(new List<KnowledgeArticle>
        {
            CreateArticle(1, "Password Reset Guide", "Reset your password")
        });
        MockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(itsmSet.Object);

        var generalSet = MockDbSetFactory.CreateMockDbSet(new List<GeneralKbArticle>
        {
            CreateGeneralArticle(1, "Onboarding Guide", "Welcome to the company")
        });
        MockContext.Setup(c => c.KnowledgeArticles).Returns(generalSet.Object);

        // Act
        var results = await _service.SemanticSearchAsync("kubernetes helm chart", 10);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SemanticSearchAsync_ShouldRespectTopKLimit_AcrossBothSources()
    {
        // Arrange — 3 from ITSM + 3 from General KB, limit is 4
        var itsmSet = MockDbSetFactory.CreateMockDbSet(new List<KnowledgeArticle>
        {
            CreateArticle(1, "ITSM API Guide 1", "api rest endpoint doc"),
            CreateArticle(2, "ITSM API Guide 2", "api rest endpoint guide"),
            CreateArticle(3, "ITSM API Guide 3", "api rest endpoint reference")
        });
        MockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(itsmSet.Object);

        var generalSet = MockDbSetFactory.CreateMockDbSet(new List<GeneralKbArticle>
        {
            CreateGeneralArticle(1, "General API Guide 1", "api integration guide one"),
            CreateGeneralArticle(2, "General API Guide 2", "api integration guide two"),
            CreateGeneralArticle(3, "General API Guide 3", "api integration guide three")
        });
        MockContext.Setup(c => c.KnowledgeArticles).Returns(generalSet.Object);

        // Act
        var results = await _service.SemanticSearchAsync("api", 4);

        // Assert — never exceeds the requested limit
        results.Should().HaveCountLessOrEqualTo(4);
    }

    // ========================================================================
    // Helpers
    // ========================================================================

    private static KnowledgeArticle CreateArticle(
        int id,
        string title,
        string body,
        PublishingState state = PublishingState.Published)
    {
        return new KnowledgeArticle
        {
            ArticleId = id,
            Number = $"KB{id:D5}",
            Title = title,
            ArticleBody = body,
            ShortDescription = title,
            PublishingState = state,
            IsDeleted = false,
            Version = 1,
            ViewCount = 0,
            HelpfulCount = 0
        };
    }

    // KB-014: factory for General KB articles
    private static GeneralKbArticle CreateGeneralArticle(
        int id,
        string title,
        string content,
        GeneralKbArticleStatus status = GeneralKbArticleStatus.Published)
    {
        return new GeneralKbArticle
        {
            Id = id,
            Title = title,
            Content = content,
            Summary = title,
            Status = status,
            IsDeleted = false
        };
    }

    private static float[] CreateMockEmbedding(int dims)
    {
        var rng = new Random(42);
        return Enumerable.Range(0, dims).Select(_ => (float)rng.NextDouble()).ToArray();
    }
}
