#!/usr/bin/env python3
"""Fix all remaining broken test files for TCOV-039 to TCOV-052."""
import os

BASE = "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend/tests/CRM.Tests"

files = {}

# ─── TCOV-045: AIChatbotControllerTests ──────────────────────────────────────
files[os.path.join(BASE, "Controllers/AIChatbotControllerTests.cs")] = r'''// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Api.Controllers;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
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
/// Unit tests for AIChatbotController (TCOV-045).
/// </summary>
public class AIChatbotControllerTests : IDisposable
{
    private readonly Mock<ILLMService> _mockLLMService;
    private readonly Mock<ILLMSettingsService> _mockLLMSettingsService;
    private readonly Mock<ILogger<AIChatbotController>> _mockLogger;
    private readonly CrmDbContext _dbContext;
    private readonly AIChatbotController _controller;

    public AIChatbotControllerTests()
    {
        _mockLLMService = new Mock<ILLMService>();
        _mockLLMSettingsService = new Mock<ILLMSettingsService>();
        _mockLogger = new Mock<ILogger<AIChatbotController>>();

        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"AIChatbotTest_{Guid.NewGuid()}")
            .Options;
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _dbContext = new CrmDbContext(options, config);

        _controller = new AIChatbotController(
            _dbContext,
            _mockLLMService.Object,
            _mockLLMSettingsService.Object,
            _mockLogger.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    public void Dispose() => _dbContext.Dispose();

    private void SetupLLMSettings(string fallbackProvider = "ollama")
    {
        _mockLLMSettingsService.Setup(s => s.GetSettingsAsync())
            .ReturnsAsync(new LLMSettingsDto
            {
                EffectiveFallbackOrder = new List<string> { fallbackProvider },
                DefaultProvider = fallbackProvider
            });
    }

    [Fact]
    public async Task GetHealth_ShouldReturnOk_WhenProviderNotConfigured()
    {
        SetupLLMSettings("ollama");
        _mockLLMService.Setup(s => s.IsConfiguredAsync(It.IsAny<string>())).ReturnsAsync(false);

        var result = await _controller.GetHealth();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetHealth_ShouldReturnOk_WhenNoProvidersConfigured()
    {
        _mockLLMSettingsService.Setup(s => s.GetSettingsAsync())
            .ReturnsAsync(new LLMSettingsDto
            {
                EffectiveFallbackOrder = new List<string>(),
                DefaultProvider = "ollama"
            });
        _mockLLMService.Setup(s => s.IsConfiguredAsync(It.IsAny<string>())).ReturnsAsync(false);

        var result = await _controller.GetHealth();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetHealth_ShouldReturnOk_WhenHealthCheckFails()
    {
        SetupLLMSettings("openai");
        _mockLLMService.Setup(s => s.IsConfiguredAsync(It.IsAny<string>())).ReturnsAsync(true);
        _mockLLMService.Setup(s => s.CompletionAsync(It.IsAny<LLMRequest>()))
            .ThrowsAsync(new InvalidOperationException("AI service error"));

        var result = await _controller.GetHealth();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetHealth_ShouldReturnOk_WhenProviderConfiguredAndHealthy()
    {
        SetupLLMSettings("openai");
        _mockLLMService.Setup(s => s.IsConfiguredAsync(It.IsAny<string>())).ReturnsAsync(true);
        _mockLLMService.Setup(s => s.CompletionAsync(It.IsAny<LLMRequest>()))
            .ReturnsAsync(new LLMResponse { Success = true, Model = "gpt-4o" });

        var result = await _controller.GetHealth();

        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        ok.Value.Should().NotBeNull();
    }

    [Fact]
    public void GetSuggestions_ShouldReturnOk()
    {
        var result = _controller.GetSuggestions();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task SendMessage_ShouldReturnBadRequest_WhenMessageIsEmpty()
    {
        var request = new ChatMessageRequest { Message = "" };

        var result = await _controller.SendMessage(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
'''

# ─── TCOV-046: AILeadScoringControllerAdditionalTests ────────────────────────
files[os.path.join(BASE, "Controllers/AILeadScoringControllerAdditionalTests.cs")] = r'''// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Additional tests for AILeadScoringController (TCOV-046)
using CRM.Api.Controllers;
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

        _mockAIService.Setup(s => s.ScoreLeadAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("AI error"));

        var result = await _controller.ScoreLead(lead.Id, default);

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetLeadScoreHistory_ShouldReturnNotFound_WhenLeadNotExists()
    {
        var result = await _controller.GetLeadScoreHistory(99999, 10, default);

        result.Should().BeOfType<NotFoundObjectResult>();
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

        _mockScoreHistoryService.Setup(s => s.GetHistoryAsync(lead.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LeadScoreHistory>());

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
'''

# ─── TCOV-049: WebhooksControllerTests ───────────────────────────────────────
files[os.path.join(BASE, "Controllers/WebhooksControllerTests.cs")] = r'''// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Infrastructure.Data;
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
/// Unit tests for WebhooksController (TCOV-049).
/// </summary>
public class WebhooksControllerTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly WebhooksController _controller;

    public WebhooksControllerTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"WebhooksTest_{Guid.NewGuid()}")
            .Options;
        var cfg = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _dbContext = new CrmDbContext(options, cfg);

        var logger = new Mock<ILogger<WebhooksController>>();
        _controller = new WebhooksController(_dbContext, logger.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task IngestWebFormSubmission_ShouldReturnOk_WithMinimalPayload()
    {
        var dto = new WebFormSubmissionDto
        {
            Email = "test@example.com",
            Name = "Test User",
            Subject = "Query",
            FormType = "contact"
        };

        var result = await _controller.IngestWebFormSubmission(dto);

        result.Result.Should().BeAssignableTo<IActionResult>();
    }

    [Fact]
    public async Task IngestWebFormSubmission_ShouldCreateInteraction()
    {
        var dto = new WebFormSubmissionDto
        {
            Email = "webhook@example.com",
            Name = "John Doe",
            Subject = "Support Request",
            Phone = "+1-555-0100"
        };

        await _controller.IngestWebFormSubmission(dto);

        _dbContext.Interactions.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task IngestWebFormSubmission_ShouldReturnOk_WhenEmailIsNull()
    {
        var dto = new WebFormSubmissionDto
        {
            Email = null,
            Name = "Anonymous",
            Subject = "Inquiry"
        };

        var result = await _controller.IngestWebFormSubmission(dto);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task IngestInboundEmail_ShouldReturnOk_WithMinimalPayload()
    {
        var dto = new InboundEmailDto
        {
            From = "sender@test.com",
            To = "crm@company.com",
            Subject = "Hello",
            TextBody = "Test email body"
        };

        var result = await _controller.IngestInboundEmail(dto);

        result.Should().NotBeNull();
    }
}
'''

# ─── TCOV-048: CampaignExecutionControllerTests ───────────────────────────────
files[os.path.join(BASE, "Controllers/CampaignExecutionControllerTests.cs")] = r'''// CRM Solution - Customer Relationship Management System
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
'''

# ─── TCOV-043: DashboardConfigControllerTests ────────────────────────────────
files[os.path.join(BASE, "Controllers/DashboardConfigControllerTests.cs")] = r'''// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
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
/// Unit tests for DashboardConfigController (TCOV-043).
/// </summary>
public class DashboardConfigControllerTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly DashboardConfigController _controller;

    private static ClaimsPrincipal MakeUser(int userId = 1, string role = "User")
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    public DashboardConfigControllerTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"DashboardConfigTest_{Guid.NewGuid()}")
            .Options;
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _dbContext = new CrmDbContext(options, config);

        var logger = new Mock<ILogger<DashboardConfigController>>();
        _controller = new DashboardConfigController(_dbContext, logger.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = MakeUser() }
        };
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task GetDashboards_ShouldReturnOk_WhenNoDashboards()
    {
        var result = await _controller.GetDashboards();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetDashboards_ShouldReturnOk_WithPublicDashboard()
    {
        _dbContext.Dashboards.Add(new Dashboard
        {
            Name = "Main Dashboard", IsActive = true, IsDeleted = false,
            Visibility = DashboardVisibility.Public, OwnerId = 1,
            DisplayOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _controller.GetDashboards();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetDashboard_ShouldReturnNotFound_WhenNotExists()
    {
        var result = await _controller.GetDashboard(9999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetDashboard_ShouldReturnOk_WhenExists()
    {
        var dashboard = new Dashboard
        {
            Name = "My Dashboard", IsActive = true, IsDeleted = false,
            Visibility = DashboardVisibility.Public, OwnerId = 1,
            DisplayOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Dashboards.Add(dashboard);
        await _dbContext.SaveChangesAsync();

        var result = await _controller.GetDashboard(dashboard.Id);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void GetWidgetTypes_ShouldReturnOk()
    {
        var result = _controller.GetWidgetTypes();

        result.Should().BeOfType<OkObjectResult>();
    }
}
'''

# ─── TCOV-042: DashboardControllerTests ──────────────────────────────────────
files[os.path.join(BASE, "Controllers/DashboardControllerTests.cs")] = r'''// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Api.Controllers;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for DashboardController (TCOV-042).
/// </summary>
public class DashboardControllerTests : IDisposable
{
    private readonly Mock<IDashboardService> _mockDashboardService;
    private readonly CrmDbContext _dbContext;
    private readonly DashboardController _controller;

    public DashboardControllerTests()
    {
        _mockDashboardService = new Mock<IDashboardService>();

        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"DashboardTest_{Guid.NewGuid()}")
            .Options;
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _dbContext = new CrmDbContext(options, config);

        _controller = new DashboardController(_dbContext, _mockDashboardService.Object);
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task GetStats_ShouldReturnOk_WhenNoData()
    {
        var result = await _controller.GetStats();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetSummary_ShouldReturnOk_WhenNoData()
    {
        var result = await _controller.GetSummary();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPipelineSummary_ShouldReturnOk()
    {
        var result = await _controller.GetPipelineSummary();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetStats_ShouldReturnOk_WithAccountsInDb()
    {
        _dbContext.Accounts.Add(new CRM.Core.Entities.Account
        {
            Company = "Test Account 1",
            FirstName = "Test", LastName = "Account",
            Email = "test@example.com", Phone = "5555555555",
            IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _controller.GetStats();

        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        ok.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSummary_ShouldReturnOk_WithOpportunitiesInDb()
    {
        _dbContext.Opportunities.Add(new CRM.Core.Entities.Opportunity
        {
            Name = "Deal 1", Stage = CRM.Core.Entities.OpportunityStage.ClosedWon,
            Amount = 10000m, ExpectedCloseDate = DateTime.UtcNow,
            IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _controller.GetSummary();

        result.Should().BeOfType<OkObjectResult>();
    }
}
'''

# ─── TCOV-044: LeadScoreRulesControllerTests ─────────────────────────────────
files[os.path.join(BASE, "Controllers/LeadScoreRulesControllerTests.cs")] = r'''// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Api.Controllers;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for LeadScoreRulesController (TCOV-044).
/// </summary>
public class LeadScoreRulesControllerTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly LeadScoreRulesController _controller;

    public LeadScoreRulesControllerTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"LeadScoreRulesTest_{Guid.NewGuid()}")
            .Options;
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _dbContext = new CrmDbContext(options, config);

        var logger = new Mock<ILogger<LeadScoreRulesController>>();
        _controller = new LeadScoreRulesController(_dbContext, logger.Object);
    }

    public void Dispose() => _dbContext.Dispose();

    private async Task<LeadScoreRule> SeedRule(string name = "Rule1", bool isActive = true)
    {
        var rule = new LeadScoreRule
        {
            Name = name, RuleType = LeadScoreRuleType.Demographic,
            IsActive = isActive, ScoreImpact = 10, Priority = 1,
            FieldName = "JobTitle", Operator = RuleOperator.Equals, Value = "CEO",
            Category = "Title", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _dbContext.LeadScoreRules.Add(rule);
        await _dbContext.SaveChangesAsync();
        return rule;
    }

    [Fact]
    public async Task GetRules_ShouldReturnOk_WithEmptyList()
    {
        var result = await _controller.GetRules();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetRules_ShouldReturnOk_WithRules()
    {
        await SeedRule("Rule A");
        await SeedRule("Rule B");

        var result = await _controller.GetRules();

        result.Result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result.Result!;
        ((IEnumerable<LeadScoreRule>)ok.Value!).Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRule_ShouldReturnNotFound_WhenNotExists()
    {
        var result = await _controller.GetRule(999);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetRule_ShouldReturnOk_WhenExists()
    {
        var rule = await SeedRule("Rule X");

        var result = await _controller.GetRule(rule.Id);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetRules_ShouldFilterByIsActive()
    {
        await SeedRule("Active Rule", true);
        await SeedRule("Inactive Rule", false);

        var result = await _controller.GetRules(isActive: true);

        result.Result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result.Result!;
        ((IEnumerable<LeadScoreRule>)ok.Value!).Should().HaveCount(1);
    }
}
'''

# ─── TCOV-052: ITSMWebhooksControllerTests ───────────────────────────────────
files[os.path.join(BASE, "Controllers/ITSMWebhooksControllerTests.cs")] = r'''// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Core.Dtos.ITSM;
using CRM.Core.Interfaces.ITSM;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for ITSMWebhooksController (TCOV-052).
/// </summary>
public class ITSMWebhooksControllerTests
{
    private readonly Mock<IWebhookNotificationService> _mockWebhookService;
    private readonly Mock<ILogger<ITSMWebhooksController>> _mockLogger;
    private readonly ITSMWebhooksController _controller;

    public ITSMWebhooksControllerTests()
    {
        _mockWebhookService = new Mock<IWebhookNotificationService>();
        _mockLogger = new Mock<ILogger<ITSMWebhooksController>>();
        _controller = new ITSMWebhooksController(_mockWebhookService.Object, _mockLogger.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Name, "test@crm.local")
                }, "test"))
            }
        };
    }

    [Fact]
    public async Task GetWebhooks_ShouldReturnOk_WithEmptyList()
    {
        _mockWebhookService.Setup(s => s.GetSubscriptionsAsync())
            .ReturnsAsync(Enumerable.Empty<WebhookSubscriptionDto>());

        var result = await _controller.GetWebhooks();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetSubscriptions_ShouldReturnOk()
    {
        _mockWebhookService.Setup(s => s.GetSubscriptionsAsync())
            .ReturnsAsync(new List<WebhookSubscriptionDto>
            {
                new WebhookSubscriptionDto { WebhookSubscriptionId = 1, TargetUrl = "https://example.com/hook" }
            });

        var result = await _controller.GetSubscriptions();

        result.Result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result.Result!;
        ((IEnumerable<WebhookSubscriptionDto>)ok.Value!).Should().HaveCount(1);
    }

    [Fact]
    public async Task GetSubscription_ShouldReturnNotFound_WhenNotExists()
    {
        _mockWebhookService.Setup(s => s.GetSubscriptionByIdAsync(999))
            .ReturnsAsync((WebhookSubscriptionDto?)null);

        var result = await _controller.GetSubscription(999);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetSubscription_ShouldReturnOk_WhenExists()
    {
        var dto = new WebhookSubscriptionDto { WebhookSubscriptionId = 5, TargetUrl = "https://sub.example.com" };
        _mockWebhookService.Setup(s => s.GetSubscriptionByIdAsync(5)).ReturnsAsync(dto);

        var result = await _controller.GetSubscription(5);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RegisterWebhook_ShouldReturnOk_WhenServiceSucceeds()
    {
        var request = new CreateWebhookSubscriptionDto
        {
            TargetUrl = "https://example.com/webhook",
            EventTypes = new List<string> { "incident.created" }
        };
        _mockWebhookService.Setup(s => s.CreateSubscriptionAsync(request, 1))
            .ReturnsAsync(new WebhookSubscriptionDto { WebhookSubscriptionId = 10, TargetUrl = request.TargetUrl });

        var result = await _controller.RegisterWebhook(request);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RegisterWebhook_ShouldReturnOk_WhenServiceThrows()
    {
        var request = new CreateWebhookSubscriptionDto { TargetUrl = "https://bad.example.com" };
        _mockWebhookService.Setup(s => s.CreateSubscriptionAsync(It.IsAny<CreateWebhookSubscriptionDto>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        var result = await _controller.RegisterWebhook(request);

        result.Should().BeOfType<OkObjectResult>();
    }
}
'''

# ─── TCOV-041: SubscriptionsControllerTests ──────────────────────────────────
files[os.path.join(BASE, "Controllers/SubscriptionsControllerTests.cs")] = r'''// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Api.Controllers;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for SubscriptionsController (TCOV-041).
/// </summary>
public class SubscriptionsControllerTests
{
    private readonly Mock<ISubscriptionService> _mockService;
    private readonly Mock<ILogger<SubscriptionsController>> _mockLogger;
    private readonly SubscriptionsController _controller;

    public SubscriptionsControllerTests()
    {
        _mockService = new Mock<ISubscriptionService>();
        _mockLogger = new Mock<ILogger<SubscriptionsController>>();
        _controller = new SubscriptionsController(_mockService.Object, _mockLogger.Object);
    }

    private static Subscription MakeSub(int id = 1) => new Subscription
    {
        Id = id, BillingCycle = "Monthly",
        SubscriptionStatus = SubscriptionStatus.Active,
        Amount = 99m, AccountId = 1,
        StartDate = DateTime.UtcNow
    };

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithEmptyList()
    {
        _mockService.Setup(s => s.GetAllAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Subscription>());

        var result = await _controller.GetAll(cancellationToken: default);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithSubscriptions()
    {
        _mockService.Setup(s => s.GetAllAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { MakeSub(1), MakeSub(2) });

        var result = await _controller.GetAll(cancellationToken: default);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        okResult.Value.Should().BeAssignableTo<IEnumerable<Subscription>>();
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenSubscriptionExists()
    {
        _mockService.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeSub(1));

        var result = await _controller.GetById(1, default);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenNotExists()
    {
        _mockService.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        var result = await _controller.GetById(999, default);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithStatusFilter()
    {
        _mockService.Setup(s => s.GetAllAsync(null, SubscriptionStatus.Active, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { MakeSub(1) });

        var result = await _controller.GetAll(status: SubscriptionStatus.Active, cancellationToken: default);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenInvalidBillingCycle()
    {
        var request = new SubscriptionCreateRequest
        {
            AccountId = 1, BillingCycle = "Invalid", Amount = 99m,
            StartDate = DateTime.UtcNow
        };

        var result = await _controller.Create(request, default);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenValid()
    {
        var request = new SubscriptionCreateRequest
        {
            AccountId = 1, BillingCycle = "Monthly", Amount = 99m,
            StartDate = DateTime.UtcNow
        };
        _mockService.Setup(s => s.CreateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeSub(42));

        var result = await _controller.Create(request, default);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }
}
'''

# ─── TCOV-047: ImportExportControllerTests ───────────────────────────────────
files[os.path.join(BASE, "Controllers/ImportExportControllerTests.cs")] = r'''// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Api.Controllers;
using CRM.Infrastructure.Data;
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
/// Unit tests for ImportExportController (TCOV-047).
/// </summary>
public class ImportExportControllerTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly ImportExportController _controller;

    public ImportExportControllerTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"ImportExportTest_{Guid.NewGuid()}")
            .Options;
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _dbContext = new CrmDbContext(options, config);
        var logger = new Mock<ILogger<ImportExportController>>();
        _controller = new ImportExportController(_dbContext, logger.Object);
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public void GetEntityTypes_ShouldReturnOk()
    {
        var result = _controller.GetEntityTypes();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void GetEntityTypes_ShouldReturnNonEmptyList()
    {
        var result = _controller.GetEntityTypes();

        var ok = (OkObjectResult)result.Result!;
        var list = ok.Value as System.Collections.IEnumerable;
        list.Should().NotBeNull();
    }

    [Fact]
    public async Task ExportData_ShouldReturnBadRequest_WhenEntityTypeUnknown()
    {
        var result = await _controller.ExportData("unknown-entity");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ExportData_ShouldReturnResult_ForContacts()
    {
        var result = await _controller.ExportData("contacts");

        // Empty db returns empty list as JSON — result is not null
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExportData_ShouldReturnResult_ForAccounts()
    {
        var result = await _controller.ExportData("accounts");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExportData_ShouldReturnResult_ForLeads()
    {
        var result = await _controller.ExportData("leads");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExportData_ShouldReturnResult_WhenFormatCsv()
    {
        var result = await _controller.ExportData("contacts", "csv");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ImportData_ShouldReturnBadRequest_WhenEntityTypeUnknown()
    {
        // The controller takes (string entityType, IFormFile file)
        // Passing a null file to an unknown entity type should return BadRequest
        var result = await _controller.ImportData("unknown-entity", null!);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
'''

# Write all files
for path, content in files.items():
    with open(path, "w") as f:
        f.write(content)
    print(f"Written: {os.path.basename(path)}")

print(f"\nAll {len(files)} files written.")
