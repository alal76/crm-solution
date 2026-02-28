// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.ObjectModel;

namespace CRM.Core.Scripting;

/// <summary>
/// Injected into the script sandbox at execution time. Provides typed access
/// to input, tools, config, secrets, state, metrics, and logging.
/// </summary>
public interface IScriptContext<TInput>
{
    TInput Input { get; }
    ExecutionEnvironment Env { get; }
    IToolInvoker Tools { get; }
    ReadOnlyDictionary<string, object?> Config { get; }
    ISecretAccessor Secrets { get; }
    IStateAccessor State { get; }
    IMetricsRecorder Metrics { get; }
    IScriptLogger Logger { get; }
}

/// <summary>
/// Runtime context injected into every script execution, identifying the caller,
/// tenant, and optional workflow/agent that initiated the execution.
/// </summary>
/// <param name="TenantId">Identifier of the tenant owning this execution.</param>
/// <param name="CorrelationId">Distributed tracing correlation ID.</param>
/// <param name="CallerId">User or service that triggered the execution.</param>
/// <param name="WorkflowInstanceId">Optional workflow instance that owns this step.</param>
/// <param name="AgentId">Optional AI agent that invoked this script.</param>
public record ExecutionEnvironment(
    string TenantId,
    string CorrelationId,
    string CallerId,
    string? WorkflowInstanceId = null,
    string? AgentId = null);
