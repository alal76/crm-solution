// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

#nullable enable

using CRM.Core.Entities.AI;

namespace CRM.Infrastructure.AI.SK.Agents;

/// <summary>
/// Static helper class that provides keyword-based intent detection, entity type detection,
/// and agent recommendation logic for routing user messages to the appropriate AI agent.
/// </summary>
public static class AgentSelectionStrategy
{
    #region Intent Detection

    /// <summary>
    /// Keyword-to-intent mapping used for intent detection.
    /// Each entry maps a keyword (or phrase) to a canonical intent string.
    /// </summary>
    private static readonly (string Keyword, string Intent)[] IntentKeywords = new[]
    {
        // Lead scoring
        ("score lead", "lead"),
        ("score this lead", "lead"),
        ("qualify lead", "lead"),
        ("lead score", "lead"),
        ("bant", "lead"),

        // Email
        ("draft email", "email"),
        ("write email", "email"),
        ("compose email", "email"),
        ("email template", "email"),
        ("send email", "email"),
        ("reply to email", "email"),

        // Support / triage
        ("triage", "support"),
        ("classify ticket", "support"),
        ("support ticket", "support"),
        ("service request", "support"),
        ("route ticket", "support"),

        // Forecast
        ("forecast", "forecast"),
        ("revenue forecast", "forecast"),
        ("pipeline forecast", "forecast"),
        ("sales forecast", "forecast"),
        ("quota", "forecast"),

        // Contract
        ("contract", "contract"),
        ("renewal", "contract"),
        ("contract review", "contract"),
        ("contract terms", "contract"),

        // Knowledge base
        ("knowledge base", "knowledge"),
        ("kb article", "knowledge"),
        ("find article", "knowledge"),
        ("search kb", "knowledge"),

        // Onboarding / help
        ("getting started", "onboarding"),
        ("how do i", "onboarding"),
        ("help me", "onboarding"),
        ("tutorial", "onboarding"),
        ("onboarding", "onboarding"),
        ("new user", "onboarding"),

        // Data / analytics
        ("report", "data"),
        ("analytics", "data"),
        ("statistics", "data"),
        ("how many", "data"),
        ("total revenue", "data"),
        ("dashboard", "data"),

        // Deal intelligence
        ("deal health", "deal"),
        ("win probability", "deal"),
        ("deal risk", "deal"),
        ("deal analysis", "deal"),

        // Sales
        ("pipeline", "opportunity"),
        ("cross-sell", "opportunity"),
        ("upsell", "opportunity"),
        ("meeting prep", "opportunity"),

        // Churn / customer success
        ("churn", "churn"),
        ("retention", "churn"),
        ("customer health", "customer"),
        ("health score", "customer"),
    };

    /// <summary>
    /// Detects the user's intent from the message content using keyword matching.
    /// Returns the canonical intent string or <c>null</c> if no intent is detected.
    /// </summary>
    /// <param name="userMessage">The user's message text.</param>
    /// <returns>The detected intent string, or <c>null</c> if no match.</returns>
    public static string? DetectIntent(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            return null;
        }

        var lower = userMessage.ToLowerInvariant();

        foreach (var (keyword, intent) in IntentKeywords)
        {
            if (lower.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return intent;
            }
        }

        return null;
    }

    #endregion

    #region Entity Type Detection

    /// <summary>
    /// Keyword-to-entity-type mapping used for entity detection.
    /// </summary>
    private static readonly (string Keyword, string EntityType)[] EntityKeywords = new[]
    {
        ("lead", "lead"),
        ("opportunity", "opportunity"),
        ("deal", "deal"),
        ("account", "account"),
        ("customer", "customer"),
        ("contact", "contact"),
        ("quote", "quote"),
        ("order", "order"),
        ("invoice", "invoice"),
        ("contract", "contract"),
        ("subscription", "subscription"),
        ("ticket", "ticket"),
        ("service request", "servicerequest"),
        ("incident", "servicerequest"),
        ("knowledge", "knowledge"),
        ("article", "article"),
        ("email", "email"),
        ("campaign", "campaign"),
        ("forecast", "forecast"),
        ("report", "report"),
    };

    /// <summary>
    /// Detects entity type references within the user message using keyword matching.
    /// Returns the first matched entity type or <c>null</c> if none detected.
    /// </summary>
    /// <param name="userMessage">The user's message text.</param>
    /// <returns>The detected entity type string, or <c>null</c> if no match.</returns>
    public static string? DetectEntityType(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            return null;
        }

        var lower = userMessage.ToLowerInvariant();

        foreach (var (keyword, entityType) in EntityKeywords)
        {
            if (lower.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return entityType;
            }
        }

        return null;
    }

    #endregion

    #region Agent Recommendation

    /// <summary>
    /// Recommends the most appropriate <see cref="AgentType"/> based on the detected
    /// entity type and intent. Falls back to <see cref="AgentType.GeneralAssistant"/>
    /// when no specific agent matches.
    /// </summary>
    /// <param name="entityType">The detected entity type (may be <c>null</c>).</param>
    /// <param name="intent">The detected intent (may be <c>null</c>).</param>
    /// <returns>The recommended <see cref="AgentType"/>.</returns>
    public static AgentType RecommendAgent(string? entityType, string? intent)
    {
        // Intent takes priority over entity type
        var primarySignal = intent ?? entityType;

        if (string.IsNullOrWhiteSpace(primarySignal))
        {
            return AgentType.GeneralAssistant;
        }

        return primarySignal.ToLowerInvariant() switch
        {
            "lead" => AgentType.LeadScoring,
            "email" => AgentType.EmailAssistant,
            "support" or "ticket" or "servicerequest" => AgentType.SupportTriage,
            "forecast" => AgentType.ForecastAnalyst,
            "contract" or "renewal" => AgentType.ContractAnalyst,
            "knowledge" or "kb" or "article" => AgentType.KnowledgeExpert,
            "onboarding" or "help" or "tutorial" => AgentType.OnboardingGuide,
            "data" or "report" or "analytics" => AgentType.DataAnalyst,
            "deal" => AgentType.DealIntelligence,
            "opportunity" or "quote" => AgentType.SalesAssistant,
            "account" or "customer" or "churn" => AgentType.CustomerSuccess,
            _ => AgentType.GeneralAssistant,
        };
    }

    #endregion
}
