// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CRM.Core.Ports.Output.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Providers.Integration;

/// <summary>
/// Integration provider for Zapier automation platform.
/// Implements IIntegrationPort using Zapier's webhook and API integration.
/// </summary>
public class ZapierProvider : IIntegrationPort
{
    private readonly HttpClient _httpClient;
    private readonly ZapierConfiguration _config;
    private readonly ILogger<ZapierProvider> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ZapierProvider(
        HttpClient httpClient,
        IOptions<ZapierConfiguration> config,
        ILogger<ZapierProvider> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }

    public string ProviderName => "Zapier";

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        // Zapier is a SaaS platform - check if we have valid configuration
        var hasConfig = !string.IsNullOrEmpty(_config.WebhookBaseUrl) ||
                       _config.EventWebhooks?.Any() == true;
        return Task.FromResult(hasConfig);
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
            // Zapier receives events via catch hook webhooks
            var webhookUrl = GetWebhookUrlForEvent(crmEvent.EventType);

            if (string.IsNullOrEmpty(webhookUrl))
            {
                result.Success = true;
                result.WebhooksTriggered = 0;
                _logger.LogDebug("No Zapier webhook configured for event type {EventType}", crmEvent.EventType);
                return result;
            }

            var payload = new
            {
                @event = crmEvent.EventType,
                event_id = crmEvent.EventId,
                timestamp = crmEvent.Timestamp.ToString("O"),
                entity_type = crmEvent.EntityType,
                entity_id = crmEvent.EntityId,
                user_id = crmEvent.UserId,
                data = crmEvent.Data,
                previous_data = crmEvent.PreviousData,
                metadata = crmEvent.Metadata
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(webhookUrl, content, cancellationToken);

            result.Success = response.IsSuccessStatusCode;
            result.WebhooksTriggered = result.Success ? 1 : 0;

            if (result.Success)
            {
                // Zapier returns a request ID
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!string.IsNullOrEmpty(responseBody))
                {
                    try
                    {
                        var zapierResponse = JsonSerializer.Deserialize<ZapierWebhookResponse>(responseBody, _jsonOptions);
                        result.MessageId = zapierResponse?.Id;
                    }
                    catch { /* Ignore parsing errors */ }
                }
            }
            else
            {
                result.Error = $"Zapier returned {response.StatusCode}";
            }

            _logger.LogInformation("Published event {EventType} to Zapier: {Success}",
                crmEvent.EventType, result.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event {EventId} to Zapier", crmEvent.EventId);
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

    private string? GetWebhookUrlForEvent(string eventType)
    {
        if (_config.EventWebhooks?.TryGetValue(eventType, out var url) == true)
        {
            return url;
        }

        // Check for wildcard mappings
        var prefix = eventType.Split('.')[0] + ".*";
        if (_config.EventWebhooks?.TryGetValue(prefix, out var wildcardUrl) == true)
        {
            return wildcardUrl;
        }

        // Default webhook
        if (_config.EventWebhooks?.TryGetValue("*", out var defaultUrl) == true)
        {
            return defaultUrl;
        }

        // Use base URL if configured
        if (!string.IsNullOrEmpty(_config.WebhookBaseUrl))
        {
            return _config.WebhookBaseUrl;
        }

        return null;
    }

    #endregion

    #region Webhook Management

    public Task<WebhookInfo> RegisterWebhookAsync(WebhookRegistration registration, CancellationToken cancellationToken = default)
    {
        // Zapier manages webhooks through its UI - we log the request
        _logger.LogInformation(
            "Zapier webhook registration request. Configure a Catch Hook trigger in Zapier pointing to: {Url}",
            registration.TargetUrl);

        var info = new WebhookInfo
        {
            Id = Guid.NewGuid().ToString(),
            Name = registration.Name,
            TargetUrl = registration.TargetUrl,
            EventTypes = registration.EventTypes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return Task.FromResult(info);
    }

    public Task<IEnumerable<WebhookInfo>> GetWebhooksAsync(string? eventType = null, CancellationToken cancellationToken = default)
    {
        // Return configured webhooks from config
        var webhooks = _config.EventWebhooks?.Select((kvp, index) => new WebhookInfo
        {
            Id = $"zapier_hook_{index}",
            Name = $"Zapier Hook - {kvp.Key}",
            TargetUrl = kvp.Value,
            EventTypes = kvp.Key == "*" ? new List<string> { "all" } : new List<string> { kvp.Key },
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        }) ?? Enumerable.Empty<WebhookInfo>();

        if (!string.IsNullOrEmpty(eventType))
        {
            webhooks = webhooks.Where(w => w.EventTypes.Contains(eventType) || w.EventTypes.Contains("all"));
        }

        return Task.FromResult(webhooks);
    }

    public Task UpdateWebhookAsync(string webhookId, WebhookRegistration update, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Zapier webhooks must be configured in Zapier.com and updated in appsettings.json");
        return Task.CompletedTask;
    }

    public Task DeleteWebhookAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Zapier webhooks must be deleted in Zapier.com and removed from appsettings.json");
        return Task.CompletedTask;
    }

    public async Task<WebhookTestResult> TestWebhookAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        var result = new WebhookTestResult();
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var webhooks = await GetWebhooksAsync(cancellationToken: cancellationToken);
            var webhook = webhooks.FirstOrDefault(w => w.Id == webhookId);

            if (webhook == null)
            {
                result.Success = false;
                result.Error = "Webhook not found";
                return result;
            }

            var testPayload = new
            {
                @event = "webhook.test",
                timestamp = DateTime.UtcNow.ToString("O"),
                test = true,
                message = "CRM webhook test"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(testPayload, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(webhook.TargetUrl, content, cancellationToken);
            sw.Stop();

            result.Success = response.IsSuccessStatusCode;
            result.StatusCode = (int)response.StatusCode;
            result.ResponseTimeMs = sw.ElapsedMilliseconds;
            result.Response = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!result.Success)
            {
                result.Error = $"Zapier returned status {response.StatusCode}";
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
        // Zapier doesn't support direct workflow triggering - use webhooks instead
        _logger.LogWarning("Zapier workflows (Zaps) cannot be triggered directly. Use event publishing to trigger Catch Hook Zaps.");

        return Task.FromResult(new WorkflowTriggerResult
        {
            Success = false,
            WorkflowId = workflowId,
            Error = "Zapier Zaps cannot be triggered directly via API. Publish events to trigger Catch Hook Zaps."
        });
    }

    public Task<IEnumerable<WorkflowInfo>> GetWorkflowsAsync(CancellationToken cancellationToken = default)
    {
        // Zapier doesn't expose Zaps via API in the standard way
        // Return info about configured webhook integrations
        var workflows = _config.EventWebhooks?.Select((kvp, index) => new WorkflowInfo
        {
            Id = $"zap_{index}",
            Name = $"Zap for {kvp.Key}",
            Description = $"Zapier integration triggered by {kvp.Key} events",
            IsActive = true,
            TriggerType = "webhook"
        }) ?? Enumerable.Empty<WorkflowInfo>();

        return Task.FromResult(workflows);
    }

    public Task<IEnumerable<WorkflowExecution>> GetWorkflowExecutionsAsync(string workflowId, int limit = 10, CancellationToken cancellationToken = default)
    {
        // Zapier doesn't expose execution history via API
        _logger.LogInformation("Zapier execution history is available in the Zapier dashboard");
        return Task.FromResult<IEnumerable<WorkflowExecution>>(Array.Empty<WorkflowExecution>());
    }

    #endregion

    #region Connection Management

    public Task<IEnumerable<ConnectedApp>> GetConnectedAppsAsync(CancellationToken cancellationToken = default)
    {
        // Return Zapier as the connected app
        var apps = new List<ConnectedApp>
        {
            new()
            {
                Id = "zapier",
                Name = "Zapier",
                Type = "automation",
                IsConnected = !string.IsNullOrEmpty(_config.WebhookBaseUrl) || _config.EventWebhooks?.Any() == true,
                Status = "Active"
            }
        };

        return Task.FromResult<IEnumerable<ConnectedApp>>(apps);
    }

    public async Task<ConnectionTestResult> TestConnectionAsync(string connectionId, CancellationToken cancellationToken = default)
    {
        var result = new ConnectionTestResult
        {
            ConnectionId = connectionId
        };

        // Test by sending a test webhook
        var webhooks = await GetWebhooksAsync(cancellationToken: cancellationToken);
        var firstWebhook = webhooks.FirstOrDefault();

        if (firstWebhook == null)
        {
            result.Success = false;
            result.Message = "No webhooks configured";
            return result;
        }

        var testResult = await TestWebhookAsync(firstWebhook.Id, cancellationToken);
        result.Success = testResult.Success;
        result.Message = testResult.Success ? "Connection successful" : testResult.Error;
        result.ResponseTimeMs = testResult.ResponseTimeMs;

        return result;
    }

    #endregion

    #region Incoming Webhook Processing

    public Task<IntegrationWebhookResult> ProcessIncomingWebhookAsync(string eventType, string payload, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(payload, _jsonOptions);

            _logger.LogInformation("Received Zapier action webhook: {EventType}", eventType);

            // Zapier sends actions back to perform CRM operations
            return Task.FromResult(new IntegrationWebhookResult
            {
                Success = true,
                EventType = eventType,
                Action = DetermineAction(eventType, data),
                ProcessedData = data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Zapier webhook");
            return Task.FromResult(new IntegrationWebhookResult
            {
                Success = false,
                Error = ex.Message
            });
        }
    }

    private static string DetermineAction(string eventType, Dictionary<string, object>? data)
    {
        if (eventType.Contains("create", StringComparison.OrdinalIgnoreCase))
            return "create";
        if (eventType.Contains("update", StringComparison.OrdinalIgnoreCase))
            return "update";
        if (eventType.Contains("delete", StringComparison.OrdinalIgnoreCase))
            return "delete";

        return data?.ContainsKey("action") == true
            ? data["action"]?.ToString() ?? "unknown"
            : "action";
    }

    #endregion

    public Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var isConfigured = !string.IsNullOrEmpty(_config.WebhookBaseUrl) || _config.EventWebhooks?.Any() == true;
        var webhookCount = _config.EventWebhooks?.Count ?? 0;

        return Task.FromResult(new ProviderHealthResult
        {
            IsHealthy = isConfigured,
            ProviderName = ProviderName,
            Message = isConfigured
                ? $"Zapier integration is configured with {webhookCount} webhook mappings."
                : "Zapier integration is not configured. Add webhook URLs to configuration.",
            Details = new Dictionary<string, object>
            {
                { "configured", isConfigured },
                { "webhookCount", webhookCount },
                { "webhookBaseUrl", _config.WebhookBaseUrl ?? "not set" }
            }
        });
    }
}

internal class ZapierWebhookResponse
{
    public string? Id { get; set; }
    public string? Status { get; set; }
}

/// <summary>
/// Configuration for Zapier provider.
/// </summary>
public class ZapierConfiguration
{
    /// <summary>
    /// Base URL for Zapier catch hooks (e.g., https://hooks.zapier.com/hooks/catch/123456).
    /// </summary>
    public string? WebhookBaseUrl { get; set; }

    /// <summary>
    /// Mapping of CRM event types to specific Zapier webhook URLs.
    /// Keys can be specific events (account.created), wildcards (account.*), or default (*).
    /// </summary>
    public Dictionary<string, string>? EventWebhooks { get; set; }

    /// <summary>
    /// Optional API key for Zapier (not commonly used for webhooks).
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Timeout for webhook calls in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    public (bool IsValid, string? Error) Validate()
    {
        if (string.IsNullOrEmpty(WebhookBaseUrl) && (EventWebhooks == null || !EventWebhooks.Any()))
        {
            return (false, "Either WebhookBaseUrl or EventWebhooks must be configured");
        }

        return (true, null);
    }
}
