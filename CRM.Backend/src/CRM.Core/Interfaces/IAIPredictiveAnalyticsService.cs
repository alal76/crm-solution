// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for AI-based predictive analytics.
/// Provides heuristic/rule-based predictions for lead scoring, churn risk, and deal win probability.
/// </summary>
public interface IAIPredictiveAnalyticsService
{
    /// <summary>
    /// Predict a lead's score based on demographic and engagement signals.
    /// </summary>
    Task<LeadScorePrediction> PredictLeadScoreAsync(int leadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Predict churn risk for an account based on activity recency, health score, and support ticket volume.
    /// </summary>
    Task<ChurnRiskPrediction> PredictChurnRiskAsync(int accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Predict the probability of winning an opportunity based on stage, age, amount, and engagement.
    /// </summary>
    Task<DealWinProbability> PredictDealWinProbabilityAsync(int opportunityId, CancellationToken cancellationToken = default);
}

#region Prediction DTOs

/// <summary>
/// Result of a lead score prediction.
/// </summary>
public class LeadScorePrediction
{
    /// <summary>Lead ID</summary>
    public int LeadId { get; set; }

    /// <summary>Predicted score (0–100)</summary>
    public int PredictedScore { get; set; }

    /// <summary>Confidence level (0.0–1.0)</summary>
    public double Confidence { get; set; }

    /// <summary>Factors that contributed to the score</summary>
    public List<PredictionFactor> Factors { get; set; } = new();

    /// <summary>When the prediction was generated (UTC)</summary>
    public DateTime PredictedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Result of a churn risk prediction.
/// </summary>
public class ChurnRiskPrediction
{
    /// <summary>Account ID</summary>
    public int AccountId { get; set; }

    /// <summary>Risk level: Low, Medium, High, Critical</summary>
    public string RiskLevel { get; set; } = "Low";

    /// <summary>Risk score (0.0–1.0)</summary>
    public double RiskScore { get; set; }

    /// <summary>Factors contributing to churn risk</summary>
    public List<PredictionFactor> Factors { get; set; } = new();

    /// <summary>Recommended actions to reduce churn risk</summary>
    public List<string> RecommendedActions { get; set; } = new();

    /// <summary>When the prediction was generated (UTC)</summary>
    public DateTime PredictedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Result of a deal win probability prediction.
/// </summary>
public class DealWinProbability
{
    /// <summary>Opportunity ID</summary>
    public int OpportunityId { get; set; }

    /// <summary>Predicted win probability (0.0–1.0)</summary>
    public double WinProbability { get; set; }

    /// <summary>Confidence level (0.0–1.0)</summary>
    public double Confidence { get; set; }

    /// <summary>Risk factors that may reduce win probability</summary>
    public List<PredictionFactor> RiskFactors { get; set; } = new();

    /// <summary>Positive factors that increase win probability</summary>
    public List<PredictionFactor> PositiveFactors { get; set; } = new();

    /// <summary>When the prediction was generated (UTC)</summary>
    public DateTime PredictedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// A single factor that influenced a prediction.
/// </summary>
public class PredictionFactor
{
    /// <summary>Factor name (e.g., "CompanySize", "EngagementScore")</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Human-readable description</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Impact weight (-1.0 to 1.0, negative = reduces score)</summary>
    public double Impact { get; set; }

    /// <summary>Raw value of the factor</summary>
    public string? Value { get; set; }
}

#endregion
