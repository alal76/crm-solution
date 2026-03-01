// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#if ITSM_ADVANCED
// This file is part of the CRM Solution.
// Copyright (c) 2025 CRM Solution Contributors
// Licensed under the AGPL-3.0 license.

using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Interface for change calendar and scheduling operations.
/// </summary>
public interface IChangeCalendarService
{
    /// <summary>
    /// Get scheduled changes within a date range.
    /// </summary>
    Task<List<ScheduledChange>> GetScheduledChangesAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Check for scheduling conflicts with a proposed change window.
    /// </summary>
    Task<List<ChangeConflict>> CheckConflictsAsync(int changeRequestId, DateTime proposedStart, DateTime proposedEnd);

    /// <summary>
    /// Get blackout periods that would prevent scheduling.
    /// </summary>
    Task<List<BlackoutPeriod>> GetBlackoutPeriodsAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Check if a proposed window falls within a blackout period.
    /// </summary>
    Task<BlackoutPeriod?> GetConflictingBlackoutAsync(DateTime proposedStart, DateTime proposedEnd);

    /// <summary>
    /// Create a blackout period (e.g., holiday freeze, maintenance window).
    /// </summary>
    Task<BlackoutPeriod> CreateBlackoutPeriodAsync(BlackoutPeriod blackout, int createdById);

    /// <summary>
    /// Find available slots for scheduling a change.
    /// </summary>
    Task<List<AvailableSlot>> FindAvailableSlotsAsync(int changeRequestId, int durationMinutes, int daysAhead = 14);

    /// <summary>
    /// Get maintenance windows that would be suitable for a change.
    /// </summary>
    Task<List<MaintenanceWindow>> GetMaintenanceWindowsAsync();
}

/// <summary>
/// Represents a scheduled change for calendar display.
/// </summary>
public class ScheduledChange
{
    public int ChangeRequestId { get; set; }
    public string ChangeNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ChangeType ChangeType { get; set; }
    public ChangeState Status { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string? RiskLevel { get; set; }
    public List<string> AffectedServices { get; set; } = new();
    public string? AssignedGroup { get; set; }
    public string? AssignedTo { get; set; }
}

/// <summary>
/// Represents a scheduling conflict between changes.
/// </summary>
public class ChangeConflict
{
    public ConflictType Type { get; set; }
    public ConflictSeverity Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? ConflictingChangeId { get; set; }
    public string? ConflictingChangeNumber { get; set; }
    public List<string> AffectedCIs { get; set; } = new();
    public DateTime? ConflictStart { get; set; }
    public DateTime? ConflictEnd { get; set; }
    public string? Recommendation { get; set; }
}

public enum ConflictType
{
    TimeOverlap,
    ResourceConflict,
    CIConflict,
    BlackoutPeriod,
    DependencyConflict,
    MaintenanceWindow
}

public enum ConflictSeverity
{
    Warning,
    Error,
    Critical
}

/// <summary>
/// Represents a blackout period during which changes should not be scheduled.
/// </summary>
public class BlackoutPeriod
{
    public int BlackoutId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public BlackoutType Type { get; set; }
    public bool AllowEmergencyChanges { get; set; }
    public bool IsRecurringYearly { get; set; }
    public bool IsActive { get; set; } = true;
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum BlackoutType
{
    Holiday,
    Freeze,
    BusinessCritical,
    Maintenance,
    Event
}

/// <summary>
/// Represents an available time slot for scheduling.
/// </summary>
public class AvailableSlot
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public SlotQuality Quality { get; set; }
    public bool IsMaintenanceWindow { get; set; }
    public string? MaintenanceWindowName { get; set; }
}

public enum SlotQuality
{
    Optimal, // Maintenance window, low-traffic period
    Good, // Outside business hours
    Acceptable, // Business hours, no conflicts
    Risky // Peak period, multiple changes scheduled
}

/// <summary>
/// Represents a recurring maintenance window.
/// </summary>
public class MaintenanceWindow
{
    public int WindowId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Service for managing change scheduling and calendar operations.
/// </summary>
public class ChangeCalendarService : IChangeCalendarService
{
    private readonly IDbContextResolver _dbContextResolver;
    private readonly ILogger<ChangeCalendarService> _logger;

    // Default maintenance windows
    private static readonly List<MaintenanceWindow> DefaultMaintenanceWindows = new()
    {
        new MaintenanceWindow
        {
            WindowId = 1,
            Name = "Weekend Maintenance (Saturday)",
            DayOfWeek = DayOfWeek.Saturday,
            StartTime = TimeSpan.FromHours(22), // 10 PM
            EndTime = TimeSpan.FromHours(26), // 2 AM (next day)
            Description = "Primary weekend maintenance window",
            IsActive = true
        },
        new MaintenanceWindow
        {
            WindowId = 2,
            Name = "Weekend Maintenance (Sunday)",
            DayOfWeek = DayOfWeek.Sunday,
            StartTime = TimeSpan.FromHours(2),
            EndTime = TimeSpan.FromHours(8),
            Description = "Sunday early morning maintenance",
            IsActive = true
        },
        new MaintenanceWindow
        {
            WindowId = 3,
            Name = "Weeknight Window",
            DayOfWeek = DayOfWeek.Wednesday,
            StartTime = TimeSpan.FromHours(23),
            EndTime = TimeSpan.FromHours(25), // 1 AM Thursday
            Description = "Mid-week maintenance window",
            IsActive = true
        }
    };

    public ChangeCalendarService(
        IDbContextResolver dbContextResolver,
        ILogger<ChangeCalendarService> logger)
    {
        _dbContextResolver = dbContextResolver;
        _logger = logger;
    }

    public async Task<List<ScheduledChange>> GetScheduledChangesAsync(DateTime startDate, DateTime endDate)
    {
        var context = _dbContextResolver.ResolveContext();

        var changes = await context.Changes
            .Where(c => c.PlannedStartDate.HasValue &&
                       c.PlannedStartDate >= startDate &&
                       c.PlannedStartDate <= endDate)
            .Where(c => c.State != ChangeState.Cancelled &&
                       c.State != ChangeState.Failed)
            .Include(c => c.AssignedTo)
            .Include(c => c.ImpactedCIs!)
                .ThenInclude(ci => ci.ConfigurationItem)
            .ToListAsync();

        return changes.Select(c => new ScheduledChange
        {
            ChangeRequestId = c.ChangeId,
            ChangeNumber = c.Number,
            Title = c.ShortDescription,
            ChangeType = c.Type,
            Status = c.State,
            ScheduledStart = c.PlannedStartDate ?? DateTime.MinValue,
            ScheduledEnd = c.PlannedEndDate ?? c.PlannedStartDate?.AddHours(2) ?? DateTime.MinValue,
            RiskLevel = c.Risk.ToString(),
            AffectedServices = c.ImpactedCIs?.Select(ci => ci.ConfigurationItem?.CIName ?? "").ToList() ?? new(),
            AssignedTo = c.AssignedTo != null ? $"{c.AssignedTo.FirstName} {c.AssignedTo.LastName}".Trim() : string.Empty
        }).ToList();
    }

    public async Task<List<ChangeConflict>> CheckConflictsAsync(
        int changeRequestId,
        DateTime proposedStart,
        DateTime proposedEnd)
    {
        var conflicts = new List<ChangeConflict>();
        var context = _dbContextResolver.ResolveContext();

        // Get the change request details
        var change = await context.Changes
            .Include(c => c.ImpactedCIs!)
                .ThenInclude(ci => ci.ConfigurationItem)
            .FirstOrDefaultAsync(c => c.ChangeId == changeRequestId);

        if (change == null)
        {
            throw new ArgumentException($"Change {changeRequestId} not found");
        }

        // 1. Check for blackout periods
        var blackout = await GetConflictingBlackoutAsync(proposedStart, proposedEnd);
        if (blackout != null)
        {
            var canOverride = change.Type == ChangeType.Emergency && blackout.AllowEmergencyChanges;
            conflicts.Add(new ChangeConflict
            {
                Type = ConflictType.BlackoutPeriod,
                Severity = canOverride ? ConflictSeverity.Warning : ConflictSeverity.Critical,
                Description = $"Proposed window overlaps with blackout period: {blackout.Name}",
                ConflictStart = blackout.StartDate,
                ConflictEnd = blackout.EndDate,
                Recommendation = canOverride
                    ? "Emergency change can proceed with additional approval"
                    : $"Reschedule to after {blackout.EndDate:g}"
            });
        }

        // 2. Check for overlapping changes
        var overlappingChanges = await context.Changes
            .Where(c => c.ChangeId != changeRequestId)
            .Where(c => c.PlannedStartDate.HasValue && c.PlannedEndDate.HasValue)
            .Where(c => c.State != ChangeState.Cancelled &&
                       c.State != ChangeState.Closed &&
                       c.State != ChangeState.Failed)
            .Where(c =>
                (c.PlannedStartDate <= proposedEnd && c.PlannedEndDate >= proposedStart))
            .Include(c => c.ImpactedCIs!)
                .ThenInclude(ci => ci.ConfigurationItem)
            .ToListAsync();

        foreach (var overlapping in overlappingChanges)
        {
            // Check for CI conflicts (same systems affected)
            var myCI = change.ImpactedCIs?.Select(ci => ci.CIId).ToList() ?? new();
            var theirCIs = overlapping.ImpactedCIs?.Select(ci => ci.CIId).ToList() ?? new();
            var sharedCIs = myCI.Intersect(theirCIs).ToList();

            if (sharedCIs.Any())
            {
                var affectedNames = overlapping.ImpactedCIs?
                    .Where(ci => sharedCIs.Contains(ci.CIId))
                    .Select(ci => ci.ConfigurationItem?.CIName ?? "Unknown")
                    .ToList() ?? new();

                conflicts.Add(new ChangeConflict
                {
                    Type = ConflictType.CIConflict,
                    Severity = ConflictSeverity.Error,
                    Description = $"Shared CI(s) affected by change {overlapping.Number}",
                    ConflictingChangeId = overlapping.ChangeId,
                    ConflictingChangeNumber = overlapping.Number,
                    AffectedCIs = affectedNames,
                    ConflictStart = overlapping.PlannedStartDate,
                    ConflictEnd = overlapping.PlannedEndDate,
                    Recommendation = "Coordinate with the other change team or reschedule"
                });
            }
            else
            {
                // Time overlap but no CI conflict
                conflicts.Add(new ChangeConflict
                {
                    Type = ConflictType.TimeOverlap,
                    Severity = ConflictSeverity.Warning,
                    Description = $"Time overlap with change {overlapping.Number} ({overlapping.ShortDescription})",
                    ConflictingChangeId = overlapping.ChangeId,
                    ConflictingChangeNumber = overlapping.Number,
                    ConflictStart = overlapping.PlannedStartDate,
                    ConflictEnd = overlapping.PlannedEndDate,
                    Recommendation = "Consider spreading changes across different windows to reduce risk"
                });
            }
        }

        // 3. Check if NOT in maintenance window (for normal changes)
        if (change.Type == ChangeType.Normal)
        {
            var isInMaintenanceWindow = await IsInMaintenanceWindowAsync(proposedStart, proposedEnd);
            if (!isInMaintenanceWindow)
            {
                conflicts.Add(new ChangeConflict
                {
                    Type = ConflictType.MaintenanceWindow,
                    Severity = ConflictSeverity.Warning,
                    Description = "Change is not scheduled during a maintenance window",
                    Recommendation = "Consider scheduling during a standard maintenance window for reduced impact"
                });
            }
        }

        _logger.LogInformation(
            "Conflict check for change {ChangeNumber}: {ConflictCount} conflicts found",
            change.Number, conflicts.Count);

        return conflicts;
    }

    public async Task<List<BlackoutPeriod>> GetBlackoutPeriodsAsync(DateTime startDate, DateTime endDate)
    {
        // In a full implementation, these would come from the database
        // For now, return some default blackout periods
        var blackouts = new List<BlackoutPeriod>
        {
            new BlackoutPeriod
            {
                BlackoutId = 1,
                Name = "Year-End Freeze",
                Description = "No changes during year-end business processing",
                StartDate = new DateTime(DateTime.UtcNow.Year, 12, 20),
                EndDate = new DateTime(DateTime.UtcNow.Year + 1, 1, 3),
                Type = BlackoutType.Freeze,
                AllowEmergencyChanges = true,
                IsActive = true
            },
            new BlackoutPeriod
            {
                BlackoutId = 2,
                Name = "Quarter-End Freeze",
                Description = "Limited changes during quarter-end processing",
                StartDate = new DateTime(DateTime.UtcNow.Year, 3, 28),
                EndDate = new DateTime(DateTime.UtcNow.Year, 4, 2),
                Type = BlackoutType.BusinessCritical,
                AllowEmergencyChanges = true,
                IsActive = true
            }
        };

        return await Task.FromResult(
            blackouts.Where(b =>
                b.IsActive &&
                ((b.StartDate <= endDate && b.EndDate >= startDate) ||
                 (b.IsRecurringYearly && IsRecurringInRange(b, startDate, endDate))))
            .ToList());
    }

    public async Task<BlackoutPeriod?> GetConflictingBlackoutAsync(DateTime proposedStart, DateTime proposedEnd)
    {
        var blackouts = await GetBlackoutPeriodsAsync(proposedStart, proposedEnd);

        return blackouts.FirstOrDefault(b =>
            b.StartDate <= proposedEnd && b.EndDate >= proposedStart);
    }

    public async Task<BlackoutPeriod> CreateBlackoutPeriodAsync(BlackoutPeriod blackout, int createdById)
    {
        // In a full implementation, this would persist to database
        blackout.CreatedById = createdById;
        blackout.CreatedAt = DateTime.UtcNow;
        blackout.IsActive = true;

        _logger.LogInformation(
            "Created blackout period: {Name} from {Start} to {End}",
            blackout.Name, blackout.StartDate, blackout.EndDate);

        return await Task.FromResult(blackout);
    }

    public async Task<List<AvailableSlot>> FindAvailableSlotsAsync(
        int changeRequestId,
        int durationMinutes,
        int daysAhead = 14)
    {
        var slots = new List<AvailableSlot>();
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(daysAhead);

        // Get existing scheduled changes
        var scheduledChanges = await GetScheduledChangesAsync(startDate, endDate);

        // Get blackout periods
        var blackouts = await GetBlackoutPeriodsAsync(startDate, endDate);

        // Get maintenance windows
        var maintenanceWindows = await GetMaintenanceWindowsAsync();

        // Find slots in maintenance windows first (optimal)
        for (var day = startDate; day <= endDate; day = day.AddDays(1))
        {
            foreach (var window in maintenanceWindows.Where(w => w.IsActive))
            {
                if (window.DayOfWeek == day.DayOfWeek)
                {
                    var windowStart = day.Date + window.StartTime;
                    var windowEnd = day.Date + window.EndTime;

                    // Handle windows that cross midnight
                    if (window.EndTime < window.StartTime)
                    {
                        windowEnd = windowEnd.AddDays(1);
                    }

                    // Check if this window is available
                    var isBlocked = blackouts.Any(b =>
                        b.StartDate <= windowEnd && b.EndDate >= windowStart);

                    var hasConflict = scheduledChanges.Any(c =>
                        c.ScheduledStart <= windowEnd && c.ScheduledEnd >= windowStart);

                    if (!isBlocked && !hasConflict && windowStart > DateTime.UtcNow)
                    {
                        var availableDuration = (int)(windowEnd - windowStart).TotalMinutes;
                        if (availableDuration >= durationMinutes)
                        {
                            slots.Add(new AvailableSlot
                            {
                                StartTime = windowStart,
                                EndTime = windowStart.AddMinutes(durationMinutes),
                                DurationMinutes = durationMinutes,
                                Quality = SlotQuality.Optimal,
                                IsMaintenanceWindow = true,
                                MaintenanceWindowName = window.Name
                            });
                        }
                    }
                }
            }
        }

        // Also find some off-hours slots (Good quality)
        for (var day = startDate; day <= endDate; day = day.AddDays(1))
        {
            // Skip weekends for off-hours slots
            if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                continue;

            // Evening slot (8 PM - midnight)
            var eveningStart = day.Date.AddHours(20);
            var eveningEnd = day.Date.AddHours(24);

            var isBlocked = blackouts.Any(b =>
                b.StartDate <= eveningEnd && b.EndDate >= eveningStart);

            var hasConflict = scheduledChanges.Any(c =>
                c.ScheduledStart <= eveningEnd && c.ScheduledEnd >= eveningStart);

            if (!isBlocked && !hasConflict && eveningStart > DateTime.UtcNow)
            {
                slots.Add(new AvailableSlot
                {
                    StartTime = eveningStart,
                    EndTime = eveningStart.AddMinutes(durationMinutes),
                    DurationMinutes = durationMinutes,
                    Quality = SlotQuality.Good,
                    IsMaintenanceWindow = false
                });
            }
        }

        return slots.OrderBy(s => s.Quality).ThenBy(s => s.StartTime).Take(10).ToList();
    }

    public Task<List<MaintenanceWindow>> GetMaintenanceWindowsAsync()
    {
        // In a full implementation, these would come from database
        return Task.FromResult(DefaultMaintenanceWindows);
    }

    private async Task<bool> IsInMaintenanceWindowAsync(DateTime start, DateTime end)
    {
        var windows = await GetMaintenanceWindowsAsync();

        foreach (var window in windows.Where(w => w.IsActive))
        {
            if (window.DayOfWeek == start.DayOfWeek)
            {
                var windowStart = start.Date + window.StartTime;
                var windowEnd = start.Date + window.EndTime;

                if (window.EndTime < window.StartTime)
                    windowEnd = windowEnd.AddDays(1);

                if (start >= windowStart && end <= windowEnd)
                    return true;
            }
        }

        return false;
    }

    private static bool IsRecurringInRange(BlackoutPeriod blackout, DateTime startDate, DateTime endDate)
    {
        if (!blackout.IsRecurringYearly)
            return false;

        // Check if the recurring dates fall within the range for the relevant year(s)
        for (var year = startDate.Year; year <= endDate.Year; year++)
        {
            var recurStart = new DateTime(year, blackout.StartDate.Month, blackout.StartDate.Day);
            var recurEnd = new DateTime(year, blackout.EndDate.Month, blackout.EndDate.Day);

            if (recurStart <= endDate && recurEnd >= startDate)
                return true;
        }

        return false;
    }
}


#endif
