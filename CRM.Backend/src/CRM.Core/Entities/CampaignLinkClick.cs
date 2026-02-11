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
