// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

#region Calendar Integration Enumerations

/// <summary>
/// External calendar provider type.
/// </summary>
public enum CalendarProvider
{
    /// <summary>Google Calendar</summary>
    Google = 0,

    /// <summary>Microsoft Outlook/Office 365 Calendar</summary>
    Outlook = 1,

    /// <summary>Apple iCal (future)</summary>
    Apple = 2
}

/// <summary>
/// Sync direction for calendar integration.
/// </summary>
public enum CalendarSyncDirection
{
    /// <summary>Only pull events from external calendar</summary>
    Import = 0,

    /// <summary>Only push CRM events to external calendar</summary>
    Export = 1,

    /// <summary>Bi-directional sync</summary>
    Bidirectional = 2
}

/// <summary>
/// Status of calendar sync operation.
/// </summary>
public enum CalendarSyncStatus
{
    /// <summary>Sync completed successfully</summary>
    Success = 0,

    /// <summary>Sync in progress</summary>
    InProgress = 1,

    /// <summary>Sync failed</summary>
    Failed = 2,

    /// <summary>Sync pending (scheduled)</summary>
    Pending = 3
}

#endregion

/// <summary>
/// Calendar integration configuration for a user.
/// Stores OAuth2 tokens and sync settings for Google/Outlook calendars.
/// Part of Marketing &amp; Sales gap analysis implementation (G4).
/// </summary>
public class CalendarIntegration : BaseEntity
{
    /// <summary>User who owns this integration</summary>
    public int UserId { get; set; }

    /// <summary>Calendar provider type</summary>
    public CalendarProvider Provider { get; set; }

    /// <summary>OAuth2 access token (encrypted)</summary>
    [Required]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>OAuth2 refresh token (encrypted)</summary>
    [Required]
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>Token expiration timestamp</summary>
    public DateTime TokenExpiresAt { get; set; }

    /// <summary>External calendar ID to sync with</summary>
    [MaxLength(500)]
    public string? CalendarId { get; set; }

    /// <summary>External calendar name (for display)</summary>
    [MaxLength(200)]
    public string? CalendarName { get; set; }

    /// <summary>User's email on the external provider</summary>
    [MaxLength(254)]
    public string? ExternalEmail { get; set; }

    /// <summary>Sync direction</summary>
    public CalendarSyncDirection SyncDirection { get; set; } = CalendarSyncDirection.Bidirectional;

    /// <summary>Last successful sync timestamp</summary>
    public DateTime? LastSyncAt { get; set; }

    /// <summary>Last sync status</summary>
    public CalendarSyncStatus LastSyncStatus { get; set; } = CalendarSyncStatus.Pending;

    /// <summary>Last sync error message (if any)</summary>
    [MaxLength(2000)]
    public string? LastSyncError { get; set; }

    /// <summary>Next scheduled sync time</summary>
    public DateTime? NextSyncAt { get; set; }

    /// <summary>Sync interval in minutes (default 15)</summary>
    public int SyncIntervalMinutes { get; set; } = 15;

    /// <summary>Is integration active?</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Sync token for incremental sync (provider-specific)</summary>
    [MaxLength(1000)]
    public string? SyncToken { get; set; }

    /// <summary>Number of events synced in last sync</summary>
    public int? LastSyncEventsCount { get; set; }

    /// <summary>Total events synced lifetime</summary>
    public int TotalEventsSynced { get; set; } = 0;

    /// <summary>Provider-specific settings JSON</summary>
    public string? SettingsJson { get; set; }

    #region Navigation Properties

    /// <summary>User who owns this integration</summary>
    public virtual User User { get; set; } = null!;

    /// <summary>Sync history records</summary>
    public virtual ICollection<CalendarSyncLog> SyncLogs { get; set; } = new List<CalendarSyncLog>();

    #endregion
}

/// <summary>
/// Log entry for calendar sync operations.
/// </summary>
public class CalendarSyncLog : BaseEntity
{
    /// <summary>Parent integration</summary>
    public int CalendarIntegrationId { get; set; }

    /// <summary>Sync start time</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>Sync end time</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Sync status</summary>
    public CalendarSyncStatus Status { get; set; }

    /// <summary>Number of events created</summary>
    public int EventsCreated { get; set; } = 0;

    /// <summary>Number of events updated</summary>
    public int EventsUpdated { get; set; } = 0;

    /// <summary>Number of events deleted</summary>
    public int EventsDeleted { get; set; } = 0;

    /// <summary>Number of conflicts resolved</summary>
    public int ConflictsResolved { get; set; } = 0;

    /// <summary>Error message if failed</summary>
    [MaxLength(4000)]
    public string? ErrorMessage { get; set; }

    /// <summary>Error stack trace</summary>
    public string? ErrorStackTrace { get; set; }

    /// <summary>Sync direction used</summary>
    public CalendarSyncDirection Direction { get; set; }

    #region Navigation Properties

    /// <summary>Parent integration</summary>
    public virtual CalendarIntegration CalendarIntegration { get; set; } = null!;

    #endregion
}

/// <summary>
/// Mapping between CRM activities and external calendar events.
/// </summary>
public class CalendarEventMapping : BaseEntity
{
    /// <summary>CRM Activity ID</summary>
    public int ActivityId { get; set; }

    /// <summary>Calendar integration used</summary>
    public int CalendarIntegrationId { get; set; }

    /// <summary>External event ID from provider</summary>
    [Required]
    [MaxLength(500)]
    public string ExternalEventId { get; set; } = string.Empty;

    /// <summary>External event UID (iCal UID)</summary>
    [MaxLength(500)]
    public string? ExternalEventUid { get; set; }

    /// <summary>External event ETag for change detection</summary>
    [MaxLength(200)]
    public string? ExternalETag { get; set; }

    /// <summary>Last synced timestamp</summary>
    public DateTime LastSyncedAt { get; set; }

    /// <summary>External event last modified</summary>
    public DateTime? ExternalLastModified { get; set; }

    /// <summary>CRM event last modified when synced</summary>
    public DateTime? CrmLastModified { get; set; }

    /// <summary>Was this event created from external source?</summary>
    public bool CreatedFromExternal { get; set; } = false;

    #region Navigation Properties

    /// <summary>CRM Activity</summary>
    public virtual Activity Activity { get; set; } = null!;

    /// <summary>Calendar integration</summary>
    public virtual CalendarIntegration CalendarIntegration { get; set; } = null!;

    #endregion
}
