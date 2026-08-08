// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Enums;
using CRM.Core.Interfaces.Scripting;
using CRM.Infrastructure.Scripting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// REV-STUB-008: Unit tests for <see cref="PythonScriptEngine"/>.
/// Mocks the crm-python-script-runner sidecar HTTP calls (mirroring
/// <c>TypeScriptScriptEngineTests</c>'s <c>MockHttpMessageHandler</c> pattern)
/// so these tests never require the real sidecar process or network access.
/// </summary>
public class PythonScriptEngineTests
{
    private static Dictionary<string, object?> Empty() => new();

    private static PythonScriptEngine CreateEngine(
        HttpMessageHandler handler,
        string baseUrl = "http://localhost:4001")
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("crm-python-script-runner")).Returns(httpClient);

        var options = Options.Create(new PythonScriptEngineOptions
        {
            BaseUrl = baseUrl,
            HttpTimeout = TimeSpan.FromSeconds(5),
            HealthCheckTimeout = TimeSpan.FromMilliseconds(500),
            DefaultScriptTimeout = TimeSpan.FromSeconds(5),
            DefaultMemoryLimitMb = 64,
        });

        return new PythonScriptEngine(factoryMock.Object, options, NullLogger<PythonScriptEngine>.Instance);
    }

    // ── Language ────────────────────────────────────────────────────────────

    [Fact]
    public void Language_ShouldReturnPython()
    {
        var engine = CreateEngine(new PythonSidecarStubHandler(HttpStatusCode.OK, """{"status":"ok","version":"1.0.0"}"""));
        Assert.Equal(ScriptLanguage.Python, engine.Language);
    }

    [Fact]
    public void PythonScriptEngine_ShouldImplementIScriptEngine()
    {
        var engine = CreateEngine(new PythonSidecarStubHandler(HttpStatusCode.OK, """{"status":"ok"}"""));
        Assert.IsAssignableFrom<IScriptEngine>(engine);
    }

    // ── IsAvailable (health check) ─────────────────────────────────────────

    [Fact]
    public void IsAvailable_ShouldReturnTrue_WhenSidecarHealthCheckSucceeds()
    {
        var engine = CreateEngine(new PythonSidecarStubHandler(HttpStatusCode.OK, """{"status":"ok","version":"1.0.0"}"""));
        Assert.True(engine.IsAvailable);
    }

    [Fact]
    public void IsAvailable_ShouldReturnFalse_WhenSidecarReturnsError()
    {
        var engine = CreateEngine(new PythonSidecarStubHandler(HttpStatusCode.InternalServerError, "boom"));
        Assert.False(engine.IsAvailable);
    }

    [Fact]
    public void IsAvailable_ShouldReturnFalse_WhenSidecarIsUnreachable()
    {
        var engine = CreateEngine(new PythonSidecarThrowingHandler(new HttpRequestException("connection refused")));
        Assert.False(engine.IsAvailable);
    }

    [Fact]
    public void IsAvailable_ShouldReturnFalse_WhenHealthCheckTimesOut()
    {
        var engine = CreateEngine(new PythonSidecarHangingHandler());
        Assert.False(engine.IsAvailable);
    }

    // ── ExecuteAsync — success ─────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenSidecarRespondsOk()
    {
        var engine = CreateEngine(new PythonSidecarStubHandler(
            HttpStatusCode.OK,
            """{"success":true,"result":8,"logs":[],"error":null,"durationMs":12}"""));

        var result = await engine.ExecuteAsync("result = x + y", new Dictionary<string, object?> { ["x"] = 5, ["y"] = 3 }, Empty());

        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMapReturnValue_FromSidecarResult()
    {
        var engine = CreateEngine(new PythonSidecarStubHandler(
            HttpStatusCode.OK,
            """{"success":true,"result":8,"logs":[],"error":null,"durationMs":12}"""));

        var result = await engine.ExecuteAsync("result = 8", Empty(), Empty());

        Assert.Equal(8L, result.ReturnValue);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCaptureLogs_FromSidecarResponse()
    {
        var engine = CreateEngine(new PythonSidecarStubHandler(
            HttpStatusCode.OK,
            """{"success":true,"result":null,"logs":["hello from python"],"error":null,"durationMs":5}"""));

        var result = await engine.ExecuteAsync("print('hello from python')", Empty(), Empty());

        Assert.Contains("hello from python", result.Logs);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMapDurationFromSidecar_WhenProvided()
    {
        var engine = CreateEngine(new PythonSidecarStubHandler(
            HttpStatusCode.OK,
            """{"success":true,"result":1,"logs":[],"error":null,"durationMs":250}"""));

        var result = await engine.ExecuteAsync("result = 1", Empty(), Empty());

        Assert.Equal(TimeSpan.FromMilliseconds(250), result.ExecutionTime);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnDictResult_WhenSidecarReturnsObject()
    {
        var engine = CreateEngine(new PythonSidecarStubHandler(
            HttpStatusCode.OK,
            """{"success":true,"result":{"name":"Test","items":[1,2,3]},"logs":[],"error":null,"durationMs":5}"""));

        var result = await engine.ExecuteAsync("result = {...}", Empty(), Empty());

        var dict = Assert.IsAssignableFrom<IDictionary<string, object?>>(result.ReturnValue);
        Assert.Equal("Test", dict["name"]);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnScriptFailure_WhenSidecarReportsSuccessFalse()
    {
        var engine = CreateEngine(new PythonSidecarStubHandler(
            HttpStatusCode.OK,
            """{"success":false,"result":null,"logs":[],"error":"NameError: name 'z' is not defined","durationMs":3}"""));

        var result = await engine.ExecuteAsync("result = z", Empty(), Empty());

        Assert.False(result.Success);
        Assert.Contains("NameError", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenCodeIsNull()
    {
        var engine = CreateEngine(new PythonSidecarStubHandler(HttpStatusCode.OK, "{}"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => engine.ExecuteAsync(null!, Empty(), Empty()));
    }

    // ── ExecuteAsync — sidecar unavailable ──────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenSidecarIsUnreachable()
    {
        var engine = CreateEngine(new PythonSidecarThrowingHandler(new HttpRequestException("connection refused")));

        var result = await engine.ExecuteAsync("result = 1", Empty(), Empty());

        Assert.False(result.Success);
        Assert.Contains("unavailable", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenSidecarReturnsServerError()
    {
        var engine = CreateEngine(new PythonSidecarStubHandler(HttpStatusCode.InternalServerError, """{"error":"internal failure"}"""));

        var result = await engine.ExecuteAsync("result = 1", Empty(), Empty());

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }

    // ── ExecuteAsync — timeout ───────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenSidecarCallTimesOut()
    {
        var engine = CreateEngine(new PythonSidecarHangingHandler());

        var result = await engine.ExecuteAsync(
            "while True: pass",
            Empty(),
            Empty(),
            timeout: TimeSpan.FromMilliseconds(100));

        Assert.False(result.Success);
        Assert.Contains("timed out", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    // ── ExecuteAsync — pre-flight ScriptSecurityPolicy rejection ────────────

    [Fact]
    public async Task ExecuteAsync_ShouldRejectBeforeCallingSidecar_WhenPreflightSecurityPolicyFlagsCode()
    {
        var spyHandler = new PythonSidecarSpyHandler(HttpStatusCode.OK, """{"success":true,"result":1,"logs":[],"error":null,"durationMs":1}""");
        var engine = CreateEngine(spyHandler);

        // ScriptSecurityPolicy.IsDataExfiltrationRisk flags source containing "socket"
        // (case-insensitive) as a data-exfiltration risk — this must be caught before
        // the HTTP call to the sidecar is ever made.
        var result = await engine.ExecuteAsync("import socket\nresult = 1", Empty(), Empty());

        Assert.False(result.Success);
        Assert.Contains("pre-flight", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, spyHandler.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRejectBeforeCallingSidecar_WhenSourceReferencesHttpClient()
    {
        var spyHandler = new PythonSidecarSpyHandler(HttpStatusCode.OK, """{"success":true,"result":1,"logs":[],"error":null,"durationMs":1}""");
        var engine = CreateEngine(spyHandler);

        var result = await engine.ExecuteAsync("result = 'HttpClient exfiltration attempt'", Empty(), Empty());

        Assert.False(result.Success);
        Assert.Equal(0, spyHandler.CallCount);
    }

    // ── ValidateSyntaxAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task ValidateSyntaxAsync_ShouldReturnEmpty_WhenSidecarReportsValid()
    {
        var engine = CreateEngine(new PythonSidecarStubHandler(HttpStatusCode.OK, """{"valid":true,"diagnostics":[]}"""));

        var diagnostics = await engine.ValidateSyntaxAsync("result = 1 + 1");

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task ValidateSyntaxAsync_ShouldReturnDiagnostics_WhenSidecarReportsInvalid()
    {
        var engine = CreateEngine(new PythonSidecarStubHandler(
            HttpStatusCode.OK,
            """{"valid":false,"diagnostics":[{"line":1,"column":0,"message":"Import of 'os' is forbidden","severity":"Error"}]}"""));

        var diagnostics = await engine.ValidateSyntaxAsync("import os");

        Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
    }

    [Fact]
    public async Task ValidateSyntaxAsync_ShouldReturnDiagnostic_WhenSidecarIsUnreachable()
    {
        var engine = CreateEngine(new PythonSidecarThrowingHandler(new HttpRequestException("connection refused")));

        var diagnostics = await engine.ValidateSyntaxAsync("result = 1");

        Assert.NotEmpty(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
    }

    [Fact]
    public async Task ValidateSyntaxAsync_ShouldThrow_WhenCodeIsNull()
    {
        var engine = CreateEngine(new PythonSidecarStubHandler(HttpStatusCode.OK, "{}"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => engine.ValidateSyntaxAsync(null!));
    }
}

// ── Test helpers ──────────────────────────────────────────────────────────────

/// <summary>Returns a fixed HTTP response for every request.</summary>
internal sealed class PythonSidecarStubHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _body;

    public PythonSidecarStubHandler(HttpStatusCode statusCode, string body)
    {
        _statusCode = statusCode;
        _body = body;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(new HttpResponseMessage(_statusCode) { Content = new StringContent(_body, Encoding.UTF8, "application/json") });
}

/// <summary>Like <see cref="PythonSidecarStubHandler"/> but records how many times it was invoked, to assert the sidecar was never called.</summary>
internal sealed class PythonSidecarSpyHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _body;

    public int CallCount { get; private set; }

    public PythonSidecarSpyHandler(HttpStatusCode statusCode, string body)
    {
        _statusCode = statusCode;
        _body = body;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CallCount++;
        return Task.FromResult(new HttpResponseMessage(_statusCode) { Content = new StringContent(_body, Encoding.UTF8, "application/json") });
    }
}

/// <summary>Simulates the sidecar being completely unreachable (connection refused).</summary>
internal sealed class PythonSidecarThrowingHandler : HttpMessageHandler
{
    private readonly Exception _exception;

    public PythonSidecarThrowingHandler(Exception exception) => _exception = exception;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => throw _exception;
}

/// <summary>Simulates a sidecar that never responds, to exercise timeout handling.</summary>
internal sealed class PythonSidecarHangingHandler : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.Infinite, cancellationToken);
        throw new InvalidOperationException("unreachable");
    }
}
