// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Core.Entities.Workflow;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for WorkflowService (TCOV Wave-A).</summary>
public class WorkflowServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<WorkflowService>> _logger;
    private readonly WorkflowService _service;

    public WorkflowServiceTests()
    {
        var opts = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new CrmDbContext(opts, null!);
        _logger = new Mock<ILogger<WorkflowService>>();
        _service = new WorkflowService(_context, _logger.Object);
    }

    public void Dispose() => _context.Dispose();

    // ── GetWorkflowDefinitionsAsync ──────────────────────────────────────────

    [Fact]
    public async Task GetWorkflowDefinitionsAsync_ShouldReturnAllNonDeleted()
    {
        _context.WorkflowDefinitions.Add(new WorkflowDefinition { Id = 1, Name = "WF-A", WorkflowKey = "wf-a", EntityType = "Account", IsDeleted = false });
        _context.WorkflowDefinitions.Add(new WorkflowDefinition { Id = 2, Name = "WF-Deleted", WorkflowKey = "wf-del", EntityType = "Account", IsDeleted = true });
        await _context.SaveChangesAsync();

        var result = await _service.GetWorkflowDefinitionsAsync();

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("WF-A");
    }

    [Fact]
    public async Task GetWorkflowDefinitionsAsync_ShouldFilterByEntityType()
    {
        _context.WorkflowDefinitions.Add(new WorkflowDefinition { Id = 1, Name = "Account WF", WorkflowKey = "acct-wf", EntityType = "Account", IsDeleted = false });
        _context.WorkflowDefinitions.Add(new WorkflowDefinition { Id = 2, Name = "Lead WF", WorkflowKey = "lead-wf", EntityType = "Lead", IsDeleted = false });
        await _context.SaveChangesAsync();

        var result = await _service.GetWorkflowDefinitionsAsync(entityType: "Lead");

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Lead WF");
    }

    [Fact]
    public async Task GetWorkflowDefinitionsAsync_ShouldFilterByStatus()
    {
        _context.WorkflowDefinitions.Add(new WorkflowDefinition { Id = 1, Name = "Draft WF", WorkflowKey = "draft", EntityType = "Account", Status = WorkflowStatus.Draft, IsDeleted = false });
        _context.WorkflowDefinitions.Add(new WorkflowDefinition { Id = 2, Name = "Active WF", WorkflowKey = "active", EntityType = "Account", Status = WorkflowStatus.Active, IsDeleted = false });
        await _context.SaveChangesAsync();

        var result = await _service.GetWorkflowDefinitionsAsync(status: WorkflowStatus.Active);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Active WF");
    }

    // ── GetWorkflowDefinitionAsync ────────────────────────────────────────

    [Fact]
    public async Task GetWorkflowDefinitionAsync_ShouldReturnDefinition_WhenExists()
    {
        _context.WorkflowDefinitions.Add(new WorkflowDefinition { Id = 5, Name = "Find Me", WorkflowKey = "find-me", EntityType = "Contact", IsDeleted = false });
        await _context.SaveChangesAsync();

        var result = await _service.GetWorkflowDefinitionAsync(5);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetWorkflowDefinitionAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetWorkflowDefinitionAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWorkflowDefinitionAsync_ShouldReturnNull_WhenDeleted()
    {
        _context.WorkflowDefinitions.Add(new WorkflowDefinition { Id = 3, Name = "Deleted WF", WorkflowKey = "del-wf", EntityType = "Account", IsDeleted = true });
        await _context.SaveChangesAsync();

        var result = await _service.GetWorkflowDefinitionAsync(3);

        result.Should().BeNull();
    }

    // ── GetWorkflowByKeyAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetWorkflowByKeyAsync_ShouldReturnDefinition_WhenKeyExists()
    {
        _context.WorkflowDefinitions.Add(new WorkflowDefinition { Id = 7, Name = "Keyed WF", WorkflowKey = "my-wf-key", EntityType = "Opportunity", IsDeleted = false });
        await _context.SaveChangesAsync();

        var result = await _service.GetWorkflowByKeyAsync("my-wf-key");

        result.Should().NotBeNull();
        result!.WorkflowKey.Should().Be("my-wf-key");
    }

    [Fact]
    public async Task GetWorkflowByKeyAsync_ShouldReturnNull_WhenKeyNotFound()
    {
        var result = await _service.GetWorkflowByKeyAsync("nonexistent-key");
        result.Should().BeNull();
    }

    // ── CreateWorkflowDefinitionAsync ────────────────────────────────────────

    [Fact]
    public async Task CreateWorkflowDefinitionAsync_ShouldPersistDefinition_AndCreateInitialVersion()
    {
        var workflow = new WorkflowDefinition
        {
            Name = "New Workflow",
            WorkflowKey = "new-wf",
            EntityType = "Account",
            Status = WorkflowStatus.Draft
        };

        var created = await _service.CreateWorkflowDefinitionAsync(workflow);

        created.Should().NotBeNull();
        created.Name.Should().Be("New Workflow");
        created.Id.Should().BeGreaterThan(0);
        created.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Should also have created an initial version
        var versions = await _context.WorkflowVersions
            .Where(v => v.WorkflowDefinitionId == created.Id)
            .ToListAsync();
        versions.Should().HaveCount(1);
        versions.First().VersionNumber.Should().Be(1);
    }

    // ── UpdateWorkflowDefinitionAsync ─────────────────────────────────────

    [Fact]
    public async Task UpdateWorkflowDefinitionAsync_ShouldReturnUpdated_WhenExists()
    {
        _context.WorkflowDefinitions.Add(new WorkflowDefinition { Id = 10, Name = "Old Name", WorkflowKey = "upd-wf", EntityType = "Lead", IsDeleted = false });
        await _context.SaveChangesAsync();

        var updates = new WorkflowDefinition { Name = "Updated Name", EntityType = "Contact" };
        var result = await _service.UpdateWorkflowDefinitionAsync(10, updates);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateWorkflowDefinitionAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.UpdateWorkflowDefinitionAsync(999, new WorkflowDefinition { Name = "Ghost" });
        result.Should().BeNull();
    }
}
