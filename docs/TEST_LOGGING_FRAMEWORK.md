# Test Logging Framework Implementation Guide

## Overview

The CRM solution now includes a comprehensive test result logging framework that:

1. **Captures all test outcomes** (Pass/Fail/Skip) with detailed information
2. **Stores results** in JSON format for UI consumption
3. **Provides dashboard UI** to view test results
4. **Integrates with build pipeline** to generate reports on every build
5. **Auto-purges old logs** to keep logs directory clean

## Components

### 1. TestResultLogger (Core Logging Service)

**Location:** `CRM.Backend/tests/Infrastructure/TestLogging/TestResultLogger.cs`

The centralized logging service that:
- Captures test results (pass/fail/skip with details)
- Writes to JSON files in `logs/test-results/` directory
- Provides summary statistics
- Auto-purges old logs (keeps last 10)

**Usage:**
```csharp
// Initialize (auto-called, but explicit is fine)
TestResultLogger.InitializeSession();

// Log a passing test
TestResultLogger.LogPass("TestName", "ClassName", duration: TimeSpan.FromMilliseconds(100));

// Log a failing test
TestResultLogger.LogFail("TestName", "ClassName", exception: ex, duration: TimeSpan.FromMilliseconds(250));

// Log a skipped test
TestResultLogger.LogSkip("TestName", "ClassName", reason: "API not available");

// Get summary
var summary = TestResultLogger.GetSummary();
Console.WriteLine($"Tests: {summary.TotalTests}, Passed: {summary.PassedTests}");
```

### 2. LoggedTestBase (Base Class for Tests)

**Location:** `CRM.Backend/tests/Infrastructure/TestLogging/LoggedTestBase.cs`

Abstract base class providing automatic try-catch wrapping for tests.

**Usage Option 1 - Inherit from Base:**
```csharp
public class MyServiceTests : LoggedTestBase
{
    public MyServiceTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void MyTest_Should_DoSomething()
    {
        RunTest(() =>
        {
            // Your test code here
            var result = MyService.DoSomething();
            result.Should().Be(expected);
        });
    }

    [Fact]
    public async Task MyAsyncTest_Should_DoSomethingAsync()
    {
        await RunTestAsync(async () =>
        {
            // Your async test code here
            var result = await MyService.DoSomethingAsync();
            result.Should().Be(expected);
        });
    }
}
```

**Usage Option 2 - Extension Method (No Base Class Needed):**
```csharp
public class MyTest
{
    [Fact]
    public void Test_Should_Pass()
    {
        (() =>
        {
            // Your test code
            Assert.True(1 == 1);
        }).WithLogging("Test_Should_Pass", "MyTest");
    }

    [Fact]
    public async Task TestAsync_Should_Pass()
    {
        await (async () =>
        {
            // Your async test code
            await Task.Delay(10);
            Assert.True(1 == 1);
        }).WithLoggingAsync("TestAsync_Should_Pass", "MyTest");
    }
}
```

### 3. LoggingTestFramework (xUnit Integration)

**Location:** `CRM.Backend/tests/Infrastructure/TestLogging/LoggingTestFramework.cs`

Custom xUnit test framework that automatically intercepts all test results regardless of how they're written.

**How it works:**
- Hooks into xUnit's message pipeline (IMessageSink)
- Captures `ITestPassed`, `ITestFailed`, `ITestSkipped` events
- Logs each result to `TestResultLogger`
- Transparent - no code changes needed

**Activation:**
- Automatically activated via assembly attribute:
  ```csharp
  [assembly: Xunit.TestFramework("CRM.Tests.Infrastructure.TestLogging.LoggingTestFramework", "CRM.Tests")]
  ```

### 4. Test Results API Controller

**Location:** `CRM.Backend/src/CRM.Api/Controllers/TestResultsController.cs`

REST API endpoints for accessing test results:
- `GET /api/test-results/latest` - Get latest test run summary
- `GET /api/test-results/session/{sessionId}` - Get specific session results
- `GET /api/test-results/sessions` - List all available test sessions

**No authentication required** for testing dashboard access.

### 5. Test Results Dashboard UI

**Location:** `CRM.Frontend/src/pages/TestResultsPage.tsx`

React component providing:
- Summary cards (total, passed, failed, skipped)
- Pass rate visualization
- Filterable test results table
- Expandable detail rows (exceptions, stack traces)
- Auto-refresh every 30 seconds
- Response to filter clicks

**Navigation:** Add to your router:
```tsx
import TestResultsPage from './pages/TestResultsPage';

<Route path="/test-results" element={<TestResultsPage />} />
```

### 6. Test Results Processor Script

**Location:** `CRM.Backend/tests/process-test-results.sh`

Bash script that:
- Reads latest `.trx` files from test runs
- Aggregates results into JSON
- Generates HTML report
- Purges old logs (keeps last 20)

**Called automatically** by `run-tests.sh` after all tests execute.

## Directory Structure

```
logs/
└── test-results/
    ├── latest-test-results.json      # Latest run (always overwritten)
    ├── test-results-20260217-123456.json  # Timestamped archive
    ├── test-results.html             # HTML report
    └── ...older files...
```

## Workflow

### During Development

1. **Run tests:**
   ```bash
   cd CRM.Backend/tests
   ./run-tests.sh
   ```

2. **Results automatically logged** to `logs/test-results/latest-test-results.json`

3. **View results** in multiple ways:
   - Browser: `http://localhost:3000/test-results` (dashboard)
   - JSON: `http://localhost:5000/api/test-results/latest` (API)
   - HTML: `logs/test-results/test-results.html` (static report)

### In Build Pipeline

Add to your CI/CD pipeline after test execution:
```bash
# Run tests
dotnet test CRM.Backend/tests/CRM.Tests.csproj

# Process results and generate reports
bash CRM.Backend/tests/process-test-results.sh
```

The pipeline can then:
- Archive the JSON results
- Generate notifications based on pass rate
- Track test trends over time
- Block deployment if pass rate drops below threshold

## Migration Guide: Converting Existing Tests

### Option A: Use Extended Base Class

Before:
```csharp
[Fact]
public void Test_ShouldPass()
{
    var result = MyService.DoSomething();
    result.Should().Be(expected);
}
```

After:
```csharp
public class MyServiceTests : LoggedTestBase
{
    [Fact]
    public void Test_ShouldPass()
    {
        RunTest(() =>
        {
            var result = MyService.DoSomething();
            result.Should().Be(expected);
        });
    }
}
```

### Option B: Use Extension Method (No Refactoring)

No code changes needed! The `LoggingTestFramework` automatically logs all tests via xUnit's message pipeline.

However, for better control and custom logic, you can add try-catch:

```csharp
[Fact]
public void Test_ShouldPass()
{
    try
    {
        var result = MyService.DoSomething();
        result.Should().Be(expected);
        TestResultLogger.LogPass(nameof(Test_ShouldPass), GetType().FullName);
    }
    catch (Exception ex)
    {
        TestResultLogger.LogFail(nameof(Test_ShouldPass), GetType().FullName, ex);
        throw;
    }
}
```

## JSON Result Format

```json
{
  "sessionId": "test-run-20260217-123456",
  "startTime": "2026-02-17T12:34:56Z",
  "endTime": "2026-02-17T12:35:10Z",
  "totalTests": 750,
  "passedTests": 742,
  "failedTests": 5,
  "skippedTests": 3,
  "totalDuration": "PT14S",
  "passRate": 98.93,
  "results": [
    {
      "sessionId": "test-run-20260217-123456",
      "testName": "MyServiceTests.Test_ShouldPass",
      "className": "CRM.Tests.Services",
      "status": "Passed",
      "duration": "PT0.025S",
      "message": null,
      "exceptionType": null,
      "stackTrace": null,
      "timestamp": "2026-02-17T12:34:56.123Z"
    },
    {
      "sessionId": "test-run-20260217-123456",
      "testName": "MyServiceTests.Test_ShouldFail",
      "className": "CRM.Tests.Services",
      "status": "Failed",
      "duration": "PT0.145S",
      "message": "Expected true, but got false",
      "exceptionType": "AssertionFailedException",
      "stackTrace": "at CRM.Tests.Services.MyServiceTests.Test_ShouldFail() in MyServiceTests.cs:line 42",
      "timestamp": "2026-02-17T12:34:57.456Z"
    }
  ]
}
```

## Features

### Summary Statistics
- **Pass Rate**: Calculated percentage of passed tests
- **Duration**: Total execution time for all tests  
- **Session ID**: Unique identifier for tracking specific test runs

### Log Purging
Old logs are automatically purged:
- Latest JSON: Always overwritten
- Timestamped archives: Last 10 kept
- HTML reports: Regenerated each run

### Dashboard Features
- **Real-time updates**: Auto-refreshes every 30 seconds
- **Filtering**: Click status chips to filter by result type
- **Deep inspection**: Click rows to expand exception details
- **Export ready**: JSON format compatible with external tools
- **Responsive:** Works on desktop, tablet, mobile (Material-UI)

## Troubleshooting

### Logs directory not created
The logs directory is created automatically on first test run.

### Results not showing in dashboard
1. Ensure API is running: `http://localhost:5000/health`
2. Check logs directory exists: `ls logs/test-results/`
3. Trigger test run to generate results: `./run-tests.sh`
4. Refresh browser: `Cmd+R` or `Ctrl+F5`

### XUnit test framework not loading
If you get "could not load assembly" error:
1. Build the test project: `dotnet build CRM.Backend/tests/CRM.Tests.csproj`
2. Ensure `[assembly: Xunit.TestFramework(...)]` is in assembly attributes
3. Clear `.vs` cache: `rm -rf .vs && dotnet clean && dotnet build`

### Performance: Too many log files
Run the purge manually:
```bash
find logs/test-results -name "test-results-*.json" -type f | sort -r | tail -n +11 | xargs rm -f
```

## Best Practices

1. **Use descriptive test names** - They appear in dashboard
   ```csharp
   // Good
   [Fact] public void GetCustomer_WithValidId_ReturnsAccount() { }
   
   // Avoid
   [Fact] public void Test1() { }
   ```

2. **Keep test duration reasonable** - Slow tests should be marked [Fact(Skip = "Performance")]

3. **Use RunTest/RunTestAsync** for consistent logging in inherited classes:
   ```csharp
   public class MyTests : LoggedTestBase
   {
       [Fact]
       public void MyTest()
       {
           RunTest(() => { /* test code */ });
       }
   }
   ```

4. **Check dashboard weekly** - Identify flaky tests and patterns

5. **Archive important results** - Copy session JSON files to your CI/CD artifact storage

## Examples

See the following files for complete examples:

- **Service Tests**: `CRM.Backend/tests/Services/AccountServiceTests.cs`
- **Controller Tests**: `CRM.Backend/tests/Controllers/AccountsControllerTests.cs`
- **Functional Tests**: `CRM.Backend/tests/Functional/ApiEndpointFunctionalTests.cs`
- **Entity Tests**: `CRM.Backend/tests/Entities/EntityValidationTests.cs`

## Version Info

This framework was introduced in **v0.561.0** as part of comprehensive test infrastructure improvements.

- **Test Framework**: xUnit 2.6.2+
- **Storage**: JSON (logs/test-results/)
- **API Version**: REST v1 (/api/test-results/)
- **UI Framework**: React + Material-UI v5

## Support

For issues or enhancements:
1. Check the "common_development_issues.md" document
2. Add new issues to that document for future developers
3. Consider creating helper scripts to automate solutions
