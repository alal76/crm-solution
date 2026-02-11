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

using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CRM.Core.Ports.Output.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Providers.Integration;

/// <summary>
/// BuiltIn integration provider that implements webhook-based event distribution.
/// Uses in-memory webhook registry and HTTP client for delivery.
/// Suitable for simple deployments without external integration platforms.
/// </summary>
public class BuiltInIntegrationProvider : IIntegrationPort
{
    private readonly HttpClient _httpClient;
    private readonly BuiltInIntegrationConfiguration _config;
    private readonly ILogger<BuiltInIntegrationProvider> _logger;

    // In-memory stores (production would use database)
    private readonly ConcurrentDictionary<string, WebhookInfo> _webhooks = new();
    private readonly ConcurrentDictionary<string, List<WorkflowExecution>> _executionHistory = new();
    private static int _webhookCounter = 0;
    private static int _executionCounter = 0;

    public BuiltInIntegrationProvider(
        HttpClient httpClient,
        IOptions<BuiltInIntegrationConfiguration> config,
        ILogger<BuiltInIntegrationProvider> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }

    public string ProviderName => "BuiltIn";

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        // BuiltIn provider is always available
        return Task.FromResult(true);
    }

    #region Event Publishing

    public async Task<EventPublishResult> PublishEventAsync(CrmEvent crmEvent, CancellationToken cancellationToken = default)
    {
        var result = new EventPublishResult
        {
            EventId = crmEvent.EventId
        };

        try
        {
            // Find all webhooks subscribed to this event type
            var matchingWebhooks = _webhooks.Values
                .Where(w => w.IsActive && w.EventTypes.Contains(crmEvent.EventType))
                .ToList();

            if (!matchingWebhooks.Any())
            {
                result.Success = true;
                result.WebhooksTriggered = 0;
                _logger.LogDebug("No webhooks registered for event type {EventType}", crmEvent.EventType);
                return result;
            }

            var successCount = 0;
            foreach (var webhook in matchingWebhooks)
            {
                var delivered = await DeliverWebhookAsync(webhook, crmEvent, cancellationToken);
                if (delivered)
                {
                    successCount++;
                    UpdateWebhookStats(webhook.Id, true);
                }
                else
                {
                    UpdateWebhookStats(webhook.Id, false);
                }
            }

            result.Success = successCount > 0;
            result.WebhooksTriggered = successCount;

            _logger.LogInformation(
                "Published event {EventType} to {Count}/{Total} webhooks",
                crmEvent.EventType, successCount, matchingWebhooks.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event {EventId}", crmEvent.EventId);
            result.Success = false;
            result.Error = ex.Message;
        }

        return result;
    }

    public async Task<BatchEventPublishResult> PublishEventsAsync(IEnumerable<CrmEvent> events, CancellationToken cancellationToken = default)
    {
        var eventList = events.ToList();
        var result = new BatchEventPublishResult
        {
            TotalCount = eventList.Count
        };

        foreach (var evt in eventList)
        {
            var publishResult = await PublishEventAsync(evt, cancellationToken);
            result.Results.Add(publishResult);

            if (publishResult.Success)
                result.SuccessCount++;
            else
                result.FailureCount++;
        }

        return result;
    }

    private async Task<bool> DeliverWebhookAsync(WebhookInfo webhook, CrmEvent crmEvent, CancellationToken cancellationToken)
    {
        try
        {
            var payload = new
            {
                @event = crmEvent.EventType,
                timestamp = crmEvent.Timestamp,
                eventId = crmEvent.EventId,
                entity = new
                {
                    type = crmEvent.EntityType,
                    id = crmEvent.EntityId
                },
                userId = crmEvent.UserId,
                data = crmEvent.Data,
                previousData = crmEvent.PreviousData,
                metadata = crmEvent.Metadata
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                Encoding.UTF8,
                "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, webhook.TargetUrl);
            request.Content = content;

            // Add standard headers
            request.Headers.Add("X-Webhook-Event", crmEvent.EventType);
            request.Headers.Add("X-Webhook-Delivery", Guid.NewGuid().ToString());
            request.Headers.Add("X-CRM-Event-Id", crmEvent.EventId);

            // Add signature if secret is configured
            // Note: In production, retrieve secret from secure storage
            if (!string.IsNullOrEmpty(_config.DefaultWebhookSecret))
            {
                var signature = ComputeSignature(await content.ReadAsStringAsync(cancellationToken), _config.DefaultWebhookSecret);
                request.Headers.Add("X-Webhook-Signature", $"sha256={signature}");
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_config.WebhookTimeoutSeconds));

            var response = await _httpClient.SendAsync(request, cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deliver webhook to {Url}", webhook.TargetUrl);
            return false;
        }
    }

    private static string ComputeSignature(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLower();
    }

    private void UpdateWebhookStats(string webhookId, bool success)
    {
        if (_webhooks.TryGetValue(webhookId, out var webhook))
        {
            webhook.LastTriggeredAt = DateTime.UtcNow;
            webhook.TotalDeliveries = (webhook.TotalDeliveries ?? 0) + 1;
            if (!success)
            {
                webhook.FailedDeliveries = (webhook.FailedDeliveries ?? 0) + 1;
            }
        }
    }

    #endregion

    #region Webhook Management

    public Task<WebhookInfo> RegisterWebhookAsync(WebhookRegistration registration, CancellationToken cancellationToken = default)
    {
        var id = $"webhook_{Interlocked.Increment(ref _webhookCounter)}";

        var webhook = new WebhookInfo
        {
            Id = id,
            Name = registration.Name,
            TargetUrl = registration.TargetUrl,
            EventTypes = registration.EventTypes,
            IsActive = registration.IsActive,
            CreatedAt = DateTime.UtcNow,
            TotalDeliveries = 0,
            FailedDeliveries = 0
        };

        _webhooks[id] = webhook;

        _logger.LogInformation("Registered webhook {WebhookId} for events: {Events}", id, string.Join(", ", registration.EventTypes));

        return Task.FromResult(webhook);
    }

    public Task<IEnumerable<WebhookInfo>> GetWebhooksAsync(string? eventType = null, CancellationToken cancellationToken = default)
    {
        var webhooks = _webhooks.Values.AsEnumerable();

        if (!string.IsNullOrEmpty(eventType))
        {
            webhooks = webhooks.Where(w => w.EventTypes.Contains(eventType));
        }

        return Task.FromResult(webhooks);
    }

    public Task UpdateWebhookAsync(string webhookId, WebhookRegistration update, CancellationToken cancellationToken = default)
    {
        if (_webhooks.TryGetValue(webhookId, out var existing))
        {
            existing.Name = update.Name;
            existing.TargetUrl = update.TargetUrl;
            existing.EventTypes = update.EventTypes;
            existing.IsActive = update.IsActive;

            _logger.LogInformation("Updated webhook {WebhookId}", webhookId);
        }
        else
        {
            _logger.LogWarning("Webhook {WebhookId} not found for update", webhookId);
        }

        return Task.CompletedTask;
    }

    public Task DeleteWebhookAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        if (_webhooks.TryRemove(webhookId, out _))
        {
            _logger.LogInformation("Deleted webhook {WebhookId}", webhookId);
        }

        return Task.CompletedTask;
    }

    public async Task<WebhookTestResult> TestWebhookAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        var result = new WebhookTestResult();
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            if (!_webhooks.TryGetValue(webhookId, out var webhook))
            {
                result.Success = false;
                result.Error = "Webhook not found";
                return result;
            }

            var testEvent = new CrmEvent
            {
                EventId = Guid.NewGuid().ToString(),
                EventType = "webhook.test",
                EntityType = "test",
                EntityId = 0,
                Timestamp = DateTime.UtcNow,
                Data = new Dictionary<string, object> { { "test", true } }
            };

            var delivered = await DeliverWebhookAsync(webhook, testEvent, cancellationToken);
            sw.Stop();

            result.Success = delivered;
            result.ResponseTimeMs = sw.ElapsedMilliseconds;

            if (!delivered)
            {
                result.Error = "Delivery failed";
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            result.Success = false;
            result.ResponseTimeMs = sw.ElapsedMilliseconds;
            result.Error = ex.Message;
        }

        return result;
    }

    #endregion

    #region Workflow Operations

    public Task<WorkflowTriggerResult> TriggerWorkflowAsync(string workflowId, object payload, CancellationToken cancellationToken = default)
    {
        // BuiltIn provider doesn't support external workflows
        // Return a simulated execution for testing
        var executionId = $"exec_{Interlocked.Increment(ref _executionCounter)}";

        var execution = new WorkflowExecution
        {
            ExecutionId = executionId,
            WorkflowId = workflowId,
            Status = "success",
            StartedAt = DateTime.UtcNow,
            FinishedAt = DateTime.UtcNow,
            DurationMs = 0,
            Input = payload,
            Output = new { message = "BuiltIn provider does not support external workflows. Use n8n or Zapier provider." }
        };

        // Store execution history
        if (!_executionHistory.ContainsKey(workflowId))
        {
            _executionHistory[workflowId] = new List<WorkflowExecution>();
        }
        _executionHistory[workflowId].Add(execution);

        _logger.LogInformation("Simulated workflow trigger for {WorkflowId}", workflowId);

        return Task.FromResult(new WorkflowTriggerResult
        {
            Success = true,
            ExecutionId = executionId,
            WorkflowId = workflowId,
            Status = "success",
            Output = execution.Output
        });
    }

    public Task<IEnumerable<WorkflowInfo>> GetWorkflowsAsync(CancellationToken cancellationToken = default)
    {
        // BuiltIn provider doesn't have external workflows
        // Return placeholder workflow info
        var workflows = new List<WorkflowInfo>
        {
            new()
            {
                Id = "builtin_webhook_dispatcher",
                Name = "BuiltIn Webhook Dispatcher",
                Description = "Automatically dispatches CRM events to registered webhooks",
                IsActive = true,
                TriggerType = "event",
                ExecutionCount = _webhooks.Values.Sum(w => w.TotalDeliveries ?? 0)
            }
        };

        return Task.FromResult<IEnumerable<WorkflowInfo>>(workflows);
    }

    public Task<IEnumerable<WorkflowExecution>> GetWorkflowExecutionsAsync(string workflowId, int limit = 10, CancellationToken cancellationToken = default)
    {
        if (_executionHistory.TryGetValue(workflowId, out var executions))
        {
            return Task.FromResult<IEnumerable<WorkflowExecution>>(
                executions.OrderByDescending(e => e.StartedAt).Take(limit));
        }

        return Task.FromResult<IEnumerable<WorkflowExecution>>(Array.Empty<WorkflowExecution>());
    }

    #endregion

    #region Connection Management

    public Task<IEnumerable<ConnectedApp>> GetConnectedAppsAsync(CancellationToken cancellationToken = default)
    {
        // BuiltIn provider tracks webhooks as "connected apps"
        var apps = _webhooks.Values.Select(w => new ConnectedApp
        {
            Id = w.Id,
            Name = w.Name,
            Type = "webhook",
            IsConnected = w.IsActive,
            ConnectedAt = w.CreatedAt,
            Status = w.IsActive ? "Active" : "Inactive"
        }).ToList();

        return Task.FromResult<IEnumerable<ConnectedApp>>(apps);
    }

    public async Task<ConnectionTestResult> TestConnectionAsync(string connectionId, CancellationToken cancellationToken = default)
    {
        // For BuiltIn, testing a connection means testing the webhook
        var testResult = await TestWebhookAsync(connectionId, cancellationToken);

        return new ConnectionTestResult
        {
            Success = testResult.Success,
            ConnectionId = connectionId,
            Message = testResult.Success ? "Connection successful" : testResult.Error,
            ResponseTimeMs = testResult.ResponseTimeMs
        };
    }

    #endregion

    #region Incoming Webhook Processing

    public Task<IntegrationWebhookResult> ProcessIncomingWebhookAsync(string eventType, string payload, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Parse the incoming payload
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(payload);

            _logger.LogInformation("Processing incoming webhook: {EventType}", eventType);

            return Task.FromResult(new IntegrationWebhookResult
            {
                Success = true,
                EventType = eventType,
                Action = eventType.Contains(".created") ? "create"
                    : eventType.Contains(".updated") ? "update"
                    : eventType.Contains(".deleted") ? "delete"
                    : "unknown",
                ProcessedData = data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing incoming webhook");
            return Task.FromResult(new IntegrationWebhookResult
            {
                Success = false,
                Error = ex.Message
            });
        }
    }

    #endregion

    public Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var activeWebhooks = _webhooks.Values.Count(w => w.IsActive);
        var totalDeliveries = _webhooks.Values.Sum(w => w.TotalDeliveries ?? 0);
        var failedDeliveries = _webhooks.Values.Sum(w => w.FailedDeliveries ?? 0);

        return Task.FromResult(new ProviderHealthResult
        {
            IsHealthy = true,
            ProviderName = ProviderName,
            Message = $"BuiltIn integration provider is healthy. {activeWebhooks} active webhooks, {totalDeliveries} total deliveries, {failedDeliveries} failed.",
            Details = new Dictionary<string, object>
            {
                { "activeWebhooks", activeWebhooks },
                { "totalWebhooks", _webhooks.Count },
                { "totalDeliveries", totalDeliveries },
                { "failedDeliveries", failedDeliveries },
                { "successRate", totalDeliveries > 0 ? Math.Round((1 - (double)failedDeliveries / totalDeliveries) * 100, 2) : 100 }
            }
        });
    }
}

/// <summary>
/// Configuration for the BuiltIn integration provider.
/// </summary>
public class BuiltInIntegrationConfiguration
{
    /// <summary>
    /// Default webhook timeout in seconds.
    /// </summary>
    public int WebhookTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Default webhook secret for signature generation.
    /// </summary>
    public string? DefaultWebhookSecret { get; set; }

    /// <summary>
    /// Maximum retry attempts for failed deliveries.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Retry delay in seconds.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 5;

    /// <summary>
    /// Whether to log webhook payloads (for debugging).
    /// </summary>
    public bool LogPayloads { get; set; } = false;
}
