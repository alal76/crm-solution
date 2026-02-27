// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// SLA time calculator that handles daylight saving time transitions.
/// Calculates business hours remaining/elapsed while accounting for DST shifts.
/// TODO-SD003-008: Adjusts SLA time calculations for daylight saving transitions.
/// </summary>
public interface IDstAwareSlaCalculator
{
    /// <summary>
    /// Calculates the target time when SLA will be breached, accounting for business hours and DST.
    /// </summary>
    /// <param name="startTime">When the clock starts (e.g., ticket creation time)</param>
    /// <param name="slaMinutes">Total minutes allowed (in business hours)</param>
    /// <param name="timeZoneId">The IANA time zone ID (e.g., "America/New_York")</param>
    /// <param name="businessHoursOnly">Whether to count only business hours (Mon-Fri 9-5)</param>
    /// <returns>The UTC DateTime when SLA will breach</returns>
    DateTime CalculateTargetTime(DateTime startTime, int slaMinutes, string timeZoneId, bool businessHoursOnly);

    /// <summary>
    /// Calculates the elapsed business minutes between two times, accounting for DST.
    /// </summary>
    /// <param name="startTime">Start time (UTC)</param>
    /// <param name="endTime">End time (UTC)</param>
    /// <param name="timeZoneId">The IANA time zone ID</param>
    /// <param name="businessHoursOnly">Whether to count only business hours</param>
    /// <returns>Number of business minutes elapsed</returns>
    int CalculateElapsedMinutes(DateTime startTime, DateTime endTime, string timeZoneId, bool businessHoursOnly);

    /// <summary>
    /// Checks if a given time falls within a DST transition period.
    /// </summary>
    /// <param name="time">Time to check (UTC)</param>
    /// <param name="timeZoneId">The IANA time zone ID</param>
    /// <returns>True if time is within DST transition window</returns>
    bool IsInDstTransition(DateTime time, string timeZoneId);
}

/// <summary>
/// Implementation of DST-aware SLA calculator using TimeZoneInfo.
/// </summary>
public class DstAwareSlaCalculator : IDstAwareSlaCalculator
{
    private readonly ILogger<DstAwareSlaCalculator> _logger;

    // Default business hours (can be enhanced to use database-configured schedules)
    private static readonly TimeSpan BusinessStartTime = new(9, 0, 0); // 9:00 AM
    private static readonly TimeSpan BusinessEndTime = new(17, 0, 0);  // 5:00 PM
    private static readonly DayOfWeek[] BusinessDays = { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };

    public DstAwareSlaCalculator(ILogger<DstAwareSlaCalculator> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public DateTime CalculateTargetTime(DateTime startTime, int slaMinutes, string timeZoneId, bool businessHoursOnly)
    {
        if (slaMinutes <= 0)
            return startTime;

        var timeZone = GetTimeZone(timeZoneId);
        if (timeZone == null)
        {
            _logger.LogWarning("Time zone {TimeZoneId} not found, using UTC", timeZoneId);
            return startTime.AddMinutes(slaMinutes);
        }

        var startUtc = startTime.Kind == DateTimeKind.Utc ? startTime : startTime.ToUniversalTime();

        if (!businessHoursOnly)
        {
            // 24x7 - simple calculation but still convert through local time for DST accuracy
            return startUtc.AddMinutes(slaMinutes);
        }

        // Business hours calculation with DST awareness
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(startUtc, timeZone);
        var remainingMinutes = slaMinutes;
        var currentTime = localTime;

        while (remainingMinutes > 0)
        {
            // If current time is outside business hours, move to next business hour start
            if (!IsWithinBusinessHours(currentTime))
            {
                currentTime = MoveToNextBusinessHourStart(currentTime);
            }

            // Calculate minutes until end of business day
            var businessDayEndLocal = currentTime.Date.Add(BusinessEndTime);
            var minutesUntilEndOfDay = (int)(businessDayEndLocal - currentTime).TotalMinutes;

            if (minutesUntilEndOfDay <= 0)
            {
                // Already past end of business day, move to next
                currentTime = MoveToNextBusinessHourStart(currentTime.Date.AddDays(1));
                continue;
            }

            // Consume as many minutes as possible from today
            var minutesToConsume = Math.Min(remainingMinutes, minutesUntilEndOfDay);
            currentTime = currentTime.AddMinutes(minutesToConsume);
            remainingMinutes -= minutesToConsume;

            // Handle DST transition - recheck after adding minutes
            if (IsInDstTransition(TimeZoneInfo.ConvertTimeToUtc(currentTime, timeZone), timeZoneId))
            {
                // During DST transition, add/subtract the DST offset difference
                var adjustment = GetDstAdjustmentMinutes(currentTime, timeZone);
                if (adjustment != 0)
                {
                    _logger.LogDebug("DST adjustment of {Minutes} minutes applied at {Time}", adjustment, currentTime);
                    currentTime = currentTime.AddMinutes(adjustment);
                }
            }
        }

        return TimeZoneInfo.ConvertTimeToUtc(currentTime, timeZone);
    }

    /// <inheritdoc />
    public int CalculateElapsedMinutes(DateTime startTime, DateTime endTime, string timeZoneId, bool businessHoursOnly)
    {
        var timeZone = GetTimeZone(timeZoneId);
        if (timeZone == null)
        {
            return (int)(endTime - startTime).TotalMinutes;
        }

        var startUtc = startTime.Kind == DateTimeKind.Utc ? startTime : startTime.ToUniversalTime();
        var endUtc = endTime.Kind == DateTimeKind.Utc ? endTime : endTime.ToUniversalTime();

        if (!businessHoursOnly)
        {
            return (int)(endUtc - startUtc).TotalMinutes;
        }

        // Calculate business minutes
        var localStart = TimeZoneInfo.ConvertTimeFromUtc(startUtc, timeZone);
        var localEnd = TimeZoneInfo.ConvertTimeFromUtc(endUtc, timeZone);
        var totalMinutes = 0;
        var currentTime = localStart;

        while (currentTime < localEnd)
        {
            if (IsWithinBusinessHours(currentTime))
            {
                var businessDayEnd = currentTime.Date.Add(BusinessEndTime);
                var endOfPeriod = localEnd < businessDayEnd ? localEnd : businessDayEnd;
                var minutesInPeriod = (int)(endOfPeriod - currentTime).TotalMinutes;
                totalMinutes += Math.Max(0, minutesInPeriod);
                currentTime = businessDayEnd;
            }
            else
            {
                currentTime = MoveToNextBusinessHourStart(currentTime);
            }
        }

        return totalMinutes;
    }

    /// <inheritdoc />
    public bool IsInDstTransition(DateTime time, string timeZoneId)
    {
        var timeZone = GetTimeZone(timeZoneId);
        if (timeZone == null)
            return false;

        // Check if this time is within 2 hours of a DST transition
        var adjustmentRules = timeZone.GetAdjustmentRules();
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(time, timeZone);

        foreach (var rule in adjustmentRules)
        {
            if (localTime.Year >= rule.DateStart.Year && localTime.Year <= rule.DateEnd.Year)
            {
                var dstStart = GetTransitionDate(rule.DaylightTransitionStart, localTime.Year);
                var dstEnd = GetTransitionDate(rule.DaylightTransitionEnd, localTime.Year);

                // Check if within 2 hours of either transition
                if (Math.Abs((localTime - dstStart).TotalHours) <= 2 ||
                    Math.Abs((localTime - dstEnd).TotalHours) <= 2)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private TimeZoneInfo? GetTimeZone(string timeZoneId)
    {
        try
        {
            // Try to find by IANA ID first (Linux/macOS), then by Windows ID
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            // Try common IANA to Windows mappings
            var windowsId = ConvertIanaToWindows(timeZoneId);
            if (!string.IsNullOrEmpty(windowsId))
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(windowsId);
                }
                catch
                {
                    // Ignore
                }
            }

            _logger.LogWarning("Time zone not found: {TimeZoneId}", timeZoneId);
            return null;
        }
    }

    private static string? ConvertIanaToWindows(string ianaId)
    {
        // Common IANA to Windows time zone mappings
        return ianaId switch
        {
            "America/New_York" => "Eastern Standard Time",
            "America/Chicago" => "Central Standard Time",
            "America/Denver" => "Mountain Standard Time",
            "America/Los_Angeles" => "Pacific Standard Time",
            "Europe/London" => "GMT Standard Time",
            "Europe/Paris" => "Romance Standard Time",
            "Europe/Berlin" => "W. Europe Standard Time",
            "Asia/Tokyo" => "Tokyo Standard Time",
            "Asia/Singapore" => "Singapore Standard Time",
            "Australia/Sydney" => "AUS Eastern Standard Time",
            "UTC" => "UTC",
            _ => null
        };
    }

    private bool IsWithinBusinessHours(DateTime localTime)
    {
        if (!BusinessDays.Contains(localTime.DayOfWeek))
            return false;

        var timeOfDay = localTime.TimeOfDay;
        return timeOfDay >= BusinessStartTime && timeOfDay < BusinessEndTime;
    }

    private DateTime MoveToNextBusinessHourStart(DateTime localTime)
    {
        var candidate = localTime;

        // If before business hours today, start at business hours
        if (candidate.TimeOfDay < BusinessStartTime && BusinessDays.Contains(candidate.DayOfWeek))
        {
            return candidate.Date.Add(BusinessStartTime);
        }

        // Move to next day
        candidate = candidate.Date.AddDays(1).Add(BusinessStartTime);

        // Skip weekends
        while (!BusinessDays.Contains(candidate.DayOfWeek))
        {
            candidate = candidate.AddDays(1);
        }

        return candidate;
    }

    private int GetDstAdjustmentMinutes(DateTime localTime, TimeZoneInfo timeZone)
    {
        // Check if we're transitioning in or out of DST
        var oneHourAgo = localTime.AddHours(-1);
        var isDstNow = timeZone.IsDaylightSavingTime(localTime);
        var wasDstBefore = timeZone.IsDaylightSavingTime(oneHourAgo);

        if (isDstNow && !wasDstBefore)
        {
            // Spring forward - lose an hour
            return 60;
        }
        else if (!isDstNow && wasDstBefore)
        {
            // Fall back - gain an hour (no adjustment needed for SLA calculation)
            return 0;
        }

        return 0;
    }

    private DateTime GetTransitionDate(TimeZoneInfo.TransitionTime transition, int year)
    {
        if (transition.IsFixedDateRule)
        {
            return new DateTime(year, transition.Month, transition.Day).Add(transition.TimeOfDay.TimeOfDay);
        }

        // Find the nth occurrence of the day of week in the month
        var firstOfMonth = new DateTime(year, transition.Month, 1);
        var dayOfWeek = transition.DayOfWeek;
        var week = transition.Week;

        // Find first occurrence
        var dayOffset = ((int)dayOfWeek - (int)firstOfMonth.DayOfWeek + 7) % 7;
        var firstOccurrence = firstOfMonth.AddDays(dayOffset);

        // Add weeks (week 5 means last occurrence)
        if (week == 5)
        {
            // Find last occurrence
            var candidate = firstOccurrence;
            while (candidate.Month == transition.Month)
            {
                if (candidate.AddDays(7).Month != transition.Month)
                    return candidate.Add(transition.TimeOfDay.TimeOfDay);
                candidate = candidate.AddDays(7);
            }
            return firstOccurrence.Add(transition.TimeOfDay.TimeOfDay);
        }

        return firstOccurrence.AddDays((week - 1) * 7).Add(transition.TimeOfDay.TimeOfDay);
    }
}
