# Test Logging Framework - Quick Reference Card

## 🚀 Quick Start (30 seconds)

```bash
# 1. Run tests
cd CRM.Backend/tests
./run-tests.sh

# 2. View results (pick one)
# Option A: Web Dashboard (auto-updates every 30s)
http://localhost:3000/test-results

# Option B: REST API
http://localhost:5000/api/test-results/latest

# Option C: Static HTML
open logs/test-results/test-results.html

# Option D: Raw JSON
cat logs/test-results/latest-test-results.json | jq
```

## 📝 Three Ways to Log Tests

### Way 1: Base Class + RunTest() ✨ Recommended
```csharp
public class MyTests : LoggedTestBase
{
    [Fact]
    public void Test_Should_Pass()
    {
        RunTest(() => {
            var result = Service.DoSomething();
            Assert.Equal(expected, result);
        });
    }

    [Fact]
    public async Task Test_Async_Should_Pass()
    {
        await RunTestAsync(async () => {
            var result = await Service.DoSomethingAsync();
            Assert.Equal(expected, result);
        });
    }
}
```

### Way 2: Extension Method (No Refactoring)
```csharp
public class MyTests
{
    [Fact]
    public void Test_Should_Pass()
    {
        (() => {
            Assert.True(1 == 1);
        }).WithLogging("Test_Should_Pass", "MyTests");
    }
}
```

### Way 3: Automatic (Framework Does It) ✅ Zero Code Changes
```csharp
public class MyTests
{
    [Fact]
    public void Test_Should_Pass()
    {
        // LoggingTestFramework automatically logs this
        // No try-catch or base class needed
        Assert.True(1 == 1);
    }
}
```

## 🔧 Available Commands

| Command | Purpose | Location |
|---------|---------|----------|
| `./run-tests.sh` | Run all tests + process results | `CRM.Backend/tests/` |
| `./process-test-results.sh` | Generate reports from TRX | `CRM.Backend/tests/` |
| `./recover-disabled-tests.sh` | Re-enable .disabled test files | `CRM.Backend/tests/` |

## 📊 Result Locations

| Format | Location | Access |
|--------|----------|--------|
| JSON | `logs/test-results/latest-test-results.json` | API or file |
| HTML | `logs/test-results/test-results.html` | Browser |
| Archived | `logs/test-results/test-results-*.json` | API sessions |

## 🎨 Dashboard Features

| Feature | Access | Purpose |
|---------|--------|---------|
| Summary Cards | Top of dashboard | At-a-glance stats |
| Status Filters | Below cards | Filter by result type |
| Results Table | Main section | View all tests + timing |
| Expandable Rows | Click test row | See exception details |
| Auto-Refresh | Runs every 30s | Live updates |

## 📈 JSON Structure

```
{
  "sessionId": "test-run-20260217-123456",
  "totalTests": 750,
  "passedTests": 742,
  "failedTests": 5,
  "skippedTests": 3,
  "passRate": 98.93,
  "results": [
    {
      "testName": "ServiceTests.Test_Should_Pass",
      "className": "CRM.Tests.Services",
      "status": "Passed",
      "duration": "PT0.025S",
      "timestamp": "2026-02-17T12:34:56Z"
    },
    ...
  ]
}
```

## 🆘 Troubleshooting

| Problem | Solution |
|---------|----------|
| Dashboard shows no results | Run `./run-tests.sh` first |
| API returns 404 | Ensure API is running on port 5000 |
| Results directory missing | Auto-created on first test run |
| Old logs taking space | Auto-purged (keeps last 20) |
| Tests won't compile | Run `dotnet clean && dotnet build` |

## 📚 Full Documentation

- **[TEST_LOGGING_FRAMEWORK.md](../docs/TEST_LOGGING_FRAMEWORK.md)** - Comprehensive guide
- **[TEST_AUTOMATION_INTEGRATION.md](../docs/TEST_AUTOMATION_INTEGRATION.md)** - Integration details
- **[Examples/ServiceTestsWithLoggingExample.cs](../tests/Examples/ServiceTestsWithLoggingExample.cs)** - Code samples

## 🎯 Key Principles

1. **Transparent**: Works with existing tests (no code changes)
2. **Automatic**: Framework logs all tests via xUnit pipeline
3. **Detailed**: Captures exceptions, stack traces, timestamps
4. **Flexible**: Use base class, extension method, or nothing
5. **Observable**: Dashboard + API + JSON files for different needs

## 💡 Pro Tips

```bash
# Run specific test category
./run-tests.sh --filter "Category=Functional"

# Run with verbose output
./run-tests.sh --verbosity detailed

# Skip test build (use cached binaries)
./run-tests.sh --no-build

# Find slow tests
jq '.results | sort_by(.duration | sub("PT"; "") | sub("S"; "") | tonumber) | reverse | .[0:10]' \
  logs/test-results/latest-test-results.json

# Count test results by status
jq '.results | group_by(.status) | map({status: .[0].status, count: length})' \
  logs/test-results/latest-test-results.json

# Generate summary
jq '{total: .totalTests, passed: .passedTests, failed: .failedTests, rate: .passRate}' \
  logs/test-results/latest-test-results.json
```

## 🚀 CI/CD Integration

Add to your pipeline (Azure DevOps / GitHub Actions):

```yaml
- name: Run Tests
  run: |
    cd CRM.Backend/tests
    ./run-tests.sh --timeout 600

- name: Archive Results
  uses: actions/upload-artifact@v3
  with:
    name: test-results
    path: logs/test-results/
  if: always()
```

## ✅ Checklist: Getting Started

- [ ] Read this document (you're here!)
- [ ] Read [TEST_LOGGING_FRAMEWORK.md](../docs/TEST_LOGGING_FRAMEWORK.md)
- [ ] Review [Examples/ServiceTestsWithLoggingExample.cs](../tests/Examples/ServiceTestsWithLoggingExample.cs)
- [ ] Run `./CRM.Backend/tests/run-tests.sh`
- [ ] Open dashboard at `http://localhost:3000/test-results`
- [ ] Try filtering/expanding results
- [ ] Check `logs/test-results/` directory

## 🎓 Learning Path

**Beginner:**
1. Run tests with `./run-tests.sh`
2. View results in dashboard / API
3. Understand JSON format

**Intermediate:**
1. Convert sample tests to use `LoggedTestBase`
2. Use `RunTest()` / `RunTestAsync()` helpers
3. Examine exception details in dashboard

**Advanced:**
1. Create custom test categories
2. Integrate into CI/CD pipeline
3. Build reports from JSON results
4. Track test trends over time

## 📞 Support

- **Issues**: Check [common_development_issues.md](../docs/common_development_issues.md)
- **Questions**: Review TEST_LOGGING_FRAMEWORK.md
- **Examples**: See Examples/ServiceTestsWithLoggingExample.cs
- **API Docs**: Visit `/api/test-results/` endpoints

---

**Last Updated:** February 17, 2026 | **Version:** 0.561.1 | **Framework:** xUnit 2.6.2+
