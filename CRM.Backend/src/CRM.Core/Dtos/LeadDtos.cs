// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#pragma warning disable SA1649 // file name should match first type name
namespace CRM.Core.Dtos;

/// <summary>
/// Summary projection of a Lead — used in list and status-filtered responses.
/// </summary>
public class LeadSummaryDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? CompanyName { get; set; }
    public string? Title { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public int Score { get; set; }
    public int FitScore { get; set; }
    public int EngagementScore { get; set; }
    public int? OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Full detail projection of a Lead — used in single-record GET responses.
/// </summary>
public class LeadDto : LeadSummaryDto
{
    public string? QualificationNotes { get; set; }
    public string? Region { get; set; }
    public string? Website { get; set; }
    public string? Tags { get; set; }
    public int? AccountId { get; set; }
    public int? ContactId { get; set; }
    public int? CampaignId { get; set; }
    public DateTime? MqlDate { get; set; }
    public DateTime? SqlDate { get; set; }
    public DateTime? LastActivityDate { get; set; }
}

/// <summary>
/// Analytics for a single lead source channel.
/// Returns lead counts grouped by source with conversion rates (TODO-CRM002-03).
/// </summary>
public class LeadSourceAnalyticsDto
{
    /// <summary>Display name of the source channel (e.g. "Web", "Referral").</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>Total non-deleted leads from this source.</summary>
    public int TotalLeads { get; set; }

    /// <summary>Leads that reached Converted status.</summary>
    public int ConvertedLeads { get; set; }

    /// <summary>Leads that reached Qualified status.</summary>
    public int QualifiedLeads { get; set; }

    /// <summary>Leads that were Disqualified.</summary>
    public int DisqualifiedLeads { get; set; }

    /// <summary>Percentage of leads converted (0–100).</summary>
    public decimal ConversionRate { get; set; }

    /// <summary>Average lead score for this source.</summary>
    public double AverageScore { get; set; }
}

/// <summary>
/// UTM attribution breakdown for lead acquisition.
/// Groups leads by UTM source / medium / campaign (TODO-CRM002-03).
/// </summary>
public class LeadAttributionDto
{
    /// <summary>UTM source parameter value (may be null for direct/unattributed).</summary>
    public string? UtmSource { get; set; }

    /// <summary>UTM medium parameter value.</summary>
    public string? UtmMedium { get; set; }

    /// <summary>UTM campaign parameter value.</summary>
    public string? UtmCampaign { get; set; }

    /// <summary>Total leads with this attribution combination.</summary>
    public int TotalLeads { get; set; }

    /// <summary>Leads that reached Converted status.</summary>
    public int ConvertedLeads { get; set; }

    /// <summary>Percentage of leads converted (0–100).</summary>
    public decimal ConversionRate { get; set; }

    /// <summary>Average lead score for this attribution group.</summary>
    public double AverageScore { get; set; }
}
