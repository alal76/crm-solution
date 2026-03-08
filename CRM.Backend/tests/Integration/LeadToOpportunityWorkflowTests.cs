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
using CRM.Core.Dtos;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CRM.Tests.Integration;

/// <summary>
/// Cross-service workflow tests covering the Lead → Opportunity pipeline.
/// Uses pure mock pattern — no InMemory DB required.
/// SPEC: SPEC-SALES-001/002
/// </summary>
public class LeadToOpportunityWorkflowTests
{
    private readonly Mock<ILeadService> _leadService = new(MockBehavior.Loose);
    private readonly Mock<IOpportunityService> _oppService = new(MockBehavior.Loose);

    #region Lead Conversion Tests

    [Fact]
    public async Task ConvertLead_ShouldReturnOpportunityId_WhenLeadIsValid()
    {
        // Arrange
        const int leadId = 1;
        const int expectedOppId = 5;
        _leadService
            .Setup(s => s.ConvertAsync(leadId, "New Opportunity", null, null, null))
            .ReturnsAsync((expectedOppId, leadId));

        // Act
        var result = await _leadService.Object.ConvertAsync(leadId, "New Opportunity", null, null, null);

        // Assert
        result.OpportunityId.Should().Be(expectedOppId);
        result.LeadId.Should().Be(leadId);
    }

    [Fact]
    public async Task ConvertLead_ShouldThrowArgumentException_WhenLeadIdIsZero()
    {
        // Arrange
        _leadService
            .Setup(s => s.ConvertAsync(0, It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<decimal?>(), It.IsAny<DateTime?>()))
            .ThrowsAsync(new ArgumentException("Lead ID must be greater than zero.", "id"));

        // Act
        var act = async () => await _leadService.Object.ConvertAsync(0, "Opp", null, null, null);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Lead ID must be greater than zero*");
    }

    [Fact]
    public async Task ConvertLead_ShouldThrowArgumentException_WhenLeadIdIsNegative()
    {
        // Arrange
        _leadService
            .Setup(s => s.ConvertAsync(-1, It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<decimal?>(), It.IsAny<DateTime?>()))
            .ThrowsAsync(new ArgumentException("Lead ID must be greater than zero.", "id"));

        // Act
        var act = async () => await _leadService.Object.ConvertAsync(-1, "Opp", null, null, null);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ConvertLead_ShouldSetOpportunityName_WhenNameProvided()
    {
        // Arrange
        const int leadId = 2;
        const int expectedOppId = 10;
        const string oppName = "Enterprise Deal Q1";

        _leadService
            .Setup(s => s.ConvertAsync(leadId, oppName, null, null, null))
            .ReturnsAsync((expectedOppId, leadId));

        // Act
        var result = await _leadService.Object.ConvertAsync(leadId, oppName, null, null, null);

        // Assert
        result.OpportunityId.Should().Be(expectedOppId);
        _leadService.Verify(s => s.ConvertAsync(leadId, oppName, null, null, null), Times.Once);
    }

    [Fact]
    public async Task ConvertLead_ShouldSetEstimatedValue_WhenValueProvided()
    {
        // Arrange
        const int leadId = 3;
        const int expectedOppId = 11;
        const decimal estimatedValue = 50_000m;

        _leadService
            .Setup(s => s.ConvertAsync(leadId, It.IsAny<string?>(), null, estimatedValue, null))
            .ReturnsAsync((expectedOppId, leadId));

        // Act
        var result = await _leadService.Object.ConvertAsync(leadId, "Deal", null, estimatedValue, null);

        // Assert
        result.OpportunityId.Should().Be(expectedOppId);
    }

    [Fact]
    public async Task ConvertLead_ShouldSetExpectedCloseDate_WhenDateProvided()
    {
        // Arrange
        const int leadId = 4;
        const int expectedOppId = 12;
        var closeDate = new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc);

        _leadService
            .Setup(s => s.ConvertAsync(leadId, It.IsAny<string?>(), null, null, closeDate))
            .ReturnsAsync((expectedOppId, leadId));

        // Act
        var result = await _leadService.Object.ConvertAsync(leadId, "Deal", null, null, closeDate);

        // Assert
        result.OpportunityId.Should().Be(expectedOppId);
    }

    #endregion

    #region Opportunity Query Tests

    [Fact]
    public async Task GetOpportunitiesByAccount_ShouldReturnList_WhenAccountHasOpportunities()
    {
        // Arrange
        const int accountId = 100;
        var expectedOpps = new List<Opportunity>
        {
            new() { Id = 1, Name = "Deal A", AccountId = accountId },
            new() { Id = 2, Name = "Deal B", AccountId = accountId }
        };

        _oppService
            .Setup(s => s.GetOpportunitiesByAccountAsync(accountId))
            .ReturnsAsync(expectedOpps);

        // Act
        var result = await _oppService.Object.GetOpportunitiesByAccountAsync(accountId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(o => o.AccountId.Should().Be(accountId));
    }

    [Fact]
    public async Task GetTotalPipeline_ShouldReturnSum_WhenOpportunitiesExist()
    {
        // Arrange
        const decimal expectedTotal = 125_000m;
        _oppService.Setup(s => s.GetTotalPipelineAsync()).ReturnsAsync(expectedTotal);

        // Act
        var result = await _oppService.Object.GetTotalPipelineAsync();

        // Assert
        result.Should().Be(expectedTotal);
    }

    [Fact]
    public async Task GetOpenOpportunities_ShouldReturnOnlyOpen_WhenMixedExist()
    {
        // Arrange
        var openOpps = new List<Opportunity>
        {
            new() { Id = 1, Name = "Open Deal 1", Stage = OpportunityStage.Discovery },
            new() { Id = 2, Name = "Open Deal 2", Stage = OpportunityStage.Proposal }
        };

        _oppService.Setup(s => s.GetOpenOpportunitiesAsync()).ReturnsAsync(openOpps);

        // Act
        var result = await _oppService.Object.GetOpenOpportunitiesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(o =>
            o.Stage.Should().NotBe(OpportunityStage.ClosedWon)
                .And.NotBe(OpportunityStage.ClosedLost));
    }

    #endregion

    #region Lead Query Tests

    [Fact]
    public async Task GetLeadsByStatus_ShouldReturnList_WhenStatusMatches()
    {
        // Arrange
        var leads = new List<LeadSummaryDto>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Smith", Status = "New" },
            new() { Id = 2, FirstName = "Bob",   LastName = "Jones", Status = "New" }
        };

        _leadService
            .Setup(s => s.GetByStatusAsync(LeadLifecycleStatus.New))
            .ReturnsAsync(leads);

        // Act
        var result = await _leadService.Object.GetByStatusAsync(LeadLifecycleStatus.New);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchLeads_ShouldReturnMatchingLeads_WhenQueryMatches()
    {
        // Arrange
        const string searchTerm = "acme";
        var leads = new List<LeadSummaryDto>
        {
            new() { Id = 5, FirstName = "John", LastName = "Doe", CompanyName = "Acme Corp" }
        };

        _leadService.Setup(s => s.SearchAsync(searchTerm)).ReturnsAsync(leads);

        // Act
        var result = await _leadService.Object.SearchAsync(searchTerm);

        // Assert
        result.Should().ContainSingle();
        result.Should().Contain(l => l.CompanyName == "Acme Corp");
    }

    [Fact]
    public async Task AssignLeadOwner_ShouldReturnTrue_WhenAssignmentSucceeds()
    {
        // Arrange
        const int leadId = 10;
        const int ownerId = 42;
        _leadService.Setup(s => s.AssignOwnerAsync(leadId, ownerId)).ReturnsAsync(true);

        // Act
        var result = await _leadService.Object.AssignOwnerAsync(leadId, ownerId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckDuplicate_ShouldReturnDuplicate_WhenMatchingLeadExists()
    {
        // Arrange
        const string email = "john@acme.com";
        var expectedResult = (IsDuplicate: true, ExistingLeadId: (int?)7, MatchedOn: "email");

        _leadService
            .Setup(s => s.CheckDuplicateAsync(email, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _leadService.Object.CheckDuplicateAsync(email, null, null, null);

        // Assert
        result.IsDuplicate.Should().BeTrue();
        result.ExistingLeadId.Should().Be(7);
        result.MatchedOn.Should().Be("email");
    }

    #endregion

    #region Opportunity Advanced Tests

    [Fact]
    public async Task CloneOpportunity_ShouldReturnClone_WhenOpportunityExists()
    {
        // Arrange
        const int opportunityId = 20;
        var options = new OpportunityCloneOptions
        {
            NewName = "Copy of Enterprise Deal",
            CloneProducts = true,
            CloneTeamMembers = false
        };
        var clonedOpp = new Opportunity { Id = 99, Name = "Copy of Enterprise Deal" };

        _oppService
            .Setup(s => s.CloneAsync(opportunityId, It.IsAny<OpportunityCloneOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clonedOpp);

        // Act
        var result = await _oppService.Object.CloneAsync(opportunityId, options, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(99);
        result.Name.Should().Be("Copy of Enterprise Deal");
    }

    [Fact]
    public async Task GetForecastSummary_ShouldReturnSummary_WhenOpportunitiesExist()
    {
        // Arrange
        var summary = new ForecastSummaryDto
        {
            TotalPipelineAmount = 300_000m,
            TotalWeightedAmount = 180_000m,
            AsOf = DateTime.UtcNow
        };

        _oppService
            .Setup(s => s.GetForecastSummaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        // Act
        var result = await _oppService.Object.GetForecastSummaryAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalPipelineAmount.Should().Be(300_000m);
        result.TotalWeightedAmount.Should().Be(180_000m);
    }

    #endregion
}
