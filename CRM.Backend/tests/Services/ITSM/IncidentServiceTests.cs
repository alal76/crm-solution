// CRM Solution - Incident Service Tests
// Minimal DTO/enum tests for ITSM module

using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Tests for Incident DTOs and enums.
/// </summary>
public class IncidentServiceTests
{
    [Fact]
    public void IncidentDto_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new IncidentDto();

        // Assert
        dto.IncidentId.Should().Be(0);
        dto.Number.Should().BeEmpty();
        dto.ShortDescription.Should().BeEmpty();
        dto.Description.Should().BeNull();
        dto.CallerId.Should().Be(0);
        dto.CallerName.Should().BeNull();
        dto.CategoryId.Should().BeNull();
        dto.CategoryName.Should().BeNull();
        dto.SubcategoryId.Should().BeNull();
        dto.SubcategoryName.Should().BeNull();
        dto.Priority.Should().Be(0);
        dto.AssignmentGroupId.Should().BeNull();
        dto.AssignmentGroupName.Should().BeNull();
        dto.AssignedToId.Should().BeNull();
        dto.AssignedToName.Should().BeNull();
        dto.ResolutionCode.Should().BeNull();
        dto.ResolutionNotes.Should().BeNull();
        dto.ResolvedAt.Should().BeNull();
        dto.ClosedAt.Should().BeNull();
        dto.SLABreached.Should().BeFalse();
        dto.ResponseDueAt.Should().BeNull();
        dto.ResolutionDueAt.Should().BeNull();
        dto.MajorIncident.Should().BeFalse();
        dto.ProblemId.Should().BeNull();
    }

    [Fact]
    public void IncidentDto_ShouldPopulateAllProperties()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var dto = new IncidentDto
        {
            IncidentId = 1,
            Number = "INC0001234",
            ShortDescription = "System is down",
            Description = "The production system is completely unavailable",
            CallerId = 100,
            CallerName = "John Doe",
            ContactType = ContactType.Phone,
            OpenedAt = now.AddHours(-2),
            CategoryId = 5,
            CategoryName = "Infrastructure",
            SubcategoryId = 10,
            SubcategoryName = "Server",
            Impact = IncidentImpact.High,
            Urgency = IncidentUrgency.High,
            Priority = 1,
            State = IncidentState.InProgress,
            AssignmentGroupId = 3,
            AssignmentGroupName = "IT Support",
            AssignedToId = 50,
            AssignedToName = "Jane Smith",
            ResolutionCode = ResolutionCode.SolvedPermanently,
            ResolutionNotes = "Server was restarted",
            ResolvedAt = now.AddHours(-1),
            ClosedAt = now,
            SLABreached = false,
            ResponseDueAt = now.AddHours(-1),
            ResolutionDueAt = now.AddHours(2),
            MajorIncident = true,
            ProblemId = 5,
            CreatedAt = now.AddHours(-3)
        };

        // Assert
        dto.IncidentId.Should().Be(1);
        dto.Number.Should().Be("INC0001234");
        dto.ShortDescription.Should().Be("System is down");
        dto.Impact.Should().Be(IncidentImpact.High);
        dto.Urgency.Should().Be(IncidentUrgency.High);
        dto.Priority.Should().Be(1);
        dto.State.Should().Be(IncidentState.InProgress);
        dto.MajorIncident.Should().BeTrue();
        dto.SLABreached.Should().BeFalse();
    }

    [Fact]
    public void CreateIncidentDto_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new CreateIncidentDto();

        // Assert
        dto.ShortDescription.Should().BeEmpty();
        dto.Description.Should().BeNull();
        dto.CallerId.Should().Be(0);
        dto.ContactType.Should().Be(ContactType.Portal); // Default per class
        dto.CategoryId.Should().BeNull();
        dto.SubcategoryId.Should().BeNull();
        dto.ConfigurationItemId.Should().BeNull();
    }

    [Fact]
    public void CreateIncidentDto_ShouldPopulateAllProperties()
    {
        // Arrange & Act
        var dto = new CreateIncidentDto
        {
            ShortDescription = "Email not working",
            Description = "User cannot send or receive emails",
            CallerId = 200,
            ContactType = ContactType.Email,
            CategoryId = 3,
            SubcategoryId = 7,
            ConfigurationItemId = 15,
            Impact = IncidentImpact.Medium,
            Urgency = IncidentUrgency.High
        };

        // Assert
        dto.ShortDescription.Should().Be("Email not working");
        dto.CallerId.Should().Be(200);
        dto.ContactType.Should().Be(ContactType.Email);
        dto.Impact.Should().Be(IncidentImpact.Medium);
        dto.Urgency.Should().Be(IncidentUrgency.High);
        dto.ConfigurationItemId.Should().Be(15);
    }

    [Fact]
    public void UpdateIncidentDto_ShouldBeAllNullable()
    {
        // Arrange & Act
        var dto = new UpdateIncidentDto();

        // Assert - all properties should be null for partial updates
        dto.ShortDescription.Should().BeNull();
        dto.Description.Should().BeNull();
        dto.CategoryId.Should().BeNull();
        dto.SubcategoryId.Should().BeNull();
        dto.Impact.Should().BeNull();
        dto.Urgency.Should().BeNull();
        dto.State.Should().BeNull();
        dto.AssignmentGroupId.Should().BeNull();
        dto.AssignedToId.Should().BeNull();
    }

    [Fact]
    public void ResolveIncidentDto_ShouldRequireResolutionCodeAndNotes()
    {
        // Arrange & Act
        var dto = new ResolveIncidentDto
        {
            ResolutionCode = ResolutionCode.SolvedPermanently,
            ResolutionNotes = "Issue was fixed by updating the configuration"
        };

        // Assert
        dto.ResolutionCode.Should().Be(ResolutionCode.SolvedPermanently);
        dto.ResolutionNotes.Should().NotBeEmpty();
    }

    [Fact]
    public void IncidentFilterDto_ShouldHaveDefaultPagination()
    {
        // Arrange & Act
        var dto = new IncidentFilterDto();

        // Assert
        dto.SearchTerm.Should().BeNull();
        dto.State.Should().BeNull();
        dto.Priority.Should().BeNull();
        dto.AssignedToId.Should().BeNull();
        dto.AssignmentGroupId.Should().BeNull();
        dto.SLABreached.Should().BeNull();
        dto.MajorIncident.Should().BeNull();
        dto.CreatedFrom.Should().BeNull();
        dto.CreatedTo.Should().BeNull();
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(20);
    }

    [Fact]
    public void IncidentImpact_ShouldHaveExpectedValues()
    {
        // Arrange & Act & Assert
        ((int)IncidentImpact.High).Should().Be(1);
        ((int)IncidentImpact.Medium).Should().Be(2);
        ((int)IncidentImpact.Low).Should().Be(3);
    }

    [Fact]
    public void IncidentUrgency_ShouldHaveExpectedValues()
    {
        // Arrange & Act & Assert
        ((int)IncidentUrgency.High).Should().Be(1);
        ((int)IncidentUrgency.Medium).Should().Be(2);
        ((int)IncidentUrgency.Low).Should().Be(3);
    }

    [Fact]
    public void IncidentState_ShouldHaveExpectedValues()
    {
        // Arrange & Act & Assert
        ((int)IncidentState.New).Should().Be(1);
        ((int)IncidentState.Assigned).Should().Be(2);
        ((int)IncidentState.InProgress).Should().Be(3);
        ((int)IncidentState.OnHold).Should().Be(4);
        ((int)IncidentState.Resolved).Should().Be(5);
        ((int)IncidentState.Closed).Should().Be(6);
        ((int)IncidentState.Cancelled).Should().Be(7);
    }

    [Fact]
    public void ContactType_ShouldHaveExpectedValues()
    {
        // Arrange & Act & Assert
        ((int)ContactType.Phone).Should().Be(1);
        ((int)ContactType.Email).Should().Be(2);
        ((int)ContactType.Portal).Should().Be(3);
        ((int)ContactType.Chat).Should().Be(4);
        ((int)ContactType.WalkIn).Should().Be(5);
        ((int)ContactType.Monitoring).Should().Be(6);
    }

    [Fact]
    public void ResolutionCode_ShouldHaveExpectedValues()
    {
        // Arrange & Act & Assert
        ((int)ResolutionCode.SolvedPermanently).Should().Be(1);
        ((int)ResolutionCode.SolvedTemporarily).Should().Be(2);
        ((int)ResolutionCode.Workaround).Should().Be(3);
        ((int)ResolutionCode.NotSolvable).Should().Be(4);
        ((int)ResolutionCode.Duplicate).Should().Be(5);
        ((int)ResolutionCode.UserError).Should().Be(6);
        ((int)ResolutionCode.ConfigurationChange).Should().Be(7);
        ((int)ResolutionCode.SoftwareUpdate).Should().Be(8);
        ((int)ResolutionCode.HardwareReplacement).Should().Be(9);
    }
}
