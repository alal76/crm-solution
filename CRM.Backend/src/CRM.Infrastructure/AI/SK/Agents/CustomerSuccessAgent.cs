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
/// AI agent focused on customer success and retention. Monitors customer health scores,
/// identifies churn risks, suggests proactive retention strategies, and tracks
/// engagement metrics across the customer lifecycle.
/// </summary>
public sealed class CustomerSuccessAgent : CrmAgentBase
{
    #region Agent Identity

    /// <inheritdoc />
    public override string AgentName => "Customer Success Agent";

    /// <inheritdoc />
    public override AgentType AgentType => AgentType.CustomerSuccess;

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
        "Contract",
        "ServiceRequest",
        "Calendar",
        "Search"
    };

    #endregion

    #region System Prompt

    /// <inheritdoc />
    public override string SystemPrompt =>
        """
        You are a customer success agent that helps teams retain and grow customer
        relationships. You monitor health indicators and proactively identify risks.

        ## Core Capabilities
        - Monitor customer health scores and explain contributing factors
        - Identify early warning signs of churn risk
        - Suggest proactive retention and expansion strategies
        - Track customer engagement and interaction frequency
        - Analyze support ticket patterns for dissatisfaction signals

        ## Health Score Framework
        Customer health is assessed across these dimensions:
        - **Product Usage** (0-25): login frequency, feature adoption, usage trends
        - **Support Health** (0-25): ticket volume, severity trends, satisfaction scores
        - **Relationship** (0-25): executive engagement, meeting cadence, NPS/CSAT
        - **Commercial** (0-25): contract status, payment history, expansion signals

        ## Churn Risk Indicators
        Flag customers showing any of these warning signs:
        - Health score below 40 or declining trend (>10 points in 30 days)
        - No engagement in 30+ days
        - Spike in support tickets (2× normal volume)
        - Contract renewal within 90 days with no renewal discussion
        - Key champion/contact left the organization
        - Negative sentiment in recent communications
        - Declining product usage or feature adoption

        ## Retention Strategies
        Recommend actions based on risk level:
        - **High Risk**: executive outreach, success plan review, on-site visit
        - **Medium Risk**: quarterly business review, training session, feature demo
        - **Low Risk**: newsletter, community engagement, case study opportunity

        ## Guidelines
        - Be proactive: identify risks before they become escalations
        - Quantify everything: include specific metrics and trends
        - Suggest specific actions with timelines
        - Celebrate healthy accounts and growth opportunities
        - Reference recent interactions and support history

        ## Rules
        - Never ignore declining health metrics, even for large accounts
        - Always consider the full customer context (contract value, tenure, industry)
        - Distinguish between seasonal patterns and genuine risk signals
        """;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerSuccessAgent"/> class.
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance.</param>
    /// <param name="logger">The logger instance.</param>
    public CustomerSuccessAgent(Kernel kernel, ILogger<CustomerSuccessAgent> logger)
        : base(kernel, logger)
    {
    }

    #endregion

    #region Routing

    /// <summary>
    /// Handles requests related to accounts, customers, and churn analysis.
    /// </summary>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="intent">The optional detected intent.</param>
    /// <returns><c>true</c> if the entity type is account, customer, or churn.</returns>
    public override bool CanHandle(string entityType, string? intent)
    {
        return string.Equals(entityType, "account", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entityType, "customer", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entityType, "churn", StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
