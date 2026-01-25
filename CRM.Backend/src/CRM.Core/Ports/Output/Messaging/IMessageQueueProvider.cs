using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Core.Ports.Output.Messaging;

/// <summary>
/// Represents the type of message broker.
/// </summary>
public enum MessageBrokerType
{
    RabbitMq,
    Redis,
    Kafka,
    AzureServiceBus,
    AwsSqs,
    InMemory
}

/// <summary>
/// Represents message delivery guarantees.
/// </summary>
public enum DeliveryGuarantee
{
    /// <summary>
    /// Message may be delivered zero or more times.
    /// </summary>
    AtMostOnce,

    /// <summary>
    /// Message will be delivered one or more times.
    /// </summary>
    AtLeastOnce,

    /// <summary>
    /// Message will be delivered exactly once.
    /// </summary>
    ExactlyOnce
}

/// <summary>
/// Represents message priority levels.
/// </summary>
public enum MessagePriority
{
    Low = 0,
    Normal = 5,
    High = 10,
    Critical = 15
}

/// <summary>
/// Base interface for all message queue providers following hexagonal architecture.
/// This port defines the contract that message queue adapters must implement.
/// </summary>
public interface IMessageQueueProvider : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Gets the type of the message broker.
    /// </summary>
    MessageBrokerType BrokerType { get; }

    /// <summary>
    /// Gets the provider name as a string.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets whether the provider is currently connected.
    /// </summary>
    bool IsConnected { get; }

    #region Connection Management

    /// <summary>
    /// Connects to the message broker.
    /// </summary>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects from the message broker.
    /// </summary>
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the connection to the message broker.
    /// </summary>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a health check.
    /// </summary>
    Task<MessageQueueHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Queue Operations

    /// <summary>
    /// Creates a queue if it doesn't exist.
    /// </summary>
    Task CreateQueueAsync(string queueName, QueueOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a queue.
    /// </summary>
    Task DeleteQueueAsync(string queueName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a queue exists.
    /// </summary>
    Task<bool> QueueExistsAsync(string queueName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets queue information.
    /// </summary>
    Task<QueueInfo> GetQueueInfoAsync(string queueName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all queues.
    /// </summary>
    Task<IEnumerable<QueueInfo>> GetQueuesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Purges all messages from a queue.
    /// </summary>
    Task<long> PurgeQueueAsync(string queueName, CancellationToken cancellationToken = default);

    #endregion

    #region Publishing

    /// <summary>
    /// Publishes a message to a queue.
    /// </summary>
    Task<string> PublishAsync<T>(string queueName, T message, PublishOptions? options = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Publishes a batch of messages to a queue.
    /// </summary>
    Task<IEnumerable<string>> PublishBatchAsync<T>(string queueName, IEnumerable<T> messages, PublishOptions? options = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Publishes a message with a delay.
    /// </summary>
    Task<string> PublishDelayedAsync<T>(string queueName, T message, TimeSpan delay, PublishOptions? options = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Schedules a message for a specific time.
    /// </summary>
    Task<string> ScheduleAsync<T>(string queueName, T message, DateTimeOffset scheduledTime, PublishOptions? options = null, CancellationToken cancellationToken = default) where T : class;

    #endregion

    #region Consuming

    /// <summary>
    /// Subscribes to a queue with a message handler.
    /// </summary>
    Task<ISubscription> SubscribeAsync<T>(string queueName, Func<T, MessageContext, Task<MessageResult>> handler, SubscribeOptions? options = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Receives a single message from a queue (pull-based).
    /// </summary>
    Task<ReceivedMessage<T>?> ReceiveAsync<T>(string queueName, TimeSpan? timeout = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Receives a batch of messages from a queue.
    /// </summary>
    Task<IEnumerable<ReceivedMessage<T>>> ReceiveBatchAsync<T>(string queueName, int maxMessages, TimeSpan? timeout = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Acknowledges a message.
    /// </summary>
    Task AcknowledgeAsync(string messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rejects a message (returns to queue or dead-letter).
    /// </summary>
    Task RejectAsync(string messageId, bool requeue = false, CancellationToken cancellationToken = default);

    #endregion

    #region Dead Letter Queue

    /// <summary>
    /// Gets messages from the dead-letter queue.
    /// </summary>
    Task<IEnumerable<ReceivedMessage<T>>> GetDeadLetterMessagesAsync<T>(string queueName, int maxMessages = 100, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Moves a message from dead-letter queue back to the main queue.
    /// </summary>
    Task RequeueDeadLetterAsync(string queueName, string messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Purges the dead-letter queue.
    /// </summary>
    Task<long> PurgeDeadLetterQueueAsync(string queueName, CancellationToken cancellationToken = default);

    #endregion
}

#region Supporting Interfaces

/// <summary>
/// Represents an active subscription to a queue.
/// </summary>
public interface ISubscription : IAsyncDisposable
{
    /// <summary>
    /// Gets the subscription identifier.
    /// </summary>
    string SubscriptionId { get; }

    /// <summary>
    /// Gets the queue name.
    /// </summary>
    string QueueName { get; }

    /// <summary>
    /// Gets whether the subscription is active.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Pauses message consumption.
    /// </summary>
    Task PauseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes message consumption.
    /// </summary>
    Task ResumeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the subscription.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);
}

#endregion

#region Message Types

/// <summary>
/// Result of a message handler execution.
/// </summary>
public enum MessageResult
{
    /// <summary>
    /// Message was processed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// Message processing failed, retry later.
    /// </summary>
    Retry,

    /// <summary>
    /// Message processing failed, move to dead-letter.
    /// </summary>
    DeadLetter,

    /// <summary>
    /// Message should be ignored and removed.
    /// </summary>
    Discard
}

/// <summary>
/// Context information for message processing.
/// </summary>
public record MessageContext(
    string MessageId,
    string QueueName,
    DateTimeOffset EnqueuedTime,
    int DeliveryCount,
    IDictionary<string, object> Headers,
    string? CorrelationId,
    string? ReplyTo
);

/// <summary>
/// A received message with its payload and context.
/// </summary>
public record ReceivedMessage<T>(
    T Payload,
    MessageContext Context
) where T : class;

#endregion

#region Options and Configuration

/// <summary>
/// Options for queue creation.
/// </summary>
public record QueueOptions
{
    /// <summary>
    /// Whether the queue should be durable (survives broker restart).
    /// </summary>
    public bool Durable { get; init; } = true;

    /// <summary>
    /// Whether the queue should auto-delete when last consumer disconnects.
    /// </summary>
    public bool AutoDelete { get; init; } = false;

    /// <summary>
    /// Maximum number of messages in the queue.
    /// </summary>
    public long? MaxLength { get; init; }

    /// <summary>
    /// Maximum size of the queue in bytes.
    /// </summary>
    public long? MaxSizeBytes { get; init; }

    /// <summary>
    /// Message time-to-live.
    /// </summary>
    public TimeSpan? MessageTtl { get; init; }

    /// <summary>
    /// Dead-letter queue name.
    /// </summary>
    public string? DeadLetterQueue { get; init; }

    /// <summary>
    /// Maximum number of delivery attempts.
    /// </summary>
    public int? MaxDeliveryAttempts { get; init; }
}

/// <summary>
/// Options for publishing messages.
/// </summary>
public record PublishOptions
{
    /// <summary>
    /// Message priority.
    /// </summary>
    public MessagePriority Priority { get; init; } = MessagePriority.Normal;

    /// <summary>
    /// Correlation ID for request-response patterns.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Reply-to queue for request-response patterns.
    /// </summary>
    public string? ReplyTo { get; init; }

    /// <summary>
    /// Message time-to-live.
    /// </summary>
    public TimeSpan? TimeToLive { get; init; }

    /// <summary>
    /// Custom headers.
    /// </summary>
    public IDictionary<string, object>? Headers { get; init; }

    /// <summary>
    /// Whether the message should be persisted.
    /// </summary>
    public bool Persistent { get; init; } = true;
}

/// <summary>
/// Options for subscribing to a queue.
/// </summary>
public record SubscribeOptions
{
    /// <summary>
    /// Number of messages to prefetch.
    /// </summary>
    public int PrefetchCount { get; init; } = 10;

    /// <summary>
    /// Whether to auto-acknowledge messages.
    /// </summary>
    public bool AutoAcknowledge { get; init; } = false;

    /// <summary>
    /// Maximum concurrent message handlers.
    /// </summary>
    public int MaxConcurrency { get; init; } = 1;

    /// <summary>
    /// Consumer tag/name.
    /// </summary>
    public string? ConsumerTag { get; init; }
}

#endregion

#region Queue Information

/// <summary>
/// Information about a queue.
/// </summary>
public record QueueInfo(
    string Name,
    long MessageCount,
    long ConsumerCount,
    bool Durable,
    bool AutoDelete,
    long? MaxLength,
    long? SizeBytes,
    DateTime? CreatedAt
);

/// <summary>
/// Result of a health check.
/// </summary>
public record MessageQueueHealthResult(
    bool IsHealthy,
    string Status,
    TimeSpan ResponseTime,
    string? ErrorMessage = null
);

#endregion

/// <summary>
/// Factory interface for creating message queue providers.
/// </summary>
public interface IMessageQueueProviderFactory
{
    /// <summary>
    /// Creates a message queue provider based on the specified type.
    /// </summary>
    IMessageQueueProvider CreateProvider(MessageBrokerType brokerType, string connectionString);

    /// <summary>
    /// Creates a message queue provider from configuration.
    /// </summary>
    IMessageQueueProvider CreateFromConfiguration();

    /// <summary>
    /// Gets all supported broker types.
    /// </summary>
    IEnumerable<MessageBrokerType> GetSupportedBrokers();
}
