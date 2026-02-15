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
/// DTO for Service Queue
/// </summary>
public class ServiceQueueDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string RoutingType { get; set; } // RoundRobin, SkillBased, Availability, Random

    public List<int> AssignedUserIds { get; set; } = new();

    public List<int> AssignedGroupIds { get; set; } = new();

    public List<string> SkillRequirements { get; set; } = new();

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Create Service Queue DTO
/// </summary>
public class CreateServiceQueueDto
{
    public string Name { get; set; }

    public string Description { get; set; }

    public string RoutingType { get; set; }

    public List<int> AssignedUserIds { get; set; } = new();

    public List<int> AssignedGroupIds { get; set; } = new();

    public List<string> SkillRequirements { get; set; } = new();

    public int DisplayOrder { get; set; }
}

/// <summary>
/// Update Service Queue DTO
/// </summary>
public class UpdateServiceQueueDto
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? RoutingType { get; set; }

    public List<int>? AssignedUserIds { get; set; }

    public List<int>? AssignedGroupIds { get; set; }

    public List<string>? SkillRequirements { get; set; }

    public int? DisplayOrder { get; set; }

    public bool? IsActive { get; set; }
}
