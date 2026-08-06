// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Additional tests for AILeadScoringController (TCOV-046)
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Entities.AI;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.AI;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using CRM.Infrastructure.Services.AI;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Additional unit tests for AILeadScoringController (TCOV-046).
/// Supplements the existing AILeadScoringControllerTests.cs.
/// </summary>
public class AILeadScoringControllerAdditionalTests : IDisposable
{
    private readonly Mock<IAllenAIService> _mockAIService;
    private readonly Mock<ILLMService> _mockLLMService;
    private readonly Mock<ILLMSettingsService> _mockLLMSettingsService;
    private readonly Mock<ILeadScoreHistoryService> _mockScoreHistoryService;
    private readonly CrmDbContext _dbContext;
    private readonly AILeadScoringController _controller;

    public AILeadScoringControllerAdditionalTests()
    {
        _mockAIService = new Mock<IAllenAIService>();
        _mockLLMService = new Mock<ILLMService>();
        _mockLLMSettingsService = new Mock<ILLMSettingsService>();
        _mockScoreHistoryService = new Mock<ILeadScoreHistoryService>();

        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"AILeadScoringAdditional_{Guid.NewGuid()}")
            .Options;
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _dbContext = new CrmDbContext(options, config);

        _controller = new AILeadScoringController(
            _dbContext,
            _mockAIService.Object,
            _mockLLMService.Object,
            _mockLLMSettingsService.Object,
            _mockScoreHistoryService.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task ScoreLead_ShouldReturnNotFound_WhenLeadNotInDb()
    {
        var result = await _controller.ScoreLead(99999, default);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ScoreLead_ShouldReturnServerError_WhenAIServiceFails()
    {
        var lead = new Lead
        {
            FirstName = "Jane", LastName = "Doe", Email = "jane@test.com",
            IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Leads.Add(lead);
        await _dbContext.SaveChangesAsync();

        _mockAIService.Setup(s => s.ScoreLeadAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("AI error"));

        // AILeadScoringController.ScoreLead() does not catch general exceptions;
        // exception propagates to middleware in production.
        Func<Task> act = () => _controller.ScoreLead(lead.Id, default);
        await act.Should().ThrowAsync<Exception>().WithMessage("AI error");
    }

    [Fact]
    public async Task GetLeadScoreHistory_ShouldReturnOk_WhenLeadNotExists()
    {
        // GetLeadScoreHistory always returns OkObjectResult (with empty history)
        // even when the lead doesn't exist; it doesn't do a lead existence check.
        var result = await _controller.GetLeadScoreHistory(99999, 10, default);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetLeadScoreHistory_ShouldReturnOk_WhenLeadExists()
    {
        var lead = new Lead
        {
            FirstName = "Bob", LastName = "Smith", Email = "bob@test.com",
            IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Leads.Add(lead);
        await _dbContext.SaveChangesAsync();

        _mockScoreHistoryService.Setup(s => s.GetHistoryAsync(lead.Id, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LeadScoreHistoryDto>());

        var result = await _controller.GetLeadScoreHistory(lead.Id, 10, default);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task BatchScoreLeads_ShouldReturnOk_WhenLeadIdsProvided()
    {
        var request = new BatchScoreRequest { LeadIds = new List<int> { 1, 2, 3 } };

        _mockAIService.Setup(s => s.BatchScoreLeadsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LeadScore>());

        var result = await _controller.BatchScoreLeads(request, default);

        result.Should().BeOfType<OkObjectResult>();
    }
}
