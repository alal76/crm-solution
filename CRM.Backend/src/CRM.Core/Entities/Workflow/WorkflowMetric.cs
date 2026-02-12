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

namespace CRM.Core.Entities.Workflow;

/// <summary>
/// Represents a time-series metric data point for workflow performance monitoring.
/// </summary>
public class WorkflowMetric : BaseEntity
{
    /// <summary>
    /// Foreign key to the workflow definition this metric belongs to.
    /// </summary>
    public int WorkflowDefinitionId { get; set; }

    /// <summary>
    /// Navigation property to the workflow definition.
    /// </summary>
    public virtual WorkflowDefinition WorkflowDefinition { get; set; } = null!;

    /// <summary>
    /// Category of the metric (e.g., "Duration", "Throughput", "ErrorRate").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string MetricType { get; set; } = string.Empty;

    /// <summary>
    /// Specific metric name within the type (e.g., "avg_completion_time_ms").
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string MetricName { get; set; } = string.Empty;

    /// <summary>
    /// Numeric value of the metric.
    /// </summary>
    public decimal MetricValue { get; set; }

    /// <summary>
    /// JSON dictionary of additional dimensions for metric slicing.
    /// </summary>
    public string? Dimensions { get; set; }

    /// <summary>
    /// Timestamp when this metric was recorded.
    /// </summary>
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
