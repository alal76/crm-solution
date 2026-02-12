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

#region Action Enumerations

/// <summary>
/// Represents the execution status of an agent action.
/// </summary>
public enum ActionStatus
{
    /// <summary>Action is pending execution or approval</summary>
    Pending = 0,

    /// <summary>Action has been approved for execution</summary>
    Approved = 1,

    /// <summary>Action was rejected by a human reviewer</summary>
    Rejected = 2,

    /// <summary>Action was successfully executed</summary>
    Executed = 3,

    /// <summary>Action execution failed</summary>
    Failed = 4,

    /// <summary>Action was cancelled before execution</summary>
    Cancelled = 5
}

/// <summary>
/// Categorizes the type of operation an agent action performs.
/// </summary>
public enum ActionType
{
    /// <summary>Read data from the CRM</summary>
    Read = 0,

    /// <summary>Write or update data in the CRM</summary>
    Write = 1,

    /// <summary>Search across CRM entities</summary>
    Search = 2,

    /// <summary>Analyze data and produce insights</summary>
    Analyze = 3,

    /// <summary>Send notifications or communications</summary>
    Notify = 4,

    /// <summary>Generate content (emails, summaries, etc.)</summary>
    Generate = 5
}

#endregion

/// <summary>
/// Represents a single action taken by an AI agent during a conversation,
/// such as invoking a Semantic Kernel plugin function.
/// </summary>
public class AgentAction : BaseEntity
{
    #region References

    /// <summary>
    /// Foreign key to the conversation this action belongs to.
    /// </summary>
    public int ConversationId { get; set; }

    /// <summary>
    /// Navigation property to the parent conversation.
    /// </summary>
    public AgentConversation? Conversation { get; set; }

    /// <summary>
    /// Foreign key to the agent that performed this action.
    /// </summary>
    public int AgentId { get; set; }

    #endregion

    #region Action Details

    /// <summary>
    /// The category of this action.
    /// </summary>
    public ActionType ActionType { get; set; }

    /// <summary>
    /// The Semantic Kernel plugin name that was invoked.
    /// </summary>
    public string PluginName { get; set; } = string.Empty;

    /// <summary>
    /// The function name within the plugin that was called.
    /// </summary>
    public string FunctionName { get; set; } = string.Empty;

    /// <summary>
    /// JSON-serialized input parameters passed to the function.
    /// </summary>
    public string? InputParameters { get; set; }

    /// <summary>
    /// JSON-serialized output result returned by the function.
    /// </summary>
    public string? OutputResult { get; set; }

    #endregion

    #region Status

    /// <summary>
    /// Current execution status of this action.
    /// </summary>
    public ActionStatus Status { get; set; } = ActionStatus.Pending;

    #endregion

    #region Approval

    /// <summary>
    /// Foreign key to an approval request, if this action required human approval.
    /// </summary>
    public int? ApprovalRequestId { get; set; }

    /// <summary>
    /// Navigation property to the approval request.
    /// </summary>
    public AgentApprovalRequest? ApprovalRequest { get; set; }

    #endregion

    #region Metrics

    /// <summary>
    /// Number of tokens consumed by this action.
    /// </summary>
    public int TokensUsed { get; set; } = 0;

    /// <summary>
    /// Wall-clock execution time in milliseconds.
    /// </summary>
    public long ExecutionTimeMs { get; set; } = 0;

    /// <summary>
    /// Error message if the action failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    #endregion
}
