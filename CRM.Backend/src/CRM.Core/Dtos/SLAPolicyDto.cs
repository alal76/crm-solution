// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for SLA Policy
/// </summary>
public class SLAPolicyDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Priority { get; set; } // Critical, High, Medium, Low

    public int InitialResponseTimeMinutes { get; set; }

    public int ResolutionTimeMinutes { get; set; }

    public bool WorkingHoursOnly { get; set; }

    public string EscalationPath { get; set; } // JSON array of user IDs

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Create SLA Policy DTO
/// </summary>
public class CreateSLAPolicyDto
{
    public string Name { get; set; }

    public string Description { get; set; }

    public string Priority { get; set; }

    public int InitialResponseTimeMinutes { get; set; }

    public int ResolutionTimeMinutes { get; set; }

    public bool WorkingHoursOnly { get; set; }

    public List<int> EscalationPathUserIds { get; set; } = new();
}

/// <summary>
/// Update SLA Policy DTO
/// </summary>
public class UpdateSLAPolicyDto
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Priority { get; set; }

    public int? InitialResponseTimeMinutes { get; set; }

    public int? ResolutionTimeMinutes { get; set; }

    public bool? WorkingHoursOnly { get; set; }

    public List<int>? EscalationPathUserIds { get; set; }

    public bool? IsActive { get; set; }
}
