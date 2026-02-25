// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Infrastructure.Jobs;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.HostedServices;

/// <summary>
/// Unit tests for AuditLogCleanupJob.
/// TODO-SYS006-007
/// </summary>
public class AuditLogCleanupJobTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<AuditLogCleanupJob>> _mockLogger;

    public AuditLogCleanupJobTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<AuditLogCleanupJob>>();
    }

    private AuditLogCleanupJob CreateJob(AuditLogCleanupOptions options)
    {
        var optionsMock = Options.Create(options);
        return new AuditLogCleanupJob(_mockServiceProvider.Object, _mockLogger.Object, optionsMock);
    }

    /// <summary>
    /// When Enabled = false the job should return immediately without creating a service scope.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldExitImmediately_WhenJobIsDisabled()
    {
        // Arrange
        var options = new AuditLogCleanupOptions { Enabled = false };
        var job = CreateJob(options);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act
        await job.StartAsync(cts.Token);
        var executeTask = job.ExecutePublicAsync(cts.Token);

        // The task should complete quickly because Enabled = false causes an immediate return
        var completedWithinTimeout = await Task.WhenAny(executeTask, Task.Delay(2000)) == executeTask;

        await job.StopAsync(CancellationToken.None);

        // Assert - service provider should never be asked for a scope
        completedWithinTimeout.Should().BeTrue("the job should exit immediately when disabled");
        _mockServiceProvider.Verify(
            sp => sp.GetService(typeof(IServiceScopeFactory)), Times.Never);
    }

    /// <summary>
    /// When Enabled = false the job should log a disabled message.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldLogDisabledMessage_WhenJobIsDisabled()
    {
        // Arrange
        var options = new AuditLogCleanupOptions { Enabled = false };
        var job = CreateJob(options);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act
        var executeTask = job.ExecutePublicAsync(cts.Token);
        await Task.WhenAny(executeTask, Task.Delay(2000));

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("disabled")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Default options should reflect the expected configuration values.
    /// </summary>
    [Fact]
    public void AuditLogCleanupOptions_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new AuditLogCleanupOptions();

        // Assert
        options.Enabled.Should().BeTrue();
        options.IntervalHours.Should().Be(24);
        options.ArchiveAfterDays.Should().Be(90);
        options.PurgeAfterDays.Should().Be(365);
    }

    /// <summary>
    /// Options can be customised and the values are stored correctly.
    /// </summary>
    [Fact]
    public void AuditLogCleanupOptions_CanBeConfiguredWithCustomValues()
    {
        // Arrange & Act
        var options = new AuditLogCleanupOptions
        {
            Enabled = false,
            IntervalHours = 6,
            ArchiveAfterDays = 30,
            PurgeAfterDays = 180
        };

        // Assert
        options.Enabled.Should().BeFalse();
        options.IntervalHours.Should().Be(6);
        options.ArchiveAfterDays.Should().Be(30);
        options.PurgeAfterDays.Should().Be(180);
    }
}

/// <summary>
/// Test-only subclass to expose the protected ExecuteAsync method.
/// </summary>
internal class AuditLogCleanupJob(
    IServiceProvider serviceProvider,
    ILogger<global::CRM.Infrastructure.Jobs.AuditLogCleanupJob> logger,
    IOptions<AuditLogCleanupOptions> options)
    : global::CRM.Infrastructure.Jobs.AuditLogCleanupJob(serviceProvider, logger, options)
{
    public Task ExecutePublicAsync(CancellationToken stoppingToken)
        => ExecuteAsync(stoppingToken);
}
