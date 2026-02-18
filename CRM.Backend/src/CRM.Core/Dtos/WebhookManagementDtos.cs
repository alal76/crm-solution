// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for webhook registration response.
/// </summary>
public class WebhookDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Secret { get; set; }
    public bool IsActive { get; set; }
    public List<string> EventTypes { get; set; } = new();
    public string? FilterCriteria { get; set; }
    public int MaxRetries { get; set; }
    public int RetryIntervalSeconds { get; set; }
    public int TimeoutSeconds { get; set; }
    public int FailureCount { get; set; }
    public string? DisabledReason { get; set; }
    public DateTime? DisabledAt { get; set; }
    public DateTime? LastDeliveryAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating webhook registration.
/// </summary>
public class CreateWebhookDto
{
    [Required(ErrorMessage = "Webhook URL is required")]
    [Url(ErrorMessage = "Webhook URL must be a valid HTTPS URL")]
    [StringLength(2048)]
    public string Url { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(256)]
    public string? Secret { get; set; }

    [Required(ErrorMessage = "At least one event type is required")]
    public List<string> EventTypes { get; set; } = new();

    [StringLength(2000)]
    public string? FilterCriteria { get; set; }

    [Range(0, 10)]
    public int MaxRetries { get; set; } = 5;

    [Range(60, 3600)]
    public int RetryIntervalSeconds { get; set; } = 300;

    [Range(5, 60)]
    public int TimeoutSeconds { get; set; } = 30;

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for updating webhook registration.
/// </summary>
public class UpdateWebhookDto
{
    [Url(ErrorMessage = "Webhook URL must be a valid HTTPS URL")]
    [StringLength(2048)]
    public string? Url { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(256)]
    public string? Secret { get; set; }

    public List<string>? EventTypes { get; set; }

    [StringLength(2000)]
    public string? FilterCriteria { get; set; }

    [Range(0, 10)]
    public int? MaxRetries { get; set; }

    [Range(60, 3600)]
    public int? RetryIntervalSeconds { get; set; }

    [Range(5, 60)]
    public int? TimeoutSeconds { get; set; }

    public bool? IsActive { get; set; }
}

/// <summary>
/// DTO for webhook delivery record.
/// </summary>
public class WebhookDeliveryDto
{
    public int Id { get; set; }
    public int WebhookId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime TriggeredAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int AttemptNumber { get; set; }
    public int? ResponseStatusCode { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public double? DurationMs { get; set; }
    public string? RequestPayload { get; set; }
    public string? ResponsePayload { get; set; }
}

/// <summary>
/// DTO for webhook event definition.
/// </summary>
public class WebhookEventDto
{
    public int Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string? PayloadSchema { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for webhook test payload.
/// </summary>
public class WebhookTestDto
{
    [Required(ErrorMessage = "Event type is required")]
    [StringLength(100)]
    public string EventType { get; set; } = string.Empty;

    public Dictionary<string, object>? Payload { get; set; }
}

/// <summary>
/// DTO for webhook test result.
/// </summary>
public class WebhookTestResultDto
{
    public int WebhookId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int? ResponseStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public string? ErrorMessage { get; set; }
    public double DurationMs { get; set; }
    public DateTime TestedAt { get; set; }
}

/// <summary>
/// DTO for webhook statistics.
/// </summary>
public class WebhookStatisticsDto
{
    public int WebhookId { get; set; }
    public string Url { get; set; } = string.Empty;
    public int TotalDeliveries { get; set; }
    public int SuccessfulDeliveries { get; set; }
    public int FailedDeliveries { get; set; }
    public int PendingDeliveries { get; set; }
    public double SuccessRate { get; set; }
    public double AverageDurationMs { get; set; }
    public int ConsecutiveFailures { get; set; }
    public DateTime? LastSuccessfulDelivery { get; set; }
    public DateTime? LastFailedDelivery { get; set; }
    public Dictionary<int, int> ResponseCodeDistribution { get; set; } = new();
}

/// <summary>
/// DTO for webhook retry logistics.
/// </summary>
public class WebhookRetryDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int DeliveryId { get; set; }

    [Range(0, 10)]
    public int MaxRetries { get; set; } = 5;

    [Range(60, 3600)]
    public int RetryIntervalSeconds { get; set; } = 300;
}

/// <summary>
/// DTO for webhook delivery history.
/// </summary>
public class WebhookDeliveryHistoryDto
{
    public int WebhookId { get; set; }
    public string Url { get; set; } = string.Empty;
    public int TotalDeliveries { get; set; }
    public int TotalCount { get; set; }
    public List<WebhookDeliveryDto> RecentDeliveries { get; set; } = new();
    public List<WebhookDeliveryDto> Deliveries { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// DTO for webhook list response.
/// </summary>
public class WebhookListDto
{
    public List<WebhookDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
