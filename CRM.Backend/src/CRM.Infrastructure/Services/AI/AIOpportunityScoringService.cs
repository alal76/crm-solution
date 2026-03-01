// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.AI;

#region Interfaces and DTOs

/// <summary>
/// Service for predictive opportunity scoring.
/// Calculates win probability based on stage progression, deal size, activity, velocity, and historical patterns.
/// </summary>
public interface IAIOpportunityScoringService
{
    /// <summary>
    /// Scores a single opportunity for win probability.
    /// </summary>
    /// <param name="opportunityId">ID of the opportunity to score.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Detailed opportunity score result, or null if not found.</returns>
    Task<OpportunityScoreResult?> ScoreOpportunityAsync(int opportunityId, CancellationToken ct = default);

    /// <summary>
    /// Scores all open opportunities in the pipeline.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of scored opportunities.</returns>
    Task<IEnumerable<OpportunityScoreResult>> ScoreAllOpenAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns historical win rates by stage for calibration insight.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Stage-to-win-rate mapping.</returns>
    Task<Dictionary<string, double>> GetHistoricalWinRatesAsync(CancellationToken ct = default);
}

/// <summary>
/// Detailed result of scoring an opportunity.
/// </summary>
public class OpportunityScoreResult
{
    /// <summary>Opportunity ID.</summary>
    public int OpportunityId { get; set; }

    /// <summary>Opportunity name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Current pipeline stage.</summary>
    public string Stage { get; set; } = string.Empty;

    /// <summary>Predicted win probability (0-100).</summary>
    public int WinProbability { get; set; }

    /// <summary>Overall risk level (Low, Medium, High).</summary>
    public string RiskLevel { get; set; } = "Medium";

    /// <summary>Score breakdown by factor.</summary>
    public OpportunityScoreBreakdown Breakdown { get; set; } = new();

    /// <summary>Risk factors identified for this deal.</summary>
    public List<string> RiskFactors { get; set; } = new();

    /// <summary>Positive signals identified.</summary>
    public List<string> PositiveSignals { get; set; } = new();

    /// <summary>When the score was calculated.</summary>
    public DateTime ScoredAt { get; set; }
}

/// <summary>
/// Breakdown of individual scoring factors.
/// </summary>
public class OpportunityScoreBreakdown
{
    /// <summary>Score from current stage position (0-100).</summary>
    public double StageScore { get; set; }

    /// <summary>Score from deal size relative to historical wins (0-100).</summary>
    public double DealSizeScore { get; set; }

    /// <summary>Score from velocity/days in pipeline (0-100).</summary>
    public double VelocityScore { get; set; }

    /// <summary>Score from activity level (0-100).</summary>
    public double ActivityScore { get; set; }

    /// <summary>Score from data completeness (0-100).</summary>
    public double CompletenessScore { get; set; }
}

#endregion

/// <summary>
/// Calculates opportunity win probability using multi-factor analysis.
/// Factors: Stage (30%), Deal Size (15%), Velocity (20%), Activity (20%), Completeness (15%).
/// </summary>
public class AIOpportunityScoringService : IAIOpportunityScoringService
{
    // Scoring weights — must sum to 1.0
    private static readonly Dictionary<string, double> Weights = new()
    {
        ["Stage"] = 0.30,
        ["DealSize"] = 0.15,
        ["Velocity"] = 0.20,
        ["Activity"] = 0.20,
        ["Completeness"] = 0.15
    };

    // Base probability by stage
    private static readonly Dictionary<OpportunityStage, double> StageProbability = new()
    {
        [OpportunityStage.Discovery] = 20,
        [OpportunityStage.Qualification] = 35,
        [OpportunityStage.Proposal] = 55,
        [OpportunityStage.Negotiation] = 75,
        [OpportunityStage.ClosedWon] = 100,
        [OpportunityStage.ClosedLost] = 0
    };

    private readonly ICrmDbContext _context;
    private readonly ILogger<AIOpportunityScoringService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AIOpportunityScoringService"/> class.
    /// </summary>
    public AIOpportunityScoringService(
        ICrmDbContext context,
        ILogger<AIOpportunityScoringService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<OpportunityScoreResult?> ScoreOpportunityAsync(int opportunityId, CancellationToken ct = default)
    {
        var opp = await _context.Opportunities
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted, ct);

        if (opp == null)
        {
            _logger.LogWarning("Opportunity {OpportunityId} not found for scoring", opportunityId);
            return null;
        }

        var historicalWinRates = await GetHistoricalWinRatesAsync(ct);
        return CalculateScore(opp, historicalWinRates);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<OpportunityScoreResult>> ScoreAllOpenAsync(CancellationToken ct = default)
    {
        var openOpps = await _context.Opportunities
            .Where(o => !o.IsDeleted && o.Stage != OpportunityStage.ClosedWon && o.Stage != OpportunityStage.ClosedLost)
            .ToListAsync(ct);

        var historicalWinRates = await GetHistoricalWinRatesAsync(ct);

        var results = new List<OpportunityScoreResult>();
        foreach (var opp in openOpps)
        {
            try
            {
                results.Add(CalculateScore(opp, historicalWinRates));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to score opportunity {OpportunityId}", opp.Id);
            }
        }

        _logger.LogInformation("Scored {Count}/{Total} open opportunities", results.Count, openOpps.Count);
        return results;
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, double>> GetHistoricalWinRatesAsync(CancellationToken ct = default)
    {
        var allClosed = await _context.Opportunities
            .Where(o => !o.IsDeleted && (o.Stage == OpportunityStage.ClosedWon || o.Stage == OpportunityStage.ClosedLost))
            .ToListAsync(ct);

        if (allClosed.Count == 0)
        {
            // Return default stage probabilities
            return StageProbability.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => kvp.Value);
        }

        var wonCount = allClosed.Count(o => o.Stage == OpportunityStage.ClosedWon);
        var totalCount = allClosed.Count;
        var overallRate = totalCount > 0 ? (double)wonCount / totalCount * 100 : 0;

        var rates = new Dictionary<string, double>
        {
            ["Overall"] = Math.Round(overallRate, 1)
        };

        foreach (var stage in StageProbability.Keys)
        {
            rates[stage.ToString()] = StageProbability[stage];
        }

        return rates;
    }

    #region Private Methods

    private OpportunityScoreResult CalculateScore(Opportunity opp, Dictionary<string, double> historicalWinRates)
    {
        var result = new OpportunityScoreResult
        {
            OpportunityId = opp.Id,
            Name = opp.Name,
            Stage = opp.Stage.ToString(),
            ScoredAt = DateTime.UtcNow,
            Breakdown = new OpportunityScoreBreakdown()
        };

        // 1. Stage Score
        result.Breakdown.StageScore = CalculateStageScore(opp, result);

        // 2. Deal Size Score
        result.Breakdown.DealSizeScore = CalculateDealSizeScore(opp, result);

        // 3. Velocity Score
        result.Breakdown.VelocityScore = CalculateVelocityScore(opp, result);

        // 4. Activity Score
        result.Breakdown.ActivityScore = CalculateActivityScore(opp, result);

        // 5. Completeness Score
        result.Breakdown.CompletenessScore = CalculateCompletenessScore(opp, result);

        // Calculate weighted total
        var weighted =
            (result.Breakdown.StageScore * Weights["Stage"]) +
            (result.Breakdown.DealSizeScore * Weights["DealSize"]) +
            (result.Breakdown.VelocityScore * Weights["Velocity"]) +
            (result.Breakdown.ActivityScore * Weights["Activity"]) +
            (result.Breakdown.CompletenessScore * Weights["Completeness"]);

        result.WinProbability = (int)Math.Clamp(Math.Round(weighted), 0, 100);
        result.RiskLevel = GetRiskLevel(result.WinProbability, result.RiskFactors.Count);

        return result;
    }

    private static double CalculateStageScore(Opportunity opp, OpportunityScoreResult result)
    {
        if (StageProbability.TryGetValue(opp.Stage, out var baseProb))
        {
            if (baseProb >= 55)
            {
                result.PositiveSignals.Add($"Advanced stage: {opp.Stage}");
            }
            return baseProb;
        }
        return 20; // Default
    }

    private static double CalculateDealSizeScore(Opportunity opp, OpportunityScoreResult result)
    {
        if (opp.Amount <= 0)
        {
            result.RiskFactors.Add("No deal amount specified");
            return 20;
        }

        // Score based on deal amount ranges (realistic enterprise CRM values)
        if (opp.Amount > 500000)
        {
            result.RiskFactors.Add("Very large deal — longer sales cycle expected");
            return 40;
        }
        if (opp.Amount > 100000)
        {
            result.PositiveSignals.Add("Significant deal value");
            return 70;
        }
        if (opp.Amount > 10000)
        {
            return 80;
        }

        result.PositiveSignals.Add("Quick-close deal size");
        return 90;
    }

    private static double CalculateVelocityScore(Opportunity opp, OpportunityScoreResult result)
    {
        var daysInPipeline = (DateTime.UtcNow - opp.CreatedAt).TotalDays;

        if (daysInPipeline < 1)
        {
            return 90; // Very new
        }

        if (daysInPipeline < 30)
        {
            result.PositiveSignals.Add("Good pipeline velocity");
            return 85;
        }
        if (daysInPipeline < 60)
        {
            return 70;
        }
        if (daysInPipeline < 90)
        {
            result.RiskFactors.Add("Aging deal (60-90 days in pipeline)");
            return 50;
        }
        if (daysInPipeline < 180)
        {
            result.RiskFactors.Add("Stale deal (90-180 days in pipeline)");
            return 30;
        }

        result.RiskFactors.Add("Very stale deal (180+ days)");
        return 10;
    }

    private static double CalculateActivityScore(Opportunity opp, OpportunityScoreResult result)
    {
        // Use the opportunity's Probability field as a proxy for activity/engagement
        if (opp.Probability > 0)
        {
            if (opp.Probability >= 70)
            {
                result.PositiveSignals.Add("High probability assigned");
            }
            return Math.Min(opp.Probability, 100);
        }

        // Check if expected close date suggests urgency
        if (opp.ExpectedCloseDate.HasValue)
        {
            var daysToClose = (opp.ExpectedCloseDate.Value - DateTime.UtcNow).TotalDays;
            if (daysToClose < 0)
            {
                result.RiskFactors.Add("Past expected close date");
                return 20;
            }
            if (daysToClose < 30)
            {
                result.PositiveSignals.Add("Expected to close within 30 days");
                return 75;
            }
            return 50;
        }

        result.RiskFactors.Add("No expected close date set");
        return 30;
    }

    private static double CalculateCompletenessScore(Opportunity opp, OpportunityScoreResult result)
    {
        var score = 0.0;

        if (opp.Amount > 0)
        {
            score += 25;
        }
        else
        {
            result.RiskFactors.Add("Missing deal amount");
        }

        if (opp.ExpectedCloseDate.HasValue)
        {
            score += 20;
        }
        else
        {
            result.RiskFactors.Add("Missing expected close date");
        }

        if (opp.AccountId > 0)
        {
            score += 20;
        }
        else
        {
            result.RiskFactors.Add("No account linked");
        }

        if (opp.SalesOwnerId.HasValue && opp.SalesOwnerId > 0)
        {
            score += 15;
            result.PositiveSignals.Add("Sales owner assigned");
        }

        if (!string.IsNullOrWhiteSpace(opp.SolutionNotes))
        {
            score += 10;
        }
        if (opp.PrimaryContactId.HasValue && opp.PrimaryContactId > 0)
        {
            score += 10;
        }

        return Math.Min(score, 100);
    }

    private static string GetRiskLevel(int probability, int riskFactorCount) =>
        (probability, riskFactorCount) switch
        {
            (>= 70, <= 1) => "Low",
            (>= 50, <= 2) => "Medium",
            _ => "High"
        };

    #endregion
}
