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
/// Represents a historical version of an email template (entity for database storage).
/// Used for version control and rollback capabilities.
/// Note: Named with "Entity" suffix to avoid conflict with IEmailTemplateService.EmailTemplateVersion DTO.
/// </summary>
public class EmailTemplateHistoryEntry : BaseEntity
{
    /// <summary>
    /// ID of the parent email template
    /// </summary>
    public int TemplateId { get; set; }

    /// <summary>
    /// Version number (1, 2, 3, etc.)
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Subject line at this version
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// HTML body content at this version
    /// </summary>
    public string HtmlBody { get; set; } = string.Empty;

    /// <summary>
    /// Plain text body at this version
    /// </summary>
    public string? TextBody { get; set; }

    /// <summary>
    /// Description of changes in this version
    /// </summary>
    public string? ChangeDescription { get; set; }

    /// <summary>
    /// User who created this version
    /// </summary>
    public int? CreatedById { get; set; }

    /// <summary>
    /// Name of user who created this version (denormalized for history)
    /// </summary>
    public string? CreatedByName { get; set; }

    // Navigation
    public virtual EmailTemplate? Template { get; set; }
    public virtual User? CreatedBy { get; set; }
}

/// <summary>
/// Tracks usage of email templates for analytics.
/// </summary>
public class EmailTemplateUsage : BaseEntity
{
    /// <summary>
    /// ID of the template that was used
    /// </summary>
    public int TemplateId { get; set; }

    /// <summary>
    /// User who used the template (null for system usage)
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Context in which the template was used (e.g., "campaign", "test_send", "api")
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// When the template was used
    /// </summary>
    public DateTime UsedAt { get; set; }

    // Navigation
    public virtual EmailTemplate? Template { get; set; }
    public virtual User? User { get; set; }
}
