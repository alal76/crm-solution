// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Service for parsing inbound emails and creating incidents/tickets.
/// </summary>
public interface IEmailToTicketService
{
    /// <summary>
    /// Parse an inbound email and create an incident.
    /// </summary>
    Task<EmailParseResult> ParseAndCreateIncidentAsync(InboundEmailDto email);

    /// <summary>
    /// Parse an inbound email and update an existing incident (thread reply).
    /// </summary>
    Task<EmailParseResult> ParseAndUpdateIncidentAsync(InboundEmailDto email, int incidentId);

    /// <summary>
    /// Extract ticket reference from email subject (e.g., "[INC-12345]").
    /// </summary>
    int? ExtractIncidentReference(string subject);

    /// <summary>
    /// Get email parsing configuration.
    /// </summary>
    Task<EmailParsingConfigDto> GetConfigurationAsync();

    /// <summary>
    /// Update email parsing configuration.
    /// </summary>
    Task UpdateConfigurationAsync(EmailParsingConfigDto config);
}

/// <summary>
/// DTO for inbound email data.
/// </summary>
public class InboundEmailDto
{
    public string From { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public List<string> To { get; set; } = new();
    public List<string> Cc { get; set; } = new();
    public string Subject { get; set; } = string.Empty;
    public string BodyText { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public List<EmailAttachmentDto> Attachments { get; set; } = new();
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public string? MessageId { get; set; }
    public string? InReplyTo { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
}

/// <summary>
/// DTO for email attachment.
/// </summary>
public class EmailAttachmentDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public long Size { get; set; }
}

/// <summary>
/// Result of email parsing operation.
/// </summary>
public class EmailParseResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int? IncidentId { get; set; }
    public string? IncidentNumber { get; set; }
    public EmailParseAction Action { get; set; }
    public int? CommentId { get; set; }
}

/// <summary>
/// Action taken when parsing an email.
/// </summary>
public enum EmailParseAction
{
    IncidentCreated,
    CommentAdded,
    Ignored,
    Failed
}

/// <summary>
/// Configuration for email parsing.
/// </summary>
public class EmailParsingConfigDto
{
    public bool IsEnabled { get; set; } = true;
    public string DefaultCategory { get; set; } = "Email";
    public int DefaultPriority { get; set; } = 3;
    public int? DefaultAssignmentGroupId { get; set; }
    public bool AutoDetectCustomer { get; set; } = true;
    public bool CreateCustomerIfNotFound { get; set; } = false;
    public bool AttachOriginalEmail { get; set; } = true;
    public int MaxAttachmentSizeMB { get; set; } = 10;
    public List<string> AllowedDomains { get; set; } = new();
    public List<string> BlockedDomains { get; set; } = new();
    public List<string> IgnoreSubjectPatterns { get; set; } = new();
    public Dictionary<string, int> PriorityKeywords { get; set; } = new();
}
