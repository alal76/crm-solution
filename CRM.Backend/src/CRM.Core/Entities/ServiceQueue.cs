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
/// Represents a service desk queue for organizing service requests.
/// </summary>
[Table("ServiceQueues")]
public class ServiceQueue : BaseEntity
{
    /// <summary>
    /// Name of the service queue.
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the service queue.
    /// </summary>
    [StringLength(1024)]
    public string? Description { get; set; }

    /// <summary>
    /// Routing type for the queue.
    /// </summary>
    [Required]
    public QueueRoutingType RoutingType { get; set; }

    /// <summary>
    /// JSON array of assigned user IDs.
    /// </summary>
    [Column(TypeName = "longtext")]
    public string AssignedUserIds { get; set; } = string.Empty;

    /// <summary>
    /// JSON array of assigned group IDs.
    /// </summary>
    [Column(TypeName = "longtext")]
    public string AssignedGroupIds { get; set; } = string.Empty;
    // JSON array of required skills.
    // </summary>
    [Column(TypeName = "longtext")]
    public string SkillRequirements { get; set; } = string.Empty;

    /// <summary>
    /// Display order for the queue.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether the queue is active.
    /// </summary>
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
