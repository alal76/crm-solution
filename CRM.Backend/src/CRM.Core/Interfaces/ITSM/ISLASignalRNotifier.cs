// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// DTO representing real-time SLA countdown data pushed to clients via SignalR.
/// </summary>
public class SLACountdownDto
{
    /// <summary>The service request ID this countdown applies to.</summary>
    public int ServiceRequestId { get; set; }

    /// <summary>Current SLA status: OnTrack, AtRisk, or Breached.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Time remaining until the response SLA deadline (null if already met).</summary>
    public TimeSpan? ResponseTimeRemaining { get; set; }

    /// <summary>Time remaining until the resolution SLA deadline (null if already met).</summary>
    public TimeSpan? ResolutionTimeRemaining { get; set; }

    /// <summary>The absolute response deadline.</summary>
    public DateTime? ResponseDeadline { get; set; }

    /// <summary>The absolute resolution deadline.</summary>
    public DateTime? ResolutionDeadline { get; set; }

    /// <summary>Percentage of allowed response time that has been consumed (0-100+).</summary>
    public double ResponsePercentageUsed { get; set; }

    /// <summary>Percentage of allowed resolution time that has been consumed (0-100+).</summary>
    public double ResolutionPercentageUsed { get; set; }
}

/// <summary>
/// Interface for sending real-time SLA notifications via SignalR.
/// Implemented in the API layer where SignalR hub context is available.
/// </summary>
public interface ISLASignalRNotifier
{
    /// <summary>
    /// Send an SLA countdown update for a specific service request.
    /// Notifies both the ticket-specific group and the all-SLA group.
    /// </summary>
    Task NotifySLAUpdate(int serviceRequestId, SLACountdownDto countdown);

    /// <summary>
    /// Notify that an SLA breach has occurred for a service request.
    /// </summary>
    Task NotifySLABreach(int serviceRequestId, string breachType);

    /// <summary>
    /// Notify that an SLA warning threshold has been reached for a service request.
    /// </summary>
    Task NotifySLAWarning(int serviceRequestId, string warningType, TimeSpan timeRemaining);
}
