// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.ComponentModel.DataAnnotations;
using CRM.Core.Models;

namespace CRM.Core.Entities;

/// <summary>
/// Records a single click on a UTM-tagged tracking link.
/// Created by UtmTrackingService when a tracked URL is resolved.
/// </summary>
public class UtmLinkClick : BaseEntity
{
    /// <summary>UTM source parameter (e.g., "newsletter").</summary>
    [MaxLength(200)]
    public string? UtmSource { get; set; }

    /// <summary>UTM medium parameter (e.g., "email").</summary>
    [MaxLength(200)]
    public string? UtmMedium { get; set; }

    /// <summary>UTM campaign parameter.</summary>
    [MaxLength(200)]
    public string? UtmCampaign { get; set; }

    /// <summary>UTM content parameter (used for A/B creative variant).</summary>
    [MaxLength(200)]
    public string? UtmContent { get; set; }

    /// <summary>UTM term parameter (paid search keyword).</summary>
    [MaxLength(200)]
    public string? UtmTerm { get; set; }

    /// <summary>The original destination URL before UTM params were appended.</summary>
    [MaxLength(2048)]
    public string? OriginalUrl { get; set; }

    /// <summary>The final URL the visitor landed on (after redirect).</summary>
    [MaxLength(2048)]
    public string? LandingUrl { get; set; }

    /// <summary>IP address of the visitor.</summary>
    [MaxLength(45)]
    public string? VisitorIp { get; set; }

    /// <summary>User-agent of the visitor's browser.</summary>
    [MaxLength(500)]
    public string? VisitorUserAgent { get; set; }

    /// <summary>FK to the lead associated with this click (resolved after form-fill).</summary>
    public int? LeadId { get; set; }

    /// <summary>FK to the CampaignTrackingLink that was clicked.</summary>
    public int? TrackingLinkId { get; set; }

    /// <summary>UTC timestamp when the click occurred.</summary>
    public DateTime ClickedAt { get; set; }
}
