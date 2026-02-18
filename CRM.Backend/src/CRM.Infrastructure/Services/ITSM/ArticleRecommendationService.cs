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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Service for recommending knowledge articles based on incident context and tracking article feedback.
/// </summary>
public class ArticleRecommendationService : IArticleRecommendationService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<ArticleRecommendationService> _logger;

    public ArticleRecommendationService(
        ICrmDbContext dbContext,
        ILogger<ArticleRecommendationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ArticleRecommendation>> GetRecommendationsAsync(
        int incidentId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting article recommendations for incident {IncidentId}", incidentId);

        // Get the incident to determine category
        var incident = await _dbContext.Incidents
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.IncidentId == incidentId && !i.IsDeleted, cancellationToken);

        if (incident == null)
        {
            _logger.LogWarning("Incident {IncidentId} not found for article recommendations", incidentId);
            return Enumerable.Empty<ArticleRecommendation>();
        }

        // Build query for published articles
        var query = _dbContext.ITSMKnowledgeArticles
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.PublishingState == PublishingState.Published);

        // Filter by category if the incident has one
        if (incident.CategoryId.HasValue)
        {
            query = query.Where(a => a.CategoryId == incident.CategoryId);
        }

        // Get articles ordered by view count (as a proxy for relevance)
        var articles = await query
            .OrderByDescending(a => a.ViewCount)
            .ThenByDescending(a => a.HelpfulCount)
            .Take(10)
            .Select(a => new
            {
                a.ArticleId,
                a.Title,
                a.ShortDescription,
                a.ViewCount,
                a.HelpfulCount,
                a.NotHelpfulCount
            })
            .ToListAsync(cancellationToken);

        // Calculate relevance scores
        var recommendations = articles.Select((a, index) =>
        {
            // Calculate relevance score based on position, views, and helpfulness
            double relevanceScore = CalculateRelevanceScore(
                index,
                a.ViewCount,
                a.HelpfulCount,
                a.NotHelpfulCount);

            return new ArticleRecommendation(
                a.ArticleId,
                a.Title,
                a.ShortDescription,
                relevanceScore,
                a.ViewCount);
        }).ToList();

        _logger.LogInformation(
            "Found {Count} article recommendations for incident {IncidentId}",
            recommendations.Count,
            incidentId);

        return recommendations;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TrendingArticle>> GetTrendingArticlesAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting top {Count} trending articles", count);

        if (count <= 0)
        {
            count = 10;
        }

        // Get published articles ordered by view count
        var articles = await _dbContext.ITSMKnowledgeArticles
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.PublishingState == PublishingState.Published)
            .OrderByDescending(a => a.ViewCount)
            .Take(count)
            .Select(a => new
            {
                a.ArticleId,
                a.Title,
                a.ViewCount,
                a.LastViewedAt,
                a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Calculate trend direction based on recent activity
        var trendingArticles = articles.Select(a =>
        {
            var trend = CalculateTrendDirection(a.ViewCount, a.LastViewedAt, a.CreatedAt);
            return new TrendingArticle(
                a.ArticleId,
                a.Title,
                a.ViewCount,
                trend);
        }).ToList();

        _logger.LogInformation("Retrieved {Count} trending articles", trendingArticles.Count);

        return trendingArticles;
    }

    /// <inheritdoc />
    public async Task SubmitFeedbackAsync(
        int articleId,
        ArticleFeedbackType feedbackType,
        int? userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Submitting {FeedbackType} feedback for article {ArticleId} by user {UserId}",
            feedbackType,
            articleId,
            userId);

        var article = await _dbContext.ITSMKnowledgeArticles
            .FirstOrDefaultAsync(a => a.ArticleId == articleId && !a.IsDeleted, cancellationToken);

        if (article == null)
        {
            _logger.LogWarning("Article {ArticleId} not found for feedback submission", articleId);
            return;
        }

        // Update feedback counts directly on the article
        switch (feedbackType)
        {
            case ArticleFeedbackType.Helpful:
                article.HelpfulCount++;
                break;
            case ArticleFeedbackType.NotHelpful:
                article.NotHelpfulCount++;
                break;
            case ArticleFeedbackType.NeedsUpdate:
                // NeedsUpdate could trigger a review workflow in the future
                // For now, log it but don't modify counts
                _logger.LogInformation(
                    "Article {ArticleId} flagged as needing update by user {UserId}",
                    articleId,
                    userId);
                break;
        }

        article.ModifiedAt = DateTime.UtcNow;
        article.ModifiedById = userId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Feedback {FeedbackType} submitted for article {ArticleId}",
            feedbackType,
            articleId);
    }

    /// <summary>
    /// Calculates a relevance score for an article recommendation.
    /// </summary>
    private static double CalculateRelevanceScore(
        int positionIndex,
        int viewCount,
        int helpfulCount,
        int notHelpfulCount)
    {
        // Base score from position (higher position = higher base score)
        double positionScore = Math.Max(0, 1.0 - (positionIndex * 0.1));

        // Helpfulness ratio (avoid division by zero)
        double totalFeedback = helpfulCount + notHelpfulCount;
        double helpfulnessScore = totalFeedback > 0
            ? (double)helpfulCount / totalFeedback
            : 0.5; // Neutral if no feedback

        // View count factor (logarithmic to prevent very high view counts from dominating)
        double viewScore = viewCount > 0
            ? Math.Min(1.0, Math.Log10(viewCount + 1) / 4.0)
            : 0;

        // Weighted combination
        double relevanceScore = (positionScore * 0.4) + (helpfulnessScore * 0.4) + (viewScore * 0.2);

        return Math.Round(relevanceScore, 2);
    }

    /// <summary>
    /// Calculates the trend direction for an article based on recent activity.
    /// </summary>
    private static TrendDirection CalculateTrendDirection(
        int viewCount,
        DateTime? lastViewedAt,
        DateTime createdAt)
    {
        // If never viewed, it's stable
        if (!lastViewedAt.HasValue || viewCount == 0)
        {
            return TrendDirection.Stable;
        }

        var now = DateTime.UtcNow;
        var timeSinceLastView = now - lastViewedAt.Value;
        var articleAge = now - createdAt;

        // Recent activity (last 7 days) indicates upward trend
        if (timeSinceLastView.TotalDays < 7)
        {
            // High view count relative to age suggests popularity
            var viewsPerDay = articleAge.TotalDays > 0
                ? viewCount / articleAge.TotalDays
                : viewCount;

            if (viewsPerDay > 1)
            {
                return TrendDirection.Up;
            }

            return TrendDirection.Stable;
        }

        // No activity in over 30 days suggests declining interest
        if (timeSinceLastView.TotalDays > 30)
        {
            return TrendDirection.Down;
        }

        return TrendDirection.Stable;
    }
}
