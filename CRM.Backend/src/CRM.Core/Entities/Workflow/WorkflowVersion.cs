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

namespace CRM.Core.Entities.Workflow;

/// <summary>
/// Represents a specific version of a workflow definition
/// </summary>
public class WorkflowVersion : BaseEntity
{
    /// <summary>
    /// Foreign key to the parent workflow definition
    /// </summary>
    public int WorkflowDefinitionId { get; set; }

    /// <summary>
    /// Navigation property to the workflow definition
    /// </summary>
    public virtual WorkflowDefinition WorkflowDefinition { get; set; } = null!;

    /// <summary>
    /// Version number (incremental)
    /// </summary>
    public int VersionNumber { get; set; }

    /// <summary>
    /// Version label (e.g., "v1.0", "v2.0-beta")
    /// </summary>
    [MaxLength(50)]
    public string? Label { get; set; }

    /// <summary>
    /// Description of changes in this version
    /// </summary>
    [MaxLength(1000)]
    public string? ChangeLog { get; set; }

    /// <summary>
    /// Version status
    /// </summary>
    public WorkflowVersionStatus Status { get; set; } = WorkflowVersionStatus.Draft;

    /// <summary>
    /// Date when this version was published
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// User who published this version
    /// </summary>
    public int? PublishedById { get; set; }

    /// <summary>
    /// Navigation property to publisher
    /// </summary>
    public virtual User? PublishedBy { get; set; }

    /// <summary>
    /// Date when this version was deprecated
    /// </summary>
    public DateTime? DeprecatedAt { get; set; }

    /// <summary>
    /// Canvas layout configuration (JSON)
    /// </summary>
    public string? CanvasLayout { get; set; }

    /// <summary>
    /// Nodes in this version
    /// </summary>
    public virtual ICollection<WorkflowNode> Nodes { get; set; } = new List<WorkflowNode>();

    /// <summary>
    /// Transitions in this version
    /// </summary>
    public virtual ICollection<WorkflowTransition> Transitions { get; set; } = new List<WorkflowTransition>();
}

/// <summary>
/// Workflow version status enumeration
/// </summary>
public enum WorkflowVersionStatus
{
    Draft = 0,
    Active = 1,
    Deprecated = 2
}
