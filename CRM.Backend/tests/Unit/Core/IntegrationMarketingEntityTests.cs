// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Unit tests for Calendar/Email Integration, Landing Pages, Lead Scoring, and Event Attendees

using System;
using System.Collections.Generic;
using CRM.Core.Entities;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Comprehensive unit tests for Integration and Marketing entities:
/// - Calendar Integration (CalendarProvider, CalendarSyncDirection, CalendarSyncStatus, CalendarIntegration, CalendarSyncLog, CalendarEventMapping)
/// - Email Integration (EmailProvider, EmailSyncStatus, EmailIntegration, EmailSyncLog, EmailMessageMapping)
/// - Landing Pages (LandingPageStatus, LandingPageTemplate, LandingPageBlockType, LandingPage, LandingPageBlock, LandingPageVisit)
/// - Lead Scoring (LeadScoreRuleType, RuleOperator, LeadScoreRule)
/// - Event Attendees (AttendeeType, AttendeeResponseStatus, EventAttendee)
/// </summary>
public class IntegrationMarketingEntityTests
{
    #region Calendar Integration Enum Tests

    [Fact]
    public void CalendarProvider_ShouldHaveCorrectValues()
    {
        // Assert
        ((int)CalendarProvider.Google).Should().Be(0);
        ((int)CalendarProvider.Outlook).Should().Be(1);
        ((int)CalendarProvider.Apple).Should().Be(2);
    }

    [Fact]
    public void CalendarProvider_ShouldHaveExactlyThreeValues()
    {
        // Assert
        Enum.GetValues<CalendarProvider>().Should().HaveCount(3);
    }

    [Theory]
    [InlineData(CalendarProvider.Google, "Google")]
    [InlineData(CalendarProvider.Outlook, "Outlook")]
    [InlineData(CalendarProvider.Apple, "Apple")]
    public void CalendarProvider_ShouldHaveCorrectNames(CalendarProvider provider, string expectedName)
    {
        // Assert
        provider.ToString().Should().Be(expectedName);
    }

    [Fact]
    public void CalendarSyncDirection_ShouldHaveCorrectValues()
    {
        // Assert
        ((int)CalendarSyncDirection.Import).Should().Be(0);
        ((int)CalendarSyncDirection.Export).Should().Be(1);
        ((int)CalendarSyncDirection.Bidirectional).Should().Be(2);
    }

    [Fact]
    public void CalendarSyncDirection_ShouldHaveExactlyThreeValues()
    {
        // Assert
        Enum.GetValues<CalendarSyncDirection>().Should().HaveCount(3);
    }

    [Theory]
    [InlineData(CalendarSyncDirection.Import, "Import")]
    [InlineData(CalendarSyncDirection.Export, "Export")]
    [InlineData(CalendarSyncDirection.Bidirectional, "Bidirectional")]
    public void CalendarSyncDirection_ShouldHaveCorrectNames(CalendarSyncDirection direction, string expectedName)
    {
        // Assert
        direction.ToString().Should().Be(expectedName);
    }

    [Fact]
    public void CalendarSyncStatus_ShouldHaveCorrectValues()
    {
        // Assert
        ((int)CalendarSyncStatus.Success).Should().Be(0);
        ((int)CalendarSyncStatus.InProgress).Should().Be(1);
        ((int)CalendarSyncStatus.Failed).Should().Be(2);
        ((int)CalendarSyncStatus.Pending).Should().Be(3);
    }

    [Fact]
    public void CalendarSyncStatus_ShouldHaveExactlyFourValues()
    {
        // Assert
        Enum.GetValues<CalendarSyncStatus>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData(CalendarSyncStatus.Success, "Success")]
    [InlineData(CalendarSyncStatus.InProgress, "InProgress")]
    [InlineData(CalendarSyncStatus.Failed, "Failed")]
    [InlineData(CalendarSyncStatus.Pending, "Pending")]
    public void CalendarSyncStatus_ShouldHaveCorrectNames(CalendarSyncStatus status, string expectedName)
    {
        // Assert
        status.ToString().Should().Be(expectedName);
    }

    #endregion

    #region CalendarIntegration Entity Tests

    [Fact]
    public void CalendarIntegration_ShouldInitializeWithDefaults()
    {
        // Act
        var integration = new CalendarIntegration();

        // Assert
        integration.UserId.Should().Be(0);
        integration.Provider.Should().Be(CalendarProvider.Google);
        integration.AccessToken.Should().BeEmpty();
        integration.RefreshToken.Should().BeEmpty();
        integration.SyncDirection.Should().Be(CalendarSyncDirection.Bidirectional);
        integration.LastSyncStatus.Should().Be(CalendarSyncStatus.Pending);
        integration.SyncIntervalMinutes.Should().Be(15);
        integration.IsActive.Should().BeTrue();
        integration.TotalEventsSynced.Should().Be(0);
        integration.SyncLogs.Should().BeEmpty();
    }

    [Fact]
    public void CalendarIntegration_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var tokenExpiry = DateTime.UtcNow.AddHours(1);
        var lastSync = DateTime.UtcNow.AddMinutes(-30);
        var nextSync = DateTime.UtcNow.AddMinutes(15);

        // Act
        var integration = new CalendarIntegration
        {
            UserId = 1,
            Provider = CalendarProvider.Outlook,
            AccessToken = "access_token_123",
            RefreshToken = "refresh_token_456",
            TokenExpiresAt = tokenExpiry,
            CalendarId = "primary",
            CalendarName = "Work Calendar",
            ExternalEmail = "user@company.com",
            SyncDirection = CalendarSyncDirection.Export,
            LastSyncAt = lastSync,
            LastSyncStatus = CalendarSyncStatus.Success,
            LastSyncError = null,
            NextSyncAt = nextSync,
            SyncIntervalMinutes = 30,
            IsActive = true,
            SyncToken = "sync_token_789",
            LastSyncEventsCount = 25,
            TotalEventsSynced = 150,
            SettingsJson = "{\"color\":\"blue\"}"
        };

        // Assert
        integration.UserId.Should().Be(1);
        integration.Provider.Should().Be(CalendarProvider.Outlook);
        integration.AccessToken.Should().Be("access_token_123");
        integration.RefreshToken.Should().Be("refresh_token_456");
        integration.TokenExpiresAt.Should().Be(tokenExpiry);
        integration.CalendarId.Should().Be("primary");
        integration.CalendarName.Should().Be("Work Calendar");
        integration.ExternalEmail.Should().Be("user@company.com");
        integration.SyncDirection.Should().Be(CalendarSyncDirection.Export);
        integration.LastSyncAt.Should().Be(lastSync);
        integration.LastSyncStatus.Should().Be(CalendarSyncStatus.Success);
        integration.NextSyncAt.Should().Be(nextSync);
        integration.SyncIntervalMinutes.Should().Be(30);
        integration.SyncToken.Should().Be("sync_token_789");
        integration.LastSyncEventsCount.Should().Be(25);
        integration.TotalEventsSynced.Should().Be(150);
        integration.SettingsJson.Should().Be("{\"color\":\"blue\"}");
    }

    [Fact]
    public void CalendarIntegration_ShouldSupportAllProviders()
    {
        // Arrange & Act
        var googleIntegration = new CalendarIntegration { Provider = CalendarProvider.Google };
        var outlookIntegration = new CalendarIntegration { Provider = CalendarProvider.Outlook };
        var appleIntegration = new CalendarIntegration { Provider = CalendarProvider.Apple };

        // Assert
        googleIntegration.Provider.Should().Be(CalendarProvider.Google);
        outlookIntegration.Provider.Should().Be(CalendarProvider.Outlook);
        appleIntegration.Provider.Should().Be(CalendarProvider.Apple);
    }

    #endregion

    #region CalendarSyncLog Entity Tests

    [Fact]
    public void CalendarSyncLog_ShouldInitializeWithDefaults()
    {
        // Act
        var log = new CalendarSyncLog();

        // Assert
        log.CalendarIntegrationId.Should().Be(0);
        log.EventsCreated.Should().Be(0);
        log.EventsUpdated.Should().Be(0);
        log.EventsDeleted.Should().Be(0);
        log.ConflictsResolved.Should().Be(0);
    }

    [Fact]
    public void CalendarSyncLog_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddMinutes(-5);
        var endTime = DateTime.UtcNow;

        // Act
        var log = new CalendarSyncLog
        {
            CalendarIntegrationId = 1,
            StartedAt = startTime,
            CompletedAt = endTime,
            Status = CalendarSyncStatus.Success,
            EventsCreated = 10,
            EventsUpdated = 5,
            EventsDeleted = 2,
            ConflictsResolved = 1,
            Direction = CalendarSyncDirection.Bidirectional
        };

        // Assert
        log.CalendarIntegrationId.Should().Be(1);
        log.StartedAt.Should().Be(startTime);
        log.CompletedAt.Should().Be(endTime);
        log.Status.Should().Be(CalendarSyncStatus.Success);
        log.EventsCreated.Should().Be(10);
        log.EventsUpdated.Should().Be(5);
        log.EventsDeleted.Should().Be(2);
        log.ConflictsResolved.Should().Be(1);
        log.Direction.Should().Be(CalendarSyncDirection.Bidirectional);
    }

    [Fact]
    public void CalendarSyncLog_ShouldCaptureErrors()
    {
        // Act
        var log = new CalendarSyncLog
        {
            Status = CalendarSyncStatus.Failed,
            ErrorMessage = "OAuth token expired",
            ErrorStackTrace = "at SyncService.Sync() line 42"
        };

        // Assert
        log.Status.Should().Be(CalendarSyncStatus.Failed);
        log.ErrorMessage.Should().Be("OAuth token expired");
        log.ErrorStackTrace.Should().Contain("line 42");
    }

    #endregion

    #region CalendarEventMapping Entity Tests

    [Fact]
    public void CalendarEventMapping_ShouldInitializeWithDefaults()
    {
        // Act
        var mapping = new CalendarEventMapping();

        // Assert
        mapping.ActivityId.Should().Be(0);
        mapping.CalendarIntegrationId.Should().Be(0);
        mapping.ExternalEventId.Should().BeEmpty();
        mapping.CreatedFromExternal.Should().BeFalse();
    }

    [Fact]
    public void CalendarEventMapping_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var lastSynced = DateTime.UtcNow.AddMinutes(-10);
        var externalModified = DateTime.UtcNow.AddHours(-1);
        var crmModified = DateTime.UtcNow.AddMinutes(-20);

        // Act
        var mapping = new CalendarEventMapping
        {
            ActivityId = 1,
            CalendarIntegrationId = 2,
            ExternalEventId = "google_event_123",
            ExternalEventUid = "uid_123@google.com",
            ExternalETag = "etag_abc",
            LastSyncedAt = lastSynced,
            ExternalLastModified = externalModified,
            CrmLastModified = crmModified,
            CreatedFromExternal = true
        };

        // Assert
        mapping.ActivityId.Should().Be(1);
        mapping.CalendarIntegrationId.Should().Be(2);
        mapping.ExternalEventId.Should().Be("google_event_123");
        mapping.ExternalEventUid.Should().Be("uid_123@google.com");
        mapping.ExternalETag.Should().Be("etag_abc");
        mapping.LastSyncedAt.Should().Be(lastSynced);
        mapping.ExternalLastModified.Should().Be(externalModified);
        mapping.CrmLastModified.Should().Be(crmModified);
        mapping.CreatedFromExternal.Should().BeTrue();
    }

    #endregion

    #region Email Integration Enum Tests

    [Fact]
    public void EmailProvider_ShouldHaveCorrectValues()
    {
        // Assert
        ((int)EmailProvider.Google).Should().Be(0);
        ((int)EmailProvider.Outlook).Should().Be(1);
        ((int)EmailProvider.Imap).Should().Be(2);
    }

    [Fact]
    public void EmailProvider_ShouldHaveExactlyThreeValues()
    {
        // Assert
        Enum.GetValues<EmailProvider>().Should().HaveCount(3);
    }

    [Theory]
    [InlineData(EmailProvider.Google, "Google")]
    [InlineData(EmailProvider.Outlook, "Outlook")]
    [InlineData(EmailProvider.Imap, "Imap")]
    public void EmailProvider_ShouldHaveCorrectNames(EmailProvider provider, string expectedName)
    {
        // Assert
        provider.ToString().Should().Be(expectedName);
    }

    [Fact]
    public void EmailSyncStatus_ShouldHaveCorrectValues()
    {
        // Assert
        ((int)EmailSyncStatus.Success).Should().Be(0);
        ((int)EmailSyncStatus.InProgress).Should().Be(1);
        ((int)EmailSyncStatus.Failed).Should().Be(2);
        ((int)EmailSyncStatus.Pending).Should().Be(3);
    }

    [Fact]
    public void EmailSyncStatus_ShouldHaveExactlyFourValues()
    {
        // Assert
        Enum.GetValues<EmailSyncStatus>().Should().HaveCount(4);
    }

    #endregion

    #region EmailIntegration Entity Tests

    [Fact]
    public void EmailIntegration_ShouldInitializeWithDefaults()
    {
        // Act
        var integration = new EmailIntegration();

        // Assert
        integration.UserId.Should().Be(0);
        integration.Provider.Should().Be(EmailProvider.Google);
        integration.EmailAddress.Should().BeEmpty();
        integration.UseSsl.Should().BeTrue();
        integration.LastSyncStatus.Should().Be(EmailSyncStatus.Pending);
        integration.SyncIntervalMinutes.Should().Be(15);
        integration.IsActive.Should().BeTrue();
        integration.TotalEmailsSynced.Should().Be(0);
        integration.SyncLogs.Should().BeEmpty();
        integration.MessageMappings.Should().BeEmpty();
    }

    [Fact]
    public void EmailIntegration_ShouldSetOAuthPropertiesCorrectly()
    {
        // Arrange
        var tokenExpiry = DateTime.UtcNow.AddHours(1);

        // Act
        var integration = new EmailIntegration
        {
            UserId = 1,
            Provider = EmailProvider.Google,
            EmailAddress = "user@gmail.com",
            AccessToken = "access_token_encrypted",
            RefreshToken = "refresh_token_encrypted",
            TokenExpiresAt = tokenExpiry,
            IsActive = true
        };

        // Assert
        integration.Provider.Should().Be(EmailProvider.Google);
        integration.EmailAddress.Should().Be("user@gmail.com");
        integration.AccessToken.Should().Be("access_token_encrypted");
        integration.RefreshToken.Should().Be("refresh_token_encrypted");
        integration.TokenExpiresAt.Should().Be(tokenExpiry);
    }

    [Fact]
    public void EmailIntegration_ShouldSetImapPropertiesCorrectly()
    {
        // Act
        var integration = new EmailIntegration
        {
            UserId = 1,
            Provider = EmailProvider.Imap,
            EmailAddress = "user@company.com",
            ImapServer = "imap.company.com",
            ImapPort = 993,
            ImapUsername = "user@company.com",
            ImapPassword = "encrypted_password",
            UseSsl = true
        };

        // Assert
        integration.Provider.Should().Be(EmailProvider.Imap);
        integration.ImapServer.Should().Be("imap.company.com");
        integration.ImapPort.Should().Be(993);
        integration.ImapUsername.Should().Be("user@company.com");
        integration.ImapPassword.Should().Be("encrypted_password");
        integration.UseSsl.Should().BeTrue();
    }

    [Fact]
    public void EmailIntegration_ShouldTrackSyncState()
    {
        // Arrange
        var lastSync = DateTime.UtcNow.AddMinutes(-15);
        var nextSync = DateTime.UtcNow.AddMinutes(15);

        // Act
        var integration = new EmailIntegration
        {
            LastSyncAt = lastSync,
            LastSyncStatus = EmailSyncStatus.Success,
            NextSyncAt = nextSync,
            LastSyncToken = "sync_delta_token",
            TotalEmailsSynced = 500
        };

        // Assert
        integration.LastSyncAt.Should().Be(lastSync);
        integration.LastSyncStatus.Should().Be(EmailSyncStatus.Success);
        integration.NextSyncAt.Should().Be(nextSync);
        integration.LastSyncToken.Should().Be("sync_delta_token");
        integration.TotalEmailsSynced.Should().Be(500);
    }

    #endregion

    #region EmailSyncLog Entity Tests

    [Fact]
    public void EmailSyncLog_ShouldInitializeWithDefaults()
    {
        // Act
        var log = new EmailSyncLog();

        // Assert
        log.EmailIntegrationId.Should().Be(0);
        log.EmailsCreated.Should().Be(0);
        log.EmailsUpdated.Should().Be(0);
        log.EmailsSkipped.Should().Be(0);
    }

    [Fact]
    public void EmailSyncLog_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddMinutes(-2);
        var endTime = DateTime.UtcNow;

        // Act
        var log = new EmailSyncLog
        {
            EmailIntegrationId = 1,
            StartedAt = startTime,
            CompletedAt = endTime,
            Status = EmailSyncStatus.Success,
            EmailsCreated = 50,
            EmailsUpdated = 10,
            EmailsSkipped = 5
        };

        // Assert
        log.EmailIntegrationId.Should().Be(1);
        log.StartedAt.Should().Be(startTime);
        log.CompletedAt.Should().Be(endTime);
        log.Status.Should().Be(EmailSyncStatus.Success);
        log.EmailsCreated.Should().Be(50);
        log.EmailsUpdated.Should().Be(10);
        log.EmailsSkipped.Should().Be(5);
    }

    [Fact]
    public void EmailSyncLog_ShouldCaptureErrors()
    {
        // Act
        var log = new EmailSyncLog
        {
            Status = EmailSyncStatus.Failed,
            ErrorMessage = "IMAP connection timeout",
            ErrorStackTrace = "at ImapClient.Connect() line 128"
        };

        // Assert
        log.Status.Should().Be(EmailSyncStatus.Failed);
        log.ErrorMessage.Should().Be("IMAP connection timeout");
        log.ErrorStackTrace.Should().Contain("line 128");
    }

    #endregion

    #region Landing Page Enum Tests

    [Fact]
    public void LandingPageStatus_ShouldHaveCorrectValues()
    {
        // Assert
        ((int)LandingPageStatus.Draft).Should().Be(0);
        ((int)LandingPageStatus.Published).Should().Be(1);
        ((int)LandingPageStatus.Archived).Should().Be(2);
        ((int)LandingPageStatus.Scheduled).Should().Be(3);
    }

    [Fact]
    public void LandingPageStatus_ShouldHaveExactlyFourValues()
    {
        // Assert
        Enum.GetValues<LandingPageStatus>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData(LandingPageStatus.Draft, "Draft")]
    [InlineData(LandingPageStatus.Published, "Published")]
    [InlineData(LandingPageStatus.Archived, "Archived")]
    [InlineData(LandingPageStatus.Scheduled, "Scheduled")]
    public void LandingPageStatus_ShouldHaveCorrectNames(LandingPageStatus status, string expectedName)
    {
        // Assert
        status.ToString().Should().Be(expectedName);
    }

    [Fact]
    public void LandingPageTemplate_ShouldHaveCorrectValues()
    {
        // Assert
        ((int)LandingPageTemplate.Blank).Should().Be(0);
        ((int)LandingPageTemplate.LeadCapture).Should().Be(1);
        ((int)LandingPageTemplate.ProductShowcase).Should().Be(2);
        ((int)LandingPageTemplate.EventRegistration).Should().Be(3);
        ((int)LandingPageTemplate.WebinarRegistration).Should().Be(4);
        ((int)LandingPageTemplate.EbookDownload).Should().Be(5);
        ((int)LandingPageTemplate.ThankYou).Should().Be(6);
    }

    [Fact]
    public void LandingPageTemplate_ShouldHaveSevenValues()
    {
        // Assert
        Enum.GetValues<LandingPageTemplate>().Should().HaveCount(7);
    }

    [Fact]
    public void LandingPageBlockType_ShouldHaveCorrectValues()
    {
        // Assert
        ((int)LandingPageBlockType.Hero).Should().Be(0);
        ((int)LandingPageBlockType.Text).Should().Be(1);
        ((int)LandingPageBlockType.Image).Should().Be(2);
        ((int)LandingPageBlockType.Video).Should().Be(3);
        ((int)LandingPageBlockType.Form).Should().Be(4);
        ((int)LandingPageBlockType.Button).Should().Be(5);
        ((int)LandingPageBlockType.TwoColumn).Should().Be(6);
        ((int)LandingPageBlockType.ThreeColumn).Should().Be(7);
        ((int)LandingPageBlockType.Features).Should().Be(8);
        ((int)LandingPageBlockType.Testimonial).Should().Be(9);
        ((int)LandingPageBlockType.Pricing).Should().Be(10);
        ((int)LandingPageBlockType.FAQ).Should().Be(11);
        ((int)LandingPageBlockType.SocialProof).Should().Be(12);
        ((int)LandingPageBlockType.Countdown).Should().Be(13);
        ((int)LandingPageBlockType.CustomHtml).Should().Be(14);
        ((int)LandingPageBlockType.Divider).Should().Be(15);
        ((int)LandingPageBlockType.Header).Should().Be(16);
        ((int)LandingPageBlockType.Footer).Should().Be(17);
    }

    [Fact]
    public void LandingPageBlockType_ShouldHaveEighteenValues()
    {
        // Assert
        Enum.GetValues<LandingPageBlockType>().Should().HaveCount(18);
    }

    #endregion

    #region LandingPage Entity Tests

    [Fact]
    public void LandingPage_ShouldInitializeWithDefaults()
    {
        // Act
        var page = new LandingPage();

        // Assert
        page.Name.Should().BeEmpty();
        page.Slug.Should().BeEmpty();
        page.Template.Should().Be(LandingPageTemplate.Blank);
        page.Status.Should().Be(LandingPageStatus.Draft);
        page.IsActive.Should().BeTrue();
        page.PageViews.Should().Be(0);
        page.UniqueVisitors.Should().Be(0);
        page.Conversions.Should().Be(0);
        page.AverageTimeOnPage.Should().Be(0);
        page.BounceRate.Should().Be(0);
        page.Blocks.Should().BeEmpty();
        page.Visits.Should().BeEmpty();
    }

    [Fact]
    public void LandingPage_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var publishDate = DateTime.UtcNow.AddDays(-7);
        var schedulePublish = DateTime.UtcNow.AddDays(1);
        var scheduleUnpublish = DateTime.UtcNow.AddDays(30);

        // Act
        var page = new LandingPage
        {
            Name = "Summer Sale Landing Page",
            Slug = "summer-sale-2025",
            Title = "50% Off Summer Sale",
            MetaDescription = "Get 50% off all summer items",
            MetaKeywords = "sale, summer, discount",
            Template = LandingPageTemplate.ProductShowcase,
            Status = LandingPageStatus.Published,
            ContentJson = "{\"blocks\":[]}",
            HtmlContent = "<html><body>Content</body></html>",
            CustomCss = ".hero { background: blue; }",
            CustomJs = "console.log('loaded');",
            FeaturedImageUrl = "https://example.com/image.jpg",
            FacebookPixelId = "123456789",
            GoogleAnalyticsId = "G-XXXXXXXXXX",
            TrackingCode = "<script>tracking</script>",
            FormDefinitionId = 1,
            CampaignId = 2,
            ThankYouPageId = 3,
            RedirectUrl = "https://example.com/thank-you",
            CreatedByUserId = 1,
            PublishedAt = publishDate,
            ScheduledPublishAt = schedulePublish,
            ScheduledUnpublishAt = scheduleUnpublish,
            ABTestVariant = "A",
            OriginalPageId = null,
            ABTestTrafficPercentage = 50,
            PageViews = 1000,
            UniqueVisitors = 800,
            Conversions = 100,
            AverageTimeOnPage = 45.5,
            BounceRate = 30.5m,
            SettingsJson = "{\"theme\":\"light\"}"
        };

        // Assert
        page.Name.Should().Be("Summer Sale Landing Page");
        page.Slug.Should().Be("summer-sale-2025");
        page.Title.Should().Be("50% Off Summer Sale");
        page.MetaDescription.Should().Be("Get 50% off all summer items");
        page.MetaKeywords.Should().Be("sale, summer, discount");
        page.Template.Should().Be(LandingPageTemplate.ProductShowcase);
        page.Status.Should().Be(LandingPageStatus.Published);
        page.ContentJson.Should().Be("{\"blocks\":[]}");
        page.HtmlContent.Should().Contain("<body>");
        page.CustomCss.Should().Contain(".hero");
        page.CustomJs.Should().Contain("console.log");
        page.FeaturedImageUrl.Should().Be("https://example.com/image.jpg");
        page.FacebookPixelId.Should().Be("123456789");
        page.GoogleAnalyticsId.Should().Be("G-XXXXXXXXXX");
        page.TrackingCode.Should().Contain("tracking");
        page.FormDefinitionId.Should().Be(1);
        page.CampaignId.Should().Be(2);
        page.ThankYouPageId.Should().Be(3);
        page.RedirectUrl.Should().Be("https://example.com/thank-you");
        page.CreatedByUserId.Should().Be(1);
        page.PublishedAt.Should().Be(publishDate);
        page.ScheduledPublishAt.Should().Be(schedulePublish);
        page.ScheduledUnpublishAt.Should().Be(scheduleUnpublish);
        page.ABTestVariant.Should().Be("A");
        page.ABTestTrafficPercentage.Should().Be(50);
        page.PageViews.Should().Be(1000);
        page.UniqueVisitors.Should().Be(800);
        page.Conversions.Should().Be(100);
        page.AverageTimeOnPage.Should().Be(45.5);
        page.BounceRate.Should().Be(30.5m);
        page.SettingsJson.Should().Be("{\"theme\":\"light\"}");
    }

    [Fact]
    public void LandingPage_ConversionRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var page = new LandingPage
        {
            UniqueVisitors = 500,
            Conversions = 50
        };

        // Act & Assert
        page.ConversionRate.Should().Be(10m); // 50/500 * 100 = 10%
    }

    [Fact]
    public void LandingPage_ConversionRate_ShouldReturnZeroWhenNoVisitors()
    {
        // Arrange
        var page = new LandingPage
        {
            UniqueVisitors = 0,
            Conversions = 0
        };

        // Act & Assert
        page.ConversionRate.Should().Be(0);
    }

    [Fact]
    public void LandingPage_ShouldSupportAllTemplates()
    {
        // Act & Assert
        foreach (var template in Enum.GetValues<LandingPageTemplate>())
        {
            var page = new LandingPage { Template = template };
            page.Template.Should().Be(template);
        }
    }

    #endregion

    #region LandingPageBlock Entity Tests

    [Fact]
    public void LandingPageBlock_ShouldInitializeWithDefaults()
    {
        // Act
        var block = new LandingPageBlock();

        // Assert
        block.LandingPageId.Should().Be(0);
        block.BlockType.Should().Be(LandingPageBlockType.Hero);
        block.SortOrder.Should().Be(0);
        block.IsVisible.Should().BeTrue();
    }

    [Fact]
    public void LandingPageBlock_ShouldSetPropertiesCorrectly()
    {
        // Act
        var block = new LandingPageBlock
        {
            LandingPageId = 1,
            BlockType = LandingPageBlockType.Features,
            SortOrder = 3,
            ContentJson = "{\"features\":[{\"title\":\"Feature 1\"}]}",
            StyleJson = "{\"backgroundColor\":\"#fff\"}",
            VisibilityCondition = "desktop-only",
            IsVisible = true
        };

        // Assert
        block.LandingPageId.Should().Be(1);
        block.BlockType.Should().Be(LandingPageBlockType.Features);
        block.SortOrder.Should().Be(3);
        block.ContentJson.Should().Contain("Feature 1");
        block.StyleJson.Should().Contain("backgroundColor");
        block.VisibilityCondition.Should().Be("desktop-only");
        block.IsVisible.Should().BeTrue();
    }

    [Fact]
    public void LandingPageBlock_ShouldSupportAllBlockTypes()
    {
        // Act & Assert
        foreach (var blockType in Enum.GetValues<LandingPageBlockType>())
        {
            var block = new LandingPageBlock { BlockType = blockType };
            block.BlockType.Should().Be(blockType);
        }
    }

    #endregion

    #region LandingPageVisit Entity Tests

    [Fact]
    public void LandingPageVisit_ShouldInitializeWithDefaults()
    {
        // Act
        var visit = new LandingPageVisit();

        // Assert
        visit.LandingPageId.Should().Be(0);
        visit.Converted.Should().BeFalse();
        visit.VisitedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void LandingPageVisit_ShouldSetTrackingPropertiesCorrectly()
    {
        // Arrange
        var visitTime = DateTime.UtcNow;

        // Act
        var visit = new LandingPageVisit
        {
            LandingPageId = 1,
            VisitorId = "visitor_123",
            IpAddressHash = "abc123hash",
            UserAgent = "Mozilla/5.0 Chrome/120",
            Referrer = "https://google.com",
            UtmSource = "google",
            UtmMedium = "cpc",
            UtmCampaign = "summer-sale",
            UtmTerm = "discount",
            UtmContent = "banner_1",
            VisitedAt = visitTime,
            TimeOnPageSeconds = 120,
            DeviceType = "desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows 11",
            Country = "United States",
            City = "New York"
        };

        // Assert
        visit.LandingPageId.Should().Be(1);
        visit.VisitorId.Should().Be("visitor_123");
        visit.IpAddressHash.Should().Be("abc123hash");
        visit.UserAgent.Should().Contain("Chrome");
        visit.Referrer.Should().Be("https://google.com");
        visit.UtmSource.Should().Be("google");
        visit.UtmMedium.Should().Be("cpc");
        visit.UtmCampaign.Should().Be("summer-sale");
        visit.UtmTerm.Should().Be("discount");
        visit.UtmContent.Should().Be("banner_1");
        visit.VisitedAt.Should().Be(visitTime);
        visit.TimeOnPageSeconds.Should().Be(120);
        visit.DeviceType.Should().Be("desktop");
        visit.Browser.Should().Be("Chrome");
        visit.OperatingSystem.Should().Be("Windows 11");
        visit.Country.Should().Be("United States");
        visit.City.Should().Be("New York");
    }

    [Fact]
    public void LandingPageVisit_ShouldTrackConversion()
    {
        // Arrange
        var conversionTime = DateTime.UtcNow;

        // Act
        var visit = new LandingPageVisit
        {
            Converted = true,
            ConvertedAt = conversionTime,
            LeadId = 123
        };

        // Assert
        visit.Converted.Should().BeTrue();
        visit.ConvertedAt.Should().Be(conversionTime);
        visit.LeadId.Should().Be(123);
    }

    #endregion

    #region Lead Score Rule Enum Tests

    [Fact]
    public void LeadScoreRuleType_ShouldHaveCorrectValues()
    {
        // Assert
        ((int)LeadScoreRuleType.Attribute).Should().Be(0);
        ((int)LeadScoreRuleType.Behavior).Should().Be(1);
        ((int)LeadScoreRuleType.Decay).Should().Be(2);
        ((int)LeadScoreRuleType.Demographic).Should().Be(3);
        ((int)LeadScoreRuleType.FitScore).Should().Be(4);
    }

    [Fact]
    public void LeadScoreRuleType_ShouldHaveFiveValues()
    {
        // Assert
        Enum.GetValues<LeadScoreRuleType>().Should().HaveCount(5);
    }

    [Theory]
    [InlineData(LeadScoreRuleType.Attribute, "Attribute")]
    [InlineData(LeadScoreRuleType.Behavior, "Behavior")]
    [InlineData(LeadScoreRuleType.Decay, "Decay")]
    [InlineData(LeadScoreRuleType.Demographic, "Demographic")]
    [InlineData(LeadScoreRuleType.FitScore, "FitScore")]
    public void LeadScoreRuleType_ShouldHaveCorrectNames(LeadScoreRuleType type, string expectedName)
    {
        // Assert
        type.ToString().Should().Be(expectedName);
    }

    [Fact]
    public void RuleOperator_ShouldHaveCorrectValues()
    {
        // Assert
        ((int)RuleOperator.Equals).Should().Be(0);
        ((int)RuleOperator.NotEquals).Should().Be(1);
        ((int)RuleOperator.Contains).Should().Be(2);
        ((int)RuleOperator.NotContains).Should().Be(3);
        ((int)RuleOperator.GreaterThan).Should().Be(4);
        ((int)RuleOperator.LessThan).Should().Be(5);
        ((int)RuleOperator.GreaterThanOrEquals).Should().Be(6);
        ((int)RuleOperator.LessThanOrEquals).Should().Be(7);
        ((int)RuleOperator.IsEmpty).Should().Be(8);
        ((int)RuleOperator.IsNotEmpty).Should().Be(9);
        ((int)RuleOperator.In).Should().Be(10);
        ((int)RuleOperator.NotIn).Should().Be(11);
    }

    [Fact]
    public void RuleOperator_ShouldHaveTwelveValues()
    {
        // Assert
        Enum.GetValues<RuleOperator>().Should().HaveCount(12);
    }

    #endregion

    #region LeadScoreRule Entity Tests

    [Fact]
    public void LeadScoreRule_ShouldInitializeWithDefaults()
    {
        // Act
        var rule = new LeadScoreRule();

        // Assert
        rule.Name.Should().BeEmpty();
        rule.RuleType.Should().Be(LeadScoreRuleType.Attribute);
        rule.Operator.Should().Be(RuleOperator.Equals);
        rule.ScoreImpact.Should().Be(10);
        rule.DecayPeriodDays.Should().Be(7);
        rule.IsActive.Should().BeTrue();
        rule.Priority.Should().Be(100);
    }

    [Fact]
    public void LeadScoreRule_ShouldSetAttributeRulePropertiesCorrectly()
    {
        // Act
        var rule = new LeadScoreRule
        {
            Name = "Director Title Bonus",
            Description = "Award bonus points to leads with Director titles",
            RuleType = LeadScoreRuleType.Attribute,
            FieldName = "JobTitle",
            Operator = RuleOperator.Contains,
            Value = "Director",
            ScoreImpact = 20,
            MaxApplications = 1,
            IsActive = true,
            Priority = 10,
            Category = "Demographics"
        };

        // Assert
        rule.Name.Should().Be("Director Title Bonus");
        rule.Description.Should().Contain("bonus points");
        rule.RuleType.Should().Be(LeadScoreRuleType.Attribute);
        rule.FieldName.Should().Be("JobTitle");
        rule.Operator.Should().Be(RuleOperator.Contains);
        rule.Value.Should().Be("Director");
        rule.ScoreImpact.Should().Be(20);
        rule.MaxApplications.Should().Be(1);
        rule.IsActive.Should().BeTrue();
        rule.Priority.Should().Be(10);
        rule.Category.Should().Be("Demographics");
    }

    [Fact]
    public void LeadScoreRule_ShouldSetDecayRulePropertiesCorrectly()
    {
        // Act
        var rule = new LeadScoreRule
        {
            Name = "Inactivity Decay",
            RuleType = LeadScoreRuleType.Decay,
            DecayDaysThreshold = 30,
            DecayPointsPerPeriod = 5,
            DecayPeriodDays = 7,
            ScoreImpact = -5
        };

        // Assert
        rule.Name.Should().Be("Inactivity Decay");
        rule.RuleType.Should().Be(LeadScoreRuleType.Decay);
        rule.DecayDaysThreshold.Should().Be(30);
        rule.DecayPointsPerPeriod.Should().Be(5);
        rule.DecayPeriodDays.Should().Be(7);
        rule.ScoreImpact.Should().Be(-5);
    }

    [Fact]
    public void LeadScoreRule_ShouldSupportComplexConditions()
    {
        // Arrange
        var conditionsJson = @"[
            {""field"": ""JobTitle"", ""operator"": ""contains"", ""value"": ""VP""},
            {""field"": ""Industry"", ""operator"": ""equals"", ""value"": ""Technology""}
        ]";

        // Act
        var rule = new LeadScoreRule
        {
            Name = "Ideal Customer Profile",
            RuleType = LeadScoreRuleType.FitScore,
            ConditionsJson = conditionsJson,
            ScoreImpact = 50
        };

        // Assert
        rule.ConditionsJson.Should().Contain("VP");
        rule.ConditionsJson.Should().Contain("Technology");
    }

    [Fact]
    public void LeadScoreRule_ShouldSupportAllRuleTypes()
    {
        // Act & Assert
        foreach (var ruleType in Enum.GetValues<LeadScoreRuleType>())
        {
            var rule = new LeadScoreRule { RuleType = ruleType };
            rule.RuleType.Should().Be(ruleType);
        }
    }

    [Fact]
    public void LeadScoreRule_ShouldSupportAllOperators()
    {
        // Act & Assert
        foreach (var op in Enum.GetValues<RuleOperator>())
        {
            var rule = new LeadScoreRule { Operator = op };
            rule.Operator.Should().Be(op);
        }
    }

    #endregion

    #region Event Attendee Enum Tests

    [Fact]
    public void AttendeeType_ShouldHaveCorrectValues()
    {
        // Assert
        ((int)AttendeeType.User).Should().Be(0);
        ((int)AttendeeType.Contact).Should().Be(1);
        ((int)AttendeeType.Lead).Should().Be(2);
    }

    [Fact]
    public void AttendeeType_ShouldHaveExactlyThreeValues()
    {
        // Assert
        Enum.GetValues<AttendeeType>().Should().HaveCount(3);
    }

    [Theory]
    [InlineData(AttendeeType.User, "User")]
    [InlineData(AttendeeType.Contact, "Contact")]
    [InlineData(AttendeeType.Lead, "Lead")]
    public void AttendeeType_ShouldHaveCorrectNames(AttendeeType type, string expectedName)
    {
        // Assert
        type.ToString().Should().Be(expectedName);
    }

    [Fact]
    public void AttendeeResponseStatus_ShouldHaveCorrectValues()
    {
        // Assert
        ((int)AttendeeResponseStatus.NotResponded).Should().Be(0);
        ((int)AttendeeResponseStatus.Accepted).Should().Be(1);
        ((int)AttendeeResponseStatus.Declined).Should().Be(2);
        ((int)AttendeeResponseStatus.Tentative).Should().Be(3);
    }

    [Fact]
    public void AttendeeResponseStatus_ShouldHaveExactlyFourValues()
    {
        // Assert
        Enum.GetValues<AttendeeResponseStatus>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData(AttendeeResponseStatus.NotResponded, "NotResponded")]
    [InlineData(AttendeeResponseStatus.Accepted, "Accepted")]
    [InlineData(AttendeeResponseStatus.Declined, "Declined")]
    [InlineData(AttendeeResponseStatus.Tentative, "Tentative")]
    public void AttendeeResponseStatus_ShouldHaveCorrectNames(AttendeeResponseStatus status, string expectedName)
    {
        // Assert
        status.ToString().Should().Be(expectedName);
    }

    #endregion

    #region EventAttendee Entity Tests

    [Fact]
    public void EventAttendee_ShouldInitializeWithDefaults()
    {
        // Act
        var attendee = new EventAttendee();

        // Assert
        attendee.ActivityId.Should().Be(0);
        attendee.AttendeeType.Should().Be(AttendeeType.User);
        attendee.AttendeeId.Should().Be(0);
        attendee.ResponseStatus.Should().Be(AttendeeResponseStatus.NotResponded);
        attendee.IsOrganizer.Should().BeFalse();
        attendee.IsRequired.Should().BeTrue();
    }

    [Fact]
    public void EventAttendee_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var responseTime = DateTime.UtcNow.AddHours(-2);
        var inviteSentTime = DateTime.UtcNow.AddDays(-1);

        // Act
        var attendee = new EventAttendee
        {
            ActivityId = 1,
            AttendeeType = AttendeeType.Contact,
            AttendeeId = 100,
            AttendeeEmail = "john@company.com",
            AttendeeName = "John Smith",
            ResponseStatus = AttendeeResponseStatus.Accepted,
            RespondedAt = responseTime,
            ResponseComment = "Looking forward to the meeting",
            IsOrganizer = false,
            IsRequired = true,
            Role = "Decision Maker",
            DidAttend = true,
            AttendanceDurationMinutes = 60,
            AttendanceNotes = "Participated actively",
            ExternalCalendarEventId = "google_event_123"
        };

        // Assert
        attendee.ActivityId.Should().Be(1);
        attendee.AttendeeType.Should().Be(AttendeeType.Contact);
        attendee.AttendeeId.Should().Be(100);
        attendee.AttendeeEmail.Should().Be("john@company.com");
        attendee.AttendeeName.Should().Be("John Smith");
        attendee.ResponseStatus.Should().Be(AttendeeResponseStatus.Accepted);
        attendee.RespondedAt.Should().Be(responseTime);
        attendee.ResponseComment.Should().Be("Looking forward to the meeting");
        attendee.IsOrganizer.Should().BeFalse();
        attendee.IsRequired.Should().BeTrue();
        attendee.Role.Should().Be("Decision Maker");
        attendee.DidAttend.Should().BeTrue();
        attendee.AttendanceDurationMinutes.Should().Be(60);
        attendee.AttendanceNotes.Should().Be("Participated actively");
        attendee.ExternalCalendarEventId.Should().Be("google_event_123");
    }

    [Fact]
    public void EventAttendee_ShouldSupportUserAttendeeType()
    {
        // Act
        var attendee = new EventAttendee
        {
            AttendeeType = AttendeeType.User,
            AttendeeId = 1,
            AttendeeName = "Internal User"
        };

        // Assert
        attendee.AttendeeType.Should().Be(AttendeeType.User);
    }

    [Fact]
    public void EventAttendee_ShouldSupportLeadAttendeeType()
    {
        // Act
        var attendee = new EventAttendee
        {
            AttendeeType = AttendeeType.Lead,
            AttendeeId = 50,
            AttendeeName = "Potential Customer"
        };

        // Assert
        attendee.AttendeeType.Should().Be(AttendeeType.Lead);
    }

    [Fact]
    public void EventAttendee_ShouldTrackPartialAttendance()
    {
        // Act
        var attendee = new EventAttendee
        {
            DidAttend = true,
            AttendanceDurationMinutes = 30, // Partial attendance
            AttendanceNotes = "Left early due to conflict"
        };

        // Assert
        attendee.DidAttend.Should().BeTrue();
        attendee.AttendanceDurationMinutes.Should().Be(30);
        attendee.AttendanceNotes.Should().Contain("early");
    }

    [Fact]
    public void EventAttendee_ShouldSupportAllResponseStatuses()
    {
        // Act & Assert
        foreach (var status in Enum.GetValues<AttendeeResponseStatus>())
        {
            var attendee = new EventAttendee { ResponseStatus = status };
            attendee.ResponseStatus.Should().Be(status);
        }
    }

    [Fact]
    public void EventAttendee_OrganizerFlag_ShouldBeSettable()
    {
        // Act
        var organizer = new EventAttendee
        {
            AttendeeType = AttendeeType.User,
            AttendeeId = 1,
            IsOrganizer = true,
            IsRequired = true
        };

        // Assert
        organizer.IsOrganizer.Should().BeTrue();
        organizer.IsRequired.Should().BeTrue();
    }

    [Fact]
    public void EventAttendee_OptionalAttendee_ShouldBeSettable()
    {
        // Act
        var optionalAttendee = new EventAttendee
        {
            IsOrganizer = false,
            IsRequired = false
        };

        // Assert
        optionalAttendee.IsOrganizer.Should().BeFalse();
        optionalAttendee.IsRequired.Should().BeFalse();
    }

    #endregion
}
