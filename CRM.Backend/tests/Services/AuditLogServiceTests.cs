// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for AuditLogService using InMemory database.
/// Tests logging Create/Update/Delete/Action, querying, statistics, search,
/// entity history, CSV export, and log cleanup.
/// </summary>
public class AuditLogServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<AuditLogService>> _mockLogger;
    private readonly AuditLogService _service;

    public AuditLogServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"AuditLogServiceTests_{Guid.NewGuid()}")
            .Options;

        _dbContext = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<AuditLogService>>();
        _service = new AuditLogService(_dbContext, _mockLogger.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region Helper Methods

    private async Task<int> SeedAuditLogAsync(
        string action = "Create",
        string entityType = "Account",
        int entityId = 1,
        string entityName = "Test Entity",
        int? userId = 1,
        DateTime? createdAt = null,
        bool isDeleted = false)
    {
        var log = new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            UserId = userId,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = createdAt ?? DateTime.UtcNow,
            IsDeleted = isDeleted,
            IpAddress = "127.0.0.1",
            UserAgent = "Test/1.0"
        };
        _dbContext.AuditLogs.Add(log);
        await _dbContext.SaveChangesAsync();
        return log.Id;
    }

    #endregion

    #region LogCreateAsync Tests

    [Fact]
    public async Task LogCreateAsync_ShouldCreateAuditLog_WithCorrectAction()
    {
        var newValues = new Dictionary<string, object> { { "Name", "Test Account" } };

        var id = await _service.LogCreateAsync("Account", 1, "Test Account", 10, newValues);

        id.Should().BeGreaterThan(0);
        var log = await _dbContext.AuditLogs.FindAsync(id);
        log.Should().NotBeNull();
        log!.Action.Should().Be("Create");
        log.EntityType.Should().Be("Account");
        log.EntityId.Should().Be(1);
        log.EntityName.Should().Be("Test Account");
        log.UserId.Should().Be(10);
    }

    [Fact]
    public async Task LogCreateAsync_ShouldSerializeNewValues()
    {
        var newValues = new Dictionary<string, object>
        {
            { "Name", "Acme Corp" },
            { "Industry", "Technology" },
            { "Revenue", 1000000 }
        };

        var id = await _service.LogCreateAsync("Account", 1, "Acme Corp", 1, newValues);

        var log = await _dbContext.AuditLogs.FindAsync(id);
        log!.NewValues.Should().NotBeNullOrEmpty();
        log.NewValues.Should().Contain("Acme Corp");
    }

    [Fact]
    public async Task LogCreateAsync_ShouldSetTimestamp()
    {
        var newValues = new Dictionary<string, object> { { "Name", "Test" } };

        var id = await _service.LogCreateAsync("Account", 1, "Test", 1, newValues);

        var log = await _dbContext.AuditLogs.FindAsync(id);
        log!.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task LogCreateAsync_ShouldStoreIpAndUserAgent()
    {
        var newValues = new Dictionary<string, object> { { "Name", "Test" } };

        var id = await _service.LogCreateAsync("Account", 1, "Test", 1, newValues,
            ipAddress: "192.168.1.1", userAgent: "Chrome/120");

        var log = await _dbContext.AuditLogs.FindAsync(id);
        log!.IpAddress.Should().Be("192.168.1.1");
        log.UserAgent.Should().Be("Chrome/120");
    }

    #endregion

    #region LogUpdateAsync Tests

    [Fact]
    public async Task LogUpdateAsync_ShouldCreateUpdateLog()
    {
        var oldValues = new Dictionary<string, object> { { "Name", "Old Name" } };
        var newValues = new Dictionary<string, object> { { "Name", "New Name" } };
        var changedProps = new List<string> { "Name" };

        var id = await _service.LogUpdateAsync("Account", 1, "Account 1", 10,
            oldValues, newValues, changedProps);

        var log = await _dbContext.AuditLogs.FindAsync(id);
        log.Should().NotBeNull();
        log!.Action.Should().Be("Update");
        log.OldValues.Should().NotBeNullOrEmpty();
        log.NewValues.Should().NotBeNullOrEmpty();
        log.ChangedProperties.Should().Contain("Name");
    }

    [Fact]
    public async Task LogUpdateAsync_ShouldStoreMultipleChangedProperties()
    {
        var oldValues = new Dictionary<string, object> { { "Name", "Old" }, { "Email", "old@test.com" } };
        var newValues = new Dictionary<string, object> { { "Name", "New" }, { "Email", "new@test.com" } };
        var changedProps = new List<string> { "Name", "Email" };

        var id = await _service.LogUpdateAsync("Contact", 1, "Contact 1", 10,
            oldValues, newValues, changedProps);

        var log = await _dbContext.AuditLogs.FindAsync(id);
        log!.ChangedProperties.Should().Contain("Name");
        log.ChangedProperties.Should().Contain("Email");
    }

    #endregion

    #region LogDeleteAsync Tests

    [Fact]
    public async Task LogDeleteAsync_ShouldCreateDeleteLog()
    {
        var oldValues = new Dictionary<string, object> { { "Name", "Deleted Entity" } };

        var id = await _service.LogDeleteAsync("Account", 1, "Deleted Account", 10, oldValues);

        var log = await _dbContext.AuditLogs.FindAsync(id);
        log.Should().NotBeNull();
        log!.Action.Should().Be("Delete");
        log.OldValues.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region LogActionAsync Tests

    [Fact]
    public async Task LogActionAsync_ShouldCreateGenericLog()
    {
        var id = await _service.LogActionAsync("Login", userId: 10, details: "User logged in from Chrome");

        var log = await _dbContext.AuditLogs.FindAsync(id);
        log.Should().NotBeNull();
        log!.Action.Should().Be("Login");
        log.UserId.Should().Be(10);
        log.Details.Should().Contain("User logged in from Chrome");
    }

    [Fact]
    public async Task LogActionAsync_ShouldWorkWithMinimalParameters()
    {
        var id = await _service.LogActionAsync("SystemStartup");

        id.Should().BeGreaterThan(0);
        var log = await _dbContext.AuditLogs.FindAsync(id);
        log!.Action.Should().Be("SystemStartup");
        log.EntityType.Should().BeNull();
        log.EntityId.Should().BeNull();
        log.UserId.Should().BeNull();
    }

    #endregion

    #region GetAuditLogsAsync Tests

    [Fact]
    public async Task GetAuditLogsAsync_ShouldReturnPaginatedResults()
    {
        for (int i = 0; i < 15; i++)
        {
            await SeedAuditLogAsync($"Create", "Account", i, $"Entity {i}");
        }

        var result = await _service.GetAuditLogsAsync(pageNumber: 1, pageSize: 10);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAuditLogsAsync_ShouldFilterByEntityType()
    {
        await SeedAuditLogAsync(entityType: "Account");
        await SeedAuditLogAsync(entityType: "Contact");
        await SeedAuditLogAsync(entityType: "Account");

        var result = await _service.GetAuditLogsAsync(entityType: "Account");

        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAuditLogsAsync_ShouldFilterByEntityId()
    {
        await SeedAuditLogAsync(entityId: 1);
        await SeedAuditLogAsync(entityId: 2);
        await SeedAuditLogAsync(entityId: 1);

        var result = await _service.GetAuditLogsAsync(entityId: 1);

        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAuditLogsAsync_ShouldFilterByUserId()
    {
        await SeedAuditLogAsync(userId: 10);
        await SeedAuditLogAsync(userId: 20);

        var result = await _service.GetAuditLogsAsync(userId: 10);

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAuditLogsAsync_ShouldFilterByAction()
    {
        await SeedAuditLogAsync(action: "Create");
        await SeedAuditLogAsync(action: "Update");
        await SeedAuditLogAsync(action: "Delete");

        var result = await _service.GetAuditLogsAsync(action: "Create");

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAuditLogsAsync_ShouldFilterByDateRange()
    {
        var jan = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var feb = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc);
        var mar = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc);

        await SeedAuditLogAsync(createdAt: jan);
        await SeedAuditLogAsync(createdAt: feb);
        await SeedAuditLogAsync(createdAt: mar);

        var result = await _service.GetAuditLogsAsync(fromDate: feb, toDate: feb.AddDays(1));

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAuditLogsAsync_ShouldExcludeDeleted()
    {
        await SeedAuditLogAsync();
        await SeedAuditLogAsync(isDeleted: true);

        var result = await _service.GetAuditLogsAsync();

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAuditLogsAsync_ShouldReturnSecondPage()
    {
        for (int i = 0; i < 15; i++)
        {
            await SeedAuditLogAsync(entityName: $"Entity {i}");
        }

        var result = await _service.GetAuditLogsAsync(pageNumber: 2, pageSize: 10);

        result.Items.Should().HaveCount(5);
        result.PageNumber.Should().Be(2);
    }

    #endregion

    #region GetEntityHistoryAsync Tests

    [Fact]
    public async Task GetEntityHistoryAsync_ShouldReturnLogsForEntity()
    {
        await SeedAuditLogAsync(entityType: "Account", entityId: 1);
        await SeedAuditLogAsync(entityType: "Account", entityId: 1, action: "Update");
        await SeedAuditLogAsync(entityType: "Account", entityId: 2);

        var result = await _service.GetEntityHistoryAsync("Account", 1);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEntityHistoryAsync_ShouldReturnEmpty_WhenNoHistory()
    {
        var result = await _service.GetEntityHistoryAsync("Account", 999);
        result.Should().BeEmpty();
    }

    #endregion

    #region GetStatisticsAsync Tests

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnCorrectCounts()
    {
        await SeedAuditLogAsync(action: "Create", entityType: "Account");
        await SeedAuditLogAsync(action: "Create", entityType: "Contact");
        await SeedAuditLogAsync(action: "Update", entityType: "Account");
        await SeedAuditLogAsync(action: "Delete", entityType: "Account");

        var fromDate = DateTime.UtcNow.AddHours(-1);
        var toDate = DateTime.UtcNow.AddHours(1);
        var stats = await _service.GetStatisticsAsync(fromDate, toDate);

        stats.TotalActions.Should().Be(4);
        stats.CreatedCount.Should().Be(2);
        stats.UpdatedCount.Should().Be(1);
        stats.DeletedCount.Should().Be(1);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldCountUniqueUsers()
    {
        await SeedAuditLogAsync(userId: 1);
        await SeedAuditLogAsync(userId: 1);
        await SeedAuditLogAsync(userId: 2);

        var fromDate = DateTime.UtcNow.AddHours(-1);
        var toDate = DateTime.UtcNow.AddHours(1);
        var stats = await _service.GetStatisticsAsync(fromDate, toDate);

        stats.UniqueUsers.Should().Be(2);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldGroupByAction()
    {
        await SeedAuditLogAsync(action: "Create");
        await SeedAuditLogAsync(action: "Create");
        await SeedAuditLogAsync(action: "Update");

        var fromDate = DateTime.UtcNow.AddHours(-1);
        var toDate = DateTime.UtcNow.AddHours(1);
        var stats = await _service.GetStatisticsAsync(fromDate, toDate);

        stats.ActionsByType.Should().ContainKey("Create");
        stats.ActionsByType["Create"].Should().Be(2);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldGroupByEntity()
    {
        await SeedAuditLogAsync(entityType: "Account");
        await SeedAuditLogAsync(entityType: "Account");
        await SeedAuditLogAsync(entityType: "Contact");

        var fromDate = DateTime.UtcNow.AddHours(-1);
        var toDate = DateTime.UtcNow.AddHours(1);
        var stats = await _service.GetStatisticsAsync(fromDate, toDate);

        stats.ActionsByEntity.Should().ContainKey("Account");
        stats.ActionsByEntity["Account"].Should().Be(2);
    }

    #endregion

    #region SearchAsync Tests

    [Fact]
    public async Task SearchAsync_ShouldSearchByAction()
    {
        await SeedAuditLogAsync(action: "Login");
        await SeedAuditLogAsync(action: "Create");

        var result = await _service.SearchAsync("Login");

        result.Items.Should().HaveCount(1);
        result.Items.First().Action.Should().Be("Login");
    }

    [Fact]
    public async Task SearchAsync_ShouldSearchByEntityType()
    {
        await SeedAuditLogAsync(entityType: "Account");
        await SeedAuditLogAsync(entityType: "Contact");

        var result = await _service.SearchAsync("Account");

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnEmpty_WhenNoMatch()
    {
        await SeedAuditLogAsync(action: "Create", entityType: "Account");

        var result = await _service.SearchAsync("XYZ_NONEXISTENT");

        result.Items.Should().BeEmpty();
    }

    #endregion

    #region ExportToCsvAsync Tests

    [Fact]
    public async Task ExportToCsvAsync_ShouldReturnCsvBytes()
    {
        await SeedAuditLogAsync(action: "Create", entityType: "Account", entityName: "Test");

        var bytes = await _service.ExportToCsvAsync();

        bytes.Should().NotBeNullOrEmpty();
        var csv = System.Text.Encoding.UTF8.GetString(bytes);
        csv.Should().Contain("Timestamp");
        csv.Should().Contain("Action");
        csv.Should().Contain("Create");
    }

    [Fact]
    public async Task ExportToCsvAsync_ShouldFilterByEntityType()
    {
        await SeedAuditLogAsync(entityType: "Account", entityName: "Acct1");
        await SeedAuditLogAsync(entityType: "Contact", entityName: "Contact1");

        var bytes = await _service.ExportToCsvAsync(entityType: "Account");

        var csv = System.Text.Encoding.UTF8.GetString(bytes);
        csv.Should().Contain("Acct1");
        csv.Should().NotContain("Contact1");
    }

    [Fact]
    public async Task ExportToCsvAsync_ShouldReturnHeaderOnly_WhenNoData()
    {
        var bytes = await _service.ExportToCsvAsync();

        bytes.Should().NotBeNullOrEmpty();
        var csv = System.Text.Encoding.UTF8.GetString(bytes);
        csv.Should().Contain("Timestamp"); // Header should still exist
    }

    #endregion

    #region DeleteOldLogsAsync Tests

    [Fact]
    public async Task DeleteOldLogsAsync_ShouldSoftDeleteOldLogs()
    {
        var oldDate = DateTime.UtcNow.AddDays(-30);
        var recentDate = DateTime.UtcNow;
        await SeedAuditLogAsync(createdAt: oldDate, entityName: "Old");
        await SeedAuditLogAsync(createdAt: recentDate, entityName: "Recent");

        var cutoff = DateTime.UtcNow.AddDays(-15);
        var deletedCount = await _service.DeleteOldLogsAsync(cutoff);

        deletedCount.Should().Be(1);
    }

    [Fact]
    public async Task DeleteOldLogsAsync_ShouldReturnZero_WhenNoOldLogs()
    {
        await SeedAuditLogAsync(createdAt: DateTime.UtcNow);

        var cutoff = DateTime.UtcNow.AddDays(-30);
        var deletedCount = await _service.DeleteOldLogsAsync(cutoff);

        deletedCount.Should().Be(0);
    }

    #endregion

    #region ArchiveLogsAsync Tests

    [Fact]
    public async Task ArchiveLogsAsync_ShouldDelegateToDeleteOldLogs()
    {
        var oldDate = DateTime.UtcNow.AddDays(-30);
        await SeedAuditLogAsync(createdAt: oldDate);

        var cutoff = DateTime.UtcNow.AddDays(-15);
        var archivedCount = await _service.ArchiveLogsAsync(cutoff);

        archivedCount.Should().Be(1);
    }

    #endregion
}
