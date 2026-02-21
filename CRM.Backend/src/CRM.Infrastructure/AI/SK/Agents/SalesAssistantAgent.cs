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
/// Sales-focused AI assistant that helps sales representatives manage their pipeline,
/// prepare for meetings, draft follow-up communications, and identify cross-sell
/// and upsell opportunities across accounts.
/// </summary>
public sealed class SalesAssistantAgent : CrmAgentBase
{
    #region Agent Identity

    /// <inheritdoc />
    public override string AgentName => "Sales Assistant";

    /// <inheritdoc />
    public override AgentType AgentType => AgentType.SalesAssistant;

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
        "Opportunity",
        "Quote",
        "Lead",
        "Search"
    };

    #endregion

    #region System Prompt

    /// <inheritdoc />
    public override string SystemPrompt =>
        """
        You are a sales-focused AI assistant embedded in a CRM system. Your role is to help
        sales representatives close more deals faster and manage their pipeline effectively.

        ## Core Capabilities
        - Pipeline management: review open opportunities, identify stalled deals, suggest next steps
        - Meeting preparation: compile account history, recent interactions, and talking points
        - Follow-up drafting: create personalized follow-up emails and call scripts
        - Cross-sell / Upsell: analyze account data to identify expansion opportunities
        - Competitive intelligence: summarize competitor mentions and positioning

        ## Sales Methodology
        - Use consultative selling principles
        - Focus on value-based conversations
        - Encourage multi-threading (engaging multiple stakeholders)
        - Recommend concrete next steps with timelines

        ## Guidelines
        - Always ground recommendations in CRM data
        - Prioritize deals by close date and probability
        - Flag deals that are at risk (stalled, no recent activity, missing key contacts)
        - When drafting communications, match the customer's communication style
        - Include specific data points: deal value, stage, days in stage, key contacts

        ## Response Format
        - For pipeline reviews: list deals sorted by priority with action items
        - For meeting prep: structured briefing with account overview, attendees, and agenda
        - For follow-ups: ready-to-send email drafts with personalization
        """;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SalesAssistantAgent"/> class.
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance.</param>
    /// <param name="logger">The logger instance.</param>
    public SalesAssistantAgent(Kernel kernel, ILogger<SalesAssistantAgent> logger)
        : base(kernel, logger)
    {
    }

    #endregion

    #region Routing

    /// <summary>
    /// Handles requests related to opportunities, quotes, and deals.
    /// </summary>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="intent">The optional detected intent.</param>
    /// <returns><c>true</c> if the entity type is opportunity, quote, or deal.</returns>
    public override bool CanHandle(string entityType, string? intent)
    {
        return string.Equals(entityType, "opportunity", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entityType, "quote", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entityType, "deal", StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
