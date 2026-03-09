// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Ports.Output.Events;

namespace CRM.Core.Entities.Events;

/// <summary>
/// AP-059 Phase 3: Raised when a quote is approved.
/// </summary>
public sealed record QuoteApprovedEvent(
    int QuoteId,
    int ApprovedByUserId,
    DateTime ApprovedAt) : DomainEventBase;

/// <summary>
/// AP-059 Phase 3: Raised when a quote is sent to a customer.
/// </summary>
public sealed record QuoteSentEvent(
    int QuoteId,
    DateTime SentAt) : DomainEventBase;

/// <summary>
/// AP-059 Phase 3: Raised when a quote is revoked/cancelled.
/// </summary>
public sealed record QuoteRevokedEvent(
    int QuoteId,
    string Reason,
    DateTime RevokedAt) : DomainEventBase;
