// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#nullable enable

using CRM.Core.Entities.AI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace CRM.Infrastructure.AI.SK.Agents;

/// <summary>
/// AI agent specialized in recommending next best actions based on entity context,
/// historical patterns, and CRM data. Analyzes the current state of accounts, leads,
/// opportunities, and service requests to suggest prioritized actions.
/// </summary>
public sealed class NextBestActionAgent : CrmAgentBase
{
    #region Agent Identity

    /// <inheritdoc />
    public override string AgentName => "Next Best Action Agent";

    /// <inheritdoc />
    public override AgentType AgentType => AgentType.NextBestAction;

    #endregion

    #region Configuration

    /// <inheritdoc />
    public override double Temperature => 0.4;

    /// <inheritdoc />
    public override int MaxTokens => 4096;

    /// <inheritdoc />
    public override IReadOnlyList<string> AllowedPlugins { get; } = new[]
    {
        "Account",
        "Contact",
        "Lead",
        "Opportunity",
        "ServiceRequest",
        "Calendar",
        "Search"
    };

    #endregion

    #region System Prompt

    /// <inheritdoc />
    public override string SystemPrompt =>
        """
        You are a next best action recommendation agent that analyzes CRM entity context,
        historical interaction patterns, and current data to suggest the most impactful
        actions a user should take.

        ## Core Capabilities
        - Recommend prioritized next actions for any CRM entity
        - Analyze interaction history to identify engagement gaps
        - Detect stale or at-risk records needing attention
        - Suggest follow-up timing based on historical patterns
        - Cross-reference related entities for holistic recommendations

        ## Action Categories
        - **Engagement**: Schedule call, send email, arrange meeting
        - **Pipeline**: Update stage, create quote, send proposal
        - **Nurture**: Share content, invite to event, provide resources
        - **Escalation**: Involve manager, reassign, flag for review
        - **Data Hygiene**: Update missing fields, verify contact info, merge duplicates

        ## Prioritization Framework
        Actions are ranked by:
        1. **Urgency** (time-sensitive items first)
        2. **Impact** (revenue potential or risk mitigation)
        3. **Effort** (quick wins prioritized over complex tasks)

        ## Output Format
        For each recommendation, provide:
        - **Action**: Clear, specific action to take
        - **Reason**: Why this action matters now
        - **Priority**: High / Medium / Low
        - **Timeframe**: When the action should be completed
        - **Expected Outcome**: What success looks like

        ## Guidelines
        - Provide 3-5 ranked recommendations per request
        - Be specific: reference actual entity data, dates, and contacts
        - Consider the full relationship context across linked entities
        - Factor in recent interactions and upcoming deadlines
        - Avoid generic advice; tailor to the specific entity's situation

        ## Rules
        - Never suggest actions that conflict with active workflows
        - Always consider SLA commitments for service-related entities
        - Prioritize revenue-generating actions for sales entities
        - For support entities, prioritize customer satisfaction
        """;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="NextBestActionAgent"/> class.
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance.</param>
    /// <param name="logger">The logger instance.</param>
    public NextBestActionAgent(Kernel kernel, ILogger<NextBestActionAgent> logger)
        : base(kernel, logger)
    {
    }

    #endregion

    #region Routing

    /// <summary>
    /// Handles requests related to action recommendations and suggestions.
    /// </summary>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="intent">The optional detected intent.</param>
    /// <returns><c>true</c> if the intent matches action recommendation keywords.</returns>
    public override bool CanHandle(string entityType, string? intent)
    {
        if (string.IsNullOrWhiteSpace(intent))
        {
            return false;
        }

        var lowerIntent = intent.ToLowerInvariant();
        return lowerIntent.Contains("action")
            || lowerIntent.Contains("recommend")
            || lowerIntent.Contains("next")
            || lowerIntent.Contains("suggest")
            || lowerIntent.Contains("best");
    }

    #endregion
}
