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
/// Permanent unsubscribe/preference record for an email address.
/// Used by the preference-centre and public unsubscribe endpoint.
/// </summary>
public class UnsubscribeRecord : BaseEntity
{
    /// <summary>Email address that unsubscribed.</summary>
    [Required]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Reason the contact chose to unsubscribe.</summary>
    public UnsubscribeReason Reason { get; set; }

    /// <summary>Free-text note elaborating the reason.</summary>
    [MaxLength(1000)]
    public string? ReasonNote { get; set; }

    /// <summary>FK to the campaign the unsubscribe originated from (if any).</summary>
    public int? CampaignId { get; set; }

    /// <summary>Navigation: the originating campaign.</summary>
    public MarketingCampaign? Campaign { get; set; }

    /// <summary>Signed token embedded in the unsubscribe link.</summary>
    [MaxLength(500)]
    public string? Token { get; set; }

    /// <summary>UTC timestamp when the unsubscribe occurred.</summary>
    public DateTime UnsubscribedAt { get; set; }

    /// <summary>Whether the contact still wishes to receive product-update emails.</summary>
    public bool ReceiveProductUpdates { get; set; } = false;

    /// <summary>
    /// Whether the contact receives transactional emails.
    /// Cannot be unsubscribed from transactional messages.
    /// </summary>
    public bool ReceiveTransactional { get; set; } = true;
}
