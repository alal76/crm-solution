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

namespace CRM.Infrastructure.AI.SK.Attributes;

#region RequiresApprovalAttribute

/// <summary>
/// Marks a Semantic Kernel plugin function as requiring human approval before execution.
/// The approval tier determines the level of review needed ("low", "medium", or "high").
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RequiresApprovalAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the approval tier: "low", "medium", or "high".
    /// Higher tiers require more senior approval and have stricter timeout policies.
    /// </summary>
    public string Tier { get; set; } = "low";

    /// <summary>
    /// Gets or sets a human-readable description of the action for the approver.
    /// This is displayed in the approval UI so the reviewer understands what will happen.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequiresApprovalAttribute"/> class
    /// with the default "low" approval tier.
    /// </summary>
    public RequiresApprovalAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RequiresApprovalAttribute"/> class
    /// with the specified approval tier.
    /// </summary>
    /// <param name="tier">The approval tier: "low", "medium", or "high".</param>
    public RequiresApprovalAttribute(string tier)
    {
        Tier = tier;
    }
}

#endregion
