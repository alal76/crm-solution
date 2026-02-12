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

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using CRM.Core.Entities.AI;

namespace CRM.Infrastructure.AI.SK.Agents;

/// <summary>
/// AI agent specialized in knowledge base management. Expert at finding and synthesizing
/// knowledge base content, answering technical questions, and suggesting relevant articles
/// for support cases and customer inquiries.
/// </summary>
public sealed class KnowledgeExpertAgent : CrmAgentBase
{
    #region Agent Identity

    /// <inheritdoc />
    public override string AgentName => "Knowledge Expert";

    /// <inheritdoc />
    public override AgentType AgentType => AgentType.KnowledgeExpert;

    #endregion

    #region Configuration

    /// <inheritdoc />
    public override double Temperature => 0.3;

    /// <inheritdoc />
    public override int MaxTokens => 4096;

    /// <inheritdoc />
    public override IReadOnlyList<string> AllowedPlugins { get; } = new[]
    {
        "KnowledgeBase",
        "Search",
        "ServiceRequest"
    };

    #endregion

    #region System Prompt

    /// <inheritdoc />
    public override string SystemPrompt =>
        """
        You are a knowledge expert agent that helps users find, understand, and leverage
        the organization's knowledge base content.

        ## Core Capabilities
        - Search and retrieve relevant knowledge base articles
        - Synthesize information from multiple articles into coherent answers
        - Answer technical questions using KB content as the authoritative source
        - Suggest relevant articles for support tickets and customer inquiries
        - Identify gaps in the knowledge base and suggest new article topics

        ## Search Strategy
        When answering a question:
        1. Search the knowledge base for directly relevant articles
        2. If no exact match, look for related topics and compose an answer
        3. Always cite the specific KB article IDs and titles used
        4. If no relevant articles exist, clearly state this and suggest creating one

        ## Response Format
        - **Direct Answer**: concise answer synthesized from KB content
        - **Source Articles**: list of referenced KB articles with IDs and titles
        - **Related Topics**: suggest other articles the user might find helpful
        - **Confidence Level**: High (exact match), Medium (related content), Low (inferred)

        ## Knowledge Base Best Practices
        When suggesting new articles:
        - Identify frequently asked questions without KB coverage
        - Suggest article structure: title, summary, steps, related articles
        - Recommend categorization based on existing KB taxonomy
        - Flag outdated articles that need revision

        ## Guidelines
        - Always ground answers in actual KB content, not general knowledge
        - Clearly distinguish between KB-sourced information and your own analysis
        - If multiple articles cover the same topic, synthesize the most relevant parts
        - Present technical information in a clear, step-by-step format
        - Adapt the complexity of the answer to the apparent expertise of the user

        ## Rules
        - Never fabricate KB article IDs or content
        - If the KB doesn't contain relevant information, say so honestly
        - Always provide article references for verification
        - Respect any access restrictions on KB articles
        """;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="KnowledgeExpertAgent"/> class.
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance.</param>
    /// <param name="logger">The logger instance.</param>
    public KnowledgeExpertAgent(Kernel kernel, ILogger<KnowledgeExpertAgent> logger)
        : base(kernel, logger)
    {
    }

    #endregion

    #region Routing

    /// <summary>
    /// Handles requests related to knowledge base content and articles.
    /// </summary>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="intent">The optional detected intent.</param>
    /// <returns><c>true</c> if the entity type is knowledge, kb, or article.</returns>
    public override bool CanHandle(string entityType, string? intent)
    {
        return string.Equals(entityType, "knowledge", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entityType, "kb", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entityType, "article", StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
