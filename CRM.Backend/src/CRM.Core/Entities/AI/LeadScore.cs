// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities.AI;

#region Lead Score Enumerations

/// <summary>
/// Lead score category based on probability to convert.
/// </summary>
public enum LeadScoreCategory
{
    /// <summary>Very low conversion probability (0-20%)</summary>
    Cold = 0,

    /// <summary>Low conversion probability (20-40%)</summary>
    Cool = 1,

    /// <summary>Medium conversion probability (40-60%)</summary>
    Warm = 2,

    /// <summary>High conversion probability (60-80%)</summary>
    Hot = 3,

    /// <summary>Very high conversion probability (80-100%)</summary>
    OnFire = 4
}

/// <summary>
/// Lead engagement level.
/// </summary>
public enum LeadEngagementLevel
{
    /// <summary>No engagement</summary>
    None = 0,

    /// <summary>Low engagement (1-2 touches)</summary>
    Low = 1,

    /// <summary>Medium engagement (3-5 touches)</summary>
    Medium = 2,

    /// <summary>High engagement (6-10 touches)</summary>
    High = 3,

    /// <summary>Very high engagement (10+ touches)</summary>
    VeryHigh = 4
}

/// <summary>
/// Lead intent signal strength.
/// </summary>
public enum IntentSignalStrength
{
    /// <summary>No intent signals</summary>
    None = 0,

    /// <summary>Weak intent signals</summary>
    Weak = 1,

    /// <summary>Moderate intent signals</summary>
    Moderate = 2,

    /// <summary>Strong intent signals</summary>
    Strong = 3,

    /// <summary>Very strong purchase intent</summary>
    VeryStrong = 4
}

#endregion

/// <summary>
/// AI-generated lead score for predictive lead qualification.
/// Uses Allen AI OLMo/Tulu models for scoring.
/// </summary>
public class LeadScore : BaseEntity
{
    #region Lead Reference

    /// <summary>Lead ID</summary>
    public int LeadId { get; set; }

    /// <summary>Navigation to Lead</summary>
    public Lead? Lead { get; set; }

    #endregion

    #region Overall Score

    /// <summary>Overall lead score (0-100)</summary>
    public decimal OverallScore { get; set; }

    /// <summary>Score category</summary>
    public LeadScoreCategory Category { get; set; }

    /// <summary>Confidence in the score (0-1)</summary>
    public decimal Confidence { get; set; }

    /// <summary>Score trend (positive = improving)</summary>
    public decimal ScoreTrend { get; set; }

    #endregion

    #region Component Scores

    /// <summary>Demographic fit score (0-100) - company size, industry, location match</summary>
    public decimal DemographicScore { get; set; }

    /// <summary>Firmographic score (0-100) - revenue, employee count, tech stack</summary>
    public decimal FirmographicScore { get; set; }

    /// <summary>Behavioral score (0-100) - website visits, email opens, content downloads</summary>
    public decimal BehavioralScore { get; set; }

    /// <summary>Engagement score (0-100) - response rate, meeting attendance</summary>
    public decimal EngagementScore { get; set; }

    /// <summary>Intent score (0-100) - purchase signals, competitor research</summary>
    public decimal IntentScore { get; set; }

    #endregion

    #region Engagement Metrics

    /// <summary>Engagement level</summary>
    public LeadEngagementLevel EngagementLevel { get; set; }

    /// <summary>Intent signal strength</summary>
    public IntentSignalStrength IntentStrength { get; set; }

    /// <summary>Days since last activity</summary>
    public int DaysSinceLastActivity { get; set; }

    /// <summary>Total touch count</summary>
    public int TotalTouches { get; set; }

    /// <summary>Email open rate (0-1)</summary>
    public decimal? EmailOpenRate { get; set; }

    /// <summary>Email click rate (0-1)</summary>
    public decimal? EmailClickRate { get; set; }

    /// <summary>Website sessions count</summary>
    public int? WebsiteSessions { get; set; }

    /// <summary>Pages viewed</summary>
    public int? PagesViewed { get; set; }

    /// <summary>Content downloads</summary>
    public int? ContentDownloads { get; set; }

    #endregion

    #region AI Analysis

    /// <summary>Predicted conversion probability (0-1)</summary>
    public decimal ConversionProbability { get; set; }

    /// <summary>Estimated days to conversion</summary>
    public int? EstimatedDaysToConversion { get; set; }

    /// <summary>Estimated deal value</summary>
    public decimal? EstimatedDealValue { get; set; }

    /// <summary>Best product/service fit</summary>
    public string? BestProductFit { get; set; }

    /// <summary>Top scoring factors (JSON array)</summary>
    public string? TopFactorsJson { get; set; }

    /// <summary>Key risk factors (JSON array)</summary>
    public string? RiskFactorsJson { get; set; }

    /// <summary>AI-generated insights</summary>
    public string? AIInsights { get; set; }

    #endregion

    #region ICP Matching

    /// <summary>Ideal Customer Profile match percentage (0-100)</summary>
    public decimal ICPMatchScore { get; set; }

    /// <summary>Similar won deals count</summary>
    public int? SimilarWonDealsCount { get; set; }

    /// <summary>Similar lost deals count</summary>
    public int? SimilarLostDealsCount { get; set; }

    /// <summary>Best matching customer segment</summary>
    public string? MatchingSegment { get; set; }

    #endregion

    #region Scoring Metadata

    /// <summary>When score was calculated</summary>
    public DateTime ScoredAt { get; set; } = DateTime.UtcNow;

    /// <summary>When score expires</summary>
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

    /// <summary>Scoring model version</summary>
    public string ModelVersion { get; set; } = "1.0";

    /// <summary>Previous score (for trend)</summary>
    public decimal? PreviousScore { get; set; }

    /// <summary>AI Model used for scoring</summary>
    public int? AIModelId { get; set; }

    /// <summary>Navigation to AI Model</summary>
    public AIModel? AIModel { get; set; }

    #endregion
}
