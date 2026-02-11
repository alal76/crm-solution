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
/// Represents a transition (connection) between workflow nodes
/// </summary>
public class WorkflowTransition : BaseEntity
{
    /// <summary>
    /// Foreign key to the workflow version
    /// </summary>
    public int WorkflowVersionId { get; set; }

    /// <summary>
    /// Navigation property to the workflow version
    /// </summary>
    public virtual WorkflowVersion WorkflowVersion { get; set; } = null!;

    /// <summary>
    /// Source node ID
    /// </summary>
    public int SourceNodeId { get; set; }

    /// <summary>
    /// Navigation property to source node
    /// </summary>
    public virtual WorkflowNode SourceNode { get; set; } = null!;

    /// <summary>
    /// Target node ID
    /// </summary>
    public int TargetNodeId { get; set; }

    /// <summary>
    /// Navigation property to target node
    /// </summary>
    public virtual WorkflowNode TargetNode { get; set; } = null!;

    /// <summary>
    /// Unique key for this transition
    /// </summary>
    [MaxLength(100)]
    public string? TransitionKey { get; set; }

    /// <summary>
    /// Label displayed on the transition
    /// </summary>
    [MaxLength(100)]
    public string? Label { get; set; }

    /// <summary>
    /// Description of the transition
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Condition type for the transition
    /// </summary>
    public TransitionConditionType ConditionType { get; set; } = TransitionConditionType.Always;

    /// <summary>
    /// Condition expression (JSON or expression language)
    /// </summary>
    public string? ConditionExpression { get; set; }

    /// <summary>
    /// Whether this is the default transition from a condition node
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Priority for transition evaluation (lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// Source handle position (top, right, bottom, left)
    /// </summary>
    [MaxLength(20)]
    public string SourceHandle { get; set; } = "right";

    /// <summary>
    /// Target handle position (top, right, bottom, left)
    /// </summary>
    [MaxLength(20)]
    public string TargetHandle { get; set; } = "left";

    /// <summary>
    /// Line style (solid, dashed, dotted)
    /// </summary>
    [MaxLength(20)]
    public string LineStyle { get; set; } = "solid";

    /// <summary>
    /// Line color
    /// </summary>
    [MaxLength(20)]
    public string Color { get; set; } = "#888888";

    /// <summary>
    /// Animation style (none, flow, pulse)
    /// </summary>
    [MaxLength(20)]
    public string AnimationStyle { get; set; } = "none";
}

/// <summary>
/// Transition condition type enumeration
/// </summary>
public enum TransitionConditionType
{
    /// <summary>
    /// Always follow this transition
    /// </summary>
    Always = 0,

    /// <summary>
    /// Follow when expression evaluates to true
    /// </summary>
    Expression = 1,

    /// <summary>
    /// Follow when field matches a value
    /// </summary>
    FieldMatch = 2,

    /// <summary>
    /// Follow when any condition is met
    /// </summary>
    Any = 3,

    /// <summary>
    /// Follow when all conditions are met
    /// </summary>
    All = 4,

    /// <summary>
    /// Follow when user selects this option
    /// </summary>
    UserChoice = 5
}
