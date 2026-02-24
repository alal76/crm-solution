// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CRM.Infrastructure.Services.Messaging;

/// <summary>
/// Interface for dead letter queue handling.
/// Manages messages that failed processing beyond retry limits.
/// TODO-INFRA-06
/// </summary>
public interface IDeadLetterQueueService
{
    /// <summary>
    /// Moves a failed message to the dead letter queue.
    /// </summary>
    Task EnqueueAsync(DeadLetterMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all messages in the dead letter queue.
    /// </summary>
    Task<IEnumerable<DeadLetterMessage>> GetMessagesAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of messages in the dead letter queue.
    /// </summary>
    Task<long> GetCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retries processing a specific dead letter message.
    /// </summary>
    Task<bool> RetryAsync(string messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retries all messages in the dead letter queue.
    /// </summary>
    Task<int> RetryAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a message from the dead letter queue.
    /// </summary>
    Task<bool> RemoveAsync(string messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Purges all messages from the dead letter queue.
    /// </summary>
    Task<long> PurgeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets dead letter messages by source stream.
    /// </summary>
    Task<IEnumerable<DeadLetterMessage>> GetBySourceAsync(string sourceStream, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a message that failed processing.
/// </summary>
public class DeadLetterMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SourceStream { get; set; } = string.Empty;
    public string OriginalMessageId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 3;
    public DateTime FailedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastRetryAt { get; set; }
    public string? CorrelationId { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
}

/// <summary>
/// Redis-backed dead letter queue service.
/// Stores failed messages in a Redis sorted set for inspection and retry.
/// </summary>
public class DeadLetterQueueService : IDeadLetterQueueService
{
    private const string DlqKeyPrefix = "crm:dlq:";
    private const string DlqIndexKey = "crm:dlq:index";

    private readonly IConnectionMultiplexer? _redis;
    private readonly IRedisStreamService _streamService;
    private readonly ILogger<DeadLetterQueueService> _logger;

    // In-memory fallback when Redis is not available
    private static readonly List<DeadLetterMessage> _inMemoryQueue = new();
    private static readonly object _lock = new();

    public DeadLetterQueueService(
        IConnectionMultiplexer? redis,
        IRedisStreamService streamService,
        ILogger<DeadLetterQueueService> logger)
    {
        _redis = redis;
        _streamService = streamService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task EnqueueAsync(DeadLetterMessage message, CancellationToken cancellationToken = default)
    {
        message.FailedAt = DateTime.UtcNow;

        if (_redis == null || !_redis.IsConnected)
        {
            lock (_lock)
            {
                _inMemoryQueue.Add(message);
            }
            _logger.LogWarning(
                "Redis unavailable. Dead letter message {Id} from {Source} stored in-memory",
                message.Id, message.SourceStream);
            return;
        }

        try
        {
            var db = _redis.GetDatabase();
            var json = System.Text.Json.JsonSerializer.Serialize(message);
            var key = $"{DlqKeyPrefix}{message.Id}";

            await db.StringSetAsync(key, json, TimeSpan.FromDays(30));
            await db.SortedSetAddAsync(DlqIndexKey,
                message.Id, message.FailedAt.Ticks);

            _logger.LogWarning(
                "Message {OriginalId} from stream {Source} moved to DLQ as {DlqId}. Error: {Error}",
                message.OriginalMessageId, message.SourceStream, message.Id, message.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue dead letter message {Id}", message.Id);
            lock (_lock)
            {
                _inMemoryQueue.Add(message);
            }
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DeadLetterMessage>> GetMessagesAsync(
        int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected)
        {
            lock (_lock)
            {
                return _inMemoryQueue
                    .OrderByDescending(m => m.FailedAt)
                    .Skip(skip).Take(take).ToList();
            }
        }

        var db = _redis.GetDatabase();
        var ids = await db.SortedSetRangeByRankAsync(
            DlqIndexKey, skip, skip + take - 1, StackExchange.Redis.Order.Descending);

        var messages = new List<DeadLetterMessage>();
        foreach (var id in ids)
        {
            var json = await db.StringGetAsync($"{DlqKeyPrefix}{id}");
            if (json.HasValue)
            {
                var msg = System.Text.Json.JsonSerializer.Deserialize<DeadLetterMessage>(json.ToString());
                if (msg != null) messages.Add(msg);
            }
        }

        return messages;
    }

    /// <inheritdoc />
    public async Task<long> GetCountAsync(CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected)
        {
            lock (_lock) { return _inMemoryQueue.Count; }
        }

        var db = _redis.GetDatabase();
        return await db.SortedSetLengthAsync(DlqIndexKey);
    }

    /// <inheritdoc />
    public async Task<bool> RetryAsync(string messageId, CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected) return false;

        var db = _redis.GetDatabase();
        var json = await db.StringGetAsync($"{DlqKeyPrefix}{messageId}");
        if (!json.HasValue) return false;

        var message = System.Text.Json.JsonSerializer.Deserialize<DeadLetterMessage>(json.ToString());
        if (message == null) return false;

        // Re-publish to original stream
        var data = new Dictionary<string, string>
        {
            ["payload"] = message.Payload,
            ["dlqRetry"] = "true",
            ["dlqMessageId"] = message.Id
        };

        await _streamService.PublishAsync(
            message.SourceStream, message.EventType, data, cancellationToken);

        // Remove from DLQ
        await RemoveAsync(messageId, cancellationToken);

        _logger.LogInformation(
            "Retried dead letter message {Id} back to stream {Stream}",
            messageId, message.SourceStream);

        return true;
    }

    /// <inheritdoc />
    public async Task<int> RetryAllAsync(CancellationToken cancellationToken = default)
    {
        var messages = await GetMessagesAsync(0, 1000, cancellationToken);
        var retried = 0;

        foreach (var msg in messages)
        {
            if (await RetryAsync(msg.Id, cancellationToken))
                retried++;
        }

        _logger.LogInformation("Retried {Count} dead letter messages", retried);
        return retried;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(string messageId, CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected)
        {
            lock (_lock) { return _inMemoryQueue.RemoveAll(m => m.Id == messageId) > 0; }
        }

        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync($"{DlqKeyPrefix}{messageId}");
        await db.SortedSetRemoveAsync(DlqIndexKey, messageId);
        return true;
    }

    /// <inheritdoc />
    public async Task<long> PurgeAsync(CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected)
        {
            lock (_lock)
            {
                var count = _inMemoryQueue.Count;
                _inMemoryQueue.Clear();
                return count;
            }
        }

        var db = _redis.GetDatabase();
        var totalCount = await db.SortedSetLengthAsync(DlqIndexKey);

        // Get all message IDs and delete them
        var ids = await db.SortedSetRangeByRankAsync(DlqIndexKey);
        foreach (var id in ids)
        {
            await db.KeyDeleteAsync($"{DlqKeyPrefix}{id}");
        }
        await db.KeyDeleteAsync(DlqIndexKey);

        _logger.LogInformation("Purged {Count} messages from dead letter queue", totalCount);
        return totalCount;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DeadLetterMessage>> GetBySourceAsync(
        string sourceStream, CancellationToken cancellationToken = default)
    {
        var all = await GetMessagesAsync(0, 1000, cancellationToken);
        return all.Where(m => m.SourceStream == sourceStream);
    }
}
