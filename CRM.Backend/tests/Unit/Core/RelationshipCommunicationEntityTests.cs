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
/// Tests for Relationship and Communication entity classes
/// </summary>
public class RelationshipCommunicationEntityTests
{
    #region Enum Tests - RelationshipStatus

    [Fact]
    public void RelationshipStatus_ShouldHave4Values()
    {
        Enum.GetValues<RelationshipStatus>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData(RelationshipStatus.Active, 0)]
    [InlineData(RelationshipStatus.Inactive, 1)]
    [InlineData(RelationshipStatus.Pending, 2)]
    [InlineData(RelationshipStatus.Terminated, 3)]
    public void RelationshipStatus_ShouldHaveCorrectValues(RelationshipStatus status, int expected)
    {
        ((int)status).Should().Be(expected);
    }

    #endregion

    #region Enum Tests - StrategicImportance

    [Fact]
    public void StrategicImportance_ShouldHave4Values()
    {
        Enum.GetValues<StrategicImportance>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData(StrategicImportance.Low, 0)]
    [InlineData(StrategicImportance.Medium, 1)]
    [InlineData(StrategicImportance.High, 2)]
    [InlineData(StrategicImportance.Critical, 3)]
    public void StrategicImportance_ShouldHaveCorrectValues(StrategicImportance importance, int expected)
    {
        ((int)importance).Should().Be(expected);
    }

    #endregion

    #region Enum Tests - RelationshipCategory

    [Fact]
    public void RelationshipCategory_ShouldHave4Values()
    {
        Enum.GetValues<RelationshipCategory>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData(RelationshipCategory.Business, 0)]
    [InlineData(RelationshipCategory.Partnership, 1)]
    [InlineData(RelationshipCategory.Hierarchy, 2)]
    [InlineData(RelationshipCategory.Dependency, 3)]
    public void RelationshipCategory_ShouldHaveCorrectValues(RelationshipCategory category, int expected)
    {
        ((int)category).Should().Be(expected);
    }

    #endregion

    #region Enum Tests - HealthImpact

    [Fact]
    public void HealthImpact_ShouldHave4Values()
    {
        Enum.GetValues<HealthImpact>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData(HealthImpact.Positive, 0)]
    [InlineData(HealthImpact.Neutral, 1)]
    [InlineData(HealthImpact.Negative, 2)]
    [InlineData(HealthImpact.Critical, 3)]
    public void HealthImpact_ShouldHaveCorrectValues(HealthImpact impact, int expected)
    {
        ((int)impact).Should().Be(expected);
    }

    #endregion

    #region Enum Tests - ChannelType

    [Fact]
    public void ChannelType_ShouldHave6Values()
    {
        Enum.GetValues<ChannelType>().Should().HaveCount(6);
    }

    [Theory]
    [InlineData(ChannelType.Email, 0)]
    [InlineData(ChannelType.WhatsApp, 1)]
    [InlineData(ChannelType.Twitter, 2)]
    [InlineData(ChannelType.Facebook, 3)]
    [InlineData(ChannelType.SMS, 4)]
    [InlineData(ChannelType.LinkedIn, 5)]
    public void ChannelType_ShouldHaveCorrectValues(ChannelType type, int expected)
    {
        ((int)type).Should().Be(expected);
    }

    #endregion

    #region Enum Tests - ChannelStatus

    [Fact]
    public void ChannelStatus_ShouldHave5Values()
    {
        Enum.GetValues<ChannelStatus>().Should().HaveCount(5);
    }

    [Theory]
    [InlineData(ChannelStatus.NotConfigured, 0)]
    [InlineData(ChannelStatus.Configured, 1)]
    [InlineData(ChannelStatus.Connected, 2)]
    [InlineData(ChannelStatus.Error, 3)]
    [InlineData(ChannelStatus.Disabled, 4)]
    public void ChannelStatus_ShouldHaveCorrectValues(ChannelStatus status, int expected)
    {
        ((int)status).Should().Be(expected);
    }

    #endregion

    #region Enum Tests - MessageDirection

    [Fact]
    public void MessageDirection_ShouldHave2Values()
    {
        Enum.GetValues<MessageDirection>().Should().HaveCount(2);
    }

    [Theory]
    [InlineData(MessageDirection.Outbound, 0)]
    [InlineData(MessageDirection.Inbound, 1)]
    public void MessageDirection_ShouldHaveCorrectValues(MessageDirection direction, int expected)
    {
        ((int)direction).Should().Be(expected);
    }

    #endregion

    #region Enum Tests - MessageStatus

    [Fact]
    public void MessageStatus_ShouldHave10Values()
    {
        Enum.GetValues<MessageStatus>().Should().HaveCount(10);
    }

    [Theory]
    [InlineData(MessageStatus.Draft, 0)]
    [InlineData(MessageStatus.Queued, 1)]
    [InlineData(MessageStatus.Sending, 2)]
    [InlineData(MessageStatus.Sent, 3)]
    [InlineData(MessageStatus.Delivered, 4)]
    [InlineData(MessageStatus.Read, 5)]
    [InlineData(MessageStatus.Failed, 6)]
    [InlineData(MessageStatus.Bounced, 7)]
    [InlineData(MessageStatus.Replied, 8)]
    [InlineData(MessageStatus.Deleted, 9)]
    public void MessageStatus_ShouldHaveCorrectValues(MessageStatus status, int expected)
    {
        ((int)status).Should().Be(expected);
    }

    #endregion

    #region Enum Tests - MessagePriority

    [Fact]
    public void MessagePriority_ShouldHave4Values()
    {
        Enum.GetValues<MessagePriority>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData(MessagePriority.Low, 0)]
    [InlineData(MessagePriority.Normal, 1)]
    [InlineData(MessagePriority.High, 2)]
    [InlineData(MessagePriority.Urgent, 3)]
    public void MessagePriority_ShouldHaveCorrectValues(MessagePriority priority, int expected)
    {
        ((int)priority).Should().Be(expected);
    }

    #endregion

    #region AccountRelationship Entity Tests

    [Fact]
    public void AccountRelationship_ShouldInitializeWithDefaults()
    {
        var relationship = new AccountRelationship();

        relationship.SourceAccountId.Should().Be(0);
        relationship.TargetAccountId.Should().Be(0);
        relationship.RelationshipTypeId.Should().Be(0);
        relationship.StrengthScore.Should().Be(50); // Default middle strength
        relationship.StrategicImportance.Should().Be("Medium"); // Default string value
        relationship.Notes.Should().BeNullOrEmpty();
        relationship.Interactions.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void AccountRelationship_ShouldSetSourceAndTargetAccounts()
    {
        var relationship = new AccountRelationship
        {
            SourceAccountId = 100,
            TargetAccountId = 200,
            RelationshipTypeId = 5
        };

        relationship.SourceAccountId.Should().Be(100);
        relationship.TargetAccountId.Should().Be(200);
        relationship.RelationshipTypeId.Should().Be(5);
    }

    [Fact]
    public void AccountRelationship_ShouldTrackStrengthAndImportance()
    {
        var relationship = new AccountRelationship
        {
            StrengthScore = 85,
            StrategicImportance = "Critical"
        };

        relationship.StrengthScore.Should().Be(85);
        relationship.StrategicImportance.Should().Be("Critical");
    }

    [Fact]
    public void AccountRelationship_ShouldTrackDates()
    {
        var startDate = DateTime.UtcNow.AddYears(-1);
        var lastReviewDate = DateTime.UtcNow.AddDays(-30);
        var nextReviewDate = DateTime.UtcNow.AddDays(60);
        var endDate = DateTime.UtcNow;

        var relationship = new AccountRelationship
        {
            RelationshipStartDate = startDate,
            LastReviewedDate = lastReviewDate,
            NextReviewDate = nextReviewDate,
            RelationshipEndDate = endDate
        };

        relationship.RelationshipStartDate.Should().Be(startDate);
        relationship.LastReviewedDate.Should().Be(lastReviewDate);
        relationship.NextReviewDate.Should().Be(nextReviewDate);
        relationship.RelationshipEndDate.Should().Be(endDate);
    }

    [Fact]
    public void AccountRelationship_ShouldHaveInteractionsCollection()
    {
        var relationship = new AccountRelationship();
        var interaction = new RelationshipInteraction
        {
            InteractionType = "Meeting",
            Subject = "Quarterly Review"
        };

        relationship.Interactions.Add(interaction);

        relationship.Interactions.Should().HaveCount(1);
        relationship.Interactions.First().InteractionType.Should().Be("Meeting");
    }

    #endregion

    #region RelationshipType Entity Tests

    [Fact]
    public void RelationshipType_ShouldInitializeWithDefaults()
    {
        var relType = new RelationshipType();

        relType.TypeName.Should().BeEmpty();
        relType.TypeCategory.Should().BeNull();
        relType.Description.Should().BeNull();
        relType.IsBidirectional.Should().BeFalse();
        relType.ReverseTypeName.Should().BeNull();
        relType.Icon.Should().BeNull();
        relType.Color.Should().BeNull();
        relType.IsActive.Should().BeTrue();
        relType.IsSystem.Should().BeFalse();
        relType.DisplayOrder.Should().Be(0);
        relType.Relationships.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void RelationshipType_ShouldSetTypeNameAndCategory()
    {
        var relType = new RelationshipType
        {
            TypeName = "Partner",
            TypeCategory = "Business",
            Description = "Business partnership relationship"
        };

        relType.TypeName.Should().Be("Partner");
        relType.TypeCategory.Should().Be("Business");
        relType.Description.Should().Contain("partnership");
    }

    [Fact]
    public void RelationshipType_ShouldConfigureBidirectionalRelationship()
    {
        var relType = new RelationshipType
        {
            TypeName = "Parent Company",
            IsBidirectional = true,
            ReverseTypeName = "Subsidiary"
        };

        relType.IsBidirectional.Should().BeTrue();
        relType.ReverseTypeName.Should().Be("Subsidiary");
    }

    [Fact]
    public void RelationshipType_ShouldSetUIProperties()
    {
        var relType = new RelationshipType
        {
            Icon = "link",
            Color = "#FF5733",
            DisplayOrder = 5
        };

        relType.Icon.Should().Be("link");
        relType.Color.Should().Be("#FF5733");
        relType.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void RelationshipType_ShouldDistinguishSystemTypes()
    {
        var systemType = new RelationshipType
        {
            TypeName = "Parent",
            IsSystem = true,
            IsActive = true
        };

        var customType = new RelationshipType
        {
            TypeName = "Referral Partner",
            IsSystem = false,
            IsActive = true
        };

        systemType.IsSystem.Should().BeTrue();
        customType.IsSystem.Should().BeFalse();
    }

    #endregion

    #region RelationshipInteraction Entity Tests

    [Fact]
    public void RelationshipInteraction_ShouldInitializeWithDefaults()
    {
        var interaction = new RelationshipInteraction();

        interaction.AccountRelationshipId.Should().Be(0);
        interaction.InteractionType.Should().BeEmpty();
        interaction.Subject.Should().BeNull();
        interaction.Description.Should().BeNull();
        interaction.DurationMinutes.Should().BeNull();
        interaction.SentimentScore.Should().Be(0);
        interaction.HealthImpact.Should().Be("Neutral");
        interaction.Location.Should().BeNull();
        interaction.MeetingLink.Should().BeNull();
    }

    [Fact]
    public void RelationshipInteraction_ShouldSetInteractionDetails()
    {
        var interactionDate = DateTime.UtcNow;
        var interaction = new RelationshipInteraction
        {
            AccountRelationshipId = 10,
            InteractionType = "Meeting",
            Subject = "Partnership Discussion",
            Description = "Discussed mutual business opportunities",
            InteractionDate = interactionDate,
            DurationMinutes = 60
        };

        interaction.InteractionType.Should().Be("Meeting");
        interaction.Subject.Should().Be("Partnership Discussion");
        interaction.DurationMinutes.Should().Be(60);
    }

    [Fact]
    public void RelationshipInteraction_ShouldTrackParticipants()
    {
        var interaction = new RelationshipInteraction
        {
            ParticipantContactIds = "[1, 2, 3]",
            ParticipantUserIds = "[10, 20]"
        };

        interaction.ParticipantContactIds.Should().Contain("1");
        interaction.ParticipantUserIds.Should().Contain("10");
    }

    [Fact]
    public void RelationshipInteraction_ShouldTrackOutcomeAndFollowUp()
    {
        var followUpDate = DateTime.UtcNow.AddDays(7);
        var interaction = new RelationshipInteraction
        {
            Outcome = "Positive - agreement reached",
            ActionItems = "Draft contract, Schedule follow-up",
            NextSteps = "Send proposal by Friday",
            FollowUpDate = followUpDate
        };

        interaction.Outcome.Should().Contain("agreement");
        interaction.ActionItems.Should().Contain("Draft contract");
        interaction.NextSteps.Should().Contain("proposal");
        interaction.FollowUpDate.Should().Be(followUpDate);
    }

    [Fact]
    public void RelationshipInteraction_ShouldTrackSentimentAndImpact()
    {
        var interaction = new RelationshipInteraction
        {
            SentimentScore = 75,
            HealthImpact = "Positive"
        };

        interaction.SentimentScore.Should().Be(75);
        interaction.HealthImpact.Should().Be("Positive");
    }

    [Fact]
    public void RelationshipInteraction_ShouldSetMeetingDetails()
    {
        var interaction = new RelationshipInteraction
        {
            Location = "Conference Room A",
            MeetingLink = "https://teams.microsoft.com/meeting/123"
        };

        interaction.Location.Should().Contain("Conference");
        interaction.MeetingLink.Should().Contain("teams.microsoft.com");
    }

    #endregion

    #region RelationshipMap Entity Tests

    [Fact]
    public void RelationshipMap_ShouldInitializeWithDefaults()
    {
        var map = new RelationshipMap();

        map.MapName.Should().BeEmpty();
        map.Description.Should().BeNull();
        map.CentralAccountId.Should().BeNull();
        map.RelationshipDepth.Should().Be(2); // Default depth
        map.MinRelationshipStrength.Should().Be(0);
        map.IsPublic.Should().BeFalse();
        map.LayoutConfig.Should().BeNull();
        map.ViewSettings.Should().BeNull();
    }

    [Fact]
    public void RelationshipMap_ShouldSetMapConfiguration()
    {
        var map = new RelationshipMap
        {
            MapName = "Enterprise Network",
            Description = "Map of all enterprise relationships",
            CentralAccountId = 100,
            RelationshipDepth = 3
        };

        map.MapName.Should().Be("Enterprise Network");
        map.CentralAccountId.Should().Be(100);
        map.RelationshipDepth.Should().Be(3);
    }

    [Fact]
    public void RelationshipMap_ShouldSetFilters()
    {
        var startDate = DateTime.UtcNow.AddYears(-1);
        var endDate = DateTime.UtcNow;

        var map = new RelationshipMap
        {
            IncludeRelationshipTypeIds = "[1, 2, 3]",
            ExcludeRelationshipTypeIds = "[5]",
            MinRelationshipStrength = 50,
            IncludeStatuses = "[\"Active\", \"Pending\"]",
            DateRangeStart = startDate,
            DateRangeEnd = endDate
        };

        map.IncludeRelationshipTypeIds.Should().Contain("1");
        map.ExcludeRelationshipTypeIds.Should().Contain("5");
        map.MinRelationshipStrength.Should().Be(50);
        map.DateRangeStart.Should().Be(startDate);
    }

    [Fact]
    public void RelationshipMap_ShouldSetVisualizationSettings()
    {
        var map = new RelationshipMap
        {
            LayoutConfig = "{\"nodeSize\": 50, \"spacing\": 100}",
            ViewSettings = "{\"zoom\": 1.5, \"panX\": 0, \"panY\": 0}"
        };

        map.LayoutConfig.Should().Contain("nodeSize");
        map.ViewSettings.Should().Contain("zoom");
    }

    [Fact]
    public void RelationshipMap_ShouldConfigureSharing()
    {
        var map = new RelationshipMap
        {
            IsPublic = false,
            SharedWithUserIds = "[10, 20, 30]",
            SharedWithGroupIds = "[1, 2]"
        };

        map.IsPublic.Should().BeFalse();
        map.SharedWithUserIds.Should().Contain("10");
        map.SharedWithGroupIds.Should().Contain("1");
    }

    #endregion

    #region AccountTerritory Entity Tests

    [Fact]
    public void AccountTerritory_ShouldInitializeWithDefaults()
    {
        var territory = new AccountTerritory();

        territory.TerritoryName.Should().BeEmpty();
        territory.TerritoryCode.Should().BeNull();
        territory.Description.Should().BeNull();
        territory.Countries.Should().BeNull();
        territory.QuotaCurrency.Should().Be("USD");
        territory.IsActive.Should().BeTrue();
        territory.AccountAssignments.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void AccountTerritory_ShouldSetTerritoryIdentification()
    {
        var territory = new AccountTerritory
        {
            TerritoryName = "North America",
            TerritoryCode = "NA-001",
            Description = "All North American accounts"
        };

        territory.TerritoryName.Should().Be("North America");
        territory.TerritoryCode.Should().Be("NA-001");
        territory.Description.Should().Contain("North American");
    }

    [Fact]
    public void AccountTerritory_ShouldSetGeographicFilters()
    {
        var territory = new AccountTerritory
        {
            Countries = "[\"USA\", \"Canada\", \"Mexico\"]",
            Regions = "[\"West Coast\"]",
            States = "[\"California\", \"Washington\", \"Oregon\"]",
            Cities = "[\"San Francisco\", \"Seattle\", \"Portland\"]"
        };

        territory.Countries.Should().Contain("USA");
        territory.Regions.Should().Contain("West Coast");
        territory.States.Should().Contain("California");
        territory.Cities.Should().Contain("San Francisco");
    }

    [Fact]
    public void AccountTerritory_ShouldSetBusinessFilters()
    {
        var territory = new AccountTerritory
        {
            Industries = "[\"Technology\", \"Healthcare\"]",
            CustomerTypes = "[\"Enterprise\", \"Mid-Market\"]",
            RevenueRangeMin = 1000000,
            RevenueRangeMax = 50000000
        };

        territory.Industries.Should().Contain("Technology");
        territory.CustomerTypes.Should().Contain("Enterprise");
        territory.RevenueRangeMin.Should().Be(1000000);
        territory.RevenueRangeMax.Should().Be(50000000);
    }

    [Fact]
    public void AccountTerritory_ShouldSetQuotaAndTargets()
    {
        var territory = new AccountTerritory
        {
            PrimaryOwnerId = 10,
            TeamMemberIds = "[10, 20, 30]",
            AnnualQuota = 5000000,
            QuotaCurrency = "EUR",
            TargetAccountCount = 50
        };

        territory.PrimaryOwnerId.Should().Be(10);
        territory.TeamMemberIds.Should().Contain("20");
        territory.AnnualQuota.Should().Be(5000000);
        territory.QuotaCurrency.Should().Be("EUR");
        territory.TargetAccountCount.Should().Be(50);
    }

    #endregion

    #region AccountTerritoryAssignment Entity Tests

    [Fact]
    public void AccountTerritoryAssignment_ShouldInitializeWithDefaults()
    {
        var assignment = new AccountTerritoryAssignment();

        assignment.AccountId.Should().Be(0);
        assignment.TerritoryId.Should().Be(0);
        assignment.IsPrimary.Should().BeTrue(); // Default is primary
        assignment.AssignedBy.Should().BeNull();
        assignment.Notes.Should().BeNull();
    }

    [Fact]
    public void AccountTerritoryAssignment_ShouldSetAssignment()
    {
        var assignedDate = DateTime.UtcNow;
        var assignment = new AccountTerritoryAssignment
        {
            AccountId = 100,
            TerritoryId = 5,
            AssignedDate = assignedDate,
            IsPrimary = true,
            AssignedBy = 10,
            Notes = "Reassigned due to territory restructuring"
        };

        assignment.AccountId.Should().Be(100);
        assignment.TerritoryId.Should().Be(5);
        assignment.IsPrimary.Should().BeTrue();
        assignment.AssignedBy.Should().Be(10);
        assignment.Notes.Should().Contain("restructuring");
    }

    #endregion

    #region CommunicationChannel Entity Tests

    [Fact]
    public void CommunicationChannel_ShouldInitializeWithDefaults()
    {
        var channel = new CommunicationChannel();

        channel.ChannelType.Should().Be(ChannelType.Email); // Enum default is 0 = Email
        channel.Name.Should().BeEmpty();
        channel.Status.Should().Be(ChannelStatus.NotConfigured);
        channel.IsEnabled.Should().BeTrue();
        channel.IsDefault.Should().BeFalse();
        channel.SmtpUseSsl.Should().BeTrue();
        channel.ImapUseSsl.Should().BeTrue();
        channel.WebhookEnabled.Should().BeFalse();
        channel.Messages.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void CommunicationChannel_ShouldSetBasicProperties()
    {
        var channel = new CommunicationChannel
        {
            ChannelType = ChannelType.WhatsApp,
            Name = "Main WhatsApp Business",
            Status = ChannelStatus.Connected,
            IsEnabled = true,
            IsDefault = true
        };

        channel.ChannelType.Should().Be(ChannelType.WhatsApp);
        channel.Name.Should().Be("Main WhatsApp Business");
        channel.Status.Should().Be(ChannelStatus.Connected);
        channel.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void CommunicationChannel_ShouldSetApiCredentials()
    {
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var channel = new CommunicationChannel
        {
            ApiKey = "api_key_123",
            ApiSecret = "secret_abc",
            ClientId = "client_id",
            ClientSecret = "client_secret",
            AccessToken = "access_token_xyz",
            RefreshToken = "refresh_token_abc",
            TokenExpiresAt = expiresAt
        };

        channel.ApiKey.Should().Be("api_key_123");
        channel.ApiSecret.Should().Be("secret_abc");
        channel.AccessToken.Should().Be("access_token_xyz");
        channel.TokenExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void CommunicationChannel_ShouldSetEmailSettings()
    {
        var channel = new CommunicationChannel
        {
            ChannelType = ChannelType.Email,
            SmtpServer = "smtp.example.com",
            SmtpPort = 587,
            SmtpUseSsl = true,
            SmtpUsername = "user@example.com",
            SmtpPassword = "encrypted_password",
            ImapServer = "imap.example.com",
            ImapPort = 993,
            ImapUseSsl = true,
            FromEmail = "noreply@example.com",
            FromName = "CRM System"
        };

        channel.SmtpServer.Should().Be("smtp.example.com");
        channel.SmtpPort.Should().Be(587);
        channel.ImapPort.Should().Be(993);
        channel.FromEmail.Should().Be("noreply@example.com");
    }

    [Fact]
    public void CommunicationChannel_ShouldSetWhatsAppSettings()
    {
        var channel = new CommunicationChannel
        {
            ChannelType = ChannelType.WhatsApp,
            WhatsAppBusinessAccountId = "wa_account_123",
            WhatsAppPhoneNumberId = "phone_123",
            WhatsAppVerifyToken = "verify_token_abc"
        };

        channel.WhatsAppBusinessAccountId.Should().Be("wa_account_123");
        channel.WhatsAppPhoneNumberId.Should().Be("phone_123");
        channel.WhatsAppVerifyToken.Should().Be("verify_token_abc");
    }

    [Fact]
    public void CommunicationChannel_ShouldSetSocialMediaSettings()
    {
        var channel = new CommunicationChannel
        {
            ChannelType = ChannelType.Facebook,
            SocialAccountId = "fb_page_123",
            SocialUsername = "company_page",
            PageAccessToken = "page_token_xyz"
        };

        channel.SocialAccountId.Should().Be("fb_page_123");
        channel.SocialUsername.Should().Be("company_page");
        channel.PageAccessToken.Should().Be("page_token_xyz");
    }

    [Fact]
    public void CommunicationChannel_ShouldSetWebhookConfiguration()
    {
        var channel = new CommunicationChannel
        {
            WebhookUrl = "https://api.example.com/webhook",
            WebhookSecret = "webhook_secret_abc",
            WebhookEnabled = true
        };

        channel.WebhookUrl.Should().Be("https://api.example.com/webhook");
        channel.WebhookSecret.Should().Be("webhook_secret_abc");
        channel.WebhookEnabled.Should().BeTrue();
    }

    [Fact]
    public void CommunicationChannel_ShouldTrackConnectionStatus()
    {
        var lastConnected = DateTime.UtcNow.AddMinutes(-30);
        var channel = new CommunicationChannel
        {
            Status = ChannelStatus.Error,
            LastConnectedAt = lastConnected,
            LastError = "Connection timeout"
        };

        channel.Status.Should().Be(ChannelStatus.Error);
        channel.LastConnectedAt.Should().Be(lastConnected);
        channel.LastError.Should().Be("Connection timeout");
    }

    #endregion

    #region CommunicationMessage Entity Tests

    [Fact]
    public void CommunicationMessage_ShouldInitializeWithDefaults()
    {
        var message = new CommunicationMessage();

        message.ChannelId.Should().Be(0);
        message.ChannelType.Should().Be(ChannelType.Email); // Enum default
        message.Subject.Should().BeNull();
        message.Body.Should().BeNull();
        message.AttachmentCount.Should().Be(0);
        message.Direction.Should().Be(MessageDirection.Outbound); // Enum default
        message.Status.Should().Be(MessageStatus.Draft);
        message.Priority.Should().Be(MessagePriority.Normal);
        message.TrackOpens.Should().BeFalse();
        message.TrackClicks.Should().BeFalse();
        message.OpenCount.Should().Be(0);
        message.ClickCount.Should().Be(0);
        message.IsPublicPost.Should().BeFalse();
        message.LikeCount.Should().Be(0);
        message.ShareCount.Should().Be(0);
        message.CommentCount.Should().Be(0);
        message.RetryCount.Should().Be(0);
        message.IsArchived.Should().BeFalse();
        message.IsStarred.Should().BeFalse();
        message.IsRead.Should().BeFalse();
        message.Replies.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void CommunicationMessage_ShouldSetMessageContent()
    {
        var message = new CommunicationMessage
        {
            Subject = "Meeting Follow-up",
            Body = "Thank you for the meeting today.",
            HtmlBody = "<p>Thank you for the meeting today.</p>",
            AttachmentsJson = "[{\"name\": \"proposal.pdf\", \"size\": 1024}]",
            AttachmentCount = 1
        };

        message.Subject.Should().Be("Meeting Follow-up");
        message.Body.Should().Contain("meeting");
        message.HtmlBody.Should().Contain("<p>");
        message.AttachmentCount.Should().Be(1);
    }

    [Fact]
    public void CommunicationMessage_ShouldSetDirectionAndStatus()
    {
        var message = new CommunicationMessage
        {
            Direction = MessageDirection.Inbound,
            Status = MessageStatus.Delivered,
            Priority = MessagePriority.High
        };

        message.Direction.Should().Be(MessageDirection.Inbound);
        message.Status.Should().Be(MessageStatus.Delivered);
        message.Priority.Should().Be(MessagePriority.High);
    }

    [Fact]
    public void CommunicationMessage_ShouldSetSenderRecipientInfo()
    {
        var message = new CommunicationMessage
        {
            FromAddress = "sender@example.com",
            FromName = "Sales Team",
            ToAddress = "customer@example.com",
            ToName = "John Doe",
            CcAddresses = "[\"cc1@example.com\", \"cc2@example.com\"]",
            BccAddresses = "[\"bcc@example.com\"]",
            ReplyToAddress = "reply@example.com"
        };

        message.FromAddress.Should().Be("sender@example.com");
        message.ToAddress.Should().Be("customer@example.com");
        message.CcAddresses.Should().Contain("cc1@example.com");
    }

    [Fact]
    public void CommunicationMessage_ShouldLinkToCrmEntities()
    {
        var message = new CommunicationMessage
        {
            AccountId = 100,
            ContactId = 200,
            LeadId = 300,
            OpportunityId = 400,
            LinkedEntityType = "ServiceRequest",
            LinkedEntityId = 500
        };

        message.AccountId.Should().Be(100);
        message.ContactId.Should().Be(200);
        message.LeadId.Should().Be(300);
        message.OpportunityId.Should().Be(400);
        message.LinkedEntityType.Should().Be("ServiceRequest");
        message.LinkedEntityId.Should().Be(500);
    }

    [Fact]
    public void CommunicationMessage_ShouldSetConversationThreading()
    {
        var message = new CommunicationMessage
        {
            ConversationId = "conv_123",
            ParentMessageId = 50,
            ExternalMessageId = "msg_ext_456",
            InReplyToExternalId = "msg_ext_400"
        };

        message.ConversationId.Should().Be("conv_123");
        message.ParentMessageId.Should().Be(50);
        message.ExternalMessageId.Should().Be("msg_ext_456");
    }

    [Fact]
    public void CommunicationMessage_ShouldTrackTimestamps()
    {
        var sentAt = DateTime.UtcNow.AddMinutes(-10);
        var deliveredAt = DateTime.UtcNow.AddMinutes(-9);
        var readAt = DateTime.UtcNow.AddMinutes(-5);
        var scheduledAt = DateTime.UtcNow.AddHours(1);

        var message = new CommunicationMessage
        {
            SentAt = sentAt,
            DeliveredAt = deliveredAt,
            ReadAt = readAt,
            ReceivedAt = sentAt,
            ScheduledAt = scheduledAt
        };

        message.SentAt.Should().Be(sentAt);
        message.DeliveredAt.Should().Be(deliveredAt);
        message.ReadAt.Should().Be(readAt);
        message.ScheduledAt.Should().Be(scheduledAt);
    }

    [Fact]
    public void CommunicationMessage_ShouldSetEmailTracking()
    {
        var message = new CommunicationMessage
        {
            ChannelType = ChannelType.Email,
            EmailTemplateId = 25,
            TrackOpens = true,
            TrackClicks = true,
            OpenCount = 5,
            ClickCount = 2
        };

        message.EmailTemplateId.Should().Be(25);
        message.TrackOpens.Should().BeTrue();
        message.TrackClicks.Should().BeTrue();
        message.OpenCount.Should().Be(5);
        message.ClickCount.Should().Be(2);
    }

    [Fact]
    public void CommunicationMessage_ShouldSetSocialMediaMetrics()
    {
        var message = new CommunicationMessage
        {
            ChannelType = ChannelType.Twitter,
            SocialPostId = "tweet_123456",
            IsPublicPost = true,
            LikeCount = 100,
            ShareCount = 25,
            CommentCount = 10
        };

        message.SocialPostId.Should().Be("tweet_123456");
        message.IsPublicPost.Should().BeTrue();
        message.LikeCount.Should().Be(100);
        message.ShareCount.Should().Be(25);
        message.CommentCount.Should().Be(10);
    }

    [Fact]
    public void CommunicationMessage_ShouldSetWhatsAppFields()
    {
        var message = new CommunicationMessage
        {
            ChannelType = ChannelType.WhatsApp,
            WhatsAppMessageType = "template",
            WhatsAppTemplateName = "order_confirmation"
        };

        message.WhatsAppMessageType.Should().Be("template");
        message.WhatsAppTemplateName.Should().Be("order_confirmation");
    }

    [Fact]
    public void CommunicationMessage_ShouldHandleErrors()
    {
        var message = new CommunicationMessage
        {
            Status = MessageStatus.Failed,
            ErrorMessage = "Recipient mailbox full",
            ErrorCode = "550",
            RetryCount = 3
        };

        message.Status.Should().Be(MessageStatus.Failed);
        message.ErrorMessage.Should().Contain("mailbox");
        message.ErrorCode.Should().Be("550");
        message.RetryCount.Should().Be(3);
    }

    [Fact]
    public void CommunicationMessage_ShouldSetMetadataAndFlags()
    {
        var message = new CommunicationMessage
        {
            MetadataJson = "{\"source\": \"campaign\", \"campaignId\": 123}",
            TagsJson = "[\"important\", \"follow-up\"]",
            IsArchived = false,
            IsStarred = true,
            IsRead = true
        };

        message.MetadataJson.Should().Contain("campaign");
        message.TagsJson.Should().Contain("important");
        message.IsStarred.Should().BeTrue();
        message.IsRead.Should().BeTrue();
    }

    [Fact]
    public void CommunicationMessage_ShouldHaveRepliesCollection()
    {
        var message = new CommunicationMessage();
        var reply = new CommunicationMessage
        {
            ParentMessageId = message.Id,
            Subject = "Re: Original Subject"
        };

        message.Replies.Add(reply);

        message.Replies.Should().HaveCount(1);
        message.Replies.First().Subject.Should().StartWith("Re:");
    }

    #endregion
}
