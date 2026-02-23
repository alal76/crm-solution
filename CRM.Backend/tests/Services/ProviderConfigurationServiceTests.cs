// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.Configuration;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for ProviderConfigurationService.
/// Uses InMemory database for DbContext, mocks IEncryptionService
/// with predictable encrypt/decrypt (prefix "ENC:" added/stripped).
/// </summary>
public class ProviderConfigurationServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<IEncryptionService> _encryptionMock;
    private readonly Mock<ILogger<ProviderConfigurationService>> _loggerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly ProviderConfigurationService _service;

    private const string EncPrefix = "ENC:";

    public ProviderConfigurationServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"ProviderConfigServiceTests_{Guid.NewGuid()}")
            .Options;

        _dbContext = new CrmDbContext(options, null!);
        _loggerMock = new Mock<ILogger<ProviderConfigurationService>>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();

        _encryptionMock = new Mock<IEncryptionService>();
        _encryptionMock.Setup(e => e.Encrypt(It.IsAny<string>()))
            .Returns<string>(plaintext => $"{EncPrefix}{plaintext}");
        _encryptionMock.Setup(e => e.Decrypt(It.IsAny<string>()))
            .Returns<string>(ciphertext =>
                ciphertext.StartsWith(EncPrefix)
                    ? ciphertext[EncPrefix.Length..]
                    : ciphertext);
        _encryptionMock.Setup(e => e.IsEncrypted(It.IsAny<string>()))
            .Returns<string>(value => value.StartsWith(EncPrefix));

        _service = new ProviderConfigurationService(
            _dbContext,
            _encryptionMock.Object,
            _loggerMock.Object,
            _httpClientFactoryMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region Helper Methods

    private async Task SeedUserAsync(int userId = 1, string username = "admin")
    {
        if (!await _dbContext.Users.AnyAsync(u => u.Id == userId))
        {
            _dbContext.Users.Add(new User
            {
                Id = userId,
                Username = username,
                Email = $"{username}@crm.local",
                PasswordHash = "hashed",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task<ProviderConfiguration> SeedConfigAsync(
        string configKey = "system.email.smtp",
        string configType = "system",
        string? providerName = null,
        string configData = """{"smtpServer":"mail.test.com"}""",
        bool isEncrypted = true,
        bool isActive = true,
        bool isDeleted = false,
        int? createdByUserId = 1)
    {
        var storedData = isEncrypted ? $"{EncPrefix}{configData}" : configData;

        var entity = new ProviderConfiguration
        {
            ConfigurationKey = configKey,
            ConfigurationType = configType,
            ProviderName = providerName,
            ConfigurationData = storedData,
            IsEncrypted = isEncrypted,
            IsActive = isActive,
            CanBeDisabledAtRuntime = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId,
            UpdatedByUserId = createdByUserId,
            IsDeleted = isDeleted
        };

        _dbContext.ProviderConfigurations.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    private async Task<ConfigurationChangeLog> SeedChangeLogAsync(
        string configKey = "system.email.smtp",
        string changeType = "updated",
        string? oldValue = null,
        string? newValue = null,
        int? providerConfigId = null,
        bool isDeleted = false)
    {
        var log = new ConfigurationChangeLog
        {
            ConfigurationKey = configKey,
            OldValue = oldValue,
            NewValue = newValue,
            ChangeType = changeType,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = 1,
            ProviderConfigurationId = providerConfigId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = isDeleted
        };

        _dbContext.ConfigurationChangeLogs.Add(log);
        await _dbContext.SaveChangesAsync();
        return log;
    }

    #endregion

    #region GetConfigurationAsync Tests

    [Fact]
    public async Task GetConfigurationAsync_ShouldReturnNull_WhenConfigKeyNotFound()
    {
        // Act
        var result = await _service.GetConfigurationAsync("nonexistent.key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetConfigurationAsync_ShouldReturnDecryptedConfig_WhenConfigExists()
    {
        // Arrange
        var rawData = """{"smtpServer":"mail.test.com","smtpPort":587}""";
        await SeedConfigAsync(configKey: "system.email.smtp", configData: rawData, isEncrypted: true);

        // Act
        var result = await _service.GetConfigurationAsync("system.email.smtp");

        // Assert
        result.Should().NotBeNull();
        result!.ConfigurationKey.Should().Be("system.email.smtp");
        result.ConfigurationData.Should().Be(rawData);
        result.ConfigurationType.Should().Be("system");
    }

    [Fact]
    public async Task GetConfigurationAsync_ShouldExcludeDeletedRecords()
    {
        // Arrange
        await SeedConfigAsync(configKey: "system.deleted.key", isDeleted: true);

        // Act
        var result = await _service.GetConfigurationAsync("system.deleted.key");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllConfigurationsAsync Tests

    [Fact]
    public async Task GetAllConfigurationsAsync_ShouldReturnAllActiveConfigs_WhenNoTypeFilter()
    {
        // Arrange
        await SeedConfigAsync(configKey: "system.email.smtp", configType: "system");
        await SeedConfigAsync(configKey: "crm.ai.openai", configType: "crm", providerName: "openai");
        await SeedConfigAsync(configKey: "crm.ai.deleted", configType: "crm", isDeleted: true);

        // Act
        var result = await _service.GetAllConfigurationsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.ConfigurationKey == "system.email.smtp");
        result.Should().Contain(c => c.ConfigurationKey == "crm.ai.openai");
    }

    [Fact]
    public async Task GetAllConfigurationsAsync_ShouldFilterByType_WhenTypeProvided()
    {
        // Arrange
        await SeedConfigAsync(configKey: "system.email.smtp", configType: "system");
        await SeedConfigAsync(configKey: "crm.ai.openai", configType: "crm", providerName: "openai");
        await SeedConfigAsync(configKey: "crm.ai.ollama", configType: "crm", providerName: "ollama");

        // Act
        var result = await _service.GetAllConfigurationsAsync("crm");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.ConfigurationType == "crm");
    }

    #endregion

    #region UpdateConfigurationAsync Tests

    [Fact]
    public async Task UpdateConfigurationAsync_ShouldCreateNewConfig_WhenKeyDoesNotExist()
    {
        // Arrange
        var configData = new Dictionary<string, object>
        {
            { "apiKey", "sk-test123" },
            { "model", "gpt-4" }
        };

        // Act
        var result = await _service.UpdateConfigurationAsync("crm.ai.openai", configData, userId: 1);

        // Assert
        result.Should().NotBeNull();
        result.ConfigurationKey.Should().Be("crm.ai.openai");
        result.IsEncrypted.Should().BeTrue();

        // Verify it was persisted
        var entity = await _dbContext.ProviderConfigurations
            .FirstOrDefaultAsync(p => p.ConfigurationKey == "crm.ai.openai");
        entity.Should().NotBeNull();
        entity!.ConfigurationData.Should().StartWith(EncPrefix);
    }

    [Fact]
    public async Task UpdateConfigurationAsync_ShouldUpdateExistingConfig_WhenKeyExists()
    {
        // Arrange
        await SeedConfigAsync(configKey: "crm.ai.openai", configType: "crm",
            configData: """{"apiKey":"old-key","model":"gpt-3.5"}""");

        var newConfigData = new Dictionary<string, object>
        {
            { "apiKey", "new-key" },
            { "model", "gpt-4" }
        };

        // Act
        var result = await _service.UpdateConfigurationAsync("crm.ai.openai", newConfigData, userId: 2);

        // Assert
        result.Should().NotBeNull();
        result.ConfigurationKey.Should().Be("crm.ai.openai");

        // Verify the data was updated in database
        var entity = await _dbContext.ProviderConfigurations
            .FirstOrDefaultAsync(p => p.ConfigurationKey == "crm.ai.openai");
        entity.Should().NotBeNull();
        entity!.UpdatedByUserId.Should().Be(2);
    }

    [Fact]
    public async Task UpdateConfigurationAsync_ShouldCreateChangeLog_OnUpdate()
    {
        // Arrange
        await SeedConfigAsync(configKey: "system.email.smtp", configType: "system",
            configData: """{"smtpServer":"old.mail.com"}""");

        var newConfigData = new Dictionary<string, object>
        {
            { "smtpServer", "new.mail.com" },
            { "smtpPort", 465 }
        };

        // Act
        await _service.UpdateConfigurationAsync("system.email.smtp", newConfigData, userId: 1);

        // Assert
        var changeLogs = await _dbContext.ConfigurationChangeLogs
            .Where(c => c.ConfigurationKey == "system.email.smtp")
            .ToListAsync();
        changeLogs.Should().NotBeEmpty();
        changeLogs.Should().Contain(c => c.ChangeType == "updated");
    }

    [Fact]
    public async Task UpdateConfigurationAsync_ShouldEncryptData_BeforeSaving()
    {
        // Arrange
        var configData = new Dictionary<string, object>
        {
            { "apiKey", "super-secret-key" }
        };

        // Act
        await _service.UpdateConfigurationAsync("crm.ai.anthropic", configData, userId: 1);

        // Assert
        _encryptionMock.Verify(e => e.Encrypt(It.IsAny<string>()), Times.AtLeastOnce);

        var entity = await _dbContext.ProviderConfigurations
            .FirstOrDefaultAsync(p => p.ConfigurationKey == "crm.ai.anthropic");
        entity.Should().NotBeNull();
        entity!.IsEncrypted.Should().BeTrue();
        entity.ConfigurationData.Should().StartWith(EncPrefix);
    }

    #endregion

    #region GetAvailableProvidersAsync Tests

    [Fact]
    public async Task GetAvailableProvidersAsync_ShouldReturnProviders_ForValidType()
    {
        // Act
        var result = await _service.GetAvailableProvidersAsync("ai");

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(p => p.Id == "openai");
        result.Should().Contain(p => p.Id == "ollama");
        result.Should().Contain(p => p.Id == "anthropic");
    }

    [Fact]
    public async Task GetAvailableProvidersAsync_ShouldReturnEmpty_ForInvalidType()
    {
        // Act
        var result = await _service.GetAvailableProvidersAsync("nonexistent_type");

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("search", "meilisearch")]
    [InlineData("chat", "chatwoot")]
    [InlineData("notifications", "novu")]
    [InlineData("analytics", "superset")]
    [InlineData("signatures", "docuseal")]
    [InlineData("workflows", "n8n")]
    public async Task GetAvailableProvidersAsync_ShouldReturnCorrectProviders_ForEachType(
        string type, string expectedProviderId)
    {
        // Act
        var result = await _service.GetAvailableProvidersAsync(type);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(p => p.Id == expectedProviderId);
    }

    #endregion

    #region GetChangeHistoryAsync Tests

    [Fact]
    public async Task GetChangeHistoryAsync_ShouldReturnFilteredLogs_WhenConfigKeyProvided()
    {
        // Arrange
        await SeedUserAsync(1, "admin");
        var config = await SeedConfigAsync(configKey: "system.email.smtp");
        await SeedChangeLogAsync(configKey: "system.email.smtp", changeType: "updated",
            providerConfigId: config.Id);
        await SeedChangeLogAsync(configKey: "crm.ai.openai", changeType: "created");

        // Act
        var result = await _service.GetChangeHistoryAsync("system.email.smtp");

        // Assert
        result.Should().HaveCount(1);
        result.First().ConfigurationKey.Should().Be("system.email.smtp");
    }

    [Fact]
    public async Task GetChangeHistoryAsync_ShouldReturnAllLogs_WhenNoFilter()
    {
        // Arrange
        await SeedUserAsync(1, "admin");
        await SeedChangeLogAsync(configKey: "system.email.smtp", changeType: "updated");
        await SeedChangeLogAsync(configKey: "crm.ai.openai", changeType: "created");
        await SeedChangeLogAsync(configKey: "crm.ai.ollama", changeType: "created");

        // Act
        var result = await _service.GetChangeHistoryAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    #endregion

    #region RollbackConfigurationAsync Tests

    [Fact]
    public async Task RollbackConfigurationAsync_ShouldRestoreOldValue_WhenChangeLogExists()
    {
        // Arrange — seed a config and a change log with a valid old value
        var config = await SeedConfigAsync(configKey: "system.email.smtp",
            configData: """{"smtpServer":"new.mail.com"}""");

        var oldData = """{"smtpServer":"old.mail.com","smtpPort":587}""";
        var encryptedOldData = $"{EncPrefix}{oldData}";

        var changeLog = await SeedChangeLogAsync(
            configKey: "system.email.smtp",
            changeType: "updated",
            oldValue: encryptedOldData,
            newValue: $"{EncPrefix}{{\"smtpServer\":\"new.mail.com\"}}",
            providerConfigId: config.Id);

        // Act
        var result = await _service.RollbackConfigurationAsync(changeLog.Id, userId: 1);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("rolled back successfully");
    }

    [Fact]
    public async Task RollbackConfigurationAsync_ShouldReturnFailure_WhenChangeLogNotFound()
    {
        // Act
        var result = await _service.RollbackConfigurationAsync(99999, userId: 1);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task RollbackConfigurationAsync_ShouldReturnFailure_WhenNoOldValueExists()
    {
        // Arrange — change log with no old value (creation event)
        var changeLog = await SeedChangeLogAsync(
            configKey: "system.email.smtp",
            changeType: "created",
            oldValue: null,
            newValue: $"{EncPrefix}{{\"smtpServer\":\"mail.com\"}}");

        // Act
        var result = await _service.RollbackConfigurationAsync(changeLog.Id, userId: 1);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("no previous value");
    }

    #endregion
}
