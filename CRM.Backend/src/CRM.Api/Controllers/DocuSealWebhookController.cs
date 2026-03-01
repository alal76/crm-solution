// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Cryptography;
using System.Text;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.DocuSeal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Webhook controller for receiving DocuSeal signature events.
///
/// Webhook events:
/// - submission.created: New submission created
/// - submission.started: First signer has started signing
/// - submission.completed: All signers have completed
/// - submission.expired: Submission has expired
/// - submitter.completed: Individual signer completed
/// - submitter.opened: Individual signer opened the document
///
/// Configure webhook URL in DocuSeal: https://your-crm-api/api/webhooks/docuseal
/// </summary>
[ApiController]
[Route("api/webhooks/docuseal")]
public class DocuSealWebhookController : CrmControllerBase
{
    private readonly ISignaturePort _signatureProvider;
    private readonly IActivityService _activityService;
    private readonly IQuoteService? _quoteService;
    private readonly DocuSealConfiguration _config;
    private readonly ILogger<DocuSealWebhookController> _logger;

    public DocuSealWebhookController(
        ISignaturePort signatureProvider,
        IActivityService activityService,
        IOptions<DocuSealConfiguration> config,
        ILogger<DocuSealWebhookController> logger,
        IQuoteService? quoteService = null)
    {
        _signatureProvider = signatureProvider;
        _activityService = activityService;
        _quoteService = quoteService;
        _config = config.Value;
        _logger = logger;
    }

    /// <summary>
    /// Receives and processes DocuSeal webhook events.
    /// </summary>
    /// <remarks>
    /// DocuSeal sends a POST request with JSON payload.
    /// The webhook signature is in the X-DocuSeal-Signature header.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> HandleWebhook(CancellationToken cancellationToken)
    {
                // Read the raw payload
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);

        if (string.IsNullOrEmpty(payload))
        {
            _logger.LogWarning("Received empty webhook payload from DocuSeal");
            return BadRequest("Empty payload");
        }

        // Get the signature from headers
        var signature = Request.Headers["X-DocuSeal-Signature"].FirstOrDefault();

        // Validate signature if configured
        if (!string.IsNullOrEmpty(_config.WebhookSecret))
        {
            if (string.IsNullOrEmpty(signature))
            {
                _logger.LogWarning("Missing webhook signature header");
                return Unauthorized("Missing signature");
            }

            if (!ValidateSignature(payload, signature))
            {
                _logger.LogWarning("Invalid webhook signature");
                return Unauthorized("Invalid signature");
            }
        }

        // Get event type from headers or parse from payload
        var eventType = Request.Headers["X-DocuSeal-Event"].FirstOrDefault();
        if (string.IsNullOrEmpty(eventType))
        {
            // Try to extract from payload
            var jsonDoc = System.Text.Json.JsonDocument.Parse(payload);
            if (jsonDoc.RootElement.TryGetProperty("event_type", out var eventTypeProp))
            {
                eventType = eventTypeProp.GetString();
            }
        }

        if (string.IsNullOrEmpty(eventType))
        {
            _logger.LogWarning("Missing event type in DocuSeal webhook");
            return BadRequest("Missing event type");
        }

        _logger.LogInformation("Received DocuSeal webhook: {EventType}", eventType);

        // Process the webhook
        var result = await _signatureProvider.ProcessWebhookAsync(eventType, payload, signature, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Failed to process DocuSeal webhook: {Error}", result.Error);
            return BadRequest(result.Error);
        }

        // Create activity in CRM timeline
        if (result.ActivityMapping != null)
        {
            await CreateActivityAsync(result, cancellationToken);
        }

        // Update CRM entity status if applicable
        if (result.NewStatus.HasValue && !string.IsNullOrEmpty(result.EntityType) && result.EntityId.HasValue)
        {
            await UpdateEntityStatusAsync(result.EntityType, result.EntityId.Value, result.NewStatus.Value, cancellationToken);
        }

        _logger.LogInformation("Successfully processed DocuSeal webhook: {EventType} for request {RequestId}",
            eventType, result.RequestId);

        return Ok(new { success = true, requestId = result.RequestId, status = result.NewStatus?.ToString() });
    }

    /// <summary>
    /// Health check endpoint for webhook configuration testing.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            provider = "DocuSeal",
            status = "healthy",
            timestamp = DateTime.UtcNow,
            webhookConfigured = !string.IsNullOrEmpty(_config.WebhookSecret)
        });
    }

    private bool ValidateSignature(string payload, string signature)
    {
        if (string.IsNullOrEmpty(_config.WebhookSecret))
        {
            return true;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config.WebhookSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computedSignature = Convert.ToBase64String(hash);

        return signature.Equals(computedSignature, StringComparison.Ordinal);
    }

    private async Task CreateActivityAsync(SignatureWebhookResult result, CancellationToken cancellationToken)
    {
        try
        {
            var mapping = result.ActivityMapping!;

            var activity = new Core.Entities.Activity
            {
                ActivityType = Core.Entities.ActivityType.Other,
                Title = mapping.Title,
                Description = mapping.Description ?? "",
                Details = System.Text.Json.JsonSerializer.Serialize(new
                {
                    provider = "DocuSeal",
                    eventType = result.EventType,
                    requestId = result.RequestId,
                    signerEmail = result.SignerEmail,
                    status = result.NewStatus?.ToString(),
                    externalId = mapping.ExternalId,
                    externalSource = mapping.ExternalSource
                }),
                EntityType = result.EntityType ?? "Signature",
                EntityId = result.EntityId ?? 0,
                AccountId = mapping.AccountId,
                ContactId = mapping.ContactId,
                ActivityDate = mapping.ActivityDate,
                CreatedAt = DateTime.UtcNow,
                Source = mapping.ExternalSource // Use Source field for external reference
            };

            // Set activity type based on event (use QuoteSent/QuoteAccepted for signature events)
            if (mapping.ActivityType == "DocumentSigned")
            {
                activity.ActivityType = Core.Entities.ActivityType.QuoteAccepted; // Most appropriate for signed documents
            }
            else if (mapping.ActivityType == "DocumentSent")
            {
                activity.ActivityType = Core.Entities.ActivityType.QuoteSent; // Signature request sent
            }

            await _activityService.CreateAsync(activity);

            _logger.LogDebug("Created activity for DocuSeal event: {ActivityType}", mapping.ActivityType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create activity for DocuSeal webhook");
            // Don't throw - activity creation failure shouldn't fail the webhook
        }
    }

    private async Task UpdateEntityStatusAsync(string entityType, int entityId, SignatureStatus status, CancellationToken cancellationToken)
    {
        try
        {
            if (entityType.Equals("Quote", StringComparison.OrdinalIgnoreCase) && _quoteService != null)
            {
                // Update Quote signature status
                var quote = await _quoteService.GetByIdAsync(entityId);
                if (quote != null)
                {
                    if (status == SignatureStatus.Completed)
                    {
                        quote.IsSigned = true;
                        quote.SignedDate = DateTime.UtcNow;
                        await _quoteService.UpdateAsync(entityId, quote);
                        _logger.LogInformation("Updated Quote {QuoteId} as signed", entityId);
                    }
                }
            }
            // Add other entity types as needed (Contract, etc.)
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update entity {EntityType}:{EntityId} status", entityType, entityId);
            // Don't throw - entity update failure shouldn't fail the webhook
        }
    }
}
