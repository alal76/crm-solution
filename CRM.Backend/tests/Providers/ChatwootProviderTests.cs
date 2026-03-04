// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
// Unit Tests: ChatwootProvider
// HTTP Pattern: ChatwootProvider injects HttpClient, so tests use TestHttpMessageHandler
// to intercept HTTP calls without making real network requests. The constructor sets
// BaseAddress and headers on the injected HttpClient.
//
// Verified method signatures from source:
//   ChatwootProvider(HttpClient, IOptions<ChatwootConfiguration>, ILogger<ChatwootProvider>)
//   ProviderName → "Chatwoot"
//   IsAvailableAsync  → GET /api/v1/accounts/{id}/agents → bool; catches all exceptions
//   CreateContactAsync(request)     → throws ArgumentNullException if null; throws HttpRequestException on HTTP error
//   GetContactAsync(externalId)     → null if empty; null on 404; null on HttpRequestException (caught)
//   FindContactByEmailAsync(email)  → null if empty; null if !success; null on Exception (caught)
//   GetConversationAsync(convId)    → null if empty; null on 404; null on HttpRequestException (caught)
//   SendMessageAsync(convId, req)   → throws ArgumentException if empty convId; throws ArgumentNullException if null req
using System.Net;
using System.Text;
using System.Text.Json;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Chatwoot;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for <see cref="ChatwootProvider"/>.
/// </summary>
public class ChatwootProviderTests
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

    private static ChatwootProvider CreateProvider(HttpStatusCode statusCode, string responseBody)
    {
        var handler = new TestHttpMessageHandler(statusCode, responseBody);
        var httpClient = new HttpClient(handler);

        var config = new ChatwootConfiguration
        {
            BaseUrl = "https://chatwoot.test.example.com",
            ApiKey = "test-api-key-abc123",
            AccountId = 1,
            TimeoutSeconds = 30
        };

        var logger = new Mock<ILogger<ChatwootProvider>>();
        return new ChatwootProvider(httpClient, Options.Create(config), logger.Object);
    }

    private static ChatwootProvider CreateThrowingProvider(Exception exception)
    {
        var handler = new ThrowingHttpMessageHandler(exception);
        var httpClient = new HttpClient(handler);

        var config = new ChatwootConfiguration
        {
            BaseUrl = "https://chatwoot.test.example.com",
            ApiKey = "test-api-key-abc123",
            AccountId = 1,
            TimeoutSeconds = 30
        };

        var logger = new Mock<ILogger<ChatwootProvider>>();
        return new ChatwootProvider(httpClient, Options.Create(config), logger.Object);
    }

    // JSON templates matching the Chatwoot private DTOs (snake_case, PropertyNameCaseInsensitive)

    private const string ContactPayloadJson =
        """
        {
          "id": 42,
          "name": "Jane Doe",
          "email": "jane@example.com",
          "phone_number": "+12345678",
          "created_at": "2024-01-01T00:00:00Z"
        }
        """;

    private const string CreateContactResponseJson =
        """
        {
          "payload": {
            "contact": {
              "id": 42,
              "name": "Jane Doe",
              "email": "jane@example.com",
              "phone_number": "+12345678",
              "created_at": "2024-01-01T00:00:00Z"
            }
          }
        }
        """;

    private const string ConversationPayloadJson =
        """
        {
          "id": 99,
          "status": "open",
          "channel": "Channel::WebWidget",
          "messages_count": 3,
          "unread_count": 1,
          "created_at": "2024-01-01T00:00:00Z"
        }
        """;

    private const string MessagePayloadJson =
        """
        {
          "id": 77,
          "content": "Hello, how can I help?",
          "content_type": "text",
          "message_type": 1,
          "private": false,
          "created_at": "2024-01-01T00:00:00Z"
        }
        """;

    // ── 1. ProviderName ─────────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsChatwoot()
    {
        var provider = CreateProvider(HttpStatusCode.OK, "{}");

        provider.ProviderName.Should().Be("Chatwoot");
    }

    // ── 2. Constructor null-guards ───────────────────────────────────────────

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenHttpClientIsNull()
    {
        var config = Options.Create(new ChatwootConfiguration
        {
            BaseUrl = "https://chatwoot.test.example.com",
            ApiKey = "key",
            AccountId = 1
        });
        var logger = new Mock<ILogger<ChatwootProvider>>();

        var act = () => new ChatwootProvider(null!, config, logger.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenConfigIsNull()
    {
        var logger = new Mock<ILogger<ChatwootProvider>>();

        var act = () => new ChatwootProvider(new HttpClient(), null!, logger.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        var config = Options.Create(new ChatwootConfiguration
        {
            BaseUrl = "https://chatwoot.test.example.com",
            ApiKey = "key",
            AccountId = 1
        });

        var act = () => new ChatwootProvider(new HttpClient(), config, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ── 3. IsAvailableAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_WhenApiReturns200()
    {
        var provider = CreateProvider(HttpStatusCode.OK, """[{"id":1,"name":"Agent"}]""");

        var result = await provider.IsAvailableAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenApiReturns401()
    {
        var provider = CreateProvider(HttpStatusCode.Unauthorized, """{"error":"unauthorized"}""");

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
    public async Task GetContactAsync_ReturnsNull_WhenExternalIdIsEmpty()
    {
        // Short-circuit check: does not make HTTP call
        var provider = CreateProvider(HttpStatusCode.OK, "{}");

        var result = await provider.GetContactAsync(string.Empty);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetContactAsync_ReturnsNull_WhenContactNotFound()
    {
        var provider = CreateProvider(HttpStatusCode.NotFound, """{"error":"not found"}""");

        var result = await provider.GetContactAsync("999");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetContactAsync_ReturnsNull_WhenServerReturns500()
    {
        // 500 causes EnsureSuccessAsync to throw HttpRequestException,
        // caught by the outer HttpRequestException catch → returns null
        var provider = CreateProvider(HttpStatusCode.InternalServerError, """{"error":"server error"}""");

        var result = await provider.GetContactAsync("123");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetContactAsync_ReturnsContact_WhenFound()
    {
        var provider = CreateProvider(HttpStatusCode.OK, ContactPayloadJson);

        var result = await provider.GetContactAsync("42");

        result.Should().NotBeNull();
        result!.ExternalId.Should().Be("42");
        result.Email.Should().Be("jane@example.com");
        result.Name.Should().Be("Jane Doe");
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
        var provider = CreateProvider(HttpStatusCode.BadRequest, """{"error":"invalid data"}""");

        await provider.Invoking(p => p.CreateContactAsync(new ChatContactCreateRequest { Name = "Test" }))
            .Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task CreateContactAsync_ReturnsCreatedContact_WhenSuccessful()
    {
        var provider = CreateProvider(HttpStatusCode.Created, CreateContactResponseJson);

        var result = await provider.CreateContactAsync(new ChatContactCreateRequest
        {
            Name = "Jane Doe",
            Email = "jane@example.com",
            Phone = "+12345678"
        });

        result.Should().NotBeNull();
        result.ExternalId.Should().Be("42");
        result.Email.Should().Be("jane@example.com");
        result.Name.Should().Be("Jane Doe");
    }

    // ── 6. FindContactByEmailAsync ────────────────────────────────────────────

    [Fact]
    public async Task FindContactByEmailAsync_ReturnsNull_WhenEmailIsEmpty()
    {
        var provider = CreateProvider(HttpStatusCode.OK, "{}");

        var result = await provider.FindContactByEmailAsync(string.Empty);

        result.Should().BeNull();
    }

    [Fact]
    public async Task FindContactByEmailAsync_ReturnsNull_WhenApiReturnsError()
    {
        var provider = CreateProvider(HttpStatusCode.InternalServerError, """{"error":"server error"}""");

        var result = await provider.FindContactByEmailAsync("test@example.com");

        result.Should().BeNull();
    }

    // ── 7. GetConversationAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetConversationAsync_ReturnsNull_WhenConversationIdIsEmpty()
    {
        var provider = CreateProvider(HttpStatusCode.OK, "{}");

        var result = await provider.GetConversationAsync(string.Empty);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetConversationAsync_ReturnsNull_WhenConversationNotFound()
    {
        var provider = CreateProvider(HttpStatusCode.NotFound, """{"error":"not found"}""");

        var result = await provider.GetConversationAsync("9999");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetConversationAsync_ReturnsConversation_WhenFound()
    {
        var provider = CreateProvider(HttpStatusCode.OK, ConversationPayloadJson);

        var result = await provider.GetConversationAsync("99");

        result.Should().NotBeNull();
        result!.ExternalId.Should().Be("99");
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
        var provider = CreateProvider(HttpStatusCode.UnprocessableEntity, """{"error":"invalid"}""");

        await provider.Invoking(p => p.SendMessageAsync("conv-42", new ChatMessageCreateRequest { Content = "Hi" }))
            .Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task SendMessageAsync_ReturnsMessage_WhenSuccessful()
    {
        var provider = CreateProvider(HttpStatusCode.Created, MessagePayloadJson);

        var result = await provider.SendMessageAsync("99", new ChatMessageCreateRequest
        {
            Content = "Hello, how can I help?",
            IsPrivate = false
        });

        result.Should().NotBeNull();
        result.ExternalId.Should().Be("77");
        result.Content.Should().Be("Hello, how can I help?");
        result.ConversationId.Should().Be("99");
    }
}
