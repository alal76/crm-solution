// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Ports.Output.Events;

namespace CRM.Core.Entities.Events;

/// <summary>
/// AP-059: Raised when an opportunity moves to a different pipeline stage.
/// </summary>
public sealed record OpportunityStageChangedEvent(
    int OpportunityId,
    OpportunityStage OldStage,
    OpportunityStage NewStage,
    int Probability) : DomainEventBase;

/// <summary>
/// AP-059: Raised when an opportunity is closed (won or lost).
/// </summary>
public sealed record OpportunityClosedEvent(
    int OpportunityId,
    OpportunityStage FinalStage,
    string? Reason,
    int? CompetitorId) : DomainEventBase;

/// <summary>
/// AP-059: Raised when the expected revenue or close date of an opportunity changes.
/// </summary>
public sealed record OpportunityRevenueUpdatedEvent(
    int OpportunityId,
    decimal Amount,
    DateTime ExpectedCloseDate) : DomainEventBase;
