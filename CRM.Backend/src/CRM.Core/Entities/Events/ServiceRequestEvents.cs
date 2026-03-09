// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Ports.Output.Events;

namespace CRM.Core.Entities.Events;

/// <summary>
/// AP-059: Raised when a service request is resolved.
/// </summary>
public sealed record ServiceRequestResolvedEvent(
    int ServiceRequestId,
    string ResolutionSummary,
    DateTime ResolvedAt) : DomainEventBase;

/// <summary>
/// AP-059: Raised when a service request is closed.
/// </summary>
public sealed record ServiceRequestClosedEvent(
    int ServiceRequestId,
    string? CloseNotes,
    DateTime ClosedAt) : DomainEventBase;

/// <summary>
/// AP-059: Raised when a service request is escalated.
/// </summary>
public sealed record ServiceRequestEscalatedEvent(
    int ServiceRequestId,
    int EscalationLevel,
    string Reason) : DomainEventBase;

/// <summary>
/// AP-059: Raised when a service request is assigned to an agent.
/// </summary>
public sealed record ServiceRequestAssignedEvent(
    int ServiceRequestId,
    int AssigneeId) : DomainEventBase;

/// <summary>
/// AP-059: Raised when a previously resolved or closed service request is reopened.
/// </summary>
public sealed record ServiceRequestReopenedEvent(
    int ServiceRequestId,
    string Reason) : DomainEventBase;
