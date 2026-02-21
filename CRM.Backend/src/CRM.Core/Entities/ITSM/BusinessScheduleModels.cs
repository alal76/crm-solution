// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
