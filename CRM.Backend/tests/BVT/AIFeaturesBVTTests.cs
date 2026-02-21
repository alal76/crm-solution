// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using CRM.Core.Entities;
using CRM.Core.Entities.AI;
using CRM.Core.Entities.KnowledgeBase;
using CRM.Core.Entities.Reports;
using CRM.Core.Interfaces.AI;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.BVT;

/// <summary>
/// Build Verification Tests (BVT) for AI features, Reports, and Knowledge Base
/// These tests ensure critical AI functionality works correctly after each build
/// </summary>
public class AIFeaturesBVTTests
{
    #region BVT-AI-001 to BVT-AI-010: AI Entity Creation

    [Fact]
    public void BVT_AI_001_LeadScore_Creation()
    {
        // Arrange & Act
        var leadScore = new LeadScore
        {
            LeadId = 1,
            OverallScore = 75m,
            Category = LeadScoreCategory.Hot,
            Confidence = 0.85m
        };

        // Assert
        leadScore.Should().NotBeNull();
        leadScore.OverallScore.Should().Be(75m);
        leadScore.Category.Should().Be(LeadScoreCategory.Hot);
    }

    [Fact]
    public void BVT_AI_002_OpportunityInsight_Creation()
    {
        // Arrange & Act
        var insight = new OpportunityInsight
        {
            OpportunityId = 1,
            WinProbability = 0.8m,
            HealthScore = 85m,
            HealthStatus = DealHealthStatus.Healthy
        };

        // Assert
        insight.Should().NotBeNull();
        insight.WinProbability.Should().Be(0.8m);
        insight.HealthStatus.Should().Be(DealHealthStatus.Healthy);
    }

    [Fact]
    public void BVT_AI_003_ChurnRisk_Creation()
    {
        // Arrange & Act
        var churnRisk = new ChurnRisk
        {
            AccountId = 1,
            ChurnProbability = 0.6m,
            RiskLevel = ChurnRiskLevel.High
        };

        // Assert
        churnRisk.Should().NotBeNull();
        churnRisk.ChurnProbability.Should().Be(0.6m);
        churnRisk.RiskLevel.Should().Be(ChurnRiskLevel.High);
    }

    [Fact]
    public void BVT_AI_004_ActionRecommendation_Creation()
    {
        // Arrange & Act
        var recommendation = new ActionRecommendation
        {
            TargetType = ActionTargetType.Lead,
            TargetEntityId = 1,
            ActionType = NextBestActionType.Call,
            Priority = ActionPriorityLevel.High
        };

        // Assert
        recommendation.Should().NotBeNull();
        recommendation.ActionType.Should().Be(NextBestActionType.Call);
        recommendation.Priority.Should().Be(ActionPriorityLevel.High);
    }

    [Fact]
    public void BVT_AI_005_EmailIntelligence_Creation()
    {
        // Arrange & Act
        var emailIntel = new EmailIntelligence
        {
            EmailMessageId = "msg-123",
            Sentiment = EmailSentiment.Positive,
            PrimaryIntent = EmailIntent.PurchaseIntent
        };

        // Assert
        emailIntel.Should().NotBeNull();
        emailIntel.Sentiment.Should().Be(EmailSentiment.Positive);
        emailIntel.PrimaryIntent.Should().Be(EmailIntent.PurchaseIntent);
    }

    [Fact]
    public void BVT_AI_006_AIModel_Creation()
    {
        // Arrange & Act
        var model = new AIModel
        {
            Name = "OLMo-7B",
            Provider = AIProvider.AllenAI_OLMo,
            ModelType = AIModelType.LeadScoring,
            Status = AIModelStatus.Active
        };

        // Assert
        model.Should().NotBeNull();
        model.Provider.Should().Be(AIProvider.AllenAI_OLMo);
        model.Status.Should().Be(AIModelStatus.Active);
    }

    [Fact]
    public void BVT_AI_007_Prediction_Creation()
    {
        // Arrange & Act
        var prediction = new Prediction
        {
            AIModelId = 1,
            EntityType = "Lead",
            EntityId = 100,
            PredictedValue = 0.85m,
            Confidence = 0.9m
        };

        // Assert
        prediction.Should().NotBeNull();
        prediction.PredictedValue.Should().Be(0.85m);
        prediction.Confidence.Should().Be(0.9m);
    }

    [Fact]
    public void BVT_AI_008_AllenAI_Configuration_Defaults()
    {
        // Arrange & Act
        var config = new AllenAIConfiguration();

        // Assert
        config.OLMoEndpoint.Should().Contain("OLMo");
        config.TuluEndpoint.Should().Contain("tulu");
        config.TimeoutSeconds.Should().Be(60);
        config.EnableCaching.Should().BeTrue();
    }

    [Fact]
    public void BVT_AI_009_LeadScoreCategory_AllValues()
    {
        // Arrange & Act
        var categories = Enum.GetValues<LeadScoreCategory>();

        // Assert
        categories.Should().HaveCountGreaterOrEqualTo(5);
        categories.Should().Contain(LeadScoreCategory.OnFire);
        categories.Should().Contain(LeadScoreCategory.Cold);
    }

    [Fact]
    public void BVT_AI_010_ChurnRiskLevel_AllValues()
    {
        // Arrange & Act
        var levels = Enum.GetValues<ChurnRiskLevel>();

        // Assert
        levels.Should().HaveCountGreaterOrEqualTo(5);
        levels.Should().Contain(ChurnRiskLevel.Critical);
        levels.Should().Contain(ChurnRiskLevel.VeryLow);
    }

    #endregion

    #region BVT-RPT-001 to BVT-RPT-010: Report Entity Creation

    [Fact]
    public void BVT_RPT_001_ReportDefinition_Creation()
    {
        // Arrange & Act
        var report = new ReportDefinition
        {
            Name = "Sales Pipeline Report",
            ReportType = ReportType.FunnelChart,
            Category = "Sales",
            Status = ReportStatus.Active
        };

        // Assert
        report.Should().NotBeNull();
        report.ReportType.Should().Be(ReportType.FunnelChart);
        report.Category.Should().Be("Sales");
    }

    [Fact]
    public void BVT_RPT_002_ReportSchedule_Creation()
    {
        // Arrange & Act
        var schedule = new ReportSchedule
        {
            Name = "Weekly Report Schedule",
            ReportDefinitionId = 1,
            Frequency = ScheduleFrequency.Weekly,
            Status = ScheduleStatus.Active,
            NextRunAt = DateTime.UtcNow.AddDays(7)
        };

        // Assert
        schedule.Should().NotBeNull();
        schedule.Frequency.Should().Be(ScheduleFrequency.Weekly);
        schedule.Status.Should().Be(ScheduleStatus.Active);
    }

    [Fact]
    public void BVT_RPT_003_ReportExecution_Creation()
    {
        // Arrange & Act
        var execution = new ReportExecution
        {
            ReportDefinitionId = 1,
            Status = ReportExecutionStatus.Completed,
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            CompletedAt = DateTime.UtcNow,
            RowCount = 150
        };

        // Assert
        execution.Should().NotBeNull();
        execution.Status.Should().Be(ReportExecutionStatus.Completed);
        execution.RowCount.Should().Be(150);
    }

    [Fact]
    public void BVT_RPT_004_ReportFolder_Creation()
    {
        // Arrange & Act
        var folder = new ReportFolder
        {
            Name = "Sales Reports",
            Description = "All sales-related reports"
        };

        // Assert
        folder.Should().NotBeNull();
        folder.Name.Should().Be("Sales Reports");
    }

    [Fact]
    public void BVT_RPT_005_ReportType_AllValues()
    {
        // Arrange & Act
        var types = Enum.GetValues<ReportType>();

        // Assert
        types.Should().HaveCountGreaterOrEqualTo(8);
        types.Should().Contain(ReportType.Table);
        types.Should().Contain(ReportType.BarChart);
        types.Should().Contain(ReportType.PieChart);
    }

    [Fact]
    public void BVT_RPT_006_ReportDataSource_AllValues()
    {
        // Arrange & Act
        var dataSources = Enum.GetValues<ReportDataSource>();

        // Assert
        dataSources.Should().HaveCountGreaterOrEqualTo(6);
        dataSources.Should().Contain(ReportDataSource.Leads);
        dataSources.Should().Contain(ReportDataSource.Opportunities);
        dataSources.Should().Contain(ReportDataSource.Accounts);
    }

    [Fact]
    public void BVT_RPT_007_ScheduleFrequency_AllValues()
    {
        // Arrange & Act
        var frequencies = Enum.GetValues<ScheduleFrequency>();

        // Assert
        frequencies.Should().HaveCountGreaterOrEqualTo(6);
        frequencies.Should().Contain(ScheduleFrequency.Daily);
        frequencies.Should().Contain(ScheduleFrequency.Weekly);
        frequencies.Should().Contain(ScheduleFrequency.Monthly);
    }

    [Fact]
    public void BVT_RPT_008_ReportWidgetConfig_Creation()
    {
        // Arrange & Act
        var config = new ReportWidgetConfig
        {
            DashboardWidgetId = 1,
            ReportDefinitionId = 1,
            TimePeriod = ReportTimePeriod.ThisMonth,
            ShowLegend = true,
            ShowDataLabels = true
        };

        // Assert
        config.Should().NotBeNull();
        config.TimePeriod.Should().Be(ReportTimePeriod.ThisMonth);
    }

    [Fact]
    public void BVT_RPT_009_ReportOutputFormat_AllValues()
    {
        // Arrange & Act
        var formats = Enum.GetValues<ReportOutputFormat>();

        // Assert
        formats.Should().HaveCountGreaterOrEqualTo(4);
        formats.Should().Contain(ReportOutputFormat.PDF);
        formats.Should().Contain(ReportOutputFormat.Excel);
        formats.Should().Contain(ReportOutputFormat.CSV);
    }

    [Fact]
    public void BVT_RPT_010_ReportAccessLevel_AllValues()
    {
        // Arrange & Act
        var accessLevels = Enum.GetValues<ReportAccessLevel>();

        // Assert
        accessLevels.Should().HaveCountGreaterOrEqualTo(3);
        accessLevels.Should().Contain(ReportAccessLevel.Private);
        accessLevels.Should().Contain(ReportAccessLevel.Team);
        accessLevels.Should().Contain(ReportAccessLevel.Organization);
    }

    #endregion

    #region BVT-KB-001 to BVT-KB-010: Knowledge Base Entity Creation

    [Fact]
    public void BVT_KB_001_KnowledgeArticle_Creation()
    {
        // Arrange & Act
        var article = new KnowledgeArticle
        {
            Title = "How to Reset Password",
            Content = "Follow these steps...",
            Status = ArticleStatus.Published,
            ArticleType = ArticleType.HowTo
        };

        // Assert
        article.Should().NotBeNull();
        article.Status.Should().Be(ArticleStatus.Published);
        article.ArticleType.Should().Be(ArticleType.HowTo);
    }

    [Fact]
    public void BVT_KB_002_KnowledgeCategory_Creation()
    {
        // Arrange & Act
        var category = new KnowledgeCategory
        {
            Name = "Account Management",
            Description = "Articles about account management"
        };

        // Assert
        category.Should().NotBeNull();
        category.Name.Should().Be("Account Management");
    }

    [Fact]
    public void BVT_KB_003_ArticleFeedback_Creation()
    {
        // Arrange & Act
        var feedback = new ArticleFeedback
        {
            KnowledgeArticleId = 1,
            IsHelpful = true,
            Rating = 5
        };

        // Assert
        feedback.Should().NotBeNull();
        feedback.IsHelpful.Should().BeTrue();
        feedback.Rating.Should().Be(5);
    }

    [Fact]
    public void BVT_KB_004_ServiceRequestArticle_Creation()
    {
        // Arrange & Act
        var link = new ServiceRequestArticle
        {
            ServiceRequestId = 1,
            KnowledgeArticleId = 1,
            WasHelpful = true,
            DeflectedCase = true
        };

        // Assert
        link.Should().NotBeNull();
        link.WasHelpful.Should().BeTrue();
        link.DeflectedCase.Should().BeTrue();
    }

    [Fact]
    public void BVT_KB_005_ArticleStatus_AllValues()
    {
        // Arrange & Act
        var statuses = Enum.GetValues<ArticleStatus>();

        // Assert
        statuses.Should().HaveCountGreaterOrEqualTo(4);
        statuses.Should().Contain(ArticleStatus.Draft);
        statuses.Should().Contain(ArticleStatus.Published);
        statuses.Should().Contain(ArticleStatus.Archived);
    }

    [Fact]
    public void BVT_KB_006_ArticleType_AllValues()
    {
        // Arrange & Act
        var types = Enum.GetValues<ArticleType>();

        // Assert
        types.Should().HaveCountGreaterOrEqualTo(5);
        types.Should().Contain(ArticleType.HowTo);
        types.Should().Contain(ArticleType.FAQ);
        types.Should().Contain(ArticleType.Troubleshooting);
    }

    [Fact]
    public void BVT_KB_007_SLAPolicy_Creation()
    {
        // Arrange & Act
        var policy = new CRM.Core.Entities.KnowledgeBase.SLAPolicy
        {
            Name = "Standard SLA",
            Description = "Standard response times",
            IsActive = true
        };

        // Assert
        policy.Should().NotBeNull();
        policy.Name.Should().Be("Standard SLA");
        policy.IsActive.Should().BeTrue();
    }

    [Fact]
    public void BVT_KB_008_SLATarget_Creation()
    {
        // Arrange & Act
        var target = new SLATarget
        {
            SLAPolicyId = 1,
            MetricType = SLAMetricType.FirstResponse,
            TargetValue = 60,
            TimeUnit = SLATimeUnit.Minutes
        };

        // Assert
        target.Should().NotBeNull();
        target.MetricType.Should().Be(SLAMetricType.FirstResponse);
        target.TargetValue.Should().Be(60);
    }

    [Fact]
    public void BVT_KB_009_SLAMetricType_AllValues()
    {
        // Arrange & Act
        var metrics = Enum.GetValues<SLAMetricType>();

        // Assert
        metrics.Should().HaveCountGreaterOrEqualTo(3);
        metrics.Should().Contain(SLAMetricType.FirstResponse);
        metrics.Should().Contain(SLAMetricType.Resolution);
    }

    // TODO: BVT_KB_010_EscalationRule_Creation test disabled - CRM.Core.Entities.KnowledgeBase.EscalationRule
    // does not exist. The available EscalationRule entities (CRM.Core.Entities.EscalationRule and
    // CRM.Core.Entities.ITSM.EscalationRule) have different property sets and do not include
    // TriggerAtPercent, TriggerMetric (SLAMetricType), or EscalationType properties.
    // Re-enable this test when a KnowledgeBase-specific EscalationRule entity is implemented.
    // [Fact]
    // public void BVT_KB_010_EscalationRule_Creation()
    // {
    //     // Arrange & Act
    //     var rule = new CRM.Core.Entities.KnowledgeBase.EscalationRule
    //     {
    //         SLAPolicyId = 1,
    //         Name = "First Level Escalation",
    //         TriggerAtPercent = 100,
    //         TriggerMetric = SLAMetricType.FirstResponse,
    //         EscalationType = EscalationType.Email,
    //         IsActive = true
    //     };
    //
    //     // Assert
    //     rule.Should().NotBeNull();
    //     rule.TriggerAtPercent.Should().Be(100);
    //     rule.EscalationType.Should().Be(EscalationType.Email);
    // }

    #endregion

    #region BVT-INT-001 to BVT-INT-005: Integration Scenarios

    [Fact]
    public void BVT_INT_001_LeadToScore_Integration()
    {
        // Arrange
        var lead = new Lead
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Status = LeadLifecycleStatus.Qualified
        };

        // Act
        var score = new LeadScore
        {
            LeadId = lead.Id,
            OverallScore = 80m,
            Category = LeadScoreCategory.OnFire
        };

        // Assert
        score.LeadId.Should().Be(lead.Id);
        score.Category.Should().Be(LeadScoreCategory.OnFire);
    }

    [Fact]
    public void BVT_INT_002_OpportunityToInsight_Integration()
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
        var insight = new OpportunityInsight
        {
            OpportunityId = opportunity.Id,
            WinProbability = 0.75m,
            PredictedValue = opportunity.Amount,
            WeightedValue = opportunity.Amount * 0.75m
        };

        // Assert
        insight.OpportunityId.Should().Be(opportunity.Id);
        insight.WeightedValue.Should().Be(75000m);
    }

    [Fact]
    public void BVT_INT_003_CustomerToChurnRisk_Integration()
    {
        // Arrange
        var account = new Account
        {
            Id = 1,
            Company = "Tech Corp",
            LifecycleStage = AccountLifecycleStage.AtRisk
        };

        // Act
        var churnRisk = new ChurnRisk
        {
            AccountId = account.Id,
            ChurnProbability = 0.7m,
            RiskLevel = ChurnRiskLevel.High,
            RecommendedAction = RetentionActionType.ExecutiveReview
        };

        // Assert
        churnRisk.AccountId.Should().Be(account.Id);
        churnRisk.RiskLevel.Should().Be(ChurnRiskLevel.High);
    }

    [Fact]
    public void BVT_INT_004_ReportToSchedule_Integration()
    {
        // Arrange
        var report = new ReportDefinition
        {
            Id = 1,
            Name = "Weekly Sales",
            ReportType = ReportType.Table
        };

        // Act
        var schedule = new ReportSchedule
        {
            Name = "Weekly Schedule",
            ReportDefinitionId = report.Id,
            Frequency = ScheduleFrequency.Weekly,
            Status = ScheduleStatus.Active
        };

        // Assert
        schedule.ReportDefinitionId.Should().Be(report.Id);
        schedule.Frequency.Should().Be(ScheduleFrequency.Weekly);
    }

    [Fact]
    public void BVT_INT_005_ArticleToServiceRequest_Integration()
    {
        // Arrange
        var article = new KnowledgeArticle
        {
            Id = 1,
            Title = "Password Reset",
            Status = ArticleStatus.Published
        };

        // Act
        var link = new ServiceRequestArticle
        {
            KnowledgeArticleId = article.Id,
            ServiceRequestId = 100,
            WasHelpful = true,
            DeflectedCase = true
        };

        // Assert
        link.KnowledgeArticleId.Should().Be(article.Id);
        link.DeflectedCase.Should().BeTrue();
    }

    #endregion

    #region BVT-CFG-001 to BVT-CFG-005: Configuration Validation

    [Fact]
    public void BVT_CFG_001_AllenAI_OLMo_Endpoint_Valid()
    {
        // Arrange & Act
        var config = new AllenAIConfiguration();

        // Assert
        config.OLMoEndpoint.Should().StartWith("https://");
        config.OLMoEndpoint.Should().Contain("huggingface");
        config.OLMoEndpoint.Should().Contain("OLMo");
    }

    [Fact]
    public void BVT_CFG_002_AllenAI_Tulu_Endpoint_Valid()
    {
        // Arrange & Act
        var config = new AllenAIConfiguration();

        // Assert
        config.TuluEndpoint.Should().StartWith("https://");
        config.TuluEndpoint.Should().Contain("huggingface");
        config.TuluEndpoint.Should().Contain("tulu");
    }

    [Fact]
    public void BVT_CFG_003_Timeout_Reasonable()
    {
        // Arrange & Act
        var config = new AllenAIConfiguration();

        // Assert
        config.TimeoutSeconds.Should().BeGreaterOrEqualTo(30);
        config.TimeoutSeconds.Should().BeLessOrEqualTo(120);
    }

    [Fact]
    public void BVT_CFG_004_BatchSize_Reasonable()
    {
        // Arrange & Act
        var config = new AllenAIConfiguration();

        // Assert
        config.BatchSize.Should().BeGreaterOrEqualTo(1);
        config.BatchSize.Should().BeLessOrEqualTo(100);
    }

    [Fact]
    public void BVT_CFG_005_CacheExpiration_Reasonable()
    {
        // Arrange & Act
        var config = new AllenAIConfiguration();

        // Assert
        config.CacheExpirationMinutes.Should().BeGreaterOrEqualTo(1);
        config.CacheExpirationMinutes.Should().BeLessOrEqualTo(1440); // Max 24 hours
    }

    #endregion
}
