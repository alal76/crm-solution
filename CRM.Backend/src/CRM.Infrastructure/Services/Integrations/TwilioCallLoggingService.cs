// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Ports.Input;
using CRM.Infrastructure.Providers.Twilio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using TwilioClient = Twilio.TwilioClient;

namespace CRM.Infrastructure.Services.Integrations;

/// <summary>
/// Implementation of ITwilioCallLoggingService for Twilio voice call logging.
/// Logs inbound/outbound calls, updates statuses, and provides call history and statistics.
/// Implements TODO-INT-07.
///
/// Call records are enriched with authoritative data fetched from the Twilio REST API
/// (<see cref="CallResource.FetchAsync(string, string, Twilio.Clients.ITwilioRestClient)"/>)
/// by Call SID whenever the client is initialized and not running in test mode, falling back
/// to the caller-supplied <see cref="TwilioCallEvent"/> data if the fetch fails or is skipped.
///
/// Note: Call records are stored in-memory for development/testing. Replace with a DbContext-backed
/// store for production persistence.
/// </summary>
public class TwilioCallLoggingService : ITwilioCallLoggingService
{
    private readonly TwilioConfiguration _config;
    private readonly ILogger<TwilioCallLoggingService> _logger;
    private readonly bool _isInitialized;

    // In-memory store for development/testing (replace with DbContext in production)
    private readonly List<CallLogEntry> _callLogs = new();
    private int _nextId = 1;

    public TwilioCallLoggingService(
        IOptions<TwilioConfiguration> options,
        ILogger<TwilioCallLoggingService> logger)
    {
        _config = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _isInitialized = TryInitializeTwilioClient();
    }

    private bool TryInitializeTwilioClient()
    {
        if (!_config.IsValid())
        {
            _logger.LogWarning(
                "Twilio configuration is invalid. Call log entries will use webhook-supplied data only.");
            return false;
        }

        try
        {
            TwilioClient.Init(_config.AccountSid, _config.AuthToken);
            _logger.LogInformation("TwilioCallLoggingService: Twilio client initialized successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Twilio client for call logging");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<int> LogInboundCallAsync(TwilioCallEvent callEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Logging inbound call {CallSid} from {From} to {To}",
            callEvent.CallSid, callEvent.From, callEvent.To);

        var snapshot = await FetchCallSnapshotAsync(callEvent.CallSid, cancellationToken);

        var entry = new CallLogEntry
        {
            Id = _nextId++,
            CallSid = callEvent.CallSid,
            From = string.IsNullOrEmpty(snapshot?.From) ? callEvent.From : snapshot.From,
            To = string.IsNullOrEmpty(snapshot?.To) ? callEvent.To : snapshot.To,
            Direction = "inbound",
            Status = string.IsNullOrEmpty(snapshot?.Status) ? callEvent.Status : snapshot.Status,
            Duration = snapshot?.Duration ?? callEvent.Duration,
            StartedAt = snapshot?.StartedAt ?? callEvent.Timestamp,
            EndedAt = snapshot?.EndedAt,
            RecordingUrl = callEvent.RecordingUrl
        };

        _callLogs.Add(entry);
        return entry.Id;
    }

    /// <inheritdoc />
    public async Task<int> LogOutboundCallAsync(TwilioCallEvent callEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Logging outbound call {CallSid} from {From} to {To}",
            callEvent.CallSid, callEvent.From, callEvent.To);

        var snapshot = await FetchCallSnapshotAsync(callEvent.CallSid, cancellationToken);

        var entry = new CallLogEntry
        {
            Id = _nextId++,
            CallSid = callEvent.CallSid,
            From = string.IsNullOrEmpty(snapshot?.From) ? callEvent.From : snapshot.From,
            To = string.IsNullOrEmpty(snapshot?.To) ? callEvent.To : snapshot.To,
            Direction = "outbound",
            Status = string.IsNullOrEmpty(snapshot?.Status) ? callEvent.Status : snapshot.Status,
            Duration = snapshot?.Duration ?? callEvent.Duration,
            StartedAt = snapshot?.StartedAt ?? callEvent.Timestamp,
            EndedAt = snapshot?.EndedAt,
            RecordingUrl = callEvent.RecordingUrl
        };

        _callLogs.Add(entry);
        return entry.Id;
    }

    /// <inheritdoc />
    public async Task UpdateCallStatusAsync(string callSid, string status, int? duration = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating call {CallSid} status to {Status}", callSid, status);

        var isTerminalStatus = status is "completed" or "failed" or "no-answer" or "busy";

        // The webhook may not always carry a duration (e.g. early ringing/answered events).
        // For terminal statuses, fall back to the authoritative Twilio call resource.
        var resolvedDuration = duration;
        if (!resolvedDuration.HasValue && isTerminalStatus)
        {
            var snapshot = await FetchCallSnapshotAsync(callSid, cancellationToken);
            resolvedDuration = snapshot?.Duration;
        }

        var entry = _callLogs.FirstOrDefault(c => c.CallSid == callSid);
        if (entry != null)
        {
            // Records are immutable, so we replace
            var index = _callLogs.IndexOf(entry);
            _callLogs[index] = entry with
            {
                Status = status,
                Duration = resolvedDuration ?? entry.Duration,
                EndedAt = isTerminalStatus ? DateTime.UtcNow : entry.EndedAt
            };
        }
        else
        {
            _logger.LogWarning("Call {CallSid} not found for status update", callSid);
        }
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
        {
            query = query.Where(c => c.StartedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(c => c.StartedAt <= endDate.Value);
        }

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

    /// <summary>
    /// Fetches the authoritative call resource from the Twilio REST API by Call SID.
    /// Returns <c>null</c> (rather than throwing) when the client isn't initialized, the
    /// service is running in test mode, the SID is missing, or the Twilio API call fails —
    /// callers fall back to whatever data they already have.
    /// </summary>
    private async Task<TwilioCallSnapshot?> FetchCallSnapshotAsync(string callSid, CancellationToken cancellationToken)
    {
        if (!_isInitialized || _config.TestMode || string.IsNullOrWhiteSpace(callSid))
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var call = await CallResource.FetchAsync(pathSid: callSid);

            var durationSeconds = int.TryParse(call.Duration, out var parsedDuration)
                ? parsedDuration
                : (int?)null;

            return new TwilioCallSnapshot(
                call.From,
                call.To,
                MapCallStatus(call.Status),
                durationSeconds,
                call.StartTime,
                call.EndTime);
        }
        catch (ApiException ex)
        {
            _logger.LogWarning(ex,
                "Twilio API error fetching call details for {CallSid}. Code: {Code}",
                callSid, ex.Code);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching call details from Twilio for {CallSid}", callSid);
            return null;
        }
    }

    private static string MapCallStatus(CallResource.StatusEnum? status)
    {
        if (status is null)
        {
            return string.Empty;
        }

        return status.ToString() switch
        {
            "Queued" => "queued",
            "Ringing" => "ringing",
            "InProgress" => "in-progress",
            "Completed" => "completed",
            "Busy" => "busy",
            "Failed" => "failed",
            "NoAnswer" => "no-answer",
            "Canceled" => "canceled",
            _ => status.ToString()!.ToLowerInvariant()
        };
    }

    /// <summary>
    /// Snapshot of authoritative call data fetched from the Twilio REST API.
    /// </summary>
    private sealed record TwilioCallSnapshot(
        string? From,
        string? To,
        string? Status,
        int? Duration,
        DateTime? StartedAt,
        DateTime? EndedAt);
}
