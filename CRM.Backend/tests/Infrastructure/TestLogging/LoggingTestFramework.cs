// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

// NOTE: Do not add assembly-level TestFramework attribute here.
// Each test project that wants to use this framework should add its own
// AssemblyInfo.cs with the attribute pointing to this framework type and its assembly name.
// Example: [assembly: Xunit.TestFramework("CRM.Tests.Infrastructure.TestLogging.LoggingTestFramework", "YourAssemblyName")]

namespace CRM.Tests.Infrastructure.TestLogging;

/// <summary>
/// Custom xUnit test framework that automatically logs all test results.
/// This hooks into xUnit's message pipeline to capture all test outcomes.
/// </summary>
public class LoggingTestFramework : XunitTestFramework
{
    public LoggingTestFramework(IMessageSink messageSink) : base(messageSink)
    {
        TestResultLogger.InitializeSession();
    }

    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
    {
        return new LoggingTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
    }
}

/// <summary>
/// Custom test framework executor that wraps the default executor with logging
/// </summary>
public class LoggingTestFrameworkExecutor : XunitTestFrameworkExecutor
{
    public LoggingTestFrameworkExecutor(
        AssemblyName assemblyName,
        ISourceInformationProvider sourceInformationProvider,
        IMessageSink diagnosticMessageSink)
        : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
    {
    }

    protected override void RunTestCases(
        System.Collections.Generic.IEnumerable<IXunitTestCase> testCases,
        IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions)
    {
        // Wrap the message sink to intercept test results
        var loggingMessageSink = new LoggingMessageSink(executionMessageSink);
        base.RunTestCases(testCases, loggingMessageSink, executionOptions);
    }
}

/// <summary>
/// Message sink that intercepts test results and logs them
/// </summary>
public class LoggingMessageSink : LongLivedMarshalByRefObject, IMessageSink
{
    private readonly IMessageSink _innerSink;

    public LoggingMessageSink(IMessageSink innerSink)
    {
        _innerSink = innerSink;
    }

    public bool OnMessage(IMessageSinkMessage message)
    {
        try
        {
            switch (message)
            {
                case ITestPassed passed:
                    TestResultLogger.LogPass(
                        passed.Test.DisplayName,
                        passed.Test.TestCase.TestMethod.TestClass.Class.Name,
                        TimeSpan.FromSeconds((double)passed.ExecutionTime));
                    break;

                case ITestFailed failed:
                    TestResultLogger.LogFail(
                        failed.Test.DisplayName,
                        failed.Test.TestCase.TestMethod.TestClass.Class.Name,
                        CreateExceptionFromMessages(failed.ExceptionTypes, failed.Messages, failed.StackTraces),
                        TimeSpan.FromSeconds((double)failed.ExecutionTime));
                    break;

                case ITestSkipped skipped:
                    TestResultLogger.LogSkip(
                        skipped.Test.DisplayName,
                        skipped.Test.TestCase.TestMethod.TestClass.Class.Name,
                        skipped.Reason);
                    break;

                case ITestAssemblyFinished _:
                    // Save results to file when all tests in assembly complete
                    TestResultLogger.SaveToFile();
                    break;
            }
        }
        catch
        {
            // Don't let logging errors break tests
        }

        return _innerSink.OnMessage(message);
    }

    private static Exception? CreateExceptionFromMessages(string[]? types, string[]? messages, string[]? stackTraces)
    {
        if (types == null || types.Length == 0)
            return null;

        var message = messages?.Length > 0 ? messages[0] : "Unknown error";
        var stackTrace = stackTraces?.Length > 0 ? stackTraces[0] : null;

        return new TestFailureException(types[0], message, stackTrace);
    }
}

/// <summary>
/// Exception wrapper for test failures captured from xUnit messages
/// </summary>
public class TestFailureException : Exception
{
    public string ExceptionTypeName { get; }
    public string? CapturedStackTrace { get; }

    public TestFailureException(string exceptionType, string message, string? stackTrace)
        : base(message)
    {
        ExceptionTypeName = exceptionType;
        CapturedStackTrace = stackTrace;
    }

    public override string? StackTrace => CapturedStackTrace ?? base.StackTrace;
}
