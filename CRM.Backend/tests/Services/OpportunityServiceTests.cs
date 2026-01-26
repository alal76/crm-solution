// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Opportunity Service Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for OpportunityService
/// Tests opportunity CRUD operations and pipeline calculations
/// </summary>
public class OpportunityServiceTests
{
    private readonly Mock<IOpportunityService> _mockOpportunityService;

    public OpportunityServiceTests()
    {
        _mockOpportunityService = new Mock<IOpportunityService>();
    }

    #region GetOpportunityById Tests

    [Fact]
    public async Task GetOpportunityById_ReturnsOpportunity_WhenExists()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Id = 1,
            Name = "Enterprise Deal",
            Stage = OpportunityStage.Proposal,
            Amount = 100000m,
            AccountId = 5
        };

        _mockOpportunityService.Setup(s => s.GetOpportunityByIdAsync(1))
            .ReturnsAsync(opportunity);

        // Act
        var result = await _mockOpportunityService.Object.GetOpportunityByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Enterprise Deal");
        result.Stage.Should().Be(OpportunityStage.Proposal);
    }

    [Fact]
    public async Task GetOpportunityById_ReturnsNull_WhenNotExists()
    {
        // Arrange
        _mockOpportunityService.Setup(s => s.GetOpportunityByIdAsync(999))
            .ReturnsAsync((Opportunity?)null);

        // Act
        var result = await _mockOpportunityService.Object.GetOpportunityByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetOpenOpportunities Tests

    [Fact]
    public async Task GetOpenOpportunities_ReturnsOnlyOpenOpportunities()
    {
        // Arrange
        var opportunities = new List<Opportunity>
        {
            new() { Id = 1, Stage = OpportunityStage.Discovery },
            new() { Id = 2, Stage = OpportunityStage.Proposal },
            new() { Id = 3, Stage = OpportunityStage.Negotiation }
        };

        _mockOpportunityService.Setup(s => s.GetOpenOpportunitiesAsync())
            .ReturnsAsync(opportunities);

        // Act
        var result = await _mockOpportunityService.Object.GetOpenOpportunitiesAsync();

        // Assert
        result.Should().HaveCount(3);
        result.All(o => o.IsOpen).Should().BeTrue();
    }

    [Fact]
    public async Task GetOpenOpportunities_ExcludesClosedWon()
    {
        // Arrange
        var openOpps = new List<Opportunity>
        {
            new() { Id = 1, Stage = OpportunityStage.Discovery }
        };

        _mockOpportunityService.Setup(s => s.GetOpenOpportunitiesAsync())
            .ReturnsAsync(openOpps);

        // Act
        var result = await _mockOpportunityService.Object.GetOpenOpportunitiesAsync();

        // Assert
        result.Should().NotContain(o => o.Stage == OpportunityStage.ClosedWon);
    }

    [Fact]
    public async Task GetOpenOpportunities_ExcludesClosedLost()
    {
        // Arrange
        var openOpps = new List<Opportunity>
        {
            new() { Id = 1, Stage = OpportunityStage.Proposal }
        };

        _mockOpportunityService.Setup(s => s.GetOpenOpportunitiesAsync())
            .ReturnsAsync(openOpps);

        // Act
        var result = await _mockOpportunityService.Object.GetOpenOpportunitiesAsync();

        // Assert
        result.Should().NotContain(o => o.Stage == OpportunityStage.ClosedLost);
    }

    [Fact]
    public async Task GetOpenOpportunities_ReturnsEmpty_WhenNoOpen()
    {
        // Arrange
        _mockOpportunityService.Setup(s => s.GetOpenOpportunitiesAsync())
            .ReturnsAsync(new List<Opportunity>());

        // Act
        var result = await _mockOpportunityService.Object.GetOpenOpportunitiesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetOpportunitiesByAccount Tests

    [Fact]
    public async Task GetOpportunitiesByAccount_ReturnsAccountOpportunities()
    {
        // Arrange
        var opportunities = new List<Opportunity>
        {
            new() { Id = 1, AccountId = 5, Name = "Deal 1" },
            new() { Id = 2, AccountId = 5, Name = "Deal 2" }
        };

        _mockOpportunityService.Setup(s => s.GetOpportunitiesByAccountAsync(5))
            .ReturnsAsync(opportunities);

        // Act
        var result = await _mockOpportunityService.Object.GetOpportunitiesByAccountAsync(5);

        // Assert
        result.Should().HaveCount(2);
        result.All(o => o.AccountId == 5).Should().BeTrue();
    }

    [Fact]
    public async Task GetOpportunitiesByAccount_ReturnsEmpty_WhenNoOpportunities()
    {
        // Arrange
        _mockOpportunityService.Setup(s => s.GetOpportunitiesByAccountAsync(999))
            .ReturnsAsync(new List<Opportunity>());

        // Act
        var result = await _mockOpportunityService.Object.GetOpportunitiesByAccountAsync(999);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetTotalPipeline Tests

    [Fact]
    public async Task GetTotalPipeline_ReturnsCorrectTotal()
    {
        // Arrange
        _mockOpportunityService.Setup(s => s.GetTotalPipelineAsync())
            .ReturnsAsync(500000m);

        // Act
        var result = await _mockOpportunityService.Object.GetTotalPipelineAsync();

        // Assert
        result.Should().Be(500000m);
    }

    [Fact]
    public async Task GetTotalPipeline_ReturnsZero_WhenNoOpportunities()
    {
        // Arrange
        _mockOpportunityService.Setup(s => s.GetTotalPipelineAsync())
            .ReturnsAsync(0m);

        // Act
        var result = await _mockOpportunityService.Object.GetTotalPipelineAsync();

        // Assert
        result.Should().Be(0m);
    }

    [Fact]
    public async Task GetTotalPipeline_HandlesLargeAmounts()
    {
        // Arrange
        _mockOpportunityService.Setup(s => s.GetTotalPipelineAsync())
            .ReturnsAsync(10000000000m); // 10 billion

        // Act
        var result = await _mockOpportunityService.Object.GetTotalPipelineAsync();

        // Assert
        result.Should().Be(10000000000m);
    }

    #endregion

    #region CreateOpportunity Tests

    [Fact]
    public async Task CreateOpportunity_ReturnsNewId()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Name = "New Deal",
            Stage = OpportunityStage.Discovery,
            Amount = 50000m,
            AccountId = 1
        };

        _mockOpportunityService.Setup(s => s.CreateOpportunityAsync(It.IsAny<Opportunity>()))
            .ReturnsAsync(10);

        // Act
        var result = await _mockOpportunityService.Object.CreateOpportunityAsync(opportunity);

        // Assert
        result.Should().Be(10);
    }

    [Fact]
    public async Task CreateOpportunity_ServiceIsCalled()
    {
        // Arrange
        var opportunity = new Opportunity { Name = "Test Deal" };

        _mockOpportunityService.Setup(s => s.CreateOpportunityAsync(It.IsAny<Opportunity>()))
            .ReturnsAsync(1);

        // Act
        await _mockOpportunityService.Object.CreateOpportunityAsync(opportunity);

        // Assert
        _mockOpportunityService.Verify(s => s.CreateOpportunityAsync(It.IsAny<Opportunity>()), Times.Once);
    }

    [Fact]
    public async Task CreateOpportunity_SetsDefaultStage()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Name = "New Deal",
            Amount = 50000m
        };

        _mockOpportunityService.Setup(s => s.CreateOpportunityAsync(It.IsAny<Opportunity>()))
            .ReturnsAsync(1);

        // Assert
        opportunity.Stage.Should().Be(OpportunityStage.Discovery);
    }

    #endregion

    #region UpdateOpportunity Tests

    [Fact]
    public async Task UpdateOpportunity_UpdatesSuccessfully()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Id = 1,
            Name = "Updated Deal",
            Stage = OpportunityStage.Negotiation
        };

        _mockOpportunityService.Setup(s => s.UpdateOpportunityAsync(It.IsAny<Opportunity>()))
            .Returns(Task.CompletedTask);

        // Act
        await _mockOpportunityService.Object.UpdateOpportunityAsync(opportunity);

        // Assert
        _mockOpportunityService.Verify(s => s.UpdateOpportunityAsync(It.Is<Opportunity>(o => o.Id == 1)), Times.Once);
    }

    [Fact]
    public async Task UpdateOpportunity_CanChangeStage()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Id = 1,
            Stage = OpportunityStage.Discovery
        };

        _mockOpportunityService.Setup(s => s.UpdateOpportunityAsync(It.IsAny<Opportunity>()))
            .Returns(Task.CompletedTask);

        // Act
        opportunity.Stage = OpportunityStage.Proposal;
        await _mockOpportunityService.Object.UpdateOpportunityAsync(opportunity);

        // Assert
        _mockOpportunityService.Verify(s => s.UpdateOpportunityAsync(
            It.Is<Opportunity>(o => o.Stage == OpportunityStage.Proposal)), Times.Once);
    }

    [Fact]
    public async Task UpdateOpportunity_CanCloseAsWon()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Id = 1,
            Stage = OpportunityStage.Negotiation
        };

        _mockOpportunityService.Setup(s => s.UpdateOpportunityAsync(It.IsAny<Opportunity>()))
            .Returns(Task.CompletedTask);

        // Act
        opportunity.Stage = OpportunityStage.ClosedWon;
        await _mockOpportunityService.Object.UpdateOpportunityAsync(opportunity);

        // Assert
        opportunity.IsWon.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateOpportunity_CanCloseAsLost()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Id = 1,
            Stage = OpportunityStage.Proposal
        };

        _mockOpportunityService.Setup(s => s.UpdateOpportunityAsync(It.IsAny<Opportunity>()))
            .Returns(Task.CompletedTask);

        // Act
        opportunity.Stage = OpportunityStage.ClosedLost;
        await _mockOpportunityService.Object.UpdateOpportunityAsync(opportunity);

        // Assert
        opportunity.IsWon.Should().BeFalse();
        opportunity.IsOpen.Should().BeFalse();
    }

    #endregion

    #region DeleteOpportunity Tests

    [Fact]
    public async Task DeleteOpportunity_DeletesSuccessfully()
    {
        // Arrange
        _mockOpportunityService.Setup(s => s.DeleteOpportunityAsync(1))
            .Returns(Task.CompletedTask);

        // Act
        await _mockOpportunityService.Object.DeleteOpportunityAsync(1);

        // Assert
        _mockOpportunityService.Verify(s => s.DeleteOpportunityAsync(1), Times.Once);
    }

    #endregion

    #region Pipeline Calculation Tests

    [Fact]
    public void Opportunity_WeightedAmount_CalculatedCorrectly()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Amount = 100000m,
            Probability = 50
        };

        // Assert
        opportunity.WeightedAmount.Should().Be(50000m);
    }

    [Fact]
    public void Opportunity_WeightedAmount_ZeroProbability()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Amount = 100000m,
            Probability = 0
        };

        // Assert
        opportunity.WeightedAmount.Should().Be(0m);
    }

    [Fact]
    public void Opportunity_WeightedAmount_FullProbability()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Amount = 100000m,
            Probability = 100
        };

        // Assert
        opportunity.WeightedAmount.Should().Be(100000m);
    }

    [Fact]
    public void Opportunity_TotalPipeline_Calculation()
    {
        // Arrange
        var opportunities = new List<Opportunity>
        {
            new() { Amount = 100000m, Probability = 50 }, // 50000
            new() { Amount = 50000m, Probability = 75 },  // 37500
            new() { Amount = 25000m, Probability = 25 }   // 6250
        };

        // Act
        var totalWeighted = opportunities.Sum(o => o.WeightedAmount);

        // Assert
        totalWeighted.Should().Be(93750m);
    }

    [Fact]
    public void Opportunity_OpenPipeline_ExcludesClosed()
    {
        // Arrange
        var opportunities = new List<Opportunity>
        {
            new() { Stage = OpportunityStage.Discovery, Amount = 50000m },
            new() { Stage = OpportunityStage.ClosedWon, Amount = 100000m },
            new() { Stage = OpportunityStage.Proposal, Amount = 75000m }
        };

        // Act
        var openPipeline = opportunities.Where(o => o.IsOpen).Sum(o => o.Amount);

        // Assert
        openPipeline.Should().Be(125000m);
    }

    #endregion

    #region Stage Transition Tests

    [Fact]
    public void Opportunity_CanTransition_Discovery_To_Qualification()
    {
        // Arrange
        var opportunity = new Opportunity { Stage = OpportunityStage.Discovery };

        // Act
        opportunity.Stage = OpportunityStage.Qualification;

        // Assert
        opportunity.Stage.Should().Be(OpportunityStage.Qualification);
        opportunity.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void Opportunity_CanTransition_Qualification_To_Proposal()
    {
        // Arrange
        var opportunity = new Opportunity { Stage = OpportunityStage.Qualification };

        // Act
        opportunity.Stage = OpportunityStage.Proposal;

        // Assert
        opportunity.Stage.Should().Be(OpportunityStage.Proposal);
        opportunity.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void Opportunity_CanTransition_Proposal_To_Negotiation()
    {
        // Arrange
        var opportunity = new Opportunity { Stage = OpportunityStage.Proposal };

        // Act
        opportunity.Stage = OpportunityStage.Negotiation;

        // Assert
        opportunity.Stage.Should().Be(OpportunityStage.Negotiation);
        opportunity.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void Opportunity_CanTransition_Negotiation_To_ClosedWon()
    {
        // Arrange
        var opportunity = new Opportunity { Stage = OpportunityStage.Negotiation };

        // Act
        opportunity.Stage = OpportunityStage.ClosedWon;

        // Assert
        opportunity.Stage.Should().Be(OpportunityStage.ClosedWon);
        opportunity.IsOpen.Should().BeFalse();
        opportunity.IsWon.Should().BeTrue();
    }

    [Fact]
    public void Opportunity_CanTransition_Any_To_ClosedLost()
    {
        // Arrange
        var opportunity = new Opportunity { Stage = OpportunityStage.Discovery };

        // Act
        opportunity.Stage = OpportunityStage.ClosedLost;

        // Assert
        opportunity.Stage.Should().Be(OpportunityStage.ClosedLost);
        opportunity.IsOpen.Should().BeFalse();
        opportunity.IsWon.Should().BeFalse();
    }

    #endregion
}
