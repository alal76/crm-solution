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
/// Lead source channel for attribution tracking.
/// Examples: Google Ads, LinkedIn, Webinar, Referral Program
/// TODO-CRM002-03: Lead source tracking and attribution
/// </summary>
public enum LeadSourceChannel
{
    /// <summary>Paid search advertising</summary>
    PaidSearch = 0,

    /// <summary>Organic search results</summary>
    OrganicSearch = 1,

    /// <summary>Social media channels</summary>
    Social = 2,

    /// <summary>Email marketing</summary>
    Email = 3,

    /// <summary>Direct website visit</summary>
    Direct = 4,

    /// <summary>Partner or affiliate referral</summary>
    Partner = 5,

    /// <summary>Customer referral</summary>
    Referral = 6,

    /// <summary>Events and webinars</summary>
    Event = 7,

    /// <summary>Content marketing</summary>
    Content = 8,

    /// <summary>Display advertising</summary>
    Display = 9,

    /// <summary>Offline channels</summary>
    Offline = 10,

    /// <summary>Other or unknown</summary>
    Other = 99
}

/// <summary>
/// Lead source entity for detailed attribution tracking.
/// Tracks where leads originate from with cost and campaign data.
/// TODO-CRM002-03: Lead source tracking and attribution
/// </summary>
[Table("LeadSources")]
public class LeadSourceEntity : BaseEntity
{
    /// <summary>Source name (e.g., "Google Ads - Brand Campaign")</summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Unique source code for identification</summary>
    [MaxLength(50)]
    public string? Code { get; set; }

    /// <summary>Description of the lead source</summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>Source channel category</summary>
    public LeadSourceChannel Channel { get; set; } = LeadSourceChannel.Other;

    /// <summary>Medium (e.g., cpc, email, social)</summary>
    [MaxLength(100)]
    public string? Medium { get; set; }

    /// <summary>Campaign name or ID</summary>
    [MaxLength(255)]
    public string? CampaignName { get; set; }

    /// <summary>Cost per lead (for paid sources)</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? CostPerLead { get; set; }

    /// <summary>Total spend on this source</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? TotalSpend { get; set; }

    /// <summary>Whether this source is currently active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>External tracking URL or UTM parameters</summary>
    [MaxLength(500)]
    public string? TrackingUrl { get; set; }

    /// <summary>Integration platform ID (e.g., Google Ads account ID)</summary>
    [MaxLength(100)]
    public string? ExternalPlatformId { get; set; }

    #region Navigation Properties

    /// <summary>Leads from this source</summary>
    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();

    #endregion
}
