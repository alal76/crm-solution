// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services.AI;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.AI;

/// <summary>
/// Unit tests for DealRiskService (TODO-AI-09).
/// Covers: opportunity not found → null; overdue close date → elevated risk; recent activity → lower risk.
/// </summary>
public class DealRiskServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<DealRiskService>> _mockLogger;
    private readonly DealRiskService _sut;

    public DealRiskServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<DealRiskService>>();
        _sut = new DealRiskService(_mockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CalculateRiskAsync_ShouldReturnNull_WhenOpportunityNotFound()
    {
        // Arrange
        _mockContext.Setup(c => c.Opportunities)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<Opportunity>()).Object);

        // Act
        var result = await _sut.CalculateRiskAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CalculateRiskAsync_ShouldReturnHighRisk_WhenCloseDateIsOverdueAndNoActivity()
    {
        // Arrange
        var opps = new List<Opportunity>
        {
            new Opportunity
            {
                Id = 1,
                IsDeleted = false,
                Stage = OpportunityStage.Qualification,
                Probability = 20,
                Amount = 10000,
                ExpectedCloseDate = DateTime.UtcNow.AddDays(-10) // overdue
            }
        };
        var interactions = new List<Interaction>(); // no activity

        _mockContext.Setup(c => c.Opportunities).Returns(MockDbSetFactory.CreateMockDbSet(opps).Object);
        _mockContext.Setup(c => c.Interactions).Returns(MockDbSetFactory.CreateMockDbSet(interactions).Object);

        // Act
        var result = await _sut.CalculateRiskAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.OpportunityId.Should().Be(1);
        result.RiskScore.Should().BeGreaterThan(40, because: "overdue close date + no activity = high risk");
        result.RiskLevel.Should().BeOneOf(DealRiskLevel.High, DealRiskLevel.Critical);
        result.RiskFactors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CalculateRiskAsync_ShouldReturnLowerRisk_WhenOpportunityHasRecentActivityAndGoodProbability()
    {
        // Arrange
        var opps = new List<Opportunity>
        {
            new Opportunity
            {
                Id = 2,
                IsDeleted = false,
                Stage = OpportunityStage.Negotiation,
                Probability = 75,
                Amount = 25000,
                ExpectedCloseDate = DateTime.UtcNow.AddDays(20),
                PrimaryContactId = 5
            }
        };
        // Recent activity 2 days ago
        var interactions = new List<Interaction>
        {
            new Interaction
            {
                Id = 10,
                OpportunityId = 2,
                IsDeleted = false,
                InteractionDate = DateTime.UtcNow.AddDays(-2)
            }
        };

        _mockContext.Setup(c => c.Opportunities).Returns(MockDbSetFactory.CreateMockDbSet(opps).Object);
        _mockContext.Setup(c => c.Interactions).Returns(MockDbSetFactory.CreateMockDbSet(interactions).Object);

        // Act
        var result = await _sut.CalculateRiskAsync(2);

        // Assert
        result.Should().NotBeNull();
        result!.RiskLevel.Should().BeOneOf(DealRiskLevel.Low, DealRiskLevel.Medium);
    }
}
