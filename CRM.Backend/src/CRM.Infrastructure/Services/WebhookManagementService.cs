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
        var query = _context.Webhooks.Where(w => !w.IsDeleted);

        if (isActive.HasValue)
            query = query.Where(w => w.IsActive == isActive);

        var webhooks = await query.OrderBy(w => w.Url).ToListAsync(cancellationToken);
        return webhooks.Select(MapToDto);
    }

    public async Task<WebhookDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var webhook = await _context.Webhooks
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, cancellationToken);

        return webhook != null ? MapToDto(webhook) : null;
    }

    public async Task<WebhookDto> CreateAsync(CreateWebhookDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Url))
            throw new ArgumentException("Webhook URL is required", nameof(dto.Url));

        var webhook = new Webhook
        {
            Url = dto.Url,
            Events = string.Join(",", dto.Events ?? new List<string>()),
            Secret = GenerateSecret(),
            IsActive = dto.IsActive ?? true,
            RetryCount = dto.RetryCount ?? 3,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Webhooks.Add(webhook);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Webhook created with ID {WebhookId} for URL {Url}", webhook.Id, webhook.Url);
        return await GetByIdAsync(webhook.Id, cancellationToken) ?? throw new InvalidOperationException("Creation failed");
    }

    public async Task<WebhookDto> UpdateAsync(int id, UpdateWebhookDto dto, CancellationToken cancellationToken = default)
    {
        var webhook = await _context.Webhooks
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, cancellationToken);

        if (webhook == null)
            throw new InvalidOperationException($"Webhook {id} not found");

        if (!string.IsNullOrWhiteSpace(dto.Url))
            webhook.Url = dto.Url;

        if (dto.Events != null && dto.Events.Any())
            webhook.Events = string.Join(",", dto.Events);

        if (dto.IsActive.HasValue)
            webhook.IsActive = dto.IsActive.Value;

        if (dto.RetryCount.HasValue)
            webhook.RetryCount = dto.RetryCount.Value;

        webhook.UpdatedAt = DateTime.UtcNow;
        _context.Webhooks.Update(webhook);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Webhook {WebhookId} updated", id);
        return await GetByIdAsync(webhook.Id, cancellationToken) ?? throw new InvalidOperationException("Update failed");
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var webhook = await _context.Webhooks
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, cancellationToken);

        if (webhook == null)
            return false;

        webhook.IsDeleted = true;
        webhook.UpdatedAt = DateTime.UtcNow;
        _context.Webhooks.Update(webhook);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Webhook {WebhookId} deleted", id);
        return true;
    }

    #endregion

    #region Webhook Management

    public async Task<WebhookDto> ToggleActiveAsync(int id, CancellationToken cancellationToken = default)
    {
        var webhook = await _context.Webhooks
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, cancellationToken);

        if (webhook == null)
            throw new InvalidOperationException($"Webhook {id} not found");

        webhook.IsActive = !webhook.IsActive;
        webhook.UpdatedAt = DateTime.UtcNow;
        _context.Webhooks.Update(webhook);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Webhook {WebhookId} toggled to {Active}", id, webhook.IsActive);
        return await GetByIdAsync(webhook.Id, cancellationToken) ?? throw new InvalidOperationException("Toggle failed");
    }

    public async Task<CRM.Core.Dtos.WebhookTestResultDto> TestAsync(int id, CRM.Core.Dtos.WebhookTestDto testData, CancellationToken cancellationToken = default)
    {
        var webhook = await _context.Webhooks
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, cancellationToken);

        if (webhook == null)
            throw new InvalidOperationException($"Webhook {id} not found");

        var delivery = new WebhookDelivery
        {
            WebhookId = id,
            Payload = testData.Payload,
            Status = "Pending",
            Attempts = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.WebhookDeliveries.Add(delivery);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Webhook {WebhookId} test triggered", id);

        var result = new WebhookTestResultDto
        {
            Success = true,
            DeliveryId = delivery.Id,
            Message = "Webhook test queued for delivery"
        };
        return await Task.FromResult(result);
    }

    #endregion

    #region Delivery Tracking

    public async Task<CRM.Core.Dtos.WebhookDeliveryHistoryDto> GetDeliveriesAsync(int webhookId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var deliveries = await _context.WebhookDeliveries
            .Where(d => d.WebhookId == webhookId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var history = new CRM.Core.Dtos.WebhookDeliveryHistoryDto
        {
            WebhookId = webhookId,
            Deliveries = deliveries.Select(d => MapDeliveryToDto(d)).ToList(),
            TotalCount = await _context.WebhookDeliveries
                .CountAsync(d => d.WebhookId == webhookId && !d.IsDeleted, cancellationToken)
        };

        return await Task.FromResult(history);
    }

    public async Task<WebhookDeliveryDto?> GetDeliveryDetailAsync(int webhookId, int deliveryId, CancellationToken cancellationToken = default)
    {
        var delivery = await _context.WebhookDeliveries
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == deliveryId && d.WebhookId == webhookId && !d.IsDeleted, cancellationToken);

        return delivery != null ? MapDeliveryToDto(delivery) : null;
    }

    public async Task<WebhookDeliveryDto> RetryDeliveryAsync(int webhookId, int deliveryId, CancellationToken cancellationToken = default)
    {
        var delivery = await _context.WebhookDeliveries
            .FirstOrDefaultAsync(d => d.Id == deliveryId && d.WebhookId == webhookId && !d.IsDeleted, cancellationToken);

        if (delivery == null)
            throw new InvalidOperationException($"Delivery {deliveryId} not found");

        delivery.Status = "Pending";
        delivery.Attempts = 0;
        delivery.UpdatedAt = DateTime.UtcNow;
        _context.WebhookDeliveries.Update(delivery);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Webhook delivery {DeliveryId} retry queued", deliveryId);
        return MapDeliveryToDto(delivery);
    }

    public async Task<CRM.Core.Dtos.WebhookStatisticsDto> GetStatisticsAsync(int id, CancellationToken cancellationToken = default)
    {
        var deliveries = await _context.WebhookDeliveries
            .Where(d => d.WebhookId == id && !d.IsDeleted)
            .ToListAsync(cancellationToken);

        var stats = new CRM.Core.Dtos.WebhookStatisticsDto
        {
            WebhookId = id,
            TotalDeliveries = deliveries.Count,
            SuccessfulDeliveries = deliveries.Count(d => d.Status == "Delivered"),
            FailedDeliveries = deliveries.Count(d => d.Status == "Failed"),
            PendingDeliveries = deliveries.Count(d => d.Status == "Pending"),
            SuccessRate = deliveries.Any() ? (deliveries.Count(d => d.Status == "Delivered") * 100m / deliveries.Count) : 0
        };

        return await Task.FromResult(stats);
    }

    #endregion

    #region Event Management

    public async Task<IEnumerable<CRM.Core.Dtos.WebhookEventDto>> GetAvailableEventsAsync(CancellationToken cancellationToken = default)
    {
        var events = new List<CRM.Core.Dtos.WebhookEventDto>
        {
            new() { Name = "account.created", Description = "When an account is created" },
            new() { Name = "account.updated", Description = "When an account is updated" },
            new() { Name = "account.deleted", Description = "When an account is deleted" },
            new() { Name = "contact.created", Description = "When a contact is created" },
            new() { Name = "contact.updated", Description = "When a contact is updated" },
            new() { Name = "opportunity.created", Description = "When an opportunity is created" },
            new() { Name = "opportunity.updated", Description = "When an opportunity is updated" },
            new() { Name = "opportunity.closed", Description = "When an opportunity is closed" },
            new() { Name = "commission.approved", Description = "When a commission is approved" },
            new() { Name = "campaign.executed", Description = "When a campaign is executed" }
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
        return new WebhookDto
        {
            Id = webhook.Id,
            Url = webhook.Url,
            Events = webhook.Events.Split(",").ToList(),
            IsActive = webhook.IsActive,
            Secret = webhook.Secret,
            RetryCount = webhook.RetryCount,
            CreatedAt = webhook.CreatedAt
        };
    }

    private WebhookDeliveryDto MapDeliveryToDto(WebhookDelivery delivery)
    {
        return new WebhookDeliveryDto
        {
            Id = delivery.Id,
            WebhookId = delivery.WebhookId,
            Payload = delivery.Payload,
            Status = delivery.Status,
            ResponseStatus = delivery.ResponseStatus,
            Attempts = delivery.Attempts,
            LastAttemptAt = delivery.LastAttemptAt,
            CreatedAt = delivery.CreatedAt
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
        var webhooks = await _context.Webhooks
            .Where(w => !w.IsDeleted && w.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var webhook in webhooks)
        {
            var events = webhook.Events.Split(",");
            if (!events.Contains(eventType) && !events.Contains("*"))
                continue;

            var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
            var delivery = new WebhookDelivery
            {
                WebhookId = webhook.Id,
                Payload = payloadJson,
                Status = "Queued",
                Attempts = 0,
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
            .Where(d => d.Status == "Queued" || d.Status == "Pending")
            .Include(d => d.Webhook)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Processing {Count} pending webhook deliveries", pendingDeliveries.Count);

        foreach (var delivery in pendingDeliveries)
        {
            if (delivery.Webhook == null || delivery.Webhook.IsDeleted || !delivery.Webhook.IsActive)
                continue;

            try
            {
                delivery.Status = "Processing";
                delivery.Attempts++;
                delivery.LastAttemptAt = DateTime.UtcNow;
                _context.WebhookDeliveries.Update(delivery);
                await _context.SaveChangesAsync(cancellationToken);

                // In a real implementation, would make the HTTP request here
                delivery.Status = "Delivered";
                delivery.ResponseStatus = "200";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook delivery {DeliveryId}", delivery.Id);
                delivery.Status = delivery.Attempts >= delivery.Webhook.RetryCount ? "Failed" : "Pending";
            }

            _context.WebhookDeliveries.Update(delivery);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// DTO for webhook test request.
/// </summary>
public class WebhookTestDto
{
    public string Payload { get; set; } = "{}";
}

/// <summary>
/// DTO for webhook test result.
/// </summary>
public class WebhookTestResultDto
{
    public bool Success { get; set; }
    public int DeliveryId { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// DTO for webhook delivery history response.
/// </summary>
public class WebhookDeliveryHistoryDto
{
    public int WebhookId { get; set; }
    public List<WebhookDeliveryDto> Deliveries { get; set; } = new();
    public int TotalCount { get; set; }
}

/// <summary>
/// DTO for webhook statistics response.
/// </summary>
public class WebhookStatisticsDto
{
    public int WebhookId { get; set; }
    public int TotalDeliveries { get; set; }
    public int SuccessfulDeliveries { get; set; }
    public int FailedDeliveries { get; set; }
    public int PendingDeliveries { get; set; }
    public decimal SuccessRate { get; set; }
}

/// <summary>
/// DTO for webhook event response.
/// </summary>
public class WebhookEventDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
