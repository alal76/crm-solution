// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Chatwoot;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CRM.Api.Controllers.Webhooks;

/// <summary>
/// Handles webhook callbacks from Chatwoot chat platform.
/// Processes conversation and message events and syncs them to the CRM Activity timeline.
/// </summary>
[ApiController]
[Route("api/webhooks/chatwoot")]
public class ChatwootWebhookController : ControllerBase
{
    private readonly IChatPort _chatProvider;
    private readonly IActivityService _activityService;
    private readonly IContactsService _contactsService;
    private readonly ChatwootConfiguration _config;
    private readonly ILogger<ChatwootWebhookController> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ChatwootWebhookController(
        IChatPort chatProvider,
        IActivityService activityService,
        IContactsService contactsService,
        IOptions<ChatwootConfiguration> config,
        ILogger<ChatwootWebhookController> logger)
    {
        _chatProvider = chatProvider;
        _activityService = activityService;
        _contactsService = contactsService;
        _config = config.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Receives all webhooks from Chatwoot.
    /// Configure this endpoint in Chatwoot under Settings → Integrations → Webhooks.
    /// </summary>
    /// <remarks>
    /// Chatwoot webhook events include:
    /// - conversation_created: New conversation started
    /// - conversation_status_changed: Conversation opened/resolved/pending
    /// - conversation_updated: Conversation metadata changed
    /// - message_created: New message in conversation
    /// - message_updated: Message edited
    /// - webwidget_triggered: Widget event (e.g., form submission)
    /// - contact_created: New contact created
    /// - contact_updated: Contact details updated
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleWebhook(
        [FromHeader(Name = "X-Chatwoot-Signature")] string? signature)
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();

            if (string.IsNullOrEmpty(payload))
            {
                _logger.LogWarning("Empty webhook payload received from Chatwoot");
                return BadRequest("Empty payload");
            }

            // Validate signature if webhook secret is configured
            if (!string.IsNullOrEmpty(_config.WebhookSecret))
            {
                if (!ValidateSignature(payload, signature))
                {
                    _logger.LogWarning("Invalid Chatwoot webhook signature");
                    return Unauthorized("Invalid signature");
                }
            }

            _logger.LogInformation("Received Chatwoot webhook. Payload length: {Length}", payload.Length);

            // Parse the webhook event
            var webhookEvent = JsonSerializer.Deserialize<ChatwootWebhookEvent>(payload, _jsonOptions);

            if (webhookEvent == null)
            {
                _logger.LogWarning("Failed to parse Chatwoot webhook payload");
                return BadRequest("Invalid payload format");
            }

            _logger.LogInformation("Processing Chatwoot webhook event: {EventType}", webhookEvent.Event);

            // Process based on event type
            var result = await ProcessWebhookEventAsync(webhookEvent, payload);

            return Ok(new
            {
                status = "processed",
                eventType = webhookEvent.Event,
                activityCreated = result.ActivityCreated,
                conversationId = result.ConversationId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Chatwoot webhook");
            // Return 200 to prevent Chatwoot from retrying indefinitely
            return Ok(new { status = "error", message = ex.Message });
        }
    }

    /// <summary>
    /// Verify webhook connectivity.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult VerifyWebhook()
    {
        return Ok(new
        {
            status = "active",
            provider = "chatwoot",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Validates the Chatwoot webhook signature.
    /// Chatwoot uses HMAC-SHA256 with the webhook secret.
    /// </summary>
    private bool ValidateSignature(string payload, string? signature)
    {
        if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(_config.WebhookSecret))
        {
            return false;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config.WebhookSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var expectedSignature = Convert.ToBase64String(hash);

        // Chatwoot may send signature as hex or base64
        var signatureHex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

        return signature == expectedSignature ||
               signature == signatureHex ||
               (signature.StartsWith("sha256=") && signature[7..] == signatureHex);
    }

    /// <summary>
    /// Processes a webhook event and creates appropriate activities.
    /// </summary>
    private async Task<WebhookProcessingResult> ProcessWebhookEventAsync(
        ChatwootWebhookEvent webhookEvent,
        string rawPayload)
    {
        var result = new WebhookProcessingResult
        {
            EventType = webhookEvent.Event,
            ConversationId = webhookEvent.Conversation?.Id.ToString()
        };

        switch (webhookEvent.Event?.ToLowerInvariant())
        {
            case "conversation_created":
                await HandleConversationCreatedAsync(webhookEvent);
                result.ActivityCreated = true;
                break;

            case "conversation_status_changed":
                await HandleConversationStatusChangedAsync(webhookEvent);
                result.ActivityCreated = true;
                break;

            case "message_created":
                await HandleMessageCreatedAsync(webhookEvent);
                result.ActivityCreated = true;
                break;

            case "contact_created":
                await HandleContactCreatedAsync(webhookEvent);
                result.ActivityCreated = false; // Contact sync only
                break;

            case "contact_updated":
                await HandleContactUpdatedAsync(webhookEvent);
                result.ActivityCreated = false; // Contact sync only
                break;

            default:
                _logger.LogDebug("Unhandled Chatwoot webhook event type: {EventType}", webhookEvent.Event);
                break;
        }

        // Also pass to the provider for any additional processing
        if (_chatProvider.ProviderName == "Chatwoot")
        {
            await _chatProvider.ProcessWebhookAsync(
                webhookEvent.Event ?? "unknown",
                rawPayload,
                Request.Headers["X-Chatwoot-Signature"].FirstOrDefault());
        }

        return result;
    }

    /// <summary>
    /// Handles conversation_created webhook - creates a new Activity entry.
    /// </summary>
    private async Task HandleConversationCreatedAsync(ChatwootWebhookEvent webhookEvent)
    {
        if (webhookEvent.Conversation == null)
        {
            _logger.LogWarning("Conversation data missing from conversation_created webhook");
            return;
        }

        var contactId = await FindCrmContactIdAsync(webhookEvent);

        var activity = new Activity
        {
            ActivityType = ActivityType.ChatMessage,
            Title = $"New chat conversation started via {webhookEvent.Conversation.Channel ?? "web"}",
            Description = $"Conversation #{webhookEvent.Conversation.Id} created in Chatwoot",
            Details = JsonSerializer.Serialize(new
            {
                chatwootConversationId = webhookEvent.Conversation.Id,
                chatwootContactId = webhookEvent.Conversation.Meta?.Sender?.Id,
                channel = webhookEvent.Conversation.Channel,
                inbox = webhookEvent.Inbox?.Name,
                inboxId = webhookEvent.Inbox?.Id
            }),
            ActivityDate = DateTime.UtcNow,
            ContactId = contactId,
            AccountId = await GetAccountIdForContactAsync(contactId),
            EntityType = "Conversation",
            EntityId = webhookEvent.Conversation.Id,
            EntityName = $"Chat #{webhookEvent.Conversation.Id}",
            Source = "Chatwoot",
            IsSystem = true,
            Category = "Chat"
        };

        await _activityService.CreateAsync(activity);

        _logger.LogInformation(
            "Created activity for Chatwoot conversation: {ConversationId}, ContactId: {ContactId}",
            webhookEvent.Conversation.Id, contactId);
    }

    /// <summary>
    /// Handles conversation_status_changed webhook.
    /// </summary>
    private async Task HandleConversationStatusChangedAsync(ChatwootWebhookEvent webhookEvent)
    {
        if (webhookEvent.Conversation == null)
        {
            return;
        }

        var contactId = await FindCrmContactIdAsync(webhookEvent);
        var status = webhookEvent.Conversation.Status ?? "unknown";

        var title = status.ToLowerInvariant() switch
        {
            "resolved" => "Chat conversation resolved",
            "open" => "Chat conversation reopened",
            "pending" => "Chat conversation marked pending",
            _ => $"Chat conversation status changed to {status}"
        };

        var activity = new Activity
        {
            ActivityType = ActivityType.StatusChanged,
            Title = title,
            Description = $"Conversation #{webhookEvent.Conversation.Id} status changed to {status}",
            Details = JsonSerializer.Serialize(new
            {
                chatwootConversationId = webhookEvent.Conversation.Id,
                newStatus = status,
                assignee = webhookEvent.Conversation.Meta?.Assignee?.Name
            }),
            ActivityDate = DateTime.UtcNow,
            ContactId = contactId,
            AccountId = await GetAccountIdForContactAsync(contactId),
            EntityType = "Conversation",
            EntityId = webhookEvent.Conversation.Id,
            OldValue = null, // We don't have the old status
            NewValue = status,
            Source = "Chatwoot",
            IsSystem = true,
            Category = "Chat"
        };

        await _activityService.CreateAsync(activity);
    }

    /// <summary>
    /// Handles message_created webhook - creates Activity for each message.
    /// </summary>
    private async Task HandleMessageCreatedAsync(ChatwootWebhookEvent webhookEvent)
    {
        if (webhookEvent.Conversation == null)
        {
            return;
        }

        var contactId = await FindCrmContactIdAsync(webhookEvent);
        var messageType = webhookEvent.MessageType ?? "unknown";
        var isIncoming = messageType == "0" || messageType.ToLowerInvariant() == "incoming";

        // Truncate long messages for the title
        var messagePreview = webhookEvent.Content?.Length > 100
            ? webhookEvent.Content[..100] + "..."
            : webhookEvent.Content ?? "(no content)";

        var activity = new Activity
        {
            ActivityType = ActivityType.ChatMessage,
            Title = isIncoming
                ? $"Chat message from {webhookEvent.Sender?.Name ?? "Customer"}"
                : $"Chat reply by {webhookEvent.Sender?.Name ?? "Agent"}",
            Description = messagePreview,
            Details = JsonSerializer.Serialize(new
            {
                chatwootConversationId = webhookEvent.Conversation.Id,
                chatwootMessageId = webhookEvent.Id,
                messageType,
                isIncoming,
                senderId = webhookEvent.Sender?.Id,
                senderName = webhookEvent.Sender?.Name,
                senderType = webhookEvent.Sender?.Type,
                channel = webhookEvent.Conversation.Channel,
                isPrivate = webhookEvent.Private,
                contentType = webhookEvent.ContentType,
                attachments = webhookEvent.Attachments?.Select(a => new { a.DataUrl, a.FileName, a.FileType })
            }),
            ActivityDate = webhookEvent.CreatedAt ?? DateTime.UtcNow,
            ContactId = contactId,
            AccountId = await GetAccountIdForContactAsync(contactId),
            EntityType = "ChatMessage",
            EntityId = webhookEvent.Id,
            EntityName = $"Message in Chat #{webhookEvent.Conversation.Id}",
            UserName = isIncoming ? null : webhookEvent.Sender?.Name,
            Source = "Chatwoot",
            IsSystem = false, // Messages are user interactions
            Category = "Chat"
        };

        await _activityService.CreateAsync(activity);

        _logger.LogDebug(
            "Created activity for Chatwoot message: {MessageId} in conversation {ConversationId}",
            webhookEvent.Id, webhookEvent.Conversation.Id);
    }

    /// <summary>
    /// Handles contact_created webhook - syncs contact to CRM.
    /// </summary>
    private async Task HandleContactCreatedAsync(ChatwootWebhookEvent webhookEvent)
    {
        if (webhookEvent.Contact == null)
        {
            return;
        }

        _logger.LogInformation(
            "Chatwoot contact created: {ContactId}, Email: {Email}",
            webhookEvent.Contact.Id, webhookEvent.Contact.Email);

        // Try to find or create a matching CRM contact
        await FindOrCreateCrmContactAsync(webhookEvent.Contact);
    }

    /// <summary>
    /// Handles contact_updated webhook - syncs contact updates to CRM.
    /// </summary>
    private async Task HandleContactUpdatedAsync(ChatwootWebhookEvent webhookEvent)
    {
        if (webhookEvent.Contact == null)
        {
            return;
        }

        _logger.LogInformation(
            "Chatwoot contact updated: {ContactId}, Email: {Email}",
            webhookEvent.Contact.Id, webhookEvent.Contact.Email);

        try
        {
            // Look up the CRM contact by email
            if (string.IsNullOrEmpty(webhookEvent.Contact.Email))
            {
                _logger.LogDebug(
                    "Chatwoot contact {ChatwootContactId} has no email, skipping CRM sync",
                    webhookEvent.Contact.Id);
                return;
            }

            var allContacts = await _contactsService.GetAllAsync();
            var crmContact = allContacts.FirstOrDefault(c =>
                c.EmailPrimary?.Equals(webhookEvent.Contact.Email, StringComparison.OrdinalIgnoreCase) == true);

            if (crmContact == null)
            {
                _logger.LogDebug(
                    "No matching CRM contact found for Chatwoot contact {ChatwootContactId} with email {Email}",
                    webhookEvent.Contact.Id, webhookEvent.Contact.Email);
                return;
            }

            // Parse name from Chatwoot into first/last name
            var fullName = webhookEvent.Contact.Name?.Trim();
            var nameParts = string.IsNullOrWhiteSpace(fullName)
                ? Array.Empty<string>()
                : fullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

            var firstName = nameParts.Length > 0 ? nameParts[0] : null;
            var lastName = nameParts.Length > 1 ? nameParts[1] : null;

            // Only update fields that Chatwoot actually provides and that differ
            var hasChanges = false;
            var updateRequest = new UpdateContactRequest();
            var updatedFields = new List<string>();

            if (firstName != null && !firstName.Equals(crmContact.FirstName, StringComparison.Ordinal))
            {
                updateRequest.FirstName = firstName;
                updatedFields.Add($"FirstName: '{crmContact.FirstName}' → '{firstName}'");
                hasChanges = true;
            }

            if (lastName != null && !lastName.Equals(crmContact.LastName, StringComparison.Ordinal))
            {
                updateRequest.LastName = lastName;
                updatedFields.Add($"LastName: '{crmContact.LastName}' → '{lastName}'");
                hasChanges = true;
            }

            if (!string.IsNullOrEmpty(webhookEvent.Contact.PhoneNumber)
                && !webhookEvent.Contact.PhoneNumber.Equals(crmContact.PhonePrimary, StringComparison.Ordinal))
            {
                updateRequest.PhonePrimary = webhookEvent.Contact.PhoneNumber;
                updatedFields.Add($"PhonePrimary: '{crmContact.PhonePrimary}' → '{webhookEvent.Contact.PhoneNumber}'");
                hasChanges = true;
            }

            if (!hasChanges)
            {
                _logger.LogDebug(
                    "No field changes detected for CRM contact {ContactId} from Chatwoot contact {ChatwootContactId}",
                    crmContact.Id, webhookEvent.Contact.Id);
                return;
            }

            await _contactsService.UpdateAsync(crmContact.Id, updateRequest, "Chatwoot");

            _logger.LogInformation(
                "Updated CRM contact {ContactId} from Chatwoot contact {ChatwootContactId}. Changes: {Changes}",
                crmContact.Id, webhookEvent.Contact.Id, string.Join("; ", updatedFields));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to sync Chatwoot contact update for ChatwootContactId={ChatwootContactId}, Email={Email}",
                webhookEvent.Contact.Id, webhookEvent.Contact.Email);
        }
    }

    /// <summary>
    /// Finds the CRM Contact ID from Chatwoot webhook data.
    /// Uses GetAllAsync and filters since there's no search method available.
    /// </summary>
    private async Task<int?> FindCrmContactIdAsync(ChatwootWebhookEvent webhookEvent)
    {
        // Try to find by email first
        var email = webhookEvent.Contact?.Email
            ?? webhookEvent.Conversation?.Meta?.Sender?.Email;

        if (!string.IsNullOrEmpty(email))
        {
            // Get all contacts and filter by email
            // NOTE: In production, this should use a search index or database query
            var allContacts = await _contactsService.GetAllAsync();
            var contact = allContacts.FirstOrDefault(c =>
                c.EmailPrimary?.Equals(email, StringComparison.OrdinalIgnoreCase) == true);
            if (contact != null)
            {
                return contact.Id;
            }
        }

        // Phone number lookup would require additional implementation
        // Skipped for now as we don't have a direct phone search method
        return null;
    }

    /// <summary>
    /// Gets the Account ID for a given Contact ID.
    /// </summary>
    private async Task<int?> GetAccountIdForContactAsync(int? contactId)
    {
        if (!contactId.HasValue)
        {
            return null;
        }

        var contact = await _contactsService.GetByIdAsync(contactId.Value);
        return contact?.AccountId;
    }

    /// <summary>
    /// Finds or creates a CRM contact from Chatwoot contact data.
    /// </summary>
    private async Task<int?> FindOrCreateCrmContactAsync(ChatwootContactPayload chatwootContact)
    {
        // First try to find existing contact by email
        if (!string.IsNullOrEmpty(chatwootContact.Email))
        {
            var allContacts = await _contactsService.GetAllAsync();
            var existing = allContacts.FirstOrDefault(c =>
                c.EmailPrimary?.Equals(chatwootContact.Email, StringComparison.OrdinalIgnoreCase) == true);
            if (existing != null)
            {
                return existing.Id;
            }
        }

        var fullName = chatwootContact.Name?.Trim();
        var nameParts = string.IsNullOrWhiteSpace(fullName)
            ? Array.Empty<string>()
            : fullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        var firstName = nameParts.Length > 0 ? nameParts[0] : "Chatwoot";
        var lastName = nameParts.Length > 1 ? nameParts[1] : "Contact";

        var request = new CreateContactRequest
        {
            FirstName = firstName,
            LastName = lastName,
            EmailPrimary = chatwootContact.Email,
            PhonePrimary = chatwootContact.PhoneNumber,
            ContactType = "Customer",
            Notes = "Created from Chatwoot webhook"
        };

        var created = await _contactsService.CreateAsync(request, "Chatwoot");

        _logger.LogInformation(
            "Created CRM contact {ContactId} from Chatwoot contact {ChatwootId}",
            created.Id, chatwootContact.Id);

        return created.Id;
    }

    #region Webhook DTOs

    private class ChatwootWebhookEvent
    {
        public string? Event { get; set; }
        public int Id { get; set; }
        public string? Content { get; set; }

        [JsonPropertyName("content_type")]
        public string? ContentType { get; set; }

        [JsonPropertyName("message_type")]
        public string? MessageType { get; set; }

        public bool? Private { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        public ChatwootConversationPayload? Conversation { get; set; }
        public ChatwootContactPayload? Contact { get; set; }
        public ChatwootSenderPayload? Sender { get; set; }
        public ChatwootInboxPayload? Inbox { get; set; }
        public List<ChatwootAttachmentPayload>? Attachments { get; set; }
    }

    private class ChatwootConversationPayload
    {
        public int Id { get; set; }
        public string? Status { get; set; }
        public string? Channel { get; set; }

        [JsonPropertyName("inbox_id")]
        public int? InboxId { get; set; }

        public ChatwootMetaPayload? Meta { get; set; }
    }

    private class ChatwootMetaPayload
    {
        public ChatwootSenderPayload? Sender { get; set; }
        public ChatwootAssigneePayload? Assignee { get; set; }
    }

    private class ChatwootSenderPayload
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Type { get; set; }
    }

    private class ChatwootAssigneePayload
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
    }

    private class ChatwootContactPayload
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }

        [JsonPropertyName("phone_number")]
        public string? PhoneNumber { get; set; }
    }

    private class ChatwootInboxPayload
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    private class ChatwootAttachmentPayload
    {
        [JsonPropertyName("data_url")]
        public string? DataUrl { get; set; }

        [JsonPropertyName("file_name")]
        public string? FileName { get; set; }

        [JsonPropertyName("file_type")]
        public string? FileType { get; set; }
    }

    private class WebhookProcessingResult
    {
        public string? EventType { get; set; }
        public string? ConversationId { get; set; }
        public bool ActivityCreated { get; set; }
    }

    #endregion
}
