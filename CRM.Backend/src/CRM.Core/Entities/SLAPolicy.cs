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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// Represents a Service Level Agreement policy
/// </summary>
[Table("SLAPolicies")]
public class SLAPolicy : BaseEntity
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    [StringLength(1024)]
    public string Description { get; set; }

    [Required]
    public ServicePriority Priority { get; set; }

    [Required]
    public int InitialResponseTimeMinutes { get; set; }

    [Required]
    public int ResolutionTimeMinutes { get; set; }

    public bool WorkingHoursOnly { get; set; }

    [Column(TypeName = "longtext")]
    public string EscalationPath { get; set; } // JSON array of user IDs in escalation order

    public bool IsActive { get; set; } = true;

    /// <summary>JSON object with business hours configuration</summary>
    [Column(TypeName = "longtext")]
    public string? BusinessHours { get; set; }

    /// <summary>JSON array of case types this policy applies to</summary>
    [Column(TypeName = "longtext")]
    public string? CaseTypesJson { get; set; }

    /// <summary>JSON array of customer segments this policy applies to</summary>
    [Column(TypeName = "longtext")]
    public string? CustomerSegmentsJson { get; set; }

    /// <summary>JSON array of customer tiers this policy applies to</summary>
    [Column(TypeName = "longtext")]
    public string? CustomerTiersJson { get; set; }

    /// <summary>JSON object with conditions to match for this policy</summary>
    [Column(TypeName = "longtext")]
    public string? MatchConditionsJson { get; set; }

    /// <summary>JSON array of products this policy applies to</summary>
    [Column(TypeName = "longtext")]
    public string? ProductsJson { get; set; }

    /// <summary>
    /// Business hours configuration ID for this SLA policy
    /// </summary>
    public int? BusinessHoursId { get; set; }

    /// <summary>
    /// Escalation rules associated with this SLA policy
    /// </summary>
    public virtual ICollection<EscalationRule> EscalationRules { get; set; } = new List<EscalationRule>();
}

/// <summary>
/// Service request priority levels
/// </summary>
public enum ServicePriority
{
    Critical = 0,
    High = 1,
    Medium = 2,
    Low = 3
}
