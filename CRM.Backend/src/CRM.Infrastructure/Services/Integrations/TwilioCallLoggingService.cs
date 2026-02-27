// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Ports.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Integrations;

/// <summary>
/// Implementation of ITwilioCallLoggingService for Twilio voice call logging.
/// Logs inbound/outbound calls, updates statuses, and provides call history and statistics.
/// Implements TODO-INT-07.
///
/// Note: This is a stub implementation. External Twilio API calls throw NotImplementedException
/// and should be wired to the Twilio REST API client once the NuGet package is added.
/// </summary>
public class TwilioCallLoggingService : ITwilioCallLoggingService
{
    private readonly ILogger<TwilioCallLoggingService> _logger;

    // In-memory store for development/testing (replace with DbContext in production)
    private readonly List<CallLogEntry> _callLogs = new();
    private int _nextId = 1;

    public TwilioCallLoggingService(
        ILogger<TwilioCallLoggingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<int> LogInboundCallAsync(TwilioCallEvent callEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Logging inbound call {CallSid} from {From} to {To}",
            callEvent.CallSid, callEvent.From, callEvent.To);

        var entry = new CallLogEntry
        {
            Id = _nextId++,
            CallSid = callEvent.CallSid,
            From = callEvent.From,
            To = callEvent.To,
            Direction = "inbound",
            Status = callEvent.Status,
            Duration = callEvent.Duration,
            StartedAt = callEvent.Timestamp,
            RecordingUrl = callEvent.RecordingUrl
        };

        _callLogs.Add(entry);
        return Task.FromResult(entry.Id);
    }

    /// <inheritdoc />
    public Task<int> LogOutboundCallAsync(TwilioCallEvent callEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Logging outbound call {CallSid} from {From} to {To}",
            callEvent.CallSid, callEvent.From, callEvent.To);

        var entry = new CallLogEntry
        {
            Id = _nextId++,
            CallSid = callEvent.CallSid,
            From = callEvent.From,
            To = callEvent.To,
            Direction = "outbound",
            Status = callEvent.Status,
            Duration = callEvent.Duration,
            StartedAt = callEvent.Timestamp,
            RecordingUrl = callEvent.RecordingUrl
        };

        _callLogs.Add(entry);
        return Task.FromResult(entry.Id);
    }

    /// <inheritdoc />
    public Task UpdateCallStatusAsync(string callSid, string status, int? duration = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating call {CallSid} status to {Status}", callSid, status);

        var entry = _callLogs.FirstOrDefault(c => c.CallSid == callSid);
        if (entry != null)
        {
            // Records are immutable, so we replace
            var index = _callLogs.IndexOf(entry);
            _callLogs[index] = entry with
            {
                Status = status,
                Duration = duration ?? entry.Duration,
                EndedAt = status is "completed" or "failed" or "no-answer" or "busy" ? DateTime.UtcNow : entry.EndedAt
            };
        }
        else
        {
            _logger.LogWarning("Call {CallSid} not found for status update", callSid);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<CallLogEntry>> GetCallHistoryAsync(
        string phoneNumber,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _callLogs.AsEnumerable()
            .Where(c => c.From == phoneNumber || c.To == phoneNumber);

        if (startDate.HasValue)
            query = query.Where(c => c.StartedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(c => c.StartedAt <= endDate.Value);

        return Task.FromResult<IReadOnlyList<CallLogEntry>>(query.OrderByDescending(c => c.StartedAt).ToList());
    }

    /// <inheritdoc />
    public Task LinkCallToEntityAsync(string callSid, string entityType, int entityId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Linking call {CallSid} to {EntityType}#{EntityId}", callSid, entityType, entityId);

        var entry = _callLogs.FirstOrDefault(c => c.CallSid == callSid);
        if (entry != null)
        {
            var index = _callLogs.IndexOf(entry);
            _callLogs[index] = entry with
            {
                LinkedEntityType = entityType,
                LinkedEntityId = entityId
            };
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<CallStatistics> GetCallStatisticsAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var calls = _callLogs.Where(c => c.StartedAt >= startDate && c.StartedAt <= endDate).ToList();

        return Task.FromResult(new CallStatistics
        {
            TotalCalls = calls.Count,
            InboundCalls = calls.Count(c => c.Direction == "inbound"),
            OutboundCalls = calls.Count(c => c.Direction == "outbound"),
            AverageDurationSeconds = calls.Where(c => c.Duration.HasValue).Select(c => (double)c.Duration!.Value).DefaultIfEmpty().Average(),
            TotalDurationSeconds = calls.Where(c => c.Duration.HasValue).Sum(c => c.Duration!.Value),
            MissedCalls = calls.Count(c => c.Status is "no-answer" or "busy"),
            LinkedCalls = calls.Count(c => c.LinkedEntityId.HasValue),
            PeriodStart = startDate,
            PeriodEnd = endDate
        });
    }
}
