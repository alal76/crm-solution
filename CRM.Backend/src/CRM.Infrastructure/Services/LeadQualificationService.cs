// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Lead Qualification Service implementation (TODO-CRM002-08).
/// Implements BANT/MEDDIC scoring models for lead qualification.
/// </summary>
public class LeadQualificationService : ILeadQualificationService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<LeadQualificationService> _logger;

    public LeadQualificationService(ICrmDbContext context, ILogger<LeadQualificationService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<LeadQualificationResult> ScoreWithBANTAsync(
        int leadId,
        int budgetScore,
        int authorityScore,
        int needScore,
        int timelineScore,
        CancellationToken ct = default)
    {
        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == leadId && !l.IsDeleted, ct)
            ?? throw new KeyNotFoundException($"Lead with ID {leadId} not found");

        // Clamp scores to 0-100
        budgetScore = Math.Clamp(budgetScore, 0, 100);
        authorityScore = Math.Clamp(authorityScore, 0, 100);
        needScore = Math.Clamp(needScore, 0, 100);
        timelineScore = Math.Clamp(timelineScore, 0, 100);

        // Persist scores on the Lead entity
        lead.BudgetScore = budgetScore;
        lead.AuthorityScore = authorityScore;
        lead.NeedScore = needScore;
        lead.TimelineScore = timelineScore;
        lead.QualificationFrameworkType = QualificationFramework.BANT;
        lead.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        var combined = (budgetScore + authorityScore + needScore + timelineScore) / 4;

        var dimensions = new Dictionary<string, int>
        {
            ["Budget"] = budgetScore,
            ["Authority"] = authorityScore,
            ["Need"] = needScore,
            ["Timeline"] = timelineScore
        };

        var recommendations = new List<string>();
        if (budgetScore < 50) recommendations.Add("Investigate and confirm budget availability.");
        if (authorityScore < 50) recommendations.Add("Identify and engage the decision maker.");
        if (needScore < 50) recommendations.Add("Better understand and document the business need.");
        if (timelineScore < 50) recommendations.Add("Clarify the buying timeline and urgency.");

        _logger.LogInformation("Scored Lead {LeadId} with BANT framework: combined={Combined}", leadId, combined);

        return new LeadQualificationResult
        {
            LeadId = leadId,
            Framework = QualificationFramework.BANT,
            CombinedScore = combined,
            DimensionScores = dimensions,
            QualificationLevel = GetQualificationLevel(combined),
            Recommendations = recommendations,
            ScoredAt = DateTime.UtcNow
        };
    }

    /// <inheritdoc />
    public async Task<LeadQualificationResult> ScoreWithMEDDICAsync(
        int leadId,
        MEDDICScores scores,
        CancellationToken ct = default)
    {
        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == leadId && !l.IsDeleted, ct)
            ?? throw new KeyNotFoundException($"Lead with ID {leadId} not found");

        // Clamp all scores
        var metrics = Math.Clamp(scores.MetricsScore, 0, 100);
        var economicBuyer = Math.Clamp(scores.EconomicBuyerScore, 0, 100);
        var decisionCriteria = Math.Clamp(scores.DecisionCriteriaScore, 0, 100);
        var decisionProcess = Math.Clamp(scores.DecisionProcessScore, 0, 100);
        var identifyPain = Math.Clamp(scores.IdentifyPainScore, 0, 100);
        var champion = Math.Clamp(scores.ChampionScore, 0, 100);

        // Persist scores on Lead entity
        lead.MetricsScore = metrics;
        lead.EconomicBuyerScore = economicBuyer;
        lead.DecisionCriteriaScore = decisionCriteria;
        lead.DecisionProcessScore = decisionProcess;
        lead.IdentifyPainScore = identifyPain;
        lead.ChampionScore = champion;
        lead.QualificationFrameworkType = QualificationFramework.MEDDIC;
        lead.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        var combined = (metrics + economicBuyer + decisionCriteria + decisionProcess + identifyPain + champion) / 6;

        var dimensions = new Dictionary<string, int>
        {
            ["Metrics"] = metrics,
            ["EconomicBuyer"] = economicBuyer,
            ["DecisionCriteria"] = decisionCriteria,
            ["DecisionProcess"] = decisionProcess,
            ["IdentifyPain"] = identifyPain,
            ["Champion"] = champion
        };

        var recommendations = new List<string>();
        if (metrics < 50) recommendations.Add("Quantify the business metrics and ROI for the prospect.");
        if (economicBuyer < 50) recommendations.Add("Identify and gain access to the economic buyer.");
        if (decisionCriteria < 50) recommendations.Add("Understand the formal decision criteria.");
        if (decisionProcess < 50) recommendations.Add("Map the decision process and all stakeholders.");
        if (identifyPain < 50) recommendations.Add("Dig deeper into the prospect's pain points.");
        if (champion < 50) recommendations.Add("Find and develop an internal champion.");

        _logger.LogInformation("Scored Lead {LeadId} with MEDDIC framework: combined={Combined}", leadId, combined);

        return new LeadQualificationResult
        {
            LeadId = leadId,
            Framework = QualificationFramework.MEDDIC,
            CombinedScore = combined,
            DimensionScores = dimensions,
            QualificationLevel = GetQualificationLevel(combined),
            Recommendations = recommendations,
            ScoredAt = DateTime.UtcNow
        };
    }

    /// <inheritdoc />
    public async Task<LeadQualificationResult?> GetQualificationAsync(int leadId, CancellationToken ct = default)
    {
        var lead = await _context.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == leadId && !l.IsDeleted, ct);

        if (lead == null) return null;

        var framework = lead.QualificationFrameworkType == QualificationFramework.None
            ? QualificationFramework.BANT
            : lead.QualificationFrameworkType;

        Dictionary<string, int> dimensions;
        int combined;

        if (framework == QualificationFramework.MEDDIC)
        {
            dimensions = new Dictionary<string, int>
            {
                ["Metrics"] = lead.MetricsScore ?? 0,
                ["EconomicBuyer"] = lead.EconomicBuyerScore ?? 0,
                ["DecisionCriteria"] = lead.DecisionCriteriaScore ?? 0,
                ["DecisionProcess"] = lead.DecisionProcessScore ?? 0,
                ["IdentifyPain"] = lead.IdentifyPainScore ?? 0,
                ["Champion"] = lead.ChampionScore ?? 0
            };
            combined = dimensions.Values.Sum() / 6;
        }
        else
        {
            dimensions = new Dictionary<string, int>
            {
                ["Budget"] = lead.BudgetScore ?? 0,
                ["Authority"] = lead.AuthorityScore ?? 0,
                ["Need"] = lead.NeedScore ?? 0,
                ["Timeline"] = lead.TimelineScore ?? 0
            };
            combined = dimensions.Values.Sum() / 4;
        }

        return new LeadQualificationResult
        {
            LeadId = leadId,
            Framework = framework,
            CombinedScore = combined,
            DimensionScores = dimensions,
            QualificationLevel = GetQualificationLevel(combined),
            Recommendations = GetRecommendationsForWeakDimensions(dimensions),
            ScoredAt = lead.UpdatedAt ?? DateTime.UtcNow
        };
    }

    /// <inheritdoc />
    public async Task<QualificationEvaluation> EvaluateAsync(
        int leadId,
        int mqlThreshold = 50,
        int sqlThreshold = 75,
        CancellationToken ct = default)
    {
        var qualification = await GetQualificationAsync(leadId, ct)
            ?? throw new KeyNotFoundException($"Lead with ID {leadId} not found or not scored");

        var weakDimensions = qualification.DimensionScores
            .Where(d => d.Value < mqlThreshold)
            .Select(d => d.Key)
            .ToList();

        var suggestedActions = new List<string>();
        if (!qualification.DimensionScores.Any())
        {
            suggestedActions.Add("Score this lead using a qualification framework (BANT or MEDDIC).");
        }
        else if (qualification.CombinedScore < mqlThreshold)
        {
            suggestedActions.Add("Lead needs more nurturing before reaching MQL status.");
            suggestedActions.Add($"Focus on improving: {string.Join(", ", weakDimensions)}.");
        }
        else if (qualification.CombinedScore < sqlThreshold)
        {
            suggestedActions.Add("Lead is MQL-ready. Continue engagement to reach SQL threshold.");
        }
        else
        {
            suggestedActions.Add("Lead is SQL-qualified. Route to sales for direct engagement.");
        }

        return new QualificationEvaluation
        {
            LeadId = leadId,
            CurrentScore = qualification.CombinedScore,
            IsMQL = qualification.CombinedScore >= mqlThreshold,
            IsSQL = qualification.CombinedScore >= sqlThreshold,
            MQLThreshold = mqlThreshold,
            SQLThreshold = sqlThreshold,
            WeakDimensions = weakDimensions,
            SuggestedActions = suggestedActions
        };
    }

    private static string GetQualificationLevel(int score) =>
        score switch
        {
            >= 75 => "SQL",
            >= 50 => "MQL",
            _ => "Unqualified"
        };

    private static List<string> GetRecommendationsForWeakDimensions(Dictionary<string, int> dimensions)
    {
        return dimensions
            .Where(d => d.Value < 50)
            .Select(d => $"Improve '{d.Key}' score (currently {d.Value}/100).")
            .ToList();
    }
}
