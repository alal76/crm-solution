using System;

namespace CRM.Core.Entities;

/// <summary>
/// CRM calendar event for meetings, calls, and scheduled activities.
/// </summary>
public class Event : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? EventType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsAllDay { get; set; }
    public string? Location { get; set; }
    public int? OrganizerId { get; set; }
    public string? RelatedEntityType { get; set; }
    public int? RelatedEntityId { get; set; }
    public string? Status { get; set; }
    public string? RecurrenceRule { get; set; }
    public virtual User? Organizer { get; set; }
}
