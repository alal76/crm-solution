// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.Workflow;
using CRM.Core.Entities.Workflow;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for WorkflowTriggerService (TCOV-021).</summary>
public class WorkflowTriggerServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<WorkflowTriggerService>> _mockLogger;
    private readonly WorkflowTriggerService _service;

    public WorkflowTriggerServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"WorkflowTriggerTestDb_{Guid.NewGuid()}")
            .Options;
        _context = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<WorkflowTriggerService>>();
        _service = new WorkflowTriggerService(_context, _mockLogger.Object);
    }

    public void Dispose() => _context.Dispose();

    private async Task<WorkflowDefinition> SeedWorkflowDefinitionAsync()
    {
        var defn = new WorkflowDefinition
        {
            Name = "TestWorkflow",
            Description = "For testing",
            Status = WorkflowStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
        _context.WorkflowDefinitions.Add(defn);
        await _context.SaveChangesAsync();
        return defn;
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoTriggersExist()
    {
        var result = await _service.GetAllAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenTriggerDoesNotExist()
    {
        var result = await _service.GetByIdAsync(9999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateTrigger_WithValidDto()
    {
        var defn = await SeedWorkflowDefinitionAsync();

        var dto = new CreateWorkflowTriggerDto
        {
            WorkflowDefinitionId = defn.Id,
            Name = "OnCreate",
            TriggerType = WorkflowTriggerType.OnCreate,
            EntityType = "Account",
            IsActive = true
        };

        var result = await _service.CreateAsync(dto);

        result.Should().NotBeNull();
        result.Name.Should().Be("OnCreate");
        result.WorkflowDefinitionId.Should().Be(defn.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTrigger_WhenExists()
    {
        var defn = await SeedWorkflowDefinitionAsync();
        var dto = new CreateWorkflowTriggerDto
        {
            WorkflowDefinitionId = defn.Id,
            Name = "TestTrigger",
            TriggerType = WorkflowTriggerType.Manual,
            IsActive = true
        };
        var created = await _service.CreateAsync(dto);

        var result = await _service.GetByIdAsync(created.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("TestTrigger");
    }

    [Fact]
    public void ValidateCronExpression_ShouldReturnTrue_ForValidExpression()
    {
        var valid = _service.ValidateCronExpression("0 * * * *", out var error);
        valid.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact]
    public void ValidateCronExpression_ShouldReturnFalse_ForInvalidExpression()
    {
        var valid = _service.ValidateCronExpression("not-a-cron-expression", out var error);
        valid.Should().BeFalse();
        error.Should().NotBeNull();
    }

    [Fact]
    public void ValidateFilterConditions_ShouldReturnTrue_ForValidJson()
    {
        var json = "{\"field\":\"status\",\"op\":\"eq\",\"value\":\"active\"}";
        var valid = _service.ValidateFilterConditions(json, out var error);
        valid.Should().BeTrue();
        error.Should().BeNull();
    }
}
