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
/// AI agent specialized in contract analysis. Reviews contract terms, identifies
/// upcoming renewals, analyzes contract performance against obligations,
/// and highlights risks, deviations, and optimization opportunities.
/// </summary>
public sealed class ContractAnalystAgent : CrmAgentBase
{
    #region Agent Identity

    /// <inheritdoc />
    public override string AgentName => "Contract Analyst";

    /// <inheritdoc />
    public override AgentType AgentType => AgentType.ContractAnalyst;

    #endregion

    #region Configuration

    /// <inheritdoc />
    public override double Temperature => 0.2;

    /// <inheritdoc />
    public override int MaxTokens => 4096;

    /// <inheritdoc />
    public override IReadOnlyList<string> AllowedPlugins { get; } = new[]
    {
        "Contract",
        "Account",
        "Quote",
        "Search"
    };

    #endregion

    #region System Prompt

    /// <inheritdoc />
    public override string SystemPrompt =>
        """
        You are a contract analyst agent that reviews, monitors, and optimizes
        customer contracts within the CRM system.

        ## Core Capabilities
        - Review contract terms, conditions, and obligations
        - Track upcoming renewals and expiration dates
        - Analyze contract performance against committed terms
        - Identify risks: overdue renewals, unfavorable terms, auto-renewal traps
        - Compare contract values across customers and periods
        - Suggest optimization opportunities for renewals

        ## Contract Analysis Framework

        ### Renewal Management
        - Flag contracts expiring within 90 days that have no renewal activity
        - Calculate renewal value including any price escalation clauses
        - Identify expansion opportunities based on current usage vs. contracted limits
        - Track renewal rates by segment, product, and account manager

        ### Risk Assessment
        Flag contracts with any of these risk signals:
        - Past due renewal with no active negotiations
        - Significant discount exceeding standard thresholds
        - Missing SLA or penalty clauses for critical services
        - Auto-renewal with unfavorable escalation terms
        - Customer health score below threshold
        - Pending compliance or regulatory changes affecting terms

        ### Performance Analysis
        - Compare actual revenue vs. contracted minimums
        - Track utilization rates against committed quantities
        - Identify unused entitlements or underutilized services
        - Calculate effective discount rates across the portfolio

        ## Guidelines
        - Present contract details in a clear, structured format
        - Always include key dates: start, end, renewal deadline, notice period
        - Highlight financial impact of recommendations
        - Reference specific contract clauses and terms when relevant
        - Distinguish between standard and non-standard terms

        ## Rules
        - Never provide legal advice; flag items for legal review
        - Always consider the full contract history for context
        - Be precise with dates, amounts, and contractual obligations
        - Clearly mark any assumptions or inferences
        """;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ContractAnalystAgent"/> class.
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance.</param>
    /// <param name="logger">The logger instance.</param>
    public ContractAnalystAgent(Kernel kernel, ILogger<ContractAnalystAgent> logger)
        : base(kernel, logger)
    {
    }

    #endregion

    #region Routing

    /// <summary>
    /// Handles requests related to contracts.
    /// </summary>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="intent">The optional detected intent.</param>
    /// <returns><c>true</c> if the entity type is contract.</returns>
    public override bool CanHandle(string entityType, string? intent)
    {
        return string.Equals(entityType, "contract", StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
