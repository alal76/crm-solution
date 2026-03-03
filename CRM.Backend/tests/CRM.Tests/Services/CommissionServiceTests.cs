// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for CommissionService.
/// </summary>
public class CommissionServiceTests : ServiceTestFixtureBase<CommissionService>
{    private readonly CommissionService _service;

    private readonly List<Commission> _commissions;
    private readonly List<CommissionPlan> _plans;
    private readonly List<CommissionTier> _tiers;
    private readonly List<CommissionPlanAssignment> _assignments;
    private readonly List<CommissionStatement> _statements;
    private readonly List<Opportunity> _opportunities;
    private readonly List<Order> _orders;

    public CommissionServiceTests()
    {        _commissions = new List<Commission>();
        _plans = new List<CommissionPlan>();
        _tiers = new List<CommissionTier>();
        _assignments = new List<CommissionPlanAssignment>();
        _statements = new List<CommissionStatement>();
        _opportunities = new List<Opportunity>();
        _orders = new List<Order>();

        var mockCommissions = MockDbSetFactory.CreateMockDbSet(_commissions);
        var mockPlans = MockDbSetFactory.CreateMockDbSet(_plans);
        var mockTiers = MockDbSetFactory.CreateMockDbSet(_tiers);
        var mockAssignments = MockDbSetFactory.CreateMockDbSet(_assignments);
        var mockStatements = MockDbSetFactory.CreateMockDbSet(_statements);
        var mockOpportunities = MockDbSetFactory.CreateMockDbSet(_opportunities);
        var mockOrders = MockDbSetFactory.CreateMockDbSet(_orders);

        // Add FindAsync(object[], CancellationToken) overload - MockDbSetFactory only sets up FindAsync(object[])
        mockCommissions.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((keys, _) =>
            {
                var id = keys.FirstOrDefault();
                if (id == null)
                    return ValueTask.FromResult<Commission?>(default);
                return ValueTask.FromResult(_commissions.FirstOrDefault(e => e.Id == Convert.ToInt32(id)));
            });
        mockPlans.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((keys, _) =>
            {
                var id = keys.FirstOrDefault();
                if (id == null)
                    return ValueTask.FromResult<CommissionPlan?>(default);
                return ValueTask.FromResult(_plans.FirstOrDefault(e => e.Id == Convert.ToInt32(id)));
            });
        mockTiers.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((keys, _) =>
            {
                var id = keys.FirstOrDefault();
                if (id == null)
                    return ValueTask.FromResult<CommissionTier?>(default);
                return ValueTask.FromResult(_tiers.FirstOrDefault(e => e.Id == Convert.ToInt32(id)));
            });

        MockContext.Setup(c => c.Commissions).Returns(mockCommissions.Object);
        MockContext.Setup(c => c.CommissionPlans).Returns(mockPlans.Object);
        MockContext.Setup(c => c.CommissionTiers).Returns(mockTiers.Object);
        MockContext.Setup(c => c.CommissionPlanAssignments).Returns(mockAssignments.Object);
        MockContext.Setup(c => c.CommissionStatements).Returns(mockStatements.Object);
        MockContext.Setup(c => c.Opportunities).Returns(mockOpportunities.Object);
        MockContext.Setup(c => c.Orders).Returns(mockOrders.Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new CommissionService(MockContext.Object, MockLogger.Object);
    }

    // ========================================================================
    // GetAllAsync
    // ========================================================================
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeletedCommissions()
    {
        // Arrange
        _commissions.AddRange(new[]
        {
            new Commission { Id = 1, UserId = 1, Status = CommissionStatus.Pending, IsDeleted = false },
            new Commission { Id = 2, UserId = 2, Status = CommissionStatus.Approved, IsDeleted = false },
            new Commission { Id = 3, UserId = 1, Status = CommissionStatus.Paid, IsDeleted = true }
        });

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByUserId()
    {
        // Arrange
        _commissions.AddRange(new[]
        {
            new Commission { Id = 1, UserId = 10, IsDeleted = false },
            new Commission { Id = 2, UserId = 20, IsDeleted = false },
            new Commission { Id = 3, UserId = 10, IsDeleted = false }
        });

        // Act
        var result = await _service.GetAllAsync(userId: 10);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.UserId == 10);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByStatus()
    {
        // Arrange
        _commissions.AddRange(new[]
        {
            new Commission { Id = 1, Status = CommissionStatus.Pending, IsDeleted = false },
            new Commission { Id = 2, Status = CommissionStatus.Approved, IsDeleted = false },
            new Commission { Id = 3, Status = CommissionStatus.Pending, IsDeleted = false }
        });

        // Act
        var result = await _service.GetAllAsync(status: CommissionStatus.Pending);

        // Assert
        result.Should().HaveCount(2);
    }

    // ========================================================================
    // GetByIdAsync
    // ========================================================================
    [Fact]
    public async Task GetByIdAsync_ShouldReturnCommission_WhenExists()
    {
        // Arrange
        _commissions.Add(new Commission { Id = 1, UserId = 5, Status = CommissionStatus.Pending, IsDeleted = false });

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // ========================================================================
    // CreateAsync / DeleteAsync
    // ========================================================================
    [Fact]
    public async Task CreateAsync_ShouldAddCommissionAndSetTimestamps()
    {
        // Arrange
        var commission = new Commission { UserId = 1, CommissionAmount = 500m, Status = CommissionStatus.Pending };

        // Act
        var result = await _service.CreateAsync(commission);

        // Assert
        result.Should().NotBeNull();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _commissions.Should().Contain(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete()
    {
        // Arrange
        _commissions.Add(new Commission { Id = 1, UserId = 1, IsDeleted = false });

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        _commissions.First(c => c.Id == 1).IsDeleted.Should().BeTrue();
    }

    // ========================================================================
    // Status Management: Approve / Reject / MarkAsPaid / Clawback
    // ========================================================================
    [Fact]
    public async Task ApproveAsync_ShouldSetApprovedStatus()
    {
        // Arrange
        _commissions.Add(new Commission { Id = 1, UserId = 1, Status = CommissionStatus.Pending, IsDeleted = false });

        // Act
        var result = await _service.ApproveAsync(1, approvedById: 42);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(CommissionStatus.Approved);
        result.ApprovedById.Should().Be(42);
        result.ApprovedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RejectAsync_ShouldSetCancelledStatus()
    {
        // Arrange
        _commissions.Add(new Commission { Id = 1, UserId = 1, Status = CommissionStatus.Pending, IsDeleted = false });

        // Act
        var result = await _service.RejectAsync(1, "Invalid deal");

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(CommissionStatus.Cancelled);
        result.Notes.Should().Contain("Invalid deal");
    }

    [Fact]
    public async Task MarkAsPaidAsync_ShouldSetPaidStatus()
    {
        // Arrange
        _commissions.Add(new Commission { Id = 1, UserId = 1, Status = CommissionStatus.Approved, IsDeleted = false });

        // Act
        var result = await _service.MarkAsPaidAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(CommissionStatus.Paid);
        result.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ClawbackAsync_ShouldSetClawedBackStatus()
    {
        // Arrange
        _commissions.Add(new Commission { Id = 1, UserId = 1, Status = CommissionStatus.Paid, IsDeleted = false });

        // Act
        var result = await _service.ClawbackAsync(1, "Customer churned");

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(CommissionStatus.ClawedBack);
        result.Notes.Should().Contain("Customer churned");
    }

    // ========================================================================
    // GetPendingApprovalsAsync / GetReadyForPayoutAsync
    // ========================================================================
    [Fact]
    public async Task GetPendingApprovalsAsync_ShouldReturnOnlyPending()
    {
        // Arrange
        _commissions.AddRange(new[]
        {
            new Commission { Id = 1, Status = CommissionStatus.Pending, IsDeleted = false },
            new Commission { Id = 2, Status = CommissionStatus.Approved, IsDeleted = false },
            new Commission { Id = 3, Status = CommissionStatus.Pending, IsDeleted = false }
        });

        // Act
        var result = await _service.GetPendingApprovalsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.Status == CommissionStatus.Pending);
    }

    [Fact]
    public async Task GetReadyForPayoutAsync_ShouldReturnOnlyApproved()
    {
        // Arrange
        _commissions.AddRange(new[]
        {
            new Commission { Id = 1, Status = CommissionStatus.Approved, IsDeleted = false },
            new Commission { Id = 2, Status = CommissionStatus.Pending, IsDeleted = false },
            new Commission { Id = 3, Status = CommissionStatus.Approved, IsDeleted = false }
        });

        // Act
        var result = await _service.GetReadyForPayoutAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.Status == CommissionStatus.Approved);
    }

    // ========================================================================
    // Commission Plans
    // ========================================================================
    [Fact]
    public async Task CreatePlanAsync_ShouldAddPlan()
    {
        // Arrange
        var plan = new CommissionPlan { Name = "Standard Plan", BaseRate = 10m, Status = CommissionPlanStatus.Active };

        // Act
        var result = await _service.CreatePlanAsync(plan);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Standard Plan");
        _plans.Should().Contain(result);
    }

    [Fact]
    public async Task DeletePlanAsync_ShouldSoftDelete()
    {
        // Arrange
        _plans.Add(new CommissionPlan { Id = 1, Name = "Old Plan", IsDeleted = false });

        // Act
        var result = await _service.DeletePlanAsync(1);

        // Assert
        result.Should().BeTrue();
        _plans.First(p => p.Id == 1).IsDeleted.Should().BeTrue();
    }

    // ========================================================================
    // Tiers
    // ========================================================================
    [Fact]
    public async Task AddTierAsync_ShouldAddTierToPlan()
    {
        // Arrange
        _plans.Add(new CommissionPlan { Id = 1, Name = "Plan", IsDeleted = false });
        var tier = new CommissionTier { Name = "Gold", CommissionRate = 15m, MinAttainmentPercent = 100m };

        // Act
        var result = await _service.AddTierAsync(1, tier);

        // Assert
        result.Should().NotBeNull();
        result.CommissionPlanId.Should().Be(1);
        _tiers.Should().Contain(result);
    }

    [Fact]
    public async Task RemoveTierAsync_ShouldSoftDelete()
    {
        // Arrange
        _tiers.Add(new CommissionTier { Id = 1, CommissionPlanId = 1, Name = "Bronze", IsDeleted = false });

        // Act
        var result = await _service.RemoveTierAsync(1);

        // Assert
        result.Should().BeTrue();
        _tiers.First(t => t.Id == 1).IsDeleted.Should().BeTrue();
    }

    // ========================================================================
    // Statements
    // ========================================================================
    [Fact]
    public async Task FinalizeStatementAsync_ShouldSetFinalizedStatus()
    {
        // Arrange
        _statements.Add(new CommissionStatement
        {
            Id = 1,
            UserId = 1,
            Status = CommissionStatementStatus.Draft,
            IsDeleted = false
        });

        // Act
        var result = await _service.FinalizeStatementAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(CommissionStatementStatus.Finalized);
        result.FinalizedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
