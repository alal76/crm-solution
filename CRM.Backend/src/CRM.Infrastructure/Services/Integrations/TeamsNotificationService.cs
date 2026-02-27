// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Text;
using System.Text.Json;
using ITeamsIntegrationService = CRM.Core.Ports.Input.ITeamsIntegrationService;
using TeamsNotificationResult = CRM.Core.Ports.Input.TeamsNotificationResult;
using AdaptiveCardPayload = CRM.Core.Ports.Input.AdaptiveCardPayload;
using AdaptiveCardAttachment = CRM.Core.Ports.Input.AdaptiveCardAttachment;
using CrmTeamsNotification = CRM.Core.Ports.Input.CrmTeamsNotification;
using NotificationPriority = CRM.Core.Ports.Input.NotificationPriority;
using TeamsChannelConfig = CRM.Core.Ports.Input.TeamsChannelConfig;
using TeamsChannelConfigResult = CRM.Core.Ports.Input.TeamsChannelConfigResult;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Integrations;

/// <summary>
/// Implementation of ITeamsIntegrationService for Microsoft Teams notifications.
/// Uses Teams incoming webhook URLs to deliver messages and Adaptive Cards.
/// Implements TODO-INT-05.
/// </summary>
public class TeamsNotificationService : ITeamsIntegrationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TeamsNotificationService> _logger;

    // In-memory channel store (in production, use a persistent store)
    private readonly List<TeamsChannelConfig> _channels = new();

    public TeamsNotificationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<TeamsNotificationService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<TeamsNotificationResult> SendNotificationAsync(
        string channelId,
        string message,
        CancellationToken cancellationToken = default)
    {
        var webhookUrl = ResolveWebhookUrl(channelId);
        if (string.IsNullOrEmpty(webhookUrl))
            return TeamsNotificationResult.Failed("Channel not configured or webhook URL not found");

        var payload = new { text = message };
        return await PostToTeamsAsync(webhookUrl, payload, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TeamsNotificationResult> PostAdaptiveCardAsync(
        string channelId,
        AdaptiveCardPayload card,
        CancellationToken cancellationToken = default)
    {
        var webhookUrl = ResolveWebhookUrl(channelId);
        if (string.IsNullOrEmpty(webhookUrl))
            return TeamsNotificationResult.Failed("Channel not configured or webhook URL not found");

        var payload = new
        {
            type = card.Type,
            attachments = card.Attachments.Select(a => new
            {
                contentType = a.ContentType,
                contentUrl = a.ContentUrl,
                content = a.Content
            })
        };

        return await PostToTeamsAsync(webhookUrl, payload, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TeamsNotificationResult> SendCrmNotificationAsync(
        string channelId,
        CrmTeamsNotification notification,
        CancellationToken cancellationToken = default)
    {
        var themeColor = notification.ThemeColor ?? notification.Priority switch
        {
            NotificationPriority.Urgent => "FF0000",
            NotificationPriority.High => "FF8C00",
            NotificationPriority.Normal => "0078D4",
            NotificationPriority.Low => "808080",
            _ => "0078D4"
        };

        // Build Adaptive Card for CRM notification
        var card = AdaptiveCardPayload.CreateSimple(
            notification.Title,
            notification.Message,
            notification.EntityUrl);

        return await PostAdaptiveCardAsync(channelId, card, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ValidateWebhookAsync(string webhookUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
            return false;

        if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out var uri))
            return false;

        // Teams webhook URLs should contain ".webhook.office.com" or "webhook.office.com"
        // or the newer "*.logic.azure.com" pattern
        var validHosts = new[] { "webhook.office.com", "logic.azure.com", "outlook.office.com" };
        if (!validHosts.Any(h => uri.Host.Contains(h, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("Webhook URL host {Host} does not match known Teams webhook patterns", uri.Host);
        }

        try
        {
            // Send a minimal test payload
            var testPayload = new { text = "CRM connectivity test - please ignore" };
            var json = JsonSerializer.Serialize(testPayload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync(webhookUrl, content, cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate Teams webhook URL: {Url}", webhookUrl);
            return false;
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TeamsChannelConfig>> GetConfiguredChannelsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<TeamsChannelConfig>>(_channels.Where(c => c.IsActive).ToList());
    }

    /// <inheritdoc />
    public async Task<TeamsChannelConfigResult> ConfigureChannelAsync(
        TeamsChannelConfig config,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(config.WebhookUrl))
            return TeamsChannelConfigResult.Failed("Webhook URL is required");

        var isValid = await ValidateWebhookAsync(config.WebhookUrl, cancellationToken);

        var channelId = string.IsNullOrEmpty(config.Id) ? Guid.NewGuid().ToString("N") : config.Id;

        var newConfig = config with
        {
            Id = channelId,
            CreatedAt = DateTime.UtcNow
        };

        // Remove existing config with the same ID
        _channels.RemoveAll(c => c.Id == channelId);
        _channels.Add(newConfig);

        _logger.LogInformation("Teams channel configured: {ChannelId} ({Name})", channelId, config.Name);
        return TeamsChannelConfigResult.Succeeded(channelId);
    }

    /// <inheritdoc />
    public Task<bool> RemoveChannelAsync(string channelId, CancellationToken cancellationToken = default)
    {
        var removed = _channels.RemoveAll(c => c.Id == channelId);
        return Task.FromResult(removed > 0);
    }

    private string? ResolveWebhookUrl(string channelId)
    {
        // Check if channelId is already a URL
        if (Uri.TryCreate(channelId, UriKind.Absolute, out _))
            return channelId;

        // Look up in configured channels
        var channel = _channels.FirstOrDefault(c => c.Id == channelId && c.IsActive);
        if (channel != null)
            return channel.WebhookUrl;

        // Fall back to configuration
        return _configuration[$"Integrations:Teams:Channels:{channelId}:WebhookUrl"];
    }

    private async Task<TeamsNotificationResult> PostToTeamsAsync(string webhookUrl, object payload, CancellationToken cancellationToken)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync(webhookUrl, content, cancellationToken);

            sw.Stop();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Teams notification sent successfully in {Latency}ms", sw.ElapsedMilliseconds);
                return TeamsNotificationResult.Succeeded((int)response.StatusCode, sw.ElapsedMilliseconds);
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Teams notification failed with status {StatusCode}: {Body}",
                (int)response.StatusCode, responseBody);

            return TeamsNotificationResult.Failed(
                $"HTTP {(int)response.StatusCode}: {responseBody}",
                (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Error sending Teams notification to {Url}", webhookUrl);
            return TeamsNotificationResult.Failed(ex.Message);
        }
    }
}
