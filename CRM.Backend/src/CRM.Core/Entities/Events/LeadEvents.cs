// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Ports.Output.Events;

namespace CRM.Core.Entities.Events;

/// <summary>
/// AP-059: Raised when a lead is converted to an account and opportunity.
/// </summary>
public sealed record LeadConvertedEvent(
    int LeadId,
    int AccountId,
    string OpportunityTitle,
    DateTime ConvertedAt) : DomainEventBase;

/// <summary>
/// AP-059: Raised when a lead is disqualified (not a fit / invalid).
/// </summary>
public sealed record LeadDisqualifiedEvent(
    int LeadId,
    string Reason,
    DateTime DisqualifiedAt) : DomainEventBase;

/// <summary>
/// AP-059: Raised when a lead meets qualification criteria (MQL/SQL).
/// </summary>
public sealed record LeadQualifiedEvent(
    int LeadId,
    int Score,
    DateTime QualifiedAt) : DomainEventBase;

/// <summary>
/// AP-059: Raised when a lead is assigned to an owner.
/// </summary>
public sealed record LeadAssignedEvent(
    int LeadId,
    int OwnerId) : DomainEventBase;
