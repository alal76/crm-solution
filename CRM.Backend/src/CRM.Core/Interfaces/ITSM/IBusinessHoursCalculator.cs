// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Interfaces.ITSM;

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
