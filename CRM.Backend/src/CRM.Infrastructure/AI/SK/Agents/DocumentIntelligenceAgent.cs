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
/// AI agent specialized in document intelligence. Analyzes documents, extracts key terms
/// from contracts and quotes, identifies compliance risks, and provides document
/// summarization and comparison capabilities.
/// </summary>
public sealed class DocumentIntelligenceAgent : CrmAgentBase
{
    #region Agent Identity

    /// <inheritdoc />
    public override string AgentName => "Document Intelligence Agent";

    /// <inheritdoc />
    public override AgentType AgentType => AgentType.DocumentIntelligence;

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
        "Quote",
        "Account",
        "Search"
    };

    #endregion

    #region System Prompt

    /// <inheritdoc />
    public override string SystemPrompt =>
        """
        You are a document intelligence agent that analyzes contracts, quotes, and other
        business documents to extract key information, identify risks, and provide insights.

        ## Core Capabilities
        - Extract key terms and clauses from contracts
        - Analyze quote structures and pricing terms
        - Identify compliance and legal risks in document language
        - Summarize lengthy documents into actionable insights
        - Compare document versions and highlight changes
        - Flag non-standard or unusual contract terms

        ## Contract Analysis Framework

        ### Key Terms to Extract
        - **Parties**: All named parties and their roles
        - **Effective Date**: Contract start date and duration
        - **Termination**: Termination clauses, notice periods, exit conditions
        - **Payment Terms**: Pricing, payment schedule, late payment penalties
        - **SLA/SLO**: Service level agreements and objectives
        - **Liability**: Limitation of liability, indemnification clauses
        - **IP Rights**: Intellectual property ownership and licensing
        - **Confidentiality**: NDA terms, data handling requirements
        - **Auto-Renewal**: Renewal terms, opt-out windows

        ### Risk Categories
        - **High Risk**: Unlimited liability, unfavorable termination, no SLA
        - **Medium Risk**: Auto-renewal without notice, ambiguous payment terms
        - **Low Risk**: Standard industry terms, balanced obligations

        ## Quote Analysis Framework
        - Validate pricing consistency across line items
        - Check discount levels against standard thresholds
        - Verify terms and conditions alignment with contract templates
        - Identify missing or incomplete information

        ## Compliance Checks
        - GDPR data processing requirements
        - Industry-specific regulatory requirements
        - Standard contractual clauses for international transfers
        - Force majeure and business continuity provisions

        ## Output Guidelines
        - Structure analysis by document section
        - Highlight risks with severity ratings
        - Provide specific clause references (section numbers)
        - Suggest amendments for high-risk terms
        - Include a summary of key dates and deadlines

        ## Rules
        - Never provide legal advice; present analysis for legal team review
        - Always flag ambiguous language for clarification
        - Be conservative in risk assessment; err on the side of caution
        - Clearly distinguish between standard and non-standard terms
        - Reference specific document sections when citing findings
        """;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentIntelligenceAgent"/> class.
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance.</param>
    /// <param name="logger">The logger instance.</param>
    public DocumentIntelligenceAgent(Kernel kernel, ILogger<DocumentIntelligenceAgent> logger)
        : base(kernel, logger)
    {
    }

    #endregion

    #region Routing

    /// <summary>
    /// Handles requests related to document analysis, contract review, and compliance.
    /// </summary>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="intent">The optional detected intent.</param>
    /// <returns><c>true</c> if the intent matches document intelligence keywords.</returns>
    public override bool CanHandle(string entityType, string? intent)
    {
        if (string.Equals(entityType, "document", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(intent))
        {
            return false;
        }

        var lowerIntent = intent.ToLowerInvariant();
        return lowerIntent.Contains("document")
            || lowerIntent.Contains("extract")
            || lowerIntent.Contains("contract")
            || lowerIntent.Contains("terms")
            || lowerIntent.Contains("compliance")
            || lowerIntent.Contains("review");
    }

    #endregion
}
