using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Core.Ports.Output.Messaging;

/// <summary>
/// Apache Kafka specific message provider interface.
/// Extends the base IMessageQueueProvider with Kafka-specific operations.
/// </summary>
public interface IKafkaProvider : IMessageQueueProvider
{
    #region Topic Management

    /// <summary>
    /// Creates a topic.
    /// </summary>
    Task CreateTopicAsync(string topicName, TopicConfiguration? config = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a topic.
    /// </summary>
    Task DeleteTopicAsync(string topicName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a topic exists.
    /// </summary>
    Task<bool> TopicExistsAsync(string topicName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all topics.
    /// </summary>
    Task<IEnumerable<TopicInfo>> GetTopicsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets topic metadata.
    /// </summary>
    Task<TopicMetadata> GetTopicMetadataAsync(string topicName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increases the number of partitions for a topic.
    /// </summary>
    Task IncreasePartitionsAsync(string topicName, int totalPartitions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates topic configuration.
    /// </summary>
    Task UpdateTopicConfigAsync(string topicName, IDictionary<string, string> configs, CancellationToken cancellationToken = default);

    #endregion

    #region Producer Operations

    /// <summary>
    /// Produces a message to a topic with a key.
    /// </summary>
    Task<ProduceResult> ProduceAsync<TKey, TValue>(string topic, TKey key, TValue value, ProduceOptions? options = null, CancellationToken cancellationToken = default) 
        where TKey : class 
        where TValue : class;

    /// <summary>
    /// Produces a message to a specific partition.
    /// </summary>
    Task<ProduceResult> ProduceToPartitionAsync<TKey, TValue>(string topic, int partition, TKey key, TValue value, ProduceOptions? options = null, CancellationToken cancellationToken = default) 
        where TKey : class 
        where TValue : class;

    /// <summary>
    /// Produces multiple messages in a batch.
    /// </summary>
    Task<IEnumerable<ProduceResult>> ProduceBatchAsync<TKey, TValue>(string topic, IEnumerable<(TKey Key, TValue Value)> messages, ProduceOptions? options = null, CancellationToken cancellationToken = default) 
        where TKey : class 
        where TValue : class;

    /// <summary>
    /// Flushes any pending messages.
    /// </summary>
    Task FlushAsync(TimeSpan timeout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a transaction.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits a transaction.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Aborts a transaction.
    /// </summary>
    Task AbortTransactionAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Consumer Operations

    /// <summary>
    /// Subscribes to a topic with a consumer group.
    /// </summary>
    Task<IKafkaSubscription> SubscribeAsync<TKey, TValue>(string topic, string groupId, Func<ConsumeResult<TKey, TValue>, Task<MessageResult>> handler, ConsumeOptions? options = null, CancellationToken cancellationToken = default) 
        where TKey : class 
        where TValue : class;

    /// <summary>
    /// Subscribes to multiple topics with a consumer group.
    /// </summary>
    Task<IKafkaSubscription> SubscribeAsync<TKey, TValue>(IEnumerable<string> topics, string groupId, Func<ConsumeResult<TKey, TValue>, Task<MessageResult>> handler, ConsumeOptions? options = null, CancellationToken cancellationToken = default) 
        where TKey : class 
        where TValue : class;

    /// <summary>
    /// Assigns specific partitions to consume from.
    /// </summary>
    Task<IKafkaSubscription> AssignPartitionsAsync<TKey, TValue>(string topic, IEnumerable<int> partitions, Func<ConsumeResult<TKey, TValue>, Task<MessageResult>> handler, ConsumeOptions? options = null, CancellationToken cancellationToken = default) 
        where TKey : class 
        where TValue : class;

    /// <summary>
    /// Seeks to a specific offset.
    /// </summary>
    Task SeekAsync(string topic, int partition, long offset, CancellationToken cancellationToken = default);

    /// <summary>
    /// Seeks to the beginning of a partition.
    /// </summary>
    Task SeekToBeginningAsync(string topic, int partition, CancellationToken cancellationToken = default);

    /// <summary>
    /// Seeks to the end of a partition.
    /// </summary>
    Task SeekToEndAsync(string topic, int partition, CancellationToken cancellationToken = default);

    /// <summary>
    /// Seeks to a specific timestamp.
    /// </summary>
    Task<IEnumerable<TopicPartitionOffset>> SeekToTimestampAsync(string topic, DateTimeOffset timestamp, CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits offsets manually.
    /// </summary>
    Task CommitAsync(IEnumerable<TopicPartitionOffset> offsets, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current consumer positions.
    /// </summary>
    Task<IEnumerable<TopicPartitionOffset>> GetPositionsAsync(string topic, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the committed offsets for a consumer group.
    /// </summary>
    Task<IEnumerable<TopicPartitionOffset>> GetCommittedOffsetsAsync(string topic, string groupId, CancellationToken cancellationToken = default);

    #endregion

    #region Consumer Groups

    /// <summary>
    /// Lists all consumer groups.
    /// </summary>
    Task<IEnumerable<ConsumerGroupInfo>> ListConsumerGroupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Describes a consumer group.
    /// </summary>
    Task<ConsumerGroupDescription> DescribeConsumerGroupAsync(string groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a consumer group.
    /// </summary>
    Task DeleteConsumerGroupAsync(string groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the lag for a consumer group.
    /// </summary>
    Task<IEnumerable<ConsumerLag>> GetConsumerLagAsync(string groupId, string topic, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets consumer group offsets.
    /// </summary>
    Task ResetOffsetsAsync(string groupId, string topic, OffsetResetStrategy strategy, CancellationToken cancellationToken = default);

    #endregion

    #region Admin Operations

    /// <summary>
    /// Gets cluster metadata.
    /// </summary>
    Task<ClusterMetadata> GetClusterMetadataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets broker information.
    /// </summary>
    Task<IEnumerable<BrokerInfo>> GetBrokersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Describes cluster configuration.
    /// </summary>
    Task<IDictionary<string, string>> DescribeClusterConfigAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the controller broker.
    /// </summary>
    Task<BrokerInfo> GetControllerAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Schema Registry (if integrated)

    /// <summary>
    /// Gets schema by ID.
    /// </summary>
    Task<string?> GetSchemaAsync(int schemaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a schema for a subject.
    /// </summary>
    Task<int> RegisterSchemaAsync(string subject, string schema, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest schema for a subject.
    /// </summary>
    Task<SchemaInfo?> GetLatestSchemaAsync(string subject, CancellationToken cancellationToken = default);

    #endregion
}

#region Kafka Specific Types

/// <summary>
/// Kafka subscription with additional controls.
/// </summary>
public interface IKafkaSubscription : ISubscription
{
    /// <summary>
    /// Gets assigned partitions.
    /// </summary>
    IEnumerable<TopicPartition> AssignedPartitions { get; }

    /// <summary>
    /// Commits the current offset.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits a specific offset.
    /// </summary>
    Task CommitAsync(TopicPartitionOffset offset, CancellationToken cancellationToken = default);
}

/// <summary>
/// Topic configuration.
/// </summary>
public record TopicConfiguration
{
    public int Partitions { get; init; } = 1;
    public short ReplicationFactor { get; init; } = 1;
    public TimeSpan? RetentionTime { get; init; }
    public long? RetentionBytes { get; init; }
    public string? CleanupPolicy { get; init; }
    public int? MinInSyncReplicas { get; init; }
    public IDictionary<string, string>? AdditionalConfigs { get; init; }
}

/// <summary>
/// Topic information.
/// </summary>
public record TopicInfo(
    string Name,
    int PartitionCount,
    int ReplicationFactor,
    bool IsInternal
);

/// <summary>
/// Topic metadata.
/// </summary>
public record TopicMetadata(
    string Name,
    IEnumerable<PartitionMetadata> Partitions,
    IDictionary<string, string> Configs
);

/// <summary>
/// Partition metadata.
/// </summary>
public record PartitionMetadata(
    int PartitionId,
    int Leader,
    IEnumerable<int> Replicas,
    IEnumerable<int> InSyncReplicas,
    long LowWatermark,
    long HighWatermark
);

/// <summary>
/// Topic partition identifier.
/// </summary>
public record TopicPartition(string Topic, int Partition);

/// <summary>
/// Topic partition with offset.
/// </summary>
public record TopicPartitionOffset(string Topic, int Partition, long Offset);

/// <summary>
/// Options for producing messages.
/// </summary>
public record ProduceOptions
{
    public IDictionary<string, byte[]>? Headers { get; init; }
    public DateTimeOffset? Timestamp { get; init; }
    public int? PartitionKey { get; init; }
    public bool RequireAck { get; init; } = true;
    public TimeSpan? Timeout { get; init; }
}

/// <summary>
/// Result of producing a message.
/// </summary>
public record ProduceResult(
    string Topic,
    int Partition,
    long Offset,
    DateTimeOffset Timestamp,
    bool Persisted
);

/// <summary>
/// Options for consuming messages.
/// </summary>
public record ConsumeOptions
{
    public OffsetResetStrategy AutoOffsetReset { get; init; } = OffsetResetStrategy.Earliest;
    public bool EnableAutoCommit { get; init; } = true;
    public TimeSpan? AutoCommitInterval { get; init; }
    public int? MaxPollRecords { get; init; }
    public TimeSpan? SessionTimeout { get; init; }
    public TimeSpan? HeartbeatInterval { get; init; }
    public int MaxPartitionFetchBytes { get; init; } = 1048576;
}

/// <summary>
/// Offset reset strategy.
/// </summary>
public enum OffsetResetStrategy
{
    Earliest,
    Latest,
    None
}

/// <summary>
/// Result of consuming a message.
/// </summary>
public record ConsumeResult<TKey, TValue>(
    string Topic,
    int Partition,
    long Offset,
    TKey Key,
    TValue Value,
    DateTimeOffset Timestamp,
    IDictionary<string, byte[]>? Headers
) where TKey : class where TValue : class;

/// <summary>
/// Consumer group information.
/// </summary>
public record ConsumerGroupInfo(
    string GroupId,
    string State,
    string ProtocolType
);

/// <summary>
/// Consumer group description.
/// </summary>
public record ConsumerGroupDescription(
    string GroupId,
    string State,
    string Protocol,
    string ProtocolType,
    string Coordinator,
    IEnumerable<ConsumerMemberInfo> Members
);

/// <summary>
/// Consumer member information.
/// </summary>
public record ConsumerMemberInfo(
    string MemberId,
    string ClientId,
    string Host,
    IEnumerable<TopicPartition> Assignments
);

/// <summary>
/// Consumer lag information.
/// </summary>
public record ConsumerLag(
    string Topic,
    int Partition,
    long CurrentOffset,
    long EndOffset,
    long Lag
);

/// <summary>
/// Kafka cluster metadata.
/// </summary>
public record ClusterMetadata(
    string ClusterId,
    int ControllerId,
    int BrokerCount,
    int TopicCount
);

/// <summary>
/// Broker information.
/// </summary>
public record BrokerInfo(
    int BrokerId,
    string Host,
    int Port,
    string? Rack,
    bool IsController
);

/// <summary>
/// Schema information.
/// </summary>
public record SchemaInfo(
    int SchemaId,
    string Subject,
    int Version,
    string Schema,
    string SchemaType
);

#endregion
