// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

#region Discount Approval Enumerations

/// <summary>
/// FUNCTIONAL: Approval status for discount request.
/// TECHNICAL: Controls quote progression.
/// </summary>
public enum DiscountApprovalStatus
{
    /// <summary>Not submitted for approval</summary>
    NotSubmitted = 0,

    /// <summary>Pending approval</summary>
    Pending = 1,

    /// <summary>Approved</summary>
    Approved = 2,

    /// <summary>Rejected</summary>
    Rejected = 3,

    /// <summary>Recalled by submitter</summary>
    Recalled = 4,

    /// <summary>Escalated to higher level</summary>
    Escalated = 5,

    /// <summary>Auto-approved (within limits)</summary>
    AutoApproved = 6
}

/// <summary>
/// FUNCTIONAL: Type of approval threshold.
/// TECHNICAL: Determines what triggers approval.
/// </summary>
public enum ApprovalThresholdType
{
    /// <summary>Discount percentage threshold</summary>
    DiscountPercent = 0,

    /// <summary>Discount amount threshold</summary>
    DiscountAmount = 1,

    /// <summary>Margin percentage threshold</summary>
    MarginPercent = 2,

    /// <summary>Deal size threshold</summary>
    DealSize = 3,

    /// <summary>Non-standard terms</summary>
    NonStandardTerms = 4,

    /// <summary>Payment terms</summary>
    PaymentTerms = 5,

    /// <summary>Custom condition</summary>
    Custom = 6
}

#endregion

/// <summary>
/// Discount approval matrix defining approval levels and thresholds.
/// </summary>
public class DiscountApprovalMatrix : BaseEntity
{
    #region Identification

    /// <summary>Matrix name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Description</summary>
    public string? Description { get; set; }

    /// <summary>Whether matrix is active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Priority (for multiple matrices)</summary>
    public int Priority { get; set; } = 0;

    #endregion

    #region Applicability

    /// <summary>Apply to all products</summary>
    public bool AppliesToAllProducts { get; set; } = true;

    /// <summary>Product categories (comma-separated)</summary>
    public string? ProductCategories { get; set; }

    /// <summary>Customer segments (comma-separated)</summary>
    public string? CustomerSegments { get; set; }

    /// <summary>Regions (comma-separated)</summary>
    public string? Regions { get; set; }

    #endregion

    #region Settings

    /// <summary>Require all levels or just highest applicable</summary>
    public bool RequireAllLevels { get; set; } = false;

    /// <summary>Allow parallel approvals</summary>
    public bool AllowParallelApproval { get; set; } = false;

    /// <summary>Auto-escalate after hours</summary>
    public int? AutoEscalateHours { get; set; }

    /// <summary>Send reminder after hours</summary>
    public int? ReminderHours { get; set; }

    #endregion

    #region Relationships

    /// <summary>Approval levels</summary>
    public ICollection<ApprovalLevel> Levels { get; set; } = new List<ApprovalLevel>();

    #endregion
}

/// <summary>
/// Individual approval level within a matrix.
/// </summary>
public class ApprovalLevel : BaseEntity
{
    #region Identification

    /// <summary>Level name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Level order (1 = first level)</summary>
    public int LevelOrder { get; set; }

    #endregion

    #region Thresholds

    /// <summary>Threshold type</summary>
    public ApprovalThresholdType ThresholdType { get; set; } = ApprovalThresholdType.DiscountPercent;

    /// <summary>Minimum value to trigger this level</summary>
    public decimal MinValue { get; set; } = 0;

    /// <summary>Maximum value for this level (null = unlimited)</summary>
    public decimal? MaxValue { get; set; }

    #endregion

    #region Approvers

    /// <summary>Approver user ID (specific user)</summary>
    public int? ApproverUserId { get; set; }

    /// <summary>Navigation to approver user</summary>
    public User? ApproverUser { get; set; }

    /// <summary>Approver role (role-based approval)</summary>
    public string? ApproverRole { get; set; }

    /// <summary>Use manager of submitter</summary>
    public bool UseSubmitterManager { get; set; } = false;

    /// <summary>Manager levels up (1 = direct manager, 2 = manager's manager)</summary>
    public int ManagerLevelsUp { get; set; } = 1;

    /// <summary>Approval group (for group-based approval)</summary>
    public int? ApprovalGroupId { get; set; }

    /// <summary>Require all group members or any one</summary>
    public bool RequireAllGroupMembers { get; set; } = false;

    #endregion

    #region Settings

    /// <summary>Whether this level can be skipped</summary>
    public bool CanSkip { get; set; } = false;

    /// <summary>Auto-approve if approver is submitter</summary>
    public bool AutoApproveIfSelf { get; set; } = true;

    /// <summary>Timeout hours before escalation</summary>
    public int? TimeoutHours { get; set; }

    /// <summary>Escalation user ID</summary>
    public int? EscalationUserId { get; set; }

    /// <summary>Navigation to escalation user</summary>
    public User? EscalationUser { get; set; }

    #endregion

    #region Notification

    /// <summary>Send email on pending</summary>
    public bool SendEmailOnPending { get; set; } = true;

    /// <summary>Notification template ID</summary>
    public int? NotificationTemplateId { get; set; }

    /// <summary>Include quote details in email</summary>
    public bool IncludeQuoteDetails { get; set; } = true;

    #endregion

    #region Relationships

    /// <summary>Parent matrix ID</summary>
    public int DiscountApprovalMatrixId { get; set; }

    /// <summary>Navigation to matrix</summary>
    public DiscountApprovalMatrix? DiscountApprovalMatrix { get; set; }

    #endregion
}

/// <summary>
/// Group of users for group-based approvals.
/// </summary>
public class ApprovalGroup : BaseEntity
{
    /// <summary>Group name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Description</summary>
    public string? Description { get; set; }

    /// <summary>Whether group is active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Group members</summary>
    public ICollection<ApprovalGroupMember> Members { get; set; } = new List<ApprovalGroupMember>();
}

/// <summary>
/// Member of an approval group.
/// </summary>
public class ApprovalGroupMember : BaseEntity
{
    /// <summary>Group ID</summary>
    public int ApprovalGroupId { get; set; }

    /// <summary>Navigation to group</summary>
    public ApprovalGroup? ApprovalGroup { get; set; }

    /// <summary>User ID</summary>
    public int UserId { get; set; }

    /// <summary>Navigation to user</summary>
    public User? User { get; set; }

    /// <summary>Whether user is active in group</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Order for round-robin assignment</summary>
    public int Order { get; set; } = 0;
}

/// <summary>
/// Approval request for a specific quote/discount.
/// </summary>
public class ApprovalRequest : BaseEntity
{
    #region Request Details

    /// <summary>Request number</summary>
    public string RequestNumber { get; set; } = string.Empty;

    /// <summary>Current status</summary>
    public DiscountApprovalStatus Status { get; set; } = DiscountApprovalStatus.NotSubmitted;

    /// <summary>Approval matrix used</summary>
    public int? DiscountApprovalMatrixId { get; set; }

    /// <summary>Navigation to matrix</summary>
    public DiscountApprovalMatrix? DiscountApprovalMatrix { get; set; }

    #endregion

    #region Quote/Deal Details

    /// <summary>Quote ID</summary>
    public int? QuoteId { get; set; }

    /// <summary>Navigation to quote</summary>
    public Quote? Quote { get; set; }

    /// <summary>Discount percentage requested</summary>
    public decimal DiscountPercent { get; set; }

    /// <summary>Discount amount requested</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>Deal amount</summary>
    public decimal DealAmount { get; set; }

    /// <summary>Margin percentage</summary>
    public decimal? MarginPercent { get; set; }

    /// <summary>Justification for discount</summary>
    public string? Justification { get; set; }

    #endregion

    #region Workflow

    /// <summary>Current approval level</summary>
    public int CurrentLevel { get; set; } = 0;

    /// <summary>Maximum level required</summary>
    public int MaxLevelRequired { get; set; }

    /// <summary>Date submitted</summary>
    public DateTime? SubmittedAt { get; set; }

    /// <summary>Date completed (approved/rejected)</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Total time to approval (hours)</summary>
    public decimal? TimeToApprovalHours { get; set; }

    #endregion

    #region Submitter

    /// <summary>Submitter user ID</summary>
    public int SubmitterId { get; set; }

    /// <summary>Navigation to submitter</summary>
    public User? Submitter { get; set; }

    #endregion

    #region Notes

    /// <summary>Final approval/rejection notes</summary>
    public string? FinalNotes { get; set; }

    #endregion

    #region Relationships

    /// <summary>Individual approval steps</summary>
    public ICollection<ApprovalStep> Steps { get; set; } = new List<ApprovalStep>();

    #endregion
}

/// <summary>
/// Individual approval step within a request.
/// </summary>
public class ApprovalStep : BaseEntity
{
    /// <summary>Step order</summary>
    public int StepOrder { get; set; }

    /// <summary>Approval level ID</summary>
    public int? ApprovalLevelId { get; set; }

    /// <summary>Navigation to approval level</summary>
    public ApprovalLevel? ApprovalLevel { get; set; }

    /// <summary>Step status</summary>
    public DiscountApprovalStatus Status { get; set; } = DiscountApprovalStatus.Pending;

    /// <summary>Assigned approver user ID</summary>
    public int? AssignedToId { get; set; }

    /// <summary>Navigation to assigned user</summary>
    public User? AssignedTo { get; set; }

    /// <summary>Approved/rejected by user ID</summary>
    public int? ActedById { get; set; }

    /// <summary>Navigation to acting user</summary>
    public User? ActedBy { get; set; }

    /// <summary>Date assigned</summary>
    public DateTime? AssignedAt { get; set; }

    /// <summary>Date acted upon</summary>
    public DateTime? ActedAt { get; set; }

    /// <summary>Due date</summary>
    public DateTime? DueAt { get; set; }

    /// <summary>Comments/notes</summary>
    public string? Comments { get; set; }

    /// <summary>Parent request ID</summary>
    public int ApprovalRequestId { get; set; }

    /// <summary>Navigation to request</summary>
    public ApprovalRequest? ApprovalRequest { get; set; }

    /// <summary>Reminder sent</summary>
    public bool ReminderSent { get; set; } = false;

    /// <summary>Reminder sent date</summary>
    public DateTime? ReminderSentAt { get; set; }

    /// <summary>Was escalated</summary>
    public bool WasEscalated { get; set; } = false;

    /// <summary>Escalated to user ID</summary>
    public int? EscalatedToId { get; set; }

    /// <summary>Escalation date</summary>
    public DateTime? EscalatedAt { get; set; }
}
