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
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of ICampaignRecipientService for campaign recipient management.
/// Handles targeting, filtering, and recipient list operations.
/// </summary>
public class CampaignRecipientService : ICampaignRecipientService, ICampaignRecipientInputPort
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<CampaignRecipientService> _logger;

    public CampaignRecipientService(ICrmDbContext context, ILogger<CampaignRecipientService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<CampaignRecipientDto>> GetRecipientsAsync(int campaignId, int page = 1, int pageSize = 100, CancellationToken cancellationToken = default)
    {
        var campaign = await _context.MarketingCampaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted, cancellationToken);

        if (campaign == null)
            throw new InvalidOperationException($"Campaign {campaignId} not found");

        var recipients = await _context.CampaignRecipients
            .Where(r => r.CampaignId == campaignId && !r.IsDeleted)
            .Include(r => r.Contact)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return recipients.Select(r => MapToDto(r)).ToList();
    }

    public async Task<int> AddRecipientsAsync(int campaignId, AddCampaignRecipientsDto dto, CancellationToken cancellationToken = default)
    {
        var campaign = await _context.MarketingCampaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted, cancellationToken);

        if (campaign == null)
            throw new InvalidOperationException($"Campaign {campaignId} not found");

        if (dto.RecipientIds == null || !dto.RecipientIds.Any())
            return 0;

        // Get existing recipients for this campaign
        var existingRecipientIds = await _context.CampaignRecipients
            .Where(r => r.CampaignId == campaignId && !r.IsDeleted)
            .Select(r => r.ContactId)
            .ToHashSetAsync(cancellationToken);

        int addedCount = 0;
        foreach (var contactId in dto.RecipientIds)
        {
            if (existingRecipientIds.Contains(contactId))
                continue; // Skip if already a recipient

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.Id == contactId, cancellationToken);

            if (contact == null)
                continue;

            var recipient = new CampaignRecipient
            {
                CampaignId = campaignId,
                ContactId = contactId,
                Email = contact.Email ?? string.Empty,
                FirstName = contact.FirstName ?? string.Empty,
                LastName = contact.LastName ?? string.Empty,
                Status = CampaignRecipientStatus.Pending.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CampaignRecipients.Add(recipient);
            addedCount++;
        }

        if (addedCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Added {Count} recipients to campaign {CampaignId}", addedCount, campaignId);
        }

        return addedCount;
    }

    public async Task<bool> RemoveRecipientAsync(int campaignId, int recipientId, CancellationToken cancellationToken = default)
    {
        var recipient = await _context.CampaignRecipients
            .FirstOrDefaultAsync(r => r.Id == recipientId && r.CampaignId == campaignId && !r.IsDeleted, cancellationToken);

        if (recipient == null)
            return false;

        recipient.IsDeleted = true;
        recipient.UpdatedAt = DateTime.UtcNow;
        _context.CampaignRecipients.Update(recipient);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recipient {RecipientId} removed from campaign {CampaignId}", recipientId, campaignId);
        return true;
    }

    public async Task<List<CampaignRecipientDto>> FilterAsync(int campaignId, string criteria, CancellationToken cancellationToken = default)
    {
        // Simple filter implementation - can be extended with more sophisticated criteria
        var recipients = await _context.CampaignRecipients
            .Where(r => r.CampaignId == campaignId && !r.IsDeleted)
            .Include(r => r.Contact)
            .Where(r => r.Contact.FirstName.Contains(criteria) || 
                       r.Contact.LastName.Contains(criteria) || 
                       r.Contact.Email.Contains(criteria))
            .ToListAsync(cancellationToken);

        return recipients.Select(r => MapToDto(r)).ToList();
    }

    public async Task<int> GetCountAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        return await _context.CampaignRecipients
            .CountAsync(r => r.CampaignId == campaignId && !r.IsDeleted, cancellationToken);
    }

    private CampaignRecipientDto MapToDto(CampaignRecipient recipient)
    {
        return new CampaignRecipientDto
        {
            Id = recipient.Id,
            CampaignId = recipient.CampaignId,
            ContactId = recipient.ContactId,
            AccountId = recipient.AccountId,
            Email = recipient.Email ?? string.Empty,
            FirstName = recipient.FirstName,
            LastName = recipient.LastName,
            Status = int.TryParse(recipient.Status, out var statusInt) ? statusInt : (int)CampaignRecipientStatus.Pending,
            AddedAt = recipient.CreatedAt,
            EngagedAt = recipient.FirstOpenedAt,
            Impressions = 0, // OpenCount not available on entity
            Clicks = 0,      // ClickCount not available on entity
            Conversions = 0, // ConvertedAt not available on entity
            Money = 0        // ConversionValue not available on entity
        };
    }
}

/// <summary>
/// Implementation of ICampaignMetricsService for campaign metrics and analytics.
/// Handles performance tracking and ROI calculations.
/// </summary>
public class CampaignMetricsService : ICampaignMetricsService, ICampaignMetricsInputPort
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<CampaignMetricsService> _logger;

    public CampaignMetricsService(ICrmDbContext context, ILogger<CampaignMetricsService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CampaignMetricsDto> GetMetricsAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        var campaign = await _context.MarketingCampaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted, cancellationToken);

        if (campaign == null)
            throw new InvalidOperationException($"Campaign {campaignId} not found");

        var recipients = await _context.CampaignRecipients
            .Where(r => r.CampaignId == campaignId && !r.IsDeleted)
            .ToListAsync(cancellationToken);

        var totalRecipients = recipients.Count;
        var sentCount = recipients.Count(r => r.SendActualTime.HasValue);
        var openedCount = recipients.Count(r => r.FirstOpenedAt.HasValue);
        var clickedCount = recipients.Count(r => r.FirstClickedAt.HasValue);
        var bouncedCount = recipients.Count(r => r.Status == CampaignRecipientStatus.Bounced.ToString());

        var metrics = new CampaignMetricsDto
        {
            CampaignId = campaignId,
            CampaignName = campaign.Name,
            Impressions = totalRecipients,
            Clicks = clickedCount,
            Conversions = 0,
            LeadsGenerated = 0,
            MqlsGenerated = 0,
            SqlsGenerated = 0,
            ReveneGenerated = 0,
            Roi = 0,
            Cpl = 0,
            Cpa = 0,
            ClickThroughRate = totalRecipients > 0 ? (clickedCount * 100m / totalRecipients) : 0m,
            ConversionRate = sentCount > 0 ? (openedCount * 100m / sentCount) : 0m
        };

        return metrics;
    }

    public async Task<CRM.Core.Dtos.CampaignAnalysisDto> AnalyzeAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        var metrics = await GetMetricsAsync(campaignId, cancellationToken);

        var analysis = new CRM.Core.Dtos.CampaignAnalysisDto
        {
            CampaignId = campaignId,
            Insights = string.Join(", ", GenerateInsights(metrics)),
            Recommendations = GenerateRecommendations(metrics),
            AnalyzedAt = DateTime.UtcNow
        };

        return await Task.FromResult(analysis);
    }

    public async Task<CRM.Core.Dtos.CampaignPreviewDto> PreviewAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        var campaign = await _context.MarketingCampaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted, cancellationToken);

        if (campaign == null)
            throw new InvalidOperationException($"Campaign {campaignId} not found");

        var preview = new CRM.Core.Dtos.CampaignPreviewDto
        {
            CampaignId = campaignId,
            Subject = campaign.Name,
            PreviewText = campaign.Description
        };

        return await Task.FromResult(preview);
    }

    public async Task<int> DuplicateAsync(int campaignId, CRM.Core.Dtos.DuplicateCampaignDto dto, CancellationToken cancellationToken = default)
    {
        var original = await _context.MarketingCampaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted, cancellationToken);

        if (original == null)
            throw new InvalidOperationException($"Campaign {campaignId} not found");

        var copy = new MarketingCampaign
        {
            Name = dto.NewName,
            Description = $"Copy of {original.Name}",
            Status = CampaignStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MarketingCampaigns.Add(copy);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Campaign {CampaignId} duplicated as {NewCampaignName}", campaignId, dto.NewName);
        return copy.Id;
    }

    public async Task<bool> RetargetAsync(int campaignId, CRM.Core.Dtos.RetargetCampaignDto dto, CancellationToken cancellationToken = default)
    {
        var campaign = await _context.MarketingCampaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted, cancellationToken);

        if (campaign == null)
            return false;

        // Retargeting logic - mark non-converters for re-engagement
        var nonConverters = await _context.CampaignRecipients
            .Where(r => r.CampaignId == campaignId && !r.IsDeleted && r.FirstClickedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var recipient in nonConverters)
        {
            recipient.Status = "Retargeted";
            recipient.UpdatedAt = DateTime.UtcNow;
        }

        _context.CampaignRecipients.UpdateRange(nonConverters);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Campaign {CampaignId} retargeted to {Count} non-converters", campaignId, nonConverters.Count);
        return true;
    }

    private List<string> GenerateInsights(CampaignMetricsDto metrics)
    {
        var insights = new List<string>();

        if (metrics.ClickThroughRate > 30)
            insights.Add("Strong email click-through rate - audience is engaged");
        else if (metrics.ClickThroughRate < 10)
            insights.Add("Low click-through rate - consider reviewing call-to-action");

        if (metrics.ConversionRate > 10)
            insights.Add("High conversion rate - campaign strategy is effective");

        if (metrics.Impressions > 0 && metrics.Clicks == 0)
            insights.Add("No clicks - audience may not be interested in the offer");

        return insights;
    }

    private List<string> GenerateRecommendations(CampaignMetricsDto metrics)
    {
        var recommendations = new List<string>();

        if (metrics.ClickThroughRate < 15)
            recommendations.Add("A/B test call-to-action to improve click-through rates");

        if (metrics.ConversionRate < 5)
            recommendations.Add("Review landing page experience and offer clarity");

        if (metrics.Impressions > 0 && (metrics.Clicks * 100m / metrics.Impressions) < 2)
            recommendations.Add("Review subject lines and preview text effectiveness");

        recommendations.Add("Segment recipients for more targeted campaigns");

        return recommendations;
    }
}
