// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

/// <summary>
/// Campaign metric entity for tracking campaign performance
/// </summary>
public class CampaignMetric : BaseEntity
{
    public int CampaignId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public double MetricValue { get; set; } = 0;
    public DateTime RecordedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Total number of emails sent</summary>
    public int TotalSent { get; set; } = 0;

    /// <summary>Total number of emails delivered</summary>
    public int TotalDelivered { get; set; } = 0;

    /// <summary>Total number of emails opened</summary>
    public int TotalOpened { get; set; } = 0;

    /// <summary>Total number of links clicked</summary>
    public int TotalClicked { get; set; } = 0;

    /// <summary>Total conversions</summary>
    public int TotalConverted { get; set; } = 0;

    /// <summary>Open rate percentage (calculated)</summary>
    public decimal? OpenRate 
    { 
        get
        {
            if (TotalDelivered == 0) return 0;
            return (decimal)TotalOpened / TotalDelivered;
        }
    }

    // Navigation properties
    public MarketingCampaign? Campaign { get; set; }
}
