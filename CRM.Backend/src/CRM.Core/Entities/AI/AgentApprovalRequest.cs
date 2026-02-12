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

#region Approval Enumerations

/// <summary>
/// Represents the status of an agent action approval request.
/// </summary>
public enum ApprovalStatus
{
    /// <summary>Approval request is awaiting a decision</summary>
    Pending = 0,

    /// <summary>Action was approved by a human reviewer</summary>
    Approved = 1,

    /// <summary>Action was rejected by a human reviewer</summary>
    Rejected = 2,

    /// <summary>Approval request expired without a decision</summary>
    Expired = 3,

    /// <summary>Action was automatically approved based on policy rules</summary>
    AutoApproved = 4
}

#endregion

/// <summary>
/// Represents a human-in-the-loop approval request for an AI agent action,
/// ensuring sensitive operations require explicit authorization before execution.
/// </summary>
public class AgentApprovalRequest : BaseEntity
{
    #region References

    /// <summary>
    /// Foreign key to the agent action that requires approval.
    /// </summary>
    public int AgentActionId { get; set; }

    /// <summary>
    /// Navigation property to the agent action.
    /// </summary>
    public AgentAction? AgentAction { get; set; }

    /// <summary>
    /// Foreign key to the conversation this approval is part of.
    /// </summary>
    public int ConversationId { get; set; }

    /// <summary>
    /// Foreign key to the agent that requested the action.
    /// </summary>
    public int AgentId { get; set; }

    #endregion

    #region Users

    /// <summary>
    /// User ID of the person who triggered the action (conversation owner).
    /// </summary>
    public int RequestedByUserId { get; set; }

    /// <summary>
    /// User ID of the person who approved or rejected the request.
    /// </summary>
    public int? ApprovedByUserId { get; set; }

    #endregion

    #region Details

    /// <summary>
    /// Human-readable description of what the action will do.
    /// </summary>
    public string ActionDescription { get; set; } = string.Empty;

    /// <summary>
    /// The Semantic Kernel plugin name for the action requiring approval.
    /// </summary>
    public string PluginName { get; set; } = string.Empty;

    /// <summary>
    /// The function name within the plugin for the action requiring approval.
    /// </summary>
    public string FunctionName { get; set; } = string.Empty;

    /// <summary>
    /// JSON-serialized parameters that will be passed to the function.
    /// </summary>
    public string? Parameters { get; set; }

    #endregion

    #region Status

    /// <summary>
    /// Current status of this approval request.
    /// </summary>
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    /// <summary>
    /// The approval tier level ("low", "medium", "high") determining who can approve.
    /// </summary>
    public string ApprovalTier { get; set; } = "low";

    #endregion

    #region Resolution

    /// <summary>
    /// Reason provided when the request is rejected.
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Timestamp when the approval decision was made.
    /// </summary>
    public DateTime? DecidedAt { get; set; }

    /// <summary>
    /// Timestamp after which this approval request automatically expires.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    #endregion
}
