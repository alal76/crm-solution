// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

using CRM.Core.Interfaces.Notifications;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Workers;

public class NotificationWorker
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly ILogger<NotificationWorker> _logger;

    public NotificationWorker(INotificationDispatcher dispatcher, ILogger<NotificationWorker> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task DispatchAsync(string channel, IEnumerable<string> payloads, CancellationToken ct = default)
    {
        _logger.LogInformation("Notification worker dispatch started");
        await _dispatcher.DispatchBatchAsync(channel, payloads, ct);
        _logger.LogInformation("Notification worker dispatch completed");
    }
}
