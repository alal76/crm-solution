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

        if (dto.ContactIds == null || !dto.ContactIds.Any())
            return 0;

        // Get existing recipients for this campaign
        var existingRecipientIds = await _context.CampaignRecipients
            .Where(r => r.CampaignId == campaignId && !r.IsDeleted)
            .Select(r => r.ContactId)
            .ToHashSetAsync(cancellationToken);

        int addedCount = 0;
        foreach (var contactId in dto.ContactIds)
        {
            if (existingRecipientIds.Contains(contactId))
                continue; // Skip if already a recipient

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted, cancellationToken);

            if (contact == null)
                continue;

            var recipient = new CampaignRecipient
            {
                CampaignId = campaignId,
                ContactId = contactId,
                Status = "Queued",
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
            ContactName = $"{recipient.Contact?.FirstName} {recipient.Contact?.LastName}",
            ContactEmail = recipient.Contact?.Email,
            Status = recipient.Status,
            SentAt = recipient.SentAt,
            OpenedAt = recipient.OpenedAt,
            ClickedAt = recipient.ClickedAt,
            CreatedAt = recipient.CreatedAt
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
        var sentCount = recipients.Count(r => r.SentAt.HasValue);
        var openedCount = recipients.Count(r => r.OpenedAt.HasValue);
        var clickedCount = recipients.Count(r => r.ClickedAt.HasValue);
        var bouncedCount = recipients.Count(r => r.Status == "Bounced");

        var metrics = new CampaignMetricsDto
        {
            CampaignId = campaignId,
            TotalRecipients = totalRecipients,
            SentCount = sentCount,
            OpenCount = openedCount,
            ClickCount = clickedCount,
            BounceCount = bouncedCount,
            OpenRate = totalRecipients > 0 ? (openedCount * 100m / totalRecipients) : 0,
            ClickRate = totalRecipients > 0 ? (clickedCount * 100m / totalRecipients) : 0,
            BounceRate = totalRecipients > 0 ? (bouncedCount * 100m / totalRecipients) : 0,
            ConversionRate = recipients.Any() ? (clickedCount * 100m / sentCount) : 0,
            CalculatedAt = DateTime.UtcNow
        };

        return metrics;
    }

    public async Task<CampaignAnalysisDto> AnalyzeAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        var metrics = await GetMetricsAsync(campaignId, cancellationToken);

        var analysis = new CampaignAnalysisDto
        {
            CampaignId = campaignId,
            Insights = GenerateInsights(metrics),
            Recommendations = GenerateRecommendations(metrics),
            AnalyzedAt = DateTime.UtcNow
        };

        return await Task.FromResult(analysis);
    }

    public async Task<CampaignPreviewDto> PreviewAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        var campaign = await _context.MarketingCampaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted, cancellationToken);

        if (campaign == null)
            throw new InvalidOperationException($"Campaign {campaignId} not found");

        var preview = new CampaignPreviewDto
        {
            CampaignId = campaignId,
            Subject = campaign.Name,
            PreviewText = campaign.Description,
            PreviewedAt = DateTime.UtcNow
        };

        return await Task.FromResult(preview);
    }

    public async Task<int> DuplicateAsync(int campaignId, DuplicateCampaignDto dto, CancellationToken cancellationToken = default)
    {
        var original = await _context.MarketingCampaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted, cancellationToken);

        if (original == null)
            throw new InvalidOperationException($"Campaign {campaignId} not found");

        var copy = new MarketingCampaign
        {
            Name = dto.NewName,
            Description = $"Copy of {original.Name}",
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MarketingCampaigns.Add(copy);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Campaign {CampaignId} duplicated as {NewCampaignName}", campaignId, dto.NewName);
        return copy.Id;
    }

    public async Task<bool> RetargetAsync(int campaignId, RetargetCampaignDto dto, CancellationToken cancellationToken = default)
    {
        var campaign = await _context.MarketingCampaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted, cancellationToken);

        if (campaign == null)
            return false;

        // Retargeting logic - mark non-converters for re-engagement
        var nonConverters = await _context.CampaignRecipients
            .Where(r => r.CampaignId == campaignId && !r.IsDeleted && r.ClickedAt == null)
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

        if (metrics.OpenRate > 30)
            insights.Add("Strong email open rate - audience is engaged");
        else if (metrics.OpenRate < 10)
            insights.Add("Low open rate - consider reviewing subject lines");

        if (metrics.ClickRate > 10)
            insights.Add("High click-through rate - call-to-action is effective");

        if (metrics.BounceRate > 5)
            insights.Add("High bounce rate - review email list quality");

        return insights;
    }

    private List<string> GenerateRecommendations(CampaignMetricsDto metrics)
    {
        var recommendations = new List<string>();

        if (metrics.OpenRate < 15)
            recommendations.Add("A/B test subject lines to improve open rates");

        if (metrics.ClickRate < 5)
            recommendations.Add("Improve call-to-action visibility and clarity");

        if (metrics.BounceRate > 2)
            recommendations.Add("Validate email addresses before sending");

        recommendations.Add("Segment recipients for more targeted campaigns");

        return recommendations;
    }
}
