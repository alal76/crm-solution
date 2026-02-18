// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// SPDX-License-Identifier: AGPL-3.0-or-later

using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Unit tests for SLA Service DTOs and entities.
/// Tests the DTOs and helper classes used for SLA management.
/// </summary>
public class SLAServiceTests
{
    #region SLATargetType Enum Tests

    [Fact]
    public void SLATargetType_HasExpectedValues()
    {
        // Assert
        SLATargetType.Incident.Should().BeDefined();
        SLATargetType.ServiceRequest.Should().BeDefined();
        SLATargetType.Problem.Should().BeDefined();
        SLATargetType.Change.Should().BeDefined();
    }

    [Theory]
    [InlineData(SLATargetType.Incident, 1)]
    [InlineData(SLATargetType.ServiceRequest, 2)]
    [InlineData(SLATargetType.Problem, 3)]
    [InlineData(SLATargetType.Change, 4)]
    public void SLATargetType_HasCorrectIntValues(SLATargetType type, int expectedValue)
    {
        // Assert
        ((int)type).Should().Be(expectedValue);
    }

    #endregion

    #region SLAPolicyDto Tests

    [Fact]
    public void SLAPolicyDto_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var dto = new SLAPolicyDto();

        // Assert
        dto.SLAPolicyId.Should().Be(0);
        dto.Name.Should().BeEmpty();
        // Note: SLATargetType is a value type so defaults to 0 (undefined) unless explicitly set
        ((int)dto.TargetType).Should().Be(0);
        dto.P1ResponseMinutes.Should().BeNull();
        dto.P1ResolutionMinutes.Should().BeNull();
        dto.UseBusinessHours.Should().BeFalse();
        dto.IsActive.Should().BeFalse();
    }

    [Fact]
    public void SLAPolicyDto_CanBeFullyPopulated()
    {
        // Arrange & Act
        var dto = new SLAPolicyDto
        {
            SLAPolicyId = 1,
            Name = "Standard SLA",
            TargetType = SLATargetType.Incident,
            P1ResponseMinutes = 15,
            P1ResolutionMinutes = 60,
            UseBusinessHours = true,
            IsActive = true
        };

        // Assert
        dto.Name.Should().Be("Standard SLA");
        dto.P1ResponseMinutes.Should().Be(15);
        dto.P1ResolutionMinutes.Should().Be(60);
        dto.UseBusinessHours.Should().BeTrue();
        dto.IsActive.Should().BeTrue();
    }

    #endregion

    #region SLAInstanceDto Tests

    [Fact]
    public void SLAInstanceDto_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var dto = new SLAInstanceDto();

        // Assert
        dto.SLAInstanceId.Should().Be(0);
        dto.TargetId.Should().Be(0);
        dto.ResponseDueAt.Should().BeNull();
        dto.ResolutionDueAt.Should().BeNull();
        dto.ResponseBreached.Should().BeFalse();
        dto.ResolutionBreached.Should().BeFalse();
        dto.MinutesUntilResponseBreach.Should().BeNull();
        dto.MinutesUntilResolutionBreach.Should().BeNull();
    }

    [Fact]
    public void SLAInstanceDto_CanTrackBreaches()
    {
        // Arrange & Act
        var dto = new SLAInstanceDto
        {
            SLAInstanceId = 1,
            TargetId = 100,
            TargetType = SLATargetType.Incident,
            ResponseDueAt = DateTime.UtcNow.AddMinutes(15),
            ResolutionDueAt = DateTime.UtcNow.AddMinutes(60),
            ResponseBreached = true,
            ResolutionBreached = false,
            MinutesUntilResponseBreach = -5,
            MinutesUntilResolutionBreach = 45
        };

        // Assert
        dto.ResponseBreached.Should().BeTrue();
        dto.ResolutionBreached.Should().BeFalse();
        dto.MinutesUntilResponseBreach.Should().Be(-5);
    }

    #endregion

    #region SLADashboardInfo Tests

    [Fact]
    public void SLADashboardInfo_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var info = new CRM.Core.Interfaces.ITSM.SLADashboardInfo();

        // Assert
        info.TotalActiveSLAs.Should().Be(0);
        info.BreachedCount.Should().Be(0);
        info.AtRiskCount.Should().Be(0);
        info.OnTrackCount.Should().Be(0);
        info.OverallComplianceRate.Should().Be(0);
        info.RecentBreaches.Should().NotBeNull();
        info.AtRiskItems.Should().NotBeNull();
    }

    [Fact]
    public void SLADashboardInfo_CanCalculateComplianceRate()
    {
        // Arrange & Act
        var info = new CRM.Core.Interfaces.ITSM.SLADashboardInfo
        {
            TotalActiveSLAs = 100,
            BreachedCount = 5,
            AtRiskCount = 10,
            OnTrackCount = 85,
            OverallComplianceRate = 95.0
        };

        // Assert
        info.OverallComplianceRate.Should().Be(95.0);
        (info.BreachedCount + info.OnTrackCount + info.AtRiskCount).Should().Be(info.TotalActiveSLAs);
    }

    #endregion

    #region SLAMetricsInfo Tests

    [Fact]
    public void SLAMetricsInfo_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var info = new CRM.Core.Interfaces.ITSM.SLAMetricsInfo();

        // Assert
        info.TotalIncidents.Should().Be(0);
        info.TotalBreaches.Should().Be(0);
        info.ResponseComplianceRate.Should().Be(0);
        info.ResolutionComplianceRate.Should().Be(0);
        info.AverageResponseTimeMinutes.Should().Be(0);
        info.AverageResolutionTimeMinutes.Should().Be(0);
        info.ComplianceByPriority.Should().NotBeNull();
    }

    [Fact]
    public void SLAMetricsInfo_CanTrackDateRange()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 31);

        // Act
        var info = new CRM.Core.Interfaces.ITSM.SLAMetricsInfo
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalIncidents = 500,
            TotalBreaches = 25,
            ResponseComplianceRate = 98.5,
            ResolutionComplianceRate = 95.0,
            ComplianceByPriority = new Dictionary<int, double>
            {
                { 1, 99.0 },
                { 2, 97.0 },
                { 3, 95.0 },
                { 4, 90.0 }
            }
        };

        // Assert
        info.StartDate.Should().Be(startDate);
        info.EndDate.Should().Be(endDate);
        info.ComplianceByPriority.Should().HaveCount(4);
        info.ComplianceByPriority[1].Should().Be(99.0);
    }

    #endregion
}
