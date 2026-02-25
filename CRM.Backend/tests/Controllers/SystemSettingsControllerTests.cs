// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Threading.Tasks;
using CRM.API.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for SystemSettingsController.
/// Tests GetSettings, UpdateSettings, GetModuleStatus, and ToggleModule endpoints.
/// Covers TODO-SYS009-002.
/// </summary>
public class SystemSettingsControllerTests
{
    private readonly Mock<ISystemSettingsService> _mockSettingsService;
    private readonly Mock<ILogger<SystemSettingsController>> _mockLogger;
    private readonly Mock<IWebHostEnvironment> _mockEnv;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly SystemSettingsController _controller;

    public SystemSettingsControllerTests()
    {
        _mockSettingsService = new Mock<ISystemSettingsService>();
        _mockLogger = new Mock<ILogger<SystemSettingsController>>();
        _mockEnv = new Mock<IWebHostEnvironment>();
        _mockConfig = new Mock<IConfiguration>();

        // Config returns empty string for SSL cert password
        _mockConfig.Setup(c => c["SSL_CERT_PASSWORD"]).Returns("test-password");

        _controller = new SystemSettingsController(
            _mockSettingsService.Object,
            _mockLogger.Object,
            _mockEnv.Object,
            _mockConfig.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    #region GetSettings Tests

    [Fact]
    public async Task GetSettings_ShouldReturnOk_WithSettings()
    {
        // Arrange
        var settings = new SystemSettingsDto
        {
            Id = 1,
            CompanyName = "ACME Corp",
            AccountsEnabled = true,
            LeadsEnabled = true
        };
        _mockSettingsService
            .Setup(s => s.GetSettingsAsync())
            .ReturnsAsync(settings);

        // Act
        var result = await _controller.GetSettings();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedSettings = okResult.Value.Should().BeOfType<SystemSettingsDto>().Subject;
        returnedSettings.CompanyName.Should().Be("ACME Corp");
    }

    [Fact]
    public async Task GetSettings_ShouldReturn500_WhenServiceThrows()
    {
        // Arrange
        _mockSettingsService
            .Setup(s => s.GetSettingsAsync())
            .ThrowsAsync(new Exception("DB error"));

        // Act
        var result = await _controller.GetSettings();

        // Assert
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetModuleStatus Tests

    [Fact]
    public async Task GetModuleStatus_ShouldReturnOk_WithModuleStatus()
    {
        // Arrange
        var moduleStatus = new ModuleStatusDto
        {
            ModuleName = "CRM",
            DisplayName = "CRM Modules",
            IsOperational = true,
            Status = "Operational"
        };
        _mockSettingsService
            .Setup(s => s.GetModuleStatusAsync())
            .ReturnsAsync(moduleStatus);

        // Act
        var result = await _controller.GetModuleStatus();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<ModuleStatusDto>();
    }

    [Fact]
    public async Task GetModuleStatus_ShouldReturn500_WhenServiceThrows()
    {
        // Arrange
        _mockSettingsService
            .Setup(s => s.GetModuleStatusAsync())
            .ThrowsAsync(new Exception("Service failure"));

        // Act
        var result = await _controller.GetModuleStatus();

        // Assert
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region UpdateSettings Tests

    [Fact]
    public async Task UpdateSettings_ShouldReturnOk_WhenUpdateSucceeds()
    {
        // Arrange
        var request = new UpdateSystemSettingsRequest
        {
            CompanyName = "New Company Name"
        };
        var updated = new SystemSettingsDto
        {
            CompanyName = "New Company Name"
        };
        _mockSettingsService
            .Setup(s => s.UpdateSettingsAsync(request, It.IsAny<int?>()))
            .ReturnsAsync(updated);

        // Act
        var result = await _controller.UpdateSettings(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedSettings = okResult.Value.Should().BeOfType<SystemSettingsDto>().Subject;
        returnedSettings.CompanyName.Should().Be("New Company Name");
    }

    [Fact]
    public async Task UpdateSettings_ShouldReturn500_WhenServiceThrows()
    {
        // Arrange
        var request = new UpdateSystemSettingsRequest { CompanyName = "X" };
        _mockSettingsService
            .Setup(s => s.UpdateSettingsAsync(request, It.IsAny<int?>()))
            .ThrowsAsync(new Exception("Update error"));

        // Act
        var result = await _controller.UpdateSettings(request);

        // Assert
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region ToggleModule Tests

    [Fact]
    public async Task ToggleModule_ShouldReturnOk_WhenModuleNameIsValid()
    {
        // Arrange
        var updatedSettings = new SystemSettingsDto { LeadsEnabled = false };
        _mockSettingsService
            .Setup(s => s.UpdateSettingsAsync(It.IsAny<UpdateSystemSettingsRequest>(), It.IsAny<int?>()))
            .ReturnsAsync(updatedSettings);

        // Act
        var result = await _controller.ToggleModule("leads", enabled: false);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<SystemSettingsDto>();
    }

    [Fact]
    public async Task ToggleModule_ShouldReturnBadRequest_WhenModuleNameIsUnknown()
    {
        // Act
        var result = await _controller.ToggleModule("nonexistent-module", enabled: true);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ToggleModule_ShouldReturnOk_WhenTogglingAccounts()
    {
        // Arrange
        var updatedSettings = new SystemSettingsDto { AccountsEnabled = true };
        _mockSettingsService
            .Setup(s => s.UpdateSettingsAsync(It.IsAny<UpdateSystemSettingsRequest>(), It.IsAny<int?>()))
            .ReturnsAsync(updatedSettings);

        // Act
        var result = await _controller.ToggleModule("accounts", enabled: true);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    #endregion
}
