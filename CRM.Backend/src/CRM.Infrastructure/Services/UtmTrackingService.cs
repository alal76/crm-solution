// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Manages UTM-tagged campaign tracking links and click event capture.
/// </summary>
public class UtmTrackingService : IUtmTrackingService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<UtmTrackingService> _logger;

    /// <summary>Initializes a new instance of UtmTrackingService.</summary>
    public UtmTrackingService(ICrmDbContext context, ILogger<UtmTrackingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CampaignTrackingLinkDto> CreateTrackingLinkAsync(int campaignId, CreateTrackingLinkDto dto, CancellationToken cancellationToken = default)
    {
        var campaignExists = await _context.MarketingCampaigns
            .AnyAsync(c => c.Id == campaignId && !c.IsDeleted, cancellationToken);

        if (!campaignExists)
            throw new ArgumentException($"Campaign {campaignId} not found.", nameof(campaignId));

        var token = GenerateShortToken();
        var trackedUrl = BuildTrackedUrl(dto.OriginalUrl, dto.UtmSource, dto.UtmMedium, dto.UtmCampaign, dto.UtmContent, token);

        var link = new CampaignTrackingLink
        {
            CampaignId = campaignId,
            OriginalUrl = dto.OriginalUrl,
            TrackedUrl = trackedUrl,
            LinkAlias = dto.LinkAlias,
            UtmSource = dto.UtmSource,
            UtmMedium = dto.UtmMedium,
            UtmCampaign = dto.UtmCampaign,
            UtmContent = dto.UtmContent,
            TrackingToken = token,
            ClickCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.CampaignTrackingLinks.Add(link);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created tracking link {Token} for campaign {CampaignId}", token, campaignId);

        return MapToDto(link);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CampaignTrackingLinkDto>> GetCampaignLinksAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        var links = await _context.CampaignTrackingLinks
            .Where(l => l.CampaignId == campaignId && !l.IsDeleted)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        return links.Select(MapToDto);
    }

    /// <inheritdoc />
    public async Task<string?> ResolveAndTrackAsync(string token, string? visitorIp, string? userAgent, CancellationToken cancellationToken = default)
    {
        var link = await _context.CampaignTrackingLinks
            .FirstOrDefaultAsync(l => l.TrackingToken == token && !l.IsDeleted, cancellationToken);

        if (link == null)
        {
            _logger.LogWarning("Unknown tracking token: {Token}", token);
            return null;
        }

        // Increment click counter
        link.ClickCount++;

        // Record click event
        var click = new UtmLinkClick
        {
            UtmSource = link.UtmSource,
            UtmMedium = link.UtmMedium,
            UtmCampaign = link.UtmCampaign,
            UtmContent = link.UtmContent,
            OriginalUrl = link.OriginalUrl,
            LandingUrl = link.TrackedUrl,
            VisitorIp = visitorIp,
            VisitorUserAgent = userAgent,
            TrackingLinkId = link.Id,
            ClickedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.UtmLinkClicks.Add(click);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Click tracked: token={Token}, ip={Ip}", token, visitorIp);

        return link.TrackedUrl;
    }

    /// <inheritdoc />
    public async Task AssociateLeadAsync(string token, int leadId, CancellationToken cancellationToken = default)
    {
        var click = await _context.UtmLinkClicks
            .Where(c => c.TrackingLinkId != null && !c.IsDeleted)
            .OrderByDescending(c => c.ClickedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (click != null && click.LeadId == null)
        {
            click.LeadId = leadId;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────

    private static string GenerateShortToken()
        => Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=')
            .Substring(0, 12);

    private static string BuildTrackedUrl(string originalUrl, string? utmSource, string? utmMedium, string? utmCampaign, string? utmContent, string token)
    {
        var sb = new StringBuilder(originalUrl);
        var separator = originalUrl.Contains('?') ? '&' : '?';

        if (!string.IsNullOrWhiteSpace(utmSource))
        {
            sb.Append($"{separator}utm_source={Uri.EscapeDataString(utmSource)}");
            separator = '&';
        }

        if (!string.IsNullOrWhiteSpace(utmMedium))
        {
            sb.Append($"{separator}utm_medium={Uri.EscapeDataString(utmMedium)}");
            separator = '&';
        }

        if (!string.IsNullOrWhiteSpace(utmCampaign))
        {
            sb.Append($"{separator}utm_campaign={Uri.EscapeDataString(utmCampaign)}");
            separator = '&';
        }

        if (!string.IsNullOrWhiteSpace(utmContent))
        {
            sb.Append($"{separator}utm_content={Uri.EscapeDataString(utmContent)}");
            separator = '&';
        }

        sb.Append($"{separator}_tk={token}");
        return sb.ToString();
    }

    private static CampaignTrackingLinkDto MapToDto(CampaignTrackingLink link) => new()
    {
        Id = link.Id,
        CampaignId = link.CampaignId,
        OriginalUrl = link.OriginalUrl,
        TrackedUrl = link.TrackedUrl,
        LinkAlias = link.LinkAlias,
        UtmSource = link.UtmSource,
        UtmMedium = link.UtmMedium,
        UtmCampaign = link.UtmCampaign,
        UtmContent = link.UtmContent,
        TrackingToken = link.TrackingToken,
        ClickCount = link.ClickCount,
        CreatedAt = link.CreatedAt
    };
}
