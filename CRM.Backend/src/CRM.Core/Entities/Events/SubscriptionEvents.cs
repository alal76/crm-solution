// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Ports.Output.Events;

namespace CRM.Core.Entities.Events;

/// <summary>
/// AP-059 Phase 3: Raised when a subscription is cancelled.
/// </summary>
public sealed record SubscriptionCancelledEvent(
    int SubscriptionId,
    string Reason,
    DateTime CancelledAt) : DomainEventBase;

/// <summary>
/// AP-059 Phase 3: Raised when a cancelled subscription is reinstated.
/// </summary>
public sealed record SubscriptionReinstatedEvent(
    int SubscriptionId,
    DateTime ReinstatedAt) : DomainEventBase;
