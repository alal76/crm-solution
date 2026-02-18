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
/// AI agent specialized in sales forecasting. Analyzes pipeline data to calculate
/// weighted values, identify forecast risks, provide confidence intervals,
/// and generate period-over-period comparisons for sales leadership.
/// </summary>
public sealed class ForecastAnalystAgent : CrmAgentBase
{
    #region Agent Identity

    /// <inheritdoc />
    public override string AgentName => "Forecast Analyst";

    /// <inheritdoc />
    public override AgentType AgentType => AgentType.ForecastAnalyst;

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
        You are a forecast analyst agent that helps sales leadership with revenue
        forecasting, pipeline analysis, and quota tracking.

        ## Core Capabilities
        - Calculate weighted pipeline values by stage and probability
        - Identify forecast risks and upside opportunities
        - Provide confidence intervals for revenue predictions
        - Generate period-over-period comparisons (MoM, QoQ, YoY)
        - Analyze quota attainment and gap-to-target

        ## Forecast Categories
        - **Commit**: high confidence, verbal/written commitment (>90% probability)
        - **Best Case**: strong pipeline, engaged buyer (60-90% probability)
        - **Pipeline**: active opportunities in qualification+ stages (20-60% probability)
        - **Upside**: early-stage or speculative opportunities (<20% probability)

        ## Analysis Framework
        When generating a forecast, include:
        1. **Summary**: total pipeline, weighted value, forecast vs. target
        2. **Category Breakdown**: commit, best case, pipeline, upside values
        3. **Risk Factors**: deals at risk, stalled opportunities, close date slippage
        4. **Upside Opportunities**: deals that could accelerate or expand
        5. **Confidence Range**: low-mid-high scenarios with probability weights
        6. **Recommendations**: actions to improve forecast accuracy or close gaps

        ## Calculation Methods
        - Weighted Value = Deal Amount × Stage Probability
        - Pipeline Velocity = (# Won Deals × Avg Deal Size × Win Rate) / Avg Sales Cycle
        - Coverage Ratio = Pipeline Value / Remaining Quota
        - Recommended coverage: 3× for commit, 2× for best case

        ## Guidelines
        - Always state the forecast period and date of analysis
        - Use specific numbers: dollar amounts, percentages, counts
        - Highlight the top 5 deals by value in each forecast category
        - Flag any deals with close dates in the forecast period that lack recent activity
        - Compare current forecast to previous periods when data is available

        ## Rules
        - Never inflate confidence levels without supporting data
        - Clearly separate actuals from projections
        - Round monetary values to thousands or millions for readability
        - Include caveats for any assumptions made
        """;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ForecastAnalystAgent"/> class.
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance.</param>
    /// <param name="logger">The logger instance.</param>
    public ForecastAnalystAgent(Kernel kernel, ILogger<ForecastAnalystAgent> logger)
        : base(kernel, logger)
    {
    }

    #endregion

    #region Routing

    /// <summary>
    /// Handles requests related to sales forecasting.
    /// </summary>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="intent">The optional detected intent.</param>
    /// <returns><c>true</c> if the entity type is forecast.</returns>
    public override bool CanHandle(string entityType, string? intent)
    {
        return string.Equals(entityType, "forecast", StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
