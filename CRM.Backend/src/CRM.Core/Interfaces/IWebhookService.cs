// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for handling incoming webhooks from external services
/// </summary>
public interface IWebhookService
{
    /// <summary>
    /// Process web form submission
    /// </summary>
    Task<WebhookIngestResult> ProcessWebFormAsync(WebFormSubmission dto);

    /// <summary>
    /// Process inbound email from email service webhook
    /// </summary>
    Task<WebhookIngestResult> ProcessInboundEmailAsync(InboundEmail dto);

    /// <summary>
    /// Process WhatsApp webhook
    /// </summary>
    Task<WebhookIngestResult> ProcessWhatsAppWebhookAsync(string payload);

    /// <summary>
    /// Process Facebook webhook
    /// </summary>
    Task<WebhookIngestResult> ProcessFacebookWebhookAsync(string payload);

    /// <summary>
    /// Process Twitter/X webhook
    /// </summary>
    Task<WebhookIngestResult> ProcessTwitterWebhookAsync(string payload);

    /// <summary>
    /// Verify webhook authenticity
    /// </summary>
    Task<bool> VerifyWebhookAsync(string channelType, string signature, string payload);
}

/// <summary>
/// Webhook ingest result
/// </summary>
public class WebhookIngestResult
{
    public bool Success { get; set; }
    /// <summary>Alias for Success for compatibility</summary>
    public bool IsSuccess { get => Success; set => Success = value; }
    public DateTime? ProcessedAt { get; set; }
    public int? InteractionId { get; set; }
    public int? MessageId { get; set; }
    public int? AccountId { get; set; }
    public int? ContactId { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Web form submission data
/// </summary>
public class WebFormSubmission
{
    public int? FormId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Subject { get; set; }
    public string? Message { get; set; }
    public string? FormType { get; set; }
    public string? CustomFieldsJson { get; set; }
}

/// <summary>
/// Inbound email data
/// </summary>
public class InboundEmail
{
    public string? From { get; set; }
    public string? FromName { get; set; }
    public string? To { get; set; }
    public string? Subject { get; set; }
    public string? TextBody { get; set; }
    public string? HtmlBody { get; set; }
    /// <summary>Alias for HtmlBody or combined content</summary>
    public string? Body { get => HtmlBody ?? TextBody; set => HtmlBody = value; }
    public DateTime? Timestamp { get; set; }
    public string? ConversationId { get; set; }
    public string? InReplyTo { get; set; }
}
