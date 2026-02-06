// This file is part of the CRM Solution.
// Tests for KnowledgeManagementService - ITSM knowledge base management

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.ITSM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace CRM.Tests.Services.ITSM;

public class KnowledgeManagementServiceTests
{
    private readonly Mock<IDbContextResolver> _mockContextResolver;
    private readonly Mock<ILogger<KnowledgeManagementService>> _mockLogger;
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly KnowledgeManagementService _service;

    public KnowledgeManagementServiceTests()
    {
        _mockContextResolver = new Mock<IDbContextResolver>();
        _mockLogger = new Mock<ILogger<KnowledgeManagementService>>();
        _mockContext = new Mock<ICrmDbContext>();
        
        _mockContextResolver.Setup(x => x.ResolveContext()).Returns(_mockContext.Object);
        
        _service = new KnowledgeManagementService(
            _mockContextResolver.Object,
            _mockLogger.Object);
    }

    #region CreateArticleAsync Tests

    [Fact]
    public async Task CreateArticleAsync_CreatesArticleWithCorrectData()
    {
        // Arrange
        var dto = new CreateKnowledgeArticleDto
        {
            Title = "How to reset password",
            ShortDescription = "Steps to reset your password",
            ArticleBody = "1. Go to login page...",
            ArticleType = "How-To",
            CategoryId = 1,
            IsInternal = false
        };
        
        var articles = new List<KnowledgeArticle>();
        var mockSet = CreateMockDbSet(articles);
        KnowledgeArticle? capturedArticle = null;
        mockSet.Setup(m => m.Add(It.IsAny<KnowledgeArticle>()))
            .Callback<KnowledgeArticle>(a => capturedArticle = a);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.CreateArticleAsync(dto, createdById: 100);

        // Assert
        capturedArticle.Should().NotBeNull();
        capturedArticle!.Title.Should().Be("How to reset password");
        capturedArticle.ShortDescription.Should().Be("Steps to reset your password");
        capturedArticle.ArticleType.Should().Be("How-To");
        capturedArticle.PublishingState.Should().Be(PublishingState.Draft);
        capturedArticle.AuthorId.Should().Be(100);
        capturedArticle.IsInternal.Should().BeFalse();
        capturedArticle.IsExternal.Should().BeTrue();
    }

    [Fact]
    public async Task CreateArticleAsync_GeneratesArticleNumber()
    {
        // Arrange
        var dto = new CreateKnowledgeArticleDto { Title = "Test Article" };
        var articles = new List<KnowledgeArticle>
        {
            new KnowledgeArticle { ArticleId = 5, Number = "KB0000005" }
        };
        var mockSet = CreateMockDbSet(articles);
        KnowledgeArticle? capturedArticle = null;
        mockSet.Setup(m => m.Add(It.IsAny<KnowledgeArticle>()))
            .Callback<KnowledgeArticle>(a => capturedArticle = a);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _service.CreateArticleAsync(dto, createdById: 1);

        // Assert
        capturedArticle.Should().NotBeNull();
        capturedArticle!.Number.Should().Be("KB0000006");
    }

    [Fact]
    public async Task CreateArticleAsync_LogsCreation()
    {
        // Arrange
        var dto = new CreateKnowledgeArticleDto { Title = "Test" };
        var articles = new List<KnowledgeArticle>();
        var mockSet = CreateMockDbSet(articles);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _service.CreateArticleAsync(dto, createdById: 1);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created knowledge article")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateArticleAsync_SetsInternalToTrue_WhenSpecified()
    {
        // Arrange
        var dto = new CreateKnowledgeArticleDto
        {
            Title = "Internal Procedure",
            IsInternal = true
        };
        var articles = new List<KnowledgeArticle>();
        var mockSet = CreateMockDbSet(articles);
        KnowledgeArticle? capturedArticle = null;
        mockSet.Setup(m => m.Add(It.IsAny<KnowledgeArticle>()))
            .Callback<KnowledgeArticle>(a => capturedArticle = a);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _service.CreateArticleAsync(dto, createdById: 1);

        // Assert
        capturedArticle!.IsInternal.Should().BeTrue();
        capturedArticle.IsExternal.Should().BeFalse();
    }

    #endregion

    #region GetArticleByIdAsync Tests

    [Fact]
    public async Task GetArticleByIdAsync_WhenExists_ReturnsArticle()
    {
        // Arrange
        var author = new User { Id = 100, Username = "author1" };
        var articles = new List<KnowledgeArticle>
        {
            new KnowledgeArticle 
            { 
                ArticleId = 1, 
                Number = "KB0000001",
                Title = "Password Reset Guide",
                ShortDescription = "How to reset passwords",
                ArticleBody = "Follow these steps...",
                ViewCount = 100,
                AuthorId = 100,
                Author = author,
                PublishingState = PublishingState.Published
            }
        };

        var mockSet = CreateMockDbSet(articles);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.GetArticleByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Password Reset Guide");
        result.AuthorName.Should().Be("author1");
    }

    [Fact]
    public async Task GetArticleByIdAsync_IncrementsViewCount()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            new KnowledgeArticle { ArticleId = 1, Title = "Test", ViewCount = 50 }
        };

        var mockSet = CreateMockDbSet(articles);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _service.GetArticleByIdAsync(1);

        // Assert
        articles[0].ViewCount.Should().Be(51);
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetArticleByIdAsync_WhenDeleted_ReturnsNull()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            new KnowledgeArticle { ArticleId = 1, IsDeleted = true }
        };

        var mockSet = CreateMockDbSet(articles);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act
        var result = await _service.GetArticleByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetArticleByIdAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>();
        var mockSet = CreateMockDbSet(articles);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act
        var result = await _service.GetArticleByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region SearchArticlesAsync Tests

    [Fact]
    public async Task SearchArticlesAsync_SearchesByTitle()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            new KnowledgeArticle { ArticleId = 1, Title = "Password Reset", PublishingState = PublishingState.Published },
            new KnowledgeArticle { ArticleId = 2, Title = "Email Setup", PublishingState = PublishingState.Published },
            new KnowledgeArticle { ArticleId = 3, Title = "Password Policy", PublishingState = PublishingState.Published }
        };

        var mockSet = CreateMockDbSet(articles);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act
        var result = await _service.SearchArticlesAsync("Password", pageNumber: 1, pageSize: 10);

        // Assert
        result.Should().HaveCount(2);
        result.Select(a => a.Title).Should().Contain("Password Reset", "Password Policy");
    }

    [Fact]
    public async Task SearchArticlesAsync_SearchesByDescription()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            new KnowledgeArticle 
            { 
                ArticleId = 1, 
                Title = "Setup Guide", 
                ShortDescription = "How to configure email settings",
                PublishingState = PublishingState.Published 
            },
            new KnowledgeArticle 
            { 
                ArticleId = 2, 
                Title = "Security Policy", 
                ShortDescription = "Password requirements",
                PublishingState = PublishingState.Published 
            }
        };

        var mockSet = CreateMockDbSet(articles);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act
        var result = await _service.SearchArticlesAsync("email", pageNumber: 1, pageSize: 10);

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Setup Guide");
    }

    [Fact]
    public async Task SearchArticlesAsync_OnlyReturnsPublished()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            new KnowledgeArticle { ArticleId = 1, Title = "Password Guide", PublishingState = PublishingState.Published },
            new KnowledgeArticle { ArticleId = 2, Title = "Password Draft", PublishingState = PublishingState.Draft },
            new KnowledgeArticle { ArticleId = 3, Title = "Password Retired", PublishingState = PublishingState.Retired }
        };

        var mockSet = CreateMockDbSet(articles);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act
        var result = await _service.SearchArticlesAsync("Password", pageNumber: 1, pageSize: 10);

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Password Guide");
    }

    [Fact]
    public async Task SearchArticlesAsync_OrdersByViewCount()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            new KnowledgeArticle { ArticleId = 1, Title = "KB 1", ViewCount = 100, PublishingState = PublishingState.Published },
            new KnowledgeArticle { ArticleId = 2, Title = "KB 2", ViewCount = 500, PublishingState = PublishingState.Published },
            new KnowledgeArticle { ArticleId = 3, Title = "KB 3", ViewCount = 200, PublishingState = PublishingState.Published }
        };

        var mockSet = CreateMockDbSet(articles);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act
        var result = (await _service.SearchArticlesAsync("KB", pageNumber: 1, pageSize: 10)).ToList();

        // Assert
        result[0].ViewCount.Should().Be(500);
        result[1].ViewCount.Should().Be(200);
        result[2].ViewCount.Should().Be(100);
    }

    [Fact]
    public async Task SearchArticlesAsync_SupportsPagination()
    {
        // Arrange
        var articles = Enumerable.Range(1, 20)
            .Select(i => new KnowledgeArticle 
            { 
                ArticleId = i, 
                Title = $"Article {i}",
                ViewCount = 20 - i,
                PublishingState = PublishingState.Published 
            })
            .ToList();

        var mockSet = CreateMockDbSet(articles);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act
        var page2 = await _service.SearchArticlesAsync("Article", pageNumber: 2, pageSize: 5);

        // Assert
        page2.Should().HaveCount(5);
    }

    #endregion

    #region UpdateArticleAsync Tests

    [Fact]
    public async Task UpdateArticleAsync_UpdatesAllFields()
    {
        // Arrange
        var existingArticle = new KnowledgeArticle 
        { 
            ArticleId = 1, 
            Title = "Old Title",
            ShortDescription = "Old Description",
            ArticleBody = "Old body",
            ArticleType = "FAQ"
        };
        var dto = new CreateKnowledgeArticleDto
        {
            Title = "New Title",
            ShortDescription = "New Description",
            ArticleBody = "New body",
            ArticleType = "How-To",
            CategoryId = 5,
            IsInternal = true
        };

        _mockContext.Setup(c => c.ITSMKnowledgeArticles.FindAsync(1)).ReturnsAsync(existingArticle);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.UpdateArticleAsync(1, dto, modifiedById: 50);

        // Assert
        existingArticle.Title.Should().Be("New Title");
        existingArticle.ShortDescription.Should().Be("New Description");
        existingArticle.ArticleBody.Should().Be("New body");
        existingArticle.ArticleType.Should().Be("How-To");
        existingArticle.IsInternal.Should().BeTrue();
        existingArticle.IsExternal.Should().BeFalse();
        existingArticle.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateArticleAsync_WhenNotFound_ThrowsException()
    {
        // Arrange
        _mockContext.Setup(c => c.ITSMKnowledgeArticles.FindAsync(It.IsAny<int>())).ReturnsAsync((KnowledgeArticle?)null);

        // Act & Assert
        var act = () => _service.UpdateArticleAsync(999, new CreateKnowledgeArticleDto(), 50);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateArticleAsync_WhenDeleted_ThrowsException()
    {
        // Arrange
        var article = new KnowledgeArticle { ArticleId = 1, IsDeleted = true };
        _mockContext.Setup(c => c.ITSMKnowledgeArticles.FindAsync(1)).ReturnsAsync(article);

        // Act & Assert
        var act = () => _service.UpdateArticleAsync(1, new CreateKnowledgeArticleDto(), 50);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region PublishArticleAsync Tests

    [Fact]
    public async Task PublishArticleAsync_PublishesArticle()
    {
        // Arrange
        var article = new KnowledgeArticle { ArticleId = 1, PublishingState = PublishingState.Draft };
        _mockContext.Setup(c => c.ITSMKnowledgeArticles.FindAsync(1)).ReturnsAsync(article);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.PublishArticleAsync(1, publisherId: 50);

        // Assert
        result.Should().BeTrue();
        article.PublishingState.Should().Be(PublishingState.Published);
        article.PublishedById.Should().Be(50);
        article.PublishedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task PublishArticleAsync_WhenNotFound_ReturnsFalse()
    {
        // Arrange
        _mockContext.Setup(c => c.ITSMKnowledgeArticles.FindAsync(It.IsAny<int>())).ReturnsAsync((KnowledgeArticle?)null);

        // Act
        var result = await _service.PublishArticleAsync(999, publisherId: 50);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task PublishArticleAsync_LogsPublication()
    {
        // Arrange
        var article = new KnowledgeArticle { ArticleId = 1, Number = "KB0000001", PublishingState = PublishingState.Draft };
        _mockContext.Setup(c => c.ITSMKnowledgeArticles.FindAsync(1)).ReturnsAsync(article);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _service.PublishArticleAsync(1, publisherId: 50);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Published article")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region RetireArticleAsync Tests

    [Fact]
    public async Task RetireArticleAsync_RetiresArticle()
    {
        // Arrange
        var article = new KnowledgeArticle { ArticleId = 1, PublishingState = PublishingState.Published };
        _mockContext.Setup(c => c.ITSMKnowledgeArticles.FindAsync(1)).ReturnsAsync(article);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.RetireArticleAsync(1, modifiedById: 50);

        // Assert
        result.Should().BeTrue();
        article.PublishingState.Should().Be(PublishingState.Retired);
    }

    [Fact]
    public async Task RetireArticleAsync_WhenNotFound_ReturnsFalse()
    {
        // Arrange
        _mockContext.Setup(c => c.ITSMKnowledgeArticles.FindAsync(It.IsAny<int>())).ReturnsAsync((KnowledgeArticle?)null);

        // Act
        var result = await _service.RetireArticleAsync(999, modifiedById: 50);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SubmitFeedbackAsync Tests

    [Fact]
    public async Task SubmitFeedbackAsync_CreatesHelpfulFeedback()
    {
        // Arrange
        var article = new KnowledgeArticle { ArticleId = 1, HelpfulCount = 10, NotHelpfulCount = 2 };
        var feedbacks = new List<ArticleFeedback>();
        var mockFeedbackSet = CreateMockDbSet(feedbacks);
        ArticleFeedback? capturedFeedback = null;
        mockFeedbackSet.Setup(m => m.Add(It.IsAny<ArticleFeedback>()))
            .Callback<ArticleFeedback>(f => capturedFeedback = f);
        
        _mockContext.Setup(c => c.ITSMArticleFeedback).Returns(mockFeedbackSet.Object);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles.FindAsync(1)).ReturnsAsync(article);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.SubmitFeedbackAsync(1, userId: 100, isHelpful: true, comment: "Great article!");

        // Assert
        result.Should().BeTrue();
        capturedFeedback.Should().NotBeNull();
        capturedFeedback!.ArticleId.Should().Be(1);
        capturedFeedback.UserId.Should().Be(100);
        capturedFeedback.IsHelpful.Should().BeTrue();
        capturedFeedback.Comment.Should().Be("Great article!");
        article.HelpfulCount.Should().Be(11);
    }

    [Fact]
    public async Task SubmitFeedbackAsync_CreatesNotHelpfulFeedback()
    {
        // Arrange
        var article = new KnowledgeArticle { ArticleId = 1, HelpfulCount = 10, NotHelpfulCount = 2 };
        var feedbacks = new List<ArticleFeedback>();
        var mockFeedbackSet = CreateMockDbSet(feedbacks);
        
        _mockContext.Setup(c => c.ITSMArticleFeedback).Returns(mockFeedbackSet.Object);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles.FindAsync(1)).ReturnsAsync(article);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.SubmitFeedbackAsync(1, userId: 100, isHelpful: false, comment: null);

        // Assert
        result.Should().BeTrue();
        article.NotHelpfulCount.Should().Be(3);
    }

    [Fact]
    public async Task SubmitFeedbackAsync_AllowsAnonymousFeedback()
    {
        // Arrange
        var feedbacks = new List<ArticleFeedback>();
        var mockFeedbackSet = CreateMockDbSet(feedbacks);
        ArticleFeedback? capturedFeedback = null;
        mockFeedbackSet.Setup(m => m.Add(It.IsAny<ArticleFeedback>()))
            .Callback<ArticleFeedback>(f => capturedFeedback = f);
        
        _mockContext.Setup(c => c.ITSMArticleFeedback).Returns(mockFeedbackSet.Object);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles.FindAsync(1)).ReturnsAsync((KnowledgeArticle?)null);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _service.SubmitFeedbackAsync(1, userId: null, isHelpful: true, comment: null);

        // Assert
        capturedFeedback!.UserId.Should().BeNull();
    }

    #endregion

    #region GetPopularArticlesAsync Tests

    [Fact]
    public async Task GetPopularArticlesAsync_ReturnsTopArticlesByViewCount()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            new KnowledgeArticle { ArticleId = 1, Title = "A", ViewCount = 100, PublishingState = PublishingState.Published },
            new KnowledgeArticle { ArticleId = 2, Title = "B", ViewCount = 500, HelpfulCount = 50, PublishingState = PublishingState.Published },
            new KnowledgeArticle { ArticleId = 3, Title = "C", ViewCount = 300, HelpfulCount = 30, PublishingState = PublishingState.Published },
            new KnowledgeArticle { ArticleId = 4, Title = "D", ViewCount = 200, PublishingState = PublishingState.Published }
        };

        var mockSet = CreateMockDbSet(articles);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act
        var result = (await _service.GetPopularArticlesAsync(3)).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].ViewCount.Should().Be(500);
        result[1].ViewCount.Should().Be(300);
        result[2].ViewCount.Should().Be(200);
    }

    [Fact]
    public async Task GetPopularArticlesAsync_ExcludesDeletedAndUnpublished()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            new KnowledgeArticle { ArticleId = 1, Title = "Published", ViewCount = 100, PublishingState = PublishingState.Published },
            new KnowledgeArticle { ArticleId = 2, Title = "Draft", ViewCount = 500, PublishingState = PublishingState.Draft },
            new KnowledgeArticle { ArticleId = 3, Title = "Deleted", ViewCount = 300, PublishingState = PublishingState.Published, IsDeleted = true }
        };

        var mockSet = CreateMockDbSet(articles);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act
        var result = await _service.GetPopularArticlesAsync(10);

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Published");
    }

    #endregion

    #region GetRecentArticlesAsync Tests

    [Fact]
    public async Task GetRecentArticlesAsync_OrdersByPublishedDate()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>
        {
            new KnowledgeArticle { ArticleId = 1, Title = "Old", PublishedDate = DateTime.UtcNow.AddDays(-10), PublishingState = PublishingState.Published },
            new KnowledgeArticle { ArticleId = 2, Title = "New", PublishedDate = DateTime.UtcNow.AddDays(-1), PublishingState = PublishingState.Published },
            new KnowledgeArticle { ArticleId = 3, Title = "Middle", PublishedDate = DateTime.UtcNow.AddDays(-5), PublishingState = PublishingState.Published }
        };

        var mockSet = CreateMockDbSet(articles);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act
        var result = (await _service.GetRecentArticlesAsync(10)).ToList();

        // Assert
        result[0].Title.Should().Be("New");
        result[1].Title.Should().Be("Middle");
        result[2].Title.Should().Be("Old");
    }

    #endregion

    #region GetCategoriesAsync Tests

    [Fact]
    public async Task GetCategoriesAsync_ReturnsPredefinedCategories()
    {
        // Arrange
        var articles = new List<KnowledgeArticle>();
        var mockSet = CreateMockDbSet(articles);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act
        var result = await _service.GetCategoriesAsync();

        // Assert
        result.Should().Contain("How-To");
        result.Should().Contain("Troubleshooting");
        result.Should().Contain("FAQ");
        result.Should().Contain("Best Practices");
    }

    #endregion

    #region GetSuggestedArticlesAsync Tests

    [Fact]
    public async Task GetSuggestedArticlesAsync_ReturnsTop5Articles()
    {
        // Arrange
        var articles = Enumerable.Range(1, 10)
            .Select(i => new KnowledgeArticle 
            { 
                ArticleId = i, 
                Title = $"Article {i}",
                ViewCount = 100 - i,
                PublishingState = PublishingState.Published 
            })
            .ToList();

        var mockSet = CreateMockDbSet(articles);
        _mockContext.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        // Act
        var result = await _service.GetSuggestedArticlesAsync("my computer is slow");

        // Assert
        result.Should().HaveCount(5);
    }

    #endregion

    #region Helper Methods

    private static Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();
        
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(default))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
        
        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        
        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Expression)
            .Returns(queryable.Expression);
        
        mockSet.As<IQueryable<T>>()
            .Setup(m => m.ElementType)
            .Returns(queryable.ElementType);
        
        mockSet.As<IQueryable<T>>()
            .Setup(m => m.GetEnumerator())
            .Returns(queryable.GetEnumerator());

        return mockSet;
    }

    #endregion
}
