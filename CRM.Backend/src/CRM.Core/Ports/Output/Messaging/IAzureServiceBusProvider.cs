using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Core.Ports.Output.Messaging;

/// <summary>
/// Azure Service Bus specific message provider interface.
/// Extends the base IMessageQueueProvider with Azure Service Bus-specific operations.
/// </summary>
public interface IAzureServiceBusProvider : IMessageQueueProvider
{
    #region Topic and Subscription Management

    /// <summary>
    /// Creates a topic.
    /// </summary>
    Task CreateTopicAsync(string topicName, ServiceBusTopicOptions? options = null, CancellationToken cancellationToken = default);

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
    Task<IEnumerable<ServiceBusTopicInfo>> GetTopicsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a subscription on a topic.
    /// </summary>
    Task CreateSubscriptionAsync(string topicName, string subscriptionName, ServiceBusSubscriptionOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a subscription.
    /// </summary>
    Task DeleteSubscriptionAsync(string topicName, string subscriptionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all subscriptions for a topic.
    /// </summary>
    Task<IEnumerable<ServiceBusSubscriptionInfo>> GetSubscriptionsAsync(string topicName, CancellationToken cancellationToken = default);

    #endregion

    #region Rules and Filters

    /// <summary>
    /// Creates a rule (filter) on a subscription.
    /// </summary>
    Task CreateRuleAsync(string topicName, string subscriptionName, string ruleName, ServiceBusRule rule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a rule from a subscription.
    /// </summary>
    Task DeleteRuleAsync(string topicName, string subscriptionName, string ruleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all rules for a subscription.
    /// </summary>
    Task<IEnumerable<ServiceBusRuleInfo>> GetRulesAsync(string topicName, string subscriptionName, CancellationToken cancellationToken = default);

    #endregion

    #region Topic Publishing

    /// <summary>
    /// Publishes a message to a topic.
    /// </summary>
    Task<string> PublishToTopicAsync<T>(string topicName, T message, ServiceBusMessageOptions? options = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Publishes a batch of messages to a topic.
    /// </summary>
    Task<IEnumerable<string>> PublishBatchToTopicAsync<T>(string topicName, IEnumerable<T> messages, ServiceBusMessageOptions? options = null, CancellationToken cancellationToken = default) where T : class;

    #endregion

    #region Subscription Consumption

    /// <summary>
    /// Subscribes to a topic subscription.
    /// </summary>
    Task<ISubscription> SubscribeToTopicAsync<T>(string topicName, string subscriptionName, Func<T, MessageContext, Task<MessageResult>> handler, ServiceBusReceiveOptions? options = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Receives messages from a subscription.
    /// </summary>
    Task<IEnumerable<ReceivedMessage<T>>> ReceiveFromSubscriptionAsync<T>(string topicName, string subscriptionName, int maxMessages = 10, TimeSpan? maxWaitTime = null, CancellationToken cancellationToken = default) where T : class;

    #endregion

    #region Sessions

    /// <summary>
    /// Subscribes to a session-enabled queue.
    /// </summary>
    Task<IServiceBusSession> AcceptSessionAsync(string queueName, string? sessionId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to a session-enabled subscription.
    /// </summary>
    Task<IServiceBusSession> AcceptTopicSessionAsync(string topicName, string subscriptionName, string? sessionId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available session IDs.
    /// </summary>
    Task<IEnumerable<string>> GetSessionIdsAsync(string queueName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a message with a session ID.
    /// </summary>
    Task<string> PublishWithSessionAsync<T>(string queueName, T message, string sessionId, ServiceBusMessageOptions? options = null, CancellationToken cancellationToken = default) where T : class;

    #endregion

    #region Message Management

    /// <summary>
    /// Peeks at messages without removing them.
    /// </summary>
    Task<IEnumerable<ReceivedMessage<T>>> PeekMessagesAsync<T>(string queueName, int maxMessages = 10, long? fromSequenceNumber = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Defers a message.
    /// </summary>
    Task DeferMessageAsync(string queueName, long sequenceNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Receives a deferred message.
    /// </summary>
    Task<ReceivedMessage<T>?> ReceiveDeferredMessageAsync<T>(string queueName, long sequenceNumber, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Cancels a scheduled message.
    /// </summary>
    Task CancelScheduledMessageAsync(string queueName, long sequenceNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes a message.
    /// </summary>
    Task CompleteMessageAsync(string queueName, string lockToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Abandons a message (returns to queue).
    /// </summary>
    Task AbandonMessageAsync(string queueName, string lockToken, IDictionary<string, object>? propertiesToModify = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dead-letters a message.
    /// </summary>
    Task DeadLetterMessageAsync(string queueName, string lockToken, string? reason = null, string? description = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renews the lock on a message.
    /// </summary>
    Task<DateTimeOffset> RenewMessageLockAsync(string queueName, string lockToken, CancellationToken cancellationToken = default);

    #endregion

    #region Namespace Management

    /// <summary>
    /// Gets namespace information.
    /// </summary>
    Task<ServiceBusNamespaceInfo> GetNamespaceInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all queues in the namespace.
    /// </summary>
    Task<IEnumerable<ServiceBusQueueInfo>> GetAllQueuesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets queue runtime properties.
    /// </summary>
    Task<ServiceBusQueueRuntimeInfo> GetQueueRuntimeInfoAsync(string queueName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets topic runtime properties.
    /// </summary>
    Task<ServiceBusTopicRuntimeInfo> GetTopicRuntimeInfoAsync(string topicName, CancellationToken cancellationToken = default);

    #endregion

    #region Premium Features

    /// <summary>
    /// Checks if the namespace is Premium tier.
    /// </summary>
    Task<bool> IsPremiumTierAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets messaging units (Premium tier).
    /// </summary>
    Task<int> GetMessagingUnitsAsync(CancellationToken cancellationToken = default);

    #endregion
}

#region Azure Service Bus Specific Types

/// <summary>
/// Session interface for Service Bus sessions.
/// </summary>
public interface IServiceBusSession : IAsyncDisposable
{
    string SessionId { get; }
    DateTimeOffset LockedUntil { get; }

    Task<T?> ReceiveMessageAsync<T>(TimeSpan? maxWaitTime = null, CancellationToken cancellationToken = default) where T : class;
    Task<IEnumerable<T>> ReceiveMessagesAsync<T>(int maxMessages, TimeSpan? maxWaitTime = null, CancellationToken cancellationToken = default) where T : class;
    Task CompleteMessageAsync(string lockToken, CancellationToken cancellationToken = default);
    Task AbandonMessageAsync(string lockToken, CancellationToken cancellationToken = default);
    Task<DateTimeOffset> RenewSessionLockAsync(CancellationToken cancellationToken = default);
    Task<byte[]> GetSessionStateAsync(CancellationToken cancellationToken = default);
    Task SetSessionStateAsync(byte[] state, CancellationToken cancellationToken = default);
}

/// <summary>
/// Options for creating a topic.
/// </summary>
public record ServiceBusTopicOptions
{
    public long? MaxSizeInMegabytes { get; init; }
    public TimeSpan? DefaultMessageTimeToLive { get; init; }
    public bool? EnableBatchedOperations { get; init; }
    public TimeSpan? DuplicateDetectionHistoryTimeWindow { get; init; }
    public bool? RequiresDuplicateDetection { get; init; }
    public bool? SupportOrdering { get; init; }
    public bool? EnablePartitioning { get; init; }
}

/// <summary>
/// Options for creating a subscription.
/// </summary>
public record ServiceBusSubscriptionOptions
{
    public TimeSpan? DefaultMessageTimeToLive { get; init; }
    public TimeSpan? LockDuration { get; init; }
    public int? MaxDeliveryCount { get; init; }
    public bool? DeadLetteringOnMessageExpiration { get; init; }
    public bool? EnableBatchedOperations { get; init; }
    public bool? RequiresSession { get; init; }
    public string? ForwardTo { get; init; }
    public string? ForwardDeadLetteredMessagesTo { get; init; }
}

/// <summary>
/// Service Bus rule definition.
/// </summary>
public record ServiceBusRule
{
    public ServiceBusFilterType FilterType { get; init; }
    public string? SqlFilter { get; init; }
    public string? CorrelationId { get; init; }
    public string? Subject { get; init; }
    public string? To { get; init; }
    public string? ReplyTo { get; init; }
    public IDictionary<string, object>? ApplicationProperties { get; init; }
    public string? SqlAction { get; init; }
}

/// <summary>
/// Service Bus filter types.
/// </summary>
public enum ServiceBusFilterType
{
    SqlFilter,
    CorrelationFilter,
    TrueFilter,
    FalseFilter
}

/// <summary>
/// Rule information.
/// </summary>
public record ServiceBusRuleInfo(
    string Name,
    ServiceBusFilterType FilterType,
    string? FilterExpression
);

/// <summary>
/// Options for sending messages.
/// </summary>
public record ServiceBusMessageOptions
{
    public string? MessageId { get; init; }
    public string? CorrelationId { get; init; }
    public string? Subject { get; init; }
    public string? To { get; init; }
    public string? ReplyTo { get; init; }
    public string? ReplyToSessionId { get; init; }
    public string? ContentType { get; init; }
    public TimeSpan? TimeToLive { get; init; }
    public string? PartitionKey { get; init; }
    public string? TransactionPartitionKey { get; init; }
    public IDictionary<string, object>? ApplicationProperties { get; init; }
}

/// <summary>
/// Options for receiving messages.
/// </summary>
public record ServiceBusReceiveOptions
{
    public ServiceBusReceiveMode ReceiveMode { get; init; } = ServiceBusReceiveMode.PeekLock;
    public int PrefetchCount { get; init; } = 0;
    public int MaxConcurrentCalls { get; init; } = 1;
    public TimeSpan? MaxAutoLockRenewalDuration { get; init; }
    public bool AutoCompleteMessages { get; init; } = true;
}

/// <summary>
/// Receive modes.
/// </summary>
public enum ServiceBusReceiveMode
{
    PeekLock,
    ReceiveAndDelete
}

/// <summary>
/// Topic information.
/// </summary>
public record ServiceBusTopicInfo(
    string Name,
    long SizeInBytes,
    int SubscriptionCount,
    ServiceBusEntityStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

/// <summary>
/// Subscription information.
/// </summary>
public record ServiceBusSubscriptionInfo(
    string Name,
    string TopicName,
    long MessageCount,
    long DeadLetterMessageCount,
    int MaxDeliveryCount,
    ServiceBusEntityStatus Status
);

/// <summary>
/// Queue information.
/// </summary>
public record ServiceBusQueueInfo(
    string Name,
    long SizeInBytes,
    ServiceBusEntityStatus Status,
    bool RequiresSession,
    bool RequiresDuplicateDetection,
    DateTimeOffset CreatedAt
);

/// <summary>
/// Entity status.
/// </summary>
public enum ServiceBusEntityStatus
{
    Active,
    Creating,
    Deleting,
    Disabled,
    ReceiveDisabled,
    SendDisabled,
    Unknown
}

/// <summary>
/// Namespace information.
/// </summary>
public record ServiceBusNamespaceInfo(
    string Name,
    string Location,
    string Sku,
    int MessagingUnits,
    DateTimeOffset CreatedAt
);

/// <summary>
/// Queue runtime information.
/// </summary>
public record ServiceBusQueueRuntimeInfo(
    string Name,
    long ActiveMessageCount,
    long DeadLetterMessageCount,
    long ScheduledMessageCount,
    long TransferMessageCount,
    long TransferDeadLetterMessageCount,
    long SizeInBytes,
    DateTimeOffset AccessedAt
);

/// <summary>
/// Topic runtime information.
/// </summary>
public record ServiceBusTopicRuntimeInfo(
    string Name,
    long SizeInBytes,
    int SubscriptionCount,
    long ScheduledMessageCount,
    DateTimeOffset AccessedAt
);

#endregion
