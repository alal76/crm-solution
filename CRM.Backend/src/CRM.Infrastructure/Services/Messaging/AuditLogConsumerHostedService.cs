// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Diagnostics;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CRM.Infrastructure.Services.Messaging;

/// <summary>
/// Background service that consumes audit events from the Redis Stream (crm:audit:stream)
/// and batch-writes them to the AuditLogs database table.
///
/// Design (FLAG-005):
/// - Reads up to the configured batch size entries per poll using XREADGROUP.
/// - Flushes to the DB every flush interval or when the buffer is full.
/// - Acknowledges (XACK) messages only after a successful DB write so that no audit event
///   is lost: if the DB write fails, messages remain pending and are retried on the next poll.
/// - If Redis is unavailable, the service exits immediately — <see cref="OptionalAuditLoggingService"/>
///   will fall back to synchronous DB writes in that case.
/// </summary>
public sealed class AuditLogConsumerHostedService : BackgroundService
{
    internal const string StreamName = "crm:audit:stream";
    internal const string GroupName = "crm-audit-consumers";
    internal const string ConsumerName = "crm-audit-consumer-1";

    // Default values — overridable for testing
    private readonly int _batchSize;
    private readonly TimeSpan _flushInterval;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<AuditLogConsumerHostedService> _logger;

    public AuditLogConsumerHostedService(
        IServiceScopeFactory scopeFactory,
        IConnectionMultiplexer? redis,
        ILogger<AuditLogConsumerHostedService> logger,
        int batchSize = 100,
        TimeSpan? flushInterval = null)
    {
        _scopeFactory = scopeFactory;
        _redis = redis;
        _logger = logger;
        _batchSize = batchSize;
        _flushInterval = flushInterval ?? TimeSpan.FromSeconds(5);
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_redis == null || !_redis.IsConnected)
        {
            _logger.LogWarning(
                "Redis is unavailable — {Service} will not start. " +
                "OptionalAuditLoggingService will fall back to synchronous DB writes.",
                nameof(AuditLogConsumerHostedService));
            return;
        }

        await EnsureConsumerGroupAsync(stoppingToken);

        _logger.LogInformation(
            "{Service} started. Stream={Stream}, Group={Group}, Consumer={Consumer}",
            nameof(AuditLogConsumerHostedService), StreamName, GroupName, ConsumerName);

        var db = _redis.GetDatabase();
        var buffer = new List<(string MessageId, AuditLog Entry)>(_batchSize);
        var flushTimer = Stopwatch.StartNew();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var entries = await db.StreamReadGroupAsync(
                    StreamName, GroupName, ConsumerName,
                    position: ">",
                    count: _batchSize - buffer.Count);

                foreach (var entry in entries)
                {
                    var auditLog = ParseStreamEntry(entry);
                    if (auditLog != null)
                    {
                        buffer.Add((entry.Id.ToString(), auditLog));
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Skipping malformed audit stream entry {MessageId}", entry.Id);
                        // Acknowledge malformed entries so they don't block the stream
                        await db.StreamAcknowledgeAsync(StreamName, GroupName, entry.Id);
                    }
                }

                var shouldFlush = buffer.Count >= _batchSize
                    || (buffer.Count > 0 && flushTimer.Elapsed >= _flushInterval);

                if (shouldFlush)
                {
                    await FlushBatchAsync(db, buffer, stoppingToken);
                    buffer.Clear();
                    flushTimer.Restart();
                }
                else if (entries.Length == 0)
                {
                    // No new messages — back-off to avoid busy-wait
                    await Task.Delay(500, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Service} poll loop; backing off 5s", nameof(AuditLogConsumerHostedService));
                await Task.Delay(5_000, stoppingToken);
            }
        }

        // Final flush on graceful shutdown
        if (buffer.Count > 0)
        {
            _logger.LogInformation("Flushing {Count} remaining audit events on shutdown", buffer.Count);
            using var shutdownCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await FlushBatchAsync(db, buffer, shutdownCts.Token);
        }

        _logger.LogInformation("{Service} stopped", nameof(AuditLogConsumerHostedService));
    }

    /// <summary>
    /// Writes the buffer to AuditLogs and acknowledges each entry in the Redis Stream.
    /// If the DB write fails, no entries are acknowledged so they are retried on the next poll.
    /// </summary>
    private async Task FlushBatchAsync(IDatabase db, List<(string MessageId, AuditLog Entry)> batch, CancellationToken ct)
    {
        if (batch.Count == 0)
        {
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ICrmDbContext>();

            context.AuditLogs.AddRange(batch.Select(b => b.Entry));
            await context.SaveChangesAsync(ct);

            // Acknowledge all entries only after the DB write succeeds
            foreach (var (messageId, _) in batch)
            {
                await db.StreamAcknowledgeAsync(StreamName, GroupName, messageId);
            }

            _logger.LogDebug(
                "Flushed and acknowledged {Count} audit events to the DB",
                batch.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "DB write failed for {Count} audit events — entries remain pending in {Stream} for retry",
                batch.Count, StreamName);
            // Intentionally do NOT acknowledge: messages stay pending and will be reprocessed.
        }
    }

    /// <summary>
    /// Deserializes a <see cref="StreamEntry"/> into an <see cref="AuditLog"/> entity.
    /// Returns <c>null</c> if the entry is missing required fields.
    /// </summary>
    internal static AuditLog? ParseStreamEntry(StreamEntry entry)
    {
        try
        {
            var fields = entry.Values.ToDictionary(
                v => v.Name.ToString(),
                v => v.Value.ToString() ?? string.Empty,
                StringComparer.OrdinalIgnoreCase);

            // "action" is the only required field
            if (!fields.TryGetValue("action", out var action) || string.IsNullOrWhiteSpace(action))
            {
                return null;
            }

            return new AuditLog
            {
                UserId = fields.TryGetValue("userId", out var uid) && int.TryParse(uid, out var userId) && userId > 0
                    ? userId
                    : null,
                Action = action,
                EntityType = fields.GetValueOrDefault("entityType") is { Length: > 0 } et ? et : null,
                EntityId = fields.TryGetValue("entityId", out var eid) && int.TryParse(eid, out var entityId) && entityId > 0
                    ? entityId
                    : null,
                OldValues = fields.GetValueOrDefault("oldValues") is { Length: > 0 } ov ? ov : null,
                NewValues = fields.GetValueOrDefault("newValues") is { Length: > 0 } nv ? nv : null,
                Details = fields.GetValueOrDefault("reason") is { Length: > 0 } r ? r : null,
                IpAddress = fields.GetValueOrDefault("ipAddress") is { Length: > 0 } ip ? ip : null,
                UserAgent = fields.GetValueOrDefault("userAgent") is { Length: > 0 } ua ? ua : null,
                CreatedAt = fields.TryGetValue("timestamp", out var ts)
                    && DateTime.TryParse(ts, null, System.Globalization.DateTimeStyles.RoundtripKind, out var time)
                    ? time
                    : DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Creates the consumer group, creating the stream itself if it does not yet exist.
    /// Idempotent — a BUSYGROUP error (group already exists) is silently ignored.
    /// </summary>
    private async Task EnsureConsumerGroupAsync(CancellationToken ct)
    {
        try
        {
            var db = _redis!.GetDatabase();
            // position "0" = process all messages from the beginning of the stream
            await db.StreamCreateConsumerGroupAsync(StreamName, GroupName, "0", createStream: true);
            _logger.LogInformation(
                "Consumer group {Group} ensured on stream {Stream}", GroupName, StreamName);
        }
        catch (RedisException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            // Expected on service restart — group already exists
            _logger.LogDebug(
                "Consumer group {Group} already exists on stream {Stream}", GroupName, StreamName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not create consumer group {Group} on stream {Stream}", GroupName, StreamName);
        }
    }
}
