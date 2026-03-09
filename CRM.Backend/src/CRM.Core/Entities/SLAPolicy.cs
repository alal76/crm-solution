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

namespace CRM.Core.Entities;

/// <summary>
/// Represents a Service Level Agreement policy.
/// </summary>
[Table("SLAPolicies")]
public class SLAPolicy : BaseEntity, IHasDomainEvents
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1024)]
    public string? Description { get; set; }

    [Required]
    public ServicePriority Priority { get; set; }

    [Required]
    public int InitialResponseTimeMinutes { get; set; }

    [Required]
    public int ResolutionTimeMinutes { get; set; }

    public bool WorkingHoursOnly { get; set; }

    [Column(TypeName = "longtext")]
    public string EscalationPath { get; set; } = string.Empty; // JSON array of user IDs in escalation order

    public bool IsActive { get; set; } = true;

    /// <summary>JSON object with business hours configuration</summary>
    [Column(TypeName = "longtext")]
    public string? BusinessHours { get; set; }

    /// <summary>JSON array of case types this policy applies to</summary>
    [Column(TypeName = "longtext")]
    public string? CaseTypesJson { get; set; }

    /// <summary>JSON array of customer segments this policy applies to</summary>
    [Column(TypeName = "longtext")]
    public string? CustomerSegmentsJson { get; set; }

    /// <summary>JSON array of customer tiers this policy applies to</summary>
    [Column(TypeName = "longtext")]
    public string? CustomerTiersJson { get; set; }

    /// <summary>JSON object with conditions to match for this policy</summary>
    [Column(TypeName = "longtext")]
    public string? MatchConditionsJson { get; set; }

    #region Domain Events

    private readonly List<IDomainEvent> _domainEvents = new();

    /// <inheritdoc />
    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <inheritdoc />
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <inheritdoc />
    public void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);

    /// <inheritdoc />
    public void ClearDomainEvents() => _domainEvents.Clear();

    #endregion

    #region Business Methods

    /// <summary>
    /// Activates the SLA policy. Throws if already active.
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            throw new BusinessRuleException("SLAPolicy.Activate", "SLA policy is already active.");

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SLAPolicyActivatedEvent(Id, DateTime.UtcNow));
    }

    /// <summary>
    /// Deactivates the SLA policy. Throws if already inactive.
    /// </summary>
    public void Deactivate(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleException("SLAPolicy.Deactivate", "Deactivation reason is required.");
        if (!IsActive)
            throw new BusinessRuleException("SLAPolicy.Deactivate", "SLA policy is already inactive.");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SLAPolicyDeactivatedEvent(Id, reason, DateTime.UtcNow));
    }

    /// <summary>
    /// Factory method for unit testing — creates an SLAPolicy with specified active state.
    /// </summary>
    internal static SLAPolicy CreateForTesting(
        bool isActive = true)
    {
        return new SLAPolicy
        {
            Id = 1,
            IsActive = isActive,
            Name = "Test SLA Policy",
            Priority = ServicePriority.Medium,
            InitialResponseTimeMinutes = 60,
            ResolutionTimeMinutes = 480,
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion

    /// <summary>JSON array of products this policy applies to</summary>
    [Column(TypeName = "longtext")]
    public string? ProductsJson { get; set; }

    /// <summary>
    /// Business hours configuration ID for this SLA policy
    /// </summary>
    public int? BusinessHoursId { get; set; }
}

/// <summary>
/// Service request priority levels
/// </summary>
public enum ServicePriority
{
    Critical = 0,
    High = 1,
    Medium = 2,
    Low = 3
}
