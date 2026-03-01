// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Intercom;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers.Webhooks;

/// <summary>
/// Webhook endpoint for Intercom events.
/// Creates CRM Activities from chat conversations and syncs contacts.
/// </summary>
[ApiController]
[Route("api/webhooks/intercom")]
public class IntercomWebhookController : CrmControllerBase
{
    private readonly IChatPort _chatProvider;
    private readonly IActivityService _activityService;
    private readonly IContactsService _contactsService;
    private readonly IntercomConfiguration _config;
    private readonly ILogger<IntercomWebhookController> _logger;

    public IntercomWebhookController(
        IProviderFactory<IChatPort> chatProviderFactory,
        IActivityService activityService,
        IContactsService contactsService,
        IOptions<IntercomConfiguration> config,
        ILogger<IntercomWebhookController> logger)
    {
        // Get the Intercom provider specifically (even if it's not the active provider)
        _chatProvider = chatProviderFactory.GetProvider("Intercom");
        _activityService = activityService;
        _contactsService = contactsService;
        _config = config.Value;
        _logger = logger;
    }

    /// <summary>
    /// Receives webhook events from Intercom.
    /// Intercom sends events for conversations, contacts, and messages.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleWebhook(CancellationToken cancellationToken)
    {
        // Read the raw body for signature validation
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync(cancellationToken);

        _logger.LogInformation("Received Intercom webhook");

        // Validate signature if configured
        var signature = Request.Headers["X-Hub-Signature"].FirstOrDefault();
        if (!string.IsNullOrEmpty(_config.WebhookSecret) && !ValidateSignature(body, signature))
        {
            _logger.LogWarning("Invalid Intercom webhook signature");
            return Unauthorized("Invalid signature");
        }

        try
        {
            // Parse the event to determine type
            var jsonDoc = JsonDocument.Parse(body);
            var topic = jsonDoc.RootElement.TryGetProperty("topic", out var topicProp)
                ? topicProp.GetString()
                : null;

            if (string.IsNullOrEmpty(topic))
            {
                _logger.LogWarning("Intercom webhook missing topic");
                return BadRequest("Missing topic");
            }

            _logger.LogInformation("Processing Intercom webhook: {Topic}", topic);

            // Process the webhook through the provider
            var result = await _chatProvider.ProcessWebhookAsync(topic, body, signature, cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning("Failed to process Intercom webhook: {Error}", result.Error);
                return BadRequest(result.Error);
            }

            // Create CRM Activity if applicable
            if (result.ActivityMapping != null)
            {
                await CreateActivityFromMappingAsync(result, cancellationToken);
            }

            // Handle contact sync for contact events
            if (topic.StartsWith("contact.") && !string.IsNullOrEmpty(result.ContactExternalId))
            {
                await SyncContactAsync(result.ContactExternalId, cancellationToken);
            }

            return Ok(new { status = "processed", eventType = topic });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Intercom webhook payload");
            return BadRequest("Invalid JSON payload");
        }
    }

    /// <summary>
    /// Verification endpoint for Intercom webhook setup.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Verify([FromQuery] string? hub_challenge)
    {
        if (!string.IsNullOrEmpty(hub_challenge))
        {
            return Ok(hub_challenge);
        }

        return Ok(new { status = "ready", provider = "intercom" });
    }

    private bool ValidateSignature(string payload, string? signature)
    {
        if (string.IsNullOrEmpty(_config.WebhookSecret) || string.IsNullOrEmpty(signature))
        {
            return true; // Skip validation if not configured
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config.WebhookSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computedSignature = "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();

        return string.Equals(computedSignature, signature, StringComparison.OrdinalIgnoreCase);
    }

    private async Task CreateActivityFromMappingAsync(
        ChatWebhookResult result,
        CancellationToken cancellationToken)
    {
        if (result.ActivityMapping == null)
        {
            return;
        }

        var mapping = result.ActivityMapping;

        // Try to find matching CRM contact
        int? contactId = mapping.ContactId;
        int? accountId = mapping.AccountId;

        if (!contactId.HasValue && !string.IsNullOrEmpty(result.ContactExternalId))
        {
            var contact = await _chatProvider.GetContactAsync(result.ContactExternalId, cancellationToken);
            if (contact != null)
            {
                contactId = contact.CrmContactId;
                accountId = contact.CrmAccountId;

                // If still not linked, try to find by email
                if (!contactId.HasValue && !string.IsNullOrEmpty(contact.Email))
                {
                    var crmContacts = await _contactsService.GetAllAsync();
                    var matchedContact = crmContacts.FirstOrDefault(c =>
                        c.EmailPrimary?.Equals(contact.Email, StringComparison.OrdinalIgnoreCase) == true);

                    if (matchedContact != null)
                    {
                        contactId = matchedContact.Id;
                        accountId = matchedContact.AccountId;
                    }
                }
            }
        }

        // Create the Activity
        var activity = new Activity
        {
            ActivityType = ActivityType.ChatMessage,
            Title = mapping.Title,
            Description = mapping.Description,
            Details = JsonSerializer.Serialize(new
            {
                conversationId = result.ConversationExternalId,
                messageId = result.Message?.ExternalId,
                channel = mapping.Channel,
                direction = mapping.Direction,
                provider = "Intercom"
            }),
            ActivityDate = mapping.ActivityDate,
            ContactId = contactId,
            AccountId = accountId,
            EntityType = contactId.HasValue ? "Contact" : (accountId.HasValue ? "Account" : null),
            EntityId = contactId ?? accountId,
            EntityName = null, // Will be populated by service
            Source = "Intercom",
            IsSystem = true,
            Category = "Chat"
        };

        await _activityService.CreateAsync(activity);

        _logger.LogInformation(
            "Created Activity for Intercom event: {EventType}, Contact: {ContactId}, Account: {AccountId}",
            result.EventType, contactId, accountId);
    }

    private async Task SyncContactAsync(string externalId, CancellationToken cancellationToken)
    {
        try
        {
            var intercomContact = await _chatProvider.GetContactAsync(externalId, cancellationToken);

            if (intercomContact == null || string.IsNullOrEmpty(intercomContact.Email))
            {
                return;
            }

            // Find existing CRM contact
            var crmContacts = await _contactsService.GetAllAsync();
            var existingContact = crmContacts.FirstOrDefault(c =>
                c.EmailPrimary?.Equals(intercomContact.Email, StringComparison.OrdinalIgnoreCase) == true);

            if (existingContact != null)
            {
                // Update the Intercom contact with CRM IDs if not already linked
                if (!intercomContact.CrmContactId.HasValue)
                {
                    await _chatProvider.UpdateContactAsync(externalId, new ChatContactUpdateRequest
                    {
                        CustomAttributes = new Dictionary<string, object>
                        {
                            { "crm_contact_id", existingContact.Id },
                            { "crm_account_id", existingContact.AccountId ?? 0 }
                        }
                    }, cancellationToken);

                    _logger.LogInformation(
                        "Linked Intercom contact {ExternalId} to CRM Contact {ContactId}",
                        externalId, existingContact.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to sync Intercom contact: {ExternalId}", externalId);
        }
    }
}
