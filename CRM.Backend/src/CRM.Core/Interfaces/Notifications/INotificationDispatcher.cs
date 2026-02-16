// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

namespace CRM.Core.Interfaces.Notifications;

public interface INotificationDispatcher
{
    Task DispatchAsync(string channel, string payload, CancellationToken ct = default);
    Task DispatchBatchAsync(string channel, IEnumerable<string> payloads, CancellationToken ct = default);
}
