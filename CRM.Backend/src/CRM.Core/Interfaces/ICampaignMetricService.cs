// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing campaign metrics and analytics.
/// </summary>
public interface ICampaignMetricService
{
    /// <summary>
    /// Create a new campaign metric record.
    /// </summary>
    Task<CampaignMetric> CreateAsync(CampaignMetric metric, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get metrics for a campaign.
    /// </summary>
    Task<CampaignMetricsDto?> GetMetricsAsync(int campaignId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze campaign performance.
    /// </summary>
    Task<CampaignAnalysisResultDto?> AnalyzeAsync(CampaignAnalysisDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get metrics preview for a campaign.
    /// </summary>
    Task<CampaignMetricsPreviewDto?> PreviewAsync(CampaignPreviewDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Duplicate campaign metrics.
    /// </summary>
    Task<CampaignMetricsDto?> DuplicateAsync(CampaignDuplicationDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retarget a campaign to a new audience.
    /// </summary>
    Task<CampaignRetargetingResultDto> RetargetAsync(CampaignRetargetingDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all campaigns with pagination.
    /// </summary>
    Task<PaginatedDto<CampaignMetricsDto>> GetAllMetricsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
}
