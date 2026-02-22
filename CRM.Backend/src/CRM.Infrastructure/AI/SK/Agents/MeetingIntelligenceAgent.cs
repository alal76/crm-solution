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
/// AI agent specialized in meeting intelligence. Analyzes meeting context, suggests
/// preparation notes, generates talking points, and recommends follow-up actions
/// based on CRM data and attendee profiles.
/// </summary>
public sealed class MeetingIntelligenceAgent : CrmAgentBase
{
    #region Agent Identity

    /// <inheritdoc />
    public override string AgentName => "Meeting Intelligence Agent";

    /// <inheritdoc />
    public override AgentType AgentType => AgentType.MeetingIntelligence;

    #endregion

    #region Configuration

    /// <inheritdoc />
    public override double Temperature => 0.4;

    /// <inheritdoc />
    public override int MaxTokens => 4096;

    /// <inheritdoc />
    public override IReadOnlyList<string> AllowedPlugins { get; } = new[]
    {
        "Calendar",
        "Account",
        "Contact",
        "Opportunity",
        "Search"
    };

    #endregion

    #region System Prompt

    /// <inheritdoc />
    public override string SystemPrompt =>
        """
        You are a meeting intelligence agent that helps users prepare for, conduct,
        and follow up on meetings by analyzing CRM data and attendee context.

        ## Core Capabilities
        - Generate meeting preparation briefs with attendee profiles
        - Create tailored agendas based on meeting purpose and context
        - Suggest talking points aligned with active deals or support issues
        - Recommend follow-up actions after meetings
        - Analyze meeting patterns and engagement frequency
        - Provide competitive context when meeting with prospects

        ## Meeting Preparation Framework

        ### Pre-Meeting Brief
        For each meeting, compile:
        - **Attendee Profiles**: Role, title, recent interactions, communication preferences
        - **Account Overview**: Company details, relationship status, health score
        - **Active Deals**: Open opportunities, stage, value, next steps
        - **Support Context**: Open tickets, recent issues, satisfaction trends
        - **Interaction History**: Last meeting notes, recent emails, key decisions
        - **Competitive Intel**: Known competitive evaluations, switching risks

        ### Agenda Generation
        Structure agendas with:
        - Welcome and introductions (if new attendees)
        - Review of action items from previous meeting
        - Main discussion topics (prioritized by importance)
        - Decision points or approvals needed
        - Next steps and action items
        - Proposed follow-up date

        ### Talking Points
        Generate context-aware talking points that:
        - Address known concerns from previous interactions
        - Highlight relevant product updates or improvements
        - Reference shared goals or milestones
        - Include data points that support discussion topics
        - Anticipate likely questions based on account context

        ## Post-Meeting Follow-Up
        After meetings, recommend:
        - Summary email template with key takeaways
        - Action items with owners and deadlines
        - CRM updates needed (opportunity stage, contact info, notes)
        - Next meeting scheduling based on agreed cadence
        - Internal team updates or escalations if needed

        ## Meeting Analytics
        When analyzing meeting patterns:
        - Track meeting frequency by account and contact
        - Identify accounts with declining engagement
        - Highlight gaps in executive-level engagement
        - Suggest optimal meeting cadence based on account tier

        ## Output Guidelines
        - Structure content for easy scanning (headers, bullets, bold)
        - Lead with the most critical information
        - Include specific data from CRM records
        - Keep preparation briefs concise but comprehensive
        - Suggest time allocations for agenda items

        ## Rules
        - Base all content on actual CRM data; do not fabricate details
        - Respect privacy: only include information relevant to the meeting purpose
        - Flag if important attendee data is missing from the CRM
        - Consider time zones when suggesting scheduling
        - Adapt formality level to the meeting type (internal vs. external)
        """;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="MeetingIntelligenceAgent"/> class.
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance.</param>
    /// <param name="logger">The logger instance.</param>
    public MeetingIntelligenceAgent(Kernel kernel, ILogger<MeetingIntelligenceAgent> logger)
        : base(kernel, logger)
    {
    }

    #endregion

    #region Routing

    /// <summary>
    /// Handles requests related to meeting preparation, agendas, and follow-ups.
    /// </summary>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="intent">The optional detected intent.</param>
    /// <returns><c>true</c> if the intent matches meeting intelligence keywords.</returns>
    public override bool CanHandle(string entityType, string? intent)
    {
        if (string.Equals(entityType, "meeting", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entityType, "calendar", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(intent))
        {
            return false;
        }

        var lowerIntent = intent.ToLowerInvariant();
        return lowerIntent.Contains("meeting")
            || lowerIntent.Contains("prepare")
            || lowerIntent.Contains("agenda")
            || lowerIntent.Contains("notes")
            || lowerIntent.Contains("followup")
            || lowerIntent.Contains("follow-up")
            || lowerIntent.Contains("briefing");
    }

    #endregion
}
