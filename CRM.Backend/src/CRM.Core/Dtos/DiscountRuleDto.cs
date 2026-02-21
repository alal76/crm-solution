// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for discount rule response
/// </summary>
public class DiscountRuleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public int? MinQuantity { get; set; }
    public string? CustomerTier { get; set; }
    public string? ProductCategory { get; set; }
    public decimal? MaxDiscount { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsCumulative { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating discount rule
/// </summary>
public class CreateDiscountRuleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountRuleType Type { get; set; }
    public decimal Value { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public int? MinQuantity { get; set; }
    public string? CustomerTier { get; set; }
    public string? ProductCategory { get; set; }
    public decimal? MaxDiscount { get; set; }
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsCumulative { get; set; } = false;
}

/// <summary>
/// DTO for updating discount rule
/// </summary>
public class UpdateDiscountRuleDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DiscountRuleType? Type { get; set; }
    public decimal? Value { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public int? MinQuantity { get; set; }
    public string? CustomerTier { get; set; }
    public string? ProductCategory { get; set; }
    public decimal? MaxDiscount { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsCumulative { get; set; }
}

/// <summary>
/// DTO for discount calculation result
/// </summary>
public class DiscountCalculationDto
{
    public decimal OrderAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public List<DiscountRuleDto> AppliedRules { get; set; } = new();
    public string CalculationDetails { get; set; } = string.Empty;
}
