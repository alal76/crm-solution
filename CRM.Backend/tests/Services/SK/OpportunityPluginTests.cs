// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Spec: SK Plugin unit tests — OpportunityPlugin
// MANDATORY TEST RULE: All method signatures verified against actual source before writing.
// Source files read:
//   OpportunityPlugin.cs — KernelFunctions: GetOpportunity, GetPipeline, GetWinRates,
//                           UpdateStage, AddOpportunityNote
//   IOpportunityService.cs — signatures confirmed
//   Opportunity.cs — Stage: OpportunityStage, Amount: decimal, IsDeleted: bool, AccountId: int?
//   CrmPluginBase.cs — SuccessResult/ErrorResult JSON format

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Plugins;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace CRM.Tests.Services.SK;

/// <summary>
/// Unit tests for <see cref="OpportunityPlugin"/>.
/// KernelFunctions tested: GetOpportunity, GetPipeline, GetWinRates, UpdateStage, AddOpportunityNote
/// </summary>
public class OpportunityPluginTests
{
    private readonly Mock<IOpportunityService> _opportunityService = new(MockBehavior.Loose);
    private readonly Mock<ICrmDbContext> _context = new(MockBehavior.Loose);
    private readonly Mock<ILogger<OpportunityPlugin>> _logger = new();
    private readonly OpportunityPlugin _sut;

    public OpportunityPluginTests()
    {
        _sut = new OpportunityPlugin(_opportunityService.Object, _context.Object, _logger.Object);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Property / Constructor tests
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void PluginName_ShouldBe_Opportunity()
    {
        _sut.PluginName.Should().Be("Opportunity");
    }

    [Fact]
    public void Description_ShouldNotBeNullOrEmpty()
    {
        _sut.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenOpportunityServiceIsNull()
    {
        var act = () => new OpportunityPlugin(null!, _context.Object, _logger.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("opportunityService");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenContextIsNull()
    {
        var act = () => new OpportunityPlugin(_opportunityService.Object, null!, _logger.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        var act = () => new OpportunityPlugin(_opportunityService.Object, _context.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetOpportunityAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetOpportunityAsync_ShouldReturnSuccessJson_WhenOpportunityExists()
    {
        var opp = new Opportunity
        {
            Id = 1,
            Name = "Deal Alpha",
            Stage = OpportunityStage.Proposal,
            Amount = 5000m,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _opportunityService.Setup(s => s.GetOpportunityByIdAsync(1)).ReturnsAsync(opp);

        var result = await _sut.GetOpportunityAsync(1);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetOpportunityAsync_ShouldReturnErrorJson_WhenNotFound()
    {
        _opportunityService.Setup(s => s.GetOpportunityByIdAsync(99)).ReturnsAsync((Opportunity?)null);

        var result = await _sut.GetOpportunityAsync(99);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        result.Should().Contain("not found");
    }

    [Fact]
    public async Task GetOpportunityAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _opportunityService.Setup(s => s.GetOpportunityByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("DB failure"));

        var result = await _sut.GetOpportunityAsync(1);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetPipelineAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPipelineAsync_ShouldReturnSuccessJson_WhenNoAccountIdFilter()
    {
        var opps = new List<Opportunity>
        {
            new() { Id = 1, Name = "Opp A", Stage = OpportunityStage.Discovery, Amount = 1000m, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Opp B", Stage = OpportunityStage.Proposal, Amount = 2000m, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        _opportunityService.Setup(s => s.GetOpenOpportunitiesAsync()).ReturnsAsync(opps);
        _opportunityService.Setup(s => s.GetTotalPipelineAsync()).ReturnsAsync(3000m);

        var result = await _sut.GetPipelineAsync();

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        var data = doc.RootElement.GetProperty("data");
        data.GetProperty("count").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task GetPipelineAsync_ShouldReturnSuccessJson_WhenAccountIdFilterProvided()
    {
        var opps = new List<Opportunity>
        {
            new() { Id = 10, Name = "Acct Opp", Stage = OpportunityStage.Negotiation, Amount = 9000m, AccountId = 5, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        _opportunityService.Setup(s => s.GetOpportunitiesByAccountAsync(5)).ReturnsAsync(opps);
        _opportunityService.Setup(s => s.GetTotalPipelineAsync()).ReturnsAsync(9000m);

        var result = await _sut.GetPipelineAsync(accountId: 5);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("count").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task GetPipelineAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _opportunityService.Setup(s => s.GetOpenOpportunitiesAsync())
            .ThrowsAsync(new Exception("Pipeline error"));

        var result = await _sut.GetPipelineAsync();

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetWinRatesAsync — uses _context.Opportunities (MockDbSet required)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetWinRatesAsync_ShouldReturnSuccessJson_WithWinRateStats()
    {
        var opportunities = new List<Opportunity>
        {
            new() { Id = 1, Stage = OpportunityStage.ClosedWon,  Amount = 5000m, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, Stage = OpportunityStage.ClosedLost, Amount = 2000m, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 3, Stage = OpportunityStage.Proposal,   Amount = 3000m, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        var mockOppSet = MockDbSetFactory.CreateMockDbSet(opportunities);
        _context.Setup(c => c.Opportunities).Returns(mockOppSet.Object);

        var result = await _sut.GetWinRatesAsync();

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        var data = doc.RootElement.GetProperty("data");
        data.GetProperty("won").GetInt32().Should().Be(1);
        data.GetProperty("lost").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task GetWinRatesAsync_ShouldReturnErrorJson_WhenContextThrows()
    {
        _context.Setup(c => c.Opportunities).Throws(new Exception("Context unavailable"));

        var result = await _sut.GetWinRatesAsync();

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UpdateStageAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStageAsync_ShouldReturnSuccessJson_WhenStageIsValid()
    {
        var opp = new Opportunity
        {
            Id = 3,
            Name = "Deal Beta",
            Stage = OpportunityStage.Proposal,
            Amount = 3000m,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _opportunityService.Setup(s => s.GetOpportunityByIdAsync(3)).ReturnsAsync(opp);
        _opportunityService.Setup(s => s.UpdateOpportunityAsync(It.IsAny<Opportunity>())).Returns(Task.CompletedTask);

        var result = await _sut.UpdateStageAsync(3, "ClosedWon");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("updated").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task UpdateStageAsync_ShouldReturnErrorJson_WhenOpportunityNotFound()
    {
        _opportunityService.Setup(s => s.GetOpportunityByIdAsync(55)).ReturnsAsync((Opportunity?)null);

        var result = await _sut.UpdateStageAsync(55, "ClosedWon");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        result.Should().Contain("not found");
    }

    [Fact]
    public async Task UpdateStageAsync_ShouldReturnErrorJson_WhenStageNameIsInvalid()
    {
        var opp = new Opportunity
        {
            Id = 4,
            Name = "Deal Gamma",
            Stage = OpportunityStage.Discovery,
            Amount = 1000m,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _opportunityService.Setup(s => s.GetOpportunityByIdAsync(4)).ReturnsAsync(opp);

        var result = await _sut.UpdateStageAsync(4, "InvalidStage");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        result.Should().Contain("Invalid stage");
    }

    [Fact]
    public async Task UpdateStageAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _opportunityService.Setup(s => s.GetOpportunityByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Update error"));

        var result = await _sut.UpdateStageAsync(1, "ClosedWon");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // AddOpportunityNoteAsync — uses _context.Notes + SaveChangesAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddOpportunityNoteAsync_ShouldReturnSuccessJson_WhenOpportunityExists()
    {
        var opp = new Opportunity
        {
            Id = 6,
            Name = "Deal Delta",
            Stage = OpportunityStage.Proposal,
            Amount = 7000m,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _opportunityService.Setup(s => s.GetOpportunityByIdAsync(6)).ReturnsAsync(opp);

        var mockNotes = MockDbSetFactory.CreateMockDbSet(new List<Note>());
        _context.Setup(c => c.Notes).Returns(mockNotes.Object);
        _context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.AddOpportunityNoteAsync(6, "Great progress on this deal.");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        result.Should().Contain("Great progress");
    }

    [Fact]
    public async Task AddOpportunityNoteAsync_ShouldReturnErrorJson_WhenOpportunityNotFound()
    {
        _opportunityService.Setup(s => s.GetOpportunityByIdAsync(88)).ReturnsAsync((Opportunity?)null);

        var result = await _sut.AddOpportunityNoteAsync(88, "Some note");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        result.Should().Contain("not found");
    }

    [Fact]
    public async Task AddOpportunityNoteAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _opportunityService.Setup(s => s.GetOpportunityByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Note failed"));

        var result = await _sut.AddOpportunityNoteAsync(1, "A note");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }
}
