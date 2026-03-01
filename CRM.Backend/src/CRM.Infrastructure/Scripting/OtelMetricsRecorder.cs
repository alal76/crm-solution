// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using CRM.Core.Scripting;

namespace CRM.Infrastructure.Scripting;

/// <summary>
/// <see cref="IMetricsRecorder"/> backed by <see cref="System.Diagnostics.Metrics"/>
/// (OTel-compatible; exposed via OpenTelemetry exporters configured in the host).
/// <list type="bullet">
///   <item><see cref="Increment"/> → named counter (<c>crm.script.metric.increments</c>)</item>
///   <item><see cref="RecordValue"/> → histogram (<c>crm.script.metric.values</c>)</item>
/// </list>
/// The <c>metric.name</c> tag disambiguates individual script metrics within these instruments.
/// </summary>
public class OtelMetricsRecorder : IMetricsRecorder
{
    private static readonly Meter ScriptMeter = new("CRM.Scripting", "1.0");

    private static readonly Counter<long> IncrementCounter =
        ScriptMeter.CreateCounter<long>("crm.script.metric.increments");

    private static readonly Histogram<double> ValueHistogram =
        ScriptMeter.CreateHistogram<double>("crm.script.metric.values");

    /// <inheritdoc/>
    public void Increment(string metricName, long value = 1, IDictionary<string, object?>? tags = null)
    {
        var tagList = new TagList { { "metric.name", metricName } };
        if (tags != null)
        {
            foreach (var kv in tags)
            {
                tagList.Add(kv.Key, kv.Value);
            }
        }

        IncrementCounter.Add(value, tagList);
    }

    /// <inheritdoc/>
    public void RecordValue(string metricName, double value, IDictionary<string, object?>? tags = null)
    {
        var tagList = new TagList { { "metric.name", metricName } };
        if (tags != null)
        {
            foreach (var kv in tags)
            {
                tagList.Add(kv.Key, kv.Value);
            }
        }

        ValueHistogram.Record(value, tagList);
    }
}
