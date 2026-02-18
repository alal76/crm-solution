// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

/// <summary>
/// Enumeration for discount rule types
/// </summary>
public enum DiscountRuleType
{
    /// <summary>Percentage discount</summary>
    Percentage = 0,

    /// <summary>Fixed amount discount</summary>
    Fixed = 1,

    /// <summary>Volume-based discount (by quantity)</summary>
    VolumeBased = 2,

    /// <summary>Customer tier-based discount</summary>
    TierBased = 3
}

/// <summary>
/// Discount rule entity for configuring discount rules
/// </summary>
public class DiscountRule : BaseEntity
{
    /// <summary>Rule name (e.g., "Enterprise Gold Discount")</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Rule description</summary>
    public string? Description { get; set; }

    /// <summary>Type of discount rule</summary>
    public DiscountRuleType Type { get; set; }

    /// <summary>Backward compatibility alias for Type</summary>
    public DiscountRuleType DiscountType 
    { 
        get => Type; 
        set => Type = value; 
    }

    /// <summary>Discount value (percentage or fixed amount)</summary>
    public decimal Value { get; set; }

    /// <summary>Backward compatibility alias for Value</summary>
    public decimal DiscountValue 
    { 
        get => Value; 
        set => Value = value; 
    }

    /// <summary>Minimum order amount to qualify for discount</summary>
    public decimal? MinOrderAmount { get; set; }

    /// <summary>For volume-based discounts: minimum quantity required</summary>
    public int? MinQuantity { get; set; }

    /// <summary>Backward compatibility alias for MinQuantity</summary>
    public int? MaxQuantity 
    { 
        get => MinQuantity; 
        set => MinQuantity = value; 
    }

    /// <summary>Promotional code for this discount</summary>
    public string? PromotionalCode { get; set; }

    /// <summary>Customer tier requirement (Gold, Silver, Bronze, null = all)</summary>
    public string? CustomerTier { get; set; }

    /// <summary>Product category this rule applies to (null = all products)</summary>
    public string? ProductCategory { get; set; }

    /// <summary>Maximum discount amount cap</summary>
    public decimal? MaxDiscount { get; set; }

    /// <summary>Backward compatibility alias for MaxDiscount</summary>
    public decimal? MaxDiscountValue 
    { 
        get => MaxDiscount; 
        set => MaxDiscount = value; 
    }

    /// <summary>Date the rule becomes effective</summary>
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;

    /// <summary>Backward compatibility alias for EffectiveDate</summary>
    public DateTime ValidFrom 
    { 
        get => EffectiveDate; 
        set => EffectiveDate = value; 
    }

    /// <summary>Date the rule expires (null = no expiration)</summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>Backward compatibility alias for ExpiryDate</summary>
    public DateTime? ValidUntil 
    { 
        get => ExpiryDate; 
        set => ExpiryDate = value; 
    }

    /// <summary>Whether this rule is currently active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Whether this discount can be combined with other discounts</summary>
    public bool IsCumulative { get; set; } = false;

    /// <summary>Backward compatibility alias for IsCumulative</summary>
    public bool CumulativeWithOther 
    { 
        get => IsCumulative; 
        set => IsCumulative = value; 
    }

    /// <summary>JSON array of product IDs this rule applies to</summary>
    public string? ApplicableProductIds { get; set; }

    /// <summary>JSON array of user IDs this rule applies to</summary>
    public string? ApplicableUserIds { get; set; }

    /// <summary>Optional JSON for additional conditions</summary>
    public string? Conditions { get; set; }
}

/// <summary>
/// Discount history entity for auditing discount applications
/// </summary>
public class DiscountHistory : BaseEntity
{
    /// <summary>Order ID the discount was applied to</summary>
    public int OrderId { get; set; }

    /// <summary>Account/Customer ID</summary>
    public int AccountId { get; set; }

    /// <summary>Product ID the discount applied to (null = order-level)</summary>
    public int? ProductId { get; set; }

    /// <summary>Discount rule ID that was applied</summary>
    public int? RuleId { get; set; }

    /// <summary>Discount amount applied</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>Original order/product amount before discount</summary>
    public decimal OriginalAmount { get; set; }

    /// <summary>Date/time when discount was applied</summary>
    public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
}
