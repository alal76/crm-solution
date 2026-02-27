// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Heuristic/rule-based implementation of <see cref="IAIPredictiveAnalyticsService"/>.
/// Uses entity data signals (activity recency, scores, pipeline stage) to produce predictions.
/// Can be replaced by an ML-backed implementation in the future.
/// </summary>
public class AIPredictiveAnalyticsService : IAIPredictiveAnalyticsService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<AIPredictiveAnalyticsService> _logger;

    public AIPredictiveAnalyticsService(ICrmDbContext context, ILogger<AIPredictiveAnalyticsService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<LeadScorePrediction> PredictLeadScoreAsync(int leadId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Predicting lead score for Lead {LeadId}", leadId);

        var lead = await _context.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == leadId && !l.IsDeleted, cancellationToken);

        if (lead == null)
        {
            _logger.LogWarning("Lead {LeadId} not found for score prediction", leadId);
            return new LeadScorePrediction
            {
                LeadId = leadId,
                PredictedScore = 0,
                Confidence = 0,
                Factors = new List<PredictionFactor>
                {
                    new() { Name = "NotFound", Description = "Lead does not exist", Impact = 0 }
                }
            };
        }

        var factors = new List<PredictionFactor>();
        double totalScore = 0;
        int factorCount = 0;

        // Factor 1: Existing score / fit score (weight 30%)
        var existingScore = lead.Score;
        var fitScore = lead.FitScore;
        var combinedScore = Math.Max(existingScore, fitScore);
        if (combinedScore > 0)
        {
            totalScore += combinedScore * 0.30;
            factorCount++;
            factors.Add(new PredictionFactor
            {
                Name = "ExistingScore",
                Description = "Current lead score / fit score",
                Impact = combinedScore / 100.0,
                Value = $"{combinedScore}"
            });
        }

        // Factor 2: Engagement score (weight 25%)
        var engagement = lead.EngagementScore;
        totalScore += engagement * 0.25;
        factorCount++;
        factors.Add(new PredictionFactor
        {
            Name = "EngagementScore",
            Description = "Lead engagement level",
            Impact = engagement / 100.0,
            Value = $"{engagement}"
        });

        // Factor 3: Activity recency (weight 20%)
        double recencyScore = 0;
        if (lead.LastActivityDate.HasValue)
        {
            var daysSinceActivity = (DateTime.UtcNow - lead.LastActivityDate.Value).TotalDays;
            recencyScore = daysSinceActivity switch
            {
                <= 7 => 100,
                <= 14 => 80,
                <= 30 => 60,
                <= 60 => 40,
                <= 90 => 20,
                _ => 5
            };
        }
        totalScore += recencyScore * 0.20;
        factorCount++;
        factors.Add(new PredictionFactor
        {
            Name = "ActivityRecency",
            Description = "How recently the lead was active",
            Impact = recencyScore / 100.0,
            Value = lead.LastActivityDate?.ToString("yyyy-MM-dd") ?? "Never"
        });

        // Factor 4: Company presence (weight 15%)
        double companyScore = string.IsNullOrWhiteSpace(lead.CompanyName) ? 20 : 70;
        if (!string.IsNullOrWhiteSpace(lead.Title))
            companyScore += 15;
        companyScore = Math.Min(companyScore, 100);
        totalScore += companyScore * 0.15;
        factorCount++;
        factors.Add(new PredictionFactor
        {
            Name = "CompanyPresence",
            Description = "Company and job title completeness",
            Impact = companyScore / 100.0,
            Value = lead.CompanyName ?? "Unknown"
        });

        // Factor 5: Lead source quality (weight 10%)
        double sourceScore = lead.Source switch
        {
            LeadSource.Referral => 90,
            LeadSource.Partner => 75,
            LeadSource.Web => 70,
            LeadSource.Event => 60,
            LeadSource.Campaign => 55,
            LeadSource.Manual => 40,
            _ => 40
        };
        totalScore += sourceScore * 0.10;
        factorCount++;
        factors.Add(new PredictionFactor
        {
            Name = "LeadSource",
            Description = "Quality signal from lead source channel",
            Impact = sourceScore / 100.0,
            Value = lead.Source.ToString()
        });

        var predictedScore = (int)Math.Clamp(Math.Round(totalScore), 0, 100);
        var confidence = factorCount switch
        {
            >= 4 => 0.75,
            >= 2 => 0.55,
            _ => 0.30
        };

        _logger.LogInformation("Lead {LeadId} predicted score: {Score} (confidence {Confidence})", leadId, predictedScore, confidence);

        return new LeadScorePrediction
        {
            LeadId = leadId,
            PredictedScore = predictedScore,
            Confidence = confidence,
            Factors = factors
        };
    }

    /// <inheritdoc />
    public async Task<ChurnRiskPrediction> PredictChurnRiskAsync(int accountId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Predicting churn risk for Account {AccountId}", accountId);

        var account = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted, cancellationToken);

        if (account == null)
        {
            _logger.LogWarning("Account {AccountId} not found for churn prediction", accountId);
            return new ChurnRiskPrediction
            {
                AccountId = accountId,
                RiskLevel = "Unknown",
                RiskScore = 0,
                Factors = new List<PredictionFactor>
                {
                    new() { Name = "NotFound", Description = "Account does not exist", Impact = 0 }
                }
            };
        }

        var factors = new List<PredictionFactor>();
        var actions = new List<string>();
        double riskScore = 0;

        // Factor 1: Health score (inverse correlation — low health = high risk)
        var healthScore = account.AccountHealthScore;
        double healthRisk = (100.0 - healthScore) / 100.0;
        riskScore += healthRisk * 0.25;
        factors.Add(new PredictionFactor
        {
            Name = "HealthScore",
            Description = "Account health score",
            Impact = -healthRisk,
            Value = $"{healthScore}"
        });
        if (healthScore < 40)
            actions.Add("Schedule executive business review to address account health concerns");

        // Factor 2: NPS score (negative NPS = high risk)
        var nps = account.NpsScore;
        double npsRisk = nps < 0 ? Math.Abs(nps) / 100.0 : 0;
        if (nps <= -50)
            npsRisk = 1.0;
        riskScore += npsRisk * 0.20;
        factors.Add(new PredictionFactor
        {
            Name = "NpsScore",
            Description = "Net Promoter Score",
            Impact = -npsRisk,
            Value = $"{nps}"
        });
        if (nps < 0)
            actions.Add("Conduct NPS follow-up survey and address detractor concerns");

        // Factor 3: Activity recency
        double activityRisk = 0;
        if (account.LastActivityDate.HasValue)
        {
            var daysSince = (DateTime.UtcNow - account.LastActivityDate.Value).TotalDays;
            activityRisk = daysSince switch
            {
                <= 14 => 0.0,
                <= 30 => 0.15,
                <= 60 => 0.40,
                <= 90 => 0.65,
                _ => 0.90
            };
        }
        else
        {
            activityRisk = 0.80;
        }
        riskScore += activityRisk * 0.25;
        factors.Add(new PredictionFactor
        {
            Name = "ActivityRecency",
            Description = "Days since last account activity",
            Impact = -activityRisk,
            Value = account.LastActivityDate?.ToString("yyyy-MM-dd") ?? "Never"
        });
        if (activityRisk > 0.50)
            actions.Add("Re-engage account with proactive outreach or check-in call");

        // Factor 4: Open service request volume (high volume = risk signal)
        var openTickets = await _context.ServiceRequests
            .CountAsync(sr => sr.AccountId == accountId && !sr.IsDeleted &&
                             sr.Status != ServiceRequestStatus.Closed &&
                             sr.Status != ServiceRequestStatus.Resolved,
                         cancellationToken);
        double ticketRisk = openTickets switch
        {
            0 => 0.05,
            1 => 0.10,
            <= 3 => 0.30,
            <= 5 => 0.55,
            _ => 0.85
        };
        riskScore += ticketRisk * 0.15;
        factors.Add(new PredictionFactor
        {
            Name = "OpenTickets",
            Description = "Number of open support tickets",
            Impact = -ticketRisk,
            Value = $"{openTickets}"
        });
        if (openTickets > 3)
            actions.Add("Prioritize resolution of open support tickets");

        // Factor 5: Satisfaction rating
        var satisfaction = account.SatisfactionRating;
        double satRisk = satisfaction < 3.0 ? (3.0 - satisfaction) / 3.0 : 0;
        riskScore += satRisk * 0.15;
        factors.Add(new PredictionFactor
        {
            Name = "SatisfactionRating",
            Description = "Customer satisfaction score (0-5)",
            Impact = -satRisk,
            Value = $"{satisfaction:F1}"
        });
        if (satisfaction < 3.0)
            actions.Add("Launch customer success improvement plan");

        riskScore = Math.Clamp(riskScore, 0, 1.0);

        var riskLevel = riskScore switch
        {
            >= 0.75 => "Critical",
            >= 0.50 => "High",
            >= 0.25 => "Medium",
            _ => "Low"
        };

        if (actions.Count == 0)
            actions.Add("Continue regular engagement cadence");

        _logger.LogInformation("Account {AccountId} churn risk: {RiskLevel} ({RiskScore:F2})", accountId, riskLevel, riskScore);

        return new ChurnRiskPrediction
        {
            AccountId = accountId,
            RiskLevel = riskLevel,
            RiskScore = riskScore,
            Factors = factors,
            RecommendedActions = actions
        };
    }

    /// <inheritdoc />
    public async Task<DealWinProbability> PredictDealWinProbabilityAsync(int opportunityId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Predicting win probability for Opportunity {OpportunityId}", opportunityId);

        var opportunity = await _context.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted, cancellationToken);

        if (opportunity == null)
        {
            _logger.LogWarning("Opportunity {OpportunityId} not found for win prediction", opportunityId);
            return new DealWinProbability
            {
                OpportunityId = opportunityId,
                WinProbability = 0,
                Confidence = 0,
                RiskFactors = new List<PredictionFactor>
                {
                    new() { Name = "NotFound", Description = "Opportunity does not exist", Impact = -1 }
                }
            };
        }

        var riskFactors = new List<PredictionFactor>();
        var positiveFactors = new List<PredictionFactor>();
        double probability = 0;

        // Factor 1: Stage progression (base probability)
        double stageBase = opportunity.Stage switch
        {
            OpportunityStage.Discovery => 0.10,
            OpportunityStage.Qualification => 0.25,
            OpportunityStage.Proposal => 0.40,
            OpportunityStage.Negotiation => 0.60,
            OpportunityStage.ClosedWon => 1.0,
            OpportunityStage.ClosedLost => 0.0,
            _ => 0.10
        };
        probability = stageBase;
        positiveFactors.Add(new PredictionFactor
        {
            Name = "SaleStage",
            Description = $"Current stage: {opportunity.Stage}",
            Impact = stageBase,
            Value = opportunity.Stage.ToString()
        });

        // If already closed, return immediately
        if (opportunity.Stage == OpportunityStage.ClosedWon || opportunity.Stage == OpportunityStage.ClosedLost)
        {
            return new DealWinProbability
            {
                OpportunityId = opportunityId,
                WinProbability = stageBase,
                Confidence = 1.0,
                RiskFactors = riskFactors,
                PositiveFactors = positiveFactors
            };
        }

        // Factor 2: Existing probability field
        var existingProb = opportunity.Probability;
        if (existingProb > 0)
        {
            probability = (probability + (existingProb / 100.0)) / 2.0;
            positiveFactors.Add(new PredictionFactor
            {
                Name = "AssignedProbability",
                Description = "Sales rep assigned probability",
                Impact = existingProb / 100.0,
                Value = $"{existingProb}%"
            });
        }

        // Factor 3: Deal age (stale deals are risky)
        var dealAgeDays = (DateTime.UtcNow - opportunity.CreatedAt).TotalDays;
        if (dealAgeDays > 180)
        {
            double ageRisk = Math.Min((dealAgeDays - 180) / 180.0, 0.30);
            probability -= ageRisk;
            riskFactors.Add(new PredictionFactor
            {
                Name = "DealAge",
                Description = $"Deal is {(int)dealAgeDays} days old — stale deals close less often",
                Impact = -ageRisk,
                Value = $"{(int)dealAgeDays} days"
            });
        }
        else
        {
            positiveFactors.Add(new PredictionFactor
            {
                Name = "DealAge",
                Description = $"Deal is {(int)dealAgeDays} days old — within healthy range",
                Impact = 0.05,
                Value = $"{(int)dealAgeDays} days"
            });
        }

        // Factor 4: Close date proximity
        if (opportunity.ExpectedCloseDate.HasValue)
        {
            var daysToClose = (opportunity.ExpectedCloseDate.Value - DateTime.UtcNow).TotalDays;
            if (daysToClose < 0)
            {
                // Past due
                double overdueRisk = Math.Min(Math.Abs(daysToClose) / 60.0, 0.25);
                probability -= overdueRisk;
                riskFactors.Add(new PredictionFactor
                {
                    Name = "OverdueClose",
                    Description = $"Expected close date passed {(int)Math.Abs(daysToClose)} days ago",
                    Impact = -overdueRisk,
                    Value = opportunity.ExpectedCloseDate.Value.ToString("yyyy-MM-dd")
                });
            }
            else if (daysToClose <= 30)
            {
                positiveFactors.Add(new PredictionFactor
                {
                    Name = "NearClose",
                    Description = $"Close date is within {(int)daysToClose} days",
                    Impact = 0.05,
                    Value = opportunity.ExpectedCloseDate.Value.ToString("yyyy-MM-dd")
                });
            }
        }
        else
        {
            riskFactors.Add(new PredictionFactor
            {
                Name = "NoCloseDate",
                Description = "No expected close date set",
                Impact = -0.05,
                Value = "Not set"
            });
            probability -= 0.05;
        }

        // Factor 5: Deal size vs typical (large deals carry more risk)
        var amount = opportunity.Amount;
        if (amount > 100_000)
        {
            riskFactors.Add(new PredictionFactor
            {
                Name = "LargeDeal",
                Description = "Large deal amounts typically have longer sales cycles and more scrutiny",
                Impact = -0.05,
                Value = $"${amount:N0}"
            });
            probability -= 0.05;
        }
        else if (amount > 0)
        {
            positiveFactors.Add(new PredictionFactor
            {
                Name = "DealSize",
                Description = "Deal amount is within typical range",
                Impact = 0.02,
                Value = $"${amount:N0}"
            });
        }

        // Factor 6: Account health (if linked)
        if (opportunity.AccountId > 0)
        {
            var acct = await _context.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == opportunity.AccountId && !a.IsDeleted, cancellationToken);

            if (acct != null)
            {
                var acctHealth = acct.AccountHealthScore;
                if (acctHealth >= 70)
                {
                    positiveFactors.Add(new PredictionFactor
                    {
                        Name = "AccountHealth",
                        Description = "Linked account has strong health score",
                        Impact = 0.08,
                        Value = $"{acctHealth}"
                    });
                    probability += 0.08;
                }
                else if (acctHealth < 40)
                {
                    riskFactors.Add(new PredictionFactor
                    {
                        Name = "AccountHealth",
                        Description = "Linked account has poor health score",
                        Impact = -0.08,
                        Value = $"{acctHealth}"
                    });
                    probability -= 0.08;
                }
            }
        }

        probability = Math.Clamp(probability, 0, 1.0);
        var confidence = riskFactors.Count + positiveFactors.Count >= 5 ? 0.70 : 0.50;

        _logger.LogInformation("Opportunity {OpportunityId} win probability: {Probability:P0} (confidence {Confidence:F2})",
            opportunityId, probability, confidence);

        return new DealWinProbability
        {
            OpportunityId = opportunityId,
            WinProbability = Math.Round(probability, 4),
            Confidence = confidence,
            RiskFactors = riskFactors,
            PositiveFactors = positiveFactors
        };
    }
}
