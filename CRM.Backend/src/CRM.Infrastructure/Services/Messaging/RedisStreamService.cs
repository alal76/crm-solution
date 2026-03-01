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
/// Abstraction for Redis Streams pub/sub for async event processing.
/// Provides reliable message delivery with consumer groups and acknowledgement.
/// TODO-INFRA-04
/// </summary>
public interface IRedisStreamService
{
    /// <summary>
    /// Publishes a message to a Redis stream.
    /// </summary>
    Task<string> PublishAsync(string streamName, string eventType, Dictionary<string, string> data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to a Redis stream using a consumer group.
    /// </summary>
    Task SubscribeAsync(string streamName, string groupName, string consumerName, Func<StreamEntry, Task> handler, CancellationToken cancellationToken = default);

    /// <summary>
    /// Acknowledges a message in a consumer group.
    /// </summary>
    Task AcknowledgeAsync(string streamName, string groupName, string messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending messages for a consumer group.
    /// </summary>
    Task<IEnumerable<StreamPendingMessageInfo>> GetPendingMessagesAsync(string streamName, string groupName, int count = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a consumer group for a stream.
    /// </summary>
    Task<bool> CreateConsumerGroupAsync(string streamName, string groupName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the length of a stream.
    /// </summary>
    Task<long> GetStreamLengthAsync(string streamName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Trims a stream to a maximum length.
    /// </summary>
    Task TrimStreamAsync(string streamName, int maxLength, CancellationToken cancellationToken = default);
}

/// <summary>
/// Redis Streams implementation for async event processing.
/// Uses StackExchange.Redis for reliable pub/sub with consumer groups.
/// </summary>
public class RedisStreamService : IRedisStreamService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<RedisStreamService> _logger;

    public RedisStreamService(
        IConnectionMultiplexer? redis,
        ILogger<RedisStreamService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> PublishAsync(
        string streamName,
        string eventType,
        Dictionary<string, string> data,
        CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected)
        {
            _logger.LogWarning("Redis not available. Message to stream {Stream} dropped", streamName);
            return string.Empty;
        }

        try
        {
            var db = _redis.GetDatabase();
            var entries = new List<NameValueEntry>
            {
                new("eventType", eventType),
                new("timestamp", DateTime.UtcNow.ToString("O")),
                new("correlationId", Guid.NewGuid().ToString())
            };

            foreach (var kvp in data)
            {
                entries.Add(new NameValueEntry(kvp.Key, kvp.Value));
            }

            var messageId = await db.StreamAddAsync(
                streamName,
                entries.ToArray());

            _logger.LogDebug(
                "Published message {MessageId} to stream {Stream} with type {EventType}",
                messageId, streamName, eventType);

            return messageId.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to stream {Stream}", streamName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SubscribeAsync(
        string streamName,
        string groupName,
        string consumerName,
        Func<StreamEntry, Task> handler,
        CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected)
        {
            _logger.LogWarning("Redis not available. Cannot subscribe to stream {Stream}", streamName);
            return;
        }

        await CreateConsumerGroupAsync(streamName, groupName, cancellationToken);

        var db = _redis.GetDatabase();

        _logger.LogInformation(
            "Starting consumer {Consumer} in group {Group} on stream {Stream}",
            consumerName, groupName, streamName);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var entries = await db.StreamReadGroupAsync(
                    streamName,
                    groupName,
                    consumerName,
                    ">", // Read only new messages
                    count: 10);

                if (entries.Length > 0)
                {
                    foreach (var entry in entries)
                    {
                        try
                        {
                            await handler(entry);
                            await db.StreamAcknowledgeAsync(streamName, groupName, entry.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "Error processing message {MessageId} from stream {Stream}",
                                entry.Id, streamName);
                            // Message remains pending for retry
                        }
                    }
                }
                else
                {
                    // No messages — wait before polling again
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading from stream {Stream}", streamName);
                await Task.Delay(5000, cancellationToken);
            }
        }

        _logger.LogInformation(
            "Consumer {Consumer} in group {Group} on stream {Stream} stopped",
            consumerName, groupName, streamName);
    }

    /// <inheritdoc />
    public async Task AcknowledgeAsync(
        string streamName,
        string groupName,
        string messageId,
        CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected) return;

        var db = _redis.GetDatabase();
        await db.StreamAcknowledgeAsync(streamName, groupName, messageId);

        _logger.LogDebug("Acknowledged message {MessageId} in group {Group} on stream {Stream}",
            messageId, groupName, streamName);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<StreamPendingMessageInfo>> GetPendingMessagesAsync(
        string streamName,
        string groupName,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected)
        {
            return Enumerable.Empty<StreamPendingMessageInfo>();
        }

        var db = _redis.GetDatabase();
        var pending = await db.StreamPendingMessagesAsync(
            streamName, groupName, count, RedisValue.Null);

        return pending;
    }

    /// <inheritdoc />
    public async Task<bool> CreateConsumerGroupAsync(
        string streamName,
        string groupName,
        CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected) return false;

        try
        {
            var db = _redis.GetDatabase();

            // Ensure stream exists
            if (!await db.KeyExistsAsync(streamName))
            {
                await db.StreamAddAsync(streamName,
                    new NameValueEntry[] { new("init", "true") });
            }

            await db.StreamCreateConsumerGroupAsync(
                streamName, groupName, "0-0");

            _logger.LogInformation(
                "Created consumer group {Group} on stream {Stream}",
                groupName, streamName);
            return true;
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            // Group already exists — not an error
            _logger.LogDebug("Consumer group {Group} already exists on stream {Stream}",
                groupName, streamName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create consumer group {Group} on stream {Stream}",
                groupName, streamName);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<long> GetStreamLengthAsync(
        string streamName,
        CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected) return 0;

        var db = _redis.GetDatabase();
        return await db.StreamLengthAsync(streamName);
    }

    /// <inheritdoc />
    public async Task TrimStreamAsync(
        string streamName,
        int maxLength,
        CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected) return;

        var db = _redis.GetDatabase();
        await db.StreamTrimAsync(streamName, maxLength);

        _logger.LogDebug("Trimmed stream {Stream} to max {MaxLength} entries",
            streamName, maxLength);
    }
}
