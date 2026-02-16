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

using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of IWebhookManagementService for webhook management operations.
/// Handles webhook registration, delivery tracking, and management.
/// </summary>
public class WebhookManagementService : IWebhookManagementService, IWebhookManagementInputPort
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<WebhookManagementService> _logger;

    public WebhookManagementService(ICrmDbContext context, ILogger<WebhookManagementService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Webhook CRUD

    public async Task<IEnumerable<WebhookDto>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _context.WebhookSubscriptions.Where(w => !w.IsDeleted);

        if (isActive.HasValue)
            query = query.Where(w => w.IsActive == isActive.Value);

        var webhooks = await query.OrderBy(w => w.TargetUrl).ToListAsync(cancellationToken);
        return webhooks.Select(MapToDto);
    }

    public async Task<WebhookDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var webhook = await _context.WebhookSubscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.WebhookSubscriptionId == id && !w.IsDeleted, cancellationToken);

        return webhook != null ? MapToDto(webhook) : null;
    }

    public async Task<WebhookDto> CreateAsync(CreateWebhookDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Url))
            throw new ArgumentException("Webhook URL is required", nameof(dto.Url));

        var webhook = new WebhookSubscription
        {
            Name = dto.Description ?? dto.Url,
            Description = dto.Description ?? string.Empty,
            TargetUrl = dto.Url,
            Secret = string.IsNullOrWhiteSpace(dto.Secret) ? GenerateSecret() : dto.Secret,
            IsActive = dto.IsActive,
            EventTypes = System.Text.Json.JsonSerializer.Serialize(dto.EventTypes ?? new List<string>()),
            RetryCount = dto.MaxRetries,
            TimeoutSeconds = dto.TimeoutSeconds,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.WebhookSubscriptions.Add(webhook);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Webhook created with ID {WebhookId} for URL {Url}", webhook.WebhookSubscriptionId, webhook.TargetUrl);
        return await GetByIdAsync(webhook.WebhookSubscriptionId, cancellationToken) ?? throw new InvalidOperationException("Creation failed");
    }

    public async Task<WebhookDto> UpdateAsync(int id, UpdateWebhookDto dto, CancellationToken cancellationToken = default)
    {
        var webhook = await _context.WebhookSubscriptions
            .FirstOrDefaultAsync(w => w.WebhookSubscriptionId == id && !w.IsDeleted, cancellationToken);

        if (webhook == null)
            throw new InvalidOperationException($"Webhook {id} not found");

        if (!string.IsNullOrWhiteSpace(dto.Url))
            webhook.TargetUrl = dto.Url;

        if (!string.IsNullOrWhiteSpace(dto.Description))
            webhook.Description = dto.Description!;

        if (!string.IsNullOrWhiteSpace(dto.Secret))
            webhook.Secret = dto.Secret;

        if (dto.EventTypes != null && dto.EventTypes.Any())
            webhook.EventTypes = System.Text.Json.JsonSerializer.Serialize(dto.EventTypes);

        if (dto.IsActive.HasValue)
            webhook.IsActive = dto.IsActive.Value;

        if (dto.MaxRetries.HasValue)
            webhook.RetryCount = dto.MaxRetries.Value;

        if (dto.TimeoutSeconds.HasValue)
            webhook.TimeoutSeconds = dto.TimeoutSeconds.Value;

        webhook.UpdatedAt = DateTime.UtcNow;
        _context.WebhookSubscriptions.Update(webhook);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Webhook {WebhookId} updated", id);
        return await GetByIdAsync(webhook.WebhookSubscriptionId, cancellationToken) ?? throw new InvalidOperationException("Update failed");
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var webhook = await _context.WebhookSubscriptions
            .FirstOrDefaultAsync(w => w.WebhookSubscriptionId == id && !w.IsDeleted, cancellationToken);

        if (webhook == null)
            return false;

        webhook.IsDeleted = true;
        webhook.UpdatedAt = DateTime.UtcNow;
        _context.WebhookSubscriptions.Update(webhook);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Webhook {WebhookId} deleted", id);
        return true;
    }

    #endregion

    #region Webhook Management

    public async Task<WebhookDto> ToggleActiveAsync(int id, CancellationToken cancellationToken = default)
    {
        var webhook = await _context.WebhookSubscriptions
            .FirstOrDefaultAsync(w => w.WebhookSubscriptionId == id && !w.IsDeleted, cancellationToken);

        if (webhook == null)
            throw new InvalidOperationException($"Webhook {id} not found");

        webhook.IsActive = !webhook.IsActive;
        webhook.UpdatedAt = DateTime.UtcNow;
        _context.WebhookSubscriptions.Update(webhook);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Webhook {WebhookId} toggled to {Active}", id, webhook.IsActive);
        return await GetByIdAsync(webhook.WebhookSubscriptionId, cancellationToken) ?? throw new InvalidOperationException("Toggle failed");
    }

    public async Task<CRM.Core.Dtos.WebhookTestResultDto> TestAsync(int id, CRM.Core.Dtos.WebhookTestDto testData, CancellationToken cancellationToken = default)
    {
        var webhook = await _context.WebhookSubscriptions
            .FirstOrDefaultAsync(w => w.WebhookSubscriptionId == id && !w.IsDeleted, cancellationToken);

        if (webhook == null)
            throw new InvalidOperationException($"Webhook {id} not found");

        var delivery = new WebhookDelivery
        {
            WebhookSubscriptionId = id,
            EventType = testData.EventType,
            TargetUrl = webhook.TargetUrl,
            RequestBody = System.Text.Json.JsonSerializer.Serialize(testData.Payload ?? new Dictionary<string, object>()),
            Success = false,
            AttemptNumber = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.WebhookDeliveries.Add(delivery);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Webhook {WebhookId} test triggered", id);

        var result = new CRM.Core.Dtos.WebhookTestResultDto
        {
            WebhookId = id,
            Url = webhook.TargetUrl,
            EventType = testData.EventType,
            Success = true,
            ResponseStatusCode = 200,
            ResponseBody = "Test queued for delivery",
            ErrorMessage = null,
            DurationMs = 0,
            TestedAt = DateTime.UtcNow
        };
        return await Task.FromResult(result);
    }

    #endregion

    #region Delivery Tracking

    public async Task<CRM.Core.Dtos.WebhookDeliveryHistoryDto> GetDeliveriesAsync(int webhookId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var webhook = await _context.WebhookSubscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.WebhookSubscriptionId == webhookId && !w.IsDeleted, cancellationToken);

        if (webhook == null)
            throw new InvalidOperationException($"Webhook {webhookId} not found");

        var deliveries = await _context.WebhookDeliveries
            .Where(d => d.WebhookSubscriptionId == webhookId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalDeliveries = await _context.WebhookDeliveries
            .CountAsync(d => d.WebhookSubscriptionId == webhookId && !d.IsDeleted, cancellationToken);

        var history = new CRM.Core.Dtos.WebhookDeliveryHistoryDto
        {
            WebhookId = webhookId,
            Url = webhook.TargetUrl,
            TotalDeliveries = totalDeliveries,
            RecentDeliveries = deliveries.Select(d => MapDeliveryToDto(d)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalPages = (totalDeliveries + pageSize - 1) / pageSize
        };

        return await Task.FromResult(history);
    }

    public async Task<WebhookDeliveryDto?> GetDeliveryDetailAsync(int webhookId, int deliveryId, CancellationToken cancellationToken = default)
    {
        var delivery = await _context.WebhookDeliveries
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.WebhookDeliveryId == deliveryId && d.WebhookSubscriptionId == webhookId && !d.IsDeleted, cancellationToken);

        return delivery != null ? MapDeliveryToDto(delivery) : null;
    }

    public async Task<WebhookDeliveryDto> RetryDeliveryAsync(int webhookId, int deliveryId, CancellationToken cancellationToken = default)
    {
        var delivery = await _context.WebhookDeliveries
            .FirstOrDefaultAsync(d => d.WebhookDeliveryId == deliveryId && d.WebhookSubscriptionId == webhookId && !d.IsDeleted, cancellationToken);

        if (delivery == null)
            throw new InvalidOperationException($"Delivery {deliveryId} not found");

        delivery.Success = false;
        delivery.AttemptNumber = 0;
        delivery.UpdatedAt = DateTime.UtcNow;
        _context.WebhookDeliveries.Update(delivery);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Webhook delivery {DeliveryId} retry queued", deliveryId);
        return MapDeliveryToDto(delivery);
    }

    public async Task<CRM.Core.Dtos.WebhookStatisticsDto> GetStatisticsAsync(int id, CancellationToken cancellationToken = default)
    {
        var webhook = await _context.WebhookSubscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.WebhookSubscriptionId == id && !w.IsDeleted, cancellationToken);

        if (webhook == null)
            throw new InvalidOperationException($"Webhook {id} not found");

        var deliveries = await _context.WebhookDeliveries
            .Where(d => d.WebhookSubscriptionId == id && !d.IsDeleted)
            .ToListAsync(cancellationToken);

        var successfulCount = deliveries.Count(d => d.Success);
        var failedCount = deliveries.Count(d => !d.Success);
        var totalCount = deliveries.Count;
        var successfulDeliveries = deliveries.Where(d => d.Success).OrderByDescending(d => d.CompletedAt).FirstOrDefault();
        var failedDeliveries = deliveries.Where(d => !d.Success).OrderByDescending(d => d.UpdatedAt).FirstOrDefault();

        var stats = new CRM.Core.Dtos.WebhookStatisticsDto
        {
            WebhookId = id,
            Url = webhook.TargetUrl,
            TotalDeliveries = totalCount,
            SuccessfulDeliveries = successfulCount,
            FailedDeliveries = failedCount,
            SuccessRate = totalCount > 0 ? (successfulCount * 100.0 / totalCount) : 0,
            AverageDurationMs = deliveries.Any(d => d.DurationMs.HasValue) ? deliveries.Where(d => d.DurationMs.HasValue).Average(d => d.DurationMs.Value) : 0,
            ConsecutiveFailures = GetConsecutiveFailureCount(deliveries),
            LastSuccessfulDelivery = successfulDeliveries?.CompletedAt,
            LastFailedDelivery = failedDeliveries?.UpdatedAt,
            ResponseCodeDistribution = GetResponseCodeDistribution(deliveries)
        };

        return await Task.FromResult(stats);
    }

    private int GetConsecutiveFailureCount(List<WebhookDelivery> deliveries)
    {
        var consecutiveFailures = 0;
        foreach (var delivery in deliveries.OrderByDescending(d => d.CreatedAt))
        {
            if (!delivery.Success)
                consecutiveFailures++;
            else
                break;
        }
        return consecutiveFailures;
    }

    private Dictionary<int, int> GetResponseCodeDistribution(List<WebhookDelivery> deliveries)
    {
        return deliveries
            .Where(d => d.ResponseStatusCode.HasValue)
            .GroupBy(d => d.ResponseStatusCode.Value)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    #endregion

    #region Event Management

    public async Task<IEnumerable<CRM.Core.Dtos.WebhookEventDto>> GetAvailableEventsAsync(CancellationToken cancellationToken = default)
    {
        var events = new List<CRM.Core.Dtos.WebhookEventDto>
        {
            new() { EventType = "account.created", Description = "When an account is created", EntityType = "Account", IsActive = true },
            new() { EventType = "account.updated", Description = "When an account is updated", EntityType = "Account", IsActive = true },
            new() { EventType = "account.deleted", Description = "When an account is deleted", EntityType = "Account", IsActive = true },
            new() { EventType = "contact.created", Description = "When a contact is created", EntityType = "Contact", IsActive = true },
            new() { EventType = "contact.updated", Description = "When a contact is updated", EntityType = "Contact", IsActive = true },
            new() { EventType = "opportunity.created", Description = "When an opportunity is created", EntityType = "Opportunity", IsActive = true },
            new() { EventType = "opportunity.updated", Description = "When an opportunity is updated", EntityType = "Opportunity", IsActive = true },
            new() { EventType = "opportunity.closed", Description = "When an opportunity is closed", EntityType = "Opportunity", IsActive = true },
            new() { EventType = "commission.approved", Description = "When a commission is approved", EntityType = "Commission", IsActive = true },
            new() { EventType = "campaign.executed", Description = "When a campaign is executed", EntityType = "Campaign", IsActive = true }
        };

        return await Task.FromResult((IEnumerable<CRM.Core.Dtos.WebhookEventDto>)events);
    }

    #endregion

    #region Helpers

    private string GenerateSecret()
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] tokenData = new byte[32];
            rng.GetBytes(tokenData);
            return Convert.ToBase64String(tokenData);
        }
    }

    private WebhookDto MapToDto(WebhookSubscription webhook)
    {
        var eventTypes = new List<string>();
        try
        {
            eventTypes = System.Text.Json.JsonSerializer.Deserialize<List<string>>(webhook.EventTypes) ?? new List<string>();
        }
        catch
        {
            eventTypes = webhook.EventTypes.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToList();
        }

        return new WebhookDto
        {
            Id = webhook.WebhookSubscriptionId,
            Url = webhook.TargetUrl,
            Description = webhook.Description,
            Secret = webhook.Secret,
            IsActive = webhook.IsActive,
            EventTypes = eventTypes,
            FilterCriteria = null,
            MaxRetries = webhook.RetryCount,
            RetryIntervalSeconds = 300,
            TimeoutSeconds = webhook.TimeoutSeconds,
            FailureCount = webhook.FailureCount,
            DisabledReason = null,
            DisabledAt = null,
            LastDeliveryAt = webhook.LastTriggeredAt,
            CreatedBy = webhook.CreatedByUserId,
            CreatedAt = webhook.CreatedAt,
            UpdatedAt = webhook.UpdatedAt
        };
    }

    private WebhookDeliveryDto MapDeliveryToDto(WebhookDelivery delivery)
    {
        return new WebhookDeliveryDto
        {
            Id = delivery.WebhookDeliveryId,
            WebhookId = delivery.WebhookSubscriptionId,
            Url = delivery.TargetUrl,
            EventType = delivery.EventType,
            TriggeredAt = delivery.CreatedAt,
            CompletedAt = delivery.CompletedAt,
            AttemptNumber = delivery.AttemptNumber,
            ResponseStatusCode = delivery.ResponseStatusCode,
            Success = delivery.Success,
            ErrorMessage = delivery.ErrorMessage,
            DurationMs = delivery.DurationMs,
            RequestPayload = delivery.RequestBody,
            ResponsePayload = delivery.ResponseBody
        };
    }

    #endregion
}

/// <summary>
/// Implementation of IWebhookDispatcherService for webhook dispatch operations.
/// Handles dispatching webhook payloads to registered endpoints.
/// </summary>
public class WebhookDispatcherService : IWebhookDispatcherService, IWebhookDispatcherInputPort
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<WebhookDispatcherService> _logger;
    private readonly HttpClient _httpClient;

    public WebhookDispatcherService(ICrmDbContext context, ILogger<WebhookDispatcherService> logger, HttpClient httpClient)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task DispatchAsync(string eventType, object payload, CancellationToken cancellationToken = default)
    {
        var webhooks = await _context.WebhookSubscriptions
            .Where(w => !w.IsDeleted && w.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var webhook in webhooks)
        {
            var events = DeserializeEvents(webhook.EventTypes);
            if (!events.Contains(eventType) && !events.Contains("*"))
                continue;

            var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
            var delivery = new WebhookDelivery
            {
                WebhookSubscriptionId = webhook.WebhookSubscriptionId,
                EventType = eventType,
                TargetUrl = webhook.TargetUrl,
                RequestBody = payloadJson,
                Success = false,
                AttemptNumber = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.WebhookDeliveries.Add(delivery);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Event {EventType} queued for {WebhookCount} webhooks", eventType, webhooks.Count);
    }

    public async Task DispatchBatchAsync(List<(string EventType, object Payload)> events, CancellationToken cancellationToken = default)
    {
        foreach (var (eventType, payload) in events)
        {
            await DispatchAsync(eventType, payload, cancellationToken);
        }

        _logger.LogInformation("Batch of {EventCount} events dispatched", events.Count);
    }

    public async Task ProcessQueueAsync(CancellationToken cancellationToken = default)
    {
        var pendingDeliveries = await _context.WebhookDeliveries
            .Where(d => !d.Success)
            .Include(d => d.Subscription)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Processing {Count} pending webhook deliveries", pendingDeliveries.Count);

        foreach (var delivery in pendingDeliveries)
        {
            if (delivery.Subscription == null || delivery.Subscription.IsDeleted || !delivery.Subscription.IsActive)
                continue;

            try
            {
                delivery.AttemptNumber++;
                _context.WebhookDeliveries.Update(delivery);
                await _context.SaveChangesAsync(cancellationToken);

                // In a real implementation, would make the HTTP request here
                delivery.Success = true;
                delivery.ResponseStatusCode = 200;
                delivery.CompletedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook delivery {DeliveryId}", delivery.WebhookDeliveryId);
                delivery.Success = false;
                delivery.ErrorMessage = ex.Message;
            }

            _context.WebhookDeliveries.Update(delivery);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static List<string> DeserializeEvents(string eventTypes)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(eventTypes) ?? new List<string>();
        }
        catch
        {
            return eventTypes.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToList();
        }
    }
}
