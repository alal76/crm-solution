// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Scripting.MultiAgent;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Scripting.MultiAgent;

/// <summary>
/// In-memory agent message bus for development and single-instance scenarios.
/// Production: swap for Redis Pub/Sub or Azure Service Bus implementation.
/// </summary>
public class InMemoryAgentMessageBus : IAgentMessageBus
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<AgentMessage>> _queues = new();
    private readonly ConcurrentDictionary<string, Func<AgentMessage, CancellationToken, Task>> _handlers = new();
    private readonly ILogger<InMemoryAgentMessageBus> _logger;

    public InMemoryAgentMessageBus(ILogger<InMemoryAgentMessageBus> logger) => _logger = logger;

    public async Task PublishAsync(AgentMessage message, CancellationToken ct = default)
    {
        _logger.LogDebug(
            "Agent message {Type}: {From} → {To}",
            message.MessageType,
            message.FromAgentId,
            message.ToAgentId);

        // Enqueue for polling consumers
        var queue = _queues.GetOrAdd(message.ToAgentId, _ => new ConcurrentQueue<AgentMessage>());
        queue.Enqueue(message);

        // Invoke registered handler if any
        if (_handlers.TryGetValue(message.ToAgentId, out var handler))
        {
            await handler(message, ct);
        }
    }

    public Task SubscribeAsync(
        string agentId,
        Func<AgentMessage, CancellationToken, Task> handler,
        CancellationToken ct = default)
    {
        _handlers[agentId] = handler;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AgentMessage>> GetPendingAsync(string agentId, CancellationToken ct = default)
    {
        var queue = _queues.GetOrAdd(agentId, _ => new ConcurrentQueue<AgentMessage>());
        var messages = new List<AgentMessage>();
        while (queue.TryDequeue(out var msg))
            messages.Add(msg);
        return Task.FromResult<IReadOnlyList<AgentMessage>>(messages);
    }
}
