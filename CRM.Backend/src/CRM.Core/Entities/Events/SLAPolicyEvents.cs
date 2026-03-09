// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Ports.Output.Events;

namespace CRM.Core.Entities.Events;

/// <summary>
/// AP-059 Phase 3: Raised when an SLA policy is activated.
/// </summary>
public sealed record SLAPolicyActivatedEvent(
    int SLAPolicyId,
    DateTime ActivatedAt) : DomainEventBase;

/// <summary>
/// AP-059 Phase 3: Raised when an SLA policy is deactivated.
/// </summary>
public sealed record SLAPolicyDeactivatedEvent(
    int SLAPolicyId,
    string Reason,
    DateTime DeactivatedAt) : DomainEventBase;
