// CRM Solution - Test Assembly Info
// Registers the custom test logging framework for automatic test result capture

using Xunit;

// Register custom xUnit framework for this assembly
// This enables automatic try-catch logging of all test results (pass/fail/skip)
[assembly: TestFramework("CRM.Tests.Infrastructure.TestLogging.LoggingTestFramework", "CRM.Tests.Services")]
