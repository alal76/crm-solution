using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Core.Ports.Output.Messaging;

/// <summary>
/// Redis specific message queue and cache provider interface.
/// Extends the base IMessageQueueProvider with Redis-specific operations.
/// Redis supports both messaging (Pub/Sub, Streams) and caching operations.
/// </summary>
public interface IRedisProvider : IMessageQueueProvider
{
    #region Redis Pub/Sub

    /// <summary>
    /// Publishes a message to a Redis channel (Pub/Sub).
    /// </summary>
    Task<long> PublishToChannelAsync<T>(string channel, T message, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Subscribes to a Redis channel (Pub/Sub).
    /// </summary>
    Task<ISubscription> SubscribeToChannelAsync<T>(string channel, Func<T, MessageContext, Task<MessageResult>> handler, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Subscribes to channels matching a pattern.
    /// </summary>
    Task<ISubscription> SubscribeToPatternAsync<T>(string pattern, Func<string, T, MessageContext, Task<MessageResult>> handler, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Unsubscribes from a channel.
    /// </summary>
    Task UnsubscribeFromChannelAsync(string channel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the number of subscribers for a channel.
    /// </summary>
    Task<long> GetChannelSubscribersAsync(string channel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists active channels matching a pattern.
    /// </summary>
    Task<IEnumerable<string>> ListChannelsAsync(string? pattern = null, CancellationToken cancellationToken = default);

    #endregion

    #region Redis Streams

    /// <summary>
    /// Adds a message to a Redis Stream.
    /// </summary>
    Task<string> StreamAddAsync<T>(string streamName, T message, string? messageId = null, StreamAddOptions? options = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Reads messages from a Redis Stream.
    /// </summary>
    Task<IEnumerable<StreamMessage<T>>> StreamReadAsync<T>(string streamName, string? fromId = null, int count = 100, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Creates a consumer group for a stream.
    /// </summary>
    Task CreateConsumerGroupAsync(string streamName, string groupName, string? startId = null, bool createStream = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads messages as part of a consumer group.
    /// </summary>
    Task<IEnumerable<StreamMessage<T>>> StreamReadGroupAsync<T>(string streamName, string groupName, string consumerName, int count = 100, TimeSpan? blockTimeout = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Acknowledges a message in a consumer group.
    /// </summary>
    Task<long> StreamAcknowledgeAsync(string streamName, string groupName, params string[] messageIds);

    /// <summary>
    /// Claims pending messages from another consumer.
    /// </summary>
    Task<IEnumerable<StreamMessage<T>>> StreamClaimAsync<T>(string streamName, string groupName, string consumerName, TimeSpan minIdleTime, params string[] messageIds) where T : class;

    /// <summary>
    /// Gets pending messages information.
    /// </summary>
    Task<StreamPendingInfo> StreamPendingAsync(string streamName, string groupName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets stream information.
    /// </summary>
    Task<StreamInfo> StreamInfoAsync(string streamName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Trims a stream to a maximum length.
    /// </summary>
    Task<long> StreamTrimAsync(string streamName, long maxLength, bool approximate = true, CancellationToken cancellationToken = default);

    #endregion

    #region Redis Lists (as Queues)

    /// <summary>
    /// Pushes to the left (head) of a list.
    /// </summary>
    Task<long> ListLeftPushAsync<T>(string key, T value, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Pushes to the right (tail) of a list.
    /// </summary>
    Task<long> ListRightPushAsync<T>(string key, T value, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Pops from the left (head) of a list.
    /// </summary>
    Task<T?> ListLeftPopAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Pops from the right (tail) of a list.
    /// </summary>
    Task<T?> ListRightPopAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Blocking pop from left with timeout.
    /// </summary>
    Task<T?> BlockingListLeftPopAsync<T>(string key, TimeSpan timeout, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Blocking pop from right with timeout.
    /// </summary>
    Task<T?> BlockingListRightPopAsync<T>(string key, TimeSpan timeout, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets the length of a list.
    /// </summary>
    Task<long> ListLengthAsync(string key, CancellationToken cancellationToken = default);

    #endregion

    #region Caching Operations

    /// <summary>
    /// Gets a value from the cache.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a value in the cache.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a value only if the key doesn't exist.
    /// </summary>
    Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets or sets a value using a factory function.
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Deletes a key.
    /// </summary>
    Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple keys.
    /// </summary>
    Task<long> DeleteAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a key exists.
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the expiration on a key.
    /// </summary>
    Task<bool> ExpireAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the time-to-live for a key.
    /// </summary>
    Task<TimeSpan?> TimeToLiveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increments a value.
    /// </summary>
    Task<long> IncrementAsync(string key, long value = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// Decrements a value.
    /// </summary>
    Task<long> DecrementAsync(string key, long value = 1, CancellationToken cancellationToken = default);

    #endregion

    #region Hash Operations

    /// <summary>
    /// Sets a hash field.
    /// </summary>
    Task HashSetAsync(string key, string field, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets multiple hash fields.
    /// </summary>
    Task HashSetAsync(string key, IDictionary<string, string> fields, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a hash field value.
    /// </summary>
    Task<string?> HashGetAsync(string key, string field, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all hash fields.
    /// </summary>
    Task<IDictionary<string, string>> HashGetAllAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a hash field.
    /// </summary>
    Task<bool> HashDeleteAsync(string key, string field, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a hash field exists.
    /// </summary>
    Task<bool> HashExistsAsync(string key, string field, CancellationToken cancellationToken = default);

    #endregion

    #region Distributed Locking

    /// <summary>
    /// Acquires a distributed lock.
    /// </summary>
    Task<IDistributedLock?> AcquireLockAsync(string lockKey, TimeSpan expiry, TimeSpan? waitTime = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extends a lock's expiration.
    /// </summary>
    Task<bool> ExtendLockAsync(string lockKey, string lockValue, TimeSpan expiry, CancellationToken cancellationToken = default);

    #endregion

    #region Server Information

    /// <summary>
    /// Gets Redis server information.
    /// </summary>
    Task<RedisServerInfo> GetServerInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets memory usage statistics.
    /// </summary>
    Task<RedisMemoryStats> GetMemoryStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cluster information (if in cluster mode).
    /// </summary>
    Task<RedisClusterInfo?> GetClusterInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Pings the Redis server.
    /// </summary>
    Task<TimeSpan> PingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all keys matching a pattern.
    /// </summary>
    Task<IEnumerable<string>> GetKeysAsync(string pattern = "*", int count = 1000, CancellationToken cancellationToken = default);

    /// <summary>
    /// Flushes the current database.
    /// </summary>
    Task FlushDatabaseAsync(CancellationToken cancellationToken = default);

    #endregion
}

#region Redis Specific Types

/// <summary>
/// Options for adding to a stream.
/// </summary>
public record StreamAddOptions
{
    /// <summary>
    /// Maximum length of the stream (auto-trim).
    /// </summary>
    public long? MaxLength { get; init; }

    /// <summary>
    /// Whether to use approximate trimming.
    /// </summary>
    public bool ApproximateTrimming { get; init; } = true;
}

/// <summary>
/// A message from a Redis Stream.
/// </summary>
public record StreamMessage<T>(
    string MessageId,
    T Payload,
    IDictionary<string, string> Fields
) where T : class;

/// <summary>
/// Pending messages information.
/// </summary>
public record StreamPendingInfo(
    long PendingCount,
    string? SmallestId,
    string? GreatestId,
    IDictionary<string, long> ConsumerPendingCounts
);

/// <summary>
/// Stream information.
/// </summary>
public record StreamInfo(
    long Length,
    int RadixTreeKeys,
    int RadixTreeNodes,
    int Groups,
    string LastGeneratedId,
    string? FirstEntryId,
    string? LastEntryId
);

/// <summary>
/// Distributed lock interface.
/// </summary>
public interface IDistributedLock : IAsyncDisposable
{
    string LockKey { get; }
    string LockValue { get; }
    bool IsAcquired { get; }
    Task<bool> ExtendAsync(TimeSpan expiry, CancellationToken cancellationToken = default);
    Task ReleaseAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Redis server information.
/// </summary>
public record RedisServerInfo(
    string Version,
    string Mode,
    int ConnectedClients,
    long UsedMemory,
    long UsedMemoryPeak,
    long TotalSystemMemory,
    double UsedCpuSys,
    double UsedCpuUser,
    long TotalConnectionsReceived,
    long TotalCommandsProcessed,
    int KeyspaceHits,
    int KeyspaceMisses,
    DateTime ServerTime
);

/// <summary>
/// Redis memory statistics.
/// </summary>
public record RedisMemoryStats(
    long UsedMemory,
    long UsedMemoryRss,
    long UsedMemoryPeak,
    long UsedMemoryLua,
    double MemoryFragmentationRatio,
    string Allocator
);

/// <summary>
/// Redis cluster information.
/// </summary>
public record RedisClusterInfo(
    bool IsClusterEnabled,
    int ClusterKnownNodes,
    int ClusterSize,
    int ClusterSlotsAssigned,
    int ClusterSlotsOk,
    int ClusterSlotsPfail,
    int ClusterSlotsFail,
    string ClusterState
);

#endregion
