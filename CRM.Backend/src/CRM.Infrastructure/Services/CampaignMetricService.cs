// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of ICampaignMetricService for campaign metrics management.
/// Handles metrics retrieval, analysis, preview, duplication, and retargeting.
/// </summary>
public class CampaignMetricService : ICampaignMetricService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<CampaignMetricService> _logger;

    public CampaignMetricService(ICrmDbContext context, ILogger<CampaignMetricService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<CampaignMetric> CreateAsync(CampaignMetric metric, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating campaign metric for campaign {CampaignId}", metric.CampaignId);

        metric.CreatedAt = DateTime.UtcNow;
        metric.UpdatedAt = DateTime.UtcNow;
        metric.IsDeleted = false;

        _context.CampaignMetrics.Add(metric);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created campaign metric {MetricId} for campaign {CampaignId}", metric.Id, metric.CampaignId);
        return metric;
    }

    /// <inheritdoc />
    public async Task<CampaignMetricsDto?> GetMetricsAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        var campaign = await _context.MarketingCampaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted, cancellationToken);

        if (campaign == null)
        {
            _logger.LogWarning("Campaign {CampaignId} not found", campaignId);
            return null;
        }

        // Get campaign metrics from CampaignMetrics table
        var metrics = await _context.CampaignMetrics
            .Where(m => m.CampaignId == campaignId && !m.IsDeleted)
            .OrderByDescending(m => m.RecordedDate)
            .FirstOrDefaultAsync(cancellationToken);

        // Get recipient stats for additional metrics
        var recipients = await _context.CampaignRecipients
            .Where(r => r.CampaignId == campaignId && !r.IsDeleted)
            .ToListAsync(cancellationToken);

        var totalRecipients = recipients.Count;
        var sentCount = recipients.Count(r => r.SendActualTime.HasValue);
        var openedCount = recipients.Count(r => r.FirstOpenedAt.HasValue);
        var clickedCount = recipients.Count(r => r.FirstClickedAt.HasValue);
        var bouncedCount = recipients.Count(r => r.Status == CampaignRecipientStatus.Bounced.ToString());

        var dto = new CampaignMetricsDto
        {
            CampaignId = campaignId,
            CampaignName = campaign.Name,
            Impressions = metrics?.TotalSent ?? totalRecipients,
            Clicks = metrics?.TotalClicked ?? clickedCount,
            Conversions = metrics?.TotalConverted ?? 0,
            LeadsGenerated = 0,
            MqlsGenerated = 0,
            SqlsGenerated = 0,
            ReveneGenerated = 0,
            Roi = 0,
            Cpl = 0,
            Cpa = 0,
            ClickThroughRate = totalRecipients > 0 ? (clickedCount * 100m / totalRecipients) : 0m,
            ConversionRate = sentCount > 0 ? ((metrics?.TotalConverted ?? 0) * 100m / sentCount) : 0m,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            TotalBudget = (int)campaign.Budget,
            ActualSpend = (int)campaign.ActualCost,
            BudgetRemaining = (int)(campaign.Budget - campaign.ActualCost),
            OpenRate = sentCount > 0 ? (openedCount * 100m / sentCount) : 0m,
            ClickRate = openedCount > 0 ? (clickedCount * 100m / openedCount) : 0m,
            BounceRate = sentCount > 0 ? (bouncedCount * 100m / sentCount) : 0m,
            CalculatedAt = DateTime.UtcNow,
            TotalRecipients = totalRecipients,
            SentCount = sentCount,
            OpenCount = openedCount,
            ClickCount = clickedCount,
            BounceCount = bouncedCount
        };

        _logger.LogInformation("Retrieved metrics for campaign {CampaignId}", campaignId);
        return dto;
    }

    /// <inheritdoc />
    public async Task<CampaignAnalysisResultDto?> AnalyzeAsync(CampaignAnalysisDto dto, CancellationToken cancellationToken = default)
    {
        var campaign = await _context.MarketingCampaigns
            .FirstOrDefaultAsync(c => c.Id == dto.CampaignId && !c.IsDeleted, cancellationToken);

        if (campaign == null)
        {
            _logger.LogWarning("Campaign {CampaignId} not found for analysis", dto.CampaignId);
            return null;
        }

        var metrics = await GetMetricsAsync(dto.CampaignId, cancellationToken);

        var result = new CampaignAnalysisResultDto
        {
            CampaignId = dto.CampaignId,
            CampaignName = campaign.Name,
            Roi = dto.Roi,
            Cpl = dto.Cpl,
            Cpa = dto.Cpa,
            LeadsGenerated = metrics?.LeadsGenerated ?? 0,
            MqlsGenerated = metrics?.MqlsGenerated ?? 0,
            SqlsGenerated = metrics?.SqlsGenerated ?? 0,
            ConversionRate = (decimal)dto.ConversionRate,
            AnalyzedAt = DateTime.UtcNow,
            Insights = GenerateInsights(metrics),
            Recommendations = GenerateRecommendations(metrics)
        };

        _logger.LogInformation("Analyzed campaign {CampaignId}", dto.CampaignId);
        return result;
    }

    /// <inheritdoc />
    public async Task<CampaignMetricsPreviewDto?> PreviewAsync(CampaignPreviewDto dto, CancellationToken cancellationToken = default)
    {
        var campaign = await _context.MarketingCampaigns
            .FirstOrDefaultAsync(c => c.Id == dto.CampaignId && !c.IsDeleted, cancellationToken);

        if (campaign == null)
        {
            _logger.LogWarning("Campaign {CampaignId} not found for preview", dto.CampaignId);
            return null;
        }

        var recipientCount = await _context.CampaignRecipients
            .CountAsync(r => r.CampaignId == dto.CampaignId && !r.IsDeleted, cancellationToken);

        var preview = new CampaignMetricsPreviewDto
        {
            CampaignId = dto.CampaignId,
            CampaignName = campaign.Name,
            Impressions = recipientCount,
            Clicks = 0,
            Conversions = 0,
            ClickThroughRate = 0,
            ConversionRate = 0,
            Roi = 0,
            BudgetRemaining = (int)(campaign.Budget - campaign.ActualCost)
        };

        _logger.LogInformation("Generated preview for campaign {CampaignId}", dto.CampaignId);
        return preview;
    }

    /// <inheritdoc />
    public async Task<CampaignMetricsDto?> DuplicateAsync(CampaignDuplicationDto dto, CancellationToken cancellationToken = default)
    {
        var sourceCampaign = await _context.MarketingCampaigns
            .FirstOrDefaultAsync(c => c.Id == dto.SourceCampaignId && !c.IsDeleted, cancellationToken);

        if (sourceCampaign == null)
        {
            _logger.LogWarning("Source campaign {CampaignId} not found for duplication", dto.SourceCampaignId);
            return null;
        }

        var targetCampaign = await _context.MarketingCampaigns
            .FirstOrDefaultAsync(c => c.Id == dto.TargetCampaignId && !c.IsDeleted, cancellationToken);

        if (targetCampaign == null)
        {
            _logger.LogWarning("Target campaign {CampaignId} not found for duplication", dto.TargetCampaignId);
            return null;
        }

        if (dto.CopyMetrics)
        {
            // Copy metrics from source to target campaign
            var sourceMetrics = await _context.CampaignMetrics
                .Where(m => m.CampaignId == dto.SourceCampaignId && !m.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var sourceMetric in sourceMetrics)
            {
                var newMetric = new CampaignMetric
                {
                    CampaignId = dto.TargetCampaignId,
                    MetricName = sourceMetric.MetricName,
                    MetricValue = dto.ResetDates ? 0 : sourceMetric.MetricValue,
                    RecordedDate = dto.ResetDates ? DateTime.UtcNow : sourceMetric.RecordedDate,
                    TotalSent = dto.ResetDates ? 0 : sourceMetric.TotalSent,
                    TotalDelivered = dto.ResetDates ? 0 : sourceMetric.TotalDelivered,
                    TotalOpened = dto.ResetDates ? 0 : sourceMetric.TotalOpened,
                    TotalClicked = dto.ResetDates ? 0 : sourceMetric.TotalClicked,
                    TotalConverted = dto.ResetDates ? 0 : sourceMetric.TotalConverted,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.CampaignMetrics.Add(newMetric);
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Duplicated {Count} metrics from campaign {SourceId} to {TargetId}",
                sourceMetrics.Count, dto.SourceCampaignId, dto.TargetCampaignId);
        }

        // Return metrics for the target campaign
        return await GetMetricsAsync(dto.TargetCampaignId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CampaignRetargetingResultDto> RetargetAsync(CampaignRetargetingDto dto, CancellationToken cancellationToken = default)
    {
        var campaign = await _context.MarketingCampaigns
            .FirstOrDefaultAsync(c => c.Id == dto.CampaignId && !c.IsDeleted, cancellationToken);

        var result = new CampaignRetargetingResultDto
        {
            CampaignId = dto.CampaignId,
            CampaignName = campaign?.Name ?? string.Empty,
            RetargetedAt = DateTime.UtcNow
        };

        if (campaign == null)
        {
            _logger.LogWarning("Campaign {CampaignId} not found for retargeting", dto.CampaignId);
            return result;
        }

        // Get recipients to retarget (non-converters without clicks)
        var query = _context.CampaignRecipients
            .Where(r => r.CampaignId == dto.CampaignId && !r.IsDeleted && r.FirstClickedAt == null);

        // Exclude specified recipients
        if (dto.ExcludeRecipientIds != null && dto.ExcludeRecipientIds.Any())
        {
            query = query.Where(r => !dto.ExcludeRecipientIds.Contains(r.Id));
        }

        // Apply criteria filter if provided
        if (!string.IsNullOrWhiteSpace(dto.Criteria))
        {
            query = query.Where(r =>
                r.Email.Contains(dto.Criteria) ||
                r.FirstName.Contains(dto.Criteria) ||
                r.LastName.Contains(dto.Criteria));
        }

        var recipientsToRetarget = await query.ToListAsync(cancellationToken);

        result.ProcessedCount = recipientsToRetarget.Count;
        result.NewRecipientsCount = 0;

        foreach (var recipient in recipientsToRetarget)
        {
            recipient.Status = "Retargeted";
            recipient.UpdatedAt = DateTime.UtcNow;
            result.NewRecipientsCount++;
        }

        if (recipientsToRetarget.Any())
        {
            _context.CampaignRecipients.UpdateRange(recipientsToRetarget);
            await _context.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Retargeted {Count} recipients for campaign {CampaignId}",
            result.NewRecipientsCount, dto.CampaignId);

        return result;
    }

    /// <inheritdoc />
    public async Task<PaginatedDto<CampaignMetricsDto>> GetAllMetricsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _context.MarketingCampaigns
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var campaigns = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var metricsItems = new List<CampaignMetricsDto>();

        foreach (var campaign in campaigns)
        {
            var metrics = await GetMetricsAsync(campaign.Id, cancellationToken);
            if (metrics != null)
            {
                metricsItems.Add(metrics);
            }
        }

        _logger.LogInformation("Retrieved metrics for {Count} campaigns (page {Page})", metricsItems.Count, page);

        return new PaginatedDto<CampaignMetricsDto>
        {
            Items = metricsItems,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    private string GenerateInsights(CampaignMetricsDto? metrics)
    {
        if (metrics == null)
            return "No metrics available for analysis.";

        var insights = new List<string>();

        if (metrics.ClickThroughRate > 30)
            insights.Add("Strong click-through rate - audience is engaged");
        else if (metrics.ClickThroughRate < 10)
            insights.Add("Low click-through rate - consider reviewing call-to-action");

        if (metrics.ConversionRate > 10)
            insights.Add("High conversion rate - campaign strategy is effective");
        else if (metrics.ConversionRate < 5)
            insights.Add("Low conversion rate - review landing page and offer");

        if (metrics.OpenRate > 25)
            insights.Add("Good open rate - subject lines are effective");
        else if (metrics.OpenRate < 15)
            insights.Add("Low open rate - optimize subject lines");

        if (metrics.BounceRate > 5)
            insights.Add("High bounce rate - clean email list recommended");

        if (!insights.Any())
            insights.Add("Campaign performance is within normal range");

        return string.Join("; ", insights);
    }

    private List<string> GenerateRecommendations(CampaignMetricsDto? metrics)
    {
        var recommendations = new List<string>();

        if (metrics == null)
        {
            recommendations.Add("Start by sending campaigns to gather performance data");
            return recommendations;
        }

        if (metrics.ClickThroughRate < 15)
            recommendations.Add("A/B test call-to-action buttons to improve click-through rates");

        if (metrics.ConversionRate < 5)
            recommendations.Add("Review landing page experience and offer clarity");

        if (metrics.OpenRate < 20)
            recommendations.Add("Test different subject lines and send times");

        if (metrics.BounceRate > 3)
            recommendations.Add("Validate email addresses before sending");

        if (metrics.Impressions > 0 && metrics.Clicks == 0)
            recommendations.Add("Review content relevance and targeting criteria");

        recommendations.Add("Segment recipients for more targeted campaigns");

        return recommendations;
    }
}
