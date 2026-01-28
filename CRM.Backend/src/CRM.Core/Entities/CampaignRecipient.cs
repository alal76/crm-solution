// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using System.ComponentModel.DataAnnotations;
using CRM.Core.Models;

namespace CRM.Core.Entities;

/// <summary>
/// Campaign recipient status
/// </summary>
public enum CampaignRecipientStatus
{
    Pending = 0,
    Sent = 1,
    Delivered = 2,
    Failed = 3,
    Bounced = 4,
    Opened = 5,
    Clicked = 6,
    Converted = 7,
    Unsubscribed = 8
}

/// <summary>
/// Bounce type
/// </summary>
public enum BounceType
{
    None = 0,
    Hard = 1,
    Soft = 2,
    Technical = 3
}

/// <summary>
/// Campaign recipient/member tracking
/// </summary>
public class CampaignRecipient : BaseEntity
{
    /// <summary>
    /// The campaign this recipient belongs to
    /// </summary>
    [Required]
    public int CampaignId { get; set; }
    
    /// <summary>
    /// Link to Contact if applicable
    /// </summary>
    public int? ContactId { get; set; }
    
    /// <summary>
    /// Link to Customer if applicable
    /// </summary>
    public int? CustomerId { get; set; }
    
    /// <summary>
    /// Recipient email (denormalized for performance)
    /// </summary>
    [MaxLength(255)]
    public string? Email { get; set; }
    
    /// <summary>
    /// Recipient first name (denormalized)
    /// </summary>
    [MaxLength(100)]
    public string? FirstName { get; set; }
    
    /// <summary>
    /// Recipient last name (denormalized)
    /// </summary>
    [MaxLength(100)]
    public string? LastName { get; set; }
    
    /// <summary>
    /// Recipient company (denormalized)
    /// </summary>
    [MaxLength(255)]
    public string? Company { get; set; }
    
    /// <summary>
    /// Current status
    /// </summary>
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";
    
    /// <summary>
    /// Scheduled send time
    /// </summary>
    public DateTime? SendScheduledTime { get; set; }
    
    /// <summary>
    /// Actual send time
    /// </summary>
    public DateTime? SendActualTime { get; set; }
    
    /// <summary>
    /// When email was delivered
    /// </summary>
    public DateTime? DeliveredAt { get; set; }
    
    /// <summary>
    /// First open timestamp
    /// </summary>
    public DateTime? FirstOpenedAt { get; set; }
    
    /// <summary>
    /// Last open timestamp
    /// </summary>
    public DateTime? LastOpenedAt { get; set; }
    
    /// <summary>
    /// Number of times opened
    /// </summary>
    public int OpenCount { get; set; } = 0;
    
    /// <summary>
    /// First click timestamp
    /// </summary>
    public DateTime? FirstClickedAt { get; set; }
    
    /// <summary>
    /// Last click timestamp
    /// </summary>
    public DateTime? LastClickedAt { get; set; }
    
    /// <summary>
    /// Number of clicks
    /// </summary>
    public int ClickCount { get; set; } = 0;
    
    /// <summary>
    /// When they converted
    /// </summary>
    public DateTime? ConvertedAt { get; set; }
    
    /// <summary>
    /// Value of conversion
    /// </summary>
    public decimal? ConversionValue { get; set; }
    
    /// <summary>
    /// When they unsubscribed
    /// </summary>
    public DateTime? UnsubscribedAt { get; set; }
    
    /// <summary>
    /// Type of bounce if bounced
    /// </summary>
    [MaxLength(50)]
    public string? BounceType { get; set; }
    
    /// <summary>
    /// Reason for bounce
    /// </summary>
    public string? BounceReason { get; set; }
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Personalization data snapshot as JSON
    /// </summary>
    public string? PersonalizationData { get; set; }
    
    /// <summary>
    /// A/B test variant assigned
    /// </summary>
    [MaxLength(10)]
    public string? ABTestVariant { get; set; }
    
    // Navigation properties
    public virtual MarketingCampaign? Campaign { get; set; }
    public virtual Contact? Contact { get; set; }
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<CampaignLinkClick> LinkClicks { get; set; } = new List<CampaignLinkClick>();
}
