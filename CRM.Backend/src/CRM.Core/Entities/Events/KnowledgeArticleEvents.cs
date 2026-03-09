// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Ports.Output.Events;

namespace CRM.Core.Entities.Events;

/// <summary>
/// AP-059 Phase 3: Raised when a knowledge article is published.
/// </summary>
public sealed record KnowledgeArticlePublishedEvent(
    int ArticleId,
    DateTime PublishedAt) : DomainEventBase;

/// <summary>
/// AP-059 Phase 3: Raised when a knowledge article is archived.
/// </summary>
public sealed record KnowledgeArticleArchivedEvent(
    int ArticleId,
    DateTime ArchivedAt) : DomainEventBase;
