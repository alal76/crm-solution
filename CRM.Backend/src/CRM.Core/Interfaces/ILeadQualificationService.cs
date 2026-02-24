// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Lead Qualification Service Interface (TODO-CRM002-08)
/// Implements BANT/MEDDIC/CHAMP scoring models for lead qualification.
/// </summary>
public interface ILeadQualificationService
{
    /// <summary>
    /// Scores a lead using the BANT framework (Budget, Authority, Need, Timeline).
    /// Each criterion is scored 0-100. Combined score is the average.
    /// </summary>
    /// <param name="leadId">Lead ID to score</param>
    /// <param name="budgetScore">Budget score (0-100)</param>
    /// <param name="authorityScore">Authority score (0-100)</param>
    /// <param name="needScore">Need score (0-100)</param>
    /// <param name="timelineScore">Timeline score (0-100)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Combined qualification score (0-100)</returns>
    Task<LeadQualificationResult> ScoreWithBANTAsync(
        int leadId,
        int budgetScore,
        int authorityScore,
        int needScore,
        int timelineScore,
        CancellationToken ct = default);

    /// <summary>
    /// Scores a lead using the MEDDIC framework.
    /// (Metrics, Economic Buyer, Decision Criteria, Decision Process, Identify Pain, Champion)
    /// </summary>
    /// <param name="leadId">Lead ID to score</param>
    /// <param name="scores">MEDDIC dimension scores</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Combined qualification score</returns>
    Task<LeadQualificationResult> ScoreWithMEDDICAsync(
        int leadId,
        MEDDICScores scores,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the current qualification scores for a lead.
    /// </summary>
    /// <param name="leadId">Lead ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Current qualification data, or null if not scored</returns>
    Task<LeadQualificationResult?> GetQualificationAsync(int leadId, CancellationToken ct = default);

    /// <summary>
    /// Evaluates whether a lead meets the threshold for MQL or SQL status.
    /// </summary>
    /// <param name="leadId">Lead ID</param>
    /// <param name="mqlThreshold">Score threshold for MQL (default 50)</param>
    /// <param name="sqlThreshold">Score threshold for SQL (default 75)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Qualification evaluation result</returns>
    Task<QualificationEvaluation> EvaluateAsync(
        int leadId,
        int mqlThreshold = 50,
        int sqlThreshold = 75,
        CancellationToken ct = default);
}

/// <summary>
/// MEDDIC dimension scores input.
/// </summary>
public class MEDDICScores
{
    /// <summary>Metrics (quantified business impact) 0-100</summary>
    public int MetricsScore { get; set; }

    /// <summary>Economic Buyer identified/engaged 0-100</summary>
    public int EconomicBuyerScore { get; set; }

    /// <summary>Decision Criteria defined 0-100</summary>
    public int DecisionCriteriaScore { get; set; }

    /// <summary>Decision Process mapped 0-100</summary>
    public int DecisionProcessScore { get; set; }

    /// <summary>Pain identified/quantified 0-100</summary>
    public int IdentifyPainScore { get; set; }

    /// <summary>Champion identified/coached 0-100</summary>
    public int ChampionScore { get; set; }
}

/// <summary>
/// Result of a lead qualification scoring.
/// </summary>
public class LeadQualificationResult
{
    /// <summary>Lead ID</summary>
    public int LeadId { get; set; }

    /// <summary>Framework used (BANT, MEDDIC, etc.)</summary>
    public QualificationFramework Framework { get; set; }

    /// <summary>Combined qualification score (0-100)</summary>
    public int CombinedScore { get; set; }

    /// <summary>Individual dimension scores</summary>
    public Dictionary<string, int> DimensionScores { get; set; } = new();

    /// <summary>Qualification level (Unqualified, MQL, SQL)</summary>
    public string QualificationLevel { get; set; } = "Unqualified";

    /// <summary>Recommendations based on weak dimensions</summary>
    public List<string> Recommendations { get; set; } = new();

    /// <summary>When qualification was last updated</summary>
    public DateTime ScoredAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Result of evaluating whether a lead meets qualification thresholds.
/// </summary>
public class QualificationEvaluation
{
    /// <summary>Lead ID</summary>
    public int LeadId { get; set; }

    /// <summary>Current combined score</summary>
    public int CurrentScore { get; set; }

    /// <summary>Is Marketing Qualified Lead</summary>
    public bool IsMQL { get; set; }

    /// <summary>Is Sales Qualified Lead</summary>
    public bool IsSQL { get; set; }

    /// <summary>MQL threshold used</summary>
    public int MQLThreshold { get; set; }

    /// <summary>SQL threshold used</summary>
    public int SQLThreshold { get; set; }

    /// <summary>Weakest dimensions that need improvement</summary>
    public List<string> WeakDimensions { get; set; } = new();

    /// <summary>Suggested next actions</summary>
    public List<string> SuggestedActions { get; set; } = new();
}
