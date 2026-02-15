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
/// DTO for Escalation Rule
/// </summary>
public class EscalationRuleDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Condition { get; set; } // JSON

    public string ConditionMetric { get; set; } // AgeMinutes, PriorityLevel, AssigneeGroup

    public int ThresholdValue { get; set; }

    public int? EscalateToUserId { get; set; }

    public int? EscalateToGroupId { get; set; }

    public bool SendNotification { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Create Escalation Rule DTO
/// </summary>
public class CreateEscalationRuleDto
{
    public string Name { get; set; }

    public string Description { get; set; }

    public string Condition { get; set; }

    public string ConditionMetric { get; set; }

    public int ThresholdValue { get; set; }

    public int? EscalateToUserId { get; set; }

    public int? EscalateToGroupId { get; set; }

    public bool SendNotification { get; set; } = true;
}

/// <summary>
/// Update Escalation Rule DTO
/// </summary>
public class UpdateEscalationRuleDto
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Condition { get; set; }

    public string? ConditionMetric { get; set; }

    public int? ThresholdValue { get; set; }

    public int? EscalateToUserId { get; set; }

    public int? EscalateToGroupId { get; set; }

    public bool? SendNotification { get; set; }

    public bool? IsActive { get; set; }
}
