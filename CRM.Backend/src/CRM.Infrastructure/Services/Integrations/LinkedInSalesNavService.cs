// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ILinkedInSalesNavService = CRM.Core.Ports.Input.ILinkedInSalesNavService;
using LinkedInSearchQuery = CRM.Core.Ports.Input.LinkedInSearchQuery;
using LinkedInSearchResult = CRM.Core.Ports.Input.LinkedInSearchResult;
using LinkedInProfile = CRM.Core.Ports.Input.LinkedInProfile;
using LinkedInEnrichResult = CRM.Core.Ports.Input.LinkedInEnrichResult;
using LinkedInImportResult = CRM.Core.Ports.Input.LinkedInImportResult;
using LinkedInActivity = CRM.Core.Ports.Input.LinkedInActivity;

#pragma warning disable SA1648 // inheritdoc used on interface-implementing member; interface resolved via alias
namespace CRM.Infrastructure.Services.Integrations;

/// <summary>
/// Stub implementation of ILinkedInSalesNavService.
/// Implements TODO-INT-10.
///
/// Provides lead enrichment, profile search, and activity import
/// from LinkedIn Sales Navigator. Actual calls require LinkedIn's
/// REST API with OAuth2 and a valid Sales Navigator licence.
/// </summary>
public class LinkedInSalesNavService : ILinkedInSalesNavService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LinkedInSalesNavService> _logger;

    public LinkedInSalesNavService(
        IConfiguration configuration,
        ILogger<LinkedInSalesNavService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<LinkedInProfile>> SearchProspectsAsync(
        LinkedInSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        _logger.LogInformation(
            "LinkedIn Sales Nav prospect search: title={Title}, company={Company}, location={Location} (stub)",
            query.Title, query.Company, query.Location);

        // Return empty until OAuth2 credentials are configured
        return Task.FromResult<IReadOnlyCollection<LinkedInProfile>>(Array.Empty<LinkedInProfile>());
    }

    /// <inheritdoc />
    public Task<LinkedInEnrichResult> EnrichLeadAsync(int leadId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Enrich lead {LeadId} from LinkedIn (stub)", leadId);

        return Task.FromResult(LinkedInEnrichResult.Failed(leadId, "LinkedIn Sales Navigator integration not configured. Set Integrations:LinkedIn:AccessToken."));
    }

    /// <inheritdoc />
    public Task<LinkedInEnrichResult> EnrichContactAsync(int contactId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Enrich contact {ContactId} from LinkedIn (stub)", contactId);

        return Task.FromResult(LinkedInEnrichResult.Failed(contactId, "LinkedIn Sales Navigator integration not configured."));
    }

    /// <inheritdoc />
    public Task<LinkedInImportResult> ImportSavedLeadsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Import saved leads from LinkedIn Sales Navigator (stub)");

        return Task.FromResult(new LinkedInImportResult
        {
            TotalProcessed = 0,
            NewLeads = 0,
            Skipped = 0,
            Errors = new[] { "LinkedIn saved-leads import not yet implemented." }
        });
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<LinkedInActivity>> GetRecentActivitiesAsync(
        string linkedInProfileUrl,
        int maxResults = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Fetch recent LinkedIn activities for {Profile}, max={Max} (stub)",
            linkedInProfileUrl, maxResults);

        return Task.FromResult<IReadOnlyCollection<LinkedInActivity>>(Array.Empty<LinkedInActivity>());
    }

    /// <inheritdoc />
    public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var accessToken = _configuration["Integrations:LinkedIn:AccessToken"];

        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogWarning("LinkedIn Sales Navigator access token not configured");
            return Task.FromResult(false);
        }

        _logger.LogInformation("LinkedIn Sales Navigator test connection (stub - returning false until API integration is complete)");
        return Task.FromResult(false);
    }

    public Task<LinkedInSearchResult> SearchLeadsAsync(LinkedInSearchQuery query, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("SearchLeadsAsync not implemented - requires LinkedIn Sales Navigator API");
        return Task.FromResult(new LinkedInSearchResult { Profiles = new List<LinkedInProfile>(), TotalResults = 0 });
    }

    public Task<LinkedInProfile?> GetProfileAsync(string linkedInUrl, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("GetProfileAsync not implemented - requires LinkedIn Sales Navigator API");
        return Task.FromResult<LinkedInProfile?>(null);
    }

    public Task<LinkedInEnrichResult> EnrichContactAsync(int contactId, string linkedInUrl, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("EnrichContactAsync not implemented - requires LinkedIn Sales Navigator API");
        return Task.FromResult(LinkedInEnrichResult.Failed(contactId, "Not implemented"));
    }

    public Task<LinkedInImportResult> ImportSavedLeadsAsync(string listId, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("ImportSavedLeadsAsync not implemented - requires LinkedIn Sales Navigator API");
        return Task.FromResult(new LinkedInImportResult { TotalProcessed = 0 });
    }

    public Task<IReadOnlyList<LinkedInActivity>> GetRecentActivitiesAsync(IReadOnlyList<int> contactIds, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("GetRecentActivitiesAsync not implemented - requires LinkedIn Sales Navigator API");
        return Task.FromResult<IReadOnlyList<LinkedInActivity>>(new List<LinkedInActivity>());
    }
}
