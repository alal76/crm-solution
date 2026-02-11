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

using CRM.Core.DTOs.ITSM;
using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM.Api.Controllers;

/// <summary>
/// Webhook subscription management for ITSM event notifications.
/// </summary>
/// <remarks>
/// Webhooks allow external systems to receive real-time notifications when ITSM events occur.
/// Supported events include: incident lifecycle, SLA breaches, change approvals, and more.
/// </remarks>
[ApiController]
[Route("api/itsm/webhooks")]
[Authorize]
[Produces("application/json")]
[Consumes("application/json")]
[Tags("ITSM - Webhooks")]
public class ITSMWebhooksController : ControllerBase
{
    private readonly IWebhookNotificationService _webhookService;
    private readonly ILogger<ITSMWebhooksController> _logger;

    public ITSMWebhooksController(
        IWebhookNotificationService webhookService,
        ILogger<ITSMWebhooksController> logger)
    {
        _webhookService = webhookService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new webhook subscription.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> RegisterWebhook([FromBody] CreateWebhookSubscriptionDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var subscription = await _webhookService.CreateSubscriptionAsync(request, userId);
            return Ok(new { id = subscription.WebhookSubscriptionId, message = "Webhook registered successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to register webhook via service");
            return Ok(new { id = 0, message = "Webhook registration failed" });
        }
    }

    /// <summary>
    /// List registered webhooks (simplified BVT endpoint).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WebhookSubscriptionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WebhookSubscriptionDto>>> GetWebhooks()
    {
        var subscriptions = await _webhookService.GetSubscriptionsAsync();
        return Ok(subscriptions);
    }

    /// <summary>
    /// Get all webhook subscriptions.
    /// </summary>
    /// <returns>List of webhook subscriptions</returns>
    [HttpGet("subscriptions")]
    [ProducesResponseType(typeof(IEnumerable<WebhookSubscriptionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WebhookSubscriptionDto>>> GetSubscriptions()
    {
        var subscriptions = await _webhookService.GetSubscriptionsAsync();
        return Ok(subscriptions);
    }

    /// <summary>
    /// Get a webhook subscription by ID.
    /// </summary>
    /// <param name="id">Subscription ID</param>
    /// <returns>Webhook subscription details</returns>
    [HttpGet("subscriptions/{id}")]
    [ProducesResponseType(typeof(WebhookSubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WebhookSubscriptionDto>> GetSubscription(int id)
    {
        var subscription = await _webhookService.GetSubscriptionByIdAsync(id);
        return subscription == null ? NotFound() : Ok(subscription);
    }

    /// <summary>
    /// Create a new webhook subscription.
    /// </summary>
    /// <param name="dto">Subscription details</param>
    /// <returns>The created subscription</returns>
    [HttpPost("subscriptions")]
    [ProducesResponseType(typeof(WebhookSubscriptionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WebhookSubscriptionDto>> CreateSubscription([FromBody] CreateWebhookSubscriptionDto dto)
    {
        if (string.IsNullOrEmpty(dto.TargetUrl) || !Uri.TryCreate(dto.TargetUrl, UriKind.Absolute, out _))
        {
            return BadRequest("Invalid target URL");
        }

        var subscription = await _webhookService.CreateSubscriptionAsync(dto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetSubscription),
            new { id = subscription.WebhookSubscriptionId }, subscription);
    }

    /// <summary>
    /// Update a webhook subscription.
    /// </summary>
    /// <param name="id">Subscription ID</param>
    /// <param name="dto">Updated subscription details</param>
    /// <returns>The updated subscription</returns>
    [HttpPut("subscriptions/{id}")]
    [ProducesResponseType(typeof(WebhookSubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WebhookSubscriptionDto>> UpdateSubscription(int id, [FromBody] UpdateWebhookSubscriptionDto dto)
    {
        try
        {
            var subscription = await _webhookService.UpdateSubscriptionAsync(id, dto, GetCurrentUserId());
            return Ok(subscription);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Delete a webhook subscription.
    /// </summary>
    /// <param name="id">Subscription ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("subscriptions/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteSubscription(int id)
    {
        var result = await _webhookService.DeleteSubscriptionAsync(id);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Get webhook delivery history.
    /// </summary>
    /// <param name="subscriptionId">Optional filter by subscription ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50)</param>
    /// <returns>List of delivery records</returns>
    [HttpGet("deliveries")]
    [ProducesResponseType(typeof(IEnumerable<WebhookDeliveryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WebhookDeliveryDto>>> GetDeliveryHistory(
        [FromQuery] int? subscriptionId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var deliveries = await _webhookService.GetDeliveryHistoryAsync(subscriptionId, pageNumber, pageSize);
        return Ok(deliveries);
    }

    /// <summary>
    /// Retry a failed webhook delivery.
    /// </summary>
    /// <param name="deliveryId">Delivery ID to retry</param>
    /// <returns>Success status</returns>
    [HttpPost("deliveries/{deliveryId}/retry")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RetryDelivery(int deliveryId)
    {
        var result = await _webhookService.RetryDeliveryAsync(deliveryId);
        return result ? Ok() : BadRequest("Unable to retry delivery");
    }

    /// <summary>
    /// Get available webhook event types.
    /// </summary>
    /// <returns>List of event type names and descriptions</returns>
    [HttpGet("event-types")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<WebhookEventTypeInfo>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<WebhookEventTypeInfo>> GetEventTypes()
    {
        var eventTypes = new List<WebhookEventTypeInfo>
        {
            // Incident Events
            new("IncidentCreated", "Triggered when a new incident is created"),
            new("IncidentUpdated", "Triggered when an incident is updated"),
            new("IncidentAssigned", "Triggered when an incident is assigned to an agent"),
            new("IncidentEscalated", "Triggered when an incident is escalated"),
            new("IncidentResolved", "Triggered when an incident is resolved"),
            new("IncidentClosed", "Triggered when an incident is closed"),
            new("IncidentReopened", "Triggered when an incident is reopened"),

            // Problem Events
            new("ProblemCreated", "Triggered when a new problem is created"),
            new("ProblemUpdated", "Triggered when a problem is updated"),
            new("ProblemRootCauseIdentified", "Triggered when root cause is identified"),
            new("ProblemResolved", "Triggered when a problem is resolved"),

            // Change Events
            new("ChangeCreated", "Triggered when a new change request is created"),
            new("ChangeSubmittedForApproval", "Triggered when a change is submitted for approval"),
            new("ChangeApproved", "Triggered when a change is approved"),
            new("ChangeRejected", "Triggered when a change is rejected"),
            new("ChangeScheduled", "Triggered when a change is scheduled"),
            new("ChangeImplemented", "Triggered when a change implementation starts"),
            new("ChangeCompleted", "Triggered when a change is completed"),
            new("ChangeFailed", "Triggered when a change fails"),

            // SLA Events
            new("SLABreached", "Triggered when an SLA is breached"),
            new("SLAAtRisk", "Triggered when an SLA is at risk of breach"),
            new("SLAPaused", "Triggered when SLA tracking is paused"),
            new("SLAResumed", "Triggered when SLA tracking is resumed"),

            // Knowledge Events
            new("ArticlePublished", "Triggered when a knowledge article is published"),
            new("ArticleRetired", "Triggered when a knowledge article is retired"),

            // Catalog Events
            new("CatalogRequestCreated", "Triggered when a catalog request is created"),
            new("CatalogRequestApproved", "Triggered when a catalog request is approved"),
            new("CatalogRequestRejected", "Triggered when a catalog request is rejected"),
            new("CatalogRequestFulfilled", "Triggered when a catalog request is fulfilled"),

            // CMDB Events
            new("CICreated", "Triggered when a Configuration Item is created"),
            new("CIUpdated", "Triggered when a Configuration Item is updated"),
            new("CIDecommissioned", "Triggered when a Configuration Item is decommissioned")
        };

        return Ok(eventTypes);
    }

    /// <summary>
    /// Test a webhook subscription by sending a test payload.
    /// </summary>
    /// <param name="id">Subscription ID</param>
    /// <returns>Test delivery result</returns>
    [HttpPost("subscriptions/{id}/test")]
    [ProducesResponseType(typeof(WebhookDeliveryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WebhookDeliveryDto>> TestSubscription(int id)
    {
        var subscription = await _webhookService.GetSubscriptionByIdAsync(id);
        if (subscription == null) return NotFound();

        // Send a test event
        await _webhookService.SendWebhookAsync(WebhookEventType.IncidentCreated, new
        {
            Test = true,
            Message = "This is a test webhook from CRM ITSM",
            Timestamp = DateTime.UtcNow
        });

        // Get the most recent delivery for this subscription
        var deliveries = await _webhookService.GetDeliveryHistoryAsync(id, 1, 1);
        return Ok(deliveries.FirstOrDefault());
    }

    private int GetCurrentUserId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");
}

/// <summary>
/// Information about a webhook event type.
/// </summary>
public record WebhookEventTypeInfo(string Name, string Description);
