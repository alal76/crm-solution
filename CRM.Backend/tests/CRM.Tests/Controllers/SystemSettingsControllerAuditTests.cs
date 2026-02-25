// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for SystemSettingsController — verifies audit logging behaviour
/// when UpdateSettings is invoked. TODO-SYS009-004
/// </summary>
public class SystemSettingsControllerAuditTests
{
    private readonly Mock<ISystemSettingsService> _mockSettings;
    private readonly Mock<ILogger<CRM.API.Controllers.SystemSettingsController>> _mockLogger;
    private readonly Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment> _mockEnv;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<IAuditLogService> _mockAuditLog;
    private readonly SystemSettingsDto _settingsDto;

    public SystemSettingsControllerAuditTests()
    {
        _mockSettings = new Mock<ISystemSettingsService>();
        _mockLogger = new Mock<ILogger<CRM.API.Controllers.SystemSettingsController>>();
        _mockEnv = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        _mockConfig = new Mock<IConfiguration>();
        _mockAuditLog = new Mock<IAuditLogService>();

        _settingsDto = new SystemSettingsDto { CompanyName = "Test Corp" };

        _mockSettings
            .Setup(s => s.UpdateSettingsAsync(It.IsAny<UpdateSystemSettingsRequest>(), It.IsAny<int?>()))
            .ReturnsAsync(_settingsDto);
        _mockConfig.Setup(c => c["SSL_CERT_PASSWORD"]).Returns("test-pass");
    }

    private CRM.API.Controllers.SystemSettingsController CreateController(bool withAuditLog)
    {
        var controller = new CRM.API.Controllers.SystemSettingsController(
            _mockSettings.Object,
            _mockLogger.Object,
            _mockEnv.Object,
            _mockConfig.Object,
            auditLogService: withAuditLog ? _mockAuditLog.Object : null);

        // Simulate an authenticated admin user with a known userId claim
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "42") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        return controller;
    }

    // ── With audit service ────────────────────────────────────────────────────

    /// <summary>
    /// UpdateSettings should call LogActionAsync exactly once with the correct
    /// entity type when an IAuditLogService is registered.
    /// </summary>
    [Fact]
    public async Task UpdateSettings_ShouldCallLogActionAsync_WhenAuditServiceIsProvided()
    {
        // Arrange
        _mockAuditLog
            .Setup(a => a.LogActionAsync(
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int?>(),
                It.IsAny<int?>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var controller = CreateController(withAuditLog: true);
        var request = new UpdateSystemSettingsRequest { CompanyName = "New Corp" };

        // Act
        var result = await controller.UpdateSettings(request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _mockAuditLog.Verify(
            a => a.LogActionAsync(
                "Update",
                "SystemSettings",
                null,
                42,  // userId parsed from claim
                It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once,
            "Audit log must be written once when settings are updated");
    }

    // ── Without audit service ────────────────────────────────────────────────

    /// <summary>
    /// UpdateSettings should succeed and return 200 OK even when no
    /// IAuditLogService is registered (backward-compat / optional injection).
    /// </summary>
    [Fact]
    public async Task UpdateSettings_ShouldSucceed_WhenAuditServiceIsNull()
    {
        // Arrange
        var controller = CreateController(withAuditLog: false);
        var request = new UpdateSystemSettingsRequest { CompanyName = "Corp Without Audit" };

        // Act
        var result = await controller.UpdateSettings(request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>(
            "UpdateSettings must return 200 OK even without an audit service");

        // Sanity check — audit mock never called
        _mockAuditLog.Verify(
            a => a.LogActionAsync(
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
