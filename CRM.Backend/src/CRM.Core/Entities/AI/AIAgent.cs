// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Entities.AI;

#region AI Agent Enumerations

/// <summary>
/// Defines the type of AI agent and its primary function.
/// </summary>
public enum AgentType
{
    /// <summary>Lead scoring agent</summary>
    LeadScoring = 0,

    /// <summary>Support ticket triage agent</summary>
    SupportTriage = 1,

    /// <summary>Next best action recommendation agent</summary>
    NextBestAction = 2,

    /// <summary>Sales intelligence agent</summary>
    SalesIntelligence = 3,

    /// <summary>Email assistant agent</summary>
    EmailAssistant = 4,

    /// <summary>Customer success agent</summary>
    CustomerSuccess = 5,

    /// <summary>Revenue intelligence agent</summary>
    RevenueIntelligence = 6,

    /// <summary>Ticket resolution agent</summary>
    TicketResolution = 7,

    /// <summary>Document intelligence agent</summary>
    DocumentIntelligence = 8,

    /// <summary>Sales coaching agent</summary>
    SalesCoach = 9,

    /// <summary>Meeting intelligence agent</summary>
    MeetingIntelligence = 10,

    /// <summary>Conversation intelligence agent</summary>
    ConversationIntelligence = 11,

    /// <summary>Multi-agent orchestrator</summary>
    Orchestrator = 12,

    /// <summary>General-purpose assistant (catch-all fallback)</summary>
    GeneralAssistant = 13,

    /// <summary>Sales assistant agent</summary>
    SalesAssistant = 14,

    /// <summary>Deal intelligence agent</summary>
    DealIntelligence = 15,

    /// <summary>Forecast analyst agent</summary>
    ForecastAnalyst = 16,

    /// <summary>Data analyst agent</summary>
    DataAnalyst = 17,

    /// <summary>Onboarding guide agent</summary>
    OnboardingGuide = 18,

    /// <summary>Contract analyst agent</summary>
    ContractAnalyst = 19,

    /// <summary>Knowledge expert agent</summary>
    KnowledgeExpert = 20
}

#endregion

/// <summary>
/// Represents an AI agent powered by Semantic Kernel that can perform
/// automated tasks, answer questions, and take actions within the CRM.
/// </summary>
public class AIAgent : BaseEntity
{
    #region Identification

    /// <summary>
    /// Unique internal name for the agent (e.g., "lead-scorer").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-friendly display name shown in the UI.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the agent's purpose and capabilities.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The system prompt that defines the agent's persona and behavior.
    /// </summary>
    public string SystemPrompt { get; set; } = string.Empty;

    #endregion

    #region Configuration

    /// <summary>
    /// The functional type of this agent.
    /// </summary>
    public AgentType AgentType { get; set; }

    /// <summary>
    /// Comma-separated list of Semantic Kernel plugin names the agent is allowed to invoke.
    /// </summary>
    public string AllowedPlugins { get; set; } = string.Empty;

    /// <summary>
    /// Optional JSON configuration specific to this agent type.
    /// </summary>
    public string? Configuration { get; set; }

    /// <summary>
    /// Whether this agent is currently active and available for use.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether actions taken by this agent require human approval before execution.
    /// </summary>
    public bool RequiresApproval { get; set; } = false;

    /// <summary>
    /// The approval tier for this agent's actions ("low", "medium", "high").
    /// Null if the agent does not require approval.
    /// </summary>
    public string? ApprovalTier { get; set; }

    #endregion

    #region Model Settings

    /// <summary>
    /// The temperature setting for the LLM (0.0 = deterministic, 1.0 = creative).
    /// </summary>
    public double Temperature { get; set; } = 0.3;

    /// <summary>
    /// Maximum number of tokens the model may generate per response.
    /// </summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>
    /// Optional model override (e.g., "gpt-4o") instead of the system default.
    /// </summary>
    public string? ModelOverride { get; set; }

    #endregion

    #region Metrics

    /// <summary>
    /// Total number of conversations this agent has participated in.
    /// </summary>
    public int TotalConversations { get; set; } = 0;

    /// <summary>
    /// Total number of actions this agent has executed.
    /// </summary>
    public int TotalActions { get; set; } = 0;

    /// <summary>
    /// Average user rating for this agent (1-5 scale).
    /// </summary>
    public double? AverageRating { get; set; }

    #endregion

    #region Lifecycle Hook FKs (SARCH-061)

    /// <summary>FK to ScriptPlugin executed when agent session begins.</summary>
    public int? OnActivateScriptId { get; set; }

    /// <summary>FK to ScriptPlugin executed when agent generates a plan/reasoning step.</summary>
    public int? OnPlanScriptId { get; set; }

    /// <summary>FK to ScriptPlugin executed before any tool is invoked.</summary>
    public int? OnBeforeToolCallScriptId { get; set; }

    /// <summary>FK to ScriptPlugin executed after each tool invocation.</summary>
    public int? OnAfterToolCallScriptId { get; set; }

    /// <summary>FK to ScriptPlugin executed before agent sends a response.</summary>
    public int? OnResponseScriptId { get; set; }

    /// <summary>FK to ScriptPlugin executed when agent encounters an error.</summary>
    public int? OnErrorScriptId { get; set; }

    /// <summary>FK to ScriptPlugin executed when agent session ends.</summary>
    public int? OnDeactivateScriptId { get; set; }

    /// <summary>FK to ScriptPlugin used as the safety guardrail (PII, toxicity, etc.).</summary>
    public int? GuardrailScriptId { get; set; }

    #endregion

    #region Budget Enforcement (SARCH-061)

    /// <summary>Maximum tokens allowed per individual LLM call. Null = no limit.</summary>
    public int? MaxTokensPerCall { get; set; }

    /// <summary>Maximum number of calls allowed per hour. Null = no limit.</summary>
    public int? MaxCallsPerHour { get; set; }

    /// <summary>Maximum cumulative cost (USD) allowed per calendar day. Null = no limit.</summary>
    public decimal? MaxCostPerDay { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Conversations this agent has participated in.
    /// </summary>
    public ICollection<AgentConversation> Conversations { get; set; } = new List<AgentConversation>();

    /// <summary>
    /// Long-term memories stored by this agent.
    /// </summary>
    public ICollection<AgentMemory> Memories { get; set; } = new List<AgentMemory>();

    #endregion
}
