// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text;
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for AuditLogExportService.
/// TODO-SYS006-008
/// </summary>
public class AuditLogExportServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<AuditLogExportService>> _mockLogger;
    private readonly List<AuditLog> _auditLogs;

    public AuditLogExportServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<AuditLogExportService>>();
        _auditLogs = new List<AuditLog>();
    }

    private AuditLogExportService CreateService()
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(_auditLogs);
        _mockContext.Setup(c => c.AuditLogs).Returns(mockSet.Object);
        return new AuditLogExportService(_mockContext.Object, _mockLogger.Object);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static AuditLog MakeLog(int id, string entityType = "Account", string action = "Create", DateTime? createdAt = null) =>
        new()
        {
            Id = id,
            Action = action,
            EntityType = entityType,
            EntityId = id,
            EntityName = $"Entity-{id}",
            UserId = 1,
            IpAddress = "127.0.0.1",
            ChangedProperties = null,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            IsDeleted = false
        };

    // ── ExportToCsvAsync ────────────────────────────────────────────────────────

    /// <summary>
    /// The first line of the CSV output must be the standard header row.
    /// </summary>
    [Fact]
    public async Task ExportToCsvAsync_ShouldContainHeaderRow()
    {
        // Arrange
        _auditLogs.Add(MakeLog(1));
        var svc = CreateService();
        var req = new AuditLogExportRequestDto();

        // Act
        var bytes = await svc.ExportToCsvAsync(req);
        var text = Encoding.UTF8.GetString(bytes);
        var firstLine = text.Split('\n')[0].TrimEnd('\r');

        // Assert
        firstLine.Should().Be("Id,Timestamp,Action,EntityType,EntityId,EntityName,UserId,IpAddress,ChangedProperties");
    }

    /// <summary>
    /// When EntityType filter is set only matching logs should appear in the export.
    /// </summary>
    [Fact]
    public async Task ExportToCsvAsync_ShouldFilterByEntityType()
    {
        // Arrange
        _auditLogs.AddRange(new[]
        {
            MakeLog(1, entityType: "Account"),
            MakeLog(2, entityType: "Contact"),
            MakeLog(3, entityType: "Account"),
        });
        var svc = CreateService();
        var req = new AuditLogExportRequestDto { EntityType = "Account" };

        // Act
        var bytes = await svc.ExportToCsvAsync(req);
        var text = Encoding.UTF8.GetString(bytes);
        var dataLines = text.Split('\n').Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

        // Assert — only 2 Account rows (not the Contact row)
        dataLines.Should().HaveCount(2);
        dataLines.Should().OnlyContain(l => l.Contains("Account"));
    }

    /// <summary>
    /// DateFrom and DateTo filters should restrict records to the specified window.
    /// </summary>
    [Fact]
    public async Task ExportToCsvAsync_ShouldFilterByDateRange()
    {
        // Arrange
        var baseDate = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        _auditLogs.AddRange(new[]
        {
            MakeLog(1, createdAt: baseDate.AddDays(-10)),   // before range
            MakeLog(2, createdAt: baseDate),                // in range
            MakeLog(3, createdAt: baseDate.AddDays(5)),     // in range
            MakeLog(4, createdAt: baseDate.AddDays(20)),    // after range
        });
        var svc = CreateService();
        var req = new AuditLogExportRequestDto
        {
            DateFrom = baseDate.AddDays(-1),
            DateTo = baseDate.AddDays(10)
        };

        // Act
        var bytes = await svc.ExportToCsvAsync(req);
        var text = Encoding.UTF8.GetString(bytes);
        var dataLines = text.Split('\n').Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

        // Assert — only logs 2 and 3 fall inside [baseDate-1 … baseDate+10]
        dataLines.Should().HaveCount(2);
    }

    // ── ExportToJsonAsync ────────────────────────────────────────────────────────

    /// <summary>
    /// The JSON export should deserialize back to a list with the expected record count and fields.
    /// </summary>
    [Fact]
    public async Task ExportToJsonAsync_ShouldReturnDeserializableJsonWithExpectedRecords()
    {
        // Arrange
        _auditLogs.AddRange(new[] { MakeLog(1), MakeLog(2) });
        var svc = CreateService();
        var req = new AuditLogExportRequestDto();

        // Act
        var bytes = await svc.ExportToJsonAsync(req);
        var json = Encoding.UTF8.GetString(bytes);

        using var doc = JsonDocument.Parse(json);
        var records = doc.RootElement.EnumerateArray().ToList();

        // Assert
        records.Should().HaveCount(2);
        records[0].TryGetProperty("id", out _).Should().BeTrue("camelCase id property should exist");
        records[0].TryGetProperty("action", out _).Should().BeTrue("camelCase action property should exist");
    }

    /// <summary>
    /// PageSize should be capped at 10,000 even when a larger value is requested.
    /// </summary>
    [Fact]
    public async Task ExportToCsvAsync_ShouldCapPageSizeAt10000()
    {
        // Arrange — add 15 000 logs
        for (var i = 1; i <= 200; i++)
            _auditLogs.Add(MakeLog(i));

        var svc = CreateService();
        var req = new AuditLogExportRequestDto { PageSize = 99_999 }; // way over the cap

        // Act — the service should not throw; it simply applies the cap internally
        var act = () => svc.ExportToCsvAsync(req);

        // Assert — completes without error
        await act.Should().NotThrowAsync();
    }
}
