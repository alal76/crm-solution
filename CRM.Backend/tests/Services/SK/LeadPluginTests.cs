// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Spec: SK Plugin unit tests — LeadPlugin
// MANDATORY TEST RULE: All method signatures verified against actual source before writing.
// Source files read:
//   LeadPlugin.cs — KernelFunctions: GetLeadAsync, SearchLeadsAsync, GetLeadScoreAsync,
//                   GetLeadStatsAsync, UpdateLeadScoreAsync, ConvertLeadAsync
//   ILeadService.cs — signatures confirmed
//   LeadDtos.cs — LeadDto (extends LeadSummaryDto), LeadSummaryDto fields confirmed
//   CrmPluginBase.cs — SuccessResult({error:false,data:...}), ErrorResult({error:true,...})

using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Plugins;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace CRM.Tests.Services.SK;

/// <summary>
/// Unit tests for <see cref="LeadPlugin"/>.
/// KernelFunctions tested: GetLead, SearchLeads, GetLeadScore, GetLeadStats,
///   UpdateLeadScore, ConvertLead
/// </summary>
public class LeadPluginTests
{
    private readonly Mock<ILeadService> _leadService = new(MockBehavior.Loose);
    private readonly Mock<ILogger<LeadPlugin>> _logger = new();
    private readonly LeadPlugin _sut;

    public LeadPluginTests()
    {
        _sut = new LeadPlugin(_leadService.Object, _logger.Object);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Property / Constructor tests
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void PluginName_ShouldBe_Lead()
    {
        _sut.PluginName.Should().Be("Lead");
    }

    [Fact]
    public void Description_ShouldNotBeNullOrEmpty()
    {
        _sut.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenLeadServiceIsNull()
    {
        var act = () => new LeadPlugin(null!, _logger.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("leadService");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        var act = () => new LeadPlugin(_leadService.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetLeadAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetLeadAsync_ShouldReturnSuccessJson_WhenLeadExists()
    {
        var leadDto = new LeadDto
        {
            Id = 1,
            FirstName = "Alice",
            LastName = "Smith",
            Email = "alice@example.com",
            Status = "New",
            Score = 75
        };
        _leadService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(leadDto);

        var result = await _sut.GetLeadAsync(1);

        result.Should().NotBeNullOrEmpty();
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetLeadAsync_ShouldReturnErrorJson_WhenLeadNotFound()
    {
        _leadService.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((LeadDto?)null);

        var result = await _sut.GetLeadAsync(99);

        result.Should().NotBeNullOrEmpty();
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        result.Should().Contain("not found");
    }

    [Fact]
    public async Task GetLeadAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _leadService.Setup(s => s.GetByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var result = await _sut.GetLeadAsync(1);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        result.Should().Contain("DB error");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SearchLeadsAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchLeadsAsync_ShouldReturnSuccessJson_WithCountAndLeads()
    {
        var leads = new List<LeadSummaryDto>
        {
            new() { Id = 1, FirstName = "Bob", LastName = "Jones", Email = "bob@example.com" },
            new() { Id = 2, FirstName = "Carol", LastName = "Brown", Email = "carol@example.com" }
        };
        _leadService.Setup(s => s.SearchAsync("bob")).ReturnsAsync(leads);

        var result = await _sut.SearchLeadsAsync("bob");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        var data = doc.RootElement.GetProperty("data");
        data.GetProperty("count").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task SearchLeadsAsync_ShouldRespectMaxResults()
    {
        var leads = Enumerable.Range(1, 20)
            .Select(i => new LeadSummaryDto { Id = i, FirstName = $"Lead{i}", Email = $"l{i}@x.com" })
            .ToList();
        _leadService.Setup(s => s.SearchAsync(It.IsAny<string>())).ReturnsAsync(leads);

        var result = await _sut.SearchLeadsAsync("lead", maxResults: 5);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("data").GetProperty("count").GetInt32().Should().Be(5);
    }

    [Fact]
    public async Task SearchLeadsAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _leadService.Setup(s => s.SearchAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Search failed"));

        var result = await _sut.SearchLeadsAsync("test");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetLeadScoreAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetLeadScoreAsync_ShouldReturnSuccessJson_WithScoreField()
    {
        var leadDto = new LeadDto { Id = 5, FirstName = "Dave", Email = "d@x.com", Score = 80 };
        _leadService.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(leadDto);

        var result = await _sut.GetLeadScoreAsync(5);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        var data = doc.RootElement.GetProperty("data");
        data.GetProperty("leadId").GetInt32().Should().Be(5);
    }

    [Fact]
    public async Task GetLeadScoreAsync_ShouldReturnErrorJson_WhenLeadNotFound()
    {
        _leadService.Setup(s => s.GetByIdAsync(42)).ReturnsAsync((LeadDto?)null);

        var result = await _sut.GetLeadScoreAsync(42);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        result.Should().Contain("not found");
    }

    [Fact]
    public async Task GetLeadScoreAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _leadService.Setup(s => s.GetByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Timeout"));

        var result = await _sut.GetLeadScoreAsync(1);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetLeadStatsAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetLeadStatsAsync_ShouldReturnSuccessJson_WhenServiceReturnsStats()
    {
        var stats = new { totalLeads = 100, converted = 25 };
        _leadService.Setup(s => s.GetStatsAsync()).ReturnsAsync((object)stats);

        var result = await _sut.GetLeadStatsAsync();

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetLeadStatsAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _leadService.Setup(s => s.GetStatsAsync()).ThrowsAsync(new Exception("Stats failed"));

        var result = await _sut.GetLeadStatsAsync();

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UpdateLeadScoreAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateLeadScoreAsync_ShouldReturnSuccess_WhenScoreIsValid()
    {
        _leadService.Setup(s => s.UpdateAsync(3, It.IsAny<Action<CRM.Core.Entities.Lead>>()))
            .ReturnsAsync(true);

        var result = await _sut.UpdateLeadScoreAsync(3, 85);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("updated").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task UpdateLeadScoreAsync_ShouldReturnErrorJson_WhenScoreOutOfRange()
    {
        var result = await _sut.UpdateLeadScoreAsync(1, 150);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        result.Should().Contain("0 and 100");
    }

    [Fact]
    public async Task UpdateLeadScoreAsync_ShouldReturnErrorJson_WhenLeadNotFound()
    {
        _leadService.Setup(s => s.UpdateAsync(999, It.IsAny<Action<CRM.Core.Entities.Lead>>()))
            .ReturnsAsync(false);

        var result = await _sut.UpdateLeadScoreAsync(999, 50);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task UpdateLeadScoreAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _leadService.Setup(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<Action<CRM.Core.Entities.Lead>>()))
            .ThrowsAsync(new Exception("Update failed"));

        var result = await _sut.UpdateLeadScoreAsync(1, 50);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ConvertLeadAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ConvertLeadAsync_ShouldReturnSuccessJson_WhenConversionSucceeds()
    {
        // ConvertAsync(int id, string?, int?, decimal?, DateTime?) -> Task<(int OpportunityId, int LeadId)>
        _leadService
            .Setup(s => s.ConvertAsync(7, null, null, null, null))
            .ReturnsAsync((OpportunityId: 101, LeadId: 7));

        var result = await _sut.ConvertLeadAsync(7);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        var data = doc.RootElement.GetProperty("data");
        data.GetProperty("converted").GetBoolean().Should().BeTrue();
        data.GetProperty("leadId").GetInt32().Should().Be(7);
        data.GetProperty("opportunityId").GetInt32().Should().Be(101);
    }

    [Fact]
    public async Task ConvertLeadAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _leadService.Setup(s => s.ConvertAsync(It.IsAny<int>(), null, null, null, null))
            .ThrowsAsync(new InvalidOperationException("Lead already converted"));

        var result = await _sut.ConvertLeadAsync(5);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        result.Should().Contain("Lead already converted");
    }
}
