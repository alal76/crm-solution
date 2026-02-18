// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
