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
