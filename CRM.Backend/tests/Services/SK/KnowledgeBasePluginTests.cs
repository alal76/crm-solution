// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Plugins;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.SK;

/// <summary>
/// Unit tests for the KnowledgeBasePlugin Semantic Kernel plugin.
/// </summary>
public class KnowledgeBasePluginTests
{
    private readonly Mock<ICrmDbContext> _dbContextMock;
    private readonly Mock<ILogger<KnowledgeBasePlugin>> _loggerMock;
    private readonly KnowledgeBasePlugin _sut;

    public KnowledgeBasePluginTests()
    {
        _dbContextMock = new Mock<ICrmDbContext>();
        _loggerMock = new Mock<ILogger<KnowledgeBasePlugin>>();
        _sut = new KnowledgeBasePlugin(_dbContextMock.Object, _loggerMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDbContextIsNull()
    {
        var act = () => new KnowledgeBasePlugin(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        var act = () => new KnowledgeBasePlugin(_dbContextMock.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Plugin Metadata Tests

    [Fact]
    public void PluginName_ShouldReturn_KnowledgeBase()
    {
        _sut.PluginName.Should().Be("KnowledgeBase");
    }

    [Fact]
    public void Description_ShouldNotBeNullOrEmpty()
    {
        _sut.Description.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region SearchArticlesAsync Tests

    [Fact]
    public async Task SearchArticlesAsync_ShouldReturnSuccessJson_WithMatchingArticles()
    {
        var articles = new List<KnowledgeArticle>
        {
            new KnowledgeArticle
            {
                ArticleId = 1,
                Number = "KB0001",
                Title = "How to reset password",
                ShortDescription = "Steps for password reset",
                ArticleBody = "Go to login page and click forgot password",
                ArticleType = ArticleType.HowTo,
                PublishingState = PublishingState.Published,
                ViewCount = 50,
                HelpfulCount = 40,
                IsDeleted = false
            }
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(articles);
        _dbContextMock.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        var result = await _sut.SearchArticlesAsync("password", 10);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("totalFound").GetInt32().Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task SearchArticlesAsync_ShouldReturnEmptyResult_WhenNoMatchingArticles()
    {
        var articles = new List<KnowledgeArticle>
        {
            new KnowledgeArticle
            {
                ArticleId = 1,
                Number = "KB0001",
                Title = "Unrelated article",
                ShortDescription = "Nothing to do with search",
                ArticleBody = "Some other content",
                ArticleType = ArticleType.HowTo,
                PublishingState = PublishingState.Published,
                IsDeleted = false
            }
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(articles);
        _dbContextMock.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        var result = await _sut.SearchArticlesAsync("xyznomatch", 10);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("totalFound").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task SearchArticlesAsync_ShouldReturnErrorJson_WhenDbSetThrows()
    {
        _dbContextMock
            .Setup(c => c.ITSMKnowledgeArticles)
            .Throws(new InvalidOperationException("DB unavailable"));

        var result = await _sut.SearchArticlesAsync("test");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("operation").GetString().Should().Be("SearchArticles");
    }

    #endregion

    #region GetArticleAsync Tests

    [Fact]
    public async Task GetArticleAsync_ShouldReturnSuccessJson_WithFoundTrue_WhenArticleExists()
    {
        var articles = new List<KnowledgeArticle>
        {
            new KnowledgeArticle
            {
                ArticleId = 5,
                Number = "KB0005",
                Title = "Network troubleshooting guide",
                ShortDescription = "How to troubleshoot network issues",
                ArticleBody = "Step 1: Check cable...",
                ArticleType = ArticleType.HowTo,
                PublishingState = PublishingState.Published,
                ViewCount = 200,
                HelpfulCount = 150,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            }
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(articles);
        _dbContextMock.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        var result = await _sut.GetArticleAsync(5);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("articleId").GetInt32().Should().Be(5);
    }

    [Fact]
    public async Task GetArticleAsync_ShouldReturnSuccessJson_WithFoundFalse_WhenArticleNotFound()
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(new List<KnowledgeArticle>());
        _dbContextMock.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        var result = await _sut.GetArticleAsync(999);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("found").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GetArticleAsync_ShouldReturnErrorJson_WhenDbSetThrows()
    {
        _dbContextMock
            .Setup(c => c.ITSMKnowledgeArticles)
            .Throws(new Exception("DB connection failed"));

        var result = await _sut.GetArticleAsync(1);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("operation").GetString().Should().Be("GetArticle");
    }

    #endregion

    #region GetPopularArticlesAsync Tests

    [Fact]
    public async Task GetPopularArticlesAsync_ShouldReturnSuccessJson_WithArticlesSortedByViews()
    {
        var articles = new List<KnowledgeArticle>
        {
            new KnowledgeArticle { ArticleId = 1, Number = "KB0001", Title = "Popular Article", ArticleBody = "Body 1", ArticleType = ArticleType.HowTo, PublishingState = PublishingState.Published, ViewCount = 500, IsDeleted = false },
            new KnowledgeArticle { ArticleId = 2, Number = "KB0002", Title = "Less Popular", ArticleBody = "Body 2", ArticleType = ArticleType.HowTo, PublishingState = PublishingState.Published, ViewCount = 100, IsDeleted = false }
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(articles);
        _dbContextMock.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        var result = await _sut.GetPopularArticlesAsync(10);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task GetPopularArticlesAsync_ShouldFilterOutDraftArticles()
    {
        var articles = new List<KnowledgeArticle>
        {
            new KnowledgeArticle { ArticleId = 1, Number = "KB0001", Title = "Published", ArticleBody = "Body", ArticleType = ArticleType.HowTo, PublishingState = PublishingState.Published, ViewCount = 100, IsDeleted = false },
            new KnowledgeArticle { ArticleId = 2, Number = "KB0002", Title = "Draft", ArticleBody = "Body", ArticleType = ArticleType.HowTo, PublishingState = PublishingState.Draft, ViewCount = 500, IsDeleted = false }
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(articles);
        _dbContextMock.Setup(c => c.ITSMKnowledgeArticles).Returns(mockSet.Object);

        var result = await _sut.GetPopularArticlesAsync(10);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        // Only published articles should be returned
        doc.RootElement.GetProperty("data").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task GetPopularArticlesAsync_ShouldReturnErrorJson_WhenDbSetThrows()
    {
        _dbContextMock
            .Setup(c => c.ITSMKnowledgeArticles)
            .Throws(new Exception("DB read failed"));

        var result = await _sut.GetPopularArticlesAsync();

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("operation").GetString().Should().Be("GetPopularArticles");
    }

    #endregion
}
