// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#nullable enable

using System.Text.Json;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;
using CRM.Infrastructure.AI.SK.Plugins;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using KBArticle = CRM.Core.Entities.KnowledgeBase.KnowledgeArticle;
using KBArticleStatus = CRM.Core.Entities.KnowledgeBase.ArticleStatus;

namespace CRM.Tests.Unit.SK;

/// <summary>
/// Unit tests for KB-018: Unified knowledge search in <see cref="KnowledgeBasePlugin"/>.
/// </summary>
public class KnowledgeBasePluginUnifiedTests
{
    private readonly Mock<ICrmDbContext> _dbContextMock = new();
    private readonly Mock<ILogger<KnowledgeBasePlugin>> _loggerMock = new();
    private readonly Mock<IUnifiedKnowledgeSearchService> _unifiedSearchMock = new();

    #region SearchArticlesAsync — Unified Search Delegation

    [Fact]
    public async Task SearchArticlesAsync_ShouldUseUnifiedSearch_WhenServiceAvailable()
    {
        // Arrange
        var expectedResults = new List<UnifiedKnowledgeSearchResultDto>
        {
            new()
            {
                Id = 1,
                Title = "Password Reset Guide",
                Summary = "How to reset your password",
                Source = KnowledgeSource.General,
                Slug = "password-reset-guide",
                RelevanceScore = 0.95,
                ViewCount = 150,
                Category = "Security",
                UpdatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = 42,
                Title = "Password Policy ITSM",
                Summary = "ITSM password policy",
                Source = KnowledgeSource.ITSM,
                Slug = "",
                RelevanceScore = 0.80,
                ViewCount = 75,
                Category = null,
                UpdatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        _unifiedSearchMock
            .Setup(s => s.SearchAsync("password", 10, KnowledgeSource.All, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        var plugin = new KnowledgeBasePlugin(
            _dbContextMock.Object,
            _loggerMock.Object,
            _unifiedSearchMock.Object);

        // Act
        var result = await plugin.SearchArticlesAsync("password", 10);

        // Assert
        _unifiedSearchMock.Verify(
            s => s.SearchAsync("password", 10, KnowledgeSource.All, It.IsAny<CancellationToken>()),
            Times.Once);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();

        var data = doc.RootElement.GetProperty("data");
        data.GetProperty("totalFound").GetInt32().Should().Be(2);

        var articles = data.GetProperty("articles");
        articles.GetArrayLength().Should().Be(2);
        articles[0].GetProperty("title").GetString().Should().Be("Password Reset Guide");
        articles[0].GetProperty("source").GetString().Should().Be("General");
        articles[1].GetProperty("source").GetString().Should().Be("ITSM");
    }

    #endregion

    #region SearchArticlesAsync — ITSM Fallback

    [Fact]
    public async Task SearchArticlesAsync_ShouldFallbackToITSM_WhenUnifiedServiceIsNull()
    {
        // Arrange
        var itsmArticles = new List<CRM.Core.Entities.ITSM.KnowledgeArticle>
        {
            new()
            {
                ArticleId = 10,
                Number = "KBA-001",
                Title = "VPN Setup Guide",
                ShortDescription = "How to configure VPN",
                ArticleBody = "Step 1: Install client...",
                ArticleType = CRM.Core.Entities.ITSM.ArticleType.HowTo,
                PublishingState = PublishingState.Published,
                ViewCount = 200,
                HelpfulCount = 50,
                PublishedDate = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                Tags = "vpn,networking",
                IsDeleted = false
            },
            new()
            {
                ArticleId = 11,
                Number = "KBA-002",
                Title = "Email Configuration",
                ShortDescription = "Setting up email",
                ArticleBody = "Configure SMTP...",
                ArticleType = CRM.Core.Entities.ITSM.ArticleType.HowTo,
                PublishingState = PublishingState.Draft, // Should be filtered out
                ViewCount = 100,
                HelpfulCount = 10,
                IsDeleted = false
            }
        };

        var mockDbSet = MockDbSetFactory.CreateMockDbSet(itsmArticles);
        _dbContextMock.Setup(c => c.ITSMKnowledgeArticles).Returns(mockDbSet.Object);

        // No unified service — pass null
        var plugin = new KnowledgeBasePlugin(
            _dbContextMock.Object,
            _loggerMock.Object,
            unifiedSearchService: null);

        // Act
        var result = await plugin.SearchArticlesAsync("VPN", 10);

        // Assert
        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();

        var data = doc.RootElement.GetProperty("data");
        // Only the published article matching "VPN" should be returned
        data.GetProperty("totalFound").GetInt32().Should().Be(1);

        var articles = data.GetProperty("articles");
        articles.GetArrayLength().Should().Be(1);
        articles[0].GetProperty("title").GetString().Should().Be("VPN Setup Guide");
    }

    #endregion

    #region SearchGeneralKBArticlesAsync

    [Fact]
    public async Task SearchGeneralKBArticlesAsync_ShouldReturnPublishedGeneralKBArticles()
    {
        // Arrange
        var generalArticles = new List<KBArticle>
        {
            new()
            {
                Id = 1,
                Title = "Getting Started with CRM",
                Summary = "A beginner's guide to the CRM system",
                Content = "Welcome to the CRM platform...",
                Status = KBArticleStatus.Published,
                IsDeleted = false,
                ViewCount = 500,
                Slug = "getting-started-crm",
                ArticleType = CRM.Core.Entities.KnowledgeBase.ArticleType.HowTo,
                PublishedAt = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = 2,
                Title = "Advanced CRM Reporting",
                Summary = "Deep dive into CRM reports",
                Content = "This article covers advanced reporting features...",
                Status = KBArticleStatus.Published,
                IsDeleted = false,
                ViewCount = 300,
                Slug = "advanced-crm-reporting",
                ArticleType = CRM.Core.Entities.KnowledgeBase.ArticleType.Documentation,
                PublishedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = 3,
                Title = "Draft CRM Article",
                Summary = "This is still in draft",
                Content = "CRM draft content...",
                Status = KBArticleStatus.Draft, // Should be filtered out
                IsDeleted = false,
                ViewCount = 10,
                Slug = "draft-crm-article"
            },
            new()
            {
                Id = 4,
                Title = "Deleted CRM Guide",
                Summary = "CRM deleted content",
                Content = "CRM guide that was deleted...",
                Status = KBArticleStatus.Published,
                IsDeleted = true, // Should be filtered out
                ViewCount = 100,
                Slug = "deleted-crm-guide"
            }
        };

        var mockDbSet = MockDbSetFactory.CreateMockDbSet(generalArticles);
        _dbContextMock.Setup(c => c.KnowledgeArticles).Returns(mockDbSet.Object);

        var plugin = new KnowledgeBasePlugin(
            _dbContextMock.Object,
            _loggerMock.Object);

        // Act
        var result = await plugin.SearchGeneralKBArticlesAsync("CRM", 10);

        // Assert
        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();

        var data = doc.RootElement.GetProperty("data");
        // Only Id=1 and Id=2 are Published + not deleted + match "CRM"
        data.GetProperty("totalFound").GetInt32().Should().Be(2);

        var articles = data.GetProperty("articles");
        articles.GetArrayLength().Should().Be(2);

        // Ordered by ViewCount desc: Id=1 (500) then Id=2 (300)
        articles[0].GetProperty("title").GetString().Should().Be("Getting Started with CRM");
        articles[0].GetProperty("slug").GetString().Should().Be("getting-started-crm");
        articles[1].GetProperty("title").GetString().Should().Be("Advanced CRM Reporting");
    }

    #endregion
}
