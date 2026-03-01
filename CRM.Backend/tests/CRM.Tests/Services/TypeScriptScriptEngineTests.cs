// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using CRM.Core.Scripting;
using CRM.Infrastructure.Scripting.TypeScript;

namespace CRM.Tests.Services;

/// <summary>
/// SARCH-038: Unit tests for <see cref="TypeScriptScriptEngine"/>.
/// Uses a mock HTTP handler to avoid requiring the sidecar to be running.
/// </summary>
public class TypeScriptScriptEngineTests
{
    private static TypeScriptScriptEngine CreateEngine(
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string? body = null)
    {
        var responseBody = body
            ?? """{"success":true,"output":{},"traceId":"abc123","durationMs":5,"memoryPeakBytes":1048576}""";

        var mockHandler = new MockHttpMessageHandler(statusCode, responseBody);
        var httpClient = new HttpClient(mockHandler)
        {
            BaseAddress = new Uri("http://localhost:4000"),
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock
            .Setup(f => f.CreateClient("crm-script-runner"))
            .Returns(httpClient);

        var options = Options.Create(new TypeScriptScriptEngineOptions
        {
            BaseUrl = "http://localhost:4000",
            HttpTimeout = TimeSpan.FromSeconds(30),
        });

        return new TypeScriptScriptEngine(
            factoryMock.Object,
            options,
            NullLogger<TypeScriptScriptEngine>.Instance);
    }

    // ── Runtime property ─────────────────────────────────────────────────────

    [Fact]
    public void Engine_ShouldHaveTypeScriptRuntime()
    {
        var engine = CreateEngine();
        Assert.Equal(ScriptRuntime.TypeScript, engine.Runtime);
    }

    // ── CompileAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CompileAsync_ShouldSucceed_ReturnsCompiledRef()
    {
        var engine = CreateEngine();
        var def = new ScriptDefinition
        {
            Name = "test",
            Source = "const x = 1;",
            Kind = ScriptKind.Transform,
            Runtime = ScriptRuntime.TypeScript,
        };

        var result = await engine.CompileAsync(def);

        Assert.True(result.Success);
        Assert.NotNull(result.CompiledRef);
        Assert.Equal(def.Id, result.CompiledRef!.ScriptId);
    }

    [Fact]
    public async Task CompileAsync_ShouldReturn64CharHexHash()
    {
        var engine = CreateEngine();
        var def = new ScriptDefinition
        {
            Name = "test",
            Source = "const x = 1;",
            Kind = ScriptKind.Transform,
            Runtime = ScriptRuntime.TypeScript,
        };

        var result = await engine.CompileAsync(def);

        // SHA-256 hex string is 64 characters
        Assert.Equal(64, result.ContentHash.Length);
    }

    [Fact]
    public async Task CompileAsync_ShouldProduceDeterministicHash_ForSameSource()
    {
        var engine = CreateEngine();
        var def = new ScriptDefinition { Name = "a", Source = "const x = 42;" };

        var r1 = await engine.CompileAsync(def);
        var r2 = await engine.CompileAsync(def);

        Assert.Equal(r1.ContentHash, r2.ContentHash);
    }

    [Fact]
    public async Task CompileAsync_ShouldProduceDifferentHashes_ForDifferentSources()
    {
        var engine = CreateEngine();
        var def1 = new ScriptDefinition { Name = "a", Source = "const x = 1;" };
        var def2 = new ScriptDefinition { Name = "b", Source = "const x = 2;" };

        var r1 = await engine.CompileAsync(def1);
        var r2 = await engine.CompileAsync(def2);

        Assert.NotEqual(r1.ContentHash, r2.ContentHash);
    }

    [Fact]
    public async Task CompileAsync_ShouldHaveNoErrorDiagnostics()
    {
        var engine = CreateEngine();
        var def = new ScriptDefinition { Name = "test", Source = "const x = 1;" };

        var result = await engine.CompileAsync(def);

        Assert.Empty(result.Diagnostics);
    }

    // ── ExecuteAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenSidecarRespondsOk()
    {
        var engine = CreateEngine();
        var def = new ScriptDefinition { Name = "test", Source = "const x = 1;" };
        var compiledResult = await engine.CompileAsync(def);
        var ctx = new SimpleScriptContext<object>(new { name = "world" });

        var execResult = await engine.ExecuteAsync<object, object>(compiledResult.CompiledRef!, ctx);

        Assert.True(execResult.Success);
        Assert.Null(execResult.Error);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenSidecarReturnsServerError()
    {
        var engine = CreateEngine(HttpStatusCode.InternalServerError, string.Empty);
        var def = new ScriptDefinition { Name = "test", Source = "const x = 1;" };
        var compiledResult = await engine.CompileAsync(def);
        var ctx = new SimpleScriptContext<object>(new { });

        var execResult = await engine.ExecuteAsync<object, object>(compiledResult.CompiledRef!, ctx);

        Assert.False(execResult.Success);
        Assert.NotNull(execResult.Error);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPopulateTraceId_WhenSidecarReturnsIt()
    {
        var engine = CreateEngine();
        var def = new ScriptDefinition { Name = "test", Source = "const x = 1;" };
        var compiledResult = await engine.CompileAsync(def);
        var ctx = new SimpleScriptContext<object>(new { });

        var execResult = await engine.ExecuteAsync<object, object>(compiledResult.CompiledRef!, ctx);

        Assert.Equal("abc123", execResult.TraceId);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPopulateResourceUsage_WhenMemoryPeakBytesReturned()
    {
        var engine = CreateEngine();
        var def = new ScriptDefinition { Name = "test", Source = "const x = 1;" };
        var compiledResult = await engine.CompileAsync(def);
        var ctx = new SimpleScriptContext<object>(new { });

        var execResult = await engine.ExecuteAsync<object, object>(compiledResult.CompiledRef!, ctx);

        Assert.NotNull(execResult.ResourceUsage);
        Assert.Equal(1_048_576L, execResult.ResourceUsage!.MemoryPeakBytes);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHavePositiveDuration()
    {
        var engine = CreateEngine();
        var def = new ScriptDefinition { Name = "test", Source = "const x = 1;" };
        var compiledResult = await engine.CompileAsync(def);
        var ctx = new SimpleScriptContext<object>(new { });

        var execResult = await engine.ExecuteAsync<object, object>(compiledResult.CompiledRef!, ctx);

        Assert.True(execResult.Duration >= TimeSpan.Zero);
    }

    // ── RunAsync (not supported) ─────────────────────────────────────────────

    [Fact]
    public async Task RunAsync_ShouldThrowNotImplementedException()
    {
        var engine = CreateEngine();
        await Assert.ThrowsAsync<NotImplementedException>(
            () => engine.RunAsync<object, object>("script-id", new { }));
    }
}

// ── Test helpers ──────────────────────────────────────────────────────────────

/// <summary>Minimal <see cref="IScriptContext{TInput}"/> implementation for tests.</summary>
internal sealed class SimpleScriptContext<TInput> : IScriptContext<TInput>
{
    public SimpleScriptContext(TInput input) => Input = input;

    public TInput Input { get; }
    public ExecutionEnvironment Env { get; } = new("test-tenant", "corr-1", "test-caller");
    public IToolInvoker Tools { get; } = null!;
    public System.Collections.ObjectModel.ReadOnlyDictionary<string, object?> Config { get; }
        = new(new System.Collections.Generic.Dictionary<string, object?>());
    public ISecretAccessor Secrets { get; } = null!;
    public IStateAccessor State { get; } = null!;
    public IMetricsRecorder Metrics { get; } = null!;
    public IScriptLogger Logger { get; } = null!;
}

/// <summary>Mock HTTP message handler that returns a fixed response.</summary>
internal sealed class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _body;

    public MockHttpMessageHandler(HttpStatusCode statusCode, string body)
    {
        _statusCode = statusCode;
        _body = body;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
        => Task.FromResult(new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_body, Encoding.UTF8, "application/json"),
        });
}
