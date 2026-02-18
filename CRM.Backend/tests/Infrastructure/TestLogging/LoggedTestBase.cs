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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace CRM.Tests.Infrastructure.TestLogging;

/// <summary>
/// Base class for tests that provides automatic try-catch logging and result capture.
/// Inherit from this class to get automatic test result logging.
/// </summary>
public abstract class LoggedTestBase
{
    protected readonly ITestOutputHelper? Output;

    protected LoggedTestBase()
    {
        TestResultLogger.InitializeSession();
    }

    protected LoggedTestBase(ITestOutputHelper output)
    {
        Output = output;
        TestResultLogger.InitializeSession();
    }

    /// <summary>
    /// Execute a test with automatic try-catch and logging.
    /// </summary>
    /// <param name="testAction">The test action to execute</param>
    /// <param name="testName">Test name (auto-populated by compiler)</param>
    /// <param name="className">Class name (defaults to this type name)</param>
    protected void RunTest(Action testAction, [CallerMemberName] string testName = "", string? className = null)
    {
        className ??= GetType().FullName;
        var sw = Stopwatch.StartNew();

        try
        {
            testAction();
            sw.Stop();

            TestResultLogger.LogPass(testName, className, sw.Elapsed);
            Output?.WriteLine($"✅ {testName} PASSED ({sw.ElapsedMilliseconds}ms)");
        }
        catch (Exception ex) when (IsSkipException(ex))
        {
            sw.Stop();
            TestResultLogger.LogSkip(testName, className, ex.Message);
            Output?.WriteLine($"⏭️ {testName} SKIPPED: {ex.Message}");
            throw; // Re-throw to let xUnit handle skip
        }
        catch (Exception ex)
        {
            sw.Stop();
            TestResultLogger.LogFail(testName, className, ex, sw.Elapsed);
            Output?.WriteLine($"❌ {testName} FAILED ({sw.ElapsedMilliseconds}ms): {ex.Message}");
            Output?.WriteLine($"   Stack: {ex.StackTrace}");
            throw; // Re-throw to let xUnit mark as failed
        }
    }

    /// <summary>
    /// Execute an async test with automatic try-catch and logging.
    /// </summary>
    protected async Task RunTestAsync(Func<Task> testAction, [CallerMemberName] string testName = "", string? className = null)
    {
        className ??= GetType().FullName;
        var sw = Stopwatch.StartNew();

        try
        {
            await testAction();
            sw.Stop();

            TestResultLogger.LogPass(testName, className, sw.Elapsed);
            Output?.WriteLine($"✅ {testName} PASSED ({sw.ElapsedMilliseconds}ms)");
        }
        catch (Exception ex) when (IsSkipException(ex))
        {
            sw.Stop();
            TestResultLogger.LogSkip(testName, className, ex.Message);
            Output?.WriteLine($"⏭️ {testName} SKIPPED: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            sw.Stop();
            TestResultLogger.LogFail(testName, className, ex, sw.Elapsed);
            Output?.WriteLine($"❌ {testName} FAILED ({sw.ElapsedMilliseconds}ms): {ex.Message}");
            Output?.WriteLine($"   Stack: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Execute a test that might be skipped based on a condition.
    /// </summary>
    protected void RunTestWithSkipCondition(bool shouldSkip, string skipReason, Action testAction,
        [CallerMemberName] string testName = "", string? className = null)
    {
        className ??= GetType().FullName;

        if (shouldSkip)
        {
            TestResultLogger.LogSkip(testName, className, skipReason);
            Output?.WriteLine($"⏭️ {testName} SKIPPED: {skipReason}");
            return;
        }

        RunTest(testAction, testName, className);
    }

    /// <summary>
    /// Execute an async test that might be skipped based on a condition.
    /// </summary>
    protected async Task RunTestWithSkipConditionAsync(bool shouldSkip, string skipReason, Func<Task> testAction,
        [CallerMemberName] string testName = "", string? className = null)
    {
        className ??= GetType().FullName;

        if (shouldSkip)
        {
            TestResultLogger.LogSkip(testName, className, skipReason);
            Output?.WriteLine($"⏭️ {testName} SKIPPED: {skipReason}");
            return;
        }

        await RunTestAsync(testAction, testName, className);
    }

    /// <summary>
    /// Check if an exception is a skip exception (xUnit Skip)
    /// </summary>
    private static bool IsSkipException(Exception ex)
    {
        return ex.GetType().Name.Contains("Skip", StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Extension methods for wrapping existing tests with try-catch logging
/// </summary>
public static class TestLoggingExtensions
{
    /// <summary>
    /// Wrap a test action with try-catch logging without using base class
    /// </summary>
    public static void WithLogging(this Action testAction, string testName, string? className = null, ITestOutputHelper? output = null)
    {
        TestResultLogger.InitializeSession();
        var sw = Stopwatch.StartNew();

        try
        {
            testAction();
            sw.Stop();
            TestResultLogger.LogPass(testName, className, sw.Elapsed);
            output?.WriteLine($"✅ {testName} PASSED ({sw.ElapsedMilliseconds}ms)");
        }
        catch (Exception ex)
        {
            sw.Stop();
            TestResultLogger.LogFail(testName, className, ex, sw.Elapsed);
            output?.WriteLine($"❌ {testName} FAILED ({sw.ElapsedMilliseconds}ms): {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Wrap an async test action with try-catch logging without using base class
    /// </summary>
    public static async Task WithLoggingAsync(this Func<Task> testAction, string testName, string? className = null, ITestOutputHelper? output = null)
    {
        TestResultLogger.InitializeSession();
        var sw = Stopwatch.StartNew();

        try
        {
            await testAction();
            sw.Stop();
            TestResultLogger.LogPass(testName, className, sw.Elapsed);
            output?.WriteLine($"✅ {testName} PASSED ({sw.ElapsedMilliseconds}ms)");
        }
        catch (Exception ex)
        {
            sw.Stop();
            TestResultLogger.LogFail(testName, className, ex, sw.Elapsed);
            output?.WriteLine($"❌ {testName} FAILED ({sw.ElapsedMilliseconds}ms): {ex.Message}");
            throw;
        }
    }
}
