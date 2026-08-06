// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Unit tests for AgentExecutionService constructor null-guards and ChatAsync validation.
// No LLM, database, or network calls are made during these tests.
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Configuration;
using CRM.Infrastructure.AI.SK.Connectors;
using CRM.Infrastructure.AI.SK.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Services.SK;

/// <summary>
/// Unit tests for <see cref="AgentExecutionService"/> constructor null-guards
/// and early-validation paths. No LLM or database calls are exercised.
/// </summary>
public sealed class AgentExecutionServiceTests
{
    // ---------------------------------------------------------------------------
    // Helpers — build minimal valid dependencies
    // ---------------------------------------------------------------------------

    private static ICrmDbContext CreateDbContext() =>
        new Mock<ICrmDbContext>().Object;

    private static CrmKernelFactory CreateKernelFactory()
    {
        var sp = new Mock<IServiceProvider>().Object;
        var opts = Mock.Of<IOptions<SemanticKernelOptions>>(
            o => o.Value == new SemanticKernelOptions());
        var logger = new Mock<ILogger<CrmKernelFactory>>().Object;
        return new CrmKernelFactory(sp, opts, logger);
    }

    private static IOptions<SemanticKernelOptions> CreateOptions() =>
        Mock.Of<IOptions<SemanticKernelOptions>>(o => o.Value == new SemanticKernelOptions());

    private static ILogger<AgentExecutionService> CreateLogger() =>
        new Mock<ILogger<AgentExecutionService>>().Object;

    private static AgentExecutionService CreateService() =>
        new AgentExecutionService(
            CreateDbContext(),
            CreateKernelFactory(),
            CreateOptions(),
            CreateLogger());

    // ---------------------------------------------------------------------------
    // Constructor null-guard tests
    // ---------------------------------------------------------------------------

    [Fact]
    public void Constructor_ShouldThrow_WhenContextIsNull()
    {
        var act = () => new AgentExecutionService(
            null!,
            CreateKernelFactory(),
            CreateOptions(),
            CreateLogger());

        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenKernelFactoryIsNull()
    {
        var act = () => new AgentExecutionService(
            CreateDbContext(),
            null!,
            CreateOptions(),
            CreateLogger());

        act.Should().Throw<ArgumentNullException>().WithParameterName("kernelFactory");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenOptionsIsNull()
    {
        // options?.Value → when options is null, options?.Value returns null,
        // then ?? throw triggers with paramName "options"
        var act = () => new AgentExecutionService(
            CreateDbContext(),
            CreateKernelFactory(),
            null!,
            CreateLogger());

        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenOptionsValueIsNull()
    {
        // options is not null but options.Value IS null → ?? throw triggers
        var badOptions = Mock.Of<IOptions<SemanticKernelOptions>>(o => o.Value == null!);

        var act = () => new AgentExecutionService(
            CreateDbContext(),
            CreateKernelFactory(),
            badOptions,
            CreateLogger());

        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        var act = () => new AgentExecutionService(
            CreateDbContext(),
            CreateKernelFactory(),
            CreateOptions(),
            null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_ShouldSucceed_WhenAllDependenciesProvided()
    {
        var act = () => CreateService();
        act.Should().NotThrow();
    }

    // ---------------------------------------------------------------------------
    // ChatAsync — early-exit validation (ArgumentException.ThrowIfNullOrWhiteSpace)
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task ChatAsync_ShouldThrow_WhenMessageIsNull()
    {
        var service = CreateService();

        Func<Task> act = async () => await service.ChatAsync(
            agentId: 1,
            userId: 1,
            message: null!);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("message");
    }

    [Fact]
    public async Task ChatAsync_ShouldThrow_WhenMessageIsEmpty()
    {
        var service = CreateService();

        Func<Task> act = async () => await service.ChatAsync(
            agentId: 1,
            userId: 1,
            message: string.Empty);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("message");
    }

    [Fact]
    public async Task ChatAsync_ShouldThrow_WhenMessageIsWhitespace()
    {
        var service = CreateService();

        Func<Task> act = async () => await service.ChatAsync(
            agentId: 1,
            userId: 1,
            message: "   ");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("message");
    }

    [Fact]
    public async Task ChatAsync_ShouldThrow_WhenMessageIsTabWhitespace()
    {
        var service = CreateService();

        Func<Task> act = async () => await service.ChatAsync(
            agentId: 1,
            userId: 1,
            message: "\t");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("message");
    }

    // ---------------------------------------------------------------------------
    // SemanticKernelOptions — default value validation
    // ---------------------------------------------------------------------------

    [Fact]
    public void SemanticKernelOptions_Defaults_ShouldBeEnabled()
    {
        var opts = new SemanticKernelOptions();
        opts.Enabled.Should().BeTrue("SK integration should be enabled by default");
    }

    [Fact]
    public void SemanticKernelOptions_SectionName_ShouldBeSemanticKernel()
    {
        SemanticKernelOptions.SectionName.Should().Be("SemanticKernel");
    }

    [Fact]
    public void SemanticKernelOptions_VectorStore_ShouldNotBeNull()
    {
        var opts = new SemanticKernelOptions();
        opts.VectorStore.Should().NotBeNull();
        opts.Agents.Should().NotBeNull();
        opts.Models.Should().NotBeNull();
    }

    // ---------------------------------------------------------------------------
    // CrmKernelFactory constructor null-guards
    // ---------------------------------------------------------------------------

    [Fact]
    public void CrmKernelFactory_Constructor_ShouldThrow_WhenServiceProviderIsNull()
    {
        var opts = CreateOptions();
        var logger = new Mock<ILogger<CrmKernelFactory>>().Object;

        var act = () => new CrmKernelFactory(null!, opts, logger);

        act.Should().Throw<ArgumentNullException>().WithParameterName("serviceProvider");
    }

    [Fact]
    public void CrmKernelFactory_Constructor_ShouldThrow_WhenOptionsIsNull()
    {
        var sp = new Mock<IServiceProvider>().Object;
        var logger = new Mock<ILogger<CrmKernelFactory>>().Object;

        var act = () => new CrmKernelFactory(sp, null!, logger);

        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void CrmKernelFactory_Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        var sp = new Mock<IServiceProvider>().Object;
        var opts = CreateOptions();

        var act = () => new CrmKernelFactory(sp, opts, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void CrmKernelFactory_Constructor_ShouldSucceed_WhenAllDependenciesProvided()
    {
        var act = CreateKernelFactory;
        act.Should().NotThrow();
    }
}
