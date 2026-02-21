// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Entities;

/// <summary>
/// Enumeration for commission rule types
/// </summary>
public enum CommissionRuleType
{
    /// <summary>Flat percentage rate</summary>
    Percentage = 0,

    /// <summary>Fixed amount</summary>
    Flat = 1,

    /// <summary>Tiered rates based on amount brackets</summary>
    Tiered = 2
}

/// <summary>
/// Commission rule entity for configuring commission calculation rules
/// </summary>
public class CommissionRule : BaseEntity
{
    /// <summary>Rule name (e.g., "Standard Sales Commission")</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Rule description</summary>
    public string? Description { get; set; }

    /// <summary>Sale type this rule applies to (e.g., "DirectSale", "PartnerSale", "Renewal")</summary>
    public string SaleType { get; set; } = string.Empty;

    /// <summary>Commission rule type (Percentage, Flat, Tiered)</summary>
    public CommissionRuleType RuleType { get; set; }

    /// <summary>Alias for RuleType for backward compatibility</summary>
    public CommissionRuleType Type
    {
        get => RuleType;
        set => RuleType = value;
    }

    /// <summary>For Percentage and Flat rules: the commission rate/amount</summary>
    public decimal Rate { get; set; }

    /// <summary>Alias for Rate for backward compatibility</summary>
    public decimal BaseRate
    {
        get => Rate;
        set => Rate = value;
    }

    /// <summary>For Tiered rules: minimum sales amount for this tier</summary>
    public decimal? MinAmount { get; set; }

    /// <summary>For Tiered rules: maximum sales amount for this tier</summary>
    public decimal? MaxAmount { get; set; }

    /// <summary>JSON array of product IDs this rule applies to</summary>
    public string? ApplicableProductIds { get; set; }

    /// <summary>JSON array of user IDs this rule applies to</summary>
    public string? ApplicableUserIds { get; set; }

    /// <summary>Date the rule becomes effective</summary>
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;

    /// <summary>Date the rule expires (null = no expiration)</summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>Whether this rule is currently active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Optional JSON for additional rule configuration</summary>
    public string? Configuration { get; set; }
}

/// <summary>
/// Commission history entity for auditing commission calculations
/// </summary>
public class CommissionHistory : BaseEntity
{
    /// <summary>Employee ID who earned the commission</summary>
    public int EmployeeId { get; set; }

    /// <summary>Sale/Opportunity/Order ID related to commission</summary>
    public int SaleId { get; set; }

    /// <summary>Commission amount calculated</summary>
    public decimal Amount { get; set; }

    /// <summary>Commission rule ID used for calculation</summary>
    public int? RuleId { get; set; }

    /// <summary>Date/time when commission was calculated</summary>
    public DateTime CalculatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Sales amount that generated the commission</summary>
    public decimal SalesAmount { get; set; }

    /// <summary>Applied commission rate/calculation details</summary>
    public string? CalculationDetails { get; set; }
}
