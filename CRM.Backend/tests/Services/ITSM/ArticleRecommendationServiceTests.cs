// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// SPDX-License-Identifier: AGPL-3.0-or-later

using CRM.Core.DTOs.ITSM;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Unit tests for Article Recommendation DTOs and enums.
/// Tests the immutable record types used for article recommendations.
/// </summary>
public class ArticleRecommendationServiceTests
{
    #region ArticleRecommendation Record Tests

    [Fact]
    public void ArticleRecommendation_CanBeCreated_WithPositionalParameters()
    {
        // Arrange & Act
        var recommendation = new ArticleRecommendation(
            ArticleId: 123,
            Title: "How to Reset Your Password",
            Summary: "Step-by-step guide for password reset",
            RelevanceScore: 0.95,
            ViewCount: 1500);

        // Assert
        recommendation.ArticleId.Should().Be(123);
        recommendation.Title.Should().Be("How to Reset Your Password");
        recommendation.Summary.Should().Be("Step-by-step guide for password reset");
        recommendation.RelevanceScore.Should().Be(0.95);
        recommendation.ViewCount.Should().Be(1500);
    }

    [Fact]
    public void ArticleRecommendation_SupportsNullSummary()
    {
        // Arrange & Act
        var recommendation = new ArticleRecommendation(
            ArticleId: 1,
            Title: "Test Article",
            Summary: null,
            RelevanceScore: 0.5,
            ViewCount: 100);

        // Assert
        recommendation.Summary.Should().BeNull();
    }

    [Fact]
    public void ArticleRecommendation_RelevanceScore_CanBeBetweenZeroAndOne()
    {
        // Arrange & Act
        var low = new ArticleRecommendation(1, "Low", null, 0.0, 0);
        var high = new ArticleRecommendation(2, "High", null, 1.0, 0);
        var mid = new ArticleRecommendation(3, "Mid", null, 0.5, 0);

        // Assert
        low.RelevanceScore.Should().Be(0.0);
        high.RelevanceScore.Should().Be(1.0);
        mid.RelevanceScore.Should().Be(0.5);
    }

    #endregion

    #region TrendingArticle Record Tests

    [Fact]
    public void TrendingArticle_CanBeCreated_WithPositionalParameters()
    {
        // Arrange & Act
        var article = new TrendingArticle(
            ArticleId: 456,
            Title: "VPN Connection Troubleshooting",
            ViewCount: 5000,
            Trend: TrendDirection.Up);

        // Assert
        article.ArticleId.Should().Be(456);
        article.Title.Should().Be("VPN Connection Troubleshooting");
        article.ViewCount.Should().Be(5000);
        article.Trend.Should().Be(TrendDirection.Up);
    }

    [Fact]
    public void TrendingArticle_SupportsDifferentTrendDirections()
    {
        // Arrange & Act
        var rising = new TrendingArticle(1, "Rising", 100, TrendDirection.Up);
        var falling = new TrendingArticle(2, "Falling", 50, TrendDirection.Down);
        var stable = new TrendingArticle(3, "Stable", 75, TrendDirection.Stable);

        // Assert
        rising.Trend.Should().Be(TrendDirection.Up);
        falling.Trend.Should().Be(TrendDirection.Down);
        stable.Trend.Should().Be(TrendDirection.Stable);
    }

    #endregion

    #region ArticleFeedbackType Enum Tests

    [Fact]
    public void ArticleFeedbackType_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<ArticleFeedbackType>().Should().HaveCount(3);
        ArticleFeedbackType.Helpful.Should().BeDefined();
        ArticleFeedbackType.NotHelpful.Should().BeDefined();
        ArticleFeedbackType.NeedsUpdate.Should().BeDefined();
    }

    [Theory]
    [InlineData(ArticleFeedbackType.Helpful, 1)]
    [InlineData(ArticleFeedbackType.NotHelpful, 2)]
    [InlineData(ArticleFeedbackType.NeedsUpdate, 3)]
    public void ArticleFeedbackType_HasCorrectIntValues(ArticleFeedbackType type, int expectedValue)
    {
        // Assert
        ((int)type).Should().Be(expectedValue);
    }

    #endregion

    #region TrendDirection Enum Tests

    [Fact]
    public void TrendDirection_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<TrendDirection>().Should().HaveCount(3);
        TrendDirection.Up.Should().BeDefined();
        TrendDirection.Down.Should().BeDefined();
        TrendDirection.Stable.Should().BeDefined();
    }

    [Theory]
    [InlineData(TrendDirection.Up, 1)]
    [InlineData(TrendDirection.Down, 2)]
    [InlineData(TrendDirection.Stable, 3)]
    public void TrendDirection_HasCorrectIntValues(TrendDirection direction, int expectedValue)
    {
        // Assert
        ((int)direction).Should().Be(expectedValue);
    }

    #endregion
}
