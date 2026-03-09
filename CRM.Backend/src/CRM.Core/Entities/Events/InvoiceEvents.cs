// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Ports.Output.Events;

namespace CRM.Core.Entities.Events;

/// <summary>
/// AP-059 Phase 3: Raised when an invoice is sent to a customer.
/// </summary>
public sealed record InvoiceSentEvent(
    int InvoiceId,
    DateTime SentAt) : DomainEventBase;

/// <summary>
/// AP-059 Phase 3: Raised when an invoice is marked as paid.
/// </summary>
public sealed record InvoiceMarkedPaidEvent(
    int InvoiceId,
    decimal AmountPaid,
    DateTime PaidAt) : DomainEventBase;

/// <summary>
/// AP-059 Phase 3: Raised when an invoice is voided.
/// </summary>
public sealed record InvoiceVoidedEvent(
    int InvoiceId,
    string Reason,
    DateTime VoidedAt) : DomainEventBase;
