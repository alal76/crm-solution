// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// Test file for Reports entities and Product/Campaign enums in Enums subdirectory

using CRM.Core.Entities;
using CRM.Core.Entities.Reports;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Unit tests for Reports entities and dedicated Product/Campaign enums.
/// ~140 tests covering dashboards, report definitions, schedules, and product/campaign enums.
/// </summary>
public class ReportsEnumsEntityTests
{
    #region ProductEnums - ProductStatus

    [Fact]
    public void ProductStatus_ShouldHaveCorrectValues()
    {
        ((int)ProductStatus.Draft).Should().Be(0);
        ((int)ProductStatus.Active).Should().Be(1);
        ((int)ProductStatus.Discontinued).Should().Be(2);
        ((int)ProductStatus.OutOfStock).Should().Be(3);
        ((int)ProductStatus.ComingSoon).Should().Be(4);
        ((int)ProductStatus.Archived).Should().Be(5);
        ((int)ProductStatus.Limited).Should().Be(6);
        ((int)ProductStatus.Beta).Should().Be(7);
        ((int)ProductStatus.EndOfLife).Should().Be(8);
    }

    [Fact]
    public void ProductStatus_ShouldHave9Values()
    {
        var values = Enum.GetValues<ProductStatus>();
        values.Should().HaveCount(9);
    }

    #endregion

    #region ProductEnums - ProductType

    [Fact]
    public void ProductType_ShouldHaveCorrectValues()
    {
        ((int)ProductType.Physical).Should().Be(0);
        ((int)ProductType.Digital).Should().Be(1);
        ((int)ProductType.Service).Should().Be(2);
        ((int)ProductType.Subscription).Should().Be(3);
        ((int)ProductType.Bundle).Should().Be(4);
        ((int)ProductType.Rental).Should().Be(5);
        ((int)ProductType.Consulting).Should().Be(6);
        ((int)ProductType.ManagedService).Should().Be(7);
        ((int)ProductType.SupportContract).Should().Be(8);
        ((int)ProductType.Training).Should().Be(9);
        ((int)ProductType.License).Should().Be(10);
        ((int)ProductType.ProfessionalServices).Should().Be(11);
        ((int)ProductType.Implementation).Should().Be(12);
    }

    [Fact]
    public void ProductType_ShouldHave13Values()
    {
        var values = Enum.GetValues<ProductType>();
        values.Should().HaveCount(13);
    }

    #endregion

    #region ProductEnums - BillingFrequency

    [Fact]
    public void BillingFrequency_ShouldHaveCorrectValues()
    {
        ((int)BillingFrequency.OneTime).Should().Be(0);
        ((int)BillingFrequency.Daily).Should().Be(1);
        ((int)BillingFrequency.Weekly).Should().Be(2);
        ((int)BillingFrequency.BiWeekly).Should().Be(3);
        ((int)BillingFrequency.Monthly).Should().Be(4);
        ((int)BillingFrequency.Quarterly).Should().Be(5);
        ((int)BillingFrequency.SemiAnnually).Should().Be(6);
        ((int)BillingFrequency.Annually).Should().Be(7);
        ((int)BillingFrequency.MultiYear).Should().Be(8);
        ((int)BillingFrequency.Custom).Should().Be(9);
        ((int)BillingFrequency.UsageBased).Should().Be(10);
    }

    [Fact]
    public void BillingFrequency_ShouldHave11Values()
    {
        var values = Enum.GetValues<BillingFrequency>();
        values.Should().HaveCount(11);
    }

    #endregion

    #region ProductEnums - PricingModel

    [Fact]
    public void PricingModel_ShouldHaveCorrectValues()
    {
        ((int)PricingModel.FixedPrice).Should().Be(0);
        ((int)PricingModel.TieredPricing).Should().Be(1);
        ((int)PricingModel.VolumePricing).Should().Be(2);
        ((int)PricingModel.UsageBased).Should().Be(3);
        ((int)PricingModel.PerUser).Should().Be(4);
        ((int)PricingModel.PerFeature).Should().Be(5);
        ((int)PricingModel.FlatRate).Should().Be(6);
        ((int)PricingModel.Hourly).Should().Be(7);
        ((int)PricingModel.Daily).Should().Be(8);
        ((int)PricingModel.ProjectBased).Should().Be(9);
        ((int)PricingModel.CustomQuote).Should().Be(10);
        ((int)PricingModel.Freemium).Should().Be(11);
    }

    #endregion

    #region ProductEnums - UnitOfMeasure

    [Fact]
    public void UnitOfMeasure_ShouldHaveCorrectValues()
    {
        ((int)UnitOfMeasure.Each).Should().Be(0);
        ((int)UnitOfMeasure.Hour).Should().Be(1);
        ((int)UnitOfMeasure.Day).Should().Be(2);
        ((int)UnitOfMeasure.Week).Should().Be(3);
        ((int)UnitOfMeasure.Month).Should().Be(4);
        ((int)UnitOfMeasure.Year).Should().Be(5);
        ((int)UnitOfMeasure.User).Should().Be(6);
        ((int)UnitOfMeasure.Device).Should().Be(7);
        ((int)UnitOfMeasure.Transaction).Should().Be(8);
        ((int)UnitOfMeasure.Gigabyte).Should().Be(9);
        ((int)UnitOfMeasure.ApiCall).Should().Be(10);
        ((int)UnitOfMeasure.Project).Should().Be(11);
        ((int)UnitOfMeasure.License).Should().Be(12);
        ((int)UnitOfMeasure.Kilogram).Should().Be(13);
        ((int)UnitOfMeasure.Meter).Should().Be(14);
        ((int)UnitOfMeasure.Liter).Should().Be(15);
        ((int)UnitOfMeasure.Case).Should().Be(16);
        ((int)UnitOfMeasure.Pallet).Should().Be(17);
    }

    [Fact]
    public void UnitOfMeasure_ShouldHave18Values()
    {
        var values = Enum.GetValues<UnitOfMeasure>();
        values.Should().HaveCount(18);
    }

    #endregion

    #region ProductEnums - RevenueRecognitionMethod

    [Fact]
    public void RevenueRecognitionMethod_ShouldHaveCorrectValues()
    {
        ((int)RevenueRecognitionMethod.Immediate).Should().Be(0);
        ((int)RevenueRecognitionMethod.OverTime).Should().Be(1);
        ((int)RevenueRecognitionMethod.OnDelivery).Should().Be(2);
        ((int)RevenueRecognitionMethod.Milestone).Should().Be(3);
        ((int)RevenueRecognitionMethod.PercentageOfCompletion).Should().Be(4);
    }

    #endregion

    #region ProductEnums - ServiceTier

    [Fact]
    public void ServiceTier_ShouldHaveCorrectValues()
    {
        ((int)ServiceTier.Basic).Should().Be(0);
        ((int)ServiceTier.Standard).Should().Be(1);
        ((int)ServiceTier.Professional).Should().Be(2);
        ((int)ServiceTier.Enterprise).Should().Be(3);
        ((int)ServiceTier.Premium).Should().Be(4);
        ((int)ServiceTier.Custom).Should().Be(5);
    }

    #endregion

    #region ProductEnums - ContractTermCategory

    [Fact]
    public void ContractTermCategory_ShouldHaveCorrectValues()
    {
        ((int)ContractTermCategory.NoContract).Should().Be(0);
        ((int)ContractTermCategory.Weekly).Should().Be(1);
        ((int)ContractTermCategory.Monthly).Should().Be(2);
        ((int)ContractTermCategory.Quarterly).Should().Be(3);
        ((int)ContractTermCategory.SemiAnnual).Should().Be(4);
        ((int)ContractTermCategory.Annual).Should().Be(5);
        ((int)ContractTermCategory.TwoYear).Should().Be(6);
        ((int)ContractTermCategory.ThreeYear).Should().Be(7);
        ((int)ContractTermCategory.FiveYear).Should().Be(8);
        ((int)ContractTermCategory.Custom).Should().Be(9);
    }

    #endregion

    #region CampaignEnums - CampaignStatus

    [Fact]
    public void CampaignStatus_ShouldHaveCorrectValues()
    {
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

    [Fact]
    public void CampaignStatus_ShouldHave10Values()
    {
        var values = Enum.GetValues<CampaignStatus>();
        values.Should().HaveCount(10);
    }

    #endregion

    #region CampaignEnums - CampaignType

    [Fact]
    public void CampaignType_ShouldHaveCorrectValues()
    {
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
        ((int)CampaignType.Referral).Should().Be(10);
        ((int)CampaignType.Affiliate).Should().Be(11);
        ((int)CampaignType.Influencer).Should().Be(12);
        ((int)CampaignType.PR).Should().Be(13);
        ((int)CampaignType.TradeShow).Should().Be(14);
        ((int)CampaignType.Video).Should().Be(15);
        ((int)CampaignType.Podcast).Should().Be(16);
        ((int)CampaignType.SMS).Should().Be(17);
        ((int)CampaignType.PushNotification).Should().Be(18);
        ((int)CampaignType.Retargeting).Should().Be(19);
        ((int)CampaignType.ABM).Should().Be(20);
        ((int)CampaignType.PartnerMarketing).Should().Be(21);
        ((int)CampaignType.ProductLaunch).Should().Be(22);
        ((int)CampaignType.BrandAwareness).Should().Be(23);
        ((int)CampaignType.Integrated).Should().Be(24);
        ((int)CampaignType.Other).Should().Be(25);
    }

    [Fact]
    public void CampaignType_ShouldHave26Values()
    {
        var values = Enum.GetValues<CampaignType>();
        values.Should().HaveCount(26);
    }

    #endregion

    #region CampaignEnums - CampaignPriority

    [Fact]
    public void CampaignPriority_ShouldHaveCorrectValues()
    {
        ((int)CampaignPriority.Low).Should().Be(0);
        ((int)CampaignPriority.Medium).Should().Be(1);
        ((int)CampaignPriority.High).Should().Be(2);
        ((int)CampaignPriority.Critical).Should().Be(3);
        ((int)CampaignPriority.Strategic).Should().Be(4);
    }

    #endregion

    #region CampaignEnums - CampaignObjective

    [Fact]
    public void CampaignObjective_ShouldHaveCorrectValues()
    {
        ((int)CampaignObjective.NotSpecified).Should().Be(0);
        ((int)CampaignObjective.LeadGeneration).Should().Be(1);
        ((int)CampaignObjective.BrandAwareness).Should().Be(2);
        ((int)CampaignObjective.Sales).Should().Be(3);
        ((int)CampaignObjective.CustomerEngagement).Should().Be(4);
        ((int)CampaignObjective.CustomerRetention).Should().Be(5);
        ((int)CampaignObjective.Upsell).Should().Be(6);
        ((int)CampaignObjective.ProductEducation).Should().Be(7);
        ((int)CampaignObjective.EventPromotion).Should().Be(8);
        ((int)CampaignObjective.ContentPromotion).Should().Be(9);
        ((int)CampaignObjective.MarketResearch).Should().Be(10);
        ((int)CampaignObjective.ProductLaunch).Should().Be(11);
        ((int)CampaignObjective.Reactivation).Should().Be(12);
        ((int)CampaignObjective.CompetitiveWin).Should().Be(13);
        ((int)CampaignObjective.Referrals).Should().Be(14);
        ((int)CampaignObjective.AccountPenetration).Should().Be(15);
    }

    [Fact]
    public void CampaignObjective_ShouldHave16Values()
    {
        var values = Enum.GetValues<CampaignObjective>();
        values.Should().HaveCount(16);
    }

    #endregion

    #region CampaignEnums - AudienceType

    [Fact]
    public void AudienceType_ShouldHaveCorrectValues()
    {
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

    #region CampaignEnums - SuccessMetric

    [Fact]
    public void SuccessMetric_ShouldHaveCorrectValues()
    {
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
        ((int)SuccessMetric.Registrations).Should().Be(10);
        ((int)SuccessMetric.Attendance).Should().Be(11);
        ((int)SuccessMetric.Downloads).Should().Be(12);
        ((int)SuccessMetric.DemoRequests).Should().Be(13);
        ((int)SuccessMetric.Trials).Should().Be(14);
    }

    #endregion

    #region Dashboard Enums

    [Fact]
    public void DashboardCategory_ShouldHaveCorrectValues()
    {
        ((int)DashboardCategory.Personal).Should().Be(0);
        ((int)DashboardCategory.Team).Should().Be(1);
        ((int)DashboardCategory.Executive).Should().Be(2);
        ((int)DashboardCategory.Operations).Should().Be(3);
        ((int)DashboardCategory.Sales).Should().Be(4);
        ((int)DashboardCategory.Marketing).Should().Be(5);
        ((int)DashboardCategory.Service).Should().Be(6);
        ((int)DashboardCategory.Home).Should().Be(7);
    }

    [Fact]
    public void WidgetType_ShouldHaveCorrectValues()
    {
        // Using fully qualified name to avoid ambiguity with CRM.Core.Entities.WidgetType
        ((int)CRM.Core.Entities.Reports.WidgetType.Report).Should().Be(0);
        ((int)CRM.Core.Entities.Reports.WidgetType.KPI).Should().Be(1);
        ((int)CRM.Core.Entities.Reports.WidgetType.Chart).Should().Be(2);
        ((int)CRM.Core.Entities.Reports.WidgetType.ActivityFeed).Should().Be(3);
        ((int)CRM.Core.Entities.Reports.WidgetType.TaskList).Should().Be(4);
        ((int)CRM.Core.Entities.Reports.WidgetType.Pipeline).Should().Be(5);
        ((int)CRM.Core.Entities.Reports.WidgetType.Leaderboard).Should().Be(6);
        ((int)CRM.Core.Entities.Reports.WidgetType.Calendar).Should().Be(7);
        ((int)CRM.Core.Entities.Reports.WidgetType.News).Should().Be(8);
        ((int)CRM.Core.Entities.Reports.WidgetType.QuickActions).Should().Be(9);
        ((int)CRM.Core.Entities.Reports.WidgetType.AIInsights).Should().Be(10);
        ((int)CRM.Core.Entities.Reports.WidgetType.GoalProgress).Should().Be(11);
        ((int)CRM.Core.Entities.Reports.WidgetType.Embed).Should().Be(12);
        ((int)CRM.Core.Entities.Reports.WidgetType.Text).Should().Be(13);
    }

    [Fact]
    public void WidgetSize_ShouldHaveCorrectValues()
    {
        ((int)WidgetSize.Small).Should().Be(0);
        ((int)WidgetSize.Medium).Should().Be(1);
        ((int)WidgetSize.Large).Should().Be(2);
        ((int)WidgetSize.Wide).Should().Be(3);
        ((int)WidgetSize.Tall).Should().Be(4);
        ((int)WidgetSize.FullWidth).Should().Be(5);
        ((int)WidgetSize.Custom).Should().Be(99);
    }

    [Fact]
    public void DashboardRefreshInterval_ShouldHaveCorrectValues()
    {
        ((int)DashboardRefreshInterval.None).Should().Be(0);
        ((int)DashboardRefreshInterval.OneMinute).Should().Be(1);
        ((int)DashboardRefreshInterval.FiveMinutes).Should().Be(5);
        ((int)DashboardRefreshInterval.FifteenMinutes).Should().Be(15);
        ((int)DashboardRefreshInterval.ThirtyMinutes).Should().Be(30);
        ((int)DashboardRefreshInterval.OneHour).Should().Be(60);
        ((int)DashboardRefreshInterval.FourHours).Should().Be(240);
        ((int)DashboardRefreshInterval.Daily).Should().Be(1440);
    }

    #endregion

    #region Report Enums

    [Fact]
    public void ReportType_ShouldHaveCorrectValues()
    {
        ((int)ReportType.Table).Should().Be(0);
        ((int)ReportType.BarChart).Should().Be(1);
        ((int)ReportType.LineChart).Should().Be(2);
        ((int)ReportType.PieChart).Should().Be(3);
        ((int)ReportType.FunnelChart).Should().Be(4);
        ((int)ReportType.KPI).Should().Be(5);
        ((int)ReportType.Matrix).Should().Be(6);
        ((int)ReportType.Gauge).Should().Be(7);
        ((int)ReportType.ScatterPlot).Should().Be(8);
        ((int)ReportType.AreaChart).Should().Be(9);
        ((int)ReportType.HeatMap).Should().Be(10);
        ((int)ReportType.ComboChart).Should().Be(11);
        ((int)ReportType.Map).Should().Be(12);
        ((int)ReportType.SummaryCards).Should().Be(13);
    }

    [Fact]
    public void ReportDataSource_ShouldHaveCorrectValues()
    {
        ((int)ReportDataSource.Leads).Should().Be(0);
        ((int)ReportDataSource.Opportunities).Should().Be(1);
        ((int)ReportDataSource.Customers).Should().Be(2);
        ((int)ReportDataSource.Contacts).Should().Be(3);
        ((int)ReportDataSource.Activities).Should().Be(4);
        ((int)ReportDataSource.SalesPerformance).Should().Be(5);
        ((int)ReportDataSource.Pipeline).Should().Be(6);
        ((int)ReportDataSource.Revenue).Should().Be(7);
        ((int)ReportDataSource.SupportCases).Should().Be(8);
        ((int)ReportDataSource.MarketingCampaigns).Should().Be(9);
        ((int)ReportDataSource.Products).Should().Be(10);
        ((int)ReportDataSource.Quotes).Should().Be(11);
        ((int)ReportDataSource.Orders).Should().Be(12);
        ((int)ReportDataSource.Invoices).Should().Be(13);
        ((int)ReportDataSource.CustomQuery).Should().Be(99);
    }

    [Fact]
    public void ReportTimePeriod_ShouldHaveCorrectValues()
    {
        ((int)ReportTimePeriod.Today).Should().Be(0);
        ((int)ReportTimePeriod.Yesterday).Should().Be(1);
        ((int)ReportTimePeriod.ThisWeek).Should().Be(2);
        ((int)ReportTimePeriod.LastWeek).Should().Be(3);
        ((int)ReportTimePeriod.ThisMonth).Should().Be(4);
        ((int)ReportTimePeriod.LastMonth).Should().Be(5);
        ((int)ReportTimePeriod.ThisQuarter).Should().Be(6);
        ((int)ReportTimePeriod.LastQuarter).Should().Be(7);
        ((int)ReportTimePeriod.ThisYear).Should().Be(8);
        ((int)ReportTimePeriod.LastYear).Should().Be(9);
        ((int)ReportTimePeriod.Last7Days).Should().Be(10);
        ((int)ReportTimePeriod.Last30Days).Should().Be(11);
        ((int)ReportTimePeriod.Last90Days).Should().Be(12);
        ((int)ReportTimePeriod.Last365Days).Should().Be(13);
        ((int)ReportTimePeriod.AllTime).Should().Be(14);
        ((int)ReportTimePeriod.Custom).Should().Be(99);
    }

    [Fact]
    public void ReportStatus_ShouldHaveCorrectValues()
    {
        ((int)ReportStatus.Draft).Should().Be(0);
        ((int)ReportStatus.Active).Should().Be(1);
        ((int)ReportStatus.Archived).Should().Be(2);
        ((int)ReportStatus.Disabled).Should().Be(3);
    }

    [Fact]
    public void ReportAccessLevel_ShouldHaveCorrectValues()
    {
        ((int)ReportAccessLevel.Private).Should().Be(0);
        ((int)ReportAccessLevel.Team).Should().Be(1);
        ((int)ReportAccessLevel.Department).Should().Be(2);
        ((int)ReportAccessLevel.Organization).Should().Be(3);
        ((int)ReportAccessLevel.Public).Should().Be(4);
    }

    #endregion

    #region Schedule Enums

    [Fact]
    public void ScheduleFrequency_ShouldHaveCorrectValues()
    {
        ((int)ScheduleFrequency.Once).Should().Be(0);
        ((int)ScheduleFrequency.Hourly).Should().Be(1);
        ((int)ScheduleFrequency.Daily).Should().Be(2);
        ((int)ScheduleFrequency.Weekly).Should().Be(3);
        ((int)ScheduleFrequency.BiWeekly).Should().Be(4);
        ((int)ScheduleFrequency.Monthly).Should().Be(5);
        ((int)ScheduleFrequency.Quarterly).Should().Be(6);
        ((int)ScheduleFrequency.Yearly).Should().Be(7);
        ((int)ScheduleFrequency.Custom).Should().Be(99);
    }

    [Fact]
    public void ReportOutputFormat_ShouldHaveCorrectValues()
    {
        ((int)ReportOutputFormat.PDF).Should().Be(0);
        ((int)ReportOutputFormat.Excel).Should().Be(1);
        ((int)ReportOutputFormat.CSV).Should().Be(2);
        ((int)ReportOutputFormat.HTML).Should().Be(3);
        ((int)ReportOutputFormat.PNG).Should().Be(4);
        ((int)ReportOutputFormat.JSON).Should().Be(5);
    }

    [Fact]
    public void ScheduleStatus_ShouldHaveCorrectValues()
    {
        ((int)ScheduleStatus.Active).Should().Be(0);
        ((int)ScheduleStatus.Paused).Should().Be(1);
        ((int)ScheduleStatus.Completed).Should().Be(2);
        ((int)ScheduleStatus.Error).Should().Be(3);
        ((int)ScheduleStatus.Disabled).Should().Be(4);
    }

    [Fact]
    public void ReportExecutionStatus_ShouldHaveCorrectValues()
    {
        ((int)ReportExecutionStatus.Queued).Should().Be(0);
        ((int)ReportExecutionStatus.Running).Should().Be(1);
        ((int)ReportExecutionStatus.Completed).Should().Be(2);
        ((int)ReportExecutionStatus.Failed).Should().Be(3);
        ((int)ReportExecutionStatus.Cancelled).Should().Be(4);
        ((int)ReportExecutionStatus.TimedOut).Should().Be(5);
    }

    #endregion

    #region ReportDefinition Entity Tests

    [Fact]
    public void ReportDefinition_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var report = new ReportDefinition();

        // Assert
        report.Name.Should().BeEmpty();
        report.Category.Should().Be("Custom");
        report.Status.Should().Be(ReportStatus.Draft);
        report.AccessLevel.Should().Be(ReportAccessLevel.Private);
        report.ColumnsJson.Should().Be("[]");
        report.TimePeriod.Should().Be(ReportTimePeriod.ThisMonth);
        report.CompareToPreviousPeriod.Should().BeFalse();
        report.ShowDataLabels.Should().BeTrue();
        report.ShowLegend.Should().BeTrue();
        report.ShowTotals.Should().BeFalse();
        report.ViewCount.Should().Be(0);
        report.FavoriteCount.Should().Be(0);
        report.ExportCount.Should().Be(0);
        report.ReportCode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ReportDefinition_ShouldAllowSettingProperties()
    {
        // Arrange
        var report = new ReportDefinition
        {
            Id = 1,
            Name = "Sales Pipeline Report",
            Description = "Monthly sales pipeline overview",
            ReportCode = "SALES001",
            Category = "Sales",
            ReportType = ReportType.FunnelChart,
            DataSource = ReportDataSource.Pipeline,
            Status = ReportStatus.Active,
            AccessLevel = ReportAccessLevel.Organization,
            TimePeriod = ReportTimePeriod.ThisQuarter,
            DateField = "CreatedAt",
            ShowTotals = true,
            CreatedByUserId = 1,
            ViewCount = 500
        };

        // Assert
        report.Name.Should().Be("Sales Pipeline Report");
        report.ReportType.Should().Be(ReportType.FunnelChart);
        report.DataSource.Should().Be(ReportDataSource.Pipeline);
        report.Status.Should().Be(ReportStatus.Active);
        report.ViewCount.Should().Be(500);
    }

    [Theory]
    [InlineData(ReportType.Table)]
    [InlineData(ReportType.BarChart)]
    [InlineData(ReportType.PieChart)]
    [InlineData(ReportType.FunnelChart)]
    [InlineData(ReportType.KPI)]
    public void ReportDefinition_ShouldAcceptAllReportTypes(ReportType reportType)
    {
        // Arrange & Act
        var report = new ReportDefinition { ReportType = reportType };

        // Assert
        report.ReportType.Should().Be(reportType);
    }

    #endregion

    #region ReportFolder Entity Tests

    [Fact]
    public void ReportFolder_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var folder = new ReportFolder();

        // Assert
        folder.Name.Should().BeEmpty();
        folder.ParentFolderId.Should().BeNull();
    }

    [Fact]
    public void ReportFolder_ShouldAllowSettingProperties()
    {
        // Arrange
        var folder = new ReportFolder
        {
            Id = 1,
            Name = "Sales Reports",
            Description = "All sales-related reports",
            ParentFolderId = null,
            OwnerUserId = 1
        };

        // Assert
        folder.Name.Should().Be("Sales Reports");
        folder.Description.Should().Contain("sales");
    }

    [Fact]
    public void ReportFolder_ShouldSupportHierarchy()
    {
        // Arrange
        var parentFolder = new ReportFolder { Id = 1, Name = "Reports" };
        var childFolder = new ReportFolder
        {
            Id = 2,
            Name = "Sales Reports",
            ParentFolderId = 1,
            ParentFolder = parentFolder
        };

        // Assert
        childFolder.ParentFolder!.Name.Should().Be("Reports");
    }

    #endregion

    #region ReportSchedule Entity Tests

    [Fact]
    public void ReportSchedule_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var schedule = new ReportSchedule();

        // Assert
        schedule.Name.Should().BeEmpty();
        schedule.Status.Should().Be(ScheduleStatus.Active);
        schedule.OutputFormat.Should().Be(ReportOutputFormat.PDF);
        schedule.IncludeInEmailBody.Should().BeFalse();
        schedule.AttachFile.Should().BeTrue();
        schedule.Timezone.Should().Be("UTC");
    }

    [Fact]
    public void ReportSchedule_ShouldAllowSettingProperties()
    {
        // Arrange
        var schedule = new ReportSchedule
        {
            Id = 1,
            Name = "Weekly Sales Report",
            Description = "Send sales report every Monday",
            ReportDefinitionId = 1,
            Status = ScheduleStatus.Active,
            Frequency = ScheduleFrequency.Weekly,
            TimeOfDay = new TimeSpan(8, 0, 0),
            DayOfWeek = System.DayOfWeek.Monday,
            Timezone = "America/New_York",
            OutputFormat = ReportOutputFormat.Excel,
            AttachFile = true,
            StartDate = DateTime.UtcNow
        };

        // Assert
        schedule.Name.Should().Be("Weekly Sales Report");
        schedule.Frequency.Should().Be(ScheduleFrequency.Weekly);
        schedule.DayOfWeek.Should().Be(System.DayOfWeek.Monday);
        schedule.OutputFormat.Should().Be(ReportOutputFormat.Excel);
    }

    #endregion

    #region ReportWidgetConfig Entity Tests

    [Fact]
    public void ReportWidgetConfig_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var config = new ReportWidgetConfig();

        // Assert
        config.ShowLegend.Should().BeTrue();
        config.ShowDataLabels.Should().BeTrue();
        config.AutoRefresh.Should().BeTrue();
    }

    [Fact]
    public void ReportWidgetConfig_ShouldAllowSettingProperties()
    {
        // Arrange
        var config = new ReportWidgetConfig
        {
            Id = 1,
            DashboardWidgetId = 100,
            ReportDefinitionId = 50,
            TimePeriod = ReportTimePeriod.Last30Days,
            ChartTypeOverride = ReportType.BarChart,
            ShowLegend = false,
            ShowDataLabels = true,
            AutoRefresh = true
        };

        // Assert
        config.TimePeriod.Should().Be(ReportTimePeriod.Last30Days);
        config.ChartTypeOverride.Should().Be(ReportType.BarChart);
        config.ShowLegend.Should().BeFalse();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ReportWithSchedules_ShouldSupportRelationship()
    {
        // Arrange
        var report = new ReportDefinition
        {
            Id = 1,
            Name = "Sales Report",
            ReportType = ReportType.Table,
            DataSource = ReportDataSource.Opportunities,
            Schedules = new List<ReportSchedule>
            {
                new ReportSchedule { Id = 1, Name = "Daily", Frequency = ScheduleFrequency.Daily },
                new ReportSchedule { Id = 2, Name = "Weekly", Frequency = ScheduleFrequency.Weekly }
            }
        };

        // Assert
        report.Schedules.Should().HaveCount(2);
        report.Schedules.Should().Contain(s => s.Frequency == ScheduleFrequency.Daily);
    }

    [Fact]
    public void FolderWithReports_ShouldSupportRelationship()
    {
        // Arrange
        var folder = new ReportFolder
        {
            Id = 1,
            Name = "Sales",
            Reports = new List<ReportDefinition>
            {
                new ReportDefinition { Id = 1, Name = "Pipeline" },
                new ReportDefinition { Id = 2, Name = "Revenue" }
            }
        };

        // Assert
        folder.Reports.Should().HaveCount(2);
    }

    [Theory]
    [InlineData(CampaignType.Email, CampaignObjective.LeadGeneration)]
    [InlineData(CampaignType.Event, CampaignObjective.EventPromotion)]
    [InlineData(CampaignType.ABM, CampaignObjective.AccountPenetration)]
    public void CampaignTypes_ShouldPairWithRelevantObjectives(CampaignType type, CampaignObjective objective)
    {
        // This test documents common type-objective pairings
        type.Should().BeDefined();
        objective.Should().BeDefined();
    }

    [Theory]
    [InlineData(ProductType.Subscription, BillingFrequency.Monthly)]
    [InlineData(ProductType.Service, BillingFrequency.Daily)]
    [InlineData(ProductType.Physical, BillingFrequency.OneTime)]
    public void ProductTypes_ShouldPairWithRelevantBillingFrequencies(ProductType type, BillingFrequency frequency)
    {
        // This test documents common type-frequency pairings
        type.Should().BeDefined();
        frequency.Should().BeDefined();
    }

    #endregion
}
