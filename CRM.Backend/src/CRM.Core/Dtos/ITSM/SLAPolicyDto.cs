// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.ITSM;

namespace CRM.Core.Dtos.ITSM;

/// <summary>
/// DTO for SLA policy response
/// </summary>
public class SLAPolicyDto
{
    public int Id { get; set; }
    public int SLAPolicyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
    public int ResponseTimeHours { get; set; }
    public int ResolutionTimeHours { get; set; }
    public bool BusinessHoursOnly { get; set; }
    public string Timezone { get; set; } = "UTC";
    public string BreachAction { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // SLA-based properties used by SLAService
    public SLATargetType TargetType { get; set; }
    public int? P1ResponseMinutes { get; set; }
    public int? P1ResolutionMinutes { get; set; }
    public int? P2ResponseMinutes { get; set; }
    public int? P2ResolutionMinutes { get; set; }
    public int? P3ResponseMinutes { get; set; }
    public int? P3ResolutionMinutes { get; set; }
    public int? P4ResponseMinutes { get; set; }
    public int? P4ResolutionMinutes { get; set; }
    public bool UseBusinessHours { get; set; }
}

/// <summary>
/// DTO for creating SLA policy
/// </summary>
public class CreateSLAPolicyDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
    public int ResponseTimeHours { get; set; }
    public int ResolutionTimeHours { get; set; }
    public bool BusinessHoursOnly { get; set; } = true;
    public string Timezone { get; set; } = "UTC";
    public string BreachAction { get; set; } = "Notify";
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for updating SLA policy
/// </summary>
public class UpdateSLAPolicyDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
    public int? ResponseTimeHours { get; set; }
    public int? ResolutionTimeHours { get; set; }
    public bool? BusinessHoursOnly { get; set; }
    public string? Timezone { get; set; }
    public string? BreachAction { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// DTO for SLA instance (tracking SLA on a ticket)
/// </summary>
public class SLAInstanceDto
{
    public int Id { get; set; }
    public int SLAInstanceId { get; set; }
    public int ServiceRequestId { get; set; }
    public int TargetId { get; set; }
    public SLATargetType TargetType { get; set; }
    public int PolicyId { get; set; }
    public DateTime ResponseTargetTime { get; set; }
    public DateTime ResolutionTargetTime { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTime? ActualResponseTime { get; set; }
    public DateTime? ActualResolutionTime { get; set; }
    public bool IsBreach { get; set; }
    public DateTime? BreachTime { get; set; }
    public DateTime CreatedAt { get; set; }

    // SLAService-specific properties
    public DateTime? ResponseDueAt { get; set; }
    public DateTime? ResolutionDueAt { get; set; }
    public bool ResponseBreached { get; set; }
    public bool ResolutionBreached { get; set; }
    public SLAState State { get; set; }
    public int? MinutesUntilResponseBreach { get; set; }
    public int? MinutesUntilResolutionBreach { get; set; }
}
