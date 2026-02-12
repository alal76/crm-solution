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
/// Represents a key-value context variable stored for a running workflow instance.
/// </summary>
public class WorkflowContextVariable : BaseEntity
{
    /// <summary>
    /// Foreign key to the workflow instance this variable belongs to.
    /// </summary>
    public int WorkflowInstanceId { get; set; }

    /// <summary>
    /// Navigation property to the workflow instance.
    /// </summary>
    public virtual WorkflowInstance WorkflowInstance { get; set; } = null!;

    /// <summary>
    /// Variable name (unique per workflow instance).
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// JSON-encoded variable value.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// CLR type hint for deserialization (e.g., "System.String", "System.Int32").
    /// </summary>
    [MaxLength(50)]
    public string? ValueType { get; set; }

    /// <summary>
    /// Key of the workflow step that last set this variable.
    /// </summary>
    [MaxLength(200)]
    public string? SetByStepKey { get; set; }

    /// <summary>
    /// Whether the value is stored encrypted at rest.
    /// </summary>
    public bool IsEncrypted { get; set; }

    /// <summary>
    /// Whether this is a system-managed variable (not user-settable).
    /// </summary>
    public bool IsSystemVariable { get; set; }
}
