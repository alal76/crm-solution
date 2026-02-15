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

namespace CRM.Core.Entities.ITSM;

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

/// <summary>
/// Represents a single day's business hours.
/// </summary>
public class BusinessDay
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsWorkingDay { get; set; } = true;
}

/// <summary>
/// Represents a holiday in the business schedule.
/// </summary>
public class Holiday
{
    public DateTime Date { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsRecurringYearly { get; set; }
}
