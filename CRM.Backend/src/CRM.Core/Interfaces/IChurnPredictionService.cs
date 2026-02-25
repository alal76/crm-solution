// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for predicting customer churn risk.
/// Implements TODO-AI-03.
/// </summary>
public interface IChurnPredictionService
{
    /// <summary>
    /// Predicts churn risk for a given account.
    /// </summary>
    /// <param name="accountId">The account ID to assess.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Churn prediction result, or null if account not found.</returns>
    Task<ChurnPredictionDto?> PredictChurnAsync(int accountId, CancellationToken ct = default);
}

/// <summary>
/// Churn risk level categories.
/// </summary>
public enum ChurnRiskLevel
{
    /// <summary>Low churn risk (&lt;30%)</summary>
    Low = 0,

    /// <summary>Medium churn risk (30-70%)</summary>
    Medium = 1,

    /// <summary>High churn risk (&gt;70%)</summary>
    High = 2
}

/// <summary>
/// Result of a churn prediction analysis.
/// </summary>
public class ChurnPredictionDto
{
    /// <summary>Account ID that was assessed.</summary>
    public int AccountId { get; set; }

    /// <summary>Churn probability between 0.0 and 1.0.</summary>
    public double ChurnProbability { get; set; }

    /// <summary>Risk classification: Low, Medium, High.</summary>
    public ChurnRiskLevel RiskLevel { get; set; }

    /// <summary>Key factors driving the churn risk score.</summary>
    public string[] KeyFactors { get; set; } = Array.Empty<string>();

    /// <summary>Recommended actions to reduce churn risk.</summary>
    public string[] RecommendedActions { get; set; } = Array.Empty<string>();

    /// <summary>When the prediction was calculated.</summary>
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}
