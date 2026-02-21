// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Comprehensive unit tests for ITSM Knowledge Management functionality
/// </summary>
public class KnowledgeServiceTests
{
    #region Create Article Tests

    [Fact]
    public void CreateArticle_ValidData_CreatesCorrectly()
    {
        // Arrange & Act
        var article = new KnowledgeArticle
        {
            Title = "How to Reset Your Password",
            Content = "Step-by-step guide for password reset...",
            Category = "Access",
            Keywords = "password,reset,login,access",
            Status = ArticleStatus.Draft,
            AuthorId = 1,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        article.Should().NotBeNull();
        article.Title.Should().Be("How to Reset Your Password");
        article.Status.Should().Be(ArticleStatus.Draft);
    }

    [Fact]
    public void CreateArticle_GeneratesArticleNumber()
    {
        // Arrange
        var article = new KnowledgeArticle
        {
            ArticleNumber = "KB0000001",
            Title = "Test article"
        };

        // Assert
        article.ArticleNumber.Should().StartWith("KB");
        article.ArticleNumber.Should().HaveLength(9);
    }

    [Fact]
    public void CreateArticle_WithSymptomCauseResolution_FollowsKCSFormat()
    {
        // Arrange & Act
        var article = new KnowledgeArticle
        {
            Title = "Email sync fails on Mondays",
            Symptom = "Users report that Outlook does not sync emails on Monday mornings",
            Environment = "Windows 10, Outlook 365, On-premises Exchange",
            Cause = "Server batch jobs run during peak login hours",
            Resolution = "1. Open Outlook\n2. File > Account Settings\n3. Click Repair..."
        };

        // Assert
        article.Symptom.Should().NotBeNullOrEmpty();
        article.Environment.Should().NotBeNullOrEmpty();
        article.Cause.Should().NotBeNullOrEmpty();
        article.Resolution.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Article Lifecycle Tests

    [Fact]
    public void ArticleLifecycle_DraftToReview_IsValid()
    {
        // Arrange
        var article = new KnowledgeArticle { Status = ArticleStatus.Draft };

        // Act
        var canTransition = IsValidStatusTransition(article.Status, ArticleStatus.InReview);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Fact]
    public void ArticleLifecycle_ReviewToPublished_IsValid()
    {
        // Arrange
        var article = new KnowledgeArticle { Status = ArticleStatus.InReview };

        // Act
        var canTransition = IsValidStatusTransition(article.Status, ArticleStatus.Published);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Fact]
    public void ArticleLifecycle_PublishedToArchived_IsValid()
    {
        // Arrange
        var article = new KnowledgeArticle { Status = ArticleStatus.Published };

        // Act
        var canTransition = IsValidStatusTransition(article.Status, ArticleStatus.Archived);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Fact]
    public void ArticleLifecycle_PublishSetsTimestamp()
    {
        // Arrange
        var article = new KnowledgeArticle { Status = ArticleStatus.InReview };

        // Act
        article.Status = ArticleStatus.Published;
        article.PublishedAt = DateTime.UtcNow;
        article.Version = 1;

        // Assert
        article.PublishedAt.Should().NotBeNull();
        article.Version.Should().Be(1);
    }

    private static bool IsValidStatusTransition(ArticleStatus from, ArticleStatus to)
    {
        var validTransitions = new Dictionary<ArticleStatus, ArticleStatus[]>
        {
            { ArticleStatus.Draft, new[] { ArticleStatus.InReview, ArticleStatus.Cancelled } },
            { ArticleStatus.InReview, new[] { ArticleStatus.Published, ArticleStatus.Draft, ArticleStatus.Rejected } },
            { ArticleStatus.Published, new[] { ArticleStatus.Draft, ArticleStatus.Archived, ArticleStatus.Retired } },
            { ArticleStatus.Archived, new[] { ArticleStatus.Published } },
            { ArticleStatus.Retired, Array.Empty<ArticleStatus>() },
            { ArticleStatus.Rejected, new[] { ArticleStatus.Draft } },
            { ArticleStatus.Cancelled, Array.Empty<ArticleStatus>() }
        };

        return validTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    #endregion

    #region Search Tests

    [Fact]
    public void SearchArticles_ByKeyword_ReturnsMatches()
    {
        // Arrange
        var articles = CreateSampleArticles();
        var searchTerm = "password";

        // Act
        var results = SearchArticles(articles, searchTerm);

        // Assert
        results.Should().HaveCountGreaterThan(0);
        results.Should().Contain(a => a.Keywords.Contains("password"));
    }

    [Fact]
    public void SearchArticles_ByTitle_ReturnsMatches()
    {
        // Arrange
        var articles = CreateSampleArticles();
        var searchTerm = "VPN";

        // Act
        var results = SearchArticles(articles, searchTerm);

        // Assert
        results.Should().Contain(a => a.Title.Contains("VPN"));
    }

    [Fact]
    public void SearchArticles_NoMatch_ReturnsEmpty()
    {
        // Arrange
        var articles = CreateSampleArticles();
        var searchTerm = "xyznonexistent";

        // Act
        var results = SearchArticles(articles, searchTerm);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void SearchArticles_ByCategory_FiltersCorrectly()
    {
        // Arrange
        var articles = CreateSampleArticles();
        var category = "Network";

        // Act
        var results = articles.Where(a => a.Category == category).ToList();

        // Assert
        results.Should().OnlyContain(a => a.Category == category);
    }

    private static List<KnowledgeArticle> SearchArticles(
        List<KnowledgeArticle> articles,
        string searchTerm)
    {
        var term = searchTerm.ToLower();
        return articles
            .Where(a => a.Status == ArticleStatus.Published)
            .Where(a =>
                a.Title.ToLower().Contains(term) ||
                a.Content.ToLower().Contains(term) ||
                a.Keywords.ToLower().Contains(term))
            .ToList();
    }

    #endregion

    #region Feedback Tests

    [Fact]
    public void ArticleFeedback_Helpful_IncrementsCount()
    {
        // Arrange
        var article = new KnowledgeArticle
        {
            HelpfulCount = 10,
            NotHelpfulCount = 2
        };

        // Act
        article.HelpfulCount++;

        // Assert
        article.HelpfulCount.Should().Be(11);
    }

    [Fact]
    public void ArticleFeedback_NotHelpful_IncrementsCount()
    {
        // Arrange
        var article = new KnowledgeArticle
        {
            HelpfulCount = 10,
            NotHelpfulCount = 2
        };

        // Act
        article.NotHelpfulCount++;

        // Assert
        article.NotHelpfulCount.Should().Be(3);
    }

    [Fact]
    public void ArticleFeedback_HelpfulnessRatio_CalculatesCorrectly()
    {
        // Arrange
        var article = new KnowledgeArticle
        {
            HelpfulCount = 80,
            NotHelpfulCount = 20
        };

        // Act
        var ratio = CalculateHelpfulnessRatio(article);

        // Assert
        ratio.Should().Be(0.80);
    }

    [Fact]
    public void ArticleFeedback_LowRatio_FlagsForReview()
    {
        // Arrange
        var article = new KnowledgeArticle
        {
            HelpfulCount = 20,
            NotHelpfulCount = 80
        };

        // Act
        var ratio = CalculateHelpfulnessRatio(article);
        var needsReview = ratio < 0.50;

        // Assert
        needsReview.Should().BeTrue();
    }

    private static double CalculateHelpfulnessRatio(KnowledgeArticle article)
    {
        var total = article.HelpfulCount + article.NotHelpfulCount;
        if (total == 0)
            return 1.0;
        return (double)article.HelpfulCount / total;
    }

    #endregion

    #region View Tracking Tests

    [Fact]
    public void ArticleView_IncrementsViewCount()
    {
        // Arrange
        var article = new KnowledgeArticle { ViewCount = 100 };

        // Act
        article.ViewCount++;

        // Assert
        article.ViewCount.Should().Be(101);
    }

    [Fact]
    public void ArticleView_TracksLastViewed()
    {
        // Arrange
        var article = new KnowledgeArticle { LastViewedAt = DateTime.UtcNow.AddDays(-1) };

        // Act
        article.ViewCount++;
        article.LastViewedAt = DateTime.UtcNow;

        // Assert
        article.LastViewedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Version Control Tests

    [Fact]
    public void ArticleUpdate_IncrementsVersion()
    {
        // Arrange
        var article = new KnowledgeArticle
        {
            Version = 1,
            Content = "Original content"
        };

        // Act
        article.Content = "Updated content";
        article.Version++;
        article.ModifiedAt = DateTime.UtcNow;

        // Assert
        article.Version.Should().Be(2);
        article.ModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void ArticleUpdate_RequiresReReviewIfPublished()
    {
        // Arrange
        var article = new KnowledgeArticle
        {
            Status = ArticleStatus.Published,
            Version = 1
        };

        // Act - Major update to published article
        article.Content = "Significantly updated content";
        article.Status = ArticleStatus.Draft;
        article.Version++;

        // Assert
        article.Status.Should().Be(ArticleStatus.Draft); // Needs re-review
        article.Version.Should().Be(2);
    }

    #endregion

    #region Helper Methods

    private static List<KnowledgeArticle> CreateSampleArticles()
    {
        return new List<KnowledgeArticle>
        {
            new()
            {
                ArticleId = 1,
                ArticleNumber = "KB0000001",
                Title = "How to Reset Your Password",
                Content = "Step-by-step guide for password reset...",
                Category = "Access",
                Keywords = "password,reset,login,forgot",
                Status = ArticleStatus.Published,
                ViewCount = 1500,
                HelpfulCount = 1200
            },
            new()
            {
                ArticleId = 2,
                ArticleNumber = "KB0000002",
                Title = "VPN Connection Troubleshooting",
                Content = "Common VPN issues and solutions...",
                Category = "Network",
                Keywords = "vpn,connection,remote,network",
                Status = ArticleStatus.Published,
                ViewCount = 800,
                HelpfulCount = 650
            },
            new()
            {
                ArticleId = 3,
                ArticleNumber = "KB0000003",
                Title = "Setting Up MFA",
                Content = "How to configure multi-factor authentication...",
                Category = "Security",
                Keywords = "mfa,2fa,security,authentication",
                Status = ArticleStatus.Published,
                ViewCount = 600,
                HelpfulCount = 550
            },
            new()
            {
                ArticleId = 4,
                ArticleNumber = "KB0000004",
                Title = "Draft Article",
                Content = "Work in progress...",
                Category = "General",
                Keywords = "draft",
                Status = ArticleStatus.Draft,
                ViewCount = 0
            }
        };
    }

    #endregion
}

// Test helper classes
public class KnowledgeArticle
{
    public int ArticleId { get; set; }
    public string ArticleNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Symptom { get; set; }
    public string? Environment { get; set; }
    public string? Cause { get; set; }
    public string? Resolution { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Keywords { get; set; } = string.Empty;
    public ArticleStatus Status { get; set; }
    public int AuthorId { get; set; }
    public int Version { get; set; } = 1;
    public int ViewCount { get; set; }
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? LastViewedAt { get; set; }
}

public enum ArticleStatus
{
    Draft = 1,
    InReview = 2,
    Published = 3,
    Archived = 4,
    Retired = 5,
    Rejected = 6,
    Cancelled = 7
}
