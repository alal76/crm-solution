// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Dtos;

/// <summary>
/// Email digest configuration for the current user (REV-FE-002).
/// Shape matches EmailDigestConfig in CRM.Frontend/src/pages/EmailDigestPage.tsx exactly
/// (camelCase over the wire via the API's default JSON naming policy).
/// </summary>
public class EmailDigestConfigDto
{
    public bool Enabled { get; set; }

    /// <summary>"daily" | "weekly" | "monthly"</summary>
    public string Frequency { get; set; } = "daily";

    /// <summary>0-6 (Sunday-Saturday). Present only when Frequency is "weekly".</summary>
    public int? DayOfWeek { get; set; }

    /// <summary>1-31. Present only when Frequency is "monthly".</summary>
    public int? DayOfMonth { get; set; }

    /// <summary>HH:mm 24-hour local time.</summary>
    public string TimeOfDay { get; set; } = "08:00";

    /// <summary>IANA timezone identifier.</summary>
    public string Timezone { get; set; } = "UTC";

    public EmailDigestSectionsDto Sections { get; set; } = new();
}

/// <summary>Content-section toggles for the email digest, matching EmailDigestConfig.sections on the frontend.</summary>
public class EmailDigestSectionsDto
{
    public bool NewLeads { get; set; } = true;

    public bool OpenOpportunities { get; set; } = true;

    public bool RecentActivities { get; set; } = true;

    public bool UpcomingTasks { get; set; } = true;

    public bool OverdueTasks { get; set; } = true;

    public bool TeamPerformance { get; set; }

    public bool KpiSummary { get; set; }
}
