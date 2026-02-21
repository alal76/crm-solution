// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

#region Enums

/// <summary>
/// FUNCTIONAL: Type of attendee for an event
/// TECHNICAL: Polymorphic reference type
/// </summary>
public enum AttendeeType
{
    /// <summary>Internal CRM user</summary>
    User = 0,

    /// <summary>External contact</summary>
    Contact = 1,

    /// <summary>Lead (potential customer)</summary>
    Lead = 2
}

/// <summary>
/// FUNCTIONAL: Response status for event invitation
/// TECHNICAL: Tracks RSVP status
/// </summary>
public enum AttendeeResponseStatus
{
    /// <summary>No response yet</summary>
    NotResponded = 0,

    /// <summary>Attendee accepted the invitation</summary>
    Accepted = 1,

    /// <summary>Attendee declined the invitation</summary>
    Declined = 2,

    /// <summary>Attendee tentatively accepted</summary>
    Tentative = 3
}

#endregion

/// <summary>
/// FUNCTIONAL: Tracks attendees for calendar events (meetings, calls, demos)
/// TECHNICAL: Polymorphic junction table linking activities to users/contacts/leads
///
/// Key Relationships:
/// - Activity (parent event)
/// - User (if AttendeeType = User)
/// - Contact (if AttendeeType = Contact)
/// - Lead (if AttendeeType = Lead)
/// </summary>
public class EventAttendee : BaseEntity
{
    #region Event Reference

    /// <summary>
    /// FUNCTIONAL: The event this attendee is associated with
    /// TECHNICAL: Foreign key to Activities table
    /// </summary>
    [Required]
    public int ActivityId { get; set; }

    #endregion

    #region Attendee Reference

    /// <summary>
    /// FUNCTIONAL: Type of attendee (User, Contact, or Lead)
    /// TECHNICAL: Determines which table AttendeeId references
    /// </summary>
    [Required]
    public AttendeeType AttendeeType { get; set; }

    /// <summary>
    /// FUNCTIONAL: ID of the attendee
    /// TECHNICAL: Polymorphic foreign key based on AttendeeType
    /// </summary>
    [Required]
    public int AttendeeId { get; set; }

    /// <summary>
    /// FUNCTIONAL: Attendee email (for external attendees or quick reference)
    /// TECHNICAL: Optional, can be populated from linked entity
    /// </summary>
    [MaxLength(255)]
    public string? AttendeeEmail { get; set; }

    /// <summary>
    /// FUNCTIONAL: Display name for the attendee
    /// TECHNICAL: Cached from linked entity for performance
    /// </summary>
    [MaxLength(200)]
    public string? AttendeeName { get; set; }

    #endregion

    #region Response Tracking

    /// <summary>
    /// FUNCTIONAL: RSVP response status
    /// TECHNICAL: Updated when attendee responds to invitation
    /// </summary>
    public AttendeeResponseStatus ResponseStatus { get; set; } = AttendeeResponseStatus.NotResponded;

    /// <summary>
    /// FUNCTIONAL: When the attendee responded
    /// TECHNICAL: Timestamp of last response update
    /// </summary>
    public DateTime? RespondedAt { get; set; }

    /// <summary>
    /// FUNCTIONAL: Optional comment with response
    /// TECHNICAL: Free text response message
    /// </summary>
    [MaxLength(500)]
    public string? ResponseComment { get; set; }

    #endregion

    #region Attendee Role

    /// <summary>
    /// FUNCTIONAL: Is this the event organizer?
    /// TECHNICAL: Only one organizer per event
    /// </summary>
    public bool IsOrganizer { get; set; } = false;

    /// <summary>
    /// FUNCTIONAL: Is this attendee required (vs optional)?
    /// TECHNICAL: Affects calendar blocking behavior
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// FUNCTIONAL: Role of the attendee in the meeting
    /// TECHNICAL: Examples: "Decision Maker", "Influencer", "Technical Lead"
    /// </summary>
    [MaxLength(100)]
    public string? Role { get; set; }

    #endregion

    #region Attendance Tracking

    /// <summary>
    /// FUNCTIONAL: Did the attendee actually attend?
    /// TECHNICAL: Set after event completion
    /// </summary>
    public bool? DidAttend { get; set; }

    /// <summary>
    /// FUNCTIONAL: How long did they attend (minutes)?
    /// TECHNICAL: For partial attendance tracking
    /// </summary>
    public int? AttendanceDurationMinutes { get; set; }

    /// <summary>
    /// FUNCTIONAL: Notes about attendance
    /// TECHNICAL: Post-meeting notes about this attendee
    /// </summary>
    [MaxLength(1000)]
    public string? AttendanceNotes { get; set; }

    #endregion

    #region External Calendar Sync

    /// <summary>
    /// FUNCTIONAL: External calendar event ID (Google, Outlook)
    /// TECHNICAL: For bi-directional sync
    /// </summary>
    [MaxLength(500)]
    public string? ExternalCalendarEventId { get; set; }

    /// <summary>
    /// FUNCTIONAL: When invitation was sent
    /// TECHNICAL: Tracks outbound invitation
    /// </summary>
    public DateTime? InvitationSentAt { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>Parent activity/event</summary>
    public virtual Activity? Activity { get; set; }

    /// <summary>User attendee (if AttendeeType = User)</summary>
    public virtual User? User { get; set; }

    #endregion
}
