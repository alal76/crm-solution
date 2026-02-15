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

/// <summary>
/// DTO for commission calculation result
/// </summary>
public class CommissionCalculationDto
{
    public decimal SalesAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal CommissionRate { get; set; }
    public CommissionRuleDto? AppliedRule { get; set; }
    public string CalculationMethod { get; set; } = string.Empty;
}
