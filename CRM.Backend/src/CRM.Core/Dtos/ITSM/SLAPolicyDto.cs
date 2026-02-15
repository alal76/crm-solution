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

namespace CRM.Core.Dtos.ITSM;

/// <summary>
/// DTO for SLA policy response
/// </summary>
public class SLAPolicyDto
{
    public int Id { get; set; }
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
    public int ServiceRequestId { get; set; }
    public int PolicyId { get; set; }
    public DateTime ResponseTargetTime { get; set; }
    public DateTime ResolutionTargetTime { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTime? ActualResponseTime { get; set; }
    public DateTime? ActualResolutionTime { get; set; }
    public bool IsBreach { get; set; }
    public DateTime? BreachTime { get; set; }
    public DateTime CreatedAt { get; set; }
}
