// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Core.Interfaces.Scripting;
using CRM.Infrastructure.Factories;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for WorkflowWorkerService (TCOV-020).</summary>
public class WorkflowWorkerServiceTests
{
    private static WorkflowWorkerService CreateService(WorkflowWorkerOptions? options = null)
    {
        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();

        var logger = NullLogger<WorkflowWorkerService>.Instance;
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(new System.Net.Http.HttpClient());

        var scriptLogger = NullLogger<ScriptEngineFactory>.Instance;
        var scriptEngineFactory = new ScriptEngineFactory(
            Enumerable.Empty<IScriptEngine>(), scriptLogger);

        return new WorkflowWorkerService(sp, logger, httpClientFactory.Object, scriptEngineFactory, options);
    }

    [Fact]
    public void WorkflowWorkerOptions_DefaultMaxConcurrentTasks_ShouldBeFive()
    {
        var opts = new WorkflowWorkerOptions();
        opts.MaxConcurrentTasks.Should().Be(5);
    }

    [Fact]
    public void WorkflowWorkerOptions_DefaultPollIntervalSeconds_ShouldBeFive()
    {
        var opts = new WorkflowWorkerOptions();
        opts.PollIntervalSeconds.Should().Be(5);
    }

    [Fact]
    public void WorkflowWorkerOptions_DefaultMaxRetryCount_ShouldBeThree()
    {
        var opts = new WorkflowWorkerOptions();
        opts.MaxRetryCount.Should().Be(3);
    }

    [Fact]
    public void Constructor_ShouldCreateService_WithDefaultOptions()
    {
        var act = () => CreateService();
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_ShouldCreateService_WithCustomOptions()
    {
        var opts = new WorkflowWorkerOptions
        {
            MaxConcurrentTasks = 10,
            PollIntervalSeconds = 30,
            WorkerId = "test-worker-01"
        };
        var act = () => CreateService(opts);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task StopAsync_ShouldComplete_WhenServiceNotYetStarted()
    {
        var svc = CreateService();
        var act = async () => await svc.StopAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}
