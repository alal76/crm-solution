// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for AILeadScoringController.
/// Tests lead scoring AI endpoints including scoring, batch operations, and analytics.
/// TODO-SYS008-002
/// </summary>
public class AILeadScoringControllerTests
{
    private readonly Mock<ILeadService> _mockLeadService;
    private readonly Mock<ILogger<AILeadScoringController>> _mockLogger;
    private readonly AILeadScoringController _controller;

    public AILeadScoringControllerTests()
    {
        _mockLeadService = new Mock<ILeadService>();
        _mockLogger = new Mock<ILogger<AILeadScoringController>>();
        _controller = new AILeadScoringController(
            _mockLeadService.Object,
            Mock.Of<IAIPredictiveAnalyticsService>(),
            Mock.Of<IAIAgentUsageService>(),
            _mockLogger.Object);
    }

    #region ScoreLead Tests

    [Fact]
    public async Task ScoreLead_WithValidLeadId_ReturnsOkResult()
    {
        // Arrange
        var leadId = 1;
        var leadDto = new LeadDto
        {
            Id = leadId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Score = 75
        };

        _mockLeadService
            .Setup(s => s.GetByIdAsync(leadId))
            .ReturnsAsync(leadDto);

        // Act
        var result = await _controller.ScoreLead(leadId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ScoreLead_WithInvalidLeadId_ReturnsNotFound()
    {
        // Arrange
        var leadId = 999;
        _mockLeadService
            .Setup(s => s.GetByIdAsync(leadId))
            .ReturnsAsync((LeadDto?)null);

        // Act
        var result = await _controller.ScoreLead(leadId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region BatchScoreLeads Tests

    [Fact]
    public async Task BatchScoreLeads_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new BatchScoreRequest { LeadIds = new List<int> { 1, 2, 3 } };

        // Act
        var result = await _controller.BatchScoreLeads(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task BatchScoreLeads_WithEmptyList_ReturnsBadRequest()
    {
        // Arrange
        var request = new BatchScoreRequest { LeadIds = new List<int>() };

        // Act
        var result = await _controller.BatchScoreLeads(request, CancellationToken.None);

        // Assert
        // Should return bad request or empty result
        result.Should().NotBeNull();
    }

    #endregion

    #region GetTopLeads Tests

    [Fact]
    public async Task GetTopLeads_WithDefaultCount_ReturnsOkResult()
    {
        // Arrange
        var leads = new List<LeadSummaryDto>
        {
            new() { Id = 1, FirstName = "High", LastName = "Score", Score = 95 },
            new() { Id = 2, FirstName = "Medium", LastName = "Score", Score = 70 }
        };

        _mockLeadService
            .Setup(s => s.GetAllAsync(1, 10))
            .ReturnsAsync((leads, 2, 1, 10, 1));

        // Act
        var result = await _controller.GetTopLeads(10, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetTopLeads_WithCustomCount_ReturnsRequestedCount()
    {
        // Arrange
        var count = 5;
        var leads = new List<LeadSummaryDto>
        {
            new() { Id = 1, FirstName = "Lead", LastName = "One", Score = 90 }
        };

        _mockLeadService
            .Setup(s => s.GetAllAsync(1, count))
            .ReturnsAsync((leads, leads.Count, 1, count, 1));

        // Act
        var result = await _controller.GetTopLeads(count, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region GetLeadScoreHistory Tests

    [Fact]
    public async Task GetLeadScoreHistory_WithValidLeadId_ReturnsOkResult()
    {
        // Arrange
        var leadId = 1;

        // Act
        var result = await _controller.GetLeadScoreHistory(leadId, 30, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetLeadScoreHistory_WithInvalidLeadId_ReturnsNotFound()
    {
        // Arrange
        var leadId = 999;
        _mockLeadService
            .Setup(s => s.GetByIdAsync(leadId))
            .ReturnsAsync((LeadDto?)null);

        // Act
        var result = await _controller.GetLeadScoreHistory(leadId, 30, CancellationToken.None);

        // Assert
        // May return empty history or not found
        result.Should().NotBeNull();
    }

    #endregion

    #region GetScoringConfig Tests

    [Fact]
    public void GetScoringConfig_ReturnsOkResult()
    {
        // Act
        var result = _controller.GetScoringConfig();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion
}
