// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos
{
    /// <summary>
    /// Data transfer object for email provider configuration.
    /// Manages SMTP, SendGrid, or other email service settings.
    /// </summary>
    public class EmailConfigDto
    {
        /// <summary>
        /// Email provider type (SMTP, SendGrid, Twilio, etc.).
        /// </summary>
        public string Provider { get; set; } = "SMTP";

        /// <summary>
        /// SMTP server hostname.
        /// </summary>
        public string? SmtpHost { get; set; }

        /// <summary>
        /// SMTP server port (typically 587 or 465).
        /// </summary>
        public int SmtpPort { get; set; } = 587;

        /// <summary>
        /// SMTP username/email.
        /// </summary>
        public string? SmtpUsername { get; set; }

        /// <summary>
        /// SMTP password (encrypted in database).
        /// </summary>
        public string? SmtpPassword { get; set; }

        /// <summary>
        /// Enable SSL/TLS for SMTP connection.
        /// </summary>
        public bool SmtpEnableSsl { get; set; } = true;

        /// <summary>
        /// Sender email address (From field).
        /// </summary>
        public string FromEmail { get; set; } = string.Empty;

        /// <summary>
        /// Sender display name (From field).
        /// </summary>
        public string FromName { get; set; } = string.Empty;

        /// <summary>
        /// Reply-to email address.
        /// </summary>
        public string? ReplyToEmail { get; set; }

        /// <summary>
        /// SendGrid API key (if using SendGrid).
        /// </summary>
        public string? SendGridApiKey { get; set; }

        /// <summary>
        /// Enable email templating.
        /// </summary>
        public bool EnableTemplates { get; set; } = true;

        /// <summary>
        /// Enable email tracking (opens, clicks).
        /// </summary>
        public bool EnableTracking { get; set; } = false;

        /// <summary>
        /// Maximum emails per minute (rate limiting).
        /// </summary>
        public int MaxEmailsPerMinute { get; set; } = 60;

        /// <summary>
        /// Indicates if configuration is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// When the configuration was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
