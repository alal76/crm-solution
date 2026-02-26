// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.DTOs.Workflow;

/// <summary>
/// Configuration stored inside <c>WorkflowNodes.Configuration</c> for Script-type nodes.
/// Serialized as JSON alongside other node configuration fields.
/// </summary>
public class ScriptNodeConfigDto
{
    /// <summary>The JavaScript or Python source code to execute.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Script language. 0=JavaScript (default), 1=Python, 2=CSharp.
    /// See <see cref="CRM.Core.Enums.ScriptLanguage"/>.
    /// </summary>
    public int Language { get; set; } = 0;

    /// <summary>Optional human-readable description of what this script does.</summary>
    public string? Description { get; set; }

    /// <summary>JSON object defining the expected input variable names and types.</summary>
    public string? ParameterSchema { get; set; }
}
