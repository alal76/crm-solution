// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net;
using System.Text;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.DocuSeal;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for DocuSealProvider.
/// Verifies HTTP-based DocuSeal e-signature integration, template management,
/// submission lifecycle, and error handling.
///
/// MANDATORY: Written after verifying source signature:
/// Class: DocuSealProvider, Namespace: CRM.Infrastructure.Providers.DocuSeal
/// Constructor: (HttpClient, IOptions&lt;DocuSealConfiguration&gt;, ILogger&lt;DocuSealProvider&gt;)
/// Config.Validate() is called in constructor and throws InvalidOperationException on failure.
/// </summary>
public class DocuSealProviderTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static DocuSealConfiguration ValidConfig() => new()
    {
        Url = "https://docuseal.example.com",
        ApiKey = "test-api-key-12345",
        DefaultExpirationDays = 30,
        TimeoutSeconds = 30
    };

    private static (DocuSealProvider provider, List<(string Method, string Url)> capturedCalls)
        CreateProvider(
            DocuSealConfiguration? config = null,
            HttpStatusCode responseStatus = HttpStatusCode.OK,
            string responseBody = "[]")
    {
        var capturedCalls = new List<(string, string)>();
        var handler = new DocuSealMockHandler(capturedCalls, responseStatus, responseBody);
        var httpClient = new HttpClient(handler);

        var effectiveConfig = config ?? ValidConfig();
        var options = Options.Create(effectiveConfig);
        var logger = new Mock<ILogger<DocuSealProvider>>();

        var provider = new DocuSealProvider(httpClient, options, logger.Object);
        return (provider, capturedCalls);
    }

    // ── Constructor Guards ───────────────────────────────────────────────────

    [Fact]
    public void Constructor_ThrowsInvalidOperationException_WhenUrlIsEmpty()
    {
        var badConfig = new DocuSealConfiguration
        {
            Url = "",
            ApiKey = "some-key",
            DefaultExpirationDays = 30,
            TimeoutSeconds = 30
        };

        var act = () => CreateProvider(badConfig);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*DocuSeal URL is required*");
    }

    [Fact]
    public void Constructor_ThrowsInvalidOperationException_WhenApiKeyIsEmpty()
    {
        var badConfig = new DocuSealConfiguration
        {
            Url = "https://docuseal.example.com",
            ApiKey = "",
            DefaultExpirationDays = 30,
            TimeoutSeconds = 30
        };

        var act = () => CreateProvider(badConfig);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*API key is required*");
    }

    [Fact]
    public void Constructor_ThrowsInvalidOperationException_WhenTimeoutOutOfRange()
    {
        var badConfig = new DocuSealConfiguration
        {
            Url = "https://docuseal.example.com",
            ApiKey = "some-key",
            DefaultExpirationDays = 30,
            TimeoutSeconds = 2   // must be 5-120
        };

        var act = () => CreateProvider(badConfig);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*TimeoutSeconds*");
    }

    [Fact]
    public void Constructor_ThrowsInvalidOperationException_WhenUrlIsNotAbsolute()
    {
        var badConfig = new DocuSealConfiguration
        {
            Url = "not-a-valid-url",
            ApiKey = "some-key",
            DefaultExpirationDays = 30,
            TimeoutSeconds = 30
        };

        var act = () => CreateProvider(badConfig);

        act.Should().Throw<InvalidOperationException>();
    }

    // ── Provider Metadata ────────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsDocuSeal()
    {
        var (provider, _) = CreateProvider();
        provider.ProviderName.Should().Be("DocuSeal");
    }

    // ── IsAvailableAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_WhenApiReturnsSuccess()
    {
        var (provider, capturedCalls) = CreateProvider(responseStatus: HttpStatusCode.OK, responseBody: "[]");

        var result = await provider.IsAvailableAsync();

        result.Should().BeTrue();
        capturedCalls.Should().ContainSingle(c => c.Url.Contains("/templates"));
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenApiReturnsServerError()
    {
        var (provider, _) = CreateProvider(responseStatus: HttpStatusCode.InternalServerError, responseBody: "{}");

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenConnectionThrows()
    {
        var throwHandler = new DocuSealThrowingHandler();
        var httpClient = new HttpClient(throwHandler);
        var options = Options.Create(ValidConfig());
        var logger = new Mock<ILogger<DocuSealProvider>>();
        var provider = new DocuSealProvider(httpClient, options, logger.Object);

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    // ── GetTemplatesAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetTemplatesAsync_ReturnsEmpty_WhenApiReturnsEmptyArray()
    {
        var (provider, _) = CreateProvider(responseBody: "[]");

        var templates = await provider.GetTemplatesAsync();

        templates.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTemplatesAsync_ReturnsTemplates_WhenApiSucceeds()
    {
        const string body = """
            [
              {"id": 1, "name": "NDA Template", "status": "active", "created_at": "2024-01-01T00:00:00Z", "updated_at": "2024-01-01T00:00:00Z"},
              {"id": 2, "name": "Quote Template", "status": "active", "created_at": "2024-02-01T00:00:00Z", "updated_at": "2024-02-01T00:00:00Z"}
            ]
            """;

        var (provider, capturedCalls) = CreateProvider(responseBody: body);

        var templates = (await provider.GetTemplatesAsync()).ToList();

        templates.Should().HaveCount(2);
        templates[0].Name.Should().Be("NDA Template");
        templates[1].Name.Should().Be("Quote Template");
        capturedCalls.Should().ContainSingle(c => c.Method == "GET" && c.Url.Contains("/api/templates"));
    }

    // ── GetTemplateAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetTemplateAsync_ReturnsNull_WhenApiReturns404()
    {
        var (provider, _) = CreateProvider(responseStatus: HttpStatusCode.NotFound, responseBody: "{}");

        var template = await provider.GetTemplateAsync("999");

        template.Should().BeNull();
    }

    [Fact]
    public async Task GetTemplateAsync_ReturnsTemplate_WhenFound()
    {
        const string body = """
            {"id": 42, "name": "NDA", "status": "active", "created_at": "2024-01-01T00:00:00Z", "updated_at": "2024-01-01T00:00:00Z"}
            """;

        var (provider, capturedCalls) = CreateProvider(responseBody: body);

        var template = await provider.GetTemplateAsync("42");

        template.Should().NotBeNull();
        template!.Name.Should().Be("NDA");
        capturedCalls.Should().ContainSingle(c => c.Url.Contains("/templates/42"));
    }

    [Fact]
    public async Task GetTemplateAsync_ThrowsArgumentException_WhenTemplateIdIsWhitespace()
    {
        var (provider, _) = CreateProvider();

        var act = async () => await provider.GetTemplateAsync("   ");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ── GetSignatureRequestAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetSignatureRequestAsync_ReturnsNull_WhenApiReturns404()
    {
        var (provider, _) = CreateProvider(responseStatus: HttpStatusCode.NotFound, responseBody: "{}");

        var result = await provider.GetSignatureRequestAsync("missing-id");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSignatureRequestAsync_ReturnsRequest_WhenFound()
    {
        const string body = """
            {
              "id": 101,
              "status": "pending",
              "submitters": [],
              "created_at": "2024-03-01T10:00:00Z",
              "updated_at": "2024-03-01T10:00:00Z"
            }
            """;

        var (provider, capturedCalls) = CreateProvider(responseBody: body);

        var result = await provider.GetSignatureRequestAsync("101");

        result.Should().NotBeNull();
        result!.Id.Should().Be("101");
        capturedCalls.Should().ContainSingle(c => c.Url.Contains("/submissions/101"));
    }

    // ── CancelSignatureRequestAsync ───────────────────────────────────────────

    [Fact]
    public async Task CancelSignatureRequestAsync_ThrowsArgumentException_WhenIdIsEmpty()
    {
        var (provider, _) = CreateProvider();

        var act = async () => await provider.CancelSignatureRequestAsync("");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CancelSignatureRequestAsync_CallsDeleteEndpoint_WhenIdIsValid()
    {
        var (provider, capturedCalls) = CreateProvider(responseStatus: HttpStatusCode.OK, responseBody: "{}");

        await provider.CancelSignatureRequestAsync("55", "Cancelled by test");

        capturedCalls.Should().ContainSingle(c => c.Method == "DELETE" && c.Url.Contains("/submissions/55"));
    }

    // ── DocuSealConfiguration.Validate ───────────────────────────────────────

    [Fact]
    public void DocuSealConfiguration_GetApiBaseUrl_AppendsApiPath()
    {
        var config = new DocuSealConfiguration { Url = "https://docuseal.example.com/" };
        config.GetApiBaseUrl().Should().Be("https://docuseal.example.com/api");
    }

    [Fact]
    public void DocuSealConfiguration_Validate_ReturnsTrue_WhenAllFieldsAreValid()
    {
        var config = ValidConfig();
        var (isValid, error) = config.Validate();
        isValid.Should().BeTrue();
        error.Should().BeNull();
    }
}

// ── Private handler helpers for DocuSeal tests ────────────────────────────────

internal class DocuSealMockHandler : HttpMessageHandler
{
    private readonly List<(string Method, string Url)> _captured;
    private readonly HttpStatusCode _status;
    private readonly string _body;

    public DocuSealMockHandler(
        List<(string Method, string Url)> captured,
        HttpStatusCode status,
        string body)
    {
        _captured = captured;
        _status = status;
        _body = body;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _captured.Add((request.Method.Method, request.RequestUri?.ToString() ?? string.Empty));
        return Task.FromResult(new HttpResponseMessage(_status)
        {
            Content = new StringContent(_body, Encoding.UTF8, "application/json")
        });
    }
}

internal class DocuSealThrowingHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        throw new HttpRequestException("Connection refused (test)");
    }
}
