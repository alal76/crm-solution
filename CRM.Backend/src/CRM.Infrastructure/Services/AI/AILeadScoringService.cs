// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.Features;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace CRM.Infrastructure.Services.AI;

#region Interfaces and DTOs

/// <summary>
/// Service for AI-enhanced lead scoring.
/// Calculates composite scores from weighted factors (completeness, engagement, fit, recency, source).
/// Optionally uses AI for sentiment analysis enrichment.
/// </summary>
public interface IAILeadScoringService
{
    /// <summary>
    /// Scores a single lead based on data completeness, engagement, fit, recency, and source.
    /// </summary>
    /// <param name="leadId">ID of the lead to score.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Detailed lead score breakdown, or null if lead not found.</returns>
    Task<LeadScoreResult?> ScoreLeadAsync(int leadId, CancellationToken ct = default);

    /// <summary>
    /// Scores all unscored or stale leads in the system.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Summary of the batch scoring operation.</returns>
    Task<BatchScoreResult> ScoreAllLeadsAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns the current scoring weights for transparency.
    /// </summary>
    /// <returns>Dictionary of factor names to weight percentages.</returns>
    Dictionary<string, double> GetScoringWeights();
}

/// <summary>
/// Detailed result of scoring a single lead.
/// </summary>
public class LeadScoreResult
{
    /// <summary>Lead ID.</summary>
    public int LeadId { get; set; }

    /// <summary>Overall composite score (0-100).</summary>
    public int TotalScore { get; set; }

    /// <summary>Letter grade (A, B, C, D, F).</summary>
    public string Grade { get; set; } = "F";

    /// <summary>Data completeness score component (0-100).</summary>
    public double CompletenessScore { get; set; }

    /// <summary>Engagement level score component (0-100).</summary>
    public double EngagementScore { get; set; }

    /// <summary>Fit/ICP match score component (0-100).</summary>
    public double FitScore { get; set; }

    /// <summary>Recency/freshness score component (0-100).</summary>
    public double RecencyScore { get; set; }

    /// <summary>Source quality score component (0-100).</summary>
    public double SourceScore { get; set; }

    /// <summary>Optional AI sentiment score (-1.0 to 1.0).</summary>
    public double? SentimentScore { get; set; }

    /// <summary>Factors that contributed to the score.</summary>
    public List<string> Factors { get; set; } = new();

    /// <summary>When the score was last calculated.</summary>
    public DateTime ScoredAt { get; set; }
}

/// <summary>
/// Summary of a batch scoring run.
/// </summary>
public class BatchScoreResult
{
    /// <summary>Total leads processed.</summary>
    public int TotalProcessed { get; set; }

    /// <summary>Leads scored successfully.</summary>
    public int Succeeded { get; set; }

    /// <summary>Leads that failed scoring.</summary>
    public int Failed { get; set; }

    /// <summary>Average score across all scored leads.</summary>
    public double AverageScore { get; set; }
}

#endregion

/// <summary>
/// Calculates lead scores using weighted factor analysis.
/// Factors: Completeness (20%), Engagement (25%), Fit (25%), Recency (15%), Source (15%).
/// Optionally enriches with AI sentiment analysis.
/// </summary>
public class AILeadScoringService : IAILeadScoringService
{
    private readonly ICrmDbContext _context;
    private readonly IServiceProvider _serviceProvider;
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<AILeadScoringService> _logger;

    // Scoring weights — must sum to 1.0
    private static readonly Dictionary<string, double> Weights = new()
    {
        ["Completeness"] = 0.20,
        ["Engagement"] = 0.25,
        ["Fit"] = 0.25,
        ["Recency"] = 0.15,
        ["Source"] = 0.15
    };

    // Source quality map
    private static readonly Dictionary<LeadSource, double> SourceQuality = new()
    {
        [LeadSource.Referral] = 100,
        [LeadSource.Partner] = 90,
        [LeadSource.Event] = 80,
        [LeadSource.Campaign] = 70,
        [LeadSource.Web] = 50,
        [LeadSource.Manual] = 40
    };

    /// <summary>
    /// Initializes a new instance of AILeadScoringService.
    /// </summary>
    public AILeadScoringService(
        ICrmDbContext context,
        IServiceProvider serviceProvider,
        IFeatureManager featureManager,
        ILogger<AILeadScoringService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<LeadScoreResult?> ScoreLeadAsync(int leadId, CancellationToken ct = default)
    {
        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == leadId && !l.IsDeleted, ct);

        if (lead == null)
        {
            _logger.LogWarning("Lead {LeadId} not found for scoring", leadId);
            return null;
        }

        return await CalculateScoreAsync(lead, ct);
    }

    /// <inheritdoc />
    public async Task<BatchScoreResult> ScoreAllLeadsAsync(CancellationToken ct = default)
    {
        var leads = await _context.Leads
            .Where(l => !l.IsDeleted && l.Status != LeadLifecycleStatus.Converted && l.Status != LeadLifecycleStatus.Disqualified)
            .ToListAsync(ct);

        var result = new BatchScoreResult();
        var totalScore = 0.0;

        foreach (var lead in leads)
        {
            try
            {
                var score = await CalculateScoreAsync(lead, ct);
                if (score != null)
                {
                    lead.Score = score.TotalScore;
                    lead.FitScore = (int)score.FitScore;
                    lead.EngagementScore = (int)score.EngagementScore;
                    lead.UpdatedAt = DateTime.UtcNow;
                    result.Succeeded++;
                    totalScore += score.TotalScore;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to score lead {LeadId}", lead.Id);
                result.Failed++;
            }
        }

        result.TotalProcessed = leads.Count;
        result.AverageScore = result.Succeeded > 0 ? totalScore / result.Succeeded : 0;

        if (result.Succeeded > 0)
        {
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Batch scored {Succeeded}/{Total} leads, average score {Avg:F1}",
                result.Succeeded, result.TotalProcessed, result.AverageScore);
        }

        return result;
    }

    /// <inheritdoc />
    public Dictionary<string, double> GetScoringWeights() => new(Weights);

    #region Private Methods

    private async Task<LeadScoreResult> CalculateScoreAsync(Lead lead, CancellationToken ct)
    {
        var result = new LeadScoreResult
        {
            LeadId = lead.Id,
            ScoredAt = DateTime.UtcNow,
            Factors = new List<string>()
        };

        // 1. Completeness Score (0-100)
        result.CompletenessScore = CalculateCompletenessScore(lead, result.Factors);

        // 2. Engagement Score (0-100)
        result.EngagementScore = CalculateEngagementScore(lead, result.Factors);

        // 3. Fit Score (0-100)
        result.FitScore = CalculateFitScore(lead, result.Factors);

        // 4. Recency Score (0-100)
        result.RecencyScore = CalculateRecencyScore(lead, result.Factors);

        // 5. Source Score (0-100)
        result.SourceScore = CalculateSourceScore(lead, result.Factors);

        // Optional: AI sentiment enrichment
        result.SentimentScore = await GetSentimentScoreAsync(lead, ct);

        // Calculate weighted total
        var weighted =
            result.CompletenessScore * Weights["Completeness"] +
            result.EngagementScore * Weights["Engagement"] +
            result.FitScore * Weights["Fit"] +
            result.RecencyScore * Weights["Recency"] +
            result.SourceScore * Weights["Source"];

        // Boost/penalize by sentiment if available
        if (result.SentimentScore.HasValue)
        {
            weighted += result.SentimentScore.Value * 5; // +/- 5 points max
            result.Factors.Add($"Sentiment adjustment: {result.SentimentScore.Value:+0.0;-0.0}");
        }

        result.TotalScore = (int)Math.Clamp(Math.Round(weighted), 0, 100);
        result.Grade = GetGrade(result.TotalScore);

        return result;
    }

    private static double CalculateCompletenessScore(Lead lead, List<string> factors)
    {
        var score = 0.0;
        var checks = new List<(bool has, double weight, string name)>
        {
            (!string.IsNullOrWhiteSpace(lead.Email), 20, "Email"),
            (!string.IsNullOrWhiteSpace(lead.Phone), 15, "Phone"),
            (!string.IsNullOrWhiteSpace(lead.CompanyName), 20, "Company"),
            (!string.IsNullOrWhiteSpace(lead.Title), 15, "Title"),
            (!string.IsNullOrWhiteSpace(lead.FirstName) && !string.IsNullOrWhiteSpace(lead.LastName), 15, "FullName"),
            (!string.IsNullOrWhiteSpace(lead.Website), 10, "Website"),
            (lead.CampaignId.HasValue, 5, "Campaign")
        };

        foreach (var (has, weight, name) in checks)
        {
            if (has)
            {
                score += weight;
            }
            else
            {
                factors.Add($"Missing: {name}");
            }
        }

        return score;
    }

    private static double CalculateEngagementScore(Lead lead, List<string> factors)
    {
        var score = 0.0;

        // Use existing engagement score if available
        if (lead.EngagementScore > 0)
        {
            score = Math.Min(lead.EngagementScore, 100);
            factors.Add($"Existing engagement score: {lead.EngagementScore}");
            return score;
        }

        // Estimate from status progression
        switch (lead.Status)
        {
            case LeadLifecycleStatus.Qualified:
                score = 80;
                factors.Add("Status: Qualified (high engagement)");
                break;
            case LeadLifecycleStatus.Working:
                score = 60;
                factors.Add("Status: Working (moderate engagement)");
                break;
            case LeadLifecycleStatus.Nurturing:
                score = 40;
                factors.Add("Status: Nurturing (developing engagement)");
                break;
            case LeadLifecycleStatus.New:
                score = 20;
                factors.Add("Status: New (low engagement)");
                break;
            default:
                score = 10;
                break;
        }

        // Boost for recent activity
        if (lead.LastActivityDate.HasValue)
        {
            var daysSinceActivity = (DateTime.UtcNow - lead.LastActivityDate.Value).TotalDays;
            if (daysSinceActivity < 7)
                score = Math.Min(score + 20, 100);
            else if (daysSinceActivity < 30)
                score = Math.Min(score + 10, 100);
        }

        return score;
    }

    private static double CalculateFitScore(Lead lead, List<string> factors)
    {
        var score = 0.0;

        // Use existing fit score if available
        if (lead.FitScore > 0)
        {
            score = Math.Min(lead.FitScore, 100);
            factors.Add($"Existing fit score: {lead.FitScore}");
            return score;
        }

        // Estimate from available data
        if (!string.IsNullOrWhiteSpace(lead.CompanyName))
            score += 30;

        if (!string.IsNullOrWhiteSpace(lead.Title))
        {
            var title = lead.Title.ToLowerInvariant();
            if (title.Contains("vp") || title.Contains("director") || title.Contains("chief") || title.Contains("head"))
            {
                score += 40;
                factors.Add("Decision-maker title detected");
            }
            else if (title.Contains("manager") || title.Contains("senior"))
            {
                score += 25;
                factors.Add("Influencer title detected");
            }
            else
            {
                score += 15;
            }
        }

        if (!string.IsNullOrWhiteSpace(lead.Website))
            score += 15;

        if (!string.IsNullOrWhiteSpace(lead.QualificationNotes))
        {
            score += 15;
            factors.Add("Qualification notes present");
        }

        return Math.Min(score, 100);
    }

    private static double CalculateRecencyScore(Lead lead, List<string> factors)
    {
        var referenceDate = lead.LastActivityDate ?? lead.CreatedAt;
        var daysSince = (DateTime.UtcNow - referenceDate).TotalDays;

        if (daysSince < 1)
        {
            factors.Add("Activity today");
            return 100;
        }
        if (daysSince < 7)
        {
            factors.Add("Activity within last week");
            return 85;
        }
        if (daysSince < 14)
            return 70;
        if (daysSince < 30)
            return 50;
        if (daysSince < 60)
        {
            factors.Add("Aging lead (30-60 days)");
            return 30;
        }
        if (daysSince < 90)
        {
            factors.Add("Stale lead (60-90 days)");
            return 15;
        }

        factors.Add("Very stale lead (90+ days)");
        return 5;
    }

    private static double CalculateSourceScore(Lead lead, List<string> factors)
    {
        if (SourceQuality.TryGetValue(lead.Source, out var quality))
        {
            factors.Add($"Source: {lead.Source}");
            return quality;
        }
        return 40; // Default
    }

    private async Task<double?> GetSentimentScoreAsync(Lead lead, CancellationToken ct)
    {
        try
        {
            var isAIEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalAI);
            if (!isAIEnabled)
                return null;

            var aiPort = _serviceProvider.GetService<IAIPort>();
            if (aiPort == null)
                return null;

            // Analyze sentiment of qualification notes if present
            var text = lead.QualificationNotes;
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var sentiment = await aiPort.AnalyzeSentimentAsync(text, ct);
            return sentiment.Score;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Sentiment analysis unavailable for lead {LeadId}", lead.Id);
            return null;
        }
    }

    private static string GetGrade(int score) => score switch
    {
        >= 90 => "A",
        >= 75 => "B",
        >= 60 => "C",
        >= 40 => "D",
        _ => "F"
    };

    #endregion
}
