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

namespace CRM.Core.DTOs.ITSM;

// ============================================================================
// Article Recommendation DTOs
// ============================================================================

/// <summary>
/// Represents a recommended knowledge article with relevance scoring.
/// </summary>
/// <param name="ArticleId">The unique identifier of the article.</param>
/// <param name="Title">The article title.</param>
/// <param name="Summary">A short summary or description of the article.</param>
/// <param name="RelevanceScore">A score indicating how relevant the article is (0.0 to 1.0).</param>
/// <param name="ViewCount">Number of times the article has been viewed.</param>
public record ArticleRecommendation(
    int ArticleId,
    string Title,
    string? Summary,
    double RelevanceScore,
    int ViewCount);

/// <summary>
/// Represents a trending knowledge article with view statistics.
/// </summary>
/// <param name="ArticleId">The unique identifier of the article.</param>
/// <param name="Title">The article title.</param>
/// <param name="ViewCount">Number of times the article has been viewed.</param>
/// <param name="Trend">The trend direction for the article.</param>
public record TrendingArticle(
    int ArticleId,
    string Title,
    int ViewCount,
    TrendDirection Trend);

/// <summary>
/// Defines the type of feedback a user can provide for an article.
/// </summary>
public enum ArticleFeedbackType
{
    /// <summary>
    /// The article was helpful in resolving the issue.
    /// </summary>
    Helpful = 1,

    /// <summary>
    /// The article was not helpful.
    /// </summary>
    NotHelpful = 2,

    /// <summary>
    /// The article needs to be updated or revised.
    /// </summary>
    NeedsUpdate = 3
}

/// <summary>
/// Defines the trend direction for article popularity.
/// </summary>
public enum TrendDirection
{
    /// <summary>
    /// Article views are increasing.
    /// </summary>
    Up = 1,

    /// <summary>
    /// Article views are decreasing.
    /// </summary>
    Down = 2,

    /// <summary>
    /// Article views are stable.
    /// </summary>
    Stable = 3
}
