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

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for reading campaign conversion data.
/// </summary>
public class CampaignConversionDto
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public string? CampaignName { get; set; }
    public int? CampaignRecipientId { get; set; }
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    public int? AccountId { get; set; }
    public string? AccountName { get; set; }
    public string ConversionType { get; set; } = "Purchase";
    public decimal? ConversionValue { get; set; }
    public string ConversionCurrency { get; set; } = "USD";
    public string AttributionModel { get; set; } = "LastTouch";
    public decimal AttributionPercentage { get; set; } = 100;
    public string? ConversionData { get; set; }
    public DateTime ConvertedAt { get; set; }
    public string? ExternalOrderId { get; set; }
    public string? ExternalTransactionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new campaign conversion.
/// </summary>
public class CreateCampaignConversionDto
{
    [Required]
    public int CampaignId { get; set; }

    public int? CampaignRecipientId { get; set; }

    public int? ContactId { get; set; }

    public int? AccountId { get; set; }
    
    public int? LeadId { get; set; }
    
    public int? OpportunityId { get; set; }

    [MaxLength(100)]
    public string? ConversionType { get; set; }

    public decimal? ConversionValue { get; set; }

    [MaxLength(10)]
    public string ConversionCurrency { get; set; } = "USD";

    [MaxLength(50)]
    public string AttributionModel { get; set; } = "LastTouch";

    [Range(0, 100)]
    public decimal AttributionPercentage { get; set; } = 100;

    public string? ConversionData { get; set; }

    public DateTime? ConvertedAt { get; set; }

    [MaxLength(100)]
    public string? ExternalOrderId { get; set; }

    [MaxLength(100)]
    public string? ExternalTransactionId { get; set; }
}

/// <summary>
/// DTO for updating an existing campaign conversion.
/// </summary>
public class UpdateCampaignConversionDto
{
    public int? CampaignRecipientId { get; set; }

    public int? ContactId { get; set; }

    public int? AccountId { get; set; }

    [MaxLength(100)]
    public string? ConversionType { get; set; }

    public decimal? ConversionValue { get; set; }

    [MaxLength(10)]
    public string? ConversionCurrency { get; set; }

    [MaxLength(50)]
    public string? AttributionModel { get; set; }

    [Range(0, 100)]
    public decimal? AttributionPercentage { get; set; }

    public string? ConversionData { get; set; }

    public DateTime? ConvertedAt { get; set; }

    [MaxLength(100)]
    public string? ExternalOrderId { get; set; }

    [MaxLength(100)]
    public string? ExternalTransactionId { get; set; }
}
