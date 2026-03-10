// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for MonitoringService (TCOV-005).
/// </summary>
public class MonitoringServiceTests : ServiceTestFixtureBase<MonitoringService>
{
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<IRedisCacheService> _mockRedis;
    private readonly Mock<IDatabaseHealthService> _mockDbHealth;
    private readonly Mock<IDockerMonitoringService> _mockDocker;
    private readonly Mock<IKubernetesMonitoringService> _mockKubernetes;
    private readonly IOptions<MonitoringOptions> _options;
    private readonly MonitoringService _service;

    public MonitoringServiceTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockRedis = new Mock<IRedisCacheService>();
        _mockDbHealth = new Mock<IDatabaseHealthService>();
        _mockDocker = new Mock<IDockerMonitoringService>();
        _mockKubernetes = new Mock<IKubernetesMonitoringService>();
        _options = Options.Create(new MonitoringOptions
        {
            EnableDockerMonitoring = false,
            EnableK8sMonitoring = false,
            HealthCheckTimeoutSeconds = 5,
            CacheDurationSeconds = 30
        });
        _service = new MonitoringService(
            _mockConfig.Object,
            _mockRedis.Object,
            _options,
            MockLogger.Object,
            _mockDbHealth.Object,
            _mockDocker.Object,
            _mockKubernetes.Object);
    }

    [Fact]
    public void Constructor_ShouldCreateService_WithValidDependencies()
    {
        _service.Should().NotBeNull();
    }

    [Fact]
    public void GetMonitoringOptions_ShouldReturnConfiguredOptions()
    {
        var result = _service.GetMonitoringOptions();

        result.Should().NotBeNull();
        result.HealthCheckTimeoutSeconds.Should().Be(5);
    }

    [Fact]
    public async Task GetContainerHealthAsync_ShouldReturnList_WhenDockerDisabled()
    {
        _mockDocker.Setup(d => d.GetContainerHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContainerHealth>());

        var result = await _service.GetContainerHealthAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPodHealthAsync_ShouldReturnList_WhenK8sDisabled()
    {
        _mockKubernetes.Setup(k => k.GetPodHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PodHealth>());

        var result = await _service.GetPodHealthAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetActiveSessionsAsync_ShouldReturnList()
    {
        var result = await _service.GetActiveSessionsAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSystemMetricsAsync_ShouldReturnValidMetrics()
    {
        var result = await _service.GetSystemMetricsAsync();

        result.Should().NotBeNull();
    }
}
