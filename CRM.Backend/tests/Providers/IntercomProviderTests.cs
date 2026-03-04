// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
// Unit Tests: IntercomProvider
// HTTP Pattern: IntercomProvider injects HttpClient, so tests use TestHttpMessageHandler
// to intercept HTTP calls without making real network requests. The constructor sets
// BaseAddress, Authorization, Accept, and Intercom-Version headers.
//
// Verified method signatures from source:
//   IntercomProvider(HttpClient, IOptions<IntercomConfiguration>, ILogger<IntercomProvider>)
//   ProviderName → "Intercom"
//   IsAvailableAsync  → GET /me → bool; catches all exceptions
//   CreateContactAsync(request)      → throws ArgumentNullException if null; throws HttpRequestException on error
//   GetContactAsync(externalId)      → throws ArgumentException if empty; null if 404; re-throws HttpRequestException (500)
//   FindContactByEmailAsync(email)   → throws ArgumentException if empty; null if !success; null if no data
//   GetConversationAsync(convId)     → throws ArgumentException if empty; null if 404; re-throws HttpRequestException (500)
//   SendMessageAsync(convId, req)    → throws ArgumentException if empty convId; throws ArgumentNullException if null req
// NOTE: Intercom GetContactAsync/GetConversationAsync use `when (ex.Message.Contains("404"))` catch filter —
//       a 500 response causes a non-matching HttpRequestException that propagates (unlike Chatwoot which catches all).
using System.Net;
using System.Text;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Intercom;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for <see cref="IntercomProvider"/>.
/// </summary>
public class IntercomProviderTests
{
    // ── Mock HTTP Handler ────────────────────────────────────────────────────

    private sealed class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseBody;

        public TestHttpMessageHandler(HttpStatusCode statusCode, string responseBody)
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

    private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Exception _exception;

        public ThrowingHttpMessageHandler(Exception exception)
        {
            _exception = exception;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => throw _exception;
    }

    // ── Factory Helpers ─────────────────────────────────────────────────────

    private static IntercomProvider CreateProvider(HttpStatusCode statusCode, string responseBody)
    {
        var handler = new TestHttpMessageHandler(statusCode, responseBody);
        var httpClient = new HttpClient(handler);

        var config = new IntercomConfiguration
        {
            BaseUrl = "https://api.intercom.io",
            AccessToken = "test-access-token-abc",
            AppId = "test-app-id",
            ApiVersion = "2.11",
            TimeoutSeconds = 30
        };

        var logger = new Mock<ILogger<IntercomProvider>>();
        return new IntercomProvider(httpClient, Options.Create(config), logger.Object);
    }

    private static IntercomProvider CreateThrowingProvider(Exception exception)
    {
        var handler = new ThrowingHttpMessageHandler(exception);
        var httpClient = new HttpClient(handler);

        var config = new IntercomConfiguration
        {
            BaseUrl = "https://api.intercom.io",
            AccessToken = "test-access-token-abc",
            AppId = "test-app-id",
            ApiVersion = "2.11",
            TimeoutSeconds = 30
        };

        var logger = new Mock<ILogger<IntercomProvider>>();
        return new IntercomProvider(httpClient, Options.Create(config), logger.Object);
    }

    // JSON templates matching Intercom private DTOs (snake_case, PropertyNameCaseInsensitive)

    private const string IntercomContactJson =
        """
        {
          "id": "c123",
          "email": "test@example.com",
          "name": "Test User",
          "phone": "+1234567890",
          "created_at": 1700000000,
          "last_seen_at": 0
        }
        """;

    private const string IntercomConversationJson =
        """
        {
          "id": "conv123",
          "state": "open",
          "created_at": 1700000000,
          "updated_at": 1700000000
        }
        """;

    private const string IntercomSearchResultOneContactJson =
        """
        {
          "type": "list",
          "data": [
            {
              "id": "c456",
              "email": "found@example.com",
              "name": "Found User",
              "created_at": 1700000000,
              "last_seen_at": 0
            }
          ],
          "total_count": 1
        }
        """;

    private const string IntercomSearchResultEmptyJson =
        """
        {
          "type": "list",
          "data": [],
          "total_count": 0
        }
        """;

    private const string IntercomConversationPartJson =
        """
        {
          "id": "part001",
          "part_type": "comment",
          "body": "How can we assist you?",
          "created_at": 1700000000
        }
        """;

    // ── 1. ProviderName ─────────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsIntercom()
    {
        var provider = CreateProvider(HttpStatusCode.OK, "{}");

        provider.ProviderName.Should().Be("Intercom");
    }

    // ── 2. Constructor null-guards ───────────────────────────────────────────

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenHttpClientIsNull()
    {
        var config = Options.Create(new IntercomConfiguration
        {
            BaseUrl = "https://api.intercom.io",
            AccessToken = "token",
            ApiVersion = "2.11"
        });
        var logger = new Mock<ILogger<IntercomProvider>>();

        var act = () => new IntercomProvider(null!, config, logger.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenConfigIsNull()
    {
        var logger = new Mock<ILogger<IntercomProvider>>();

        var act = () => new IntercomProvider(new HttpClient(), null!, logger.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        var config = Options.Create(new IntercomConfiguration
        {
            BaseUrl = "https://api.intercom.io",
            AccessToken = "token",
            ApiVersion = "2.11"
        });

        var act = () => new IntercomProvider(new HttpClient(), config, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ── 3. IsAvailableAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_WhenMeEndpointReturns200()
    {
        var provider = CreateProvider(HttpStatusCode.OK, """{"type":"admin","id":"1","name":"Admin"}""");

        var result = await provider.IsAvailableAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenMeEndpointReturns401()
    {
        var provider = CreateProvider(HttpStatusCode.Unauthorized, """{"type":"error.list","errors":[{"code":"unauthorized"}]}""");

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenHttpExceptionThrown()
    {
        var provider = CreateThrowingProvider(new HttpRequestException("Connection refused"));

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    // ── 4. GetContactAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetContactAsync_ThrowsArgumentException_WhenExternalIdIsEmpty()
    {
        // Intercom uses ArgumentException.ThrowIfNullOrWhiteSpace (unlike Chatwoot's soft null check)
        var provider = CreateProvider(HttpStatusCode.OK, "{}");

        await provider.Invoking(p => p.GetContactAsync(string.Empty))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetContactAsync_ThrowsArgumentException_WhenExternalIdIsWhitespace()
    {
        var provider = CreateProvider(HttpStatusCode.OK, "{}");

        await provider.Invoking(p => p.GetContactAsync("   "))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetContactAsync_ReturnsNull_WhenContactNotFound()
    {
        var provider = CreateProvider(HttpStatusCode.NotFound, """{"type":"error.list","errors":[{"code":"not_found"}]}""");

        var result = await provider.GetContactAsync("nonexistent-id");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetContactAsync_ReturnsContact_WhenFound()
    {
        var provider = CreateProvider(HttpStatusCode.OK, IntercomContactJson);

        var result = await provider.GetContactAsync("c123");

        result.Should().NotBeNull();
        result!.ExternalId.Should().Be("c123");
        result.Email.Should().Be("test@example.com");
        result.Name.Should().Be("Test User");
        result.Phone.Should().Be("+1234567890");
    }

    // ── 5. CreateContactAsync ────────────────────────────────────────────────

    [Fact]
    public async Task CreateContactAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        var provider = CreateProvider(HttpStatusCode.OK, "{}");

        await provider.Invoking(p => p.CreateContactAsync(null!))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateContactAsync_ThrowsHttpRequestException_WhenApiReturnsError()
    {
        var provider = CreateProvider(
            HttpStatusCode.UnprocessableEntity,
            """{"type":"error.list","errors":[{"code":"parameter_invalid","field":"email"}]}""");

        await provider.Invoking(p => p.CreateContactAsync(new ChatContactCreateRequest { Name = "New User" }))
            .Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task CreateContactAsync_ReturnsContact_WhenSuccessful()
    {
        var provider = CreateProvider(HttpStatusCode.OK, IntercomContactJson);

        var result = await provider.CreateContactAsync(new ChatContactCreateRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Phone = "+1234567890"
        });

        result.Should().NotBeNull();
        result.ExternalId.Should().Be("c123");
        result.Email.Should().Be("test@example.com");
    }

    // ── 6. FindContactByEmailAsync ────────────────────────────────────────────

    [Fact]
    public async Task FindContactByEmailAsync_ThrowsArgumentException_WhenEmailIsEmpty()
    {
        // Intercom throws (unlike Chatwoot which returns null)
        var provider = CreateProvider(HttpStatusCode.OK, "{}");

        await provider.Invoking(p => p.FindContactByEmailAsync(string.Empty))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task FindContactByEmailAsync_ReturnsNull_WhenApiReturnsError()
    {
        var provider = CreateProvider(HttpStatusCode.InternalServerError, """{"error":"server error"}""");

        var result = await provider.FindContactByEmailAsync("search@example.com");

        result.Should().BeNull();
    }

    [Fact]
    public async Task FindContactByEmailAsync_ReturnsNull_WhenNoContactsFound()
    {
        var provider = CreateProvider(HttpStatusCode.OK, IntercomSearchResultEmptyJson);

        var result = await provider.FindContactByEmailAsync("notfound@example.com");

        result.Should().BeNull();
    }

    [Fact]
    public async Task FindContactByEmailAsync_ReturnsFirstContact_WhenFound()
    {
        var provider = CreateProvider(HttpStatusCode.OK, IntercomSearchResultOneContactJson);

        var result = await provider.FindContactByEmailAsync("found@example.com");

        result.Should().NotBeNull();
        result!.ExternalId.Should().Be("c456");
        result.Email.Should().Be("found@example.com");
        result.Name.Should().Be("Found User");
    }

    // ── 7. GetConversationAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetConversationAsync_ThrowsArgumentException_WhenConversationIdIsEmpty()
    {
        var provider = CreateProvider(HttpStatusCode.OK, "{}");

        await provider.Invoking(p => p.GetConversationAsync(string.Empty))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetConversationAsync_ReturnsNull_WhenConversationNotFound()
    {
        var provider = CreateProvider(HttpStatusCode.NotFound, """{"type":"error.list","errors":[{"code":"not_found"}]}""");

        var result = await provider.GetConversationAsync("nonexistent-conv");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetConversationAsync_ReturnsConversation_WhenFound()
    {
        var provider = CreateProvider(HttpStatusCode.OK, IntercomConversationJson);

        var result = await provider.GetConversationAsync("conv123");

        result.Should().NotBeNull();
        result!.ExternalId.Should().Be("conv123");
        result.Status.Should().Be("open");
    }

    // ── 8. SendMessageAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task SendMessageAsync_ThrowsArgumentException_WhenConversationIdIsEmpty()
    {
        var provider = CreateProvider(HttpStatusCode.OK, "{}");
        var request = new ChatMessageCreateRequest { Content = "Hello" };

        await provider.Invoking(p => p.SendMessageAsync(string.Empty, request))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SendMessageAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        var provider = CreateProvider(HttpStatusCode.OK, "{}");

        await provider.Invoking(p => p.SendMessageAsync("conv-42", null!))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendMessageAsync_ThrowsHttpRequestException_WhenApiReturnsError()
    {
        var provider = CreateProvider(HttpStatusCode.NotFound, """{"type":"error.list","errors":[{"code":"not_found"}]}""");

        await provider.Invoking(p => p.SendMessageAsync("bad-conv-id", new ChatMessageCreateRequest { Content = "Hi" }))
            .Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task SendMessageAsync_ReturnsMessage_WhenSuccessful()
    {
        var provider = CreateProvider(HttpStatusCode.OK, IntercomConversationPartJson);

        var result = await provider.SendMessageAsync("conv123", new ChatMessageCreateRequest
        {
            Content = "How can we assist you?",
            IsPrivate = false
        });

        result.Should().NotBeNull();
        result.ExternalId.Should().Be("part001");
        result.Content.Should().Be("How can we assist you?");
        result.ConversationId.Should().Be("conv123");
    }
}
