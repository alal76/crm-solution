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

using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing campaign metrics and analytics.
/// </summary>
public interface ICampaignMetricService
{
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
