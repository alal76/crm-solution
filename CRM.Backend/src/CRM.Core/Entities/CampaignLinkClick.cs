// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

/// <summary>
/// Tracks link clicks from campaign emails
/// </summary>
public class CampaignLinkClick : BaseEntity
{
    /// <summary>
    /// The recipient who clicked
    /// </summary>
    [Required]
    public int CampaignRecipientId { get; set; }
    
    /// <summary>
    /// The campaign
    /// </summary>
    [Required]
    public int CampaignId { get; set; }
    
    /// <summary>
    /// The URL that was clicked
    /// </summary>
    [Required]
    public string LinkUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Label/name of the link
    /// </summary>
    [MaxLength(255)]
    public string? LinkLabel { get; set; }
    
    /// <summary>
    /// When the click occurred
    /// </summary>
    public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// User agent string
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// IP address
    /// </summary>
    [MaxLength(50)]
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// Device type (Desktop, Mobile, Tablet)
    /// </summary>
    [MaxLength(50)]
    public string? DeviceType { get; set; }
    
    /// <summary>
    /// Browser name
    /// </summary>
    [MaxLength(100)]
    public string? Browser { get; set; }
    
    /// <summary>
    /// Operating system
    /// </summary>
    [MaxLength(100)]
    public string? OperatingSystem { get; set; }
    
    /// <summary>
    /// Location data as JSON
    /// </summary>
    public string? LocationData { get; set; }
    
    // Navigation properties
    public virtual CampaignRecipient? CampaignRecipient { get; set; }
    public virtual MarketingCampaign? Campaign { get; set; }
}
