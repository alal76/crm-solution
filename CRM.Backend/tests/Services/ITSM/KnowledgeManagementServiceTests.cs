// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.ITSM;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.ITSMServices.Knowledge;

public class KnowledgeManagementServiceTests
{
    private readonly Mock<IDbContextResolver> _mockResolver;
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<KnowledgeManagementService>> _mockLogger;
    private readonly IKnowledgeManagementService _service;

    public KnowledgeManagementServiceTests()
    {
        _mockResolver = new Mock<IDbContextResolver>();
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<KnowledgeManagementService>>();

        _mockResolver.Setup(r => r.ResolveContext()).Returns(_mockContext.Object);
        _service = new KnowledgeManagementService(_mockResolver.Object, _mockLogger.Object);
    }

    // ========================================================================
    // CreateArticleAsync
    // ========================================================================

    [Fact]
    public async Task CreateArticleAsync_ShouldCreateArticle_WhenValidDtoProvided()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(articles);
        mockSet.Setup(m => m.Add(It.IsAny<KnowledgeArticle>())).Callback<KnowledgeArticle>(e => articles.Add(e));
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new CreateKnowledgeArticleDto
        {
            Title = "How to reset password",
            ArticleBody = "Step 1: Click forgot password...",
            ArticleType = ArticleType.HowTo,
            ShortDescription = "Password reset instructions"
        };

        // Act
        var result = await _service.CreateArticleAsync(dto, authorId: 1);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("How to reset password");
        result.ArticleType.Should().Be(ArticleType.HowTo);
        mockSet.Verify(m => m.Add(It.IsAny<KnowledgeArticle>()), Times.Once);
    }

    [Fact]
    public async Task CreateArticleAsync_ShouldSetDraftState_ByDefault()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(articles);
        mockSet.Setup(m => m.Add(It.IsAny<KnowledgeArticle>())).Callback<KnowledgeArticle>(e => articles.Add(e));
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new CreateKnowledgeArticleDto
        {
            Title = "Draft article",
            ArticleBody = "Content",
            ArticleType = ArticleType.FAQ
        };

        // Act
        var result = await _service.CreateArticleAsync(dto, authorId: 1);

        // Assert
        result.PublishingState.Should().Be(PublishingState.Draft);
    }

    // ========================================================================
    // GetArticleByIdAsync
    // ========================================================================

    [Fact]
    public async Task GetArticleByIdAsync_ShouldReturnArticle_WhenExists()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            new()
            {
                ArticleId = 1, Number = "KB0001", Title = "VPN Setup",
                ArticleBody = "Instructions...", ArticleType = ArticleType.HowTo,
                PublishingState = PublishingState.Published, AuthorId = 1,
                ViewCount = 42, CreatedAt = DateTime.UtcNow, IsDeleted = false
            }
        };
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(MockDbSetFactory.CreateMockDbSet(articles).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.GetArticleByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("VPN Setup");
        result.ViewCount.Should().Be(43);
    }

    [Fact]
    public async Task GetArticleByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(MockDbSetFactory.CreateMockDbSet(new List<KnowledgeArticle>()).Object);

        // Act
        var result = await _service.GetArticleByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // ========================================================================
    // SearchArticlesAsync
    // ========================================================================

    [Fact]
    public async Task SearchArticlesAsync_ShouldReturnMatchingArticles()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            new() { ArticleId = 1, Number = "KB0001", Title = "VPN troubleshooting", ArticleBody = "Steps...", ArticleType = ArticleType.Troubleshooting, PublishingState = PublishingState.Published, AuthorId = 1, CreatedAt = DateTime.UtcNow },
            new() { ArticleId = 2, Number = "KB0002", Title = "Email setup guide", ArticleBody = "Steps...", ArticleType = ArticleType.HowTo, PublishingState = PublishingState.Published, AuthorId = 1, CreatedAt = DateTime.UtcNow },
            new() { ArticleId = 3, Number = "KB0003", Title = "VPN FAQ", ArticleBody = "Common...", ArticleType = ArticleType.FAQ, PublishingState = PublishingState.Published, AuthorId = 1, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(MockDbSetFactory.CreateMockDbSet(articles).Object);

        // Act
        var results = await _service.SearchArticlesAsync("VPN", pageNumber: 1, pageSize: 20);

        // Assert
        results.Should().NotBeNull();
        results.Count().Should().Be(2);
    }

    // ========================================================================
    // UpdateArticleAsync
    // ========================================================================

    [Fact]
    public async Task UpdateArticleAsync_ShouldUpdateFields_WhenArticleExists()
    {
        // Arrange
        var article = new KnowledgeArticle
        {
            ArticleId = 1, Number = "KB0001", Title = "Old title",
            ArticleBody = "Old body", ArticleType = ArticleType.HowTo,
            PublishingState = PublishingState.Draft, AuthorId = 1,
            CreatedAt = DateTime.UtcNow
        };
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(MockDbSetFactory.CreateMockDbSet(new List<KnowledgeArticle> { article }).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new CreateKnowledgeArticleDto
        {
            Title = "Updated title",
            ArticleBody = "Updated body",
            ArticleType = ArticleType.Troubleshooting
        };

        // Act
        var result = await _service.UpdateArticleAsync(1, dto, modifiedById: 2);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Updated title");
    }

    // ========================================================================
    // PublishArticleAsync / RetireArticleAsync
    // ========================================================================

    [Fact]
    public async Task PublishArticleAsync_ShouldSetPublishedState()
    {
        // Arrange
        var article = new KnowledgeArticle
        {
            ArticleId = 1, Number = "KB0001", Title = "Ready to publish",
            ArticleBody = "Content", ArticleType = ArticleType.HowTo,
            PublishingState = PublishingState.Draft, AuthorId = 1,
            CreatedAt = DateTime.UtcNow
        };
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(MockDbSetFactory.CreateMockDbSet(new List<KnowledgeArticle> { article }).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.PublishArticleAsync(1, publishedById: 3);

        // Assert
        result.Should().BeTrue();
        article.PublishingState.Should().Be(PublishingState.Published);
    }

    [Fact]
    public async Task RetireArticleAsync_ShouldSetRetiredState()
    {
        // Arrange
        var article = new KnowledgeArticle
        {
            ArticleId = 1, Number = "KB0001", Title = "Outdated article",
            ArticleBody = "Content", ArticleType = ArticleType.HowTo,
            PublishingState = PublishingState.Published, AuthorId = 1,
            CreatedAt = DateTime.UtcNow
        };
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(MockDbSetFactory.CreateMockDbSet(new List<KnowledgeArticle> { article }).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.RetireArticleAsync(1, modifiedById: 2);

        // Assert
        result.Should().BeTrue();
        article.PublishingState.Should().Be(PublishingState.Retired);
    }

    // ========================================================================
    // SubmitFeedbackAsync
    // ========================================================================

    [Fact]
    public async Task SubmitFeedbackAsync_ShouldRecordFeedback_WhenArticleExists()
    {
        // Arrange
        var article = new KnowledgeArticle
        {
            ArticleId = 1, Number = "KB0001", Title = "Feedback test",
            ArticleBody = "Content", ArticleType = ArticleType.FAQ,
            PublishingState = PublishingState.Published, AuthorId = 1,
            HelpfulCount = 5, NotHelpfulCount = 2,
            CreatedAt = DateTime.UtcNow
        };
        var feedbacks = new List<ArticleFeedback>();
        var mockFeedbackSet = MockDbSetFactory.CreateMockDbSet(feedbacks);
        mockFeedbackSet.Setup(m => m.Add(It.IsAny<ArticleFeedback>())).Callback<ArticleFeedback>(e => feedbacks.Add(e));

        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(MockDbSetFactory.CreateMockDbSet(new List<KnowledgeArticle> { article }).Object);
        _mockContext.Setup(c => c.ITSMArticleFeedback).Returns(mockFeedbackSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.SubmitFeedbackAsync(1, userId: 10, isHelpful: true, comment: "Very helpful!");

        // Assert
        result.Should().BeTrue();
    }

    // ========================================================================
    // GetPopularArticlesAsync / GetRecentArticlesAsync
    // ========================================================================

    [Fact]
    public async Task GetPopularArticlesAsync_ShouldReturnTopArticles_ByViewCount()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            new() { ArticleId = 1, Number = "KB0001", Title = "Popular", ArticleBody = "A", ArticleType = ArticleType.HowTo, PublishingState = PublishingState.Published, AuthorId = 1, ViewCount = 100, CreatedAt = DateTime.UtcNow },
            new() { ArticleId = 2, Number = "KB0002", Title = "Less popular", ArticleBody = "B", ArticleType = ArticleType.FAQ, PublishingState = PublishingState.Published, AuthorId = 1, ViewCount = 10, CreatedAt = DateTime.UtcNow },
            new() { ArticleId = 3, Number = "KB0003", Title = "Most popular", ArticleBody = "C", ArticleType = ArticleType.HowTo, PublishingState = PublishingState.Published, AuthorId = 1, ViewCount = 500, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(MockDbSetFactory.CreateMockDbSet(articles).Object);

        // Act
        var result = await _service.GetPopularArticlesAsync(count: 2);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Title.Should().Be("Most popular");
    }

    [Fact]
    public async Task GetRecentArticlesAsync_ShouldReturnLatestArticles()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            new() { ArticleId = 1, Number = "KB0001", Title = "Old article", ArticleBody = "A", ArticleType = ArticleType.HowTo, PublishingState = PublishingState.Published, AuthorId = 1, CreatedAt = DateTime.UtcNow.AddDays(-30) },
            new() { ArticleId = 2, Number = "KB0002", Title = "New article", ArticleBody = "B", ArticleType = ArticleType.FAQ, PublishingState = PublishingState.Published, AuthorId = 1, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new() { ArticleId = 3, Number = "KB0003", Title = "Newest article", ArticleBody = "C", ArticleType = ArticleType.HowTo, PublishingState = PublishingState.Published, AuthorId = 1, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(MockDbSetFactory.CreateMockDbSet(articles).Object);

        // Act
        var result = await _service.GetRecentArticlesAsync(count: 2);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Title.Should().Be("Newest article");
    }

    // ========================================================================
    // GetSuggestedArticlesAsync
    // ========================================================================

    [Fact]
    public async Task GetSuggestedArticlesAsync_ShouldReturnRelevantArticles()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            new() { ArticleId = 1, Number = "KB0001", Title = "Password reset guide", ArticleBody = "How to reset your password", ArticleType = ArticleType.HowTo, PublishingState = PublishingState.Published, AuthorId = 1, CreatedAt = DateTime.UtcNow },
            new() { ArticleId = 2, Number = "KB0002", Title = "Network connectivity", ArticleBody = "VPN setup", ArticleType = ArticleType.Troubleshooting, PublishingState = PublishingState.Published, AuthorId = 1, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(MockDbSetFactory.CreateMockDbSet(articles).Object);

        // Act
        var result = await _service.GetSuggestedArticlesAsync("I can't reset my password");

        // Assert
        result.Should().NotBeNull();
    }

    // ========================================================================
    // GetCategoriesAsync
    // ========================================================================

    [Fact]
    public async Task GetCategoriesAsync_ShouldReturnCategories()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            new() { ArticleId = 1, Number = "KB0001", Title = "A", ArticleBody = "A", ArticleType = ArticleType.HowTo, PublishingState = PublishingState.Published, AuthorId = 1, CreatedAt = DateTime.UtcNow },
            new() { ArticleId = 2, Number = "KB0002", Title = "B", ArticleBody = "B", ArticleType = ArticleType.FAQ, PublishingState = PublishingState.Published, AuthorId = 1, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(MockDbSetFactory.CreateMockDbSet(articles).Object);

        // Act
        var result = await _service.GetCategoriesAsync();

        // Assert
        result.Should().NotBeNull();
    }
}
