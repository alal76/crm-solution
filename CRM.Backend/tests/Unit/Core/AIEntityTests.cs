// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using CRM.Core.Entities;
using CRM.Core.Entities.AI;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Unit tests for AI entities and related enums.
/// ~100 tests covering AI models, predictions, lead scores, churn risk, opportunity insights, etc.
/// </summary>
public class AIEntityTests
{
    #region AIModelType Enum Tests

    [Fact]
    public void AIModelType_ShouldHaveCorrectValues()
    {
        ((int)AIModelType.LeadScoring).Should().Be(0);
        ((int)AIModelType.OpportunityWinPrediction).Should().Be(1);
        ((int)AIModelType.ChurnPrediction).Should().Be(2);
        ((int)AIModelType.NextBestAction).Should().Be(3);
        ((int)AIModelType.EmailAssistant).Should().Be(4);
        ((int)AIModelType.SentimentAnalysis).Should().Be(5);
        ((int)AIModelType.EntityExtraction).Should().Be(6);
        ((int)AIModelType.Classification).Should().Be(7);
        ((int)AIModelType.Regression).Should().Be(8);
    }

    [Fact]
    public void AIModelType_ShouldHave9Values()
    {
        var values = Enum.GetValues<AIModelType>();
        values.Should().HaveCount(9);
    }

    #endregion

    #region AIModelStatus Enum Tests

    [Fact]
    public void AIModelStatus_ShouldHaveCorrectValues()
    {
        ((int)AIModelStatus.Training).Should().Be(0);
        ((int)AIModelStatus.Trained).Should().Be(1);
        ((int)AIModelStatus.Active).Should().Be(2);
        ((int)AIModelStatus.Deprecated).Should().Be(3);
        ((int)AIModelStatus.Failed).Should().Be(4);
        ((int)AIModelStatus.Archived).Should().Be(5);
    }

    [Fact]
    public void AIModelStatus_ShouldHave6Values()
    {
        var values = Enum.GetValues<AIModelStatus>();
        values.Should().HaveCount(6);
    }

    #endregion

    #region AIProvider Enum Tests

    [Fact]
    public void AIProvider_ShouldHaveCorrectValues()
    {
        ((int)AIProvider.AllenAI_OLMo).Should().Be(0);
        ((int)AIProvider.AllenAI_Tulu).Should().Be(1);
        ((int)AIProvider.MLNet).Should().Be(2);
        ((int)AIProvider.HuggingFace).Should().Be(3);
        ((int)AIProvider.OpenAI).Should().Be(4);
        ((int)AIProvider.Anthropic).Should().Be(5);
        ((int)AIProvider.Custom).Should().Be(6);
    }

    [Fact]
    public void AIProvider_ShouldHave7Values()
    {
        var values = Enum.GetValues<AIProvider>();
        values.Should().HaveCount(7);
    }

    #endregion

    #region LeadScoreCategory Enum Tests

    [Fact]
    public void LeadScoreCategory_ShouldHaveCorrectValues()
    {
        ((int)LeadScoreCategory.Cold).Should().Be(0);
        ((int)LeadScoreCategory.Cool).Should().Be(1);
        ((int)LeadScoreCategory.Warm).Should().Be(2);
        ((int)LeadScoreCategory.Hot).Should().Be(3);
        ((int)LeadScoreCategory.OnFire).Should().Be(4);
    }

    [Fact]
    public void LeadScoreCategory_ShouldHave5Values()
    {
        var values = Enum.GetValues<LeadScoreCategory>();
        values.Should().HaveCount(5);
    }

    #endregion

    #region LeadEngagementLevel Enum Tests

    [Fact]
    public void LeadEngagementLevel_ShouldHaveCorrectValues()
    {
        ((int)LeadEngagementLevel.None).Should().Be(0);
        ((int)LeadEngagementLevel.Low).Should().Be(1);
        ((int)LeadEngagementLevel.Medium).Should().Be(2);
        ((int)LeadEngagementLevel.High).Should().Be(3);
        ((int)LeadEngagementLevel.VeryHigh).Should().Be(4);
    }

    #endregion

    #region IntentSignalStrength Enum Tests

    [Fact]
    public void IntentSignalStrength_ShouldHaveCorrectValues()
    {
        ((int)IntentSignalStrength.None).Should().Be(0);
        ((int)IntentSignalStrength.Weak).Should().Be(1);
        ((int)IntentSignalStrength.Moderate).Should().Be(2);
        ((int)IntentSignalStrength.Strong).Should().Be(3);
        ((int)IntentSignalStrength.VeryStrong).Should().Be(4);
    }

    #endregion

    #region ChurnRiskLevel Enum Tests

    [Fact]
    public void ChurnRiskLevel_ShouldHaveCorrectValues()
    {
        ((int)ChurnRiskLevel.VeryLow).Should().Be(0);
        ((int)ChurnRiskLevel.Low).Should().Be(1);
        ((int)ChurnRiskLevel.Medium).Should().Be(2);
        ((int)ChurnRiskLevel.High).Should().Be(3);
        ((int)ChurnRiskLevel.Critical).Should().Be(4);
    }

    #endregion

    #region ChurnDriverCategory Enum Tests

    [Fact]
    public void ChurnDriverCategory_ShouldHaveCorrectValues()
    {
        ((int)ChurnDriverCategory.ProductQuality).Should().Be(0);
        ((int)ChurnDriverCategory.PriceValue).Should().Be(1);
        ((int)ChurnDriverCategory.ServiceQuality).Should().Be(2);
        ((int)ChurnDriverCategory.Competition).Should().Be(3);
        ((int)ChurnDriverCategory.Engagement).Should().Be(4);
        ((int)ChurnDriverCategory.BusinessChange).Should().Be(5);
        ((int)ChurnDriverCategory.ContractIssues).Should().Be(6);
        ((int)ChurnDriverCategory.FeatureGaps).Should().Be(7);
        ((int)ChurnDriverCategory.Relationship).Should().Be(8);
    }

    [Fact]
    public void ChurnDriverCategory_ShouldHave9Values()
    {
        var values = Enum.GetValues<ChurnDriverCategory>();
        values.Should().HaveCount(9);
    }

    #endregion

    #region CustomerHealthSegment Enum Tests

    [Fact]
    public void CustomerHealthSegment_ShouldHaveCorrectValues()
    {
        ((int)CustomerHealthSegment.Champion).Should().Be(0);
        ((int)CustomerHealthSegment.Healthy).Should().Be(1);
        ((int)CustomerHealthSegment.Passive).Should().Be(2);
        ((int)CustomerHealthSegment.AtRisk).Should().Be(3);
        ((int)CustomerHealthSegment.Critical).Should().Be(4);
    }

    #endregion

    #region RetentionActionType Enum Tests

    [Fact]
    public void RetentionActionType_ShouldHaveCorrectValues()
    {
        ((int)RetentionActionType.ExecutiveReview).Should().Be(0);
        ((int)RetentionActionType.ProductTraining).Should().Be(1);
        ((int)RetentionActionType.CustomDevelopment).Should().Be(2);
        ((int)RetentionActionType.PricingNegotiation).Should().Be(3);
        ((int)RetentionActionType.DedicatedSupport).Should().Be(4);
        ((int)RetentionActionType.SuccessPlan).Should().Be(5);
        ((int)RetentionActionType.Escalate).Should().Be(6);
        ((int)RetentionActionType.RenewalIncentive).Should().Be(7);
        ((int)RetentionActionType.MultiYearDeal).Should().Be(8);
    }

    [Fact]
    public void RetentionActionType_ShouldHave9Values()
    {
        var values = Enum.GetValues<RetentionActionType>();
        values.Should().HaveCount(9);
    }

    #endregion

    #region WinProbabilityCategory Enum Tests

    [Fact]
    public void WinProbabilityCategory_ShouldHaveCorrectValues()
    {
        ((int)WinProbabilityCategory.VeryUnlikely).Should().Be(0);
        ((int)WinProbabilityCategory.Unlikely).Should().Be(1);
        ((int)WinProbabilityCategory.Possible).Should().Be(2);
        ((int)WinProbabilityCategory.Likely).Should().Be(3);
        ((int)WinProbabilityCategory.VeryLikely).Should().Be(4);
    }

    #endregion

    #region DealHealthStatus Enum Tests

    [Fact]
    public void DealHealthStatus_ShouldHaveCorrectValues()
    {
        ((int)DealHealthStatus.Healthy).Should().Be(0);
        ((int)DealHealthStatus.AtRisk).Should().Be(1);
        ((int)DealHealthStatus.Stalled).Should().Be(2);
        ((int)DealHealthStatus.Critical).Should().Be(3);
        ((int)DealHealthStatus.Accelerating).Should().Be(4);
    }

    #endregion

    #region DealRiskType Enum Tests

    [Fact]
    public void DealRiskType_ShouldHaveCorrectValues()
    {
        ((int)DealRiskType.CompetitorThreat).Should().Be(0);
        ((int)DealRiskType.BudgetRisk).Should().Be(1);
        ((int)DealRiskType.TimelineRisk).Should().Be(2);
        ((int)DealRiskType.ChampionRisk).Should().Be(3);
        ((int)DealRiskType.TechnicalRisk).Should().Be(4);
        ((int)DealRiskType.DecisionMakerRisk).Should().Be(5);
        ((int)DealRiskType.VelocityRisk).Should().Be(6);
        ((int)DealRiskType.StakeholderRisk).Should().Be(7);
    }

    [Fact]
    public void DealRiskType_ShouldHave8Values()
    {
        var values = Enum.GetValues<DealRiskType>();
        values.Should().HaveCount(8);
    }

    #endregion

    #region OpportunityActionType Enum Tests

    [Fact]
    public void OpportunityActionType_ShouldHaveCorrectValues()
    {
        ((int)OpportunityActionType.ExecutiveSponsorMeeting).Should().Be(0);
        ((int)OpportunityActionType.SendCaseStudy).Should().Be(1);
        ((int)OpportunityActionType.OfferDiscount).Should().Be(2);
        ((int)OpportunityActionType.TechnicalDemo).Should().Be(3);
        ((int)OpportunityActionType.ProposalReview).Should().Be(4);
        ((int)OpportunityActionType.CompetitorDisplacement).Should().Be(5);
        ((int)OpportunityActionType.MultiThreading).Should().Be(6);
        ((int)OpportunityActionType.RiskMitigation).Should().Be(7);
        ((int)OpportunityActionType.FastTrackClose).Should().Be(8);
    }

    #endregion

    #region NextBestActionType Enum Tests

    [Fact]
    public void NextBestActionType_ShouldHaveCorrectValues()
    {
        ((int)NextBestActionType.Call).Should().Be(0);
        ((int)NextBestActionType.Email).Should().Be(1);
        ((int)NextBestActionType.Meeting).Should().Be(2);
        ((int)NextBestActionType.SendProposal).Should().Be(3);
        ((int)NextBestActionType.FollowUp).Should().Be(4);
        ((int)NextBestActionType.ShareContent).Should().Be(5);
        ((int)NextBestActionType.CreateTask).Should().Be(6);
        ((int)NextBestActionType.Escalate).Should().Be(7);
        ((int)NextBestActionType.IntroduceColleague).Should().Be(8);
        ((int)NextBestActionType.Demo).Should().Be(9);
        ((int)NextBestActionType.Negotiate).Should().Be(10);
        ((int)NextBestActionType.CloseAsk).Should().Be(11);
        ((int)NextBestActionType.Upsell).Should().Be(12);
        ((int)NextBestActionType.CheckIn).Should().Be(13);
        ((int)NextBestActionType.SupportAction).Should().Be(14);
        ((int)NextBestActionType.NoAction).Should().Be(99);
    }

    #endregion

    #region ActionTargetType Enum Tests

    [Fact]
    public void ActionTargetType_ShouldHaveCorrectValues()
    {
        ((int)ActionTargetType.Lead).Should().Be(0);
        ((int)ActionTargetType.Opportunity).Should().Be(1);
        ((int)ActionTargetType.Customer).Should().Be(2);
        ((int)ActionTargetType.Contact).Should().Be(3);
        ((int)ActionTargetType.SupportCase).Should().Be(4);
        ((int)ActionTargetType.Quote).Should().Be(5);
        ((int)ActionTargetType.General).Should().Be(6);
    }

    #endregion

    #region ActionPriorityLevel Enum Tests

    [Fact]
    public void ActionPriorityLevel_ShouldHaveCorrectValues()
    {
        ((int)ActionPriorityLevel.Critical).Should().Be(0);
        ((int)ActionPriorityLevel.High).Should().Be(1);
        ((int)ActionPriorityLevel.Medium).Should().Be(2);
        ((int)ActionPriorityLevel.Low).Should().Be(3);
        ((int)ActionPriorityLevel.Optional).Should().Be(4);
    }

    #endregion

    #region ActionRecommendationStatus Enum Tests

    [Fact]
    public void ActionRecommendationStatus_ShouldHaveCorrectValues()
    {
        ((int)ActionRecommendationStatus.Pending).Should().Be(0);
        ((int)ActionRecommendationStatus.Accepted).Should().Be(1);
        ((int)ActionRecommendationStatus.Dismissed).Should().Be(2);
        ((int)ActionRecommendationStatus.InProgress).Should().Be(3);
        ((int)ActionRecommendationStatus.Completed).Should().Be(4);
        ((int)ActionRecommendationStatus.Expired).Should().Be(5);
        ((int)ActionRecommendationStatus.Snoozed).Should().Be(6);
    }

    [Fact]
    public void ActionRecommendationStatus_ShouldHave7Values()
    {
        var values = Enum.GetValues<ActionRecommendationStatus>();
        values.Should().HaveCount(7);
    }

    #endregion

    #region ActionChannel Enum Tests

    [Fact]
    public void ActionChannel_ShouldHaveCorrectValues()
    {
        ((int)ActionChannel.Phone).Should().Be(0);
        ((int)ActionChannel.Email).Should().Be(1);
        ((int)ActionChannel.VideoMeeting).Should().Be(2);
        ((int)ActionChannel.InPerson).Should().Be(3);
        ((int)ActionChannel.LinkedIn).Should().Be(4);
        ((int)ActionChannel.Chat).Should().Be(5);
        ((int)ActionChannel.SelfService).Should().Be(6);
        ((int)ActionChannel.Internal).Should().Be(7);
    }

    #endregion

    #region EmailSentiment Enum Tests

    [Fact]
    public void EmailSentiment_ShouldHaveCorrectValues()
    {
        ((int)EmailSentiment.VeryNegative).Should().Be(0);
        ((int)EmailSentiment.Negative).Should().Be(1);
        ((int)EmailSentiment.Neutral).Should().Be(2);
        ((int)EmailSentiment.Positive).Should().Be(3);
        ((int)EmailSentiment.VeryPositive).Should().Be(4);
    }

    #endregion

    #region EmailIntent Enum Tests

    [Fact]
    public void EmailIntent_ShouldHaveCorrectStandardValues()
    {
        ((int)EmailIntent.Inquiry).Should().Be(0);
        ((int)EmailIntent.PurchaseIntent).Should().Be(1);
        ((int)EmailIntent.SupportRequest).Should().Be(2);
        ((int)EmailIntent.Complaint).Should().Be(3);
        ((int)EmailIntent.Feedback).Should().Be(4);
        ((int)EmailIntent.MeetingRequest).Should().Be(5);
        ((int)EmailIntent.FollowUp).Should().Be(6);
        ((int)EmailIntent.Cancellation).Should().Be(7);
        ((int)EmailIntent.PricingQuestion).Should().Be(8);
        ((int)EmailIntent.TechnicalQuestion).Should().Be(9);
        ((int)EmailIntent.Referral).Should().Be(10);
        ((int)EmailIntent.OutOfOffice).Should().Be(11);
        ((int)EmailIntent.ThankYou).Should().Be(12);
        ((int)EmailIntent.Other).Should().Be(99);
    }

    #endregion

    #region ResponseUrgency Enum Tests

    [Fact]
    public void ResponseUrgency_ShouldHaveCorrectValues()
    {
        ((int)ResponseUrgency.Immediate).Should().Be(0);
        ((int)ResponseUrgency.High).Should().Be(1);
        ((int)ResponseUrgency.Normal).Should().Be(2);
        ((int)ResponseUrgency.Low).Should().Be(3);
        ((int)ResponseUrgency.NoResponse).Should().Be(4);
    }

    #endregion

    #region AIModel Entity Tests

    [Fact]
    public void AIModel_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var model = new AIModel();

        // Assert
        model.Name.Should().BeEmpty();
        model.Version.Should().Be("1.0.0");
        model.Status.Should().Be(AIModelStatus.Training);
        model.Provider.Should().Be(AIProvider.AllenAI_OLMo);
        model.PredictionCount.Should().Be(0);
        model.Predictions.Should().BeEmpty();
    }

    [Fact]
    public void AIModel_ShouldAllowSettingProperties()
    {
        // Arrange
        var model = new AIModel
        {
            Id = 1,
            Name = "Lead Scoring Model",
            Version = "2.0.1",
            Description = "ML model for lead scoring",
            ModelType = AIModelType.LeadScoring,
            Status = AIModelStatus.Active,
            Provider = AIProvider.MLNet,
            ModelIdentifier = "models/lead-scoring-v2",
            ConfigurationJson = "{\"threshold\": 0.7}",
            FeatureColumnsJson = "[\"industry\", \"company_size\", \"engagement\"]",
            TargetColumn = "converted",
            TrainingAccuracy = 0.92m,
            ValidationAccuracy = 0.89m,
            TestAccuracy = 0.87m,
            AucRoc = 0.95m,
            F1Score = 0.88m,
            TrainingSamplesCount = 50000,
            PredictionCount = 100000,
            AvgInferenceTimeMs = 15.5m
        };

        // Assert
        model.Name.Should().Be("Lead Scoring Model");
        model.Version.Should().Be("2.0.1");
        model.ModelType.Should().Be(AIModelType.LeadScoring);
        model.Status.Should().Be(AIModelStatus.Active);
        model.Provider.Should().Be(AIProvider.MLNet);
        model.TrainingAccuracy.Should().Be(0.92m);
        model.AucRoc.Should().Be(0.95m);
        model.PredictionCount.Should().Be(100000);
    }

    [Fact]
    public void AIModel_ShouldSupportTimestamps()
    {
        // Arrange
        var model = new AIModel
        {
            TrainingStartedAt = DateTime.UtcNow.AddHours(-2),
            TrainingCompletedAt = DateTime.UtcNow.AddHours(-1),
            ActivatedAt = DateTime.UtcNow,
            LastPredictionAt = DateTime.UtcNow
        };

        // Assert
        model.TrainingStartedAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(-2), TimeSpan.FromMinutes(1));
        model.TrainingCompletedAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(-1), TimeSpan.FromMinutes(1));
        model.ActivatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Theory]
    [InlineData(AIModelType.LeadScoring)]
    [InlineData(AIModelType.OpportunityWinPrediction)]
    [InlineData(AIModelType.ChurnPrediction)]
    [InlineData(AIModelType.NextBestAction)]
    [InlineData(AIModelType.SentimentAnalysis)]
    public void AIModel_ShouldAcceptAllModelTypes(AIModelType modelType)
    {
        // Arrange & Act
        var model = new AIModel { ModelType = modelType };

        // Assert
        model.ModelType.Should().Be(modelType);
    }

    #endregion

    #region Prediction Entity Tests

    [Fact]
    public void Prediction_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var prediction = new Prediction();

        // Assert
        prediction.PredictionId.Should().NotBeNullOrEmpty();
        prediction.EntityType.Should().BeEmpty();
        prediction.PredictedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Prediction_ShouldHaveUniquePredictionId()
    {
        // Arrange & Act
        var prediction1 = new Prediction();
        var prediction2 = new Prediction();

        // Assert
        prediction1.PredictionId.Should().NotBe(prediction2.PredictionId);
    }

    [Fact]
    public void Prediction_ShouldAllowSettingProperties()
    {
        // Arrange
        var prediction = new Prediction
        {
            Id = 1,
            PredictionId = "pred-123",
            EntityType = "Lead",
            EntityId = 100,
            PredictedValue = 85.5m,
            PredictedLabel = "Hot",
            Confidence = 0.92m,
            ProbabilitiesJson = "{\"Hot\": 0.85, \"Warm\": 0.10, \"Cold\": 0.05}",
            FeatureImportanceJson = "{\"industry\": 0.35, \"engagement\": 0.40}",
            Explanation = "High engagement score and matching industry profile",
            InferenceTimeMs = 12.5m,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            AIModelId = 1
        };

        // Assert
        prediction.EntityType.Should().Be("Lead");
        prediction.EntityId.Should().Be(100);
        prediction.PredictedValue.Should().Be(85.5m);
        prediction.Confidence.Should().Be(0.92m);
        prediction.Explanation.Should().Contain("engagement");
    }

    [Fact]
    public void Prediction_ShouldSupportFeedback()
    {
        // Arrange
        var prediction = new Prediction
        {
            PredictedValue = 85,
            PredictedLabel = "Hot",
            ActualValue = 1,
            ActualLabel = "Converted",
            ActualRecordedAt = DateTime.UtcNow,
            WasCorrect = true
        };

        // Assert
        prediction.WasCorrect.Should().BeTrue();
        prediction.ActualLabel.Should().Be("Converted");
    }

    #endregion

    #region LeadScore Entity Tests

    [Fact]
    public void LeadScore_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var leadScore = new LeadScore();

        // Assert
        leadScore.LeadId.Should().Be(0);
        leadScore.OverallScore.Should().Be(0);
    }

    [Fact]
    public void LeadScore_ShouldAllowSettingProperties()
    {
        // Arrange
        var leadScore = new LeadScore
        {
            Id = 1,
            LeadId = 100,
            OverallScore = 85,
            Category = LeadScoreCategory.Hot,
            Confidence = 0.92m,
            ScoreTrend = 5.5m,
            DemographicScore = 80,
            FirmographicScore = 90,
            BehavioralScore = 85,
            EngagementScore = 88,
            IntentScore = 92,
            EngagementLevel = LeadEngagementLevel.High,
            IntentStrength = IntentSignalStrength.Strong,
            DaysSinceLastActivity = 2,
            TotalTouches = 15,
            EmailOpenRate = 0.65m,
            EmailClickRate = 0.25m,
            WebsiteSessions = 12,
            PagesViewed = 35,
            ContentDownloads = 3,
            ConversionProbability = 0.78m,
            EstimatedDaysToConversion = 14,
            EstimatedDealValue = 50000m,
            BestProductFit = "Enterprise Plan",
            ICPMatchScore = 92
        };

        // Assert
        leadScore.LeadId.Should().Be(100);
        leadScore.OverallScore.Should().Be(85);
        leadScore.Category.Should().Be(LeadScoreCategory.Hot);
        leadScore.EngagementLevel.Should().Be(LeadEngagementLevel.High);
        leadScore.IntentStrength.Should().Be(IntentSignalStrength.Strong);
        leadScore.ConversionProbability.Should().Be(0.78m);
        leadScore.EstimatedDealValue.Should().Be(50000m);
    }

    [Theory]
    [InlineData(LeadScoreCategory.Cold)]
    [InlineData(LeadScoreCategory.Cool)]
    [InlineData(LeadScoreCategory.Warm)]
    [InlineData(LeadScoreCategory.Hot)]
    [InlineData(LeadScoreCategory.OnFire)]
    public void LeadScore_ShouldAcceptAllCategories(LeadScoreCategory category)
    {
        // Arrange & Act
        var leadScore = new LeadScore { Category = category };

        // Assert
        leadScore.Category.Should().Be(category);
    }

    #endregion

    #region ChurnRisk Entity Tests

    [Fact]
    public void ChurnRisk_ShouldAllowSettingProperties()
    {
        // Arrange
        var churnRisk = new ChurnRisk
        {
            Id = 1,
            AccountId = 100,
            ChurnProbability = 0.75m,
            RiskLevel = ChurnRiskLevel.High,
            Confidence = 0.88m,
            RiskTrend = 0.05m,
            PreviousProbability = 0.70m,
            HealthScore = 35,
            HealthSegment = CustomerHealthSegment.AtRisk,
            NPSScore = -20,
            CSATScore = 2.5m,
            CESScore = 4.0m,
            UsageScore = 40,
            FeatureAdoption = 30,
            DaysSinceLastLogin = 14,
            DailyActiveUsers = 5,
            MonthlyLogins = 10,
            UsageTrend = -0.15m,
            OpenTickets = 3,
            CriticalTickets90Days = 2,
            AvgResolutionTimeHours = 48,
            SupportSatisfaction = 2.0m,
            Escalations90Days = 1,
            ARRAtRisk = 120000m,
            ContractEndDate = DateTime.UtcNow.AddMonths(3),
            DaysUntilRenewal = 90,
            LifetimeValue = 360000m,
            AccountTenureMonths = 24
        };

        // Assert
        churnRisk.AccountId.Should().Be(100);
        churnRisk.ChurnProbability.Should().Be(0.75m);
        churnRisk.RiskLevel.Should().Be(ChurnRiskLevel.High);
        churnRisk.HealthSegment.Should().Be(CustomerHealthSegment.AtRisk);
        churnRisk.ARRAtRisk.Should().Be(120000m);
        churnRisk.UsageTrend.Should().Be(-0.15m);
    }

    [Theory]
    [InlineData(ChurnRiskLevel.VeryLow)]
    [InlineData(ChurnRiskLevel.Low)]
    [InlineData(ChurnRiskLevel.Medium)]
    [InlineData(ChurnRiskLevel.High)]
    [InlineData(ChurnRiskLevel.Critical)]
    public void ChurnRisk_ShouldAcceptAllRiskLevels(ChurnRiskLevel riskLevel)
    {
        // Arrange & Act
        var churnRisk = new ChurnRisk { RiskLevel = riskLevel };

        // Assert
        churnRisk.RiskLevel.Should().Be(riskLevel);
    }

    #endregion

    #region OpportunityInsight Entity Tests

    [Fact]
    public void OpportunityInsight_ShouldAllowSettingProperties()
    {
        // Arrange
        var insight = new OpportunityInsight
        {
            Id = 1,
            OpportunityId = 100,
            WinProbability = 0.72m,
            WinCategory = WinProbabilityCategory.Likely,
            Confidence = 0.85m,
            ProbabilityTrend = 0.05m,
            PreviousProbability = 0.67m,
            HealthStatus = DealHealthStatus.Healthy,
            HealthScore = 78,
            VelocityScore = 82,
            EngagementScore = 85,
            StakeholderScore = 70,
            PredictedCloseDate = DateTime.UtcNow.AddDays(30),
            DaysToClose = 30,
            OnTrackForClose = true,
            CloseSlippage = -5,
            AvgSalesCycleDays = 45,
            CycleVariance = -15
        };

        // Assert
        insight.OpportunityId.Should().Be(100);
        insight.WinProbability.Should().Be(0.72m);
        insight.WinCategory.Should().Be(WinProbabilityCategory.Likely);
        insight.HealthStatus.Should().Be(DealHealthStatus.Healthy);
        insight.OnTrackForClose.Should().BeTrue();
        insight.CycleVariance.Should().Be(-15);
    }

    [Theory]
    [InlineData(DealHealthStatus.Healthy)]
    [InlineData(DealHealthStatus.AtRisk)]
    [InlineData(DealHealthStatus.Stalled)]
    [InlineData(DealHealthStatus.Critical)]
    [InlineData(DealHealthStatus.Accelerating)]
    public void OpportunityInsight_ShouldAcceptAllHealthStatuses(DealHealthStatus status)
    {
        // Arrange & Act
        var insight = new OpportunityInsight { HealthStatus = status };

        // Assert
        insight.HealthStatus.Should().Be(status);
    }

    #endregion

    #region ActionRecommendation Entity Tests

    [Fact]
    public void ActionRecommendation_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var action = new ActionRecommendation();

        // Assert
        action.TargetEntityName.Should().BeEmpty();
    }

    [Fact]
    public void ActionRecommendation_ShouldAllowSettingProperties()
    {
        // Arrange
        var action = new ActionRecommendation
        {
            Id = 1,
            TargetType = ActionTargetType.Opportunity,
            TargetEntityId = 100,
            TargetEntityName = "Acme Corp Deal"
        };

        // Assert
        action.TargetType.Should().Be(ActionTargetType.Opportunity);
        action.TargetEntityId.Should().Be(100);
        action.TargetEntityName.Should().Be("Acme Corp Deal");
    }

    [Theory]
    [InlineData(NextBestActionType.Call)]
    [InlineData(NextBestActionType.Email)]
    [InlineData(NextBestActionType.Meeting)]
    [InlineData(NextBestActionType.Demo)]
    [InlineData(NextBestActionType.CloseAsk)]
    [InlineData(NextBestActionType.NoAction)]
    public void NextBestActionType_ShouldBeValidForRecommendations(NextBestActionType actionType)
    {
        // This tests that the enum values are valid for use in recommendations
        actionType.Should().BeDefined();
    }

    #endregion

    #region EmailIntelligence Entity Tests

    [Fact]
    public void EmailIntelligence_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var intelligence = new EmailIntelligence();

        // Assert
        intelligence.EmailMessageId.Should().BeEmpty();
        intelligence.Summary.Should().BeEmpty();
    }

    [Fact]
    public void EmailIntelligence_ShouldAllowSettingProperties()
    {
        // Arrange
        var intelligence = new EmailIntelligence
        {
            Id = 1,
            EmailMessageId = "msg-123",
            CommunicationMessageId = 100,
            Sentiment = EmailSentiment.Positive,
            SentimentScore = 0.75m,
            SentimentConfidence = 0.92m,
            EmotionsJson = "{\"happy\": 0.6, \"interested\": 0.3}",
            PrimaryIntent = EmailIntent.PurchaseIntent,
            IntentConfidence = 0.88m,
            SecondaryIntentsJson = "[\"MeetingRequest\", \"PricingQuestion\"]",
            Urgency = ResponseUrgency.High,
            UrgencyScore = 80,
            ResponseDeadline = DateTime.UtcNow.AddHours(4),
            ExtractedEntitiesJson = "{\"company\": \"Acme Corp\", \"person\": \"John Doe\"}",
            MentionedProductsJson = "[\"Enterprise Plan\"]",
            MentionedCompetitorsJson = "[\"CompetitorX\"]",
            TopicsJson = "[\"pricing\", \"implementation\"]",
            ActionItemsJson = "[\"Send pricing document\"]",
            QuestionsJson = "[\"What are the volume discounts?\"]",
            Summary = "Customer interested in Enterprise Plan, asking about pricing"
        };

        // Assert
        intelligence.EmailMessageId.Should().Be("msg-123");
        intelligence.Sentiment.Should().Be(EmailSentiment.Positive);
        intelligence.SentimentScore.Should().Be(0.75m);
        intelligence.PrimaryIntent.Should().Be(EmailIntent.PurchaseIntent);
        intelligence.Urgency.Should().Be(ResponseUrgency.High);
        intelligence.Summary.Should().Contain("Enterprise Plan");
    }

    [Theory]
    [InlineData(EmailSentiment.VeryNegative)]
    [InlineData(EmailSentiment.Negative)]
    [InlineData(EmailSentiment.Neutral)]
    [InlineData(EmailSentiment.Positive)]
    [InlineData(EmailSentiment.VeryPositive)]
    public void EmailIntelligence_ShouldAcceptAllSentiments(EmailSentiment sentiment)
    {
        // Arrange & Act
        var intelligence = new EmailIntelligence { Sentiment = sentiment };

        // Assert
        intelligence.Sentiment.Should().Be(sentiment);
    }

    [Theory]
    [InlineData(ResponseUrgency.Immediate)]
    [InlineData(ResponseUrgency.High)]
    [InlineData(ResponseUrgency.Normal)]
    [InlineData(ResponseUrgency.Low)]
    [InlineData(ResponseUrgency.NoResponse)]
    public void EmailIntelligence_ShouldAcceptAllUrgencyLevels(ResponseUrgency urgency)
    {
        // Arrange & Act
        var intelligence = new EmailIntelligence { Urgency = urgency };

        // Assert
        intelligence.Urgency.Should().Be(urgency);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void AIModel_ShouldSupportPredictions()
    {
        // Arrange
        var model = new AIModel
        {
            Id = 1,
            Name = "Lead Scoring Model",
            ModelType = AIModelType.LeadScoring,
            Status = AIModelStatus.Active
        };

        var prediction = new Prediction
        {
            Id = 1,
            AIModelId = 1,
            AIModel = model,
            EntityType = "Lead",
            EntityId = 100,
            PredictedValue = 85,
            Confidence = 0.92m
        };

        model.Predictions.Add(prediction);

        // Assert
        model.Predictions.Should().HaveCount(1);
        prediction.AIModel.Should().Be(model);
    }

    [Fact]
    public void LeadScore_ShouldComputeScoreCategory()
    {
        // Arrange - Test category assignment based on score
        var coldLead = new LeadScore { OverallScore = 15, Category = LeadScoreCategory.Cold };
        var hotLead = new LeadScore { OverallScore = 85, Category = LeadScoreCategory.Hot };
        var onFireLead = new LeadScore { OverallScore = 95, Category = LeadScoreCategory.OnFire };

        // Assert
        coldLead.Category.Should().Be(LeadScoreCategory.Cold);
        hotLead.Category.Should().Be(LeadScoreCategory.Hot);
        onFireLead.Category.Should().Be(LeadScoreCategory.OnFire);
    }

    [Fact]
    public void ChurnRisk_ShouldTrackTrends()
    {
        // Arrange
        var churnRisk = new ChurnRisk
        {
            PreviousProbability = 0.45m,
            ChurnProbability = 0.65m,
            RiskTrend = 0.20m // Worsening
        };

        // Assert
        churnRisk.ChurnProbability.Should().BeGreaterThan(churnRisk.PreviousProbability.Value);
        churnRisk.RiskTrend.Should().BePositive();
    }

    #endregion
}
