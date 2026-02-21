// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Comprehensive unit tests for CommissionService
/// Uses InMemory database to properly support async EF Core operations (Include, ToListAsync, etc.)
/// </summary>
public class CommissionServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<CommissionService>> _mockLogger;
    private readonly CommissionService _commissionService;

    public CommissionServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<CommissionService>>();
        _commissionService = new CommissionService(_dbContext, _mockLogger.Object);

        // Seed Users required for Include(c => c.User) navigation
        SeedUsersAsync().GetAwaiter().GetResult();
    }

    private async Task SeedUsersAsync()
    {
        _dbContext.Users.AddRange(
            new User { Id = 1, Username = "user1", Email = "user1@test.com", PasswordHash = "hash", FirstName = "User", LastName = "One" },
            new User { Id = 2, Username = "user2", Email = "user2@test.com", PasswordHash = "hash", FirstName = "User", LastName = "Two" },
            new User { Id = 10, Username = "approver", Email = "approver@test.com", PasswordHash = "hash", FirstName = "Approver", LastName = "User" }
        );

        // Seed a default CommissionPlan for FK references
        _dbContext.CommissionPlans.Add(new CommissionPlan { Id = 1, Name = "Default Plan", BaseRate = 0.05m, Status = CommissionPlanStatus.Active, CommissionType = CommissionType.FlatPercentage });

        await _dbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    #region CRUD Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCommissions_WhenNoFilterApplied()
    {
        // Arrange - users and commission plan already seeded in constructor
        _dbContext.Commissions.AddRange(
            new Commission { UserId = 1, CommissionAmount = 1000m, Status = CommissionStatus.Pending, CommissionPlanId = 1 },
            new Commission { UserId = 2, CommissionAmount = 2000m, Status = CommissionStatus.Approved, CommissionPlanId = 1 }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _commissionService.GetAllAsync(cancellationToken: CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCommission_WhenIdExists()
    {
        // Arrange
        var commission = new Commission { UserId = 1, CommissionAmount = 1500m, CommissionPlanId = 1 };
        _dbContext.Commissions.Add(commission);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _commissionService.GetByIdAsync(commission.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Amount.Should().Be(1500m);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdNotExists()
    {
        // Act
        var result = await _commissionService.GetByIdAsync(999, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateCommission_WhenValidDataProvided()
    {
        // Arrange
        var commission = new Commission
        {
            UserId = 1,
            CommissionAmount = 2500m,
            Status = CommissionStatus.Pending,
            CommissionPlanId = 1
        };

        // Act
        var result = await _commissionService.CreateAsync(commission, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(2500m);
        result.Id.Should().BeGreaterThan(0);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.IsDeleted.Should().BeFalse();

        // Verify persisted
        var fromDb = await _dbContext.Commissions.FindAsync(result.Id);
        fromDb.Should().NotBeNull();
        fromDb!.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCommission_WhenValidDataProvided()
    {
        // Arrange - create existing commission first
        var existing = new Commission
        {
            UserId = 1,
            CommissionAmount = 1000m,
            Status = CommissionStatus.Pending,
            CommissionPlanId = 1
        };
        _dbContext.Commissions.Add(existing);
        await _dbContext.SaveChangesAsync();
        var existingId = existing.Id;

        // Detach everything so the service's Update can track the new instance
        _dbContext.ChangeTracker.Clear();

        var updated = new Commission
        {
            Id = existingId,
            UserId = 1,
            CommissionAmount = 3000m,
            Status = CommissionStatus.Approved,
            CommissionPlanId = 1
        };

        // Act
        var result = await _commissionService.UpdateAsync(updated, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(3000m);
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteCommission_WhenIdExists()
    {
        // Arrange
        var commission = new Commission { UserId = 1, CommissionAmount = 1000m, IsDeleted = false, CommissionPlanId = 1 };
        _dbContext.Commissions.Add(commission);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _commissionService.DeleteAsync(commission.Id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var fromDb = await _dbContext.Commissions.FindAsync(commission.Id);
        fromDb!.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region Commission Calculation Tests

    [Fact]
    public async Task CalculateForDealAsync_ShouldCalculateCommission_WhenOpportunityValid()
    {
        // Arrange - service uses Include(o => o.SalesOwner) and reads SalesOwnerId
        var plan = new CommissionPlan { Name = "Standard", BaseRate = 0.05m, Status = CommissionPlanStatus.Active, CommissionType = CommissionType.FlatPercentage };
        _dbContext.CommissionPlans.Add(plan);
        await _dbContext.SaveChangesAsync();

        var opportunity = new Opportunity { Name = "Big Deal", Amount = 10000m, SalesOwnerId = 1 };
        _dbContext.Opportunities.Add(opportunity);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _commissionService.CalculateForDealAsync(opportunity.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FinalAmount.Should().Be(500m); // 10000 * 0.05
    }

    [Fact]
    public async Task CalculateForOrderAsync_ShouldCalculateCommission_WhenOrderValid()
    {
        // Arrange - use the default seeded plan with 5% rate
        var order = new Order { TotalAmount = 5000m };
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _commissionService.CalculateForOrderAsync(order.Id, CancellationToken.None);

        // Assert - uses default 5% rate (no user assignment)
        result.Should().NotBeNull();
        result.FinalAmount.Should().Be(250m); // 5000 * 0.05 (default rate when no plan assigned)
    }

    [Fact]
    public async Task CalculateForPeriodAsync_ShouldCalculateSummary_WhenValidDateRange()
    {
        // Arrange
        var userId = 1;
        var fromDate = new DateTime(2024, 01, 01);
        var toDate = new DateTime(2024, 01, 31);

        _dbContext.Commissions.AddRange(
            new Commission { UserId = userId, CommissionAmount = 500m, CreatedAt = new DateTime(2024, 01, 15), CommissionPlanId = 1 },
            new Commission { UserId = userId, CommissionAmount = 750m, CreatedAt = new DateTime(2024, 01, 20), CommissionPlanId = 1 }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _commissionService.CalculateForPeriodAsync(userId, fromDate, toDate, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalEarned.Should().Be(1250m);
        result.DealCount.Should().Be(2);
    }

    #endregion

    #region Approval Workflow Tests

    [Fact]
    public async Task ApproveAsync_ShouldApproveCommission_WhenPending()
    {
        // Arrange
        var approvedById = 10;
        var commission = new Commission
        {
            UserId = 1,
            Status = CommissionStatus.Pending,
            CommissionPlanId = 1
        };
        _dbContext.Commissions.Add(commission);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _commissionService.ApproveAsync(commission.Id, approvedById, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(CommissionStatus.Approved);
        result.ApprovedById.Should().Be(approvedById);
        result.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RejectAsync_ShouldSetStatusToCancelled_WhenPending()
    {
        // Arrange - note: RejectAsync sets Status = Cancelled and appends to Notes
        var reason = "Does not meet criteria";
        var commission = new Commission
        {
            UserId = 1,
            Status = CommissionStatus.Pending,
            CommissionPlanId = 1
        };
        _dbContext.Commissions.Add(commission);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _commissionService.RejectAsync(commission.Id, reason, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(CommissionStatus.Cancelled);
        result.Notes.Should().Contain("Rejected:");
        result.Notes.Should().Contain(reason);
    }

    #endregion

    #region Payout Tests

    [Fact]
    public async Task MarkAsPaidAsync_ShouldMarkCommissionAsPaid_WhenApproved()
    {
        // Arrange
        var paidDate = DateTime.UtcNow;
        var commission = new Commission
        {
            UserId = 1,
            Status = CommissionStatus.Approved,
            CommissionPlanId = 1
        };
        _dbContext.Commissions.Add(commission);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _commissionService.MarkAsPaidAsync(commission.Id, paidDate, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(CommissionStatus.Paid);
        result.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetReadyForPayoutAsync_ShouldReturnApprovedCommissions()
    {
        // Arrange
        _dbContext.Commissions.AddRange(
            new Commission { UserId = 1, Status = CommissionStatus.Approved, CommissionAmount = 500m, CommissionPlanId = 1 },
            new Commission { UserId = 2, Status = CommissionStatus.Approved, CommissionAmount = 750m, CommissionPlanId = 1 },
            new Commission { UserId = 1, Status = CommissionStatus.Pending, CommissionAmount = 100m, CommissionPlanId = 1 }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _commissionService.GetReadyForPayoutAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(c => c.Status.Should().Be(CommissionStatus.Approved));
    }

    #endregion

    #region Plan Management Tests

    [Fact]
    public async Task GetPlansAsync_ShouldReturnAllPlans()
    {
        // Arrange - one plan already seeded in constructor
        _dbContext.CommissionPlans.AddRange(
            new CommissionPlan { Name = "Plan A", IsActive = true, Status = CommissionPlanStatus.Active },
            new CommissionPlan { Name = "Plan B", IsActive = false, Status = CommissionPlanStatus.Inactive }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _commissionService.GetPlansAsync(cancellationToken: CancellationToken.None);

        // Assert - 1 seeded + 2 added = 3
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task CreatePlanAsync_ShouldCreateNewPlan_WhenValidDataProvided()
    {
        // Arrange
        var plan = new CommissionPlan
        {
            Name = "New Plan",
            BaseRate = 0.15m,
            CommissionType = CommissionType.FlatPercentage,
            IsActive = true
        };

        // Act
        var result = await _commissionService.CreatePlanAsync(plan, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Plan");
        result.IsActive.Should().BeTrue();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task AssignPlanToUserAsync_ShouldAssignPlan_WhenValidIds()
    {
        // Arrange
        var plan = new CommissionPlan { Name = "Plan A", Status = CommissionPlanStatus.Active };
        _dbContext.CommissionPlans.Add(plan);
        var user = new User { FirstName = "Test", LastName = "User", Email = "test@test.com", Username = "testuser", PasswordHash = "hash" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _commissionService.AssignPlanToUserAsync(plan.Id, user.Id, cancellationToken: CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        // Verify assignment persisted
        var assignments = _dbContext.CommissionPlanAssignments.Where(a => a.UserId == user.Id && !a.IsDeleted).ToList();
        assignments.Should().ContainSingle();
        assignments[0].CommissionPlanId.Should().Be(plan.Id);
        assignments[0].IsActive.Should().BeTrue();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnStatistics()
    {
        // Arrange
        _dbContext.CommissionPlans.Add(new CommissionPlan { Name = "Active Plan", Status = CommissionPlanStatus.Active });
        _dbContext.Commissions.AddRange(
            new Commission { UserId = 1, CommissionAmount = 1000m, Status = CommissionStatus.Paid, CommissionPlanId = 1 },
            new Commission { UserId = 2, CommissionAmount = 2000m, Status = CommissionStatus.Approved, CommissionPlanId = 1 },
            new Commission { UserId = 1, CommissionAmount = 500m, Status = CommissionStatus.Pending, CommissionPlanId = 1 }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _commissionService.GetStatisticsAsync(cancellationToken: CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalAmount.Should().Be(3500m);
        result.CommissionCount.Should().Be(3);
        result.PendingApprovals.Should().Be(1);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public async Task GetAllAsync_WithUserFilter_ShouldReturnOnlyUserCommissions()
    {
        // Arrange
        _dbContext.Commissions.AddRange(
            new Commission { UserId = 1, CommissionAmount = 1000m, CommissionPlanId = 1 },
            new Commission { UserId = 2, CommissionAmount = 2000m, CommissionPlanId = 1 }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _commissionService.GetAllAsync(userId: 1, cancellationToken: CancellationToken.None);

        // Assert
        result.Should().ContainSingle();
        result.Should().AllSatisfy(c => c.UserId.Should().Be(1));
    }

    [Fact]
    public async Task ClawbackAsync_ShouldUpdateCommissionStatus_WhenValid()
    {
        // Arrange
        var reason = "Clawback: Customer churn";
        var commission = new Commission
        {
            UserId = 1,
            Status = CommissionStatus.Paid,
            CommissionAmount = 1000m,
            CommissionPlanId = 1
        };
        _dbContext.Commissions.Add(commission);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _commissionService.ClawbackAsync(commission.Id, reason, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(CommissionStatus.Clawback);
        result.Notes.Should().Contain("Clawback:");
    }

    [Fact]
    public async Task RecalculateAsync_ShouldRecalculateCommission_WhenNoLinkedDeal()
    {
        // Arrange - commission with no OpportunityId/OrderId stays same amount
        var commission = new Commission
        {
            UserId = 1,
            CommissionAmount = 1000m,
            Status = CommissionStatus.Pending,
            CommissionPlanId = 1
        };
        _dbContext.Commissions.Add(commission);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _commissionService.RecalculateAsync(commission.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(commission.Id);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdNotExists()
    {
        // Act
        var result = await _commissionService.DeleteAsync(999, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenCommissionNotFound()
    {
        // Arrange
        var commission = new Commission { Id = 999, UserId = 1, CommissionPlanId = 1 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _commissionService.UpdateAsync(commission, CancellationToken.None));
    }

    #endregion
}
