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

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

#region Enums

/// <summary>
/// FUNCTIONAL: Type of lead scoring rule
/// TECHNICAL: Determines how the rule is evaluated
/// </summary>
public enum LeadScoreRuleType
{
    /// <summary>Score based on lead attributes (e.g., job title, company size)</summary>
    Attribute = 0,

    /// <summary>Score based on lead behavior (e.g., page views, email opens)</summary>
    Behavior = 1,

    /// <summary>Score decay over time for inactive leads</summary>
    Decay = 2,

    /// <summary>Score based on demographic information</summary>
    Demographic = 3,

    /// <summary>Score based on fit with ideal customer profile</summary>
    FitScore = 4
}

/// <summary>
/// FUNCTIONAL: Comparison operators for rule conditions
/// TECHNICAL: Used in attribute-based and behavioral rules
/// </summary>
public enum RuleOperator
{
    Equals = 0,
    NotEquals = 1,
    Contains = 2,
    NotContains = 3,
    GreaterThan = 4,
    LessThan = 5,
    GreaterThanOrEquals = 6,
    LessThanOrEquals = 7,
    IsEmpty = 8,
    IsNotEmpty = 9,
    In = 10,
    NotIn = 11
}

#endregion

/// <summary>
/// FUNCTIONAL: Configurable rule for scoring leads based on various criteria
/// TECHNICAL: Stores rule definition, conditions, and score impact
/// 
/// Key Features:
/// - Attribute-based scoring (job title, company size, industry)
/// - Behavioral scoring (email opens, page views, form submissions)
/// - Time-based decay for inactive leads
/// - Multiple condition support with JSON storage
/// </summary>
public class LeadScoreRule : BaseEntity
{
    #region Rule Identification

    /// <summary>
    /// FUNCTIONAL: Display name for the scoring rule
    /// TECHNICAL: Required, max 200 chars
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// FUNCTIONAL: Detailed description of what the rule does
    /// TECHNICAL: Optional, max 1000 chars
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    #endregion

    #region Rule Configuration

    /// <summary>
    /// FUNCTIONAL: Type of scoring rule (Attribute, Behavior, Decay, etc.)
    /// TECHNICAL: Determines evaluation logic
    /// </summary>
    public LeadScoreRuleType RuleType { get; set; } = LeadScoreRuleType.Attribute;

    /// <summary>
    /// FUNCTIONAL: The field/attribute this rule evaluates
    /// TECHNICAL: Lead entity field name (e.g., "JobTitle", "CompanySize", "Industry")
    /// </summary>
    [MaxLength(100)]
    public string? FieldName { get; set; }

    /// <summary>
    /// FUNCTIONAL: How to compare the field value
    /// TECHNICAL: Comparison operator enum
    /// </summary>
    public RuleOperator Operator { get; set; } = RuleOperator.Equals;

    /// <summary>
    /// FUNCTIONAL: The value to compare against
    /// TECHNICAL: String representation, parsed based on field type
    /// </summary>
    [MaxLength(500)]
    public string? Value { get; set; }

    /// <summary>
    /// FUNCTIONAL: Complex conditions as JSON
    /// TECHNICAL: For multi-condition rules, stores JSON array of conditions
    /// Example: [{"field": "JobTitle", "operator": "contains", "value": "Director"},
    ///           {"field": "Industry", "operator": "equals", "value": "Technology"}]
    /// </summary>
    public string? ConditionsJson { get; set; }

    #endregion

    #region Scoring Impact

    /// <summary>
    /// FUNCTIONAL: Points to add/subtract when rule matches
    /// TECHNICAL: Can be positive (bonus) or negative (penalty)
    /// </summary>
    public int ScoreImpact { get; set; } = 10;

    /// <summary>
    /// FUNCTIONAL: Maximum times this rule can apply per lead
    /// TECHNICAL: Prevents score runaway, null = unlimited
    /// </summary>
    public int? MaxApplications { get; set; }

    #endregion

    #region Decay Configuration (for RuleType = Decay)

    /// <summary>
    /// FUNCTIONAL: Days of inactivity before decay applies
    /// TECHNICAL: Only used for Decay rule type
    /// </summary>
    public int? DecayDaysThreshold { get; set; }

    /// <summary>
    /// FUNCTIONAL: Points to decay per period after threshold
    /// TECHNICAL: Applied by background job
    /// </summary>
    public int? DecayPointsPerPeriod { get; set; }

    /// <summary>
    /// FUNCTIONAL: How often decay applies (days)
    /// TECHNICAL: Default is every 7 days
    /// </summary>
    public int? DecayPeriodDays { get; set; } = 7;

    #endregion

    #region Status and Priority

    /// <summary>
    /// FUNCTIONAL: Whether the rule is currently active
    /// TECHNICAL: Inactive rules are not evaluated
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// FUNCTIONAL: Evaluation order (lower = higher priority)
    /// TECHNICAL: Rules are evaluated in priority order
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// FUNCTIONAL: Category for grouping rules in UI
    /// TECHNICAL: Examples: "Demographics", "Engagement", "Fit"
    /// </summary>
    [MaxLength(50)]
    public string? Category { get; set; }

    #endregion

    #region Behavior Tracking (for RuleType = Behavior)

    /// <summary>
    /// FUNCTIONAL: The action/event this rule tracks
    /// TECHNICAL: Examples: "EmailOpen", "PageView", "FormSubmit", "FileDownload"
    /// </summary>
    [MaxLength(100)]
    public string? ActionType { get; set; }

    /// <summary>
    /// FUNCTIONAL: Specific action identifier
    /// TECHNICAL: Examples: Campaign ID for email, Page URL pattern, Form ID
    /// </summary>
    [MaxLength(255)]
    public string? ActionIdentifier { get; set; }

    #endregion
}
