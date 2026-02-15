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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Core.Entities.KnowledgeBase;

namespace CRM.Core.Entities.ITSM;

public enum SLATargetType
{
    Incident = 1,
    ServiceRequest = 2,
    Problem = 3,
    Change = 4
}

public enum SLAState
{
    Active = 1,
    Paused = 2,
    Completed = 3,
    Breached = 4,
    Cancelled = 5
}

public class SLAPolicy
{
    [Key]
    public int SLAPolicyId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public SLATargetType TargetType { get; set; }

    // Response SLA (minutes) by priority
    public int? P1ResponseMinutes { get; set; } = 15;

    public int? P2ResponseMinutes { get; set; } = 30;

    public int? P3ResponseMinutes { get; set; } = 120;

    public int? P4ResponseMinutes { get; set; } = 480;

    // Resolution SLA (minutes) by priority
    public int? P1ResolutionMinutes { get; set; } = 240;

    public int? P2ResolutionMinutes { get; set; } = 480;

    public int? P3ResolutionMinutes { get; set; } = 1440;

    public int? P4ResolutionMinutes { get; set; } = 7200;

    // Business Hours
    public bool UseBusinessHours { get; set; } = true;

    public int? BusinessHoursScheduleId { get; set; }

    [ForeignKey(nameof(BusinessHoursScheduleId))]
    public BusinessHoursSchedule? BusinessHoursSchedule { get; set; }

    // Conditions (JSON - can be evaluated for specific incidents/requests)
    public string? Conditions { get; set; }

    /// <summary>
    /// Business hours configuration ID for this SLA policy
    /// </summary>
    public int? BusinessHoursId { get; set; }

    /// <summary>
    /// Navigation to business hours configuration
    /// </summary>
    public virtual BusinessHours? BusinessHours { get; set; }

    /// <summary>
    /// Escalation rules associated with this SLA policy
    /// </summary>
    public virtual ICollection<EscalationRule> EscalationRules { get; set; } = new List<EscalationRule>();

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public User? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
}

public class SLAInstance
{
    [Key]
    public int SLAInstanceId { get; set; }

    [Required]
    public int TargetId { get; set; }

    [Required]
    public SLATargetType TargetType { get; set; }

    [Required]
    public int SLAPolicyId { get; set; }

    [ForeignKey(nameof(SLAPolicyId))]
    public SLAPolicy? SLAPolicy { get; set; }

    // Response SLA
    public DateTime? ResponseDueAt { get; set; }

    public DateTime? ResponseActualAt { get; set; }

    public bool ResponseBreached { get; set; } = false;

    public int? ResponseBusinessMinutes { get; set; }

    // Resolution SLA
    public DateTime? ResolutionDueAt { get; set; }

    public DateTime? ResolutionActualAt { get; set; }

    public bool ResolutionBreached { get; set; } = false;

    public int? ResolutionBusinessMinutes { get; set; }

    // Tracking
    [Required]
    public SLAState State { get; set; } = SLAState.Active;

    public DateTime? PausedAt { get; set; }

    public int PausedMinutes { get; set; } = 0;

    public string? PauseReason { get; set; }

    // Notifications
    public bool Response50PercentNotificationSent { get; set; } = false;

    public bool Response75PercentNotificationSent { get; set; } = false;

    public bool ResponseBreachNotificationSent { get; set; } = false;

    public bool Resolution50PercentNotificationSent { get; set; } = false;

    public bool Resolution75PercentNotificationSent { get; set; } = false;

    public bool ResolutionBreachNotificationSent { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedAt { get; set; }
}

public class BusinessHoursSchedule
{
    [Key]
    public int ScheduleId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? TimeZone { get; set; } = "UTC";

    // Business Hours (JSON with day-specific times)
    // Example: {"Monday": {"start": "09:00", "end": "17:00"}, ...}
    public string? BusinessHours { get; set; }

    // Holidays (JSON array of dates)
    // Example: ["2025-01-01", "2025-12-25", ...]
    public string? Holidays { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
}
