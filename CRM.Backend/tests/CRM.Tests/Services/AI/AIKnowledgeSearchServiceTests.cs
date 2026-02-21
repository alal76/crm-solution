// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.AI;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;
using Xunit;

namespace CRM.Tests.Services.AI;

public class AIKnowledgeSearchServiceTests
{
    private readonly Mock<IDbContextResolver> _mockResolver;
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<IFeatureManager> _mockFeatureManager;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<AIKnowledgeSearchService>> _mockLogger;
    private readonly AIKnowledgeSearchService _service;

    public AIKnowledgeSearchServiceTests()
    {
        _mockResolver = new Mock<IDbContextResolver>();
        _mockContext = new Mock<ICrmDbContext>();
        _mockFeatureManager = new Mock<IFeatureManager>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<AIKnowledgeSearchService>>();

        _mockResolver.Setup(r => r.ResolveContext()).Returns(_mockContext.Object);

        // Default: AI not available (feature flag off)
        _mockFeatureManager.Setup(f => f.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(false);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IFeatureManager))).Returns(_mockFeatureManager.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IAIPort))).Returns((IAIPort?)null);

        _service = new AIKnowledgeSearchService(
            _mockServiceProvider.Object,
            _mockResolver.Object,
            _mockFeatureManager.Object,
            _mockLogger.Object);
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
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

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
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

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
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

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
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

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
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act & Assert - should complete without throwing
        await _service.IndexArticleAsync(1);
    }

    [Fact]
    public async Task IndexArticleAsync_ShouldComplete_WhenArticleNotFound()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(articles);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act & Assert - should complete without throwing even if article not found
        await _service.IndexArticleAsync(999);
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
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act & Assert - should complete without throwing
        await _service.ReindexAllAsync();
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
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act & Assert - should complete without throwing
        await _service.ReindexAllAsync();
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
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // First index the article
        await _service.ReindexAllAsync();

        // Act
        var results = await _service.SemanticSearchAsync("server problems", 10);

        // Assert - AI path should be attempted
        mockAIPort.Verify(a => a.GetEmbeddingAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
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

    private static float[] CreateMockEmbedding(int dims)
    {
        var rng = new Random(42);
        return Enumerable.Range(0, dims).Select(_ => (float)rng.NextDouble()).ToArray();
    }
}
