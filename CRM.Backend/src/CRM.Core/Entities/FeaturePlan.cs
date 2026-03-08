using System;

namespace CRM.Core.Entities;

/// <summary>
/// Feature plan for subscription tiers and feature gating.
/// </summary>
public class FeaturePlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? BillingPeriod { get; set; }
    public string? FeaturesJson { get; set; }
    public int? MaxUsers { get; set; }
    public int? MaxStorage { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
