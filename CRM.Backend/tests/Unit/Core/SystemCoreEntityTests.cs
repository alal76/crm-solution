// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using Xunit;
using FluentAssertions;
using CRM.Core.Entities;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Tests for core system entities including lookups, geography, settings, tags, notes, and activities
/// </summary>
public class SystemCoreEntityTests
{
    #region LocalityType Enum Tests

    [Theory]
    [InlineData(LocalityType.Neighborhood, 0)]
    [InlineData(LocalityType.District, 1)]
    [InlineData(LocalityType.Sector, 2)]
    [InlineData(LocalityType.Ward, 3)]
    [InlineData(LocalityType.Block, 4)]
    [InlineData(LocalityType.Zone, 5)]
    [InlineData(LocalityType.Quarter, 6)]
    [InlineData(LocalityType.Suburb, 7)]
    [InlineData(LocalityType.Village, 8)]
    [InlineData(LocalityType.Township, 9)]
    [InlineData(LocalityType.Other, 99)]
    public void LocalityType_ShouldHaveCorrectValues(LocalityType localityType, int expectedValue)
    {
        ((int)localityType).Should().Be(expectedValue);
    }

    [Fact]
    public void LocalityType_ShouldHaveExpectedCount()
    {
        Enum.GetValues<LocalityType>().Should().HaveCount(11);
    }

    [Fact]
    public void LocalityType_DefaultShouldBeNeighborhood()
    {
        default(LocalityType).Should().Be(LocalityType.Neighborhood);
    }

    #endregion

    #region Locality Entity Tests

    [Fact]
    public void Locality_ShouldInitializeWithDefaults()
    {
        var locality = new Locality();

        locality.Name.Should().BeEmpty();
        locality.AlternateName.Should().BeNull();
        locality.LocalityType.Should().Be(LocalityType.Neighborhood);
        locality.ZipCodeId.Should().BeNull();
        locality.City.Should().BeEmpty();
        locality.StateCode.Should().BeNull();
        locality.CountryCode.Should().Be("US");
        locality.Latitude.Should().BeNull();
        locality.Longitude.Should().BeNull();
        locality.IsUserCreated.Should().BeFalse();
        locality.IsActive.Should().BeTrue();
        locality.CreatedByUserId.Should().BeNull();
        locality.ZipCode.Should().BeNull();
    }

    [Fact]
    public void Locality_ShouldAllowSettingAllProperties()
    {
        var locality = new Locality
        {
            Id = 1,
            Name = "Downtown",
            AlternateName = "City Center",
            LocalityType = LocalityType.District,
            ZipCodeId = 100,
            City = "Los Angeles",
            StateCode = "CA",
            CountryCode = "US",
            Latitude = 34.0522m,
            Longitude = -118.2437m,
            IsUserCreated = true,
            IsActive = true,
            CreatedByUserId = 5
        };

        locality.Id.Should().Be(1);
        locality.Name.Should().Be("Downtown");
        locality.AlternateName.Should().Be("City Center");
        locality.LocalityType.Should().Be(LocalityType.District);
        locality.ZipCodeId.Should().Be(100);
        locality.City.Should().Be("Los Angeles");
        locality.StateCode.Should().Be("CA");
        locality.CountryCode.Should().Be("US");
        locality.Latitude.Should().Be(34.0522m);
        locality.Longitude.Should().Be(-118.2437m);
        locality.IsUserCreated.Should().BeTrue();
        locality.IsActive.Should().BeTrue();
        locality.CreatedByUserId.Should().Be(5);
    }

    [Fact]
    public void Locality_ShouldInheritFromBaseEntity()
    {
        typeof(Locality).Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void Locality_NavigationToZipCode_ShouldWork()
    {
        var zipCode = new ZipCode { Id = 1, PostalCode = "90210" };
        var locality = new Locality { ZipCodeId = 1, ZipCode = zipCode };

        locality.ZipCode.Should().BeSameAs(zipCode);
    }

    #endregion

    #region ZipCode Entity Tests

    [Fact]
    public void ZipCode_ShouldInitializeWithDefaults()
    {
        var zipCode = new ZipCode();

        zipCode.Id.Should().Be(0);
        zipCode.Country.Should().BeEmpty();
        zipCode.CountryCode.Should().BeEmpty();
        zipCode.PostalCode.Should().BeEmpty();
        zipCode.City.Should().BeEmpty();
        zipCode.State.Should().BeNull();
        zipCode.StateCode.Should().BeNull();
        zipCode.County.Should().BeNull();
        zipCode.CountyCode.Should().BeNull();
        zipCode.Community.Should().BeNull();
        zipCode.CommunityCode.Should().BeNull();
        zipCode.Latitude.Should().BeNull();
        zipCode.Longitude.Should().BeNull();
        zipCode.Accuracy.Should().BeNull();
        zipCode.IsActive.Should().BeTrue();
        zipCode.Localities.Should().BeNull();
        zipCode.Addresses.Should().BeNull();
    }

    [Fact]
    public void ZipCode_ShouldAllowSettingAllProperties()
    {
        var zipCode = new ZipCode
        {
            Id = 1,
            Country = "United States",
            CountryCode = "US",
            PostalCode = "90210",
            City = "Beverly Hills",
            State = "California",
            StateCode = "CA",
            County = "Los Angeles",
            CountyCode = "037",
            Community = "Beverly Hills",
            CommunityCode = "0601",
            Latitude = 34.0901m,
            Longitude = -118.4065m,
            Accuracy = 6,
            IsActive = true
        };

        zipCode.Id.Should().Be(1);
        zipCode.Country.Should().Be("United States");
        zipCode.CountryCode.Should().Be("US");
        zipCode.PostalCode.Should().Be("90210");
        zipCode.City.Should().Be("Beverly Hills");
        zipCode.State.Should().Be("California");
        zipCode.StateCode.Should().Be("CA");
        zipCode.County.Should().Be("Los Angeles");
        zipCode.CountyCode.Should().Be("037");
        zipCode.Community.Should().Be("Beverly Hills");
        zipCode.CommunityCode.Should().Be("0601");
        zipCode.Latitude.Should().Be(34.0901m);
        zipCode.Longitude.Should().Be(-118.4065m);
        zipCode.Accuracy.Should().Be(6);
        zipCode.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ZipCode_ShouldSupportNavigationProperties()
    {
        var zipCode = new ZipCode
        {
            Localities = new List<Locality> { new Locality { Name = "Downtown" } },
            Addresses = new List<Address> { new Address { Line1 = "123 Main St" } }
        };

        zipCode.Localities.Should().HaveCount(1);
        zipCode.Addresses.Should().HaveCount(1);
    }

    [Fact]
    public void ZipCode_DoesNotInheritFromBaseEntity()
    {
        // ZipCode is a standalone entity without BaseEntity inheritance
        typeof(ZipCode).Should().NotBeAssignableTo<BaseEntity>();
    }

    #endregion

    #region LookupCategory Entity Tests

    [Fact]
    public void LookupCategory_ShouldInitializeWithDefaults()
    {
        var category = new LookupCategory();

        category.Name.Should().BeEmpty();
        category.Description.Should().BeNull();
        category.IsActive.Should().BeTrue();
        category.Items.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void LookupCategory_ShouldAllowSettingProperties()
    {
        var category = new LookupCategory
        {
            Id = 1,
            Name = "Industries",
            Description = "Industry classification lookup values",
            IsActive = true
        };

        category.Id.Should().Be(1);
        category.Name.Should().Be("Industries");
        category.Description.Should().Be("Industry classification lookup values");
        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public void LookupCategory_ShouldInheritFromBaseEntity()
    {
        typeof(LookupCategory).Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void LookupCategory_ShouldSupportItemsCollection()
    {
        var category = new LookupCategory { Name = "Industries" };
        category.Items.Add(new LookupItem { Key = "tech", Value = "Technology" });
        category.Items.Add(new LookupItem { Key = "finance", Value = "Finance" });

        category.Items.Should().HaveCount(2);
    }

    #endregion

    #region LookupItem Entity Tests

    [Fact]
    public void LookupItem_ShouldInitializeWithDefaults()
    {
        var item = new LookupItem();

        item.LookupCategoryId.Should().Be(0);
        item.Category.Should().BeNull();
        item.Key.Should().BeEmpty();
        item.Value.Should().BeEmpty();
        item.Meta.Should().BeNull();
        item.SortOrder.Should().Be(0);
        item.IsActive.Should().BeTrue();
    }

    [Fact]
    public void LookupItem_ShouldAllowSettingAllProperties()
    {
        var item = new LookupItem
        {
            Id = 1,
            LookupCategoryId = 10,
            Key = "technology",
            Value = "Technology",
            Meta = "{\"icon\": \"computer\"}",
            SortOrder = 5,
            IsActive = true
        };

        item.Id.Should().Be(1);
        item.LookupCategoryId.Should().Be(10);
        item.Key.Should().Be("technology");
        item.Value.Should().Be("Technology");
        item.Meta.Should().Be("{\"icon\": \"computer\"}");
        item.SortOrder.Should().Be(5);
        item.IsActive.Should().BeTrue();
    }

    [Fact]
    public void LookupItem_ShouldInheritFromBaseEntity()
    {
        typeof(LookupItem).Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void LookupItem_NavigationToCategory_ShouldWork()
    {
        var category = new LookupCategory { Id = 1, Name = "Industries" };
        var item = new LookupItem
        {
            LookupCategoryId = 1,
            Category = category,
            Key = "tech",
            Value = "Technology"
        };

        item.Category.Should().BeSameAs(category);
    }

    #endregion

    #region Department Entity Tests

    [Fact]
    public void Department_ShouldInitializeWithDefaults()
    {
        var department = new Department();

        department.Name.Should().BeEmpty();
        department.Description.Should().BeEmpty();
        department.DepartmentCode.Should().BeNull();
        department.IsActive.Should().BeTrue();
        department.ParentDepartmentId.Should().BeNull();
        department.Users.Should().NotBeNull().And.BeEmpty();
        department.Profiles.Should().NotBeNull().And.BeEmpty();
        department.ParentDepartment.Should().BeNull();
        department.SubDepartments.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Department_ShouldAllowSettingAllProperties()
    {
        var department = new Department
        {
            Id = 1,
            Name = "Sales",
            Description = "Sales department",
            DepartmentCode = "SALES",
            IsActive = true,
            ParentDepartmentId = 10
        };

        department.Id.Should().Be(1);
        department.Name.Should().Be("Sales");
        department.Description.Should().Be("Sales department");
        department.DepartmentCode.Should().Be("SALES");
        department.IsActive.Should().BeTrue();
        department.ParentDepartmentId.Should().Be(10);
    }

    [Fact]
    public void Department_ShouldInheritFromBaseEntity()
    {
        typeof(Department).Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void Department_ShouldSupportHierarchy()
    {
        var parent = new Department { Id = 1, Name = "Operations" };
        var child = new Department
        {
            Id = 2,
            Name = "IT",
            ParentDepartmentId = 1,
            ParentDepartment = parent
        };
        parent.SubDepartments.Add(child);

        child.ParentDepartment.Should().BeSameAs(parent);
        parent.SubDepartments.Should().Contain(child);
    }

    [Fact]
    public void Department_ShouldSupportUserCollection()
    {
        var department = new Department { Name = "HR" };
        department.Users.Add(new User { Username = "hruser1" });
        department.Users.Add(new User { Username = "hruser2" });

        department.Users.Should().HaveCount(2);
    }

    #endregion

    #region SystemSettings Entity Tests

    [Fact]
    public void SystemSettings_ShouldInitializeModuleDefaults()
    {
        var settings = new SystemSettings();

        // Module defaults
        settings.AccountsEnabled.Should().BeTrue();
        settings.ContactsEnabled.Should().BeTrue();
        settings.LeadsEnabled.Should().BeTrue();
        settings.OpportunitiesEnabled.Should().BeTrue();
        settings.ProductsEnabled.Should().BeTrue();
        settings.ServicesEnabled.Should().BeTrue();
        settings.CampaignsEnabled.Should().BeTrue();
        settings.QuotesEnabled.Should().BeTrue();
        settings.TasksEnabled.Should().BeTrue();
        settings.ActivitiesEnabled.Should().BeTrue();
        settings.NotesEnabled.Should().BeTrue();
        settings.WorkflowsEnabled.Should().BeTrue();
        settings.ReportsEnabled.Should().BeTrue();
        settings.DashboardEnabled.Should().BeTrue();
        settings.EmailEnabled.Should().BeTrue();
        settings.WhatsAppEnabled.Should().BeTrue();
        settings.SocialMediaEnabled.Should().BeTrue();
        settings.CommunicationsEnabled.Should().BeTrue();
        settings.InteractionsEnabled.Should().BeTrue();
    }

    [Fact]
    public void SystemSettings_ShouldInitializeBrandingDefaults()
    {
        var settings = new SystemSettings();

        settings.CompanyName.Should().Be("CRM System");
        settings.CompanyLogoUrl.Should().BeNull();
        settings.CompanyLoginLogoUrl.Should().BeNull();
        settings.PrimaryColor.Should().Be("#6750A4");
        settings.SecondaryColor.Should().Be("#625B71");
        settings.TertiaryColor.Should().Be("#7D5260");
        settings.SurfaceColor.Should().Be("#FFFBFE");
        settings.BackgroundColor.Should().Be("#FFFBFE");
        settings.UseGroupHeaderColor.Should().BeFalse();
    }

    [Fact]
    public void SystemSettings_ShouldInitializeSecurityDefaults()
    {
        var settings = new SystemSettings();

        settings.RequireTwoFactor.Should().BeFalse();
        settings.MinPasswordLength.Should().Be(8);
        settings.MaxPasswordLength.Should().Be(128);
        settings.RequireUppercase.Should().BeTrue();
        settings.RequireLowercase.Should().BeTrue();
        settings.RequireNumbers.Should().BeTrue();
        settings.RequireSpecialChars.Should().BeFalse();
        settings.DefaultPasswordExpirationDays.Should().Be(0);
        settings.SessionTimeoutMinutes.Should().Be(60);
        settings.AllowUserRegistration.Should().BeTrue();
        settings.RequireApprovalForNewUsers.Should().BeTrue();
        settings.QuickAdminLoginEnabled.Should().BeTrue();
    }

    [Fact]
    public void SystemSettings_ShouldInitializeSocialLoginDefaults()
    {
        var settings = new SystemSettings();

        // Google
        settings.GoogleAuthEnabled.Should().BeFalse();
        settings.GoogleClientId.Should().BeNull();
        settings.GoogleClientSecret.Should().BeNull();

        // Microsoft
        settings.MicrosoftAuthEnabled.Should().BeFalse();
        settings.MicrosoftClientId.Should().BeNull();
        settings.MicrosoftClientSecret.Should().BeNull();
        settings.MicrosoftTenantId.Should().Be("common");

        // Azure AD
        settings.AzureAdAuthEnabled.Should().BeFalse();
        settings.AzureAdClientId.Should().BeNull();
        settings.AzureAdClientSecret.Should().BeNull();

        // LinkedIn
        settings.LinkedInAuthEnabled.Should().BeFalse();
        settings.LinkedInClientId.Should().BeNull();
        settings.LinkedInClientSecret.Should().BeNull();

        // Facebook
        settings.FacebookAuthEnabled.Should().BeFalse();
        settings.FacebookAppId.Should().BeNull();
        settings.FacebookAppSecret.Should().BeNull();
    }

    [Fact]
    public void SystemSettings_ShouldInitializeFeatureFlagDefaults()
    {
        var settings = new SystemSettings();

        settings.ShowDemoData.Should().BeFalse();
        settings.ApiAccessEnabled.Should().BeTrue();
        settings.EmailNotificationsEnabled.Should().BeTrue();
        settings.AuditLoggingEnabled.Should().BeTrue();
    }

    [Fact]
    public void SystemSettings_ShouldInitializeCustomizationDefaults()
    {
        var settings = new SystemSettings();

        settings.CustomFieldsConfig.Should().BeNull();
        settings.DateFormat.Should().Be("MM/dd/yyyy");
        settings.TimeFormat.Should().Be("12h");
        settings.DefaultCurrency.Should().Be("USD");
        settings.DefaultTimezone.Should().Be("America/New_York");
        settings.DefaultLanguage.Should().Be("en");
    }

    [Fact]
    public void SystemSettings_ShouldInitializeQuoteDefaults()
    {
        var settings = new SystemSettings();

        settings.QuoteTermsAndConditions.Should().BeNull();
        settings.QuoteValidityDays.Should().Be(30);
        settings.QuoteNumberPrefix.Should().Be("QT-");
        settings.QuoteNumberSequence.Should().Be(1000);
        settings.DefaultTaxRate.Should().Be(0);
    }

    [Fact]
    public void SystemSettings_ShouldInitializeDatabaseProviderDefaults()
    {
        var settings = new SystemSettings();

        settings.MariaDbEnabled.Should().BeTrue();
        settings.PostgreSqlEnabled.Should().BeFalse();
        settings.SqlServerEnabled.Should().BeFalse();
        settings.SqliteEnabled.Should().BeFalse();
        settings.MySqlEnabled.Should().BeFalse();
        settings.ActiveDatabaseProvider.Should().Be("mariadb");
    }

    [Fact]
    public void SystemSettings_ShouldInitializeSslDefaults()
    {
        var settings = new SystemSettings();

        settings.HttpsEnabled.Should().BeFalse();
        settings.SslCertificatePath.Should().BeNull();
        settings.SslPrivateKeyPath.Should().BeNull();
        settings.SslCertificateExpiry.Should().BeNull();
        settings.SslCertificateSubject.Should().BeNull();
        settings.ForceHttpsRedirect.Should().BeFalse();
    }

    [Fact]
    public void SystemSettings_ShouldInitializeStatisticsDefaults()
    {
        var settings = new SystemSettings();

        settings.StatisticsRefreshEnabled.Should().BeFalse();
        settings.StatisticsRefreshIntervalMinutes.Should().Be(60);
        settings.StatisticsLastRefreshed.Should().BeNull();
    }

    [Fact]
    public void SystemSettings_ShouldInheritFromBaseEntity()
    {
        typeof(SystemSettings).Should().BeAssignableTo<BaseEntity>();
    }

    #endregion

    #region Tag Entity Tests

    [Fact]
    public void Tag_ShouldInitializeWithDefaults()
    {
        var tag = new Tag();

        tag.Name.Should().BeEmpty();
        tag.Color.Should().BeNull();
        tag.Description.Should().BeNull();
        tag.EntityTags.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Tag_ShouldAllowSettingProperties()
    {
        var tag = new Tag
        {
            Id = 1,
            Name = "VIP",
            Color = "#FF0000",
            Description = "Very important customer"
        };

        tag.Id.Should().Be(1);
        tag.Name.Should().Be("VIP");
        tag.Color.Should().Be("#FF0000");
        tag.Description.Should().Be("Very important customer");
    }

    [Fact]
    public void Tag_ShouldInheritFromBaseEntity()
    {
        typeof(Tag).Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void Tag_ShouldSupportEntityTagsCollection()
    {
        var tag = new Tag { Name = "Important" };
        tag.EntityTags.Add(new EntityTag { EntityType = "Customer", EntityId = 1 });
        tag.EntityTags.Add(new EntityTag { EntityType = "Contact", EntityId = 2 });

        tag.EntityTags.Should().HaveCount(2);
    }

    #endregion

    #region EntityTag Entity Tests

    [Fact]
    public void EntityTag_ShouldInitializeWithDefaults()
    {
        var entityTag = new EntityTag();

        entityTag.EntityType.Should().BeEmpty();
        entityTag.EntityId.Should().Be(0);
        entityTag.TagId.Should().Be(0);
        entityTag.TagName.Should().BeNull();
        entityTag.SortOrder.Should().Be(0);
        entityTag.CreatedBy.Should().BeNull();
        entityTag.Tag.Should().BeNull();
    }

    [Fact]
    public void EntityTag_ShouldAllowSettingProperties()
    {
        var entityTag = new EntityTag
        {
            Id = 1,
            EntityType = "Customer",
            EntityId = 100,
            TagId = 5,
            TagName = "VIP",
            SortOrder = 1,
            CreatedBy = 10
        };

        entityTag.Id.Should().Be(1);
        entityTag.EntityType.Should().Be("Customer");
        entityTag.EntityId.Should().Be(100);
        entityTag.TagId.Should().Be(5);
        entityTag.TagName.Should().Be("VIP");
        entityTag.SortOrder.Should().Be(1);
        entityTag.CreatedBy.Should().Be(10);
    }

    [Fact]
    public void EntityTag_ShouldInheritFromBaseEntity()
    {
        typeof(EntityTag).Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void EntityTag_NavigationToTag_ShouldWork()
    {
        var tag = new Tag { Id = 1, Name = "Important" };
        var entityTag = new EntityTag
        {
            TagId = 1,
            Tag = tag,
            EntityType = "Contact",
            EntityId = 100
        };

        entityTag.Tag.Should().BeSameAs(tag);
    }

    #endregion

    #region CustomField Entity Tests

    [Fact]
    public void CustomField_ShouldInitializeWithDefaults()
    {
        var field = new CustomField();

        field.EntityType.Should().BeNull();
        field.EntityId.Should().Be(0);
        field.Key.Should().BeNull();
        field.Value.Should().BeNull();
    }

    [Fact]
    public void CustomField_ShouldAllowSettingProperties()
    {
        var field = new CustomField
        {
            Id = 1,
            EntityType = "Customer",
            EntityId = 100,
            Key = "custom_field_1",
            Value = "Custom Value"
        };

        field.Id.Should().Be(1);
        field.EntityType.Should().Be("Customer");
        field.EntityId.Should().Be(100);
        field.Key.Should().Be("custom_field_1");
        field.Value.Should().Be("Custom Value");
    }

    [Fact]
    public void CustomField_ShouldInheritFromBaseEntity()
    {
        typeof(CustomField).Should().BeAssignableTo<BaseEntity>();
    }

    #endregion

    #region ConversationStatus Enum Tests

    [Theory]
    [InlineData(ConversationStatus.Open, 0)]
    [InlineData(ConversationStatus.Pending, 1)]
    [InlineData(ConversationStatus.Resolved, 2)]
    [InlineData(ConversationStatus.Closed, 3)]
    [InlineData(ConversationStatus.Spam, 4)]
    [InlineData(ConversationStatus.Archived, 5)]
    public void ConversationStatus_ShouldHaveCorrectValues(ConversationStatus status, int expectedValue)
    {
        ((int)status).Should().Be(expectedValue);
    }

    [Fact]
    public void ConversationStatus_ShouldHaveExpectedCount()
    {
        Enum.GetValues<ConversationStatus>().Should().HaveCount(6);
    }

    [Fact]
    public void ConversationStatus_DefaultShouldBeOpen()
    {
        default(ConversationStatus).Should().Be(ConversationStatus.Open);
    }

    #endregion

    #region Conversation Entity Tests

    [Fact]
    public void Conversation_ShouldInitializeWithDefaults()
    {
        var conversation = new Conversation();

        conversation.ConversationId.Should().NotBeNullOrEmpty(); // GUID generated
        conversation.Subject.Should().BeNull();
        conversation.LastMessagePreview.Should().BeNull();
        conversation.Status.Should().Be(ConversationStatus.Open);
        conversation.Priority.Should().Be(MessagePriority.Normal);
        conversation.ParticipantAddress.Should().BeNull();
        conversation.ParticipantName.Should().BeNull();
        conversation.AccountId.Should().BeNull();
        conversation.ContactId.Should().BeNull();
        conversation.LeadId.Should().BeNull();
        conversation.AssignedToUserId.Should().BeNull();
        conversation.LastRespondedByUserId.Should().BeNull();
        conversation.MessageCount.Should().Be(0);
        conversation.UnreadCount.Should().Be(0);
        conversation.InboundCount.Should().Be(0);
        conversation.OutboundCount.Should().Be(0);
        conversation.IsStarred.Should().BeFalse();
        conversation.IsMuted.Should().BeFalse();
        conversation.IsPinned.Should().BeFalse();
        conversation.Messages.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Conversation_ShouldAllowSettingAllProperties()
    {
        var conversation = new Conversation
        {
            Id = 1,
            PrimaryChannelType = ChannelType.Email,
            ConversationId = "conv-123",
            Subject = "Support Request",
            LastMessagePreview = "Thanks for contacting us...",
            Status = ConversationStatus.Pending,
            Priority = MessagePriority.High,
            ParticipantAddress = "customer@example.com",
            ParticipantName = "John Doe",
            AccountId = 10,
            ContactId = 20,
            LeadId = 30,
            AssignedToUserId = 5,
            LastRespondedByUserId = 5,
            MessageCount = 10,
            UnreadCount = 2,
            InboundCount = 6,
            OutboundCount = 4,
            FirstMessageAt = DateTime.UtcNow.AddDays(-1),
            LastMessageAt = DateTime.UtcNow,
            ResolvedAt = null,
            TagsJson = "[\"urgent\", \"billing\"]",
            MetadataJson = "{\"source\": \"web\"}",
            IsStarred = true,
            IsMuted = false,
            IsPinned = true
        };

        conversation.Id.Should().Be(1);
        conversation.PrimaryChannelType.Should().Be(ChannelType.Email);
        conversation.ConversationId.Should().Be("conv-123");
        conversation.Subject.Should().Be("Support Request");
        conversation.Status.Should().Be(ConversationStatus.Pending);
        conversation.Priority.Should().Be(MessagePriority.High);
        conversation.MessageCount.Should().Be(10);
        conversation.UnreadCount.Should().Be(2);
        conversation.IsStarred.Should().BeTrue();
        conversation.IsPinned.Should().BeTrue();
    }

    [Fact]
    public void Conversation_ShouldInheritFromBaseEntity()
    {
        typeof(Conversation).Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void Conversation_ConversationId_ShouldBeGuidFormat()
    {
        var conversation = new Conversation();

        Guid.TryParse(conversation.ConversationId, out _).Should().BeTrue();
    }

    #endregion

    #region NoteVisibility Enum Tests

    [Theory]
    [InlineData(NoteVisibility.Private, 0)]
    [InlineData(NoteVisibility.Team, 1)]
    [InlineData(NoteVisibility.Public, 2)]
    public void NoteVisibility_ShouldHaveCorrectValues(NoteVisibility visibility, int expectedValue)
    {
        ((int)visibility).Should().Be(expectedValue);
    }

    [Fact]
    public void NoteVisibility_ShouldHaveExpectedCount()
    {
        Enum.GetValues<NoteVisibility>().Should().HaveCount(3);
    }

    [Fact]
    public void NoteVisibility_DefaultShouldBePrivate()
    {
        default(NoteVisibility).Should().Be(NoteVisibility.Private);
    }

    #endregion

    #region NoteType Enum Tests

    [Theory]
    [InlineData(NoteType.General, 0)]
    [InlineData(NoteType.CallNotes, 1)]
    [InlineData(NoteType.MeetingNotes, 2)]
    [InlineData(NoteType.Feedback, 3)]
    [InlineData(NoteType.Requirement, 4)]
    [InlineData(NoteType.Issue, 5)]
    [InlineData(NoteType.Idea, 6)]
    [InlineData(NoteType.Warning, 7)]
    [InlineData(NoteType.Other, 8)]
    public void NoteType_ShouldHaveCorrectValues(NoteType noteType, int expectedValue)
    {
        ((int)noteType).Should().Be(expectedValue);
    }

    [Fact]
    public void NoteType_ShouldHaveExpectedCount()
    {
        Enum.GetValues<NoteType>().Should().HaveCount(9);
    }

    [Fact]
    public void NoteType_DefaultShouldBeGeneral()
    {
        default(NoteType).Should().Be(NoteType.General);
    }

    #endregion

    #region Note Entity Tests

    [Fact]
    public void Note_ShouldInitializeWithDefaults()
    {
        var note = new Note();

        note.Title.Should().BeEmpty();
        note.Content.Should().BeEmpty();
        note.Summary.Should().BeNull();
        note.NoteType.Should().Be(NoteType.General);
        note.Visibility.Should().Be(NoteVisibility.Team);
        note.IsPinned.Should().BeFalse();
        note.IsImportant.Should().BeFalse();
        note.EntityType.Should().BeNull();
        note.EntityId.Should().BeNull();
        note.AccountId.Should().BeNull();
        note.ContactId.Should().BeNull();
        note.LeadId.Should().BeNull();
        note.CreatedByUserId.Should().BeNull();
        note.Tags.Should().BeNull();
        note.Category.Should().BeNull();
        note.Attachments.Should().BeNull();
        note.MentionedUsers.Should().BeNull();
        note.RelatedNotes.Should().BeNull();
        note.CustomFields.Should().BeNull();
    }

    [Fact]
    public void Note_ShouldAllowSettingAllProperties()
    {
        var note = new Note
        {
            Id = 1,
            Title = "Meeting Notes",
            Content = "Discussed quarterly targets...",
            Summary = "Q1 targets reviewed",
            NoteType = NoteType.MeetingNotes,
            Visibility = NoteVisibility.Team,
            IsPinned = true,
            IsImportant = true,
            EntityType = "Customer",
            EntityId = 100,
            AccountId = 100,
            ContactId = 50,
            LeadId = null,
            CreatedByUserId = 5,
            LastModifiedByUserId = 5,
            Tags = "meeting,quarterly",
            Category = "Sales",
            Attachments = "[\"file1.pdf\"]",
            MentionedUsers = "[1, 2, 3]",
            RelatedNotes = "[10, 11]",
            CustomFields = "{\"field1\": \"value1\"}",
            ContextPath = "/customers/100"
        };

        note.Id.Should().Be(1);
        note.Title.Should().Be("Meeting Notes");
        note.NoteType.Should().Be(NoteType.MeetingNotes);
        note.Visibility.Should().Be(NoteVisibility.Team);
        note.IsPinned.Should().BeTrue();
        note.IsImportant.Should().BeTrue();
        note.EntityType.Should().Be("Customer");
        note.EntityId.Should().Be(100);
        note.CreatedByUserId.Should().Be(5);
    }

    [Fact]
    public void Note_ShouldInheritFromBaseEntity()
    {
        typeof(Note).Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void Note_ShouldSupportPolymorphicAttachment()
    {
        var note = new Note
        {
            EntityType = "Opportunity",
            EntityId = 50,
            Title = "Deal Notes"
        };

        note.EntityType.Should().Be("Opportunity");
        note.EntityId.Should().Be(50);
    }

    #endregion

    #region ActivityType Enum Tests

    [Theory]
    [InlineData(ActivityType.EmailSent, 0)]
    [InlineData(ActivityType.EmailReceived, 1)]
    [InlineData(ActivityType.CallMade, 2)]
    [InlineData(ActivityType.CallReceived, 3)]
    [InlineData(ActivityType.MeetingScheduled, 4)]
    [InlineData(ActivityType.MeetingCompleted, 5)]
    [InlineData(ActivityType.ChatMessage, 6)]
    [InlineData(ActivityType.SMSSent, 7)]
    [InlineData(ActivityType.AccountCreated, 10)]
    [InlineData(ActivityType.AccountUpdated, 11)]
    [InlineData(ActivityType.OpportunityCreated, 12)]
    [InlineData(ActivityType.OpportunityUpdated, 13)]
    [InlineData(ActivityType.OpportunityWon, 14)]
    [InlineData(ActivityType.OpportunityLost, 15)]
    [InlineData(ActivityType.OpportunityStageChanged, 16)]
    [InlineData(ActivityType.QuoteCreated, 20)]
    [InlineData(ActivityType.QuoteSent, 21)]
    [InlineData(ActivityType.QuoteAccepted, 22)]
    [InlineData(ActivityType.QuoteRejected, 23)]
    [InlineData(ActivityType.TaskCreated, 30)]
    [InlineData(ActivityType.TaskCompleted, 31)]
    [InlineData(ActivityType.TaskOverdue, 32)]
    [InlineData(ActivityType.NoteAdded, 40)]
    [InlineData(ActivityType.NoteUpdated, 41)]
    [InlineData(ActivityType.CampaignLaunched, 50)]
    [InlineData(ActivityType.CampaignCompleted, 51)]
    [InlineData(ActivityType.LeadCaptured, 52)]
    [InlineData(ActivityType.OwnerChanged, 60)]
    [InlineData(ActivityType.TagsChanged, 61)]
    [InlineData(ActivityType.StatusChanged, 62)]
    [InlineData(ActivityType.FileUploaded, 63)]
    [InlineData(ActivityType.FileDeleted, 64)]
    [InlineData(ActivityType.Other, 99)]
    public void ActivityType_ShouldHaveCorrectValues(ActivityType activityType, int expectedValue)
    {
        ((int)activityType).Should().Be(expectedValue);
    }

    [Fact]
    public void ActivityType_ShouldHaveExpectedCount()
    {
        Enum.GetValues<ActivityType>().Should().HaveCount(39);
    }

    [Fact]
    public void ActivityType_DefaultShouldBeEmailSent()
    {
        default(ActivityType).Should().Be(ActivityType.EmailSent);
    }

    [Fact]
    public void ActivityType_CommunicationTypes_ShouldBeInRange0To10()
    {
        ((int)ActivityType.EmailSent).Should().BeInRange(0, 9);
        ((int)ActivityType.EmailReceived).Should().BeInRange(0, 9);
        ((int)ActivityType.CallMade).Should().BeInRange(0, 9);
        ((int)ActivityType.CallReceived).Should().BeInRange(0, 9);
        ((int)ActivityType.MeetingScheduled).Should().BeInRange(0, 9);
        ((int)ActivityType.MeetingCompleted).Should().BeInRange(0, 9);
        ((int)ActivityType.ChatMessage).Should().BeInRange(0, 9);
        ((int)ActivityType.SMSSent).Should().BeInRange(0, 9);
    }

    [Fact]
    public void ActivityType_CrmActions_ShouldBeInRange10To20()
    {
        ((int)ActivityType.AccountCreated).Should().BeInRange(10, 19);
        ((int)ActivityType.AccountUpdated).Should().BeInRange(10, 19);
        ((int)ActivityType.OpportunityCreated).Should().BeInRange(10, 19);
        ((int)ActivityType.OpportunityWon).Should().BeInRange(10, 19);
        ((int)ActivityType.OpportunityLost).Should().BeInRange(10, 19);
    }

    #endregion

    #region Activity Entity Tests

    [Fact]
    public void Activity_ShouldInitializeWithDefaults()
    {
        var activity = new Activity();

        activity.Title.Should().BeEmpty();
        activity.Description.Should().BeNull();
        activity.Details.Should().BeNull();
        activity.ActivityDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        activity.DurationMinutes.Should().BeNull();
        activity.UserId.Should().BeNull();
        activity.UserName.Should().BeNull();
        activity.UserEmail.Should().BeNull();
        activity.EntityType.Should().BeNull();
        activity.EntityId.Should().BeNull();
        activity.EntityName.Should().BeNull();
        activity.AccountId.Should().BeNull();
        activity.ContactId.Should().BeNull();
        activity.OldValue.Should().BeNull();
        activity.NewValue.Should().BeNull();
        activity.FieldsChanged.Should().BeNull();
        activity.IsSystem.Should().BeFalse();
        activity.IsPrivate.Should().BeFalse();
        activity.IsImportant.Should().BeFalse();
        activity.Tags.Should().BeNull();
        activity.Category.Should().BeNull();
        activity.Source.Should().BeNull();
        activity.Attendees.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Activity_ShouldAllowSettingAllProperties()
    {
        var activityDate = DateTime.UtcNow.AddHours(-1);
        var activity = new Activity
        {
            Id = 1,
            ActivityType = ActivityType.MeetingCompleted,
            Title = "Client Meeting",
            Description = "Discussed requirements",
            Details = "{\"location\": \"Office\"}",
            ActivityDate = activityDate,
            DurationMinutes = 60,
            UserId = 5,
            UserName = "John Doe",
            UserEmail = "john@example.com",
            EntityType = "Customer",
            EntityId = 100,
            EntityName = "Acme Corp",
            SecondaryEntityType = "Opportunity",
            SecondaryEntityId = 50,
            SecondaryEntityName = "Big Deal",
            AccountId = 100,
            ContactId = 200,
            OpportunityId = 50,
            OldValue = "Prospect",
            NewValue = "Customer",
            FieldsChanged = "[\"Status\"]",
            IsSystem = false,
            IsPrivate = false,
            IsImportant = true,
            Tags = "meeting,client",
            Category = "Sales",
            IpAddress = "192.168.1.1",
            UserAgent = "Chrome/100",
            Source = "Web"
        };

        activity.Id.Should().Be(1);
        activity.ActivityType.Should().Be(ActivityType.MeetingCompleted);
        activity.Title.Should().Be("Client Meeting");
        activity.DurationMinutes.Should().Be(60);
        activity.EntityType.Should().Be("Customer");
        activity.EntityId.Should().Be(100);
        activity.IsImportant.Should().BeTrue();
        activity.Source.Should().Be("Web");
    }

    [Fact]
    public void Activity_ShouldInheritFromBaseEntity()
    {
        typeof(Activity).Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void Activity_ShouldSupportAttendeesCollection()
    {
        var activity = new Activity { Title = "Team Meeting" };
        activity.Attendees.Add(new EventAttendee { AttendeeId = 1, AttendeeType = AttendeeType.User });
        activity.Attendees.Add(new EventAttendee { AttendeeId = 2, AttendeeType = AttendeeType.User });

        activity.Attendees.Should().HaveCount(2);
    }

    [Fact]
    public void Activity_ShouldSupportPolymorphicEntity()
    {
        var activity = new Activity
        {
            EntityType = "Lead",
            EntityId = 99,
            EntityName = "Hot Lead"
        };

        activity.EntityType.Should().Be("Lead");
        activity.EntityId.Should().Be(99);
        activity.EntityName.Should().Be("Hot Lead");
    }

    [Fact]
    public void Activity_ShouldSupportSecondaryEntity()
    {
        var activity = new Activity
        {
            EntityType = "Customer",
            EntityId = 1,
            SecondaryEntityType = "Quote",
            SecondaryEntityId = 100,
            SecondaryEntityName = "Q-2024-001"
        };

        activity.SecondaryEntityType.Should().Be("Quote");
        activity.SecondaryEntityId.Should().Be(100);
        activity.SecondaryEntityName.Should().Be("Q-2024-001");
    }

    [Fact]
    public void Activity_ShouldTrackChanges()
    {
        var activity = new Activity
        {
            ActivityType = ActivityType.StatusChanged,
            OldValue = "Active",
            NewValue = "Inactive",
            FieldsChanged = "[\"Status\", \"UpdatedAt\"]"
        };

        activity.OldValue.Should().Be("Active");
        activity.NewValue.Should().Be("Inactive");
        activity.FieldsChanged.Should().Contain("Status");
    }

    #endregion
}
