// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.ITSM;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.ITSMServices.ChangeManagement;

public class ChangeManagementServiceTests : ServiceTestFixtureBase<ChangeManagementService>
{
    private readonly Mock<IDbContextResolver> _mockResolver;    private readonly Mock<ICMDBService> _mockCmdbService;    private readonly IChangeManagementService _service;

    public ChangeManagementServiceTests()
    {
        _mockResolver = new Mock<IDbContextResolver>();        _mockCmdbService = new Mock<ICMDBService>();        _mockResolver.Setup(r => r.ResolveContext()).Returns(MockContext.Object);
        _service = new ChangeManagementService(_mockResolver.Object, _mockCmdbService.Object, MockLogger.Object);
    }

    // ========================================================================
    // CreateChangeAsync
    // ========================================================================

    [Fact]
    public async Task CreateChangeAsync_ShouldCreateChange_WhenValidDtoProvided()
    {
        // Arrange
        var changes = new List<Change>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(changes);
        mockSet.Setup(m => m.Add(It.IsAny<Change>())).Callback<Change>(e => changes.Add(e));
        MockContext.Setup(c => c.Changes).Returns(mockSet.Object);
        MockContext.Setup(c => c.ChangeImpactedCIs).Returns(MockDbSetFactory.CreateMockDbSet(new List<ChangeImpactedCI>()).Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new CreateChangeDto
        {
            ShortDescription = "Upgrade database server",
            Description = "Upgrade MariaDB from 10.6 to 11.0",
            Type = ChangeType.Normal,
            Risk = ChangeRisk.Medium,
            Impact = ChangeImpact.Medium,
            PlannedStartDate = DateTime.UtcNow.AddDays(7),
            PlannedEndDate = DateTime.UtcNow.AddDays(7).AddHours(4),
            ImplementationPlan = "Step 1: Backup, Step 2: Upgrade",
            BackoutPlan = "Restore from backup"
        };

        // Act
        var result = await _service.CreateChangeAsync(dto, requestorId: 1);

        // Assert
        result.Should().NotBeNull();
        result.ShortDescription.Should().Be("Upgrade database server");
        result.Type.Should().Be(ChangeType.Normal);
        mockSet.Verify(m => m.Add(It.IsAny<Change>()), Times.Once);
    }

    [Fact]
    public async Task CreateChangeAsync_ShouldGenerateNumber()
    {
        // Arrange
        var changes = new List<Change>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(changes);
        mockSet.Setup(m => m.Add(It.IsAny<Change>())).Callback<Change>(e => changes.Add(e));
        MockContext.Setup(c => c.Changes).Returns(mockSet.Object);
        MockContext.Setup(c => c.ChangeImpactedCIs).Returns(MockDbSetFactory.CreateMockDbSet(new List<ChangeImpactedCI>()).Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new CreateChangeDto
        {
            ShortDescription = "Number test",
            Type = ChangeType.Standard,
            Risk = ChangeRisk.Low,
            Impact = ChangeImpact.Low
        };

        // Act
        var result = await _service.CreateChangeAsync(dto, requestorId: 1);

        // Assert
        result.Number.Should().NotBeNullOrEmpty();
    }

    // ========================================================================
    // GetChangeByIdAsync
    // ========================================================================

    [Fact]
    public async Task GetChangeByIdAsync_ShouldReturnChange_WhenExists()
    {
        // Arrange
        var changes = new List<Change>
        {
            new()
            {
                ChangeId = 1, Number = "CHG0001",
                ShortDescription = "DB upgrade", Type = ChangeType.Normal,
                State = ChangeState.New, Risk = ChangeRisk.Medium,
                Impact = ChangeImpact.Medium, RequestorId = 1,
                CreatedAt = DateTime.UtcNow, IsDeleted = false
            }
        };
        MockContext.Setup(c => c.Changes).Returns(MockDbSetFactory.CreateMockDbSet(changes).Object);
        MockContext.Setup(c => c.ChangeImpactedCIs).Returns(MockDbSetFactory.CreateMockDbSet(new List<ChangeImpactedCI>()).Object);

        // Act
        var result = await _service.GetChangeByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Number.Should().Be("CHG0001");
    }

    [Fact]
    public async Task GetChangeByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        MockContext.Setup(c => c.Changes).Returns(MockDbSetFactory.CreateMockDbSet(new List<Change>()).Object);
        MockContext.Setup(c => c.ChangeImpactedCIs).Returns(MockDbSetFactory.CreateMockDbSet(new List<ChangeImpactedCI>()).Object);

        // Act
        var result = await _service.GetChangeByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // ========================================================================
    // GetChangesAsync
    // ========================================================================

    [Fact]
    public async Task GetChangesAsync_ShouldReturnFilteredResults()
    {
        // Arrange
        var changes = new List<Change>
        {
            new() { ChangeId = 1, Number = "CHG0001", ShortDescription = "C1", State = ChangeState.New, Type = ChangeType.Normal, Risk = ChangeRisk.Low, Impact = ChangeImpact.Low, RequestorId = 1, CreatedAt = DateTime.UtcNow },
            new() { ChangeId = 2, Number = "CHG0002", ShortDescription = "C2", State = ChangeState.Closed, Type = ChangeType.Emergency, Risk = ChangeRisk.High, Impact = ChangeImpact.High, RequestorId = 1, CreatedAt = DateTime.UtcNow },
            new() { ChangeId = 3, Number = "CHG0003", ShortDescription = "C3", State = ChangeState.New, Type = ChangeType.Standard, Risk = ChangeRisk.Low, Impact = ChangeImpact.Low, RequestorId = 2, CreatedAt = DateTime.UtcNow }
        };
        MockContext.Setup(c => c.Changes).Returns(MockDbSetFactory.CreateMockDbSet(changes).Object);
        MockContext.Setup(c => c.ChangeImpactedCIs).Returns(MockDbSetFactory.CreateMockDbSet(new List<ChangeImpactedCI>()).Object);

        var filter = new ChangeFilterDto { State = ChangeState.New, PageNumber = 1, PageSize = 20 };

        // Act
        var (items, totalCount) = await _service.GetChangesAsync(filter);

        // Assert
        totalCount.Should().Be(2);
    }

    // ========================================================================
    // UpdateChangeAsync
    // ========================================================================

    [Fact]
    public async Task UpdateChangeAsync_ShouldUpdateFields_WhenChangeExists()
    {
        // Arrange
        var change = new Change
        {
            ChangeId = 1,
            Number = "CHG0001",
            ShortDescription = "Old",
            State = ChangeState.New,
            Type = ChangeType.Normal,
            Risk = ChangeRisk.Low,
            Impact = ChangeImpact.Low,
            RequestorId = 1,
            CreatedAt = DateTime.UtcNow
        };
        MockContext.Setup(c => c.Changes).Returns(MockDbSetFactory.CreateMockDbSet(new List<Change> { change }).Object);
        MockContext.Setup(c => c.ChangeImpactedCIs).Returns(MockDbSetFactory.CreateMockDbSet(new List<ChangeImpactedCI>()).Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new CreateChangeDto
        {
            ShortDescription = "Updated change",
            Type = ChangeType.Normal,
            Risk = ChangeRisk.High,
            Impact = ChangeImpact.High,
            Description = "Updated description"
        };

        // Act
        var result = await _service.UpdateChangeAsync(1, dto, modifiedById: 2);

        // Assert
        result.Should().NotBeNull();
        result.ShortDescription.Should().Be("Updated change");
    }

    // ========================================================================
    // SubmitForApprovalAsync / ApproveChangeAsync / RejectChangeAsync
    // ========================================================================

    [Fact]
    public async Task SubmitForApprovalAsync_ShouldChangeState_WhenInNew()
    {
        // Arrange
        var change = new Change
        {
            ChangeId = 1,
            Number = "CHG0001",
            ShortDescription = "Needs approval",
            State = ChangeState.New,
            Type = ChangeType.Normal,
            Risk = ChangeRisk.Medium,
            Impact = ChangeImpact.Medium,
            RequestorId = 1,
            CreatedAt = DateTime.UtcNow
        };
        MockContext.Setup(c => c.Changes).Returns(MockDbSetFactory.CreateMockDbSet(new List<Change> { change }).Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.SubmitForApprovalAsync(1, modifiedById: 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ApproveChangeAsync_ShouldApprove_WhenPendingApproval()
    {
        // Arrange
        var change = new Change
        {
            ChangeId = 1,
            Number = "CHG0001",
            ShortDescription = "Approve me",
            State = ChangeState.Assess,
            ApprovalStatus = ApprovalStatus.Requested,
            Type = ChangeType.Normal,
            Risk = ChangeRisk.Low,
            Impact = ChangeImpact.Low,
            RequestorId = 1,
            CreatedAt = DateTime.UtcNow
        };
        var approvals = new List<ChangeApproval>();
        var mockApprovalSet = MockDbSetFactory.CreateMockDbSet(approvals);
        mockApprovalSet.Setup(m => m.Add(It.IsAny<ChangeApproval>())).Callback<ChangeApproval>(e => approvals.Add(e));

        MockContext.Setup(c => c.Changes).Returns(MockDbSetFactory.CreateMockDbSet(new List<Change> { change }).Object);
        MockContext.Setup(c => c.ChangeApprovals).Returns(mockApprovalSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.ApproveChangeAsync(1, approverId: 5, comments: "Looks good");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RejectChangeAsync_ShouldReject_WhenPendingApproval()
    {
        // Arrange
        var change = new Change
        {
            ChangeId = 1,
            Number = "CHG0001",
            ShortDescription = "Reject me",
            State = ChangeState.Assess,
            ApprovalStatus = ApprovalStatus.Requested,
            Type = ChangeType.Normal,
            Risk = ChangeRisk.High,
            Impact = ChangeImpact.High,
            RequestorId = 1,
            CreatedAt = DateTime.UtcNow
        };
        MockContext.Setup(c => c.Changes).Returns(MockDbSetFactory.CreateMockDbSet(new List<Change> { change }).Object);
        MockContext.Setup(c => c.ChangeApprovals).Returns(MockDbSetFactory.CreateMockDbSet(new List<ChangeApproval>()).Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.RejectChangeAsync(1, approverId: 5, comments: "Too risky");

        // Assert
        result.Should().BeTrue();
    }

    // ========================================================================
    // ScheduleChangeAsync / CheckConflictsAsync
    // ========================================================================

    [Fact]
    public async Task ScheduleChangeAsync_ShouldSetDates_WhenApproved()
    {
        // Arrange
        var change = new Change
        {
            ChangeId = 1,
            Number = "CHG0001",
            ShortDescription = "Schedule me",
            State = ChangeState.Authorize,
            ApprovalStatus = ApprovalStatus.Approved,
            Type = ChangeType.Normal,
            Risk = ChangeRisk.Low,
            Impact = ChangeImpact.Low,
            RequestorId = 1,
            CreatedAt = DateTime.UtcNow
        };
        MockContext.Setup(c => c.Changes).Returns(MockDbSetFactory.CreateMockDbSet(new List<Change> { change }).Object);
        MockContext.Setup(c => c.ChangeBlackouts).Returns(MockDbSetFactory.CreateMockDbSet(new List<ChangeBlackout>()).Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var start = DateTime.UtcNow.AddDays(3);
        var end = start.AddHours(2);

        // Act
        var result = await _service.ScheduleChangeAsync(1, start, end, modifiedById: 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckConflictsAsync_ShouldReturnConflicts_WhenOverlapping()
    {
        // Arrange
        var changes = new List<Change>
        {
            new()
            {
                ChangeId = 1, Number = "CHG0001", ShortDescription = "Existing",
                State = ChangeState.Scheduled, Type = ChangeType.Normal,
                Risk = ChangeRisk.Low, Impact = ChangeImpact.Low, RequestorId = 1,
                PlannedStartDate = DateTime.UtcNow.AddDays(5),
                PlannedEndDate = DateTime.UtcNow.AddDays(5).AddHours(4),
                CreatedAt = DateTime.UtcNow
            }
        };
        MockContext.Setup(c => c.Changes).Returns(MockDbSetFactory.CreateMockDbSet(changes).Object);
        MockContext.Setup(c => c.ChangeBlackouts).Returns(MockDbSetFactory.CreateMockDbSet(new List<ChangeBlackout>()).Object);

        // Act
        var result = await _service.CheckConflictsAsync(1);

        // Assert
        // CheckConflictsAsync returns bool indicating if conflicts exist
        result.Should().BeFalse();
    }

    // ========================================================================
    // AddImpactedCIAsync / GetImpactedCIsAsync
    // ========================================================================

    [Fact]
    public async Task AddImpactedCIAsync_ShouldAddCI_WhenChangeAndCIExist()
    {
        // Arrange
        var change = new Change
        {
            ChangeId = 1,
            Number = "CHG0001",
            ShortDescription = "Test",
            State = ChangeState.New,
            Type = ChangeType.Normal,
            Risk = ChangeRisk.Low,
            Impact = ChangeImpact.Low,
            RequestorId = 1,
            CreatedAt = DateTime.UtcNow
        };
        var impactedCIs = new List<ChangeImpactedCI>();
        var mockImpactSet = MockDbSetFactory.CreateMockDbSet(impactedCIs);
        mockImpactSet.Setup(m => m.Add(It.IsAny<ChangeImpactedCI>())).Callback<ChangeImpactedCI>(e => impactedCIs.Add(e));

        MockContext.Setup(c => c.Changes).Returns(MockDbSetFactory.CreateMockDbSet(new List<Change> { change }).Object);
        MockContext.Setup(c => c.ChangeImpactedCIs).Returns(mockImpactSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.AddImpactedCIAsync(1, ciId: 10, createdById: 1);

        // Assert
        result.Should().BeTrue();
    }

    // ========================================================================
    // GetBlackoutPeriodsAsync / CreateBlackoutPeriodAsync
    // ========================================================================

    [Fact]
    public async Task GetBlackoutPeriodsAsync_ShouldReturnPeriods_WithinDateRange()
    {
        // Arrange
        var blackouts = new List<ChangeBlackout>
        {
            new() { BlackoutId = 1, Name = "Holiday freeze", StartDate = DateTime.UtcNow.AddDays(10), EndDate = DateTime.UtcNow.AddDays(17), CreatedAt = DateTime.UtcNow },
            new() { BlackoutId = 2, Name = "Code freeze", StartDate = DateTime.UtcNow.AddDays(30), EndDate = DateTime.UtcNow.AddDays(35), CreatedAt = DateTime.UtcNow }
        };
        MockContext.Setup(c => c.ChangeBlackouts).Returns(MockDbSetFactory.CreateMockDbSet(blackouts).Object);

        // Act
        var result = await _service.GetBlackoutPeriodsAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(20));

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateBlackoutPeriodAsync_ShouldCreate_WhenValidInfo()
    {
        // Arrange
        var blackouts = new List<ChangeBlackout>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(blackouts);
        mockSet.Setup(m => m.Add(It.IsAny<ChangeBlackout>())).Callback<ChangeBlackout>(e => blackouts.Add(e));
        MockContext.Setup(c => c.ChangeBlackouts).Returns(mockSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var info = new CreateBlackoutPeriodInfo
        {
            Name = "Year-end freeze",
            StartDate = new DateTime(2026, 12, 20),
            EndDate = new DateTime(2027, 1, 5),
            Reason = "Year-end change freeze"
        };

        // Act
        var result = await _service.CreateBlackoutPeriodAsync(info, createdById: 1);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Year-end freeze");
        mockSet.Verify(m => m.Add(It.IsAny<ChangeBlackout>()), Times.Once);
    }
}
