// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for AuditLogService (TCOV Wave-A).</summary>
public class AuditLogServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<AuditLogService>> _logger;
    private readonly AuditLogService _service;

    public AuditLogServiceTests()
    {
        var opts = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new CrmDbContext(opts, null!);
        _logger = new Mock<ILogger<AuditLogService>>();
        _service = new AuditLogService(_context, _logger.Object);
    }

    public void Dispose() => _context.Dispose();

    // ── LogCreateAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task LogCreateAsync_ShouldCreateAuditLog_WithCreateAction()
    {
        var newValues = new Dictionary<string, object> { { "Name", "Test Account" } };

        var id = await _service.LogCreateAsync("Account", 1, "Test Account", 10, newValues);

        id.Should().BeGreaterThan(0);
        var log = await _context.AuditLogs.FindAsync(id);
        log.Should().NotBeNull();
        log!.Action.Should().Be("Create");
        log.EntityType.Should().Be("Account");
        log.EntityId.Should().Be(1);
        log.UserId.Should().Be(10);
    }

    [Fact]
    public async Task LogCreateAsync_ShouldAllowNullUserId()
    {
        var newValues = new Dictionary<string, object> { { "Name", "System Op" } };

        var id = await _service.LogCreateAsync("System", 99, "System Op", null, newValues);

        var log = await _context.AuditLogs.FindAsync(id);
        log!.UserId.Should().BeNull();
    }

    // ── LogUpdateAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task LogUpdateAsync_ShouldCreateAuditLog_WithUpdateAction()
    {
        var oldValues = new Dictionary<string, object> { { "Name", "Old Name" } };
        var newValues = new Dictionary<string, object> { { "Name", "New Name" } };

        var id = await _service.LogUpdateAsync(
            "Contact", 5, "Old Name", 20, oldValues, newValues,
            new List<string> { "Name" });

        id.Should().BeGreaterThan(0);
        var log = await _context.AuditLogs.FindAsync(id);
        log!.Action.Should().Be("Update");
        log.EntityType.Should().Be("Contact");
        log.ChangedProperties.Should().Contain("Name");
    }

    // ── LogDeleteAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task LogDeleteAsync_ShouldCreateAuditLog_WithDeleteAction()
    {
        var oldValues = new Dictionary<string, object> { { "Name", "Deleted Entity" } };

        var id = await _service.LogDeleteAsync("Lead", 7, "Deleted Entity", 30, oldValues);

        id.Should().BeGreaterThan(0);
        var log = await _context.AuditLogs.FindAsync(id);
        log!.Action.Should().Be("Delete");
        log.EntityType.Should().Be("Lead");
    }

    // ── LogActionAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task LogActionAsync_ShouldCreateAuditLog_WithCustomAction()
    {
        var id = await _service.LogActionAsync("Login", entityType: "User", entityId: 1, userId: 5, details: "IP: 127.0.0.1");

        id.Should().BeGreaterThan(0);
        var log = await _context.AuditLogs.FindAsync(id);
        log!.Action.Should().Be("Login");
        log.Details.Should().Contain("127.0.0.1");
    }

    // ── GetEntityHistoryAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetEntityHistoryAsync_ShouldReturnLogs_ForEntityTypeAndId()
    {
        _context.AuditLogs.Add(new AuditLog { Id = 1, Action = "Create", EntityType = "Account", EntityId = 10, IsDeleted = false, CreatedAt = DateTime.UtcNow });
        _context.AuditLogs.Add(new AuditLog { Id = 2, Action = "Update", EntityType = "Account", EntityId = 10, IsDeleted = false, CreatedAt = DateTime.UtcNow });
        _context.AuditLogs.Add(new AuditLog { Id = 3, Action = "Create", EntityType = "Contact", EntityId = 10, IsDeleted = false, CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _service.GetEntityHistoryAsync("Account", 10);

        result.Should().HaveCount(2);
        result.Should().Contain(l => l.Action == "Create");
        result.Should().Contain(l => l.Action == "Update");
    }

    [Fact]
    public async Task GetEntityHistoryAsync_ShouldReturnEmpty_WhenNoHistory()
    {
        var result = await _service.GetEntityHistoryAsync("Account", 999);
        result.Should().BeEmpty();
    }

    // ── GetAuditLogsAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAuditLogsAsync_ShouldReturnPaged_WithTotalCount()
    {
        for (int i = 1; i <= 5; i++)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                Id = i,
                Action = "Create",
                EntityType = "Account",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();

        var result = await _service.GetAuditLogsAsync(entityType: "Account", pageNumber: 1, pageSize: 3);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAuditLogsAsync_ShouldFilterByAction()
    {
        _context.AuditLogs.Add(new AuditLog { Id = 1, Action = "Create", IsDeleted = false, CreatedAt = DateTime.UtcNow });
        _context.AuditLogs.Add(new AuditLog { Id = 2, Action = "Delete", IsDeleted = false, CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _service.GetAuditLogsAsync(action: "Delete");

        result.TotalCount.Should().Be(1);
        result.Items.Should().Contain(l => l.Action == "Delete");
    }
}
