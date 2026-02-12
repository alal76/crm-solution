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

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using CRM.Core.Entities.AI;

namespace CRM.Infrastructure.AI.SK.Agents;

/// <summary>
/// Abstract base class for all CRM AI agents.
/// Each agent defines a system prompt, a set of allowed plugins, and lifecycle hooks
/// for context enrichment and response post-processing.
/// Concrete agents (Sales, Support, Analytics, etc.) inherit from this class.
/// </summary>
public abstract class CrmAgentBase
{
    #region Fields

    /// <summary>
    /// The Semantic Kernel instance configured for this agent.
    /// </summary>
    protected readonly Kernel Kernel;

    /// <summary>
    /// Logger available to all derived agents.
    /// </summary>
    protected readonly ILogger Logger;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CrmAgentBase"/> class.
    /// </summary>
    /// <param name="kernel">A pre-configured Semantic Kernel instance.</param>
    /// <param name="logger">Logger instance for the derived agent.</param>
    protected CrmAgentBase(Kernel kernel, ILogger logger)
    {
        Kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Abstract Members

    /// <summary>
    /// Gets the agent's unique name identifier (e.g. "SalesAssistant", "SupportAgent").
    /// </summary>
    public abstract string AgentName { get; }

    /// <summary>
    /// Gets the agent type enum value for database storage and routing.
    /// </summary>
    public abstract AgentType AgentType { get; }

    /// <summary>
    /// Gets the system prompt that defines the agent's persona and behavioral boundaries.
    /// This is prepended to every conversation with the LLM.
    /// </summary>
    public abstract string SystemPrompt { get; }

    /// <summary>
    /// Gets the list of plugin names this agent is allowed to invoke.
    /// Only these plugins will be registered in the agent's kernel.
    /// </summary>
    public abstract IReadOnlyList<string> AllowedPlugins { get; }

    #endregion

    #region Virtual Members (Overridable)

    /// <summary>
    /// Gets the temperature for LLM calls (0.0 = deterministic, 1.0 = creative).
    /// Default is 0.3 for factual CRM operations.
    /// </summary>
    public virtual double Temperature => 0.3;

    /// <summary>
    /// Gets the maximum tokens per LLM response.
    /// Default is 4096 which accommodates most CRM-related answers.
    /// </summary>
    public virtual int MaxTokens => 4096;

    #endregion

    #region Lifecycle Hooks

    /// <summary>
    /// Called before the main LLM execution to enrich the conversation context.
    /// Override to inject entity-specific data (e.g. account details, opportunity pipeline)
    /// into the user message or as a separate context block.
    /// </summary>
    /// <param name="userMessage">The original user message.</param>
    /// <param name="entityType">Optional CRM entity type the conversation relates to.</param>
    /// <param name="entityId">Optional CRM entity ID for context loading.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The enriched user message (may include additional context).</returns>
    public virtual Task<string> EnrichContextAsync(
        string userMessage,
        string? entityType,
        int? entityId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(userMessage);
    }

    /// <summary>
    /// Called after the LLM response to perform any post-processing.
    /// Override to parse structured output, extract action items, update metrics, etc.
    /// </summary>
    /// <param name="agentResponse">The raw LLM response content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The post-processed response.</returns>
    public virtual Task<string> PostProcessAsync(
        string agentResponse,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(agentResponse);
    }

    /// <summary>
    /// Validates whether this agent can handle a request for the given entity type and intent.
    /// Override to restrict agents to specific domains (e.g. Sales agent only handles Opportunity/Quote).
    /// </summary>
    /// <param name="entityType">The CRM entity type (e.g. "Account", "Lead").</param>
    /// <param name="intent">Optional intent classification from the user message.</param>
    /// <returns><c>true</c> if the agent can handle the request; otherwise <c>false</c>.</returns>
    public virtual bool CanHandle(string entityType, string? intent = null)
    {
        return true;
    }

    #endregion
}
