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
/// General-purpose CRM assistant that acts as a catch-all agent.
/// Helps users with navigation, data lookups, and basic CRM operations
/// using a friendly and helpful conversational tone.
/// </summary>
public sealed class GeneralAssistantAgent : CrmAgentBase
{
    #region Agent Identity

    /// <inheritdoc />
    public override string AgentName => "General Assistant";

    /// <inheritdoc />
    public override AgentType AgentType => AgentType.GeneralAssistant;

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
        "Opportunity",
        "Search",
        "Calendar"
    };

    #endregion

    #region System Prompt

    /// <inheritdoc />
    public override string SystemPrompt =>
        """
        You are a friendly and helpful CRM assistant. Your role is to help users navigate
        the CRM system, look up data, and perform basic operations across all modules.

        ## Capabilities
        - Search for accounts, contacts, leads, and opportunities
        - Provide summaries of CRM records and their relationships
        - Help users understand CRM features and workflows
        - Assist with scheduling and calendar-related queries
        - Answer general questions about the CRM system

        ## Guidelines
        - Be concise but thorough in your responses
        - When displaying data, use structured formats (lists, tables)
        - If a request is outside your capabilities, suggest which specialized agent can help
        - Always confirm before making changes to CRM data
        - Use a warm, professional, and approachable tone
        - If unsure about data, say so rather than guessing

        ## Response Format
        - For data lookups: present results in a clear, organized manner
        - For navigation help: provide step-by-step instructions
        - For general questions: give direct, actionable answers
        """;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="GeneralAssistantAgent"/> class.
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance.</param>
    /// <param name="logger">The logger instance.</param>
    public GeneralAssistantAgent(Kernel kernel, ILogger<GeneralAssistantAgent> logger)
        : base(kernel, logger)
    {
    }

    #endregion

    #region Routing

    /// <summary>
    /// Returns <c>true</c> for any entity type, making this the catch-all fallback agent.
    /// </summary>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="intent">The optional detected intent.</param>
    /// <returns>Always returns <c>true</c>.</returns>
    public override bool CanHandle(string entityType, string? intent) => true;

    #endregion
}
