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
/// Lead Source Configuration Entity (TODO-CRM002-03)
/// Tracks lead attribution sources with cost tracking and unique tracking codes.
/// </summary>
public class LeadSourceConfig : BaseEntity
{
    /// <summary>
    /// Display name of the lead source
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether this lead source is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Cost per lead acquired from this source (for ROI tracking)
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal? CostPerLead { get; set; }

    /// <summary>
    /// Unique tracking code for attribution (e.g., UTM source parameter)
    /// </summary>
    [MaxLength(100)]
    public string? TrackingCode { get; set; }

    /// <summary>
    /// Description of the lead source
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Category of the lead source (e.g., Organic, Paid, Referral)
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// Parent campaign ID if this source is linked to a campaign
    /// </summary>
    public int? CampaignId { get; set; }

    /// <summary>
    /// Navigation property to campaign
    /// </summary>
    [ForeignKey("CampaignId")]
    public virtual MarketingCampaign? Campaign { get; set; }

    /// <summary>
    /// Leads from this source
    /// </summary>
    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();
}
