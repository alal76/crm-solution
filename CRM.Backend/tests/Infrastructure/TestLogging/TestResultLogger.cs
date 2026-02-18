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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Xunit.Abstractions;

namespace CRM.Tests.Infrastructure.TestLogging;

/// <summary>
/// Centralized test result logger that captures pass/fail/skip status with details.
/// Outputs results to a JSON log file for consumption by the UI dashboard.
/// </summary>
public static class TestResultLogger
{
    private static readonly ConcurrentBag<TestResult> _results = new();
    private static readonly object _writeLock = new();
    private static string? _sessionId;
    private static DateTime _sessionStart;
    private static bool _initialized;

    /// <summary>
    /// Get the logs directory path (repository root/logs/test-results)
    /// </summary>
    public static string LogsDirectory
    {
        get
        {
            // Navigate from tests folder to repository root
            var testsDir = AppDomain.CurrentDomain.BaseDirectory;
            var repoRoot = Path.GetFullPath(Path.Combine(testsDir, "..", "..", "..", "..", ".."));
            return Path.Combine(repoRoot, "logs", "test-results");
        }
    }

    /// <summary>
    /// Get the latest log file path
    /// </summary>
    public static string LatestLogFilePath => Path.Combine(LogsDirectory, "latest-test-results.json");

    /// <summary>
    /// Initialize a new test session
    /// </summary>
    public static void InitializeSession()
    {
        if (_initialized) return;

        _sessionId = $"test-run-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
        _sessionStart = DateTime.UtcNow;
        _initialized = true;

        // Ensure logs directory exists
        Directory.CreateDirectory(LogsDirectory);
    }

    /// <summary>
    /// Log a test result
    /// </summary>
    public static void LogResult(TestResult result)
    {
        InitializeSession();
        result.SessionId = _sessionId;
        _results.Add(result);

        // Write incrementally to avoid data loss
        WriteResults();
    }

    /// <summary>
    /// Log a passed test
    /// </summary>
    public static void LogPass(string testName, string? className = null, TimeSpan? duration = null, string? message = null)
    {
        LogResult(new TestResult
        {
            TestName = testName,
            ClassName = className ?? ExtractClassName(testName),
            Status = TestStatus.Passed,
            Duration = duration ?? TimeSpan.Zero,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Log a failed test
    /// </summary>
    public static void LogFail(string testName, string? className = null, Exception? exception = null, TimeSpan? duration = null, string? message = null)
    {
        LogResult(new TestResult
        {
            TestName = testName,
            ClassName = className ?? ExtractClassName(testName),
            Status = TestStatus.Failed,
            Duration = duration ?? TimeSpan.Zero,
            Message = message ?? exception?.Message,
            ExceptionType = exception?.GetType().Name,
            StackTrace = exception?.StackTrace,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Log a skipped test
    /// </summary>
    public static void LogSkip(string testName, string? className = null, string? reason = null)
    {
        LogResult(new TestResult
        {
            TestName = testName,
            ClassName = className ?? ExtractClassName(testName),
            Status = TestStatus.Skipped,
            Duration = TimeSpan.Zero,
            Message = reason ?? "Test skipped",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get summary statistics
    /// </summary>
    public static TestRunSummary GetSummary()
    {
        var results = _results.ToList();
        return new TestRunSummary
        {
            SessionId = _sessionId ?? "unknown",
            StartTime = _sessionStart,
            EndTime = DateTime.UtcNow,
            TotalTests = results.Count,
            PassedTests = results.Count(r => r.Status == TestStatus.Passed),
            FailedTests = results.Count(r => r.Status == TestStatus.Failed),
            SkippedTests = results.Count(r => r.Status == TestStatus.Skipped),
            TotalDuration = TimeSpan.FromTicks(results.Sum(r => r.Duration.Ticks)),
            Results = results.OrderBy(r => r.ClassName).ThenBy(r => r.TestName).ToList()
        };
    }

    /// <summary>
    /// Save all results to log file (public accessor for WriteResults)
    /// </summary>
    public static void SaveToFile()
    {
        WriteResults();
    }

    /// <summary>
    /// Write results to log file
    /// </summary>
    private static void WriteResults()
    {
        lock (_writeLock)
        {
            try
            {
                var summary = GetSummary();
                var json = JsonSerializer.Serialize(summary, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                // Write to latest file (always overwritten)
                File.WriteAllText(LatestLogFilePath, json);

                // Also write timestamped archive
                var archivePath = Path.Combine(LogsDirectory, $"test-results-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");
                File.WriteAllText(archivePath, json);

                // Purge old archives (keep last 10)
                PurgeOldLogs();
            }
            catch
            {
                // Silently fail - don't break tests due to logging issues
            }
        }
    }

    /// <summary>
    /// Purge old log files, keeping only the latest 10
    /// </summary>
    private static void PurgeOldLogs()
    {
        try
        {
            var logFiles = Directory.GetFiles(LogsDirectory, "test-results-*.json")
                .OrderByDescending(f => f)
                .Skip(10)
                .ToList();

            foreach (var file in logFiles)
            {
                File.Delete(file);
            }
        }
        catch
        {
            // Silently fail
        }
    }

    /// <summary>
    /// Extract class name from fully qualified test name
    /// </summary>
    private static string ExtractClassName(string testName)
    {
        var lastDot = testName.LastIndexOf('.');
        return lastDot > 0 ? testName.Substring(0, lastDot) : testName;
    }

    /// <summary>
    /// Clear all results (for testing purposes)
    /// </summary>
    public static void Clear()
    {
        while (_results.TryTake(out _)) { }
        _initialized = false;
        _sessionId = null;
    }
}

/// <summary>
/// Represents a single test result
/// </summary>
public class TestResult
{
    public string? SessionId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string? ClassName { get; set; }
    public TestStatus Status { get; set; }
    public TimeSpan Duration { get; set; }
    public string? Message { get; set; }
    public string? ExceptionType { get; set; }
    public string? StackTrace { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Test status enumeration
/// </summary>
public enum TestStatus
{
    Passed,
    Failed,
    Skipped
}

/// <summary>
/// Summary of a test run
/// </summary>
public class TestRunSummary
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int TotalTests { get; set; }
    public int PassedTests { get; set; }
    public int FailedTests { get; set; }
    public int SkippedTests { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public double PassRate => TotalTests > 0 ? (double)PassedTests / TotalTests * 100 : 0;
    public List<TestResult> Results { get; set; } = new();
}
