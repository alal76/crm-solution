// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Core.Entities.Events;
using CRM.Core.Exceptions;
using CRM.Core.Ports.Output.Events;

namespace CRM.Core.Entities.ITSM;

public enum IncidentImpact
{
    High = 1,
    Medium = 2,
    Low = 3
}

public enum IncidentUrgency
{
    High = 1,
    Medium = 2,
    Low = 3
}

public enum IncidentState
{
    New = 1,
    Assigned = 2,
    InProgress = 3,
    OnHold = 4,
    Resolved = 5,
    Closed = 6,
    Cancelled = 7
}

public enum ContactType
{
    Phone = 1,
    Email = 2,
    Portal = 3,
    Chat = 4,
    WalkIn = 5,
    Monitoring = 6
}

public enum ResolutionCode
{
    SolvedPermanently = 1,
    SolvedTemporarily = 2,
    Workaround = 3,
    NotSolvable = 4,
    Duplicate = 5,
    UserError = 6,
    ConfigurationChange = 7,
    SoftwareUpdate = 8,
    HardwareReplacement = 9
}

/// <summary>
/// Represents an ITSM incident record.
/// </summary>
public class Incident : IHasDomainEvents
{
    [Key]
    public int IncidentId { get; set; }

    [Required]
    [StringLength(20)]
    public string Number { get; set; } = string.Empty;

    [Required]
    [StringLength(160)]
    public string ShortDescription { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Caller Information
    [Required]
    public int CallerId { get; set; }

    [ForeignKey(nameof(CallerId))]
    public User? Caller { get; set; }

    public ContactType ContactType { get; set; }

    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;

    public int? OpenedById { get; set; }

    [ForeignKey(nameof(OpenedById))]
    public User? OpenedBy { get; set; }

    // Classification
    public int? CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public ServiceRequestCategory? Category { get; set; }

    public int? SubcategoryId { get; set; }

    [ForeignKey(nameof(SubcategoryId))]
    public ServiceRequestSubcategory? Subcategory { get; set; }

    public int? ConfigurationItemId { get; set; }

    // ConfigurationItem FK will be added in Phase 2
    public int? ServiceId { get; set; }

    // Service FK will be added in Phase 2
    // Prioritization
    [Required]
    public IncidentImpact Impact { get; set; }

    [Required]
    public IncidentUrgency Urgency { get; set; }

    // Priority is calculated: Impact + Urgency (2-6 scale, lower is higher priority)
    public int Priority => ((int)Impact) + ((int)Urgency);

    // Assignment
    [Required]
    public IncidentState State { get; set; } = IncidentState.New;

    public int? AssignmentGroupId { get; set; }

    [ForeignKey(nameof(AssignmentGroupId))]
    public UserGroup? AssignmentGroup { get; set; }

    public int? AssignedToId { get; set; }

    [ForeignKey(nameof(AssignedToId))]
    public User? AssignedTo { get; set; }

    public int EscalationLevel { get; set; } = 0;

    // Resolution
    public ResolutionCode? ResolutionCode { get; set; }

    public string? ResolutionNotes { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public int? ResolvedById { get; set; }

    [ForeignKey(nameof(ResolvedById))]
    public User? ResolvedBy { get; set; }

    public DateTime? ClosedAt { get; set; }

    public int? ClosedById { get; set; }

    [ForeignKey(nameof(ClosedById))]
    public User? ClosedBy { get; set; }

    // SLA
    public bool SLABreached { get; set; } = false;

    public DateTime? ResponseDueAt { get; set; }

    public DateTime? ResolutionDueAt { get; set; }

    public int? BusinessElapsedMinutes { get; set; }

    // Relationships
    public bool MajorIncident { get; set; } = false;

    public int? ParentIncidentId { get; set; }

    [ForeignKey(nameof(ParentIncidentId))]
    public Incident? ParentIncident { get; set; }

    public ICollection<Incident>? ChildIncidents { get; set; }

    public int? ProblemId { get; set; }

    // Problem FK will be added in Phase 1.2
    public int? ChangeRequestId { get; set; }

    // Change FK will be added in Phase 2
    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation Properties
    public ICollection<IncidentComment>? Comments { get; set; }

    public ICollection<IncidentAttachment>? Attachments { get; set; }

    public ICollection<IncidentHistory>? History { get; set; }

    public ICollection<ProblemIncident>? ProblemIncidents { get; set; }

    #region Domain Events

    private readonly List<IDomainEvent> _domainEvents = new();

    /// <inheritdoc />
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <inheritdoc />
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <inheritdoc />
    public void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);

    /// <inheritdoc />
    public void ClearDomainEvents() => _domainEvents.Clear();

    #endregion

    #region Business Methods

    /// <summary>Resolves the incident with a summary. Sets SLABreached if past resolution due date.</summary>
    public void Resolve(string resolutionSummary)
    {
        if (State == IncidentState.Closed)
            throw new BusinessRuleException("Incident.Resolve", "Cannot resolve a closed incident.");
        if (State == IncidentState.Resolved)
            throw new BusinessRuleException("Incident.Resolve", "Incident is already resolved.");

        State = IncidentState.Resolved;
        ResolvedAt = DateTime.UtcNow;
        ResolutionNotes = resolutionSummary;
        SLABreached = ResolutionDueAt.HasValue && DateTime.UtcNow > ResolutionDueAt.Value;
        ModifiedAt = DateTime.UtcNow;
        AddDomainEvent(new IncidentResolvedEvent(IncidentId, resolutionSummary, DateTime.UtcNow, SLABreached));
    }

    /// <summary>Closes the incident. Incident must be resolved first.</summary>
    public void Close(string? notes = null)
    {
        if (State != IncidentState.Resolved)
            throw new BusinessRuleException("Incident.Close", "Incident must be resolved before closing.");

        State = IncidentState.Closed;
        ClosedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
        AddDomainEvent(new IncidentClosedEvent(IncidentId, notes, DateTime.UtcNow));
    }

    /// <summary>Escalates the incident to a higher support tier.</summary>
    public void Escalate(int escalationLevel, string reason)
    {
        if (State == IncidentState.Closed || State == IncidentState.Resolved)
            throw new BusinessRuleException("Incident.Escalate", "Cannot escalate a closed or resolved incident.");

        EscalationLevel = escalationLevel;
        ModifiedAt = DateTime.UtcNow;
        AddDomainEvent(new IncidentEscalatedEvent(IncidentId, escalationLevel, reason));
    }

    /// <summary>Internal factory for unit tests — bypasses service layer.</summary>
    internal static Incident CreateForTesting(
        IncidentState state = IncidentState.New,
        DateTime? resolutionDueAt = null)
    {
        return new Incident
        {
            IncidentId = 1,
            Number = "INC-TEST-001",
            ShortDescription = "Test Incident",
            CallerId = 1,
            Impact = IncidentImpact.Medium,
            Urgency = IncidentUrgency.Medium,
            State = state,
            ResolutionDueAt = resolutionDueAt
        };
    }

    #endregion
}

/// <summary>
/// Represents a comment on an incident.
/// </summary>
public class IncidentComment
{
    [Key]
    public int CommentId { get; set; }

    [Required]
    public int IncidentId { get; set; }

    [ForeignKey(nameof(IncidentId))]
    public Incident? Incident { get; set; }

    [Required]
    public string Comment { get; set; } = string.Empty;

    public bool IsInternal { get; set; } = false;

    [Required]
    public int CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public User? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}

/// <summary>
/// Represents a file attachment on an incident.
/// </summary>
public class IncidentAttachment
{
    [Key]
    public int AttachmentId { get; set; }

    [Required]
    public int IncidentId { get; set; }

    [ForeignKey(nameof(IncidentId))]
    public Incident? Incident { get; set; }

    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ContentType { get; set; }

    public long FileSize { get; set; }

    [Required]
    public int UploadedById { get; set; }

    [ForeignKey(nameof(UploadedById))]
    public User? UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}

/// <summary>
/// Represents a historical change record for an incident.
/// </summary>
public class IncidentHistory
{
    [Key]
    public int HistoryId { get; set; }

    [Required]
    public int IncidentId { get; set; }

    [ForeignKey(nameof(IncidentId))]
    public Incident? Incident { get; set; }

    [Required]
    [StringLength(100)]
    public string Field { get; set; } = string.Empty;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    [Required]
    public int ChangedById { get; set; }

    [ForeignKey(nameof(ChangedById))]
    public User? ChangedBy { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
