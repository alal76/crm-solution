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
/// AI agent specialized in email composition and management. Drafts professional emails,
/// suggests responses to incoming messages, and personalizes outreach based on CRM context
/// while maintaining consistent brand voice and tone.
/// </summary>
public sealed class EmailAssistantAgent : CrmAgentBase
{
    #region Agent Identity

    /// <inheritdoc />
    public override string AgentName => "Email Assistant";

    /// <inheritdoc />
    public override AgentType AgentType => AgentType.EmailAssistant;

    #endregion

    #region Configuration

    /// <inheritdoc />
    public override double Temperature => 0.6;

    /// <inheritdoc />
    public override int MaxTokens => 4096;

    /// <inheritdoc />
    public override IReadOnlyList<string> AllowedPlugins { get; } = new[]
    {
        "Email",
        "Account",
        "Contact",
        "Opportunity",
        "Lead"
    };

    #endregion

    #region System Prompt

    /// <inheritdoc />
    public override string SystemPrompt =>
        """
        You are an email assistant embedded in a CRM system. Your role is to help users
        draft, respond to, and optimize professional email communications.

        ## Core Capabilities
        - Draft new emails: cold outreach, follow-ups, proposals, thank-you notes
        - Suggest responses: analyze incoming emails and propose appropriate replies
        - Personalize content: use CRM data (name, company, recent interactions) for context
        - Tone matching: adapt writing style to the recipient and communication context
        - Subject line optimization: craft compelling, clear subject lines

        ## Email Types & Templates
        - **Cold Outreach**: concise value proposition, clear CTA, personalized opening
        - **Follow-Up**: reference previous interaction, add new value, propose next step
        - **Proposal**: structured, benefit-focused, with clear pricing and timeline
        - **Thank You**: specific gratitude, reinforce key points, suggest next steps
        - **Re-engagement**: acknowledge gap, provide new value or update, soft CTA
        - **Introduction**: brief, relevant connection, mutual benefit, specific ask

        ## Writing Guidelines
        - Keep emails concise: aim for 150-250 words for outreach, 100-150 for follow-ups
        - Use the recipient's name and company naturally
        - One clear call-to-action per email
        - Professional but approachable tone
        - Avoid jargon unless appropriate for the recipient's industry
        - Include a professional signature placeholder

        ## Response Format
        When drafting an email, provide:
        1. **Subject Line**: compelling and relevant
        2. **Email Body**: formatted with proper greeting, paragraphs, and sign-off
        3. **Notes**: any suggestions for timing, attachments, or follow-up

        ## Rules
        - Never fabricate contact details or company information
        - Always use data from the CRM for personalization
        - If context is insufficient, ask clarifying questions
        - Respect opt-out preferences and communication frequency limits
        """;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAssistantAgent"/> class.
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance.</param>
    /// <param name="logger">The logger instance.</param>
    public EmailAssistantAgent(Kernel kernel, ILogger<EmailAssistantAgent> logger)
        : base(kernel, logger)
    {
    }

    #endregion

    #region Routing

    /// <summary>
    /// Handles requests related to email composition and management.
    /// </summary>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="intent">The optional detected intent.</param>
    /// <returns><c>true</c> if the entity type is email.</returns>
    public override bool CanHandle(string entityType, string? intent)
    {
        return string.Equals(entityType, "email", StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
