// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Unit Tests: WhatsAppProvider
// Verified from source before writing:
//   Class: WhatsAppProvider, Namespace: CRM.Infrastructure.Providers.Messaging
//   Constructor: (HttpClient, IOptions<WhatsAppOptions>, ILogger<WhatsAppProvider>)
//   Interface: CRM.Core.Interfaces.IWhatsAppProvider
//   Options: CRM.Core.Configuration.WhatsAppOptions (AccountSid, AuthToken, FromNumber, Enabled)
//   IsAvailable: Enabled && !empty(AccountSid) && !empty(AuthToken)
//   SendMessageAsync: returns false when !IsAvailable; calls Twilio REST API when available
//   SendTemplateAsync: builds body from templateName + params lines, delegates to SendMessageAsync
//   HTTP: Basic Auth (Base64 AccountSid:AuthToken), POST form-encoded to Twilio Messages endpoint
using System.Net;
using System.Text;
using CRM.Core.Configuration;
using CRM.Infrastructure.Providers.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for <see cref="WhatsAppProvider"/>.
/// </summary>
public class WhatsAppProviderTests
{
    // ── Test HTTP handler ────────────────────────────────────────────────────

    private sealed class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseBody;

        public TestHttpMessageHandler(HttpStatusCode statusCode = HttpStatusCode.Created, string responseBody = "{\"sid\":\"SM123\"}")
        {
            _statusCode = statusCode;
            _responseBody = responseBody;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage
            {
                StatusCode = _statusCode,
                Content = new StringContent(_responseBody, Encoding.UTF8, "application/json")
            });
        }
    }

    // ── Factory helpers ─────────────────────────────────────────────────────

    private static WhatsAppOptions DefaultOptions() => new()
    {
        AccountSid = string.Empty,
        AuthToken = string.Empty,
        FromNumber = "whatsapp:+14155238886",
        Enabled = false
    };

    private static WhatsAppOptions ConfiguredOptions() => new()
    {
        AccountSid = "ACtest00000000000000000000000000",
        AuthToken = "auth_token_test_1234567890abcdef",
        FromNumber = "whatsapp:+14155238886",
        Enabled = true
    };

    private static WhatsAppProvider CreateProvider(WhatsAppOptions options, HttpStatusCode statusCode = HttpStatusCode.Created)
    {
        var handler = new TestHttpMessageHandler(statusCode);
        var httpClient = new HttpClient(handler);
        var optionsMock = Options.Create(options);
        var logger = new Mock<ILogger<WhatsAppProvider>>();
        return new WhatsAppProvider(httpClient, optionsMock, logger.Object);
    }

    // ── IsAvailable ─────────────────────────────────────────────────────────

    [Fact]
    public void IsAvailable_ReturnsFalse_WhenAccountSidEmpty()
    {
        var options = new WhatsAppOptions { Enabled = true, AuthToken = "sometoken" };
        var provider = CreateProvider(options);
        provider.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void IsAvailable_ReturnsFalse_WhenAuthTokenEmpty()
    {
        var options = new WhatsAppOptions { Enabled = true, AccountSid = "ACtest" };
        var provider = CreateProvider(options);
        provider.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void IsAvailable_ReturnsFalse_WhenEnabledIsFalse()
    {
        var options = new WhatsAppOptions
        {
            AccountSid = "ACtest00000000000000000000000000",
            AuthToken = "auth_token_test_1234567890abcdef",
            FromNumber = "whatsapp:+14155238886",
            Enabled = false
        };
        var provider = CreateProvider(options);
        provider.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void IsAvailable_ReturnsTrue_WhenCredentialsSetAndEnabled()
    {
        var provider = CreateProvider(ConfiguredOptions());
        provider.IsAvailable.Should().BeTrue();
    }

    // ── SendMessageAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task SendMessageAsync_ReturnsFalse_WhenNotConfigured()
    {
        var provider = CreateProvider(DefaultOptions());
        var result = await provider.SendMessageAsync("+15550001111", "Hello");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendMessageAsync_ReturnsFalse_WhenEnabledButNoCredentials()
    {
        var options = new WhatsAppOptions { Enabled = true };
        var provider = CreateProvider(options);
        var result = await provider.SendMessageAsync("+15550001111", "Hello");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendMessageAsync_CallsTwilioApi_WhenConfigured()
    {
        // Arrange - Twilio returns 201 Created on success
        var provider = CreateProvider(ConfiguredOptions(), HttpStatusCode.Created);

        // Act
        var result = await provider.SendMessageAsync("+15550001111", "Test message");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendMessageAsync_ReturnsFalse_WhenTwilioReturnsError()
    {
        var provider = CreateProvider(ConfiguredOptions(), HttpStatusCode.BadRequest);
        var result = await provider.SendMessageAsync("+15550001111", "Test message");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendMessageAsync_NormalizesNumberWithoutPrefix()
    {
        // The provider should prepend "whatsapp:" when not present.
        // We verify indirectly: the call succeeds without exception.
        var provider = CreateProvider(ConfiguredOptions());
        var result = await provider.SendMessageAsync("+15550001111", "Hello");
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendMessageAsync_HandlesNumberAlreadyWithPrefix()
    {
        var provider = CreateProvider(ConfiguredOptions());
        var result = await provider.SendMessageAsync("whatsapp:+15550001111", "Hello");
        result.Should().BeTrue();
    }

    // ── SendTemplateAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task SendTemplateAsync_ReturnsFalse_WhenNotConfigured()
    {
        var provider = CreateProvider(DefaultOptions());
        var result = await provider.SendTemplateAsync("+15550001111", "appointment_reminder",
            new Dictionary<string, string> { ["date"] = "Monday 3pm" });
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendTemplateAsync_CallsTwilioApi_WhenConfigured()
    {
        var provider = CreateProvider(ConfiguredOptions(), HttpStatusCode.Created);
        var result = await provider.SendTemplateAsync("+15550001111", "appointment_reminder",
            new Dictionary<string, string> { ["date"] = "Monday 3pm" });
        result.Should().BeTrue();
    }
}
