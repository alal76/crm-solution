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

namespace CRM.Core.Ports.Output.Providers;

#region Integration Port Interface

/// <summary>
/// Output port for integration platform operations.
/// Enables workflow automation and third-party system integrations.
/// Implementations: BuiltIn (webhooks), n8n, Zapier, Make (Integromat), Workato.
/// </summary>
public interface IIntegrationPort
{
    /// <summary>
    /// Gets the unique identifier for this integration provider.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Checks if the integration provider is properly configured and available.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    #region Event Publishing

    /// <summary>
    /// Publishes a CRM event to the integration platform.
    /// </summary>
    /// <param name="crmEvent">The event to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Publication result.</returns>
    Task<EventPublishResult> PublishEventAsync(CrmEvent crmEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes multiple events in batch.
    /// </summary>
    /// <param name="events">Events to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Batch publication result.</returns>
    Task<BatchEventPublishResult> PublishEventsAsync(IEnumerable<CrmEvent> events, CancellationToken cancellationToken = default);

    #endregion

    #region Webhook Management

    /// <summary>
    /// Registers a webhook endpoint.
    /// </summary>
    /// <param name="registration">Webhook registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Registered webhook info.</returns>
    Task<WebhookInfo> RegisterWebhookAsync(WebhookRegistration registration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets registered webhooks.
    /// </summary>
    /// <param name="eventType">Optional event type filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of webhooks.</returns>
    Task<IEnumerable<WebhookInfo>> GetWebhooksAsync(string? eventType = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a webhook registration.
    /// </summary>
    /// <param name="webhookId">Webhook ID.</param>
    /// <param name="update">Updated registration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateWebhookAsync(string webhookId, WebhookRegistration update, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a webhook registration.
    /// </summary>
    /// <param name="webhookId">Webhook ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteWebhookAsync(string webhookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests a webhook endpoint.
    /// </summary>
    /// <param name="webhookId">Webhook ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Test result.</returns>
    Task<WebhookTestResult> TestWebhookAsync(string webhookId, CancellationToken cancellationToken = default);

    #endregion

    #region Workflow Operations (n8n/Zapier specific)

    /// <summary>
    /// Triggers a workflow/zap by name or ID.
    /// </summary>
    /// <param name="workflowId">Workflow identifier.</param>
    /// <param name="payload">Data payload for the workflow.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Trigger result.</returns>
    Task<WorkflowTriggerResult> TriggerWorkflowAsync(string workflowId, object payload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available workflows.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of workflows.</returns>
    Task<IEnumerable<WorkflowInfo>> GetWorkflowsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets workflow execution history.
    /// </summary>
    /// <param name="workflowId">Workflow ID.</param>
    /// <param name="limit">Maximum executions to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Execution history.</returns>
    Task<IEnumerable<WorkflowExecution>> GetWorkflowExecutionsAsync(string workflowId, int limit = 10, CancellationToken cancellationToken = default);

    #endregion

    #region Connection Management

    /// <summary>
    /// Gets connected applications/services.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Connected apps.</returns>
    Task<IEnumerable<ConnectedApp>> GetConnectedAppsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests a connection to an external app.
    /// </summary>
    /// <param name="connectionId">Connection ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Connection test result.</returns>
    Task<ConnectionTestResult> TestConnectionAsync(string connectionId, CancellationToken cancellationToken = default);

    #endregion

    #region Incoming Webhook Processing

    /// <summary>
    /// Processes an incoming webhook from the integration platform.
    /// </summary>
    /// <param name="eventType">Event type.</param>
    /// <param name="payload">Webhook payload.</param>
    /// <param name="headers">Request headers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Processing result.</returns>
    Task<IntegrationWebhookResult> ProcessIncomingWebhookAsync(string eventType, string payload, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    #endregion

    /// <summary>
    /// Gets the health status of the integration provider.
    /// </summary>
    Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default);
}

#endregion

#region Integration DTOs

/// <summary>
/// A CRM event to publish.
/// </summary>
public class CrmEvent
{
    /// <summary>
    /// Unique event ID.
    /// </summary>
    public string EventId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Event type (e.g., account.created, opportunity.won).
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Entity type involved.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Entity ID.
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// Event timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who triggered the event.
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Event payload data.
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }

    /// <summary>
    /// Previous values (for update events).
    /// </summary>
    public Dictionary<string, object>? PreviousData { get; set; }

    /// <summary>
    /// Custom metadata.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Result of event publication.
/// </summary>
public class EventPublishResult
{
    public bool Success { get; set; }
    public string EventId { get; set; } = string.Empty;
    public string? MessageId { get; set; }
    public int? WebhooksTriggered { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Result of batch event publication.
/// </summary>
public class BatchEventPublishResult
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<EventPublishResult> Results { get; set; } = new();
}

/// <summary>
/// Webhook registration request.
/// </summary>
public class WebhookRegistration
{
    /// <summary>
    /// Webhook name/description.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Target URL for the webhook.
    /// </summary>
    public string TargetUrl { get; set; } = string.Empty;

    /// <summary>
    /// Events to subscribe to.
    /// </summary>
    public List<string> EventTypes { get; set; } = new();

    /// <summary>
    /// Secret for signature verification.
    /// </summary>
    public string? Secret { get; set; }

    /// <summary>
    /// Custom headers to include.
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Whether the webhook is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Filter conditions for events.
    /// </summary>
    public Dictionary<string, string>? Filters { get; set; }
}

/// <summary>
/// Webhook information.
/// </summary>
public class WebhookInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public List<string> EventTypes { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
    public int? TotalDeliveries { get; set; }
    public int? FailedDeliveries { get; set; }
}

/// <summary>
/// Webhook test result.
/// </summary>
public class WebhookTestResult
{
    public bool Success { get; set; }
    public int? StatusCode { get; set; }
    public long? ResponseTimeMs { get; set; }
    public string? Response { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Workflow information (n8n/Zapier).
/// </summary>
public class WorkflowInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string? TriggerType { get; set; } // webhook, schedule, manual
    public DateTime? LastExecutedAt { get; set; }
    public int? ExecutionCount { get; set; }
}

/// <summary>
/// Result of triggering a workflow.
/// </summary>
public class WorkflowTriggerResult
{
    public bool Success { get; set; }
    public string? ExecutionId { get; set; }
    public string WorkflowId { get; set; } = string.Empty;
    public string? Status { get; set; }
    public object? Output { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Workflow execution record.
/// </summary>
public class WorkflowExecution
{
    public string ExecutionId { get; set; } = string.Empty;
    public string WorkflowId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // running, success, failed
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public long? DurationMs { get; set; }
    public string? Error { get; set; }
    public object? Input { get; set; }
    public object? Output { get; set; }
}

/// <summary>
/// Connected application/service.
/// </summary>
public class ConnectedApp
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // slack, salesforce, hubspot, etc.
    public bool IsConnected { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public string? Status { get; set; }
}

/// <summary>
/// Connection test result.
/// </summary>
public class ConnectionTestResult
{
    public bool Success { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
    public string? Message { get; set; }
    public long? ResponseTimeMs { get; set; }
}

/// <summary>
/// Result of processing an incoming webhook from integration platform.
/// </summary>
public class IntegrationWebhookResult
{
    public bool Success { get; set; }
    public string? EventType { get; set; }
    public string? Action { get; set; } // create, update, delete
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public Dictionary<string, object>? ProcessedData { get; set; }
    public string? Error { get; set; }
}

#endregion

#region Event Type Constants

/// <summary>
/// Standard CRM event types for integration triggers.
/// </summary>
public static class CrmEventTypes
{
    // Account Events
    public const string AccountCreated = "account.created";
    public const string AccountUpdated = "account.updated";
    public const string AccountDeleted = "account.deleted";
    public const string AccountMerged = "account.merged";

    // Contact Events
    public const string ContactCreated = "contact.created";
    public const string ContactUpdated = "contact.updated";
    public const string ContactDeleted = "contact.deleted";

    // Opportunity Events
    public const string OpportunityCreated = "opportunity.created";
    public const string OpportunityUpdated = "opportunity.updated";
    public const string OpportunityStageChanged = "opportunity.stage_changed";
    public const string OpportunityWon = "opportunity.won";
    public const string OpportunityLost = "opportunity.lost";
    public const string OpportunityClosed = "opportunity.closed";

    // Lead Events
    public const string LeadCreated = "lead.created";
    public const string LeadConverted = "lead.converted";
    public const string LeadQualified = "lead.qualified";
    public const string LeadDisqualified = "lead.disqualified";

    // Activity Events
    public const string ActivityCreated = "activity.created";
    public const string ActivityCompleted = "activity.completed";
    public const string TaskCreated = "task.created";
    public const string TaskCompleted = "task.completed";

    // Quote/Contract Events
    public const string QuoteCreated = "quote.created";
    public const string QuoteSent = "quote.sent";
    public const string QuoteAccepted = "quote.accepted";
    public const string ContractSigned = "contract.signed";

    // Case/Ticket Events
    public const string CaseCreated = "case.created";
    public const string CaseUpdated = "case.updated";
    public const string CaseResolved = "case.resolved";
    public const string CaseEscalated = "case.escalated";

    // Communication Events
    public const string EmailSent = "email.sent";
    public const string EmailReceived = "email.received";
    public const string CallCompleted = "call.completed";
    public const string MeetingScheduled = "meeting.scheduled";
}

#endregion
