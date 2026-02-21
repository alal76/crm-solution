// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.AI;

namespace CRM.Core.Interfaces.AI;

/// <summary>
/// Enhanced AI Analytics Service interface for predictive analytics and intelligence.
/// Provides semantic search, advanced lead scoring, opportunity predictions, and sales forecasting.
/// </summary>
public interface IAIAnalyticsService
{
    #region Semantic Search

    /// <summary>
    /// Performs semantic search on knowledge base articles using AI embeddings.
    /// </summary>
    /// <param name="query">Natural language search query.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <param name="minRelevanceScore">Minimum relevance score (0-1) for results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of semantically relevant articles with scores.</returns>
    Task<SemanticSearchResult> SearchKnowledgeBaseAsync(
        string query,
        int maxResults = 10,
        double minRelevanceScore = 0.5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates and stores embeddings for a knowledge article.
    /// </summary>
    /// <param name="articleId">The article ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful.</returns>
    Task<bool> IndexKnowledgeArticleAsync(int articleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-indexes all knowledge articles with embeddings.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of articles indexed.</returns>
    Task<int> ReindexKnowledgeBaseAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Enhanced Lead Scoring

    /// <summary>
    /// Calculates an ML-enhanced lead score using multiple signals.
    /// </summary>
    /// <param name="leadId">The lead ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Enhanced lead score with detailed breakdown.</returns>
    Task<EnhancedLeadScore> CalculateEnhancedLeadScoreAsync(int leadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Predicts lead conversion probability using historical patterns.
    /// </summary>
    /// <param name="leadId">The lead ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Conversion prediction with confidence interval.</returns>
    Task<ConversionPrediction> PredictLeadConversionAsync(int leadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Identifies the optimal next action for a lead.
    /// </summary>
    /// <param name="leadId">The lead ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Recommended next action with reasoning.</returns>
    Task<NextBestAction> GetNextBestActionForLeadAsync(int leadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch calculates enhanced scores for multiple leads.
    /// </summary>
    /// <param name="leadIds">List of lead IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of enhanced scores.</returns>
    Task<List<EnhancedLeadScore>> BatchCalculateLeadScoresAsync(
        IEnumerable<int> leadIds,
        CancellationToken cancellationToken = default);

    #endregion

    #region Opportunity Predictions

    /// <summary>
    /// Predicts opportunity win probability using ML model.
    /// </summary>
    /// <param name="opportunityId">The opportunity ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Win prediction with factors.</returns>
    Task<WinPrediction> PredictOpportunityWinAsync(int opportunityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Predicts the expected close date for an opportunity.
    /// </summary>
    /// <param name="opportunityId">The opportunity ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Close date prediction with confidence.</returns>
    Task<CloseDatePrediction> PredictCloseDateAsync(int opportunityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Identifies risk factors for an opportunity.
    /// </summary>
    /// <param name="opportunityId">The opportunity ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Risk analysis with mitigation recommendations.</returns>
    Task<OpportunityRiskAnalysis> AnalyzeOpportunityRisksAsync(int opportunityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all at-risk opportunities with reasons.
    /// </summary>
    /// <param name="riskThreshold">Minimum risk level (0-1) to include.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of at-risk opportunities.</returns>
    Task<List<AtRiskOpportunity>> GetAtRiskOpportunitiesAsync(
        double riskThreshold = 0.3,
        CancellationToken cancellationToken = default);

    #endregion

    #region Sales Forecasting

    /// <summary>
    /// Generates a sales forecast for a given period.
    /// </summary>
    /// <param name="startDate">Forecast start date.</param>
    /// <param name="endDate">Forecast end date.</param>
    /// <param name="teamId">Optional team filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Sales forecast with breakdowns.</returns>
    Task<SalesForecastResult> GenerateSalesForecastAsync(
        DateTime startDate,
        DateTime endDate,
        int? teamId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Predicts pipeline health metrics.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Pipeline health analysis.</returns>
    Task<PipelineHealthAnalysis> AnalyzePipelineHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Identifies trends and patterns in sales data.
    /// </summary>
    /// <param name="periodMonths">Number of months to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Identified trends.</returns>
    Task<SalesTrendAnalysis> IdentifySalesTrendsAsync(int periodMonths = 12, CancellationToken cancellationToken = default);

    #endregion

    #region Account Intelligence

    /// <summary>
    /// Predicts account churn risk.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Churn risk prediction.</returns>
    Task<ChurnPrediction> PredictAccountChurnAsync(int accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Identifies upsell/cross-sell opportunities for an account.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Product recommendations.</returns>
    Task<ProductRecommendations> GetProductRecommendationsAsync(int accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates account lifetime value prediction.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>CLV prediction.</returns>
    Task<CLVPrediction> PredictAccountLifetimeValueAsync(int accountId, CancellationToken cancellationToken = default);

    #endregion
}

#region DTOs

/// <summary>
/// Result from semantic search.
/// </summary>
public class SemanticSearchResult
{
    public string Query { get; set; } = string.Empty;
    public int TotalResults { get; set; }
    public TimeSpan SearchTime { get; set; }
    public List<SemanticSearchItem> Items { get; set; } = new();
}

/// <summary>
/// Individual semantic search result item.
/// </summary>
public class SemanticSearchItem
{
    public int ArticleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Category { get; set; }
    public double RelevanceScore { get; set; }
    public string? HighlightedContent { get; set; }
    public List<string> MatchedKeywords { get; set; } = new();
}

/// <summary>
/// Enhanced lead score with detailed breakdown.
/// </summary>
public class EnhancedLeadScore
{
    public int LeadId { get; set; }
    public string LeadName { get; set; } = string.Empty;
    public int OverallScore { get; set; }
    public string ScoreCategory { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public DateTime ScoredAt { get; set; } = DateTime.UtcNow;

    // Score Components
    public int DemographicScore { get; set; }
    public int FirmographicScore { get; set; }
    public int BehavioralScore { get; set; }
    public int EngagementScore { get; set; }
    public int IntentScore { get; set; }
    public int RecencyScore { get; set; }
    public int FitScore { get; set; }

    // Score Factors
    public List<ScoreFactor> PositiveFactors { get; set; } = new();
    public List<ScoreFactor> NegativeFactors { get; set; } = new();

    // Predictions
    public double ConversionProbability { get; set; }
    public int? EstimatedDaysToConversion { get; set; }
    public string? PredictedOutcome { get; set; }
}

/// <summary>
/// Factor contributing to a score.
/// </summary>
public class ScoreFactor
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Impact { get; set; }
    public string Category { get; set; } = string.Empty;
}

/// <summary>
/// Lead conversion prediction.
/// </summary>
public class ConversionPrediction
{
    public int LeadId { get; set; }
    public double Probability { get; set; }
    public double LowerBound { get; set; }
    public double UpperBound { get; set; }
    public double Confidence { get; set; }
    public string Outcome { get; set; } = string.Empty;
    public int? DaysToConversion { get; set; }
    public List<string> KeyFactors { get; set; } = new();
    public DateTime PredictedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Recommended next action for a lead.
/// </summary>
public class NextBestAction
{
    public int LeadId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Timing { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
    public double ImpactScore { get; set; }
    public List<string> TalkingPoints { get; set; } = new();
}

/// <summary>
/// Opportunity win prediction.
/// </summary>
public class WinPrediction
{
    public int OpportunityId { get; set; }
    public string OpportunityName { get; set; } = string.Empty;
    public double WinProbability { get; set; }
    public string WinCategory { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public decimal WeightedValue { get; set; }
    public List<WinFactor> PositiveFactors { get; set; } = new();
    public List<WinFactor> RiskFactors { get; set; } = new();
    public string RecommendedAction { get; set; } = string.Empty;
    public DateTime PredictedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Factor affecting win probability.
/// </summary>
public class WinFactor
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Impact { get; set; }
    public string Category { get; set; } = string.Empty;
}

/// <summary>
/// Close date prediction.
/// </summary>
public class CloseDatePrediction
{
    public int OpportunityId { get; set; }
    public DateTime PredictedCloseDate { get; set; }
    public DateTime EarliestDate { get; set; }
    public DateTime LatestDate { get; set; }
    public double Confidence { get; set; }
    public int DaysFromNow { get; set; }
    public string Stage { get; set; } = string.Empty;
    public List<string> Assumptions { get; set; } = new();
}

/// <summary>
/// Opportunity risk analysis.
/// </summary>
public class OpportunityRiskAnalysis
{
    public int OpportunityId { get; set; }
    public double OverallRiskScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public List<RiskFactor> Risks { get; set; } = new();
    public List<MitigationAction> Mitigations { get; set; } = new();
}

/// <summary>
/// Individual risk factor.
/// </summary>
public class RiskFactor
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Severity { get; set; }
    public double Probability { get; set; }
    public string Impact { get; set; } = string.Empty;
}

/// <summary>
/// Mitigation action for a risk.
/// </summary>
public class MitigationAction
{
    public string RiskName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
}

/// <summary>
/// At-risk opportunity summary.
/// </summary>
public class AtRiskOpportunity
{
    public int OpportunityId { get; set; }
    public string OpportunityName { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Stage { get; set; } = string.Empty;
    public DateTime? ExpectedCloseDate { get; set; }
    public double RiskScore { get; set; }
    public string PrimaryRisk { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
}

/// <summary>
/// Sales forecast.
/// </summary>
public class SalesForecastResult
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? TeamId { get; set; }
    public decimal TotalForecast { get; set; }
    public decimal BestCase { get; set; }
    public decimal WorstCase { get; set; }
    public decimal Committed { get; set; }
    public decimal Pipeline { get; set; }
    public double Confidence { get; set; }
    public List<ForecastByStage> ByStage { get; set; } = new();
    public List<ForecastByMonth> ByMonth { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Forecast breakdown by stage.
/// </summary>
public class ForecastByStage
{
    public string Stage { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
    public double AverageWinProbability { get; set; }
    public decimal WeightedAmount { get; set; }
}

/// <summary>
/// Forecast breakdown by month.
/// </summary>
public class ForecastByMonth
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Forecast { get; set; }
    public decimal Committed { get; set; }
    public decimal BestCase { get; set; }
}

/// <summary>
/// Pipeline health analysis.
/// </summary>
public class PipelineHealthAnalysis
{
    public decimal TotalPipelineValue { get; set; }
    public int TotalOpportunities { get; set; }
    public double OverallHealthScore { get; set; }
    public string HealthStatus { get; set; } = string.Empty;
    public decimal AverageDealSize { get; set; }
    public double AverageWinRate { get; set; }
    public int AverageSalesCycleDays { get; set; }
    public List<PipelineIssue> Issues { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Pipeline issue.
/// </summary>
public class PipelineIssue
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public int AffectedOpportunities { get; set; }
    public decimal AffectedValue { get; set; }
}

/// <summary>
/// Sales trend analysis.
/// </summary>
public class SalesTrendAnalysis
{
    public int PeriodMonths { get; set; }
    public List<TrendItem> RevenueNotes { get; set; } = new();
    public List<TrendItem> WinRateTrend { get; set; } = new();
    public List<TrendItem> DealSizeTrend { get; set; } = new();
    public List<TrendItem> SalesCycleTrend { get; set; } = new();
    public List<string> Insights { get; set; } = new();
    public string OverallDirection { get; set; } = string.Empty;
}

/// <summary>
/// Trend data point.
/// </summary>
public class TrendItem
{
    public DateTime Period { get; set; }
    public double Value { get; set; }
    public double Change { get; set; }
    public string Direction { get; set; } = string.Empty;
}

/// <summary>
/// Account churn prediction.
/// </summary>
public class ChurnPrediction
{
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public double ChurnProbability { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public List<string> RiskFactors { get; set; } = new();
    public List<string> RetentionActions { get; set; } = new();
    public DateTime PredictedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Product recommendations for cross-sell/upsell.
/// </summary>
public class ProductRecommendations
{
    public int AccountId { get; set; }
    public List<ProductRecommendation> Recommendations { get; set; } = new();
    public decimal EstimatedUpsellPotential { get; set; }
}

/// <summary>
/// Individual product recommendation.
/// </summary>
public class ProductRecommendation
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string RecommendationType { get; set; } = string.Empty;
    public double Relevance { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public decimal? EstimatedValue { get; set; }
}

/// <summary>
/// Account lifetime value prediction.
/// </summary>
public class CLVPrediction
{
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public decimal PredictedCLV { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal PotentialValue { get; set; }
    public int? ExpectedLifetimeMonths { get; set; }
    public string Segment { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

#endregion
