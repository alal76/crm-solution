using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Core.Ports.Output.Messaging;

/// <summary>
/// RabbitMQ specific message queue provider interface.
/// Extends the base IMessageQueueProvider with RabbitMQ-specific operations.
/// </summary>
public interface IRabbitMqProvider : IMessageQueueProvider
{
    #region Exchange Management

    /// <summary>
    /// Declares an exchange.
    /// </summary>
    Task DeclareExchangeAsync(string exchangeName, RabbitMqExchangeType type, ExchangeOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an exchange.
    /// </summary>
    Task DeleteExchangeAsync(string exchangeName, bool ifUnused = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an exchange exists.
    /// </summary>
    Task<bool> ExchangeExistsAsync(string exchangeName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Binds an exchange to another exchange.
    /// </summary>
    Task BindExchangeAsync(string sourceExchange, string destinationExchange, string routingKey = "", IDictionary<string, object>? arguments = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unbinds an exchange from another exchange.
    /// </summary>
    Task UnbindExchangeAsync(string sourceExchange, string destinationExchange, string routingKey = "", CancellationToken cancellationToken = default);

    #endregion

    #region Queue Bindings

    /// <summary>
    /// Binds a queue to an exchange.
    /// </summary>
    Task BindQueueAsync(string queueName, string exchangeName, string routingKey = "", IDictionary<string, object>? arguments = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unbinds a queue from an exchange.
    /// </summary>
    Task UnbindQueueAsync(string queueName, string exchangeName, string routingKey = "", CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the bindings for a queue.
    /// </summary>
    Task<IEnumerable<QueueBinding>> GetQueueBindingsAsync(string queueName, CancellationToken cancellationToken = default);

    #endregion

    #region Publishing with Routing

    /// <summary>
    /// Publishes a message to an exchange with a routing key.
    /// </summary>
    Task<string> PublishToExchangeAsync<T>(string exchangeName, string routingKey, T message, PublishOptions? options = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Publishes a message and waits for confirmation.
    /// </summary>
    Task<bool> PublishWithConfirmAsync<T>(string exchangeName, string routingKey, T message, TimeSpan timeout, PublishOptions? options = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Enables publisher confirms on the channel.
    /// </summary>
    Task EnablePublisherConfirmsAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Topic and Headers Routing

    /// <summary>
    /// Subscribes to messages matching a topic pattern.
    /// </summary>
    Task<ISubscription> SubscribeToTopicAsync<T>(string exchangeName, string topicPattern, Func<T, MessageContext, Task<MessageResult>> handler, SubscribeOptions? options = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Subscribes to messages matching specific headers.
    /// </summary>
    Task<ISubscription> SubscribeToHeadersAsync<T>(string exchangeName, IDictionary<string, object> headers, bool matchAll, Func<T, MessageContext, Task<MessageResult>> handler, SubscribeOptions? options = null, CancellationToken cancellationToken = default) where T : class;

    #endregion

    #region RPC (Request-Reply Pattern)

    /// <summary>
    /// Sends an RPC request and waits for a response.
    /// </summary>
    Task<TResponse?> RpcCallAsync<TRequest, TResponse>(string queueName, TRequest request, TimeSpan timeout, CancellationToken cancellationToken = default) 
        where TRequest : class 
        where TResponse : class;

    /// <summary>
    /// Registers an RPC server handler.
    /// </summary>
    Task<ISubscription> RegisterRpcServerAsync<TRequest, TResponse>(string queueName, Func<TRequest, CancellationToken, Task<TResponse>> handler, CancellationToken cancellationToken = default) 
        where TRequest : class 
        where TResponse : class;

    #endregion

    #region Management API

    /// <summary>
    /// Gets server information.
    /// </summary>
    Task<RabbitMqServerInfo> GetServerInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all exchanges.
    /// </summary>
    Task<IEnumerable<ExchangeInfo>> GetExchangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets connections to the broker.
    /// </summary>
    Task<IEnumerable<RabbitMqConnection>> GetConnectionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets channels on the current connection.
    /// </summary>
    Task<IEnumerable<RabbitMqChannel>> GetChannelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets consumers for a queue.
    /// </summary>
    Task<IEnumerable<RabbitMqConsumer>> GetConsumersAsync(string? queueName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cluster nodes.
    /// </summary>
    Task<IEnumerable<RabbitMqNode>> GetClusterNodesAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Policies and Shovel

    /// <summary>
    /// Creates or updates a policy.
    /// </summary>
    Task SetPolicyAsync(string policyName, string pattern, PolicyDefinition definition, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a policy.
    /// </summary>
    Task DeletePolicyAsync(string policyName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all policies.
    /// </summary>
    Task<IEnumerable<RabbitMqPolicy>> GetPoliciesAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Virtual Hosts

    /// <summary>
    /// Gets all virtual hosts.
    /// </summary>
    Task<IEnumerable<RabbitMqVirtualHost>> GetVirtualHostsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current virtual host.
    /// </summary>
    string CurrentVirtualHost { get; }

    #endregion
}

#region RabbitMQ Specific Types

/// <summary>
/// RabbitMQ exchange types.
/// </summary>
public enum RabbitMqExchangeType
{
    Direct,
    Fanout,
    Topic,
    Headers,
    ConsistentHash
}

/// <summary>
/// Options for exchange declaration.
/// </summary>
public record ExchangeOptions
{
    public bool Durable { get; init; } = true;
    public bool AutoDelete { get; init; } = false;
    public bool Internal { get; init; } = false;
    public IDictionary<string, object>? Arguments { get; init; }
    public string? AlternateExchange { get; init; }
}

/// <summary>
/// Queue binding information.
/// </summary>
public record QueueBinding(
    string QueueName,
    string ExchangeName,
    string RoutingKey,
    IDictionary<string, object>? Arguments
);

/// <summary>
/// Exchange information.
/// </summary>
public record ExchangeInfo(
    string Name,
    RabbitMqExchangeType Type,
    bool Durable,
    bool AutoDelete,
    bool Internal
);

/// <summary>
/// RabbitMQ server information.
/// </summary>
public record RabbitMqServerInfo(
    string Version,
    string ErlangVersion,
    string ClusterName,
    int NodeCount,
    long MessageCount,
    long ConsumerCount,
    long ConnectionCount
);

/// <summary>
/// RabbitMQ connection information.
/// </summary>
public record RabbitMqConnection(
    string Name,
    string Host,
    int Port,
    string Username,
    string State,
    int Channels,
    DateTime ConnectedAt
);

/// <summary>
/// RabbitMQ channel information.
/// </summary>
public record RabbitMqChannel(
    int Number,
    string ConnectionName,
    string State,
    int PrefetchCount,
    int ConsumerCount,
    long MessagesUnacknowledged,
    bool Confirm
);

/// <summary>
/// RabbitMQ consumer information.
/// </summary>
public record RabbitMqConsumer(
    string ConsumerTag,
    string QueueName,
    string ChannelDetails,
    bool Ack,
    bool Exclusive,
    int PrefetchCount
);

/// <summary>
/// RabbitMQ cluster node information.
/// </summary>
public record RabbitMqNode(
    string Name,
    string Type,
    bool Running,
    long MemoryUsed,
    long DiskFree,
    int FileDescriptorsUsed,
    int SocketsUsed
);

/// <summary>
/// Policy definition.
/// </summary>
public record PolicyDefinition
{
    public string? HaMode { get; init; }
    public int? HaParams { get; init; }
    public string? HaSyncMode { get; init; }
    public long? MessageTtl { get; init; }
    public long? MaxLength { get; init; }
    public string? DeadLetterExchange { get; init; }
    public string? DeadLetterRoutingKey { get; init; }
    public int? DeliveryLimit { get; init; }
}

/// <summary>
/// RabbitMQ policy information.
/// </summary>
public record RabbitMqPolicy(
    string Name,
    string Pattern,
    string ApplyTo,
    int Priority,
    PolicyDefinition Definition
);

/// <summary>
/// RabbitMQ virtual host information.
/// </summary>
public record RabbitMqVirtualHost(
    string Name,
    string Description,
    long MessageCount,
    long QueueCount,
    bool Tracing
);

#endregion
