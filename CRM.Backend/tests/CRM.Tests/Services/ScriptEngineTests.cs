// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using CRM.Core.Scripting;
using CRM.Infrastructure.Scripting;
using CRM.Infrastructure.Scripting.Roslyn;

namespace CRM.Tests.Services;

/// <summary>Unit tests for <see cref="RoslynScriptEngine"/> — SARCH-029.</summary>
public class RoslynScriptEngineTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static RoslynScriptEngine CreateEngine()
    {
        var cacheMock = new Mock<IDistributedCache>();
        // Cache miss by default → forces compile on every call in tests
        cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
                 .ReturnsAsync((byte[]?)null);
        var store = new ScriptArtefactStore(cacheMock.Object, NullLogger<ScriptArtefactStore>.Instance);
        return new RoslynScriptEngine(store, NullLogger<RoslynScriptEngine>.Instance);
    }

    private static ScriptDefinition Build(string source) => new()
    {
        Name = "TestScript",
        Source = source,
        Kind = ScriptKind.Transform,
        Runtime = ScriptRuntime.DotNet,
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Runtime property
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Engine_ShouldHaveDotNetRuntime()
    {
        var engine = CreateEngine();
        Assert.Equal(ScriptRuntime.DotNet, engine.Runtime);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Compile tests
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CompileAsync_ShouldSucceed_ForValidScript()
    {
        var engine = CreateEngine();
        var result = await engine.CompileAsync(Build("int x = 1 + 1;"));
        Assert.True(result.Success);
        Assert.NotNull(result.CompiledRef);
    }

    [Fact]
    public async Task CompileAsync_ShouldFail_ForInvalidSyntax()
    {
        var engine = CreateEngine();
        var result = await engine.CompileAsync(Build("var x = @@@ invalid +++"));
        Assert.False(result.Success);
        Assert.NotEmpty(result.Diagnostics);
    }

    [Fact]
    public async Task CompileAsync_ShouldReturnContentHash()
    {
        var engine = CreateEngine();
        var result = await engine.CompileAsync(Build("int x = 42;"));
        Assert.NotEmpty(result.ContentHash);
        Assert.Equal(64, result.ContentHash.Length); // SHA-256 hex = 64 chars
    }

    [Fact]
    public async Task CompileAsync_ShouldReturnSameHash_ForIdenticalSource()
    {
        var engine = CreateEngine();
        const string source = "int x = 42;";
        var r1 = await engine.CompileAsync(Build(source));
        var r2 = await engine.CompileAsync(Build(source));
        Assert.Equal(r1.ContentHash, r2.ContentHash);
    }

    [Fact]
    public async Task CompileAsync_ShouldReturnDifferentHash_ForDifferentSource()
    {
        var engine = CreateEngine();
        var r1 = await engine.CompileAsync(Build("int x = 1;"));
        var r2 = await engine.CompileAsync(Build("int y = 2;"));
        Assert.NotEqual(r1.ContentHash, r2.ContentHash);
    }

    [Fact]
    public async Task CompileAsync_ShouldReturnErrorDiagnostics_WhenSyntaxInvalid()
    {
        var engine = CreateEngine();
        var result = await engine.CompileAsync(Build("class {{{ }}}"));
        Assert.True(
            result.Diagnostics.Count > 0,
            "Expected at least one diagnostic for invalid syntax.");
        Assert.Contains(result.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Execute tests
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_ForCompiledScript()
    {
        var engine = CreateEngine();
        var compiled = await engine.CompileAsync(Build("int x = 1;"));
        Assert.NotNull(compiled.CompiledRef);

        var result = await engine.ExecuteAsync<object, object>(compiled.CompiledRef!, null!);
        Assert.True(result.Success);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ScriptDefinition defaults
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ScriptDefinition_ShouldHaveDefaults()
    {
        var def = new ScriptDefinition();
        Assert.Equal("1.0.0", def.Version);
        Assert.Equal(ScriptRuntime.DotNet, def.Runtime);
        Assert.Equal(64, def.MemoryLimitMb);
        Assert.Equal(TimeSpan.FromSeconds(30), def.Timeout);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CompilationResult helpers
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void CompilationResult_Success_ShouldBeFalse_WhenHasErrors()
    {
        var result = new CompilationResult
        {
            CompiledRef = null,
            ContentHash = "abc",
            Diagnostics = new[] { new CRM.Core.Scripting.DiagnosticMessage(DiagnosticSeverity.Error, "E01", "Error", 1, 1) },
        };
        Assert.False(result.Success);
    }

    [Fact]
    public void CompilationResult_Success_ShouldBeTrue_WhenOnlyWarnings()
    {
        var result = new CompilationResult
        {
            CompiledRef = null,
            ContentHash = "abc",
            Diagnostics = new[] { new CRM.Core.Scripting.DiagnosticMessage(DiagnosticSeverity.Warning, "W01", "Warning", 1, 1) },
        };
        Assert.True(result.Success);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Enum value counts (keeps SPEC-GEN-001 in sync)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ScriptLifecycleState_ShouldHave6Values()
        => Assert.Equal(6, Enum.GetValues<ScriptLifecycleState>().Length);

    [Fact]
    public void ScriptKind_ShouldHave6Values()
        => Assert.Equal(6, Enum.GetValues<ScriptKind>().Length);
}
