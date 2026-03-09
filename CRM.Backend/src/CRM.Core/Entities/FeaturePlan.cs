// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
