// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Core.Entities.Workflow;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for <see cref="CampaignExecutionService"/> service-layer methods.
/// Tests focus on the CRUD / workflow-linking operations that touch only
/// the database (no workflow-engine execution paths).
/// </summary>
public class CampaignExecutionServiceNewTests
{
    // Helper: build a shared InMemory CrmDbContext + the transitive services
    private static (CrmDbContext context, CampaignExecutionService service) BuildSut(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(dbName ?? $"CampaignExec_{Guid.NewGuid()}")
            .Options;
        var context = new CrmDbContext(options, null!);

        var workflowService = new WorkflowService(context, Mock.Of<ILogger<WorkflowService>>());
        var workflowInstanceService = new WorkflowInstanceService(
            context,
            Mock.Of<ILogger<WorkflowInstanceService>>(),
            workflowService,
            Mock.Of<CRM.Core.Interfaces.IHttpCalloutService>());

        var service = new CampaignExecutionService(
            context,
            workflowService,
            workflowInstanceService,
            Mock.Of<ILogger<CampaignExecutionService>>());

        return (context, service);
    }

    // ──────────────────────────────────────────────────────────────────
    // GetCampaignWorkflowsAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCampaignWorkflowsAsync_WhenNoneExist_ReturnsEmptyList()
    {
        // Arrange
        var (_, service) = BuildSut();

        // Act
        var result = await service.GetCampaignWorkflowsAsync(campaignId: 999);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCampaignWorkflowsAsync_ReturnsOnlyWorkflowsForGivenCampaign()
    {
        // Arrange
        var (context, service) = BuildSut();
        // Seed WorkflowDefinitions so Include(cw => cw.WorkflowDefinition) resolves (required FK = inner join in InMemory)
        context.WorkflowDefinitions.AddRange(
            new WorkflowDefinition { Id = 10 },
            new WorkflowDefinition { Id = 11 });
        context.CampaignWorkflows.AddRange(
            new CampaignWorkflow { CampaignId = 1, WorkflowDefinitionId = 10, TriggerEvent = "start", WorkflowType = "Sequential", IsActive = true },
            new CampaignWorkflow { CampaignId = 2, WorkflowDefinitionId = 11, TriggerEvent = "end", WorkflowType = "Sequential", IsActive = true });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetCampaignWorkflowsAsync(campaignId: 1);

        // Assert
        result.Should().ContainSingle(w => w.CampaignId == 1);
    }

    [Fact]
    public async Task GetCampaignWorkflowsAsync_SoftDeletedWorkflow_NotReturned()
    {
        // Arrange
        var (context, service) = BuildSut();
        context.CampaignWorkflows.Add(new CampaignWorkflow
        {
            CampaignId = 5,
            WorkflowDefinitionId = 20,
            TriggerEvent = "t",
            WorkflowType = "Sequential",
            IsActive = false,
            IsDeleted = true
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetCampaignWorkflowsAsync(campaignId: 5);

        // Assert
        result.Should().BeEmpty();
    }

    // ──────────────────────────────────────────────────────────────────
    // GetCampaignWorkflowByIdAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCampaignWorkflowByIdAsync_NonExistentId_ReturnsNull()
    {
        // Arrange
        var (_, service) = BuildSut();

        // Act
        var result = await service.GetCampaignWorkflowByIdAsync(campaignWorkflowId: 9999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCampaignWorkflowByIdAsync_ExistingId_ReturnsWorkflow()
    {
        // Arrange
        var (context, service) = BuildSut();
        // Seed WorkflowDefinition so Include resolves correctly in InMemory
        context.WorkflowDefinitions.Add(new WorkflowDefinition { Id = 30 });
        var workflow = new CampaignWorkflow
        {
            Id = 100,
            CampaignId = 3,
            WorkflowDefinitionId = 30,
            TriggerEvent = "click",
            WorkflowType = "TriggerBased",
            IsActive = true
        };
        context.CampaignWorkflows.Add(workflow);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetCampaignWorkflowByIdAsync(100);

        // Assert
        result.Should().NotBeNull();
        result!.CampaignId.Should().Be(3);
    }

    // ──────────────────────────────────────────────────────────────────
    // LinkWorkflowToCampaignAsync — validation guards
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LinkWorkflowToCampaignAsync_CampaignNotFound_ThrowsArgumentException()
    {
        // Arrange
        var (_, service) = BuildSut();

        // Act
        var act = async () =>
            await service.LinkWorkflowToCampaignAsync(
                campaignId: 999,
                workflowDefinitionId: 1,
                workflowType: "Sequential",
                triggerEvent: "start");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Campaign not found*");
    }

    [Fact]
    public async Task LinkWorkflowToCampaignAsync_WorkflowNotFound_ThrowsArgumentException()
    {
        // Arrange
        var (context, service) = BuildSut();
        context.MarketingCampaigns.Add(new MarketingCampaign { Id = 10, Name = "Test Campaign" });
        await context.SaveChangesAsync();

        // Act
        var act = async () =>
            await service.LinkWorkflowToCampaignAsync(
                campaignId: 10,
                workflowDefinitionId: 999,
                workflowType: "Sequential",
                triggerEvent: "start");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Workflow not found*");
    }

    [Fact]
    public async Task LinkWorkflowToCampaignAsync_ValidLink_CreatesWorkflowLink()
    {
        // Arrange
        var (context, service) = BuildSut();
        context.MarketingCampaigns.Add(new MarketingCampaign { Id = 20, Name = "My Campaign" });
        context.WorkflowDefinitions.Add(new WorkflowDefinition { Id = 50, Name = "My Workflow" });
        await context.SaveChangesAsync();

        // Act
        var result = await service.LinkWorkflowToCampaignAsync(
            campaignId: 20,
            workflowDefinitionId: 50,
            workflowType: "Sequential",
            triggerEvent: "CampaignStarted");

        // Assert
        result.Should().NotBeNull();
        result.CampaignId.Should().Be(20);
        result.WorkflowDefinitionId.Should().Be(50);
        result.TriggerEvent.Should().Be("CampaignStarted");
        result.IsActive.Should().BeTrue();
    }
}
