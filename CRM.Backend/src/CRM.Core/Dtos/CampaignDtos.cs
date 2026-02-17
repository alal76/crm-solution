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
/// DTO for marketing campaign response.
/// </summary>
public class CampaignDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CampaignCode { get; set; }
    public string? Description { get; set; }
    public int Objective { get; set; }
    public int CampaignType { get; set; }
    public int Status { get; set; }
    public int Priority { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal Budget { get; set; }
    public decimal ActualCost { get; set; }
    public decimal ActualRevenue { get; set; }
    public int LeadsGenerated { get; set; }
    public int MqlsGenerated { get; set; }
    public int SqlsGenerated { get; set; }
    public int? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public decimal Roi { get; set; }
    public decimal Cpl { get; set; }
    public decimal Cpa { get; set; }
}

/// <summary>
/// DTO for creating marketing campaign.
/// </summary>
public class CreateCampaignDto
{
    [Required(ErrorMessage = "Campaign name is required")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 255 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string? CampaignCode { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(0, 20)]
    public int? Objective { get; set; }

    [Required(ErrorMessage = "Campaign type is required")]
    [Range(0, 20)]
    public int CampaignType { get; set; }

    [Range(0, 10)]
    public int Priority { get; set; } = 1;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Budget { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ActualCost { get; set; }

    public int? OwnerId { get; set; }

    [StringLength(500)]
    public string? SegmentCriteria { get; set; }

    [StringLength(500)]
    public string? Tags { get; set; }
}

/// <summary>
/// DTO for updating marketing campaign.
/// </summary>
public class UpdateCampaignDto
{
    [StringLength(255, MinimumLength = 1)]
    public string? Name { get; set; }

    [StringLength(50)]
    public string? CampaignCode { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(0, 20)]
    public int? Objective { get; set; }

    [Range(0, 20)]
    public int? CampaignType { get; set; }

    [Range(0, 10)]
    public int? Priority { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? Budget { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? ActualCost { get; set; }

    [Range(0, int.MaxValue)]
    public int? Status { get; set; }

    public int? OwnerId { get; set; }
}

/// <summary>
/// DTO for campaign recipient.
/// </summary>
public class CampaignRecipientDto
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public int? ContactId { get; set; }
    public int? LeadId { get; set; }
    public int? AccountId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int Status { get; set; }
    public DateTime AddedAt { get; set; }
    public DateTime? EngagedAt { get; set; }
    public int Impressions { get; set; }
    public int Clicks { get; set; }
    public int Conversions { get; set; }
    public decimal Money { get; set; }
}

/// <summary>
/// DTO for adding campaign recipients.
/// </summary>
public class AddCampaignRecipientsDto
{
    [Required]
    public List<int> RecipientIds { get; set; } = new();

    [StringLength(100)]
    public string? RecipientType { get; set; } = "Contact";
}

/// <summary>
/// DTO for campaign metrics.
/// </summary>
public class CampaignMetricsDto
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public int Impressions { get; set; }
    public int Clicks { get; set; }
    public int Conversions { get; set; }
    public int LeadsGenerated { get; set; }
    public int MqlsGenerated { get; set; }
    public int SqlsGenerated { get; set; }
    public decimal ReveneGenerated { get; set; }
    public decimal Roi { get; set; }
    public decimal Cpl { get; set; }
    public decimal Cpa { get; set; }
    public decimal ClickThroughRate { get; set; }
    public decimal ConversionRate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int TotalBudget { get; set; }
    public int ActualSpend { get; set; }
    public int BudgetRemaining { get; set; }
    public decimal OpenRate { get; set; }
    public decimal ClickRate { get; set; }
    public decimal BounceRate { get; set; }
    public DateTime? CalculatedAt { get; set; }
    public int? TotalRecipients { get; set; }
    public int? SentCount { get; set; }
    public int? OpenCount { get; set; }
    public int? ClickCount { get; set; }
    public int? BounceCount { get; set; }
}

/// <summary>
/// DTO for campaign execution result.
/// </summary>
public class CampaignExecutionResultDto
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public int RecipientsCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public DateTime ExecutedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// DTO for campaign preview.
/// </summary>
public class CampaignPreviewDto
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public string? PreviewHtml { get; set; }
    public string? PreviewText { get; set; }
    public string? Subject { get; set; }
    public string? FromName { get; set; }
    public string? FromEmail { get; set; }
    public int EstimatedRecipients { get; set; }
}

/// <summary>
/// DTO for duplicating campaign.
/// </summary>
public class DuplicateCampaignDto
{
    [Required(ErrorMessage = "New campaign name is required")]
    [StringLength(255)]
    public string NewName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? NewCampaignCode { get; set; }

    public bool CopyRecipients { get; set; }
    public bool CopyMetrics { get; set; }
}

/// <summary>
/// DTO for cloning campaign.
/// </summary>
public class CloneCampaignDto
{
    [Required(ErrorMessage = "New campaign name is required")]
    [StringLength(255)]
    public string NewName { get; set; } = string.Empty;

    public bool IncludeRecipients { get; set; } = true;
    public bool ResetMetrics { get; set; } = true;
}

/// <summary>
/// DTO for scheduling campaign.
/// </summary>
public class ScheduleCampaignDto
{
    [Required]
    public DateTime ScheduledDate { get; set; }

    [Range(0, 23)]
    public int? ScheduledHour { get; set; }

    [Range(0, 59)]
    public int? ScheduledMinute { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for retargeting campaign.
/// </summary>
public class RetargetCampaignDto
{
    [Required]
    [StringLength(100)]
    public string Criteria { get; set; } = string.Empty;

    public List<int>? ExcludeRecipientIds { get; set; }
    public List<int>? IncludeRecipientIds { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for analyzing campaign.
/// </summary>
public class CampaignAnalysisDto
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public decimal Roi { get; set; }
    public decimal Cpl { get; set; }
    public decimal Cpa { get; set; }
    public double ClickThroughRate { get; set; }
    public double ConversionRate { get; set; }
    public double DateQualificationRate { get; set; }
    public double SalesQualificationRate { get; set; }
    public int TopPerformingSegment { get; set; }
    public string? Insights { get; set; }
    public List<string> Recommendations { get; set; } = new();
    public DateTime AnalyzedAt { get; set; }
}

/// <summary>
/// DTO for campaign list response with pagination.
/// </summary>
public class CampaignListDto
{
    public List<CampaignDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// DTO for campaign analysis results.
/// </summary>
public class CampaignAnalysisResultDto
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public decimal Roi { get; set; }
    public decimal Cpl { get; set; }
    public decimal Cpa { get; set; }
    public int LeadsGenerated { get; set; }
    public int MqlsGenerated { get; set; }
    public int SqlsGenerated { get; set; }
    public decimal ConversionRate { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public string? Insights { get; set; }
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// DTO for campaign metrics preview.
/// </summary>
public class CampaignMetricsPreviewDto
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public int Impressions { get; set; }
    public int Clicks { get; set; }
    public int Conversions { get; set; }
    public decimal ClickThroughRate { get; set; }
    public decimal ConversionRate { get; set; }
    public decimal Roi { get; set; }
    public int BudgetRemaining { get; set; }
}

/// <summary>
/// DTO for duplicating a campaign.
/// </summary>
public class CampaignDuplicationDto
{
    public int SourceCampaignId { get; set; }
    public int TargetCampaignId { get; set; }
    public bool CopyRecipients { get; set; } = false;
    public bool CopyMetrics { get; set; } = false;
    public bool ResetDates { get; set; } = true;
}

/// <summary>
/// DTO for campaign retargeting.
/// </summary>
public class CampaignRetargetingDto
{
    public int CampaignId { get; set; }
    
    [StringLength(500)]
    public string? Criteria { get; set; }

    public List<int>? ExcludeRecipientIds { get; set; }
}

/// <summary>
/// Result of campaign retargeting.
/// </summary>
public class CampaignRetargetingResultDto
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public int NewRecipientsCount { get; set; }
    public int ProcessedCount { get; set; }
    public int SkippedCount { get; set; }
    public DateTime RetargetedAt { get; set; }
}

