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

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

#region Email Integration Enumerations

/// <summary>
/// External email provider type.
/// </summary>
public enum EmailProvider
{
    /// <summary>Gmail/Google Workspace</summary>
    Google = 0,
    /// <summary>Microsoft Outlook/Office 365</summary>
    Outlook = 1,
    /// <summary>Generic IMAP server</summary>
    Imap = 2
}

/// <summary>
/// Email sync status.
/// </summary>
public enum EmailSyncStatus
{
    /// <summary>Sync completed successfully</summary>
    Success = 0,
    /// <summary>Sync in progress</summary>
    InProgress = 1,
    /// <summary>Sync failed</summary>
    Failed = 2,
    /// <summary>Sync pending</summary>
    Pending = 3
}

#endregion

/// <summary>
/// Email integration configuration for syncing inboxes.
/// Part of Marketing & Sales gap analysis implementation (G5).
/// </summary>
public class EmailIntegration : BaseEntity
{
    /// <summary>User who owns this integration</summary>
    public int UserId { get; set; }

    /// <summary>Email provider type</summary>
    public EmailProvider Provider { get; set; }

    /// <summary>Email address for the integration</summary>
    [MaxLength(254)]
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary>OAuth2 access token (encrypted)</summary>
    public string? AccessToken { get; set; }

    /// <summary>OAuth2 refresh token (encrypted)</summary>
    public string? RefreshToken { get; set; }

    /// <summary>Token expiration timestamp</summary>
    public DateTime? TokenExpiresAt { get; set; }

    /// <summary>IMAP server hostname (for generic IMAP)</summary>
    [MaxLength(200)]
    public string? ImapServer { get; set; }

    /// <summary>IMAP port (for generic IMAP)</summary>
    public int? ImapPort { get; set; }

    /// <summary>IMAP username (for generic IMAP)</summary>
    [MaxLength(254)]
    public string? ImapUsername { get; set; }

    /// <summary>IMAP password (encrypted)</summary>
    public string? ImapPassword { get; set; }

    /// <summary>Use SSL for IMAP connection</summary>
    public bool UseSsl { get; set; } = true;

    /// <summary>Last successful sync time</summary>
    public DateTime? LastSyncAt { get; set; }

    /// <summary>Last sync status</summary>
    public EmailSyncStatus LastSyncStatus { get; set; } = EmailSyncStatus.Pending;

    /// <summary>Last sync error message</summary>
    [MaxLength(2000)]
    public string? LastSyncError { get; set; }

    /// <summary>Next scheduled sync time</summary>
    public DateTime? NextSyncAt { get; set; }

    /// <summary>Sync interval in minutes</summary>
    public int SyncIntervalMinutes { get; set; } = 15;

    /// <summary>Last sync token for incremental sync (provider-specific)</summary>
    [MaxLength(2000)]
    public string? LastSyncToken { get; set; }

    /// <summary>Is integration active?</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Total emails synced</summary>
    public int TotalEmailsSynced { get; set; } = 0;

    /// <summary>Provider-specific settings JSON</summary>
    public string? SettingsJson { get; set; }

    #region Navigation Properties

    /// <summary>User who owns this integration</summary>
    public virtual User User { get; set; } = null!;

    /// <summary>Sync logs</summary>
    public virtual ICollection<EmailSyncLog> SyncLogs { get; set; } = new List<EmailSyncLog>();

    /// <summary>Email mappings</summary>
    public virtual ICollection<EmailMessageMapping> MessageMappings { get; set; } = new List<EmailMessageMapping>();

    #endregion
}

/// <summary>
/// Log entry for email sync operations.
/// </summary>
public class EmailSyncLog : BaseEntity
{
    /// <summary>Parent integration</summary>
    public int EmailIntegrationId { get; set; }

    /// <summary>Sync start time</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>Sync end time</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Sync status</summary>
    public EmailSyncStatus Status { get; set; }

    /// <summary>Number of emails created</summary>
    public int EmailsCreated { get; set; } = 0;

    /// <summary>Number of emails updated</summary>
    public int EmailsUpdated { get; set; } = 0;

    /// <summary>Number of emails skipped</summary>
    public int EmailsSkipped { get; set; } = 0;

    /// <summary>Error message if failed</summary>
    [MaxLength(4000)]
    public string? ErrorMessage { get; set; }

    /// <summary>Error stack trace</summary>
    public string? ErrorStackTrace { get; set; }

    #region Navigation Properties

    /// <summary>Parent integration</summary>
    public virtual EmailIntegration EmailIntegration { get; set; } = null!;

    #endregion
}

/// <summary>
/// Mapping between external email messages and CRM CommunicationMessage records.
/// </summary>
public class EmailMessageMapping : BaseEntity
{
    /// <summary>CRM CommunicationMessage ID</summary>
    public int CommunicationMessageId { get; set; }

    /// <summary>Email integration used</summary>
    public int EmailIntegrationId { get; set; }

    /// <summary>External message ID</summary>
    [Required]
    [MaxLength(500)]
    public string ExternalMessageId { get; set; } = string.Empty;

    /// <summary>External thread/conversation ID</summary>
    [MaxLength(500)]
    public string? ExternalThreadId { get; set; }

    /// <summary>External message ETag or change key</summary>
    [MaxLength(200)]
    public string? ExternalChangeKey { get; set; }

    /// <summary>Last synced timestamp</summary>
    public DateTime LastSyncedAt { get; set; }

    #region Navigation Properties

    /// <summary>CRM message</summary>
    public virtual CommunicationMessage CommunicationMessage { get; set; } = null!;

    /// <summary>Email integration</summary>
    public virtual EmailIntegration EmailIntegration { get; set; } = null!;

    #endregion
}
