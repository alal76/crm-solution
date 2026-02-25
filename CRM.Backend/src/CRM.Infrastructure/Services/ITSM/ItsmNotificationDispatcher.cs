// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces.ITSM;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Fans ITSM notifications out to all registered <see cref="IItsmNotificationChannel"/> implementations.
/// TODO-SD005-010: Slack/Teams integration dispatcher.
/// </summary>
public sealed class ItsmNotificationDispatcher : IItsmNotificationDispatcher
{
    private readonly IEnumerable<IItsmNotificationChannel> _channels;
    private readonly ILogger<ItsmNotificationDispatcher> _logger;

    public ItsmNotificationDispatcher(
        IEnumerable<IItsmNotificationChannel> channels,
        ILogger<ItsmNotificationDispatcher> logger)
    {
        _channels = channels;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task DispatchAsync(
        string title,
        string body,
        string urgency,
        int? serviceRequestId = null,
        CancellationToken ct = default)
    {
        var configured = _channels.Where(c => c.IsConfigured).ToList();
        if (configured.Count == 0)
        {
            _logger.LogDebug("No ITSM notification channels configured — skipping dispatch");
            return;
        }

        var tasks = configured.Select(channel => SendSafeAsync(channel, title, body, urgency, serviceRequestId, ct));
        await Task.WhenAll(tasks);
    }

    private async Task SendSafeAsync(
        IItsmNotificationChannel channel,
        string title,
        string body,
        string urgency,
        int? serviceRequestId,
        CancellationToken ct)
    {
        try
        {
            await channel.SendNotificationAsync(title, body, urgency, serviceRequestId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in ITSM notification channel {Channel}", channel.ChannelName);
        }
    }
}
