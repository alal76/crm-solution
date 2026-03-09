// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Ports.Output.Events;

namespace CRM.Core.Entities.Events;

/// <summary>
/// AP-059: Raised when an account's lifecycle stage changes (e.g., Lead → Active, Active → AtRisk).
/// </summary>
/// <remarks>AccountLifecycleStage is defined in CRM.Core.Entities (Account.cs).</remarks>
public sealed record AccountLifecycleChangedEvent(
    int AccountId,
    AccountLifecycleStage OldStage,
    AccountLifecycleStage NewStage) : DomainEventBase;

/// <summary>
/// AP-059: Raised when the primary contact of an account is set or changed.
/// </summary>
public sealed record AccountPrimaryContactSetEvent(
    int AccountId,
    int ContactId) : DomainEventBase;

/// <summary>
/// AP-059: Raised when an account is deactivated.
/// </summary>
public sealed record AccountDeactivatedEvent(
    int AccountId,
    string Reason,
    DateTime DeactivatedAt) : DomainEventBase;
