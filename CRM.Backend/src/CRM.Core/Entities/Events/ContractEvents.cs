// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Ports.Output.Events;

namespace CRM.Core.Entities.Events;

/// <summary>
/// AP-059: Raised when a contract gets formal approval.
/// </summary>
public sealed record ContractApprovedEvent(
    int ContractId,
    int ApprovedByUserId,
    DateTime ApprovedAt) : DomainEventBase;

/// <summary>
/// AP-059: Raised when a contract is renewed, extending its effective period.
/// </summary>
public sealed record ContractRenewedEvent(
    int ContractId,
    DateTime NewEndDate) : DomainEventBase;

/// <summary>
/// AP-059: Raised when a contract is terminated before its natural end date.
/// </summary>
public sealed record ContractTerminatedEvent(
    int ContractId,
    string Reason,
    int TerminatedByUserId,
    DateTime TerminatedAt) : DomainEventBase;

/// <summary>
/// AP-059: Raised when a contract passes its end date without renewal.
/// </summary>
public sealed record ContractExpiredEvent(
    int ContractId,
    DateTime ExpiredAt) : DomainEventBase;
