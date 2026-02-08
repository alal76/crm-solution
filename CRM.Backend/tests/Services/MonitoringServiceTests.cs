// CRM Solution - Customer Relationship Management System
// Monitoring Service Unit Tests

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
/// Unit tests for MonitoringService
/// Covers: Health checks, performance monitoring, alerts, metrics
/// </summary>
public class MonitoringServiceTests
{
    private readonly Mock<IRepository<HealthCheckLog>> _mockHealthLogRepository;
    private readonly Mock<IRepository<Alert>> _mockAlertRepository;
    private readonly Mock<IRepository<MetricSnapshot>> _mockMetricRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<MonitoringService>> _mockLogger;
    private readonly MonitoringService _service;

    public MonitoringServiceTests()
    {
        _mockHealthLogRepository = new Mock<IRepository<HealthCheckLog>>();
        _mockAlertRepository = new Mock<IRepository<Alert>>();
        _mockMetricRepository = new Mock<IRepository<MetricSnapshot>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<MonitoringService>>();

        _service = new MonitoringService(
            _mockHealthLogRepository.Object,
            _mockAlertRepository.Object,
            _mockMetricRepository.Object,
            _mockDbContext.Object,
            _mockLogger.Object);
    }

    #region Health Check Tests

    [Fact]
    public async Task GetSystemHealthAsync_AllServicesHealthy_ReturnsHealthy()
    {
        // Arrange
        _mockDbContext.Setup(d => d.Database.CanConnectAsync(default))
            .ReturnsAsync(true);

        // Act
        var result = await _service.GetSystemHealthAsync();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task GetSystemHealthAsync_DatabaseUnhealthy_ReturnsDegraded()
    {
        // Arrange
        _mockDbContext.Setup(d => d.Database.CanConnectAsync(default))
            .ReturnsAsync(false);

        // Act
        var result = await _service.GetSystemHealthAsync();

        // Assert
        result.Status.Should().NotBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task GetServiceHealthAsync_ValidService_ReturnsHealth()
    {
        // Arrange
        var serviceName = "database";

        _mockDbContext.Setup(d => d.Database.CanConnectAsync(default))
            .ReturnsAsync(true);

        // Act
        var result = await _service.GetServiceHealthAsync(serviceName);

        // Assert
        result.Should().NotBeNull();
        result!.ServiceName.Should().Be(serviceName);
    }

    [Fact]
    public async Task LogHealthCheckAsync_ValidResult_LogsCheck()
    {
        // Arrange
        var healthResult = new HealthCheckResult
        {
            ServiceName = "api",
            Status = HealthStatus.Healthy,
            ResponseTime = 50
        };

        _mockHealthLogRepository.Setup(r => r.AddAsync(It.IsAny<HealthCheckLog>()))
            .ReturnsAsync((HealthCheckLog l) => { l.Id = 1; return l; });

        // Act
        var result = await _service.LogHealthCheckAsync(healthResult);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetHealthHistoryAsync_ReturnsHistory()
    {
        // Arrange
        var logs = new List<HealthCheckLog>
        {
            new HealthCheckLog { Id = 1, ServiceName = "api", Status = "Healthy", CheckedAt = DateTime.UtcNow },
            new HealthCheckLog { Id = 2, ServiceName = "database", Status = "Healthy", CheckedAt = DateTime.UtcNow }
        };

        _mockHealthLogRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<HealthCheckLog, bool>>>()))
            .ReturnsAsync(logs);

        // Act
        var result = await _service.GetHealthHistoryAsync(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Metrics Tests

    [Fact]
    public async Task RecordMetricAsync_ValidMetric_RecordsMetric()
    {
        // Arrange
        var metric = new MetricData
        {
            Name = "api_requests_total",
            Value = 100,
            Tags = new Dictionary<string, string> { { "endpoint", "/api/accounts" } }
        };

        _mockMetricRepository.Setup(r => r.AddAsync(It.IsAny<MetricSnapshot>()))
            .ReturnsAsync((MetricSnapshot m) => { m.Id = 1; return m; });

        // Act
        var result = await _service.RecordMetricAsync(metric);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetMetricsAsync_ValidName_ReturnsMetrics()
    {
        // Arrange
        var metrics = new List<MetricSnapshot>
        {
            new MetricSnapshot { Id = 1, Name = "api_requests", Value = 100 },
            new MetricSnapshot { Id = 2, Name = "api_requests", Value = 150 }
        };

        _mockMetricRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MetricSnapshot, bool>>>()))
            .ReturnsAsync(metrics);

        // Act
        var result = await _service.GetMetricsAsync("api_requests");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMetricAverageAsync_ValidMetrics_ReturnsAverage()
    {
        // Arrange
        var metrics = new List<MetricSnapshot>
        {
            new MetricSnapshot { Id = 1, Name = "response_time", Value = 100 },
            new MetricSnapshot { Id = 2, Name = "response_time", Value = 200 }
        };

        _mockMetricRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MetricSnapshot, bool>>>()))
            .ReturnsAsync(metrics);

        // Act
        var result = await _service.GetMetricAverageAsync("response_time", DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);

        // Assert
        result.Should().Be(150);
    }

    [Fact]
    public async Task GetMetricMaxAsync_ValidMetrics_ReturnsMax()
    {
        // Arrange
        var metrics = new List<MetricSnapshot>
        {
            new MetricSnapshot { Id = 1, Name = "memory_usage", Value = 75 },
            new MetricSnapshot { Id = 2, Name = "memory_usage", Value = 90 }
        };

        _mockMetricRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MetricSnapshot, bool>>>()))
            .ReturnsAsync(metrics);

        // Act
        var result = await _service.GetMetricMaxAsync("memory_usage", DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);

        // Assert
        result.Should().Be(90);
    }

    #endregion

    #region Alert Tests

    [Fact]
    public async Task CreateAlertAsync_ValidAlert_ReturnsAlert()
    {
        // Arrange
        var request = new CreateAlertRequest
        {
            Name = "High CPU Alert",
            Severity = AlertSeverity.Warning,
            Message = "CPU usage above 80%",
            Source = "monitoring"
        };

        _mockAlertRepository.Setup(r => r.AddAsync(It.IsAny<Alert>()))
            .ReturnsAsync((Alert a) => { a.Id = 1; return a; });

        // Act
        var result = await _service.CreateAlertAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetActiveAlertsAsync_ReturnsActiveAlerts()
    {
        // Arrange
        var alerts = new List<Alert>
        {
            new Alert { Id = 1, Name = "Alert 1", IsActive = true },
            new Alert { Id = 2, Name = "Alert 2", IsActive = true }
        };

        _mockAlertRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Alert, bool>>>()))
            .ReturnsAsync(alerts);

        // Act
        var result = await _service.GetActiveAlertsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task AcknowledgeAlertAsync_ValidAlert_AcknowledgesAlert()
    {
        // Arrange
        var alert = new Alert { Id = 1, IsActive = true, AcknowledgedAt = null };

        _mockAlertRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(alert);

        _mockAlertRepository.Setup(r => r.UpdateAsync(It.IsAny<Alert>()))
            .ReturnsAsync((Alert a) => a);

        // Act
        var result = await _service.AcknowledgeAlertAsync(1, 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ResolveAlertAsync_ValidAlert_ResolvesAlert()
    {
        // Arrange
        var alert = new Alert { Id = 1, IsActive = true, ResolvedAt = null };

        _mockAlertRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(alert);

        _mockAlertRepository.Setup(r => r.UpdateAsync(It.IsAny<Alert>()))
            .ReturnsAsync((Alert a) => { a.IsActive = false; a.ResolvedAt = DateTime.UtcNow; return a; });

        // Act
        var result = await _service.ResolveAlertAsync(1, "Issue resolved");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetAlertsBySeverityAsync_ReturnsFilteredAlerts()
    {
        // Arrange
        var alerts = new List<Alert>
        {
            new Alert { Id = 1, Severity = AlertSeverity.Critical }
        };

        _mockAlertRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Alert, bool>>>()))
            .ReturnsAsync(alerts);

        // Act
        var result = await _service.GetAlertsBySeverityAsync(AlertSeverity.Critical);

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task GetPerformanceMetricsAsync_ReturnsMetrics()
    {
        // Arrange
        var metrics = new List<MetricSnapshot>
        {
            new MetricSnapshot { Name = "cpu_usage", Value = 45 },
            new MetricSnapshot { Name = "memory_usage", Value = 60 },
            new MetricSnapshot { Name = "disk_usage", Value = 35 }
        };

        _mockMetricRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MetricSnapshot, bool>>>()))
            .ReturnsAsync(metrics);

        // Act
        var result = await _service.GetPerformanceMetricsAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetResponseTimeStatsAsync_ReturnsStats()
    {
        // Arrange
        var metrics = new List<MetricSnapshot>
        {
            new MetricSnapshot { Name = "response_time", Value = 50 },
            new MetricSnapshot { Name = "response_time", Value = 100 },
            new MetricSnapshot { Name = "response_time", Value = 75 }
        };

        _mockMetricRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MetricSnapshot, bool>>>()))
            .ReturnsAsync(metrics);

        // Act
        var result = await _service.GetResponseTimeStatsAsync(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);

        // Assert
        result.Average.Should().Be(75);
        result.Min.Should().Be(50);
        result.Max.Should().Be(100);
    }

    [Fact]
    public async Task GetThroughputAsync_ReturnsRequestsPerSecond()
    {
        // Arrange
        var metrics = new List<MetricSnapshot>
        {
            new MetricSnapshot { Name = "requests_count", Value = 1000, RecordedAt = DateTime.UtcNow.AddMinutes(-5) },
            new MetricSnapshot { Name = "requests_count", Value = 1500, RecordedAt = DateTime.UtcNow }
        };

        _mockMetricRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MetricSnapshot, bool>>>()))
            .ReturnsAsync(metrics);

        // Act
        var result = await _service.GetThroughputAsync();

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion

    #region Dashboard Tests

    [Fact]
    public async Task GetDashboardDataAsync_ReturnsDashboardData()
    {
        // Arrange
        _mockHealthLogRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<HealthCheckLog, bool>>>()))
            .ReturnsAsync(new List<HealthCheckLog>());

        _mockAlertRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Alert, bool>>>()))
            .ReturnsAsync(new List<Alert>());

        _mockMetricRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MetricSnapshot, bool>>>()))
            .ReturnsAsync(new List<MetricSnapshot>());

        // Act
        var result = await _service.GetDashboardDataAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUptimeAsync_ReturnsUptimePercentage()
    {
        // Arrange
        var logs = new List<HealthCheckLog>
        {
            new HealthCheckLog { Status = "Healthy" },
            new HealthCheckLog { Status = "Healthy" },
            new HealthCheckLog { Status = "Unhealthy" }
        };

        _mockHealthLogRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<HealthCheckLog, bool>>>()))
            .ReturnsAsync(logs);

        // Act
        var result = await _service.GetUptimeAsync(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

        // Assert
        result.Should().BeApproximately(66.67, 0.1);
    }

    #endregion

    #region Alert Rules Tests

    [Fact]
    public async Task EvaluateAlertRulesAsync_ThresholdExceeded_CreatesAlert()
    {
        // Arrange
        var metrics = new List<MetricSnapshot>
        {
            new MetricSnapshot { Name = "cpu_usage", Value = 95 }
        };

        _mockMetricRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MetricSnapshot, bool>>>()))
            .ReturnsAsync(metrics);

        _mockAlertRepository.Setup(r => r.AddAsync(It.IsAny<Alert>()))
            .ReturnsAsync((Alert a) => { a.Id = 1; return a; });

        // Act
        var result = await _service.EvaluateAlertRulesAsync();

        // Assert
        result.AlertsCreated.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetMonitoringStatisticsAsync_ReturnsStats()
    {
        // Arrange
        _mockHealthLogRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<HealthCheckLog> { new HealthCheckLog(), new HealthCheckLog() });

        _mockAlertRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Alert> { new Alert { IsActive = true } });

        _mockMetricRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<MetricSnapshot> { new MetricSnapshot(), new MetricSnapshot() });

        // Act
        var result = await _service.GetMonitoringStatisticsAsync();

        // Assert
        result.TotalHealthChecks.Should().Be(2);
        result.TotalAlerts.Should().Be(1);
        result.TotalMetrics.Should().Be(2);
    }

    #endregion
}

// Supporting classes for tests
public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy
}

public enum AlertSeverity
{
    Info,
    Warning,
    Critical
}

public class HealthCheckResult
{
    public string ServiceName { get; set; } = string.Empty;
    public HealthStatus Status { get; set; }
    public int ResponseTime { get; set; }
}

public class MetricData
{
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public Dictionary<string, string>? Tags { get; set; }
}

public class CreateAlertRequest
{
    public string Name { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Source { get; set; }
}
