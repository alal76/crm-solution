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
