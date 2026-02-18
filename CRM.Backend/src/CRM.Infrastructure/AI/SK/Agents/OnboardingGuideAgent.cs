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
/// AI agent that guides new users through CRM setup and onboarding.
/// Explains features, suggests best practices, helps configure initial data,
/// and provides step-by-step tutorials for common workflows.
/// </summary>
public sealed class OnboardingGuideAgent : CrmAgentBase
{
    #region Agent Identity

    /// <inheritdoc />
    public override string AgentName => "Onboarding Guide";

    /// <inheritdoc />
    public override AgentType AgentType => AgentType.OnboardingGuide;

    #endregion

    #region Configuration

    /// <inheritdoc />
    public override double Temperature => 0.5;

    /// <inheritdoc />
    public override int MaxTokens => 4096;

    /// <inheritdoc />
    public override IReadOnlyList<string> AllowedPlugins { get; } = new[]
    {
        "Account",
        "Contact",
        "Lead",
        "Search",
        "Calendar"
    };

    #endregion

    #region System Prompt

    /// <inheritdoc />
    public override string SystemPrompt =>
        """
        You are an onboarding guide for a CRM system. Your role is to help new users
        get started, understand features, and follow best practices.

        ## Core Responsibilities
        - Guide users through initial CRM setup and configuration
        - Explain CRM concepts: accounts, contacts, leads, opportunities, pipeline
        - Provide step-by-step tutorials for common workflows
        - Suggest best practices for data organization and management
        - Help users configure their preferences and dashboard

        ## Onboarding Checklist
        Walk new users through these steps:
        1. **Profile Setup**: complete user profile, set preferences
        2. **Import Data**: help import existing contacts, accounts, or leads
        3. **Create First Account**: walk through creating an account record
        4. **Add Contacts**: demonstrate adding contacts and linking to accounts
        5. **Create Lead/Opportunity**: show how to track sales pipeline
        6. **Dashboard Tour**: explain dashboard widgets and customization
        7. **Search & Navigation**: demonstrate how to find and filter data
        8. **Calendar & Activities**: set up calendar integration and track activities

        ## Communication Style
        - Patient and encouraging, never condescending
        - Use simple, jargon-free language
        - Provide examples and analogies to explain concepts
        - Break complex tasks into small, manageable steps
        - Celebrate progress and milestones

        ## Response Format
        - For tutorials: numbered step-by-step instructions
        - For explanations: clear definitions with practical examples
        - For best practices: bullet points with reasoning
        - Always offer to help with the next step

        ## Rules
        - Adapt complexity to the user's apparent experience level
        - If a user seems stuck, offer alternative approaches
        - Never overwhelm with too much information at once
        - Suggest one next action at the end of each response
        """;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="OnboardingGuideAgent"/> class.
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance.</param>
    /// <param name="logger">The logger instance.</param>
    public OnboardingGuideAgent(Kernel kernel, ILogger<OnboardingGuideAgent> logger)
        : base(kernel, logger)
    {
    }

    #endregion

    #region Routing

    /// <summary>
    /// Handles requests related to onboarding, help, and tutorials.
    /// </summary>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="intent">The optional detected intent.</param>
    /// <returns><c>true</c> if the entity type is onboarding, help, or tutorial.</returns>
    public override bool CanHandle(string entityType, string? intent)
    {
        return string.Equals(entityType, "onboarding", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entityType, "help", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entityType, "tutorial", StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
