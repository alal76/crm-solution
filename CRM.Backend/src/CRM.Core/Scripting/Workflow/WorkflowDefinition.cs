// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using System.Collections.Generic;

namespace CRM.Core.Scripting.Workflow;

/// <summary>Top-level YAML WDL (Workflow Definition Language) structure.</summary>
public class WorkflowDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string? Description { get; set; }
    public string? TriggerEvent { get; set; }
    public Dictionary<string, object> InputSchema { get; set; } = new();
    public Dictionary<string, object> OutputSchema { get; set; } = new();
    public List<WorkflowStep> Steps { get; set; } = new();
    public WorkflowRetryPolicy? RetryPolicy { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(30);
}

/// <summary>Retry policy for the workflow definition.</summary>
public class WorkflowRetryPolicy
{
    public int MaxAttempts { get; set; } = 3;
    public string BackoffType { get; set; } = "exponential"; // linear, exponential
    public int InitialDelaySeconds { get; set; } = 5;
}

/// <summary>A single step in a workflow definition.</summary>
public class WorkflowStep
{
    /// <summary>Gets or sets the step name (unique within the workflow).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the step type.</summary>
    public WorkflowStepType Type { get; set; }

    /// <summary>Gets or sets the CEL condition expression; step is skipped when false.</summary>
    public string? Condition { get; set; }

    /// <summary>Gets or sets the script ID or inline source (Script step).</summary>
    public string? Script { get; set; }

    /// <summary>Gets or sets the tool name for Tool step.</summary>
    public string? Tool { get; set; }

    /// <summary>Gets or sets the input key-value pairs; values may contain ${} expressions.</summary>
    public Dictionary<string, object> Input { get; set; } = new();

    /// <summary>Gets or sets the output binding expression.</summary>
    public string? Output { get; set; }

    /// <summary>Gets or sets the next step name; null means end.</summary>
    public string? Next { get; set; }

    /// <summary>Gets or sets the error handler step name.</summary>
    public string? OnError { get; set; }

    /// <summary>Gets or sets delay duration in seconds (Delay step).</summary>
    public int? DelaySeconds { get; set; }

    /// <summary>Gets or sets the parallel branch names (Parallel step).</summary>
    public List<string>? ParallelBranches { get; set; }

    /// <summary>Gets or sets the CEL expression for collection to iterate (Loop step).</summary>
    public string? LoopOver { get; set; }

    /// <summary>Gets or sets the loop variable name for each iteration item (Loop step).</summary>
    public string? LoopAs { get; set; }

    /// <summary>Gets or sets the child workflow ID (Subworkflow step).</summary>
    public string? SubworkflowId { get; set; }

    /// <summary>Gets or sets the per-step timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 60;
}

/// <summary>Supported step types in the YAML WDL.</summary>
public enum WorkflowStepType
{
    Script = 0,
    Tool = 1,
    Condition = 2,
    Parallel = 3,
    Loop = 4,
    Delay = 5,
    Subworkflow = 6,
    Approval = 7,
}
