// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Threading;
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
/// Unit tests for ChurnPredictionService (TODO-AI-03).
/// Covers: account not found → null; stale account → high risk; active account → low risk.
/// </summary>
public class ChurnPredictionServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<ChurnPredictionService>> _mockLogger;
    private readonly ChurnPredictionService _sut;

    public ChurnPredictionServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ChurnPredictionService>>();
        _sut = new ChurnPredictionService(_mockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task PredictChurnAsync_ShouldReturnNull_WhenAccountNotFound()
    {
        // Arrange
        var accounts = new List<Account>();
        _mockContext.Setup(c => c.Accounts).Returns(MockDbSetFactory.CreateMockDbSet(accounts).Object);

        // Act
        var result = await _sut.PredictChurnAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task PredictChurnAsync_ShouldIndicateHighRisk_WhenAccountHasNoRecentActivityAndOpenTickets()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, IsDeleted = false, Priority = AccountPriority.Low }
        };
        // Interactions: last one 50 days ago → stale
        var interactions = new List<Interaction>
        {
            new Interaction { Id = 1, AccountId = 1, IsDeleted = false, InteractionDate = DateTime.UtcNow.AddDays(-50) }
        };
        // 3 open tickets
        var tickets = new List<ServiceRequest>
        {
            new ServiceRequest { Id = 1, AccountId = 1, IsDeleted = false, Status = ServiceRequestStatus.Open },
            new ServiceRequest { Id = 2, AccountId = 1, IsDeleted = false, Status = ServiceRequestStatus.Open },
            new ServiceRequest { Id = 3, AccountId = 1, IsDeleted = false, Status = ServiceRequestStatus.InProgress }
        };
        // No open opportunities
        var opportunities = new List<Opportunity>();

        _mockContext.Setup(c => c.Accounts).Returns(MockDbSetFactory.CreateMockDbSet(accounts).Object);
        _mockContext.Setup(c => c.Interactions).Returns(MockDbSetFactory.CreateMockDbSet(interactions).Object);
        _mockContext.Setup(c => c.ServiceRequests).Returns(MockDbSetFactory.CreateMockDbSet(tickets).Object);
        _mockContext.Setup(c => c.Opportunities).Returns(MockDbSetFactory.CreateMockDbSet(opportunities).Object);

        // Act
        var result = await _sut.PredictChurnAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.AccountId.Should().Be(1);
        result!.ChurnProbability.Should().BeGreaterThan(0.40, because: "stale interactions and open tickets drive high risk");
        result.RiskLevel.Should().BeOneOf(ChurnRiskLevel.Medium, ChurnRiskLevel.High);
    }

    [Fact]
    public async Task PredictChurnAsync_ShouldIndicateLowRisk_WhenAccountHasRecentInteractionAndNoTickets()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 2, IsDeleted = false, Priority = AccountPriority.High }
        };
        var interactions = new List<Interaction>
        {
            new Interaction { Id = 10, AccountId = 2, IsDeleted = false, InteractionDate = DateTime.UtcNow.AddDays(-3) }
        };
        var tickets = new List<ServiceRequest>();
        var opportunities = new List<Opportunity>
        {
            new Opportunity { Id = 5, AccountId = 2, IsDeleted = false, Stage = OpportunityStage.Proposal, Probability = 60 }
        };

        _mockContext.Setup(c => c.Accounts).Returns(MockDbSetFactory.CreateMockDbSet(accounts).Object);
        _mockContext.Setup(c => c.Interactions).Returns(MockDbSetFactory.CreateMockDbSet(interactions).Object);
        _mockContext.Setup(c => c.ServiceRequests).Returns(MockDbSetFactory.CreateMockDbSet(tickets).Object);
        _mockContext.Setup(c => c.Opportunities).Returns(MockDbSetFactory.CreateMockDbSet(opportunities).Object);

        // Act
        var result = await _sut.PredictChurnAsync(2);

        // Assert
        result.Should().NotBeNull();
        result!.ChurnProbability.Should().BeLessThan(0.30, because: "recent interaction and active opportunity signal engagement");
        result.RiskLevel.Should().Be(ChurnRiskLevel.Low);
    }
}
