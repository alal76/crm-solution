// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Core.Scripting.MultiAgent;

/// <summary>
/// Message envelope for inter-agent communication.
/// SARCH-083: Typed messaging between agents in a multi-agent session.
/// </summary>
public class AgentMessage
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString("N");
    public string FromAgentId { get; init; } = string.Empty;
    public string ToAgentId { get; init; } = string.Empty; // empty = broadcast
    public string SessionId { get; init; } = string.Empty;
    public string MessageType { get; init; } = string.Empty; // "task", "result", "status", "broadcast"
    public string Payload { get; init; } = string.Empty; // JSON payload
    public DateTime SentAt { get; init; } = DateTime.UtcNow;
    public int Priority { get; init; } = 0; // higher = more urgent
    public string? CorrelationId { get; init; } // trace multi-step exchanges
}

public interface IAgentMessageBus
{
    Task PublishAsync(AgentMessage message, CancellationToken ct = default);
    Task SubscribeAsync(string agentId, Func<AgentMessage, CancellationToken, Task> handler, CancellationToken ct = default);
    Task<IReadOnlyList<AgentMessage>> GetPendingAsync(string agentId, CancellationToken ct = default);
}
