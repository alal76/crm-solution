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

using System.Text.Json;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Options for calendar sync service configuration.
/// </summary>
public class CalendarSyncOptions
{
    public const string SectionName = "CalendarSync";

    public string GoogleClientId { get; set; } = string.Empty;
    public string GoogleClientSecret { get; set; } = string.Empty;
    public string GoogleRedirectUri { get; set; } = string.Empty;

    public string OutlookClientId { get; set; } = string.Empty;
    public string OutlookClientSecret { get; set; } = string.Empty;
    public string OutlookTenantId { get; set; } = "common";
    public string OutlookRedirectUri { get; set; } = string.Empty;

    public int DefaultSyncIntervalMinutes { get; set; } = 15;
    public int MaxEventsPerSync { get; set; } = 100;
    public int SyncLookbackDays { get; set; } = 30;
    public int SyncLookaheadDays { get; set; } = 90;
}

/// <summary>
/// Interface for calendar sync service.
/// Handles OAuth2 flow and bi-directional sync with Google/Outlook calendars.
/// Part of Marketing & Sales gap analysis implementation (G4).
/// </summary>
public interface ICalendarSyncService
{
    // OAuth2 Authorization
    Task<string> GetGoogleAuthUrlAsync(int userId, string? state = null);
    Task<string> GetOutlookAuthUrlAsync(int userId, string? state = null);
    Task<CalendarIntegration> HandleGoogleCallbackAsync(string code, int userId);
    Task<CalendarIntegration> HandleOutlookCallbackAsync(string code, int userId);

    // Integration Management
    Task<CalendarIntegration?> GetIntegrationAsync(int userId, CalendarProvider provider);
    Task<IEnumerable<CalendarIntegration>> GetUserIntegrationsAsync(int userId);
    Task<bool> DisconnectAsync(int userId, CalendarProvider provider);
    Task<CalendarIntegration> UpdateSettingsAsync(int integrationId, CalendarSyncDirection direction, int intervalMinutes);

    // Sync Operations
    Task<CalendarSyncLog> SyncAsync(int integrationId);
    Task<CalendarSyncLog> SyncNowAsync(int userId, CalendarProvider provider);
    Task SyncAllDueAsync();

    // Event Operations
    Task PushEventToExternalAsync(Activity activity, int userId);
    Task<Activity?> PullEventFromExternalAsync(string externalEventId, int integrationId);

    // Token Management
    Task<bool> RefreshTokenIfNeededAsync(CalendarIntegration integration);
}

/// <summary>
/// Calendar sync service implementation.
/// Provides OAuth2 integration with Google Calendar and Microsoft Outlook.
/// </summary>
public class CalendarSyncService : ICalendarSyncService
{
    private readonly CrmDbContext _context;
    private readonly ILogger<CalendarSyncService> _logger;
    private readonly CalendarSyncOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public CalendarSyncService(
        CrmDbContext context,
        ILogger<CalendarSyncService> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _options = new CalendarSyncOptions();
        configuration.GetSection(CalendarSyncOptions.SectionName).Bind(_options);
    }

    #region OAuth2 Authorization

    /// <inheritdoc />
    public Task<string> GetGoogleAuthUrlAsync(int userId, string? state = null)
    {
        var scopes = new[]
        {
            "https://www.googleapis.com/auth/calendar",
            "https://www.googleapis.com/auth/calendar.events",
            "https://www.googleapis.com/auth/userinfo.email"
        };

        var url = "https://accounts.google.com/o/oauth2/v2/auth?" +
            $"client_id={Uri.EscapeDataString(_options.GoogleClientId)}&" +
            $"redirect_uri={Uri.EscapeDataString(_options.GoogleRedirectUri)}&" +
            "response_type=code&" +
            $"scope={Uri.EscapeDataString(string.Join(" ", scopes))}&" +
            "access_type=offline&" +
            "prompt=consent&" +
            $"state={Uri.EscapeDataString(state ?? userId.ToString())}";

        return Task.FromResult(url);
    }

    /// <inheritdoc />
    public Task<string> GetOutlookAuthUrlAsync(int userId, string? state = null)
    {
        var scopes = new[]
        {
            "Calendars.ReadWrite",
            "User.Read",
            "offline_access"
        };

        var url = $"https://login.microsoftonline.com/{_options.OutlookTenantId}/oauth2/v2.0/authorize?" +
            $"client_id={Uri.EscapeDataString(_options.OutlookClientId)}&" +
            "response_type=code&" +
            $"redirect_uri={Uri.EscapeDataString(_options.OutlookRedirectUri)}&" +
            $"scope={Uri.EscapeDataString(string.Join(" ", scopes))}&" +
            $"state={Uri.EscapeDataString(state ?? userId.ToString())}";

        return Task.FromResult(url);
    }

    /// <inheritdoc />
    public async Task<CalendarIntegration> HandleGoogleCallbackAsync(string code, int userId)
    {
        _logger.LogInformation("Handling Google OAuth callback for user {UserId}", userId);

        // Exchange code for tokens
        var client = _httpClientFactory.CreateClient();
        var tokenRequest = new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _options.GoogleClientId,
            ["client_secret"] = _options.GoogleClientSecret,
            ["redirect_uri"] = _options.GoogleRedirectUri,
            ["grant_type"] = "authorization_code"
        };

        var tokenResponse = await client.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(tokenRequest));

        tokenResponse.EnsureSuccessStatusCode();
        var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
        var tokens = JsonSerializer.Deserialize<GoogleTokenResponse>(tokenJson);

        if (tokens == null)
        {
            throw new InvalidOperationException("Failed to parse Google token response");
        }

        // Get user email from Google
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var userInfoResponse = await client.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
        userInfoResponse.EnsureSuccessStatusCode();
        var userInfoJson = await userInfoResponse.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(userInfoJson);

        // Create or update integration
        var integration = await _context.CalendarIntegrations
            .FirstOrDefaultAsync(i => i.UserId == userId && i.Provider == CalendarProvider.Google);

        if (integration == null)
        {
            integration = new CalendarIntegration
            {
                UserId = userId,
                Provider = CalendarProvider.Google,
                CreatedAt = DateTime.UtcNow
            };
            _context.CalendarIntegrations.Add(integration);
        }

        integration.AccessToken = tokens.AccessToken;
        integration.RefreshToken = tokens.RefreshToken ?? integration.RefreshToken;
        integration.TokenExpiresAt = DateTime.UtcNow.AddSeconds(tokens.ExpiresIn);
        integration.ExternalEmail = userInfo?.Email;
        integration.CalendarId = "primary"; // Google primary calendar
        integration.CalendarName = "Primary Calendar";
        integration.IsActive = true;
        integration.UpdatedAt = DateTime.UtcNow;
        integration.NextSyncAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully connected Google Calendar for user {UserId}", userId);
        return integration;
    }

    /// <inheritdoc />
    public async Task<CalendarIntegration> HandleOutlookCallbackAsync(string code, int userId)
    {
        _logger.LogInformation("Handling Outlook OAuth callback for user {UserId}", userId);

        // Exchange code for tokens
        var client = _httpClientFactory.CreateClient();
        var tokenRequest = new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _options.OutlookClientId,
            ["client_secret"] = _options.OutlookClientSecret,
            ["redirect_uri"] = _options.OutlookRedirectUri,
            ["grant_type"] = "authorization_code"
        };

        var tokenResponse = await client.PostAsync(
            $"https://login.microsoftonline.com/{_options.OutlookTenantId}/oauth2/v2.0/token",
            new FormUrlEncodedContent(tokenRequest));

        tokenResponse.EnsureSuccessStatusCode();
        var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
        var tokens = JsonSerializer.Deserialize<MicrosoftTokenResponse>(tokenJson);

        if (tokens == null)
        {
            throw new InvalidOperationException("Failed to parse Microsoft token response");
        }

        // Get user info from Microsoft Graph
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var userInfoResponse = await client.GetAsync("https://graph.microsoft.com/v1.0/me");
        userInfoResponse.EnsureSuccessStatusCode();
        var userInfoJson = await userInfoResponse.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<MicrosoftUserInfo>(userInfoJson);

        // Create or update integration
        var integration = await _context.CalendarIntegrations
            .FirstOrDefaultAsync(i => i.UserId == userId && i.Provider == CalendarProvider.Outlook);

        if (integration == null)
        {
            integration = new CalendarIntegration
            {
                UserId = userId,
                Provider = CalendarProvider.Outlook,
                CreatedAt = DateTime.UtcNow
            };
            _context.CalendarIntegrations.Add(integration);
        }

        integration.AccessToken = tokens.AccessToken;
        integration.RefreshToken = tokens.RefreshToken ?? integration.RefreshToken;
        integration.TokenExpiresAt = DateTime.UtcNow.AddSeconds(tokens.ExpiresIn);
        integration.ExternalEmail = userInfo?.Mail ?? userInfo?.UserPrincipalName;
        integration.CalendarId = "primary"; // Microsoft primary calendar
        integration.CalendarName = "Calendar";
        integration.IsActive = true;
        integration.UpdatedAt = DateTime.UtcNow;
        integration.NextSyncAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully connected Outlook Calendar for user {UserId}", userId);
        return integration;
    }

    #endregion

    #region Integration Management

    /// <inheritdoc />
    public async Task<CalendarIntegration?> GetIntegrationAsync(int userId, CalendarProvider provider)
    {
        return await _context.CalendarIntegrations
            .FirstOrDefaultAsync(i => i.UserId == userId && i.Provider == provider && !i.IsDeleted);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CalendarIntegration>> GetUserIntegrationsAsync(int userId)
    {
        return await _context.CalendarIntegrations
            .Where(i => i.UserId == userId && !i.IsDeleted)
            .OrderBy(i => i.Provider)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<bool> DisconnectAsync(int userId, CalendarProvider provider)
    {
        var integration = await _context.CalendarIntegrations
            .FirstOrDefaultAsync(i => i.UserId == userId && i.Provider == provider);

        if (integration == null)
        {
            return false;
        }

        // Soft delete
        integration.IsDeleted = true;
        integration.IsActive = false;
        integration.UpdatedAt = DateTime.UtcNow;

        // Remove event mappings
        var mappings = await _context.CalendarEventMappings
            .Where(m => m.CalendarIntegrationId == integration.Id)
            .ToListAsync();

        _context.CalendarEventMappings.RemoveRange(mappings);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Disconnected {Provider} calendar for user {UserId}", provider, userId);
        return true;
    }

    /// <inheritdoc />
    public async Task<CalendarIntegration> UpdateSettingsAsync(int integrationId, CalendarSyncDirection direction, int intervalMinutes)
    {
        var integration = await _context.CalendarIntegrations.FindAsync(integrationId)
            ?? throw new KeyNotFoundException($"Integration {integrationId} not found");

        integration.SyncDirection = direction;
        integration.SyncIntervalMinutes = Math.Max(5, Math.Min(intervalMinutes, 1440)); // 5 min to 24 hours
        integration.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return integration;
    }

    #endregion

    #region Sync Operations

    /// <inheritdoc />
    public async Task<CalendarSyncLog> SyncAsync(int integrationId)
    {
        var integration = await _context.CalendarIntegrations
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.Id == integrationId && i.IsActive && !i.IsDeleted)
            ?? throw new KeyNotFoundException($"Integration {integrationId} not found or inactive");

        var log = new CalendarSyncLog
        {
            CalendarIntegrationId = integrationId,
            StartedAt = DateTime.UtcNow,
            Status = CalendarSyncStatus.InProgress,
            Direction = integration.SyncDirection,
            CreatedAt = DateTime.UtcNow
        };

        _context.CalendarSyncLogs.Add(log);
        await _context.SaveChangesAsync();

        try
        {
            // Refresh token if needed
            if (!await RefreshTokenIfNeededAsync(integration))
            {
                throw new InvalidOperationException("Failed to refresh access token");
            }

            // Perform sync based on provider
            switch (integration.Provider)
            {
                case CalendarProvider.Google:
                    await SyncGoogleCalendarAsync(integration, log);
                    break;
                case CalendarProvider.Outlook:
                    await SyncOutlookCalendarAsync(integration, log);
                    break;
                default:
                    throw new NotSupportedException($"Provider {integration.Provider} not supported");
            }

            // Update success status
            log.Status = CalendarSyncStatus.Success;
            log.CompletedAt = DateTime.UtcNow;

            integration.LastSyncAt = DateTime.UtcNow;
            integration.LastSyncStatus = CalendarSyncStatus.Success;
            integration.LastSyncError = null;
            integration.LastSyncEventsCount = log.EventsCreated + log.EventsUpdated;
            integration.TotalEventsSynced += log.EventsCreated + log.EventsUpdated;
            integration.NextSyncAt = DateTime.UtcNow.AddMinutes(integration.SyncIntervalMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Calendar sync failed for integration {IntegrationId}", integrationId);

            log.Status = CalendarSyncStatus.Failed;
            log.CompletedAt = DateTime.UtcNow;
            log.ErrorMessage = ex.Message;
            log.ErrorStackTrace = ex.StackTrace;

            integration.LastSyncStatus = CalendarSyncStatus.Failed;
            integration.LastSyncError = ex.Message;
            integration.NextSyncAt = DateTime.UtcNow.AddMinutes(5); // Retry sooner on failure
        }

        await _context.SaveChangesAsync();
        return log;
    }

    /// <inheritdoc />
    public async Task<CalendarSyncLog> SyncNowAsync(int userId, CalendarProvider provider)
    {
        var integration = await _context.CalendarIntegrations
            .FirstOrDefaultAsync(i => i.UserId == userId && i.Provider == provider && i.IsActive && !i.IsDeleted)
            ?? throw new KeyNotFoundException($"No active {provider} integration found for user {userId}");

        return await SyncAsync(integration.Id);
    }

    /// <inheritdoc />
    public async Task SyncAllDueAsync()
    {
        var now = DateTime.UtcNow;
        var dueIntegrations = await _context.CalendarIntegrations
            .Where(i => i.IsActive && !i.IsDeleted && (i.NextSyncAt == null || i.NextSyncAt <= now))
            .ToListAsync();

        _logger.LogInformation("Found {Count} calendar integrations due for sync", dueIntegrations.Count);

        foreach (var integration in dueIntegrations)
        {
            try
            {
                await SyncAsync(integration.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync integration {IntegrationId}", integration.Id);
            }
        }
    }

    private async Task SyncGoogleCalendarAsync(CalendarIntegration integration, CalendarSyncLog log)
    {
        _logger.LogInformation("Syncing Google Calendar for integration {IntegrationId}", integration.Id);

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", integration.AccessToken);

        var timeMin = DateTime.UtcNow.AddDays(-_options.SyncLookbackDays).ToString("o");
        var timeMax = DateTime.UtcNow.AddDays(_options.SyncLookaheadDays).ToString("o");

        var url = $"https://www.googleapis.com/calendar/v3/calendars/{integration.CalendarId}/events?" +
            $"timeMin={Uri.EscapeDataString(timeMin)}&" +
            $"timeMax={Uri.EscapeDataString(timeMax)}&" +
            $"maxResults={_options.MaxEventsPerSync}&" +
            "singleEvents=true&" +
            "orderBy=startTime";

        if (!string.IsNullOrEmpty(integration.SyncToken))
        {
            url += $"&syncToken={Uri.EscapeDataString(integration.SyncToken)}";
        }

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var events = JsonSerializer.Deserialize<GoogleCalendarEventsResponse>(json);

        if (events?.Items != null)
        {
            foreach (var googleEvent in events.Items)
            {
                await ProcessGoogleEventAsync(integration, googleEvent, log);
            }
        }

        // Save sync token for incremental sync
        if (!string.IsNullOrEmpty(events?.NextSyncToken))
        {
            integration.SyncToken = events.NextSyncToken;
        }
    }

    private async Task SyncOutlookCalendarAsync(CalendarIntegration integration, CalendarSyncLog log)
    {
        _logger.LogInformation("Syncing Outlook Calendar for integration {IntegrationId}", integration.Id);

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", integration.AccessToken);

        var startDateTime = DateTime.UtcNow.AddDays(-_options.SyncLookbackDays).ToString("o");
        var endDateTime = DateTime.UtcNow.AddDays(_options.SyncLookaheadDays).ToString("o");

        var url = "https://graph.microsoft.com/v1.0/me/calendar/events?" +
            $"$filter=start/dateTime ge '{startDateTime}' and end/dateTime le '{endDateTime}'&" +
            "$orderby=start/dateTime&" +
            $"$top={_options.MaxEventsPerSync}";

        // Use delta query if we have a delta link
        if (!string.IsNullOrEmpty(integration.SyncToken))
        {
            url = integration.SyncToken;
        }

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var events = JsonSerializer.Deserialize<OutlookCalendarEventsResponse>(json);

        if (events?.Value != null)
        {
            foreach (var outlookEvent in events.Value)
            {
                await ProcessOutlookEventAsync(integration, outlookEvent, log);
            }
        }

        // Save delta link for incremental sync
        if (!string.IsNullOrEmpty(events?.DeltaLink))
        {
            integration.SyncToken = events.DeltaLink;
        }
    }

    private async Task ProcessGoogleEventAsync(CalendarIntegration integration, GoogleCalendarEvent googleEvent, CalendarSyncLog log)
    {
        // Check if we already have a mapping for this event
        var mapping = await _context.CalendarEventMappings
            .FirstOrDefaultAsync(m => m.CalendarIntegrationId == integration.Id && m.ExternalEventId == googleEvent.Id);

        if (mapping == null)
        {
            // Calculate duration from start/end times
            var startTime = ParseGoogleDateTime(googleEvent.Start);
            var endTime = ParseGoogleDateTime(googleEvent.End);
            var durationMinutes = (int)(endTime - startTime).TotalMinutes;
            var isAllDay = googleEvent.Start?.Date != null;

            // Store additional calendar-specific data in Details JSON
            var detailsJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                location = googleEvent.Location,
                isAllDay = isAllDay,
                endTime = endTime.ToString("o"),
                syncedFrom = "Google"
            });

            // Create new activity from Google event
            var activity = new Activity
            {
                Title = googleEvent.Summary ?? "Untitled Event",
                Description = googleEvent.Description,
                ActivityType = ActivityType.MeetingScheduled,
                ActivityDate = startTime,
                DurationMinutes = durationMinutes > 0 ? durationMinutes : 60,
                UserId = integration.UserId,
                Details = detailsJson,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            // Create mapping
            mapping = new CalendarEventMapping
            {
                ActivityId = activity.Id,
                CalendarIntegrationId = integration.Id,
                ExternalEventId = googleEvent.Id,
                ExternalEventUid = googleEvent.ICalUID,
                ExternalETag = googleEvent.Etag,
                LastSyncedAt = DateTime.UtcNow,
                ExternalLastModified = googleEvent.Updated,
                CrmLastModified = activity.UpdatedAt,
                CreatedFromExternal = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.CalendarEventMappings.Add(mapping);
            log.EventsCreated++;
        }
        else
        {
            // Update existing activity if external event changed
            if (mapping.ExternalETag != googleEvent.Etag)
            {
                var activity = await _context.Activities.FindAsync(mapping.ActivityId);
                if (activity != null)
                {
                    var startTime = ParseGoogleDateTime(googleEvent.Start);
                    var endTime = ParseGoogleDateTime(googleEvent.End);
                    var durationMinutes = (int)(endTime - startTime).TotalMinutes;
                    var isAllDay = googleEvent.Start?.Date != null;

                    var detailsJson = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        location = googleEvent.Location,
                        isAllDay = isAllDay,
                        endTime = endTime.ToString("o"),
                        syncedFrom = "Google"
                    });

                    activity.Title = googleEvent.Summary ?? "Untitled Event";
                    activity.Description = googleEvent.Description;
                    activity.ActivityDate = startTime;
                    activity.DurationMinutes = durationMinutes > 0 ? durationMinutes : 60;
                    activity.Details = detailsJson;
                    activity.UpdatedAt = DateTime.UtcNow;

                    mapping.ExternalETag = googleEvent.Etag;
                    mapping.LastSyncedAt = DateTime.UtcNow;
                    mapping.ExternalLastModified = googleEvent.Updated;

                    log.EventsUpdated++;
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task ProcessOutlookEventAsync(CalendarIntegration integration, OutlookCalendarEvent outlookEvent, CalendarSyncLog log)
    {
        // Check if we already have a mapping for this event
        var mapping = await _context.CalendarEventMappings
            .FirstOrDefaultAsync(m => m.CalendarIntegrationId == integration.Id && m.ExternalEventId == outlookEvent.Id);

        if (mapping == null)
        {
            // Calculate duration from start/end times
            var startTime = DateTime.Parse(outlookEvent.Start?.DateTime ?? DateTime.UtcNow.ToString());
            var endTime = DateTime.Parse(outlookEvent.End?.DateTime ?? DateTime.UtcNow.AddHours(1).ToString());
            var durationMinutes = (int)(endTime - startTime).TotalMinutes;

            // Store additional calendar-specific data in Details JSON
            var detailsJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                location = outlookEvent.Location?.DisplayName,
                isAllDay = outlookEvent.IsAllDay,
                endTime = endTime.ToString("o"),
                syncedFrom = "Outlook"
            });

            // Create new activity from Outlook event
            var activity = new Activity
            {
                Title = outlookEvent.Subject ?? "Untitled Event",
                Description = outlookEvent.BodyPreview,
                ActivityType = ActivityType.MeetingScheduled,
                ActivityDate = startTime,
                DurationMinutes = durationMinutes > 0 ? durationMinutes : 60,
                UserId = integration.UserId,
                Details = detailsJson,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            // Create mapping
            mapping = new CalendarEventMapping
            {
                ActivityId = activity.Id,
                CalendarIntegrationId = integration.Id,
                ExternalEventId = outlookEvent.Id,
                ExternalEventUid = outlookEvent.ICalUId,
                ExternalETag = outlookEvent.ChangeKey,
                LastSyncedAt = DateTime.UtcNow,
                ExternalLastModified = outlookEvent.LastModifiedDateTime,
                CrmLastModified = activity.UpdatedAt,
                CreatedFromExternal = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.CalendarEventMappings.Add(mapping);
            log.EventsCreated++;
        }
        else
        {
            // Update existing activity if external event changed
            if (mapping.ExternalETag != outlookEvent.ChangeKey)
            {
                var activity = await _context.Activities.FindAsync(mapping.ActivityId);
                if (activity != null)
                {
                    var startTime = DateTime.Parse(outlookEvent.Start?.DateTime ?? DateTime.UtcNow.ToString());
                    var endTime = DateTime.Parse(outlookEvent.End?.DateTime ?? DateTime.UtcNow.AddHours(1).ToString());
                    var durationMinutes = (int)(endTime - startTime).TotalMinutes;

                    var detailsJson = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        location = outlookEvent.Location?.DisplayName,
                        isAllDay = outlookEvent.IsAllDay,
                        endTime = endTime.ToString("o"),
                        syncedFrom = "Outlook"
                    });

                    activity.Title = outlookEvent.Subject ?? "Untitled Event";
                    activity.Description = outlookEvent.BodyPreview;
                    activity.ActivityDate = startTime;
                    activity.DurationMinutes = durationMinutes > 0 ? durationMinutes : 60;
                    activity.Details = detailsJson;
                    activity.UpdatedAt = DateTime.UtcNow;

                    mapping.ExternalETag = outlookEvent.ChangeKey;
                    mapping.LastSyncedAt = DateTime.UtcNow;
                    mapping.ExternalLastModified = outlookEvent.LastModifiedDateTime;

                    log.EventsUpdated++;
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    private static DateTime ParseGoogleDateTime(GoogleDateTime? dateTime)
    {
        if (dateTime == null)
        {
            return DateTime.UtcNow;
        }

        // All-day events use Date, timed events use DateTime
        if (!string.IsNullOrEmpty(dateTime.Date))
        {
            return DateTime.Parse(dateTime.Date);
        }

        return DateTime.Parse(dateTime.DateTime ?? DateTime.UtcNow.ToString());
    }

    #endregion

    #region Event Operations

    /// <inheritdoc />
    public async Task PushEventToExternalAsync(Activity activity, int userId)
    {
        var integrations = await _context.CalendarIntegrations
            .Where(i => i.UserId == userId && i.IsActive && !i.IsDeleted &&
                       (i.SyncDirection == CalendarSyncDirection.Export ||
                        i.SyncDirection == CalendarSyncDirection.Bidirectional))
            .ToListAsync();

        foreach (var integration in integrations)
        {
            try
            {
                await RefreshTokenIfNeededAsync(integration);

                switch (integration.Provider)
                {
                    case CalendarProvider.Google:
                        await PushEventToGoogleAsync(activity, integration);
                        break;
                    case CalendarProvider.Outlook:
                        await PushEventToOutlookAsync(activity, integration);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to push event {ActivityId} to {Provider} for user {UserId}",
                    activity.Id, integration.Provider, userId);
            }
        }
    }

    private async Task PushEventToGoogleAsync(Activity activity, CalendarIntegration integration)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", integration.AccessToken);

        // Parse details JSON for location and isAllDay
        string? location = null;
        bool isAllDay = false;
        DateTime? endTime = null;

        if (!string.IsNullOrEmpty(activity.Details))
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(activity.Details);
                if (doc.RootElement.TryGetProperty("location", out var locProp))
                    location = locProp.GetString();
                if (doc.RootElement.TryGetProperty("isAllDay", out var allDayProp))
                    isAllDay = allDayProp.GetBoolean();
                if (doc.RootElement.TryGetProperty("endTime", out var endProp))
                    endTime = DateTime.Parse(endProp.GetString() ?? DateTime.UtcNow.AddHours(1).ToString());
            }
            catch { /* Ignore parse errors */ }
        }

        var startTime = activity.ActivityDate;
        endTime ??= startTime.AddMinutes(activity.DurationMinutes ?? 60);

        var googleEvent = new
        {
            summary = activity.Title,
            description = activity.Description,
            location = location,
            start = isAllDay
                ? new { date = startTime.ToString("yyyy-MM-dd") }
                : new { dateTime = startTime.ToString("o"), timeZone = "UTC" } as object,
            end = isAllDay
                ? new { date = endTime.Value.ToString("yyyy-MM-dd") }
                : new { dateTime = endTime.Value.ToString("o"), timeZone = "UTC" } as object
        };

        var existingMapping = await _context.CalendarEventMappings
            .FirstOrDefaultAsync(m => m.ActivityId == activity.Id && m.CalendarIntegrationId == integration.Id);

        HttpResponseMessage response;
        if (existingMapping != null)
        {
            // Update existing event
            response = await client.PatchAsync(
                $"https://www.googleapis.com/calendar/v3/calendars/{integration.CalendarId}/events/{existingMapping.ExternalEventId}",
                new StringContent(JsonSerializer.Serialize(googleEvent), System.Text.Encoding.UTF8, "application/json"));
        }
        else
        {
            // Create new event
            response = await client.PostAsync(
                $"https://www.googleapis.com/calendar/v3/calendars/{integration.CalendarId}/events",
                new StringContent(JsonSerializer.Serialize(googleEvent), System.Text.Encoding.UTF8, "application/json"));
        }

        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync();
        var createdEvent = JsonSerializer.Deserialize<GoogleCalendarEvent>(responseJson);

        if (createdEvent != null && existingMapping == null)
        {
            var mapping = new CalendarEventMapping
            {
                ActivityId = activity.Id,
                CalendarIntegrationId = integration.Id,
                ExternalEventId = createdEvent.Id,
                ExternalEventUid = createdEvent.ICalUID,
                ExternalETag = createdEvent.Etag,
                LastSyncedAt = DateTime.UtcNow,
                CrmLastModified = activity.UpdatedAt,
                CreatedFromExternal = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.CalendarEventMappings.Add(mapping);
            await _context.SaveChangesAsync();
        }
    }

    private async Task PushEventToOutlookAsync(Activity activity, CalendarIntegration integration)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", integration.AccessToken);

        // Parse details JSON for location and isAllDay
        string? location = null;
        bool isAllDay = false;
        DateTime? endTime = null;

        if (!string.IsNullOrEmpty(activity.Details))
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(activity.Details);
                if (doc.RootElement.TryGetProperty("location", out var locProp))
                    location = locProp.GetString();
                if (doc.RootElement.TryGetProperty("isAllDay", out var allDayProp))
                    isAllDay = allDayProp.GetBoolean();
                if (doc.RootElement.TryGetProperty("endTime", out var endProp))
                    endTime = DateTime.Parse(endProp.GetString() ?? DateTime.UtcNow.AddHours(1).ToString());
            }
            catch { /* Ignore parse errors */ }
        }

        var startTime = activity.ActivityDate;
        endTime ??= startTime.AddMinutes(activity.DurationMinutes ?? 60);

        var outlookEvent = new
        {
            subject = activity.Title,
            bodyPreview = activity.Description,
            location = new { displayName = location },
            isAllDay = isAllDay,
            start = new { dateTime = startTime.ToString("o"), timeZone = "UTC" },
            end = new { dateTime = endTime.Value.ToString("o"), timeZone = "UTC" }
        };

        var existingMapping = await _context.CalendarEventMappings
            .FirstOrDefaultAsync(m => m.ActivityId == activity.Id && m.CalendarIntegrationId == integration.Id);

        HttpResponseMessage response;
        if (existingMapping != null)
        {
            // Update existing event
            var request = new HttpRequestMessage(HttpMethod.Patch,
                $"https://graph.microsoft.com/v1.0/me/calendar/events/{existingMapping.ExternalEventId}")
            {
                Content = new StringContent(JsonSerializer.Serialize(outlookEvent), System.Text.Encoding.UTF8, "application/json")
            };
            response = await client.SendAsync(request);
        }
        else
        {
            // Create new event
            response = await client.PostAsync(
                "https://graph.microsoft.com/v1.0/me/calendar/events",
                new StringContent(JsonSerializer.Serialize(outlookEvent), System.Text.Encoding.UTF8, "application/json"));
        }

        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync();
        var createdEvent = JsonSerializer.Deserialize<OutlookCalendarEvent>(responseJson);

        if (createdEvent != null && existingMapping == null)
        {
            var mapping = new CalendarEventMapping
            {
                ActivityId = activity.Id,
                CalendarIntegrationId = integration.Id,
                ExternalEventId = createdEvent.Id,
                ExternalEventUid = createdEvent.ICalUId,
                ExternalETag = createdEvent.ChangeKey,
                LastSyncedAt = DateTime.UtcNow,
                CrmLastModified = activity.UpdatedAt,
                CreatedFromExternal = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.CalendarEventMappings.Add(mapping);
            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task<Activity?> PullEventFromExternalAsync(string externalEventId, int integrationId)
    {
        var mapping = await _context.CalendarEventMappings
            .Include(m => m.Activity)
            .FirstOrDefaultAsync(m => m.ExternalEventId == externalEventId && m.CalendarIntegrationId == integrationId);

        return mapping?.Activity;
    }

    #endregion

    #region Token Management

    /// <inheritdoc />
    public async Task<bool> RefreshTokenIfNeededAsync(CalendarIntegration integration)
    {
        // Check if token is still valid (with 5 minute buffer)
        if (integration.TokenExpiresAt > DateTime.UtcNow.AddMinutes(5))
        {
            return true;
        }

        _logger.LogInformation("Refreshing token for integration {IntegrationId}", integration.Id);

        try
        {
            var client = _httpClientFactory.CreateClient();

            switch (integration.Provider)
            {
                case CalendarProvider.Google:
                    var googleRequest = new Dictionary<string, string>
                    {
                        ["refresh_token"] = integration.RefreshToken,
                        ["client_id"] = _options.GoogleClientId,
                        ["client_secret"] = _options.GoogleClientSecret,
                        ["grant_type"] = "refresh_token"
                    };

                    var googleResponse = await client.PostAsync(
                        "https://oauth2.googleapis.com/token",
                        new FormUrlEncodedContent(googleRequest));

                    googleResponse.EnsureSuccessStatusCode();
                    var googleJson = await googleResponse.Content.ReadAsStringAsync();
                    var googleTokens = JsonSerializer.Deserialize<GoogleTokenResponse>(googleJson);

                    if (googleTokens != null)
                    {
                        integration.AccessToken = googleTokens.AccessToken;
                        integration.TokenExpiresAt = DateTime.UtcNow.AddSeconds(googleTokens.ExpiresIn);
                    }
                    break;

                case CalendarProvider.Outlook:
                    var outlookRequest = new Dictionary<string, string>
                    {
                        ["refresh_token"] = integration.RefreshToken,
                        ["client_id"] = _options.OutlookClientId,
                        ["client_secret"] = _options.OutlookClientSecret,
                        ["grant_type"] = "refresh_token"
                    };

                    var outlookResponse = await client.PostAsync(
                        $"https://login.microsoftonline.com/{_options.OutlookTenantId}/oauth2/v2.0/token",
                        new FormUrlEncodedContent(outlookRequest));

                    outlookResponse.EnsureSuccessStatusCode();
                    var outlookJson = await outlookResponse.Content.ReadAsStringAsync();
                    var outlookTokens = JsonSerializer.Deserialize<MicrosoftTokenResponse>(outlookJson);

                    if (outlookTokens != null)
                    {
                        integration.AccessToken = outlookTokens.AccessToken;
                        if (!string.IsNullOrEmpty(outlookTokens.RefreshToken))
                        {
                            integration.RefreshToken = outlookTokens.RefreshToken;
                        }
                        integration.TokenExpiresAt = DateTime.UtcNow.AddSeconds(outlookTokens.ExpiresIn);
                    }
                    break;
            }

            integration.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh token for integration {IntegrationId}", integration.Id);
            return false;
        }
    }

    #endregion
}

#region OAuth Response Models

internal class GoogleTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = string.Empty;
}

internal class GoogleUserInfo
{
    public string? Email { get; set; }
    public string? Name { get; set; }
}

internal class MicrosoftTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = string.Empty;
}

internal class MicrosoftUserInfo
{
    public string? Mail { get; set; }
    public string? UserPrincipalName { get; set; }
    public string? DisplayName { get; set; }
}

internal class GoogleCalendarEventsResponse
{
    public List<GoogleCalendarEvent>? Items { get; set; }
    public string? NextSyncToken { get; set; }
    public string? NextPageToken { get; set; }
}

internal class GoogleCalendarEvent
{
    public string Id { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public GoogleDateTime? Start { get; set; }
    public GoogleDateTime? End { get; set; }
    public string? Etag { get; set; }
    public string? ICalUID { get; set; }
    public DateTime? Updated { get; set; }
}

internal class GoogleDateTime
{
    public string? Date { get; set; }
    public string? DateTime { get; set; }
    public string? TimeZone { get; set; }
}

internal class OutlookCalendarEventsResponse
{
    public List<OutlookCalendarEvent>? Value { get; set; }
    public string? DeltaLink { get; set; }
    public string? NextLink { get; set; }
}

internal class OutlookCalendarEvent
{
    public string Id { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? BodyPreview { get; set; }
    public OutlookLocation? Location { get; set; }
    public OutlookDateTime? Start { get; set; }
    public OutlookDateTime? End { get; set; }
    public bool IsAllDay { get; set; }
    public string? ChangeKey { get; set; }
    public string? ICalUId { get; set; }
    public DateTime? LastModifiedDateTime { get; set; }
}

internal class OutlookLocation
{
    public string? DisplayName { get; set; }
}

internal class OutlookDateTime
{
    public string? DateTime { get; set; }
    public string? TimeZone { get; set; }
}

#endregion
