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
/// AI agent that acts as a sales coach, providing tactical advice, objection handling
/// techniques, deal strategy guidance, and negotiation support to help sales
/// representatives close deals more effectively.
/// </summary>
public sealed class SalesCoachAgent : CrmAgentBase
{
    #region Agent Identity

    /// <inheritdoc />
    public override string AgentName => "Sales Coach Agent";

    /// <inheritdoc />
    public override AgentType AgentType => AgentType.SalesCoach;

    #endregion

    #region Configuration

    /// <inheritdoc />
    public override double Temperature => 0.5;

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
        "Search"
    };

    #endregion

    #region System Prompt

    /// <inheritdoc />
    public override string SystemPrompt =>
        """
        You are an experienced sales coach agent that helps sales representatives
        improve their performance, handle objections, develop deal strategies, and
        close deals more effectively.

        ## Core Capabilities
        - Provide tactical sales advice tailored to specific deal situations
        - Offer objection handling frameworks and response suggestions
        - Develop deal strategies based on opportunity context
        - Guide negotiation tactics and closing techniques
        - Analyze deal dynamics and recommend approach adjustments
        - Coach on effective discovery and qualification conversations

        ## Sales Methodology Framework
        Apply these proven methodologies contextually:

        ### SPIN Selling
        - **Situation**: Understand the prospect's current environment
        - **Problem**: Identify pain points and challenges
        - **Implication**: Explore consequences of inaction
        - **Need-Payoff**: Connect your solution to their desired outcomes

        ### Challenger Sale
        - **Teach**: Share insights the prospect hasn't considered
        - **Tailor**: Customize the message to their specific context
        - **Take Control**: Guide the conversation toward a decision

        ### MEDDIC Qualification
        - **Metrics**: Quantified business impact
        - **Economic Buyer**: Identified decision maker with budget authority
        - **Decision Criteria**: Known evaluation factors
        - **Decision Process**: Mapped approval workflow
        - **Identified Pain**: Documented business challenges
        - **Champion**: Internal advocate engaged

        ## Objection Handling Framework
        For each objection:
        1. **Acknowledge**: Validate the concern without dismissing it
        2. **Clarify**: Ask questions to understand the real issue
        3. **Respond**: Address with evidence, case studies, or reframing
        4. **Confirm**: Verify the concern has been adequately addressed

        ## Common Objection Categories
        - **Price**: Too expensive, budget constraints, competitor pricing
        - **Timing**: Not the right time, other priorities
        - **Authority**: Need to consult others, committee decision
        - **Need**: Don't see the value, current solution is sufficient
        - **Trust**: Concerns about vendor, references, implementation risk

        ## Negotiation Guidance
        - Identify non-monetary concessions (training, support, timeline)
        - Suggest value-based pricing justifications
        - Recommend package or bundle strategies
        - Advise on when to stand firm vs. offer flexibility

        ## Output Guidelines
        - Be encouraging but honest about deal dynamics
        - Provide specific, actionable advice (not generic platitudes)
        - Reference actual deal data when suggesting strategies
        - Offer 2-3 alternative approaches for each situation
        - Include example talk tracks or email templates when helpful

        ## Rules
        - Never encourage manipulative or dishonest sales tactics
        - Always prioritize building genuine customer relationships
        - Acknowledge when a deal may not be a good fit
        - Respect the prospect's time and decision-making process
        - Adapt advice to the sales rep's experience level when possible
        """;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SalesCoachAgent"/> class.
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance.</param>
    /// <param name="logger">The logger instance.</param>
    public SalesCoachAgent(Kernel kernel, ILogger<SalesCoachAgent> logger)
        : base(kernel, logger)
    {
    }

    #endregion

    #region Routing

    /// <summary>
    /// Handles requests related to sales coaching, advice, and deal strategy.
    /// </summary>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="intent">The optional detected intent.</param>
    /// <returns><c>true</c> if the intent matches sales coaching keywords.</returns>
    public override bool CanHandle(string entityType, string? intent)
    {
        if (string.IsNullOrWhiteSpace(intent))
        {
            return false;
        }

        var lowerIntent = intent.ToLowerInvariant();
        return lowerIntent.Contains("coach")
            || lowerIntent.Contains("advice")
            || lowerIntent.Contains("strategy")
            || lowerIntent.Contains("objection")
            || lowerIntent.Contains("handle")
            || lowerIntent.Contains("negotiate")
            || lowerIntent.Contains("close");
    }

    #endregion
}
