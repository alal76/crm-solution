// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.DocuSign;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers.Webhooks;

/// <summary>
/// Webhook controller for DocuSign Connect events.
/// Handles envelope status changes and updates CRM entities accordingly.
/// </summary>
[ApiController]
[Route("api/webhooks/docusign")]
[AllowAnonymous]
public class DocuSignWebhookController : CrmControllerBase
{
    private readonly DocuSignConfiguration _config;
    private readonly ISignaturePort _signatureProvider;
    private readonly IActivityService _activityService;
    private readonly ILogger<DocuSignWebhookController> _logger;

    public DocuSignWebhookController(
        IOptions<DocuSignConfiguration> config,
        ISignaturePort signatureProvider,
        IActivityService activityService,
        ILogger<DocuSignWebhookController> logger)
    {
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _signatureProvider = signatureProvider ?? throw new ArgumentNullException(nameof(signatureProvider));
        _activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles DocuSign Connect webhook events.
    /// Supports both JSON and XML payloads (DocuSign default is XML).
    /// </summary>
    [HttpPost]
    [Consumes("application/json", "application/xml", "text/xml")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleWebhook(CancellationToken cancellationToken)
    {
                // Read the raw body
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var body = await reader.ReadToEndAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(body))
        {
            _logger.LogWarning("Received empty webhook payload from DocuSign");
            return BadRequest("Empty payload");
        }

        // Get signature header for validation
        var signature = Request.Headers["X-DocuSign-Signature-1"].FirstOrDefault();

        // Validate signature if configured
        if (!string.IsNullOrWhiteSpace(_config.WebhookSecret))
        {
            if (!ValidateSignature(body, signature))
            {
                _logger.LogWarning("DocuSign webhook signature validation failed");
                return Unauthorized("Invalid signature");
            }
        }

        // Parse the payload (detect JSON vs XML)
        DocuSignWebhookEvent? webhookEvent;

        if (body.TrimStart().StartsWith("{"))
        {
            webhookEvent = ParseJsonPayload(body);
        }
        else
        {
            webhookEvent = ParseXmlPayload(body);
        }

        if (webhookEvent == null)
        {
            _logger.LogWarning("Failed to parse DocuSign webhook payload");
            return BadRequest("Invalid payload format");
        }

        _logger.LogInformation(
            "Received DocuSign webhook: EnvelopeId={EnvelopeId}, Status={Status}",
            webhookEvent.EnvelopeId,
            webhookEvent.Status);

        // Process the event
        await ProcessEnvelopeStatusChange(webhookEvent, cancellationToken);

        return Ok(new { received = true, envelopeId = webhookEvent.EnvelopeId });
    }

    /// <summary>
    /// Validates the HMAC-SHA256 signature from DocuSign.
    /// </summary>
    private bool ValidateSignature(string payload, string? signature)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config.WebhookSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computedSignature = Convert.ToBase64String(hash);

        return computedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Parses JSON payload from DocuSign Connect.
    /// </summary>
    private DocuSignWebhookEvent? ParseJsonPayload(string body)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // DocuSign Connect JSON structure
            var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var envelope = root.TryGetProperty("envelopeSummary", out var envSummary)
                ? envSummary
                : root;

            return new DocuSignWebhookEvent
            {
                EnvelopeId = GetStringValue(envelope, "envelopeId"),
                Status = GetStringValue(envelope, "status"),
                StatusChangedDateTime = GetDateTimeValue(envelope, "statusChangedDateTime"),
                EmailSubject = GetStringValue(envelope, "emailSubject"),
                SenderEmail = GetSenderEmail(root),
                Recipients = ParseRecipients(envelope)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse DocuSign JSON payload");
            return null;
        }
    }

    /// <summary>
    /// Parses XML payload from DocuSign Connect (default format).
    /// </summary>
    private DocuSignWebhookEvent? ParseXmlPayload(string body)
    {
        try
        {
            var doc = XDocument.Parse(body);
            var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

            var envelopeStatus = doc.Descendants(ns + "EnvelopeStatus").FirstOrDefault();
            if (envelopeStatus == null)
            {
                return null;
            }

            return new DocuSignWebhookEvent
            {
                EnvelopeId = envelopeStatus.Element(ns + "EnvelopeID")?.Value,
                Status = envelopeStatus.Element(ns + "Status")?.Value,
                StatusChangedDateTime = DateTime.TryParse(
                    envelopeStatus.Element(ns + "StatusChangedDateTime")?.Value,
                    out var dt) ? dt : DateTime.UtcNow,
                EmailSubject = envelopeStatus.Element(ns + "Subject")?.Value,
                SenderEmail = envelopeStatus.Element(ns + "Sender")?.Element(ns + "Email")?.Value,
                Recipients = ParseXmlRecipients(envelopeStatus, ns)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse DocuSign XML payload");
            return null;
        }
    }

    private static string? GetStringValue(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null;
    }

    private static DateTime? GetDateTimeValue(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop))
        {
            if (prop.ValueKind == JsonValueKind.String && DateTime.TryParse(prop.GetString(), out var dt))
            {
                return dt;
            }
        }
        return null;
    }

    private static string? GetSenderEmail(JsonElement root)
    {
        if (root.TryGetProperty("sender", out var sender))
        {
            return GetStringValue(sender, "email");
        }
        return null;
    }

    private static List<RecipientEvent> ParseRecipients(JsonElement envelope)
    {
        var recipients = new List<RecipientEvent>();

        if (envelope.TryGetProperty("recipients", out var recipientsElement))
        {
            if (recipientsElement.TryGetProperty("signers", out var signers))
            {
                foreach (var signer in signers.EnumerateArray())
                {
                    recipients.Add(new RecipientEvent
                    {
                        Email = GetStringValue(signer, "email"),
                        Name = GetStringValue(signer, "name"),
                        Status = GetStringValue(signer, "status"),
                        SignedDateTime = GetDateTimeValue(signer, "signedDateTime"),
                        DeliveredDateTime = GetDateTimeValue(signer, "deliveredDateTime"),
                        RoutingOrder = GetStringValue(signer, "routingOrder")
                    });
                }
            }
        }

        return recipients;
    }

    private static List<RecipientEvent> ParseXmlRecipients(XElement envelopeStatus, XNamespace ns)
    {
        var recipients = new List<RecipientEvent>();

        var recipientStatuses = envelopeStatus.Element(ns + "RecipientStatuses");
        if (recipientStatuses == null)
            return recipients;

        foreach (var recipientStatus in recipientStatuses.Elements(ns + "RecipientStatus"))
        {
            recipients.Add(new RecipientEvent
            {
                Email = recipientStatus.Element(ns + "Email")?.Value,
                Name = recipientStatus.Element(ns + "UserName")?.Value,
                Status = recipientStatus.Element(ns + "Status")?.Value,
                SignedDateTime = DateTime.TryParse(
                    recipientStatus.Element(ns + "Signed")?.Value, out var signed) ? signed : null,
                DeliveredDateTime = DateTime.TryParse(
                    recipientStatus.Element(ns + "Delivered")?.Value, out var delivered) ? delivered : null,
                RoutingOrder = recipientStatus.Element(ns + "RoutingOrder")?.Value
            });
        }

        return recipients;
    }

    /// <summary>
    /// Processes envelope status change and creates CRM activities.
    /// </summary>
    private async Task ProcessEnvelopeStatusChange(DocuSignWebhookEvent webhookEvent, CancellationToken cancellationToken)
    {
        // Determine activity type based on status
        var activityType = webhookEvent.Status?.ToLowerInvariant() switch
        {
            "completed" => "ContractSigned",
            "declined" => "ContractDeclined",
            "voided" => "ContractCancelled",
            "sent" => "ContractSent",
            "delivered" => "ContractViewed",
            _ => "ContractStatusChanged"
        };

        // Get signature request details to find linked entity
        var sigRequest = await _signatureProvider.GetSignatureRequestAsync(
            webhookEvent.EnvelopeId ?? string.Empty,
            cancellationToken);

        // Create activity for timeline
        var activityDetails = new
        {
            envelopeId = webhookEvent.EnvelopeId,
            status = webhookEvent.Status,
            emailSubject = webhookEvent.EmailSubject,
            sender = webhookEvent.SenderEmail,
            statusChangedAt = webhookEvent.StatusChangedDateTime,
            recipients = webhookEvent.Recipients?.Select(r => new
            {
                email = r.Email,
                name = r.Name,
                status = r.Status,
                signedAt = r.SignedDateTime
            })
        };

        // Process based on status
        switch (webhookEvent.Status?.ToLowerInvariant())
        {
            case "completed":
                _logger.LogInformation(
                    "DocuSign envelope {EnvelopeId} completed - all signers have signed",
                    webhookEvent.EnvelopeId);
                // Could trigger additional business logic here
                break;

            case "declined":
                var decliner = webhookEvent.Recipients?.FirstOrDefault(r =>
                    r.Status?.Equals("declined", StringComparison.OrdinalIgnoreCase) == true);
                _logger.LogInformation(
                    "DocuSign envelope {EnvelopeId} declined by {Email}",
                    webhookEvent.EnvelopeId,
                    decliner?.Email);
                break;

            case "voided":
                _logger.LogInformation(
                    "DocuSign envelope {EnvelopeId} was voided",
                    webhookEvent.EnvelopeId);
                break;
        }

        // Log activity if we have entity context
        if (sigRequest != null && !string.IsNullOrWhiteSpace(sigRequest.EntityType) &&
            sigRequest.EntityId.HasValue)
        {
            _logger.LogDebug(
                "Creating activity for {EntityType}/{EntityId} from DocuSign webhook",
                sigRequest.EntityType,
                sigRequest.EntityId.Value);

            // Activity creation would go here via IActivityService
            // The activity service integration depends on the specific implementation
        }
    }

    #region Internal Types

    private class DocuSignWebhookEvent
    {
        public string? EnvelopeId { get; set; }
        public string? Status { get; set; }
        public DateTime? StatusChangedDateTime { get; set; }
        public string? EmailSubject { get; set; }
        public string? SenderEmail { get; set; }
        public List<RecipientEvent>? Recipients { get; set; }
    }

    private class RecipientEvent
    {
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? Status { get; set; }
        public DateTime? SignedDateTime { get; set; }
        public DateTime? DeliveredDateTime { get; set; }
        public string? RoutingOrder { get; set; }
    }

    #endregion
}
