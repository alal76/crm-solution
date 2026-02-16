// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

using System.Text.Json;
using CRM.Core.Interfaces.Notifications;
using CRM.Core.Ports.Output.Providers;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Notifications;

public class NotificationDispatcher : INotificationDispatcher
{
    private readonly ILogger<NotificationDispatcher> _logger;
    private readonly INotificationPort _notificationPort;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public NotificationDispatcher(INotificationPort notificationPort, ILogger<NotificationDispatcher> logger)
    {
        _notificationPort = notificationPort;
        _logger = logger;
    }

    public Task DispatchAsync(string channel, string payload, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(channel))
        {
            throw new ArgumentException("Notification channel is required.", nameof(channel));
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ArgumentException("Notification payload is required.", nameof(payload));
        }

        var normalizedChannel = channel.Trim().ToLowerInvariant();
        _logger.LogInformation("Notification dispatch requested for channel {Channel}", normalizedChannel);

        return normalizedChannel switch
        {
            "email" => DispatchEmailAsync(payload, ct),
            "template_email" => DispatchTemplateEmailAsync(payload, ct),
            "sms" => DispatchSmsAsync(payload, ct),
            "push" => DispatchPushAsync(payload, ct),
            "in_app" or "inapp" => DispatchInAppAsync(payload, ct),
            "multi" or "multi_channel" or "multi-channel" => DispatchMultiChannelAsync(payload, ct),
            "workflow" => DispatchWorkflowAsync(payload, ct),
            _ => throw new InvalidOperationException($"Unsupported notification channel: {channel}")
        };
    }

    public async Task DispatchBatchAsync(string channel, IEnumerable<string> payloads, CancellationToken ct = default)
    {
        if (payloads == null)
        {
            throw new ArgumentNullException(nameof(payloads));
        }

        var count = payloads is ICollection<string> collection ? collection.Count : payloads.Count();
        _logger.LogInformation("Notification batch dispatch requested for channel {Channel} (count {Count})", channel, count);

        foreach (var payload in payloads)
        {
            await DispatchAsync(channel, payload, ct);
        }
    }

    private async Task DispatchEmailAsync(string payload, CancellationToken ct)
    {
        var request = DeserializePayload<EmailNotificationRequest>(payload, "email");
        await _notificationPort.SendEmailAsync(request, ct);
    }

    private async Task DispatchTemplateEmailAsync(string payload, CancellationToken ct)
    {
        var request = DeserializePayload<TemplateEmailPayload>(payload, "template_email");
        await _notificationPort.SendTemplateEmailAsync(request.TemplateId, request.RecipientEmail, request.Data, ct);
    }

    private async Task DispatchSmsAsync(string payload, CancellationToken ct)
    {
        var request = DeserializePayload<SmsNotificationRequest>(payload, "sms");
        await _notificationPort.SendSmsAsync(request, ct);
    }

    private async Task DispatchPushAsync(string payload, CancellationToken ct)
    {
        var request = DeserializePayload<PushNotificationRequest>(payload, "push");
        await _notificationPort.SendPushAsync(request, ct);
    }

    private async Task DispatchInAppAsync(string payload, CancellationToken ct)
    {
        var request = DeserializePayload<InAppNotificationRequest>(payload, "in_app");
        await _notificationPort.SendInAppAsync(request, ct);
    }

    private async Task DispatchMultiChannelAsync(string payload, CancellationToken ct)
    {
        var request = DeserializePayload<MultiChannelNotificationRequest>(payload, "multi_channel");
        await _notificationPort.SendNotificationAsync(request, ct);
    }

    private async Task DispatchWorkflowAsync(string payload, CancellationToken ct)
    {
        var request = DeserializePayload<WorkflowNotificationPayload>(payload, "workflow");
        await _notificationPort.TriggerWorkflowAsync(request.WorkflowId, request.SubscriberId, request.Payload, ct);
    }

    private static TPayload DeserializePayload<TPayload>(string payload, string channel) where TPayload : class
    {
        try
        {
            var result = JsonSerializer.Deserialize<TPayload>(payload, JsonOptions);
            if (result == null)
            {
                throw new JsonException("Payload deserialized to null.");
            }

            return result;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid {channel} payload JSON.", ex);
        }
    }

    private sealed class TemplateEmailPayload
    {
        public string TemplateId { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;
        public JsonElement Data { get; set; }
    }

    private sealed class WorkflowNotificationPayload
    {
        public string WorkflowId { get; set; } = string.Empty;
        public string SubscriberId { get; set; } = string.Empty;
        public JsonElement Payload { get; set; }
    }
}
