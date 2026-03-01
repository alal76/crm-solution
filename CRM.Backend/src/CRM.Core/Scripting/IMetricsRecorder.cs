// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Scripting;

/// <summary>
/// Provides scripts with a structured API to emit application metrics
/// (counters, gauges, histograms) to the platform's telemetry backend.
/// </summary>
public interface IMetricsRecorder
{
    /// <summary>
    /// Increments a named counter metric by <paramref name="value"/>.
    /// </summary>
    /// <param name="metricName">Fully-qualified metric name (e.g., "crm.script.tool_calls").</param>
    /// <param name="value">Amount to increment; defaults to 1.</param>
    /// <param name="tags">Optional key-value tags attached to the metric data point.</param>
    void Increment(string metricName, long value = 1, IDictionary<string, object?>? tags = null);

    /// <summary>
    /// Records an arbitrary numeric value for a named metric (gauge/histogram).
    /// </summary>
    /// <param name="metricName">Fully-qualified metric name.</param>
    /// <param name="value">The measured value.</param>
    /// <param name="tags">Optional key-value tags attached to the metric data point.</param>
    void RecordValue(string metricName, double value, IDictionary<string, object?>? tags = null);
}
