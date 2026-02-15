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

using System.ComponentModel.DataAnnotations.Schema;
using CRM.Core.Models;

namespace CRM.Core.Entities;

#region Email Sequence Enumerations

/// <summary>
/// FUNCTIONAL: Sequence status lifecycle.
/// TECHNICAL: Controls whether sequences are sent.
/// </summary>
public enum EmailSequenceStatus
{
    /// <summary>Sequence is in draft mode</summary>
    Draft = 0,

    /// <summary>Sequence is active and enrolling</summary>
    Active = 1,

    /// <summary>Sequence is paused</summary>
    Paused = 2,

    /// <summary>Sequence is archived</summary>
    Archived = 3
}

/// <summary>
/// FUNCTIONAL: Type of email step in sequence.
/// TECHNICAL: Determines content and action.
/// </summary>
public enum EmailStepType
{
    /// <summary>Automated email send</summary>
    Email = 0,

    /// <summary>Wait/delay step</summary>
    Wait = 1,

    /// <summary>Task creation for manual follow-up</summary>
    Task = 2,

    /// <summary>Condition/branch step</summary>
    Condition = 3,

    /// <summary>LinkedIn action (view profile, connect)</summary>
    LinkedIn = 4,

    /// <summary>Phone call reminder</summary>
    Call = 5,

    /// <summary>SMS/text message</summary>
    SMS = 6,

    /// <summary>Internal notification</summary>
    Notification = 7
}

/// <summary>
/// FUNCTIONAL: Timing mode for step execution.
/// TECHNICAL: Determines when step fires.
/// </summary>
public enum StepTimingMode
{
    /// <summary>Delay from previous step</summary>
    Delay = 0,

    /// <summary>Specific time of day</summary>
    SpecificTime = 1,

    /// <summary>Business hours only</summary>
    BusinessHours = 2,

    /// <summary>Recipient timezone</summary>
    RecipientTimezone = 3
}

/// <summary>
/// FUNCTIONAL: Enrollment status for a recipient.
/// TECHNICAL: Controls email sending and tracking.
/// </summary>
public enum EnrollmentStatus
{
    /// <summary>Actively receiving sequence</summary>
    Active = 0,

    /// <summary>Paused by user</summary>
    Paused = 1,

    /// <summary>Completed all steps</summary>
    Completed = 2,

    /// <summary>Unsubscribed/opted out</summary>
    Unsubscribed = 3,

    /// <summary>Bounced email</summary>
    Bounced = 4,

    /// <summary>Replied (auto-exit)</summary>
    Replied = 5,

    /// <summary>Meeting booked (goal achieved)</summary>
    MeetingBooked = 6,

    /// <summary>Converted to opportunity</summary>
    Converted = 7,

    /// <summary>Manually removed</summary>
    Removed = 8,

    /// <summary>Error occurred</summary>
    Error = 9
}

/// <summary>
/// FUNCTIONAL: Exit condition triggers.
/// TECHNICAL: Determines automatic unenrollment.
/// </summary>
public enum SequenceExitCondition
{
    /// <summary>No auto-exit</summary>
    None = 0,

    /// <summary>Exit on reply</summary>
    OnReply = 1,

    /// <summary>Exit on meeting booked</summary>
    OnMeetingBooked = 2,

    /// <summary>Exit on opportunity created</summary>
    OnOpportunityCreated = 3,

    /// <summary>Exit on link click</summary>
    OnLinkClick = 4,

    /// <summary>Exit on unsubscribe</summary>
    OnUnsubscribe = 5,

    /// <summary>Exit on bounce</summary>
    OnBounce = 6,

    /// <summary>Exit on status change</summary>
    OnStatusChange = 7
}

#endregion

/// <summary>
/// Email sequence (drip campaign) for automated outreach.
/// Contains ordered steps with emails, delays, and actions.
/// </summary>
public class EmailSequence : BaseEntity
{
    #region Identification

    /// <summary>Sequence name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Sequence description</summary>
    public string? Description { get; set; }

    /// <summary>Current status</summary>
    public EmailSequenceStatus Status { get; set; } = EmailSequenceStatus.Draft;

    /// <summary>Whether this sequence is active and can enroll new recipients</summary>
    public bool IsActive { get; set; } = true;

    #endregion

    #region Configuration

    /// <summary>From email address</summary>
    public string? FromEmail { get; set; }

    /// <summary>From name</summary>
    public string? FromName { get; set; }

    /// <summary>Reply-to email address</summary>
    public string? ReplyToEmail { get; set; }

    /// <summary>Sender user ID (for individual sending)</summary>
    public int? SenderId { get; set; }

    /// <summary>Navigation to sender</summary>
    public User? Sender { get; set; }

    /// <summary>Whether to send from each rep's email</summary>
    public bool SendFromOwner { get; set; } = true;

    /// <summary>Default timezone for sending</summary>
    public string Timezone { get; set; } = "America/New_York";

    #endregion

    #region Sending Schedule

    /// <summary>Days to send (0=Sun, 1=Mon, etc.)</summary>
    public string? SendingDays { get; set; } // JSON array [1,2,3,4,5]

    /// <summary>Start hour for sending (0-23)</summary>
    public int SendingStartHour { get; set; } = 9;

    /// <summary>End hour for sending (0-23)</summary>
    public int SendingEndHour { get; set; } = 17;

    /// <summary>Maximum emails per day per recipient</summary>
    public int? MaxEmailsPerDay { get; set; }

    /// <summary>Throttle between sends (minutes)</summary>
    public int? ThrottleMinutes { get; set; }

    #endregion

    #region Exit Conditions

    /// <summary>Exit conditions (JSON array)</summary>
    public string? ExitConditions { get; set; }

    /// <summary>Exit on reply</summary>
    public bool ExitOnReply { get; set; } = true;

    /// <summary>Exit on meeting booked</summary>
    public bool ExitOnMeetingBooked { get; set; } = true;

    /// <summary>Exit on bounce</summary>
    public bool ExitOnBounce { get; set; } = true;

    /// <summary>Exit on unsubscribe</summary>
    public bool ExitOnUnsubscribe { get; set; } = true;

    #endregion

    #region Statistics

    /// <summary>Total enrolled</summary>
    public int TotalEnrolled { get; set; } = 0;

    /// <summary>Currently active</summary>
    public int ActiveEnrollments { get; set; } = 0;

    /// <summary>Completed sequence</summary>
    public int TotalCompleted { get; set; } = 0;

    /// <summary>Total emails sent</summary>
    public int TotalEmailsSent { get; set; } = 0;

    /// <summary>Total opens</summary>
    public int TotalOpens { get; set; } = 0;

    /// <summary>Total clicks</summary>
    public int TotalClicks { get; set; } = 0;

    /// <summary>Total replies</summary>
    public int TotalReplies { get; set; } = 0;

    /// <summary>Total bounces</summary>
    public int TotalBounces { get; set; } = 0;

    /// <summary>Total unsubscribes</summary>
    public int TotalUnsubscribes { get; set; } = 0;

    /// <summary>Total meetings booked</summary>
    public int TotalMeetingsBooked { get; set; } = 0;

    #endregion

    #region Relationships

    /// <summary>Owning user ID</summary>
    public int? OwnerId { get; set; }

    /// <summary>Navigation to owner</summary>
    public User? Owner { get; set; }

    /// <summary>Sequence steps</summary>
    public ICollection<EmailSequenceStep> Steps { get; set; } = new List<EmailSequenceStep>();

    /// <summary>Enrollments</summary>
    public ICollection<EmailSequenceEnrollment> Enrollments { get; set; } = new List<EmailSequenceEnrollment>();

    #endregion
}

/// <summary>
/// Individual step within an email sequence.
/// </summary>
public class EmailSequenceStep : BaseEntity
{
    #region Identification

    /// <summary>Step order in sequence</summary>
    public int StepOrder { get; set; }

    /// <summary>Alias for step order - Order property</summary>
    [NotMapped]
    public int? Order
    {
        get => StepOrder;
        set => StepOrder = value ?? 0;
    }

    /// <summary>Step name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Step type</summary>
    public EmailStepType StepType { get; set; } = EmailStepType.Email;

    /// <summary>Whether step is active</summary>
    public bool IsActive { get; set; } = true;

    #endregion

    #region Email Template

    /// <summary>Email template content/body</summary>
    public string? Template { get; set; }

    #region Timing

    /// <summary>Timing mode</summary>
    public StepTimingMode TimingMode { get; set; } = StepTimingMode.Delay;

    /// <summary>Delay in days from previous step</summary>
    public int DelayDays { get; set; } = 1;

    /// <summary>Delay in hours</summary>
    public int DelayHours { get; set; } = 0;

    /// <summary>Delay in minutes</summary>
    public int DelayMinutes { get; set; } = 0;

    /// <summary>Specific time of day (HH:mm)</summary>
    public string? SpecificTime { get; set; }

    #endregion

    #region Email Content

    /// <summary>Email subject line</summary>
    public string? Subject { get; set; }

    /// <summary>Email body (HTML)</summary>
    public string? Body { get; set; }

    /// <summary>Plain text version</summary>
    public string? BodyPlainText { get; set; }

    /// <summary>Email template ID</summary>
    public int? EmailTemplateId { get; set; }

    /// <summary>Whether this is a reply (thread)</summary>
    public bool IsReply { get; set; } = false;

    /// <summary>Step to reply to</summary>
    public int? ReplyToStepId { get; set; }

    #endregion

    #region Task Content (for task steps)

    /// <summary>Task title</summary>
    public string? TaskTitle { get; set; }

    /// <summary>Task description</summary>
    public string? TaskDescription { get; set; }

    /// <summary>Task priority</summary>
    public string? TaskPriority { get; set; }

    /// <summary>Task due offset (days from step)</summary>
    public int TaskDueDays { get; set; } = 0;

    #endregion

    #region Condition (for condition steps)

    /// <summary>Condition type (opened, clicked, replied)</summary>
    public string? ConditionType { get; set; }

    /// <summary>Condition value</summary>
    public string? ConditionValue { get; set; }

    /// <summary>Step to go to if condition is true</summary>
    public int? TrueStepId { get; set; }

    /// <summary>Step to go to if condition is false</summary>
    public int? FalseStepId { get; set; }

    #endregion

    #region A/B Testing

    /// <summary>Whether this is an A/B test step</summary>
    public bool IsABTest { get; set; } = false;

    /// <summary>A/B variant (A, B, C, etc.)</summary>
    public string? ABVariant { get; set; }

    /// <summary>A/B test percentage split</summary>
    public int? ABSplitPercent { get; set; }

    #endregion

    #region Statistics

    /// <summary>Times this step was executed</summary>
    public int ExecutionCount { get; set; } = 0;

    /// <summary>Emails sent from this step</summary>
    public int EmailsSent { get; set; } = 0;

    /// <summary>Opens from this step</summary>
    public int Opens { get; set; } = 0;

    /// <summary>Clicks from this step</summary>
    public int Clicks { get; set; } = 0;

    /// <summary>Replies from this step</summary>
    public int Replies { get; set; } = 0;

    /// <summary>Bounces from this step</summary>
    public int Bounces { get; set; } = 0;

    #endregion

    #region Relationships

    /// <summary>Parent sequence ID</summary>
    public int EmailSequenceId { get; set; }

    /// <summary>Alias for EmailSequenceId</summary>
    [NotMapped]
    public int? SequenceId
    {
        get => EmailSequenceId;
        set => EmailSequenceId = value ?? 0;
    }

    /// <summary>Navigation to sequence</summary>
    public EmailSequence? EmailSequence { get; set; }

    #endregion
}

/// <summary>
/// Enrollment of a lead/contact in an email sequence.
/// </summary>
public class EmailSequenceEnrollment : BaseEntity
{
    #region Enrollment Details

    /// <summary>Current enrollment status</summary>
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    /// <summary>Enrollment date</summary>
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    /// <summary>Completion/exit date</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Exit reason</summary>
    public SequenceExitCondition? ExitReason { get; set; }

    /// <summary>Exit notes</summary>
    public string? ExitNotes { get; set; }

    #endregion

    #region Progress

    /// <summary>Current step index</summary>
    public int CurrentStepIndex { get; set; } = 0;

    /// <summary>Current step ID</summary>
    public int? CurrentStepId { get; set; }

    /// <summary>Next step scheduled time</summary>
    public DateTime? NextStepScheduledAt { get; set; }

    /// <summary>Last step executed time</summary>
    public DateTime? LastStepExecutedAt { get; set; }

    /// <summary>Steps completed count</summary>
    public int StepsCompleted { get; set; } = 0;

    /// <summary>Emails sent to this enrollment</summary>
    public int EmailsSent { get; set; } = 0;

    #endregion

    #region Recipient Info

    /// <summary>Recipient email</summary>
    public string RecipientEmail { get; set; } = string.Empty;

    /// <summary>Recipient name</summary>
    public string? RecipientName { get; set; }

    /// <summary>Recipient timezone</summary>
    public string? RecipientTimezone { get; set; }

    #endregion

    #region Engagement Metrics

    /// <summary>Total opens</summary>
    public int TotalOpens { get; set; } = 0;

    /// <summary>Total clicks</summary>
    public int TotalClicks { get; set; } = 0;

    /// <summary>Has replied</summary>
    public bool HasReplied { get; set; } = false;

    /// <summary>Reply date</summary>
    public DateTime? RepliedAt { get; set; }

    /// <summary>Has bounced</summary>
    public bool HasBounced { get; set; } = false;

    /// <summary>Bounce date</summary>
    public DateTime? BouncedAt { get; set; }

    /// <summary>Has unsubscribed</summary>
    public bool HasUnsubscribed { get; set; } = false;

    /// <summary>Unsubscribe date</summary>
    public DateTime? UnsubscribedAt { get; set; }

    /// <summary>Meeting booked</summary>
    public bool MeetingBooked { get; set; } = false;

    /// <summary>Meeting booked date</summary>
    public DateTime? MeetingBookedAt { get; set; }

    #endregion

    #region Relationships

    /// <summary>Sequence ID</summary>
    public int EmailSequenceId { get; set; }

    /// <summary>Alias for EmailSequenceId</summary>
    [NotMapped]
    public int? SequenceId
    {
        get => EmailSequenceId;
        set => EmailSequenceId = value ?? 0;
    }

    /// <summary>Navigation to sequence</summary>
    public EmailSequence? EmailSequence { get; set; }

    /// <summary>Lead ID (if lead)</summary>
    public int? LeadId { get; set; }

    /// <summary>Navigation to lead</summary>
    public Lead? Lead { get; set; }

    /// <summary>Contact ID (if contact)</summary>
    public int? ContactId { get; set; }

    /// <summary>Navigation to contact</summary>
    public Contact? Contact { get; set; }

    /// <summary>Enrolled by user ID</summary>
    public int? EnrolledById { get; set; }

    /// <summary>Navigation to enrolling user</summary>
    public User? EnrolledBy { get; set; }

    /// <summary>Step executions</summary>
    public ICollection<EmailSequenceStepExecution> StepExecutions { get; set; } = new List<EmailSequenceStepExecution>();

    #endregion
}

/// <summary>
/// Individual step execution within an enrollment.
/// </summary>
public class EmailSequenceStepExecution : BaseEntity
{
    /// <summary>Step ID executed</summary>
    public int EmailSequenceStepId { get; set; }

    /// <summary>Navigation to step</summary>
    public EmailSequenceStep? EmailSequenceStep { get; set; }

    /// <summary>Enrollment ID</summary>
    public int EmailSequenceEnrollmentId { get; set; }

    /// <summary>Navigation to enrollment</summary>
    public EmailSequenceEnrollment? EmailSequenceEnrollment { get; set; }

    /// <summary>Scheduled time</summary>
    public DateTime ScheduledAt { get; set; }

    /// <summary>Executed time</summary>
    public DateTime? ExecutedAt { get; set; }

    /// <summary>Whether successful</summary>
    public bool Success { get; set; } = false;

    /// <summary>Error message if failed</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Email message ID (for tracking)</summary>
    public string? MessageId { get; set; }

    /// <summary>Opens for this execution</summary>
    public int Opens { get; set; } = 0;

    /// <summary>Clicks for this execution</summary>
    public int Clicks { get; set; } = 0;

    /// <summary>Replied</summary>
    public bool Replied { get; set; } = false;

    /// <summary>Reply date</summary>
    public DateTime? RepliedAt { get; set; }

    /// <summary>Bounced</summary>
    public bool Bounced { get; set; } = false;

    /// <summary>Bounce type</summary>
    public string? BounceType { get; set; }

    #endregion
}
