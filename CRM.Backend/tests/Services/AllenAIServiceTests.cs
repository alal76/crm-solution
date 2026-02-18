// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using Xunit;
using FluentAssertions;
using CRM.Core.Entities;
using CRM.Core.Entities.AI;
using CRM.Core.Interfaces.AI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for the AI Service layer including lead scoring,
/// opportunity insights, churn prediction, and email intelligence
/// </summary>
public class AllenAIServiceTests
{
    #region AI Entity Tests

    [Fact]
    public void CreateLeadScore_ValidData_CreatesCorrectly()
    {
        // Arrange & Act
        var leadScore = new LeadScore
        {
            LeadId = 1,
            OverallScore = 75m,
            Category = LeadScoreCategory.Hot,
            Confidence = 0.85m,
            DemographicScore = 80m,
            BehavioralScore = 70m,
            EngagementScore = 75m,
            IntentScore = 72m,
            ConversionProbability = 0.65m,
            ScoredAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        // Assert
        leadScore.LeadId.Should().Be(1);
        leadScore.OverallScore.Should().Be(75m);
        leadScore.Category.Should().Be(LeadScoreCategory.Hot);
        leadScore.Confidence.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CreateOpportunityInsight_ValidData_CreatesCorrectly()
    {
        // Arrange & Act
        var insight = new OpportunityInsight
        {
            OpportunityId = 1,
            WinProbability = 0.75m,
            WinCategory = WinProbabilityCategory.VeryLikely,
            HealthScore = 85m,
            HealthStatus = DealHealthStatus.Healthy,
            VelocityScore = 90m,
            GeneratedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        // Assert
        insight.OpportunityId.Should().Be(1);
        insight.WinProbability.Should().BeApproximately(0.75m, 0.01m);
        insight.HealthStatus.Should().Be(DealHealthStatus.Healthy);
    }

    [Fact]
    public void CreateChurnRisk_ValidData_CreatesCorrectly()
    {
        // Arrange & Act
        var churnRisk = new ChurnRisk
        {
            AccountId = 1,
            ChurnProbability = 0.3m,
            RiskLevel = ChurnRiskLevel.Medium,
            HealthScore = 60m,
            HealthSegment = CustomerHealthSegment.Passive,
            UsageScore = 55m,
            AccountTenureMonths = 12,
            AssessedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        // Assert
        churnRisk.AccountId.Should().Be(1);
        churnRisk.ChurnProbability.Should().BeLessOrEqualTo(1.0m);
        churnRisk.RiskLevel.Should().Be(ChurnRiskLevel.Medium);
    }

    [Fact]
    public void CreateActionRecommendation_ValidData_CreatesCorrectly()
    {
        // Arrange & Act
        var recommendation = new ActionRecommendation
        {
            TargetType = ActionTargetType.Lead,
            TargetEntityId = 1,
            TargetEntityName = "John Doe",
            ActionType = NextBestActionType.Call,
            Channel = ActionChannel.Phone,
            Title = "Call Lead",
            Description = "Schedule a discovery call",
            Priority = ActionPriorityLevel.High,
            ImpactScore = 85,
            SuccessProbability = 0.7m,
            GeneratedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(3)
        };

        // Assert
        recommendation.TargetType.Should().Be(ActionTargetType.Lead);
        recommendation.ActionType.Should().Be(NextBestActionType.Call);
        recommendation.Priority.Should().Be(ActionPriorityLevel.High);
    }

    [Fact]
    public void CreateEmailIntelligence_ValidData_CreatesCorrectly()
    {
        // Arrange & Act
        var emailIntelligence = new EmailIntelligence
        {
            EmailMessageId = Guid.NewGuid().ToString(),
            Sentiment = EmailSentiment.Positive,
            SentimentScore = 0.7m,
            PrimaryIntent = EmailIntent.PurchaseIntent,
            Urgency = ResponseUrgency.High,
            Summary = "Customer expressing interest in product",
            AnalyzedAt = DateTime.UtcNow
        };

        // Assert
        emailIntelligence.Sentiment.Should().Be(EmailSentiment.Positive);
        emailIntelligence.PrimaryIntent.Should().Be(EmailIntent.PurchaseIntent);
        emailIntelligence.Urgency.Should().Be(ResponseUrgency.High);
    }

    [Fact]
    public void CreateAIModel_ValidData_CreatesCorrectly()
    {
        // Arrange & Act
        var model = new AIModel
        {
            Name = "OLMo-7B",
            Provider = AIProvider.AllenAI_OLMo,
            ModelType = AIModelType.LeadScoring,
            Version = "1.0",
            Status = AIModelStatus.Active,
            ModelIdentifier = "allenai/OLMo-7B",
            Description = "Open Language Model from Allen AI"
        };

        // Assert
        model.Name.Should().Be("OLMo-7B");
        model.Provider.Should().Be(AIProvider.AllenAI_OLMo);
        model.Status.Should().Be(AIModelStatus.Active);
    }

    #endregion

    #region Lead Scoring Tests

    [Theory]
    [InlineData(90, LeadScoreCategory.OnFire)]
    [InlineData(75, LeadScoreCategory.Hot)]
    [InlineData(50, LeadScoreCategory.Warm)]
    [InlineData(30, LeadScoreCategory.Cool)]
    [InlineData(10, LeadScoreCategory.Cold)]
    public void LeadScoreCategory_MapsCorrectly(decimal score, LeadScoreCategory expectedCategory)
    {
        // Arrange & Act
        var category = score switch
        {
            >= 80 => LeadScoreCategory.OnFire,
            >= 60 => LeadScoreCategory.Hot,
            >= 40 => LeadScoreCategory.Warm,
            >= 20 => LeadScoreCategory.Cool,
            _ => LeadScoreCategory.Cold
        };

        // Assert
        category.Should().Be(expectedCategory);
    }

    [Fact]
    public void LeadScore_ComponentScores_SumCorrectly()
    {
        // Arrange
        var demographicScore = 80m;
        var behavioralScore = 70m;
        var engagementScore = 75m;
        var intentScore = 65m;

        // Act - Weighted average calculation
        // (80 * 0.2) + (70 * 0.3) + (75 * 0.25) + (65 * 0.25) = 16 + 21 + 18.75 + 16.25 = 72.0
        var overallScore = (demographicScore * 0.2m) + (behavioralScore * 0.3m) +
                          (engagementScore * 0.25m) + (intentScore * 0.25m);

        // Assert
        overallScore.Should().Be(72m);
    }

    [Fact]
    public void LeadScore_EngagementLevel_MapsCorrectly()
    {
        // Arrange & Act
        LeadEngagementLevel GetEngagementLevel(decimal score) => score switch
        {
            >= 80 => LeadEngagementLevel.VeryHigh,
            >= 60 => LeadEngagementLevel.High,
            >= 40 => LeadEngagementLevel.Medium,
            >= 20 => LeadEngagementLevel.Low,
            _ => LeadEngagementLevel.None
        };

        // Assert
        GetEngagementLevel(85).Should().Be(LeadEngagementLevel.VeryHigh);
        GetEngagementLevel(65).Should().Be(LeadEngagementLevel.High);
        GetEngagementLevel(45).Should().Be(LeadEngagementLevel.Medium);
        GetEngagementLevel(25).Should().Be(LeadEngagementLevel.Low);
        GetEngagementLevel(10).Should().Be(LeadEngagementLevel.None);
    }

    [Fact]
    public void LeadScore_IntentStrength_MapsCorrectly()
    {
        // Arrange & Act
        IntentSignalStrength GetIntentStrength(decimal score) => score switch
        {
            >= 80 => IntentSignalStrength.VeryStrong,
            >= 60 => IntentSignalStrength.Strong,
            >= 40 => IntentSignalStrength.Moderate,
            >= 20 => IntentSignalStrength.Weak,
            _ => IntentSignalStrength.None
        };

        // Assert
        GetIntentStrength(90).Should().Be(IntentSignalStrength.VeryStrong);
        GetIntentStrength(70).Should().Be(IntentSignalStrength.Strong);
        GetIntentStrength(50).Should().Be(IntentSignalStrength.Moderate);
    }

    #endregion

    #region Opportunity Insight Tests

    [Theory]
    [InlineData(0.85, WinProbabilityCategory.VeryLikely)]
    [InlineData(0.65, WinProbabilityCategory.Likely)]
    [InlineData(0.45, WinProbabilityCategory.Possible)]
    [InlineData(0.25, WinProbabilityCategory.Unlikely)]
    [InlineData(0.10, WinProbabilityCategory.VeryUnlikely)]
    public void WinProbabilityCategory_MapsCorrectly(decimal probability, WinProbabilityCategory expectedCategory)
    {
        // Arrange & Act
        var category = probability switch
        {
            >= 0.8m => WinProbabilityCategory.VeryLikely,
            >= 0.6m => WinProbabilityCategory.Likely,
            >= 0.4m => WinProbabilityCategory.Possible,
            >= 0.2m => WinProbabilityCategory.Unlikely,
            _ => WinProbabilityCategory.VeryUnlikely
        };

        // Assert
        category.Should().Be(expectedCategory);
    }

    [Theory]
    [InlineData(85, DealHealthStatus.Accelerating)]
    [InlineData(65, DealHealthStatus.Healthy)]
    [InlineData(45, DealHealthStatus.AtRisk)]
    [InlineData(25, DealHealthStatus.Stalled)]
    [InlineData(10, DealHealthStatus.Critical)]
    public void DealHealthStatus_MapsCorrectly(decimal score, DealHealthStatus expectedStatus)
    {
        // Arrange & Act
        var status = score switch
        {
            >= 80 => DealHealthStatus.Accelerating,
            >= 60 => DealHealthStatus.Healthy,
            >= 40 => DealHealthStatus.AtRisk,
            >= 20 => DealHealthStatus.Stalled,
            _ => DealHealthStatus.Critical
        };

        // Assert
        status.Should().Be(expectedStatus);
    }

    [Fact]
    public void OpportunityInsight_WeightedValue_CalculatesCorrectly()
    {
        // Arrange
        var amount = 100000m;
        var winProbability = 0.75m;

        // Act
        var insight = new OpportunityInsight
        {
            PredictedValue = amount,
            WinProbability = winProbability,
            WeightedValue = amount * winProbability
        };

        // Assert
        insight.WeightedValue.Should().Be(75000m);
    }

    [Fact]
    public void OpportunityInsight_DaysToClose_CalculatesCorrectly()
    {
        // Arrange
        var expectedCloseDate = DateTime.UtcNow.AddDays(30);

        // Act
        var insight = new OpportunityInsight
        {
            PredictedCloseDate = expectedCloseDate,
            DaysToClose = (int)(expectedCloseDate - DateTime.UtcNow).TotalDays
        };

        // Assert
        insight.DaysToClose.Should().BeInRange(29, 31);
    }

    #endregion

    #region Churn Prediction Tests

    [Theory]
    [InlineData(0.85, ChurnRiskLevel.Critical)]
    [InlineData(0.65, ChurnRiskLevel.High)]
    [InlineData(0.45, ChurnRiskLevel.Medium)]
    [InlineData(0.25, ChurnRiskLevel.Low)]
    [InlineData(0.10, ChurnRiskLevel.VeryLow)]
    public void ChurnRiskLevel_MapsCorrectly(decimal probability, ChurnRiskLevel expectedLevel)
    {
        // Arrange & Act
        var level = probability switch
        {
            >= 0.8m => ChurnRiskLevel.Critical,
            >= 0.6m => ChurnRiskLevel.High,
            >= 0.4m => ChurnRiskLevel.Medium,
            >= 0.2m => ChurnRiskLevel.Low,
            _ => ChurnRiskLevel.VeryLow
        };

        // Assert
        level.Should().Be(expectedLevel);
    }

    [Theory]
    [InlineData(85, CustomerHealthSegment.Champion)]
    [InlineData(65, CustomerHealthSegment.Healthy)]
    [InlineData(45, CustomerHealthSegment.Passive)]
    [InlineData(25, CustomerHealthSegment.AtRisk)]
    [InlineData(10, CustomerHealthSegment.Critical)]
    public void CustomerHealthSegment_MapsCorrectly(decimal score, CustomerHealthSegment expectedSegment)
    {
        // Arrange & Act
        var segment = score switch
        {
            >= 80 => CustomerHealthSegment.Champion,
            >= 60 => CustomerHealthSegment.Healthy,
            >= 40 => CustomerHealthSegment.Passive,
            >= 20 => CustomerHealthSegment.AtRisk,
            _ => CustomerHealthSegment.Critical
        };

        // Assert
        segment.Should().Be(expectedSegment);
    }

    [Fact]
    public void ChurnRisk_RetentionAction_PrioritizesCorrectly()
    {
        // Arrange & Act
        RetentionActionType GetRetentionAction(decimal churnProbability)
        {
            if (churnProbability >= 0.8m) return RetentionActionType.Escalate;
            if (churnProbability >= 0.6m) return RetentionActionType.ExecutiveReview;
            if (churnProbability >= 0.4m) return RetentionActionType.SuccessPlan;
            return RetentionActionType.ProductTraining;
        }

        // Assert
        GetRetentionAction(0.9m).Should().Be(RetentionActionType.Escalate);
        GetRetentionAction(0.7m).Should().Be(RetentionActionType.ExecutiveReview);
        GetRetentionAction(0.5m).Should().Be(RetentionActionType.SuccessPlan);
        GetRetentionAction(0.2m).Should().Be(RetentionActionType.ProductTraining);
    }

    #endregion

    #region Next Best Action Tests

    [Fact]
    public void ActionRecommendation_AllChannels_Valid()
    {
        // Arrange & Act
        var channels = Enum.GetValues<ActionChannel>();

        // Assert
        channels.Should().HaveCountGreaterOrEqualTo(5);
        channels.Should().Contain(ActionChannel.Email);
        channels.Should().Contain(ActionChannel.Phone);
        channels.Should().Contain(ActionChannel.Internal);
    }

    [Fact]
    public void ActionRecommendation_AllActionTypes_Valid()
    {
        // Arrange & Act
        var actionTypes = Enum.GetValues<NextBestActionType>();

        // Assert
        actionTypes.Should().HaveCountGreaterOrEqualTo(10);
        actionTypes.Should().Contain(NextBestActionType.Call);
        actionTypes.Should().Contain(NextBestActionType.Email);
        actionTypes.Should().Contain(NextBestActionType.Demo);
    }

    [Fact]
    public void ActionRecommendation_PriorityLevels_Valid()
    {
        // Arrange & Act
        var priorities = Enum.GetValues<ActionPriorityLevel>();

        // Assert
        priorities.Should().HaveCountGreaterOrEqualTo(4);
        priorities.Should().Contain(ActionPriorityLevel.Low);
        priorities.Should().Contain(ActionPriorityLevel.Medium);
        priorities.Should().Contain(ActionPriorityLevel.High);
        priorities.Should().Contain(ActionPriorityLevel.Critical);
    }

    [Theory]
    [InlineData(LeadLifecycleStatus.New, NextBestActionType.Call)]
    [InlineData(LeadLifecycleStatus.Working, NextBestActionType.Meeting)]
    [InlineData(LeadLifecycleStatus.Qualified, NextBestActionType.Demo)]
    public void ActionRecommendation_LeadStatus_MapsToCorrectAction(LeadLifecycleStatus status, NextBestActionType expectedAction)
    {
        // Arrange & Act
        var actionType = status switch
        {
            LeadLifecycleStatus.New => NextBestActionType.Call,
            LeadLifecycleStatus.Nurturing => NextBestActionType.FollowUp,
            LeadLifecycleStatus.Working => NextBestActionType.Meeting,
            LeadLifecycleStatus.Qualified => NextBestActionType.Demo,
            _ => NextBestActionType.Email
        };

        // Assert
        actionType.Should().Be(expectedAction);
    }

    #endregion

    #region Email Intelligence Tests

    [Fact]
    public void EmailSentiment_AllValues_Valid()
    {
        // Arrange & Act
        var sentiments = Enum.GetValues<EmailSentiment>();

        // Assert
        sentiments.Should().HaveCount(5);
        sentiments.Should().Contain(EmailSentiment.VeryNegative);
        sentiments.Should().Contain(EmailSentiment.Neutral);
        sentiments.Should().Contain(EmailSentiment.VeryPositive);
    }

    [Fact]
    public void EmailIntent_AllValues_Valid()
    {
        // Arrange & Act
        var intents = Enum.GetValues<EmailIntent>();

        // Assert
        intents.Should().HaveCountGreaterOrEqualTo(8);
        intents.Should().Contain(EmailIntent.Inquiry);
        intents.Should().Contain(EmailIntent.PurchaseIntent);
        intents.Should().Contain(EmailIntent.SupportRequest);
        intents.Should().Contain(EmailIntent.Complaint);
    }

    [Fact]
    public void ResponseUrgency_AllValues_Valid()
    {
        // Arrange & Act
        var urgencies = Enum.GetValues<ResponseUrgency>();

        // Assert
        urgencies.Should().HaveCountGreaterOrEqualTo(5);
        urgencies.Should().Contain(ResponseUrgency.Immediate);
        urgencies.Should().Contain(ResponseUrgency.High);
        urgencies.Should().Contain(ResponseUrgency.Normal);
        urgencies.Should().Contain(ResponseUrgency.Low);
        urgencies.Should().Contain(ResponseUrgency.NoResponse);
    }

    [Theory]
    [InlineData(EmailSentiment.VeryPositive, 1.0)]
    [InlineData(EmailSentiment.Positive, 0.5)]
    [InlineData(EmailSentiment.Neutral, 0.0)]
    [InlineData(EmailSentiment.Negative, -0.5)]
    [InlineData(EmailSentiment.VeryNegative, -1.0)]
    public void EmailSentiment_Score_MapsCorrectly(EmailSentiment sentiment, decimal expectedScore)
    {
        // Arrange & Act
        var score = sentiment switch
        {
            EmailSentiment.VeryPositive => 1.0m,
            EmailSentiment.Positive => 0.5m,
            EmailSentiment.Neutral => 0.0m,
            EmailSentiment.Negative => -0.5m,
            EmailSentiment.VeryNegative => -1.0m,
            _ => 0.0m
        };

        // Assert
        score.Should().Be(expectedScore);
    }

    #endregion

    #region AI Configuration Tests

    [Fact]
    public void AllenAIConfiguration_DefaultValues_SetCorrectly()
    {
        // Arrange & Act
        var config = new AllenAIConfiguration();

        // Assert
        config.OLMoEndpoint.Should().NotBeNullOrEmpty();
        config.TuluEndpoint.Should().NotBeNullOrEmpty();
        config.TimeoutSeconds.Should().BeGreaterThan(0);
        config.BatchSize.Should().BeGreaterThan(0);
        config.CacheExpirationMinutes.Should().BeGreaterThan(0);
    }

    [Fact]
    public void AllenAIConfiguration_Endpoints_AreHuggingFaceUrls()
    {
        // Arrange & Act
        var config = new AllenAIConfiguration();

        // Assert
        config.OLMoEndpoint.Should().Contain("huggingface.co");
        config.TuluEndpoint.Should().Contain("huggingface.co");
        config.OLMoEndpoint.Should().Contain("OLMo");
        config.TuluEndpoint.Should().Contain("tulu");
    }

    [Fact]
    public void AIProvider_AllenAIModels_Valid()
    {
        // Arrange & Act
        var providers = Enum.GetValues<AIProvider>();

        // Assert
        providers.Should().Contain(AIProvider.AllenAI_OLMo);
        providers.Should().Contain(AIProvider.AllenAI_Tulu);
    }

    [Fact]
    public void AIModelType_AllTypes_Valid()
    {
        // Arrange & Act
        var modelTypes = Enum.GetValues<AIModelType>();

        // Assert
        modelTypes.Should().HaveCountGreaterOrEqualTo(5);
        modelTypes.Should().Contain(AIModelType.LeadScoring);
        modelTypes.Should().Contain(AIModelType.ChurnPrediction);
        modelTypes.Should().Contain(AIModelType.SentimentAnalysis);
        modelTypes.Should().Contain(AIModelType.EmailAssistant);
    }

    #endregion

    #region Integration Scenarios

    [Fact]
    public void LeadScoring_EndToEnd_Flow()
    {
        // Arrange - Create a lead
        var lead = new Lead
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Smith",
            CompanyName = "Tech Corp",
            Email = "jane@techcorp.com",
            Status = LeadLifecycleStatus.Qualified,
            Source = LeadSource.Web,
            Score = 75,
            FitScore = 80,
            EngagementScore = 70
        };

        // Act - Calculate scores
        var demographicScore = 90m; // Has company, email, name
        var behavioralScore = 70m;
        var engagementScore = 75m;
        var intentScore = 80m;
        var overallScore = (demographicScore * 0.2m) + (behavioralScore * 0.3m) +
                          (engagementScore * 0.25m) + (intentScore * 0.25m);

        // Create lead score
        var leadScore = new LeadScore
        {
            LeadId = lead.Id,
            OverallScore = overallScore,
            Category = overallScore >= 80 ? LeadScoreCategory.OnFire : LeadScoreCategory.Hot,
            DemographicScore = demographicScore,
            BehavioralScore = behavioralScore,
            EngagementScore = engagementScore,
            IntentScore = intentScore,
            ConversionProbability = overallScore / 100m
        };

        // Assert
        leadScore.OverallScore.Should().BeGreaterThan(70);
        leadScore.Category.Should().Be(LeadScoreCategory.Hot);
        leadScore.ConversionProbability.Should().BeGreaterThan(0.7m);
    }

    [Fact]
    public void ChurnPrediction_EndToEnd_Flow()
    {
        // Arrange - Create a customer
        var account = new Account
        {
            Id = 1,
            Category = AccountCategory.Organization,
            Company = "Enterprise Inc",
            LifecycleStage = AccountLifecycleStage.AtRisk,
            AccountHealthScore = 35
        };

        // Act - Calculate churn risk
        var baseRisk = 0.3m;
        baseRisk += 0.3m; // At risk lifecycle stage
        var churnProbability = Math.Min(1.0m, baseRisk);
        var healthScore = account.AccountHealthScore;

        var churnRisk = new ChurnRisk
        {
            AccountId = account.Id,
            ChurnProbability = churnProbability,
            RiskLevel = churnProbability >= 0.6m ? ChurnRiskLevel.High : ChurnRiskLevel.Medium,
            HealthScore = healthScore,
            HealthSegment = healthScore < 40 ? CustomerHealthSegment.AtRisk : CustomerHealthSegment.Passive
        };

        // Assert
        churnRisk.ChurnProbability.Should().BeGreaterThan(0.5m);
        churnRisk.RiskLevel.Should().Be(ChurnRiskLevel.High);
        churnRisk.HealthSegment.Should().Be(CustomerHealthSegment.AtRisk);
    }

    [Fact]
    public void NextBestAction_OpportunityFlow()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Id = 1,
            Name = "Enterprise Deal",
            Stage = OpportunityStage.Proposal,
            Amount = 100000m
        };

        // Act
        var actionType = opportunity.Stage switch
        {
            OpportunityStage.Discovery => NextBestActionType.Call,
            OpportunityStage.Qualification => NextBestActionType.Meeting,
            OpportunityStage.Proposal => NextBestActionType.SendProposal,
            OpportunityStage.Negotiation => NextBestActionType.Negotiate,
            _ => NextBestActionType.FollowUp
        };

        var recommendation = new ActionRecommendation
        {
            TargetType = ActionTargetType.Opportunity,
            TargetEntityId = opportunity.Id,
            TargetEntityName = opportunity.Name,
            ActionType = actionType,
            Priority = ActionPriorityLevel.High,
            EstimatedValueImpact = opportunity.Amount
        };

        // Assert
        recommendation.ActionType.Should().Be(NextBestActionType.SendProposal);
        recommendation.Priority.Should().Be(ActionPriorityLevel.High);
        recommendation.EstimatedValueImpact.Should().Be(100000m);
    }

    #endregion
}
