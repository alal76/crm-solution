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

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces.ITSM;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Service for sending webhook notifications for ITSM events.
/// Implements retry logic, signature verification, and delivery tracking.
/// </summary>
public class WebhookNotificationService : IWebhookNotificationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookNotificationService> _logger;
    private readonly List<WebhookSubscription> _subscriptions = new();
    private readonly List<WebhookDelivery> _deliveries = new();
    private int _nextSubscriptionId = 1;
    private int _nextDeliveryId = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public WebhookNotificationService(
        IHttpClientFactory httpClientFactory,
        ILogger<WebhookNotificationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        InitializeDefaultSubscriptions();
    }

    private void InitializeDefaultSubscriptions()
    {
        _subscriptions.Add(new WebhookSubscription
        {
            WebhookSubscriptionId = _nextSubscriptionId++,
            Name = "Slack Notifications",
            Description = "Send ITSM events to Slack",
            TargetUrl = "https://hooks.slack.com/services/example",
            Secret = GenerateSecret(),
            EventTypes = "[\"IncidentCreated\",\"IncidentResolved\",\"SLABreached\"]",
            IsActive = true,
            TimeoutSeconds = 30,
            RetryCount = 3,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        });

        _subscriptions.Add(new WebhookSubscription
        {
            WebhookSubscriptionId = _nextSubscriptionId++,
            Name = "External Ticketing System",
            Description = "Sync incidents to external system",
            TargetUrl = "https://api.example.com/webhooks/itsm",
            Secret = GenerateSecret(),
            EventTypes = "[\"IncidentCreated\",\"IncidentUpdated\",\"IncidentClosed\"]",
            IsActive = true,
            TimeoutSeconds = 30,
            RetryCount = 3,
            CreatedAt = DateTime.UtcNow.AddDays(-14)
        });
    }

    /// <inheritdoc />
    public async Task SendWebhookAsync(WebhookEventType eventType, object payload)
    {
        var eventTypeString = eventType.ToString();

        var matchingSubscriptions = _subscriptions
            .Where(s => s.IsActive && !s.IsDeleted)
            .Where(s => SubscriptionMatchesEvent(s, eventTypeString))
            .ToList();

        if (matchingSubscriptions.Count == 0)
        {
            _logger.LogDebug("No webhook subscriptions found for event type {EventType}", eventTypeString);
            return;
        }

        _logger.LogInformation("Sending webhook for {EventType} to {Count} subscriptions",
            eventTypeString, matchingSubscriptions.Count);

        foreach (var subscription in matchingSubscriptions)
        {
            await SendWebhookToSubscriptionAsync(subscription, eventTypeString, payload);
        }
    }

    private bool SubscriptionMatchesEvent(WebhookSubscription subscription, string eventType)
    {
        try
        {
            var eventTypes = JsonSerializer.Deserialize<List<string>>(subscription.EventTypes) ?? new List<string>();
            return eventTypes.Contains(eventType) || eventTypes.Contains("*");
        }
        catch
        {
            return false;
        }
    }

    private async Task SendWebhookToSubscriptionAsync(
        WebhookSubscription subscription,
        string eventType,
        object payload)
    {
        var wrappedPayload = new WebhookPayload<object>
        {
            EventType = eventType,
            EventId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
            Source = "CRM-ITSM",
            Data = payload
        };

        var jsonPayload = JsonSerializer.Serialize(wrappedPayload, JsonOptions);
        var delivery = new WebhookDelivery
        {
            WebhookDeliveryId = _nextDeliveryId++,
            WebhookSubscriptionId = subscription.WebhookSubscriptionId,
            EventType = eventType,
            TargetUrl = subscription.TargetUrl,
            RequestBody = jsonPayload,
            AttemptNumber = 1,
            CreatedAt = DateTime.UtcNow
        };

        var startTime = DateTime.UtcNow;

        try
        {
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(subscription.TimeoutSeconds);

            using var request = new HttpRequestMessage(HttpMethod.Post, subscription.TargetUrl);
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            AddCustomHeaders(request, subscription);

            if (!string.IsNullOrEmpty(subscription.Secret))
            {
                var signature = ComputeSignature(jsonPayload, subscription.Secret);
                request.Headers.Add("X-Webhook-Signature", signature);
            }

            request.Headers.Add("X-Webhook-Event", eventType);
            request.Headers.Add("X-Webhook-Delivery-Id", delivery.WebhookDeliveryId.ToString());

            var response = await client.SendAsync(request);

            delivery.ResponseStatusCode = (int)response.StatusCode;
            delivery.ResponseBody = await response.Content.ReadAsStringAsync();
            delivery.Success = response.IsSuccessStatusCode;
            delivery.CompletedAt = DateTime.UtcNow;
            delivery.DurationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;

            if (delivery.Success)
            {
                subscription.SuccessCount++;
                _logger.LogDebug("Webhook delivered successfully to {Url}", subscription.TargetUrl);
            }
            else
            {
                subscription.FailureCount++;
                delivery.ErrorMessage = $"HTTP {delivery.ResponseStatusCode}";
                _logger.LogWarning("Webhook delivery failed to {Url}: HTTP {StatusCode}",
                    subscription.TargetUrl, delivery.ResponseStatusCode);
            }
        }
        catch (Exception ex)
        {
            delivery.Success = false;
            delivery.ErrorMessage = ex.Message;
            delivery.CompletedAt = DateTime.UtcNow;
            delivery.DurationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            subscription.FailureCount++;

            _logger.LogError(ex, "Webhook delivery failed to {Url}", subscription.TargetUrl);
        }

        subscription.LastTriggeredAt = DateTime.UtcNow;
        _deliveries.Add(delivery);
    }

    private void AddCustomHeaders(HttpRequestMessage request, WebhookSubscription subscription)
    {
        try
        {
            if (string.IsNullOrEmpty(subscription.Headers)) return;

            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(subscription.Headers);
            if (headers != null)
            {
                foreach (var (key, value) in headers)
                {
                    request.Headers.TryAddWithoutValidation(key, value);
                }
            }
        }
        catch
        {
            // Ignore invalid header configuration
        }
    }

    private static string ComputeSignature(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return $"sha256={Convert.ToHexString(hash).ToLowerInvariant()}";
    }

    /// <inheritdoc />
    public Task<WebhookSubscriptionDto> CreateSubscriptionAsync(CreateWebhookSubscriptionDto dto, int createdByUserId)
    {
        var subscription = new WebhookSubscription
        {
            WebhookSubscriptionId = _nextSubscriptionId++,
            Name = dto.Name,
            Description = dto.Description,
            TargetUrl = dto.TargetUrl,
            Secret = dto.Secret ?? GenerateSecret(),
            EventTypes = JsonSerializer.Serialize(dto.EventTypes),
            Headers = dto.Headers != null ? JsonSerializer.Serialize(dto.Headers) : "{}",
            IsActive = true,
            TimeoutSeconds = dto.TimeoutSeconds,
            RetryCount = dto.RetryCount,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };

        _subscriptions.Add(subscription);
        _logger.LogInformation("Created webhook subscription {Name} for {Url} by user {UserId}",
            subscription.Name, subscription.TargetUrl, createdByUserId);

        return Task.FromResult(MapToDto(subscription));
    }

    /// <inheritdoc />
    public Task<IEnumerable<WebhookSubscriptionDto>> GetSubscriptionsAsync()
    {
        var result = _subscriptions
            .Where(s => !s.IsDeleted)
            .Select(MapToDto)
            .AsEnumerable();

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<WebhookSubscriptionDto?> GetSubscriptionByIdAsync(int id)
    {
        var subscription = _subscriptions.FirstOrDefault(s => s.WebhookSubscriptionId == id && !s.IsDeleted);
        return Task.FromResult(subscription != null ? MapToDto(subscription) : null);
    }

    /// <inheritdoc />
    public Task<WebhookSubscriptionDto> UpdateSubscriptionAsync(int id, UpdateWebhookSubscriptionDto dto, int modifiedByUserId)
    {
        var subscription = _subscriptions.FirstOrDefault(s => s.WebhookSubscriptionId == id);
        if (subscription == null)
        {
            throw new KeyNotFoundException($"Webhook subscription {id} not found");
        }

        if (dto.Name != null) subscription.Name = dto.Name;
        if (dto.Description != null) subscription.Description = dto.Description;
        if (dto.TargetUrl != null) subscription.TargetUrl = dto.TargetUrl;
        if (dto.Secret != null) subscription.Secret = dto.Secret;
        if (dto.EventTypes != null) subscription.EventTypes = JsonSerializer.Serialize(dto.EventTypes);
        if (dto.Headers != null) subscription.Headers = JsonSerializer.Serialize(dto.Headers);
        if (dto.IsActive.HasValue) subscription.IsActive = dto.IsActive.Value;
        if (dto.TimeoutSeconds.HasValue) subscription.TimeoutSeconds = dto.TimeoutSeconds.Value;
        if (dto.RetryCount.HasValue) subscription.RetryCount = dto.RetryCount.Value;

        // Note: modifiedByUserId tracked via logs for now

        _logger.LogInformation("Updated webhook subscription {Id} by user {UserId}", id, modifiedByUserId);
        return Task.FromResult(MapToDto(subscription));
    }

    /// <inheritdoc />
    public Task<bool> DeleteSubscriptionAsync(int id)
    {
        var subscription = _subscriptions.FirstOrDefault(s => s.WebhookSubscriptionId == id);
        if (subscription == null)
        {
            return Task.FromResult(false);
        }

        subscription.IsDeleted = true;

        _logger.LogInformation("Deleted webhook subscription {Id}", id);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<IEnumerable<WebhookDeliveryDto>> GetDeliveryHistoryAsync(int? subscriptionId = null, int pageNumber = 1, int pageSize = 50)
    {
        var query = _deliveries.AsEnumerable();

        if (subscriptionId.HasValue)
        {
            query = query.Where(d => d.WebhookSubscriptionId == subscriptionId.Value);
        }

        var result = query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(d =>
            {
                var sub = _subscriptions.FirstOrDefault(s => s.WebhookSubscriptionId == d.WebhookSubscriptionId);
                return new WebhookDeliveryDto
                {
                    WebhookDeliveryId = d.WebhookDeliveryId,
                    WebhookSubscriptionId = d.WebhookSubscriptionId,
                    SubscriptionName = sub?.Name ?? "Unknown",
                    EventType = d.EventType,
                    TargetUrl = d.TargetUrl,
                    RequestBody = d.RequestBody,
                    ResponseStatusCode = d.ResponseStatusCode,
                    ResponseBody = d.ResponseBody,
                    Success = d.Success,
                    ErrorMessage = d.ErrorMessage,
                    AttemptNumber = d.AttemptNumber,
                    CreatedAt = d.CreatedAt,
                    CompletedAt = d.CompletedAt,
                    DurationMs = d.DurationMs
                };
            });

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public async Task<bool> RetryDeliveryAsync(int deliveryId)
    {
        var delivery = _deliveries.FirstOrDefault(d => d.WebhookDeliveryId == deliveryId);
        if (delivery == null)
        {
            return false;
        }

        var subscription = _subscriptions.FirstOrDefault(s => s.WebhookSubscriptionId == delivery.WebhookSubscriptionId);
        if (subscription == null || !subscription.IsActive)
        {
            return false;
        }

        _logger.LogInformation("Retrying webhook delivery {DeliveryId}", deliveryId);

        // Create a retry delivery
        var retryDelivery = new WebhookDelivery
        {
            WebhookDeliveryId = _nextDeliveryId++,
            WebhookSubscriptionId = subscription.WebhookSubscriptionId,
            EventType = delivery.EventType,
            TargetUrl = subscription.TargetUrl,
            RequestBody = delivery.RequestBody,
            AttemptNumber = delivery.AttemptNumber + 1,
            CreatedAt = DateTime.UtcNow
        };

        var startTime = DateTime.UtcNow;

        try
        {
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(subscription.TimeoutSeconds);

            using var request = new HttpRequestMessage(HttpMethod.Post, subscription.TargetUrl);
            request.Content = new StringContent(delivery.RequestBody ?? "{}", Encoding.UTF8, "application/json");

            if (!string.IsNullOrEmpty(subscription.Secret))
            {
                var signature = ComputeSignature(delivery.RequestBody ?? "{}", subscription.Secret);
                request.Headers.Add("X-Webhook-Signature", signature);
            }

            request.Headers.Add("X-Webhook-Event", delivery.EventType);
            request.Headers.Add("X-Webhook-Delivery-Id", retryDelivery.WebhookDeliveryId.ToString());
            request.Headers.Add("X-Webhook-Retry", "true");

            var response = await client.SendAsync(request);

            retryDelivery.ResponseStatusCode = (int)response.StatusCode;
            retryDelivery.ResponseBody = await response.Content.ReadAsStringAsync();
            retryDelivery.Success = response.IsSuccessStatusCode;
            retryDelivery.CompletedAt = DateTime.UtcNow;
            retryDelivery.DurationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;

            if (retryDelivery.Success)
            {
                subscription.SuccessCount++;
            }
            else
            {
                subscription.FailureCount++;
                retryDelivery.ErrorMessage = $"HTTP {retryDelivery.ResponseStatusCode}";
            }
        }
        catch (Exception ex)
        {
            retryDelivery.Success = false;
            retryDelivery.ErrorMessage = ex.Message;
            retryDelivery.CompletedAt = DateTime.UtcNow;
            retryDelivery.DurationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            subscription.FailureCount++;
        }

        _deliveries.Add(retryDelivery);
        return retryDelivery.Success;
    }

    private static WebhookSubscriptionDto MapToDto(WebhookSubscription entity)
    {
        return new WebhookSubscriptionDto
        {
            WebhookSubscriptionId = entity.WebhookSubscriptionId,
            Name = entity.Name,
            Description = entity.Description,
            TargetUrl = entity.TargetUrl,
            Secret = MaskSecret(entity.Secret),
            EventTypes = DeserializeEventTypes(entity.EventTypes),
            Headers = DeserializeHeaders(entity.Headers),
            IsActive = entity.IsActive,
            TimeoutSeconds = entity.TimeoutSeconds,
            RetryCount = entity.RetryCount,
            SuccessCount = entity.SuccessCount,
            FailureCount = entity.FailureCount,
            LastTriggeredAt = entity.LastTriggeredAt,
            CreatedAt = entity.CreatedAt
        };
    }

    private static string MaskSecret(string? secret)
    {
        if (string.IsNullOrEmpty(secret) || secret.Length < 8)
        {
            return "****";
        }
        return $"{secret[..4]}...{secret[^4..]}";
    }

    private static List<string> DeserializeEventTypes(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new List<string>();
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static Dictionary<string, string> DeserializeHeaders(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new Dictionary<string, string>();
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    private static string GenerateSecret()
    {
        return $"whsec_{Guid.NewGuid():N}";
    }
}
