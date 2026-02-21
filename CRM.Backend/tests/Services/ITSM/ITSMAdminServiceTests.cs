// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Tests for SLA Policy Admin DTOs (from CRM.Core.Dtos.ITSM namespace).
/// </summary>
public class ITSMAdminServiceTests
{
    [Fact]
    public void SLAPolicyDto_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new CRM.Core.Dtos.ITSM.SLAPolicyDto();

        // Assert
        dto.Id.Should().Be(0);
        dto.Name.Should().BeEmpty();
        dto.Description.Should().BeNull();
        dto.Priority.Should().BeNull();
        dto.Category.Should().BeNull();
        dto.ResponseTimeHours.Should().Be(0);
        dto.ResolutionTimeHours.Should().Be(0);
        dto.BusinessHoursOnly.Should().BeFalse();
        dto.Timezone.Should().Be("UTC");
        dto.BreachAction.Should().BeEmpty();
        dto.IsActive.Should().BeFalse();
    }

    [Fact]
    public void SLAPolicyDto_ShouldPopulateAllProperties()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var dto = new CRM.Core.Dtos.ITSM.SLAPolicyDto
        {
            Id = 1,
            Name = "Critical Priority SLA",
            Description = "SLA for P1 tickets",
            Priority = "Critical",
            Category = "Infrastructure",
            ResponseTimeHours = 1,
            ResolutionTimeHours = 4,
            BusinessHoursOnly = false,
            Timezone = "America/New_York",
            BreachAction = "Escalate",
            IsActive = true,
            CreatedAt = now.AddDays(-30),
            UpdatedAt = now
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.Name.Should().Be("Critical Priority SLA");
        dto.Description.Should().Be("SLA for P1 tickets");
        dto.Priority.Should().Be("Critical");
        dto.Category.Should().Be("Infrastructure");
        dto.ResponseTimeHours.Should().Be(1);
        dto.ResolutionTimeHours.Should().Be(4);
        dto.BusinessHoursOnly.Should().BeFalse();
        dto.Timezone.Should().Be("America/New_York");
        dto.BreachAction.Should().Be("Escalate");
        dto.IsActive.Should().BeTrue();
        dto.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void CreateSLAPolicyDto_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new CreateSLAPolicyDto();

        // Assert
        dto.Name.Should().BeEmpty();
        dto.Description.Should().BeNull();
        dto.Priority.Should().BeNull();
        dto.Category.Should().BeNull();
        dto.ResponseTimeHours.Should().Be(0);
        dto.ResolutionTimeHours.Should().Be(0);
        dto.BusinessHoursOnly.Should().BeTrue(); // Default per class
        dto.Timezone.Should().Be("UTC"); // Default per class
        dto.BreachAction.Should().Be("Notify"); // Default per class
        dto.IsActive.Should().BeTrue(); // Default per class
    }

    [Fact]
    public void CreateSLAPolicyDto_ShouldPopulateAllProperties()
    {
        // Arrange & Act
        var dto = new CreateSLAPolicyDto
        {
            Name = "High Priority SLA",
            Description = "SLA for P2 tickets",
            Priority = "High",
            Category = "Applications",
            ResponseTimeHours = 2,
            ResolutionTimeHours = 8,
            BusinessHoursOnly = true,
            Timezone = "Europe/London",
            BreachAction = "Notify,Escalate",
            IsActive = true
        };

        // Assert
        dto.Name.Should().Be("High Priority SLA");
        dto.Description.Should().Be("SLA for P2 tickets");
        dto.Priority.Should().Be("High");
        dto.Category.Should().Be("Applications");
        dto.ResponseTimeHours.Should().Be(2);
        dto.ResolutionTimeHours.Should().Be(8);
        dto.BusinessHoursOnly.Should().BeTrue();
        dto.Timezone.Should().Be("Europe/London");
        dto.BreachAction.Should().Be("Notify,Escalate");
        dto.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateSLAPolicyDto_ShouldBeAllNullable()
    {
        // Arrange & Act
        var dto = new UpdateSLAPolicyDto();

        // Assert - all properties should be null for partial updates
        dto.Name.Should().BeNull();
        dto.Description.Should().BeNull();
        dto.Priority.Should().BeNull();
        dto.Category.Should().BeNull();
        dto.ResponseTimeHours.Should().BeNull();
        dto.ResolutionTimeHours.Should().BeNull();
        dto.BusinessHoursOnly.Should().BeNull();
        dto.Timezone.Should().BeNull();
        dto.BreachAction.Should().BeNull();
        dto.IsActive.Should().BeNull();
    }

    [Fact]
    public void UpdateSLAPolicyDto_ShouldAcceptPartialUpdates()
    {
        // Arrange & Act
        var dto = new UpdateSLAPolicyDto
        {
            ResponseTimeHours = 4,
            IsActive = false
        };

        // Assert - only set properties should have values
        dto.Name.Should().BeNull();
        dto.ResponseTimeHours.Should().Be(4);
        dto.IsActive.Should().BeFalse();
        dto.ResolutionTimeHours.Should().BeNull();
    }

    [Fact]
    public void SLAInstanceDto_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new CRM.Core.Dtos.ITSM.SLAInstanceDto();

        // Assert
        dto.Id.Should().Be(0);
        dto.ServiceRequestId.Should().Be(0);
        dto.PolicyId.Should().Be(0);
        dto.CurrentStatus.Should().BeEmpty();
        dto.ActualResponseTime.Should().BeNull();
        dto.ActualResolutionTime.Should().BeNull();
        dto.IsBreach.Should().BeFalse();
        dto.BreachTime.Should().BeNull();
    }

    [Fact]
    public void SLAInstanceDto_ShouldPopulateAllProperties()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var dto = new CRM.Core.Dtos.ITSM.SLAInstanceDto
        {
            Id = 100,
            ServiceRequestId = 500,
            PolicyId = 1,
            ResponseTargetTime = now.AddHours(1),
            ResolutionTargetTime = now.AddHours(4),
            CurrentStatus = "InProgress",
            ActualResponseTime = now.AddMinutes(30),
            ActualResolutionTime = null,
            IsBreach = false,
            BreachTime = null,
            CreatedAt = now.AddHours(-1)
        };

        // Assert
        dto.Id.Should().Be(100);
        dto.ServiceRequestId.Should().Be(500);
        dto.PolicyId.Should().Be(1);
        dto.CurrentStatus.Should().Be("InProgress");
        dto.ActualResponseTime.Should().NotBeNull();
        dto.ActualResolutionTime.Should().BeNull();
        dto.IsBreach.Should().BeFalse();
    }

    [Fact]
    public void SLAInstanceDto_ShouldTrackBreachCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var dto = new CRM.Core.Dtos.ITSM.SLAInstanceDto
        {
            Id = 101,
            ServiceRequestId = 501,
            PolicyId = 1,
            ResponseTargetTime = now.AddHours(-2),
            ResolutionTargetTime = now.AddHours(-1),
            CurrentStatus = "Breached",
            IsBreach = true,
            BreachTime = now.AddHours(-1)
        };

        // Assert
        dto.IsBreach.Should().BeTrue();
        dto.BreachTime.Should().NotBeNull();
        dto.CurrentStatus.Should().Be("Breached");
    }
}
