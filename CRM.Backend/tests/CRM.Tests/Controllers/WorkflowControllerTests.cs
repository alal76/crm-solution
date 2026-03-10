// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Api.Controllers;
using CRM.Core.Dtos.Workflow;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for WorkflowController (TCOV-039).
/// </summary>
public class WorkflowControllerTests : IDisposable
{
    private readonly Mock<IWorkflowService> _mockWorkflowService;
    private readonly Mock<ILLMService> _mockLLMService;
    private readonly Mock<ILLMSettingsService> _mockLLMSettingsService;
    private readonly Mock<IWorkflowFieldSchemaService> _mockFieldSchemaService;
    private readonly CrmDbContext _dbContext;
    private readonly WorkflowController _controller;

    public WorkflowControllerTests()
    {
        _mockWorkflowService = new Mock<IWorkflowService>();
        _mockLLMService = new Mock<ILLMService>();
        _mockLLMSettingsService = new Mock<ILLMSettingsService>();
        _mockFieldSchemaService = new Mock<IWorkflowFieldSchemaService>();

        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"WorkflowTest_{Guid.NewGuid()}")
            .Options;
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _dbContext = new CrmDbContext(options, config);

        _controller = new WorkflowController(
            _dbContext,
            _mockWorkflowService.Object,
            _mockLLMService.Object,
            _mockLLMSettingsService.Object,
            _mockFieldSchemaService.Object);
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task GetWorkflows_ShouldReturnOk_WhenCalled()
    {
        _mockWorkflowService
            .Setup(s => s.GetWorkflowDefinitionsAsync(null, null, null, null, 0, 50))
            .ReturnsAsync(new List<WorkflowDefinition>());

        var result = await _controller.GetWorkflows();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetWorkflows_ShouldReturnOk_WithFilterParams()
    {
        _mockWorkflowService
            .Setup(s => s.GetWorkflowDefinitionsAsync("Account", null, "Sales", null, 0, 50))
            .ReturnsAsync(new List<WorkflowDefinition>());

        var result = await _controller.GetWorkflows(entityType: "Account", category: "Sales");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetWorkflow_ShouldReturnNotFound_WhenWorkflowDoesNotExist()
    {
        _mockWorkflowService
            .Setup(s => s.GetWorkflowDefinitionAsync(999))
            .ReturnsAsync((WorkflowDefinition?)null);

        var result = await _controller.GetWorkflow(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetWorkflow_ShouldReturnOk_WhenWorkflowExists()
    {
        var workflow = new WorkflowDefinition
        {
            Id = 1,
            WorkflowKey = "test-key",
            Name = "Test Workflow",
            Versions = new List<WorkflowVersion>()
        };
        _mockWorkflowService.Setup(s => s.GetWorkflowDefinitionAsync(1)).ReturnsAsync(workflow);

        var result = await _controller.GetWorkflow(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CreateWorkflow_ShouldReturnBadRequest_WhenWorkflowKeyIsEmpty()
    {
        var dto = new CreateWorkflowDto { WorkflowKey = "", Name = "Test" };

        var result = await _controller.CreateWorkflow(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateWorkflow_ShouldReturnBadRequest_WhenNameIsEmpty()
    {
        var dto = new CreateWorkflowDto { WorkflowKey = "key-1", Name = "" };

        var result = await _controller.CreateWorkflow(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateWorkflow_ShouldReturnConflict_WhenKeyAlreadyExists()
    {
        var dto = new CreateWorkflowDto { WorkflowKey = "existing-key", Name = "Test Workflow" };
        _mockWorkflowService
            .Setup(s => s.GetWorkflowByKeyAsync("existing-key"))
            .ReturnsAsync(new WorkflowDefinition { WorkflowKey = "existing-key" });

        var result = await _controller.CreateWorkflow(dto);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task CreateWorkflow_ShouldReturnCreated_WhenValid()
    {
        var dto = new CreateWorkflowDto { WorkflowKey = "new-key", Name = "New Workflow" };
        _mockWorkflowService
            .Setup(s => s.GetWorkflowByKeyAsync("new-key"))
            .ReturnsAsync((WorkflowDefinition?)null);
        _mockWorkflowService
            .Setup(s => s.CreateWorkflowDefinitionAsync(It.IsAny<WorkflowDefinition>()))
            .ReturnsAsync(new WorkflowDefinition { Id = 1 });

        // WorkflowController calls GetCurrentUserId() which reads User.FindFirst()
        // Set up a minimal claims principal so it doesn't throw NullReferenceException
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new System.Security.Claims.ClaimsPrincipal(
                    new System.Security.Claims.ClaimsIdentity(
                        new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") }))
            }
        };

        var result = await _controller.CreateWorkflow(dto);

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task UpdateWorkflow_ShouldReturnNotFound_WhenWorkflowDoesNotExist()
    {
        var dto = new UpdateWorkflowDto { Name = "Updated" };

        var result = await _controller.UpdateWorkflow(999, dto);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteWorkflow_ShouldReturnOk_WhenSuccess()
    {
        _mockWorkflowService.Setup(s => s.DeleteWorkflowDefinitionAsync(1)).ReturnsAsync(true);

        var result = await _controller.DeleteWorkflow(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteWorkflow_ShouldReturnBadRequest_WhenNotFound()
    {
        _mockWorkflowService.Setup(s => s.DeleteWorkflowDefinitionAsync(999)).ReturnsAsync(false);

        var result = await _controller.DeleteWorkflow(999);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task PauseWorkflow_ShouldReturnOk_WhenSuccess()
    {
        _mockWorkflowService.Setup(s => s.PauseWorkflowAsync(1)).ReturnsAsync(true);

        var result = await _controller.PauseWorkflow(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task PauseWorkflow_ShouldReturnBadRequest_WhenNotFound()
    {
        _mockWorkflowService.Setup(s => s.PauseWorkflowAsync(99)).ReturnsAsync(false);

        var result = await _controller.PauseWorkflow(99);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
