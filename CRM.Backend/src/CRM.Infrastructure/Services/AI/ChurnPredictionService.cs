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
#pragma warning disable CS0618 // Type or member is obsolete

namespace CRM.Infrastructure.Services.AI;

/// <summary>
/// Heuristic churn prediction service based on account activity signals.
/// Implements TODO-AI-03.
/// </summary>
public class ChurnPredictionService : IChurnPredictionService
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<ChurnPredictionService> _logger;

    public ChurnPredictionService(ICrmDbContext db, ILogger<ChurnPredictionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ChurnPredictionDto?> PredictChurnAsync(int accountId, CancellationToken ct = default)
    {
        var account = await _db.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted, ct);

        if (account is null) return null;

        var factors = new List<string>();
        double score = 0;

        // Factor 1: Days since last interaction (weight 35%)
        var lastInteraction = await _db.Interactions
            .AsNoTracking()
            .Where(i => i.AccountId == accountId && !i.IsDeleted)
            .OrderByDescending(i => i.InteractionDate)
            .Select(i => i.InteractionDate)
            .FirstOrDefaultAsync(ct);

        var daysSinceLast = lastInteraction == DateTime.MinValue
            ? 365
            : (DateTime.UtcNow - lastInteraction).TotalDays;

        if (daysSinceLast > 90)
        {
            score += 35;
            factors.Add($"No contact in {(int)daysSinceLast} days");
        }
        else if (daysSinceLast > 30)
        {
            score += 15;
            factors.Add($"Low engagement — last contact {(int)daysSinceLast} days ago");
        }

        // Factor 2: Open service requests (weight 25%)
        var openTickets = await _db.ServiceRequests
            .AsNoTracking()
            .CountAsync(s => s.AccountId == accountId && !s.IsDeleted &&
                             (s.Status == ServiceRequestStatus.Open ||
                              s.Status == ServiceRequestStatus.InProgress), ct);

        if (openTickets >= 3)
        {
            score += 25;
            factors.Add($"{openTickets} unresolved support tickets");
        }
        else if (openTickets >= 1)
        {
            score += 10;
            factors.Add($"{openTickets} open support ticket(s)");
        }

        // Factor 3: No open opportunity (weight 20%)
        var openOpps = await _db.Opportunities
            .AsNoTracking()
            .CountAsync(o => o.AccountId == accountId && !o.IsDeleted &&
                             o.Stage != OpportunityStage.ClosedWon &&
                             o.Stage != OpportunityStage.ClosedLost, ct);

        if (openOpps == 0)
        {
            score += 20;
            factors.Add("No active opportunities in pipeline");
        }

        // Factor 4: Account priority (weight 10%)
        if (account.Priority == AccountPriority.Low)
        {
            score += 10;
            factors.Add("Account is low priority");
        }

        // Factor 5: No interactions in the last year (weight 10%)
        if (daysSinceLast > 180)
        {
            score += 10;
            factors.Add("No meaningful engagement in 6+ months");
        }

        var probability = Math.Min(score / 100.0, 1.0);
        var riskLevel = probability switch
        {
            >= 0.7 => ChurnRiskLevel.High,
            >= 0.4 => ChurnRiskLevel.Medium,
            _      => ChurnRiskLevel.Low
        };

        var recommendations = BuildRecommendations(riskLevel, factors);

        _logger.LogDebug("Churn prediction for account {AccountId}: {Probability:P0} ({RiskLevel})",
            accountId, probability, riskLevel);

        return new ChurnPredictionDto
        {
            AccountId = accountId,
            ChurnProbability = probability,
            RiskLevel = riskLevel,
            KeyFactors = factors.ToArray(),
            RecommendedActions = recommendations,
            CalculatedAt = DateTime.UtcNow
        };
    }

    private static string[] BuildRecommendations(ChurnRiskLevel level, List<string> factors)
    {
        var recs = new List<string>();
        if (level == ChurnRiskLevel.High)
        {
            recs.Add("Schedule an urgent executive check-in call");
            recs.Add("Review contract renewal status immediately");
            recs.Add("Assign a dedicated customer success manager");
        }
        else if (level == ChurnRiskLevel.Medium)
        {
            recs.Add("Send a personalised re-engagement email");
            recs.Add("Offer a product demo or training session");
        }
        else
        {
            recs.Add("Maintain regular quarterly business reviews");
            recs.Add("Share relevant success stories and product updates");
        }
        return recs.ToArray();
    }
}
