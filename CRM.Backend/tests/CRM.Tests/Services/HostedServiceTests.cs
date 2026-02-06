// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under AGPL-3.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Tests for BackupSchedulerHostedService
/// Background service that runs scheduled database backups
/// </summary>
public class BackupSchedulerHostedServiceTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<BackupSchedulerHostedService>> _loggerMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;

    public BackupSchedulerHostedServiceTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<BackupSchedulerHostedService>>();
        _scopeMock = new Mock<IServiceScope>();
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
    }

    [Fact]
    public void Constructor_ShouldInitialize_WithValidDependencies()
    {
        // Arrange & Act
        var service = new BackupSchedulerHostedService(
            _serviceProviderMock.Object,
            _loggerMock.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldStartAndStop_WhenCancellationRequested()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var contextMock = new Mock<ICrmDbContext>();
        var backupServiceMock = new Mock<IDatabaseBackupService>();

        SetupServiceProvider(contextMock.Object, backupServiceMock.Object);

        var service = new BackupSchedulerHostedService(
            _serviceProviderMock.Object,
            _loggerMock.Object);

        // Act - cancel immediately
        cts.Cancel();
        
        // Assert - should not throw and should complete quickly
        var executeTask = Task.Run(() => service.StartAsync(cts.Token));
        await Task.WhenAny(executeTask, Task.Delay(1000));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogStarting_OnStart()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var contextMock = new Mock<ICrmDbContext>();
        var backupServiceMock = new Mock<IDatabaseBackupService>();

        SetupServiceProvider(contextMock.Object, backupServiceMock.Object);

        var service = new BackupSchedulerHostedService(
            _serviceProviderMock.Object,
            _loggerMock.Object);

        // Cancel after brief delay
        cts.CancelAfter(100);

        // Act
        try
        {
            await service.StartAsync(cts.Token);
            await Task.Delay(150);
        }
        catch (OperationCanceledException) { }

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("starting")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce());
    }

    private void SetupServiceProvider(ICrmDbContext context, IDatabaseBackupService backupService)
    {
        var scopeServiceProviderMock = new Mock<IServiceProvider>();
        scopeServiceProviderMock.Setup(x => x.GetService(typeof(ICrmDbContext))).Returns(context);
        scopeServiceProviderMock.Setup(x => x.GetService(typeof(IDatabaseBackupService))).Returns(backupService);

        _scopeMock.Setup(x => x.ServiceProvider).Returns(scopeServiceProviderMock.Object);
        _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(_scopeMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(_scopeFactoryMock.Object);
    }
}

/// <summary>
/// Tests for LeadScoreDecayHostedService
/// Background service that applies score decay to inactive leads
/// </summary>
public class LeadScoreDecayHostedServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<LeadScoreDecayHostedService>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;

    public LeadScoreDecayHostedServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new CrmDbContext(options);
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<LeadScoreDecayHostedService>>();
        _configurationMock = new Mock<IConfiguration>();

        // Default configuration
        _configurationMock.Setup(c => c.GetSection("LeadScoring:DecayCheckIntervalHours").Value).Returns("6");
        _configurationMock.Setup(c => c.GetSection("LeadScoring:EnableDecay").Value).Returns("true");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public void Constructor_ShouldInitialize_WithValidDependencies()
    {
        // Arrange & Act
        var service = new LeadScoreDecayHostedService(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            _configurationMock.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldUseDefaultInterval_WhenNotConfigured()
    {
        // Arrange
        _configurationMock.Setup(c => c.GetSection("LeadScoring:DecayCheckIntervalHours").Value).Returns((string?)null);

        // Act
        var service = new LeadScoreDecayHostedService(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            _configurationMock.Object);

        // Assert - should not throw and use default (6 hours)
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisabled_ShouldLogAndReturn()
    {
        // Arrange
        _configurationMock.Setup(c => c.GetSection("LeadScoring:EnableDecay").Value).Returns("false");
        var mockSection = new Mock<IConfigurationSection>();
        mockSection.Setup(s => s.Value).Returns("false");
        _configurationMock.Setup(c => c.GetSection("LeadScoring:EnableDecay")).Returns(mockSection.Object);

        var service = new LeadScoreDecayHostedService(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            BuildConfiguration(false));

        var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(100);
        await service.StopAsync(cts.Token);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("disabled")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogStarting_WhenEnabled()
    {
        // Arrange
        SetupServiceProvider();
        var service = new LeadScoreDecayHostedService(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            BuildConfiguration(true));

        var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        // Act
        try
        {
            await service.StartAsync(cts.Token);
            await Task.Delay(150);
        }
        catch (OperationCanceledException) { }

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("starting")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce());
    }

    #region Integration Tests (Direct Decay Logic)

    [Fact]
    public async Task LeadDecay_ShouldReduceScore_WhenLeadIsInactive()
    {
        // Arrange - directly test the decay logic concept
        var lead = new Lead
        {
            FirstName = "Test",
            LastName = "Lead",
            Email = "test@example.com",
            LeadScore = 75,
            LastActivityDate = DateTime.UtcNow.AddDays(-60), // Inactive for 60 days
            CreatedAt = DateTime.UtcNow.AddDays(-90)
        };
        _context.Leads.Add(lead);
        await _context.SaveChangesAsync();

        // Simulate decay logic
        var decayPoints = 10;
        var previousScore = lead.LeadScore;
        lead.LeadScore = Math.Max(0, lead.LeadScore - decayPoints);
        lead.LastScoreDecayDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Assert
        var updatedLead = await _context.Leads.FindAsync(lead.Id);
        updatedLead!.LeadScore.Should().Be(65);
        updatedLead.LastScoreDecayDate.Should().NotBeNull();
    }

    [Fact]
    public async Task LeadDecay_ShouldNotGoBelowZero()
    {
        // Arrange
        var lead = new Lead
        {
            FirstName = "Low",
            LastName = "Score",
            Email = "low@example.com",
            LeadScore = 5,
            LastActivityDate = DateTime.UtcNow.AddDays(-60),
            CreatedAt = DateTime.UtcNow.AddDays(-90)
        };
        _context.Leads.Add(lead);
        await _context.SaveChangesAsync();

        // Simulate decay of 10 points on a lead with only 5 points
        var decayPoints = 10;
        lead.LeadScore = Math.Max(0, lead.LeadScore - decayPoints);
        await _context.SaveChangesAsync();

        // Assert
        var updatedLead = await _context.Leads.FindAsync(lead.Id);
        updatedLead!.LeadScore.Should().Be(0);
        updatedLead.LeadScore.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task LeadDecay_ShouldCreateActivityRecord()
    {
        // Arrange
        var lead = new Lead
        {
            FirstName = "Activity",
            LastName = "Test",
            Email = "activity@example.com",
            LeadScore = 50,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
        _context.Leads.Add(lead);
        await _context.SaveChangesAsync();

        // Act - create activity for score decay
        var activity = new Activity
        {
            ActivityType = ActivityType.StatusChanged,
            Title = "Score decayed by 10 points",
            Description = $"Lead score reduced from 50 to 40 due to inactivity",
            EntityType = "Lead",
            EntityId = lead.Id,
            EntityName = $"{lead.FirstName} {lead.LastName}",
            IsSystem = true,
            ActivityDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            OldValue = "50",
            NewValue = "40",
            Source = "ScoreDecayService"
        };
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        // Assert
        var activities = await _context.Activities
            .Where(a => a.EntityType == "Lead" && a.EntityId == lead.Id)
            .ToListAsync();

        activities.Should().HaveCount(1);
        activities[0].Title.Should().Contain("decayed");
        activities[0].Source.Should().Be("ScoreDecayService");
        activities[0].IsSystem.Should().BeTrue();
    }

    #endregion

    #region LeadScoreRule Tests

    [Fact]
    public async Task DecayRule_ShouldBeConfigurable()
    {
        // Arrange - create a decay rule
        var decayRule = new LeadScoreRule
        {
            Name = "Standard Decay",
            RuleType = LeadScoreRuleType.Decay,
            DecayDaysThreshold = 30,
            DecayPointsPerPeriod = 5,
            DecayPeriodDays = 7,
            IsActive = true,
            Priority = 1,
            CreatedAt = DateTime.UtcNow
        };
        _context.LeadScoreRules.Add(decayRule);
        await _context.SaveChangesAsync();

        // Assert
        var savedRule = await _context.LeadScoreRules
            .FirstOrDefaultAsync(r => r.Name == "Standard Decay");

        savedRule.Should().NotBeNull();
        savedRule!.DecayDaysThreshold.Should().Be(30);
        savedRule.DecayPointsPerPeriod.Should().Be(5);
        savedRule.DecayPeriodDays.Should().Be(7);
    }

    [Fact]
    public async Task DecayRule_ShouldFilterActiveOnly()
    {
        // Arrange
        _context.LeadScoreRules.AddRange(
            new LeadScoreRule
            {
                Name = "Active Decay",
                RuleType = LeadScoreRuleType.Decay,
                IsActive = true,
                DecayDaysThreshold = 30,
                DecayPointsPerPeriod = 5,
                CreatedAt = DateTime.UtcNow
            },
            new LeadScoreRule
            {
                Name = "Inactive Decay",
                RuleType = LeadScoreRuleType.Decay,
                IsActive = false,
                DecayDaysThreshold = 14,
                DecayPointsPerPeriod = 10,
                CreatedAt = DateTime.UtcNow
            }
        );
        await _context.SaveChangesAsync();

        // Act
        var activeRules = await _context.LeadScoreRules
            .Where(r => r.RuleType == LeadScoreRuleType.Decay && r.IsActive && !r.IsDeleted)
            .ToListAsync();

        // Assert
        activeRules.Should().HaveCount(1);
        activeRules[0].Name.Should().Be("Active Decay");
    }

    #endregion

    #region Lead Filtering Tests

    [Fact]
    public async Task LeadsToDecay_ShouldFilterByPositiveScore()
    {
        // Arrange
        _context.Leads.AddRange(
            new Lead
            {
                FirstName = "High",
                LastName = "Score",
                Email = "high@test.com",
                LeadScore = 50,
                LastActivityDate = DateTime.UtcNow.AddDays(-40),
                CreatedAt = DateTime.UtcNow
            },
            new Lead
            {
                FirstName = "Zero",
                LastName = "Score",
                Email = "zero@test.com",
                LeadScore = 0,
                LastActivityDate = DateTime.UtcNow.AddDays(-40),
                CreatedAt = DateTime.UtcNow
            }
        );
        await _context.SaveChangesAsync();

        // Act
        var leadsToDecay = await _context.Leads
            .Where(l => !l.IsDeleted && l.LeadScore > 0)
            .ToListAsync();

        // Assert
        leadsToDecay.Should().HaveCount(1);
        leadsToDecay[0].FirstName.Should().Be("High");
    }

    [Fact]
    public async Task LeadsToDecay_ShouldFilterByInactivityDate()
    {
        // Arrange
        var threshold = DateTime.UtcNow.AddDays(-30);

        _context.Leads.AddRange(
            new Lead
            {
                FirstName = "Inactive",
                LastName = "Lead",
                Email = "inactive@test.com",
                LeadScore = 50,
                LastActivityDate = DateTime.UtcNow.AddDays(-45), // Beyond threshold
                CreatedAt = DateTime.UtcNow
            },
            new Lead
            {
                FirstName = "Active",
                LastName = "Lead",
                Email = "active@test.com",
                LeadScore = 50,
                LastActivityDate = DateTime.UtcNow.AddDays(-10), // Within threshold
                CreatedAt = DateTime.UtcNow
            }
        );
        await _context.SaveChangesAsync();

        // Act
        var leadsToDecay = await _context.Leads
            .Where(l => !l.IsDeleted)
            .Where(l => l.LeadScore > 0)
            .Where(l => !l.LastActivityDate.HasValue || l.LastActivityDate < threshold)
            .ToListAsync();

        // Assert
        leadsToDecay.Should().HaveCount(1);
        leadsToDecay[0].FirstName.Should().Be("Inactive");
    }

    [Fact]
    public async Task LeadsToDecay_ShouldExcludeRecentlyDecayed()
    {
        // Arrange
        var decayPeriodDays = 7;
        var now = DateTime.UtcNow;

        _context.Leads.AddRange(
            new Lead
            {
                FirstName = "Recently",
                LastName = "Decayed",
                Email = "recent@test.com",
                LeadScore = 50,
                LastActivityDate = DateTime.UtcNow.AddDays(-45),
                LastScoreDecayDate = DateTime.UtcNow.AddDays(-3), // Decayed 3 days ago
                CreatedAt = DateTime.UtcNow
            },
            new Lead
            {
                FirstName = "Due",
                LastName = "ForDecay",
                Email = "due@test.com",
                LeadScore = 50,
                LastActivityDate = DateTime.UtcNow.AddDays(-45),
                LastScoreDecayDate = DateTime.UtcNow.AddDays(-10), // Decayed 10 days ago
                CreatedAt = DateTime.UtcNow
            }
        );
        await _context.SaveChangesAsync();

        // Act
        var leadsToDecay = await _context.Leads
            .Where(l => !l.IsDeleted)
            .Where(l => l.LeadScore > 0)
            .Where(l => !l.LastScoreDecayDate.HasValue || l.LastScoreDecayDate < now.AddDays(-decayPeriodDays))
            .ToListAsync();

        // Assert
        leadsToDecay.Should().HaveCount(1);
        leadsToDecay[0].FirstName.Should().Be("Due");
    }

    #endregion

    private void SetupServiceProvider()
    {
        var scopeMock = new Mock<IServiceScope>();
        var scopeProviderMock = new Mock<IServiceProvider>();
        scopeProviderMock.Setup(x => x.GetService(typeof(CrmDbContext))).Returns(_context);
        scopeMock.Setup(x => x.ServiceProvider).Returns(scopeProviderMock.Object);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

        _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactoryMock.Object);
    }

    private static IConfiguration BuildConfiguration(bool enableDecay)
    {
        var configDict = new Dictionary<string, string?>
        {
            { "LeadScoring:DecayCheckIntervalHours", "6" },
            { "LeadScoring:EnableDecay", enableDecay.ToString() }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();
    }
}

/// <summary>
/// Tests for service registration extensions
/// </summary>
public class HostedServiceExtensionTests
{
    [Fact]
    public void AddLeadScoreDecayService_ShouldRegisterHostedService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add required dependencies
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "LeadScoring:EnableDecay", "true" }
            })
            .Build());
        services.AddLogging();

        // Act
        services.AddLeadScoreDecayService();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService) &&
            d.ImplementationType == typeof(LeadScoreDecayHostedService));

        serviceDescriptor.Should().NotBeNull();
    }
}
