// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
