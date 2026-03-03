// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
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
/// Unit tests for NextBestActionService (TODO-AI-04).
/// Covers: account not found → empty; no opportunities → CreateOpportunity; long no contact → ScheduleCall.
/// </summary>
public class NextBestActionServiceTests : ServiceTestFixtureBase<NextBestActionService>
{    private readonly NextBestActionService _sut;

    public NextBestActionServiceTests()
    {        _sut = new NextBestActionService(MockContext.Object, MockLogger.Object);
    }

    [Fact]
    public async Task GetRecommendationsAsync_ShouldReturnEmpty_WhenAccountNotFound()
    {
        // Arrange
        MockContext.Setup(c => c.Accounts)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<Account>()).Object);

        // Act
        var result = await _sut.GetRecommendationsAsync(999);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRecommendationsAsync_ShouldRecommendCreateOpportunity_WhenAccountHasNoOpportunities()
    {
        // Arrange
        var accounts = new List<Account> { new Account { Id = 1, IsDeleted = false } };
        var interactions = new List<Interaction>
        {
            new Interaction { Id = 1, AccountId = 1, IsDeleted = false, InteractionDate = DateTime.UtcNow.AddDays(-5) }
        };
        var opportunities = new List<Opportunity>();
        var tickets = new List<ServiceRequest>();

        MockContext.Setup(c => c.Accounts).Returns(MockDbSetFactory.CreateMockDbSet(accounts).Object);
        MockContext.Setup(c => c.Interactions).Returns(MockDbSetFactory.CreateMockDbSet(interactions).Object);
        MockContext.Setup(c => c.Opportunities).Returns(MockDbSetFactory.CreateMockDbSet(opportunities).Object);
        MockContext.Setup(c => c.ServiceRequests).Returns(MockDbSetFactory.CreateMockDbSet(tickets).Object);

        // Act
        var result = (await _sut.GetRecommendationsAsync(1)).ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(r => r.ActionType == NextBestActionType.CreateOpportunity);
    }

    [Fact]
    public async Task GetRecommendationsAsync_ShouldRecommendScheduleCall_WhenNoContactFor30Days()
    {
        // Arrange
        var accounts = new List<Account> { new Account { Id = 2, IsDeleted = false } };
        // Last contact 35 days ago
        var interactions = new List<Interaction>
        {
            new Interaction { Id = 2, AccountId = 2, IsDeleted = false, InteractionDate = DateTime.UtcNow.AddDays(-35) }
        };
        var opportunities = new List<Opportunity>
        {
            new Opportunity { Id = 10, AccountId = 2, IsDeleted = false, Stage = OpportunityStage.Proposal, Probability = 50, Amount = 5000 }
        };
        var tickets = new List<ServiceRequest>();

        MockContext.Setup(c => c.Accounts).Returns(MockDbSetFactory.CreateMockDbSet(accounts).Object);
        MockContext.Setup(c => c.Interactions).Returns(MockDbSetFactory.CreateMockDbSet(interactions).Object);
        MockContext.Setup(c => c.Opportunities).Returns(MockDbSetFactory.CreateMockDbSet(opportunities).Object);
        MockContext.Setup(c => c.ServiceRequests).Returns(MockDbSetFactory.CreateMockDbSet(tickets).Object);

        // Act
        var result = (await _sut.GetRecommendationsAsync(2)).ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(r => r.ActionType == NextBestActionType.ScheduleCall,
            because: "30+ days without contact triggers a scheduled call recommendation");
    }
}
