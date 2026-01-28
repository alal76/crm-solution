// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using System.ComponentModel.DataAnnotations;
using CRM.Core.Models;

namespace CRM.Core.Entities;

/// <summary>
/// Conversion type
/// </summary>
public enum ConversionType
{
    Purchase = 0,
    Signup = 1,
    Download = 2,
    FormSubmit = 3,
    Demo = 4,
    Trial = 5,
    Subscription = 6,
    Quote = 7,
    Meeting = 8,
    Custom = 9
}

/// <summary>
/// Attribution model
/// </summary>
public enum AttributionModel
{
    FirstTouch = 0,
    LastTouch = 1,
    Linear = 2,
    TimeDecay = 3,
    PositionBased = 4,
    Custom = 5
}

/// <summary>
/// Campaign conversion tracking
/// </summary>
public class CampaignConversion : BaseEntity
{
    /// <summary>
    /// The campaign
    /// </summary>
    [Required]
    public int CampaignId { get; set; }
    
    /// <summary>
    /// The recipient if applicable
    /// </summary>
    public int? CampaignRecipientId { get; set; }
    
    /// <summary>
    /// The contact if applicable
    /// </summary>
    public int? ContactId { get; set; }
    
    /// <summary>
    /// The customer if applicable
    /// </summary>
    public int? CustomerId { get; set; }
    
    /// <summary>
    /// Type of conversion
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ConversionType { get; set; } = "Purchase";
    
    /// <summary>
    /// Value of the conversion
    /// </summary>
    public decimal? ConversionValue { get; set; }
    
    /// <summary>
    /// Currency of the conversion value
    /// </summary>
    [MaxLength(10)]
    public string ConversionCurrency { get; set; } = "USD";
    
    /// <summary>
    /// Attribution model used
    /// </summary>
    [MaxLength(50)]
    public string AttributionModel { get; set; } = "LastTouch";
    
    /// <summary>
    /// Percentage attributed to this campaign
    /// </summary>
    public decimal AttributionPercentage { get; set; } = 100;
    
    /// <summary>
    /// Additional conversion data as JSON
    /// </summary>
    public string? ConversionData { get; set; }
    
    /// <summary>
    /// When the conversion occurred
    /// </summary>
    public DateTime ConvertedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// External order ID reference
    /// </summary>
    [MaxLength(100)]
    public string? ExternalOrderId { get; set; }
    
    /// <summary>
    /// External transaction ID reference
    /// </summary>
    [MaxLength(100)]
    public string? ExternalTransactionId { get; set; }
    
    // Navigation properties
    public virtual MarketingCampaign? Campaign { get; set; }
    public virtual CampaignRecipient? CampaignRecipient { get; set; }
    public virtual Contact? Contact { get; set; }
    public virtual Customer? Customer { get; set; }
}
