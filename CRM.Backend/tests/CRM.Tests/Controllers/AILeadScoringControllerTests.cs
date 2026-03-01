// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Controllers;
using CRM.Core.Entities;
using CRM.Core.Entities.AI;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.AI;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using CRM.Infrastructure.Services.AI;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for AILeadScoringController.
/// Tests lead scoring AI endpoints including scoring, batch operations, and analytics.
/// TODO-SYS008-002
/// </summary>
public class AILeadScoringControllerTests : IDisposable
{
    private readonly Mock<IAllenAIService> _mockAIService;
    private readonly Mock<ILLMService> _mockLLMService;
    private readonly Mock<ILLMSettingsService> _mockLLMSettingsService;
    private readonly Mock<ILeadScoreHistoryService> _mockScoreHistoryService;
    private readonly CrmDbContext _dbContext;
    private readonly AILeadScoringController _controller;

    public AILeadScoringControllerTests()
    {
        _mockAIService = new Mock<IAllenAIService>();
        _mockLLMService = new Mock<ILLMService>();
        _mockLLMSettingsService = new Mock<ILLMSettingsService>();
        _mockScoreHistoryService = new Mock<ILeadScoreHistoryService>();

        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"AILeadScoringTest_{Guid.NewGuid()}")
            .Options;
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _dbContext = new CrmDbContext(options, config);

        _controller = new AILeadScoringController(
            _dbContext,
            _mockAIService.Object,
            _mockLLMService.Object,
            _mockLLMSettingsService.Object,
            _mockScoreHistoryService.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    #region ScoreLead Tests

    [Fact]
    public async Task ScoreLead_WithValidLeadId_ReturnsOkResult()
    {
        // Arrange
        var lead = new Lead { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" };
        _dbContext.Leads.Add(lead);
        await _dbContext.SaveChangesAsync();

        _mockAIService
            .Setup(s => s.ScoreLeadAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LeadScore { LeadId = 1, OverallScore = 85, Confidence = 0.9m });

        // Act
        var result = await _controller.ScoreLead(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ScoreLead_WithInvalidLeadId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.ScoreLead(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region BatchScoreLeads Tests

    [Fact]
    public async Task BatchScoreLeads_WithValidRequest_ReturnsResult()
    {
        // Arrange
        _dbContext.Leads.Add(new Lead { Id = 1, FirstName = "A", LastName = "B" });
        await _dbContext.SaveChangesAsync();
        var request = new BatchScoreRequest { LeadIds = new List<int> { 1 } };

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
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region GetTopLeads Tests

    [Fact]
    public async Task GetTopLeads_ReturnsOkResult()
    {
        // Arrange
        _dbContext.Leads.Add(new Lead { Id = 1, FirstName = "A", LastName = "B", Score = 90 });
        _dbContext.Leads.Add(new Lead { Id = 2, FirstName = "C", LastName = "D", Score = 80 });
        await _dbContext.SaveChangesAsync();

        _mockAIService
            .Setup(s => s.GetTopLeadsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LeadScore>());

        // Act
        var result = await _controller.GetTopLeads(10, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
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
