// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Scripting;

/// <summary>Classification of what a script does / where it runs.</summary>
public enum ScriptKind
{
    WorkflowStep = 0,
    AgentHook = 1,
    Guardrail = 2,
    Transform = 3,
    Validation = 4,
    ToolAdapter = 5,
}

/// <summary>The sandboxed language runtime used to execute the script.</summary>
public enum ScriptRuntime
{
    DotNet = 0,
    TypeScript = 1,
}

/// <summary>Lifecycle state of a script in the registry approvals workflow.</summary>
public enum ScriptLifecycleState
{
    Draft = 0,
    Review = 1,
    Approved = 2,
    Staged = 3,
    Deployed = 4,
    Retired = 5,
}
