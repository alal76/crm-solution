// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Unit Tests: FacebookMessengerProvider
// Verified from source before writing:
//   Class: FacebookMessengerProvider, Namespace: CRM.Infrastructure.Providers.Messaging
//   Constructor: (HttpClient, IOptions<FacebookMessengerOptions>, ILogger<FacebookMessengerProvider>)
//   Interface: CRM.Core.Interfaces.IFacebookMessengerProvider
//   Options: CRM.Core.Configuration.FacebookMessengerOptions (PageAccessToken, VerifyToken, AppSecret, Enabled)
//   IsAvailable: Enabled && !empty(PageAccessToken)
//   SendMessageAsync: returns false when !IsAvailable; calls Graph API POST when available
//   HTTP: POST https://graph.facebook.com/v18.0/me/messages?access_token={PageAccessToken}
//   Request body: {"recipient":{"id":"{psid}"},"message":{"text":"{message}"}}
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
/// Unit tests for <see cref="FacebookMessengerProvider"/>.
/// </summary>
public class FacebookMessengerProviderTests
{
    // ── Test HTTP handler ────────────────────────────────────────────────────

    private sealed class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseBody;

        public TestHttpMessageHandler(
            HttpStatusCode statusCode = HttpStatusCode.OK,
            string responseBody = "{\"recipient_id\":\"1234\",\"message_id\":\"mid.001\"}")
        {
            _statusCode = statusCode;
            _responseBody = responseBody;
        }

        public HttpRequestMessage? LastRequest { get; private set; }

        /// <summary>Request body captured before content disposal.</summary>
        public string LastRequestBody { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            if (request.Content != null)
            {
                LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            return new HttpResponseMessage
            {
                StatusCode = _statusCode,
                Content = new StringContent(_responseBody, Encoding.UTF8, "application/json")
            };
        }
    }

    // ── Factory helpers ─────────────────────────────────────────────────────

    private static FacebookMessengerOptions DefaultOptions() => new()
    {
        PageAccessToken = string.Empty,
        VerifyToken = string.Empty,
        AppSecret = string.Empty,
        Enabled = false
    };

    private static FacebookMessengerOptions ConfiguredOptions() => new()
    {
        PageAccessToken = "EAAtest_page_access_token",
        VerifyToken = "crm-fb-verify-dev",
        AppSecret = "test_app_secret_1234567890abcdef",
        Enabled = true
    };

    private static (FacebookMessengerProvider provider, TestHttpMessageHandler handler) CreateProvider(
        FacebookMessengerOptions options,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new TestHttpMessageHandler(statusCode);
        var httpClient = new HttpClient(handler);
        var optionsMock = Options.Create(options);
        var logger = new Mock<ILogger<FacebookMessengerProvider>>();
        return (new FacebookMessengerProvider(httpClient, optionsMock, logger.Object), handler);
    }

    // ── IsAvailable ─────────────────────────────────────────────────────────

    [Fact]
    public void IsAvailable_ReturnsFalse_WhenDisabled()
    {
        var options = new FacebookMessengerOptions
        {
            PageAccessToken = "EAAsome_token",
            Enabled = false
        };
        var (provider, _) = CreateProvider(options);
        provider.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void IsAvailable_ReturnsFalse_WhenPageAccessTokenEmpty()
    {
        var options = new FacebookMessengerOptions
        {
            PageAccessToken = string.Empty,
            Enabled = true
        };
        var (provider, _) = CreateProvider(options);
        provider.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void IsAvailable_ReturnsFalse_WhenNotConfigured()
    {
        var (provider, _) = CreateProvider(DefaultOptions());
        provider.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void IsAvailable_ReturnsTrue_WhenConfigured()
    {
        var (provider, _) = CreateProvider(ConfiguredOptions());
        provider.IsAvailable.Should().BeTrue();
    }

    // ── SendMessageAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task SendMessageAsync_ReturnsFalse_WhenNotConfigured()
    {
        var (provider, _) = CreateProvider(DefaultOptions());
        var result = await provider.SendMessageAsync("1234567890", "Hello");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendMessageAsync_ReturnsFalse_WhenDisabledWithToken()
    {
        var options = new FacebookMessengerOptions
        {
            PageAccessToken = "EAAsome_token",
            Enabled = false
        };
        var (provider, _) = CreateProvider(options);
        var result = await provider.SendMessageAsync("1234567890", "Hello");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendMessageAsync_CallsGraphApi_WhenConfigured()
    {
        // Arrange — Graph API returns 200 OK on success
        var (provider, handler) = CreateProvider(ConfiguredOptions(), HttpStatusCode.OK);

        // Act
        var result = await provider.SendMessageAsync("1234567890", "Test message");

        // Assert
        result.Should().BeTrue();
        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        handler.LastRequest.RequestUri!.AbsolutePath.Should().Contain("me/messages");
    }

    [Fact]
    public async Task SendMessageAsync_ReturnsFalse_WhenGraphApiReturnsError()
    {
        var (provider, _) = CreateProvider(ConfiguredOptions(), HttpStatusCode.BadRequest);
        var result = await provider.SendMessageAsync("1234567890", "Test message");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendMessageAsync_IncludesAccessTokenInUrl()
    {
        var options = ConfiguredOptions();
        var (provider, handler) = CreateProvider(options, HttpStatusCode.OK);

        await provider.SendMessageAsync("1234567890", "Hello");

        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.RequestUri!.Query.Should().Contain("access_token=");
        handler.LastRequest.RequestUri.Query.Should().Contain(options.PageAccessToken);
    }

    [Fact]
    public async Task SendMessageAsync_SendsCorrectJsonPayload()
    {
        var (provider, handler) = CreateProvider(ConfiguredOptions(), HttpStatusCode.OK);
        const string psid = "9876543210";
        const string text = "Hello from CRM";

        await provider.SendMessageAsync(psid, text);

        handler.LastRequest.Should().NotBeNull();
        handler.LastRequestBody.Should().Contain(psid);
        handler.LastRequestBody.Should().Contain(text);
    }
}
