// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// Tracks escalation events for service requests.
/// Used by EscalationAnalyticsService for analytics and reporting.
/// TODO-SD005-011
/// </summary>
[Table("EscalationLogs")]
public class EscalationLog : BaseEntity
{
    /// <summary>The service request that was escalated</summary>
    public int ServiceRequestId { get; set; }

    /// <summary>Navigation to the service request</summary>
    [ForeignKey("ServiceRequestId")]
    public virtual ServiceRequest? ServiceRequest { get; set; }

    /// <summary>When the escalation occurred</summary>
    public DateTime EscalatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Escalation level number</summary>
    public int LevelNumber { get; set; } = 1;

    /// <summary>Reason for escalation</summary>
    public string? Reason { get; set; }

    /// <summary>User who triggered the escalation (null for auto-escalations)</summary>
    public int? EscalatedByUserId { get; set; }

    /// <summary>Navigation to escalating user</summary>
    [ForeignKey("EscalatedByUserId")]
    public virtual User? EscalatedByUser { get; set; }

    /// <summary>Additional notes</summary>
    public string? Notes { get; set; }
}
