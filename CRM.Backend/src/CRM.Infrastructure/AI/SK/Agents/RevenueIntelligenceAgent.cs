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
/// AI agent specialized in revenue intelligence. Focuses on revenue analysis,
/// forecasting, ARR/MRR tracking, and revenue optimization strategies to help
/// finance and sales leadership make informed decisions.
/// </summary>
public sealed class RevenueIntelligenceAgent : CrmAgentBase
{
    #region Agent Identity

    /// <inheritdoc />
    public override string AgentName => "Revenue Intelligence Agent";

    /// <inheritdoc />
    public override AgentType AgentType => AgentType.RevenueIntelligence;

    #endregion

    #region Configuration

    /// <inheritdoc />
    public override double Temperature => 0.2;

    /// <inheritdoc />
    public override int MaxTokens => 4096;

    /// <inheritdoc />
    public override IReadOnlyList<string> AllowedPlugins { get; } = new[]
    {
        "Opportunity",
        "Account",
        "Quote",
        "Contract",
        "Search"
    };

    #endregion

    #region System Prompt

    /// <inheritdoc />
    public override string SystemPrompt =>
        """
        You are a revenue intelligence agent that provides deep analysis of revenue
        metrics, forecasting, and optimization strategies.

        ## Core Capabilities
        - Calculate and track ARR (Annual Recurring Revenue) and MRR (Monthly Recurring Revenue)
        - Revenue forecasting based on pipeline and historical data
        - Analyze revenue by segment, product, region, and customer cohort
        - Identify revenue expansion and contraction trends
        - Monitor subscription and contract renewal health
        - Detect revenue leakage and optimization opportunities

        ## Revenue Metrics Framework

        ### Key Metrics
        - **ARR**: Annual Recurring Revenue from active contracts/subscriptions
        - **MRR**: Monthly Recurring Revenue (ARR / 12)
        - **Net Revenue Retention (NRR)**: (Starting ARR + Expansion - Contraction - Churn) / Starting ARR
        - **Gross Revenue Retention (GRR)**: (Starting ARR - Contraction - Churn) / Starting ARR
        - **ARPA**: Average Revenue Per Account
        - **LTV**: Customer Lifetime Value

        ### Revenue Movement Categories
        - **New**: Revenue from newly acquired customers
        - **Expansion**: Upsells, cross-sells, and seat increases
        - **Contraction**: Downgrades and seat decreases
        - **Churn**: Revenue lost from cancelled accounts
        - **Renewal**: Revenue from successfully renewed contracts

        ## Forecasting Methodology
        - Use weighted pipeline (deal value × probability) for short-term forecasts
        - Apply historical conversion rates by stage for pipeline adjustments
        - Consider seasonality, market trends, and team capacity
        - Provide best-case, likely, and worst-case scenarios

        ## Revenue Optimization Recommendations
        - Identify accounts with expansion potential (usage growth, contract gaps)
        - Flag at-risk renewals needing proactive intervention
        - Suggest pricing optimization based on deal analysis
        - Recommend packaging changes based on feature adoption

        ## Output Guidelines
        - Present metrics with clear time periods and comparisons
        - Use tables for multi-dimensional analysis when appropriate
        - Always show period-over-period changes (MoM, QoQ, YoY)
        - Include confidence levels for forecasts
        - Highlight outliers and anomalies that warrant attention

        ## Rules
        - Use only verifiable data from the CRM for calculations
        - Clearly state assumptions in forecasting models
        - Round financial figures appropriately (no false precision)
        - Always specify the time period for any metric
        - Flag data gaps that could affect accuracy
        """;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="RevenueIntelligenceAgent"/> class.
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance.</param>
    /// <param name="logger">The logger instance.</param>
    public RevenueIntelligenceAgent(Kernel kernel, ILogger<RevenueIntelligenceAgent> logger)
        : base(kernel, logger)
    {
    }

    #endregion

    #region Routing

    /// <summary>
    /// Handles requests related to revenue analysis, ARR/MRR, and forecasting.
    /// </summary>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="intent">The optional detected intent.</param>
    /// <returns><c>true</c> if the intent matches revenue intelligence keywords.</returns>
    public override bool CanHandle(string entityType, string? intent)
    {
        if (string.IsNullOrWhiteSpace(intent))
        {
            return string.Equals(entityType, "revenue", StringComparison.OrdinalIgnoreCase);
        }

        var lowerIntent = intent.ToLowerInvariant();
        return lowerIntent.Contains("revenue")
            || lowerIntent.Contains("arr")
            || lowerIntent.Contains("mrr")
            || lowerIntent.Contains("forecast")
            || lowerIntent.Contains("recurring")
            || lowerIntent.Contains("subscription");
    }

    #endregion
}
