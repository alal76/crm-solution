// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for SLA Policy.
/// </summary>
public class SLAPolicyDto
{
    public int Id { get; set; }

    /// <summary>
    /// Name of the SLA policy.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the SLA policy.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Priority of the SLA policy (Critical, High, Medium, Low).
    /// </summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>
    /// Initial response time in minutes.
    /// </summary>
    public int InitialResponseTimeMinutes { get; set; }

    /// <summary>
    /// Resolution time in minutes.
    /// </summary>
    public int ResolutionTimeMinutes { get; set; }

    /// <summary>
    /// Whether the policy applies only during working hours.
    /// </summary>
    public bool WorkingHoursOnly { get; set; }

    /// <summary>
    /// Escalation path as a JSON array of user IDs.
    /// </summary>
    public string EscalationPath { get; set; } = string.Empty;

    /// <summary>
    /// Whether the policy is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Date and time the policy was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time the policy was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Create SLA Policy DTO
/// </summary>
public class CreateSLAPolicyDto
{
    /// <summary>
    /// Name of the SLA policy.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the SLA policy.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Priority of the SLA policy.
    /// </summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>
    /// Initial response time in minutes.
    /// </summary>
    public int InitialResponseTimeMinutes { get; set; }

    /// <summary>
    /// Resolution time in minutes.
    /// </summary>
    public int ResolutionTimeMinutes { get; set; }

    /// <summary>
    /// Whether the policy applies only during working hours.
    /// </summary>
    public bool WorkingHoursOnly { get; set; }

    /// <summary>
    /// List of user IDs for escalation path.
    /// </summary>
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
