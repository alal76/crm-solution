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

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for escalation policy response
/// </summary>
public class EscalationPolicyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int InitialAssignmentMinutes { get; set; }
    public int MaxEscalationLevels { get; set; }
    public bool IsActive { get; set; }
    public bool NotifyDuringEscalation { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<EscalationLevelDto> Levels { get; set; } = new();
}

/// <summary>
/// DTO for creating escalation policy
/// </summary>
public class CreateEscalationPolicyDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int InitialAssignmentMinutes { get; set; } = 15;
    public int MaxEscalationLevels { get; set; } = 3;
    public bool IsActive { get; set; } = true;
    public bool NotifyDuringEscalation { get; set; } = true;
    public List<CreateEscalationLevelDto> Levels { get; set; } = new();
}

/// <summary>
/// DTO for updating escalation policy
/// </summary>
public class UpdateEscalationPolicyDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int InitialAssignmentMinutes { get; set; }
    public int MaxEscalationLevels { get; set; }
    public bool IsActive { get; set; }
    public bool NotifyDuringEscalation { get; set; }
    public List<CreateEscalationLevelDto> Levels { get; set; } = new();
}

/// <summary>
/// DTO for escalation level details
/// </summary>
public class EscalationLevelDto
{
    public int Id { get; set; }
    public int PolicyId { get; set; }
    public int Level { get; set; }
    public int EscalationAfterMinutes { get; set; }
    public int? EscalateToUserId { get; set; }
    public int? EscalateToGroupId { get; set; }
    public string? NotificationTemplate { get; set; }
    public bool SendNotification { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating escalation level
/// </summary>
public class CreateEscalationLevelDto
{
    public int Level { get; set; }
    public int EscalationAfterMinutes { get; set; }
    public int? EscalateToUserId { get; set; }
    public int? EscalateToGroupId { get; set; }
    public string? NotificationTemplate { get; set; }
    public bool SendNotification { get; set; } = true;
}

/// <summary>
/// DTO for escalation history tracking
/// </summary>
public class EscalationHistoryDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int PolicyId { get; set; }
    public int Level { get; set; }
    public DateTime EscalatedAt { get; set; }
    public int? EscalatedToUserId { get; set; }
    public int? EscalatedToGroupId { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
