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
/// Integration provider for n8n workflow automation platform.
/// Implements IIntegrationPort using n8n's REST API.
/// </summary>
public class N8nProvider : IIntegrationPort
{
    private readonly HttpClient _httpClient;
    private readonly N8nConfiguration _config;
    private readonly ILogger<N8nProvider> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public N8nProvider(
        HttpClient httpClient,
        IOptions<N8nConfiguration> config,
        ILogger<N8nProvider> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;

        // Set base address and auth
        if (!string.IsNullOrEmpty(_config.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        }

        if (!string.IsNullOrEmpty(_config.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("X-N8N-API-KEY", _config.ApiKey);
        }
    }

    public string ProviderName => "n8n";

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/healthz", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
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
            // n8n receives events via webhook trigger nodes
            // We need to find workflows with CRM webhook triggers for this event type
            var webhookUrl = GetWebhookUrlForEvent(crmEvent.EventType);

            if (string.IsNullOrEmpty(webhookUrl))
            {
                result.Success = true;
                result.WebhooksTriggered = 0;
                _logger.LogDebug("No n8n webhook configured for event type {EventType}", crmEvent.EventType);
                return result;
            }

            var payload = new
            {
                @event = crmEvent.EventType,
                eventId = crmEvent.EventId,
                timestamp = crmEvent.Timestamp,
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
                JsonSerializer.Serialize(payload, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(webhookUrl, content, cancellationToken);

            result.Success = response.IsSuccessStatusCode;
            result.WebhooksTriggered = result.Success ? 1 : 0;

            if (!result.Success)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                result.Error = $"n8n returned {response.StatusCode}: {errorBody}";
            }

            _logger.LogInformation("Published event {EventType} to n8n: {Success}",
                crmEvent.EventType, result.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event {EventId} to n8n", crmEvent.EventId);
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
            {
                result.SuccessCount++;
            }
            else
            {
                result.FailureCount++;
            }
        }

        return result;
    }

    private string? GetWebhookUrlForEvent(string eventType)
    {
        // Map event types to n8n webhook URLs from configuration
        if (_config.EventWebhooks?.TryGetValue(eventType, out var url) == true)
        {
            return url;
        }

        // Check for wildcard mappings (e.g., "account.*")
        var prefix = eventType.Split('.')[0] + ".*";
        if (_config.EventWebhooks?.TryGetValue(prefix, out var wildcardUrl) == true)
        {
            return wildcardUrl;
        }

        // Check for default webhook
        if (_config.EventWebhooks?.TryGetValue("*", out var defaultUrl) == true)
        {
            return defaultUrl;
        }

        return null;
    }

    #endregion

    #region Webhook Management

    public Task<WebhookInfo> RegisterWebhookAsync(WebhookRegistration registration, CancellationToken cancellationToken = default)
    {
        // n8n manages webhooks through its UI - we just return info about the registration request
        var info = new WebhookInfo
        {
            Id = Guid.NewGuid().ToString(),
            Name = registration.Name,
            TargetUrl = registration.TargetUrl,
            EventTypes = registration.EventTypes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Webhook registration request logged for n8n. Configure webhook trigger in n8n UI with URL: {Url}", registration.TargetUrl);

        return Task.FromResult(info);
    }

    public async Task<IEnumerable<WebhookInfo>> GetWebhooksAsync(string? eventType = null, CancellationToken cancellationToken = default)
    {
        // n8n API doesn't directly expose webhooks, but we can list workflows that have webhook triggers
        var workflows = await GetWorkflowsAsync(cancellationToken);

        var webhooks = workflows
            .Where(w => w.TriggerType == "webhook")
            .Select(w => new WebhookInfo
            {
                Id = w.Id,
                Name = w.Name,
                TargetUrl = $"{_config.WebhookBaseUrl}/webhook/{w.Id}",
                EventTypes = new List<string> { "custom" }, // n8n webhooks are generic
                IsActive = w.IsActive,
                CreatedAt = DateTime.UtcNow.AddDays(-7) // Placeholder
            });

        if (!string.IsNullOrEmpty(eventType))
        {
            webhooks = webhooks.Where(w => w.EventTypes.Contains(eventType));
        }

        return webhooks;
    }

    public Task UpdateWebhookAsync(string webhookId, WebhookRegistration update, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("n8n webhooks must be updated through the n8n UI. Workflow ID: {WorkflowId}", webhookId);
        return Task.CompletedTask;
    }

    public Task DeleteWebhookAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("n8n webhooks must be deleted through the n8n UI. Workflow ID: {WorkflowId}", webhookId);
        return Task.CompletedTask;
    }

    public async Task<WebhookTestResult> TestWebhookAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        var result = new WebhookTestResult();
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var testPayload = new
            {
                @event = "webhook.test",
                timestamp = DateTime.UtcNow,
                test = true
            };

            var webhookUrl = $"{_config.WebhookBaseUrl}/webhook/{webhookId}";
            var content = new StringContent(
                JsonSerializer.Serialize(testPayload, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(webhookUrl, content, cancellationToken);
            sw.Stop();

            result.Success = response.IsSuccessStatusCode;
            result.StatusCode = (int)response.StatusCode;
            result.ResponseTimeMs = sw.ElapsedMilliseconds;
            result.Response = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!result.Success)
            {
                result.Error = $"Webhook returned status {response.StatusCode}";
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

    public async Task<WorkflowTriggerResult> TriggerWorkflowAsync(string workflowId, object payload, CancellationToken cancellationToken = default)
    {
        var result = new WorkflowTriggerResult
        {
            WorkflowId = workflowId
        };

        try
        {
            // n8n can trigger workflows via webhook or via the API
            // For API trigger, we use the workflow activation endpoint with test data
            var content = new StringContent(
                JsonSerializer.Serialize(payload, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Try webhook trigger first
            if (!string.IsNullOrEmpty(_config.WebhookBaseUrl))
            {
                var webhookResponse = await _httpClient.PostAsync(
                    $"{_config.WebhookBaseUrl}/webhook/{workflowId}",
                    content,
                    cancellationToken);

                if (webhookResponse.IsSuccessStatusCode)
                {
                    result.Success = true;
                    result.ExecutionId = Guid.NewGuid().ToString();
                    result.Status = "triggered";
                    result.Output = await webhookResponse.Content.ReadFromJsonAsync<object>(cancellationToken);
                    return result;
                }
            }

            // Fallback to API execution
            var apiResponse = await _httpClient.PostAsync(
                $"/api/v1/workflows/{workflowId}/execute",
                content,
                cancellationToken);

            if (apiResponse.IsSuccessStatusCode)
            {
                var execResult = await apiResponse.Content.ReadFromJsonAsync<N8nExecutionResponse>(cancellationToken);
                result.Success = true;
                result.ExecutionId = execResult?.Data?.Id;
                result.Status = execResult?.Data?.Status ?? "triggered";
                result.Output = execResult?.Data?.Data;
            }
            else
            {
                result.Success = false;
                result.Error = $"n8n API returned {apiResponse.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering n8n workflow {WorkflowId}", workflowId);
            result.Success = false;
            result.Error = ex.Message;
        }

        return result;
    }

    public async Task<IEnumerable<WorkflowInfo>> GetWorkflowsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/workflows", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get n8n workflows: {StatusCode}", response.StatusCode);
                return Array.Empty<WorkflowInfo>();
            }

            var result = await response.Content.ReadFromJsonAsync<N8nWorkflowsResponse>(cancellationToken);

            return result?.Data?.Select(w => new WorkflowInfo
            {
                Id = w.Id,
                Name = w.Name,
                Description = w.Description,
                IsActive = w.Active,
                TriggerType = DetermineTriggerType(w.Nodes),
                LastExecutedAt = w.UpdatedAt,
                ExecutionCount = w.StaticData != null ? 1 : 0
            }) ?? Array.Empty<WorkflowInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting n8n workflows");
            return Array.Empty<WorkflowInfo>();
        }
    }

    public async Task<IEnumerable<WorkflowExecution>> GetWorkflowExecutionsAsync(string workflowId, int limit = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/v1/executions?workflowId={workflowId}&limit={limit}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Array.Empty<WorkflowExecution>();
            }

            var result = await response.Content.ReadFromJsonAsync<N8nExecutionsResponse>(cancellationToken);

            return result?.Data?.Select(e => new WorkflowExecution
            {
                ExecutionId = e.Id,
                WorkflowId = workflowId,
                Status = e.Finished switch { true when e.StoppedAt != null => "success", true => "error", _ => "running" },
                StartedAt = e.StartedAt,
                FinishedAt = e.StoppedAt,
                DurationMs = e.StoppedAt.HasValue && e.StartedAt != default
                    ? (long)(e.StoppedAt.Value - e.StartedAt).TotalMilliseconds
                    : null,
                Error = e.Mode == "error" ? "Execution failed" : null
            }) ?? Array.Empty<WorkflowExecution>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting n8n workflow executions");
            return Array.Empty<WorkflowExecution>();
        }
    }

    private static string DetermineTriggerType(List<N8nNode>? nodes)
    {
        if (nodes == null || !nodes.Any())
        {
            return "manual";
        }

        var triggerNode = nodes.FirstOrDefault(n =>
            n.Type?.Contains("trigger", StringComparison.OrdinalIgnoreCase) == true ||
            n.Type?.Contains("webhook", StringComparison.OrdinalIgnoreCase) == true ||
            n.Type?.Contains("schedule", StringComparison.OrdinalIgnoreCase) == true);

        if (triggerNode == null)
        {
            return "manual";
        }

        if (triggerNode.Type?.Contains("webhook", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "webhook";
        }
        if (triggerNode.Type?.Contains("schedule", StringComparison.OrdinalIgnoreCase) == true ||
            triggerNode.Type?.Contains("cron", StringComparison.OrdinalIgnoreCase) == true)
            return "schedule";

        return "trigger";
    }

    #endregion

    #region Connection Management

    public async Task<IEnumerable<ConnectedApp>> GetConnectedAppsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/credentials", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Array.Empty<ConnectedApp>();
            }

            var result = await response.Content.ReadFromJsonAsync<N8nCredentialsResponse>(cancellationToken);

            return result?.Data?.Select(c => new ConnectedApp
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type,
                IsConnected = true,
                ConnectedAt = c.CreatedAt,
                Status = "Active"
            }) ?? Array.Empty<ConnectedApp>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting n8n credentials");
            return Array.Empty<ConnectedApp>();
        }
    }

    public async Task<ConnectionTestResult> TestConnectionAsync(string connectionId, CancellationToken cancellationToken = default)
    {
        var result = new ConnectionTestResult
        {
            ConnectionId = connectionId
        };
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await _httpClient.PostAsync(
                $"/api/v1/credentials/{connectionId}/test",
                null,
                cancellationToken);

            sw.Stop();
            result.ResponseTimeMs = sw.ElapsedMilliseconds;
            result.Success = response.IsSuccessStatusCode;
            result.Message = result.Success ? "Connection test successful" : "Connection test failed";
        }
        catch (Exception ex)
        {
            sw.Stop();
            result.Success = false;
            result.ResponseTimeMs = sw.ElapsedMilliseconds;
            result.Message = ex.Message;
        }

        return result;
    }

    #endregion

    #region Incoming Webhook Processing

    public Task<IntegrationWebhookResult> ProcessIncomingWebhookAsync(string eventType, string payload, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(payload, _jsonOptions);

            // n8n sends execution results back via webhooks
            _logger.LogInformation("Received n8n webhook callback: {EventType}", eventType);

            return Task.FromResult(new IntegrationWebhookResult
            {
                Success = true,
                EventType = eventType,
                Action = "callback",
                ProcessedData = data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing n8n webhook");
            return Task.FromResult(new IntegrationWebhookResult
            {
                Success = false,
                Error = ex.Message
            });
        }
    }

    #endregion

    public async Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/healthz", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var workflows = await GetWorkflowsAsync(cancellationToken);
                var activeCount = workflows.Count(w => w.IsActive);

                return new ProviderHealthResult
                {
                    IsHealthy = true,
                    ProviderName = ProviderName,
                    Message = $"n8n is healthy. {activeCount} active workflows.",
                    Details = new Dictionary<string, object>
                    {
                        { "baseUrl", _config.BaseUrl ?? "not configured" },
                        { "totalWorkflows", workflows.Count() },
                        { "activeWorkflows", activeCount }
                    }
                };
            }

            return new ProviderHealthResult
            {
                IsHealthy = false,
                ProviderName = ProviderName,
                Message = $"n8n health check failed: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new ProviderHealthResult
            {
                IsHealthy = false,
                ProviderName = ProviderName,
                Message = $"n8n health check failed: {ex.Message}"
            };
        }
    }
}

#region n8n API Response Models

internal class N8nWorkflowsResponse
{
    public List<N8nWorkflow>? Data { get; set; }
}

internal class N8nWorkflow
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Active { get; set; }
    public List<N8nNode>? Nodes { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public object? StaticData { get; set; }
}

internal class N8nNode
{
    public string? Type { get; set; }
    public string? Name { get; set; }
}

internal class N8nExecutionsResponse
{
    public List<N8nExecution>? Data { get; set; }
}

internal class N8nExecution
{
    public string Id { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? StoppedAt { get; set; }
    public bool Finished { get; set; }
    public string? Mode { get; set; }
}

internal class N8nExecutionResponse
{
    public N8nExecutionData? Data { get; set; }
}

internal class N8nExecutionData
{
    public string? Id { get; set; }
    public string? Status { get; set; }
    public object? Data { get; set; }
}

internal class N8nCredentialsResponse
{
    public List<N8nCredential>? Data { get; set; }
}

internal class N8nCredential
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

#endregion

/// <summary>
/// Configuration for n8n provider.
/// </summary>
public class N8nConfiguration
{
    /// <summary>
    /// Base URL of n8n instance (e.g., https://n8n.company.com).
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// n8n API key for authentication.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Base URL for n8n webhooks (may differ from API URL).
    /// </summary>
    public string? WebhookBaseUrl { get; set; }

    /// <summary>
    /// Mapping of CRM event types to n8n webhook URLs.
    /// Keys can be specific events (account.created) or wildcards (account.*, *).
    /// </summary>
    public Dictionary<string, string>? EventWebhooks { get; set; }

    /// <summary>
    /// Timeout for n8n API calls in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    public (bool IsValid, string? Error) Validate()
    {
        if (string.IsNullOrEmpty(BaseUrl))
        {
            return (false, "BaseUrl is required");
        }

        return (true, null);
    }
}
