// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Ports.Output.Events;

namespace CRM.Core.Entities.Events;

/// <summary>
/// AP-059 Phase 3: Raised when a campaign is launched (activated).
/// </summary>
public sealed record CampaignLaunchedEvent(
    int CampaignId,
    DateTime LaunchedAt) : DomainEventBase;

/// <summary>
/// AP-059 Phase 3: Raised when an active campaign is paused.
/// </summary>
public sealed record CampaignPausedEvent(
    int CampaignId,
    DateTime PausedAt) : DomainEventBase;

/// <summary>
/// AP-059 Phase 3: Raised when a campaign is completed.
/// </summary>
public sealed record CampaignCompletedEvent(
    int CampaignId,
    DateTime CompletedAt) : DomainEventBase;
