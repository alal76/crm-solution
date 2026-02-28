// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Core.Scripting.MultiAgent;

/// <summary>
/// Episodic memory model for agents.
/// SARCH-084: Short-term session memory + long-term episodic memory across sessions.
/// </summary>
public class AgentEpisode
{
    public string EpisodeId { get; init; } = Guid.NewGuid().ToString("N");
    public string AgentId { get; init; } = string.Empty;
    public string SessionId { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<EpisodeEvent> Events { get; init; } = new();
    public Dictionary<string, object?> LearnedFacts { get; init; } = new();
    public List<string> Tags { get; init; } = new();
}

public class EpisodeEvent
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string EventType { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string? ToolName { get; init; }
}

public interface IAgentMemoryStore
{
    Task SaveEpisodeAsync(AgentEpisode episode, CancellationToken ct = default);
    Task<IReadOnlyList<AgentEpisode>> GetRecentEpisodesAsync(string agentId, int maxCount = 10, CancellationToken ct = default);
    Task<IReadOnlyList<AgentEpisode>> SearchEpisodesAsync(string agentId, string query, CancellationToken ct = default);
}

/// <summary>In-memory episodic memory store (production: use vector DB or SQL).</summary>
public class InMemoryAgentMemoryStore : IAgentMemoryStore
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, List<AgentEpisode>> _episodes = new();

    public Task SaveEpisodeAsync(AgentEpisode episode, CancellationToken ct = default)
    {
        var list = _episodes.GetOrAdd(episode.AgentId, _ => new List<AgentEpisode>());
        lock (list)
        {
            list.Add(episode);
            if (list.Count > 100)
                list.RemoveAt(0);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AgentEpisode>> GetRecentEpisodesAsync(string agentId, int maxCount = 10, CancellationToken ct = default)
    {
        if (!_episodes.TryGetValue(agentId, out var list))
            return Task.FromResult<IReadOnlyList<AgentEpisode>>(new List<AgentEpisode>());

        lock (list)
        {
            return Task.FromResult<IReadOnlyList<AgentEpisode>>(list.TakeLast(maxCount).ToList());
        }
    }

    public Task<IReadOnlyList<AgentEpisode>> SearchEpisodesAsync(string agentId, string query, CancellationToken ct = default)
    {
        if (!_episodes.TryGetValue(agentId, out var list))
            return Task.FromResult<IReadOnlyList<AgentEpisode>>(new List<AgentEpisode>());

        lock (list)
        {
            var results = list
                .Where(e => e.Summary.Contains(query, StringComparison.OrdinalIgnoreCase)
                    || e.Tags.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            return Task.FromResult<IReadOnlyList<AgentEpisode>>(results);
        }
    }
}
