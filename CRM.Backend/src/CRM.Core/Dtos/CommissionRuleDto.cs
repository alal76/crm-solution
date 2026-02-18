// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for commission rule response
/// </summary>
public class CommissionRuleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SaleType { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating commission rule
/// </summary>
public class CreateCommissionRuleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SaleType { get; set; } = string.Empty;
    public CommissionRuleType RuleType { get; set; }
    public decimal Rate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for updating commission rule
/// </summary>
public class UpdateCommissionRuleDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? SaleType { get; set; }
    public CommissionRuleType? RuleType { get; set; }
    public decimal? Rate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool? IsActive { get; set; }
}
