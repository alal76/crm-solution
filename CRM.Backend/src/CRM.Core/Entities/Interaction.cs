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
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

/// <summary>
/// Interaction type enumeration
/// </summary>
public enum InteractionType
{
    Email = 0,
    Phone = 1,
    Meeting = 2,
    VideoCall = 3,
    Chat = 4,
    SMS = 5,
    SocialMedia = 6,
    InPerson = 7,
    WebForm = 8,
    Note = 9,
    Task = 10,
    Demo = 11,
    Presentation = 12,
    Contract = 13,
    Support = 14,
    Other = 15
}

/// <summary>
/// Interaction direction
/// </summary>
public enum InteractionDirection
{
    Inbound = 0,
    Outbound = 1,
    Internal = 2
}

/// <summary>
/// Interaction outcome
/// </summary>
public enum InteractionOutcome
{
    None = 0,
    Successful = 1,
    Unsuccessful = 2,
    FollowUpRequired = 3,
    NoResponse = 4,
    Voicemail = 5,
    Rescheduled = 6,
    Cancelled = 7
}

/// <summary>
/// Interaction sentiment
/// </summary>
public enum InteractionSentiment
{
    VeryNegative = 0,
    Negative = 1,
    Neutral = 2,
    Positive = 3,
    VeryPositive = 4
}

/// <summary>
/// Interaction entity for tracking customer communications
/// </summary>
public class Interaction : BaseEntity
{
    #region Basic Information

    /// <summary>Type of interaction</summary>
    public InteractionType InteractionType { get; set; } = InteractionType.Note;

    /// <summary>Legacy type field</summary>
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    /// <summary>Interaction direction</summary>
    public InteractionDirection Direction { get; set; } = InteractionDirection.Outbound;

    /// <summary>Subject/title of the interaction</summary>
    [Required]
    [MaxLength(500)]
    public string Subject { get; set; } = string.Empty;

    /// <summary>Full description/content</summary>
    [MaxLength(10000)]
    public string Description { get; set; } = string.Empty;

    #endregion

    #region Timing

    /// <summary>Date/time of interaction</summary>
    public DateTime InteractionDate { get; set; }

    /// <summary>End time of interaction</summary>
    public DateTime? EndTime { get; set; }

    /// <summary>Duration in minutes</summary>
    [Range(0, 1440)] // Max 24 hours
    public int? DurationMinutes { get; set; }

    /// <summary>Scheduled date if planned</summary>
    public DateTime? ScheduledDate { get; set; }

    /// <summary>Actual completion date</summary>
    public DateTime? CompletedDate { get; set; }

    #endregion

    #region Status & Outcome

    /// <summary>Outcome of the interaction</summary>
    public InteractionOutcome Outcome { get; set; } = InteractionOutcome.None;

    /// <summary>Sentiment detected/recorded</summary>
    public InteractionSentiment Sentiment { get; set; } = InteractionSentiment.Neutral;

    /// <summary>Whether interaction is completed</summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>Whether interaction is private</summary>
    public bool IsPrivate { get; set; } = false;

    /// <summary>Priority level (1-5)</summary>
    [Range(1, 5)]
    public int? Priority { get; set; } = 1;

    #endregion

    #region Communication Details

    /// <summary>Phone number involved</summary>
    [Phone]
    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    /// <summary>Email address involved</summary>
    [EmailAddress]
    [MaxLength(200)]
    public string? EmailAddress { get; set; }

    /// <summary>Location (for meetings)</summary>
    [MaxLength(500)]
    public string? Location { get; set; }

    /// <summary>Video meeting link</summary>
    [Url]
    [MaxLength(1000)]
    public string? MeetingLink { get; set; }

    /// <summary>Recording URL</summary>
    [Url]
    [MaxLength(1000)]
    public string? RecordingUrl { get; set; }

    #endregion

    #region Email Specific

    /// <summary>Email CC recipients</summary>
    [MaxLength(2000)]
    public string? EmailCc { get; set; }

    /// <summary>Email BCC recipients</summary>
    [MaxLength(2000)]
    public string? EmailBcc { get; set; }

    /// <summary>Whether email was opened</summary>
    public bool? EmailOpened { get; set; }

    /// <summary>Date email was opened</summary>
    public DateTime? EmailOpenedDate { get; set; }

    /// <summary>Whether email links were clicked</summary>
    public bool? EmailClicked { get; set; }

    /// <summary>Date email was clicked</summary>
    public DateTime? EmailClickedDate { get; set; }

    /// <summary>Whether email bounced</summary>
    public bool? EmailBounced { get; set; }

    #endregion

    #region Meeting Specific

    /// <summary>Meeting attendees (JSON array)</summary>
    [MaxLength(5000)]
    public string? Attendees { get; set; }

    /// <summary>Meeting notes</summary>
    [MaxLength(10000)]
    public string? MeetingNotes { get; set; }

    /// <summary>Meeting agenda</summary>
    [MaxLength(5000)]
    public string? MeetingAgenda { get; set; }

    /// <summary>Action items (JSON array)</summary>
    [MaxLength(5000)]
    public string? ActionItems { get; set; }

    #endregion

    #region Call Specific

    /// <summary>Call recording URL</summary>
    [Url]
    [MaxLength(1000)]
    public string? CallRecordingUrl { get; set; }

    /// <summary>Call transcript</summary>
    [MaxLength(50000)]
    public string? CallTranscript { get; set; }

    /// <summary>Call disposition code</summary>
    [MaxLength(100)]
    public string? CallDisposition { get; set; }

    #endregion

    #region Follow-up

    /// <summary>Follow-up date</summary>
    public DateTime? FollowUpDate { get; set; }

    /// <summary>Follow-up notes</summary>
    [MaxLength(2000)]
    public string? FollowUpNotes { get; set; }

    /// <summary>Related follow-up interaction ID</summary>
    public int? FollowUpInteractionId { get; set; }

    #endregion

    #region Relationships

    /// <summary>Associated customer ID</summary>
    [Column("CustomerId")]
    public int? AccountId { get; set; }

    /// <summary>Associated contact ID</summary>
    public int? ContactId { get; set; }

    /// <summary>Associated opportunity ID</summary>
    public int? OpportunityId { get; set; }

    /// <summary>Associated campaign ID</summary>
    public int? CampaignId { get; set; }

    /// <summary>Assigned user ID</summary>
    public int? AssignedToUserId { get; set; }

    /// <summary>User who created the interaction</summary>
    public int? CreatedByUserId { get; set; }

    #endregion

    #region Classification & Metadata

    /// <summary>Comma-separated tags</summary>
    [MaxLength(500)]
    public string? Tags { get; set; }

    /// <summary>Category</summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>Attachments (JSON array of file URLs)</summary>
    [MaxLength(5000)]
    public string? Attachments { get; set; }

    /// <summary>Custom fields (JSON)</summary>
    [MaxLength(10000)]
    public string? CustomFields { get; set; }

    #endregion

    #region Navigation Properties

    public Account? Account { get; set; }
    public Opportunity? Opportunity { get; set; }
    public MarketingCampaign? Campaign { get; set; }
    public User? AssignedToUser { get; set; }
    public User? CreatedByUser { get; set; }
    public Interaction? FollowUpInteraction { get; set; }

    #endregion
}
