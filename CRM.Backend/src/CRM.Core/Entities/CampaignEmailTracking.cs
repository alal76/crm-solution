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
/// Records a single email tracking event (open, click, bounce, etc.) per send.
/// Populated from email provider webhooks.
/// </summary>
public class CampaignEmailTracking : BaseEntity
{
    /// <summary>FK to the campaign that sent the email.</summary>
    public int? CampaignId { get; set; }

    /// <summary>Navigation: the campaign.</summary>
    public MarketingCampaign? Campaign { get; set; }

    /// <summary>FK to the nurture enrollment that triggered this send.</summary>
    public int? EnrollmentId { get; set; }

    /// <summary>Navigation: the nurture enrollment.</summary>
    public NurtureEnrollment? Enrollment { get; set; }

    /// <summary>Email address of the recipient.</summary>
    [Required]
    [MaxLength(320)]
    public string RecipientEmail { get; set; } = string.Empty;

    /// <summary>The tracking event that occurred.</summary>
    public EmailTrackingEvent Event { get; set; }

    /// <summary>URL that was clicked (for Click events).</summary>
    [MaxLength(2048)]
    public string? ClickedUrl { get; set; }

    /// <summary>User-agent string from the tracking pixel/redirect.</summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>IP address captured at event time.</summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>Provider-assigned message/send ID for correlation.</summary>
    [MaxLength(200)]
    public string? MessageId { get; set; }

    /// <summary>UTC timestamp when the event occurred.</summary>
    public DateTime EventAt { get; set; }
}
