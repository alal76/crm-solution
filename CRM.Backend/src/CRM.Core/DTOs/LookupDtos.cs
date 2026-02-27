// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.DTOs;

// ─────────────────────────────────────────────────────────────────
// LookupCategory DTOs
// ─────────────────────────────────────────────────────────────────

public class LookupCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystemManaged { get; set; }
    public bool AllowCustomValues { get; set; }
    public string? EntityType { get; set; }
    public string? PropertyName { get; set; }
    public string? ValidationSchema { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class LookupCategoryDetailDto : LookupCategoryDto
{
    public List<LookupItemDto> Items { get; set; } = [];
}

public class CreateLookupCategoryDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(100)]
    public string? EntityType { get; set; }

    [MaxLength(100)]
    public string? PropertyName { get; set; }

    public bool IsActive { get; set; } = true;
    public bool AllowCustomValues { get; set; } = true;
    public string? ValidationSchema { get; set; }
}

public class UpdateLookupCategoryDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(100)]
    public string? EntityType { get; set; }

    [MaxLength(100)]
    public string? PropertyName { get; set; }

    public bool IsActive { get; set; }
    public bool AllowCustomValues { get; set; }
    public string? ValidationSchema { get; set; }
}

// ─────────────────────────────────────────────────────────────────
// LookupItem DTOs
// ─────────────────────────────────────────────────────────────────

public class LookupItemDto
{
    public int Id { get; set; }
    public int LookupCategoryId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Meta { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public bool IsSystemValue { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public string? ValidationRules { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateLookupItemDto
{
    [Required]
    [MaxLength(200)]
    public string Key { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Value { get; set; } = string.Empty;

    public string? Meta { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public string? ValidationRules { get; set; }
}

public class UpdateLookupItemDto
{
    [Required]
    [MaxLength(200)]
    public string Key { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Value { get; set; } = string.Empty;

    public string? Meta { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public string? ValidationRules { get; set; }
}

public class ReorderItemsDto
{
    /// <summary>Ordered list of item IDs, top to bottom</summary>
    [Required]
    public List<int> OrderedIds { get; set; } = [];
}
