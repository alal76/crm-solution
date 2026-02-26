// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities.AI;
using CRM.Core.Enums;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.Scripting;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Factories;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace CRM.Tests.Unit.Services;

/// <summary>
/// Unit tests for <see cref="ScriptPluginService"/> — 10 scenarios.
///
/// Uses an EF Core InMemory database so real DbSet/SaveChanges semantics work
/// without needing to mock the complex ICrmDbContext interface members.
/// ScriptEngineFactory is constructed with an empty engine list — CRUD methods
/// do not invoke the engine, so no engine availability is required.
/// </summary>
public sealed class ScriptPluginServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly ScriptPluginService _sut;

    public ScriptPluginServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // IConfiguration is required by CrmDbContext but not exercised in CRUD
        var config = Substitute.For<IConfiguration>();
        _context = new CrmDbContext(options, config);

        // ScriptEngineFactory with no engines — CRUD tests do not call GetEngine
        var engineFactory = new ScriptEngineFactory(
            Array.Empty<IScriptEngine>(),
            NullLogger<ScriptEngineFactory>.Instance);

        _sut = new ScriptPluginService(
            _context,
            engineFactory,
            NullLogger<ScriptPluginService>.Instance);
    }

    public void Dispose() => _context.Dispose();

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static ScriptPlugin MakePlugin(
        string name,
        bool isActive = true,
        string code = "return 1;") =>
        new()
        {
            Name = name,
            Code = code,
            Language = ScriptLanguage.JavaScript,
            IsActive = isActive,
            IsDeleted = false,
            Version = 1,
            CreatedAt = DateTime.UtcNow,
        };

    // ─────────────────────────────────────────────────────────────────────────
    // GetAllAsync
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Scenario 1 — Only active plugins returned when includeInactive=false.</summary>
    [Fact]
    public async Task GetAllAsync_ShouldReturnActivePlugins_WhenIncludeInactiveFalse()
    {
        _context.ScriptPlugins.AddRange(
            MakePlugin("Alpha", isActive: true),
            MakePlugin("Beta", isActive: true),
            MakePlugin("Gamma", isActive: false));
        await _context.SaveChangesAsync();

        var result = await _sut.GetAllAsync(includeInactive: false);

        result.Should().HaveCount(2);
        result.Select(p => p.Name).Should().BeEquivalentTo(new[] { "Alpha", "Beta" });
    }

    /// <summary>Scenario 2 — All plugins (active + inactive) returned when includeInactive=true.</summary>
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPlugins_WhenIncludeInactiveTrue()
    {
        _context.ScriptPlugins.AddRange(
            MakePlugin("Alpha", isActive: true),
            MakePlugin("Beta", isActive: true),
            MakePlugin("Gamma", isActive: false));
        await _context.SaveChangesAsync();

        var result = await _sut.GetAllAsync(includeInactive: true);

        result.Should().HaveCount(3);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetByIdAsync
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Scenario 3 — Existing plugin is returned with correct name.</summary>
    [Fact]
    public async Task GetByIdAsync_ShouldReturnPlugin_WhenExists()
    {
        var plugin = MakePlugin("MyPlugin");
        _context.ScriptPlugins.Add(plugin);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(plugin.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("MyPlugin");
    }

    /// <summary>Scenario 4 — Non-existent id returns null.</summary>
    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _sut.GetByIdAsync(99999);

        result.Should().BeNull();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CreateAsync
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Scenario 5 — Plugin is persisted with the correct fields.</summary>
    [Fact]
    public async Task CreateAsync_ShouldAddPlugin_WithCorrectFields()
    {
        var dto = new CreateScriptPluginDto(
            Name: "TestPlugin",
            Description: "A test plugin",
            Language: (int)ScriptLanguage.JavaScript,
            Code: "return 42;",
            ParameterSchema: null,
            ReturnValueDescription: null);

        var result = await _sut.CreateAsync(dto, createdByUserId: 1);

        result.Should().NotBeNull();
        result.Name.Should().Be("TestPlugin");
        result.Code.Should().Be("return 42;");
        result.Language.Should().Be((int)ScriptLanguage.JavaScript);
        result.IsActive.Should().BeTrue();
        result.Version.Should().Be(1);
    }

    /// <summary>Scenario 6 — ArgumentException thrown when name is empty.</summary>
    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenNameIsEmpty()
    {
        var dto = new CreateScriptPluginDto(
            Name: string.Empty,
            Description: null,
            Language: 0,
            Code: "return 1;",
            ParameterSchema: null,
            ReturnValueDescription: null);

        Func<Task> act = () => _sut.CreateAsync(dto, createdByUserId: 1);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UpdateAsync
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Scenario 7 — Name and code are overwritten when plugin exists.</summary>
    [Fact]
    public async Task UpdateAsync_ShouldUpdateFields_WhenExists()
    {
        var plugin = MakePlugin("Original");
        _context.ScriptPlugins.Add(plugin);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateScriptPluginDto(
            Name: "Updated",
            Description: "Updated description",
            Code: "return 99;",
            ParameterSchema: null,
            ReturnValueDescription: null,
            IsActive: true);

        var result = await _sut.UpdateAsync(plugin.Id, updateDto);

        result.Name.Should().Be("Updated");
        result.Code.Should().Be("return 99;");
    }

    /// <summary>Scenario 8 — InvalidOperationException thrown when id does not exist.</summary>
    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        var updateDto = new UpdateScriptPluginDto(
            Name: "Irrelevant",
            Description: null,
            Code: "return 1;",
            ParameterSchema: null,
            ReturnValueDescription: null,
            IsActive: true);

        Func<Task> act = () => _sut.UpdateAsync(99999, updateDto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DeleteAsync
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Scenario 9 — IsDeleted is set to true (soft delete) when plugin exists.</summary>
    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete_WhenExists()
    {
        var plugin = MakePlugin("ToDelete");
        _context.ScriptPlugins.Add(plugin);
        await _context.SaveChangesAsync();

        await _sut.DeleteAsync(plugin.Id);

        var stored = await _context.ScriptPlugins.FindAsync(plugin.Id);
        stored.Should().NotBeNull();
        stored!.IsDeleted.Should().BeTrue();
    }

    /// <summary>Scenario 10 — InvalidOperationException thrown when id does not exist.</summary>
    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        Func<Task> act = () => _sut.DeleteAsync(99999);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }
}
