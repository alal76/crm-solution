// CRM Solution — CRM Test Suite
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.BuiltIn;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

#nullable enable

/// <summary>Tests for <see cref="BuiltInNotificationProvider"/> (TCOV-055).</summary>
public class BuiltInNotificationProviderTests
{
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly Mock<ILogger<BuiltInNotificationProvider>> _loggerMock = new();

    private BuiltInNotificationProvider Create()
    {
        // Bind an empty configuration section for Smtp
        var configSection = new Mock<IConfigurationSection>();
        configSection.Setup(s => s.Key).Returns("Smtp");
        configSection.Setup(s => s.GetChildren()).Returns(Enumerable.Empty<IConfigurationSection>());
        _configMock.Setup(c => c.GetSection("Smtp")).Returns(configSection.Object);
        return new BuiltInNotificationProvider(_configMock.Object, _loggerMock.Object);
    }

    // ─── Constructor ─────────────────────────────────────────────────────────────
    [Fact]
    public void Constructor_NullConfiguration_ShouldThrow()
    {
        var act = () => new BuiltInNotificationProvider(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var configSection = new Mock<IConfigurationSection>();
        configSection.Setup(s => s.GetChildren()).Returns(Enumerable.Empty<IConfigurationSection>());
        _configMock.Setup(c => c.GetSection("Smtp")).Returns(configSection.Object);
        var act = () => new BuiltInNotificationProvider(_configMock.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_ValidParams_ShouldNotThrow()
    {
        var act = () => Create();
        act.Should().NotThrow();
    }

    // ─── Properties ─────────────────────────────────────────────────────────────
    [Fact]
    public void ProviderName_ShouldReturnBuiltIn()
    {
        Create().ProviderName.Should().Be("BuiltIn");
    }

    [Fact]
    public void SupportedChannels_ShouldContainEmail()
    {
        Create().SupportedChannels.Should().Contain("email");
    }

    // ─── Email ───────────────────────────────────────────────────────────────────
    [Fact]
    public async Task SendEmailAsync_NullRequest_ShouldThrow()
    {
        var act = async () => await Create().SendEmailAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendEmailAsync_EmptyTo_ShouldThrow()
    {
        var req = new EmailNotificationRequest { To = "", Subject = "Hi", Body = "Body" };
        var act = async () => await Create().SendEmailAsync(req);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SendEmailAsync_EmptySubject_ShouldThrow()
    {
        var req = new EmailNotificationRequest { To = "t@t.com", Subject = "", Body = "Body" };
        var act = async () => await Create().SendEmailAsync(req);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SendEmailAsync_NoSmtpConfigured_ShouldReturnDevSuccess()
    {
        // SMTP not configured => dev mode returns success
        var result = await Create().SendEmailAsync(new EmailNotificationRequest
        {
            To = "user@example.com",
            Subject = "Hello",
            Body = "Test body"
        });
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Provider.Should().Be("BuiltIn");
        result.Channel.Should().Be("email");
    }

    [Fact]
    public async Task SendTemplateEmailAsync_ShouldReturnUnsupported()
    {
        var result = await Create().SendTemplateEmailAsync("tpl-001", "user@example.com", new { });
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
    }
}
