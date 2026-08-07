// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net;
using System.Text;
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Ports;
using CRM.Infrastructure.Services.Integrations;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for LinkedInSalesNavService's messaging path (REV-STUB-004): SendInMailAsync
/// and TestConnectionAsync. The pre-existing search/enrich stub methods (INT-003) are
/// untouched by REV-STUB-004 and are not re-tested here.
///
/// MANDATORY: Written after verifying source for:
///   Class: LinkedInSalesNavService, Namespace: CRM.Infrastructure.Services.Integrations
///   Constructor: (IConfiguration, IProviderConfigurationService, IHttpClientFactory, ILogger)
/// </summary>
public class LinkedInSalesNavServiceTests
{
    private static IConfiguration EmptyConfiguration() =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();

    private static Mock<IProviderConfigurationService> ConfigServiceMock(string? accessToken)
    {
        var mock = new Mock<IProviderConfigurationService>();
        mock.Setup(m => m.GetConfigurationAsync("crm.linkedin.salesnavigator", It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessToken == null
                ? null
                : new ProviderConfigurationDto
                {
                    Id = 1,
                    ConfigurationKey = "crm.linkedin.salesnavigator",
                    ConfigurationType = "crm",
                    ConfigurationData = JsonSerializer.Serialize(new { AccessToken = accessToken }),
                    IsEncrypted = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
        return mock;
    }

    private static LinkedInSalesNavService BuildService(
        Mock<IProviderConfigurationService> configService,
        HttpMessageHandler? handler = null,
        IConfiguration? configuration = null)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(handler ?? new TestHttpMessageHandler(HttpStatusCode.OK, "{}")));

        return new LinkedInSalesNavService(
            configuration ?? EmptyConfiguration(),
            configService.Object,
            factory.Object,
            Mock.Of<ILogger<LinkedInSalesNavService>>());
    }

    // ── SendInMailAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task SendInMailAsync_ReturnsSuccess_WhenLinkedInAccepts()
    {
        var configService = ConfigServiceMock("valid-access-token");
        var handler = new TestHttpMessageHandler(HttpStatusCode.Created, "{}");
        handler.ExtraResponseHeaders.Add("x-restli-id", "msg-123");
        var svc = BuildService(configService, handler);

        var result = await svc.SendInMailAsync("urn:li:person:abc123", "Hello", "Let's connect");

        result.Success.Should().BeTrue();
        result.ExternalMessageId.Should().Be("msg-123");
        handler.LastRequest!.RequestUri!.ToString().Should().Contain("/rest/messages");
        handler.LastRequest.Headers.Authorization!.Parameter.Should().Be("valid-access-token");
    }

    [Fact]
    public async Task SendInMailAsync_ReturnsFailure_WhenLinkedInReturns403_PartnerProgramNotApproved()
    {
        var configService = ConfigServiceMock("valid-access-token");
        var handler = new TestHttpMessageHandler(HttpStatusCode.Forbidden, """{"message":"ACCESS_DENIED"}""");
        var svc = BuildService(configService, handler);

        var result = await svc.SendInMailAsync("urn:li:person:abc123", "Hello", "Let's connect");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("403");
    }

    [Fact]
    public async Task SendInMailAsync_ReturnsFailure_WhenAccessTokenNotConfigured()
    {
        var configService = ConfigServiceMock(accessToken: null);
        var svc = BuildService(configService);

        var result = await svc.SendInMailAsync("urn:li:person:abc123", "Hello", "Let's connect");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not configured");
    }

    // ── TestConnectionAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task TestConnectionAsync_ReturnsTrue_WhenLinkedInReturns200()
    {
        var configService = ConfigServiceMock("valid-access-token");
        var handler = new TestHttpMessageHandler(HttpStatusCode.OK, """{"sub":"abc123"}""");
        var svc = BuildService(configService, handler);

        var connected = await svc.TestConnectionAsync();

        connected.Should().BeTrue();
    }

    [Fact]
    public async Task TestConnectionAsync_ReturnsFalse_WhenTokenNotConfigured()
    {
        var configService = ConfigServiceMock(accessToken: null);
        var svc = BuildService(configService);

        var connected = await svc.TestConnectionAsync();

        connected.Should().BeFalse();
    }

    // ── Test HTTP handler ───────────────────────────────────────────────────

    private sealed class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _body;

        public HttpRequestMessage? LastRequest { get; private set; }
        public Dictionary<string, string> ExtraResponseHeaders { get; } = new();

        public TestHttpMessageHandler(HttpStatusCode statusCode, string body)
        {
            _statusCode = statusCode;
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json")
            };

            foreach (var (key, value) in ExtraResponseHeaders)
            {
                response.Headers.TryAddWithoutValidation(key, value);
            }

            return Task.FromResult(response);
        }
    }
}
