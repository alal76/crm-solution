// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Tests for Problem DTOs and enums.
/// </summary>
public class ProblemServiceTests
{
    [Fact]
    public void ProblemDto_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new ProblemDto();

        // Assert
        dto.Id.Should().Be(0);
        dto.ProblemId.Should().Be(0);
        dto.Number.Should().BeEmpty();
        dto.ShortDescription.Should().BeEmpty();
        dto.Description.Should().BeNull();
        dto.RootCause.Should().BeNull();
        dto.Workaround.Should().BeNull();
        dto.KnownError.Should().BeFalse();
        dto.Solution.Should().BeNull();
        dto.ProblemInvestigatorId.Should().BeNull();
        dto.ProblemInvestigatorName.Should().BeNull();
        dto.RelatedIncidentCount.Should().Be(0);
    }

    [Fact]
    public void ProblemDto_ShouldPopulateAllProperties()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var dto = new ProblemDto
        {
            Id = 1,
            ProblemId = 1,
            Number = "PRB0001234",
            ShortDescription = "Network latency issues",
            Description = "Users experiencing high latency during peak hours",
            Priority = ProblemPriority.High,
            State = ProblemState.RootCauseAnalysis,
            RootCause = "Switch configuration issue",
            Workaround = "Restart the switch daily",
            KnownError = true,
            Solution = "Replace the faulty switch",
            ProblemInvestigatorId = 50,
            ProblemInvestigatorName = "Network Engineer",
            CreatedAt = now.AddDays(-5),
            RelatedIncidentCount = 12
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.Number.Should().Be("PRB0001234");
        dto.ShortDescription.Should().Be("Network latency issues");
        dto.Priority.Should().Be(ProblemPriority.High);
        dto.State.Should().Be(ProblemState.RootCauseAnalysis);
        dto.RootCause.Should().Be("Switch configuration issue");
        dto.Workaround.Should().Be("Restart the switch daily");
        dto.KnownError.Should().BeTrue();
        dto.Solution.Should().Be("Replace the faulty switch");
        dto.RelatedIncidentCount.Should().Be(12);
    }

    [Fact]
    public void CreateProblemDto_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new CreateProblemDto();

        // Assert
        dto.ShortDescription.Should().BeEmpty();
        dto.Description.Should().BeNull();
        dto.CategoryId.Should().BeNull();
        dto.IncidentIds.Should().BeNull();
    }

    [Fact]
    public void CreateProblemDto_ShouldPopulateAllProperties()
    {
        // Arrange & Act
        var dto = new CreateProblemDto
        {
            ShortDescription = "Database connectivity issues",
            Description = "Multiple applications reporting database connection failures",
            Priority = ProblemPriority.Critical,
            CategoryId = 5,
            IncidentIds = new List<int> { 1, 2, 3, 4 }
        };

        // Assert
        dto.ShortDescription.Should().Be("Database connectivity issues");
        dto.Description.Should().NotBeNull();
        dto.Priority.Should().Be(ProblemPriority.Critical);
        dto.CategoryId.Should().Be(5);
        dto.IncidentIds.Should().HaveCount(4);
    }

    [Fact]
    public void UpdateProblemDto_ShouldBeAllNullable()
    {
        // Arrange & Act
        var dto = new UpdateProblemDto();

        // Assert - all properties should be null for partial updates
        dto.ShortDescription.Should().BeNull();
        dto.Description.Should().BeNull();
        dto.State.Should().BeNull();
        dto.RootCause.Should().BeNull();
        dto.Workaround.Should().BeNull();
        dto.Solution.Should().BeNull();
        dto.Resolution.Should().BeNull();
        dto.ClosureComments.Should().BeNull();
        dto.KnownError.Should().BeNull();
        dto.ProblemInvestigatorId.Should().BeNull();
    }

    [Fact]
    public void ProblemFilterDto_ShouldHaveDefaultPagination()
    {
        // Arrange & Act
        var dto = new ProblemFilterDto();

        // Assert
        dto.SearchTerm.Should().BeNull();
        dto.State.Should().BeNull();
        dto.Priority.Should().BeNull();
        dto.KnownError.Should().BeNull();
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(20);
    }

    [Fact]
    public void ProblemState_ShouldHaveExpectedValues()
    {
        // Arrange & Act & Assert
        ((int)ProblemState.New).Should().Be(1);
        ((int)ProblemState.Investigating).Should().Be(2);
        ((int)ProblemState.RootCauseAnalysis).Should().Be(3);
        ((int)ProblemState.KnownError).Should().Be(4);
        ((int)ProblemState.Resolved).Should().Be(5);
        ((int)ProblemState.Closed).Should().Be(6);
        ((int)ProblemState.Cancelled).Should().Be(7);
    }

    [Fact]
    public void ProblemPriority_ShouldHaveExpectedValues()
    {
        // Arrange & Act & Assert
        ((int)ProblemPriority.Critical).Should().Be(1);
        ((int)ProblemPriority.High).Should().Be(2);
        ((int)ProblemPriority.Medium).Should().Be(3);
        ((int)ProblemPriority.Low).Should().Be(4);
    }

    [Fact]
    public void ProblemPriority_ShouldBeOrderedByServerity()
    {
        // Arrange - higher number = lower priority
        var priorities = new[] { ProblemPriority.Critical, ProblemPriority.High, ProblemPriority.Medium, ProblemPriority.Low };

        // Act & Assert
        for (int i = 0; i < priorities.Length - 1; i++)
        {
            ((int)priorities[i]).Should().BeLessThan((int)priorities[i + 1]);
        }
    }
}
