// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Interface for business hours calculations used in SLA management.
/// Supports multiple time zones, holidays, and custom schedules.
/// </summary>
public interface IBusinessHoursCalculator
{
    /// <summary>
    /// Calculate the due date by adding business minutes to a start time.
    /// </summary>
    Task<DateTime> AddBusinessMinutesAsync(DateTime startTime, int businessMinutes, int? scheduleId = null);

    /// <summary>
    /// Calculate elapsed business minutes between two dates.
    /// </summary>
    Task<int> GetElapsedBusinessMinutesAsync(DateTime startTime, DateTime endTime, int? scheduleId = null);

    /// <summary>
    /// Check if a given time is within business hours.
    /// </summary>
    Task<bool> IsBusinessTimeAsync(DateTime dateTime, int? scheduleId = null);

    /// <summary>
    /// Get the next business day start time from a given date.
    /// </summary>
    Task<DateTime> GetNextBusinessStartAsync(DateTime fromDate, int? scheduleId = null);

    /// <summary>
    /// Check if a date is a holiday.
    /// </summary>
    Task<bool> IsHolidayAsync(DateTime date, int? scheduleId = null);
}

/// <summary>
/// Represents a business hours schedule with daily time ranges.
/// </summary>
public class BusinessSchedule
{
    public int ScheduleId { get; set; }
    public string Name { get; set; } = "Default";
    public string TimeZoneId { get; set; } = "UTC";
    public List<BusinessDay> Days { get; set; } = new();
    public List<Holiday> Holidays { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public class BusinessDay
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsWorkingDay { get; set; } = true;
}

public class Holiday
{
    public DateTime Date { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsRecurringYearly { get; set; }
}

/// <summary>
/// Business hours calculator for accurate SLA time calculations.
/// Handles weekends, holidays, and custom business schedules.
/// </summary>
public class BusinessHoursCalculator : IBusinessHoursCalculator
{
    private readonly IDbContextResolver _dbContextResolver;
    private readonly ILogger<BusinessHoursCalculator> _logger;

    // Default schedule: Mon-Fri, 9 AM - 5 PM UTC
    private static readonly BusinessSchedule DefaultSchedule = new()
    {
        ScheduleId = 0,
        Name = "Default Business Hours",
        TimeZoneId = "UTC",
        Days = new List<BusinessDay>
        {
            new() { DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(17), IsWorkingDay = true },
            new() { DayOfWeek = DayOfWeek.Tuesday, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(17), IsWorkingDay = true },
            new() { DayOfWeek = DayOfWeek.Wednesday, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(17), IsWorkingDay = true },
            new() { DayOfWeek = DayOfWeek.Thursday, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(17), IsWorkingDay = true },
            new() { DayOfWeek = DayOfWeek.Friday, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(17), IsWorkingDay = true },
            new() { DayOfWeek = DayOfWeek.Saturday, IsWorkingDay = false },
            new() { DayOfWeek = DayOfWeek.Sunday, IsWorkingDay = false },
        }
    };

    public BusinessHoursCalculator(IDbContextResolver dbContextResolver, ILogger<BusinessHoursCalculator> logger)
    {
        _dbContextResolver = dbContextResolver;
        _logger = logger;
    }

    public async Task<DateTime> AddBusinessMinutesAsync(DateTime startTime, int businessMinutes, int? scheduleId = null)
    {
        var schedule = await GetScheduleAsync(scheduleId);
        var tz = GetTimeZone(schedule.TimeZoneId);

        // Convert to schedule's timezone
        var localStart = TimeZoneInfo.ConvertTimeFromUtc(startTime.ToUniversalTime(), tz);
        var currentTime = localStart;
        var remainingMinutes = businessMinutes;

        // Safety limit to prevent infinite loops
        const int maxIterations = 10000;
        var iterations = 0;

        while (remainingMinutes > 0 && iterations < maxIterations)
        {
            iterations++;

            // Check if current day is a working day
            var daySchedule = GetDaySchedule(schedule, currentTime.DayOfWeek);
            var isHoliday = await IsHolidayAsync(currentTime.Date, scheduleId);

            if (!daySchedule.IsWorkingDay || isHoliday)
            {
                // Skip to next day at start of business hours
                currentTime = GetNextWorkingDayStart(schedule, currentTime);
                continue;
            }

            // Get business hours for this day
            var dayStart = currentTime.Date + daySchedule.StartTime;
            var dayEnd = currentTime.Date + daySchedule.EndTime;

            // If before business hours, jump to start
            if (currentTime < dayStart)
            {
                currentTime = dayStart;
            }

            // If after business hours, jump to next day
            if (currentTime >= dayEnd)
            {
                currentTime = GetNextWorkingDayStart(schedule, currentTime);
                continue;
            }

            // Calculate available minutes in this day
            var availableMinutes = (int)(dayEnd - currentTime).TotalMinutes;

            if (remainingMinutes <= availableMinutes)
            {
                // We can fit remaining minutes in this day
                currentTime = currentTime.AddMinutes(remainingMinutes);
                remainingMinutes = 0;
            }
            else
            {
                // Use up this day and continue to next
                remainingMinutes -= availableMinutes;
                currentTime = GetNextWorkingDayStart(schedule, currentTime);
            }
        }

        if (iterations >= maxIterations)
        {
            _logger.LogWarning("Business hours calculation exceeded max iterations for {Minutes} minutes from {Start}",
                businessMinutes, startTime);
        }

        // Convert back to UTC — use safe helper to handle DST ambiguous/invalid edge cases (TODO-SD003-008).
        return SafeConvertLocalToUtc(currentTime, tz);
    }

    public async Task<int> GetElapsedBusinessMinutesAsync(DateTime startTime, DateTime endTime, int? scheduleId = null)
    {
        if (endTime <= startTime)
            return 0;

        var schedule = await GetScheduleAsync(scheduleId);
        var tz = GetTimeZone(schedule.TimeZoneId);

        // Convert to schedule's timezone
        var localStart = TimeZoneInfo.ConvertTimeFromUtc(startTime.ToUniversalTime(), tz);
        var localEnd = TimeZoneInfo.ConvertTimeFromUtc(endTime.ToUniversalTime(), tz);

        var currentTime = localStart;
        var totalMinutes = 0;

        // Safety limit
        const int maxIterations = 10000;
        var iterations = 0;

        while (currentTime < localEnd && iterations < maxIterations)
        {
            iterations++;

            var daySchedule = GetDaySchedule(schedule, currentTime.DayOfWeek);
            var isHoliday = await IsHolidayAsync(currentTime.Date, scheduleId);

            if (!daySchedule.IsWorkingDay || isHoliday)
            {
                // Skip to next day
                currentTime = currentTime.Date.AddDays(1);
                continue;
            }

            var dayStart = currentTime.Date + daySchedule.StartTime;
            var dayEnd = currentTime.Date + daySchedule.EndTime;

            // Adjust for start/end boundaries
            var effectiveStart = currentTime < dayStart ? dayStart : currentTime;
            var effectiveEnd = localEnd < dayEnd ? localEnd : dayEnd;

            if (effectiveStart < effectiveEnd && effectiveStart >= dayStart && effectiveEnd <= dayEnd)
            {
                totalMinutes += (int)(effectiveEnd - effectiveStart).TotalMinutes;
            }

            // Move to next day
            currentTime = currentTime.Date.AddDays(1);
        }

        return totalMinutes;
    }

    public async Task<bool> IsBusinessTimeAsync(DateTime dateTime, int? scheduleId = null)
    {
        var schedule = await GetScheduleAsync(scheduleId);
        var tz = GetTimeZone(schedule.TimeZoneId);

        var localTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime.ToUniversalTime(), tz);

        // Check if holiday
        if (await IsHolidayAsync(localTime.Date, scheduleId))
            return false;

        // Check day schedule
        var daySchedule = GetDaySchedule(schedule, localTime.DayOfWeek);
        if (!daySchedule.IsWorkingDay)
            return false;

        var timeOfDay = localTime.TimeOfDay;
        return timeOfDay >= daySchedule.StartTime && timeOfDay < daySchedule.EndTime;
    }

    public async Task<DateTime> GetNextBusinessStartAsync(DateTime fromDate, int? scheduleId = null)
    {
        var schedule = await GetScheduleAsync(scheduleId);
        var tz = GetTimeZone(schedule.TimeZoneId);

        var localTime = TimeZoneInfo.ConvertTimeFromUtc(fromDate.ToUniversalTime(), tz);
        var nextStart = GetNextWorkingDayStart(schedule, localTime);

        // Use safe helper to handle DST ambiguous/invalid edge cases (TODO-SD003-008).
        return SafeConvertLocalToUtc(nextStart, tz);
    }

    public async Task<bool> IsHolidayAsync(DateTime date, int? scheduleId = null)
    {
        var schedule = await GetScheduleAsync(scheduleId);
        var dateOnly = date.Date;

        foreach (var holiday in schedule.Holidays)
        {
            if (holiday.IsRecurringYearly)
            {
                // Check month and day only
                if (holiday.Date.Month == dateOnly.Month && holiday.Date.Day == dateOnly.Day)
                    return true;
            }
            else
            {
                if (holiday.Date.Date == dateOnly)
                    return true;
            }
        }

        // Also check database for custom holidays if available
        try
        {
            // Check if there's a holiday table - this would need to be added to the ITSM schema
            // For now, just use the schedule's built-in holidays
        }
        catch
        {
            // Ignore database errors for holiday lookup
        }

        return false;
    }

    private async Task<BusinessSchedule> GetScheduleAsync(int? scheduleId)
    {
        if (!scheduleId.HasValue || scheduleId == 0)
        {
            return DefaultSchedule;
        }

        try
        {
            var context = _dbContextResolver.ResolveContext();

            // Load custom schedule from database
            var dbSchedule = await context.BusinessHoursSchedules
                .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId && !s.IsDeleted && s.IsActive);

            if (dbSchedule == null)
            {
                _logger.LogWarning("Business schedule {ScheduleId} not found or inactive, using default", scheduleId);
                return DefaultSchedule;
            }

            // Convert database entity to BusinessSchedule
            var schedule = new BusinessSchedule
            {
                ScheduleId = dbSchedule.ScheduleId,
                Name = dbSchedule.Name,
                TimeZoneId = dbSchedule.TimeZone ?? "UTC",
                IsActive = dbSchedule.IsActive,
                Days = ParseBusinessHours(dbSchedule.BusinessHours),
                Holidays = ParseHolidays(dbSchedule.Holidays),
            };

            return schedule;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load business schedule {ScheduleId}, using default", scheduleId);
        }

        return DefaultSchedule;
    }

    /// <summary>
    /// Parses JSON business hours into BusinessDay list.
    /// Expected format: {"Monday": {"start": "09:00", "end": "17:00"}, ...}
    /// </summary>
    private List<BusinessDay> ParseBusinessHours(string? businessHoursJson)
    {
        if (string.IsNullOrWhiteSpace(businessHoursJson))
        {
            return DefaultSchedule.Days;
        }

        try
        {
            var result = new List<BusinessDay>();
            using var doc = System.Text.Json.JsonDocument.Parse(businessHoursJson);

            foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
            {
                var dayName = day.ToString();
                if (doc.RootElement.TryGetProperty(dayName, out var dayConfig))
                {
                    var startStr = dayConfig.GetProperty("start").GetString() ?? "09:00";
                    var endStr = dayConfig.GetProperty("end").GetString() ?? "17:00";

                    result.Add(new BusinessDay
                    {
                        DayOfWeek = day,
                        StartTime = TimeSpan.Parse(startStr),
                        EndTime = TimeSpan.Parse(endStr),
                        IsWorkingDay = true,
                    });
                }
                else
                {
                    // Day not defined means not a working day
                    result.Add(new BusinessDay
                    {
                        DayOfWeek = day,
                        IsWorkingDay = false,
                    });
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse business hours JSON, using defaults");
            return DefaultSchedule.Days;
        }
    }

    /// <summary>
    /// Parses JSON holidays into Holiday list.
    /// Expected format: ["2025-01-01", "2025-12-25", ...] or [{"date": "2025-01-01", "name": "New Year", "recurring": true}, ...]
    /// </summary>
    private List<Holiday> ParseHolidays(string? holidaysJson)
    {
        if (string.IsNullOrWhiteSpace(holidaysJson))
        {
            return new List<Holiday>();
        }

        try
        {
            var result = new List<Holiday>();
            using var doc = System.Text.Json.JsonDocument.Parse(holidaysJson);

            foreach (var element in doc.RootElement.EnumerateArray())
            {
                if (element.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    // Simple date string format
                    if (DateTime.TryParse(element.GetString(), out var date))
                    {
                        result.Add(new Holiday { Date = date, Name = "Holiday" });
                    }
                }
                else if (element.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    // Complex object format
                    var dateStr = element.GetProperty("date").GetString();
                    var name = element.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "Holiday" : "Holiday";
                    var recurring = element.TryGetProperty("recurring", out var recurProp) && recurProp.GetBoolean();

                    if (DateTime.TryParse(dateStr, out var date))
                    {
                        result.Add(new Holiday { Date = date, Name = name, IsRecurringYearly = recurring });
                    }
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse holidays JSON, using empty list");
            return new List<Holiday>();
        }
    }

    private static BusinessDay GetDaySchedule(BusinessSchedule schedule, DayOfWeek dayOfWeek)
    {
        var day = schedule.Days.FirstOrDefault(d => d.DayOfWeek == dayOfWeek);
        return day ?? new BusinessDay { DayOfWeek = dayOfWeek, IsWorkingDay = false };
    }

    private DateTime GetNextWorkingDayStart(BusinessSchedule schedule, DateTime currentTime)
    {
        var nextDay = currentTime.Date.AddDays(1);
        const int maxDays = 365; // Safety limit

        for (var i = 0; i < maxDays; i++)
        {
            var daySchedule = GetDaySchedule(schedule, nextDay.DayOfWeek);
            if (daySchedule.IsWorkingDay)
            {
                // Check synchronously - would need async version for production
                return nextDay + daySchedule.StartTime;
            }
            nextDay = nextDay.AddDays(1);
        }

        // Fallback - should never reach here
        return currentTime.AddDays(1);
    }

    private static TimeZoneInfo GetTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch
        {
            // Fallback to UTC if timezone not found
            return TimeZoneInfo.Utc;
        }
    }

    /// <summary>
    /// Safely converts a local <see cref="DateTime"/> to UTC, correctly handling DST edge cases
    /// (TODO-SD003-008).
    /// <list type="bullet">
    ///   <item><description>
    ///     <b>Ambiguous time</b> (clock falls back — same local time occurs twice): resolves to
    ///     the <em>standard-time</em> occurrence (i.e. the first occurrence, larger UTC offset).
    ///   </description></item>
    ///   <item><description>
    ///     <b>Invalid time</b> (clock springs forward — local time does not exist): advances
    ///     minute-by-minute until a valid local time is found, then converts that.
    ///   </description></item>
    /// </list>
    /// </summary>
    /// <param name="localTime">A local <see cref="DateTime"/> (Kind should be Unspecified or Local).</param>
    /// <param name="tz">The timezone to convert from.</param>
    /// <returns>The corresponding UTC <see cref="DateTime"/>.</returns>
    internal static DateTime SafeConvertLocalToUtc(DateTime localTime, TimeZoneInfo tz)
    {
        // UTC timezone — no conversion needed.
        if (tz.Equals(TimeZoneInfo.Utc))
        {
            return DateTime.SpecifyKind(localTime, DateTimeKind.Utc);
        }

        // Ensure DateTimeKind is Unspecified so that TimeZoneInfo treats it as local to 'tz'.
        var unspecified = localTime.Kind == DateTimeKind.Unspecified
            ? localTime
            : DateTime.SpecifyKind(localTime, DateTimeKind.Unspecified);

        // Handle ambiguous time (fall-back DST transition: same local time occurs twice).
        if (tz.IsAmbiguousTime(unspecified))
        {
            // GetAmbiguousTimeOffsets returns both possible UTC offsets for the ambiguous local time.
            // Standard time has the more-negative (smaller) offset (e.g. EST = -5 hours vs EDT = -4 hours).
            // Using Min resolves the ambiguous time to standard time (the second / post fall-back occurrence),
            // which is the conservative choice for SLA calculations.
            var offsets = tz.GetAmbiguousTimeOffsets(unspecified);
            var standardOffset = offsets.Min();
            return DateTime.SpecifyKind(unspecified - standardOffset, DateTimeKind.Utc);
        }

        // Handle invalid time (spring-forward DST gap: local time does not exist).
        if (tz.IsInvalidTime(unspecified))
        {
            // Advance in 1-minute steps until we land on a valid local time after the gap.
            var adjusted = unspecified;
            const int maxMinutes = 120; // DST gaps are at most 60 minutes; guard against infinite loops.
            for (var i = 0; i < maxMinutes && tz.IsInvalidTime(adjusted); i++)
            {
                adjusted = adjusted.AddMinutes(1);
            }
            return TimeZoneInfo.ConvertTimeToUtc(adjusted, tz);
        }

        return TimeZoneInfo.ConvertTimeToUtc(unspecified, tz);
    }
}

/// <summary>
/// Extension methods for business hours calculations in SLA context.
/// </summary>
public static class BusinessHoursExtensions
{
    /// <summary>
    /// Calculate the SLA due date using business hours if required.
    /// </summary>
    public static async Task<DateTime> CalculateSLADueDateAsync(
        this IBusinessHoursCalculator calculator,
        DateTime startTime,
        int targetMinutes,
        bool useBusinessHours)
    {
        if (!useBusinessHours)
        {
            // Simple calendar time calculation
            return startTime.AddMinutes(targetMinutes);
        }

        return await calculator.AddBusinessMinutesAsync(startTime, targetMinutes);
    }

    /// <summary>
    /// Calculate percentage of SLA time elapsed.
    /// </summary>
    public static async Task<double> CalculateSLAPercentageAsync(
        this IBusinessHoursCalculator calculator,
        DateTime startTime,
        DateTime dueTime,
        DateTime currentTime,
        bool useBusinessHours)
    {
        if (!useBusinessHours)
        {
            var totalMinutes = (dueTime - startTime).TotalMinutes;
            var elapsedMinutes = (currentTime - startTime).TotalMinutes;
            return totalMinutes > 0 ? (elapsedMinutes / totalMinutes) * 100 : 100;
        }

        var totalBusinessMinutes = await calculator.GetElapsedBusinessMinutesAsync(startTime, dueTime);
        var elapsedBusinessMinutes = await calculator.GetElapsedBusinessMinutesAsync(startTime, currentTime);

        return totalBusinessMinutes > 0 ? (elapsedBusinessMinutes / (double)totalBusinessMinutes) * 100 : 100;
    }
}
