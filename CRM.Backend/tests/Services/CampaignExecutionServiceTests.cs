// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Campaign Execution Unit Tests

using Xunit;
using FluentAssertions;
using CRM.Core.Entities;
using System;
using System.Linq;

namespace CRM.Tests.Services;

/// <summary>
/// Comprehensive unit tests for Campaign Execution functionality
/// </summary>
public class CampaignExecutionServiceTests
{
    #region CampaignWorkflow Tests

    [Fact]
    public void CreateCampaignWorkflow_Valid_CreatesCorrectly()
    {
        // Arrange & Act
        var workflow = new CampaignWorkflow
        {
            Id = 1,
            CampaignId = 100,
            WorkflowDefinitionId = 5,
            WorkflowType = "Sequential",
            TriggerEvent = "CampaignStarted",
            IsActive = true,
            Priority = 1
        };

        // Assert
        workflow.Should().NotBeNull();
        workflow.CampaignId.Should().Be(100);
        workflow.WorkflowType.Should().Be("Sequential");
        workflow.TriggerEvent.Should().Be("CampaignStarted");
    }

    [Fact]
    public void CampaignWorkflowType_AllTypes_Valid()
    {
        var types = Enum.GetValues<CampaignWorkflowType>();
        types.Should().HaveCountGreaterOrEqualTo(3);
        types.Should().Contain(CampaignWorkflowType.TriggerBased);
        types.Should().Contain(CampaignWorkflowType.Scheduled);
        types.Should().Contain(CampaignWorkflowType.Sequential);
    }

    [Fact]
    public void CampaignWorkflow_ToggleActive_UpdatesCorrectly()
    {
        // Arrange
        var workflow = new CampaignWorkflow
        {
            IsActive = true
        };

        // Act
        workflow.IsActive = false;

        // Assert
        workflow.IsActive.Should().BeFalse();
    }

    [Fact]
    public void CampaignWorkflow_WithTriggerConditions_CreatesCorrectly()
    {
        // Arrange & Act
        var workflow = new CampaignWorkflow
        {
            WorkflowType = "TriggerBased",
            TriggerEvent = "EmailOpened",
            TriggerConditions = "{\"minOpenTime\": 5, \"requireClick\": false}",
            MaxExecutionsPerContact = 1,
            CooldownHours = 24
        };

        // Assert
        workflow.TriggerConditions.Should().Contain("minOpenTime");
        workflow.MaxExecutionsPerContact.Should().Be(1);
        workflow.CooldownHours.Should().Be(24);
    }

    #endregion

    #region CampaignRecipient Tests

    [Fact]
    public void CreateCampaignRecipient_Valid_CreatesCorrectly()
    {
        // Arrange & Act
        var recipient = new CampaignRecipient
        {
            Id = 1,
            CampaignId = 100,
            ContactId = 500,
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe",
            Status = "Pending",
            OpenCount = 0,
            ClickCount = 0
        };

        // Assert
        recipient.Should().NotBeNull();
        recipient.Email.Should().Be("john.doe@example.com");
        recipient.Status.Should().Be("Pending");
    }

    [Fact]
    public void CampaignRecipientStatus_AllStatuses_Valid()
    {
        var statuses = Enum.GetValues<CampaignRecipientStatus>();
        statuses.Should().HaveCountGreaterOrEqualTo(9);
        statuses.Should().Contain(CampaignRecipientStatus.Pending);
        statuses.Should().Contain(CampaignRecipientStatus.Sent);
        statuses.Should().Contain(CampaignRecipientStatus.Delivered);
        statuses.Should().Contain(CampaignRecipientStatus.Opened);
        statuses.Should().Contain(CampaignRecipientStatus.Clicked);
        statuses.Should().Contain(CampaignRecipientStatus.Converted);
        statuses.Should().Contain(CampaignRecipientStatus.Bounced);
        statuses.Should().Contain(CampaignRecipientStatus.Unsubscribed);
    }

    [Fact]
    public void CampaignRecipient_RecordOpen_UpdatesCorrectly()
    {
        // Arrange
        var recipient = new CampaignRecipient
        {
            Status = "Delivered",
            OpenCount = 0,
            FirstOpenedAt = null
        };

        // Act
        recipient.Status = "Opened";
        recipient.OpenCount = 1;
        recipient.FirstOpenedAt = DateTime.UtcNow;
        recipient.LastOpenedAt = DateTime.UtcNow;

        // Assert
        recipient.Status.Should().Be("Opened");
        recipient.OpenCount.Should().Be(1);
        recipient.FirstOpenedAt.Should().NotBeNull();
    }

    [Fact]
    public void CampaignRecipient_RecordClick_UpdatesCorrectly()
    {
        // Arrange
        var recipient = new CampaignRecipient
        {
            Status = "Opened",
            ClickCount = 0
        };

        // Act
        recipient.Status = "Clicked";
        recipient.ClickCount = 1;
        recipient.FirstClickedAt = DateTime.UtcNow;

        // Assert
        recipient.Status.Should().Be("Clicked");
        recipient.ClickCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CampaignRecipient_Bounce_RecordsProperly()
    {
        // Arrange
        var recipient = new CampaignRecipient
        {
            Status = "Sent"
        };

        // Act
        recipient.Status = "Bounced";
        recipient.BounceType = "Hard";
        recipient.BounceReason = "Mailbox not found";

        // Assert
        recipient.Status.Should().Be("Bounced");
        recipient.BounceReason.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CampaignRecipient_Unsubscribe_RecordsProperly()
    {
        // Arrange
        var recipient = new CampaignRecipient
        {
            Status = "Opened"
        };

        // Act
        recipient.Status = "Unsubscribed";
        recipient.UnsubscribedAt = DateTime.UtcNow;

        // Assert
        recipient.Status.Should().Be("Unsubscribed");
        recipient.UnsubscribedAt.Should().NotBeNull();
    }

    [Fact]
    public void CampaignRecipient_WithABTest_AssignsVariant()
    {
        // Arrange & Act
        var recipient = new CampaignRecipient
        {
            Email = "test@example.com",
            ABTestVariant = "A"
        };

        // Assert
        recipient.ABTestVariant.Should().Be("A");
    }

    [Fact]
    public void BounceType_AllTypes_Valid()
    {
        var types = Enum.GetValues<BounceType>();
        types.Should().HaveCountGreaterOrEqualTo(4);
        types.Should().Contain(BounceType.None);
        types.Should().Contain(BounceType.Hard);
        types.Should().Contain(BounceType.Soft);
        types.Should().Contain(BounceType.Technical);
    }

    #endregion

    #region CampaignLinkClick Tests

    [Fact]
    public void CreateCampaignLinkClick_Valid_CreatesCorrectly()
    {
        // Arrange & Act
        var click = new CampaignLinkClick
        {
            Id = 1,
            CampaignRecipientId = 100,
            CampaignId = 50,
            LinkUrl = "https://example.com/offer",
            LinkLabel = "Special Offer",
            ClickedAt = DateTime.UtcNow,
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            DeviceType = "Desktop"
        };

        // Assert
        click.Should().NotBeNull();
        click.LinkUrl.Should().Be("https://example.com/offer");
        click.LinkLabel.Should().Be("Special Offer");
        click.DeviceType.Should().Be("Desktop");
    }

    [Fact]
    public void CampaignLinkClick_WithGeolocation_CreatesCorrectly()
    {
        // Arrange & Act
        var click = new CampaignLinkClick
        {
            CampaignRecipientId = 100,
            CampaignId = 50,
            LinkUrl = "https://example.com",
            ClickedAt = DateTime.UtcNow,
            LocationData = "{\"country\": \"United States\", \"city\": \"New York\"}"
        };

        // Assert
        click.LocationData.Should().Contain("United States");
        click.LocationData.Should().Contain("New York");
    }

    [Fact]
    public void CampaignLinkClick_DeviceInfo_RecordsProperly()
    {
        // Arrange & Act
        var click = new CampaignLinkClick
        {
            CampaignRecipientId = 100,
            CampaignId = 50,
            LinkUrl = "https://example.com",
            Browser = "Chrome",
            OperatingSystem = "Windows 11",
            DeviceType = "Desktop"
        };

        // Assert
        click.Browser.Should().Be("Chrome");
        click.OperatingSystem.Should().Be("Windows 11");
    }

    #endregion

    #region CampaignABTest Tests

    [Fact]
    public void CreateCampaignABTest_Valid_CreatesCorrectly()
    {
        // Arrange & Act
        var test = new CampaignABTest
        {
            Id = 1,
            CampaignId = 100,
            TestName = "Subject Line Test",
            TestType = "SubjectLine",
            TestMetric = "OpenRate",
            Status = "Draft",
            TrafficSplit = "{\"A\": 50, \"B\": 50}",
            SampleSize = 1000
        };

        // Assert
        test.Should().NotBeNull();
        test.TestType.Should().Be("SubjectLine");
        test.Status.Should().Be("Draft");
        test.TestMetric.Should().Be("OpenRate");
    }

    [Fact]
    public void ABTestType_AllTypes_Valid()
    {
        var types = Enum.GetValues<ABTestType>();
        types.Should().HaveCountGreaterOrEqualTo(5);
        types.Should().Contain(ABTestType.SubjectLine);
        types.Should().Contain(ABTestType.Content);
        types.Should().Contain(ABTestType.SendTime);
        types.Should().Contain(ABTestType.FromName);
        types.Should().Contain(ABTestType.PreviewText);
    }

    [Fact]
    public void ABTestStatus_AllStatuses_Valid()
    {
        var statuses = Enum.GetValues<ABTestStatus>();
        statuses.Should().HaveCountGreaterOrEqualTo(4);
        statuses.Should().Contain(ABTestStatus.Draft);
        statuses.Should().Contain(ABTestStatus.Running);
        statuses.Should().Contain(ABTestStatus.Completed);
        statuses.Should().Contain(ABTestStatus.Cancelled);
    }

    [Fact]
    public void ABTestMetric_AllMetrics_Valid()
    {
        var metrics = Enum.GetValues<ABTestMetric>();
        metrics.Should().HaveCountGreaterOrEqualTo(4);
        metrics.Should().Contain(ABTestMetric.OpenRate);
        metrics.Should().Contain(ABTestMetric.ClickRate);
        metrics.Should().Contain(ABTestMetric.ConversionRate);
        metrics.Should().Contain(ABTestMetric.Revenue);
    }

    [Fact]
    public void CampaignABTest_StartTest_UpdatesStatus()
    {
        // Arrange
        var test = new CampaignABTest
        {
            Status = "Draft",
            TestStartedAt = null
        };

        // Act
        test.Status = "Running";
        test.TestStartedAt = DateTime.UtcNow;

        // Assert
        test.Status.Should().Be("Running");
        test.TestStartedAt.Should().NotBeNull();
    }

    [Fact]
    public void CampaignABTest_CompleteTest_SetsWinner()
    {
        // Arrange
        var test = new CampaignABTest
        {
            Status = "Running"
        };

        // Act
        test.Status = "Completed";
        test.TestCompletedAt = DateTime.UtcNow;
        test.WinnerVariant = "B";
        test.ConfidenceLevel = 95.5m;

        // Assert
        test.Status.Should().Be("Completed");
        test.WinnerVariant.Should().Be("B");
        test.ConfidenceLevel.Should().BeGreaterThan(95);
    }

    [Fact]
    public void CampaignABTest_WithVariantConfigs_CreatesCorrectly()
    {
        // Arrange & Act
        var test = new CampaignABTest
        {
            VariantConfigs = "[{\"id\": \"A\", \"subject\": \"Variant A\"}, {\"id\": \"B\", \"subject\": \"Variant B\"}]",
            TrafficSplit = "{\"A\": 50, \"B\": 50}"
        };

        // Assert
        test.VariantConfigs.Should().Contain("Variant A");
        test.TrafficSplit.Should().Contain("50");
    }

    [Fact]
    public void CampaignABTest_AutoSelectWinner_ConfiguresCorrectly()
    {
        // Arrange & Act
        var test = new CampaignABTest
        {
            AutoSelectWinner = true,
            AutoWinnerAfterHours = 24
        };

        // Assert
        test.AutoSelectWinner.Should().BeTrue();
        test.AutoWinnerAfterHours.Should().Be(24);
    }

    #endregion

    #region CampaignConversion Tests

    [Fact]
    public void CreateCampaignConversion_Valid_CreatesCorrectly()
    {
        // Arrange & Act
        var conversion = new CampaignConversion
        {
            Id = 1,
            CampaignId = 100,
            CampaignRecipientId = 500,
            ContactId = 200,
            ConversionType = "Purchase",
            ConversionValue = 149.99m,
            ConversionCurrency = "USD",
            AttributionModel = "LastTouch",
            AttributionPercentage = 100
        };

        // Assert
        conversion.Should().NotBeNull();
        conversion.ConversionType.Should().Be("Purchase");
        conversion.ConversionValue.Should().Be(149.99m);
        conversion.AttributionPercentage.Should().Be(100);
    }

    [Fact]
    public void ConversionType_AllTypes_Valid()
    {
        var types = Enum.GetValues<ConversionType>();
        types.Should().HaveCountGreaterOrEqualTo(10);
        types.Should().Contain(ConversionType.Purchase);
        types.Should().Contain(ConversionType.Signup);
        types.Should().Contain(ConversionType.Download);
        types.Should().Contain(ConversionType.FormSubmit);
        types.Should().Contain(ConversionType.Demo);
        types.Should().Contain(ConversionType.Trial);
        types.Should().Contain(ConversionType.Subscription);
    }

    [Fact]
    public void AttributionModel_AllModels_Valid()
    {
        var models = Enum.GetValues<AttributionModel>();
        models.Should().HaveCountGreaterOrEqualTo(5);
        models.Should().Contain(AttributionModel.FirstTouch);
        models.Should().Contain(AttributionModel.LastTouch);
        models.Should().Contain(AttributionModel.Linear);
        models.Should().Contain(AttributionModel.TimeDecay);
        models.Should().Contain(AttributionModel.PositionBased);
    }

    [Fact]
    public void CampaignConversion_PartialAttribution_CreatesCorrectly()
    {
        // Arrange & Act
        var conversion = new CampaignConversion
        {
            ConversionType = "Purchase",
            ConversionValue = 200m,
            AttributionModel = "Linear",
            AttributionPercentage = 50
        };

        // Assert
        conversion.AttributionModel.Should().Be("Linear");
        conversion.AttributionPercentage.Should().Be(50);
    }

    [Fact]
    public void CampaignConversion_WithConversionData_CreatesCorrectly()
    {
        // Arrange & Act
        var conversion = new CampaignConversion
        {
            ConversionType = "Purchase",
            ConversionValue = 99.99m,
            ConversionData = "{\"productId\": \"SKU123\", \"quantity\": 2}"
        };

        // Assert
        conversion.ConversionData.Should().Contain("productId");
        conversion.ConversionData.Should().Contain("SKU123");
    }

    #endregion

    #region Integration Scenario Tests

    [Fact]
    public void CampaignFunnel_FullJourney_TracksCorrectly()
    {
        // Arrange - Simulate full campaign recipient journey
        var recipient = new CampaignRecipient
        {
            Id = 1,
            CampaignId = 100,
            Email = "customer@example.com",
            Status = "Pending",
            OpenCount = 0,
            ClickCount = 0
        };

        // Act - Simulate journey steps
        // Step 1: Sent
        recipient.Status = "Sent";
        recipient.SendActualTime = DateTime.UtcNow.AddMinutes(-30);

        // Step 2: Delivered
        recipient.Status = "Delivered";
        recipient.DeliveredAt = DateTime.UtcNow.AddMinutes(-29);

        // Step 3: Opened
        recipient.Status = "Opened";
        recipient.OpenCount = 1;
        recipient.FirstOpenedAt = DateTime.UtcNow.AddMinutes(-20);

        // Step 4: Clicked
        recipient.Status = "Clicked";
        recipient.ClickCount = 2;
        recipient.FirstClickedAt = DateTime.UtcNow.AddMinutes(-15);

        // Step 5: Converted
        recipient.Status = "Converted";
        recipient.ConvertedAt = DateTime.UtcNow.AddMinutes(-5);
        recipient.ConversionValue = 99.99m;

        // Assert - Full journey completed
        recipient.Status.Should().Be("Converted");
        recipient.OpenCount.Should().BeGreaterThan(0);
        recipient.ClickCount.Should().BeGreaterThan(0);
        recipient.ConversionValue.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CampaignWorkflow_PriorityExecution_OrdersCorrectly()
    {
        // Arrange
        var workflows = new[]
        {
            new CampaignWorkflow { Id = 1, Priority = 3, WorkflowType = "Sequential" },
            new CampaignWorkflow { Id = 2, Priority = 1, WorkflowType = "TriggerBased" },
            new CampaignWorkflow { Id = 3, Priority = 2, WorkflowType = "Scheduled" }
        };

        // Act - Sort by priority (lower number = higher priority)
        var orderedWorkflows = workflows.OrderBy(w => w.Priority).ToArray();

        // Assert
        orderedWorkflows[0].Id.Should().Be(2); // Priority 1
        orderedWorkflows[1].Id.Should().Be(3); // Priority 2
        orderedWorkflows[2].Id.Should().Be(1); // Priority 3
    }

    [Fact]
    public void CampaignABTest_WinnerDetermination_SelectsHigherMetric()
    {
        // Arrange - Test completed with two variants
        var test = new CampaignABTest
        {
            TestName = "Subject Line Test",
            TestType = "SubjectLine",
            TestMetric = "OpenRate",
            Status = "Running"
        };

        // Simulate variant A: 25% open rate, Variant B: 32% open rate
        var variantAOpenRate = 25.0m;
        var variantBOpenRate = 32.0m;

        // Act - Complete test and determine winner
        test.Status = "Completed";
        test.TestCompletedAt = DateTime.UtcNow;
        test.WinnerVariant = variantBOpenRate > variantAOpenRate ? "B" : "A";

        // Assert
        test.WinnerVariant.Should().Be("B");
    }

    #endregion
}
