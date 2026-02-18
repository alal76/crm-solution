// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.AI;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.AI;

public class AIOpportunityScoringServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<AIOpportunityScoringService>> _mockLogger;
    private readonly AIOpportunityScoringService _service;

    public AIOpportunityScoringServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<AIOpportunityScoringService>>();

        _service = new AIOpportunityScoringService(
            _mockContext.Object,
            _mockLogger.Object);
    }

    // ========================================================================
    // ScoreOpportunityAsync - basic
    // ========================================================================

    [Fact]
    public async Task ScoreOpportunityAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var opps = new List<Opportunity>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(opps);
        _mockContext.Setup(c => c.Opportunities).Returns(mockSet.Object);

        // Act
        var result = await _service.ScoreOpportunityAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ScoreOpportunityAsync_ShouldReturnResult_WhenExists()
    {
        // Arrange
        var opp = CreateOpportunity(1, "Big Deal", OpportunityStage.Proposal, 50000m);
        var opps = new List<Opportunity> { opp };
        var mockSet = MockDbSetFactory.CreateMockDbSet(opps);
        _mockContext.Setup(c => c.Opportunities).Returns(mockSet.Object);

        // Act
        var result = await _service.ScoreOpportunityAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.OpportunityId.Should().Be(1);
        result.Name.Should().Be("Big Deal");
        result.WinProbability.Should().BeInRange(0, 100);
        result.RiskLevel.Should().NotBeNullOrEmpty();
        result.ScoredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ========================================================================
    // Stage-based probabilities
    // ========================================================================

    [Theory]
    [InlineData(OpportunityStage.Discovery)]
    [InlineData(OpportunityStage.Qualification)]
    [InlineData(OpportunityStage.Proposal)]
    [InlineData(OpportunityStage.Negotiation)]
    public async Task ScoreOpportunityAsync_ShouldAssignHigherProbabilityForLaterStages(OpportunityStage stage)
    {
        // Arrange
        var opp = CreateOpportunity(1, "Deal", stage, 50000m);
        var opps = new List<Opportunity> { opp };
        var mockSet = MockDbSetFactory.CreateMockDbSet(opps);
        _mockContext.Setup(c => c.Opportunities).Returns(mockSet.Object);

        // Act
        var result = await _service.ScoreOpportunityAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.WinProbability.Should().BeGreaterThan(0);
        result.Breakdown.Should().NotBeNull();
        result.Breakdown.StageScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ScoreOpportunityAsync_NegotiationShouldScoreHigherThanDiscovery()
    {
        // Arrange
        var discoveryOpp = CreateOpportunity(1, "Early Deal", OpportunityStage.Discovery, 50000m);
        var negotiationOpp = CreateOpportunity(2, "Late Deal", OpportunityStage.Negotiation, 50000m);
        var opps = new List<Opportunity> { discoveryOpp, negotiationOpp };
        var mockSet = MockDbSetFactory.CreateMockDbSet(opps);
        _mockContext.Setup(c => c.Opportunities).Returns(mockSet.Object);

        // Act
        var earlyResult = await _service.ScoreOpportunityAsync(1);
        var lateResult = await _service.ScoreOpportunityAsync(2);

        // Assert
        lateResult!.Breakdown.StageScore.Should().BeGreaterThan(earlyResult!.Breakdown.StageScore);
    }

    // ========================================================================
    // Risk levels
    // ========================================================================

    [Fact]
    public async Task ScoreOpportunityAsync_ShouldAssignRiskLevel()
    {
        // Arrange
        var opp = CreateOpportunity(1, "Deal", OpportunityStage.Proposal, 50000m);
        var opps = new List<Opportunity> { opp };
        var mockSet = MockDbSetFactory.CreateMockDbSet(opps);
        _mockContext.Setup(c => c.Opportunities).Returns(mockSet.Object);

        // Act
        var result = await _service.ScoreOpportunityAsync(1);

        // Assert
        result!.RiskLevel.Should().BeOneOf("Low", "Medium", "High");
    }

    [Fact]
    public async Task ScoreOpportunityAsync_ShouldReturnRiskFactorsAndPositiveSignals()
    {
        // Arrange
        var opp = CreateOpportunity(1, "Deal", OpportunityStage.Proposal, 50000m);
        opp.ExpectedCloseDate = DateTime.UtcNow.AddDays(30);
        var opps = new List<Opportunity> { opp };
        var mockSet = MockDbSetFactory.CreateMockDbSet(opps);
        _mockContext.Setup(c => c.Opportunities).Returns(mockSet.Object);

        // Act
        var result = await _service.ScoreOpportunityAsync(1);

        // Assert
        result!.RiskFactors.Should().NotBeNull();
        result.PositiveSignals.Should().NotBeNull();
    }

    // ========================================================================
    // ScoreAllOpenAsync
    // ========================================================================

    [Fact]
    public async Task ScoreAllOpenAsync_ShouldScoreOnlyOpenOpportunities()
    {
        // Arrange
        var opps = new List<Opportunity>
        {
            CreateOpportunity(1, "Open Deal", OpportunityStage.Proposal, 30000m),
            CreateOpportunity(2, "Won Deal", OpportunityStage.ClosedWon, 50000m),
            CreateOpportunity(3, "Lost Deal", OpportunityStage.ClosedLost, 20000m)
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(opps);
        _mockContext.Setup(c => c.Opportunities).Returns(mockSet.Object);

        // Act
        var results = await _service.ScoreAllOpenAsync();

        // Assert
        results.Should().HaveCount(1); // Only the open one
        results.First().OpportunityId.Should().Be(1);
    }

    [Fact]
    public async Task ScoreAllOpenAsync_ShouldExcludeDeletedOpportunities()
    {
        // Arrange
        var opps = new List<Opportunity>
        {
            CreateOpportunity(1, "Active", OpportunityStage.Proposal, 30000m),
            CreateOpportunity(2, "Deleted", OpportunityStage.Qualification, 10000m)
        };
        opps[1].IsDeleted = true;
        var mockSet = MockDbSetFactory.CreateMockDbSet(opps);
        _mockContext.Setup(c => c.Opportunities).Returns(mockSet.Object);

        // Act
        var results = await _service.ScoreAllOpenAsync();

        // Assert
        results.Should().HaveCount(1);
        results.First().OpportunityId.Should().Be(1);
    }

    // ========================================================================
    // GetHistoricalWinRatesAsync
    // ========================================================================

    [Fact]
    public async Task GetHistoricalWinRatesAsync_ShouldReturnRatesByStage()
    {
        // Arrange
        var opps = new List<Opportunity>
        {
            CreateOpportunity(1, "Won A", OpportunityStage.ClosedWon, 10000m),
            CreateOpportunity(2, "Won B", OpportunityStage.ClosedWon, 20000m),
            CreateOpportunity(3, "Lost C", OpportunityStage.ClosedLost, 15000m)
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(opps);
        _mockContext.Setup(c => c.Opportunities).Returns(mockSet.Object);

        // Act
        var rates = await _service.GetHistoricalWinRatesAsync();

        // Assert
        rates.Should().NotBeNull();
        rates.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetHistoricalWinRatesAsync_ShouldReturnEmptyDict_WhenNoClosedDeals()
    {
        // Arrange
        var opps = new List<Opportunity>
        {
            CreateOpportunity(1, "Open", OpportunityStage.Proposal, 10000m)
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(opps);
        _mockContext.Setup(c => c.Opportunities).Returns(mockSet.Object);

        // Act
        var rates = await _service.GetHistoricalWinRatesAsync();

        // Assert
        rates.Should().NotBeNull();
    }

    // ========================================================================
    // Breakdown scores
    // ========================================================================

    [Fact]
    public async Task ScoreOpportunityAsync_BreakdownShouldHaveAllComponents()
    {
        // Arrange
        var opp = CreateOpportunity(1, "Full Deal", OpportunityStage.Qualification, 75000m);
        opp.ExpectedCloseDate = DateTime.UtcNow.AddDays(45);
        opp.Probability = 50;
        var opps = new List<Opportunity> { opp };
        var mockSet = MockDbSetFactory.CreateMockDbSet(opps);
        _mockContext.Setup(c => c.Opportunities).Returns(mockSet.Object);

        // Act
        var result = await _service.ScoreOpportunityAsync(1);

        // Assert
        var breakdown = result!.Breakdown;
        breakdown.StageScore.Should().BeGreaterThanOrEqualTo(0);
        breakdown.DealSizeScore.Should().BeGreaterThanOrEqualTo(0);
        breakdown.VelocityScore.Should().BeGreaterThanOrEqualTo(0);
        breakdown.CompletenessScore.Should().BeGreaterThanOrEqualTo(0);
    }

    // ========================================================================
    // Helpers
    // ========================================================================

    private static Opportunity CreateOpportunity(int id, string name, OpportunityStage stage, decimal amount)
    {
        return new Opportunity
        {
            Id = id,
            Name = name,
            Stage = stage,
            Amount = amount,
            Probability = (int)stage * 20 + 10,
            ExpectedCloseDate = DateTime.UtcNow.AddDays(60),
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            IsDeleted = false
        };
    }
}
