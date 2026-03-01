// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using CRM.Core.Models;

namespace CRM.Core.Entities;

/// <summary>
/// A UTM-tagged link associated with a campaign.
/// Each link gets a short tracking token appended so clicks can be captured.
/// </summary>
public class CampaignTrackingLink : BaseEntity
{
    /// <summary>FK to the parent campaign.</summary>
    public int CampaignId { get; set; }

    /// <summary>Navigation: the parent campaign.</summary>
    public MarketingCampaign Campaign { get; set; } = null!;

    /// <summary>The original destination URL.</summary>
    [Required]
    [MaxLength(2048)]
    public string OriginalUrl { get; set; } = string.Empty;

    /// <summary>
    /// The tracked URL — original URL with UTM params appended
    /// plus a short token path segment for click capture.
    /// </summary>
    [Required]
    [MaxLength(2048)]
    public string TrackedUrl { get; set; } = string.Empty;

    /// <summary>Human-readable alias for the link (e.g., "Hero CTA").</summary>
    [MaxLength(100)]
    public string? LinkAlias { get; set; }

    /// <summary>UTM source parameter.</summary>
    [MaxLength(200)]
    public string? UtmSource { get; set; }

    /// <summary>UTM medium parameter.</summary>
    [MaxLength(200)]
    public string? UtmMedium { get; set; }

    /// <summary>UTM campaign parameter.</summary>
    [MaxLength(200)]
    public string? UtmCampaign { get; set; }

    /// <summary>UTM content parameter.</summary>
    [MaxLength(200)]
    public string? UtmContent { get; set; }

    /// <summary>Short, URL-safe token embedded in the tracked URL.</summary>
    [MaxLength(50)]
    public string? TrackingToken { get; set; }

    /// <summary>Total number of clicks recorded for this link.</summary>
    public int ClickCount { get; set; }
}
