// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.DTOs.ITSM;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Tests for escalation analytics summary DTOs.
/// Validates EscalationAnalyticsSummaryDto structure and helpers.
/// TODO-SD005-011: Escalation Analytics Reports.
/// </summary>
public class EscalationAnalyticsSummaryTests
{
    [Fact]
    public void EscalationAnalyticsSummaryDto_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var dto = new EscalationAnalyticsSummaryDto();

        // Assert
        dto.TotalEscalations.Should().Be(0);
        dto.TotalServiceRequests.Should().Be(0);
        dto.OverallEscalationRate.Should().Be(0);
        dto.AverageTimeToEscalateBySeverity.Should().NotBeNull().And.BeEmpty();
        dto.EscalationRateByCategory.Should().NotBeNull().And.BeEmpty();
        dto.TopEscalatedRequestTypes.Should().NotBeNull().And.BeEmpty();
        dto.ResolutionRateAfterEscalation.Should().Be(0);
        dto.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void EscalationAnalyticsSummaryDto_CanBeFullyPopulated()
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(-30);
        var end = DateTime.UtcNow;

        // Act
        var dto = new EscalationAnalyticsSummaryDto
        {
            PeriodStart = start,
            PeriodEnd = end,
            TotalEscalations = 42,
            TotalServiceRequests = 300,
            OverallEscalationRate = 14.0,
            ResolutionRateAfterEscalation = 88.5,
            AverageTimeToEscalateBySeverity = new List<EscalationTimeBySeverityDto>
            {
                new() { Priority = "Critical", EscalationCount = 10, AverageMinutesToEscalate = 45.5 }
            },
            EscalationRateByCategory = new List<EscalationRateByCategoryDto>
            {
                new() { CategoryId = 1, CategoryName = "Hardware", TotalRequests = 100, EscalatedRequests = 15, EscalationRate = 15.0 }
            },
            TopEscalatedRequestTypes = new List<TopEscalatedRequestTypeDto>
            {
                new() { Rank = 1, CategoryName = "Network", EscalationCount = 20, PercentageOfTotal = 47.6 }
            }
        };

        // Assert
        dto.TotalEscalations.Should().Be(42);
        dto.TotalServiceRequests.Should().Be(300);
        dto.OverallEscalationRate.Should().Be(14.0);
        dto.ResolutionRateAfterEscalation.Should().Be(88.5);
        dto.AverageTimeToEscalateBySeverity.Should().HaveCount(1);
        dto.AverageTimeToEscalateBySeverity[0].Priority.Should().Be("Critical");
        dto.EscalationRateByCategory.Should().HaveCount(1);
        dto.EscalationRateByCategory[0].EscalationRate.Should().Be(15.0);
        dto.TopEscalatedRequestTypes.Should().HaveCount(1);
        dto.TopEscalatedRequestTypes[0].Rank.Should().Be(1);
    }

    [Fact]
    public void EscalationTimeBySeverityDto_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var dto = new EscalationTimeBySeverityDto();

        // Assert
        dto.Priority.Should().BeEmpty();
        dto.EscalationCount.Should().Be(0);
        dto.AverageMinutesToEscalate.Should().Be(0);
    }

    [Fact]
    public void EscalationRateByCategoryDto_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var dto = new EscalationRateByCategoryDto();

        // Assert
        dto.CategoryId.Should().Be(0);
        dto.CategoryName.Should().BeEmpty();
        dto.TotalRequests.Should().Be(0);
        dto.EscalatedRequests.Should().Be(0);
        dto.EscalationRate.Should().Be(0);
    }

    [Fact]
    public void TopEscalatedRequestTypeDto_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var dto = new TopEscalatedRequestTypeDto();

        // Assert
        dto.Rank.Should().Be(0);
        dto.CategoryName.Should().BeEmpty();
        dto.EscalationCount.Should().Be(0);
        dto.PercentageOfTotal.Should().Be(0);
    }
}
