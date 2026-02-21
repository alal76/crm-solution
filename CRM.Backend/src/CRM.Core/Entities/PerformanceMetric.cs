// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// Performance metrics for API and query performance tracking
/// </summary>
[Table("PerformanceMetrics")]
public class PerformanceMetric : BaseEntity
{
    /// <summary>
    /// Endpoint or operation name
    /// </summary>
    public string EndpointName { get; set; } = string.Empty;

    /// <summary>
    /// HTTP method (GET, POST, PUT, DELETE, PATCH)
    /// </summary>
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>
    /// Route pattern
    /// </summary>
    public string Route { get; set; } = string.Empty;

    /// <summary>
    /// Response time in milliseconds
    /// </summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Database query duration in milliseconds
    /// </summary>
    public long? QueryDurationMs { get; set; }

    /// <summary>
    /// Number of database rows affected
    /// </summary>
    public int? RowsAffected { get; set; }

    /// <summary>
    /// Whether the request was cached
    /// </summary>
    public bool WasCached { get; set; }

    /// <summary>
    /// User ID if applicable
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Timestamp of the request
    /// </summary>
    public DateTime RequestTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Exception message if error occurred
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Query text/signature for analysis
    /// </summary>
    public string? QuerySignature { get; set; }
}
