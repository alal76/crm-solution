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
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Unit.Services.Scripting;

/// <summary>
/// Unit tests for <see cref="PythonScriptEngine"/> (REV-STUB-008).
///
/// Historical note: prior to REV-STUB-008, this engine was an always-unavailable stub
/// (<c>IsAvailable</c> hard-coded to <c>false</c>, constructed with only an
/// <see cref="Microsoft.Extensions.Logging.ILogger{TCategoryName}"/>). It now delegates
/// execution to the crm-python-script-runner sidecar over HTTP (see
/// <c>python-script-runner/</c> at the repository root), mirroring
/// <c>TypeScriptScriptEngine</c>'s HTTP-delegation shape. These tests were rewritten to
/// construct the engine with a mocked <see cref="IHttpClientFactory"/> so they never
/// require the real sidecar process or network access — see
/// <c>CRM.Tests.Services.PythonScriptEngineTests</c> (tests/CRM.Tests project) for the
/// full mocked-sidecar coverage; this file keeps a smaller, focused set of scenarios in
/// the historical location so the project continues to build.
/// </summary>
public class PythonScriptEngineTests
{
    private static Dictionary<string, object?> Empty() => new();

    private static PythonScriptEngine CreateEngine(HttpStatusCode statusCode, string body)
    {
        var handler = new FakeHandler(statusCode, body);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:4001") };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("crm-python-script-runner")).Returns(httpClient);

        var options = Options.Create(new PythonScriptEngineOptions
        {
            BaseUrl = "http://localhost:4001",
            HttpTimeout = TimeSpan.FromSeconds(5),
            HealthCheckTimeout = TimeSpan.FromMilliseconds(500),
        });

        return new PythonScriptEngine(factoryMock.Object, options, NullLogger<PythonScriptEngine>.Instance);
    }

    private static PythonScriptEngine CreateUnreachableEngine()
    {
        var handler = new ThrowingHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:4001") };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("crm-python-script-runner")).Returns(httpClient);

        var options = Options.Create(new PythonScriptEngineOptions
        {
            BaseUrl = "http://localhost:4001",
            HttpTimeout = TimeSpan.FromSeconds(5),
            HealthCheckTimeout = TimeSpan.FromMilliseconds(500),
        });

        return new PythonScriptEngine(factoryMock.Object, options, NullLogger<PythonScriptEngine>.Instance);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Language / availability
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Language_ShouldReturnPython()
    {
        var engine = CreateEngine(HttpStatusCode.OK, """{"status":"ok"}""");
        engine.Language.Should().Be(ScriptLanguage.Python);
    }

    [Fact]
    public void IsAvailable_ShouldReturnFalse_WhenSidecarIsUnreachable()
    {
        var engine = CreateUnreachableEngine();
        engine.IsAvailable.Should().BeFalse("no sidecar is listening in the unit test environment");
    }

    [Fact]
    public void IsAvailable_ShouldReturnTrue_WhenSidecarHealthCheckSucceeds()
    {
        var engine = CreateEngine(HttpStatusCode.OK, """{"status":"ok"}""");
        engine.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void PythonScriptEngine_Should_ImplementIScriptEngine()
    {
        var engine = CreateEngine(HttpStatusCode.OK, """{"status":"ok"}""");
        engine.Should().BeAssignableTo<IScriptEngine>();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ExecuteAsync — graceful failure when sidecar unavailable
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenSidecarIsUnavailable()
    {
        var engine = CreateUnreachableEngine();

        var result = await engine.ExecuteAsync("print('hello')", Empty(), Empty());

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ExecuteAsync_ErrorMessage_ShouldMentionSidecar_WhenUnavailable()
    {
        var engine = CreateUnreachableEngine();

        var result = await engine.ExecuteAsync("x = 1", Empty(), Empty());

        result.ErrorMessage.Should().ContainEquivalentOf("sidecar",
            because: "users need to know the Python sidecar is what's unavailable");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenCodeIsNull()
    {
        var engine = CreateUnreachableEngine();

        var act = () => engine.ExecuteAsync(null!, Empty(), Empty());

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCompleteGracefully_WhenCancellationTokenIsAlreadyCancelled()
    {
        var engine = CreateUnreachableEngine();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await engine.ExecuteAsync("pass", Empty(), Empty(), cancellationToken: cts.Token);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSucceed_WhenSidecarRespondsOk()
    {
        var engine = CreateEngine(HttpStatusCode.OK, """{"success":true,"result":5,"logs":[],"error":null,"durationMs":10}""");

        var result = await engine.ExecuteAsync("result = 2 + 3", Empty(), Empty());

        result.Success.Should().BeTrue();
        result.ReturnValue.Should().Be(5L);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ValidateSyntaxAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ValidateSyntaxAsync_ShouldReturnDiagnostic_WhenSidecarIsUnavailable()
    {
        var engine = CreateUnreachableEngine();

        var diagnostics = await engine.ValidateSyntaxAsync("def foo(): pass");

        diagnostics.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ValidateSyntaxAsync_Diagnostic_ShouldHaveErrorSeverity_WhenSidecarIsUnavailable()
    {
        var engine = CreateUnreachableEngine();

        var diagnostics = await engine.ValidateSyntaxAsync("def foo(): pass");

        diagnostics.Should().ContainSingle(d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public async Task ValidateSyntaxAsync_ShouldThrow_WhenCodeIsNull()
    {
        var engine = CreateUnreachableEngine();

        var act = () => engine.ValidateSyntaxAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ValidateSyntaxAsync_ShouldReturnEmpty_WhenSidecarReportsValid()
    {
        var engine = CreateEngine(HttpStatusCode.OK, """{"valid":true,"diagnostics":[]}""");

        var diagnostics = await engine.ValidateSyntaxAsync("result = 1");

        diagnostics.Should().BeEmpty();
    }
}

// ── Test helpers ──────────────────────────────────────────────────────────────

internal sealed class FakeHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _body;

    public FakeHandler(HttpStatusCode statusCode, string body)
    {
        _statusCode = statusCode;
        _body = body;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(new HttpResponseMessage(_statusCode) { Content = new StringContent(_body, Encoding.UTF8, "application/json") });
}

internal sealed class ThrowingHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => throw new HttpRequestException("connection refused (no sidecar listening in test environment)");
}
