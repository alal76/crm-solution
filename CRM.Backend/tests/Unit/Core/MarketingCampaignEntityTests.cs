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
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Comprehensive tests for MarketingCampaign entity and related types.
/// Covers: MarketingCampaign (1390+ lines), CampaignRecipient, CampaignABTest,
/// CampaignEnums (CampaignStatus, CampaignType, CampaignObjective, etc.)
/// </summary>
public class MarketingCampaignEntityTests
{
    #region MarketingCampaign Basic Properties

    [Fact]
    public void MarketingCampaign_NewInstance_HasDefaultValues()
    {
        // Arrange & Act
        var campaign = new MarketingCampaign();

        // Assert - Basic fields
        campaign.Name.Should().Be(string.Empty);
        campaign.Description.Should().Be(string.Empty);
        campaign.Type.Should().Be(string.Empty);
        campaign.CampaignCode.Should().BeNull();

        // Assert - Status fields
        campaign.Status.Should().Be(CampaignStatus.Draft);
        campaign.Priority.Should().Be(CampaignPriority.Medium);
        campaign.ObjectiveType.Should().Be(CampaignObjective.LeadGeneration);
        campaign.CampaignType.Should().Be(CampaignType.Email);
        campaign.PrimarySuccessMetric.Should().Be(SuccessMetric.LeadsGenerated);
        campaign.AudienceType.Should().Be(AudienceType.Prospects);

        // Assert - Budget fields default to 0
        campaign.Budget.Should().Be(0);
        campaign.ActualCost.Should().Be(0);
        campaign.CurrencyCode.Should().Be("USD");

        // Assert - Target audience
        campaign.TargetAudience.Should().Be(0);

        // Assert - Boolean flags
        campaign.IsEvergreen.Should().BeFalse();
        campaign.IsABTest.Should().BeFalse();
    }

    [Fact]
    public void MarketingCampaign_InheritsFromBaseEntity()
    {
        // Arrange & Act
        var campaign = new MarketingCampaign();

        // Assert
        campaign.Should().BeAssignableTo<BaseEntity>();
        campaign.Id.Should().Be(0);
        // CreatedAt may be set by BaseEntity constructor or entity
        campaign.UpdatedAt.Should().BeNull();
        campaign.IsDeleted.Should().BeFalse();
    }

    [Theory]
    [InlineData("Winter Promo 2026", "WP2026", "Holiday promotional campaign")]
    [InlineData("Q1 Lead Gen", "Q1LG", "First quarter lead generation")]
    [InlineData("Product Launch", "PL2026", "New product launch campaign")]
    public void MarketingCampaign_CanSetBasicInfo(string name, string code, string description)
    {
        // Arrange
        var campaign = new MarketingCampaign();

        // Act
        campaign.Name = name;
        campaign.CampaignCode = code;
        campaign.Description = description;

        // Assert
        campaign.Name.Should().Be(name);
        campaign.CampaignCode.Should().Be(code);
        campaign.Description.Should().Be(description);
    }

    #endregion

    #region Campaign Timeline & Scheduling

    [Fact]
    public void MarketingCampaign_TimelineProperties_CanBeSet()
    {
        // Arrange
        var campaign = new MarketingCampaign();
        var startDate = new DateTime(2026, 3, 1);
        var endDate = new DateTime(2026, 3, 31);

        // Act
        campaign.StartDate = startDate;
        campaign.EndDate = endDate;
        campaign.ActualStartDate = startDate.AddDays(1);
        campaign.ActualEndDate = endDate.AddDays(-1);
        campaign.DurationDays = 30;
        campaign.Timezone = "America/New_York";

        // Assert
        campaign.StartDate.Should().Be(startDate);
        campaign.EndDate.Should().Be(endDate);
        campaign.ActualStartDate.Should().Be(startDate.AddDays(1));
        campaign.ActualEndDate.Should().Be(endDate.AddDays(-1));
        campaign.DurationDays.Should().Be(30);
        campaign.Timezone.Should().Be("America/New_York");
    }

    [Fact]
    public void MarketingCampaign_EvergreenCampaign_HasNoEndDate()
    {
        // Arrange
        var campaign = new MarketingCampaign();

        // Act
        campaign.IsEvergreen = true;
        campaign.StartDate = DateTime.UtcNow;
        campaign.EndDate = null;

        // Assert
        campaign.IsEvergreen.Should().BeTrue();
        campaign.EndDate.Should().BeNull();
    }

    #endregion

    #region Budget & Financials

    [Theory]
    [InlineData(10000, 5000, 50)]
    [InlineData(50000, 50000, 100)]
    [InlineData(100000, 25000, 25)]
    [InlineData(0, 0, 0)]
    public void MarketingCampaign_BudgetUtilization_CalculatesCorrectly(
        decimal budget, decimal actualCost, double expectedUtilization)
    {
        // Arrange
        var campaign = new MarketingCampaign
        {
            Budget = budget,
            ActualCost = actualCost
        };

        // Act & Assert
        campaign.BudgetUtilization.Should().BeApproximately(expectedUtilization, 0.001);
    }

    [Theory]
    [InlineData(10000, 12000, true)]
    [InlineData(10000, 10000, false)]
    [InlineData(10000, 5000, false)]
    public void MarketingCampaign_IsOverBudget_ReturnsCorrectValue(
        decimal budget, decimal actualCost, bool expectedOverBudget)
    {
        // Arrange
        var campaign = new MarketingCampaign
        {
            Budget = budget,
            ActualCost = actualCost
        };

        // Act & Assert
        campaign.IsOverBudget.Should().Be(expectedOverBudget);
    }

    [Theory]
    [InlineData(10000, 3000, 7000)]
    [InlineData(50000, 50000, 0)]
    [InlineData(100000, 120000, -20000)]
    public void MarketingCampaign_BudgetRemaining_CalculatesCorrectly(
        decimal budget, decimal actualCost, decimal expectedRemaining)
    {
        // Arrange
        var campaign = new MarketingCampaign
        {
            Budget = budget,
            ActualCost = actualCost
        };

        // Act & Assert
        campaign.BudgetRemaining.Should().Be(expectedRemaining);
    }

    [Fact]
    public void MarketingCampaign_FinancialMetrics_CanBeSet()
    {
        // Arrange
        var campaign = new MarketingCampaign();

        // Act
        campaign.DailyBudget = 500m;
        campaign.MonthlyBudget = 15000m;
        campaign.ExpectedRevenue = 100000m;
        campaign.ActualRevenue = 85000m;
        campaign.PipelineInfluenced = 500000m;
        campaign.PipelineCreated = 250000m;
        campaign.CostPerLead = 50m;
        campaign.CostPerMql = 100m;
        campaign.CostPerSql = 200m;
        campaign.CostPerOpportunity = 500m;
        campaign.CostPerAcquisition = 1000m;

        // Assert
        campaign.DailyBudget.Should().Be(500m);
        campaign.MonthlyBudget.Should().Be(15000m);
        campaign.ExpectedRevenue.Should().Be(100000m);
        campaign.ActualRevenue.Should().Be(85000m);
        campaign.PipelineInfluenced.Should().Be(500000m);
        campaign.PipelineCreated.Should().Be(250000m);
        campaign.CostPerLead.Should().Be(50m);
        campaign.CostPerMql.Should().Be(100m);
        campaign.CostPerSql.Should().Be(200m);
        campaign.CostPerOpportunity.Should().Be(500m);
        campaign.CostPerAcquisition.Should().Be(1000m);
    }

    #endregion

    #region Target Audience

    [Fact]
    public void MarketingCampaign_TargetAudienceFields_CanBeSet()
    {
        // Arrange
        var campaign = new MarketingCampaign();

        // Act
        campaign.TargetAudience = 50000;
        campaign.TargetAudienceDescription = "Tech professionals in the US";
        campaign.AudienceType = AudienceType.Prospects;
        campaign.TargetDemographics = "{\"age\": \"25-45\", \"income\": \"75k+\"}";
        campaign.TargetFirmographics = "{\"size\": \"50-500\", \"industry\": \"Technology\"}";
        campaign.TargetGeography = "United States, Canada";
        campaign.TargetIndustries = "Software, SaaS, Technology";
        campaign.TargetSegments = "Enterprise, Mid-Market";
        campaign.TargetPersonas = "IT Manager, CTO, Developer";
        campaign.TargetJobTitles = "CTO, VP Engineering, Director IT";
        campaign.TargetSeniorityLevels = "C-Level, VP, Director";
        campaign.TargetAccounts = "[123, 456, 789]";
        campaign.ExclusionCriteria = "Competitors, existing customers";
        campaign.SuppressionLists = "do-not-contact, bounced";

        // Assert
        campaign.TargetAudience.Should().Be(50000);
        campaign.TargetAudienceDescription.Should().Be("Tech professionals in the US");
        campaign.AudienceType.Should().Be(AudienceType.Prospects);
        campaign.TargetDemographics.Should().Contain("25-45");
        campaign.TargetFirmographics.Should().Contain("Technology");
        campaign.TargetGeography.Should().Contain("United States");
        campaign.TargetIndustries.Should().Contain("SaaS");
        campaign.TargetSegments.Should().Contain("Enterprise");
        campaign.TargetPersonas.Should().Contain("CTO");
        campaign.TargetJobTitles.Should().Contain("VP Engineering");
        campaign.TargetSeniorityLevels.Should().Contain("C-Level");
        campaign.TargetAccounts.Should().Contain("123");
        campaign.ExclusionCriteria.Should().Contain("Competitors");
        campaign.SuppressionLists.Should().Contain("do-not-contact");
    }

    #endregion

    #region Lead Generation Metrics

    [Fact]
    public void MarketingCampaign_LeadGenerationMetrics_DefaultToZero()
    {
        // Arrange & Act
        var campaign = new MarketingCampaign();

        // Assert
        campaign.LeadsGenerated.Should().Be(0);
        campaign.MqlsGenerated.Should().Be(0);
        campaign.SqlsGenerated.Should().Be(0);
        campaign.SalsGenerated.Should().Be(0);
        campaign.OpportunitiesCreated.Should().Be(0);
        campaign.OpportunitiesInfluenced.Should().Be(0);
        campaign.DealsWon.Should().Be(0);
        campaign.LeadToMqlRate.Should().Be(0);
        campaign.MqlToSqlRate.Should().Be(0);
        campaign.SqlToOpportunityRate.Should().Be(0);
        campaign.OpportunityToWinRate.Should().Be(0);
        campaign.ConversionRate.Should().Be(0);
        campaign.AverageLeadScore.Should().Be(0);
    }

    [Fact]
    public void MarketingCampaign_LeadGenerationMetrics_CanBeSet()
    {
        // Arrange
        var campaign = new MarketingCampaign();

        // Act
        campaign.LeadsGenerated = 500;
        campaign.MqlsGenerated = 250;
        campaign.SqlsGenerated = 100;
        campaign.SalsGenerated = 75;
        campaign.OpportunitiesCreated = 50;
        campaign.OpportunitiesInfluenced = 80;
        campaign.DealsWon = 25;
        campaign.AccountsAcquired = 20;
        campaign.LeadToMqlRate = 50.0;
        campaign.MqlToSqlRate = 40.0;
        campaign.SqlToOpportunityRate = 50.0;
        campaign.OpportunityToWinRate = 50.0;
        campaign.ConversionRate = 4.0;
        campaign.AverageLeadScore = 75.5;
        campaign.LeadQualityDistribution = "{\"A\": 20, \"B\": 35, \"C\": 30, \"D\": 15}";

        // Assert
        campaign.LeadsGenerated.Should().Be(500);
        campaign.MqlsGenerated.Should().Be(250);
        campaign.SqlsGenerated.Should().Be(100);
        campaign.SalsGenerated.Should().Be(75);
        campaign.OpportunitiesCreated.Should().Be(50);
        campaign.OpportunitiesInfluenced.Should().Be(80);
        campaign.DealsWon.Should().Be(25);
        campaign.AccountsAcquired.Should().Be(20);
        campaign.LeadToMqlRate.Should().BeApproximately(50.0, 0.01);
        campaign.MqlToSqlRate.Should().BeApproximately(40.0, 0.01);
        campaign.SqlToOpportunityRate.Should().BeApproximately(50.0, 0.01);
        campaign.OpportunityToWinRate.Should().BeApproximately(50.0, 0.01);
        campaign.ConversionRate.Should().BeApproximately(4.0, 0.01);
        campaign.AverageLeadScore.Should().BeApproximately(75.5, 0.01);
    }

    #endregion

    #region Reach & Engagement Metrics

    [Fact]
    public void MarketingCampaign_EngagementMetrics_DefaultToZero()
    {
        // Arrange & Act
        var campaign = new MarketingCampaign();

        // Assert
        campaign.Impressions.Should().Be(0);
        campaign.Reach.Should().Be(0);
        campaign.Frequency.Should().Be(0);
        campaign.Clicks.Should().Be(0);
        campaign.ClickThroughRate.Should().Be(0);
        campaign.LandingPageVisits.Should().Be(0);
        campaign.FormSubmissions.Should().Be(0);
        campaign.FormConversionRate.Should().Be(0);
        campaign.ContentDownloads.Should().Be(0);
        campaign.VideoViews.Should().Be(0);
        campaign.VideoCompletionRate.Should().Be(0);
        campaign.DemoRequests.Should().Be(0);
        campaign.TrialSignups.Should().Be(0);
    }

    [Theory]
    [InlineData(100000, 1000, 1.0)]
    [InlineData(50000, 500, 1.0)]
    [InlineData(1000000, 0, 0)]
    public void MarketingCampaign_OverallEngagementRate_CalculatesCorrectly(
        long impressions, int clicks, double expectedRate)
    {
        // Arrange
        var campaign = new MarketingCampaign
        {
            Impressions = impressions,
            Clicks = clicks
        };

        // Act & Assert
        campaign.OverallEngagementRate.Should().BeApproximately(expectedRate, 0.01);
    }

    #endregion

    #region Email Campaign Metrics

    [Fact]
    public void MarketingCampaign_EmailMetrics_DefaultToZero()
    {
        // Arrange & Act
        var campaign = new MarketingCampaign();

        // Assert
        campaign.EmailsSent.Should().Be(0);
        campaign.EmailsDelivered.Should().Be(0);
        campaign.DeliveryRate.Should().Be(0);
        campaign.EmailsOpened.Should().Be(0);
        campaign.OpenRate.Should().Be(0);
        campaign.EmailClicks.Should().Be(0);
        campaign.EmailClickRate.Should().Be(0);
        campaign.ClickToOpenRate.Should().Be(0);
        campaign.HardBounces.Should().Be(0);
        campaign.SoftBounces.Should().Be(0);
        campaign.Bounces.Should().Be(0);
        campaign.BounceRate.Should().Be(0);
        campaign.Unsubscribes.Should().Be(0);
        campaign.UnsubscribeRate.Should().Be(0);
        campaign.SpamComplaints.Should().Be(0);
        campaign.ComplaintRate.Should().Be(0);
        campaign.EmailForwards.Should().Be(0);
        campaign.ListGrowth.Should().Be(0);
    }

    [Fact]
    public void MarketingCampaign_EmailMetrics_CanBeSet()
    {
        // Arrange
        var campaign = new MarketingCampaign();

        // Act
        campaign.EmailsSent = 10000;
        campaign.EmailsDelivered = 9500;
        campaign.DeliveryRate = 95.0;
        campaign.EmailsOpened = 2850;
        campaign.OpenRate = 30.0;
        campaign.EmailClicks = 570;
        campaign.EmailClickRate = 6.0;
        campaign.ClickToOpenRate = 20.0;
        campaign.HardBounces = 50;
        campaign.SoftBounces = 100;
        campaign.Bounces = 150;
        campaign.BounceRate = 1.5;
        campaign.Unsubscribes = 20;
        campaign.UnsubscribeRate = 0.2;
        campaign.SpamComplaints = 5;
        campaign.ComplaintRate = 0.05;

        // Assert
        campaign.EmailsSent.Should().Be(10000);
        campaign.EmailsDelivered.Should().Be(9500);
        campaign.DeliveryRate.Should().BeApproximately(95.0, 0.01);
        campaign.EmailsOpened.Should().Be(2850);
        campaign.OpenRate.Should().BeApproximately(30.0, 0.01);
        campaign.EmailClicks.Should().Be(570);
        campaign.EmailClickRate.Should().BeApproximately(6.0, 0.01);
        campaign.ClickToOpenRate.Should().BeApproximately(20.0, 0.01);
        campaign.HardBounces.Should().Be(50);
        campaign.SoftBounces.Should().Be(100);
        campaign.Bounces.Should().Be(150);
        campaign.BounceRate.Should().BeApproximately(1.5, 0.01);
        campaign.Unsubscribes.Should().Be(20);
        campaign.UnsubscribeRate.Should().BeApproximately(0.2, 0.01);
    }

    #endregion

    #region Social Media Metrics

    [Fact]
    public void MarketingCampaign_SocialMetrics_DefaultToZero()
    {
        // Arrange & Act
        var campaign = new MarketingCampaign();

        // Assert
        campaign.SocialReach.Should().Be(0);
        campaign.SocialEngagement.Should().Be(0);
        campaign.SocialEngagementRate.Should().Be(0);
        campaign.SocialShares.Should().Be(0);
        campaign.SocialComments.Should().Be(0);
        campaign.SocialLikes.Should().Be(0);
        campaign.SocialSaves.Should().Be(0);
        campaign.NewFollowers.Should().Be(0);
        campaign.ProfileVisits.Should().Be(0);
        campaign.Mentions.Should().Be(0);
        campaign.SentimentScore.Should().BeNull();
    }

    [Fact]
    public void MarketingCampaign_SocialMetrics_CanBeSet()
    {
        // Arrange
        var campaign = new MarketingCampaign();

        // Act
        campaign.SocialReach = 100000;
        campaign.SocialEngagement = 5000;
        campaign.SocialEngagementRate = 5.0;
        campaign.SocialShares = 500;
        campaign.SocialComments = 200;
        campaign.SocialLikes = 4000;
        campaign.SocialSaves = 300;
        campaign.NewFollowers = 150;
        campaign.ProfileVisits = 1000;
        campaign.Mentions = 75;
        campaign.SentimentScore = 65;

        // Assert
        campaign.SocialReach.Should().Be(100000);
        campaign.SocialEngagement.Should().Be(5000);
        campaign.SocialEngagementRate.Should().BeApproximately(5.0, 0.01);
        campaign.SocialShares.Should().Be(500);
        campaign.SocialComments.Should().Be(200);
        campaign.SocialLikes.Should().Be(4000);
        campaign.SocialSaves.Should().Be(300);
        campaign.NewFollowers.Should().Be(150);
        campaign.ProfileVisits.Should().Be(1000);
        campaign.Mentions.Should().Be(75);
        campaign.SentimentScore.Should().Be(65);
    }

    #endregion

    #region Paid Advertising Metrics

    [Fact]
    public void MarketingCampaign_PaidAdMetrics_CanBeSet()
    {
        // Arrange
        var campaign = new MarketingCampaign();

        // Act
        campaign.AdSpend = 5000m;
        campaign.CostPerClick = 2.50m;
        campaign.CostPerMille = 15.00m;
        campaign.Roas = 4.5;
        campaign.QualityScore = 8.5;
        campaign.AveragePosition = 1.8;
        campaign.ImpressionShare = 65.0;
        campaign.Keywords = "[\"crm software\", \"sales automation\"]";
        campaign.NegativeKeywords = "[\"free\", \"cheap\"]";

        // Assert
        campaign.AdSpend.Should().Be(5000m);
        campaign.CostPerClick.Should().Be(2.50m);
        campaign.CostPerMille.Should().Be(15.00m);
        campaign.Roas.Should().BeApproximately(4.5, 0.01);
        campaign.QualityScore.Should().BeApproximately(8.5, 0.01);
        campaign.AveragePosition.Should().BeApproximately(1.8, 0.01);
        campaign.ImpressionShare.Should().BeApproximately(65.0, 0.01);
        campaign.Keywords.Should().Contain("crm software");
        campaign.NegativeKeywords.Should().Contain("free");
    }

    #endregion

    #region Event/Webinar Metrics

    [Fact]
    public void MarketingCampaign_EventMetrics_DefaultToZero()
    {
        // Arrange & Act
        var campaign = new MarketingCampaign();

        // Assert
        campaign.Registrations.Should().Be(0);
        campaign.Attendance.Should().Be(0);
        campaign.AttendanceRate.Should().Be(0);
        campaign.NoShows.Should().Be(0);
        campaign.OnDemandViews.Should().Be(0);
        campaign.PollResponses.Should().Be(0);
        campaign.QuestionsAsked.Should().Be(0);
    }

    [Fact]
    public void MarketingCampaign_EventMetrics_CanBeSet()
    {
        // Arrange
        var campaign = new MarketingCampaign();
        var eventDate = new DateTime(2026, 4, 15, 14, 0, 0);

        // Act
        campaign.Registrations = 500;
        campaign.Attendance = 350;
        campaign.AttendanceRate = 70.0;
        campaign.NoShows = 150;
        campaign.EventCapacity = 1000;
        campaign.EventLocation = "Virtual - Zoom";
        campaign.EventDateTime = eventDate;
        campaign.WebinarPlatform = "Zoom";
        campaign.WebinarRecordingUrl = "https://example.com/recording";
        campaign.OnDemandViews = 200;
        campaign.PollResponses = 100;
        campaign.QuestionsAsked = 25;
        campaign.EventSatisfactionScore = 4.5;

        // Assert
        campaign.Registrations.Should().Be(500);
        campaign.Attendance.Should().Be(350);
        campaign.AttendanceRate.Should().BeApproximately(70.0, 0.01);
        campaign.NoShows.Should().Be(150);
        campaign.EventCapacity.Should().Be(1000);
        campaign.EventLocation.Should().Be("Virtual - Zoom");
        campaign.EventDateTime.Should().Be(eventDate);
        campaign.WebinarPlatform.Should().Be("Zoom");
        campaign.WebinarRecordingUrl.Should().Be("https://example.com/recording");
        campaign.OnDemandViews.Should().Be(200);
        campaign.PollResponses.Should().Be(100);
        campaign.QuestionsAsked.Should().Be(25);
        campaign.EventSatisfactionScore.Should().BeApproximately(4.5, 0.01);
    }

    #endregion

    #region ROI & Performance

    [Fact]
    public void MarketingCampaign_ROIMetrics_CanBeSet()
    {
        // Arrange
        var campaign = new MarketingCampaign();

        // Act
        campaign.ROI = 350.0;
        campaign.TargetRoi = 300.0;
        campaign.TargetLeads = 500;
        campaign.TargetConversions = 50;
        campaign.GoalAchievementPercent = 115.0;
        campaign.CampaignHealthScore = 85;
        campaign.BenchmarkComparison = "Above industry average";

        // Assert
        campaign.ROI.Should().BeApproximately(350.0, 0.01);
        campaign.TargetRoi.Should().BeApproximately(300.0, 0.01);
        campaign.TargetLeads.Should().Be(500);
        campaign.TargetConversions.Should().Be(50);
        campaign.GoalAchievementPercent.Should().BeApproximately(115.0, 0.01);
        campaign.CampaignHealthScore.Should().Be(85);
        campaign.BenchmarkComparison.Should().Be("Above industry average");
    }

    #endregion

    #region Content & Creative

    [Fact]
    public void MarketingCampaign_ContentFields_CanBeSet()
    {
        // Arrange
        var campaign = new MarketingCampaign();

        // Act
        campaign.MessageSubject = "Don't Miss Our Biggest Sale!";
        campaign.PreheaderText = "Up to 50% off all products";
        campaign.MessageBody = "<html><body>Sale content here</body></html>";
        campaign.FromName = "Marketing Team";
        campaign.FromEmail = "marketing@company.com";
        campaign.ReplyToEmail = "sales@company.com";
        campaign.CallToAction = "Shop Now";
        campaign.CtaUrl = "https://example.com/sale";
        campaign.LandingPageUrl = "https://example.com/landing";
        campaign.TrackingUrl = "https://example.com/landing?utm_source=email";
        campaign.CreativeAssets = "[\"banner1.jpg\", \"banner2.jpg\"]";
        campaign.TemplateId = "template-123";

        // Assert
        campaign.MessageSubject.Should().Be("Don't Miss Our Biggest Sale!");
        campaign.PreheaderText.Should().Be("Up to 50% off all products");
        campaign.MessageBody.Should().Contain("Sale content here");
        campaign.FromName.Should().Be("Marketing Team");
        campaign.FromEmail.Should().Be("marketing@company.com");
        campaign.ReplyToEmail.Should().Be("sales@company.com");
        campaign.CallToAction.Should().Be("Shop Now");
        campaign.CtaUrl.Should().Be("https://example.com/sale");
        campaign.LandingPageUrl.Should().Be("https://example.com/landing");
        campaign.TrackingUrl.Should().Contain("utm_source=email");
        campaign.CreativeAssets.Should().Contain("banner1.jpg");
        campaign.TemplateId.Should().Be("template-123");
    }

    #endregion

    #region UTM Tracking

    [Fact]
    public void MarketingCampaign_UTMFields_CanBeSet()
    {
        // Arrange
        var campaign = new MarketingCampaign();

        // Act
        campaign.UtmSource = "newsletter";
        campaign.UtmMedium = "email";
        campaign.UtmCampaign = "winter-sale-2026";
        campaign.UtmContent = "hero-banner";
        campaign.UtmTerm = "discount";

        // Assert
        campaign.UtmSource.Should().Be("newsletter");
        campaign.UtmMedium.Should().Be("email");
        campaign.UtmCampaign.Should().Be("winter-sale-2026");
        campaign.UtmContent.Should().Be("hero-banner");
        campaign.UtmTerm.Should().Be("discount");
    }

    #endregion

    #region A/B Testing

    [Fact]
    public void MarketingCampaign_ABTestFields_CanBeSet()
    {
        // Arrange
        var campaign = new MarketingCampaign();

        // Act
        campaign.IsABTest = true;
        campaign.ABTestVariants = "[{\"id\": \"A\", \"subject\": \"Sale!\"}, {\"id\": \"B\", \"subject\": \"Discount!\"}]";
        campaign.ABTestMetric = "OpenRate";
        campaign.WinningVariant = "A";
        campaign.StatisticalSignificance = 95.5;
        campaign.ABTestResults = "{\"A\": {\"openRate\": 32.5}, \"B\": {\"openRate\": 28.0}}";

        // Assert
        campaign.IsABTest.Should().BeTrue();
        campaign.ABTestVariants.Should().Contain("Sale!");
        campaign.ABTestMetric.Should().Be("OpenRate");
        campaign.WinningVariant.Should().Be("A");
        campaign.StatisticalSignificance.Should().BeApproximately(95.5, 0.01);
        campaign.ABTestResults.Should().Contain("32.5");
    }

    #endregion

    #region Channels & Platforms

    [Fact]
    public void MarketingCampaign_ChannelFields_CanBeSet()
    {
        // Arrange
        var campaign = new MarketingCampaign();

        // Act
        campaign.Channels = "[\"email\", \"social\", \"ppc\"]";
        campaign.Platforms = "Email, LinkedIn, Google Ads";
        campaign.SocialNetworks = "[\"LinkedIn\", \"Twitter\", \"Facebook\"]";
        campaign.AdPlatforms = "Google Ads, LinkedIn Ads";
        campaign.ExternalCampaignIds = "{\"google\": \"123\", \"linkedin\": \"456\"}";

        // Assert
        campaign.Channels.Should().Contain("email");
        campaign.Platforms.Should().Contain("LinkedIn");
        campaign.SocialNetworks.Should().Contain("Twitter");
        campaign.AdPlatforms.Should().Contain("Google Ads");
        campaign.ExternalCampaignIds.Should().Contain("123");
    }

    #endregion

    #region Assignment & Ownership

    [Fact]
    public void MarketingCampaign_OwnershipFields_CanBeSet()
    {
        // Arrange
        var campaign = new MarketingCampaign();
        var approvedDate = DateTime.UtcNow;

        // Act
        campaign.OwnerId = 1;
        campaign.AssignedToUserId = 2;
        campaign.TeamMembers = "[1, 2, 3, 4]";
        campaign.ApprovedByUserId = 5;
        campaign.ApprovedDate = approvedDate;
        campaign.Department = "Marketing";
        campaign.CostCenter = "MKT-001";

        // Assert
        campaign.OwnerId.Should().Be(1);
        campaign.AssignedToUserId.Should().Be(2);
        campaign.TeamMembers.Should().Contain("3");
        campaign.ApprovedByUserId.Should().Be(5);
        campaign.ApprovedDate.Should().Be(approvedDate);
        campaign.Department.Should().Be("Marketing");
        campaign.CostCenter.Should().Be("MKT-001");
    }

    #endregion

    #region Campaign Hierarchy

    [Fact]
    public void MarketingCampaign_HierarchyFields_CanBeSet()
    {
        // Arrange
        var campaign = new MarketingCampaign();

        // Act
        campaign.ParentCampaignId = 100;
        campaign.RelatedCampaigns = "[101, 102, 103]";
        campaign.Program = "Q1 Lead Generation";
        campaign.Initiative = "Digital Transformation";
        campaign.FiscalQuarter = "Q1";
        campaign.FiscalYear = 2026;

        // Assert
        campaign.ParentCampaignId.Should().Be(100);
        campaign.RelatedCampaigns.Should().Contain("102");
        campaign.Program.Should().Be("Q1 Lead Generation");
        campaign.Initiative.Should().Be("Digital Transformation");
        campaign.FiscalQuarter.Should().Be("Q1");
        campaign.FiscalYear.Should().Be(2026);
    }

    #endregion

    #region Classification & Tags

    [Fact]
    public void MarketingCampaign_ClassificationFields_CanBeSet()
    {
        // Arrange
        var campaign = new MarketingCampaign();

        // Act
        campaign.Tags = "lead-gen, email, nurture";
        campaign.Category = "Demand Generation";
        campaign.SubCategory = "Email Nurture";
        campaign.Region = "North America";

        // Assert
        campaign.Tags.Should().Contain("lead-gen");
        campaign.Category.Should().Be("Demand Generation");
        campaign.SubCategory.Should().Be("Email Nurture");
        campaign.Region.Should().Be("North America");
    }

    #endregion

    #region Documentation & Notes

    [Fact]
    public void MarketingCampaign_DocumentationFields_CanBeSet()
    {
        // Arrange
        var campaign = new MarketingCampaign();

        // Act
        campaign.Notes = "Campaign notes visible to all";
        campaign.InternalNotes = "Internal team notes only";
        campaign.SuccessCriteria = "500 leads, 50 MQLs";
        campaign.LessonsLearned = "Subject lines with personalization performed better";
        campaign.Attachments = "[\"brief.pdf\", \"creative-assets.zip\"]";
        campaign.BriefUrl = "https://docs.example.com/brief";
        campaign.ReportUrl = "https://analytics.example.com/report";

        // Assert
        campaign.Notes.Should().Be("Campaign notes visible to all");
        campaign.InternalNotes.Should().Be("Internal team notes only");
        campaign.SuccessCriteria.Should().Contain("500 leads");
        campaign.LessonsLearned.Should().Contain("personalization");
        campaign.Attachments.Should().Contain("brief.pdf");
        campaign.BriefUrl.Should().Be("https://docs.example.com/brief");
        campaign.ReportUrl.Should().Be("https://analytics.example.com/report");
    }

    #endregion

    #region Custom Fields & Integration

    [Fact]
    public void MarketingCampaign_IntegrationFields_CanBeSet()
    {
        // Arrange
        var campaign = new MarketingCampaign();
        var syncDate = DateTime.UtcNow;

        // Act
        campaign.CustomFields = "{\"field1\": \"value1\", \"field2\": \"value2\"}";
        campaign.ExternalId = "EXT-12345";
        campaign.SyncStatus = "Synced";
        campaign.LastSyncDate = syncDate;

        // Assert
        campaign.CustomFields.Should().Contain("field1");
        campaign.ExternalId.Should().Be("EXT-12345");
        campaign.SyncStatus.Should().Be("Synced");
        campaign.LastSyncDate.Should().Be(syncDate);
    }

    #endregion

    #region Calculated Properties

    [Theory]
    [InlineData(CampaignStatus.Active, true)]
    [InlineData(CampaignStatus.Draft, false)]
    [InlineData(CampaignStatus.Completed, false)]
    [InlineData(CampaignStatus.Paused, false)]
    public void MarketingCampaign_IsActive_ReturnsCorrectValue(CampaignStatus status, bool expectedActive)
    {
        // Arrange
        var campaign = new MarketingCampaign { Status = status };

        // Act & Assert
        campaign.IsActive.Should().Be(expectedActive);
    }

    [Fact]
    public void MarketingCampaign_DaysRemaining_CalculatesCorrectly()
    {
        // Arrange
        var campaign = new MarketingCampaign
        {
            EndDate = DateTime.UtcNow.AddDays(10)
        };

        // Act & Assert
        campaign.DaysRemaining.Should().BeInRange(9, 11); // Allow for timing differences
    }

    [Fact]
    public void MarketingCampaign_DaysRemaining_NullWhenNoEndDate()
    {
        // Arrange
        var campaign = new MarketingCampaign { EndDate = null };

        // Act & Assert
        campaign.DaysRemaining.Should().BeNull();
    }

    [Fact]
    public void MarketingCampaign_IsEndingSoon_TrueWhenWithin7Days()
    {
        // Arrange
        var campaign = new MarketingCampaign
        {
            EndDate = DateTime.UtcNow.AddDays(5)
        };

        // Act & Assert
        campaign.IsEndingSoon.Should().BeTrue();
    }

    [Fact]
    public void MarketingCampaign_IsEndingSoon_FalseWhenMoreThan7Days()
    {
        // Arrange
        var campaign = new MarketingCampaign
        {
            EndDate = DateTime.UtcNow.AddDays(14)
        };

        // Act & Assert
        campaign.IsEndingSoon.Should().BeFalse();
    }

    #endregion

    #region Navigation Properties

    [Fact]
    public void MarketingCampaign_NavigationProperties_CanBeSet()
    {
        // Arrange
        var campaign = new MarketingCampaign();
        var owner = new User { Id = 1, FirstName = "John", LastName = "Doe" };
        var parentCampaign = new MarketingCampaign { Id = 100, Name = "Parent Campaign" };

        // Act
        campaign.Owner = owner;
        campaign.ParentCampaign = parentCampaign;
        campaign.GeneratedLeads = new List<Lead> { new Lead { Id = 1 } };
        campaign.Products = new List<Product> { new Product { Id = 1 } };
        campaign.Opportunities = new List<Opportunity> { new Opportunity { Id = 1 } };
        campaign.ChildCampaigns = new List<MarketingCampaign> { new MarketingCampaign { Id = 200 } };

        // Assert
        campaign.Owner.Should().Be(owner);
        campaign.ParentCampaign.Should().Be(parentCampaign);
        campaign.GeneratedLeads.Should().HaveCount(1);
        campaign.Products.Should().HaveCount(1);
        campaign.Opportunities.Should().HaveCount(1);
        campaign.ChildCampaigns.Should().HaveCount(1);
    }

    #endregion

    #region CampaignStatus Enum

    [Fact]
    public void CampaignStatus_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<CampaignStatus>().Should().HaveCount(10);

        ((int)CampaignStatus.Draft).Should().Be(0);
        ((int)CampaignStatus.Scheduled).Should().Be(1);
        ((int)CampaignStatus.Active).Should().Be(2);
        ((int)CampaignStatus.Paused).Should().Be(3);
        ((int)CampaignStatus.Completed).Should().Be(4);
        ((int)CampaignStatus.Cancelled).Should().Be(5);
        ((int)CampaignStatus.Archived).Should().Be(6);
        ((int)CampaignStatus.PendingApproval).Should().Be(7);
        ((int)CampaignStatus.Rejected).Should().Be(8);
        ((int)CampaignStatus.InReview).Should().Be(9);
    }

    [Theory]
    [InlineData(CampaignStatus.Draft, "Draft")]
    [InlineData(CampaignStatus.Active, "Active")]
    [InlineData(CampaignStatus.Completed, "Completed")]
    [InlineData(CampaignStatus.PendingApproval, "PendingApproval")]
    public void CampaignStatus_ToStringReturnsName(CampaignStatus status, string expectedName)
    {
        // Act & Assert
        status.ToString().Should().Be(expectedName);
    }

    #endregion

    #region CampaignType Enum

    [Fact]
    public void CampaignType_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<CampaignType>().Should().HaveCount(26);

        ((int)CampaignType.Email).Should().Be(0);
        ((int)CampaignType.SocialMedia).Should().Be(1);
        ((int)CampaignType.PaidSearch).Should().Be(2);
        ((int)CampaignType.DisplayAds).Should().Be(3);
        ((int)CampaignType.ContentMarketing).Should().Be(4);
        ((int)CampaignType.SEO).Should().Be(5);
        ((int)CampaignType.Event).Should().Be(6);
        ((int)CampaignType.Webinar).Should().Be(7);
        ((int)CampaignType.DirectMail).Should().Be(8);
        ((int)CampaignType.Telemarketing).Should().Be(9);
        ((int)CampaignType.ABM).Should().Be(20);
        ((int)CampaignType.Integrated).Should().Be(24);
        ((int)CampaignType.Other).Should().Be(25);
    }

    #endregion

    #region CampaignPriority Enum

    [Fact]
    public void CampaignPriority_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<CampaignPriority>().Should().HaveCount(5);

        ((int)CampaignPriority.Low).Should().Be(0);
        ((int)CampaignPriority.Medium).Should().Be(1);
        ((int)CampaignPriority.High).Should().Be(2);
        ((int)CampaignPriority.Critical).Should().Be(3);
        ((int)CampaignPriority.Strategic).Should().Be(4);
    }

    #endregion

    #region CampaignObjective Enum

    [Fact]
    public void CampaignObjective_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<CampaignObjective>().Should().HaveCount(16);

        ((int)CampaignObjective.NotSpecified).Should().Be(0);
        ((int)CampaignObjective.LeadGeneration).Should().Be(1);
        ((int)CampaignObjective.BrandAwareness).Should().Be(2);
        ((int)CampaignObjective.Sales).Should().Be(3);
        ((int)CampaignObjective.CustomerEngagement).Should().Be(4);
        ((int)CampaignObjective.CustomerRetention).Should().Be(5);
        ((int)CampaignObjective.Upsell).Should().Be(6);
        ((int)CampaignObjective.ProductEducation).Should().Be(7);
        ((int)CampaignObjective.AccountPenetration).Should().Be(15);
    }

    #endregion

    #region AudienceType Enum

    [Fact]
    public void AudienceType_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<AudienceType>().Should().HaveCount(8);

        ((int)AudienceType.Prospects).Should().Be(0);
        ((int)AudienceType.Leads).Should().Be(1);
        ((int)AudienceType.Customers).Should().Be(2);
        ((int)AudienceType.FormerCustomers).Should().Be(3);
        ((int)AudienceType.Partners).Should().Be(4);
        ((int)AudienceType.Mixed).Should().Be(5);
        ((int)AudienceType.TargetAccounts).Should().Be(6);
        ((int)AudienceType.Lookalike).Should().Be(7);
    }

    #endregion

    #region SuccessMetric Enum

    [Fact]
    public void SuccessMetric_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<SuccessMetric>().Should().HaveCount(15);

        ((int)SuccessMetric.LeadsGenerated).Should().Be(0);
        ((int)SuccessMetric.MQLs).Should().Be(1);
        ((int)SuccessMetric.SQLs).Should().Be(2);
        ((int)SuccessMetric.Opportunities).Should().Be(3);
        ((int)SuccessMetric.Revenue).Should().Be(4);
        ((int)SuccessMetric.CTR).Should().Be(5);
        ((int)SuccessMetric.ConversionRate).Should().Be(6);
        ((int)SuccessMetric.EngagementRate).Should().Be(7);
        ((int)SuccessMetric.CostPerLead).Should().Be(8);
        ((int)SuccessMetric.ROI).Should().Be(9);
        ((int)SuccessMetric.Trials).Should().Be(14);
    }

    #endregion

    #region CampaignRecipient Entity

    [Fact]
    public void CampaignRecipient_NewInstance_HasDefaultValues()
    {
        // Arrange & Act
        var recipient = new CampaignRecipient();

        // Assert
        recipient.Status.Should().Be("Pending");
        recipient.OpenCount.Should().Be(0);
        recipient.ClickCount.Should().Be(0);
        recipient.CampaignId.Should().Be(0);
        recipient.ContactId.Should().BeNull();
        recipient.AccountId.Should().BeNull();
    }

    [Fact]
    public void CampaignRecipient_InheritsFromBaseEntity()
    {
        // Arrange & Act
        var recipient = new CampaignRecipient();

        // Assert
        recipient.Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void CampaignRecipient_CanSetBasicInfo()
    {
        // Arrange
        var recipient = new CampaignRecipient();

        // Act
        recipient.CampaignId = 1;
        recipient.ContactId = 100;
        recipient.AccountId = 200;
        recipient.Email = "test@example.com";
        recipient.FirstName = "John";
        recipient.LastName = "Doe";
        recipient.Company = "Acme Corp";

        // Assert
        recipient.CampaignId.Should().Be(1);
        recipient.ContactId.Should().Be(100);
        recipient.AccountId.Should().Be(200);
        recipient.Email.Should().Be("test@example.com");
        recipient.FirstName.Should().Be("John");
        recipient.LastName.Should().Be("Doe");
        recipient.Company.Should().Be("Acme Corp");
    }

    [Fact]
    public void CampaignRecipient_CanSetTrackingTimestamps()
    {
        // Arrange
        var recipient = new CampaignRecipient();
        var now = DateTime.UtcNow;

        // Act
        recipient.SendScheduledTime = now.AddHours(1);
        recipient.SendActualTime = now.AddHours(1).AddMinutes(5);
        recipient.DeliveredAt = now.AddHours(1).AddMinutes(6);
        recipient.FirstOpenedAt = now.AddHours(2);
        recipient.LastOpenedAt = now.AddHours(3);
        recipient.FirstClickedAt = now.AddHours(2).AddMinutes(1);
        recipient.LastClickedAt = now.AddHours(3).AddMinutes(1);
        recipient.ConvertedAt = now.AddDays(1);
        recipient.UnsubscribedAt = null;

        // Assert
        recipient.SendScheduledTime.Should().Be(now.AddHours(1));
        recipient.SendActualTime.Should().Be(now.AddHours(1).AddMinutes(5));
        recipient.DeliveredAt.Should().Be(now.AddHours(1).AddMinutes(6));
        recipient.FirstOpenedAt.Should().Be(now.AddHours(2));
        recipient.LastOpenedAt.Should().Be(now.AddHours(3));
        recipient.FirstClickedAt.Should().Be(now.AddHours(2).AddMinutes(1));
        recipient.LastClickedAt.Should().Be(now.AddHours(3).AddMinutes(1));
        recipient.ConvertedAt.Should().Be(now.AddDays(1));
        recipient.UnsubscribedAt.Should().BeNull();
    }

    [Fact]
    public void CampaignRecipient_CanSetEngagementCounts()
    {
        // Arrange
        var recipient = new CampaignRecipient();

        // Act
        recipient.OpenCount = 5;
        recipient.ClickCount = 3;
        recipient.ConversionValue = 1500.00m;

        // Assert
        recipient.OpenCount.Should().Be(5);
        recipient.ClickCount.Should().Be(3);
        recipient.ConversionValue.Should().Be(1500.00m);
    }

    [Fact]
    public void CampaignRecipient_CanSetBounceInfo()
    {
        // Arrange
        var recipient = new CampaignRecipient();

        // Act
        recipient.BounceType = "Hard";
        recipient.BounceReason = "Mailbox does not exist";
        recipient.ErrorMessage = "550 User unknown";

        // Assert
        recipient.BounceType.Should().Be("Hard");
        recipient.BounceReason.Should().Be("Mailbox does not exist");
        recipient.ErrorMessage.Should().Be("550 User unknown");
    }

    [Fact]
    public void CampaignRecipient_CanSetABTestVariant()
    {
        // Arrange
        var recipient = new CampaignRecipient();

        // Act
        recipient.ABTestVariant = "B";
        recipient.PersonalizationData = "{\"product\": \"CRM Pro\", \"discount\": \"20%\"}";

        // Assert
        recipient.ABTestVariant.Should().Be("B");
        recipient.PersonalizationData.Should().Contain("CRM Pro");
    }

    #endregion

    #region CampaignRecipientStatus Enum

    [Fact]
    public void CampaignRecipientStatus_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<CampaignRecipientStatus>().Should().HaveCount(9);

        ((int)CampaignRecipientStatus.Pending).Should().Be(0);
        ((int)CampaignRecipientStatus.Sent).Should().Be(1);
        ((int)CampaignRecipientStatus.Delivered).Should().Be(2);
        ((int)CampaignRecipientStatus.Failed).Should().Be(3);
        ((int)CampaignRecipientStatus.Bounced).Should().Be(4);
        ((int)CampaignRecipientStatus.Opened).Should().Be(5);
        ((int)CampaignRecipientStatus.Clicked).Should().Be(6);
        ((int)CampaignRecipientStatus.Converted).Should().Be(7);
        ((int)CampaignRecipientStatus.Unsubscribed).Should().Be(8);
    }

    #endregion

    #region BounceType Enum

    [Fact]
    public void BounceType_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<BounceType>().Should().HaveCount(4);

        ((int)BounceType.None).Should().Be(0);
        ((int)BounceType.Hard).Should().Be(1);
        ((int)BounceType.Soft).Should().Be(2);
        ((int)BounceType.Technical).Should().Be(3);
    }

    #endregion

    #region CampaignABTest Entity

    [Fact]
    public void CampaignABTest_NewInstance_HasDefaultValues()
    {
        // Arrange & Act
        var abTest = new CampaignABTest();

        // Assert
        abTest.TestName.Should().Be(string.Empty);
        abTest.TestType.Should().Be("SubjectLine");
        abTest.TestMetric.Should().Be("OpenRate");
        abTest.Status.Should().Be("Draft");
        abTest.AutoSelectWinner.Should().BeFalse();
        abTest.CampaignId.Should().Be(0);
    }

    [Fact]
    public void CampaignABTest_InheritsFromBaseEntity()
    {
        // Arrange & Act
        var abTest = new CampaignABTest();

        // Assert
        abTest.Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void CampaignABTest_CanSetBasicInfo()
    {
        // Arrange
        var abTest = new CampaignABTest();

        // Act
        abTest.CampaignId = 1;
        abTest.TestName = "Subject Line Test Q1";
        abTest.TestType = "SubjectLine";
        abTest.TestMetric = "OpenRate";

        // Assert
        abTest.CampaignId.Should().Be(1);
        abTest.TestName.Should().Be("Subject Line Test Q1");
        abTest.TestType.Should().Be("SubjectLine");
        abTest.TestMetric.Should().Be("OpenRate");
    }

    [Fact]
    public void CampaignABTest_CanSetTestConfiguration()
    {
        // Arrange
        var abTest = new CampaignABTest();

        // Act
        abTest.TrafficSplit = "{\"A\": 50, \"B\": 50}";
        abTest.SampleSize = 1000;
        abTest.SamplePercentage = 20.0m;
        abTest.VariantConfigs = "[{\"id\": \"A\", \"subject\": \"Sale!\"}, {\"id\": \"B\", \"subject\": \"Discount!\"}]";
        abTest.WinningCriteria = "{\"metric\": \"OpenRate\", \"threshold\": 0.95}";

        // Assert
        abTest.TrafficSplit.Should().Contain("50");
        abTest.SampleSize.Should().Be(1000);
        abTest.SamplePercentage.Should().Be(20.0m);
        abTest.VariantConfigs.Should().Contain("Sale!");
        abTest.WinningCriteria.Should().Contain("threshold");
    }

    [Fact]
    public void CampaignABTest_CanSetResults()
    {
        // Arrange
        var abTest = new CampaignABTest();
        var startTime = DateTime.UtcNow;
        var completedTime = startTime.AddHours(24);
        var deployedTime = completedTime.AddMinutes(5);

        // Act
        abTest.WinnerVariant = "A";
        abTest.ConfidenceLevel = 95.5m;
        abTest.TestStartedAt = startTime;
        abTest.TestCompletedAt = completedTime;
        abTest.WinnerDeployedAt = deployedTime;
        abTest.Status = "Completed";

        // Assert
        abTest.WinnerVariant.Should().Be("A");
        abTest.ConfidenceLevel.Should().Be(95.5m);
        abTest.TestStartedAt.Should().Be(startTime);
        abTest.TestCompletedAt.Should().Be(completedTime);
        abTest.WinnerDeployedAt.Should().Be(deployedTime);
        abTest.Status.Should().Be("Completed");
    }

    [Fact]
    public void CampaignABTest_CanSetAutoWinner()
    {
        // Arrange
        var abTest = new CampaignABTest();

        // Act
        abTest.AutoSelectWinner = true;
        abTest.AutoWinnerAfterHours = 24;

        // Assert
        abTest.AutoSelectWinner.Should().BeTrue();
        abTest.AutoWinnerAfterHours.Should().Be(24);
    }

    #endregion

    #region ABTestType Enum

    [Fact]
    public void ABTestType_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<ABTestType>().Should().HaveCount(5);

        ((int)ABTestType.SubjectLine).Should().Be(0);
        ((int)ABTestType.FromName).Should().Be(1);
        ((int)ABTestType.Content).Should().Be(2);
        ((int)ABTestType.SendTime).Should().Be(3);
        ((int)ABTestType.PreviewText).Should().Be(4);
    }

    #endregion

    #region ABTestMetric Enum

    [Fact]
    public void ABTestMetric_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<ABTestMetric>().Should().HaveCount(4);

        ((int)ABTestMetric.OpenRate).Should().Be(0);
        ((int)ABTestMetric.ClickRate).Should().Be(1);
        ((int)ABTestMetric.ConversionRate).Should().Be(2);
        ((int)ABTestMetric.Revenue).Should().Be(3);
    }

    #endregion

    #region ABTestStatus Enum

    [Fact]
    public void ABTestStatus_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<ABTestStatus>().Should().HaveCount(4);

        ((int)ABTestStatus.Draft).Should().Be(0);
        ((int)ABTestStatus.Running).Should().Be(1);
        ((int)ABTestStatus.Completed).Should().Be(2);
        ((int)ABTestStatus.Cancelled).Should().Be(3);
    }

    #endregion

    #region Complete Campaign Scenarios

    [Fact]
    public void MarketingCampaign_FullEmailCampaignScenario()
    {
        // Arrange & Act - Create a complete email campaign
        var campaign = new MarketingCampaign
        {
            Id = 1,
            Name = "Q1 Product Launch",
            CampaignCode = "PL-Q1-2026",
            Description = "Launch campaign for new CRM Pro features",
            CampaignType = CampaignType.Email,
            Status = CampaignStatus.Active,
            Priority = CampaignPriority.High,
            ObjectiveType = CampaignObjective.ProductLaunch,
            PrimarySuccessMetric = SuccessMetric.LeadsGenerated,

            StartDate = new DateTime(2026, 1, 15),
            EndDate = new DateTime(2026, 2, 15),

            Budget = 50000m,
            ActualCost = 35000m,

            TargetAudience = 100000,
            AudienceType = AudienceType.Prospects,

            EmailsSent = 95000,
            EmailsDelivered = 92000,
            DeliveryRate = 96.84,
            EmailsOpened = 27600,
            OpenRate = 30.0,
            EmailClicks = 5520,
            EmailClickRate = 6.0,

            LeadsGenerated = 750,
            MqlsGenerated = 375,
            LeadToMqlRate = 50.0,

            ROI = 250.0
        };

        // Assert
        campaign.Name.Should().Be("Q1 Product Launch");
        campaign.CampaignType.Should().Be(CampaignType.Email);
        campaign.Status.Should().Be(CampaignStatus.Active);
        campaign.IsActive.Should().BeTrue();
        campaign.BudgetUtilization.Should().BeApproximately(70.0, 0.01);
        campaign.IsOverBudget.Should().BeFalse();
        campaign.BudgetRemaining.Should().Be(15000m);
    }

    [Fact]
    public void MarketingCampaign_FullWebinarCampaignScenario()
    {
        // Arrange & Act - Create a complete webinar campaign
        var campaign = new MarketingCampaign
        {
            Id = 2,
            Name = "CRM Best Practices Webinar",
            CampaignType = CampaignType.Webinar,
            Status = CampaignStatus.Completed,
            ObjectiveType = CampaignObjective.LeadGeneration,

            EventDateTime = new DateTime(2026, 2, 20, 14, 0, 0),
            EventLocation = "Zoom Webinar",
            WebinarPlatform = "Zoom",
            EventCapacity = 500,

            Registrations = 450,
            Attendance = 315,
            AttendanceRate = 70.0,
            NoShows = 135,

            PollResponses = 200,
            QuestionsAsked = 45,
            EventSatisfactionScore = 4.7,

            LeadsGenerated = 315,
            MqlsGenerated = 150,

            WebinarRecordingUrl = "https://example.com/webinar-recording",
            OnDemandViews = 500
        };

        // Assert
        campaign.CampaignType.Should().Be(CampaignType.Webinar);
        campaign.Registrations.Should().Be(450);
        campaign.Attendance.Should().Be(315);
        campaign.AttendanceRate.Should().BeApproximately(70.0, 0.01);
        campaign.EventSatisfactionScore.Should().BeApproximately(4.7, 0.01);
        campaign.OnDemandViews.Should().Be(500);
    }

    #endregion
}
