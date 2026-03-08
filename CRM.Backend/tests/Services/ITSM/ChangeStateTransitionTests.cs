// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Services.ITSM;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// ITSM-045: Negative/edge-case tests for change management state transitions
/// and constraint violations.
/// </summary>
public class ChangeStateTransitionTests : ServiceTestFixtureBase<ChangeManagementService>
{
    private readonly Mock<ICMDBService> _mockCmdbService;
    private readonly ChangeManagementService _service;

    public ChangeStateTransitionTests()
    {
        _mockCmdbService = new Mock<ICMDBService>();
        _service = new ChangeManagementService(MockContext.Object, _mockCmdbService.Object, MockLogger.Object);
    }

    [Fact]
    public async Task SubmitForApprovalAsync_ShouldThrowKeyNotFound_WhenChangeNotFound()
    {
        // Arrange
        MockContext.Setup(c => c.Changes.FindAsync(999)).ReturnsAsync((Change?)null);

        // Act
        var act = () => _service.SubmitForApprovalAsync(999, 1);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task SubmitForApprovalAsync_ShouldThrowKeyNotFound_WhenChangeSoftDeleted()
    {
        // Arrange
        var change = new Change { ChangeId = 1, IsDeleted = true, State = ChangeState.New, ShortDescription = "Test", Type = CRM.Core.Entities.ITSM.ChangeType.Normal, Risk = ChangeRisk.Low, Impact = CRM.Core.Entities.ITSM.ChangeImpact.Low };
        MockContext.Setup(c => c.Changes.FindAsync(1)).ReturnsAsync(change);

        // Act
        var act = () => _service.SubmitForApprovalAsync(1, 1);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Theory]
    [InlineData(ChangeState.Assess)]
    [InlineData(ChangeState.Authorize)]
    [InlineData(ChangeState.Scheduled)]
    [InlineData(ChangeState.Implement)]
    [InlineData(ChangeState.Closed)]
    [InlineData(ChangeState.Cancelled)]
    public async Task SubmitForApprovalAsync_ShouldThrowInvalidOperation_WhenNotInNewState(ChangeState state)
    {
        // Arrange
        var change = new Change { ChangeId = 2, State = state, IsDeleted = false, ShortDescription = "Test", Type = CRM.Core.Entities.ITSM.ChangeType.Normal, Risk = ChangeRisk.Low, Impact = CRM.Core.Entities.ITSM.ChangeImpact.Low };
        MockContext.Setup(c => c.Changes.FindAsync(2)).ReturnsAsync(change);

        // Act
        var act = () => _service.SubmitForApprovalAsync(2, 1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*New state*");
    }

    [Fact]
    public async Task SubmitForApprovalAsync_ShouldTransitionToAssess_WhenInNewState()
    {
        // Arrange
        var change = new Change
        {
            ChangeId = 3,
            Number = "CHG001",
            State = ChangeState.New,
            IsDeleted = false,
            ShortDescription = "Test",
            Type = CRM.Core.Entities.ITSM.ChangeType.Normal,
            Risk = ChangeRisk.Low,
            Impact = CRM.Core.Entities.ITSM.ChangeImpact.Low
        };
        MockContext.Setup(c => c.Changes.FindAsync(3)).ReturnsAsync(change);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.SubmitForApprovalAsync(3, 1);

        // Assert
        result.Should().BeTrue();
        change.State.Should().Be(ChangeState.Assess);
    }

    [Fact]
    public async Task ApproveChangeAsync_ShouldTransitionToAuthorize_WhenInAssessState()
    {
        // Arrange
        var change = new Change
        {
            ChangeId = 4,
            Number = "CHG002",
            State = ChangeState.Assess,
            IsDeleted = false,
            ShortDescription = "Test",
            Type = CRM.Core.Entities.ITSM.ChangeType.Normal,
            Risk = ChangeRisk.Low,
            Impact = CRM.Core.Entities.ITSM.ChangeImpact.Low
        };
        MockContext.Setup(c => c.Changes.FindAsync(4)).ReturnsAsync(change);

        var approvals = new List<ChangeApproval>();
        var mockApprovals = MockDbSetFactory.CreateMockDbSet(approvals);
        MockContext.Setup(c => c.ChangeApprovals).Returns(mockApprovals.Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.ApproveChangeAsync(4, 100, "Approved");

        // Assert
        result.Should().BeTrue();
        change.State.Should().Be(ChangeState.Authorize);
    }

    [Fact]
    public async Task CreateChangeAsync_ShouldGenerateNumber_WhenValid()
    {
        // Arrange
        var changes = new List<Change>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(changes);
        MockContext.Setup(c => c.Changes).Returns(mockSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var impactedCIs = new List<CRM.Core.Entities.ITSM.ChangeImpactedCI>();
        var mockImpactedCISet = MockDbSetFactory.CreateMockDbSet(impactedCIs);
        MockContext.Setup(c => c.ChangeImpactedCIs).Returns(mockImpactedCISet.Object);

        var dto = new CreateChangeDto
        {
            ShortDescription = "Test Change",
            Description = "Test description",
            Type = CRM.Core.Entities.ITSM.ChangeType.Normal,
            Risk = ChangeRisk.Medium,
            Impact = CRM.Core.Entities.ITSM.ChangeImpact.Medium
        };

        // Act
        var result = await _service.CreateChangeAsync(dto, 1);

        // Assert
        result.Should().NotBeNull();
        result.Number.Should().NotBeNullOrEmpty();
    }
}
