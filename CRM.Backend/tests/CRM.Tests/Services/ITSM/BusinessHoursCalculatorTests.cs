// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using System.Threading.Tasks;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Unit tests for <see cref="BusinessHoursCalculator"/>.
/// All tests pass <c>scheduleId = null</c> so the service uses its built-in
/// default schedule (Mon–Fri, 09:00–17:00 UTC) — no database queries are issued.
/// </summary>
public class BusinessHoursCalculatorServiceTests
{
    private readonly BusinessHoursCalculator _calculator;

    public BusinessHoursCalculatorServiceTests()
    {
        var mockDb = new Mock<ICrmDbContext>();
        var mockLogger = new Mock<ILogger<BusinessHoursCalculator>>();
        _calculator = new BusinessHoursCalculator(mockDb.Object, mockLogger.Object);
    }

    // ──────────────────────────────────────────────────────────────────
    // IsBusinessTimeAsync — uses default schedule
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task IsBusinessTimeAsync_MondayAt10Am_ReturnsTrue()
    {
        // 2025-01-06 is a Monday
        var time = new DateTime(2025, 1, 6, 10, 0, 0, DateTimeKind.Utc);
        var result = await _calculator.IsBusinessTimeAsync(time);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsBusinessTimeAsync_MondayAt8Am_ReturnsFalse()
    {
        // Before 09:00 business start
        var time = new DateTime(2025, 1, 6, 8, 0, 0, DateTimeKind.Utc);
        var result = await _calculator.IsBusinessTimeAsync(time);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsBusinessTimeAsync_MondayAt17Pm_ReturnsFalse()
    {
        // At exactly 17:00 (end of day, exclusive)
        var time = new DateTime(2025, 1, 6, 17, 0, 0, DateTimeKind.Utc);
        var result = await _calculator.IsBusinessTimeAsync(time);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsBusinessTimeAsync_Saturday_ReturnsFalse()
    {
        // 2025-01-11 is Saturday
        var time = new DateTime(2025, 1, 11, 10, 0, 0, DateTimeKind.Utc);
        var result = await _calculator.IsBusinessTimeAsync(time);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsBusinessTimeAsync_Sunday_ReturnsFalse()
    {
        // 2025-01-12 is Sunday
        var time = new DateTime(2025, 1, 12, 14, 0, 0, DateTimeKind.Utc);
        var result = await _calculator.IsBusinessTimeAsync(time);
        result.Should().BeFalse();
    }

    // ──────────────────────────────────────────────────────────────────
    // AddBusinessMinutesAsync — uses default schedule
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddBusinessMinutesAsync_ZeroMinutes_ReturnsSameTime()
    {
        var start = new DateTime(2025, 1, 6, 10, 0, 0, DateTimeKind.Utc); // Mon 10:00
        var result = await _calculator.AddBusinessMinutesAsync(start, 0);
        result.Should().BeCloseTo(start, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task AddBusinessMinutesAsync_60MinutesWithinDay_AddsCorrectly()
    {
        var start = new DateTime(2025, 1, 6, 10, 0, 0, DateTimeKind.Utc); // Mon 10:00
        var expected = new DateTime(2025, 1, 6, 11, 0, 0, DateTimeKind.Utc); // Mon 11:00

        var result = await _calculator.AddBusinessMinutesAsync(start, 60);

        result.Should().BeCloseTo(expected, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task AddBusinessMinutesAsync_OverlapIntoNextDay_SkipsToNextMorning()
    {
        // Start at Mon 16:30 (30 min before end of business day)
        var start = new DateTime(2025, 1, 6, 16, 30, 0, DateTimeKind.Utc);

        // Ask for 60 min: 30 min fills Monday, then 30 min on Tuesday starting at 09:00
        var result = await _calculator.AddBusinessMinutesAsync(start, 60);

        // Expect Tuesday 09:30
        var expected = new DateTime(2025, 1, 7, 9, 30, 0, DateTimeKind.Utc);
        result.Should().BeCloseTo(expected, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task AddBusinessMinutesAsync_StartingOnWeekend_SkipsToNextMonday()
    {
        // 2025-01-11 is Saturday at 14:00
        var start = new DateTime(2025, 1, 11, 14, 0, 0, DateTimeKind.Utc);

        // 30 minutes of business time should land on Monday 09:30
        var result = await _calculator.AddBusinessMinutesAsync(start, 30);

        var expectedMonday = new DateTime(2025, 1, 13, 9, 30, 0, DateTimeKind.Utc); // 2025-01-13 is Monday
        result.Should().BeCloseTo(expectedMonday, TimeSpan.FromMinutes(1));
    }

    // ──────────────────────────────────────────────────────────────────
    // GetElapsedBusinessMinutesAsync — uses default schedule
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetElapsedBusinessMinutesAsync_EndBeforeStart_ReturnsZero()
    {
        var start = new DateTime(2025, 1, 7, 10, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2025, 1, 6, 10, 0, 0, DateTimeKind.Utc); // end < start

        var result = await _calculator.GetElapsedBusinessMinutesAsync(start, end);

        result.Should().Be(0);
    }

    [Fact]
    public async Task GetElapsedBusinessMinutesAsync_OneHourSameDay_Returns60()
    {
        var start = new DateTime(2025, 1, 6, 10, 0, 0, DateTimeKind.Utc); // Mon 10:00
        var end = new DateTime(2025, 1, 6, 11, 0, 0, DateTimeKind.Utc);   // Mon 11:00

        var result = await _calculator.GetElapsedBusinessMinutesAsync(start, end);

        result.Should().Be(60);
    }

    // ──────────────────────────────────────────────────────────────────
    // IsHolidayAsync — default schedule has no holidays
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task IsHolidayAsync_WithDefaultSchedule_ReturnsFalseForAnyDate()
    {
        var monday = new DateTime(2025, 1, 6, 0, 0, 0, DateTimeKind.Utc);

        var result = await _calculator.IsHolidayAsync(monday);

        result.Should().BeFalse();
    }
}
