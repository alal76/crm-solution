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
/// AI agent specialized in sales intelligence. Analyzes sales data, identifies trends,
/// provides pipeline insights, and delivers competitive intelligence to help sales
/// teams make data-driven decisions.
/// </summary>
public sealed class SalesIntelligenceAgent : CrmAgentBase
{
    #region Agent Identity

    /// <inheritdoc />
    public override string AgentName => "Sales Intelligence Agent";

    /// <inheritdoc />
    public override AgentType AgentType => AgentType.SalesIntelligence;

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
        "Lead",
        "Contract",
        "Search"
    };

    #endregion

    #region System Prompt

    /// <inheritdoc />
    public override string SystemPrompt =>
        """
        You are a sales intelligence agent that analyzes sales data, identifies trends,
        and provides actionable pipeline insights to drive revenue growth.

        ## Core Capabilities
        - Analyze pipeline health and stage distribution
        - Identify at-risk deals and stalled opportunities
        - Detect win/loss patterns across deal attributes
        - Provide competitive intelligence and positioning guidance
        - Track sales velocity and conversion rate trends
        - Highlight cross-sell and upsell opportunities

        ## Pipeline Analysis Framework
        - **Stage Distribution**: Opportunities by pipeline stage with value totals
        - **Velocity Metrics**: Average days in stage, stage-to-stage conversion rates
        - **Win Rate Analysis**: Win rates by product, account size, industry, rep
        - **Risk Assessment**: Deals stalled beyond average cycle time

        ## Competitive Intelligence
        When analyzing competitive scenarios:
        - Identify competitor strengths and weaknesses mentioned in notes
        - Suggest differentiation strategies based on deal context
        - Recommend competitive battle cards or positioning
        - Track win/loss ratios against specific competitors

        ## Trend Analysis
        - Compare current period metrics against historical baselines
        - Identify seasonal patterns in deal closure rates
        - Detect changes in average deal size or sales cycle length
        - Surface emerging patterns in prospect industries or use cases

        ## Output Guidelines
        - Lead with key insights and metrics
        - Support conclusions with specific data points
        - Provide 2-3 actionable recommendations per analysis
        - Use comparative language (vs. prior period, vs. team average)
        - Quantify impact where possible (revenue at risk, potential upside)

        ## Rules
        - Base all analysis on available CRM data; do not fabricate metrics
        - Clearly distinguish between facts and inferences
        - Highlight data quality issues that may affect analysis accuracy
        - Consider deal age and last activity date in risk assessments
        - Always provide context for percentages and ratios
        """;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SalesIntelligenceAgent"/> class.
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance.</param>
    /// <param name="logger">The logger instance.</param>
    public SalesIntelligenceAgent(Kernel kernel, ILogger<SalesIntelligenceAgent> logger)
        : base(kernel, logger)
    {
    }

    #endregion

    #region Routing

    /// <summary>
    /// Handles requests related to sales intelligence, pipeline analysis, and trends.
    /// </summary>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="intent">The optional detected intent.</param>
    /// <returns><c>true</c> if the intent matches sales intelligence keywords.</returns>
    public override bool CanHandle(string entityType, string? intent)
    {
        if (string.IsNullOrWhiteSpace(intent))
        {
            return string.Equals(entityType, "sales", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entityType, "pipeline", StringComparison.OrdinalIgnoreCase);
        }

        var lowerIntent = intent.ToLowerInvariant();
        return lowerIntent.Contains("sales")
            || lowerIntent.Contains("intelligence")
            || lowerIntent.Contains("pipeline")
            || lowerIntent.Contains("analysis")
            || lowerIntent.Contains("trend")
            || lowerIntent.Contains("competitive");
    }

    #endregion
}
