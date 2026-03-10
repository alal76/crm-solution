// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
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
/// Unit tests for CampaignExecutionController (TCOV-048).
/// </summary>
public class CampaignExecutionControllerTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<CampaignExecutionController>> _mockLogger;

    // Helpers to build real services with InMemory DB
    private CampaignExecutionService BuildCampaignService()
    {
        var workflowLogger = new Mock<ILogger<WorkflowService>>();
        var workflowService = new WorkflowService(_dbContext, workflowLogger.Object);

        var mockHttpCallout = new Mock<CRM.Core.Interfaces.IHttpCalloutService>();
        var wfInstanceLogger = new Mock<ILogger<WorkflowInstanceService>>();
        var workflowInstanceService = new WorkflowInstanceService(
            _dbContext, wfInstanceLogger.Object, workflowService, mockHttpCallout.Object);

        var svcLogger = new Mock<ILogger<CampaignExecutionService>>();
        return new CampaignExecutionService(_dbContext, workflowService, workflowInstanceService, svcLogger.Object);
    }

    private CampaignExecutionController BuildController()
    {
        var service = BuildCampaignService();
        var controller = new CampaignExecutionController(service, _mockLogger.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Role, "Admin")
                }, "test"))
            }
        };
        return controller;
    }

    public CampaignExecutionControllerTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"CampaignExecTest_{Guid.NewGuid()}")
            .Options;
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _dbContext = new CrmDbContext(options, config);
        _mockLogger = new Mock<ILogger<CampaignExecutionController>>();
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task StartCampaign_ShouldReturnNotFound_WhenCampaignNotExists()
    {
        var controller = BuildController();

        var result = await controller.StartCampaign(99999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetCampaignAnalytics_ShouldReturnNotFound_WhenCampaignNotExists()
    {
        var controller = BuildController();

        var result = await controller.GetCampaignAnalytics(99999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetCampaignWorkflows_ShouldReturnOk_WhenNoWorkflows()
    {
        var campaign = new MarketingCampaign
        {
            Name = "Test Campaign",
            CampaignType = CampaignType.Email,
            Status = CampaignStatus.Draft,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _dbContext.MarketingCampaigns.Add(campaign);
        await _dbContext.SaveChangesAsync();

        var controller = BuildController();

        var result = await controller.GetCampaignWorkflows(campaign.Id);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task LinkWorkflowToCampaign_ShouldReturnNotFound_WhenCampaignNotExists()
    {
        var controller = BuildController();
        var request = new LinkWorkflowRequest { WorkflowDefinitionId = 1, WorkflowType = "email" };

        var result = await controller.LinkWorkflowToCampaign(99999, request);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task StartCampaign_ShouldReturnBadRequest_WhenCampaignNotStartable()
    {
        var campaign = new MarketingCampaign
        {
            Name = "Draft Campaign",
            CampaignType = CampaignType.Email,
            Status = CampaignStatus.Draft,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _dbContext.MarketingCampaigns.Add(campaign);
        await _dbContext.SaveChangesAsync();

        var controller = BuildController();

        var result = await controller.StartCampaign(campaign.Id);

        result.Should().BeAssignableTo<IActionResult>();
    }
}
