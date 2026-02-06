// CRM Solution - Customer Relationship Management System
// Cloud Deployment Service Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for CloudDeploymentService
/// Covers: Cloud provider management, deployments, health checks
/// </summary>
public class CloudDeploymentServiceTests
{
    private readonly Mock<IRepository<CloudProvider>> _mockProviderRepository;
    private readonly Mock<IRepository<CloudDeployment>> _mockDeploymentRepository;
    private readonly Mock<IRepository<DeploymentAttempt>> _mockAttemptRepository;
    private readonly Mock<IRepository<HealthCheckLog>> _mockHealthLogRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<CloudDeploymentService>> _mockLogger;
    private readonly CloudDeploymentService _service;

    public CloudDeploymentServiceTests()
    {
        _mockProviderRepository = new Mock<IRepository<CloudProvider>>();
        _mockDeploymentRepository = new Mock<IRepository<CloudDeployment>>();
        _mockAttemptRepository = new Mock<IRepository<DeploymentAttempt>>();
        _mockHealthLogRepository = new Mock<IRepository<HealthCheckLog>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<CloudDeploymentService>>();

        _service = new CloudDeploymentService(
            _mockProviderRepository.Object,
            _mockDeploymentRepository.Object,
            _mockAttemptRepository.Object,
            _mockHealthLogRepository.Object,
            _mockDbContext.Object,
            _mockLogger.Object);
    }

    #region Provider Tests

    [Fact]
    public async Task CreateProviderAsync_ValidProvider_ReturnsProvider()
    {
        // Arrange
        var request = new CreateCloudProviderDto
        {
            Name = "Azure",
            Type = CloudProviderType.Azure,
            Configuration = "{\"subscriptionId\": \"test\"}"
        };

        _mockProviderRepository.Setup(r => r.AddAsync(It.IsAny<CloudProvider>()))
            .ReturnsAsync((CloudProvider p) => { p.Id = 1; return p; });

        // Act
        var result = await _service.CreateProviderAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Azure");
    }

    [Fact]
    public async Task GetProviderByIdAsync_ExistingProvider_ReturnsProvider()
    {
        // Arrange
        var provider = new CloudProvider { Id = 1, Name = "AWS", Type = CloudProviderType.AWS };

        _mockProviderRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(provider);

        // Act
        var result = await _service.GetProviderByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("AWS");
    }

    [Fact]
    public async Task GetAllProvidersAsync_ReturnsProviders()
    {
        // Arrange
        var providers = new List<CloudProvider>
        {
            new CloudProvider { Id = 1, Name = "Azure" },
            new CloudProvider { Id = 2, Name = "AWS" }
        };

        _mockProviderRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(providers);

        // Act
        var result = await _service.GetAllProvidersAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateProviderAsync_ValidUpdate_UpdatesProvider()
    {
        // Arrange
        var existing = new CloudProvider { Id = 1, Name = "Old Name" };
        var updateDto = new UpdateCloudProviderDto { Id = 1, Name = "New Name" };

        _mockProviderRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        _mockProviderRepository.Setup(r => r.UpdateAsync(It.IsAny<CloudProvider>()))
            .ReturnsAsync((CloudProvider p) => p);

        // Act
        var result = await _service.UpdateProviderAsync(updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task DeleteProviderAsync_ExistingProvider_DeletesProvider()
    {
        // Arrange
        _mockProviderRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteProviderAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TestProviderConnectionAsync_ValidProvider_ReturnsSuccess()
    {
        // Arrange
        var provider = new CloudProvider { Id = 1, Name = "Azure", IsActive = true };

        _mockProviderRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(provider);

        // Act
        var result = await _service.TestProviderConnectionAsync(1);

        // Assert
        result.IsConnected.Should().BeTrue();
    }

    #endregion

    #region Deployment Tests

    [Fact]
    public async Task CreateDeploymentAsync_ValidDeployment_ReturnsDeployment()
    {
        // Arrange
        var request = new CreateDeploymentDto
        {
            Name = "Production Deployment",
            ProviderId = 1,
            Environment = "production"
        };

        var provider = new CloudProvider { Id = 1, Name = "Azure" };

        _mockProviderRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(provider);

        _mockDeploymentRepository.Setup(r => r.AddAsync(It.IsAny<CloudDeployment>()))
            .ReturnsAsync((CloudDeployment d) => { d.Id = 1; return d; });

        // Act
        var result = await _service.CreateDeploymentAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetDeploymentByIdAsync_ExistingDeployment_ReturnsDeployment()
    {
        // Arrange
        var deployment = new CloudDeployment { Id = 1, Name = "Test Deployment" };

        _mockDeploymentRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(deployment);

        // Act
        var result = await _service.GetDeploymentByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Deployment");
    }

    [Fact]
    public async Task GetDeploymentsByProviderAsync_ReturnsProviderDeployments()
    {
        // Arrange
        var deployments = new List<CloudDeployment>
        {
            new CloudDeployment { Id = 1, ProviderId = 1 },
            new CloudDeployment { Id = 2, ProviderId = 1 }
        };

        _mockDeploymentRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CloudDeployment, bool>>>()))
            .ReturnsAsync(deployments);

        // Act
        var result = await _service.GetDeploymentsByProviderAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task StartDeploymentAsync_ValidDeployment_StartsDeployment()
    {
        // Arrange
        var deployment = new CloudDeployment { Id = 1, Status = DeploymentStatus.Pending };

        _mockDeploymentRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(deployment);

        _mockDeploymentRepository.Setup(r => r.UpdateAsync(It.IsAny<CloudDeployment>()))
            .ReturnsAsync((CloudDeployment d) => { d.Status = DeploymentStatus.InProgress; return d; });

        _mockAttemptRepository.Setup(r => r.AddAsync(It.IsAny<DeploymentAttempt>()))
            .ReturnsAsync((DeploymentAttempt a) => { a.Id = 1; return a; });

        // Act
        var result = await _service.StartDeploymentAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task StopDeploymentAsync_RunningDeployment_StopsDeployment()
    {
        // Arrange
        var deployment = new CloudDeployment { Id = 1, Status = DeploymentStatus.Running };

        _mockDeploymentRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(deployment);

        _mockDeploymentRepository.Setup(r => r.UpdateAsync(It.IsAny<CloudDeployment>()))
            .ReturnsAsync((CloudDeployment d) => { d.Status = DeploymentStatus.Stopped; return d; });

        // Act
        var result = await _service.StopDeploymentAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RestartDeploymentAsync_ValidDeployment_RestartsDeployment()
    {
        // Arrange
        var deployment = new CloudDeployment { Id = 1, Status = DeploymentStatus.Running };

        _mockDeploymentRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(deployment);

        _mockDeploymentRepository.Setup(r => r.UpdateAsync(It.IsAny<CloudDeployment>()))
            .ReturnsAsync((CloudDeployment d) => d);

        // Act
        var result = await _service.RestartDeploymentAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Deployment Attempt Tests

    [Fact]
    public async Task GetDeploymentAttemptsAsync_ReturnsAttempts()
    {
        // Arrange
        var attempts = new List<DeploymentAttempt>
        {
            new DeploymentAttempt { Id = 1, DeploymentId = 1, Status = AttemptStatus.Success },
            new DeploymentAttempt { Id = 2, DeploymentId = 1, Status = AttemptStatus.Failed }
        };

        _mockAttemptRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<DeploymentAttempt, bool>>>()))
            .ReturnsAsync(attempts);

        // Act
        var result = await _service.GetDeploymentAttemptsAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetLatestAttemptAsync_ReturnsLatestAttempt()
    {
        // Arrange
        var attempts = new List<DeploymentAttempt>
        {
            new DeploymentAttempt { Id = 1, DeploymentId = 1, StartedAt = DateTime.UtcNow.AddHours(-1) },
            new DeploymentAttempt { Id = 2, DeploymentId = 1, StartedAt = DateTime.UtcNow }
        };

        _mockAttemptRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<DeploymentAttempt, bool>>>()))
            .ReturnsAsync(attempts);

        // Act
        var result = await _service.GetLatestAttemptAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task CheckDeploymentHealthAsync_HealthyDeployment_ReturnsHealthy()
    {
        // Arrange
        var deployment = new CloudDeployment { Id = 1, Status = DeploymentStatus.Running };

        _mockDeploymentRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(deployment);

        _mockHealthLogRepository.Setup(r => r.AddAsync(It.IsAny<HealthCheckLog>()))
            .ReturnsAsync((HealthCheckLog h) => { h.Id = 1; return h; });

        // Act
        var result = await _service.CheckDeploymentHealthAsync(1);

        // Assert
        result.IsHealthy.Should().BeTrue();
    }

    [Fact]
    public async Task GetHealthHistoryAsync_ReturnsHistory()
    {
        // Arrange
        var logs = new List<HealthCheckLog>
        {
            new HealthCheckLog { Id = 1, DeploymentId = 1, Status = "Healthy" },
            new HealthCheckLog { Id = 2, DeploymentId = 1, Status = "Unhealthy" }
        };

        _mockHealthLogRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<HealthCheckLog, bool>>>()))
            .ReturnsAsync(logs);

        // Act
        var result = await _service.GetHealthHistoryAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Scaling Tests

    [Fact]
    public async Task ScaleDeploymentAsync_ValidScale_ScalesDeployment()
    {
        // Arrange
        var deployment = new CloudDeployment { Id = 1, InstanceCount = 1 };

        _mockDeploymentRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(deployment);

        _mockDeploymentRepository.Setup(r => r.UpdateAsync(It.IsAny<CloudDeployment>()))
            .ReturnsAsync((CloudDeployment d) => { d.InstanceCount = 3; return d; });

        // Act
        var result = await _service.ScaleDeploymentAsync(1, 3);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetScalingHistoryAsync_ReturnsHistory()
    {
        // Arrange
        var attempts = new List<DeploymentAttempt>
        {
            new DeploymentAttempt { Id = 1, Type = "Scale" }
        };

        _mockAttemptRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<DeploymentAttempt, bool>>>()))
            .ReturnsAsync(attempts);

        // Act
        var result = await _service.GetScalingHistoryAsync(1);

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatisticsAsync_ReturnsStats()
    {
        // Arrange
        var providers = new List<CloudProvider>
        {
            new CloudProvider { Type = CloudProviderType.Azure },
            new CloudProvider { Type = CloudProviderType.AWS }
        };

        var deployments = new List<CloudDeployment>
        {
            new CloudDeployment { Status = DeploymentStatus.Running },
            new CloudDeployment { Status = DeploymentStatus.Stopped }
        };

        _mockProviderRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(providers);

        _mockDeploymentRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(deployments);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.TotalProviders.Should().Be(2);
        result.TotalDeployments.Should().Be(2);
        result.RunningDeployments.Should().Be(1);
    }

    #endregion
}

// Supporting classes for tests
public enum CloudProviderType
{
    Azure,
    AWS,
    GCP,
    OnPremise
}

public enum DeploymentStatus
{
    Pending,
    InProgress,
    Running,
    Stopped,
    Failed
}

public enum AttemptStatus
{
    InProgress,
    Success,
    Failed
}

public class CreateCloudProviderDto
{
    public string Name { get; set; } = string.Empty;
    public CloudProviderType Type { get; set; }
    public string? Configuration { get; set; }
}

public class UpdateCloudProviderDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Configuration { get; set; }
}

public class CreateDeploymentDto
{
    public string Name { get; set; } = string.Empty;
    public int ProviderId { get; set; }
    public string Environment { get; set; } = string.Empty;
}
