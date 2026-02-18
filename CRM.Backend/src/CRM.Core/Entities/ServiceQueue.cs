// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// Represents a service desk queue for organizing service requests
/// </summary>
[Table("ServiceQueues")]
public class ServiceQueue : BaseEntity
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    [StringLength(1024)]
    public string Description { get; set; }

    [Required]
    public QueueRoutingType RoutingType { get; set; }

    [Column(TypeName = "longtext")]
    public string AssignedUserIds { get; set; } // JSON array of user IDs

    [Column(TypeName = "longtext")]
    public string AssignedGroupIds { get; set; } // JSON array of group IDs

    [Column(TypeName = "longtext")]
    public string SkillRequirements { get; set; } // JSON array of required skills

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Queue routing strategy
/// </summary>
public enum QueueRoutingType
{
    RoundRobin = 0,
    SkillBased = 1,
    Availability = 2,
    Random = 3
}
