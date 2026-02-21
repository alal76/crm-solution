// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Unit tests for Dashboard, DashboardWidget, and SocialMediaFollow entities.
/// </summary>
public class DashboardSocialMediaEntityTests
{
    #region DashboardVisibility Enum

    [Theory]
    [InlineData(DashboardVisibility.Public, 0)]
    [InlineData(DashboardVisibility.Private, 1)]
    [InlineData(DashboardVisibility.RoleBased, 2)]
    public void DashboardVisibility_ShouldHaveCorrectValues(DashboardVisibility visibility, int expectedValue)
    {
        // Assert
        ((int)visibility).Should().Be(expectedValue);
    }

    [Fact]
    public void DashboardVisibility_ShouldHaveAllExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<DashboardVisibility>();

        // Assert
        values.Should().HaveCount(3);
        values.Should().Contain(DashboardVisibility.Public);
        values.Should().Contain(DashboardVisibility.Private);
        values.Should().Contain(DashboardVisibility.RoleBased);
    }

    #endregion

    #region WidgetType Enum

    [Theory]
    [InlineData(WidgetType.StatCard, 0)]
    [InlineData(WidgetType.LineChart, 1)]
    [InlineData(WidgetType.BarChart, 2)]
    [InlineData(WidgetType.PieChart, 3)]
    [InlineData(WidgetType.DataTable, 4)]
    [InlineData(WidgetType.ActivityList, 5)]
    [InlineData(WidgetType.TaskList, 6)]
    [InlineData(WidgetType.PipelineFunnel, 7)]
    [InlineData(WidgetType.ProgressGauge, 8)]
    [InlineData(WidgetType.MapWidget, 9)]
    [InlineData(WidgetType.CalendarWidget, 10)]
    [InlineData(WidgetType.CustomContent, 11)]
    [InlineData(WidgetType.Leaderboard, 12)]
    [InlineData(WidgetType.KPICard, 13)]
    [InlineData(WidgetType.AreaChart, 14)]
    [InlineData(WidgetType.StackedBarChart, 15)]
    public void WidgetType_ShouldHaveCorrectValues(WidgetType widgetType, int expectedValue)
    {
        // Assert
        ((int)widgetType).Should().Be(expectedValue);
    }

    [Fact]
    public void WidgetType_ShouldHave16Values()
    {
        // Arrange & Act
        var values = Enum.GetValues<WidgetType>();

        // Assert
        values.Should().HaveCount(16);
    }

    #endregion

    #region Dashboard Entity Tests

    [Fact]
    public void Dashboard_ShouldInitializeWithDefaults()
    {
        // Act
        var dashboard = new Dashboard();

        // Assert
        dashboard.Name.Should().BeEmpty();
        dashboard.Visibility.Should().Be(DashboardVisibility.Public);
        dashboard.LayoutConfig.Should().BeNull();
        dashboard.IsDefault.Should().BeFalse();
        dashboard.IsSystem.Should().BeFalse();
        dashboard.IsActive.Should().BeTrue();
        dashboard.RefreshIntervalSeconds.Should().Be(300);
        dashboard.ColumnCount.Should().Be(3);
        dashboard.DisplayOrder.Should().Be(0);
        dashboard.IconName.Should().Be("Dashboard");
        dashboard.OwnerId.Should().BeNull();
        dashboard.AllowedRoles.Should().BeNull();
        dashboard.Widgets.Should().NotBeNull();
        dashboard.Widgets.Should().BeEmpty();
    }

    [Fact]
    public void Dashboard_ShouldSetAllProperties()
    {
        // Arrange
        var owner = new User { Id = 1, Username = "admin" };
        var widget = new DashboardWidget { Id = 1, Title = "Sales Chart" };

        // Act
        var dashboard = new Dashboard
        {
            Id = 1,
            Name = "Sales Dashboard",
            Description = "Overview of sales metrics",
            Visibility = DashboardVisibility.RoleBased,
            LayoutConfig = "{\"columns\": 3}",
            IsDefault = true,
            IsSystem = false,
            IsActive = true,
            RefreshIntervalSeconds = 60,
            ColumnCount = 4,
            DisplayOrder = 5,
            IconName = "Analytics",
            OwnerId = 1,
            Owner = owner,
            AllowedRoles = "Admin,Sales",
            Widgets = new List<DashboardWidget> { widget },
        };

        // Assert
        dashboard.Name.Should().Be("Sales Dashboard");
        dashboard.Description.Should().Be("Overview of sales metrics");
        dashboard.Visibility.Should().Be(DashboardVisibility.RoleBased);
        dashboard.LayoutConfig.Should().Be("{\"columns\": 3}");
        dashboard.IsDefault.Should().BeTrue();
        dashboard.IsSystem.Should().BeFalse();
        dashboard.IsActive.Should().BeTrue();
        dashboard.RefreshIntervalSeconds.Should().Be(60);
        dashboard.ColumnCount.Should().Be(4);
        dashboard.DisplayOrder.Should().Be(5);
        dashboard.IconName.Should().Be("Analytics");
        dashboard.OwnerId.Should().Be(1);
        dashboard.Owner.Should().Be(owner);
        dashboard.AllowedRoles.Should().Be("Admin,Sales");
        dashboard.Widgets.Should().ContainSingle();
    }

    [Fact]
    public void Dashboard_SystemDashboard_ShouldSetProperties()
    {
        // Act
        var dashboard = new Dashboard
        {
            Name = "System Overview",
            IsSystem = true,
            IsDefault = true,
            Visibility = DashboardVisibility.Public,
        };

        // Assert
        dashboard.IsSystem.Should().BeTrue();
        dashboard.IsDefault.Should().BeTrue();
        dashboard.Visibility.Should().Be(DashboardVisibility.Public);
    }

    [Fact]
    public void Dashboard_PrivateDashboard_ShouldRequireOwner()
    {
        // Arrange
        var owner = new User { Id = 1, Username = "user" };

        // Act
        var dashboard = new Dashboard
        {
            Name = "My Dashboard",
            Visibility = DashboardVisibility.Private,
            OwnerId = 1,
            Owner = owner,
        };

        // Assert
        dashboard.Visibility.Should().Be(DashboardVisibility.Private);
        dashboard.OwnerId.Should().Be(1);
        dashboard.Owner.Should().NotBeNull();
    }

    [Theory]
    [InlineData(60, "1 minute refresh")]
    [InlineData(300, "5 minute refresh")]
    [InlineData(600, "10 minute refresh")]
    [InlineData(3600, "1 hour refresh")]
    public void Dashboard_ShouldSupportVariousRefreshIntervals(int seconds, string description)
    {
        // Act
        var dashboard = new Dashboard
        {
            Name = description,
            RefreshIntervalSeconds = seconds,
        };

        // Assert
        dashboard.RefreshIntervalSeconds.Should().Be(seconds);
    }

    #endregion

    #region DashboardWidget Entity Tests

    [Fact]
    public void DashboardWidget_ShouldInitializeWithDefaults()
    {
        // Act
        var widget = new DashboardWidget();

        // Assert
        widget.Title.Should().BeEmpty();
        widget.WidgetType.Should().Be(WidgetType.StatCard);
        widget.DataSource.Should().BeEmpty();
        widget.ConfigJson.Should().BeNull();
        widget.RowIndex.Should().Be(0);
        widget.ColumnIndex.Should().Be(0);
        widget.RowSpan.Should().Be(1);
        widget.ColumnSpan.Should().Be(1);
        widget.DisplayOrder.Should().Be(0);
        widget.RefreshIntervalSeconds.Should().Be(0);
        widget.IsVisible.Should().BeTrue();
        widget.ShowTrend.Should().BeFalse();
        widget.TrendPeriodDays.Should().Be(30);
        widget.DashboardId.Should().Be(0);
    }

    [Fact]
    public void DashboardWidget_ShouldSetAllProperties()
    {
        // Arrange
        var dashboard = new Dashboard { Id = 1, Name = "Main Dashboard" };

        // Act
        var widget = new DashboardWidget
        {
            Id = 1,
            Title = "Revenue Chart",
            Subtitle = "Monthly revenue trends",
            WidgetType = WidgetType.LineChart,
            DataSource = "api/analytics/revenue",
            ConfigJson = "{\"period\": \"monthly\"}",
            RowIndex = 2,
            ColumnIndex = 1,
            RowSpan = 2,
            ColumnSpan = 3,
            DisplayOrder = 5,
            RefreshIntervalSeconds = 120,
            IsVisible = true,
            IconName = "TrendingUp",
            Color = "#4caf50",
            BackgroundColor = "#e8f5e9",
            NavigationLink = "/reports/revenue",
            ShowTrend = true,
            TrendPeriodDays = 60,
            DashboardId = 1,
            Dashboard = dashboard,
        };

        // Assert
        widget.Title.Should().Be("Revenue Chart");
        widget.Subtitle.Should().Be("Monthly revenue trends");
        widget.WidgetType.Should().Be(WidgetType.LineChart);
        widget.DataSource.Should().Be("api/analytics/revenue");
        widget.ConfigJson.Should().Be("{\"period\": \"monthly\"}");
        widget.RowIndex.Should().Be(2);
        widget.ColumnIndex.Should().Be(1);
        widget.RowSpan.Should().Be(2);
        widget.ColumnSpan.Should().Be(3);
        widget.DisplayOrder.Should().Be(5);
        widget.RefreshIntervalSeconds.Should().Be(120);
        widget.IsVisible.Should().BeTrue();
        widget.IconName.Should().Be("TrendingUp");
        widget.Color.Should().Be("#4caf50");
        widget.BackgroundColor.Should().Be("#e8f5e9");
        widget.NavigationLink.Should().Be("/reports/revenue");
        widget.ShowTrend.Should().BeTrue();
        widget.TrendPeriodDays.Should().Be(60);
        widget.DashboardId.Should().Be(1);
        widget.Dashboard.Should().Be(dashboard);
    }

    [Theory]
    [InlineData(WidgetType.StatCard, "Single value display")]
    [InlineData(WidgetType.LineChart, "Time series data")]
    [InlineData(WidgetType.BarChart, "Category comparisons")]
    [InlineData(WidgetType.PieChart, "Part-to-whole relationships")]
    [InlineData(WidgetType.DataTable, "Tabular data")]
    [InlineData(WidgetType.PipelineFunnel, "Sales pipeline stages")]
    [InlineData(WidgetType.Leaderboard, "Performance rankings")]
    public void DashboardWidget_ShouldSupportVariousWidgetTypes(WidgetType widgetType, string description)
    {
        // Act
        var widget = new DashboardWidget
        {
            Title = description,
            WidgetType = widgetType,
        };

        // Assert
        widget.WidgetType.Should().Be(widgetType);
    }

    [Fact]
    public void DashboardWidget_HiddenWidget_ShouldNotBeVisible()
    {
        // Act
        var widget = new DashboardWidget
        {
            Title = "Hidden Widget",
            IsVisible = false,
        };

        // Assert
        widget.IsVisible.Should().BeFalse();
    }

    [Theory]
    [InlineData(1, 1, "Single cell widget")]
    [InlineData(2, 2, "2x2 widget")]
    [InlineData(1, 3, "Full-width widget")]
    [InlineData(4, 3, "Large widget")]
    public void DashboardWidget_ShouldSupportVariousSizes(int rowSpan, int colSpan, string description)
    {
        // Act
        var widget = new DashboardWidget
        {
            Title = description,
            RowSpan = rowSpan,
            ColumnSpan = colSpan,
        };

        // Assert
        widget.RowSpan.Should().Be(rowSpan);
        widget.ColumnSpan.Should().Be(colSpan);
    }

    #endregion

    #region SocialMediaFollow Entity Tests

    [Fact]
    public void SocialMediaFollow_ShouldInitializeWithDefaults()
    {
        // Act
        var follow = new SocialMediaFollow();

        // Assert
        follow.SocialMediaAccountId.Should().Be(0);
        follow.FollowedByUserId.Should().Be(0);
        follow.EntityType.Should().BeEmpty();
        follow.EntityId.Should().Be(0);
        follow.IsActive.Should().BeTrue();
        follow.NotifyOnActivity.Should().BeTrue();
        follow.NotificationFrequency.Should().Be(NotificationFrequency.Daily);
        follow.LastNotifiedAt.Should().BeNull();
        follow.Notes.Should().BeNull();
    }

    [Fact]
    public void SocialMediaFollow_ShouldSetAllProperties()
    {
        // Arrange
        var user = new User { Id = 1, Username = "analyst" };
        var account = new SocialMediaAccount { Id = 1, HandleOrUsername = "@competitor" };
        var followedAt = DateTime.UtcNow;

        // Act
        var follow = new SocialMediaFollow
        {
            Id = 1,
            SocialMediaAccountId = 1,
            SocialMediaAccount = account,
            FollowedByUserId = 1,
            FollowedByUser = user,
            EntityType = "Account",
            EntityId = 100,
            FollowedAt = followedAt,
            IsActive = true,
            NotifyOnActivity = true,
            NotificationFrequency = NotificationFrequency.Immediate,
            LastNotifiedAt = followedAt.AddHours(-1),
            Notes = "Monitoring competitor activity",
        };

        // Assert
        follow.SocialMediaAccountId.Should().Be(1);
        follow.SocialMediaAccount.Should().Be(account);
        follow.FollowedByUserId.Should().Be(1);
        follow.FollowedByUser.Should().Be(user);
        follow.EntityType.Should().Be("Account");
        follow.EntityId.Should().Be(100);
        follow.FollowedAt.Should().Be(followedAt);
        follow.IsActive.Should().BeTrue();
        follow.NotifyOnActivity.Should().BeTrue();
        follow.NotificationFrequency.Should().Be(NotificationFrequency.Immediate);
        follow.LastNotifiedAt.Should().NotBeNull();
        follow.Notes.Should().Be("Monitoring competitor activity");
    }

    [Theory]
    [InlineData(NotificationFrequency.Immediate, 0)]
    [InlineData(NotificationFrequency.Daily, 1)]
    [InlineData(NotificationFrequency.Weekly, 2)]
    [InlineData(NotificationFrequency.Never, 3)]
    public void NotificationFrequency_ShouldHaveCorrectValues(NotificationFrequency frequency, int expectedValue)
    {
        // Assert
        ((int)frequency).Should().Be(expectedValue);
    }

    [Fact]
    public void NotificationFrequency_ShouldHave4Values()
    {
        // Arrange & Act
        var values = Enum.GetValues<NotificationFrequency>();

        // Assert
        values.Should().HaveCount(4);
    }

    [Fact]
    public void SocialMediaFollow_InactiveFollow_ShouldNotNotify()
    {
        // Act
        var follow = new SocialMediaFollow
        {
            IsActive = false,
            NotifyOnActivity = true,
        };

        // Assert
        follow.IsActive.Should().BeFalse();
    }

    [Theory]
    [InlineData("Account", 1)]
    [InlineData("Contact", 2)]
    [InlineData("Lead", 3)]
    public void SocialMediaFollow_ShouldSupportDifferentEntityTypes(string entityType, int entityId)
    {
        // Act
        var follow = new SocialMediaFollow
        {
            EntityType = entityType,
            EntityId = entityId,
        };

        // Assert
        follow.EntityType.Should().Be(entityType);
        follow.EntityId.Should().Be(entityId);
    }

    [Fact]
    public void SocialMediaFollow_WithNotifications_ShouldTrackLastNotified()
    {
        // Arrange
        var lastNotified = DateTime.UtcNow.AddDays(-1);

        // Act
        var follow = new SocialMediaFollow
        {
            NotifyOnActivity = true,
            NotificationFrequency = NotificationFrequency.Daily,
            LastNotifiedAt = lastNotified,
        };

        // Assert
        follow.NotifyOnActivity.Should().BeTrue();
        follow.NotificationFrequency.Should().Be(NotificationFrequency.Daily);
        follow.LastNotifiedAt.Should().Be(lastNotified);
    }

    #endregion

    #region SocialMediaPlatform Enum

    [Theory]
    [InlineData(SocialMediaPlatform.LinkedIn, 0)]
    [InlineData(SocialMediaPlatform.Twitter, 1)]
    [InlineData(SocialMediaPlatform.Facebook, 2)]
    [InlineData(SocialMediaPlatform.Instagram, 3)]
    [InlineData(SocialMediaPlatform.YouTube, 4)]
    [InlineData(SocialMediaPlatform.TikTok, 5)]
    [InlineData(SocialMediaPlatform.WhatsApp, 6)]
    [InlineData(SocialMediaPlatform.Telegram, 7)]
    [InlineData(SocialMediaPlatform.WeChat, 8)]
    [InlineData(SocialMediaPlatform.Other, 99)]
    public void SocialMediaPlatform_ShouldHaveCorrectValues(SocialMediaPlatform platform, int expectedValue)
    {
        // Assert
        ((int)platform).Should().Be(expectedValue);
    }

    [Fact]
    public void SocialMediaPlatform_ShouldHave10Values()
    {
        // Arrange & Act
        var values = Enum.GetValues<SocialMediaPlatform>();

        // Assert
        values.Should().HaveCount(10);
    }

    #endregion

    #region SocialMediaAccountType Enum

    [Theory]
    [InlineData(SocialMediaAccountType.Personal, 0)]
    [InlineData(SocialMediaAccountType.CompanyPage, 1)]
    [InlineData(SocialMediaAccountType.Group, 2)]
    [InlineData(SocialMediaAccountType.Channel, 3)]
    public void SocialMediaAccountType_ShouldHaveCorrectValues(SocialMediaAccountType accountType, int expectedValue)
    {
        // Assert
        ((int)accountType).Should().Be(expectedValue);
    }

    [Fact]
    public void SocialMediaAccountType_ShouldHave4Values()
    {
        // Arrange & Act
        var values = Enum.GetValues<SocialMediaAccountType>();

        // Assert
        values.Should().HaveCount(4);
    }

    #endregion

    #region SocialEngagementLevel Enum

    [Theory]
    [InlineData(SocialEngagementLevel.High, 0)]
    [InlineData(SocialEngagementLevel.Medium, 1)]
    [InlineData(SocialEngagementLevel.Low, 2)]
    [InlineData(SocialEngagementLevel.Inactive, 3)]
    public void SocialEngagementLevel_ShouldHaveCorrectValues(SocialEngagementLevel level, int expectedValue)
    {
        // Assert
        ((int)level).Should().Be(expectedValue);
    }

    [Fact]
    public void SocialEngagementLevel_ShouldHave4Values()
    {
        // Arrange & Act
        var values = Enum.GetValues<SocialEngagementLevel>();

        // Assert
        values.Should().HaveCount(4);
    }

    #endregion

    #region SocialMediaAccount Entity Tests

    [Fact]
    public void SocialMediaAccount_ShouldInitializeWithDefaults()
    {
        // Act
        var account = new SocialMediaAccount();

        // Assert
        account.Platform.Should().Be(SocialMediaPlatform.LinkedIn);
        account.AccountType.Should().Be(SocialMediaAccountType.Personal);
        account.HandleOrUsername.Should().BeEmpty();
        account.ProfileUrl.Should().BeNull();
        account.DisplayName.Should().BeNull();
        account.FollowerCount.Should().BeNull();
        account.FollowingCount.Should().BeNull();
        account.IsVerifiedAccount.Should().BeFalse();
        account.IsActive.Should().BeTrue();
        account.IsValidated.Should().BeFalse();
        account.EngagementLevel.Should().BeNull();
    }

    [Fact]
    public void SocialMediaAccount_ShouldSetAllProperties()
    {
        // Act
        var account = new SocialMediaAccount
        {
            Id = 1,
            Platform = SocialMediaPlatform.LinkedIn,
            PlatformOther = null,
            AccountType = SocialMediaAccountType.CompanyPage,
            HandleOrUsername = "acme-corp",
            DisplayName = "ACME Corporation",
            ProfileUrl = "https://linkedin.com/company/acme-corp",
            EngagementLevel = SocialEngagementLevel.High,
            IsVerifiedAccount = true,
            IsActive = true,
            FollowerCount = 50000,
            FollowingCount = 500,
            LastActivityDate = DateTime.UtcNow,
            IsValidated = true,
            LastValidatedAt = DateTime.UtcNow,
            ValidationError = null,
            Notes = "Primary company page",
            CreatedBy = 1,
            UpdatedBy = 1,
        };

        // Assert
        account.Platform.Should().Be(SocialMediaPlatform.LinkedIn);
        account.AccountType.Should().Be(SocialMediaAccountType.CompanyPage);
        account.HandleOrUsername.Should().Be("acme-corp");
        account.DisplayName.Should().Be("ACME Corporation");
        account.ProfileUrl.Should().Contain("linkedin.com");
        account.EngagementLevel.Should().Be(SocialEngagementLevel.High);
        account.IsVerifiedAccount.Should().BeTrue();
        account.IsActive.Should().BeTrue();
        account.FollowerCount.Should().Be(50000);
        account.FollowingCount.Should().Be(500);
        account.IsValidated.Should().BeTrue();
        account.ValidationError.Should().BeNull();
        account.Notes.Should().Be("Primary company page");
    }

    [Fact]
    public void SocialMediaAccount_PlatformName_ShouldReturnCorrectValue()
    {
        // Arrange
        var linkedInAccount = new SocialMediaAccount { Platform = SocialMediaPlatform.LinkedIn };
        var otherAccount = new SocialMediaAccount { Platform = SocialMediaPlatform.Other, PlatformOther = "Discord" };
        var otherWithoutName = new SocialMediaAccount { Platform = SocialMediaPlatform.Other };

        // Assert
        linkedInAccount.PlatformName.Should().Be("LinkedIn");
        otherAccount.PlatformName.Should().Be("Discord");
        otherWithoutName.PlatformName.Should().Be("Other");
    }

    [Theory]
    [InlineData(SocialMediaPlatform.LinkedIn, "Professional networking")]
    [InlineData(SocialMediaPlatform.Twitter, "Microblogging")]
    [InlineData(SocialMediaPlatform.Facebook, "Social networking")]
    [InlineData(SocialMediaPlatform.Instagram, "Photo sharing")]
    [InlineData(SocialMediaPlatform.YouTube, "Video sharing")]
    [InlineData(SocialMediaPlatform.TikTok, "Short video")]
    public void SocialMediaAccount_ShouldSupportAllPlatforms(SocialMediaPlatform platform, string description)
    {
        // Act
        var account = new SocialMediaAccount
        {
            HandleOrUsername = description,
            Platform = platform,
        };

        // Assert
        account.Platform.Should().Be(platform);
    }

    [Fact]
    public void SocialMediaAccount_WithValidation_ShouldTrackValidationStatus()
    {
        // Act
        var account = new SocialMediaAccount
        {
            HandleOrUsername = "@user",
            IsValidated = true,
            LastValidatedAt = DateTime.UtcNow,
            ValidationError = null,
        };

        // Assert
        account.IsValidated.Should().BeTrue();
        account.LastValidatedAt.Should().NotBeNull();
        account.ValidationError.Should().BeNull();
    }

    [Fact]
    public void SocialMediaAccount_FailedValidation_ShouldTrackError()
    {
        // Act
        var account = new SocialMediaAccount
        {
            HandleOrUsername = "@user",
            IsValidated = false,
            LastValidatedAt = DateTime.UtcNow,
            ValidationError = "Profile not found",
        };

        // Assert
        account.IsValidated.Should().BeFalse();
        account.ValidationError.Should().Be("Profile not found");
    }

    #endregion
}
