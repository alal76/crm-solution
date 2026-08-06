// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ApprovalStatus = CRM.Core.Entities.ITSM.ApprovalStatus;

namespace CRM.Tests.Services.ITSM;

public class ChangeManagementServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ICMDBService> _mockCmdbService;
    private readonly Mock<ILogger<ChangeManagementService>> _mockLogger;
    private readonly ChangeManagementService _service;

    public ChangeManagementServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"ChangeTestDb_{Guid.NewGuid()}")
            .Options;
        _context = new CrmDbContext(options, null);
        // Seed a user so Include(c => c.Requestor) navigation can be resolved
        _context.Users.Add(new User { Id = 1, Username = "testuser", Email = "test@test.com", FirstName = "Test", LastName = "User", PasswordHash = "hash", CreatedAt = DateTime.UtcNow });
        _context.SaveChanges();
        _mockCmdbService = new Mock<ICMDBService>();
        _mockLogger = new Mock<ILogger<ChangeManagementService>>();
        _service = new ChangeManagementService(_context, _mockCmdbService.Object, _mockLogger.Object);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CreateChangeAsync_ShouldAddChangeToDatabase()
    {
        var dto = new CreateChangeDto
        {
            ShortDescription = "Deploy new firmware",
            Type = ChangeType.Normal,
            Risk = ChangeRisk.Medium,
            Impact = ChangeImpact.Medium
        };

        var result = await _service.CreateChangeAsync(dto, createdById: 1);

        result.Should().NotBeNull();
        result.ShortDescription.Should().Be("Deploy new firmware");
        _context.Changes.Count().Should().Be(1);
    }

    [Fact]
    public async Task CreateChangeAsync_ShouldSetStateToNew()
    {
        var dto = new CreateChangeDto
        {
            ShortDescription = "Network upgrade",
            Type = ChangeType.Emergency,
            Risk = ChangeRisk.High,
            Impact = ChangeImpact.High
        };

        var result = await _service.CreateChangeAsync(dto, createdById: 1);

        result.State.Should().Be(ChangeState.New);
    }

    [Fact]
    public async Task CreateChangeAsync_ShouldGenerateChangeNumber()
    {
        var dto = new CreateChangeDto
        {
            ShortDescription = "Change with number",
            Type = ChangeType.Standard,
            Risk = ChangeRisk.Low,
            Impact = ChangeImpact.Low
        };

        var result = await _service.CreateChangeAsync(dto, createdById: 1);

        result.Number.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetChangeByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetChangeByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetChangeByIdAsync_ShouldReturnChange_WhenExists()
    {
        var change = new Change
        {
            Number = "CHG0001",
            ShortDescription = "Existing Change",
            Type = ChangeType.Normal,
            Risk = ChangeRisk.Low,
            Impact = ChangeImpact.Low,
            State = ChangeState.New,
            RequestorId = 1,
            CreatedAt = DateTime.UtcNow
        };
        _context.Changes.Add(change);
        await _context.SaveChangesAsync();

        var result = await _service.GetChangeByIdAsync(change.ChangeId);

        result.Should().NotBeNull();
        result!.ShortDescription.Should().Be("Existing Change");
    }

    [Fact]
    public async Task SubmitForApprovalAsync_ShouldTransitionToAssess_WhenStateIsNew()
    {
        var change = new Change
        {
            Number = "CHG0002",
            ShortDescription = "Approval Test",
            Type = ChangeType.Normal,
            Risk = ChangeRisk.Medium,
            Impact = ChangeImpact.Medium,
            State = ChangeState.New,
            RequestorId = 1,
            CreatedAt = DateTime.UtcNow
        };
        _context.Changes.Add(change);
        await _context.SaveChangesAsync();

        var result = await _service.SubmitForApprovalAsync(change.ChangeId, modifiedById: 1);

        result.Should().BeTrue();
        var updated = await _context.Changes.FindAsync(change.ChangeId);
        updated!.State.Should().Be(ChangeState.Assess);
    }

    [Fact]
    public async Task SubmitForApprovalAsync_ShouldThrow_WhenChangeNotFound()
    {
        var act = async () => await _service.SubmitForApprovalAsync(999, modifiedById: 1);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ApproveChangeAsync_ShouldAddApprovalRecord()
    {
        var change = new Change
        {
            Number = "CHG0003",
            ShortDescription = "Approve Test",
            Type = ChangeType.Normal,
            Risk = ChangeRisk.Low,
            Impact = ChangeImpact.Low,
            State = ChangeState.Assess,
            RequestorId = 1,
            CreatedAt = DateTime.UtcNow
        };
        _context.Changes.Add(change);
        await _context.SaveChangesAsync();

        var result = await _service.ApproveChangeAsync(change.ChangeId, approverId: 2, comments: "Looks good");

        result.Should().BeTrue();
        _context.ChangeApprovals.Count().Should().Be(1);
        var approval = await _context.ChangeApprovals.FirstAsync();
        approval.ApprovalStatus.Should().Be(ApprovalStatus.Approved);
    }

    [Fact]
    public async Task GetChangesAsync_ShouldFilterByState()
    {
        _context.Changes.AddRange(
            new Change { Number = "CHG00A", ShortDescription = "New Change", Type = ChangeType.Standard, Risk = ChangeRisk.Low, Impact = ChangeImpact.Low, State = ChangeState.New, RequestorId = 1, CreatedAt = DateTime.UtcNow },
            new Change { Number = "CHG00B", ShortDescription = "Assess Change", Type = ChangeType.Normal, Risk = ChangeRisk.Medium, Impact = ChangeImpact.Medium, State = ChangeState.Assess, RequestorId = 1, CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        var filter = new ChangeFilterDto { State = ChangeState.New, PageNumber = 1, PageSize = 20 };
        var (items, totalCount) = await _service.GetChangesAsync(filter);

        totalCount.Should().Be(1);
        items.First().State.Should().Be(ChangeState.New);
    }
}
