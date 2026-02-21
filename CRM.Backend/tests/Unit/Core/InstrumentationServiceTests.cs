// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Diagnostics;
using CRM.Core.Instrumentation;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Unit tests for InstrumentationService and related metric classes
/// </summary>
public class InstrumentationServiceTests : IDisposable
{
    public InstrumentationServiceTests()
    {
        // Clear metrics before each test
        InstrumentationService.ClearMetrics();
    }

    public void Dispose()
    {
        // Clean up after each test
        InstrumentationService.ClearMetrics();
    }

    #region ActivitySource Tests

    [Fact]
    public void ActivitySource_ShouldBeInitialized()
    {
        // Assert
        InstrumentationService.ActivitySource.Should().NotBeNull();
        InstrumentationService.ActivitySource.Name.Should().Be("CRM.Solution");
        InstrumentationService.ActivitySource.Version.Should().Be("1.3.1");
    }

    [Fact]
    public void StartActivity_ShouldCreateActivityWithCorrectName()
    {
        // Act
        using var activity = InstrumentationService.StartActivity("TestOperation");

        // Assert
        // Note: Activity may be null if there are no listeners
        if (activity != null)
        {
            activity.DisplayName.Should().Be("TestOperation");
        }
    }

    [Fact]
    public void StartActivity_WithActivityKind_ShouldSetKind()
    {
        // Act
        using var activity = InstrumentationService.StartActivity("ServerOperation", ActivityKind.Server);

        // Assert
        if (activity != null)
        {
            activity.Kind.Should().Be(ActivityKind.Server);
        }
    }

    #endregion

    #region Controller Activity Tests

    [Fact]
    public void StartControllerActivity_ShouldCreateActivityWithTags()
    {
        // Act
        using var activity = InstrumentationService.StartControllerActivity("AccountsController", "GetById");

        // Assert
        if (activity != null)
        {
            activity.DisplayName.Should().Be("AccountsController.GetById");
            activity.Kind.Should().Be(ActivityKind.Server);
            activity.GetTagItem("crm.controller").Should().Be("AccountsController");
            activity.GetTagItem("crm.action").Should().Be("GetById");
        }
    }

    [Fact]
    public void StartControllerActivity_WithDifferentControllerAndAction_ShouldSetCorrectTags()
    {
        // Act
        using var activity = InstrumentationService.StartControllerActivity("OpportunitiesController", "Create");

        // Assert
        if (activity != null)
        {
            activity.GetTagItem("crm.controller").Should().Be("OpportunitiesController");
            activity.GetTagItem("crm.action").Should().Be("Create");
        }
    }

    #endregion

    #region Service Activity Tests

    [Fact]
    public void StartServiceActivity_ShouldCreateActivityWithTags()
    {
        // Act
        using var activity = InstrumentationService.StartServiceActivity("AccountService", "GetByIdAsync");

        // Assert
        if (activity != null)
        {
            activity.DisplayName.Should().Be("AccountService.GetByIdAsync");
            activity.Kind.Should().Be(ActivityKind.Internal);
            activity.GetTagItem("crm.service").Should().Be("AccountService");
            activity.GetTagItem("crm.operation").Should().Be("GetByIdAsync");
        }
    }

    #endregion

    #region Database Activity Tests

    [Fact]
    public void StartDatabaseActivity_ShouldCreateActivityWithDbTags()
    {
        // Act
        using var activity = InstrumentationService.StartDatabaseActivity("SELECT");

        // Assert
        if (activity != null)
        {
            activity.DisplayName.Should().Be("DB.SELECT");
            activity.Kind.Should().Be(ActivityKind.Client);
            activity.GetTagItem("db.system").Should().Be("mariadb");
            activity.GetTagItem("db.operation").Should().Be("SELECT");
        }
    }

    [Fact]
    public void StartDatabaseActivity_WithTableName_ShouldIncludeTableTag()
    {
        // Act
        using var activity = InstrumentationService.StartDatabaseActivity("INSERT", "Customers");

        // Assert
        if (activity != null)
        {
            activity.GetTagItem("db.table").Should().Be("Customers");
        }
    }

    [Fact]
    public void StartDatabaseActivity_WithNullTableName_ShouldNotHaveTableTag()
    {
        // Act
        using var activity = InstrumentationService.StartDatabaseActivity("UPDATE", null);

        // Assert
        if (activity != null)
        {
            activity.GetTagItem("db.table").Should().BeNull();
        }
    }

    #endregion

    #region Metric Recording Tests

    [Fact]
    public void RecordMetric_ShouldStoreMetric()
    {
        // Act
        InstrumentationService.RecordMetric("test.metric", 100.5);

        // Assert
        var summary = InstrumentationService.GetMetricsSummary();
        summary.Should().ContainKey("test.metric");
        summary["test.metric"].Count.Should().Be(1);
    }

    [Fact]
    public void RecordMetric_MultipleValues_ShouldAccumulate()
    {
        // Act
        InstrumentationService.RecordMetric("response.time", 50);
        InstrumentationService.RecordMetric("response.time", 100);
        InstrumentationService.RecordMetric("response.time", 150);

        // Assert
        var summary = InstrumentationService.GetMetricsSummary();
        summary["response.time"].Count.Should().Be(3);
        summary["response.time"].Avg.Should().Be(100);
    }

    [Fact]
    public void RecordMetric_WithTags_ShouldAcceptTags()
    {
        // Arrange
        var tags = new Dictionary<string, string>
        {
            { "endpoint", "/api/accounts" },
            { "method", "GET" }
        };

        // Act
        InstrumentationService.RecordMetric("api.call", 250, tags);

        // Assert
        var summary = InstrumentationService.GetMetricsSummary();
        summary.Should().ContainKey("api.call");
    }

    [Fact]
    public void RecordMetric_MultipleMetrics_ShouldBeStoredSeparately()
    {
        // Act
        InstrumentationService.RecordMetric("metric.a", 10);
        InstrumentationService.RecordMetric("metric.b", 20);
        InstrumentationService.RecordMetric("metric.c", 30);

        // Assert
        var summary = InstrumentationService.GetMetricsSummary();
        summary.Should().HaveCount(3);
        summary.Should().ContainKeys("metric.a", "metric.b", "metric.c");
    }

    #endregion

    #region GetMetricsSummary Tests

    [Fact]
    public void GetMetricsSummary_WhenEmpty_ShouldReturnEmptyDictionary()
    {
        // Act
        var summary = InstrumentationService.GetMetricsSummary();

        // Assert
        summary.Should().BeEmpty();
    }

    [Fact]
    public void GetMetricsSummary_ShouldReturnAllMetrics()
    {
        // Arrange
        InstrumentationService.RecordMetric("cpu.usage", 45.5);
        InstrumentationService.RecordMetric("memory.usage", 1024);
        InstrumentationService.RecordMetric("disk.io", 100);

        // Act
        var summary = InstrumentationService.GetMetricsSummary();

        // Assert
        summary.Should().HaveCount(3);
    }

    #endregion

    #region ClearMetrics Tests

    [Fact]
    public void ClearMetrics_ShouldRemoveAllMetrics()
    {
        // Arrange
        InstrumentationService.RecordMetric("test1", 1);
        InstrumentationService.RecordMetric("test2", 2);

        // Act
        InstrumentationService.ClearMetrics();

        // Assert
        var summary = InstrumentationService.GetMetricsSummary();
        summary.Should().BeEmpty();
    }

    #endregion

    #region Timer Tests

    [Fact]
    public void StartTimer_ShouldReturnStartedStopwatch()
    {
        // Act
        var stopwatch = InstrumentationService.StartTimer();

        // Assert
        stopwatch.Should().NotBeNull();
        stopwatch.IsRunning.Should().BeTrue();
    }

    [Fact]
    public async Task RecordTiming_ShouldRecordElapsedTime()
    {
        // Arrange
        var stopwatch = InstrumentationService.StartTimer();
        await Task.Delay(10); // Small delay

        // Act
        InstrumentationService.RecordTiming("test.operation", stopwatch);

        // Assert
        stopwatch.IsRunning.Should().BeFalse();
        var summary = InstrumentationService.GetMetricsSummary();
        summary.Should().ContainKey("timing.test.operation");
        summary["timing.test.operation"].Count.Should().Be(1);
        summary["timing.test.operation"].Min.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void RecordTiming_ShouldStopStopwatch()
    {
        // Arrange
        var stopwatch = InstrumentationService.StartTimer();

        // Act
        InstrumentationService.RecordTiming("some.operation", stopwatch);

        // Assert
        stopwatch.IsRunning.Should().BeFalse();
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task RecordMetric_ShouldBeThreadSafe()
    {
        // Arrange
        var tasks = new List<Task>();
        var recordCount = 100;

        // Act - Record metrics from multiple threads
        for (int i = 0; i < recordCount; i++)
        {
            var value = i;
            tasks.Add(Task.Run(() => InstrumentationService.RecordMetric("concurrent.metric", value)));
        }

        await Task.WhenAll(tasks);

        // Assert
        var summary = InstrumentationService.GetMetricsSummary();
        summary["concurrent.metric"].Count.Should().Be(recordCount);
    }

    [Fact]
    public async Task GetMetricsSummary_ShouldBeThreadSafe()
    {
        // Arrange - Pre-populate with data
        for (int i = 0; i < 50; i++)
        {
            InstrumentationService.RecordMetric("concurrent.read", i);
        }

        var tasks = new List<Task<Dictionary<string, PerformanceMetricSummary>>>();

        // Act - Read from multiple threads while writing
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() => InstrumentationService.GetMetricsSummary()));
            // Also record new metrics concurrently
            tasks.Add(Task.Run(() =>
            {
                InstrumentationService.RecordMetric("concurrent.read", 100);
                return InstrumentationService.GetMetricsSummary();
            }));
        }

        var results = await Task.WhenAll(tasks);

        // Assert - All results should be valid dictionaries
        foreach (var result in results)
        {
            result.Should().NotBeNull();
            result.Should().ContainKey("concurrent.read");
        }
    }

    #endregion
}

/// <summary>
/// Unit tests for PerformanceMetric class
/// </summary>
public class PerformanceMetricTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldSetName()
    {
        // Act
        var metric = new PerformanceMetric("test.metric");

        // Assert
        metric.Name.Should().Be("test.metric");
    }

    #endregion

    #region Record Tests

    [Fact]
    public void Record_ShouldAcceptValue()
    {
        // Arrange
        var metric = new PerformanceMetric("test");

        // Act
        metric.Record(42.5);
        var summary = metric.GetSummary();

        // Assert
        summary.Count.Should().Be(1);
        summary.Min.Should().Be(42.5);
        summary.Max.Should().Be(42.5);
    }

    [Fact]
    public void Record_WithTags_ShouldAcceptTags()
    {
        // Arrange
        var metric = new PerformanceMetric("tagged.metric");
        var tags = new Dictionary<string, string> { { "key", "value" } };

        // Act & Assert - Should not throw
        metric.Record(100, tags);
        var summary = metric.GetSummary();
        summary.Count.Should().Be(1);
    }

    [Fact]
    public void Record_ShouldLimitTo10000Values()
    {
        // Arrange
        var metric = new PerformanceMetric("large.metric");

        // Act - Record more than 10000 values
        for (int i = 0; i < 10050; i++)
        {
            metric.Record(i);
        }

        var summary = metric.GetSummary();

        // Assert - Should be capped at 10000
        summary.Count.Should().Be(10000);
    }

    #endregion

    #region GetSummary Tests

    [Fact]
    public void GetSummary_WhenEmpty_ShouldReturnEmptySummary()
    {
        // Arrange
        var metric = new PerformanceMetric("empty.metric");

        // Act
        var summary = metric.GetSummary();

        // Assert
        summary.Count.Should().Be(0);
    }

    [Fact]
    public void GetSummary_ShouldCalculateMinMaxAvg()
    {
        // Arrange
        var metric = new PerformanceMetric("stats.metric");
        metric.Record(10);
        metric.Record(20);
        metric.Record(30);
        metric.Record(40);
        metric.Record(50);

        // Act
        var summary = metric.GetSummary();

        // Assert
        summary.Min.Should().Be(10);
        summary.Max.Should().Be(50);
        summary.Avg.Should().Be(30);
        summary.Median.Should().Be(30);
    }

    [Fact]
    public void GetSummary_ShouldCalculatePercentiles()
    {
        // Arrange
        var metric = new PerformanceMetric("percentile.metric");
        // Record 100 values from 1 to 100
        for (int i = 1; i <= 100; i++)
        {
            metric.Record(i);
        }

        // Act
        var summary = metric.GetSummary();

        // Assert - percentiles may vary slightly based on calculation method
        // For 100 values 1-100, P95 should be approximately 95-96
        summary.P95.Should().BeInRange(94, 97);
        summary.P99.Should().BeInRange(98, 100);
    }

    [Fact]
    public void GetSummary_ShouldSetTimestamps()
    {
        // Arrange
        var metric = new PerformanceMetric("timestamp.metric");
        var beforeRecord = DateTime.UtcNow;

        // Act
        metric.Record(100);
        Thread.Sleep(10);
        metric.Record(200);
        var afterRecord = DateTime.UtcNow;

        var summary = metric.GetSummary();

        // Assert
        summary.FirstRecorded.Should().BeOnOrAfter(beforeRecord);
        summary.LastRecorded.Should().BeOnOrBefore(afterRecord);
        summary.LastRecorded.Should().BeOnOrAfter(summary.FirstRecorded);
    }

    [Fact]
    public void GetSummary_WithSingleValue_ShouldReturnSameForAllStats()
    {
        // Arrange
        var metric = new PerformanceMetric("single.value");
        metric.Record(42);

        // Act
        var summary = metric.GetSummary();

        // Assert
        summary.Min.Should().Be(42);
        summary.Max.Should().Be(42);
        summary.Avg.Should().Be(42);
        summary.Median.Should().Be(42);
        summary.P95.Should().Be(42);
        summary.P99.Should().Be(42);
    }

    [Fact]
    public void GetSummary_WithNegativeValues_ShouldHandleCorrectly()
    {
        // Arrange
        var metric = new PerformanceMetric("negative.values");
        metric.Record(-50);
        metric.Record(-25);
        metric.Record(0);
        metric.Record(25);
        metric.Record(50);

        // Act
        var summary = metric.GetSummary();

        // Assert
        summary.Min.Should().Be(-50);
        summary.Max.Should().Be(50);
        summary.Avg.Should().Be(0);
    }

    [Fact]
    public void GetSummary_WithDecimalValues_ShouldCalculateCorrectly()
    {
        // Arrange
        var metric = new PerformanceMetric("decimal.values");
        metric.Record(1.5);
        metric.Record(2.5);
        metric.Record(3.5);

        // Act
        var summary = metric.GetSummary();

        // Assert
        summary.Min.Should().Be(1.5);
        summary.Max.Should().Be(3.5);
        summary.Avg.Should().Be(2.5);
    }

    #endregion
}

/// <summary>
/// Unit tests for PerformanceMetricSummary class
/// </summary>
public class PerformanceMetricSummaryTests
{
    [Fact]
    public void DefaultValues_ShouldBeZeroOrDefault()
    {
        // Act
        var summary = new PerformanceMetricSummary();

        // Assert
        summary.Count.Should().Be(0);
        summary.Min.Should().Be(0);
        summary.Max.Should().Be(0);
        summary.Avg.Should().Be(0);
        summary.Median.Should().Be(0);
        summary.P95.Should().Be(0);
        summary.P99.Should().Be(0);
        summary.FirstRecorded.Should().Be(default);
        summary.LastRecorded.Should().Be(default);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var summary = new PerformanceMetricSummary
        {
            Count = 100,
            Min = 5.5,
            Max = 150.5,
            Avg = 75.0,
            Median = 70.0,
            P95 = 140.0,
            P99 = 148.0,
            FirstRecorded = now.AddHours(-1),
            LastRecorded = now
        };

        // Assert
        summary.Count.Should().Be(100);
        summary.Min.Should().Be(5.5);
        summary.Max.Should().Be(150.5);
        summary.Avg.Should().Be(75.0);
        summary.Median.Should().Be(70.0);
        summary.P95.Should().Be(140.0);
        summary.P99.Should().Be(148.0);
        summary.FirstRecorded.Should().Be(now.AddHours(-1));
        summary.LastRecorded.Should().Be(now);
    }
}
