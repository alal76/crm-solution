// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// Tracks every score change for a lead, enabling history charts and trend analysis.
/// Does not inherit BaseEntity (no soft-delete, no RowVersion) — append-only audit log.
/// FEAT-AISCORING: AI Lead Scoring Real-time Triggers
/// </summary>
public class LeadScoreHistory
{
    /// <summary>Primary key (auto-increment).</summary>
    public int Id { get; set; }

    /// <summary>Foreign key to the Lead entity.</summary>
    public int LeadId { get; set; }

    /// <summary>The new overall fit score (0-100) after this change.</summary>
    public int Score { get; set; }

    /// <summary>The score before this change (0-100).</summary>
    public int PreviousScore { get; set; }

    /// <summary>Score delta = Score - PreviousScore (can be negative on decay).</summary>
    public int Delta { get; set; }

    /// <summary>Short reason for the change (max 200 chars): "auto_score", "decay", "manual", "lead_updated".</summary>
    [MaxLength(200)]
    public string Reason { get; set; } = string.Empty;

    /// <summary>JSON snapshot of individual score components at the time of scoring (max 2000 chars, nullable).</summary>
    [MaxLength(2000)]
    public string? ScoreComponentsJson { get; set; }

    /// <summary>UTC timestamp when this score was recorded.</summary>
    public DateTime ScoredAt { get; set; }

    /// <summary>Actor that triggered the change: "system", "user", or "decay".</summary>
    [MaxLength(20)]
    public string ScoredBy { get; set; } = "system";

    /// <summary>Navigation property to the parent Lead.</summary>
    [ForeignKey(nameof(LeadId))]
    public Lead? Lead { get; set; }
}
