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

using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Interface for knowledge article recommendation based on incidents.
/// </summary>
public interface IArticleRecommendationService
{
    /// <summary>
    /// Get recommended articles for an incident based on its content.
    /// </summary>
    Task<List<ArticleRecommendation>> GetRecommendationsAsync(int incidentId, int maxResults = 5);

    /// <summary>
    /// Get recommended articles based on text content (search query).
    /// </summary>
    Task<List<ArticleRecommendation>> SearchArticlesAsync(string query, int maxResults = 10);

    /// <summary>
    /// Record when a recommended article was helpful.
    /// </summary>
    Task RecordArticleFeedbackAsync(int incidentId, int articleId, ArticleFeedbackType feedback, int userId);

    /// <summary>
    /// Get trending/popular articles.
    /// </summary>
    Task<List<TrendingArticle>> GetTrendingArticlesAsync(int daysBack = 30, int maxResults = 10);

    /// <summary>
    /// Get related articles for a given article.
    /// </summary>
    Task<List<ArticleRecommendation>> GetRelatedArticlesAsync(int articleId, int maxResults = 5);

    /// <summary>
    /// Train/update the recommendation model with new data.
    /// </summary>
    Task UpdateRecommendationModelAsync();

    /// <summary>
    /// Get recommendation statistics.
    /// </summary>
    Task<RecommendationStats> GetStatsAsync(DateTime fromDate, DateTime toDate);
}

/// <summary>
/// A recommended knowledge article.
/// </summary>
public class ArticleRecommendation
{
    public int ArticleId { get; set; }
    public string ArticleNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Category { get; set; }
    public double RelevanceScore { get; set; }
    public List<string> MatchedKeywords { get; set; } = new();
    public int ViewCount { get; set; }
    public int HelpfulCount { get; set; }
    public DateTime LastModified { get; set; }
    public string? Author { get; set; }
}

/// <summary>
/// Feedback type for articles.
/// </summary>
public enum ArticleFeedbackType
{
    Helpful,
    NotHelpful,
    Viewed,
    SolvedIncident
}

/// <summary>
/// A trending knowledge article.
/// </summary>
public class TrendingArticle
{
    public int ArticleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int ViewCount { get; set; }
    public int RecentViews { get; set; }
    public int HelpfulCount { get; set; }
    public double TrendScore { get; set; }
    public TrendDirection Trend { get; set; }
}

public enum TrendDirection
{
    Rising,
    Stable,
    Falling
}

/// <summary>
/// Recommendation system statistics.
/// </summary>
public class RecommendationStats
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalRecommendations { get; set; }
    public int ArticlesViewed { get; set; }
    public int ArticlesMarkedHelpful { get; set; }
    public int IncidentsSolvedByArticle { get; set; }
    public double HelpfulnessRate { get; set; }
    public double ArticleResolutionRate { get; set; }
    public Dictionary<string, int> TopCategories { get; set; } = new();
    public List<TopRecommendedArticle> TopArticles { get; set; } = new();
}

public class TopRecommendedArticle
{
    public int ArticleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TimesRecommended { get; set; }
    public int TimesHelpful { get; set; }
    public double EffectivenessRate { get; set; }
}

/// <summary>
/// Article feedback record.
/// </summary>
public class ArticleFeedbackRecord
{
    public int FeedbackId { get; set; }
    public int ArticleId { get; set; }
    public int? IncidentId { get; set; }
    public int UserId { get; set; }
    public ArticleFeedbackType FeedbackType { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Service for recommending knowledge articles based on incidents.
/// </summary>
public class ArticleRecommendationService : IArticleRecommendationService
{
    private readonly IDbContextResolver _dbContextResolver;
    private readonly ILogger<ArticleRecommendationService> _logger;

    // In-memory storage for feedback (would be database in production)
    private static readonly List<ArticleFeedbackRecord> _feedbackRecords = new();
    private static readonly Dictionary<int, int> _articleViewCounts = new();
    private static readonly Dictionary<int, int> _articleHelpfulCounts = new();
    private static int _nextFeedbackId = 1;

    // Simple keyword index for demo (would be more sophisticated in production)
    private static readonly Dictionary<string, double> KeywordWeights = new()
    {
        { "password", 2.0 },
        { "reset", 1.5 },
        { "login", 1.5 },
        { "access", 1.2 },
        { "denied", 1.3 },
        { "error", 1.0 },
        { "network", 1.5 },
        { "connection", 1.3 },
        { "email", 1.5 },
        { "outlook", 1.5 },
        { "vpn", 2.0 },
        { "printer", 1.5 },
        { "software", 1.0 },
        { "install", 1.2 },
        { "slow", 1.0 },
        { "performance", 1.2 },
        { "crash", 1.5 },
        { "blue screen", 2.0 },
        { "update", 1.0 },
        { "microsoft", 1.0 },
        { "office", 1.2 },
        { "teams", 1.5 },
        { "sharepoint", 1.5 },
        { "server", 1.3 },
        { "database", 1.5 }
    };

    public ArticleRecommendationService(
        IDbContextResolver dbContextResolver,
        ILogger<ArticleRecommendationService> logger)
    {
        _dbContextResolver = dbContextResolver;
        _logger = logger;
    }

    public async Task<List<ArticleRecommendation>> GetRecommendationsAsync(int incidentId, int maxResults = 5)
    {
        var context = _dbContextResolver.ResolveContext();

        var incident = await context.Incidents
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.IncidentId == incidentId);

        if (incident == null)
        {
            throw new ArgumentException($"Incident {incidentId} not found");
        }

        // Build search text from incident
        var searchText = $"{incident.ShortDescription} {incident.Description}";

        if (incident.Category != null)
        {
            searchText += $" {incident.Category.Name}";
        }

        var recommendations = await SearchArticlesAsync(searchText, maxResults);

        _logger.LogDebug(
            "Found {Count} article recommendations for incident {IncidentNumber}",
            recommendations.Count, incident.Number);

        return recommendations;
    }

    public async Task<List<ArticleRecommendation>> SearchArticlesAsync(string query, int maxResults = 10)
    {
        var context = _dbContextResolver.ResolveContext();

        // Get all published articles
        var articles = await context.ITSMKnowledgeArticles
            .Where(a => a.PublishingState == PublishingState.Published)
            .Include(a => a.Author)
            .ToListAsync();

        var recommendations = new List<ArticleRecommendation>();
        var queryWords = ExtractKeywords(query);

        foreach (var article in articles)
        {
            var articleText = $"{article.Title} {article.ArticleBody} {article.Tags} {article.Category}";
            var articleWords = ExtractKeywords(articleText);

            var (score, matchedKeywords) = CalculateRelevanceScore(queryWords, articleWords);

            if (score > 0)
            {
                // Boost score based on article quality metrics
                var viewCount = _articleViewCounts.GetValueOrDefault(article.ArticleId, 0);
                var helpfulCount = _articleHelpfulCounts.GetValueOrDefault(article.ArticleId, 0);

                // Apply popularity boost
                var popularityBoost = Math.Log10(viewCount + 1) * 0.1;
                var helpfulBoost = helpfulCount * 0.05;

                score += popularityBoost + helpfulBoost;

                recommendations.Add(new ArticleRecommendation
                {
                    ArticleId = article.ArticleId,
                    ArticleNumber = article.Number,
                    Title = article.Title,
                    Summary = TruncateContent(article.ArticleBody, 200),
                    Category = article.Category?.Name,
                    RelevanceScore = score,
                    MatchedKeywords = matchedKeywords,
                    ViewCount = viewCount,
                    HelpfulCount = helpfulCount,
                    LastModified = article.ModifiedAt ?? article.CreatedAt,
                    Author = article.Author?.Username
                });
            }
        }

        return recommendations
            .OrderByDescending(r => r.RelevanceScore)
            .Take(maxResults)
            .ToList();
    }

    public async Task RecordArticleFeedbackAsync(int incidentId, int articleId, ArticleFeedbackType feedback, int userId)
    {
        var feedbackRecord = new ArticleFeedbackRecord
        {
            FeedbackId = _nextFeedbackId++,
            ArticleId = articleId,
            IncidentId = incidentId,
            UserId = userId,
            FeedbackType = feedback,
            CreatedAt = DateTime.UtcNow
        };

        _feedbackRecords.Add(feedbackRecord);

        // Update counts
        switch (feedback)
        {
            case ArticleFeedbackType.Viewed:
                _articleViewCounts[articleId] = _articleViewCounts.GetValueOrDefault(articleId, 0) + 1;
                break;
            case ArticleFeedbackType.Helpful:
            case ArticleFeedbackType.SolvedIncident:
                _articleHelpfulCounts[articleId] = _articleHelpfulCounts.GetValueOrDefault(articleId, 0) + 1;
                break;
        }

        _logger.LogInformation(
            "Recorded {Feedback} feedback for article {ArticleId} from incident {IncidentId}",
            feedback, articleId, incidentId);

        await Task.CompletedTask;
    }

    public async Task<List<TrendingArticle>> GetTrendingArticlesAsync(int daysBack = 30, int maxResults = 10)
    {
        var context = _dbContextResolver.ResolveContext();
        var cutoffDate = DateTime.UtcNow.AddDays(-daysBack);

        // Get recent feedback for trending calculation
        var recentFeedback = _feedbackRecords
            .Where(f => f.CreatedAt >= cutoffDate)
            .GroupBy(f => f.ArticleId)
            .Select(g => new
            {
                ArticleId = g.Key,
                RecentViews = g.Count(f => f.FeedbackType == ArticleFeedbackType.Viewed),
                RecentHelpful = g.Count(f => f.FeedbackType == ArticleFeedbackType.Helpful ||
                                             f.FeedbackType == ArticleFeedbackType.SolvedIncident)
            })
            .ToList();

        var articles = await context.ITSMKnowledgeArticles
            .Where(a => a.PublishingState == PublishingState.Published)
            .ToListAsync();

        var trending = new List<TrendingArticle>();

        foreach (var article in articles)
        {
            var recent = recentFeedback.FirstOrDefault(f => f.ArticleId == article.ArticleId);
            var totalViews = _articleViewCounts.GetValueOrDefault(article.ArticleId, 0);
            var totalHelpful = _articleHelpfulCounts.GetValueOrDefault(article.ArticleId, 0);
            var recentViews = recent?.RecentViews ?? 0;

            // Calculate trend score
            var trendScore = recentViews * 2 + (recent?.RecentHelpful ?? 0) * 5;

            // Determine trend direction
            var avgRecentViews = daysBack > 0 ? recentViews / (double)daysBack : 0;
            var avgHistoricalViews = totalViews > recentViews
                ? (totalViews - recentViews) / 30.0 // Assume 30 days history
                : 0;

            var trend = avgRecentViews > avgHistoricalViews * 1.2
                ? TrendDirection.Rising
                : avgRecentViews < avgHistoricalViews * 0.8
                    ? TrendDirection.Falling
                    : TrendDirection.Stable;

            if (trendScore > 0 || totalViews > 0)
            {
                trending.Add(new TrendingArticle
                {
                    ArticleId = article.ArticleId,
                    Title = article.Title,
                    Category = article.Category?.Name,
                    ViewCount = totalViews,
                    RecentViews = recentViews,
                    HelpfulCount = totalHelpful,
                    TrendScore = trendScore,
                    Trend = trend
                });
            }
        }

        return trending
            .OrderByDescending(t => t.TrendScore)
            .Take(maxResults)
            .ToList();
    }

    public async Task<List<ArticleRecommendation>> GetRelatedArticlesAsync(int articleId, int maxResults = 5)
    {
        var context = _dbContextResolver.ResolveContext();

        var article = await context.ITSMKnowledgeArticles
            .FirstOrDefaultAsync(a => a.ArticleId == articleId);

        if (article == null)
        {
            return new List<ArticleRecommendation>();
        }

        // Build search text from current article
        var searchText = $"{article.Title} {article.Tags} {article.Category}";

        var recommendations = await SearchArticlesAsync(searchText, maxResults + 1);

        // Remove the source article from results
        return recommendations
            .Where(r => r.ArticleId != articleId)
            .Take(maxResults)
            .ToList();
    }

    public async Task UpdateRecommendationModelAsync()
    {
        // In a real implementation, this would:
        // 1. Retrain ML model with new feedback data
        // 2. Update keyword weights based on successful matches
        // 3. Recalculate article similarity scores
        // 4. Update category associations

        _logger.LogInformation("Recommendation model update triggered");

        // Simulate model update
        await Task.Delay(100);

        // Update keyword weights based on feedback
        var successfulArticles = _feedbackRecords
            .Where(f => f.FeedbackType == ArticleFeedbackType.SolvedIncident)
            .Select(f => f.ArticleId)
            .Distinct()
            .ToList();

        // Would analyze successful articles and update weights
        _logger.LogInformation(
            "Model updated with data from {Count} successful resolutions",
            successfulArticles.Count);
    }

    public async Task<RecommendationStats> GetStatsAsync(DateTime fromDate, DateTime toDate)
    {
        var context = _dbContextResolver.ResolveContext();

        var feedbackInRange = _feedbackRecords
            .Where(f => f.CreatedAt >= fromDate && f.CreatedAt <= toDate)
            .ToList();

        var viewed = feedbackInRange.Count(f => f.FeedbackType == ArticleFeedbackType.Viewed);
        var helpful = feedbackInRange.Count(f => f.FeedbackType == ArticleFeedbackType.Helpful);
        var solved = feedbackInRange.Count(f => f.FeedbackType == ArticleFeedbackType.SolvedIncident);

        // Get articles for category breakdown
        var articleIds = feedbackInRange.Select(f => f.ArticleId).Distinct().ToList();
        var articles = await context.ITSMKnowledgeArticles
            .Where(a => articleIds.Contains(a.ArticleId))
            .ToListAsync();

        var stats = new RecommendationStats
        {
            FromDate = fromDate,
            ToDate = toDate,
            TotalRecommendations = feedbackInRange.Count,
            ArticlesViewed = viewed,
            ArticlesMarkedHelpful = helpful,
            IncidentsSolvedByArticle = solved,
            HelpfulnessRate = viewed > 0 ? (double)helpful / viewed * 100 : 0,
            ArticleResolutionRate = viewed > 0 ? (double)solved / viewed * 100 : 0,
            TopCategories = articles
                .Where(a => a.Category != null && !string.IsNullOrEmpty(a.Category.Name))
                .GroupBy(a => a.Category!.Name)
                .ToDictionary(g => g.Key, g => g.Count()),
            TopArticles = feedbackInRange
                .GroupBy(f => f.ArticleId)
                .Select(g =>
                {
                    var article = articles.FirstOrDefault(a => a.ArticleId == g.Key);
                    return new TopRecommendedArticle
                    {
                        ArticleId = g.Key,
                        Title = article?.Title ?? $"Article {g.Key}",
                        TimesRecommended = g.Count(),
                        TimesHelpful = g.Count(f => f.FeedbackType == ArticleFeedbackType.Helpful ||
                                                    f.FeedbackType == ArticleFeedbackType.SolvedIncident),
                        EffectivenessRate = g.Count() > 0
                            ? (double)g.Count(f => f.FeedbackType == ArticleFeedbackType.Helpful ||
                                                   f.FeedbackType == ArticleFeedbackType.SolvedIncident) / g.Count() * 100
                            : 0
                    };
                })
                .OrderByDescending(a => a.TimesRecommended)
                .Take(10)
                .ToList()
        };

        return stats;
    }

    private List<string> ExtractKeywords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        // Simple keyword extraction
        var words = text.ToLower()
            .Split(new[] { ' ', '\n', '\r', '\t', '.', ',', '!', '?', ';', ':', '-', '(', ')', '[', ']' },
                   StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2)
            .Where(w => !StopWords.Contains(w))
            .ToList();

        return words;
    }

    private (double score, List<string> matchedKeywords) CalculateRelevanceScore(
        List<string> queryWords,
        List<string> articleWords)
    {
        double score = 0;
        var matchedKeywords = new List<string>();
        var articleWordSet = articleWords.ToHashSet();

        foreach (var word in queryWords.Distinct())
        {
            if (articleWordSet.Contains(word))
            {
                var weight = KeywordWeights.GetValueOrDefault(word, 1.0);
                score += weight;
                matchedKeywords.Add(word);
            }
        }

        // Normalize by query length
        if (queryWords.Count > 0)
        {
            score = score / Math.Sqrt(queryWords.Count);
        }

        return (score, matchedKeywords);
    }

    private static string TruncateContent(string? content, int maxLength)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        if (content.Length <= maxLength)
            return content;

        // Find last space before maxLength
        var lastSpace = content.LastIndexOf(' ', maxLength);
        if (lastSpace > maxLength / 2)
        {
            return content[..lastSpace] + "...";
        }

        return content[..maxLength] + "...";
    }

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for",
        "of", "with", "by", "from", "as", "is", "was", "are", "were", "been",
        "be", "have", "has", "had", "do", "does", "did", "will", "would", "could",
        "should", "may", "might", "must", "can", "this", "that", "these", "those",
        "it", "its", "i", "you", "we", "they", "he", "she", "who", "which", "what",
        "when", "where", "why", "how", "all", "each", "every", "both", "few", "more",
        "most", "other", "some", "such", "no", "not", "only", "own", "same", "so",
        "than", "too", "very", "just", "also", "now", "then", "here", "there", "any"
    };
}

