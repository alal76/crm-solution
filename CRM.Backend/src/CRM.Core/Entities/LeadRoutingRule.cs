// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

#region Lead Routing Enumerations

/// <summary>
/// FUNCTIONAL: How leads are distributed among available reps.
/// TECHNICAL: Determines assignment algorithm.
/// </summary>
public enum LeadAssignmentType
{
    /// <summary>Sequential distribution to each rep in turn</summary>
    RoundRobin = 0,

    /// <summary>Based on rep capacity/weighting</summary>
    Weighted = 1,

    /// <summary>Based on geographic territory</summary>
    Territory = 2,

    /// <summary>Based on lead score matching rep expertise</summary>
    ScoreBased = 3,

    /// <summary>First available rep claims the lead</summary>
    FirstCome = 4,

    /// <summary>Random assignment</summary>
    Random = 5,

    /// <summary>Manual queue for manager assignment</summary>
    ManualQueue = 6,

    /// <summary>Skills-based routing</summary>
    SkillsBased = 7,

    /// <summary>Load-balanced across team</summary>
    LoadBalanced = 8
}

/// <summary>
/// FUNCTIONAL: Criteria type for routing rule matching.
/// TECHNICAL: Determines how to evaluate lead for rule.
/// </summary>
public enum RoutingCriteriaType
{
    /// <summary>Lead source channel</summary>
    LeadSource = 0,

    /// <summary>Lead score threshold</summary>
    LeadScore = 1,

    /// <summary>Geographic region</summary>
    Territory = 2,

    /// <summary>Industry vertical</summary>
    Industry = 3,

    /// <summary>Company size</summary>
    CompanySize = 4,

    /// <summary>Annual revenue range</summary>
    AnnualRevenue = 5,

    /// <summary>Product interest</summary>
    ProductInterest = 6,

    /// <summary>Campaign source</summary>
    Campaign = 7,

    /// <summary>Lead status</summary>
    LeadStatus = 8,

    /// <summary>Custom field value</summary>
    CustomField = 9
}

/// <summary>
/// FUNCTIONAL: Routing rule status.
/// TECHNICAL: Controls whether rule is evaluated.
/// </summary>
public enum RoutingRuleStatus
{
    /// <summary>Rule is active and being evaluated</summary>
    Active = 0,

    /// <summary>Rule is inactive/disabled</summary>
    Inactive = 1,

    /// <summary>Rule is in draft mode</summary>
    Draft = 2
}

#endregion

/// <summary>
/// Lead routing rule for automatic lead assignment.
/// Evaluates incoming leads and assigns to appropriate reps/teams.
/// </summary>
public class LeadRoutingRule : BaseEntity
{
    #region Identification

    /// <summary>Rule name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Rule description</summary>
    public string? Description { get; set; }

    /// <summary>Rule status</summary>
    public RoutingRuleStatus Status { get; set; } = RoutingRuleStatus.Active;

    /// <summary>Priority (lower number = higher priority)</summary>
    public int Priority { get; set; } = 100;

    #endregion

    #region Assignment Configuration

    /// <summary>How leads are assigned</summary>
    public LeadAssignmentType AssignmentType { get; set; } = LeadAssignmentType.RoundRobin;

    /// <summary>Whether to assign to team or specific users</summary>
    public bool AssignToTeam { get; set; } = false;

    /// <summary>Target team ID (if assigning to team)</summary>
    public int? TeamId { get; set; }

    /// <summary>Navigation to team</summary>
    public Team? Team { get; set; }

    /// <summary>Fallback owner if no match (manager queue)</summary>
    public int? FallbackOwnerId { get; set; }

    /// <summary>Navigation to fallback owner</summary>
    public User? FallbackOwner { get; set; }

    #endregion

    #region Timing

    /// <summary>Rule effective start date</summary>
    public DateTime? EffectiveStartDate { get; set; }

    /// <summary>Rule effective end date</summary>
    public DateTime? EffectiveEndDate { get; set; }

    /// <summary>Whether to only route during business hours</summary>
    public bool BusinessHoursOnly { get; set; } = false;

    /// <summary>Timezone for business hours evaluation</summary>
    public string? Timezone { get; set; }

    #endregion

    #region Round Robin State

    /// <summary>Current position in round robin rotation</summary>
    public int RoundRobinPosition { get; set; } = 0;

    /// <summary>Last assignment date</summary>
    public DateTime? LastAssignmentDate { get; set; }

    /// <summary>Total leads assigned by this rule</summary>
    public int TotalLeadsAssigned { get; set; } = 0;

    #endregion

    #region Notification

    /// <summary>Send notification on assignment</summary>
    public bool SendNotification { get; set; } = true;

    /// <summary>Notification email template ID</summary>
    public int? NotificationTemplateId { get; set; }

    /// <summary>Also notify manager</summary>
    public bool NotifyManager { get; set; } = false;

    #endregion

    #region Relationships

    /// <summary>Routing criteria for this rule</summary>
    public ICollection<LeadRoutingCriteria> Criteria { get; set; } = new List<LeadRoutingCriteria>();

    /// <summary>Target users for assignment</summary>
    public ICollection<LeadRoutingTarget> Targets { get; set; } = new List<LeadRoutingTarget>();

    #endregion
}

/// <summary>
/// Criteria for matching leads to a routing rule.
/// </summary>
public class LeadRoutingCriteria : BaseEntity
{
    /// <summary>Criteria type</summary>
    public RoutingCriteriaType CriteriaType { get; set; }

    /// <summary>Field name (for custom fields)</summary>
    public string? FieldName { get; set; }

    /// <summary>Operator (equals, contains, greater_than, etc.)</summary>
    public string Operator { get; set; } = "equals";

    /// <summary>Value to match</summary>
    public string? Value { get; set; }

    /// <summary>Secondary value (for range operators)</summary>
    public string? ValueTo { get; set; }

    /// <summary>Logical operator to next criteria (AND/OR)</summary>
    public string LogicalOperator { get; set; } = "AND";

    /// <summary>Order of evaluation</summary>
    public int Order { get; set; } = 0;

    /// <summary>Parent routing rule ID</summary>
    public int LeadRoutingRuleId { get; set; }

    /// <summary>Navigation to routing rule</summary>
    public LeadRoutingRule? LeadRoutingRule { get; set; }
}

/// <summary>
/// Target user for lead routing with optional weighting.
/// </summary>
public class LeadRoutingTarget : BaseEntity
{
    /// <summary>Target user ID</summary>
    public int UserId { get; set; }

    /// <summary>Navigation to user</summary>
    public User? User { get; set; }

    /// <summary>Weight for weighted assignment (0-100)</summary>
    public int Weight { get; set; } = 100;

    /// <summary>Maximum leads per day (null = unlimited)</summary>
    public int? MaxLeadsPerDay { get; set; }

    /// <summary>Maximum leads per week</summary>
    public int? MaxLeadsPerWeek { get; set; }

    /// <summary>Whether target is active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Leads assigned today</summary>
    public int LeadsAssignedToday { get; set; } = 0;

    /// <summary>Leads assigned this week</summary>
    public int LeadsAssignedThisWeek { get; set; } = 0;

    /// <summary>Last assignment date for this target</summary>
    public DateTime? LastAssignmentDate { get; set; }

    /// <summary>Total leads assigned to this target</summary>
    public int TotalLeadsAssigned { get; set; } = 0;

    /// <summary>Parent routing rule ID</summary>
    public int LeadRoutingRuleId { get; set; }

    /// <summary>Navigation to routing rule</summary>
    public LeadRoutingRule? LeadRoutingRule { get; set; }
}

/// <summary>
/// Log of lead routing assignments for audit and analytics.
/// </summary>
public class LeadRoutingLog : BaseEntity
{
    /// <summary>Lead ID that was routed</summary>
    public int LeadId { get; set; }

    /// <summary>Navigation to lead</summary>
    public Lead? Lead { get; set; }

    /// <summary>Routing rule that matched</summary>
    public int? LeadRoutingRuleId { get; set; }

    /// <summary>Navigation to routing rule</summary>
    public LeadRoutingRule? LeadRoutingRule { get; set; }

    /// <summary>User lead was assigned to</summary>
    public int? AssignedToUserId { get; set; }

    /// <summary>Navigation to assigned user</summary>
    public User? AssignedToUser { get; set; }

    /// <summary>Previous owner (if reassignment)</summary>
    public int? PreviousOwnerId { get; set; }

    /// <summary>Assignment date/time</summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Assignment method used</summary>
    public LeadAssignmentType AssignmentType { get; set; }

    /// <summary>Whether assignment was successful</summary>
    public bool Success { get; set; } = true;

    /// <summary>Failure reason if not successful</summary>
    public string? FailureReason { get; set; }

    /// <summary>Response time in seconds (first touch)</summary>
    public int? ResponseTimeSeconds { get; set; }

    /// <summary>Whether lead was contacted within SLA</summary>
    public bool? ContactedWithinSLA { get; set; }
}
