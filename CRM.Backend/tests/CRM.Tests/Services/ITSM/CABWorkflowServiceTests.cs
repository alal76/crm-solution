// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.ITSM;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

// Disambiguate from CRM.Core.Entities.ApprovalStatus
using ApprovalStatus = CRM.Core.Entities.ITSM.ApprovalStatus;

namespace CRM.Tests.Services.ITSM;

public class CABWorkflowServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<CABWorkflowService>> _mockLogger;
    private readonly CABWorkflowService _service;

    public CABWorkflowServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"CABWorkflowTestDb_{Guid.NewGuid()}")
            .Options;
        _context = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<CABWorkflowService>>();
        _service = new CABWorkflowService(_context, _mockLogger.Object);
    }

    public void Dispose() => _context.Dispose();

    private async Task<Change> SeedChangeAsync(ChangeState state = ChangeState.New,
        ChangeType type = ChangeType.Normal, ChangeRisk risk = ChangeRisk.High)
    {
        var change = new Change
        {
            Number = $"CHG{Guid.NewGuid():N}".Substring(0, 10),
            ShortDescription = "Test Change",
            Type = type,
            Risk = risk,
            Impact = ChangeImpact.Medium,
            State = state,
            RequestorId = 1,
            CreatedAt = DateTime.UtcNow
        };
        _context.Changes.Add(change);
        await _context.SaveChangesAsync();
        return change;
    }

    [Fact]
    public async Task InitializeWorkflowAsync_ShouldThrow_WhenChangeNotFound()
    {
        var act = async () => await _service.InitializeWorkflowAsync(999, initiatedById: 1);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task InitializeWorkflowAsync_ShouldReturnWorkflow_WhenChangeExists()
    {
        var change = await SeedChangeAsync();

        var result = await _service.InitializeWorkflowAsync(change.ChangeId, initiatedById: 1);

        result.Should().NotBeNull();
        result.ChangeRequestId.Should().Be(change.ChangeId);
        result.InitiatedById.Should().Be(1);
    }

    [Fact]
    public async Task InitializeWorkflowAsync_ShouldUpdateChangeStateToAwaitingApproval()
    {
        var change = await SeedChangeAsync();

        await _service.InitializeWorkflowAsync(change.ChangeId, initiatedById: 1);

        var updated = await _context.Changes.FindAsync(change.ChangeId);
        updated!.State.Should().Be(ChangeState.AwaitingApproval);
    }

    [Fact]
    public async Task SubmitApprovalAsync_ShouldReturnFalse_WhenNoPendingApproval()
    {
        var change = await SeedChangeAsync();

        var result = await _service.SubmitApprovalAsync(change.ChangeId, approverId: 99, isApproved: true);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SubmitApprovalAsync_ShouldSetStateToRejected_WhenRejected()
    {
        var change = await SeedChangeAsync(ChangeState.AwaitingApproval);

        // Create a pending approval record
        var approval = new ChangeApproval
        {
            ChangeId = change.ChangeId,
            ApproverId = 5,
            ApprovalRole = ApprovalRole.ChangeManager,
            ApprovalStatus = ApprovalStatus.Requested,
            CreatedAt = DateTime.UtcNow
        };
        _context.ChangeApprovals.Add(approval);
        await _context.SaveChangesAsync();

        await _service.SubmitApprovalAsync(change.ChangeId, approverId: 5, isApproved: false, comments: "Too risky");

        var updated = await _context.Changes.FindAsync(change.ChangeId);
        updated!.State.Should().Be(ChangeState.Rejected);
    }

    [Fact]
    public async Task SubmitApprovalAsync_ShouldSetChangeToApproved_WhenAllApprovalsComplete()
    {
        var change = await SeedChangeAsync(ChangeState.AwaitingApproval);

        var approval = new ChangeApproval
        {
            ChangeId = change.ChangeId,
            ApproverId = 3,
            ApprovalRole = ApprovalRole.ChangeManager,
            ApprovalStatus = ApprovalStatus.Requested,
            CreatedAt = DateTime.UtcNow
        };
        _context.ChangeApprovals.Add(approval);
        await _context.SaveChangesAsync();

        var result = await _service.SubmitApprovalAsync(change.ChangeId, approverId: 3, isApproved: true);

        result.Should().BeTrue();
        var updated = await _context.Changes.FindAsync(change.ChangeId);
        updated!.State.Should().Be(ChangeState.Approved);
    }

    [Fact]
    public async Task GetWorkflowStatusAsync_ShouldReturnNull_WhenChangeNotFound()
    {
        var result = await _service.GetWorkflowStatusAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWorkflowStatusAsync_ShouldReturnStatus_WhenApprovalsExist()
    {
        var change = await SeedChangeAsync(ChangeState.AwaitingApproval);
        _context.ChangeApprovals.Add(new ChangeApproval
        {
            ChangeId = change.ChangeId,
            ApproverId = 2,
            ApprovalRole = ApprovalRole.ChangeManager,
            ApprovalStatus = ApprovalStatus.Requested,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetWorkflowStatusAsync(change.ChangeId);

        result.Should().NotBeNull();
        result!.ChangeRequestId.Should().Be(change.ChangeId);
        result.TotalStages.Should().Be(1);
    }
}
