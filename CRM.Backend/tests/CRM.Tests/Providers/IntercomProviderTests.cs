// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// Licensed under the GNU Affero General Public License v3.0

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Intercom;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for IntercomProvider implementing IChatPort.
/// </summary>
public class IntercomProviderTests
{
    private readonly Mock<ILogger<IntercomProvider>> _loggerMock;
    private readonly IntercomConfiguration _config;
    private readonly JsonSerializerOptions _jsonOptions;

    public IntercomProviderTests()
    {
        _loggerMock = new Mock<ILogger<IntercomProvider>>();
        _config = new IntercomConfiguration
        {
            AccessToken = "test-access-token",
            AppId = "test-app-id",
            BaseUrl = "https://api.intercom.io",
            ApiVersion = "2.11",
            WebhookSecret = "test-webhook-secret",
            DefaultAdminId = "admin-123"
        };
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };
    }

    private IntercomProvider CreateProvider(HttpClient httpClient)
    {
        return new IntercomProvider(
            httpClient,
            Options.Create(_config),
            _loggerMock.Object);
    }

    private HttpClient CreateMockHttpClient(HttpStatusCode statusCode, object? responseContent = null)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = responseContent != null
                    ? new StringContent(JsonSerializer.Serialize(responseContent, _jsonOptions), Encoding.UTF8, "application/json")
                    : new StringContent("{}", Encoding.UTF8, "application/json")
            });

        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl)
        };
    }

    private HttpClient CreateMockHttpClient(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) => handler(request));

        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl)
        };
    }

    #region Provider Identity Tests

    [Fact]
    public void ProviderName_ReturnsIntercom()
    {
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK);
        var provider = CreateProvider(httpClient);

        Assert.Equal("Intercom", provider.ProviderName);
    }

    [Fact]
    public async Task IsAvailableAsync_WhenConfigValid_ReturnsTrue()
    {
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, new { admins = new[] { new { id = "1" } } });
        var provider = CreateProvider(httpClient);

        var result = await provider.IsAvailableAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task IsAvailableAsync_WhenApiUnreachable_ReturnsFalse()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl)
        };
        var provider = CreateProvider(httpClient);

        var result = await provider.IsAvailableAsync();

        Assert.False(result);
    }

    #endregion

    #region Contact Management Tests

    [Fact]
    public async Task CreateContactAsync_WithValidData_ReturnsContact()
    {
        var createRequest = new ChatContactCreateRequest
        {
            Email = "test@example.com",
            Name = "Test User",
            Phone = "+1234567890"
        };

        var mockResponse = new
        {
            id = "contact-123",
            email = "test@example.com",
            name = "Test User",
            phone = "+1234567890",
            role = "user",
            created_at = 1700000000L
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
        var provider = CreateProvider(httpClient);

        var result = await provider.CreateContactAsync(createRequest);

        Assert.NotNull(result);
        Assert.Equal("contact-123", result.ExternalId);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("Test User", result.Name);
    }

    [Fact]
    public async Task GetContactAsync_WithValidId_ReturnsContact()
    {
        var mockResponse = new
        {
            id = "contact-456",
            email = "existing@example.com",
            name = "Existing User",
            phone = "+0987654321",
            created_at = 1700000000L,
            last_seen_at = 1700001000L
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
        var provider = CreateProvider(httpClient);

        var result = await provider.GetContactAsync("contact-456");

        Assert.NotNull(result);
        Assert.Equal("contact-456", result.ExternalId);
        Assert.Equal("existing@example.com", result.Email);
    }

    [Fact]
    public async Task GetContactAsync_WithInvalidId_ReturnsNull()
    {
        var httpClient = CreateMockHttpClient(HttpStatusCode.NotFound);
        var provider = CreateProvider(httpClient);

        var result = await provider.GetContactAsync("nonexistent-id");

        Assert.Null(result);
    }

    [Fact]
    public async Task FindContactByEmailAsync_WithExistingEmail_ReturnsContact()
    {
        var mockSearchResponse = new
        {
            type = "list",
            data = new[]
            {
                new
                {
                    id = "contact-789",
                    email = "search@example.com",
                    name = "Found User",
                    created_at = 1700000000L
                }
            },
            total_count = 1
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockSearchResponse);
        var provider = CreateProvider(httpClient);

        var result = await provider.FindContactByEmailAsync("search@example.com");

        Assert.NotNull(result);
        Assert.Equal("contact-789", result.ExternalId);
        Assert.Equal("search@example.com", result.Email);
    }

    [Fact]
    public async Task FindContactByEmailAsync_WithNoResults_ReturnsNull()
    {
        var mockSearchResponse = new
        {
            type = "list",
            data = Array.Empty<object>(),
            total_count = 0
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockSearchResponse);
        var provider = CreateProvider(httpClient);

        var result = await provider.FindContactByEmailAsync("notfound@example.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateContactAsync_WithValidData_Succeeds()
    {
        var updateRequest = new ChatContactUpdateRequest
        {
            Name = "Updated Name",
            Phone = "+1111111111"
        };

        var mockResponse = new
        {
            id = "contact-update-123",
            email = "update@example.com",
            name = "Updated Name",
            phone = "+1111111111",
            created_at = 1700000000L
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
        var provider = CreateProvider(httpClient);

        // Should not throw
        await provider.UpdateContactAsync("contact-update-123", updateRequest);
    }

    #endregion

    #region Conversation Management Tests

    [Fact]
    public async Task CreateConversationAsync_WithValidData_ReturnsConversation()
    {
        var createRequest = new ChatConversationCreateRequest
        {
            ContactExternalId = "contact-123",
            InitialMessage = "Hello, I need help!"
        };

        var mockResponse = new
        {
            id = "conv-123",
            state = "open",
            created_at = 1700000000L,
            updated_at = 1700000000L
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
        var provider = CreateProvider(httpClient);

        var result = await provider.CreateConversationAsync(createRequest);

        Assert.NotNull(result);
        Assert.Equal("conv-123", result.ExternalId);
        Assert.Equal("open", result.Status);
    }

    [Fact]
    public async Task GetConversationAsync_WithValidId_ReturnsConversation()
    {
        var mockResponse = new
        {
            id = "conv-456",
            state = "open",
            created_at = 1700000000L,
            updated_at = 1700001000L,
            contacts = new
            {
                contacts = new[] { new { id = "contact-789" } }
            },
            assignee = new { id = "admin-1", name = "Agent Smith" },
            conversation_parts = new
            {
                parts = new[]
                {
                    new
                    {
                        id = "part-1",
                        body = "Hi there!",
                        part_type = "comment",
                        author = new { type = "user", id = "contact-789" },
                        created_at = 1700000500L
                    }
                }
            },
            statistics = new
            {
                count_conversation_parts = 1
            }
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
        var provider = CreateProvider(httpClient);

        var result = await provider.GetConversationAsync("conv-456");

        Assert.NotNull(result);
        Assert.Equal("conv-456", result.ExternalId);
        Assert.Equal("contact-789", result.ContactExternalId);
        Assert.Equal("admin-1", result.AssignedAgentId);
        Assert.Equal("Agent Smith", result.AssignedAgentName);
    }

    [Fact]
    public async Task SendMessageAsync_WithValidData_ReturnsMessage()
    {
        var messageRequest = new ChatMessageCreateRequest
        {
            Content = "Thank you for contacting support!",
            ContentType = "text"
        };

        // Intercom API returns a single conversation part for reply endpoint
        var mockResponse = new
        {
            id = "part-new-1",
            body = "Thank you for contacting support!",
            part_type = "comment",
            author = new { type = "admin", id = "admin-123" },
            created_at = 1700002000L
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
        var provider = CreateProvider(httpClient);

        var result = await provider.SendMessageAsync("conv-msg-123", messageRequest);

        Assert.NotNull(result);
        Assert.Equal("Thank you for contacting support!", result.Content);
    }

    [Fact]
    public async Task ResolveConversationAsync_ClosesConversation()
    {
        var mockResponse = new
        {
            id = "conv-resolve-123",
            state = "closed",
            created_at = 1700000000L,
            updated_at = 1700003000L
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
        var provider = CreateProvider(httpClient);

        // Should not throw
        await provider.ResolveConversationAsync("conv-resolve-123");
    }

    [Fact]
    public async Task ReopenConversationAsync_OpensConversation()
    {
        var mockResponse = new
        {
            id = "conv-reopen-123",
            state = "open",
            created_at = 1700000000L,
            updated_at = 1700004000L
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
        var provider = CreateProvider(httpClient);

        // Should not throw
        await provider.ReopenConversationAsync("conv-reopen-123");
    }

    #endregion

    #region Agent Management Tests

    [Fact]
    public async Task GetAgentsAsync_ReturnsAgentList()
    {
        var mockResponse = new
        {
            admins = new[]
            {
                new
                {
                    id = "admin-1",
                    name = "Agent One",
                    email = "agent1@company.com",
                    type = "admin"
                },
                new
                {
                    id = "admin-2",
                    name = "Agent Two",
                    email = "agent2@company.com",
                    type = "admin"
                }
            }
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
        var provider = CreateProvider(httpClient);

        var result = await provider.GetAgentsAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, a => a.ExternalId == "admin-1");
        Assert.Contains(result, a => a.ExternalId == "admin-2");
    }

    [Fact]
    public async Task AssignAgentAsync_AssignsAgentToConversation()
    {
        var mockResponse = new
        {
            id = "conv-assign-123",
            assignee = new { id = "admin-new", name = "New Agent" }
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
        var provider = CreateProvider(httpClient);

        // Should not throw
        await provider.AssignAgentAsync("conv-assign-123", "admin-new");
    }

    #endregion

    #region Webhook Processing Tests

    [Fact]
    public async Task ProcessWebhookAsync_ConversationUserCreated_ReturnsResult()
    {
        var webhookPayload = JsonSerializer.Serialize(new
        {
            type = "notification_event",
            topic = "conversation.user.created",
            data = new
            {
                item = new
                {
                    id = "conv-webhook-1",
                    state = "open",
                    contacts = new
                    {
                        contacts = new[] { new { id = "contact-wh-1" } }
                    },
                    source = new
                    {
                        body = "Initial message"
                    }
                }
            }
        });

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK);
        var provider = CreateProvider(httpClient);

        var result = await provider.ProcessWebhookAsync(
            "conversation.user.created",
            webhookPayload,
            null);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("conversation.user.created", result.EventType);
    }

    [Fact]
    public async Task ProcessWebhookAsync_MessageCreated_ExtractsMessage()
    {
        var webhookPayload = JsonSerializer.Serialize(new
        {
            type = "notification_event",
            topic = "conversation.admin.replied",
            data = new
            {
                item = new
                {
                    id = "conv-webhook-2",
                    state = "open",
                    conversation_parts = new
                    {
                        parts = new[]
                        {
                            new
                            {
                                id = "part-wh-1",
                                body = "Agent response",
                                part_type = "comment",
                                author = new { type = "admin", id = "admin-wh-1" },
                                created_at = 1700005000L
                            }
                        }
                    }
                }
            }
        });

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK);
        var provider = CreateProvider(httpClient);

        var result = await provider.ProcessWebhookAsync(
            "conversation.admin.replied",
            webhookPayload,
            null);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Message);
        Assert.Equal("Agent response", result.Message.Content);
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_WhenHealthy_ReturnsHealthyResult()
    {
        // /me endpoint returns app info
        var mockResponse = new
        {
            id = "app-123",
            type = "app",
            name = "Test CRM App"
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
        var provider = CreateProvider(httpClient);

        var result = await provider.HealthCheckAsync();

        Assert.NotNull(result);
        Assert.True(result.IsHealthy);
        Assert.Equal("Intercom", result.ProviderName);
        Assert.True(result.ResponseTimeMs >= 0);
        Assert.NotNull(result.Details);
        Assert.True(result.Details.ContainsKey("app_id"));
    }

    [Fact]
    public async Task HealthCheckAsync_WhenUnhealthy_ReturnsUnhealthyResult()
    {
        var httpClient = CreateMockHttpClient(HttpStatusCode.Unauthorized);
        var provider = CreateProvider(httpClient);

        var result = await provider.HealthCheckAsync();

        Assert.NotNull(result);
        Assert.False(result.IsHealthy);
        Assert.Equal("Intercom", result.ProviderName);
        Assert.NotNull(result.Message);
    }

    [Fact]
    public async Task HealthCheckAsync_WhenException_ReturnsUnhealthyResult()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection timeout"));

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl)
        };
        var provider = CreateProvider(httpClient);

        var result = await provider.HealthCheckAsync();

        Assert.NotNull(result);
        Assert.False(result.IsHealthy);
        Assert.Contains("Connection timeout", result.Message);
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void IntercomConfiguration_Validate_WithValidConfig_ReturnsValid()
    {
        var config = new IntercomConfiguration
        {
            AccessToken = "valid-token",
            AppId = "valid-app-id",
            BaseUrl = "https://api.intercom.io"
        };

        var (isValid, errors) = config.Validate();

        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public void IntercomConfiguration_Validate_WithMissingToken_ReturnsInvalid()
    {
        var config = new IntercomConfiguration
        {
            AccessToken = "",
            AppId = "valid-app-id"
        };

        var (isValid, errors) = config.Validate();

        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("AccessToken"));
    }

    [Fact]
    public void IntercomConfiguration_Validate_WithMissingAppId_ReturnsInvalid()
    {
        var config = new IntercomConfiguration
        {
            AccessToken = "valid-token",
            AppId = ""
        };

        var (isValid, errors) = config.Validate();

        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("AppId"));
    }

    #endregion
}
