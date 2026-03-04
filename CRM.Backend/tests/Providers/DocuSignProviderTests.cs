// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.DocuSign;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for DocuSignProvider.
/// DocuSign uses the official DocuSign .NET SDK (JWT auth) – actual API calls require
/// real credentials and cannot be mocked at the HTTP layer. Tests therefore cover:
///   - Constructor behaviour with empty / partial configuration
///   - ProviderName
///   - IsAvailableAsync short-circuits to false when configuration is invalid
///   - DocuSignConfiguration.Validate() logic for all required fields
///   - DocuSignConfiguration helper methods (GetApiBaseUrl, GetOAuthBaseUrl)
///
/// MANDATORY: Written after verifying source signature:
/// Class: DocuSignProvider, Namespace: CRM.Infrastructure.Providers.DocuSign
/// Constructor: (IOptions&lt;DocuSignConfiguration&gt;, ILogger&lt;DocuSignProvider&gt;)
/// No HttpClient is injected – provider uses DocuSign SDK's DocuSignClient directly.
/// </summary>
public class DocuSignProviderTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static DocuSignProvider CreateProvider(DocuSignConfiguration? config = null)
    {
        var effectiveConfig = config ?? new DocuSignConfiguration();
        var options = Options.Create(effectiveConfig);
        var logger = new Mock<ILogger<DocuSignProvider>>();
        return new DocuSignProvider(options, logger.Object);
    }

    private static DocuSignConfiguration ValidConfig() => new()
    {
        IntegrationKey = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
        UserId = "11111111-2222-3333-4444-555555555555",
        AccountId = "99999999-aaaa-bbbb-cccc-dddddddddddd",
        RsaPrivateKey = "-----BEGIN RSA PRIVATE KEY-----\nFAKEKEY\n-----END RSA PRIVATE KEY-----",
        Environment = "demo",
        DefaultExpirationDays = 14,
        TimeoutSeconds = 60
    };

    // ── Constructor Guards ───────────────────────────────────────────────────

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenConfigIsNull()
    {
        var act = () => new DocuSignProvider(null!, new Mock<ILogger<DocuSignProvider>>().Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        var act = () => new DocuSignProvider(Options.Create(new DocuSignConfiguration()), null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_Succeeds_WhenIntegrationKeyIsEmpty()
    {
        // DocuSignProvider allows deferred configuration – no exception when key is empty
        var config = new DocuSignConfiguration();
        var provider = CreateProvider(config);

        provider.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_Succeeds_WithValidConfig()
    {
        var provider = CreateProvider(ValidConfig());
        provider.Should().NotBeNull();
    }

    // ── Provider Metadata ────────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsDocuSign()
    {
        var provider = CreateProvider();
        provider.ProviderName.Should().Be("DocuSign");
    }

    // ── IsAvailableAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenNotConfigured()
    {
        // Empty config means Validate() returns false → IsAvailableAsync should short-circuit
        var provider = CreateProvider(new DocuSignConfiguration());

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenRsaPrivateKeyIsMissing()
    {
        var config = new DocuSignConfiguration
        {
            IntegrationKey = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
            UserId = "11111111-2222-3333-4444-555555555555",
            AccountId = "99999999-aaaa-bbbb-cccc-dddddddddddd",
            RsaPrivateKey = "",
            Environment = "demo"
        };
        var provider = CreateProvider(config);

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    // ── CreateSignatureRequestAsync argument guards ────────────────────────────

    [Fact]
    public async Task CreateSignatureRequestAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        var provider = CreateProvider();

        var act = async () => await provider.CreateSignatureRequestAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── GetTemplateAsync argument guards ──────────────────────────────────────

    [Fact]
    public async Task GetTemplateAsync_ThrowsArgumentException_WhenTemplateIdIsWhitespace()
    {
        var provider = CreateProvider();

        var act = async () => await provider.GetTemplateAsync("   ");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ── DocuSignConfiguration.Validate ───────────────────────────────────────

    [Fact]
    public void DocuSignConfiguration_Validate_ReturnsFalse_WhenIntegrationKeyIsEmpty()
    {
        var config = ValidConfig();
        config.IntegrationKey = "";

        var (isValid, error) = config.Validate();

        isValid.Should().BeFalse();
        error.Should().Contain("Integration Key");
    }

    [Fact]
    public void DocuSignConfiguration_Validate_ReturnsFalse_WhenUserIdIsEmpty()
    {
        var config = ValidConfig();
        config.UserId = "";

        var (isValid, error) = config.Validate();

        isValid.Should().BeFalse();
        error.Should().Contain("User ID");
    }

    [Fact]
    public void DocuSignConfiguration_Validate_ReturnsFalse_WhenAccountIdIsEmpty()
    {
        var config = ValidConfig();
        config.AccountId = "";

        var (isValid, error) = config.Validate();

        isValid.Should().BeFalse();
        error.Should().Contain("Account ID");
    }

    [Fact]
    public void DocuSignConfiguration_Validate_ReturnsFalse_WhenRsaPrivateKeyIsEmpty()
    {
        var config = ValidConfig();
        config.RsaPrivateKey = "";

        var (isValid, error) = config.Validate();

        isValid.Should().BeFalse();
        error.Should().Contain("RSA Private Key");
    }

    [Fact]
    public void DocuSignConfiguration_Validate_ReturnsFalse_WhenEnvironmentIsInvalid()
    {
        var config = ValidConfig();
        config.Environment = "staging";

        var (isValid, error) = config.Validate();

        isValid.Should().BeFalse();
        error.Should().Contain("Environment");
    }

    [Fact]
    public void DocuSignConfiguration_Validate_ReturnsTrue_WhenAllFieldsAreSet()
    {
        var (isValid, error) = ValidConfig().Validate();

        isValid.Should().BeTrue();
        error.Should().BeNull();
    }

    // ── DocuSignConfiguration helper URLs ────────────────────────────────────

    [Fact]
    public void DocuSignConfiguration_GetApiBaseUrl_ReturnsDemoUrl_WhenEnvironmentIsDemo()
    {
        var config = new DocuSignConfiguration { Environment = "demo" };
        config.GetApiBaseUrl().Should().Contain("demo.docusign.net");
    }

    [Fact]
    public void DocuSignConfiguration_GetApiBaseUrl_ReturnsProductionUrl_WhenEnvironmentIsProduction()
    {
        var config = new DocuSignConfiguration { Environment = "production" };
        config.GetApiBaseUrl().Should().NotContain("demo");
    }

    [Fact]
    public void DocuSignConfiguration_GetOAuthBaseUrl_ReturnsDemoOAuth_WhenEnvironmentIsDemo()
    {
        var config = new DocuSignConfiguration { Environment = "demo" };
        config.GetOAuthBaseUrl().Should().Contain("account-d.docusign.com");
    }

    [Fact]
    public void DocuSignConfiguration_GetOAuthBaseUrl_ReturnsProductionOAuth_WhenEnvironmentIsProduction()
    {
        var config = new DocuSignConfiguration { Environment = "production" };
        config.GetOAuthBaseUrl().Should().Be("https://account.docusign.com");
    }

    [Fact]
    public void DocuSignConfiguration_GetRsaPrivateKeyBytes_ReturnsPemBytes_WhenNotFilePath()
    {
        const string pem = "-----BEGIN RSA PRIVATE KEY-----\nFAKEDATA\n-----END RSA PRIVATE KEY-----";
        var config = new DocuSignConfiguration { RsaPrivateKey = pem };

        var bytes = config.GetRsaPrivateKeyBytes();

        bytes.Should().NotBeEmpty();
        System.Text.Encoding.UTF8.GetString(bytes).Should().Be(pem);
    }
}
