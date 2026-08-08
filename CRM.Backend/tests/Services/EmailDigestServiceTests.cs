// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for EmailDigestService (REV-FE-002) using an InMemory database.
/// Covers config CRUD (GET/PUT semantics) and per-section content aggregation
/// (new leads, open opportunities, recent activities, upcoming/overdue tasks,
/// team performance, KPI summary).
/// </summary>
public class EmailDigestServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly TaskService _taskService;
    private readonly ActivityService _activityService;
    private readonly Mock<INotificationPort> _mockNotificationPort;
    private readonly EmailDigestService _service;

    public EmailDigestServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"EmailDigestServiceTests_{Guid.NewGuid()}")
            .Options;

        _dbContext = new CrmDbContext(options, null!);
        _taskService = new TaskService(_dbContext, new Mock<ILogger<TaskService>>().Object);
        _activityService = new ActivityService(_dbContext, new Mock<ILogger<ActivityService>>().Object);
        _mockNotificationPort = new Mock<INotificationPort>();

        _service = new EmailDigestService(
            _dbContext,
            _taskService,
            _activityService,
            _mockNotificationPort.Object,
            new Mock<ILogger<EmailDigestService>>().Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    private async Task<User> SeedUserAsync(int? departmentId = null)
    {
        var user = new User
        {
            Username = $"user{Guid.NewGuid():N}",
            Email = $"user{Guid.NewGuid():N}@example.com",
            FirstName = "Ada",
            LastName = "Lovelace",
            PasswordHash = "hash",
            DepartmentId = departmentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    // ── Config CRUD ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetConfigAsync_ShouldReturnDefaults_WhenNoConfigSaved()
    {
        var dto = await _service.GetConfigAsync(userId: 999);

        dto.Enabled.Should().BeFalse();
        dto.Frequency.Should().Be("daily");
        dto.TimeOfDay.Should().Be("08:00");
        dto.Sections.NewLeads.Should().BeTrue();
        dto.Sections.TeamPerformance.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateConfigAsync_ShouldCreateNewRow_WhenNoneExists()
    {
        var user = await SeedUserAsync();

        var dto = new EmailDigestConfigDto
        {
            Enabled = true,
            Frequency = "weekly",
            DayOfWeek = 3,
            TimeOfDay = "09:30",
            Timezone = "America/New_York",
            Sections = new EmailDigestSectionsDto { NewLeads = false, KpiSummary = true }
        };

        var saved = await _service.UpdateConfigAsync(user.Id, dto);

        saved.Enabled.Should().BeTrue();
        saved.Frequency.Should().Be("weekly");
        saved.DayOfWeek.Should().Be(3);
        saved.TimeOfDay.Should().Be("09:30");
        saved.Timezone.Should().Be("America/New_York");
        saved.Sections.NewLeads.Should().BeFalse();
        saved.Sections.KpiSummary.Should().BeTrue();

        (await _dbContext.EmailDigestConfigs.CountAsync(c => c.UserId == user.Id)).Should().Be(1);
    }

    [Fact]
    public async Task UpdateConfigAsync_ShouldUpdateExistingRow_NotCreateDuplicate()
    {
        var user = await SeedUserAsync();

        await _service.UpdateConfigAsync(user.Id, new EmailDigestConfigDto { Enabled = true, Frequency = "daily", TimeOfDay = "08:00" });
        await _service.UpdateConfigAsync(user.Id, new EmailDigestConfigDto { Enabled = true, Frequency = "monthly", DayOfMonth = 15, TimeOfDay = "10:00" });

        (await _dbContext.EmailDigestConfigs.CountAsync(c => c.UserId == user.Id)).Should().Be(1);
        var dto = await _service.GetConfigAsync(user.Id);
        dto.Frequency.Should().Be("monthly");
        dto.DayOfMonth.Should().Be(15);
    }

    [Fact]
    public async Task UpdateConfigAsync_ShouldClearDayOfWeek_WhenFrequencyIsNotWeekly()
    {
        var user = await SeedUserAsync();

        await _service.UpdateConfigAsync(user.Id, new EmailDigestConfigDto { Enabled = true, Frequency = "weekly", DayOfWeek = 2, TimeOfDay = "08:00" });
        var saved = await _service.UpdateConfigAsync(user.Id, new EmailDigestConfigDto { Enabled = true, Frequency = "daily", DayOfWeek = 2, TimeOfDay = "08:00" });

        saved.DayOfWeek.Should().BeNull();
    }

    // ── Content aggregation: New Leads ──────────────────────────────────────

    [Fact]
    public async Task BuildDigestContentAsync_ShouldIncludeOnlyOwnedLeadsSincePeriodStart()
    {
        var user = await SeedUserAsync();
        var otherUser = await SeedUserAsync();

        _dbContext.Leads.Add(new Lead { FirstName = "New", LastName = "Lead", OwnerId = user.Id, CreatedAt = DateTime.UtcNow.AddHours(-1) });
        _dbContext.Leads.Add(new Lead { FirstName = "Old", LastName = "Lead", OwnerId = user.Id, CreatedAt = DateTime.UtcNow.AddDays(-30) });
        _dbContext.Leads.Add(new Lead { FirstName = "Other", LastName = "Owner", OwnerId = otherUser.Id, CreatedAt = DateTime.UtcNow.AddHours(-1) });
        await _dbContext.SaveChangesAsync();

        var config = new EmailDigestConfig
        {
            UserId = user.Id,
            IncludeNewLeads = true,
            LastSentAt = DateTime.UtcNow.AddDays(-2)
        };

        var content = await _service.BuildDigestContentAsync(user.Id, config);

        content.NewLeads.Should().NotBeNull();
        content.NewLeads!.Should().ContainSingle(l => l.Name == "New Lead");
    }

    [Fact]
    public async Task BuildDigestContentAsync_ShouldLeaveNewLeadsNull_WhenSectionDisabled()
    {
        var user = await SeedUserAsync();
        var config = new EmailDigestConfig { UserId = user.Id, IncludeNewLeads = false };

        var content = await _service.BuildDigestContentAsync(user.Id, config);

        content.NewLeads.Should().BeNull();
    }

    // ── Content aggregation: Open Opportunities ─────────────────────────────

    [Fact]
    public async Task BuildDigestContentAsync_ShouldIncludeOnlyOwnedOpenOpportunities()
    {
        var user = await SeedUserAsync();

        _dbContext.Opportunities.Add(new Opportunity { Name = "Open Deal", SalesOwnerId = user.Id, Stage = OpportunityStage.Proposal, Amount = 5000, AccountId = 1 });
        _dbContext.Opportunities.Add(new Opportunity { Name = "Won Deal", SalesOwnerId = user.Id, Stage = OpportunityStage.ClosedWon, Amount = 8000, AccountId = 1, ClosedDate = DateTime.UtcNow });
        await _dbContext.SaveChangesAsync();

        var config = new EmailDigestConfig { UserId = user.Id, IncludeOpenOpportunities = true };

        var content = await _service.BuildDigestContentAsync(user.Id, config);

        content.OpenOpportunities.Should().NotBeNull();
        content.OpenOpportunities!.Should().ContainSingle(o => o.Name == "Open Deal");
    }

    // ── Content aggregation: Upcoming / Overdue tasks ───────────────────────

    [Fact]
    public async Task BuildDigestContentAsync_ShouldSeparateUpcomingAndOverdueTasks()
    {
        var user = await SeedUserAsync();

        _dbContext.CrmTasks.Add(new CrmTask { Subject = "Due Today", AssignedToUserId = user.Id, DueDate = DateTime.UtcNow.Date.AddHours(2), Status = CrmTaskStatus.NotStarted });
        _dbContext.CrmTasks.Add(new CrmTask { Subject = "Overdue", AssignedToUserId = user.Id, DueDate = DateTime.UtcNow.AddDays(-3), Status = CrmTaskStatus.InProgress });
        _dbContext.CrmTasks.Add(new CrmTask { Subject = "Done", AssignedToUserId = user.Id, DueDate = DateTime.UtcNow.AddDays(-3), Status = CrmTaskStatus.Completed });
        await _dbContext.SaveChangesAsync();

        var config = new EmailDigestConfig { UserId = user.Id, IncludeUpcomingTasks = true, IncludeOverdueTasks = true };

        var content = await _service.BuildDigestContentAsync(user.Id, config);

        content.UpcomingTasks.Should().NotBeNull();
        content.UpcomingTasks!.Should().ContainSingle(t => t.Subject == "Due Today");
        content.OverdueTasks.Should().NotBeNull();
        content.OverdueTasks!.Should().ContainSingle(t => t.Subject == "Overdue");
    }

    // ── Content aggregation: Recent Activities ──────────────────────────────

    [Fact]
    public async Task BuildDigestContentAsync_ShouldIncludeOnlyRecentActivitiesForUser()
    {
        var user = await SeedUserAsync();

        _dbContext.Activities.Add(new Activity { Title = "Called customer", UserId = user.Id, ActivityDate = DateTime.UtcNow.AddHours(-3), ActivityType = ActivityType.CallMade });
        _dbContext.Activities.Add(new Activity { Title = "Old email", UserId = user.Id, ActivityDate = DateTime.UtcNow.AddDays(-30), ActivityType = ActivityType.EmailSent });
        await _dbContext.SaveChangesAsync();

        var config = new EmailDigestConfig { UserId = user.Id, IncludeRecentActivities = true, LastSentAt = DateTime.UtcNow.AddDays(-1) };

        var content = await _service.BuildDigestContentAsync(user.Id, config);

        content.RecentActivities.Should().NotBeNull();
        content.RecentActivities!.Should().ContainSingle(a => a.Title == "Called customer");
    }

    // ── Content aggregation: Team Performance (v1 scope) ────────────────────

    [Fact]
    public async Task BuildDigestContentAsync_TeamPerformance_ShouldCountDepartmentPeersAsTeam()
    {
        var manager = await SeedUserAsync(departmentId: 42);
        var report = await SeedUserAsync(departmentId: 42);
        var outsider = await SeedUserAsync(departmentId: 99);

        _dbContext.Opportunities.Add(new Opportunity
        {
            Name = "Report's Won Deal",
            SalesOwnerId = report.Id,
            Stage = OpportunityStage.ClosedWon,
            Amount = 1000,
            AccountId = 1,
            ClosedDate = DateTime.UtcNow
        });
        _dbContext.Opportunities.Add(new Opportunity
        {
            Name = "Outsider's Won Deal",
            SalesOwnerId = outsider.Id,
            Stage = OpportunityStage.ClosedWon,
            Amount = 1000,
            AccountId = 1,
            ClosedDate = DateTime.UtcNow
        });
        _dbContext.Activities.Add(new Activity { Title = "Report activity", UserId = report.Id, ActivityDate = DateTime.UtcNow, ActivityType = ActivityType.CallMade });
        await _dbContext.SaveChangesAsync();

        var config = new EmailDigestConfig { UserId = manager.Id, IncludeTeamPerformance = true, LastSentAt = DateTime.UtcNow.AddDays(-7) };

        var content = await _service.BuildDigestContentAsync(manager.Id, config);

        content.TeamPerformance.Should().NotBeNull();
        content.TeamPerformance!.DirectReportCount.Should().Be(1);
        content.TeamPerformance.DealsClosedByTeam.Should().Be(1);
        content.TeamPerformance.ActivitiesLoggedByTeam.Should().Be(1);
    }

    [Fact]
    public async Task BuildDigestContentAsync_TeamPerformance_ShouldBeZero_WhenUserHasNoDepartment()
    {
        var user = await SeedUserAsync(departmentId: null);
        var config = new EmailDigestConfig { UserId = user.Id, IncludeTeamPerformance = true };

        var content = await _service.BuildDigestContentAsync(user.Id, config);

        content.TeamPerformance.Should().NotBeNull();
        content.TeamPerformance!.DirectReportCount.Should().Be(0);
    }

    // ── Content aggregation: KPI Summary (v1 scope) ─────────────────────────

    [Fact]
    public async Task BuildDigestContentAsync_KpiSummary_ShouldSummarizePipelineAndClosedDeals()
    {
        var user = await SeedUserAsync();

        _dbContext.Opportunities.Add(new Opportunity { Name = "Open A", SalesOwnerId = user.Id, Stage = OpportunityStage.Discovery, Amount = 1000, AccountId = 1 });
        _dbContext.Opportunities.Add(new Opportunity { Name = "Open B", SalesOwnerId = user.Id, Stage = OpportunityStage.Negotiation, Amount = 2000, AccountId = 1 });
        _dbContext.Opportunities.Add(new Opportunity { Name = "Won", SalesOwnerId = user.Id, Stage = OpportunityStage.ClosedWon, Amount = 5000, AccountId = 1, ClosedDate = DateTime.UtcNow });
        _dbContext.CrmTasks.Add(new CrmTask { Subject = "Completed", AssignedToUserId = user.Id, Status = CrmTaskStatus.Completed, CompletedDate = DateTime.UtcNow });
        await _dbContext.SaveChangesAsync();

        var config = new EmailDigestConfig { UserId = user.Id, IncludeKpiSummary = true, LastSentAt = DateTime.UtcNow.AddDays(-7) };

        var content = await _service.BuildDigestContentAsync(user.Id, config);

        content.KpiSummary.Should().NotBeNull();
        content.KpiSummary!.OpenPipelineCount.Should().Be(2);
        content.KpiSummary.OpenPipelineValue.Should().Be(3000);
        content.KpiSummary.DealsClosedWonThisPeriod.Should().Be(1);
        content.KpiSummary.RevenueClosedWonThisPeriod.Should().Be(5000);
        content.KpiSummary.TasksCompletedThisPeriod.Should().Be(1);
    }

    // ── Send ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task SendDigestAsync_ShouldCallNotificationPort_AndNotUpdateLastSentAt_WhenPreview()
    {
        var user = await SeedUserAsync();
        _mockNotificationPort
            .Setup(p => p.SendEmailAsync(It.IsAny<EmailNotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationResult { Success = true });

        var config = new EmailDigestConfig { UserId = user.Id, IncludeKpiSummary = true };

        var result = await _service.SendDigestAsync(user, config, isPreview: true);

        result.Should().BeTrue();
        config.LastSentAt.Should().BeNull();
        _mockNotificationPort.Verify(p => p.SendEmailAsync(
            It.Is<EmailNotificationRequest>(r => r.To == user.Email && r.Subject.Contains("Preview")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendDigestAsync_ShouldUpdateLastSentAt_WhenNotPreview()
    {
        var user = await SeedUserAsync();
        _mockNotificationPort
            .Setup(p => p.SendEmailAsync(It.IsAny<EmailNotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationResult { Success = true });

        var config = new EmailDigestConfig { UserId = user.Id, IncludeKpiSummary = true };
        _dbContext.EmailDigestConfigs.Add(config);
        await _dbContext.SaveChangesAsync();

        var result = await _service.SendDigestAsync(user, config, isPreview: false);

        result.Should().BeTrue();
        config.LastSentAt.Should().NotBeNull();
    }

    [Fact]
    public async Task SendDigestAsync_ShouldReturnFalse_WhenNotificationPortFails()
    {
        var user = await SeedUserAsync();
        _mockNotificationPort
            .Setup(p => p.SendEmailAsync(It.IsAny<EmailNotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationResult { Success = false, Error = "SMTP down" });

        var config = new EmailDigestConfig { UserId = user.Id };

        var result = await _service.SendDigestAsync(user, config, isPreview: true);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendDigestAsync_ShouldReturnFalse_WhenUserHasNoEmail()
    {
        var user = new User { Username = "noemail", Email = "", FirstName = "No", LastName = "Email", PasswordHash = "x" };
        var config = new EmailDigestConfig { UserId = 12345 };

        var result = await _service.SendDigestAsync(user, config, isPreview: true);

        result.Should().BeFalse();
        _mockNotificationPort.Verify(p => p.SendEmailAsync(It.IsAny<EmailNotificationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
