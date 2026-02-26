// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using CRM.Core.Enums;

namespace CRM.Core.Entities.AI;

/// <summary>
/// Represents a user-authored script that can be surfaced as a Semantic Kernel plugin,
/// allowing agents and workflows to invoke custom business logic written in
/// <see cref="ScriptLanguage.JavaScript"/>, <see cref="ScriptLanguage.Python"/>, or
/// <see cref="ScriptLanguage.CSharp"/>.
/// </summary>
/// <remarks>
/// Script plugins are sandboxed at runtime. The <see cref="ParameterSchema"/> property
/// provides a JSON Schema document that describes the expected input parameters so the
/// kernel can perform argument validation before execution.
/// </remarks>
public class ScriptPlugin : BaseEntity
{
    #region Identification

    /// <summary>
    /// Human-friendly display name for the plugin, shown in the UI and agent tool listings.
    /// </summary>
    /// <example>Calculate Discount</example>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional narrative description of what the plugin does, its purpose, and any
    /// preconditions or side-effects. Surfaced to the LLM as the tool description.
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    #endregion

    #region Script Content

    /// <summary>
    /// The scripting language in which <see cref="Code"/> is written.
    /// Determines which runtime engine is used for sandboxed execution.
    /// </summary>
    public ScriptLanguage Language { get; set; } = ScriptLanguage.JavaScript;

    /// <summary>
    /// The full source code of the script. The code must expose a top-level function
    /// (or module export, depending on <see cref="Language"/>) that the runtime will invoke.
    /// </summary>
    [Required]
    [MaxLength(50000)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// A JSON Schema document (draft-07 or later) describing the parameters the script
    /// expects. Used for argument validation and for generating the Semantic Kernel
    /// function description that the planner sees.
    /// </summary>
    /// <example>
    /// {"type":"object","properties":{"discountPct":{"type":"number"}},"required":["discountPct"]}
    /// </example>
    [MaxLength(5000)]
    public string? ParameterSchema { get; set; }

    /// <summary>
    /// Plain-text description of the value returned by the script. This is appended to
    /// the Semantic Kernel tool description so the planner knows what to do with the result.
    /// </summary>
    [MaxLength(1000)]
    public string? ReturnValueDescription { get; set; }

    #endregion

    #region Lifecycle

    /// <summary>
    /// Indicates whether the plugin is available for use by agents and workflows.
    /// Inactive plugins are hidden from the Semantic Kernel tool registry.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Monotonically increasing schema version. Increment this when the
    /// <see cref="ParameterSchema"/> or <see cref="Code"/> contract changes in a
    /// breaking way so that dependent agents can be flagged for review.
    /// Defaults to <c>1</c>.
    /// </summary>
    public int Version { get; set; } = 1;

    #endregion

    #region Test Execution Metadata

    /// <summary>
    /// UTC timestamp of the most recent successful test execution of this plugin.
    /// <c>null</c> indicates the plugin has never been test-run.
    /// </summary>
    public DateTime? LastTestedAt { get; set; }

    /// <summary>
    /// JSON blob containing the outcome of the last test execution, including return
    /// value, log output, and any error details. Maximum 2 000 characters.
    /// </summary>
    [MaxLength(2000)]
    public string? LastTestResult { get; set; }

    #endregion

    #region Audit

    /// <summary>
    /// Foreign key to the <c>Users</c> table identifying who authored this plugin.
    /// <c>null</c> if created programmatically (e.g., seed data).
    /// </summary>
    public int? CreatedBy { get; set; }

    #endregion
}
