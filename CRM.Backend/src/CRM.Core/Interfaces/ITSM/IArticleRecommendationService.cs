// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.DTOs.ITSM;

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Service for recommending knowledge articles based on incident context and tracking article feedback.
/// </summary>
public interface IArticleRecommendationService
{
    /// <summary>
    /// Gets article recommendations based on incident category and metadata.
    /// </summary>
    /// <param name="incidentId">The incident ID to get recommendations for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of recommended articles with relevance scores.</returns>
    Task<IEnumerable<ArticleRecommendation>> GetRecommendationsAsync(int incidentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets trending articles based on view count and recent activity.
    /// </summary>
    /// <param name="count">Number of trending articles to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of trending articles with trend direction.</returns>
    Task<IEnumerable<TrendingArticle>> GetTrendingArticlesAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits user feedback for an article.
    /// </summary>
    /// <param name="articleId">The article ID to submit feedback for.</param>
    /// <param name="feedbackType">Type of feedback (Helpful, NotHelpful, NeedsUpdate).</param>
    /// <param name="userId">Optional user ID who submitted the feedback.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SubmitFeedbackAsync(int articleId, ArticleFeedbackType feedbackType, int? userId, CancellationToken cancellationToken = default);
}
