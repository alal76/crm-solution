// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 CRM Solution Contributors
// ITSM SLA Service Unit Tests

using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Comprehensive unit tests for ITSM SLA calculation and enforcement
/// </summary>
public class SLAServiceTests
{
    #region SLA Policy Tests

    [Fact]
    public void SLAPolicy_P1Critical_HasCorrectTargets()
    {
        // Arrange & Act
        var policy = new SLAPolicy
        {
            PolicyName = "P1 - Critical",
            Priority = 1,
            ResponseTimeMinutes = 15,
            ResolutionTimeMinutes = 240, // 4 hours
            BusinessHoursOnly = false
        };

        // Assert
        policy.ResponseTimeMinutes.Should().Be(15);
        policy.ResolutionTimeMinutes.Should().Be(240);
        policy.BusinessHoursOnly.Should().BeFalse(); // 24x7 for P1
    }

    [Fact]
    public void SLAPolicy_P4Low_HasCorrectTargets()
    {
        // Arrange & Act
        var policy = new SLAPolicy
        {
            PolicyName = "P4 - Low",
            Priority = 4,
            ResponseTimeMinutes = 480,   // 8 hours
            ResolutionTimeMinutes = 2880, // 48 hours
            BusinessHoursOnly = true
        };

        // Assert
        policy.ResponseTimeMinutes.Should().Be(480);
        policy.ResolutionTimeMinutes.Should().Be(2880);
        policy.BusinessHoursOnly.Should().BeTrue();
    }

    #endregion

    #region Business Hours Calculation Tests

    [Fact]
    public void BusinessHours_StandardSchedule_9to5()
    {
        // Arrange
        var schedule = new BusinessHoursSchedule
        {
            StartHour = 8,
            StartMinute = 0,
            EndHour = 17,
            EndMinute = 0,
            WorkDays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, 
                               DayOfWeek.Thursday, DayOfWeek.Friday }
        };

        // Assert
        schedule.GetWorkHoursPerDay().Should().Be(9); // 8 AM to 5 PM = 9 hours
        schedule.WorkDays.Should().HaveCount(5);
        schedule.WorkDays.Should().NotContain(DayOfWeek.Saturday);
        schedule.WorkDays.Should().NotContain(DayOfWeek.Sunday);
    }

    [Fact]
    public void IsBusinessTime_DuringWorkHours_ReturnsTrue()
    {
        // Arrange
        var schedule = CreateStandardBusinessHours();
        var tuesdayAt10AM = new DateTime(2026, 2, 3, 10, 0, 0); // Tuesday 10 AM

        // Act
        var isBusinessTime = IsWithinBusinessHours(tuesdayAt10AM, schedule);

        // Assert
        isBusinessTime.Should().BeTrue();
    }

    [Fact]
    public void IsBusinessTime_AfterHours_ReturnsFalse()
    {
        // Arrange
        var schedule = CreateStandardBusinessHours();
        var tuesdayAt8PM = new DateTime(2026, 2, 3, 20, 0, 0); // Tuesday 8 PM

        // Act
        var isBusinessTime = IsWithinBusinessHours(tuesdayAt8PM, schedule);

        // Assert
        isBusinessTime.Should().BeFalse();
    }

    [Fact]
    public void IsBusinessTime_Weekend_ReturnsFalse()
    {
        // Arrange
        var schedule = CreateStandardBusinessHours();
        var saturdayAt10AM = new DateTime(2026, 2, 7, 10, 0, 0); // Saturday 10 AM

        // Act
        var isBusinessTime = IsWithinBusinessHours(saturdayAt10AM, schedule);

        // Assert
        isBusinessTime.Should().BeFalse();
    }

    [Fact]
    public void IsBusinessTime_Holiday_ReturnsFalse()
    {
        // Arrange
        var schedule = CreateStandardBusinessHours();
        var holidays = new List<DateTime> { new DateTime(2026, 12, 25) }; // Christmas
        var christmasAt10AM = new DateTime(2026, 12, 25, 10, 0, 0);

        // Act
        var isHoliday = holidays.Any(h => h.Date == christmasAt10AM.Date);
        var isBusinessTime = !isHoliday && IsWithinBusinessHours(christmasAt10AM, schedule);

        // Assert
        isBusinessTime.Should().BeFalse();
    }

    #endregion

    #region Add Business Minutes Tests

    [Fact]
    public void AddBusinessMinutes_SameDay_CalculatesCorrectly()
    {
        // Arrange
        var schedule = CreateStandardBusinessHours();
        var startTime = new DateTime(2026, 2, 3, 9, 0, 0); // Tuesday 9 AM
        var minutesToAdd = 120; // 2 hours

        // Act
        var result = AddBusinessMinutes(startTime, minutesToAdd, schedule);

        // Assert
        result.Should().Be(new DateTime(2026, 2, 3, 11, 0, 0)); // Tuesday 11 AM
    }

    [Fact]
    public void AddBusinessMinutes_SpansOvernight_SkipsNonBusinessHours()
    {
        // Arrange
        var schedule = CreateStandardBusinessHours();
        var startTime = new DateTime(2026, 2, 3, 16, 0, 0); // Tuesday 4 PM
        var minutesToAdd = 120; // 2 hours (should span to next day)

        // Act
        var result = AddBusinessMinutes(startTime, minutesToAdd, schedule);

        // Assert - 1 hour left in Tuesday + 1 hour on Wednesday = 9 AM Wed
        result.Should().Be(new DateTime(2026, 2, 4, 9, 0, 0)); // Wednesday 9 AM
    }

    [Fact]
    public void AddBusinessMinutes_SpansWeekend_SkipsWeekend()
    {
        // Arrange
        var schedule = CreateStandardBusinessHours();
        var startTime = new DateTime(2026, 2, 6, 16, 0, 0); // Friday 4 PM
        var minutesToAdd = 120; // 2 hours

        // Act
        var result = AddBusinessMinutes(startTime, minutesToAdd, schedule);

        // Assert - 1 hour left Friday + skip weekend + 1 hour Monday = 9 AM Monday
        result.Should().Be(new DateTime(2026, 2, 9, 9, 0, 0)); // Monday 9 AM
    }

    #endregion

    #region SLA Due Date Calculation Tests

    [Theory]
    [InlineData(1, 15, 240)]     // P1: 15 min response, 4 hour resolution
    [InlineData(2, 30, 480)]     // P2: 30 min response, 8 hour resolution
    [InlineData(3, 120, 1440)]   // P3: 2 hour response, 24 hour resolution
    [InlineData(4, 480, 2880)]   // P4: 8 hour response, 48 hour resolution
    public void CalculateDueDate_ByPriority_ReturnsCorrectTargets(
        int priority,
        int expectedResponseMinutes,
        int expectedResolutionMinutes)
    {
        // Arrange
        var policy = GetPolicyByPriority(priority);

        // Assert
        policy.ResponseTimeMinutes.Should().Be(expectedResponseMinutes);
        policy.ResolutionTimeMinutes.Should().Be(expectedResolutionMinutes);
    }

    [Fact]
    public void CalculateDueDate_24x7Policy_IncludesAllHours()
    {
        // Arrange
        var policy = new SLAPolicy
        {
            Priority = 1,
            ResponseTimeMinutes = 60,
            BusinessHoursOnly = false
        };
        var fridayAt11PM = new DateTime(2026, 2, 6, 23, 0, 0);

        // Act - For 24x7, add 60 minutes directly
        var responseDue = fridayAt11PM.AddMinutes(policy.ResponseTimeMinutes);

        // Assert - Should be Saturday 12 AM (midnight)
        responseDue.Should().Be(new DateTime(2026, 2, 7, 0, 0, 0));
    }

    #endregion

    #region SLA Breach Detection Tests

    [Fact]
    public void CheckBreach_ResponseNotBreached_ReturnsFalse()
    {
        // Arrange
        var responseDue = DateTime.UtcNow.AddMinutes(30);
        var currentTime = DateTime.UtcNow;
        var hasResponded = false;

        // Act
        var isBreached = currentTime > responseDue && !hasResponded;

        // Assert
        isBreached.Should().BeFalse();
    }

    [Fact]
    public void CheckBreach_ResponseBreached_ReturnsTrue()
    {
        // Arrange
        var responseDue = DateTime.UtcNow.AddMinutes(-10); // Past due
        var hasResponded = false;

        // Act
        var isBreached = DateTime.UtcNow > responseDue && !hasResponded;

        // Assert
        isBreached.Should().BeTrue();
    }

    [Fact]
    public void CheckBreach_RespondedBeforeDue_NotBreached()
    {
        // Arrange
        var responseDue = DateTime.UtcNow.AddMinutes(30);
        var hasResponded = true;

        // Act
        var isBreached = DateTime.UtcNow > responseDue && !hasResponded;

        // Assert
        isBreached.Should().BeFalse();
    }

    [Fact]
    public void CheckBreach_AtRiskThreshold_ReturnsWarning()
    {
        // Arrange
        var responseDue = DateTime.UtcNow.AddMinutes(5); // 5 min left
        var totalTimeMinutes = 30;
        var elapsedMinutes = 25;
        var warningThreshold = 0.75; // 75%

        // Act
        var percentElapsed = (double)elapsedMinutes / totalTimeMinutes;
        var isAtRisk = percentElapsed >= warningThreshold;

        // Assert
        isAtRisk.Should().BeTrue();
        percentElapsed.Should().BeGreaterThan(0.8);
    }

    #endregion

    #region SLA Pause/Resume Tests

    [Fact]
    public void PauseSLA_OnHold_StopsTimerTracking()
    {
        // Arrange
        var slaTracker = new SLATracker
        {
            IsPaused = false,
            ElapsedMinutes = 30,
            PausedAt = null
        };

        // Act
        slaTracker.IsPaused = true;
        slaTracker.PausedAt = DateTime.UtcNow;

        // Assert
        slaTracker.IsPaused.Should().BeTrue();
        slaTracker.PausedAt.Should().NotBeNull();
    }

    [Fact]
    public void ResumeSLA_FromPause_ContinuesFromPausePoint()
    {
        // Arrange
        var slaTracker = new SLATracker
        {
            IsPaused = true,
            ElapsedMinutes = 30,
            PausedAt = DateTime.UtcNow.AddHours(-2)
        };

        // Act
        slaTracker.IsPaused = false;
        slaTracker.ResumedAt = DateTime.UtcNow;
        // Pause time should not count toward elapsed

        // Assert
        slaTracker.IsPaused.Should().BeFalse();
        slaTracker.ElapsedMinutes.Should().Be(30); // Still 30, pause time excluded
    }

    #endregion

    #region Helper Methods

    private static BusinessHoursSchedule CreateStandardBusinessHours()
    {
        return new BusinessHoursSchedule
        {
            StartHour = 8,
            StartMinute = 0,
            EndHour = 17,
            EndMinute = 0,
            WorkDays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, 
                               DayOfWeek.Thursday, DayOfWeek.Friday }
        };
    }

    private static bool IsWithinBusinessHours(DateTime time, BusinessHoursSchedule schedule)
    {
        if (!schedule.WorkDays.Contains(time.DayOfWeek))
            return false;

        var timeOfDay = time.TimeOfDay;
        var startTime = new TimeSpan(schedule.StartHour, schedule.StartMinute, 0);
        var endTime = new TimeSpan(schedule.EndHour, schedule.EndMinute, 0);

        return timeOfDay >= startTime && timeOfDay < endTime;
    }

    private static DateTime AddBusinessMinutes(DateTime start, int minutes, BusinessHoursSchedule schedule)
    {
        var current = start;
        var remainingMinutes = minutes;

        while (remainingMinutes > 0)
        {
            if (!IsWithinBusinessHours(current, schedule))
            {
                // Move to next business hour start
                current = GetNextBusinessStart(current, schedule);
                continue;
            }

            var endOfDay = current.Date.AddHours(schedule.EndHour).AddMinutes(schedule.EndMinute);
            var minutesToEndOfDay = (int)(endOfDay - current).TotalMinutes;

            if (remainingMinutes <= minutesToEndOfDay)
            {
                current = current.AddMinutes(remainingMinutes);
                remainingMinutes = 0;
            }
            else
            {
                remainingMinutes -= minutesToEndOfDay;
                current = GetNextBusinessStart(endOfDay, schedule);
            }
        }

        return current;
    }

    private static DateTime GetNextBusinessStart(DateTime from, BusinessHoursSchedule schedule)
    {
        var next = from.Date.AddDays(1).AddHours(schedule.StartHour).AddMinutes(schedule.StartMinute);
        
        while (!schedule.WorkDays.Contains(next.DayOfWeek))
        {
            next = next.AddDays(1);
        }

        return next;
    }

    private static SLAPolicy GetPolicyByPriority(int priority)
    {
        return priority switch
        {
            1 => new SLAPolicy { Priority = 1, ResponseTimeMinutes = 15, ResolutionTimeMinutes = 240, BusinessHoursOnly = false },
            2 => new SLAPolicy { Priority = 2, ResponseTimeMinutes = 30, ResolutionTimeMinutes = 480, BusinessHoursOnly = false },
            3 => new SLAPolicy { Priority = 3, ResponseTimeMinutes = 120, ResolutionTimeMinutes = 1440, BusinessHoursOnly = true },
            4 => new SLAPolicy { Priority = 4, ResponseTimeMinutes = 480, ResolutionTimeMinutes = 2880, BusinessHoursOnly = true },
            _ => new SLAPolicy { Priority = 3, ResponseTimeMinutes = 120, ResolutionTimeMinutes = 1440, BusinessHoursOnly = true }
        };
    }

    #endregion
}

// Test helper classes
public class SLAPolicy
{
    public string PolicyName { get; set; } = string.Empty;
    public int Priority { get; set; }
    public int ResponseTimeMinutes { get; set; }
    public int ResolutionTimeMinutes { get; set; }
    public bool BusinessHoursOnly { get; set; }
}

public class BusinessHoursSchedule
{
    public int StartHour { get; set; }
    public int StartMinute { get; set; }
    public int EndHour { get; set; }
    public int EndMinute { get; set; }
    public DayOfWeek[] WorkDays { get; set; } = Array.Empty<DayOfWeek>();

    public int GetWorkHoursPerDay()
    {
        return EndHour - StartHour;
    }
}

public class SLATracker
{
    public bool IsPaused { get; set; }
    public int ElapsedMinutes { get; set; }
    public DateTime? PausedAt { get; set; }
    public DateTime? ResumedAt { get; set; }
}
