// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using CRM.Tests.Infrastructure.TestLogging;
using Xunit;
using Xunit.Abstractions;

namespace CRM.Tests.Functional.Examples;

/// <summary>
/// EXAMPLE: Service Tests with Try-Catch Logging Framework
///
/// This file demonstrates best practices for using the new test logging framework:
/// 1. Inherit from LoggedTestBase
/// 2. Use RunTest() / RunTestAsync() helpers
/// 3. Tests automatically logged with pass/fail/skip status
/// 4. Detailed exception information captured
/// 5. Execution time tracked
///
/// Copy this pattern to your own test files.
/// </summary>
public class ServiceTestsWithLoggingExample : LoggedTestBase
{
    private readonly ITestOutputHelper _output;

    public ServiceTestsWithLoggingExample(ITestOutputHelper output) : base(output)
    {
        _output = output;
    }

    #region Synchronous Test Examples

    [Fact]
    [Trait("Category", "Example")]
    public void Example_SimpleTest_ShouldPass()
    {
        RunTest(() =>
        {
            // Arrange
            var expected = 42;

            // Act
            var actual = 21 + 21;

            // Assert
            Assert.Equal(expected, actual);
        });
    }

    [Fact]
    [Trait("Category", "Example")]
    public void Example_TestWithDetailedAssertion_ShouldPass()
    {
        RunTest(() =>
        {
            // Arrange
            var user = new { Name = "John", Email = "john@example.com" };

            // Act
            var result = user.Name.ToUpper();

            // Assert
            Assert.Equal("JOHN", result);
            Assert.Contains("@", user.Email);
        });
    }

    [Fact(Skip = "Intentionally failing example - for demo purposes")]
    [Trait("Category", "Example")]
    public void Example_TestWithException_ShouldFail()
    {
        // This test demonstrates how exceptions appear in test logs
        // The Skip attribute prevents it from running in CI
        var exception = Record.Exception((Action)(() =>
            throw new InvalidOperationException("Simulated failure for demo purposes")));

        Assert.NotNull(exception); // Assert: exception was captured as expected
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    [Trait("Category", "Example")]
    public void Example_TestWithConditionalSkip_BasedOnCondition()
    {
        bool shouldSkip = true;
        string skipReason = "Demo API not available";

        RunTestWithSkipCondition(shouldSkip, skipReason, () =>
        {
            // This code won't execute when shouldSkip=true
            // The test will be marked as "Skipped" in results
            Assert.Fail("This should not run");
        });
        Assert.True(shouldSkip, "Test is configured to skip in this example scenario");
    }

    #endregion

    #region Asynchronous Test Examples

    [Fact]
    [Trait("Category", "Example")]
    public async System.Threading.Tasks.Task Example_AsyncTest_ShouldPass()
    {
        await RunTestAsync(async () =>
        {
            // Arrange
            var delayMs = 10;

            // Act
            await System.Threading.Tasks.Task.Delay(delayMs);

            // Assert
            Assert.True(true); // Dummy assertion
        });
    }

    [Fact]
    [Trait("Category", "Example")]
    public async System.Threading.Tasks.Task Example_AsyncHttpTest_ShouldHandleResponse()
    {
        await RunTestAsync(async () =>
        {
            // Arrange
            using var client = new HttpClient();

            // Act - Simulate HTTP call (won't actually run without real endpoint)
            var content = new { data = "test" };

            // Assert
            Assert.NotNull(content);
        });
    }

    [Fact(Skip = "Intentionally failing example - for demo purposes")]
    [Trait("Category", "Example")]
    public async System.Threading.Tasks.Task Example_AsyncTestWithConditionalSkip_BasedOnEnv()
    {
        bool apiAvailable = false;  // Simulate API check
        string skipReason = "API server not available - skipping integration test";

        await RunTestWithSkipConditionAsync(
            apiAvailable,
            skipReason,
            async () =>
            {
                // This would only run if apiAvailable = true
                await System.Threading.Tasks.Task.Delay(1);
                Assert.True(false);
            }
        );
    }

    #endregion

    #region Data-Driven Test Examples

    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(2, 3, 5)]
    [InlineData(-1, 1, 0)]
    [Trait("Category", "Example")]
    public void Example_DataDrivenTest_Addition(int a, int b, int expected)
    {
        RunTest(() =>
        {
            // Act
            var result = a + b;

            // Assert
            Assert.Equal(expected, result);
        });
    }

    [Theory]
    [InlineData("John", "Doe", "John Doe")]
    [InlineData("Jane", "Smith", "Jane Smith")]
    [InlineData("", "NoFirst", "NoFirst")]
    [Trait("Category", "Example")]
    public void Example_StringManipulation_Concatenation(string first, string last, string expected)
    {
        RunTest(() =>
        {
            // Act
            var result = $"{first} {last}".Trim();

            // Assert
            Assert.Equal(expected, result);
        });
    }

    #endregion

    #region Advanced Patterns

    [Fact]
    [Trait("Category", "Example")]
    public void Example_TestWithDurationTracking_ShowsInResults()
    {
        RunTest(() =>
        {
            // Simulate some work
            var sw = Stopwatch.StartNew();
            System.Threading.Thread.Sleep(50);
            sw.Stop();

            // Duration is automatically captured by RunTest()
            Assert.True(sw.ElapsedMilliseconds >= 50);
            _output.WriteLine($"Operation took {sw.ElapsedMilliseconds}ms");
        });
    }

    [Fact]
    [Trait("Category", "Example")]
    public void Example_TestWithCustomOutput_IntegrationWithLogging()
    {
        RunTest(() =>
        {
            // Test output helper messages appear in both:
            // 1. xUnit console output
            // 2. Test results JSON (if captured)
            _output.WriteLine("This message appears in test output");
            _output.WriteLine("Useful for tracing test execution flow");

            var testData = new { TimeStamp = DateTime.UtcNow };
            _output.WriteLine($"Test data: {JsonSerializer.Serialize(testData)}");

            Assert.NotNull(testData);
        });
    }

    [Fact]
    [Trait("Category", "Example")]
    public void Example_TestWithMultipleAssertions_AllCapturedOnFailure()
    {
        RunTest(() =>
        {
            var person = new { Name = "John", Age = 30, Active = true };

            // All assertions are evaluated
            Assert.NotNull(person);
            Assert.Equal("John", person.Name);
            Assert.True(person.Age > 0);
            Assert.True(person.Active);

            // If any fail, exception is captured with details
        });
    }

    [Fact(Skip = "Intentionally failing example - for demo purposes")]
    [Trait("Category", "Example")]
    public void Example_TestWithManualExceptionHandling_FlexibleApproach()
    {
        var testName = nameof(Example_TestWithManualExceptionHandling_FlexibleApproach);
        var sw = Stopwatch.StartNew();

        try
        {
            // Test code
            var value = int.Parse("not-a-number");  // Will throw
            Assert.Fail("Should not reach here");
        }
        catch (FormatException ex)
        {
            sw.Stop();
            // Can log and decide whether to fail or continue
            TestResultLogger.LogFail(testName, GetType().FullName, ex, sw.Elapsed);
            throw;  // Re-throw to mark test as failed
        }
    }

    #endregion

    #region Result Checking

    [Fact]
    [Trait("Category", "Example")]
    public void Example_GetTestSummary_ShowsCurrentSessionStatistics()
    {
        RunTest(() =>
        {
            // Get current test session summary
            var summary = TestResultLogger.GetSummary();

            // Summary includes:
            // - SessionId: Unique test run ID
            // - Total/Passed/Failed/Skipped counts
            // - Pass rate percentage
            // - All individual test results with timing

            _output.WriteLine($"Session: {summary.SessionId}");
            _output.WriteLine($"Tests run so far: {summary.TotalTests}");
            _output.WriteLine($"Pass rate: {summary.PassRate:F1}%");

            Assert.NotNull(summary);
            Assert.True(summary.TotalTests > 0);
        });
    }

    #endregion
}

/// <summary>
/// Example using Extension Method (No Base Class Required)
/// Demonstrates backward-compatible approach for existing tests
/// </summary>
public class ServiceTestsWithExtensionMethodExample
{
    [Fact]
    [Trait("Category", "Example")]
    public void Example_WithExtensionMethod_NoInheritance()
    {
        Action testAction = () =>
        {
            // Test code here
            var result = 10 + 5;
            Assert.Equal(15, result);
        };
        testAction.WithLogging(
            testName: nameof(Example_WithExtensionMethod_NoInheritance),
            className: nameof(ServiceTestsWithExtensionMethodExample)
        );
    }

    [Fact]
    [Trait("Category", "Example")]
    public async System.Threading.Tasks.Task Example_WithExtensionMethodAsync_NoInheritance()
    {
        Func<System.Threading.Tasks.Task> testAction = async () =>
        {
            // Async test code
            await System.Threading.Tasks.Task.Delay(10);
            Assert.True(true);
        };
        await testAction.WithLoggingAsync(
            testName: nameof(Example_WithExtensionMethodAsync_NoInheritance),
            className: nameof(ServiceTestsWithExtensionMethodExample)
        );
    }
}

/// <summary>
/// Example of Completely Unmodified Test
/// LoggingTestFramework intercepts these automatically
/// No changes needed - all tests logged transparently
/// </summary>
public class VanillaServiceTests
{
    [Fact]
    [Trait("Category", "Example")]
    public void Example_VanillaTest_AutomaticallyLogged()
    {
        // No try-catch needed
        // No base class required
        // No extension methods
        // LoggingTestFramework silently logs result
        var result = 5 + 3;
        Assert.Equal(8, result);
    }

    [Fact]
    [Trait("Category", "Example")]
    public void Example_VanillaTest_WithDetailedAssertions()
    {
        // All assertions tested
        // First failure stops test (as normal)
        // LoggingTestFramework captures the exception
        var data = new List<int> { 1, 2, 3 };
        Assert.NotNull(data);
        Assert.Equal(3, data.Count);
        Assert.Contains(2, data);
    }
}

/// <summary>
/// Sample test data class
/// </summary>
public class SampleTestData
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
}
