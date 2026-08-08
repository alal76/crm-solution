// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

/// <summary>
/// Frequency at which a user's email digest is sent.
/// </summary>
public enum EmailDigestFrequency
{
    Daily = 0,
    Weekly = 1,
    Monthly = 2
}

/// <summary>
/// Stores a single user's email digest configuration (REV-FE-002).
/// One row per user. Mirrors the shape of EmailDigestConfig on
/// CRM.Frontend/src/pages/EmailDigestPage.tsx exactly so the DTO can be a
/// near 1:1 passthrough.
/// </summary>
public class EmailDigestConfig : BaseEntity
{
    /// <summary>ID of the user this digest configuration belongs to.</summary>
    public int UserId { get; set; }

    /// <summary>Whether the digest is enabled for this user.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>How often the digest should be sent.</summary>
    public EmailDigestFrequency Frequency { get; set; } = EmailDigestFrequency.Daily;

    /// <summary>Day of week (0=Sunday .. 6=Saturday) for weekly digests. Null unless Frequency=Weekly.</summary>
    public int? DayOfWeek { get; set; }

    /// <summary>Day of month (1-31) for monthly digests. Null unless Frequency=Monthly.</summary>
    public int? DayOfMonth { get; set; }

    /// <summary>Local time of day the digest should be sent.</summary>
    public TimeSpan TimeOfDay { get; set; } = new TimeSpan(8, 0, 0);

    /// <summary>IANA timezone identifier (e.g. America/New_York) the TimeOfDay/DayOfWeek/DayOfMonth are relative to.</summary>
    public string Timezone { get; set; } = "UTC";

    // === Content sections (mirrors EmailDigestConfig.sections in EmailDigestPage.tsx) ===

    public bool IncludeNewLeads { get; set; } = true;

    public bool IncludeOpenOpportunities { get; set; } = true;

    public bool IncludeRecentActivities { get; set; } = true;

    public bool IncludeUpcomingTasks { get; set; } = true;

    public bool IncludeOverdueTasks { get; set; } = true;

    /// <summary>Managers-only section: team metrics/leaderboard. See EmailDigestService for v1 scope notes.</summary>
    public bool IncludeTeamPerformance { get; set; }

    /// <summary>KPI snapshot section. See EmailDigestService for v1 scope notes.</summary>
    public bool IncludeKpiSummary { get; set; }

    /// <summary>
    /// UTC timestamp of the last time a scheduled (non-preview) digest was successfully sent.
    /// Used by EmailDigestJob to avoid double-sending within the same due hour and as the
    /// "since" boundary for content that reports on activity "since the last digest".
    /// </summary>
    public DateTime? LastSentAt { get; set; }

    // Navigation
    public virtual User? User { get; set; }
}
