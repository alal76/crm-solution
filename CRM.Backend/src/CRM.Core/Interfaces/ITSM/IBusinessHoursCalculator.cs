// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
