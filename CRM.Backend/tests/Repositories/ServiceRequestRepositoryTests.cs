// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
/// Unit tests for Service Request Repository
/// Covers: Ticket-specific queries, SLA, escalation
/// </summary>
public class ServiceRequestRepositoryTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<DbSet<ServiceRequestEntity>> _mockDbSet;
    private readonly Mock<ILogger<ServiceRequestRepository>> _mockLogger;
    private readonly ServiceRequestRepository _repository;

    public ServiceRequestRepositoryTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockDbSet = new Mock<DbSet<ServiceRequestEntity>>();
        _mockLogger = new Mock<ILogger<ServiceRequestRepository>>();

        _mockContext.Setup(c => c.Set<ServiceRequestEntity>()).Returns(_mockDbSet.Object);
        _repository = new ServiceRequestRepository(_mockContext.Object, _mockLogger.Object);
    }

    #region GetByStatus Tests

    [Fact]
    public async Task GetByStatusAsync_HasMatches_ReturnsRequests()
    {
        // Arrange
        var requests = new List<ServiceRequestEntity>
        {
            new ServiceRequestEntity { Id = 1, Status = "Open" },
            new ServiceRequestEntity { Id = 2, Status = "Open" },
            new ServiceRequestEntity { Id = 3, Status = "Closed" }
        }.AsQueryable();

        SetupMockDbSet(requests);

        // Act
        var result = await _repository.GetByStatusAsync("Open");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOpenAsync_ReturnsOpenRequests()
    {
        // Arrange
        var requests = new List<ServiceRequestEntity>
        {
            new ServiceRequestEntity { Id = 1, Status = "Open" },
            new ServiceRequestEntity { Id = 2, Status = "In Progress" },
            new ServiceRequestEntity { Id = 3, Status = "Closed" }
        }.AsQueryable();

        SetupMockDbSet(requests);

        // Act
        var result = await _repository.GetOpenAsync();

        // Assert
        result.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region GetByPriority Tests

    [Fact]
    public async Task GetByPriorityAsync_HasMatches_ReturnsRequests()
    {
        // Arrange
        var requests = new List<ServiceRequestEntity>
        {
            new ServiceRequestEntity { Id = 1, Priority = "High" },
            new ServiceRequestEntity { Id = 2, Priority = "High" },
            new ServiceRequestEntity { Id = 3, Priority = "Low" }
        }.AsQueryable();

        SetupMockDbSet(requests);

        // Act
        var result = await _repository.GetByPriorityAsync("High");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCriticalAsync_ReturnsCriticalRequests()
    {
        // Arrange
        var requests = new List<ServiceRequestEntity>
        {
            new ServiceRequestEntity { Id = 1, Priority = "Critical" },
            new ServiceRequestEntity { Id = 2, Priority = "Critical" },
            new ServiceRequestEntity { Id = 3, Priority = "High" }
        }.AsQueryable();

        SetupMockDbSet(requests);

        // Act
        var result = await _repository.GetCriticalAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByAssignee Tests

    [Fact]
    public async Task GetByAssigneeAsync_HasRequests_ReturnsAssigneeRequests()
    {
        // Arrange
        var requests = new List<ServiceRequestEntity>
        {
            new ServiceRequestEntity { Id = 1, AssignedToId = 1 },
            new ServiceRequestEntity { Id = 2, AssignedToId = 1 },
            new ServiceRequestEntity { Id = 3, AssignedToId = 2 }
        }.AsQueryable();

        SetupMockDbSet(requests);

        // Act
        var result = await _repository.GetByAssigneeAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUnassignedAsync_ReturnsUnassignedRequests()
    {
        // Arrange
        var requests = new List<ServiceRequestEntity>
        {
            new ServiceRequestEntity { Id = 1, AssignedToId = null },
            new ServiceRequestEntity { Id = 2, AssignedToId = null },
            new ServiceRequestEntity { Id = 3, AssignedToId = 1 }
        }.AsQueryable();

        SetupMockDbSet(requests);

        // Act
        var result = await _repository.GetUnassignedAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByAccount Tests

    [Fact]
    public async Task GetByAccountAsync_HasRequests_ReturnsAccountRequests()
    {
        // Arrange
        var requests = new List<ServiceRequestEntity>
        {
            new ServiceRequestEntity { Id = 1, AccountId = 1 },
            new ServiceRequestEntity { Id = 2, AccountId = 1 },
            new ServiceRequestEntity { Id = 3, AccountId = 2 }
        }.AsQueryable();

        SetupMockDbSet(requests);

        // Act
        var result = await _repository.GetByAccountAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region SLA Tests

    [Fact]
    public async Task GetSLABreachedAsync_ReturnsBreachedRequests()
    {
        // Arrange
        var requests = new List<ServiceRequestEntity>
        {
            new ServiceRequestEntity { Id = 1, SLADueDate = DateTime.UtcNow.AddHours(-2), Status = "Open" },
            new ServiceRequestEntity { Id = 2, SLADueDate = DateTime.UtcNow.AddHours(-1), Status = "Open" },
            new ServiceRequestEntity { Id = 3, SLADueDate = DateTime.UtcNow.AddHours(5), Status = "Open" }
        }.AsQueryable();

        SetupMockDbSet(requests);

        // Act
        var result = await _repository.GetSLABreachedAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSLAAtRiskAsync_ReturnsAtRiskRequests()
    {
        // Arrange
        var requests = new List<ServiceRequestEntity>
        {
            new ServiceRequestEntity { Id = 1, SLADueDate = DateTime.UtcNow.AddMinutes(30), Status = "Open" },
            new ServiceRequestEntity { Id = 2, SLADueDate = DateTime.UtcNow.AddMinutes(45), Status = "Open" },
            new ServiceRequestEntity { Id = 3, SLADueDate = DateTime.UtcNow.AddHours(5), Status = "Open" }
        }.AsQueryable();

        SetupMockDbSet(requests);

        // Act
        var result = await _repository.GetSLAAtRiskAsync(60); // 60 minutes threshold

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Escalation Tests

    [Fact]
    public async Task GetEscalatedAsync_ReturnsEscalatedRequests()
    {
        // Arrange
        var requests = new List<ServiceRequestEntity>
        {
            new ServiceRequestEntity { Id = 1, IsEscalated = true },
            new ServiceRequestEntity { Id = 2, IsEscalated = true },
            new ServiceRequestEntity { Id = 3, IsEscalated = false }
        }.AsQueryable();

        SetupMockDbSet(requests);

        // Act
        var result = await _repository.GetEscalatedAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPendingEscalationAsync_ReturnsPendingEscalation()
    {
        // Arrange
        var requests = new List<ServiceRequestEntity>
        {
            new ServiceRequestEntity { Id = 1, EscalationDueAt = DateTime.UtcNow.AddHours(-1), IsEscalated = false, Status = "Open" },
            new ServiceRequestEntity { Id = 2, EscalationDueAt = DateTime.UtcNow.AddHours(5), IsEscalated = false, Status = "Open" }
        }.AsQueryable();

        SetupMockDbSet(requests);

        // Act
        var result = await _repository.GetPendingEscalationAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion

    #region Category Tests

    [Fact]
    public async Task GetByCategoryAsync_ReturnsRequestsByCategory()
    {
        // Arrange
        var requests = new List<ServiceRequestEntity>
        {
            new ServiceRequestEntity { Id = 1, CategoryId = 1 },
            new ServiceRequestEntity { Id = 2, CategoryId = 1 },
            new ServiceRequestEntity { Id = 3, CategoryId = 2 }
        }.AsQueryable();

        SetupMockDbSet(requests);

        // Act
        var result = await _repository.GetByCategoryAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchAsync_BySubject_ReturnsMatches()
    {
        // Arrange
        var requests = new List<ServiceRequestEntity>
        {
            new ServiceRequestEntity { Id = 1, Subject = "Cannot login to system" },
            new ServiceRequestEntity { Id = 2, Subject = "Login page not working" },
            new ServiceRequestEntity { Id = 3, Subject = "Need new monitor" }
        }.AsQueryable();

        SetupMockDbSet(requests);

        // Act
        var result = await _repository.SearchAsync("login");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByTicketNumberAsync_ExistingTicket_ReturnsRequest()
    {
        // Arrange
        var requests = new List<ServiceRequestEntity>
        {
            new ServiceRequestEntity { Id = 1, TicketNumber = "SR-001" }
        }.AsQueryable();

        SetupMockDbSet(requests);

        // Act
        var result = await _repository.GetByTicketNumberAsync("SR-001");

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetCountByStatusAsync_ReturnsStatusCounts()
    {
        // Arrange
        var requests = new List<ServiceRequestEntity>
        {
            new ServiceRequestEntity { Id = 1, Status = "Open" },
            new ServiceRequestEntity { Id = 2, Status = "Open" },
            new ServiceRequestEntity { Id = 3, Status = "Closed" }
        }.AsQueryable();

        SetupMockDbSet(requests);

        // Act
        var result = await _repository.GetCountByStatusAsync();

        // Assert
        result["Open"].Should().Be(2);
    }

    [Fact]
    public async Task GetCountByPriorityAsync_ReturnsPriorityCounts()
    {
        // Arrange
        var requests = new List<ServiceRequestEntity>
        {
            new ServiceRequestEntity { Id = 1, Priority = "High" },
            new ServiceRequestEntity { Id = 2, Priority = "High" },
            new ServiceRequestEntity { Id = 3, Priority = "Low" }
        }.AsQueryable();

        SetupMockDbSet(requests);

        // Act
        var result = await _repository.GetCountByPriorityAsync();

        // Assert
        result["High"].Should().Be(2);
    }

    [Fact]
    public async Task GetAverageResolutionTimeAsync_CalculatesAverage()
    {
        // Arrange
        var requests = new List<ServiceRequestEntity>
        {
            new ServiceRequestEntity { Id = 1, CreatedAt = DateTime.UtcNow.AddHours(-10), ResolvedAt = DateTime.UtcNow, Status = "Resolved" },
            new ServiceRequestEntity { Id = 2, CreatedAt = DateTime.UtcNow.AddHours(-20), ResolvedAt = DateTime.UtcNow, Status = "Resolved" }
        }.AsQueryable();

        SetupMockDbSet(requests);

        // Act
        var result = await _repository.GetAverageResolutionTimeAsync();

        // Assert
        result.Should().Be(15); // Average of 10 and 20 hours
    }

    [Fact]
    public async Task GetSLAComplianceRateAsync_CalculatesRate()
    {
        // Arrange
        var requests = new List<ServiceRequestEntity>
        {
            new ServiceRequestEntity { Id = 1, SLAMet = true, Status = "Closed" },
            new ServiceRequestEntity { Id = 2, SLAMet = true, Status = "Closed" },
            new ServiceRequestEntity { Id = 3, SLAMet = false, Status = "Closed" }
        }.AsQueryable();

        SetupMockDbSet(requests);

        // Act
        var result = await _repository.GetSLAComplianceRateAsync();

        // Assert
        // 2 out of 3 met SLA = 66.67%
        result.Should().BeApproximately(66.67m, 1);
    }

    #endregion

    #region Recent Activity Tests

    [Fact]
    public async Task GetRecentlyCreatedAsync_ReturnsRecent()
    {
        // Arrange
        var requests = new List<ServiceRequestEntity>
        {
            new ServiceRequestEntity { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new ServiceRequestEntity { Id = 2, CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new ServiceRequestEntity { Id = 3, CreatedAt = DateTime.UtcNow.AddDays(-15) }
        }.AsQueryable();

        SetupMockDbSet(requests);

        // Act
        var result = await _repository.GetRecentlyCreatedAsync(7);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentlyResolvedAsync_ReturnsRecentlyResolved()
    {
        // Arrange
        var requests = new List<ServiceRequestEntity>
        {
            new ServiceRequestEntity { Id = 1, ResolvedAt = DateTime.UtcNow.AddDays(-1), Status = "Resolved" },
            new ServiceRequestEntity { Id = 2, ResolvedAt = DateTime.UtcNow.AddDays(-5), Status = "Resolved" },
            new ServiceRequestEntity { Id = 3, Status = "Open" }
        }.AsQueryable();

        SetupMockDbSet(requests);

        // Act
        var result = await _repository.GetRecentlyResolvedAsync(7);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task BulkAssignAsync_AssignsRequests()
    {
        // Arrange
        var requestIds = new[] { 1, 2, 3 };
        var assigneeId = 10;

        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(3);

        // Act
        var result = await _repository.BulkAssignAsync(requestIds, assigneeId);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task BulkUpdateStatusAsync_UpdatesStatus()
    {
        // Arrange
        var requestIds = new[] { 1, 2, 3 };
        var newStatus = "Closed";

        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(3);

        // Act
        var result = await _repository.BulkUpdateStatusAsync(requestIds, newStatus);

        // Assert
        result.Should().Be(3);
    }

    #endregion

    #region Helper Methods

    private void SetupMockDbSet(IQueryable<ServiceRequestEntity> data)
    {
        _mockDbSet.As<IQueryable<ServiceRequestEntity>>().Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<ServiceRequestEntity>>().Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<ServiceRequestEntity>>().Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<ServiceRequestEntity>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    }

    #endregion
}

// Supporting class
public class ServiceRequestEntity
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? TicketNumber { get; set; }
    public string Status { get; set; } = "Open";
    public string Priority { get; set; } = "Medium";
    public int? AssignedToId { get; set; }
    public int? AccountId { get; set; }
    public int? CategoryId { get; set; }
    public bool IsEscalated { get; set; }
    public bool SLAMet { get; set; }
    public DateTime? SLADueDate { get; set; }
    public DateTime? EscalationDueAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public bool IsDeleted { get; set; }
}
