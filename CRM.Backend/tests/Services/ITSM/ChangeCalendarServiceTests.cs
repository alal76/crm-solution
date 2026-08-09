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
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Tests for <see cref="ChangeCalendarService"/>'s current (non-obsolete) surface:
/// <see cref="ChangeCalendarService.FindAvailableSlotsAsync"/>, which now sources blackout
/// periods and scheduled changes from the injected <see cref="IChangeManagementServiceEx"/>
/// (real ChangeBlackout/Changes data) instead of the service's old in-memory placeholder data,
/// and <see cref="ChangeCalendarService.GetMaintenanceWindowsAsync"/>.
/// </summary>
public class ChangeCalendarServiceTests
{
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<ChangeCalendarService>> _mockLogger;
    private readonly Mock<IChangeManagementServiceEx> _mockChangeManagementServiceEx;
    private readonly ChangeCalendarService _service;

    public ChangeCalendarServiceTests()
    {
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ChangeCalendarService>>();
        _mockChangeManagementServiceEx = new Mock<IChangeManagementServiceEx>();
        _service = new ChangeCalendarService(_mockDbContext.Object, _mockLogger.Object, _mockChangeManagementServiceEx.Object);
    }

    private static ChangeDto MakeChangeDto(DateTime? start, DateTime? end, ChangeState state = ChangeState.New) => new()
    {
        ChangeId = 1,
        Number = "CHG0000001",
        ShortDescription = "Test change",
        State = state,
        Type = CRM.Core.Entities.ITSM.ChangeType.Normal,
        Risk = ChangeRisk.Medium,
        Impact = ChangeImpact.Medium,
        PlannedStartDate = start,
        PlannedEndDate = end,
        CreatedAt = DateTime.UtcNow
    };

    private void SetupNoBlackoutsOrChanges()
    {
        _mockChangeManagementServiceEx
            .Setup(s => s.GetBlackoutPeriodsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChangeBlackoutDto>());
        _mockChangeManagementServiceEx
            .Setup(s => s.ListChangesAsync(It.IsAny<ChangeFilterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enumerable.Empty<ChangeDto>(), 0));
    }

    // ────────────────────────────────────────────────────────────────
    // FindAvailableSlotsAsync
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task FindAvailableSlotsAsync_ShouldCallChangeManagementServiceEx_ForBlackoutsAndScheduledChanges()
    {
        SetupNoBlackoutsOrChanges();

        await _service.FindAvailableSlotsAsync(changeRequestId: 1, durationMinutes: 60, daysAhead: 14);

        _mockChangeManagementServiceEx.Verify(
            s => s.GetBlackoutPeriodsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _mockChangeManagementServiceEx.Verify(
            s => s.ListChangesAsync(
                It.Is<ChangeFilterDto>(f =>
                    f.PlannedStartFrom.HasValue &&
                    f.PlannedStartTo.HasValue &&
                    f.PageNumber == 1 &&
                    f.PageSize == 500),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task FindAvailableSlotsAsync_ShouldReturnOptimalSlots_WhenNoBlackoutsOrConflicts()
    {
        SetupNoBlackoutsOrChanges();

        // 14 days ahead guarantees at least one occurrence of every maintenance-window day of week.
        var slots = await _service.FindAvailableSlotsAsync(changeRequestId: 1, durationMinutes: 60, daysAhead: 14);

        slots.Should().NotBeEmpty();
        slots.Should().Contain(s => s.Quality == SlotQuality.Optimal && s.IsMaintenanceWindow);
    }

    [Fact]
    public async Task FindAvailableSlotsAsync_ShouldReturnNoSlots_WhenBlackoutCoversEntireWindow()
    {
        var blackoutCoveringEverything = new ChangeBlackoutDto
        {
            BlackoutId = 1,
            Name = "Full freeze",
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        _mockChangeManagementServiceEx
            .Setup(s => s.GetBlackoutPeriodsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChangeBlackoutDto> { blackoutCoveringEverything });
        _mockChangeManagementServiceEx
            .Setup(s => s.ListChangesAsync(It.IsAny<ChangeFilterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enumerable.Empty<ChangeDto>(), 0));

        var slots = await _service.FindAvailableSlotsAsync(changeRequestId: 1, durationMinutes: 60, daysAhead: 14);

        slots.Should().BeEmpty();
    }

    [Fact]
    public async Task FindAvailableSlotsAsync_ShouldReturnNoSlots_WhenScheduledChangeCoversEntireWindow()
    {
        var conflictingChange = MakeChangeDto(
            start: DateTime.UtcNow.AddDays(-1),
            end: DateTime.UtcNow.AddDays(30),
            state: ChangeState.Scheduled);

        _mockChangeManagementServiceEx
            .Setup(s => s.GetBlackoutPeriodsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChangeBlackoutDto>());
        _mockChangeManagementServiceEx
            .Setup(s => s.ListChangesAsync(It.IsAny<ChangeFilterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<ChangeDto> { conflictingChange }.AsEnumerable(), 1));

        var slots = await _service.FindAvailableSlotsAsync(changeRequestId: 2, durationMinutes: 60, daysAhead: 14);

        slots.Should().BeEmpty();
    }

    [Fact]
    public async Task FindAvailableSlotsAsync_ShouldIgnoreCancelledAndFailedChanges_WhenCheckingConflicts()
    {
        // A Cancelled change spanning the whole window should NOT block slots, since
        // FindAvailableSlotsAsync filters out Cancelled/Failed changes before conflict-checking.
        var cancelledChange = MakeChangeDto(
            start: DateTime.UtcNow.AddDays(-1),
            end: DateTime.UtcNow.AddDays(30),
            state: ChangeState.Cancelled);

        _mockChangeManagementServiceEx
            .Setup(s => s.GetBlackoutPeriodsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChangeBlackoutDto>());
        _mockChangeManagementServiceEx
            .Setup(s => s.ListChangesAsync(It.IsAny<ChangeFilterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<ChangeDto> { cancelledChange }.AsEnumerable(), 1));

        var slots = await _service.FindAvailableSlotsAsync(changeRequestId: 3, durationMinutes: 60, daysAhead: 14);

        slots.Should().NotBeEmpty();
    }

    // ────────────────────────────────────────────────────────────────
    // GetMaintenanceWindowsAsync
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMaintenanceWindowsAsync_ShouldReturnDefaultWindows()
    {
        var windows = await _service.GetMaintenanceWindowsAsync();

        windows.Should().HaveCount(3);
        windows.Should().Contain(w => w.DayOfWeek == DayOfWeek.Saturday && w.Name.Contains("Saturday"));
        windows.Should().Contain(w => w.DayOfWeek == DayOfWeek.Sunday && w.Name.Contains("Sunday"));
        windows.Should().Contain(w => w.DayOfWeek == DayOfWeek.Wednesday);
        windows.Should().OnlyContain(w => w.IsActive);
    }
}
