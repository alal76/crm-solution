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

namespace CRM.Core.Entities;

/// <summary>
/// Email template category for organization
/// </summary>
public enum EmailTemplateCategory
{
    General = 0,
    Sales = 1,
    Marketing = 2,
    Support = 3,
    Welcome = 4,
    FollowUp = 5,
    Newsletter = 6,
    Notification = 7,
    Transactional = 8,
    System = 9,
    Billing = 10,
    Internal = 11,
    Custom = 99
}

/// <summary>
/// Email template for creating reusable email content
/// Supports merge fields/placeholders for personalization
/// </summary>
public class EmailTemplate : BaseEntity
{
    /// <summary>
    /// Template name for identification
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Template description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Category for organization
    /// </summary>
    public EmailTemplateCategory Category { get; set; } = EmailTemplateCategory.General;

    /// <summary>
    /// Email subject line (supports merge fields)
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Plain text body (supports merge fields)
    /// </summary>
    public string? PlainTextBody { get; set; }

    /// <summary>
    /// HTML body (supports merge fields)
    /// </summary>
    public string? HtmlBody { get; set; }

    /// <summary>
    /// Whether this template is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is a system template (cannot be deleted)
    /// </summary>
    public bool IsSystem { get; set; } = false;

    /// <summary>
    /// JSON array of available merge fields
    /// </summary>
    public string? MergeFieldsJson { get; set; }

    /// <summary>
    /// Default From email for this template
    /// </summary>
    public string? FromEmail { get; set; }

    /// <summary>
    /// Default From name for this template
    /// </summary>
    public string? FromName { get; set; }

    /// <summary>
    /// Default Reply-To address
    /// </summary>
    public string? ReplyToEmail { get; set; }

    /// <summary>
    /// JSON array of default attachments
    /// </summary>
    public string? DefaultAttachmentsJson { get; set; }

    /// <summary>
    /// User who created this template
    /// </summary>
    public int? CreatedByUserId { get; set; }

    /// <summary>
    /// Number of times this template was used
    /// </summary>
    public int UsageCount { get; set; } = 0;

    /// <summary>
    /// Last time this template was used
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Preview text for email clients
    /// </summary>
    public string? PreviewText { get; set; }

    /// <summary>
    /// Associated channel ID (optional)
    /// </summary>
    public int? ChannelId { get; set; }

    /// <summary>
    /// URL-friendly identifier (slug)
    /// </summary>
    public string? Slug { get; set; }

    /// <summary>
    /// Alias for PlainTextBody for service compatibility
    /// </summary>
    public string? TextBody
    {
        get => PlainTextBody;
        set => PlainTextBody = value;
    }

    /// <summary>
    /// Alias for ReplyToEmail for service compatibility
    /// </summary>
    public string? ReplyTo
    {
        get => ReplyToEmail;
        set => ReplyToEmail = value;
    }

    /// <summary>
    /// Template version number
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Template purpose (e.g., WelcomeEmail, PasswordReset)
    /// </summary>
    public string? Purpose { get; set; }

    /// <summary>
    /// Whether this is the default template for its purpose
    /// </summary>
    public bool IsDefault { get; set; } = false;

    // Navigation
    public virtual ICollection<CommunicationMessage> Messages { get; set; } = new List<CommunicationMessage>();
}
