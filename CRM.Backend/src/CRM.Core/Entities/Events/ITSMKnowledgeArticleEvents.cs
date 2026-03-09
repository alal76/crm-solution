// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Ports.Output.Events;

namespace CRM.Core.Entities.Events;

/// <summary>
/// KB-019: Raised when an ITSM knowledge article is published.
/// </summary>
public sealed record ITSMKnowledgeArticlePublishedEvent(
    int ArticleId, int PublishedByUserId, DateTime PublishedAt) : DomainEventBase;

/// <summary>
/// KB-019: Raised when an ITSM knowledge article is submitted for review.
/// </summary>
public sealed record ITSMKnowledgeArticleSubmittedForReviewEvent(
    int ArticleId, DateTime SubmittedAt) : DomainEventBase;

/// <summary>
/// KB-019: Raised when an ITSM knowledge article is approved.
/// </summary>
public sealed record ITSMKnowledgeArticleApprovedEvent(
    int ArticleId, DateTime ApprovedAt) : DomainEventBase;

/// <summary>
/// KB-019: Raised when an ITSM knowledge article is retired.
/// </summary>
public sealed record ITSMKnowledgeArticleRetiredEvent(
    int ArticleId, string Reason, DateTime RetiredAt) : DomainEventBase;
