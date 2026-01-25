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
    public int? InteractionId { get; set; }
    public int? MessageId { get; set; }
    public int? CustomerId { get; set; }
    public int? ContactId { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Web form submission data
/// </summary>
public class WebFormSubmission
{
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
    public string? ConversationId { get; set; }
    public string? InReplyTo { get; set; }
}
