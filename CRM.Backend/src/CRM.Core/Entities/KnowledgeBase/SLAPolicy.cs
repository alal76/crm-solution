// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace CRM.Core.Entities.KnowledgeBase;

#region SLA Enumerations

/// <summary>
/// SLA priority level.
/// </summary>
public enum SLAPriority
{
    /// <summary>Critical priority</summary>
    Critical = 0,

    /// <summary>High priority</summary>
    High = 1,

    /// <summary>Medium priority</summary>
    Medium = 2,

    /// <summary>Low priority</summary>
    Low = 3
}

/// <summary>
/// SLA metric type.
/// </summary>
public enum SLAMetricType
{
    /// <summary>First response time</summary>
    FirstResponse = 0,

    /// <summary>Resolution time</summary>
    Resolution = 1,

    /// <summary>Next response time</summary>
    NextResponse = 2,

    /// <summary>Time to assignment</summary>
    Assignment = 3,

    /// <summary>Custom metric</summary>
    Custom = 99
}

/// <summary>
/// Time unit for SLA targets.
/// </summary>
public enum SLATimeUnit
{
    /// <summary>Minutes</summary>
    Minutes = 0,

    /// <summary>Hours</summary>
    Hours = 1,

    /// <summary>Business hours</summary>
    BusinessHours = 2,

    /// <summary>Days</summary>
    Days = 3,

    /// <summary>Business days</summary>
    BusinessDays = 4
}

/// <summary>
/// SLA status.
/// </summary>
public enum SLAStatus
{
    /// <summary>SLA is on track</summary>
    OnTrack = 0,

    /// <summary>SLA is at risk</summary>
    AtRisk = 1,

    /// <summary>SLA is breached</summary>
    Breached = 2,

    /// <summary>SLA is paused</summary>
    Paused = 3,

    /// <summary>SLA is completed (met)</summary>
    Met = 4
}

/// <summary>
/// Escalation type.
/// </summary>
public enum EscalationType
{
    /// <summary>Email notification</summary>
    Email = 0,

    /// <summary>Reassign to user</summary>
    ReassignUser = 1,

    /// <summary>Reassign to team</summary>
    ReassignTeam = 2,

    /// <summary>Priority increase</summary>
    IncreasePriority = 3,

    /// <summary>Webhook notification</summary>
    Webhook = 4,

    /// <summary>SMS notification</summary>
    SMS = 5,

    /// <summary>Custom action</summary>
    Custom = 99
}

#endregion

/// <summary>
/// SLA Policy definition.
/// </summary>
public class SLAPolicy : BaseEntity
{
    #region Identification

    /// <summary>Policy name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Policy description</summary>
    public string? Description { get; set; }

    /// <summary>Is policy active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Is this the default policy</summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>Priority (higher = evaluated first)</summary>
    public int Priority { get; set; } = 0;

    #endregion

    #region Applicability

    /// <summary>Apply to case priority (null = all)</summary>
    public SLAPriority? CasePriority { get; set; }

    /// <summary>Apply to customer segments (JSON array)</summary>
    public string? CustomerSegmentsJson { get; set; }

    /// <summary>Apply to products (JSON array)</summary>
    public string? ProductsJson { get; set; }

    /// <summary>Apply to case types (JSON array)</summary>
    public string? CaseTypesJson { get; set; }

    /// <summary>Apply to customer tiers (JSON array)</summary>
    public string? CustomerTiersJson { get; set; }

    /// <summary>Custom match conditions (JSON)</summary>
    public string? MatchConditionsJson { get; set; }

    #endregion

    #region Business Hours

    /// <summary>Business hours ID</summary>
    public int? BusinessHoursId { get; set; }

    /// <summary>Navigation to business hours</summary>
    public BusinessHours? BusinessHours { get; set; }

    /// <summary>Include holidays in calculation</summary>
    public bool ExcludeHolidays { get; set; } = true;

    #endregion

    #region Relationships

    /// <summary>SLA targets for this policy</summary>
    public ICollection<SLATarget> Targets { get; set; } = new List<SLATarget>();

    /// <summary>Escalation rules</summary>
    public ICollection<SLAPolicyEscalationRule> EscalationRules { get; set; } = new List<SLAPolicyEscalationRule>();

    #endregion
}

/// <summary>
/// SLA target metric within a policy.
/// </summary>
public class SLATarget : BaseEntity
{
    #region References

    /// <summary>SLA Policy ID</summary>
    public int SLAPolicyId { get; set; }

    /// <summary>Navigation to policy</summary>
    public SLAPolicy? SLAPolicy { get; set; }

    #endregion

    #region Target Configuration

    /// <summary>Metric type</summary>
    public SLAMetricType MetricType { get; set; }

    /// <summary>Target value</summary>
    public int TargetValue { get; set; }

    /// <summary>Time unit</summary>
    public SLATimeUnit TimeUnit { get; set; } = SLATimeUnit.Hours;

    /// <summary>Warning threshold percentage (e.g., 80%)</summary>
    public int WarningThresholdPercent { get; set; } = 75;

    /// <summary>Is target active</summary>
    public bool IsActive { get; set; } = true;

    #endregion
}

/// <summary>
/// Business hours definition.
/// </summary>
public class BusinessHours : BaseEntity
{
    /// <summary>Name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Timezone</summary>
    public string Timezone { get; set; } = "UTC";

    /// <summary>Is 24/7</summary>
    public bool Is24x7 { get; set; } = false;

    /// <summary>Is active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Schedule (JSON - day of week: start/end times)</summary>
    public string ScheduleJson { get; set; } = "{}";

    /// <summary>Holidays (JSON array of dates)</summary>
    public string? HolidaysJson { get; set; }

    /// <summary>Policies using these hours</summary>
    public ICollection<SLAPolicy> Policies { get; set; } = new List<SLAPolicy>();
}

/// <summary>
/// Escalation rule within SLA policy.
/// </summary>
public class SLAPolicyEscalationRule : BaseEntity
{
    #region References

    /// <summary>SLA Policy ID</summary>
    public int SLAPolicyId { get; set; }

    /// <summary>Navigation to policy</summary>
    public SLAPolicy? SLAPolicy { get; set; }

    #endregion

    #region Trigger Configuration

    /// <summary>Rule name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Trigger at percentage of SLA (e.g., 50%, 100%)</summary>
    public int TriggerAtPercent { get; set; } = 100;

    /// <summary>Which SLA metric triggers this</summary>
    public SLAMetricType TriggerMetric { get; set; }

    /// <summary>Is rule active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Order of execution</summary>
    public int ExecutionOrder { get; set; } = 0;

    #endregion

    #region Action Configuration

    /// <summary>Escalation type</summary>
    public EscalationType EscalationType { get; set; }

    /// <summary>Email recipients (for email type)</summary>
    public string? EmailRecipientsJson { get; set; }

    /// <summary>Email template ID</summary>
    public int? EmailTemplateId { get; set; }

    /// <summary>Reassign to user ID</summary>
    public int? ReassignToUserId { get; set; }

    /// <summary>Navigation to user</summary>
    public User? ReassignToUser { get; set; }

    /// <summary>Reassign to team ID</summary>
    public int? ReassignToTeamId { get; set; }

    /// <summary>New priority level</summary>
    public SLAPriority? NewPriority { get; set; }

    /// <summary>Webhook URL</summary>
    public string? WebhookUrl { get; set; }

    /// <summary>Custom action configuration (JSON)</summary>
    public string? ActionConfigJson { get; set; }

    #endregion
}

/// <summary>
/// SLA instance tracking for a service request.
/// </summary>
public class SLAInstance : BaseEntity
{
    #region References

    /// <summary>Service request ID</summary>
    public int ServiceRequestId { get; set; }

    /// <summary>Navigation to service request</summary>
    public ServiceRequest? ServiceRequest { get; set; }

    /// <summary>SLA Policy ID</summary>
    public int SLAPolicyId { get; set; }

    /// <summary>Navigation to policy</summary>
    public SLAPolicy? SLAPolicy { get; set; }

    /// <summary>SLA Target ID</summary>
    public int SLATargetId { get; set; }

    /// <summary>Navigation to target</summary>
    public SLATarget? SLATarget { get; set; }

    #endregion

    #region Timing

    /// <summary>SLA started at</summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>SLA due at</summary>
    public DateTime DueAt { get; set; }

    /// <summary>Warning threshold time</summary>
    public DateTime WarningAt { get; set; }

    /// <summary>Completed at</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Time elapsed (minutes)</summary>
    public int ElapsedMinutes { get; set; } = 0;

    /// <summary>Time remaining (minutes)</summary>
    public int RemainingMinutes { get; set; }

    /// <summary>Business time elapsed (minutes)</summary>
    public int BusinessTimeElapsedMinutes { get; set; } = 0;

    #endregion

    #region Status

    /// <summary>Current SLA status</summary>
    public SLAStatus Status { get; set; } = SLAStatus.OnTrack;

    /// <summary>Was SLA breached</summary>
    public bool WasBreached { get; set; } = false;

    /// <summary>Breach time (if breached)</summary>
    public DateTime? BreachedAt { get; set; }

    /// <summary>Minutes over SLA (if breached)</summary>
    public int? MinutesOverSla { get; set; }

    #endregion

    #region Pause Tracking

    /// <summary>Is SLA currently paused</summary>
    public bool IsPaused { get; set; } = false;

    /// <summary>Paused at</summary>
    public DateTime? PausedAt { get; set; }

    /// <summary>Total pause time (minutes)</summary>
    public int TotalPauseMinutes { get; set; } = 0;

    /// <summary>Pause reason</summary>
    public string? PauseReason { get; set; }

    #endregion

    #region Escalation Tracking

    /// <summary>Escalation level reached (0 = none)</summary>
    public int EscalationLevel { get; set; } = 0;

    /// <summary>Escalations triggered (JSON array)</summary>
    public string? EscalationsTriggeredJson { get; set; }

    /// <summary>Last escalation time</summary>
    public DateTime? LastEscalationAt { get; set; }

    #endregion
}
