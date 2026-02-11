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

namespace CRM.Core.DTOs.ITSM;

/// <summary>
/// DTO for webhook subscription.
/// </summary>
public class WebhookSubscriptionDto
{
    public int WebhookSubscriptionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public string? Secret { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> EventTypes { get; set; } = new();
    public Dictionary<string, string> Headers { get; set; } = new();
    public int RetryCount { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}

/// <summary>
/// DTO for creating a webhook subscription.
/// </summary>
public class CreateWebhookSubscriptionDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public string? Secret { get; set; }
    public List<string> EventTypes { get; set; } = new();
    public Dictionary<string, string>? Headers { get; set; }
    public int RetryCount { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// DTO for updating a webhook subscription.
/// </summary>
public class UpdateWebhookSubscriptionDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? TargetUrl { get; set; }
    public string? Secret { get; set; }
    public bool? IsActive { get; set; }
    public List<string>? EventTypes { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public int? RetryCount { get; set; }
    public int? TimeoutSeconds { get; set; }
}

/// <summary>
/// DTO for webhook delivery record.
/// </summary>
public class WebhookDeliveryDto
{
    public int WebhookDeliveryId { get; set; }
    public int WebhookSubscriptionId { get; set; }
    public string SubscriptionName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public string? RequestBody { get; set; }
    public int? ResponseStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int AttemptNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public double? DurationMs { get; set; }
}

/// <summary>
/// Standard webhook payload wrapper.
/// </summary>
public class WebhookPayload<T>
{
    public string EventType { get; set; } = string.Empty;
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = "CRM-ITSM";
    public T? Data { get; set; }
}

/// <summary>
/// Webhook event for incident operations.
/// </summary>
public class IncidentWebhookData
{
    public int IncidentId { get; set; }
    public string IncidentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string State { get; set; } = string.Empty;
    public int? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? PreviousState { get; set; }
}

/// <summary>
/// Webhook event for SLA operations.
/// </summary>
public class SLAWebhookData
{
    public int SLAInstanceId { get; set; }
    public int TargetId { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public string TargetNumber { get; set; } = string.Empty;
    public string PolicyName { get; set; } = string.Empty;
    public int Priority { get; set; }
    public DateTime? ResponseDueAt { get; set; }
    public DateTime? ResolutionDueAt { get; set; }
    public double? PercentConsumed { get; set; }
    public bool IsBreached { get; set; }
    public string BreachType { get; set; } = string.Empty;
}

/// <summary>
/// Webhook event for change operations.
/// </summary>
public class ChangeWebhookData
{
    public int ChangeId { get; set; }
    public string ChangeNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int? RequestedById { get; set; }
    public string? RequestedByName { get; set; }
    public DateTime? ScheduledStart { get; set; }
    public DateTime? ScheduledEnd { get; set; }
    public string? ApprovalStatus { get; set; }
    public string? PreviousState { get; set; }
}
