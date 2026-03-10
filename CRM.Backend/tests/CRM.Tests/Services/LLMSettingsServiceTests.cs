// CRM Solution — Unit Tests
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for LLMSettingsService (TCOV-035).</summary>
public class LLMSettingsServiceTests
{
    private readonly Mock<ICrmDbContext> _mockCtx;
    private readonly Mock<IEncryptionService> _mockEncryption;
    private readonly LLMSettingsService _service;

    private readonly List<LLMProviderSetting> _settings;

    public LLMSettingsServiceTests()
    {
        _mockCtx = new Mock<ICrmDbContext>();
        _mockEncryption = new Mock<IEncryptionService>();
        var logger = new Mock<ILogger<LLMSettingsService>>().Object;
        var mockServiceProvider = new Mock<IServiceProvider>().Object;
        var options = Options.Create(new LLMProviderOptions());

        _settings = new List<LLMProviderSetting>();
        _mockCtx.Setup(c => c.LLMProviderSettings).Returns(MockDbSetFactory.CreateMockDbSet(_settings).Object);
        _mockCtx.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mockEncryption.Setup(e => e.IsEncrypted(It.IsAny<string>())).Returns(false);
        _mockEncryption.Setup(e => e.Encrypt(It.IsAny<string>())).Returns<string>(s => "ENC:" + s);
        _mockEncryption.Setup(e => e.Decrypt(It.IsAny<string>())).Returns<string>(s => s.Replace("ENC:", ""));

        _service = new LLMSettingsService(_mockCtx.Object, logger, options, mockServiceProvider, _mockEncryption.Object);
    }

    // ── GetSettingsAsync ──────────────────────────────────────────────────────
    [Fact]
    public async Task GetSettingsAsync_ShouldReturnDefaultProvider_WhenNoDbSettings()
    {
        var result = await _service.GetSettingsAsync();
        result.Should().NotBeNull();
        // Default from LLMProviderOptions (empty string when not configured)
        result.DefaultProvider.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSettingsAsync_ShouldUseDbValue_WhenSettingExists()
    {
        _settings.Add(new LLMProviderSetting
        {
            Id = 1,
            SettingKey = "DefaultProvider",
            SettingValue = "openai",
            IsDeleted = false
        });
        _mockCtx.Setup(c => c.LLMProviderSettings).Returns(MockDbSetFactory.CreateMockDbSet(_settings).Object);

        var result = await _service.GetSettingsAsync();
        result.DefaultProvider.Should().Be("openai");
    }

    // ── GetSettingValueAsync ──────────────────────────────────────────────────
    [Fact]
    public async Task GetSettingValueAsync_ShouldReturnNull_WhenKeyNotFound()
    {
        var result = await _service.GetSettingValueAsync("NonExistentKey");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSettingValueAsync_ShouldReturnValue_WhenKeyExists()
    {
        _settings.Add(new LLMProviderSetting
        {
            Id = 1,
            SettingKey = "TimeoutSeconds",
            SettingValue = "60",
            IsDeleted = false
        });
        _mockCtx.Setup(c => c.LLMProviderSettings).Returns(MockDbSetFactory.CreateMockDbSet(_settings).Object);

        var result = await _service.GetSettingValueAsync("TimeoutSeconds");
        result.Should().Be("60");
    }

    // ── GetSettingsByCategoryAsync ────────────────────────────────────────────
    [Fact]
    public async Task GetSettingsByCategoryAsync_ShouldReturnEmpty_WhenNoneInCategory()
    {
        var result = await _service.GetSettingsByCategoryAsync("nonexistent");
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSettingsByCategoryAsync_ShouldFilterByCategory()
    {
        _settings.AddRange(new[]
        {
            new LLMProviderSetting { Id = 1, SettingKey = "k1", SettingValue = "v1", Category = "openai", IsDeleted = false },
            new LLMProviderSetting { Id = 2, SettingKey = "k2", SettingValue = "v2", Category = "anthropic", IsDeleted = false }
        });
        _mockCtx.Setup(c => c.LLMProviderSettings).Returns(MockDbSetFactory.CreateMockDbSet(_settings).Object);

        var result = await _service.GetSettingsByCategoryAsync("openai");
        result.Should().ContainKey("k1");
        result.Should().NotContainKey("k2");
    }
}
