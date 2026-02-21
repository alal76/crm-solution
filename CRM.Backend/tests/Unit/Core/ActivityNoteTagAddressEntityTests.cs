// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Models;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

#region ActivityType Enum Tests

public class ActivityTypeEnumTests
{
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
    public void ActivityType_ShouldHaveCorrectValue(ActivityType type, int expectedValue)
    {
        ((int)type).Should().Be(expectedValue);
    }

    [Fact]
    public void ActivityType_ShouldHave39Values()
    {
        Enum.GetValues(typeof(ActivityType)).Length.Should().Be(39);
    }

    [Fact]
    public void ActivityType_AllCommunicationTypes_ShouldBe0To7()
    {
        // Communication types are 0-7
        var communicationTypes = new[] {
            ActivityType.EmailSent,
            ActivityType.EmailReceived,
            ActivityType.CallMade,
            ActivityType.CallReceived,
            ActivityType.MeetingScheduled,
            ActivityType.MeetingCompleted,
            ActivityType.ChatMessage,
            ActivityType.SMSSent
        };

        communicationTypes.Select(t => (int)t).Should().AllSatisfy(x => x.Should().BeGreaterThanOrEqualTo(0).And.BeLessThanOrEqualTo(7));
    }

    [Fact]
    public void ActivityType_AllCrmActionTypes_ShouldBe10To16()
    {
        var crmActionTypes = new[] {
            ActivityType.AccountCreated,
            ActivityType.AccountUpdated,
            ActivityType.OpportunityCreated,
            ActivityType.OpportunityUpdated,
            ActivityType.OpportunityWon,
            ActivityType.OpportunityLost,
            ActivityType.OpportunityStageChanged
        };

        crmActionTypes.Select(t => (int)t).Should().AllSatisfy(x => x.Should().BeGreaterThanOrEqualTo(10).And.BeLessThanOrEqualTo(16));
    }
}

#endregion

#region Activity Entity Tests

public class ActivityEntityTests
{
    [Fact]
    public void Activity_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var activity = new Activity();

        // Assert
        activity.Title.Should().BeEmpty();
        activity.Description.Should().BeNull();
        activity.Details.Should().BeNull();
        activity.ActivityDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        activity.DurationMinutes.Should().BeNull();
        activity.IsSystem.Should().BeFalse();
        activity.IsPrivate.Should().BeFalse();
        activity.IsImportant.Should().BeFalse();
    }

    [Fact]
    public void Activity_ActorProperties_ShouldBeNullByDefault()
    {
        // Act
        var activity = new Activity();

        // Assert
        activity.UserId.Should().BeNull();
        activity.UserName.Should().BeNull();
        activity.UserEmail.Should().BeNull();
    }

    [Fact]
    public void Activity_RelatedEntityProperties_ShouldBeNullByDefault()
    {
        // Act
        var activity = new Activity();

        // Assert
        activity.EntityType.Should().BeNull();
        activity.EntityId.Should().BeNull();
        activity.EntityName.Should().BeNull();
        activity.SecondaryEntityType.Should().BeNull();
        activity.SecondaryEntityId.Should().BeNull();
        activity.SecondaryEntityName.Should().BeNull();
    }

    [Fact]
    public void Activity_SpecificRelationshipIds_ShouldBeNullByDefault()
    {
        // Act
        var activity = new Activity();

        // Assert
        activity.AccountId.Should().BeNull();
        activity.ContactId.Should().BeNull();
        activity.OpportunityId.Should().BeNull();
        activity.CampaignId.Should().BeNull();
        activity.ProductId.Should().BeNull();
        activity.TaskId.Should().BeNull();
        activity.QuoteId.Should().BeNull();
        activity.InteractionId.Should().BeNull();
        activity.NoteId.Should().BeNull();
    }

    [Fact]
    public void Activity_ChangeTracking_ShouldBeNullByDefault()
    {
        // Act
        var activity = new Activity();

        // Assert
        activity.OldValue.Should().BeNull();
        activity.NewValue.Should().BeNull();
        activity.FieldsChanged.Should().BeNull();
    }

    [Fact]
    public void Activity_MetadataProperties_ShouldBeNullByDefault()
    {
        // Act
        var activity = new Activity();

        // Assert
        activity.Tags.Should().BeNull();
        activity.Category.Should().BeNull();
        activity.IpAddress.Should().BeNull();
        activity.UserAgent.Should().BeNull();
        activity.Source.Should().BeNull();
        activity.CustomFields.Should().BeNull();
    }

    [Fact]
    public void Activity_NavigationCollections_ShouldBeInitialized()
    {
        // Act
        var activity = new Activity();

        // Assert
        activity.Attendees.Should().NotBeNull();
        activity.Attendees.Should().BeEmpty();
    }

    [Fact]
    public void Activity_ShouldSetAllProperties()
    {
        // Arrange
        var activityDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var activity = new Activity
        {
            ActivityType = ActivityType.MeetingCompleted,
            Title = "Client Meeting",
            Description = "Quarterly review meeting",
            Details = "{\"notes\":\"test\"}",
            ActivityDate = activityDate,
            DurationMinutes = 60,
            UserId = 1,
            UserName = "John Doe",
            UserEmail = "john@example.com",
            EntityType = "Customer",
            EntityId = 100,
            EntityName = "Acme Corp",
            AccountId = 100,
            ContactId = 50,
            IsSystem = true,
            IsPrivate = false,
            IsImportant = true,
            Tags = "important,meeting",
            Category = "Sales",
            Source = "Web"
        };

        // Assert
        activity.ActivityType.Should().Be(ActivityType.MeetingCompleted);
        activity.Title.Should().Be("Client Meeting");
        activity.Description.Should().Be("Quarterly review meeting");
        activity.DurationMinutes.Should().Be(60);
        activity.UserId.Should().Be(1);
        activity.EntityType.Should().Be("Customer");
        activity.EntityId.Should().Be(100);
        activity.IsImportant.Should().BeTrue();
    }

    [Fact]
    public void Activity_ShouldInheritFromBaseEntity()
    {
        // Arrange & Act
        var activity = new Activity { Id = 1 };

        // Assert
        activity.Should().BeAssignableTo<BaseEntity>();
        activity.Id.Should().Be(1);
    }
}

#endregion

#region NoteVisibility Enum Tests

public class NoteVisibilityEnumTests
{
    [Theory]
    [InlineData(NoteVisibility.Private, 0)]
    [InlineData(NoteVisibility.Team, 1)]
    [InlineData(NoteVisibility.Public, 2)]
    public void NoteVisibility_ShouldHaveCorrectValue(NoteVisibility visibility, int expectedValue)
    {
        ((int)visibility).Should().Be(expectedValue);
    }

    [Fact]
    public void NoteVisibility_ShouldHave3Values()
    {
        Enum.GetValues(typeof(NoteVisibility)).Length.Should().Be(3);
    }
}

#endregion

#region NoteType Enum Tests

public class NoteTypeEnumTests
{
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
    public void NoteType_ShouldHaveCorrectValue(NoteType type, int expectedValue)
    {
        ((int)type).Should().Be(expectedValue);
    }

    [Fact]
    public void NoteType_ShouldHave9Values()
    {
        Enum.GetValues(typeof(NoteType)).Length.Should().Be(9);
    }
}

#endregion

#region Note Entity Tests

public class NoteEntityTests
{
    [Fact]
    public void Note_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var note = new Note();

        // Assert
        note.Title.Should().BeEmpty();
        note.Content.Should().BeEmpty();
        note.Summary.Should().BeNull();
        note.NoteType.Should().Be(NoteType.General);
        note.Visibility.Should().Be(NoteVisibility.Team);
        note.IsPinned.Should().BeFalse();
        note.IsImportant.Should().BeFalse();
    }

    [Fact]
    public void Note_PolymorphicProperties_ShouldBeNullByDefault()
    {
        // Act
        var note = new Note();

        // Assert
        note.EntityType.Should().BeNull();
        note.EntityId.Should().BeNull();
    }

    [Fact]
    public void Note_LegacyRelationshipIds_ShouldBeNullByDefault()
    {
        // Act
        var note = new Note();

        // Assert
        note.AccountId.Should().BeNull();
        note.ContactId.Should().BeNull();
        note.OpportunityId.Should().BeNull();
        note.CampaignId.Should().BeNull();
        note.ProductId.Should().BeNull();
        note.TaskId.Should().BeNull();
        note.InteractionId.Should().BeNull();
        note.LeadId.Should().BeNull();
        note.ServiceRequestId.Should().BeNull();
        note.QuoteId.Should().BeNull();
    }

    [Fact]
    public void Note_AuthorshipProperties_ShouldBeNullByDefault()
    {
        // Act
        var note = new Note();

        // Assert
        note.CreatedByUserId.Should().BeNull();
        note.LastModifiedByUserId.Should().BeNull();
    }

    [Fact]
    public void Note_ClassificationAndMetadata_ShouldBeNullByDefault()
    {
        // Act
        var note = new Note();

        // Assert
        note.Tags.Should().BeNull();
        note.Category.Should().BeNull();
        note.Attachments.Should().BeNull();
        note.MentionedUsers.Should().BeNull();
        note.RelatedNotes.Should().BeNull();
        note.CustomFields.Should().BeNull();
        note.ContextPath.Should().BeNull();
    }

    [Fact]
    public void Note_ShouldSetAllProperties()
    {
        // Act
        var note = new Note
        {
            Title = "Meeting Notes",
            Content = "Discussion about quarterly goals",
            Summary = "Q4 planning",
            NoteType = NoteType.MeetingNotes,
            Visibility = NoteVisibility.Team,
            IsPinned = true,
            IsImportant = true,
            EntityType = "Customer",
            EntityId = 100,
            AccountId = 100,
            ContactId = 50,
            CreatedByUserId = 1,
            Tags = "meeting,q4,planning",
            Category = "Planning",
            Attachments = "[\"file1.pdf\",\"file2.docx\"]"
        };

        // Assert
        note.Title.Should().Be("Meeting Notes");
        note.Content.Should().Be("Discussion about quarterly goals");
        note.NoteType.Should().Be(NoteType.MeetingNotes);
        note.Visibility.Should().Be(NoteVisibility.Team);
        note.IsPinned.Should().BeTrue();
        note.IsImportant.Should().BeTrue();
        note.EntityType.Should().Be("Customer");
        note.EntityId.Should().Be(100);
    }

    [Fact]
    public void Note_ShouldInheritFromBaseEntity()
    {
        // Arrange & Act
        var note = new Note { Id = 1 };

        // Assert
        note.Should().BeAssignableTo<BaseEntity>();
        note.Id.Should().Be(1);
    }

    [Fact]
    public void Note_ShouldHaveNavigationProperties()
    {
        // Act
        var note = new Note
        {
            Account = new Account { Id = 1, Company = "Test" },
            Contact = new Contact { Id = 1, FirstName = "John" },
            CreatedByUser = new User { Id = 1, Username = "admin" }
        };

        // Assert
        note.Account.Should().NotBeNull();
        note.Contact.Should().NotBeNull();
        note.CreatedByUser.Should().NotBeNull();
    }
}

#endregion

#region Tag Entity Tests

public class TagEntityTests
{
    [Fact]
    public void Tag_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var tag = new Tag();

        // Assert
        tag.Name.Should().BeEmpty();
        tag.Color.Should().BeNull();
        tag.Description.Should().BeNull();
        tag.EntityTags.Should().NotBeNull();
        tag.EntityTags.Should().BeEmpty();
    }

    [Fact]
    public void Tag_ShouldSetAllProperties()
    {
        // Act
        var tag = new Tag
        {
            Name = "VIP",
            Color = "#FF0000",
            Description = "Very Important Person tag"
        };

        // Assert
        tag.Name.Should().Be("VIP");
        tag.Color.Should().Be("#FF0000");
        tag.Description.Should().Be("Very Important Person tag");
    }

    [Fact]
    public void Tag_ShouldInheritFromBaseEntity()
    {
        // Arrange & Act
        var tag = new Tag { Id = 1, Name = "Test" };

        // Assert
        tag.Should().BeAssignableTo<BaseEntity>();
        tag.Id.Should().Be(1);
    }

    [Fact]
    public void Tag_EntityTags_ShouldBeModifiable()
    {
        // Arrange
        var tag = new Tag { Id = 1, Name = "Test" };
        var entityTag = new EntityTag { TagId = 1, EntityType = "Customer", EntityId = 100 };

        // Act
        tag.EntityTags.Add(entityTag);

        // Assert
        tag.EntityTags.Should().HaveCount(1);
        tag.EntityTags.First().TagId.Should().Be(1);
    }
}

#endregion

#region EntityTag Entity Tests

public class EntityTagEntityTests
{
    [Fact]
    public void EntityTag_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var entityTag = new EntityTag();

        // Assert
        entityTag.EntityType.Should().BeEmpty();
        entityTag.EntityId.Should().Be(0);
        entityTag.TagId.Should().Be(0);
        entityTag.TagName.Should().BeNull();
        entityTag.SortOrder.Should().Be(0);
        entityTag.CreatedBy.Should().BeNull();
    }

    [Fact]
    public void EntityTag_ShouldSetAllProperties()
    {
        // Act
        var entityTag = new EntityTag
        {
            EntityType = "Customer",
            EntityId = 100,
            TagId = 5,
            TagName = "VIP",
            SortOrder = 1,
            CreatedBy = 1
        };

        // Assert
        entityTag.EntityType.Should().Be("Customer");
        entityTag.EntityId.Should().Be(100);
        entityTag.TagId.Should().Be(5);
        entityTag.TagName.Should().Be("VIP");
        entityTag.SortOrder.Should().Be(1);
        entityTag.CreatedBy.Should().Be(1);
    }

    [Fact]
    public void EntityTag_ShouldInheritFromBaseEntity()
    {
        // Arrange & Act
        var entityTag = new EntityTag { Id = 1 };

        // Assert
        entityTag.Should().BeAssignableTo<BaseEntity>();
        entityTag.Id.Should().Be(1);
    }

    [Fact]
    public void EntityTag_ShouldHaveTagNavigationProperty()
    {
        // Act
        var tag = new Tag { Id = 1, Name = "Test" };
        var entityTag = new EntityTag
        {
            TagId = 1,
            Tag = tag
        };

        // Assert
        entityTag.Tag.Should().NotBeNull();
        entityTag.Tag!.Name.Should().Be("Test");
    }
}

#endregion

#region Address Entity Tests

public class AddressEntityTests
{
    [Fact]
    public void Address_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var address = new Address();

        // Assert
        address.Label.Should().Be("Primary");
        address.Line1.Should().BeEmpty();
        address.Line2.Should().BeNull();
        address.Line3.Should().BeNull();
        address.City.Should().BeEmpty();
        address.State.Should().BeNull();
        address.PostalCode.Should().BeNull();
        address.County.Should().BeNull();
        address.CountryCode.Should().Be("US");
        address.Country.Should().Be("United States");
        address.IsVerified.Should().BeFalse();
        address.IsPrimary.Should().BeFalse();
    }

    [Fact]
    public void Address_MasterDataLinks_ShouldBeNullByDefault()
    {
        // Act
        var address = new Address();

        // Assert
        address.ZipCodeId.Should().BeNull();
        address.LocalityId.Should().BeNull();
        address.Locality.Should().BeNull();
    }

    [Fact]
    public void Address_GeocodingProperties_ShouldBeNullByDefault()
    {
        // Act
        var address = new Address();

        // Assert
        address.Latitude.Should().BeNull();
        address.Longitude.Should().BeNull();
        address.GeocodeAccuracy.Should().BeNull();
    }

    [Fact]
    public void Address_VerificationProperties_ShouldBeNullByDefault()
    {
        // Act
        var address = new Address();

        // Assert
        address.VerifiedDate.Should().BeNull();
        address.VerificationSource.Should().BeNull();
    }

    [Fact]
    public void Address_AdditionalInfoProperties_ShouldBeNullByDefault()
    {
        // Act
        var address = new Address();

        // Assert
        address.IsResidential.Should().BeNull();
        address.DeliveryInstructions.Should().BeNull();
        address.AccessHours.Should().BeNull();
        address.SiteContactName.Should().BeNull();
        address.SiteContactPhone.Should().BeNull();
        address.Notes.Should().BeNull();
        address.AddressXml.Should().BeNull();
    }

    [Fact]
    public void Address_FormattedAddress_ShouldCombineFields()
    {
        // Arrange
        var address = new Address
        {
            Line1 = "123 Main Street",
            Line2 = "Suite 100",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "United States"
        };

        // Act
        var formatted = address.FormattedAddress;

        // Assert
        formatted.Should().Contain("123 Main Street");
        formatted.Should().Contain("Suite 100");
        formatted.Should().Contain("Springfield");
        formatted.Should().Contain("IL");
        formatted.Should().Contain("62701");
        formatted.Should().Contain("United States");
    }

    [Fact]
    public void Address_FormattedAddress_ShouldSkipNullFields()
    {
        // Arrange
        var address = new Address
        {
            Line1 = "123 Main Street",
            City = "Springfield",
            Country = "United States"
        };

        // Act
        var formatted = address.FormattedAddress;

        // Assert
        formatted.Should().Be("123 Main Street, Springfield, United States");
    }

    [Fact]
    public void Address_GenerateAddressXml_ShouldCreateValidXml()
    {
        // Arrange
        var address = new Address
        {
            Label = "Primary",
            Line1 = "123 Main Street",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "United States"
        };

        // Act
        var xml = address.GenerateAddressXml();

        // Assert
        xml.Should().StartWith("<Address>");
        xml.Should().EndWith("</Address>");
        xml.Should().Contain("<Label>Primary</Label>");
        xml.Should().Contain("<Line1>123 Main Street</Line1>");
        xml.Should().Contain("<City>Springfield</City>");
        xml.Should().Contain("<State>IL</State>");
    }

    [Fact]
    public void Address_GenerateAddressXml_ShouldEscapeSpecialCharacters()
    {
        // Arrange
        var address = new Address
        {
            Line1 = "O'Brien & Associates",
            City = "Springfield"
        };

        // Act
        var xml = address.GenerateAddressXml();

        // Assert
        xml.Should().Contain("&apos;"); // Escaped apostrophe
        xml.Should().Contain("&amp;");  // Escaped ampersand
    }

    [Fact]
    public void Address_UpdateAddressXml_ShouldSetAddressXmlProperty()
    {
        // Arrange
        var address = new Address
        {
            Line1 = "123 Main Street",
            City = "Springfield"
        };

        // Act
        address.UpdateAddressXml();

        // Assert
        address.AddressXml.Should().NotBeNull();
        address.AddressXml.Should().Contain("<Line1>123 Main Street</Line1>");
    }

    [Fact]
    public void Address_ShouldSetAllProperties()
    {
        // Arrange
        var verifiedDate = DateTime.UtcNow;

        // Act
        var address = new Address
        {
            Label = "Office",
            Line1 = "456 Corporate Ave",
            Line2 = "Floor 5",
            Line3 = "Building A",
            City = "Chicago",
            State = "IL",
            PostalCode = "60601",
            County = "Cook",
            CountryCode = "US",
            Country = "United States",
            ZipCodeId = 1,
            LocalityId = 100,
            Locality = "Downtown",
            Latitude = 41.8781m,
            Longitude = -87.6298m,
            GeocodeAccuracy = "High",
            IsVerified = true,
            VerifiedDate = verifiedDate,
            VerificationSource = "USPS",
            IsResidential = false,
            DeliveryInstructions = "Use rear entrance",
            IsPrimary = true,
            CreatedBy = 1,
            UpdatedBy = 2
        };

        // Assert
        address.Label.Should().Be("Office");
        address.Line1.Should().Be("456 Corporate Ave");
        address.Latitude.Should().Be(41.8781m);
        address.Longitude.Should().Be(-87.6298m);
        address.IsVerified.Should().BeTrue();
        address.VerifiedDate.Should().Be(verifiedDate);
        address.IsResidential.Should().BeFalse();
        address.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void Address_ShouldInheritFromBaseEntity()
    {
        // Arrange & Act
        var address = new Address { Id = 1 };

        // Assert
        address.Should().BeAssignableTo<BaseEntity>();
        address.Id.Should().Be(1);
    }
}

#endregion

#region Department Entity Tests

public class DepartmentEntityTests
{
    [Fact]
    public void Department_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var department = new Department();

        // Assert
        department.Name.Should().BeEmpty();
        department.Description.Should().BeEmpty();
        department.DepartmentCode.Should().BeNull();
        department.IsActive.Should().BeTrue();
        department.ParentDepartmentId.Should().BeNull();
    }

    [Fact]
    public void Department_NavigationCollections_ShouldBeInitialized()
    {
        // Act
        var department = new Department();

        // Assert
        department.Users.Should().NotBeNull();
        department.Users.Should().BeEmpty();
        department.Profiles.Should().NotBeNull();
        department.Profiles.Should().BeEmpty();
        department.SubDepartments.Should().NotBeNull();
        department.SubDepartments.Should().BeEmpty();
    }

    [Fact]
    public void Department_ShouldSetAllProperties()
    {
        // Act
        var department = new Department
        {
            Name = "Sales",
            Description = "Sales department responsible for revenue",
            DepartmentCode = "SALES",
            IsActive = true,
            ParentDepartmentId = 1
        };

        // Assert
        department.Name.Should().Be("Sales");
        department.Description.Should().Be("Sales department responsible for revenue");
        department.DepartmentCode.Should().Be("SALES");
        department.IsActive.Should().BeTrue();
        department.ParentDepartmentId.Should().Be(1);
    }

    [Fact]
    public void Department_ShouldInheritFromBaseEntity()
    {
        // Arrange & Act
        var department = new Department { Id = 1, Name = "Test" };

        // Assert
        department.Should().BeAssignableTo<BaseEntity>();
        department.Id.Should().Be(1);
    }

    [Fact]
    public void Department_ShouldSupportHierarchy()
    {
        // Arrange
        var parentDept = new Department { Id = 1, Name = "Corporate" };
        var childDept = new Department
        {
            Id = 2,
            Name = "Sales",
            ParentDepartmentId = 1,
            ParentDepartment = parentDept
        };

        // Act
        parentDept.SubDepartments.Add(childDept);

        // Assert
        parentDept.SubDepartments.Should().HaveCount(1);
        childDept.ParentDepartment.Should().Be(parentDept);
        childDept.ParentDepartmentId.Should().Be(1);
    }

    [Fact]
    public void Department_Users_ShouldBeModifiable()
    {
        // Arrange
        var department = new Department { Id = 1, Name = "Sales" };
        var user = new User { Id = 1, Username = "sales_rep" };

        // Act
        department.Users.Add(user);

        // Assert
        department.Users.Should().HaveCount(1);
        department.Users.First().Username.Should().Be("sales_rep");
    }
}

#endregion

#region BaseEntity Inheritance Tests

public class ActivityNoteTagBaseEntityInheritanceTests
{
    [Fact]
    public void Activity_ShouldHaveBaseEntityProperties()
    {
        // Arrange & Act
        var activity = new Activity
        {
            Id = 100,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        // Assert
        activity.Id.Should().Be(100);
        activity.CreatedAt.Should().BeBefore(DateTime.UtcNow);
        activity.UpdatedAt.Should().NotBeNull();
        activity.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Note_ShouldHaveBaseEntityProperties()
    {
        // Arrange & Act
        var note = new Note
        {
            Id = 200,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = true
        };

        // Assert
        note.Id.Should().Be(200);
        note.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Tag_ShouldHaveBaseEntityProperties()
    {
        // Arrange & Act
        var tag = new Tag
        {
            Id = 300,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        // Assert
        tag.Id.Should().Be(300);
        tag.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Address_ShouldHaveBaseEntityProperties()
    {
        // Arrange & Act
        var address = new Address
        {
            Id = 400,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        // Assert
        address.Id.Should().Be(400);
        address.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Department_ShouldHaveBaseEntityProperties()
    {
        // Arrange & Act
        var department = new Department
        {
            Id = 500,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        // Assert
        department.Id.Should().Be(500);
        department.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void EntityTag_ShouldHaveBaseEntityProperties()
    {
        // Arrange & Act
        var entityTag = new EntityTag
        {
            Id = 600,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        // Assert
        entityTag.Id.Should().Be(600);
        entityTag.IsDeleted.Should().BeFalse();
    }
}

#endregion
