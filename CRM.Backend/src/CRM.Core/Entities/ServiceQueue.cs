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
