// CRM Solution - Customer Relationship Management System
// Workflow Repository Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace CRM.Tests.Repositories;

/// <summary>
/// Unit tests for Workflow Repository
/// Covers: Workflow-specific queries, instances, execution
/// </summary>
public class WorkflowRepositoryTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<DbSet<WorkflowEntity>> _mockDbSet;
    private readonly Mock<ILogger<WorkflowRepository>> _mockLogger;
    private readonly WorkflowRepository _repository;

    public WorkflowRepositoryTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockDbSet = new Mock<DbSet<WorkflowEntity>>();
        _mockLogger = new Mock<ILogger<WorkflowRepository>>();

        _mockContext.Setup(c => c.Set<WorkflowEntity>()).Returns(_mockDbSet.Object);
        _repository = new WorkflowRepository(_mockContext.Object, _mockLogger.Object);
    }

    #region GetByStatus Tests

    [Fact]
    public async Task GetByStatusAsync_HasMatches_ReturnsWorkflows()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, Status = "Active" },
            new WorkflowEntity { Id = 2, Status = "Active" },
            new WorkflowEntity { Id = 3, Status = "Draft" }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetByStatusAsync("Active");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsActiveWorkflows()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, Status = "Active" },
            new WorkflowEntity { Id = 2, Status = "Active" },
            new WorkflowEntity { Id = 3, Status = "Draft" }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetActiveAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDraftsAsync_ReturnsDraftWorkflows()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, Status = "Draft" },
            new WorkflowEntity { Id = 2, Status = "Active" }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetDraftsAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion

    #region GetByType Tests

    [Fact]
    public async Task GetByTypeAsync_HasMatches_ReturnsWorkflows()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, WorkflowType = "Approval" },
            new WorkflowEntity { Id = 2, WorkflowType = "Approval" },
            new WorkflowEntity { Id = 3, WorkflowType = "Automation" }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetByTypeAsync("Approval");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetApprovalWorkflowsAsync_ReturnsApprovalWorkflows()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, WorkflowType = "Approval" },
            new WorkflowEntity { Id = 2, WorkflowType = "Approval" },
            new WorkflowEntity { Id = 3, WorkflowType = "Automation" }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetApprovalWorkflowsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAutomationWorkflowsAsync_ReturnsAutomationWorkflows()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, WorkflowType = "Automation" },
            new WorkflowEntity { Id = 2, WorkflowType = "Automation" },
            new WorkflowEntity { Id = 3, WorkflowType = "Approval" }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetAutomationWorkflowsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByTrigger Tests

    [Fact]
    public async Task GetByTriggerAsync_HasMatches_ReturnsWorkflows()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, TriggerType = "OnCreate" },
            new WorkflowEntity { Id = 2, TriggerType = "OnCreate" },
            new WorkflowEntity { Id = 3, TriggerType = "OnUpdate" }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetByTriggerAsync("OnCreate");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetScheduledWorkflowsAsync_ReturnsScheduledWorkflows()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, TriggerType = "Scheduled", Status = "Active" },
            new WorkflowEntity { Id = 2, TriggerType = "Scheduled", Status = "Active" },
            new WorkflowEntity { Id = 3, TriggerType = "OnCreate", Status = "Active" }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetScheduledWorkflowsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByEntity Tests

    [Fact]
    public async Task GetByEntityTypeAsync_ReturnsWorkflowsForEntity()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, EntityType = "Account" },
            new WorkflowEntity { Id = 2, EntityType = "Account" },
            new WorkflowEntity { Id = 3, EntityType = "Contact" }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetByEntityTypeAsync("Account");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchAsync_ByName_ReturnsMatches()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, Name = "Quote Approval Workflow" },
            new WorkflowEntity { Id = 2, Name = "Contract Approval" },
            new WorkflowEntity { Id = 3, Name = "Lead Assignment" }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.SearchAsync("Approval");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Version Tests

    [Fact]
    public async Task GetLatestVersionAsync_ReturnsLatestVersion()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, Name = "Quote Approval", Version = 1 },
            new WorkflowEntity { Id = 2, Name = "Quote Approval", Version = 2 },
            new WorkflowEntity { Id = 3, Name = "Quote Approval", Version = 3 }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetLatestVersionAsync("Quote Approval");

        // Assert
        result.Should().NotBeNull();
        result!.Version.Should().Be(3);
    }

    [Fact]
    public async Task GetAllVersionsAsync_ReturnsAllVersions()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, Name = "Quote Approval", Version = 1 },
            new WorkflowEntity { Id = 2, Name = "Quote Approval", Version = 2 },
            new WorkflowEntity { Id = 3, Name = "Contract Approval", Version = 1 }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetAllVersionsAsync("Quote Approval");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetCountByStatusAsync_ReturnsStatusCounts()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, Status = "Active" },
            new WorkflowEntity { Id = 2, Status = "Active" },
            new WorkflowEntity { Id = 3, Status = "Draft" }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetCountByStatusAsync();

        // Assert
        result["Active"].Should().Be(2);
    }

    [Fact]
    public async Task GetCountByTypeAsync_ReturnsTypeCounts()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, WorkflowType = "Approval" },
            new WorkflowEntity { Id = 2, WorkflowType = "Approval" },
            new WorkflowEntity { Id = 3, WorkflowType = "Automation" }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetCountByTypeAsync();

        // Assert
        result["Approval"].Should().Be(2);
    }

    [Fact]
    public async Task GetExecutionCountAsync_ReturnsExecutionCount()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, ExecutionCount = 100 },
            new WorkflowEntity { Id = 2, ExecutionCount = 200 },
            new WorkflowEntity { Id = 3, ExecutionCount = 150 }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetTotalExecutionCountAsync();

        // Assert
        result.Should().Be(450);
    }

    [Fact]
    public async Task GetSuccessRateAsync_CalculatesRate()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, ExecutionCount = 100, SuccessCount = 90 },
            new WorkflowEntity { Id = 2, ExecutionCount = 100, SuccessCount = 80 }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetAverageSuccessRateAsync();

        // Assert
        result.Should().Be(85); // (90+80) / (100+100) * 100 = 85%
    }

    #endregion

    #region Instance Tests

    [Fact]
    public async Task GetRunningInstancesAsync_ReturnsRunningInstances()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, RunningInstances = 5 },
            new WorkflowEntity { Id = 2, RunningInstances = 3 },
            new WorkflowEntity { Id = 3, RunningInstances = 0 }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetWithRunningInstancesAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTotalRunningInstancesAsync_ReturnsTotalCount()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, RunningInstances = 5 },
            new WorkflowEntity { Id = 2, RunningInstances = 3 },
            new WorkflowEntity { Id = 3, RunningInstances = 2 }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetTotalRunningInstancesAsync();

        // Assert
        result.Should().Be(10);
    }

    #endregion

    #region Recent Activity Tests

    [Fact]
    public async Task GetRecentlyCreatedAsync_ReturnsRecent()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new WorkflowEntity { Id = 2, CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new WorkflowEntity { Id = 3, CreatedAt = DateTime.UtcNow.AddDays(-40) }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetRecentlyCreatedAsync(30);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentlyModifiedAsync_ReturnsRecentlyModified()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, UpdatedAt = DateTime.UtcNow.AddDays(-1) },
            new WorkflowEntity { Id = 2, UpdatedAt = DateTime.UtcNow.AddDays(-5) },
            new WorkflowEntity { Id = 3, UpdatedAt = DateTime.UtcNow.AddDays(-20) }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetRecentlyModifiedAsync(7);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentlyExecutedAsync_ReturnsRecentlyExecuted()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, LastExecutedAt = DateTime.UtcNow.AddHours(-2) },
            new WorkflowEntity { Id = 2, LastExecutedAt = DateTime.UtcNow.AddDays(-1) },
            new WorkflowEntity { Id = 3, LastExecutedAt = DateTime.UtcNow.AddDays(-10) }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetRecentlyExecutedAsync(7);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Owner Tests

    [Fact]
    public async Task GetByOwnerAsync_ReturnsOwnerWorkflows()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, OwnerId = 1 },
            new WorkflowEntity { Id = 2, OwnerId = 1 },
            new WorkflowEntity { Id = 3, OwnerId = 2 }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetByOwnerAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task GetMostExecutedAsync_ReturnsMostExecuted()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, ExecutionCount = 100 },
            new WorkflowEntity { Id = 2, ExecutionCount = 500 },
            new WorkflowEntity { Id = 3, ExecutionCount = 50 }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetMostExecutedAsync(2);

        // Assert
        result.Should().HaveCount(2);
        result.First().ExecutionCount.Should().Be(500);
    }

    [Fact]
    public async Task GetLongestAverageExecutionTimeAsync_ReturnsLongest()
    {
        // Arrange
        var workflows = new List<WorkflowEntity>
        {
            new WorkflowEntity { Id = 1, AverageExecutionTimeMs = 1000 },
            new WorkflowEntity { Id = 2, AverageExecutionTimeMs = 5000 },
            new WorkflowEntity { Id = 3, AverageExecutionTimeMs = 500 }
        }.AsQueryable();

        SetupMockDbSet(workflows);

        // Act
        var result = await _repository.GetLongestAverageExecutionTimeAsync(2);

        // Assert
        result.Should().HaveCount(2);
        result.First().AverageExecutionTimeMs.Should().Be(5000);
    }

    #endregion

    #region Helper Methods

    private void SetupMockDbSet(IQueryable<WorkflowEntity> data)
    {
        _mockDbSet.As<IQueryable<WorkflowEntity>>().Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<WorkflowEntity>>().Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<WorkflowEntity>>().Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<WorkflowEntity>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    }

    #endregion
}

// Supporting class
public class WorkflowEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public string WorkflowType { get; set; } = "Approval";
    public string? TriggerType { get; set; }
    public string? EntityType { get; set; }
    public int Version { get; set; } = 1;
    public int? OwnerId { get; set; }
    public int ExecutionCount { get; set; }
    public int SuccessCount { get; set; }
    public int RunningInstances { get; set; }
    public long AverageExecutionTimeMs { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastExecutedAt { get; set; }
    public bool IsDeleted { get; set; }
}
