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
/// AI agent specialized in deal intelligence. Analyzes opportunity health,
/// identifies risks, suggests next best actions, and estimates win probability
/// based on historical deal patterns and current pipeline data.
/// </summary>
public sealed class DealIntelligenceAgent : CrmAgentBase
{
    #region Agent Identity

    /// <inheritdoc />
    public override string AgentName => "Deal Intelligence Agent";

    /// <inheritdoc />
    public override AgentType AgentType => AgentType.DealIntelligence;

    #endregion

    #region Configuration

    /// <inheritdoc />
    public override double Temperature => 0.3;

    /// <inheritdoc />
    public override int MaxTokens => 4096;

    /// <inheritdoc />
    public override IReadOnlyList<string> AllowedPlugins { get; } = new[]
    {
        "Opportunity",
        "Account",
        "Contact",
        "Quote",
        "Contract",
        "Search"
    };

    #endregion

    #region System Prompt

    /// <inheritdoc />
    public override string SystemPrompt =>
        """
        You are a deal intelligence agent that analyzes opportunity health and provides
        actionable insights to help sales teams close deals more effectively.

        ## Core Analysis Areas

        ### Deal Health Assessment
        - Activity recency: when was the last meaningful interaction?
        - Stage velocity: how long has the deal been in the current stage vs. average?
        - Stakeholder engagement: are all key decision makers identified and engaged?
        - Competitive positioning: are competitors mentioned or active in the account?
        - Champion strength: is there an internal champion driving the deal?

        ### Risk Identification
        Flag deals with any of these risk signals:
        - No activity in the last 14 days
        - Time in current stage exceeds 1.5× the average
        - Missing key decision maker contacts
        - Deal value changed significantly (>20% decrease)
        - Close date pushed more than twice
        - No next step or follow-up scheduled

        ### Win Probability Estimation
        Consider these factors when estimating probability:
        - Historical win rates for similar deal sizes and industries
        - Current stage conversion rates
        - Number of stakeholders engaged
        - Competitive landscape
        - Time in pipeline relative to average deal cycle

        ### Next Best Actions
        Always suggest 2-3 specific, actionable next steps such as:
        - Schedule a meeting with [specific stakeholder role]
        - Send a proposal or revised quote
        - Conduct a technical demonstration
        - Engage executive sponsor
        - Address specific objection or concern

        ## Guidelines
        - Ground all analysis in available CRM data
        - Be specific: reference actual dates, amounts, and contact names
        - Clearly separate facts from inferences
        - Prioritize actions by expected impact on deal outcome
        - Use percentage ranges for win probability (e.g., 60-70%)
        """;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DealIntelligenceAgent"/> class.
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance.</param>
    /// <param name="logger">The logger instance.</param>
    public DealIntelligenceAgent(Kernel kernel, ILogger<DealIntelligenceAgent> logger)
        : base(kernel, logger)
    {
    }

    #endregion

    #region Routing

    /// <summary>
    /// Handles requests related to opportunities and deals.
    /// </summary>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="intent">The optional detected intent.</param>
    /// <returns><c>true</c> if the entity type is opportunity or deal.</returns>
    public override bool CanHandle(string entityType, string? intent)
    {
        return string.Equals(entityType, "opportunity", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entityType, "deal", StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
