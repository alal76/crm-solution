// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Smoke Tests (BVT) for Allen AI Integration

using Xunit;
using FluentAssertions;
using CRM.Core.Entities;
using CRM.Core.Entities.AI;
using CRM.Core.Interfaces.AI;
using CRM.Infrastructure.Services.AI;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CRM.Tests.BVT;

/// <summary>
/// Smoke Tests for Allen AI Integration
/// Tests verify that AI services are properly configured and accessible
/// </summary>
public class AllenAISmokeBVTTests
{
    #region SMOKE-AI-001 to SMOKE-AI-010: Configuration Validation

    [Fact]
    public void SMOKE_AI_001_Configuration_OLMo_Endpoint_Valid()
    {
        // Arrange & Act
        var config = new AllenAIConfiguration();

        // Assert - Validate OLMo endpoint structure
        config.OLMoEndpoint.Should().NotBeNullOrEmpty();
        config.OLMoEndpoint.Should().StartWith("https://");
        config.OLMoEndpoint.Should().Contain("huggingface");
        config.OLMoEndpoint.Should().Contain("allenai");
        config.OLMoEndpoint.Should().Contain("OLMo");
    }

    [Fact]
    public void SMOKE_AI_002_Configuration_Tulu_Endpoint_Valid()
    {
        // Arrange & Act
        var config = new AllenAIConfiguration();

        // Assert - Validate Tulu endpoint structure
        config.TuluEndpoint.Should().NotBeNullOrEmpty();
        config.TuluEndpoint.Should().StartWith("https://");
        config.TuluEndpoint.Should().Contain("huggingface");
        config.TuluEndpoint.Should().Contain("allenai");
        config.TuluEndpoint.Should().Contain("tulu");
    }

    [Fact]
    public void SMOKE_AI_003_Configuration_Defaults_Reasonable()
    {
        // Arrange & Act
        var config = new AllenAIConfiguration();

        // Assert - Validate sensible defaults
        config.TimeoutSeconds.Should().BeInRange(30, 120);
        config.MaxRetries.Should().BeInRange(1, 5);
        config.BatchSize.Should().BeInRange(1, 50);
        config.CacheExpirationMinutes.Should().BeInRange(1, 1440);
    }

    [Fact]
    public void SMOKE_AI_004_Configuration_Caching_Enabled()
    {
        // Arrange & Act
        var config = new AllenAIConfiguration();

        // Assert - Caching should be enabled by default for performance
        config.EnableCaching.Should().BeTrue();
    }

    [Fact]
    public void SMOKE_AI_005_Configuration_LocalFallback_Enabled()
    {
        // Arrange & Act
        var config = new AllenAIConfiguration();

        // Assert - Local fallback ensures resilience
        config.EnableLocalFallback.Should().BeTrue();
    }

    #endregion

    #region SMOKE-AI-011 to SMOKE-AI-020: Provider Constants Validation

    [Fact]
    public void SMOKE_AI_011_Provider_AllenAI_Constant_Valid()
    {
        // Assert - Provider constant is defined correctly
        AIServiceHelper.Providers.AllenAI.Should().Be("allenai");
    }

    [Fact]
    public void SMOKE_AI_012_DefaultModel_AllenAI_Valid()
    {
        // Assert - Default model is a valid OLMo model
        AIServiceHelper.DefaultModels.AllenAI.Should().Contain("OLMo");
    }

    [Fact]
    public void SMOKE_AI_013_AIProvider_Enum_Contains_AllenAI()
    {
        // Assert - AIProvider enum includes Allen AI models
        var providers = Enum.GetValues<AIProvider>();
        providers.Should().Contain(AIProvider.AllenAI_OLMo);
        providers.Should().Contain(AIProvider.AllenAI_Tulu);
    }

    [Fact]
    public void SMOKE_AI_014_AIModelType_Enum_Valid()
    {
        // Assert - All AI model types are defined
        var types = Enum.GetValues<AIModelType>();
        types.Should().Contain(AIModelType.LeadScoring);
        types.Should().Contain(AIModelType.ChurnPrediction);
        types.Should().Contain(AIModelType.OpportunityPrediction);
    }

    [Fact]
    public void SMOKE_AI_015_LLMSettings_Contains_AllenAI()
    {
        // Arrange
        var settings = new LLMSettingsDto();

        // Assert - AllenAI should be in fallback order
        settings.FallbackOrder.Should().Contain("allenai");
    }

    #endregion

    #region SMOKE-AI-021 to SMOKE-AI-030: Entity Validation

    [Fact]
    public void SMOKE_AI_021_LeadScore_AllCategories_Valid()
    {
        // Assert - All lead score categories exist
        var categories = Enum.GetValues<LeadScoreCategory>();
        categories.Should().HaveCount(5);
        categories.Should().Contain(LeadScoreCategory.OnFire);
        categories.Should().Contain(LeadScoreCategory.Hot);
        categories.Should().Contain(LeadScoreCategory.Warm);
        categories.Should().Contain(LeadScoreCategory.Cool);
        categories.Should().Contain(LeadScoreCategory.Cold);
    }

    [Fact]
    public void SMOKE_AI_022_ChurnRisk_AllLevels_Valid()
    {
        // Assert - All churn risk levels exist
        var levels = Enum.GetValues<ChurnRiskLevel>();
        levels.Should().HaveCountGreaterOrEqualTo(5);
    }

    [Fact]
    public void SMOKE_AI_023_DealHealthStatus_AllValues_Valid()
    {
        // Assert - All deal health statuses exist
        var statuses = Enum.GetValues<DealHealthStatus>();
        statuses.Should().Contain(DealHealthStatus.Healthy);
        statuses.Should().Contain(DealHealthStatus.AtRisk);
        statuses.Should().Contain(DealHealthStatus.Critical);
    }

    [Fact]
    public void SMOKE_AI_024_NextBestActionType_AllValues_Valid()
    {
        // Assert - All action types exist
        var actions = Enum.GetValues<NextBestActionType>();
        actions.Should().Contain(NextBestActionType.Call);
        actions.Should().Contain(NextBestActionType.Email);
        actions.Should().Contain(NextBestActionType.Meeting);
    }

    [Fact]
    public void SMOKE_AI_025_EmailSentiment_AllValues_Valid()
    {
        // Assert - Email sentiment analysis categories
        var sentiments = Enum.GetValues<EmailSentiment>();
        sentiments.Should().Contain(EmailSentiment.Positive);
        sentiments.Should().Contain(EmailSentiment.Neutral);
        sentiments.Should().Contain(EmailSentiment.Negative);
    }

    #endregion

    #region SMOKE-AI-031 to SMOKE-AI-040: Model Creation Tests

    [Fact]
    public void SMOKE_AI_031_LeadScore_WithAIInsights_Creates()
    {
        // Arrange & Act
        var score = new LeadScore
        {
            LeadId = 1,
            OverallScore = 85m,
            Category = LeadScoreCategory.Hot,
            AIInsights = "High engagement detected. Strong buying signals.",
            ModelVersion = "1.0-olmo"
        };

        // Assert
        score.AIInsights.Should().NotBeNullOrEmpty();
        score.ModelVersion.Should().Contain("olmo");
    }

    [Fact]
    public void SMOKE_AI_032_OpportunityInsight_WithPrediction_Creates()
    {
        // Arrange & Act
        var insight = new OpportunityInsight
        {
            OpportunityId = 1,
            WinProbability = 0.75m,
            HealthScore = 80m,
            AIRecommendations = "Schedule follow-up meeting. Address technical concerns.",
            AIGeneratedNextSteps = "1. Demo 2. Proposal 3. Negotiation"
        };

        // Assert
        insight.WinProbability.Should().BeInRange(0m, 1m);
        insight.AIRecommendations.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SMOKE_AI_033_ChurnRisk_WithFactors_Creates()
    {
        // Arrange & Act
        var risk = new ChurnRisk
        {
            CustomerId = 1,
            ChurnProbability = 0.45m,
            RiskLevel = ChurnRiskLevel.Medium,
            TopRiskFactors = "Low engagement, No recent purchases, Support tickets increased"
        };

        // Assert
        risk.ChurnProbability.Should().BeInRange(0m, 1m);
        risk.TopRiskFactors.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SMOKE_AI_034_ActionRecommendation_Creates()
    {
        // Arrange & Act
        var action = new ActionRecommendation
        {
            TargetType = ActionTargetType.Lead,
            TargetEntityId = 100,
            ActionType = NextBestActionType.Call,
            Title = "Follow up with high-value lead",
            Reason = "Lead score increased by 20 points",
            Priority = ActionPriorityLevel.High,
            Confidence = 0.9m
        };

        // Assert
        action.Title.Should().NotBeNullOrEmpty();
        action.Confidence.Should().BeInRange(0m, 1m);
    }

    [Fact]
    public void SMOKE_AI_035_AIModel_Registration_Creates()
    {
        // Arrange & Act
        var model = new AIModel
        {
            Name = "OLMo-7B-v1",
            Provider = AIProvider.AllenAI_OLMo,
            ModelType = AIModelType.LeadScoring,
            Endpoint = "https://api-inference.huggingface.co/models/allenai/OLMo-7B",
            Status = AIModelStatus.Active,
            Version = "1.0.0"
        };

        // Assert
        model.Provider.Should().Be(AIProvider.AllenAI_OLMo);
        model.Endpoint.Should().Contain("huggingface");
    }

    #endregion

    #region SMOKE-AI-041 to SMOKE-AI-050: AIServiceHelper Tests

    [Fact]
    public void SMOKE_AI_041_GetDefaultModel_AllenAI_ReturnsOLMo()
    {
        // Arrange
        var settings = new LLMSettingsDto
        {
            AllenAI = new LLMProviderSettingsDto { DefaultModel = "OLMo-2-0325-32B-Instruct" }
        };

        // Act
        var model = AIServiceHelper.GetDefaultModelForProvider(settings, "allenai");

        // Assert
        model.Should().Contain("OLMo");
    }

    [Fact]
    public void SMOKE_AI_042_GetDefaultModel_AllenAI_FallbackDefault()
    {
        // Arrange
        var settings = new LLMSettingsDto();

        // Act
        var model = AIServiceHelper.GetDefaultModelForProvider(settings, "allenai");

        // Assert
        model.Should().Be("OLMo-2-0325-32B-Instruct");
    }

    [Fact]
    public void SMOKE_AI_043_GetProviderSettings_AllenAI_Returns()
    {
        // Arrange
        var settings = new LLMSettingsDto
        {
            AllenAI = new LLMProviderSettingsDto { IsConfigured = true }
        };

        // Act
        var providerSettings = AIServiceHelper.GetProviderSettings(settings, "allenai");

        // Assert
        providerSettings.Should().NotBeNull();
        providerSettings!.IsConfigured.Should().BeTrue();
    }

    [Fact]
    public void SMOKE_AI_044_IsProviderAvailable_AllenAI_Configured()
    {
        // Arrange
        var settings = new LLMSettingsDto
        {
            AllenAI = new LLMProviderSettingsDto 
            { 
                IsConfigured = true, 
                Enabled = true 
            }
        };

        // Act
        var available = AIServiceHelper.IsProviderAvailable(settings, "allenai");

        // Assert
        available.Should().BeTrue();
    }

    [Fact]
    public void SMOKE_AI_045_ValidTemperature_Range()
    {
        // Act & Assert
        AIServiceHelper.GetValidTemperature(0.7, 0.5).Should().Be(0.7);
        AIServiceHelper.GetValidTemperature(-1.0, 0.5).Should().Be(0.0);
        AIServiceHelper.GetValidTemperature(3.0, 0.5).Should().Be(2.0);
        AIServiceHelper.GetValidTemperature(null, 0.5).Should().Be(0.5);
    }

    [Fact]
    public void SMOKE_AI_046_ValidMaxTokens_Range()
    {
        // Act & Assert
        AIServiceHelper.GetValidMaxTokens(4096, 2048).Should().Be(4096);
        AIServiceHelper.GetValidMaxTokens(-100, 2048).Should().Be(1);
        AIServiceHelper.GetValidMaxTokens(1000000, 2048).Should().Be(128000);
        AIServiceHelper.GetValidMaxTokens(null, 2048).Should().Be(2048);
    }

    #endregion

    #region SMOKE-AI-051 to SMOKE-AI-060: Integration Readiness Tests

    [Fact]
    public void SMOKE_AI_051_HuggingFace_Endpoint_Format()
    {
        // Arrange
        var config = new AllenAIConfiguration();

        // Act & Assert - Validate the endpoint follows HuggingFace API format
        Uri.TryCreate(config.OLMoEndpoint, UriKind.Absolute, out var uri).Should().BeTrue();
        uri!.Host.Should().Be("api-inference.huggingface.co");
        uri.Scheme.Should().Be("https");
    }

    [Fact]
    public void SMOKE_AI_052_ModelPath_Contains_AllenAI_Org()
    {
        // Arrange
        var config = new AllenAIConfiguration();

        // Assert - Model paths should reference allenai organization
        config.OLMoEndpoint.Should().Contain("allenai");
        config.TuluEndpoint.Should().Contain("allenai");
    }

    [Fact]
    public void SMOKE_AI_053_Prediction_Entity_Creates()
    {
        // Arrange & Act
        var prediction = new Prediction
        {
            AIModelId = 1,
            EntityType = "Lead",
            EntityId = 100,
            PredictedValue = 0.85m,
            Confidence = 0.92m,
            ModelVersion = "OLMo-7B-v1",
            ResponseTimeMs = 250
        };

        // Assert
        prediction.PredictedValue.Should().BeInRange(0m, 1m);
        prediction.ResponseTimeMs.Should().BePositive();
    }

    [Fact]
    public void SMOKE_AI_054_AIModelStatus_Transitions()
    {
        // Arrange
        var model = new AIModel { Status = AIModelStatus.Draft };

        // Act & Assert - Valid status transitions
        model.Status = AIModelStatus.Training;
        model.Status.Should().Be(AIModelStatus.Training);
        
        model.Status = AIModelStatus.Active;
        model.Status.Should().Be(AIModelStatus.Active);
    }

    [Fact]
    public void SMOKE_AI_055_FallbackOrder_Includes_AllenAI()
    {
        // Arrange
        var settings = new LLMSettingsDto();

        // Assert - AllenAI should be in fallback chain
        settings.FallbackOrder.Should().Contain("allenai");
        var index = settings.FallbackOrder.IndexOf("allenai");
        index.Should().BeGreaterThan(-1);
    }

    #endregion

    #region SMOKE-AI-061 to SMOKE-AI-070: Service Interface Validation

    [Fact]
    public void SMOKE_AI_061_IAllenAIService_Interface_Exists()
    {
        // Assert - Interface type should be accessible
        var interfaceType = typeof(IAllenAIService);
        interfaceType.Should().NotBeNull();
        interfaceType.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void SMOKE_AI_062_IAllenAIService_HasLeadScoring()
    {
        // Assert - Lead scoring methods exist
        var methods = typeof(IAllenAIService).GetMethods();
        methods.Should().Contain(m => m.Name == "ScoreLeadAsync");
        methods.Should().Contain(m => m.Name == "BatchScoreLeadsAsync");
        methods.Should().Contain(m => m.Name == "GetTopLeadsAsync");
    }

    [Fact]
    public void SMOKE_AI_063_IAllenAIService_HasOpportunityInsights()
    {
        // Assert - Opportunity insight methods exist
        var methods = typeof(IAllenAIService).GetMethods();
        methods.Should().Contain(m => m.Name == "GenerateOpportunityInsightAsync");
        methods.Should().Contain(m => m.Name == "PredictWinProbabilityAsync");
    }

    [Fact]
    public void SMOKE_AI_064_IAllenAIService_HasChurnPrediction()
    {
        // Assert - Churn prediction methods exist
        var methods = typeof(IAllenAIService).GetMethods();
        methods.Should().Contain(m => m.Name == "CalculateChurnRiskAsync");
        methods.Should().Contain(m => m.Name == "GetHighChurnRiskCustomersAsync");
    }

    [Fact]
    public void SMOKE_AI_065_IAllenAIService_HasNextBestAction()
    {
        // Assert - Next best action methods exist
        var methods = typeof(IAllenAIService).GetMethods();
        methods.Should().Contain(m => m.Name == "GetRecommendedActionsAsync");
    }

    #endregion
}
