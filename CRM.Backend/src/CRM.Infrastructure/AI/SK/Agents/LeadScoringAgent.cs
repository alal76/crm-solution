// -----------------------------------------------------------------------
// CRM Solution - Semantic Kernel AI Agents
// Copyright (c) 2024-2026 Abhishek Lal (CRM Solution). All rights reserved.
// Licensed under the GNU Affero General Public License v3.0.
// See LICENSE file in the project root for full license information.
//
// This file is part of the CRM Solution, an enterprise-grade
// Customer Relationship Management system.
//
// Author: Abhishek Lal
// Repository: https://github.com/abhisheklal04/crm-solution
// Documentation: See /docs folder for architecture and API reference
//
// IMPORTANT: This is proprietary code. Unauthorized copying, modification,
// or distribution is strictly prohibited.
// -----------------------------------------------------------------------

#nullable enable

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using CRM.Core.Entities.AI;

namespace CRM.Infrastructure.AI.SK.Agents;

/// <summary>
/// AI agent specialized in scoring leads using the BANT (Budget, Authority, Need, Timeline)
/// qualification framework. Produces structured JSON output with scores, recommendations,
/// and reasoning for each dimension.
/// </summary>
public sealed class LeadScoringAgent : CrmAgentBase
{
    #region Agent Identity

    /// <inheritdoc />
    public override string AgentName => "Lead Scoring Agent";

    /// <inheritdoc />
    public override AgentType AgentType => AgentType.LeadScoring;

    #endregion

    #region Configuration

    /// <inheritdoc />
    public override double Temperature => 0.2;

    /// <inheritdoc />
    public override int MaxTokens => 2048;

    /// <inheritdoc />
    public override IReadOnlyList<string> AllowedPlugins { get; } = new[]
    {
        "Lead",
        "Account",
        "Contact",
        "Search"
    };

    #endregion

    #region System Prompt

    /// <inheritdoc />
    public override string SystemPrompt =>
        """
        You are a lead scoring agent that evaluates leads using the BANT qualification framework.
        You MUST return a structured JSON object for every scoring request.

        ## BANT Scoring Rubric

        ### Budget (0-25 points)
        - 25 = Confirmed budget allocated for this purchase
        - 20 = Budget identified but not yet formally allocated
        - 15 = Exploring budget options, willing to invest
        - 10 = Budget situation unknown or unclear
        -  0 = No budget available or not a priority

        ### Authority (0-25 points)
        - 25 = Direct decision maker with signing authority
        - 20 = Strong influencer who can champion internally
        - 15 = Has some influence in the decision process
        - 10 = End user or evaluator without decision power
        -  0 = No authority or access to decision makers

        ### Need (0-25 points)
        - 25 = Critical business pain that must be resolved
        - 20 = Clear, well-defined need with measurable impact
        - 15 = Nice-to-have improvement, not urgent
        - 10 = Exploring possibilities, no concrete need yet
        -  0 = No identifiable need for the solution

        ### Timeline (0-25 points)
        - 25 = Immediate need, purchasing within 1 month
        - 20 = Short-term, purchasing within 1-3 months
        - 15 = Medium-term, purchasing within 3-6 months
        - 10 = Long-term, purchasing within 6-12 months
        -  0 = No defined timeline or over 12 months

        ## Output Format (strict JSON)
        You MUST respond with ONLY this JSON structure, no additional text:
        ```json
        {
          "budget": <int 0-25>,
          "authority": <int 0-25>,
          "need": <int 0-25>,
          "timeline": <int 0-25>,
          "total": <int 0-100>,
          "recommendation": "<Hot|Warm|Cool|Cold>",
          "reasoning": "<brief explanation of scoring rationale>"
        }
        ```

        ## Recommendation Thresholds
        - Hot:  total >= 75 → Immediate follow-up, high conversion potential
        - Warm: total >= 50 → Nurture actively, good potential
        - Cool: total >= 25 → Long-term nurture, monitor for changes
        - Cold: total <  25 → Low priority, minimal investment

        ## Rules
        - Scores MUST be in multiples of 5 (0, 5, 10, 15, 20, 25)
        - Total MUST equal the sum of all four dimensions
        - Reasoning should be 1-3 sentences summarizing key factors
        - Base scoring on all available lead data, interactions, and context
        """;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="LeadScoringAgent"/> class.
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance.</param>
    /// <param name="logger">The logger instance.</param>
    public LeadScoringAgent(Kernel kernel, ILogger<LeadScoringAgent> logger)
        : base(kernel, logger)
    {
    }

    #endregion

    #region Routing

    /// <summary>
    /// Handles requests related to lead entities.
    /// </summary>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="intent">The optional detected intent.</param>
    /// <returns><c>true</c> if the entity type is lead.</returns>
    public override bool CanHandle(string entityType, string? intent)
    {
        return string.Equals(entityType, "lead", StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Post-Processing

    /// <summary>
    /// Validates the JSON structure and score ranges returned by the LLM.
    /// Ensures budget, authority, need, and timeline are within 0-25 range
    /// and that the total equals their sum.
    /// </summary>
    /// <param name="agentResponse">The raw LLM response.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The validated JSON response, or an error JSON if validation fails.</returns>
    public override Task<string> PostProcessAsync(
        string agentResponse,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Strip markdown code fences if present
            var json = agentResponse.Trim();
            if (json.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            {
                json = json["```json".Length..];
            }
            else if (json.StartsWith("```"))
            {
                json = json["```".Length..];
            }

            if (json.EndsWith("```"))
            {
                json = json[..^"```".Length];
            }

            json = json.Trim();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var budget = root.GetProperty("budget").GetInt32();
            var authority = root.GetProperty("authority").GetInt32();
            var need = root.GetProperty("need").GetInt32();
            var timeline = root.GetProperty("timeline").GetInt32();
            var total = root.GetProperty("total").GetInt32();

            // Validate ranges
            if (budget < 0 || budget > 25 ||
                authority < 0 || authority > 25 ||
                need < 0 || need > 25 ||
                timeline < 0 || timeline > 25)
            {
                Logger.LogWarning("Lead scoring response contained out-of-range values. Clamping.");
            }

            // Validate total
            var expectedTotal = budget + authority + need + timeline;
            if (total != expectedTotal)
            {
                Logger.LogWarning(
                    "Lead scoring total mismatch: reported {Total}, expected {Expected}. Correcting.",
                    total, expectedTotal);
            }

            return Task.FromResult(json);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to validate lead scoring JSON response.");

            var errorJson = JsonSerializer.Serialize(new
            {
                budget = 0,
                authority = 0,
                need = 0,
                timeline = 0,
                total = 0,
                recommendation = "Cold",
                reasoning = $"Scoring failed: unable to parse LLM response. Error: {ex.Message}"
            });

            return Task.FromResult(errorJson);
        }
    }

    #endregion
}
