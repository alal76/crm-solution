// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// TCOV2-D03 — OpportunitiesController unit tests
using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Api.Hubs;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for OpportunitiesController (TCOV2-D03).
/// Tests HTTP contract only.  The controller uses a private MapToDto method;
/// the service mock returns Opportunity entities which the controller transforms.
/// [Authorize] attribute present but not exercised in unit tests.
/// </summary>
public class OpportunitiesControllerTests
{
    private readonly Mock<IOpportunityService> _mockOpportunityService;
    private readonly Mock<IWinLossAnalysisService> _mockWinLossService;
    private readonly Mock<ILogger<OpportunitiesController>> _mockLogger;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly OpportunitiesController _controller;

    public OpportunitiesControllerTests()
    {
        _mockOpportunityService = new Mock<IOpportunityService>();
        _mockWinLossService = new Mock<IWinLossAnalysisService>();
        _mockLogger = new Mock<ILogger<OpportunitiesController>>();
        _mockNotificationService = new Mock<ICrmNotificationService>();

        _controller = new OpportunitiesController(
            _mockOpportunityService.Object,
            _mockWinLossService.Object,
            _mockLogger.Object,
            _mockNotificationService.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "TestAuth"))
            }
        };
    }

    private static Opportunity MakeOpportunity(int id = 1) => new()
    {
        Id = id,
        Name = $"Deal {id}",
        AccountId = 1,
        Amount = 10_000m,
        Stage = OpportunityStage.Discovery,
        Products = new List<OpportunityProduct>(),
        Competitors = new List<OpportunityCompetitor>()
    };

    // ── GetOpen ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetOpen_ShouldReturnOk_WithOpportunities()
    {
        // Arrange
        var opps = new List<Opportunity> { MakeOpportunity(1), MakeOpportunity(2) };
        _mockOpportunityService.Setup(s => s.GetOpenOpportunitiesAsync()).ReturnsAsync(opps);

        // Act
        var result = await _controller.GetOpen();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        ok.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOpen_ShouldReturnOk_WhenEmpty()
    {
        _mockOpportunityService.Setup(s => s.GetOpenOpportunitiesAsync())
            .ReturnsAsync(new List<Opportunity>());

        var result = await _controller.GetOpen();

        result.Should().BeOfType<OkObjectResult>();
        _mockOpportunityService.Verify(s => s.GetOpenOpportunitiesAsync(), Times.Once);
    }

    // ── GetById ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenOpportunityExists()
    {
        // Arrange
        var opp = MakeOpportunity(7);
        _mockOpportunityService.Setup(s => s.GetOpportunityByIdAsync(7)).ReturnsAsync(opp);

        // Act
        var result = await _controller.GetById(7);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenOpportunityMissing()
    {
        // Arrange
        _mockOpportunityService.Setup(s => s.GetOpportunityByIdAsync(404)).ReturnsAsync((Opportunity?)null);

        // Act
        var result = await _controller.GetById(404);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── GetByAccountId ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByAccountId_ShouldReturnOk()
    {
        // Arrange
        var opps = new List<Opportunity> { MakeOpportunity(1) };
        _mockOpportunityService.Setup(s => s.GetOpportunitiesByAccountAsync(10)).ReturnsAsync(opps);

        // Act
        var result = await _controller.GetByAccountId(10);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockOpportunityService.Verify(s => s.GetOpportunitiesByAccountAsync(10), Times.Once);
    }

    // ── MapToDto — Forecast / Win-Loss fields (OpportunityDto gap remediation) ────

    [Fact]
    public async Task GetById_ShouldMapForecastAndWinLossFields_OntoDto()
    {
        // Arrange
        var opp = MakeOpportunity(11);
        opp.ForecastCategory = ForecastCategory.Commit;
        opp.LossReasonCategory = LossReasonCategory.Competition;
        opp.LossReason = "Lost to Acme Corp on price and feature set.";
        opp.CompetitorWinnerId = 55;
        opp.WinLossNotes = "Competitor undercut pricing by 20%.";
        opp.ClosedDate = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);
        _mockOpportunityService.Setup(s => s.GetOpportunityByIdAsync(11)).ReturnsAsync(opp);

        // Act
        var result = await _controller.GetById(11);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<OpportunityDto>().Subject;
        dto.ForecastCategory.Should().Be((int)ForecastCategory.Commit);
        dto.LossReasonCategory.Should().Be((int)LossReasonCategory.Competition);
        dto.LossReason.Should().Be("Lost to Acme Corp on price and feature set.");
        dto.CompetitorWinnerId.Should().Be(55);
        dto.WinLossNotes.Should().Be("Competitor undercut pricing by 20%.");
        dto.ClosedDate.Should().Be(opp.ClosedDate.Value.ToString("o"));
    }

    [Fact]
    public async Task GetById_ShouldMapDefaultForecastCategory_AndNullWinLossFields_WhenOpportunityIsOpen()
    {
        // Arrange — a fresh, still-open opportunity should carry the default ForecastCategory
        // and no win/loss data, since Close() hasn't been called.
        var opp = MakeOpportunity(12);
        _mockOpportunityService.Setup(s => s.GetOpportunityByIdAsync(12)).ReturnsAsync(opp);

        // Act
        var result = await _controller.GetById(12);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<OpportunityDto>().Subject;
        dto.ForecastCategory.Should().Be((int)ForecastCategory.Pipeline);
        dto.LossReasonCategory.Should().BeNull();
        dto.LossReason.Should().BeNull();
        dto.CompetitorWinnerId.Should().BeNull();
        dto.WinLossNotes.Should().BeNull();
        dto.ClosedDate.Should().BeNull();
    }
}
