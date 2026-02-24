// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Ports.Input;

/// <summary>
/// Interface for LinkedIn Sales Navigator integration.
/// Implements TODO-INT-10: LinkedIn Sales Navigator integration.
/// </summary>
public interface ILinkedInSalesNavService
{
    /// <summary>
    /// Searches for leads on LinkedIn Sales Navigator.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search results from LinkedIn.</returns>
    Task<LinkedInSearchResult> SearchLeadsAsync(LinkedInSearchQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a LinkedIn profile by URL or ID and enriches a CRM contact.
    /// </summary>
    /// <param name="linkedInProfileUrl">The LinkedIn profile URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Enriched profile data.</returns>
    Task<LinkedInProfile?> GetProfileAsync(string linkedInProfileUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enriches a CRM contact with LinkedIn data.
    /// </summary>
    /// <param name="contactId">The CRM contact ID.</param>
    /// <param name="linkedInProfileUrl">The LinkedIn profile URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the enrichment operation.</returns>
    Task<LinkedInEnrichResult> EnrichContactAsync(int contactId, string linkedInProfileUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs saved leads from LinkedIn Sales Navigator into CRM.
    /// </summary>
    /// <param name="listId">The Sales Navigator lead list ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Summary of the import operation.</returns>
    Task<LinkedInImportResult> ImportSavedLeadsAsync(string listId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent activity updates for watched LinkedIn profiles.
    /// </summary>
    /// <param name="contactIds">CRM contact IDs with linked LinkedIn profiles.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of recent activities.</returns>
    Task<IReadOnlyList<LinkedInActivity>> GetRecentActivitiesAsync(
        IReadOnlyList<int> contactIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the connection to LinkedIn Sales Navigator API.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the connection is valid.</returns>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// LinkedIn search query parameters.
/// </summary>
public record LinkedInSearchQuery
{
    public string? Keywords { get; init; }
    public string? Title { get; init; }
    public string? Company { get; init; }
    public string? Industry { get; init; }
    public string? Location { get; init; }
    public int MaxResults { get; init; } = 25;
}

/// <summary>
/// LinkedIn search results.
/// </summary>
public record LinkedInSearchResult
{
    public IReadOnlyList<LinkedInProfile> Profiles { get; init; } = Array.Empty<LinkedInProfile>();
    public int TotalResults { get; init; }
    public bool HasMore { get; init; }
}

/// <summary>
/// LinkedIn profile data.
/// </summary>
public record LinkedInProfile
{
    public string LinkedInId { get; init; } = string.Empty;
    public string ProfileUrl { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Title { get; init; }
    public string? Company { get; init; }
    public string? Industry { get; init; }
    public string? Location { get; init; }
    public string? Summary { get; init; }
    public string? PhotoUrl { get; init; }
    public int? ConnectionCount { get; init; }
}

/// <summary>
/// Result of enriching a CRM contact with LinkedIn data.
/// </summary>
public record LinkedInEnrichResult
{
    public bool Success { get; init; }
    public int ContactId { get; init; }
    public int FieldsUpdated { get; init; }
    public string? ErrorMessage { get; init; }

    public static LinkedInEnrichResult Succeeded(int contactId, int fieldsUpdated) =>
        new() { Success = true, ContactId = contactId, FieldsUpdated = fieldsUpdated };

    public static LinkedInEnrichResult Failed(int contactId, string error) =>
        new() { Success = false, ContactId = contactId, ErrorMessage = error };
}

/// <summary>
/// Result of importing leads from LinkedIn.
/// </summary>
public record LinkedInImportResult
{
    public int TotalProcessed { get; init; }
    public int NewLeads { get; init; }
    public int UpdatedContacts { get; init; }
    public int Skipped { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}

/// <summary>
/// LinkedIn activity update.
/// </summary>
public record LinkedInActivity
{
    public int ContactId { get; init; }
    public string LinkedInId { get; init; } = string.Empty;
    public string ActivityType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; }
}
