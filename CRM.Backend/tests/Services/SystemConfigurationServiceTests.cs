// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Ports;
using CRM.Infrastructure.Services.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for SystemConfigurationService.
/// Mocks IProviderConfigurationService directly (no DB).
/// </summary>
public class SystemConfigurationServiceTests
{
    private readonly Mock<IProviderConfigurationService> _providerConfigMock;
    private readonly Mock<ILogger<SystemConfigurationService>> _loggerMock;
    private readonly SystemConfigurationService _service;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public SystemConfigurationServiceTests()
    {
        _providerConfigMock = new Mock<IProviderConfigurationService>();
        _loggerMock = new Mock<ILogger<SystemConfigurationService>>();

        _service = new SystemConfigurationService(
            _providerConfigMock.Object,
            _loggerMock.Object);
    }

    #region Helper Methods

    private static ProviderConfigurationDto CreateProviderConfigDto(
        string configKey,
        object configDataObj,
        string configType = "system",
        string? providerName = null)
    {
        var json = JsonSerializer.Serialize(configDataObj, JsonOptions);
        return new ProviderConfigurationDto
        {
            Id = 1,
            ConfigurationKey = configKey,
            ConfigurationType = configType,
            ProviderName = providerName,
            ConfigurationData = json,
            IsEncrypted = true,
            IsActive = true,
            CanBeDisabledAtRuntime = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UpdatedByUserName = "admin"
        };
    }

    private void SetupGetConfigReturnsNull(string configKey)
    {
        _providerConfigMock
            .Setup(p => p.GetConfigurationAsync(configKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProviderConfigurationDto?)null);
    }

    private void SetupGetConfigReturns(string configKey, object configDataObj)
    {
        _providerConfigMock
            .Setup(p => p.GetConfigurationAsync(configKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProviderConfigDto(configKey, configDataObj));
    }

    #endregion

    #region GetSystemConfigAsync Tests

    [Fact]
    public async Task GetSystemConfigAsync_ShouldReturnEmptyConfig_WhenNothingConfigured()
    {
        // Arrange — all config lookups return null
        SetupGetConfigReturnsNull("system.email.smtp");
        SetupGetConfigReturnsNull("system.2fa.config");
        SetupGetConfigReturnsNull("system.sso.google");
        SetupGetConfigReturnsNull("system.sso.microsoft");
        SetupGetConfigReturnsNull("system.sso.azure");
        SetupGetConfigReturnsNull("system.sso.linkedin");
        SetupGetConfigReturnsNull("system.sso.facebook");

        // Act
        var result = await _service.GetSystemConfigAsync();

        // Assert
        result.Should().NotBeNull();
        result.EmailServer.Should().BeNull();
        result.TwoFactor.Should().BeNull();
        result.SocialLogin.Should().NotBeNull(); // Always initialized
        result.SocialLogin!.Google.Should().BeNull();
        result.SocialLogin!.Microsoft.Should().BeNull();
    }

    [Fact]
    public async Task GetSystemConfigAsync_ShouldReturnEmailConfig_WhenEmailConfigured()
    {
        // Arrange
        var emailConfig = new EmailServerConfigDto
        {
            SmtpServer = "smtp.example.com",
            SmtpPort = 587,
            UseTls = true,
            FromEmail = "noreply@example.com",
            FromName = "CRM System"
        };

        SetupGetConfigReturns("system.email.smtp", emailConfig);
        SetupGetConfigReturnsNull("system.2fa.config");
        SetupGetConfigReturnsNull("system.sso.google");
        SetupGetConfigReturnsNull("system.sso.microsoft");
        SetupGetConfigReturnsNull("system.sso.azure");
        SetupGetConfigReturnsNull("system.sso.linkedin");
        SetupGetConfigReturnsNull("system.sso.facebook");

        // Act
        var result = await _service.GetSystemConfigAsync();

        // Assert
        result.EmailServer.Should().NotBeNull();
        result.EmailServer!.SmtpServer.Should().Be("smtp.example.com");
        result.EmailServer.SmtpPort.Should().Be(587);
        result.EmailServer.UseTls.Should().BeTrue();
    }

    [Fact]
    public async Task GetSystemConfigAsync_ShouldReturnSocialLogin_WhenSSOConfigured()
    {
        // Arrange
        SetupGetConfigReturnsNull("system.email.smtp");
        SetupGetConfigReturnsNull("system.2fa.config");
        SetupGetConfigReturnsNull("system.sso.microsoft");
        SetupGetConfigReturnsNull("system.sso.azure");
        SetupGetConfigReturnsNull("system.sso.linkedin");
        SetupGetConfigReturnsNull("system.sso.facebook");

        var googleConfig = new GoogleOAuthDto
        {
            Enabled = true,
            ClientId = "google-client-id-123",
            ClientSecret = "google-secret"
        };
        SetupGetConfigReturns("system.sso.google", googleConfig);

        // Act
        var result = await _service.GetSystemConfigAsync();

        // Assert
        result.SocialLogin.Should().NotBeNull();
        result.SocialLogin!.Google.Should().NotBeNull();
        result.SocialLogin.Google!.Enabled.Should().BeTrue();
        result.SocialLogin.Google.ClientId.Should().Be("google-client-id-123");
    }

    #endregion

    #region UpdateEmailServerAsync Tests

    [Fact]
    public async Task UpdateEmailServerAsync_ShouldCallProviderConfigService_WithCorrectKey()
    {
        // Arrange
        var emailConfig = new EmailServerConfigDto
        {
            SmtpServer = "smtp.example.com",
            SmtpPort = 587,
            UseTls = true,
            FromEmail = "noreply@example.com",
            FromName = "CRM"
        };

        _providerConfigMock
            .Setup(p => p.UpdateConfigurationAsync(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProviderConfigDto("system.email.smtp", emailConfig));

        // Act
        await _service.UpdateEmailServerAsync(emailConfig, userId: 1);

        // Assert
        _providerConfigMock.Verify(
            p => p.UpdateConfigurationAsync(
                "system.email.smtp",
                It.IsAny<Dictionary<string, object>>(),
                1,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region UpdateTwoFactorAsync Tests

    [Fact]
    public async Task UpdateTwoFactorAsync_ShouldCallProviderConfigService_WithCorrectKey()
    {
        // Arrange
        var twoFaConfig = new TwoFactorConfigDto
        {
            Provider = "totp",
            Required = true,
            Issuer = "CRM Solution"
        };

        _providerConfigMock
            .Setup(p => p.UpdateConfigurationAsync(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProviderConfigDto("system.2fa.config", twoFaConfig));

        // Act
        await _service.UpdateTwoFactorAsync(twoFaConfig, userId: 1);

        // Assert
        _providerConfigMock.Verify(
            p => p.UpdateConfigurationAsync(
                "system.2fa.config",
                It.IsAny<Dictionary<string, object>>(),
                1,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region UpdateSocialLoginAsync Tests

    [Fact]
    public async Task UpdateSocialLoginAsync_ShouldUpdateAllProviders_WhenAllProvided()
    {
        // Arrange
        var socialConfig = new SocialLoginConfigDto
        {
            Google = new GoogleOAuthDto { Enabled = true, ClientId = "g-id", ClientSecret = "g-secret" },
            Microsoft = new MicrosoftOAuthDto { Enabled = true, ClientId = "ms-id", ClientSecret = "ms-secret", TenantId = "t-1" },
            AzureAd = new AzureAdDto { Enabled = true, ClientId = "az-id", ClientSecret = "az-secret", TenantId = "t-2" },
            LinkedIn = new LinkedInOAuthDto { Enabled = true, ClientId = "li-id", ClientSecret = "li-secret" },
            Facebook = new FacebookOAuthDto { Enabled = true, AppId = "fb-id", AppSecret = "fb-secret" }
        };

        _providerConfigMock
            .Setup(p => p.UpdateConfigurationAsync(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, Dictionary<string, object> _, int _, CancellationToken _) =>
                CreateProviderConfigDto(key, new { }));

        // Act
        await _service.UpdateSocialLoginAsync(socialConfig, userId: 1);

        // Assert — each SSO provider key should be updated
        _providerConfigMock.Verify(p => p.UpdateConfigurationAsync(
            "system.sso.google", It.IsAny<Dictionary<string, object>>(), 1, It.IsAny<CancellationToken>()), Times.Once);
        _providerConfigMock.Verify(p => p.UpdateConfigurationAsync(
            "system.sso.microsoft", It.IsAny<Dictionary<string, object>>(), 1, It.IsAny<CancellationToken>()), Times.Once);
        _providerConfigMock.Verify(p => p.UpdateConfigurationAsync(
            "system.sso.azure", It.IsAny<Dictionary<string, object>>(), 1, It.IsAny<CancellationToken>()), Times.Once);
        _providerConfigMock.Verify(p => p.UpdateConfigurationAsync(
            "system.sso.linkedin", It.IsAny<Dictionary<string, object>>(), 1, It.IsAny<CancellationToken>()), Times.Once);
        _providerConfigMock.Verify(p => p.UpdateConfigurationAsync(
            "system.sso.facebook", It.IsAny<Dictionary<string, object>>(), 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region TestEmailServerAsync Tests

    [Fact]
    public async Task TestEmailServerAsync_ShouldReturnFailure_WhenConnectionFails()
    {
        // Arrange — use an unreachable server on a random port
        var config = new EmailServerConfigDto
        {
            SmtpServer = "192.0.2.1",   // RFC 5737 TEST-NET — guaranteed non-routable
            SmtpPort = 25,
            UseTls = false,
            FromEmail = "test@test.com",
            FromName = "Test"
        };

        // Act
        var result = await _service.TestEmailServerAsync(config);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        // The failure message should mention the SMTP server or connection issue
        result.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task TestEmailServerAsync_ShouldReturnFailure_WhenSmtpServerIsEmpty()
    {
        // Arrange
        var config = new EmailServerConfigDto
        {
            SmtpServer = "",
            SmtpPort = 587,
            FromEmail = "test@test.com",
            FromName = "Test"
        };

        // Act
        var result = await _service.TestEmailServerAsync(config);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("SMTP server");
    }

    #endregion

    #region TestSocialProviderAsync Tests

    [Fact]
    public async Task TestSocialProviderAsync_ShouldReturnSuccess_ForValidGoogleConfig()
    {
        // Arrange
        var credentials = new Dictionary<string, string>
        {
            { "clientId", "google-client-id" },
            { "clientSecret", "google-client-secret" }
        };

        // Act
        var result = await _service.TestSocialProviderAsync("google", credentials);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("structurally valid");
    }

    [Fact]
    public async Task TestSocialProviderAsync_ShouldReturnFailure_ForMissingClientId()
    {
        // Arrange — missing clientId
        var credentials = new Dictionary<string, string>
        {
            { "clientSecret", "google-client-secret" }
        };

        // Act
        var result = await _service.TestSocialProviderAsync("google", credentials);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("clientId");
    }

    [Fact]
    public async Task TestSocialProviderAsync_ShouldReturnFailure_ForUnknownProvider()
    {
        // Arrange
        var credentials = new Dictionary<string, string>
        {
            { "clientId", "id" },
            { "clientSecret", "secret" }
        };

        // Act
        var result = await _service.TestSocialProviderAsync("unknown_provider", credentials);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Unknown provider");
    }

    #endregion
}
