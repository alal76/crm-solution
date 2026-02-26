// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Enums;
using CRM.Core.Models;

namespace CRM.Core.Entities;

/// <summary>
/// Represents a customer satisfaction survey sent to a contact or account.
/// Supports CSAT, NPS, and CES survey types.
/// </summary>
public class SatisfactionSurvey : BaseEntity
{
    /// <summary>Entity type this survey relates to (e.g. "ServiceRequest", "Account").</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>ID of the related entity.</summary>
    public int EntityId { get; set; }

    /// <summary>Survey type: CSAT, NPS, or CES.</summary>
    public SurveyType Type { get; set; }

    /// <summary>Current lifecycle status of the survey.</summary>
    public SurveyStatus Status { get; set; } = SurveyStatus.Pending;

    /// <summary>FK to the contact who receives the survey (optional).</summary>
    public int? ContactId { get; set; }

    /// <summary>FK to the account associated with this survey (optional).</summary>
    public int? AccountId { get; set; }

    /// <summary>Timestamp when the survey was dispatched.</summary>
    public DateTime? SentAt { get; set; }

    /// <summary>Timestamp when a response was received.</summary>
    public DateTime? ResponseReceivedAt { get; set; }

    /// <summary>Expiry timestamp after which the survey link is invalid.</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Unique 32-character hex token used in public survey URLs.</summary>
    public string? ExternalToken { get; set; }

    /// <summary>Email subject line for the survey invitation.</summary>
    public string? Subject { get; set; }

    // ── Navigation properties ─────────────────────────────────────────────────

    /// <summary>Contact who is targeted by this survey.</summary>
    public virtual Contact? Contact { get; set; }

    /// <summary>Account associated with this survey.</summary>
    public virtual Account? Account { get; set; }

    /// <summary>The single response submitted for this survey.</summary>
    public virtual SatisfactionResponse? Response { get; set; }
}
