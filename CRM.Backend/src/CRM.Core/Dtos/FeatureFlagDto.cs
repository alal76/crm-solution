// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos;

/// <summary>
/// DTO for feature flag audit log entries
/// </summary>
public class FeatureFlagAuditEntryDto
{
    public int Id { get; set; }
    public string FlagName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string NewValue { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public int ChangedById { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? Reason { get; set; }
    public string? TargetingInfo { get; set; }
}

/// <summary>
/// DTO for feature flag data transfer
/// </summary>
public class FeatureFlagDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ProviderCategory { get; set; }
    public string? ActiveProvider { get; set; }
    public bool RequiresRestart { get; set; }
    public int RolloutPercentage { get; set; } = 100;
    public string[] TargetedUserIds { get; set; } = Array.Empty<string>();
    public string[] TargetedRoles { get; set; } = Array.Empty<string>();
}

/// <summary>
/// DTO for creating/updating feature flags
/// </summary>
public class UpdateFeatureFlagDto
{
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public int RolloutPercentage { get; set; } = 100;
    public string? Reason { get; set; }
    public string[]? TargetedUserIds { get; set; }
    public string[]? TargetedRoles { get; set; }
}

/// <summary>
/// DTO for A/B testing variant assignment
/// </summary>
public class FlagVariantDto
{
    public string FlagName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public int Weight { get; set; }
    public Dictionary<string, string> Config { get; set; } = new();
}

/// <summary>
/// DTO for feature flag with variants
/// </summary>
public class FeatureFlagWithVariantsDto : FeatureFlagDto
{
    public FlagVariantDto[] Variants { get; set; } = Array.Empty<FlagVariantDto>();
}

/// <summary>
/// DTO for provider type update
/// </summary>
public class UpdateProviderTypeDto
{
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Reason { get; set; }
}
