// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
    Orchestrator = 12
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
