// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under GNU AGPL v3

using System.Diagnostics;

namespace CRM.Core.Instrumentation;

/// <summary>
/// Central instrumentation service for application-wide telemetry, tracing, and performance monitoring.
/// </summary>
public static class InstrumentationService
{
    // Activity source for distributed tracing
    public static readonly ActivitySource ActivitySource = new("CRM.Solution", "1.3.1");
    
    // Performance counters
    private static readonly Dictionary<string, PerformanceMetric> _metrics = new();
    private static readonly object _lock = new();

    /// <summary>
    /// Start a new trace activity for an operation
    /// </summary>
    public static Activity? StartActivity(string operationName, ActivityKind kind = ActivityKind.Internal)
    {
        return ActivitySource.StartActivity(operationName, kind);
    }

    /// <summary>
    /// Start a trace for API controller action
    /// </summary>
    public static Activity? StartControllerActivity(string controllerName, string actionName)
    {
        var activity = ActivitySource.StartActivity($"{controllerName}.{actionName}", ActivityKind.Server);
        activity?.SetTag("crm.controller", controllerName);
        activity?.SetTag("crm.action", actionName);
        return activity;
    }

    /// <summary>
    /// Start a trace for service operation
    /// </summary>
    public static Activity? StartServiceActivity(string serviceName, string operationName)
    {
        var activity = ActivitySource.StartActivity($"{serviceName}.{operationName}", ActivityKind.Internal);
        activity?.SetTag("crm.service", serviceName);
        activity?.SetTag("crm.operation", operationName);
        return activity;
    }

    /// <summary>
    /// Start a trace for database operation
    /// </summary>
    public static Activity? StartDatabaseActivity(string operationName, string? tableName = null)
    {
        var activity = ActivitySource.StartActivity($"DB.{operationName}", ActivityKind.Client);
        activity?.SetTag("db.system", "mariadb");
        activity?.SetTag("db.operation", operationName);
        if (tableName != null)
            activity?.SetTag("db.table", tableName);
        return activity;
    }

    /// <summary>
    /// Record a performance metric
    /// </summary>
    public static void RecordMetric(string metricName, double value, Dictionary<string, string>? tags = null)
    {
        lock (_lock)
        {
            if (!_metrics.ContainsKey(metricName))
            {
                _metrics[metricName] = new PerformanceMetric(metricName);
            }
            _metrics[metricName].Record(value, tags);
        }
    }

    /// <summary>
    /// Get performance metrics summary
    /// </summary>
    public static Dictionary<string, PerformanceMetricSummary> GetMetricsSummary()
    {
        lock (_lock)
        {
            return _metrics.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.GetSummary()
            );
        }
    }

    /// <summary>
    /// Clear all metrics
    /// </summary>
    public static void ClearMetrics()
    {
        lock (_lock)
        {
            _metrics.Clear();
        }
    }

    /// <summary>
    /// Start a stopwatch for timing operations
    /// </summary>
    public static Stopwatch StartTimer() => Stopwatch.StartNew();

    /// <summary>
    /// Record timing from a stopwatch
    /// </summary>
    public static void RecordTiming(string operationName, Stopwatch stopwatch)
    {
        stopwatch.Stop();
        RecordMetric($"timing.{operationName}", stopwatch.ElapsedMilliseconds);
    }
}

/// <summary>
/// Holds performance metric data with statistical calculations
/// </summary>
public class PerformanceMetric
{
    public string Name { get; }
    private readonly List<(double Value, DateTime Timestamp, Dictionary<string, string>? Tags)> _values = new();

    public PerformanceMetric(string name)
    {
        Name = name;
    }

    public void Record(double value, Dictionary<string, string>? tags = null)
    {
        _values.Add((value, DateTime.UtcNow, tags));
        // Keep only last 10000 values to prevent memory issues
        if (_values.Count > 10000)
            _values.RemoveAt(0);
    }

    public PerformanceMetricSummary GetSummary()
    {
        if (_values.Count == 0)
            return new PerformanceMetricSummary();

        var values = _values.Select(v => v.Value).ToList();
        values.Sort();

        return new PerformanceMetricSummary
        {
            Count = _values.Count,
            Min = values.First(),
            Max = values.Last(),
            Avg = values.Average(),
            Median = values[values.Count / 2],
            P95 = values[(int)(values.Count * 0.95)],
            P99 = values[(int)(values.Count * 0.99)],
            FirstRecorded = _values.First().Timestamp,
            LastRecorded = _values.Last().Timestamp
        };
    }
}

/// <summary>
/// Summary statistics for a performance metric
/// </summary>
public class PerformanceMetricSummary
{
    public int Count { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double Avg { get; set; }
    public double Median { get; set; }
    public double P95 { get; set; }
    public double P99 { get; set; }
    public DateTime FirstRecorded { get; set; }
    public DateTime LastRecorded { get; set; }
}
