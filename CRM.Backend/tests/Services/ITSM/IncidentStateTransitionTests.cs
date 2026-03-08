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
/// ITSM-045: Negative/edge-case tests for incident state transitions,
/// constraint violations, and invalid operations.
/// </summary>
public class IncidentStateTransitionTests : ServiceTestFixtureBase<IncidentService>
{
    private readonly Mock<ISLAService> _mockSlaService;
    private readonly IncidentService _service;

    public IncidentStateTransitionTests()
    {
        _mockSlaService = new Mock<ISLAService>();
        _service = new IncidentService(MockContext.Object, _mockSlaService.Object, MockLogger.Object);
    }

    // --- CloseIncidentAsync state checks ---

    [Fact]
    public async Task CloseIncidentAsync_ShouldThrowInvalidOperation_WhenIncidentIsNotResolved()
    {
        // Arrange
        var incident = new Incident
        {
            IncidentId = 1,
            ShortDescription = "Test",
            State = IncidentState.InProgress,
            IsDeleted = false
        };
        MockContext.Setup(c => c.Incidents.FindAsync(1)).ReturnsAsync(incident);

        // Act
        var act = () => _service.CloseIncidentAsync(1, 100);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not resolved*");
    }

    [Fact]
    public async Task CloseIncidentAsync_ShouldReturnFalse_WhenIncidentNotFound()
    {
        // Arrange
        MockContext.Setup(c => c.Incidents.FindAsync(999)).ReturnsAsync((Incident?)null);

        // Act
        var result = await _service.CloseIncidentAsync(999, 100);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CloseIncidentAsync_ShouldReturnFalse_WhenIncidentIsSoftDeleted()
    {
        // Arrange
        var incident = new Incident
        {
            IncidentId = 2,
            ShortDescription = "Deleted",
            State = IncidentState.Resolved,
            IsDeleted = true
        };
        MockContext.Setup(c => c.Incidents.FindAsync(2)).ReturnsAsync(incident);

        // Act
        var result = await _service.CloseIncidentAsync(2, 100);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(IncidentState.New)]
    [InlineData(IncidentState.Assigned)]
    [InlineData(IncidentState.OnHold)]
    [InlineData(IncidentState.Closed)]
    [InlineData(IncidentState.Cancelled)]
    public async Task CloseIncidentAsync_ShouldThrowInvalidOperation_WhenNotInResolvedState(IncidentState state)
    {
        // Arrange
        var incident = new Incident
        {
            IncidentId = 10,
            ShortDescription = "Test",
            State = state,
            IsDeleted = false
        };
        MockContext.Setup(c => c.Incidents.FindAsync(10)).ReturnsAsync(incident);

        // Act
        var act = () => _service.CloseIncidentAsync(10, 100);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // --- ReopenIncidentAsync state checks ---

    [Fact]
    public async Task ReopenIncidentAsync_ShouldThrowInvalidOperation_WhenIncidentIsNew()
    {
        // Arrange
        var incident = new Incident
        {
            IncidentId = 3,
            ShortDescription = "Test",
            State = IncidentState.New,
            IsDeleted = false
        };
        MockContext.Setup(c => c.Incidents.FindAsync(3)).ReturnsAsync(incident);

        // Act
        var act = () => _service.ReopenIncidentAsync(3, 100);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*reopen*");
    }

    [Theory]
    [InlineData(IncidentState.New)]
    [InlineData(IncidentState.Assigned)]
    [InlineData(IncidentState.InProgress)]
    [InlineData(IncidentState.OnHold)]
    public async Task ReopenIncidentAsync_ShouldThrowInvalidOperation_WhenNotResolvedOrClosed(IncidentState state)
    {
        // Arrange
        var incident = new Incident
        {
            IncidentId = 11,
            ShortDescription = "Test",
            State = state,
            IsDeleted = false
        };
        MockContext.Setup(c => c.Incidents.FindAsync(11)).ReturnsAsync(incident);

        // Act
        var act = () => _service.ReopenIncidentAsync(11, 100);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // --- ResolveIncidentAsync ---

    [Fact]
    public async Task ResolveIncidentAsync_ShouldThrowKeyNotFound_WhenIncidentNotFound()
    {
        // Arrange
        MockContext.Setup(c => c.Incidents.FindAsync(999)).ReturnsAsync((Incident?)null);
        var dto = new ResolveIncidentDto
        {
            ResolutionCode = ResolutionCode.SolvedPermanently,
            ResolutionNotes = "Fixed"
        };

        // Act
        var act = () => _service.ResolveIncidentAsync(999, dto, 100);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ResolveIncidentAsync_ShouldThrowKeyNotFound_WhenIncidentSoftDeleted()
    {
        // Arrange
        var incident = new Incident { IncidentId = 4, ShortDescription = "Deleted", IsDeleted = true };
        MockContext.Setup(c => c.Incidents.FindAsync(4)).ReturnsAsync(incident);
        var dto = new ResolveIncidentDto
        {
            ResolutionCode = ResolutionCode.SolvedPermanently,
            ResolutionNotes = "Fixed"
        };

        // Act
        var act = () => _service.ResolveIncidentAsync(4, dto, 100);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // --- EscalateIncidentAsync ---

    [Fact]
    public async Task EscalateIncidentAsync_ShouldReturnFalse_WhenIncidentNotFound()
    {
        // Arrange
        MockContext.Setup(c => c.Incidents.FindAsync(999)).ReturnsAsync((Incident?)null);

        // Act
        var result = await _service.EscalateIncidentAsync(999, 100);

        // Assert
        result.Should().BeFalse();
    }

    // --- AssignIncidentAsync ---

    [Fact]
    public async Task AssignIncidentAsync_ShouldReturnFalse_WhenIncidentNotFound()
    {
        // Arrange
        MockContext.Setup(c => c.Incidents.FindAsync(999)).ReturnsAsync((Incident?)null);

        // Act
        var result = await _service.AssignIncidentAsync(999, 1, null, 100);

        // Assert
        result.Should().BeFalse();
    }

    // --- GetIncidentByIdAsync ---

    [Fact]
    public async Task GetIncidentByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange — Set up an empty collection
        var incidents = new List<Incident>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(incidents);
        MockContext.Setup(c => c.Incidents).Returns(mockSet.Object);

        // Act
        var result = await _service.GetIncidentByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }
}
