// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Enums;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.Scripting;
using CRM.Infrastructure.AI.SK.Plugins;
using CRM.Infrastructure.Factories;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using NSubstitute;
using Xunit;

namespace CRM.Tests.Unit.Services;

/// <summary>
/// Unit tests for <see cref="ScriptPluginLoader"/> — 4 scenarios.
///
/// IScriptPluginService is mocked with NSubstitute.
/// ScriptEngineFactory is constructed with a single mock IScriptEngine set to
/// JavaScript / IsAvailable=true by default.  Individual tests override
/// IsAvailable to exercise the "skip with warning" path.
/// Kernel is created via Kernel.CreateBuilder().Build() (transitive SK reference
/// from CRM.Infrastructure).
/// </summary>
public sealed class ScriptPluginLoaderTests
{
    private readonly IScriptPluginService _scriptPluginService;
    private readonly IScriptEngine _mockEngine;
    private readonly ScriptPluginLoader _sut;

    public ScriptPluginLoaderTests()
    {
        _scriptPluginService = Substitute.For<IScriptPluginService>();

        _mockEngine = Substitute.For<IScriptEngine>();
        _mockEngine.Language.Returns(ScriptLanguage.JavaScript);
        _mockEngine.IsAvailable.Returns(true);

        // Set up a successful default execution result so KernelFunction delegates
        // do not error if invoked during plugin construction.
        _mockEngine.ExecuteAsync(
                Arg.Any<string>(),
                Arg.Any<Dictionary<string, object?>>(),
                Arg.Any<Dictionary<string, object?>>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ScriptExecutionResult(
                Success: true,
                ReturnValue: string.Empty,
                Logs: Array.Empty<string>(),
                ErrorMessage: null,
                ExecutionTime: TimeSpan.Zero));

        var engineFactory = new ScriptEngineFactory(
            new[] { _mockEngine },
            NullLogger<ScriptEngineFactory>.Instance);

        _sut = new ScriptPluginLoader(
            _scriptPluginService,
            engineFactory,
            NullLogger<ScriptPluginLoader>.Instance);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static ScriptPluginDto MakeDto(
        int id,
        string name,
        ScriptLanguage language = ScriptLanguage.JavaScript) =>
        new(
            Id: id,
            Name: name,
            Description: $"Description for {name}",
            Language: (int)language,
            Code: "return 1;",
            ParameterSchema: null,
            ReturnValueDescription: null,
            IsActive: true,
            Version: 1,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null);

    // ─────────────────────────────────────────────────────────────────────────
    // LoadActivePluginsAsync
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Scenario 1 — Empty array returned when no active plugins exist.</summary>
    [Fact]
    public async Task LoadActivePluginsAsync_ShouldReturnEmptyArray_WhenNoPlugins()
    {
        _scriptPluginService
            .GetAllAsync(includeInactive: false, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<ScriptPluginDto>)Array.Empty<ScriptPluginDto>());

        var result = await _sut.LoadActivePluginsAsync();

        result.Should().BeEmpty();
    }

    /// <summary>Scenario 2 — One KernelPlugin is created for a single active plugin.</summary>
    [Fact]
    public async Task LoadActivePluginsAsync_ShouldReturnOnePlugin_WhenOneActiveExists()
    {
        var dto = MakeDto(id: 1, name: "MyPlugin");

        _scriptPluginService
            .GetAllAsync(includeInactive: false, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<ScriptPluginDto>)new List<ScriptPluginDto> { dto });

        var result = await _sut.LoadActivePluginsAsync();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("MyPlugin");
    }

    /// <summary>
    /// Scenario 3 — Plugin is silently skipped (result is empty) when the script
    /// engine throws InvalidOperationException due to IsAvailable=false.
    /// </summary>
    [Fact]
    public async Task LoadActivePluginsAsync_ShouldSkipPlugin_WhenEngineUnavailable()
    {
        var dto = MakeDto(id: 2, name: "UnavailablePlugin");

        _scriptPluginService
            .GetAllAsync(includeInactive: false, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<ScriptPluginDto>)new List<ScriptPluginDto> { dto });

        // Make the engine report itself as unavailable so GetEngine() throws
        _mockEngine.IsAvailable.Returns(false);

        var result = await _sut.LoadActivePluginsAsync();

        result.Should().BeEmpty();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ImportPluginsIntoKernelAsync
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Scenario 4 — Active plugin is imported into the kernel's Plugins collection.</summary>
    [Fact]
    public async Task ImportPluginsIntoKernelAsync_ShouldImportAllActivePlugins()
    {
        var dto = MakeDto(id: 1, name: "ImportedPlugin");

        _mockEngine.IsAvailable.Returns(true);

        _scriptPluginService
            .GetAllAsync(includeInactive: false, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<ScriptPluginDto>)new List<ScriptPluginDto> { dto });

        var kernel = Kernel.CreateBuilder().Build();

        await _sut.ImportPluginsIntoKernelAsync(kernel);

        kernel.Plugins.Should().HaveCount(1);
        kernel.Plugins["ImportedPlugin"].Should().NotBeNull();
    }
}
