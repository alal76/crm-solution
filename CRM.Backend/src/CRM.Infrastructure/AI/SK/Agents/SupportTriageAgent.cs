// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

#nullable enable

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using CRM.Core.Entities.AI;

namespace CRM.Infrastructure.AI.SK.Agents;

/// <summary>
/// AI agent specialized in triaging support requests. Classifies tickets by category
/// and priority, suggests routing and assignees, recommends relevant knowledge base
/// articles, and estimates resolution timelines.
/// </summary>
public sealed class SupportTriageAgent : CrmAgentBase
{
    #region Agent Identity

    /// <inheritdoc />
    public override string AgentName => "Support Triage Agent";

    /// <inheritdoc />
    public override AgentType AgentType => AgentType.SupportTriage;

    #endregion

    #region Configuration

    /// <inheritdoc />
    public override double Temperature => 0.2;

    /// <inheritdoc />
    public override int MaxTokens => 2048;

    /// <inheritdoc />
    public override IReadOnlyList<string> AllowedPlugins { get; } = new[]
    {
        "ServiceRequest",
        "KnowledgeBase",
        "Account",
        "Search"
    };

    #endregion

    #region System Prompt

    /// <inheritdoc />
    public override string SystemPrompt =>
        """
        You are a support triage agent that classifies, prioritizes, and routes incoming
        service requests. You MUST return a structured JSON object for every triage request.

        ## Classification Taxonomy

        ### Categories
        - Hardware:        Physical device, peripheral, or infrastructure issues
        - Software:        Application bugs, crashes, feature requests, configuration
        - Network:         Connectivity, VPN, DNS, firewall, bandwidth issues
        - Account/Billing: Login, permissions, invoicing, subscription, payment issues
        - Security:        Data breach, vulnerability, compliance, access violation
        - Other:           Anything not fitting the above categories

        ### Priority Levels
        - P1-Critical: Complete system down or data loss affecting multiple users.
                        SLA: 1-hour response, 4-hour resolution target.
        - P2-High:     Major feature broken, significant business impact, no workaround.
                        SLA: 4-hour response, 8-hour resolution target.
        - P3-Medium:   Feature impaired but workaround available, moderate impact.
                        SLA: 8-hour response, 24-hour resolution target.
        - P4-Low:      Minor issue, cosmetic defect, general inquiry.
                        SLA: 24-hour response, 72-hour resolution target.

        ### Routing Rules
        - P1-Critical → Senior Engineer (immediate escalation)
        - P2-High     → Specialist Team (domain-specific experts)
        - P3-Medium   → Standard Queue (general support team)
        - P4-Low      → Self-Service with KB article suggestions

        ## Output Format (strict JSON)
        You MUST respond with ONLY this JSON structure, no additional text:
        ```json
        {
          "category": "<Hardware|Software|Network|Account/Billing|Security|Other>",
          "priority": "<P1-Critical|P2-High|P3-Medium|P4-Low>",
          "suggestedAssignee": "<role or team name>",
          "suggestedArticles": [<array of KB article IDs if found, otherwise empty>],
          "estimatedResolutionTime": "<e.g., 4 hours, 24 hours, 3 days>",
          "reasoning": "<brief explanation of classification rationale>"
        }
        ```

        ## Rules
        - Always err on the side of higher priority when in doubt
        - Include KB article IDs only if they are genuinely relevant
        - Reasoning should explain why the category and priority were chosen
        - Consider the customer's tier and contract SLA if available
        - For security issues, always assign P1 or P2 priority
        """;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SupportTriageAgent"/> class.
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance.</param>
    /// <param name="logger">The logger instance.</param>
    public SupportTriageAgent(Kernel kernel, ILogger<SupportTriageAgent> logger)
        : base(kernel, logger)
    {
    }

    #endregion

    #region Routing

    /// <summary>
    /// Handles requests related to service requests, tickets, and support.
    /// </summary>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="intent">The optional detected intent.</param>
    /// <returns><c>true</c> if the entity type is servicerequest, ticket, or support.</returns>
    public override bool CanHandle(string entityType, string? intent)
    {
        return string.Equals(entityType, "servicerequest", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entityType, "ticket", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entityType, "support", StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Context Enrichment

    /// <summary>
    /// Enriches the triage request by searching the knowledge base for related articles
    /// and appending their summaries to the user message for improved classification.
    /// </summary>
    /// <param name="userMessage">The original support request description.</param>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="entityId">The optional service request identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The enriched message with KB context appended.</returns>
    public override async Task<string> EnrichContextAsync(
        string userMessage,
        string? entityType,
        int? entityId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug(
                "Enriching triage context for entity {EntityType}/{EntityId}.",
                entityType, entityId);

            // The orchestrator or plugin layer will handle the actual KB search.
            // Here we annotate the message so the LLM knows to consider KB articles.
            var enriched = $"""
                ## Support Request
                {userMessage}

                ## Instructions
                When triaging this request, search the knowledge base for relevant articles
                and include their IDs in the suggestedArticles array if applicable.
                """;

            return await Task.FromResult(enriched);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to enrich triage context. Proceeding with original message.");
            return userMessage;
        }
    }

    #endregion
}
