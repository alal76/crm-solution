// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Ports.Input;

/// <summary>
/// Interface for Twilio voice call logging integration.
/// Implements TODO-INT-07: Twilio voice call logging.
/// </summary>
public interface ITwilioCallLoggingService
{
    /// <summary>
    /// Logs an inbound call from Twilio.
    /// </summary>
    /// <param name="callEvent">The Twilio call event data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The logged call record ID.</returns>
    Task<int> LogInboundCallAsync(TwilioCallEvent callEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an outbound call initiated through Twilio.
    /// </summary>
    /// <param name="callEvent">The Twilio call event data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The logged call record ID.</returns>
    Task<int> LogOutboundCallAsync(TwilioCallEvent callEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the status of an ongoing call (e.g., answered, completed, failed).
    /// </summary>
    /// <param name="callSid">The Twilio call SID.</param>
    /// <param name="status">The new call status.</param>
    /// <param name="duration">Call duration in seconds (if completed).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateCallStatusAsync(string callSid, string status, int? duration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets call history for a given phone number.
    /// </summary>
    /// <param name="phoneNumber">The phone number to search for.</param>
    /// <param name="startDate">Optional start date filter.</param>
    /// <param name="endDate">Optional end date filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of call log entries.</returns>
    Task<IReadOnlyList<CallLogEntry>> GetCallHistoryAsync(
        string phoneNumber,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Links a call record to a CRM entity (Account, Contact, Lead).
    /// </summary>
    /// <param name="callSid">The Twilio call SID.</param>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="entityId">The CRM entity ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task LinkCallToEntityAsync(string callSid, string entityType, int entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets call statistics for a time period.
    /// </summary>
    /// <param name="startDate">Start date.</param>
    /// <param name="endDate">End date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Aggregated call statistics.</returns>
    Task<CallStatistics> GetCallStatisticsAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Twilio call event data received from webhook.
/// </summary>
public record TwilioCallEvent
{
    /// <summary>The Twilio Call SID.</summary>
    public string CallSid { get; init; } = string.Empty;

    /// <summary>The caller's phone number.</summary>
    public string From { get; init; } = string.Empty;

    /// <summary>The called phone number.</summary>
    public string To { get; init; } = string.Empty;

    /// <summary>Call direction: inbound or outbound.</summary>
    public string Direction { get; init; } = string.Empty;

    /// <summary>Current call status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Call duration in seconds.</summary>
    public int? Duration { get; init; }

    /// <summary>Recording URL if call was recorded.</summary>
    public string? RecordingUrl { get; init; }

    /// <summary>Timestamp of the call event.</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>Twilio Account SID.</summary>
    public string? AccountSid { get; init; }
}

/// <summary>
/// A single call log entry.
/// </summary>
public record CallLogEntry
{
    /// <summary>Internal record ID.</summary>
    public int Id { get; init; }

    /// <summary>The Twilio Call SID.</summary>
    public string CallSid { get; init; } = string.Empty;

    /// <summary>Caller phone number.</summary>
    public string From { get; init; } = string.Empty;

    /// <summary>Called phone number.</summary>
    public string To { get; init; } = string.Empty;

    /// <summary>Call direction.</summary>
    public string Direction { get; init; } = string.Empty;

    /// <summary>Call status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Duration in seconds.</summary>
    public int? Duration { get; init; }

    /// <summary>When the call started.</summary>
    public DateTime StartedAt { get; init; }

    /// <summary>When the call ended.</summary>
    public DateTime? EndedAt { get; init; }

    /// <summary>Linked CRM entity type.</summary>
    public string? LinkedEntityType { get; init; }

    /// <summary>Linked CRM entity ID.</summary>
    public int? LinkedEntityId { get; init; }

    /// <summary>Recording URL.</summary>
    public string? RecordingUrl { get; init; }
}

/// <summary>
/// Aggregated call statistics.
/// </summary>
public record CallStatistics
{
    /// <summary>Total number of calls.</summary>
    public int TotalCalls { get; init; }

    /// <summary>Inbound call count.</summary>
    public int InboundCalls { get; init; }

    /// <summary>Outbound call count.</summary>
    public int OutboundCalls { get; init; }

    /// <summary>Average call duration in seconds.</summary>
    public double AverageDurationSeconds { get; init; }

    /// <summary>Total call duration in seconds.</summary>
    public long TotalDurationSeconds { get; init; }

    /// <summary>Number of missed/unanswered calls.</summary>
    public int MissedCalls { get; init; }

    /// <summary>Number of calls linked to CRM entities.</summary>
    public int LinkedCalls { get; init; }

    /// <summary>Period start.</summary>
    public DateTime PeriodStart { get; init; }

    /// <summary>Period end.</summary>
    public DateTime PeriodEnd { get; init; }
}
