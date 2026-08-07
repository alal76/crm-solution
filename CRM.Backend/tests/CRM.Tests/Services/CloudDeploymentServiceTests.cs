// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for CloudDeploymentService (TCOV-002, REV-STUB-006).
///
/// REV-STUB-006 coverage: Test{Aws,Azure,Gcp,DigitalOcean}Connection now make real,
/// network-validating calls (AWS STS GetCallerIdentity, Azure AD token + one ARM call,
/// a GCP service-account JWT-bearer token exchange + one Cloud Resource Manager call, and a
/// DigitalOcean GET /v2/account call). All of these are routed through the injected
/// <see cref="IHttpClientFactory"/> (directly for DigitalOcean/AWS/GCP's resource call, and via
/// SDK-provided HttpClient/HttpMessageHandler injection points for AWS's STS client, Azure's
/// ClientSecretCredential/ArmClient, and Google's ServiceAccountCredential), so every test below
/// mocks <see cref="IHttpClientFactory"/> to return a fake handler. No test makes a real network
/// call.
/// </summary>
public class CloudDeploymentServiceTests : ServiceTestFixtureBase<CloudDeploymentService>
{
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly CloudDeploymentService _service;

    public CloudDeploymentServiceTests()
    {
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _service = new CloudDeploymentService(_mockDbContext.Object, MockLogger.Object, _mockHttpClientFactory.Object);
    }

    [Fact]
    public void Constructor_ShouldCreateService_WithValidDependencies()
    {
        _service.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProvidersAsync_ShouldReturnEmpty_WhenNoProvidersExist()
    {
        _mockDbContext.Setup(c => c.CloudProviders)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<CloudProvider>()).Object);

        var result = await _service.GetProvidersAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProviderByIdAsync_ShouldReturnNull_WhenProviderNotFound()
    {
        _mockDbContext.Setup(c => c.CloudProviders)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<CloudProvider>()).Object);

        var result = await _service.GetProviderByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProvidersAsync_ShouldReturnProvider_WhenProviderExists()
    {
        var providers = new List<CloudProvider>
        {
            new() { Id = 1, Name = "Test AWS", IsDeleted = false, ProviderType = CRM.Core.Entities.CloudProviderType.AWS }
        };
        _mockDbContext.Setup(c => c.CloudProviders)
            .Returns(MockDbSetFactory.CreateMockDbSet(providers).Object);

        var result = await _service.GetProvidersAsync();

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteProviderAsync_ShouldReturnFalse_WhenProviderNotFound()
    {
        _mockDbContext.Setup(c => c.CloudProviders)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<CloudProvider>()).Object);

        var result = await _service.DeleteProviderAsync(999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetDeploymentsAsync_ShouldReturnEmpty_WhenNoDeploymentsExist()
    {
        _mockDbContext.Setup(c => c.CloudDeployments)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<CloudDeployment>()).Object);

        var result = await _service.GetDeploymentsAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    // ── Test helpers ─────────────────────────────────────────────────────────

    private static readonly RSA GcpTestKey = RSA.Create(2048);

    private void SetupHttpClientFactory(HttpMessageHandler handler)
    {
        _mockHttpClientFactory
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            // disposeHandler:false - several providers request more than one HttpClient per test
            // (e.g. Azure's credential transport + ARM transport) that must share one handler.
            .Returns(() => new HttpClient(handler, disposeHandler: false));
    }

    private async Task<ProviderConnectionResult> TestConnection(CloudProvider provider)
    {
        _mockDbContext.Setup(c => c.CloudProviders)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<CloudProvider> { provider }).Object);

        return await _service.TestProviderConnectionAsync(new TestProviderConnectionRequest { ProviderId = provider.Id });
    }

    private static CloudProvider AwsProvider(int id = 1) => new()
    {
        Id = id,
        Name = "Test AWS",
        ProviderType = CloudProviderType.AWS,
        AccessKeyId = "AKIAFAKEACCESSKEY",
        SecretAccessKey = "fakeSecretAccessKey",
        Region = "us-east-1"
    };

    private static CloudProvider AzureProvider(int id = 1) => new()
    {
        Id = id,
        Name = "Test Azure",
        ProviderType = CloudProviderType.Azure,
        TenantId = "11111111-1111-1111-1111-111111111111",
        SubscriptionId = "33333333-3333-3333-3333-333333333333",
        AccessKeyId = "22222222-2222-2222-2222-222222222222", // Client ID
        SecretAccessKey = "fake-client-secret",
        Region = "eastus"
    };

    private static CloudProvider GcpProvider(int id = 1)
    {
        var pem = GcpTestKey.ExportPkcs8PrivateKeyPem();
        var serviceAccountJson = JsonSerializer.Serialize(new
        {
            type = "service_account",
            client_email = "test@test-project.iam.gserviceaccount.com",
            private_key = pem,
            token_uri = "https://oauth2.googleapis.com/token"
        });

        return new CloudProvider
        {
            Id = id,
            Name = "Test GCP",
            ProviderType = CloudProviderType.GoogleCloud,
            ProjectId = "test-project",
            SecretAccessKey = serviceAccountJson,
            Region = "us-central1"
        };
    }

    private static CloudProvider DigitalOceanProvider(int id = 1) => new()
    {
        Id = id,
        Name = "Test DigitalOcean",
        ProviderType = CloudProviderType.DigitalOcean,
        AccessKeyId = "do_fake_token_abc123",
        Region = "nyc1"
    };

    // ── AWS ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TestAwsConnection_ReturnsSuccess_WhenStsAcceptsCredentials()
    {
        const string xml = """
        <GetCallerIdentityResponse xmlns="https://sts.amazonaws.com/doc/2011-06-15/">
          <GetCallerIdentityResult>
            <Arn>arn:aws:iam::123456789012:user/test</Arn>
            <UserId>AIDACKCEVSQ6C2EXAMPLE</UserId>
            <Account>123456789012</Account>
          </GetCallerIdentityResult>
          <ResponseMetadata><RequestId>abcd-1234</RequestId></ResponseMetadata>
        </GetCallerIdentityResponse>
        """;
        SetupHttpClientFactory(new SingleResponseHandler(HttpStatusCode.OK, xml, "text/xml"));

        var result = await TestConnection(AwsProvider());

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("123456789012");
    }

    [Fact]
    public async Task TestAwsConnection_ReturnsFailure_WhenStsRejectsCredentials()
    {
        const string xml = """
        <ErrorResponse xmlns="https://sts.amazonaws.com/doc/2011-06-15/">
          <Error>
            <Type>Sender</Type>
            <Code>InvalidClientTokenId</Code>
            <Message>The security token included in the request is invalid.</Message>
          </Error>
          <RequestId>abcd-5678</RequestId>
        </ErrorResponse>
        """;
        SetupHttpClientFactory(new SingleResponseHandler(HttpStatusCode.Forbidden, xml, "text/xml"));

        var result = await TestConnection(AwsProvider());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("AWS connection failed");
    }

    [Fact]
    public async Task TestAwsConnection_ReturnsFailure_WhenCredentialsMissing()
    {
        var provider = AwsProvider();
        provider.SecretAccessKey = null;

        var result = await TestConnection(provider);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("required");
    }

    [Fact]
    public async Task TestAwsConnection_ReturnsFailure_OnNetworkError()
    {
        SetupHttpClientFactory(new ThrowingHandler());

        var result = await TestConnection(AwsProvider());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("AWS connection failed");
    }

    // ── Azure ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TestAzureConnection_ReturnsSuccess_WhenArmAcceptsToken()
    {
        var handler = new RoutingHandler();
        handler.Register("login.microsoftonline.com", HttpStatusCode.OK, JsonSerializer.Serialize(new
        {
            token_type = "Bearer",
            expires_in = 3600,
            access_token = "fake-arm-token"
        }));
        handler.Register("management.azure.com", HttpStatusCode.OK, JsonSerializer.Serialize(new
        {
            id = "/subscriptions/33333333-3333-3333-3333-333333333333",
            subscriptionId = "33333333-3333-3333-3333-333333333333",
            displayName = "Test Subscription",
            state = "Enabled"
        }));
        SetupHttpClientFactory(handler);

        var result = await TestConnection(AzureProvider());

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Test Subscription");
    }

    [Fact]
    public async Task TestAzureConnection_ReturnsFailure_WhenTokenRequestRejected()
    {
        var handler = new RoutingHandler();
        handler.Register("login.microsoftonline.com", HttpStatusCode.Unauthorized, JsonSerializer.Serialize(new
        {
            error = "invalid_client",
            error_description = "AADSTS7000215: Invalid client secret provided."
        }));
        SetupHttpClientFactory(handler);

        var result = await TestConnection(AzureProvider());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Azure connection failed");
    }

    [Fact]
    public async Task TestAzureConnection_ReturnsFailure_WhenCredentialsMissing()
    {
        var provider = AzureProvider();
        provider.TenantId = null;

        var result = await TestConnection(provider);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("required");
    }

    // ── GCP ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TestGcpConnection_ReturnsSuccess_WhenTokenAndProjectCallSucceed()
    {
        var handler = new RoutingHandler();
        handler.Register("oauth2.googleapis.com", HttpStatusCode.OK, JsonSerializer.Serialize(new
        {
            access_token = "fake-gcp-token",
            expires_in = 3600,
            token_type = "Bearer"
        }));
        handler.Register("cloudresourcemanager.googleapis.com", HttpStatusCode.OK, JsonSerializer.Serialize(new
        {
            projectId = "test-project",
            name = "Test Project"
        }));
        SetupHttpClientFactory(handler);

        var result = await TestConnection(GcpProvider());

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("test-project");
    }

    [Fact]
    public async Task TestGcpConnection_ReturnsFailure_WhenTokenExchangeRejected()
    {
        var handler = new RoutingHandler();
        handler.Register("oauth2.googleapis.com", HttpStatusCode.BadRequest, JsonSerializer.Serialize(new
        {
            error = "invalid_grant",
            error_description = "Invalid JWT Signature."
        }));
        SetupHttpClientFactory(handler);

        var result = await TestConnection(GcpProvider());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("GCP connection failed");
    }

    [Fact]
    public async Task TestGcpConnection_ReturnsFailure_WhenServiceAccountSecretMissing()
    {
        var provider = GcpProvider();
        provider.SecretAccessKey = null;

        var result = await TestConnection(provider);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("required");
    }

    // ── DigitalOcean ─────────────────────────────────────────────────────────

    [Fact]
    public async Task TestDigitalOceanConnection_ReturnsSuccess_WhenAccountCallSucceeds()
    {
        var json = JsonSerializer.Serialize(new { account = new { email = "owner@example.com" } });
        SetupHttpClientFactory(new SingleResponseHandler(HttpStatusCode.OK, json, "application/json"));

        var result = await TestConnection(DigitalOceanProvider());

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("owner@example.com");
    }

    [Fact]
    public async Task TestDigitalOceanConnection_ReturnsFailure_WhenTokenRejected()
    {
        var json = JsonSerializer.Serialize(new { id = "unauthorized", message = "Unable to authenticate you." });
        SetupHttpClientFactory(new SingleResponseHandler(HttpStatusCode.Unauthorized, json, "application/json"));

        var result = await TestConnection(DigitalOceanProvider());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("DigitalOcean connection failed");
    }

    [Fact]
    public async Task TestDigitalOceanConnection_ReturnsFailure_WhenTokenMissing()
    {
        var provider = DigitalOceanProvider();
        provider.AccessKeyId = null;

        var result = await TestConnection(provider);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("required");
    }

    [Fact]
    public async Task TestDigitalOceanConnection_ReturnsFailure_OnNetworkError()
    {
        SetupHttpClientFactory(new ThrowingHandler());

        var result = await TestConnection(DigitalOceanProvider());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("DigitalOcean connection failed");
    }
}

// ── Fake HttpMessageHandlers for cloud-provider connection tests ────────────

/// <summary>
/// Returns the same canned response for every request. Used for single-endpoint providers
/// (AWS STS, DigitalOcean).
/// </summary>
internal sealed class SingleResponseHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _status;
    private readonly string _body;
    private readonly string _mediaType;

    public SingleResponseHandler(HttpStatusCode status, string body, string mediaType)
    {
        _status = status;
        _body = body;
        _mediaType = mediaType;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage(_status)
        {
            Content = new StringContent(_body, Encoding.UTF8, _mediaType)
        });
    }
}

/// <summary>
/// Routes canned responses by request host. Used for providers whose connection test hits two
/// different hosts in sequence (an auth/token endpoint, then a resource endpoint): Azure
/// (login.microsoftonline.com + management.azure.com) and GCP (oauth2.googleapis.com +
/// cloudresourcemanager.googleapis.com).
/// </summary>
internal sealed class RoutingHandler : HttpMessageHandler
{
    private readonly List<(string HostContains, HttpStatusCode Status, string Body)> _routes = new();

    public void Register(string hostContains, HttpStatusCode status, string body)
        => _routes.Add((hostContains, status, body));

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var host = request.RequestUri?.Host ?? string.Empty;
        foreach (var (hostContains, status, body) in _routes)
        {
            if (host.Contains(hostContains, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new HttpResponseMessage(status)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                });
            }
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}

/// <summary>
/// Simulates a network failure (DNS/connection error) for every request.
/// </summary>
internal sealed class ThrowingHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => throw new HttpRequestException("Simulated network failure: no such host is known");
}
