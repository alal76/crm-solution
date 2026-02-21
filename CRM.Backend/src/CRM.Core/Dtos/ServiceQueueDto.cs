// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
