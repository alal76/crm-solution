// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for UTM link tracking operations.
/// Creates trackable links that capture UTM parameters and click analytics.
/// </summary>
public interface IUtmTrackingService
{
    /// <summary>Creates a UTM-tagged tracking link for a campaign.</summary>
    /// <param name="campaignId">Parent campaign ID.</param>
    /// <param name="dto">Link creation parameters including UTM params.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created tracking link DTO with <c>TrackedUrl</c> populated.</returns>
    Task<CampaignTrackingLinkDto> CreateTrackingLinkAsync(int campaignId, CreateTrackingLinkDto dto, CancellationToken cancellationToken = default);

    /// <summary>Gets all tracking links for a campaign.</summary>
    /// <param name="campaignId">Campaign ID to query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of tracking link DTOs.</returns>
    Task<IEnumerable<CampaignTrackingLinkDto>> GetCampaignLinksAsync(int campaignId, CancellationToken cancellationToken = default);

    /// <summary>Resolves a short tracking token to its destination URL and records the click.</summary>
    /// <param name="token">Short token embedded in the tracking URL.</param>
    /// <param name="visitorIp">Visitor IP address (may be null).</param>
    /// <param name="userAgent">Visitor user-agent (may be null).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Original destination URL (for 302 redirect) or null if token unknown.</returns>
    Task<string?> ResolveAndTrackAsync(string token, string? visitorIp, string? userAgent, CancellationToken cancellationToken = default);

    /// <summary>Associates a resolved tracking click with a known lead.</summary>
    /// <param name="token">The tracking token from the clicked link.</param>
    /// <param name="leadId">The lead ID to associate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AssociateLeadAsync(string token, int leadId, CancellationToken cancellationToken = default);
}
