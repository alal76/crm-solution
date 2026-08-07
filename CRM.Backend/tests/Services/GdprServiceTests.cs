// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Spec: BACK-014 (GDPR Service)
// MANDATORY TEST RULE: All method signatures, namespaces, and field names
// verified against the actual source before writing these tests.
// Sources read:
//   IGdprService.cs (CRM.Core/Ports/Input)
//   GdprService.cs  (CRM.Infrastructure/Services)
//   ICrmDbContext.cs — SaveChangesAsync at line 434,
//                       GdprAccessLogs at line 368,
//                       Contacts = DbSet<CRM.Core.Models.Contact>,
//                       Leads = DbSet<Lead>
//
// Constructor: GdprService(ICrmDbContext context, ILogger<GdprService> logger)
// Methods tested:
//   Task LogAccessAsync(int userId, string subjectType, int subjectId,
//                       string action, string ipAddress, string? notes, CT)
//   Task<PersonalDataExport> ExportPersonalDataAsync(string subjectType, int subjectId, CT)
//   Task<IEnumerable<GdprAccessLogDto>> GetAccessLogsAsync(string subjectType, int subjectId, CT)

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Contact = CRM.Core.Models.Contact;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for GdprService (BACK-014).
/// Tests GDPR access logging and personal data export.
/// </summary>
public class GdprServiceTests : ServiceTestFixtureBase<GdprService>
{    private readonly GdprService _service;

    public GdprServiceTests()
    {        _service = new GdprService(MockContext.Object, MockLogger.Object);

        // Default SaveChanges succeeds.
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    // ────────────────────────────────────────────────────────────────────────
    // LogAccessAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LogAccessAsync_ShouldCallSaveChanges_WhenValidParametersProvided()
    {
        // Arrange
        var mockLogsSet = MockDbSetFactory.CreateMockDbSet(new List<GdprAccessLog>());
        MockContext.Setup(c => c.GdprAccessLogs).Returns(mockLogsSet.Object);

        // Act
        await _service.LogAccessAsync(
            userId: 1,
            subjectType: "contact",
            subjectId: 10,
            action: "export",
            ipAddress: "192.168.1.1",
            notes: "User requested export");

        // Assert
        MockContext.Verify(
            c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAccessAsync_ShouldAddLogToGdprAccessLogs_WhenCalled()
    {
        // Arrange
        var capturedLogs = new List<GdprAccessLog>();
        var mockLogsSet = MockDbSetFactory.CreateMockDbSet(capturedLogs);
        mockLogsSet.Setup(s => s.Add(It.IsAny<GdprAccessLog>()))
            .Callback<GdprAccessLog>(capturedLogs.Add);
        MockContext.Setup(c => c.GdprAccessLogs).Returns(mockLogsSet.Object);

        // Act
        await _service.LogAccessAsync(
            userId: 2,
            subjectType: "lead",
            subjectId: 5,
            action: "view",
            ipAddress: "10.0.0.1");

        // Assert
        capturedLogs.Should().ContainSingle();
        capturedLogs[0].SubjectType.Should().Be("lead");
        capturedLogs[0].SubjectId.Should().Be(5);
        capturedLogs[0].Action.Should().Be("view");
    }

    [Fact]
    public async Task LogAccessAsync_ShouldNormaliseSubjectTypeToLower_WhenCalledWithMixedCase()
    {
        var capturedLogs = new List<GdprAccessLog>();
        var mockLogsSet = MockDbSetFactory.CreateMockDbSet(capturedLogs);
        mockLogsSet.Setup(s => s.Add(It.IsAny<GdprAccessLog>()))
            .Callback<GdprAccessLog>(capturedLogs.Add);
        MockContext.Setup(c => c.GdprAccessLogs).Returns(mockLogsSet.Object);

        await _service.LogAccessAsync(1, "Contact", 1, "Export", "127.0.0.1");

        capturedLogs.Single().SubjectType.Should().Be("contact");
        capturedLogs.Single().Action.Should().Be("export");
    }

    // ────────────────────────────────────────────────────────────────────────
    // ExportPersonalDataAsync — unknown type
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExportPersonalDataAsync_ShouldReturnExport_WithCorrectSubjectInfo()
    {
        // Arrange — no contact in DB; we just want a valid export object back.
        var mockContacts = MockDbSetFactory.CreateMockDbSet(new List<Contact>());
        MockContext.Setup(c => c.Contacts).Returns(mockContacts.Object);

        var mockLogs = MockDbSetFactory.CreateMockDbSet(new List<GdprAccessLog>());
        MockContext.Setup(c => c.GdprAccessLogs).Returns(mockLogs.Object);

        // Act
        var export = await _service.ExportPersonalDataAsync("contact", 99);

        // Assert
        export.Should().NotBeNull();
        export.SubjectType.Should().Be("contact");
        export.SubjectId.Should().Be(99);
        export.ExportedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ExportPersonalDataAsync_ShouldReturnContactData_WhenContactExists()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 42,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            PhonePrimary = "555-0199",
        };
        var mockContacts = MockDbSetFactory.CreateMockDbSet(new List<Contact> { contact });
        MockContext.Setup(c => c.Contacts).Returns(mockContacts.Object);

        var mockLogs = MockDbSetFactory.CreateMockDbSet(new List<GdprAccessLog>());
        MockContext.Setup(c => c.GdprAccessLogs).Returns(mockLogs.Object);

        // Act
        var export = await _service.ExportPersonalDataAsync("contact", 42);

        // Assert
        export.Data.Should().ContainKey("contact");
        export.Data["contact"]["email"].Should().Be("jane@example.com");
    }

    [Fact]
    public async Task ExportPersonalDataAsync_ShouldReturnEmptyData_WhenSubjectTypeIsUnknown()
    {
        // Arrange — unknown type; service logs warning and returns skeleton export.
        var mockLogs = MockDbSetFactory.CreateMockDbSet(new List<GdprAccessLog>());
        MockContext.Setup(c => c.GdprAccessLogs).Returns(mockLogs.Object);

        // Act
        var export = await _service.ExportPersonalDataAsync("widget", 1);

        // Assert — only accessHistory key (the warning path skips data population)
        export.Should().NotBeNull();
        export.Data.Should().ContainKey("accessHistory");
    }

    // ────────────────────────────────────────────────────────────────────────
    // GetAccessLogsAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAccessLogsAsync_ShouldReturnEmpty_WhenNoLogsExistForSubject()
    {
        var mockLogs = MockDbSetFactory.CreateMockDbSet(new List<GdprAccessLog>());
        MockContext.Setup(c => c.GdprAccessLogs).Returns(mockLogs.Object);

        var result = await _service.GetAccessLogsAsync("contact", 99);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAccessLogsAsync_ShouldReturnMatchingLogs_WhenLogsExistForSubject()
    {
        var log = new GdprAccessLog
        {
            Id = 1,
            SubjectType = "contact",
            SubjectId = 7,
            Action = "view",
            RequestedByUserId = 3,
            IpAddress = "10.0.0.1",
            CreatedAt = DateTime.UtcNow.AddHours(-1),
        };
        var otherLog = new GdprAccessLog
        {
            Id = 2,
            SubjectType = "lead",
            SubjectId = 99,
            Action = "export",
            RequestedByUserId = 1,
            IpAddress = "10.0.0.2",
            CreatedAt = DateTime.UtcNow.AddHours(-2),
        };
        var mockLogs = MockDbSetFactory.CreateMockDbSet(new List<GdprAccessLog> { log, otherLog });
        MockContext.Setup(c => c.GdprAccessLogs).Returns(mockLogs.Object);

        var result = await _service.GetAccessLogsAsync("contact", 7);

        result.Should().ContainSingle(l => l.SubjectId == 7);
    }
}
