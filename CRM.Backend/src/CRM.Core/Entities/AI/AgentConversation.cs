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

#region Conversation Enumerations

/// <summary>
/// Represents the current status of an agent conversation.
/// </summary>
public enum ConversationStatus
{
    /// <summary>Conversation is actively in progress</summary>
    Active = 0,

    /// <summary>Conversation completed successfully</summary>
    Completed = 1,

    /// <summary>Conversation was cancelled by the user</summary>
    Cancelled = 2,

    /// <summary>Conversation failed due to an error</summary>
    Failed = 3,

    /// <summary>Conversation is paused waiting for human approval</summary>
    WaitingForApproval = 4
}

#endregion

/// <summary>
/// Represents a conversation session between a user and an AI agent,
/// including the full message history, token usage, and user feedback.
/// </summary>
public class AgentConversation : BaseEntity
{
    #region References

    /// <summary>
    /// Foreign key to the AI agent participating in this conversation.
    /// </summary>
    public int AgentId { get; set; }

    /// <summary>
    /// Navigation property to the AI agent.
    /// </summary>
    public AIAgent? Agent { get; set; }

    /// <summary>
    /// Foreign key to the user who initiated this conversation.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Navigation property to the user.
    /// </summary>
    public User? User { get; set; }

    #endregion

    #region Context

    /// <summary>
    /// Optional entity type this conversation is related to (e.g., "Lead", "Account").
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Optional entity ID this conversation is related to.
    /// </summary>
    public int? EntityId { get; set; }

    #endregion

    #region Status

    /// <summary>
    /// Current status of this conversation.
    /// </summary>
    public ConversationStatus Status { get; set; } = ConversationStatus.Active;

    #endregion

    #region Content

    /// <summary>
    /// JSON array containing the full message history for this conversation.
    /// </summary>
    public string Messages { get; set; } = string.Empty;

    /// <summary>
    /// Total number of messages exchanged in this conversation.
    /// </summary>
    public int MessageCount { get; set; } = 0;

    #endregion

    #region Metrics

    /// <summary>
    /// Total tokens consumed across all messages in this conversation.
    /// </summary>
    public int TotalTokensUsed { get; set; } = 0;

    /// <summary>
    /// Estimated cost in USD for this conversation's token usage.
    /// </summary>
    public decimal EstimatedCost { get; set; } = 0;

    #endregion

    #region Feedback

    /// <summary>
    /// Optional user rating for this conversation (1-5 scale).
    /// </summary>
    public int? UserRating { get; set; }

    /// <summary>
    /// Optional free-text feedback from the user.
    /// </summary>
    public string? UserFeedback { get; set; }

    /// <summary>
    /// Timestamp when the conversation was completed or ended.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    #endregion

    #region Navigation

    /// <summary>
    /// Actions taken by the agent during this conversation.
    /// </summary>
    public ICollection<AgentAction> Actions { get; set; } = new List<AgentAction>();

    #endregion
}
