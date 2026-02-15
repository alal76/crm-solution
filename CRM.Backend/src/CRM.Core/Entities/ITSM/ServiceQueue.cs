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
