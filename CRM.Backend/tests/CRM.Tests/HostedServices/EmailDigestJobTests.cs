// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Jobs;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.HostedServices;

/// <summary>
/// Unit tests for EmailDigestJob (REV-FE-002). Verifies the "is this config due this hour"
/// scheduling logic and that ExecuteAsync sends via INotificationPort (mocked — no real emails)
/// only for configs that are due, updating LastSentAt so the same hour isn't re-sent.
/// </summary>
public class EmailDigestJobTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<INotificationPort> _mockNotificationPort;
    private readonly ServiceProvider _serviceProvider;

    public EmailDigestJobTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"EmailDigestJobTests_{Guid.NewGuid()}")
            .Options;
        _dbContext = new CrmDbContext(options, null!);
        _mockNotificationPort = new Mock<INotificationPort>();
        _mockNotificationPort
            .Setup(p => p.SendEmailAsync(It.IsAny<EmailNotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationResult { Success = true });

        var services = new ServiceCollection();
        services.AddSingleton<ICrmDbContext>(_dbContext);
        services.AddSingleton(_mockNotificationPort.Object);
        services.AddLogging();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IEmailDigestService, EmailDigestService>();
        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
        _serviceProvider.Dispose();
    }

    private static User MakeUser(int id) => new()
    {
        Id = id,
        Username = $"user{id}",
        Email = $"user{id}@example.com",
        FirstName = "Test",
        LastName = "User",
        PasswordHash = "hash",
        IsActive = true
    };

    // ── IsDueThisHour scheduling logic ──────────────────────────────────────

    [Fact]
    public void IsDueThisHour_Daily_ShouldBeTrue_WhenHourMatchesUtc()
    {
        var now = new DateTime(2026, 3, 10, 8, 0, 0, DateTimeKind.Utc);
        var config = new EmailDigestConfig { Frequency = EmailDigestFrequency.Daily, TimeOfDay = new TimeSpan(8, 0, 0), Timezone = "UTC" };

        EmailDigestJob.IsDueThisHour(config, now).Should().BeTrue();
    }

    [Fact]
    public void IsDueThisHour_Daily_ShouldBeFalse_WhenHourDoesNotMatch()
    {
        var now = new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc);
        var config = new EmailDigestConfig { Frequency = EmailDigestFrequency.Daily, TimeOfDay = new TimeSpan(8, 0, 0), Timezone = "UTC" };

        EmailDigestJob.IsDueThisHour(config, now).Should().BeFalse();
    }

    [Fact]
    public void IsDueThisHour_Weekly_ShouldBeTrue_OnlyWhenDayOfWeekAndHourMatch()
    {
        // 2026-03-10 is a Tuesday (DayOfWeek = 2)
        var tuesday8am = new DateTime(2026, 3, 10, 8, 0, 0, DateTimeKind.Utc);
        var wednesday8am = new DateTime(2026, 3, 11, 8, 0, 0, DateTimeKind.Utc);
        var config = new EmailDigestConfig { Frequency = EmailDigestFrequency.Weekly, DayOfWeek = 2, TimeOfDay = new TimeSpan(8, 0, 0), Timezone = "UTC" };

        EmailDigestJob.IsDueThisHour(config, tuesday8am).Should().BeTrue();
        EmailDigestJob.IsDueThisHour(config, wednesday8am).Should().BeFalse();
    }

    [Fact]
    public void IsDueThisHour_Monthly_ShouldClampToLastDayOfShortMonth()
    {
        // February 2026 has 28 days; DayOfMonth=31 should fire on Feb 28.
        var feb28 = new DateTime(2026, 2, 28, 8, 0, 0, DateTimeKind.Utc);
        var config = new EmailDigestConfig { Frequency = EmailDigestFrequency.Monthly, DayOfMonth = 31, TimeOfDay = new TimeSpan(8, 0, 0), Timezone = "UTC" };

        EmailDigestJob.IsDueThisHour(config, feb28).Should().BeTrue();
    }

    [Fact]
    public void IsDueThisHour_ShouldBeFalse_WhenDisabledConfigAlreadySentWithinTheHour()
    {
        var now = new DateTime(2026, 3, 10, 8, 0, 0, DateTimeKind.Utc);
        var config = new EmailDigestConfig
        {
            Frequency = EmailDigestFrequency.Daily,
            TimeOfDay = new TimeSpan(8, 0, 0),
            Timezone = "UTC",
            LastSentAt = now.AddMinutes(-10)
        };

        EmailDigestJob.IsDueThisHour(config, now).Should().BeFalse();
    }

    [Fact]
    public void IsDueThisHour_UnknownTimezone_ShouldFallBackToUtcInsteadOfThrowing()
    {
        var now = new DateTime(2026, 3, 10, 8, 0, 0, DateTimeKind.Utc);
        var config = new EmailDigestConfig { Frequency = EmailDigestFrequency.Daily, TimeOfDay = new TimeSpan(8, 0, 0), Timezone = "Not/ARealZone" };

        var act = () => EmailDigestJob.IsDueThisHour(config, now);

        act.Should().NotThrow();
    }

    // ── ExecuteAsync integration (InMemory DB + mocked notification port) ──

    [Fact]
    public async Task ExecuteAsync_ShouldSendOnlyToDueEnabledConfigs_AndSetLastSentAt()
    {
        var now = DateTime.UtcNow;
        var dueHourUtc = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);

        var dueUser = MakeUser(1);
        var notDueUser = MakeUser(2);
        var disabledUser = MakeUser(3);
        _dbContext.Users.AddRange(dueUser, notDueUser, disabledUser);

        _dbContext.EmailDigestConfigs.AddRange(
            new EmailDigestConfig
            {
                UserId = dueUser.Id,
                IsEnabled = true,
                Frequency = EmailDigestFrequency.Daily,
                TimeOfDay = new TimeSpan(dueHourUtc.Hour, 0, 0),
                Timezone = "UTC"
            },
            new EmailDigestConfig
            {
                UserId = notDueUser.Id,
                IsEnabled = true,
                Frequency = EmailDigestFrequency.Daily,
                TimeOfDay = new TimeSpan((dueHourUtc.Hour + 5) % 24, 0, 0),
                Timezone = "UTC"
            },
            new EmailDigestConfig
            {
                UserId = disabledUser.Id,
                IsEnabled = false,
                Frequency = EmailDigestFrequency.Daily,
                TimeOfDay = new TimeSpan(dueHourUtc.Hour, 0, 0),
                Timezone = "UTC"
            });
        await _dbContext.SaveChangesAsync();

        var job = new EmailDigestJob(_serviceProvider, new Mock<ILogger<EmailDigestJob>>().Object);

        var sentCount = await job.ExecuteAsync(CancellationToken.None);

        sentCount.Should().Be(1);
        _mockNotificationPort.Verify(p => p.SendEmailAsync(
            It.Is<EmailNotificationRequest>(r => r.To == dueUser.Email),
            It.IsAny<CancellationToken>()), Times.Once);

        var dueConfig = await _dbContext.EmailDigestConfigs.AsNoTracking().FirstAsync(c => c.UserId == dueUser.Id);
        dueConfig.LastSentAt.Should().NotBeNull();

        var notDueConfig = await _dbContext.EmailDigestConfigs.AsNoTracking().FirstAsync(c => c.UserId == notDueUser.Id);
        notDueConfig.LastSentAt.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnZero_WhenNoConfigsAreDue()
    {
        var user = MakeUser(1);
        _dbContext.Users.Add(user);
        _dbContext.EmailDigestConfigs.Add(new EmailDigestConfig
        {
            UserId = user.Id,
            IsEnabled = true,
            Frequency = EmailDigestFrequency.Daily,
            TimeOfDay = new TimeSpan((DateTime.UtcNow.Hour + 12) % 24, 0, 0),
            Timezone = "UTC"
        });
        await _dbContext.SaveChangesAsync();

        var job = new EmailDigestJob(_serviceProvider, new Mock<ILogger<EmailDigestJob>>().Object);

        var sentCount = await job.ExecuteAsync(CancellationToken.None);

        sentCount.Should().Be(0);
        _mockNotificationPort.Verify(p => p.SendEmailAsync(It.IsAny<EmailNotificationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
