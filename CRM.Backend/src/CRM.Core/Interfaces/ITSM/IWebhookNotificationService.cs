// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.DTOs.ITSM;

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Service for sending webhook notifications for ITSM events.
/// </summary>
public interface IWebhookNotificationService
{
    /// <summary>
    /// Send a webhook notification for an event.
    /// </summary>
    Task SendWebhookAsync(WebhookEventType eventType, object payload);

    /// <summary>
    /// Register a webhook subscription.
    /// </summary>
    Task<WebhookSubscriptionDto> CreateSubscriptionAsync(CreateWebhookSubscriptionDto dto, int createdByUserId);

    /// <summary>
    /// Get all webhook subscriptions.
    /// </summary>
    Task<IEnumerable<WebhookSubscriptionDto>> GetSubscriptionsAsync();

    /// <summary>
    /// Get webhook subscription by ID.
    /// </summary>
    Task<WebhookSubscriptionDto?> GetSubscriptionByIdAsync(int id);

    /// <summary>
    /// Update a webhook subscription.
    /// </summary>
    Task<WebhookSubscriptionDto> UpdateSubscriptionAsync(int id, UpdateWebhookSubscriptionDto dto, int modifiedByUserId);

    /// <summary>
    /// Delete a webhook subscription.
    /// </summary>
    Task<bool> DeleteSubscriptionAsync(int id);

    /// <summary>
    /// Get webhook delivery history.
    /// </summary>
    Task<IEnumerable<WebhookDeliveryDto>> GetDeliveryHistoryAsync(int? subscriptionId = null, int pageNumber = 1, int pageSize = 50);

    /// <summary>
    /// Retry a failed webhook delivery.
    /// </summary>
    Task<bool> RetryDeliveryAsync(int deliveryId);
}

/// <summary>
/// Types of ITSM events that can trigger webhooks.
/// </summary>
public enum WebhookEventType
{
    // Incident Events
    IncidentCreated = 1,
    IncidentUpdated = 2,
    IncidentAssigned = 3,
    IncidentEscalated = 4,
    IncidentResolved = 5,
    IncidentClosed = 6,
    IncidentReopened = 7,

    // Problem Events
    ProblemCreated = 10,
    ProblemUpdated = 11,
    ProblemRootCauseIdentified = 12,
    ProblemResolved = 13,

    // Change Events
    ChangeCreated = 20,
    ChangeSubmittedForApproval = 21,
    ChangeApproved = 22,
    ChangeRejected = 23,
    ChangeScheduled = 24,
    ChangeImplemented = 25,
    ChangeCompleted = 26,
    ChangeFailed = 27,

    // SLA Events
    SLABreached = 30,
    SLAAtRisk = 31,
    SLAPaused = 32,
    SLAResumed = 33,

    // Knowledge Events
    ArticlePublished = 40,
    ArticleRetired = 41,

    // Catalog Events
    CatalogRequestCreated = 50,
    CatalogRequestApproved = 51,
    CatalogRequestRejected = 52,
    CatalogRequestFulfilled = 53,

    // CMDB Events
    CICreated = 60,
    CIUpdated = 61,
    CIDecommissioned = 62
}
