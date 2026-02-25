// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Threading.Tasks;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Unit tests for BusinessHoursCalculator.
/// Tests business hours calculations for SLA management.
/// </summary>
public class BusinessHoursCalculatorTests
{
    private readonly Mock<IDbContextResolver> _mockDbContextResolver;
    private readonly Mock<ILogger<BusinessHoursCalculator>> _mockLogger;
    private readonly BusinessHoursCalculator _calculator;

    public BusinessHoursCalculatorTests()
    {
        _mockDbContextResolver = new Mock<IDbContextResolver>();
        _mockLogger = new Mock<ILogger<BusinessHoursCalculator>>();
        _calculator = new BusinessHoursCalculator(_mockDbContextResolver.Object, _mockLogger.Object);
    }

    #region AddBusinessMinutesAsync Tests

    [Fact]
    public async Task AddBusinessMinutesAsync_WithZeroMinutes_ReturnsSameTime()
    {
        // Arrange
        var startTime = new DateTime(2025, 1, 6, 10, 0, 0, DateTimeKind.Utc); // Monday 10 AM

        // Act
        var result = await _calculator.AddBusinessMinutesAsync(startTime, 0);

        // Assert
        result.Should().BeCloseTo(startTime, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task AddBusinessMinutesAsync_WithinSameDay_CalculatesCorrectly()
    {
        // Arrange
        var startTime = new DateTime(2025, 1, 6, 10, 0, 0, DateTimeKind.Utc); // Monday 10 AM UTC
        var businessMinutes = 60; // 1 hour

        // Act
        var result = await _calculator.AddBusinessMinutesAsync(startTime, businessMinutes);

        // Assert
        result.Should().BeCloseTo(new DateTime(2025, 1, 6, 11, 0, 0, DateTimeKind.Utc), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task AddBusinessMinutesAsync_CrossingLunchTime_CalculatesCorrectly()
    {
        // Arrange - Default schedule is 9 AM - 5 PM (no lunch break in default)
        var startTime = new DateTime(2025, 1, 6, 12, 0, 0, DateTimeKind.Utc); // Monday noon UTC
        var businessMinutes = 300; // 5 hours

        // Act
        var result = await _calculator.AddBusinessMinutesAsync(startTime, businessMinutes);

        // Assert - 12 PM + 5 hours = 5 PM, at end of business day
        result.Should().BeCloseTo(new DateTime(2025, 1, 6, 17, 0, 0, DateTimeKind.Utc), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task AddBusinessMinutesAsync_CrossingEndOfDay_ContinuesNextDay()
    {
        // Arrange
        var startTime = new DateTime(2025, 1, 6, 16, 0, 0, DateTimeKind.Utc); // Monday 4 PM UTC
        var businessMinutes = 120; // 2 hours

        // Act
        var result = await _calculator.AddBusinessMinutesAsync(startTime, businessMinutes);

        // Assert - 1 hour remaining Monday + 1 hour Tuesday = Tuesday 10 AM
        result.Should().BeCloseTo(new DateTime(2025, 1, 7, 10, 0, 0, DateTimeKind.Utc), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task AddBusinessMinutesAsync_StartingOnWeekend_JumpsToMonday()
    {
        // Arrange
        var startTime = new DateTime(2025, 1, 4, 10, 0, 0, DateTimeKind.Utc); // Saturday 10 AM UTC
        var businessMinutes = 60; // 1 hour

        // Act
        var result = await _calculator.AddBusinessMinutesAsync(startTime, businessMinutes);

        // Assert - Should start Monday 9 AM + 1 hour = Monday 10 AM
        result.Should().BeCloseTo(new DateTime(2025, 1, 6, 10, 0, 0, DateTimeKind.Utc), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task AddBusinessMinutesAsync_StartingBeforeBusinessHours_JumpsToStart()
    {
        // Arrange
        var startTime = new DateTime(2025, 1, 6, 7, 0, 0, DateTimeKind.Utc); // Monday 7 AM UTC (before 9 AM start)
        var businessMinutes = 60; // 1 hour

        // Act
        var result = await _calculator.AddBusinessMinutesAsync(startTime, businessMinutes);

        // Assert - Should start at 9 AM + 1 hour = 10 AM
        result.Should().BeCloseTo(new DateTime(2025, 1, 6, 10, 0, 0, DateTimeKind.Utc), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task AddBusinessMinutesAsync_StartingAfterBusinessHours_JumpsToNextDay()
    {
        // Arrange
        var startTime = new DateTime(2025, 1, 6, 18, 0, 0, DateTimeKind.Utc); // Monday 6 PM UTC (after 5 PM end)
        var businessMinutes = 60; // 1 hour

        // Act
        var result = await _calculator.AddBusinessMinutesAsync(startTime, businessMinutes);

        // Assert - Should start Tuesday 9 AM + 1 hour = Tuesday 10 AM
        result.Should().BeCloseTo(new DateTime(2025, 1, 7, 10, 0, 0, DateTimeKind.Utc), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task AddBusinessMinutesAsync_MultipleBusinessDays_CalculatesCorrectly()
    {
        // Arrange
        var startTime = new DateTime(2025, 1, 6, 9, 0, 0, DateTimeKind.Utc); // Monday 9 AM UTC
        var businessMinutes = 480 * 3; // 3 business days (8 hours each)

        // Act
        var result = await _calculator.AddBusinessMinutesAsync(startTime, businessMinutes);

        // Assert - Should be Wednesday 5 PM (1440 business minutes = Mon 9-5 + Tue 9-5 + Wed 9-5)
        result.Should().BeCloseTo(new DateTime(2025, 1, 8, 17, 0, 0, DateTimeKind.Utc), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task AddBusinessMinutesAsync_SpanningWeekend_SkipsWeekend()
    {
        // Arrange
        var startTime = new DateTime(2025, 1, 10, 16, 0, 0, DateTimeKind.Utc); // Friday 4 PM UTC
        var businessMinutes = 120; // 2 hours

        // Act
        var result = await _calculator.AddBusinessMinutesAsync(startTime, businessMinutes);

        // Assert - 1 hour Friday + skip weekend + 1 hour Monday = Monday 10 AM
        result.Should().BeCloseTo(new DateTime(2025, 1, 13, 10, 0, 0, DateTimeKind.Utc), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task AddBusinessMinutesAsync_LargeMinuteCount_HandlesCorrectly()
    {
        // Arrange
        var startTime = new DateTime(2025, 1, 6, 9, 0, 0, DateTimeKind.Utc); // Monday 9 AM UTC
        var businessMinutes = 480 * 5; // 1 full work week

        // Act
        var result = await _calculator.AddBusinessMinutesAsync(startTime, businessMinutes);

        // Assert - Should be Friday 5 PM (5 business days from Monday 9 AM)
        result.Should().BeCloseTo(new DateTime(2025, 1, 10, 17, 0, 0, DateTimeKind.Utc), TimeSpan.FromMinutes(1));
    }

    #endregion

    #region GetElapsedBusinessMinutesAsync Tests

    [Fact]
    public async Task GetElapsedBusinessMinutesAsync_EndBeforeStart_ReturnsZero()
    {
        // Arrange
        var startTime = new DateTime(2025, 1, 6, 12, 0, 0, DateTimeKind.Utc);
        var endTime = new DateTime(2025, 1, 6, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var result = await _calculator.GetElapsedBusinessMinutesAsync(startTime, endTime);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task GetElapsedBusinessMinutesAsync_SameTime_ReturnsZero()
    {
        // Arrange
        var time = new DateTime(2025, 1, 6, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var result = await _calculator.GetElapsedBusinessMinutesAsync(time, time);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task GetElapsedBusinessMinutesAsync_WithinSameDay_CalculatesCorrectly()
    {
        // Arrange
        var startTime = new DateTime(2025, 1, 6, 10, 0, 0, DateTimeKind.Utc); // Monday 10 AM
        var endTime = new DateTime(2025, 1, 6, 12, 0, 0, DateTimeKind.Utc); // Monday 12 PM

        // Act
        var result = await _calculator.GetElapsedBusinessMinutesAsync(startTime, endTime);

        // Assert
        result.Should().Be(120); // 2 hours = 120 minutes
    }

    [Fact]
    public async Task GetElapsedBusinessMinutesAsync_CrossingDay_CalculatesCorrectly()
    {
        // Arrange
        var startTime = new DateTime(2025, 1, 6, 16, 0, 0, DateTimeKind.Utc); // Monday 4 PM
        var endTime = new DateTime(2025, 1, 7, 10, 0, 0, DateTimeKind.Utc); // Tuesday 10 AM

        // Act
        var result = await _calculator.GetElapsedBusinessMinutesAsync(startTime, endTime);

        // Assert - 1 hour Monday (4-5 PM) + 1 hour Tuesday (9-10 AM) = 120 minutes
        result.Should().Be(120);
    }

    [Fact]
    public async Task GetElapsedBusinessMinutesAsync_SpanningWeekend_SkipsWeekend()
    {
        // Arrange
        var startTime = new DateTime(2025, 1, 10, 16, 0, 0, DateTimeKind.Utc); // Friday 4 PM
        var endTime = new DateTime(2025, 1, 13, 10, 0, 0, DateTimeKind.Utc); // Monday 10 AM

        // Act
        var result = await _calculator.GetElapsedBusinessMinutesAsync(startTime, endTime);

        // Assert - 1 hour Friday + skip weekend + 1 hour Monday = 120 minutes
        result.Should().Be(120);
    }

    [Fact]
    public async Task GetElapsedBusinessMinutesAsync_EntirelyOnWeekend_ReturnsZero()
    {
        // Arrange
        var startTime = new DateTime(2025, 1, 4, 10, 0, 0, DateTimeKind.Utc); // Saturday 10 AM
        var endTime = new DateTime(2025, 1, 5, 16, 0, 0, DateTimeKind.Utc); // Sunday 4 PM

        // Act
        var result = await _calculator.GetElapsedBusinessMinutesAsync(startTime, endTime);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task GetElapsedBusinessMinutesAsync_OutsideBusinessHours_OnlyCountsBusinessTime()
    {
        // Arrange
        var startTime = new DateTime(2025, 1, 6, 7, 0, 0, DateTimeKind.Utc); // Monday 7 AM (before hours)
        var endTime = new DateTime(2025, 1, 6, 19, 0, 0, DateTimeKind.Utc); // Monday 7 PM (after hours)

        // Act
        var result = await _calculator.GetElapsedBusinessMinutesAsync(startTime, endTime);

        // Assert - Only 9 AM to 5 PM counts = 480 minutes
        result.Should().Be(480);
    }

    [Fact]
    public async Task GetElapsedBusinessMinutesAsync_FullWorkWeek_ReturnsCorrectMinutes()
    {
        // Arrange
        var startTime = new DateTime(2025, 1, 6, 9, 0, 0, DateTimeKind.Utc); // Monday 9 AM
        var endTime = new DateTime(2025, 1, 10, 17, 0, 0, DateTimeKind.Utc); // Friday 5 PM

        // Act
        var result = await _calculator.GetElapsedBusinessMinutesAsync(startTime, endTime);

        // Assert - 5 days * 8 hours * 60 minutes = 2400 minutes
        result.Should().Be(2400);
    }

    #endregion

    #region IsBusinessTimeAsync Tests

    [Fact]
    public async Task IsBusinessTimeAsync_DuringBusinessHours_ReturnsTrue()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 6, 12, 0, 0, DateTimeKind.Utc); // Monday noon

        // Act
        var result = await _calculator.IsBusinessTimeAsync(dateTime);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsBusinessTimeAsync_BeforeBusinessHours_ReturnsFalse()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 6, 8, 0, 0, DateTimeKind.Utc); // Monday 8 AM (before 9 AM)

        // Act
        var result = await _calculator.IsBusinessTimeAsync(dateTime);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsBusinessTimeAsync_AfterBusinessHours_ReturnsFalse()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 6, 18, 0, 0, DateTimeKind.Utc); // Monday 6 PM (after 5 PM)

        // Act
        var result = await _calculator.IsBusinessTimeAsync(dateTime);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsBusinessTimeAsync_OnWeekend_ReturnsFalse()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 4, 12, 0, 0, DateTimeKind.Utc); // Saturday noon

        // Act
        var result = await _calculator.IsBusinessTimeAsync(dateTime);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsBusinessTimeAsync_AtExactStartTime_ReturnsTrue()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 6, 9, 0, 0, DateTimeKind.Utc); // Monday 9 AM exactly

        // Act
        var result = await _calculator.IsBusinessTimeAsync(dateTime);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsBusinessTimeAsync_AtExactEndTime_ReturnsFalse()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 6, 17, 0, 0, DateTimeKind.Utc); // Monday 5 PM exactly (end exclusive)

        // Act
        var result = await _calculator.IsBusinessTimeAsync(dateTime);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsBusinessTimeAsync_OneMinuteBeforeEnd_ReturnsTrue()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 6, 16, 59, 0, DateTimeKind.Utc); // Monday 4:59 PM

        // Act
        var result = await _calculator.IsBusinessTimeAsync(dateTime);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region GetNextBusinessStartAsync Tests

    [Fact]
    public async Task GetNextBusinessStartAsync_DuringBusinessHours_ReturnsNextDayStart()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 6, 12, 0, 0, DateTimeKind.Utc); // Monday noon

        // Act
        var result = await _calculator.GetNextBusinessStartAsync(dateTime);

        // Assert - Next business start is Tuesday 9 AM
        result.Should().BeCloseTo(new DateTime(2025, 1, 7, 9, 0, 0, DateTimeKind.Utc), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetNextBusinessStartAsync_BeforeBusinessHours_ReturnsNextDayStart()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 6, 7, 0, 0, DateTimeKind.Utc); // Monday 7 AM

        // Act
        var result = await _calculator.GetNextBusinessStartAsync(dateTime);

        // Assert - Service always returns the next working day start (Tue 9 AM)
        result.Should().BeCloseTo(new DateTime(2025, 1, 7, 9, 0, 0, DateTimeKind.Utc), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetNextBusinessStartAsync_AfterBusinessHours_ReturnsNextDayStart()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 6, 18, 0, 0, DateTimeKind.Utc); // Monday 6 PM

        // Act
        var result = await _calculator.GetNextBusinessStartAsync(dateTime);

        // Assert - Next business start is Tuesday 9 AM
        result.Should().BeCloseTo(new DateTime(2025, 1, 7, 9, 0, 0, DateTimeKind.Utc), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetNextBusinessStartAsync_OnFridayEvening_ReturnsMondayStart()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 10, 18, 0, 0, DateTimeKind.Utc); // Friday 6 PM

        // Act
        var result = await _calculator.GetNextBusinessStartAsync(dateTime);

        // Assert - Next business start is Monday 9 AM
        result.Should().BeCloseTo(new DateTime(2025, 1, 13, 9, 0, 0, DateTimeKind.Utc), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetNextBusinessStartAsync_OnSaturday_ReturnsMondayStart()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 4, 12, 0, 0, DateTimeKind.Utc); // Saturday noon

        // Act
        var result = await _calculator.GetNextBusinessStartAsync(dateTime);

        // Assert - Next business start is Monday 9 AM
        result.Should().BeCloseTo(new DateTime(2025, 1, 6, 9, 0, 0, DateTimeKind.Utc), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetNextBusinessStartAsync_OnSunday_ReturnsMondayStart()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 5, 12, 0, 0, DateTimeKind.Utc); // Sunday noon

        // Act
        var result = await _calculator.GetNextBusinessStartAsync(dateTime);

        // Assert - Next business start is Monday 9 AM
        result.Should().BeCloseTo(new DateTime(2025, 1, 6, 9, 0, 0, DateTimeKind.Utc), TimeSpan.FromMinutes(1));
    }

    #endregion

    #region IsHolidayAsync Tests

    [Fact]
    public async Task IsHolidayAsync_RegularWorkday_ReturnsFalse()
    {
        // Arrange
        var date = new DateTime(2025, 1, 6); // Monday

        // Act
        var result = await _calculator.IsHolidayAsync(date);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsHolidayAsync_Weekend_StillReturnsFalse()
    {
        // Arrange - Weekend days are not "holidays" in the traditional sense
        var date = new DateTime(2025, 1, 4); // Saturday

        // Act
        var result = await _calculator.IsHolidayAsync(date);

        // Assert - Weekend is handled separately, not as a holiday
        result.Should().BeFalse();
    }

    #endregion

    #region Edge Cases and Integration Tests

    [Theory]
    [InlineData(1)]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(45)]
    [InlineData(60)]
    public async Task AddBusinessMinutesAsync_VariousMinuteValues_CalculatesCorrectly(int minutes)
    {
        // Arrange
        var startTime = new DateTime(2025, 1, 6, 10, 0, 0, DateTimeKind.Utc); // Monday 10 AM

        // Act
        var result = await _calculator.AddBusinessMinutesAsync(startTime, minutes);

        // Assert - result should be startTime + minutes of business time
        var expected = new DateTime(2025, 1, 6, 10, 0, 0, DateTimeKind.Utc).AddMinutes(minutes);
        result.Should().BeCloseTo(expected, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task RoundTrip_AddAndElapsed_ShouldMatch()
    {
        // Arrange
        var startTime = new DateTime(2025, 1, 6, 10, 0, 0, DateTimeKind.Utc); // Monday 10 AM
        var minutesToAdd = 180; // 3 hours

        // Act
        var endTime = await _calculator.AddBusinessMinutesAsync(startTime, minutesToAdd);
        var elapsedMinutes = await _calculator.GetElapsedBusinessMinutesAsync(startTime, endTime);

        // Assert
        elapsedMinutes.Should().Be(minutesToAdd);
    }

    [Fact]
    public async Task RoundTrip_CrossingDays_ShouldMatch()
    {
        // Arrange
        var startTime = new DateTime(2025, 1, 6, 14, 0, 0, DateTimeKind.Utc); // Monday 2 PM
        var minutesToAdd = 480; // 8 hours (full day)

        // Act
        var endTime = await _calculator.AddBusinessMinutesAsync(startTime, minutesToAdd);
        var elapsedMinutes = await _calculator.GetElapsedBusinessMinutesAsync(startTime, endTime);

        // Assert
        elapsedMinutes.Should().Be(minutesToAdd);
    }

    [Fact]
    public async Task RoundTrip_SpanningWeekend_ShouldMatch()
    {
        // Arrange
        var startTime = new DateTime(2025, 1, 10, 14, 0, 0, DateTimeKind.Utc); // Friday 2 PM
        var minutesToAdd = 480; // 8 hours

        // Act
        var endTime = await _calculator.AddBusinessMinutesAsync(startTime, minutesToAdd);
        var elapsedMinutes = await _calculator.GetElapsedBusinessMinutesAsync(startTime, endTime);

        // Assert
        elapsedMinutes.Should().Be(minutesToAdd);
    }

    #endregion

    #region SLA Time Scenarios

    [Fact]
    public async Task SLAScenario_4HourResponse_FromMondayMorning()
    {
        // Arrange
        var ticketCreated = new DateTime(2025, 1, 6, 9, 30, 0, DateTimeKind.Utc); // Monday 9:30 AM
        var slaMinutes = 240; // 4 hour SLA

        // Act
        var dueTime = await _calculator.AddBusinessMinutesAsync(ticketCreated, slaMinutes);

        // Assert - Should be Monday 1:30 PM
        dueTime.Should().BeCloseTo(new DateTime(2025, 1, 6, 13, 30, 0, DateTimeKind.Utc), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task SLAScenario_4HourResponse_FromMondayAfternoon()
    {
        // Arrange
        var ticketCreated = new DateTime(2025, 1, 6, 15, 0, 0, DateTimeKind.Utc); // Monday 3 PM
        var slaMinutes = 240; // 4 hour SLA

        // Act
        var dueTime = await _calculator.AddBusinessMinutesAsync(ticketCreated, slaMinutes);

        // Assert - 2 hours Monday (3-5 PM) + 2 hours Tuesday (9-11 AM) = Tuesday 11 AM
        dueTime.Should().BeCloseTo(new DateTime(2025, 1, 7, 11, 0, 0, DateTimeKind.Utc), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task SLAScenario_8HourResolution_FromFridayAfternoon()
    {
        // Arrange
        var ticketCreated = new DateTime(2025, 1, 10, 14, 0, 0, DateTimeKind.Utc); // Friday 2 PM
        var slaMinutes = 480; // 8 hour SLA

        // Act
        var dueTime = await _calculator.AddBusinessMinutesAsync(ticketCreated, slaMinutes);

        // Assert - 3 hours Friday (2-5 PM) + 5 hours Monday (9 AM - 2 PM) = Monday 2 PM
        dueTime.Should().BeCloseTo(new DateTime(2025, 1, 13, 14, 0, 0, DateTimeKind.Utc), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task SLAScenario_CheckSLABreach_WithinTime()
    {
        // Arrange
        var ticketCreated = new DateTime(2025, 1, 6, 10, 0, 0, DateTimeKind.Utc); // Monday 10 AM
        var slaMinutes = 120; // 2 hour SLA
        var currentTime = new DateTime(2025, 1, 6, 11, 30, 0, DateTimeKind.Utc); // Monday 11:30 AM

        // Act
        var elapsedMinutes = await _calculator.GetElapsedBusinessMinutesAsync(ticketCreated, currentTime);

        // Assert - 90 minutes elapsed, still within 120 minute SLA
        elapsedMinutes.Should().Be(90);
        elapsedMinutes.Should().BeLessThan(slaMinutes);
    }

    [Fact]
    public async Task SLAScenario_CheckSLABreach_Exceeded()
    {
        // Arrange
        var ticketCreated = new DateTime(2025, 1, 6, 10, 0, 0, DateTimeKind.Utc); // Monday 10 AM
        var slaMinutes = 120; // 2 hour SLA
        var currentTime = new DateTime(2025, 1, 6, 14, 0, 0, DateTimeKind.Utc); // Monday 2 PM

        // Act
        var elapsedMinutes = await _calculator.GetElapsedBusinessMinutesAsync(ticketCreated, currentTime);

        // Assert - 240 minutes elapsed, exceeds 120 minute SLA
        elapsedMinutes.Should().Be(240);
        elapsedMinutes.Should().BeGreaterThan(slaMinutes);
    }

    [Fact]
    public async Task SLAScenario_CalculateRemainingTime()
    {
        // Arrange
        var ticketCreated = new DateTime(2025, 1, 6, 10, 0, 0, DateTimeKind.Utc); // Monday 10 AM
        var slaMinutes = 240; // 4 hour SLA
        var currentTime = new DateTime(2025, 1, 6, 11, 30, 0, DateTimeKind.Utc); // Monday 11:30 AM

        // Act
        var elapsedMinutes = await _calculator.GetElapsedBusinessMinutesAsync(ticketCreated, currentTime);
        var remainingMinutes = slaMinutes - elapsedMinutes;

        // Assert - 90 minutes elapsed, 150 minutes remaining
        elapsedMinutes.Should().Be(90);
        remainingMinutes.Should().Be(150);
    }

    #endregion

    #region DST Handling Tests (TODO-SD003-008)

    /// <summary>
    /// Tests for <see cref="BusinessHoursCalculator.SafeConvertLocalToUtc"/>,
    /// which handles DST ambiguous and invalid times (TODO-SD003-008).
    /// Eastern Time (UTC-5 EST / UTC-4 EDT) is used because its DST transitions
    /// are predictable and well-documented:
    ///   - Spring forward: 2nd Sunday of March  2:00 AM ET → 3:00 AM ET (gap: 2:00-2:59 AM doesn't exist)
    ///   - Fall back:      1st Sunday of November 2:00 AM ET → 1:00 AM ET (ambiguous: 1:00-1:59 AM occurs twice)
    /// 2025 transitions:
    ///   - Spring: March 9 at 2:00 AM ET
    ///   - Fall:   November 2 at 2:00 AM ET
    /// </summary>

    // ── UTC timezone: no DST, simple path ──────────────────────────────────────

    [Fact]
    public void SafeConvertLocalToUtc_UtcTimezone_ReturnsSameInstant()
    {
        // Arrange
        var localTime = new DateTime(2025, 6, 15, 14, 30, 0);
        var tz = TimeZoneInfo.Utc;

        // Act
        var result = BusinessHoursCalculator.SafeConvertLocalToUtc(localTime, tz);

        // Assert
        result.Should().Be(new DateTime(2025, 6, 15, 14, 30, 0, DateTimeKind.Utc));
        result.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void SafeConvertLocalToUtc_NormalEasternSummerTime_ConvertsCorrectly()
    {
        // Arrange — July 4, 2025 1:00 PM EDT (UTC-4), no DST edge case
        var easternTz = TryGetEasternTimezone();
        var localTime = new DateTime(2025, 7, 4, 13, 0, 0);

        // Act
        var result = BusinessHoursCalculator.SafeConvertLocalToUtc(localTime, easternTz);

        // Assert: EDT = UTC-4, 1 PM EDT → 5 PM UTC
        result.Should().Be(new DateTime(2025, 7, 4, 17, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void SafeConvertLocalToUtc_NormalEasternWinterTime_ConvertsCorrectly()
    {
        // Arrange — January 15, 2025 9:00 AM EST (UTC-5)
        var easternTz = TryGetEasternTimezone();
        var localTime = new DateTime(2025, 1, 15, 9, 0, 0);

        // Act
        var result = BusinessHoursCalculator.SafeConvertLocalToUtc(localTime, easternTz);

        // Assert: EST = UTC-5, 9 AM EST → 2 PM UTC
        result.Should().Be(new DateTime(2025, 1, 15, 14, 0, 0, DateTimeKind.Utc));
    }

    // ── Spring-forward: invalid local time (gap) ────────────────────────────────

    [Fact]
    public void SafeConvertLocalToUtc_InvalidTime_SpringForward_AdvancesToNextValidTime()
    {
        // Arrange — March 9, 2025: 2:30 AM ET does NOT exist (spring-forward gap).
        var easternTz = TryGetEasternTimezone();
        var invalidLocalTime = new DateTime(2025, 3, 9, 2, 30, 0);

        easternTz.IsInvalidTime(invalidLocalTime).Should().BeTrue(
            "2:30 AM Eastern on spring-forward day should be in the DST gap");

        // Act
        var result = BusinessHoursCalculator.SafeConvertLocalToUtc(invalidLocalTime, easternTz);

        // Assert: should resolve to a time at or after 3:00 AM EDT = 7:00 AM UTC
        result.Kind.Should().Be(DateTimeKind.Utc);
        result.Should().BeOnOrAfter(new DateTime(2025, 3, 9, 7, 0, 0, DateTimeKind.Utc),
            "time in the spring-forward gap should resolve to 3:00 AM EDT or later");
    }

    [Fact]
    public void SafeConvertLocalToUtc_InvalidTime_ExactGapStart_AdvancesToNextValidTime()
    {
        // Arrange — exactly 2:00 AM ET (first invalid minute of the gap)
        var easternTz = TryGetEasternTimezone();
        var gapStart = new DateTime(2025, 3, 9, 2, 0, 0);

        easternTz.IsInvalidTime(gapStart).Should().BeTrue("2:00 AM ET is the start of the spring-forward gap");

        // Act
        var result = BusinessHoursCalculator.SafeConvertLocalToUtc(gapStart, easternTz);

        // Assert
        result.Kind.Should().Be(DateTimeKind.Utc);
        result.Should().BeOnOrAfter(new DateTime(2025, 3, 9, 7, 0, 0, DateTimeKind.Utc));
    }

    // ── Fall-back: ambiguous local time ─────────────────────────────────────────

    [Fact]
    public void SafeConvertLocalToUtc_AmbiguousTime_FallBack_UsesStandardTimeOffset()
    {
        // Arrange — November 2, 2025: 1:30 AM ET is ambiguous (occurs twice).
        var easternTz = TryGetEasternTimezone();
        var ambiguousLocalTime = new DateTime(2025, 11, 2, 1, 30, 0);

        easternTz.IsAmbiguousTime(ambiguousLocalTime).Should().BeTrue(
            "1:30 AM Eastern on fall-back day should be ambiguous");

        // Act
        var result = BusinessHoursCalculator.SafeConvertLocalToUtc(ambiguousLocalTime, easternTz);

        // Assert: resolves to standard time (EST = UTC-5), so 1:30 AM EST → 6:30 AM UTC
        result.Kind.Should().Be(DateTimeKind.Utc);
        result.Should().Be(new DateTime(2025, 11, 2, 6, 30, 0, DateTimeKind.Utc),
            "ambiguous 1:30 AM should resolve to standard-time offset = 6:30 AM UTC");
    }

    [Fact]
    public void SafeConvertLocalToUtc_AmbiguousTime_ReturnsDeterministicResult()
    {
        // Repeated calls with the same ambiguous time should return the same UTC instant.
        var easternTz = TryGetEasternTimezone();
        var ambiguousLocalTime = new DateTime(2025, 11, 2, 1, 0, 0);

        easternTz.IsAmbiguousTime(ambiguousLocalTime).Should().BeTrue();

        var result1 = BusinessHoursCalculator.SafeConvertLocalToUtc(ambiguousLocalTime, easternTz);
        var result2 = BusinessHoursCalculator.SafeConvertLocalToUtc(ambiguousLocalTime, easternTz);

        result1.Should().Be(result2, "conversion of an ambiguous time should be deterministic");
    }

    // ── Integration: AddBusinessMinutesAsync does not throw around DST ──────────

    [Fact]
    public async Task AddBusinessMinutesAsync_AroundSpringForward_DoesNotThrow()
    {
        // Default UTC schedule — no DST; exercises SafeConvertLocalToUtc code path.
        var startTimeUtc = new DateTime(2025, 3, 9, 1, 0, 0, DateTimeKind.Utc);
        Func<Task> act = async () => await _calculator.AddBusinessMinutesAsync(startTimeUtc, 60);
        await act.Should().NotThrowAsync();
    }

    // ── Helper ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns Eastern <see cref="TimeZoneInfo"/> using the IANA id (Linux/macOS)
    /// then the Windows id as a fallback.
    /// </summary>
    private static TimeZoneInfo TryGetEasternTimezone()
    {
        foreach (var id in new[] { "America/New_York", "Eastern Standard Time" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { /* try next */ }
        }

        throw new InvalidOperationException(
            "Eastern Time zone not found. Install tzdata (Linux) or run on macOS/Windows.");
    }

    #endregion
}

/// <summary>
/// Tests for BusinessSchedule and related DTOs.
/// </summary>
public class BusinessScheduleTests
{
    [Fact]
    public void BusinessSchedule_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var schedule = new BusinessSchedule();

        // Assert
        schedule.ScheduleId.Should().Be(0);
        schedule.Name.Should().Be("Default");
        schedule.TimeZoneId.Should().Be("UTC");
        schedule.Days.Should().NotBeNull();
        schedule.Days.Should().BeEmpty();
        schedule.Holidays.Should().NotBeNull();
        schedule.Holidays.Should().BeEmpty();
        schedule.IsActive.Should().BeTrue();
    }

    [Fact]
    public void BusinessDay_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var day = new BusinessDay();

        // Assert
        day.DayOfWeek.Should().Be(DayOfWeek.Sunday);
        day.StartTime.Should().Be(TimeSpan.Zero);
        day.EndTime.Should().Be(TimeSpan.Zero);
        day.IsWorkingDay.Should().BeTrue();
    }

    [Fact]
    public void Holiday_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var holiday = new Holiday();

        // Assert
        holiday.Date.Should().Be(default);
        holiday.Name.Should().BeEmpty();
        holiday.IsRecurringYearly.Should().BeFalse();
    }

    [Fact]
    public void BusinessSchedule_CanBeConfigured()
    {
        // Arrange & Act
        var schedule = new BusinessSchedule
        {
            ScheduleId = 1,
            Name = "Extended Hours",
            TimeZoneId = "America/New_York",
            IsActive = true,
            Days = new List<BusinessDay>
            {
                new() { DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(18), IsWorkingDay = true },
                new() { DayOfWeek = DayOfWeek.Saturday, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(13), IsWorkingDay = true }
            },
            Holidays = new List<Holiday>
            {
                new() { Date = new DateTime(2025, 12, 25), Name = "Christmas", IsRecurringYearly = true },
                new() { Date = new DateTime(2025, 1, 1), Name = "New Year", IsRecurringYearly = true }
            }
        };

        // Assert
        schedule.ScheduleId.Should().Be(1);
        schedule.Name.Should().Be("Extended Hours");
        schedule.TimeZoneId.Should().Be("America/New_York");
        schedule.Days.Should().HaveCount(2);
        schedule.Days[0].StartTime.Should().Be(TimeSpan.FromHours(8));
        schedule.Holidays.Should().HaveCount(2);
        schedule.Holidays[0].IsRecurringYearly.Should().BeTrue();
    }

}
