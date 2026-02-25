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

namespace CRM.Infrastructure.Services.AI;

/// <summary>
/// Heuristic deal risk scoring based on opportunity signals.
/// Implements TODO-AI-09.
/// </summary>
public class DealRiskService : IDealRiskService
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<DealRiskService> _logger;

    public DealRiskService(ICrmDbContext db, ILogger<DealRiskService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<DealRiskDto?> CalculateRiskAsync(int opportunityId, CancellationToken ct = default)
    {
        var opp = await _db.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted, ct);

        if (opp is null) return null;

        var factors = new List<string>();
        var mitigations = new List<string>();
        int riskScore = 0;

        // Factor 1: Close date proximity (25 pts if past due, 15 pts if within 7 days)
        if (opp.ExpectedCloseDate.HasValue)
        {
            var daysToClose = (opp.ExpectedCloseDate.Value - DateTime.UtcNow).TotalDays;
            if (daysToClose < 0)
            {
                riskScore += 25;
                factors.Add("Close date is overdue");
                mitigations.Add("Update expected close date to a realistic future date");
            }
            else if (daysToClose <= 7)
            {
                riskScore += 15;
                factors.Add("Close date within 7 days — decision needed urgently");
                mitigations.Add("Accelerate decision process — schedule a closing call today");
            }
        }
        else
        {
            riskScore += 10;
            factors.Add("No close date set");
            mitigations.Add("Set a realistic expected close date");
        }

        // Factor 2: No recent activity (20 pts if 14+ days, 10 pts if 7+ days)
        var lastInteraction = await _db.Interactions
            .AsNoTracking()
            .Where(i => i.OpportunityId == opportunityId && !i.IsDeleted)
            .OrderByDescending(i => i.InteractionDate)
            .Select(i => i.InteractionDate)
            .FirstOrDefaultAsync(ct);

        var daysSinceActivity = lastInteraction == DateTime.MinValue
            ? 999
            : (DateTime.UtcNow - lastInteraction).TotalDays;

        if (daysSinceActivity > 14)
        {
            riskScore += 20;
            factors.Add($"No activity for {(int)daysSinceActivity} days");
            mitigations.Add("Immediately reach out to re-engage the prospect");
        }
        else if (daysSinceActivity > 7)
        {
            riskScore += 10;
            factors.Add($"Low activity — last touchpoint {(int)daysSinceActivity} days ago");
            mitigations.Add("Schedule a follow-up call or send a status email");
        }

        // Factor 3: Low probability (20 pts if < 25%)
        if (opp.Probability < 25)
        {
            riskScore += 20;
            factors.Add($"Low win probability ({opp.Probability}%)");
            mitigations.Add("Investigate blockers and qualify/disqualify the opportunity");
        }

        // Factor 4: Early stage with imminent close date
        if (opp.Stage == OpportunityStage.Discovery || opp.Stage == OpportunityStage.Qualification)
        {
            if (opp.ExpectedCloseDate.HasValue &&
                (opp.ExpectedCloseDate.Value - DateTime.UtcNow).TotalDays < 30)
            {
                riskScore += 15;
                factors.Add("Opportunity still in early stage with close date < 30 days");
                mitigations.Add("Advance the sales process rapidly or extend the close date");
            }
        }

        // Factor 5: No contacts associated
        var contactCount = await _db.Contacts
            .AsNoTracking()
            .CountAsync(c => c.AccountId == opp.AccountId, ct);

        if (contactCount == 0)
        {
            riskScore += 10;
            factors.Add("No contacts linked to the account");
            mitigations.Add("Add at least one decision-maker contact to the account");
        }
        else if (contactCount == 1)
        {
            riskScore += 5;
            factors.Add("Only one contact — single point of failure");
            mitigations.Add("Identify and engage additional stakeholders");
        }

        riskScore = Math.Clamp(riskScore, 0, 100);
        var riskLevel = riskScore switch
        {
            >= 70 => DealRiskLevel.Critical,
            >= 45 => DealRiskLevel.High,
            >= 20 => DealRiskLevel.Medium,
            _     => DealRiskLevel.Low
        };

        _logger.LogDebug("Deal risk for opportunity {Id}: score={Score}, level={Level}",
            opportunityId, riskScore, riskLevel);

        return new DealRiskDto
        {
            OpportunityId = opportunityId,
            RiskScore = riskScore,
            RiskLevel = riskLevel,
            RiskFactors = factors.ToArray(),
            MitigationSuggestions = mitigations.ToArray(),
            CalculatedAt = DateTime.UtcNow
        };
    }
}
