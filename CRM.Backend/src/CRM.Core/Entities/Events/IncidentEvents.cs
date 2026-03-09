// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Ports.Output.Events;

namespace CRM.Core.Entities.Events;

/// <summary>
/// AP-059: Raised when an ITSM incident is resolved.
/// </summary>
public sealed record IncidentResolvedEvent(
    int IncidentId,
    string ResolutionSummary,
    DateTime ResolvedAt,
    bool SlaBreach) : DomainEventBase;

/// <summary>
/// AP-059: Raised when an ITSM incident is formally closed.
/// </summary>
public sealed record IncidentClosedEvent(
    int IncidentId,
    string? Notes,
    DateTime ClosedAt) : DomainEventBase;

/// <summary>
/// AP-059: Raised when an ITSM incident is escalated to a higher support tier.
/// </summary>
public sealed record IncidentEscalatedEvent(
    int IncidentId,
    int EscalationLevel,
    string Reason) : DomainEventBase;
