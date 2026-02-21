// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Features;

/// <summary>
/// Centralized feature flag definitions following Microsoft.FeatureManagement conventions.
/// These flags control provider selection at deployment time.
/// </summary>
public static class FeatureFlags
{
    #region Provider Selection Flags

    /// <summary>
    /// When true, uses external chat provider (Chatwoot/Intercom) instead of BuiltIn.
    /// Configuration key: FeatureManagement:UseExternalChat
    /// </summary>
    public const string UseExternalChat = "UseExternalChat";

    /// <summary>
    /// When true, uses external search provider (Meilisearch/Algolia) instead of BuiltIn.
    /// Configuration key: FeatureManagement:UseExternalSearch
    /// </summary>
    public const string UseExternalSearch = "UseExternalSearch";

    /// <summary>
    /// When true, uses external notification provider (Novu/Twilio) instead of BuiltIn.
    /// Configuration key: FeatureManagement:UseExternalNotifications
    /// </summary>
    public const string UseExternalNotifications = "UseExternalNotifications";

    /// <summary>
    /// When true, uses external analytics provider (Superset/PowerBI) instead of BuiltIn.
    /// Configuration key: FeatureManagement:UseExternalAnalytics
    /// </summary>
    public const string UseExternalAnalytics = "UseExternalAnalytics";

    /// <summary>
    /// When true, uses external e-signature provider (DocuSeal/DocuSign) instead of BuiltIn.
    /// Configuration key: FeatureManagement:UseExternalSignatures
    /// </summary>
    public const string UseExternalSignatures = "UseExternalSignatures";

    /// <summary>
    /// When true, uses external AI provider (OpenAI/Azure) instead of local Ollama.
    /// Configuration key: FeatureManagement:UseExternalAI
    /// </summary>
    public const string UseExternalAI = "UseExternalAI";

    /// <summary>
    /// When true, uses external integration platform (n8n/Zapier).
    /// Configuration key: FeatureManagement:UseExternalIntegrations
    /// </summary>
    public const string UseExternalIntegrations = "UseExternalIntegrations";

    #endregion

    #region Module Enablement Flags

    /// <summary>
    /// When true, ITSM module is enabled.
    /// </summary>
    public const string EnableITSM = "EnableITSM";

    /// <summary>
    /// When true, Marketing module is enabled.
    /// </summary>
    public const string EnableMarketing = "EnableMarketing";

    /// <summary>
    /// When true, Customer Portal is enabled.
    /// </summary>
    public const string EnableCustomerPortal = "EnableCustomerPortal";

    /// <summary>
    /// When true, Partner Portal is enabled.
    /// </summary>
    public const string EnablePartnerPortal = "EnablePartnerPortal";

    /// <summary>
    /// When true, Knowledge Base module is enabled.
    /// </summary>
    public const string EnableKnowledgeBase = "EnableKnowledgeBase";

    #endregion

    #region Feature Rollout Flags (for gradual deployments)

    /// <summary>
    /// When true, new search experience is enabled.
    /// </summary>
    public const string NewSearchExperience = "NewSearchExperience";

    /// <summary>
    /// When true, AI Assistant is enabled.
    /// </summary>
    public const string AIAssistant = "AIAssistant";

    /// <summary>
    /// When true, real-time notifications via SignalR are enabled.
    /// </summary>
    public const string RealTimeNotifications = "RealTimeNotifications";

    /// <summary>
    /// When true, advanced workflow automation is enabled.
    /// </summary>
    public const string AdvancedWorkflows = "AdvancedWorkflows";

    #endregion

    #region AI Agent Subsystem Flags (ADR-004)

    /// <summary>
    /// Master switch for the entire AI Agent subsystem (Semantic Kernel).
    /// When false, all agent features are disabled regardless of individual agent flags.
    /// </summary>
    public const string EnableAgentSubsystem = "EnableAgentSubsystem";

    // --- Per-Agent Toggles (P0) ---

    /// <summary>AI-powered lead scoring using BANT criteria, firmographics, and behavioral signals.</summary>
    public const string EnableAgent_LeadScoring = "EnableAgent_LeadScoring";

    /// <summary>Auto-classify, prioritize, and route support tickets with KB suggestions.</summary>
    public const string EnableAgent_SupportTriage = "EnableAgent_SupportTriage";

    /// <summary>Context-aware action recommendations for any CRM entity.</summary>
    public const string EnableAgent_NextBestAction = "EnableAgent_NextBestAction";

    /// <summary>Deal analysis, risk assessment, and competitive intelligence.</summary>
    public const string EnableAgent_SalesIntelligence = "EnableAgent_SalesIntelligence";

    // --- Per-Agent Toggles (P1) ---

    /// <summary>AI email drafting with tone and context awareness.</summary>
    public const string EnableAgent_EmailAssistant = "EnableAgent_EmailAssistant";

    /// <summary>Churn prediction, health scoring, and proactive outreach recommendations.</summary>
    public const string EnableAgent_CustomerSuccess = "EnableAgent_CustomerSuccess";

    /// <summary>Revenue forecasting, pipeline analytics, and quota attainment analysis.</summary>
    public const string EnableAgent_RevenueIntelligence = "EnableAgent_RevenueIntelligence";

    /// <summary>AI-assisted ticket resolution with knowledge base integration.</summary>
    public const string EnableAgent_TicketResolution = "EnableAgent_TicketResolution";

    // --- Per-Agent Toggles (P2) ---

    /// <summary>Contract analysis, quote extraction, and document summarization.</summary>
    public const string EnableAgent_DocumentIntelligence = "EnableAgent_DocumentIntelligence";

    /// <summary>Coaching insights, deal strategy guidance, and talk-track suggestions.</summary>
    public const string EnableAgent_SalesCoach = "EnableAgent_SalesCoach";

    /// <summary>Meeting transcription, action-item extraction, and follow-up generation.</summary>
    public const string EnableAgent_MeetingIntelligence = "EnableAgent_MeetingIntelligence";

    /// <summary>Call/chat analysis, sentiment trending, and objection tracking.</summary>
    public const string EnableAgent_ConversationIntelligence = "EnableAgent_ConversationIntelligence";

    // --- Infrastructure Flags ---

    /// <summary>When true, the multi-agent orchestrator routes natural-language queries to the best agent.</summary>
    public const string EnableAgentOrchestrator = "EnableAgentOrchestrator";

    /// <summary>When true, human approval workflow is enforced for write operations marked [RequiresApproval].</summary>
    public const string EnableAgentApprovalWorkflow = "EnableAgentApprovalWorkflow";

    /// <summary>When true, agent long-term memory (vector store backed) is enabled.</summary>
    public const string EnableAgentMemory = "EnableAgentMemory";

    #endregion
}
