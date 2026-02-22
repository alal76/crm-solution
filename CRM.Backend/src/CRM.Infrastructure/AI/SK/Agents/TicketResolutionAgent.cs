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
/// AI agent specialized in resolving support tickets. Analyzes ticket descriptions,
/// suggests resolutions based on knowledge base articles, recommends categorization,
/// and provides step-by-step resolution guidance.
/// </summary>
public sealed class TicketResolutionAgent : CrmAgentBase
{
    #region Agent Identity

    /// <inheritdoc />
    public override string AgentName => "Ticket Resolution Agent";

    /// <inheritdoc />
    public override AgentType AgentType => AgentType.TicketResolution;

    #endregion

    #region Configuration

    /// <inheritdoc />
    public override double Temperature => 0.3;

    /// <inheritdoc />
    public override int MaxTokens => 4096;

    /// <inheritdoc />
    public override IReadOnlyList<string> AllowedPlugins { get; } = new[]
    {
        "ServiceRequest",
        "KnowledgeBase",
        "Account",
        "Contact",
        "Search"
    };

    #endregion

    #region System Prompt

    /// <inheritdoc />
    public override string SystemPrompt =>
        """
        You are a ticket resolution agent that analyzes support tickets and provides
        resolution guidance based on knowledge base articles, historical tickets, and
        technical expertise.

        ## Core Capabilities
        - Analyze ticket descriptions to identify root causes
        - Search and recommend relevant knowledge base articles
        - Provide step-by-step resolution instructions
        - Suggest categorization and tagging improvements
        - Identify patterns in recurring issues
        - Recommend workarounds when immediate fixes are unavailable

        ## Resolution Process
        1. **Understand**: Parse the ticket description and identify the core issue
        2. **Research**: Search KB articles and historical tickets for similar issues
        3. **Diagnose**: Determine the root cause based on available information
        4. **Resolve**: Provide clear, actionable resolution steps
        5. **Prevent**: Suggest preventive measures to avoid recurrence

        ## Resolution Quality Standards
        - Steps must be clear, numbered, and actionable
        - Include expected outcomes for each step
        - Provide alternative approaches when the primary solution may not work
        - Reference specific KB articles by ID when available
        - Estimate time to complete the resolution

        ## Categorization Recommendations
        When suggesting categorization:
        - **Type**: Bug, Feature Request, How-To, Configuration, Integration
        - **Component**: Identify the specific module or subsystem affected
        - **Root Cause**: Code defect, misconfiguration, user error, infrastructure

        ## Output Format
        Provide responses with these sections:
        - **Diagnosis**: Summary of the identified issue
        - **Resolution Steps**: Numbered step-by-step instructions
        - **Related KB Articles**: Article IDs and titles if applicable
        - **Workaround**: Alternative approach if main resolution is complex
        - **Prevention**: Steps to avoid recurrence

        ## Rules
        - Never suggest actions that could cause data loss without explicit warnings
        - Always recommend backing up data before destructive operations
        - Prioritize simplest effective solution first
        - Clearly distinguish between confirmed fixes and potential solutions
        - If insufficient information is available, list what data is needed
        """;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="TicketResolutionAgent"/> class.
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance.</param>
    /// <param name="logger">The logger instance.</param>
    public TicketResolutionAgent(Kernel kernel, ILogger<TicketResolutionAgent> logger)
        : base(kernel, logger)
    {
    }

    #endregion

    #region Routing

    /// <summary>
    /// Handles requests related to ticket resolution and troubleshooting.
    /// </summary>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="intent">The optional detected intent.</param>
    /// <returns><c>true</c> if the intent matches resolution keywords.</returns>
    public override bool CanHandle(string entityType, string? intent)
    {
        if (string.IsNullOrWhiteSpace(intent))
        {
            return false;
        }

        var lowerIntent = intent.ToLowerInvariant();
        return lowerIntent.Contains("resolve")
            || lowerIntent.Contains("resolution")
            || lowerIntent.Contains("fix")
            || lowerIntent.Contains("solution")
            || lowerIntent.Contains("workaround");
    }

    #endregion

    #region Context Enrichment

    /// <summary>
    /// Enriches the resolution request by annotating the message with instructions
    /// to search the knowledge base for relevant articles and historical resolutions.
    /// </summary>
    /// <param name="userMessage">The original ticket description or resolution query.</param>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="entityId">The optional service request identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The enriched message with KB lookup context appended.</returns>
    public override async Task<string> EnrichContextAsync(
        string userMessage,
        string? entityType,
        int? entityId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug(
                "Enriching resolution context for entity {EntityType}/{EntityId}.",
                entityType, entityId);

            var enriched = $"""
                ## Ticket Resolution Request
                {userMessage}

                ## Instructions
                When resolving this ticket:
                1. Search the knowledge base for relevant articles matching the issue description
                2. Check for similar historical tickets and their resolutions
                3. Include KB article IDs in your response if relevant articles are found
                4. Provide step-by-step resolution instructions
                5. Suggest a workaround if the primary resolution is complex or time-consuming
                """;

            return await Task.FromResult(enriched);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to enrich resolution context. Proceeding with original message.");
            return userMessage;
        }
    }

    #endregion
}
