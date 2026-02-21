// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Entities.ITSM;

/// <summary>
/// Service queue entity for managing support ticket queues
/// </summary>
public class ServiceQueue : BaseEntity
{
    /// <summary>Queue name (e.g., "Support", "Premium Support", "VIP")</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Queue description</summary>
    public string? Description { get; set; }

    /// <summary>Queue priority level (1 = highest, 10 = lowest)</summary>
    public int Priority { get; set; } = 5;

    /// <summary>Whether this queue is currently active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Default assignment group for this queue</summary>
    public string? AssignmentGroup { get; set; }

    /// <summary>SLA policy ID to apply to tickets in this queue</summary>
    public int? DefaultSLAPolicyId { get; set; }

    /// <summary>Maximum queue depth before warning</summary>
    public int? MaxQueueDepth { get; set; }

    /// <summary>Optional JSON for routing rules and configuration</summary>
    public string? RoutingConfiguration { get; set; }
}
