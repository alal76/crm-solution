// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for WorkflowInstanceService (TCOV-028).</summary>
public class WorkflowInstanceServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<WorkflowInstanceService>> _logger;
    private readonly Mock<IWorkflowService> _workflowService;
    private readonly Mock<IHttpCalloutService> _httpCalloutService;
    private readonly WorkflowInstanceService _service;

    public WorkflowInstanceServiceTests()
    {
        var opts = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new CrmDbContext(opts, null!);
        _logger = new Mock<ILogger<WorkflowInstanceService>>();
        _workflowService = new Mock<IWorkflowService>();
        _httpCalloutService = new Mock<IHttpCalloutService>();
        _service = new WorkflowInstanceService(_context, _logger.Object, _workflowService.Object, _httpCalloutService.Object);
    }

    public void Dispose() => _context.Dispose();

    // ── GetInstanceAsync ────────────────────────────────────────────────────
    [Fact]
    public async Task GetInstanceAsync_ShouldReturnNull_WhenInstanceNotFound()
    {
        var result = await _service.GetInstanceAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetInstanceAsync_ShouldReturnNull_WhenInstanceIsDeleted()
    {
        _context.WorkflowInstances.Add(new WorkflowInstance { Id = 1, IsDeleted = true });
        await _context.SaveChangesAsync();

        var result = await _service.GetInstanceAsync(1);
        result.Should().BeNull();
    }

    // ── GetInstanceByCorrelationIdAsync ──────────────────────────────────────
    [Fact]
    public async Task GetInstanceByCorrelationIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetInstanceByCorrelationIdAsync("no-such-correlation");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetInstanceByCorrelationIdAsync_ShouldReturnInstance_WhenExists()
    {
        // Seed required navigations (non-nullable FKs need matching entities for EF InMemory includes)
        _context.WorkflowDefinitions.Add(new WorkflowDefinition { Id = 1, IsDeleted = false });
        _context.WorkflowVersions.Add(new WorkflowVersion { Id = 1, WorkflowDefinitionId = 1, IsDeleted = false });
        _context.WorkflowInstances.Add(new WorkflowInstance
        {
            Id = 10,
            CorrelationId = "corr-abc",
            WorkflowDefinitionId = 1,
            WorkflowVersionId = 1,
            IsDeleted = false
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetInstanceByCorrelationIdAsync("corr-abc");
        result.Should().NotBeNull();
        result!.Id.Should().Be(10);
    }

    // ── CancelInstanceAsync ─────────────────────────────────────────────────
    [Fact]
    public async Task CancelInstanceAsync_ShouldReturnFalse_WhenInstanceNotFound()
    {
        var result = await _service.CancelInstanceAsync(999, "no reason");
        result.Should().BeFalse();
    }

    // ── GetInstancesAsync ───────────────────────────────────────────────────
    [Fact]
    public async Task GetInstancesAsync_ShouldReturnEmpty_WhenNoInstancesExist()
    {
        var result = await _service.GetInstancesAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetInstancesAsync_ShouldFilterByEntityType()
    {
        // Seed required navigations (non-nullable FKs need matching entities for EF InMemory includes)
        _context.WorkflowDefinitions.Add(new WorkflowDefinition { Id = 2, IsDeleted = false });
        _context.WorkflowVersions.Add(new WorkflowVersion { Id = 2, WorkflowDefinitionId = 2, IsDeleted = false });
        _context.WorkflowInstances.AddRange(
            new WorkflowInstance { Id = 3, EntityType = "Account", WorkflowDefinitionId = 2, WorkflowVersionId = 2, IsDeleted = false },
            new WorkflowInstance { Id = 4, EntityType = "Contact", WorkflowDefinitionId = 2, WorkflowVersionId = 2, IsDeleted = false }
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetInstancesAsync(entityType: "Account");
        result.Should().HaveCount(1);
        result[0].EntityType.Should().Be("Account");
    }

    // ── StartWorkflowAsync ──────────────────────────────────────────────────
    [Fact]
    public async Task StartWorkflowAsync_ShouldThrow_WhenWorkflowDefinitionNotFound()
    {
        Func<Task> act = () => _service.StartWorkflowAsync(999, "Account", 1, "OnCreate");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not active*");
    }

    [Fact]
    public async Task StartWorkflowAsync_ShouldThrow_WhenWorkflowIsInactive()
    {
        _context.WorkflowDefinitions.Add(new WorkflowDefinition
        {
            Id = 1,
            Status = WorkflowStatus.Draft,
            IsDeleted = false,
            Name = "Draft WF"
        });
        await _context.SaveChangesAsync();

        Func<Task> act = () => _service.StartWorkflowAsync(1, "Account", 1, "OnCreate");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not active*");
    }
}
