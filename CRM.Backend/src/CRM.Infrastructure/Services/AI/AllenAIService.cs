// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Entities.AI;
using CRM.Core.Interfaces.AI;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// Disambiguate types that exist in both CRM.Core.Entities.AI and CRM.Core.Interfaces (from global using)
using ChurnRiskLevel = CRM.Core.Entities.AI.ChurnRiskLevel;
using NextBestActionType = CRM.Core.Entities.AI.NextBestActionType;

namespace CRM.Infrastructure.Services.AI;

/// <summary>
/// AI service implementation using Allen AI's OLMo and Tulu models.
/// These are free, open-source models from AI2 (Allen Institute for AI).
/// </summary>
public class AllenAIService : IAllenAIService
{
    private readonly CrmDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AllenAIService> _logger;
    private readonly AllenAIConfiguration _config;
    private readonly IMemoryCache _cache;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public AllenAIService(
        CrmDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<AllenAIService> logger,
        IOptions<AllenAIConfiguration> config,
        IMemoryCache cache)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _config = config.Value;
        _cache = cache;
    }

    #region Lead Scoring

    /// <inheritdoc />
    public async Task<LeadScore> ScoreLeadAsync(int leadId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"lead_score_{leadId}";

        if (_config.EnableCaching && _cache.TryGetValue<LeadScore>(cacheKey, out var cachedScore) && cachedScore != null
            && cachedScore.ExpiresAt > DateTime.UtcNow)
        {
            return cachedScore;
        }

        var lead = await _context.Leads
            .Include(l => l.ProductInterests)
            .FirstOrDefaultAsync(l => l.Id == leadId, cancellationToken);

        if (lead == null)
        {
            throw new ArgumentException($"Lead with ID {leadId} not found", nameof(leadId));
        }

        var score = await CalculateLeadScoreAsync(lead, cancellationToken);

        // Save to database
        _context.LeadScores.Add(score);
        await _context.SaveChangesAsync(cancellationToken);

        // Cache the result
        if (_config.EnableCaching)
        {
            _cache.Set(cacheKey, score, TimeSpan.FromMinutes(_config.CacheExpirationMinutes));
        }

        return score;
    }

    /// <inheritdoc />
    public async Task<List<LeadScore>> BatchScoreLeadsAsync(IEnumerable<int> leadIds, CancellationToken cancellationToken = default)
    {
        var results = new List<LeadScore>();
        var leadIdList = leadIds.ToList();

        foreach (var batch in leadIdList.Chunk(_config.BatchSize))
        {
            var batchTasks = batch.Select(id => ScoreLeadAsync(id, cancellationToken));
            var batchResults = await Task.WhenAll(batchTasks);
            results.AddRange(batchResults);
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<List<LeadScore>> GetTopLeadsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _context.LeadScores
            .Where(s => s.ExpiresAt > DateTime.UtcNow && !s.IsDeleted)
            .OrderByDescending(s => s.ConversionProbability)
            .Take(count)
            .Include(s => s.Lead)
            .ToListAsync(cancellationToken);
    }

    private async Task<LeadScore> CalculateLeadScoreAsync(Lead lead, CancellationToken cancellationToken)
    {
        // Build feature vector for lead scoring
        var features = BuildLeadFeatures(lead);

        // Use AI model to predict conversion probability
        var prompt = BuildLeadScoringPrompt(features);
        var response = await InvokeModelAsync(prompt, cancellationToken);
        var (probability, insights) = ParseLeadScoringResponse(response);

        // Calculate component scores
        var demographicScore = CalculateDemographicScore(lead);
        var behavioralScore = CalculateBehavioralScore(lead);
        var engagementScore = CalculateEngagementScore(lead);
        var intentScore = CalculateIntentScore(lead);

        var overallScore = (demographicScore * 0.2m) + (behavioralScore * 0.3m) +
                          (engagementScore * 0.25m) + (intentScore * 0.25m);

        var category = overallScore switch
        {
            >= 80 => LeadScoreCategory.OnFire,
            >= 60 => LeadScoreCategory.Hot,
            >= 40 => LeadScoreCategory.Warm,
            >= 20 => LeadScoreCategory.Cool,
            _ => LeadScoreCategory.Cold
        };

        return new LeadScore
        {
            LeadId = lead.Id,
            OverallScore = overallScore,
            Category = category,
            Confidence = probability > 0 ? 0.85m : 0.7m,
            DemographicScore = demographicScore,
            FirmographicScore = demographicScore * 0.8m, // Simplified
            BehavioralScore = behavioralScore,
            EngagementScore = engagementScore,
            IntentScore = intentScore,
            EngagementLevel = GetEngagementLevel(engagementScore),
            IntentStrength = GetIntentStrength(intentScore),
            ConversionProbability = probability > 0 ? probability : overallScore / 100m,
            EstimatedDaysToConversion = EstimateDaysToConversion(overallScore),
            AIInsights = insights,
            ICPMatchScore = demographicScore,
            ScoredAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            ModelVersion = "1.0-olmo"
        };
    }

    private Dictionary<string, object> BuildLeadFeatures(Lead lead)
    {
        return new Dictionary<string, object>
        {
            ["company"] = lead.CompanyName ?? "",
            ["title"] = lead.Title ?? "",
            ["source"] = lead.Source.ToString(),
            ["status"] = lead.Status.ToString(),
            ["productInterestCount"] = lead.ProductInterests?.Count ?? 0,
            ["daysInPipeline"] = (DateTime.UtcNow - lead.CreatedAt).Days,
            ["hasEmail"] = !string.IsNullOrEmpty(lead.Email),
            ["hasPhone"] = !string.IsNullOrEmpty(lead.Phone),
            ["score"] = lead.Score,
            ["fitScore"] = lead.FitScore,
            ["engagementScore"] = lead.EngagementScore
        };
    }

    private string BuildLeadScoringPrompt(Dictionary<string, object> features)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Analyze this lead and predict conversion probability (0-100%):");
        sb.AppendLine();
        foreach (var (key, value) in features)
        {
            sb.AppendLine($"- {key}: {value}");
        }
        sb.AppendLine();
        sb.AppendLine("Respond with a JSON object containing:");
        sb.AppendLine("- probability: number (0-100)");
        sb.AppendLine("- insights: string (brief analysis)");
        sb.AppendLine("- topFactors: array of strings (key factors)");

        return sb.ToString();
    }

    private (decimal probability, string insights) ParseLeadScoringResponse(string response)
    {
        try
        {
            // Try to extract probability from response
            var probability = ExtractNumberFromText(response, "probability", 50);
            var insights = ExtractTextBetween(response, "insights", "topFactors") ??
                          "Lead analysis completed using AI model.";

            return (probability / 100m, insights);
        }
        catch
        {
            return (0.5m, "Unable to parse AI response. Using default scoring.");
        }
    }

    private decimal CalculateDemographicScore(Lead lead)
    {
        var score = 50m;

        if (!string.IsNullOrEmpty(lead.CompanyName))
        {
            score += 10;
        }
        if (!string.IsNullOrEmpty(lead.Title))
        {
            score += 10;
        }
        if (!string.IsNullOrEmpty(lead.Region))
        {
            score += 10;
        }
        if (!string.IsNullOrEmpty(lead.Email))
        {
            score += 10;
        }
        if (!string.IsNullOrEmpty(lead.Phone))
        {
            score += 10;
        }

        return Math.Min(100, score);
    }

    private decimal CalculateBehavioralScore(Lead lead)
    {
        // Use the lead's engagement score from entity
        return Math.Min(100, 30 + (lead.EngagementScore * 0.7m));
    }

    private decimal CalculateEngagementScore(Lead lead)
    {
        var productInterestCount = lead.ProductInterests?.Count ?? 0;
        var hasScore = lead.Score > 0;

        var score = 40m;
        score += productInterestCount * 15;
        if (hasScore)
        {
            score += 20;
        }

        return Math.Min(100, score);
    }

    private decimal CalculateIntentScore(Lead lead)
    {
        var score = 30m;

        if (lead.Status == LeadLifecycleStatus.Qualified)
        {
            score += 30;
        }
        else if (lead.Status == LeadLifecycleStatus.Working)
        {
            score += 20;
        }
        else if (lead.Status == LeadLifecycleStatus.Nurturing)
        {
            score += 10;
        }

        if (lead.ProductInterests?.Any() == true)
        {
            score += 20;
        }

        return Math.Min(100, score);
    }

    private LeadEngagementLevel GetEngagementLevel(decimal score) => score switch
    {
        >= 80 => LeadEngagementLevel.VeryHigh,
        >= 60 => LeadEngagementLevel.High,
        >= 40 => LeadEngagementLevel.Medium,
        >= 20 => LeadEngagementLevel.Low,
        _ => LeadEngagementLevel.None
    };

    private IntentSignalStrength GetIntentStrength(decimal score) => score switch
    {
        >= 80 => IntentSignalStrength.VeryStrong,
        >= 60 => IntentSignalStrength.Strong,
        >= 40 => IntentSignalStrength.Moderate,
        >= 20 => IntentSignalStrength.Weak,
        _ => IntentSignalStrength.None
    };

    private int EstimateDaysToConversion(decimal score) => score switch
    {
        >= 80 => 7,
        >= 60 => 14,
        >= 40 => 30,
        >= 20 => 60,
        _ => 90
    };

    #endregion

    #region Opportunity Insights

    /// <inheritdoc />
    public async Task<OpportunityInsight> GenerateOpportunityInsightAsync(int opportunityId, CancellationToken cancellationToken = default)
    {
        var opportunity = await _context.Opportunities
            .Include(o => o.Account)
            .Include(o => o.SalesOwner)
            .FirstOrDefaultAsync(o => o.Id == opportunityId, cancellationToken);

        if (opportunity == null)
        {
            throw new ArgumentException($"Opportunity with ID {opportunityId} not found", nameof(opportunityId));
        }

        // Build insight based on opportunity data
        var winProbability = CalculateWinProbability(opportunity);
        var healthScore = CalculateOpportunityHealth(opportunity);

        var insight = new OpportunityInsight
        {
            OpportunityId = opportunityId,
            WinProbability = winProbability,
            WinCategory = GetWinCategory(winProbability),
            Confidence = 0.8m,
            HealthScore = healthScore,
            HealthStatus = GetHealthStatus(healthScore),
            VelocityScore = CalculateVelocityScore(opportunity),
            EngagementScore = 70m, // Placeholder
            StakeholderScore = 60m, // Placeholder
            PredictedCloseDate = opportunity.ExpectedCloseDate,
            DaysToClose = opportunity.ExpectedCloseDate.HasValue
                ? (int)(opportunity.ExpectedCloseDate.Value - DateTime.UtcNow).TotalDays
                : null,
            OnTrackForClose = healthScore >= 60,
            PredictedValue = opportunity.Amount,
            WeightedValue = opportunity.Amount * winProbability,
            RiskLevel = 100 - healthScore,
            RecommendedAction = GetRecommendedAction(healthScore, winProbability),
            ActionPriority = healthScore switch { < 50 => 1, < 70 => 2, _ => 3 },
            AIInsights = $"Opportunity health score: {healthScore:F0}%. Win probability: {winProbability * 100:F0}%.",
            GeneratedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            ModelVersion = "1.0-olmo"
        };

        _context.OpportunityInsights.Add(insight);
        await _context.SaveChangesAsync(cancellationToken);

        return insight;
    }

    /// <inheritdoc />
    public async Task<decimal> PredictWinProbabilityAsync(int opportunityId, CancellationToken cancellationToken = default)
    {
        var opportunity = await _context.Opportunities
            .FirstOrDefaultAsync(o => o.Id == opportunityId, cancellationToken);

        if (opportunity == null)
        {
            return 0;
        }

        return CalculateWinProbability(opportunity);
    }

    /// <inheritdoc />
    public async Task<List<OpportunityInsight>> GetAtRiskOpportunitiesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.OpportunityInsights
            .Where(i => i.HealthStatus == DealHealthStatus.AtRisk ||
                        i.HealthStatus == DealHealthStatus.Critical ||
                        i.HealthStatus == DealHealthStatus.Stalled)
            .Where(i => i.ExpiresAt > DateTime.UtcNow && !i.IsDeleted)
            .OrderBy(i => i.HealthScore)
            .Include(i => i.Opportunity)
            .ToListAsync(cancellationToken);
    }

    private decimal CalculateWinProbability(Opportunity opportunity)
    {
        var stageFactor = opportunity.Stage switch
        {
            OpportunityStage.Discovery => 0.10m,
            OpportunityStage.Qualification => 0.25m,
            OpportunityStage.Proposal => 0.50m,
            OpportunityStage.Negotiation => 0.75m,
            OpportunityStage.ClosedWon => 1.0m,
            OpportunityStage.ClosedLost => 0.0m,
            _ => 0.3m
        };

        // Adjust based on amount and time in pipeline
        var daysSinceCreation = (DateTime.UtcNow - opportunity.CreatedAt).Days;
        var timeFactor = daysSinceCreation > 90 ? 0.8m : 1.0m;

        return stageFactor * timeFactor;
    }

    private decimal CalculateOpportunityHealth(Opportunity opportunity)
    {
        var score = 50m;

        // Stage progress
        if (opportunity.Stage >= OpportunityStage.Proposal)
        {
            score += 20;
        }
        else if (opportunity.Stage >= OpportunityStage.Qualification)
        {
            score += 10;
        }

        // Has expected close date
        if (opportunity.ExpectedCloseDate.HasValue)
        {
            score += 10;
        }

        // Amount specified
        if (opportunity.Amount > 0)
        {
            score += 10;
        }

        // Time factor - deduct if too long
        var daysSinceCreation = (DateTime.UtcNow - opportunity.CreatedAt).Days;
        if (daysSinceCreation > 90)
        {
            score -= 20;
        }
        else if (daysSinceCreation > 60)
        {
            score -= 10;
        }

        return Math.Max(0, Math.Min(100, score));
    }

    private decimal CalculateVelocityScore(Opportunity opportunity)
    {
        var daysSinceCreation = (DateTime.UtcNow - opportunity.CreatedAt).Days;
        var stageProgress = (int)opportunity.Stage / 5.0m * 100;

        if (daysSinceCreation == 0)
        {
            return 100;
        }

        // Expected: move ~1 stage per 2 weeks
        var expectedProgress = (daysSinceCreation / 14.0m) * 20m;
        var velocity = stageProgress / Math.Max(expectedProgress, 1) * 100;

        return Math.Max(0, Math.Min(100, velocity));
    }

    private WinProbabilityCategory GetWinCategory(decimal probability) => probability switch
    {
        >= 0.8m => WinProbabilityCategory.VeryLikely,
        >= 0.6m => WinProbabilityCategory.Likely,
        >= 0.4m => WinProbabilityCategory.Possible,
        >= 0.2m => WinProbabilityCategory.Unlikely,
        _ => WinProbabilityCategory.VeryUnlikely
    };

    private DealHealthStatus GetHealthStatus(decimal score) => score switch
    {
        >= 80 => DealHealthStatus.Accelerating,
        >= 60 => DealHealthStatus.Healthy,
        >= 40 => DealHealthStatus.AtRisk,
        >= 20 => DealHealthStatus.Stalled,
        _ => DealHealthStatus.Critical
    };

    private OpportunityActionType GetRecommendedAction(decimal healthScore, decimal winProbability)
    {
        if (healthScore < 40)
        {
            return OpportunityActionType.RiskMitigation;
        }
        if (winProbability >= 0.8m)
        {
            return OpportunityActionType.FastTrackClose;
        }
        if (winProbability >= 0.6m)
        {
            return OpportunityActionType.ProposalReview;
        }
        return OpportunityActionType.TechnicalDemo;
    }

    #endregion

    #region Churn Prediction

    /// <inheritdoc />
    public async Task<ChurnRisk> CalculateChurnRiskAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Accounts
            .Include(c => c.Opportunities)
            .FirstOrDefaultAsync(c => c.Id == accountId, cancellationToken);

        if (customer == null)
        {
            throw new ArgumentException($"Account with ID {accountId} not found", nameof(accountId));
        }

        var churnProbability = CalculateChurnProbability(customer);
        var healthScore = CalculateCustomerHealth(customer);

        var churnRisk = new ChurnRisk
        {
            AccountId = accountId,
            ChurnProbability = churnProbability,
            RiskLevel = GetChurnRiskLevel(churnProbability),
            Confidence = 0.75m,
            HealthScore = healthScore,
            HealthSegment = GetHealthSegment(healthScore),
            UsageScore = 60m, // Placeholder - would need actual usage data
            FeatureAdoption = 50m, // Placeholder
            DaysSinceLastLogin = 0, // Placeholder
            UsageTrend = 0,
            OpenTickets = 0, // Would need to query service requests
            ARRAtRisk = 0, // Would calculate from subscription data
            AccountTenureMonths = (int)((DateTime.UtcNow - customer.CreatedAt).Days / 30.0),
            RecommendedAction = GetRetentionAction(churnProbability),
            ActionUrgency = churnProbability switch { >= 0.7m => 1, >= 0.4m => 2, _ => 3 },
            AIInsights = $"Account health: {healthScore:F0}%. Churn risk: {churnProbability * 100:F0}%.",
            AssessedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            ModelVersion = "1.0-olmo"
        };

        _context.ChurnRisks.Add(churnRisk);
        await _context.SaveChangesAsync(cancellationToken);

        return churnRisk;
    }

    /// <inheritdoc />
    public async Task<List<ChurnRisk>> GetHighChurnRiskAccountsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _context.ChurnRisks
            .Where(c => c.RiskLevel >= ChurnRiskLevel.High && !c.IsDeleted)
            .Where(c => c.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(c => c.ChurnProbability)
            .Take(count)
            .Include(c => c.Account)
            .ToListAsync(cancellationToken);
    }

    private decimal CalculateChurnProbability(Account customer)
    {
        var risk = 0.3m; // Base risk

        // Account age - newer customers have higher churn risk
        var monthsSinceCreation = (DateTime.UtcNow - customer.CreatedAt).Days / 30.0m;
        if (monthsSinceCreation < 3)
        {
            risk += 0.2m;
        }
        else if (monthsSinceCreation < 6)
        {
            risk += 0.1m;
        }

        // Lifecycle stage factors
        if (customer.LifecycleStage == AccountLifecycleStage.Churned)
        {
            risk = 1.0m;
        }
        else if (customer.LifecycleStage == AccountLifecycleStage.AtRisk)
        {
            risk += 0.3m;
        }
        else if (customer.LifecycleStage == AccountLifecycleStage.Active)
        {
            risk -= 0.1m;
        }

        // Opportunities - active opportunities indicate engagement
        var activeOpportunities = customer.Opportunities?.Count(o =>
            o.Stage != OpportunityStage.ClosedWon && o.Stage != OpportunityStage.ClosedLost) ?? 0;
        if (activeOpportunities > 0)
        {
            risk -= 0.15m;
        }

        return Math.Max(0, Math.Min(1, risk));
    }

    private decimal CalculateCustomerHealth(Account customer)
    {
        var score = customer.AccountHealthScore; // Use the health score from entity

        if (score == 0)
        {
            // Calculate if not set
            score = 50;

            if (customer.LifecycleStage == AccountLifecycleStage.Active)
            {
                score += 20;
            }
            if (customer.LifecycleStage == AccountLifecycleStage.AtRisk)
            {
                score -= 20;
            }

            var monthsSinceCreation = (DateTime.UtcNow - customer.CreatedAt).Days / 30.0;
            if (monthsSinceCreation >= 12)
            {
                score += 15; // Loyal customer
            }
            if (monthsSinceCreation >= 24)
            {
                score += 10;
            }
        }

        return Math.Max(0, Math.Min(100, score));
    }

    private ChurnRiskLevel GetChurnRiskLevel(decimal probability) => probability switch
    {
        >= 0.8m => ChurnRiskLevel.Critical,
        >= 0.6m => ChurnRiskLevel.High,
        >= 0.4m => ChurnRiskLevel.Medium,
        >= 0.2m => ChurnRiskLevel.Low,
        _ => ChurnRiskLevel.VeryLow
    };

    private CustomerHealthSegment GetHealthSegment(decimal score) => score switch
    {
        >= 80 => CustomerHealthSegment.Champion,
        >= 60 => CustomerHealthSegment.Healthy,
        >= 40 => CustomerHealthSegment.Passive,
        >= 20 => CustomerHealthSegment.AtRisk,
        _ => CustomerHealthSegment.Critical
    };

    private RetentionActionType GetRetentionAction(decimal churnProbability)
    {
        if (churnProbability >= 0.8m)
        {
            return RetentionActionType.Escalate;
        }
        if (churnProbability >= 0.6m)
        {
            return RetentionActionType.ExecutiveReview;
        }
        if (churnProbability >= 0.4m)
        {
            return RetentionActionType.SuccessPlan;
        }
        return RetentionActionType.ProductTraining;
    }

    #endregion

    #region Next Best Action

    /// <inheritdoc />
    public async Task<List<ActionRecommendation>> GetRecommendedActionsAsync(
        ActionTargetType targetType,
        int entityId,
        int maxActions = 5,
        CancellationToken cancellationToken = default)
    {
        // First check for existing recommendations
        var existing = await _context.ActionRecommendations
            .Where(a => a.TargetType == targetType && a.TargetEntityId == entityId)
            .Where(a => a.Status == ActionRecommendationStatus.Pending)
            .Where(a => a.ExpiresAt > DateTime.UtcNow && !a.IsDeleted)
            .OrderByDescending(a => a.ImpactScore)
            .Take(maxActions)
            .ToListAsync(cancellationToken);

        if (existing.Count > 0)
        {
            return existing;
        }

        // Generate new recommendations
        var recommendations = await GenerateRecommendationsAsync(targetType, entityId, maxActions, cancellationToken);

        if (recommendations.Any())
        {
            _context.ActionRecommendations.AddRange(recommendations);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return recommendations;
    }

    /// <inheritdoc />
    public async Task<List<ActionRecommendation>> GetUserRecommendationsAsync(
        int userId,
        int maxActions = 20,
        CancellationToken cancellationToken = default)
    {
        return await _context.ActionRecommendations
            .Where(a => a.AssignedUserId == userId || a.AssignedUserId == null)
            .Where(a => a.Status == ActionRecommendationStatus.Pending)
            .Where(a => a.ExpiresAt > DateTime.UtcNow && !a.IsDeleted)
            .OrderByDescending(a => a.Priority)
            .ThenByDescending(a => a.ImpactScore)
            .Take(maxActions)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<ActionRecommendation>> GenerateRecommendationsAsync(
        ActionTargetType targetType,
        int entityId,
        int maxActions,
        CancellationToken cancellationToken)
    {
        var recommendations = new List<ActionRecommendation>();

        switch (targetType)
        {
            case ActionTargetType.Lead:
                var lead = await _context.Leads.FindAsync(new object[] { entityId }, cancellationToken);
                if (lead != null)
                {
                    recommendations.Add(CreateLeadRecommendation(lead));
                }
                break;

            case ActionTargetType.Opportunity:
                var opp = await _context.Opportunities.FindAsync(new object[] { entityId }, cancellationToken);
                if (opp != null)
                {
                    recommendations.Add(CreateOpportunityRecommendation(opp));
                }
                break;

            case ActionTargetType.Customer:
                var customer = await _context.Accounts.FindAsync(new object[] { entityId }, cancellationToken);
                if (customer != null)
                {
                    recommendations.Add(CreateAccountRecommendation(customer));
                }
                break;
        }

        return recommendations.Take(maxActions).ToList();
    }

    private ActionRecommendation CreateLeadRecommendation(Lead lead)
    {
        var actionType = lead.Status switch
        {
            LeadLifecycleStatus.New => NextBestActionType.Call,
            LeadLifecycleStatus.Nurturing => NextBestActionType.FollowUp,
            LeadLifecycleStatus.Working => NextBestActionType.Meeting,
            LeadLifecycleStatus.Qualified => NextBestActionType.Demo,
            _ => NextBestActionType.Email
        };

        return new ActionRecommendation
        {
            TargetType = ActionTargetType.Lead,
            TargetEntityId = lead.Id,
            TargetEntityName = lead.FullName,
            ActionType = actionType,
            Channel = actionType == NextBestActionType.Call ? ActionChannel.Phone : ActionChannel.Email,
            Title = $"{actionType} - {lead.FullName}",
            Description = GetActionDescription(actionType, lead),
            Rationale = $"Lead is in {lead.Status} status and requires follow-up.",
            Priority = ActionPriorityLevel.Medium,
            ImpactScore = 70,
            SuccessProbability = 0.6m,
            Confidence = 0.75m,
            RelevanceScore = 80,
            GeneratedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            ModelVersion = "1.0-olmo"
        };
    }

    private ActionRecommendation CreateOpportunityRecommendation(Opportunity opportunity)
    {
        var actionType = opportunity.Stage switch
        {
            OpportunityStage.Discovery => NextBestActionType.Call,
            OpportunityStage.Qualification => NextBestActionType.Meeting,
            OpportunityStage.Proposal => NextBestActionType.SendProposal,
            OpportunityStage.Negotiation => NextBestActionType.Negotiate,
            _ => NextBestActionType.FollowUp
        };

        return new ActionRecommendation
        {
            TargetType = ActionTargetType.Opportunity,
            TargetEntityId = opportunity.Id,
            TargetEntityName = opportunity.Name,
            ActionType = actionType,
            Channel = ActionChannel.Email,
            Title = $"{actionType} - {opportunity.Name}",
            Description = $"Progress opportunity to next stage with {actionType.ToString().ToLower()} action.",
            Rationale = $"Opportunity is at {opportunity.Stage} stage.",
            Priority = ActionPriorityLevel.High,
            ImpactScore = 85,
            EstimatedValueImpact = opportunity.Amount,
            SuccessProbability = 0.7m,
            Confidence = 0.8m,
            RelevanceScore = 90,
            GeneratedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            ModelVersion = "1.0-olmo"
        };
    }

    private ActionRecommendation CreateAccountRecommendation(Account account)
    {
        // Get account display name based on category
        var accountName = account.Category == AccountCategory.Organization
            ? account.Company
            : $"{account.FirstName} {account.LastName}".Trim();

        return new ActionRecommendation
        {
            TargetType = ActionTargetType.Customer,
            TargetEntityId = account.Id,
            TargetEntityName = accountName,
            ActionType = NextBestActionType.CheckIn,
            Channel = ActionChannel.Email,
            Title = $"Account Check-in - {accountName}",
            Description = "Schedule an account success check-in to maintain relationship.",
            Rationale = "Regular account engagement improves retention.",
            Priority = ActionPriorityLevel.Medium,
            ImpactScore = 60,
            SuccessProbability = 0.8m,
            Confidence = 0.7m,
            RelevanceScore = 70,
            GeneratedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            ModelVersion = "1.0-olmo"
        };
    }

    private string GetActionDescription(NextBestActionType actionType, Lead lead)
    {
        return actionType switch
        {
            NextBestActionType.Call => $"Call {lead.FirstName} to discuss their needs and qualify the opportunity.",
            NextBestActionType.Email => $"Send a follow-up email to {lead.FirstName} with relevant information.",
            NextBestActionType.Meeting => $"Schedule a discovery meeting with {lead.FirstName}.",
            NextBestActionType.Demo => $"Arrange a product demo for {lead.FirstName}.",
            NextBestActionType.FollowUp => $"Follow up with {lead.FirstName} on previous conversation.",
            _ => $"Take action on lead {lead.FullName}."
        };
    }

    #endregion

    #region Email Intelligence

    /// <inheritdoc />
    public async Task<EmailIntelligence> AnalyzeEmailAsync(
        string emailContent,
        string? subject = null,
        CancellationToken cancellationToken = default)
    {
        var prompt = $"""
            Analyze this email and provide:
            1. Sentiment (very negative, negative, neutral, positive, very positive)
            2. Primary intent (inquiry, purchase intent, support request, complaint, feedback, meeting request, etc.)
            3. Urgency (immediate, high, normal, low, no response needed)
            4. Key action items
            5. Brief summary

            Subject: {subject ?? "No subject"}

            Email content:
            {emailContent}

            Respond in JSON format.
            """;

        var response = await InvokeModelAsync(prompt, cancellationToken);

        // Parse response and create EmailIntelligence
        var sentiment = ExtractSentiment(response);
        var intent = ExtractIntent(response);
        var urgency = ExtractUrgency(response);

        return new EmailIntelligence
        {
            EmailMessageId = Guid.NewGuid().ToString(),
            Sentiment = sentiment,
            SentimentScore = GetSentimentScore(sentiment),
            SentimentConfidence = 0.8m,
            PrimaryIntent = intent,
            IntentConfidence = 0.75m,
            Urgency = urgency,
            UrgencyScore = GetUrgencyScore(urgency),
            Summary = ExtractSummary(response) ?? "Email analyzed.",
            AnalyzedAt = DateTime.UtcNow,
            ProcessingTimeMs = 100, // Placeholder
            ModelVersion = "1.0-olmo"
        };
    }

    /// <inheritdoc />
    public async Task<string> GenerateEmailResponseAsync(
        string emailContent,
        string? context = null,
        CancellationToken cancellationToken = default)
    {
        var prompt = $"""
            Generate a professional response to this email.

            Original email:
            {emailContent}

            {(context != null ? $"Additional context: {context}" : "")}

            Write a helpful, professional response.
            """;

        return await InvokeModelAsync(prompt, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<string>> ExtractActionItemsAsync(string emailContent, CancellationToken cancellationToken = default)
    {
        var prompt = $"""
            Extract action items from this email. List each action item on a separate line.

            Email:
            {emailContent}

            Action items:
            """;

        var response = await InvokeModelAsync(prompt, cancellationToken);

        return response
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().TrimStart('-', '*', '•', ' '))
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
    }

    private EmailSentiment ExtractSentiment(string response)
    {
        var lower = response.ToLower();
        if (lower.Contains("very positive"))
        {
            return EmailSentiment.VeryPositive;
        }
        if (lower.Contains("very negative"))
        {
            return EmailSentiment.VeryNegative;
        }
        if (lower.Contains("positive"))
        {
            return EmailSentiment.Positive;
        }
        if (lower.Contains("negative"))
        {
            return EmailSentiment.Negative;
        }
        return EmailSentiment.Neutral;
    }

    private EmailIntent ExtractIntent(string response)
    {
        var lower = response.ToLower();
        if (lower.Contains("purchase") || lower.Contains("buy"))
        {
            return EmailIntent.PurchaseIntent;
        }
        if (lower.Contains("support") || lower.Contains("help"))
        {
            return EmailIntent.SupportRequest;
        }
        if (lower.Contains("complaint") || lower.Contains("unhappy"))
        {
            return EmailIntent.Complaint;
        }
        if (lower.Contains("meeting") || lower.Contains("schedule"))
        {
            return EmailIntent.MeetingRequest;
        }
        if (lower.Contains("question") || lower.Contains("inquiry"))
        {
            return EmailIntent.Inquiry;
        }
        if (lower.Contains("feedback"))
        {
            return EmailIntent.Feedback;
        }
        if (lower.Contains("follow"))
        {
            return EmailIntent.FollowUp;
        }
        return EmailIntent.Other;
    }

    private ResponseUrgency ExtractUrgency(string response)
    {
        var lower = response.ToLower();
        if (lower.Contains("immediate") || lower.Contains("urgent"))
        {
            return ResponseUrgency.Immediate;
        }
        if (lower.Contains("high"))
        {
            return ResponseUrgency.High;
        }
        if (lower.Contains("low"))
        {
            return ResponseUrgency.Low;
        }
        if (lower.Contains("no response"))
        {
            return ResponseUrgency.NoResponse;
        }
        return ResponseUrgency.Normal;
    }

    private decimal GetSentimentScore(EmailSentiment sentiment) => sentiment switch
    {
        EmailSentiment.VeryPositive => 1.0m,
        EmailSentiment.Positive => 0.5m,
        EmailSentiment.Neutral => 0.0m,
        EmailSentiment.Negative => -0.5m,
        EmailSentiment.VeryNegative => -1.0m,
        _ => 0.0m
    };

    private decimal GetUrgencyScore(ResponseUrgency urgency) => urgency switch
    {
        ResponseUrgency.Immediate => 100,
        ResponseUrgency.High => 75,
        ResponseUrgency.Normal => 50,
        ResponseUrgency.Low => 25,
        ResponseUrgency.NoResponse => 0,
        _ => 50
    };

    private string? ExtractSummary(string response)
    {
        var summaryIndex = response.IndexOf("summary", StringComparison.OrdinalIgnoreCase);
        if (summaryIndex >= 0 && summaryIndex + 10 < response.Length)
        {
            var endIndex = response.IndexOf('\n', summaryIndex);
            if (endIndex > summaryIndex)
            {
                return response[(summaryIndex + 8)..endIndex].Trim().Trim(':', '"', ' ');
            }
        }
        return null;
    }

    #endregion

    #region Text Generation

    /// <inheritdoc />
    public async Task<string> GenerateTextAsync(string prompt, int maxTokens = 500, CancellationToken cancellationToken = default)
    {
        return await InvokeModelAsync(prompt, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string> SummarizeTextAsync(string content, int maxLength = 200, CancellationToken cancellationToken = default)
    {
        var prompt = $"""
            Summarize the following text in {maxLength} characters or less:

            {content}

            Summary:
            """;

        return await InvokeModelAsync(prompt, cancellationToken);
    }

    #endregion

    #region Model Management

    /// <inheritdoc />
    public async Task<List<AIModel>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AIModels
            .Where(m => m.Status == AIModelStatus.Active && !m.IsDeleted)
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> CheckModelHealthAsync(AIProvider provider, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = provider switch
            {
                AIProvider.AllenAI_OLMo => _config.OLMoEndpoint,
                AIProvider.AllenAI_Tulu => _config.TuluEndpoint,
                _ => null
            };

            if (string.IsNullOrEmpty(endpoint))
            {
                return false;
            }

            using var client = _httpClientFactory.CreateClient("AllenAI");
            var response = await client.GetAsync(endpoint, cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed for provider {Provider}", provider);
            return false;
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task<string> InvokeModelAsync(string prompt, CancellationToken cancellationToken)
    {
        var endpoint = _config.DefaultProvider switch
        {
            AIProvider.AllenAI_Tulu => _config.TuluEndpoint,
            _ => _config.OLMoEndpoint
        };

        try
        {
            using var client = _httpClientFactory.CreateClient("AllenAI");
            client.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);

            if (!string.IsNullOrEmpty(_config.HuggingFaceApiKey))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.HuggingFaceApiKey);
            }

            var request = new { inputs = prompt };
            var response = await client.PostAsJsonAsync(endpoint, request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync(cancellationToken);
                return ParseHuggingFaceResponse(result);
            }

            _logger.LogWarning("AI model request failed: {StatusCode}", response.StatusCode);

            // Fallback to local processing
            if (_config.EnableLocalFallback)
            {
                return GenerateLocalResponse(prompt);
            }

            return "Unable to process request.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking AI model");

            if (_config.EnableLocalFallback)
            {
                return GenerateLocalResponse(prompt);
            }

            throw;
        }
    }

    private string ParseHuggingFaceResponse(string response)
    {
        try
        {
            using var doc = JsonDocument.Parse(response);
            if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
            {
                var first = doc.RootElement[0];
                if (first.TryGetProperty("generated_text", out var text))
                {
                    return text.GetString() ?? "";
                }
            }
            return response;
        }
        catch
        {
            return response;
        }
    }

    private string GenerateLocalResponse(string prompt)
    {
        // Simple local fallback for when API is unavailable
        return $"Analysis completed locally. Prompt length: {prompt.Length} characters.";
    }

    private decimal ExtractNumberFromText(string text, string field, decimal defaultValue)
    {
        try
        {
            var fieldIndex = text.IndexOf(field, StringComparison.OrdinalIgnoreCase);
            if (fieldIndex >= 0)
            {
                var afterField = text[(fieldIndex + field.Length)..];
                var match = System.Text.RegularExpressions.Regex.Match(afterField, @"\d+\.?\d*");
                if (match.Success && decimal.TryParse(match.Value, out var value))
                {
                    return value;
                }
            }
        }
        catch { }
        return defaultValue;
    }

    private string? ExtractTextBetween(string text, string start, string end)
    {
        try
        {
            var startIndex = text.IndexOf(start, StringComparison.OrdinalIgnoreCase);
            if (startIndex < 0)
            {
                return null;
            }

            var endIndex = text.IndexOf(end, startIndex + start.Length, StringComparison.OrdinalIgnoreCase);
            if (endIndex < 0)
            {
                endIndex = text.Length;
            }

            return text[(startIndex + start.Length)..endIndex].Trim().Trim(':', '"', ' ');
        }
        catch
        {
            return null;
        }
    }

    #endregion
}
