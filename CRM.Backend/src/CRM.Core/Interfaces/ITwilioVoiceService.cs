// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Enhanced service interface for Twilio voice call integration.
/// Provides methods for voice call logging with call duration, recording URL, and activity creation.
/// </summary>
public interface ITwilioVoiceService
{
    /// <summary>
    /// Logs a voice call with complete details including duration and recording.
    /// </summary>
    /// <param name="callData">The call data to log.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created activity ID.</returns>
    Task<int> LogCallAsync(VoiceCallData callData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets call details from Twilio by SID.
    /// </summary>
    /// <param name="callSid">The Twilio call SID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The call details.</returns>
    Task<VoiceCallData?> GetCallDetailsAsync(string callSid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the recording URL for a call.
    /// </summary>
    /// <param name="callSid">The Twilio call SID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The recording URL if available.</returns>
    Task<string?> GetRecordingUrlAsync(string callSid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a Twilio webhook event for call status.
    /// </summary>
    /// <param name="payload">The webhook payload.</param>
    /// <param name="signature">The Twilio signature for verification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Processing result.</returns>
    Task<TwilioWebhookResult> ProcessCallWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a Twilio recording webhook.
    /// </summary>
    /// <param name="payload">The webhook payload.</param>
    /// <param name="signature">The Twilio signature for verification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Processing result.</returns>
    Task<TwilioWebhookResult> ProcessRecordingWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets call statistics for a date range.
    /// </summary>
    /// <param name="startDate">Start date.</param>
    /// <param name="endDate">End date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Call statistics.</returns>
    Task<VoiceCallStatistics> GetStatisticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the Twilio connection.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Connection test result.</returns>
    Task<TwilioConnectionResult> TestConnectionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Voice call data for logging.
/// </summary>
public record VoiceCallData
{
    /// <summary>Twilio call SID.</summary>
    public string CallSid { get; init; } = string.Empty;

    /// <summary>Direction (inbound/outbound).</summary>
    public CallDirection Direction { get; init; }

    /// <summary>From phone number.</summary>
    public string FromNumber { get; init; } = string.Empty;

    /// <summary>To phone number.</summary>
    public string ToNumber { get; init; } = string.Empty;

    /// <summary>Call status.</summary>
    public CallStatusType Status { get; init; }

    /// <summary>Call start time.</summary>
    public DateTime StartTime { get; init; }

    /// <summary>Call end time.</summary>
    public DateTime? EndTime { get; init; }

    /// <summary>Call duration in seconds.</summary>
    public int DurationSeconds { get; init; }

    /// <summary>Recording URL if call was recorded.</summary>
    public string? RecordingUrl { get; init; }

    /// <summary>Recording SID.</summary>
    public string? RecordingSid { get; init; }

    /// <summary>Recording duration in seconds.</summary>
    public int? RecordingDurationSeconds { get; init; }

    /// <summary>Linked CRM contact ID.</summary>
    public int? ContactId { get; init; }

    /// <summary>Linked CRM account ID.</summary>
    public int? AccountId { get; init; }

    /// <summary>CRM user who made/received the call.</summary>
    public int? UserId { get; init; }

    /// <summary>Call notes.</summary>
    public string? Notes { get; init; }

    /// <summary>Call outcome/disposition.</summary>
    public string? Outcome { get; init; }

    /// <summary>Call cost in account currency.</summary>
    public decimal? Cost { get; init; }
}

/// <summary>
/// Call direction.
/// </summary>
public enum CallDirection
{
    /// <summary>Inbound call.</summary>
    Inbound = 0,

    /// <summary>Outbound call.</summary>
    Outbound = 1
}

/// <summary>
/// Call status.
/// </summary>
public enum CallStatusType
{
    /// <summary>Call is queued.</summary>
    Queued = 0,

    /// <summary>Call is ringing.</summary>
    Ringing = 1,

    /// <summary>Call is in progress.</summary>
    InProgress = 2,

    /// <summary>Call completed.</summary>
    Completed = 3,

    /// <summary>Call busy.</summary>
    Busy = 4,

    /// <summary>Call failed.</summary>
    Failed = 5,

    /// <summary>No answer.</summary>
    NoAnswer = 6,

    /// <summary>Call cancelled.</summary>
    Cancelled = 7
}

/// <summary>
/// Result of Twilio webhook processing.
/// </summary>
public record TwilioWebhookResult
{
    /// <summary>Whether processing succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>Call SID processed.</summary>
    public string? CallSid { get; init; }

    /// <summary>Activity ID created/updated.</summary>
    public int? ActivityId { get; init; }

    /// <summary>Event type processed.</summary>
    public string? EventType { get; init; }

    /// <summary>Error message if failed.</summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Voice call statistics.
/// </summary>
public record VoiceCallStatistics
{
    /// <summary>Total number of calls.</summary>
    public int TotalCalls { get; init; }

    /// <summary>Inbound calls.</summary>
    public int InboundCalls { get; init; }

    /// <summary>Outbound calls.</summary>
    public int OutboundCalls { get; init; }

    /// <summary>Completed calls.</summary>
    public int CompletedCalls { get; init; }

    /// <summary>Missed calls.</summary>
    public int MissedCalls { get; init; }

    /// <summary>Total duration in seconds.</summary>
    public int TotalDurationSeconds { get; init; }

    /// <summary>Average call duration in seconds.</summary>
    public double AverageDurationSeconds { get; init; }

    /// <summary>Number of recorded calls.</summary>
    public int RecordedCalls { get; init; }

    /// <summary>Total cost.</summary>
    public decimal TotalCost { get; init; }

    /// <summary>Start of the period.</summary>
    public DateTime PeriodStart { get; init; }

    /// <summary>End of the period.</summary>
    public DateTime PeriodEnd { get; init; }
}

/// <summary>
/// Result of Twilio connection test.
/// </summary>
public record TwilioConnectionResult
{
    /// <summary>Whether connection succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>Twilio account SID.</summary>
    public string? AccountSid { get; init; }

    /// <summary>Account friendly name.</summary>
    public string? AccountName { get; init; }

    /// <summary>Error message if failed.</summary>
    public string? ErrorMessage { get; init; }
}
