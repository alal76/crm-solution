// Phase 3 Week 12: ChatwootProvider Unit Tests
// Tests the Chatwoot API integration for chat functionality

using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Chatwoot;

namespace CRM.Tests.Providers;

public class ChatwootProviderTests
{
    private readonly Mock<IOptions<ChatwootConfiguration>> _optionsMock;
    private readonly Mock<ILogger<ChatwootProvider>> _loggerMock;
    private readonly ChatwootConfiguration _config;
    private readonly JsonSerializerOptions _jsonOptions;

    public ChatwootProviderTests()
    {
        _config = new ChatwootConfiguration
        {
            BaseUrl = "https://chatwoot.example.com",
            ApiKey = "test-api-key",
            AccountId = 1,
            DefaultInboxId = 1,
            ApiInboxId = 2,
            WebhookSecret = "test-webhook-secret"
        };

        _optionsMock = new Mock<IOptions<ChatwootConfiguration>>();
        _optionsMock.Setup(o => o.Value).Returns(_config);

        _loggerMock = new Mock<ILogger<ChatwootProvider>>();
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false
        };
    }

    private ChatwootProvider CreateProvider(HttpClient httpClient)
    {
        return new ChatwootProvider(httpClient, _optionsMock.Object, _loggerMock.Object);
    }

    private HttpClient CreateMockHttpClient(HttpResponseMessage response)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl)
        };
    }

    private HttpClient CreateMockHttpClientWithSequence(params Func<HttpResponseMessage>[] responseFactories)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var callIndex = 0;
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                var index = callIndex;
                callIndex++;
                return index < responseFactories.Length
                    ? responseFactories[index]()
                    : new HttpResponseMessage(HttpStatusCode.NotFound);
            });

        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl)
        };
    }

    #region Basic Provider Tests

    [Fact]
    public void ProviderName_Should_Return_Chatwoot()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var provider = CreateProvider(httpClient);

        // Assert
        Assert.Equal("Chatwoot", provider.ProviderName);
    }

    [Fact]
    public async Task IsAvailableAsync_Should_Return_True_When_Api_Responds()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var httpClient = CreateMockHttpClient(response);
        var provider = CreateProvider(httpClient);

        // Act
        var result = await provider.IsAvailableAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsAvailableAsync_Should_Return_False_When_Api_Fails()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        var httpClient = CreateMockHttpClient(response);
        var provider = CreateProvider(httpClient);

        // Act
        var result = await provider.IsAvailableAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HealthCheckAsync_Should_Return_Healthy_Result()
    {
        // Arrange
        var chatwootResponse = new[]
        {
            new { id = 1, name = "Agent 1", email = "agent1@example.com", availability_status = "online" }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(chatwootResponse, _jsonOptions), Encoding.UTF8, "application/json")
        };

        var httpClient = CreateMockHttpClient(response);
        var provider = CreateProvider(httpClient);

        // Act
        var result = await provider.HealthCheckAsync();

        // Assert
        Assert.True(result.IsHealthy);
        Assert.Equal("Chatwoot", result.ProviderName);
        Assert.True(result.ResponseTimeMs >= 0);
    }

    [Fact]
    public async Task HealthCheckAsync_Should_Return_Unhealthy_On_Error()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var httpClient = CreateMockHttpClient(response);
        var provider = CreateProvider(httpClient);

        // Act
        var result = await provider.HealthCheckAsync();

        // Assert
        Assert.False(result.IsHealthy);
        Assert.Equal("Chatwoot", result.ProviderName);
    }

    #endregion

    #region Contact Management Tests

    [Fact]
    public async Task CreateContactAsync_Should_Return_Created_Contact()
    {
        // Arrange
        var chatwootResponse = new
        {
            payload = new
            {
                contact = new
                {
                    id = 123,
                    email = "john@example.com",
                    phone_number = "+1234567890",
                    name = "John Doe"
                }
            }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(chatwootResponse, _jsonOptions), Encoding.UTF8, "application/json")
        };

        var httpClient = CreateMockHttpClient(response);
        var provider = CreateProvider(httpClient);

        var request = new ChatContactCreateRequest
        {
            Email = "john@example.com",
            Phone = "+1234567890",
            Name = "John Doe",
            CustomAttributes = new Dictionary<string, object>
            {
                { "crm_id", "CRM-001" }
            }
        };

        // Act
        var result = await provider.CreateContactAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123", result.ExternalId);
        Assert.Equal("john@example.com", result.Email);
        Assert.Equal("John Doe", result.Name);
    }

    [Fact]
    public async Task GetContactAsync_Should_Return_Contact_When_Found()
    {
        // Arrange
        var chatwootResponse = new
        {
            id = 123,
            email = "john@example.com",
            phone_number = "+1234567890",
            name = "John Doe",
            custom_attributes = new Dictionary<string, object>
            {
                { "crm_id", "CRM-001" }
            }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(chatwootResponse, _jsonOptions), Encoding.UTF8, "application/json")
        };

        var httpClient = CreateMockHttpClient(response);
        var provider = CreateProvider(httpClient);

        // Act
        var result = await provider.GetContactAsync("123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123", result.ExternalId);
        Assert.Equal("john@example.com", result.Email);
    }

    [Fact]
    public async Task GetContactAsync_Should_Return_Null_When_NotFound()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var httpClient = CreateMockHttpClient(response);
        var provider = CreateProvider(httpClient);

        // Act
        var result = await provider.GetContactAsync("999");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FindContactByEmailAsync_Should_Return_Contact_When_Found()
    {
        // Arrange
        var chatwootResponse = new
        {
            payload = new[]
            {
                new
                {
                    id = 123,
                    email = "john@example.com",
                    name = "John Doe"
                }
            }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(chatwootResponse, _jsonOptions), Encoding.UTF8, "application/json")
        };

        var httpClient = CreateMockHttpClient(response);
        var provider = CreateProvider(httpClient);

        // Act
        var result = await provider.FindContactByEmailAsync("john@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123", result.ExternalId);
        Assert.Equal("john@example.com", result.Email);
    }

    [Fact]
    public async Task FindContactByEmailAsync_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        var chatwootResponse = new { payload = Array.Empty<object>() };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(chatwootResponse, _jsonOptions), Encoding.UTF8, "application/json")
        };

        var httpClient = CreateMockHttpClient(response);
        var provider = CreateProvider(httpClient);

        // Act
        var result = await provider.FindContactByEmailAsync("unknown@example.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateContactAsync_Should_Not_Throw()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };

        var httpClient = CreateMockHttpClient(response);
        var provider = CreateProvider(httpClient);

        var request = new ChatContactUpdateRequest
        {
            Name = "John Updated",
            CustomAttributes = new Dictionary<string, object>
            {
                { "tier", "gold" }
            }
        };

        // Act & Assert (should not throw)
        await provider.UpdateContactAsync("123", request);
    }

    #endregion

    #region Conversation Management Tests

    [Fact]
    public async Task CreateConversationAsync_Should_Return_Created_Conversation()
    {
        // Arrange - CreateConversationAsync first calls GetContactAsync, then creates conversation
        var contactResponse = new
        {
            payload = new
            {
                id = 123,
                name = "Test Contact",
                email = "test@example.com"
            }
        };
        var conversationResponse = new
        {
            id = 789,
            inbox_id = 1,
            contact_id = 123,
            status = "open",
            channel = "api",
            meta = new
            {
                sender = new { id = 123, name = "Test Contact" }
            }
        };

        var httpClient = CreateMockHttpClientWithSequence(
            () => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(contactResponse, _jsonOptions), Encoding.UTF8, "application/json")
            },
            () => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(conversationResponse, _jsonOptions), Encoding.UTF8, "application/json")
            });

        var provider = CreateProvider(httpClient);

        var request = new ChatConversationCreateRequest
        {
            ContactExternalId = "123"
        };

        // Act
        var result = await provider.CreateConversationAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("789", result.ExternalId);
        Assert.Equal("123", result.ContactExternalId);
        Assert.Equal("open", result.Status);
    }

    [Fact]
    public async Task GetConversationAsync_Should_Return_Conversation_When_Found()
    {
        // Arrange
        var chatwootResponse = new
        {
            id = 789,
            inbox_id = 1,
            contact_id = 123,
            status = "open",
            channel = "api",
            messages = new[]
            {
                new { id = 1, content = "Hello!", message_type = 0 }
            }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(chatwootResponse, _jsonOptions), Encoding.UTF8, "application/json")
        };

        var httpClient = CreateMockHttpClient(response);
        var provider = CreateProvider(httpClient);

        // Act
        var result = await provider.GetConversationAsync("789");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("789", result.ExternalId);
        Assert.Equal("open", result.Status);
    }

    [Fact]
    public async Task GetConversationAsync_Should_Return_Null_When_NotFound()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var httpClient = CreateMockHttpClient(response);
        var provider = CreateProvider(httpClient);

        // Act
        var result = await provider.GetConversationAsync("999");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetContactConversationsAsync_Should_Return_Conversations()
    {
        // Arrange
        var chatwootResponse = new
        {
            payload = new[]
            {
                new { id = 789, status = "open", contact_id = 123 },
                new { id = 790, status = "resolved", contact_id = 123 }
            }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(chatwootResponse, _jsonOptions), Encoding.UTF8, "application/json")
        };

        var httpClient = CreateMockHttpClient(response);
        var provider = CreateProvider(httpClient);

        // Act
        var result = await provider.GetContactConversationsAsync("123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ResolveConversationAsync_Should_Not_Throw()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };

        var httpClient = CreateMockHttpClient(response);
        var provider = CreateProvider(httpClient);

        // Act & Assert (should not throw)
        await provider.ResolveConversationAsync("789");
    }

    #endregion

    #region Message Tests

    [Fact]
    public async Task SendMessageAsync_Should_Return_Created_Message()
    {
        // Arrange
        var chatwootResponse = new
        {
            id = 1001,
            content = "Hello from agent!",
            message_type = 1,
            created_at = DateTime.UtcNow
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(chatwootResponse, _jsonOptions), Encoding.UTF8, "application/json")
        };

        var httpClient = CreateMockHttpClient(response);
        var provider = CreateProvider(httpClient);

        var request = new ChatMessageCreateRequest
        {
            Content = "Hello from agent!",
            IsPrivate = false
        };

        // Act
        var result = await provider.SendMessageAsync("789", request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("1001", result.ExternalId);
        Assert.Equal("Hello from agent!", result.Content);
    }

    [Fact]
    public async Task GetMessagesAsync_Should_Return_Messages()
    {
        // Arrange
        var chatwootResponse = new
        {
            payload = new[]
            {
                new
                {
                    id = 1001,
                    content = "Hello!",
                    message_type = 0,
                    created_at = DateTime.UtcNow.AddMinutes(-5)
                },
                new
                {
                    id = 1002,
                    content = "Hi there!",
                    message_type = 1,
                    created_at = DateTime.UtcNow
                }
            }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(chatwootResponse, _jsonOptions), Encoding.UTF8, "application/json")
        };

        var httpClient = CreateMockHttpClient(response);
        var provider = CreateProvider(httpClient);

        // Act
        var result = await provider.GetMessagesAsync("789");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region Agent Tests

    [Fact]
    public async Task GetAgentsAsync_Should_Return_Agents()
    {
        // Arrange
        var chatwootResponse = new[]
        {
            new { id = 1, name = "Agent 1", email = "agent1@example.com", availability_status = "online" },
            new { id = 2, name = "Agent 2", email = "agent2@example.com", availability_status = "offline" }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(chatwootResponse, _jsonOptions), Encoding.UTF8, "application/json")
        };

        var httpClient = CreateMockHttpClient(response);
        var provider = CreateProvider(httpClient);

        // Act
        var result = await provider.GetAgentsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAgentStatusAsync_Should_Return_Agent()
    {
        // Arrange - GetAgentStatusAsync calls GetAgentsAsync internally (returns a list)
        var chatwootResponse = new[]
        {
            new
            {
                id = 1,
                name = "Agent 1",
                email = "agent1@example.com",
                availability_status = "online"
            },
            new
            {
                id = 2,
                name = "Agent 2",
                email = "agent2@example.com",
                availability_status = "offline"
            }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(chatwootResponse, _jsonOptions), Encoding.UTF8, "application/json")
        };

        var httpClient = CreateMockHttpClient(response);
        var provider = CreateProvider(httpClient);

        // Act
        var result = await provider.GetAgentStatusAsync("1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("online", result.Status);
    }

    [Fact]
    public async Task AssignAgentAsync_Should_Not_Throw()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };

        var httpClient = CreateMockHttpClient(response);
        var provider = CreateProvider(httpClient);

        // Act & Assert (should not throw)
        await provider.AssignAgentAsync("789", "1");
    }

    #endregion

    #region Webhook Tests

    [Fact]
    public async Task ProcessWebhookAsync_Message_Created_Should_Return_Result()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var provider = CreateProvider(httpClient);

        var payload = @"{
            ""event"": ""message_created"",
            ""id"": 1001,
            ""content"": ""Hello!"",
            ""conversation"": {
                ""id"": 789,
                ""status"": ""open""
            },
            ""sender"": {
                ""id"": 123,
                ""name"": ""Customer""
            }
        }";

        // Act
        var result = await provider.ProcessWebhookAsync("message_created", payload);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("message_created", result.EventType);
        Assert.Equal("789", result.ConversationExternalId);
    }

    [Fact]
    public async Task ProcessWebhookAsync_Conversation_Created_Should_Return_Result()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var provider = CreateProvider(httpClient);

        var payload = @"{
            ""event"": ""conversation_created"",
            ""conversation"": {
                ""id"": 789,
                ""status"": ""open"",
                ""contact_id"": 123
            }
        }";

        // Act
        var result = await provider.ProcessWebhookAsync("conversation_created", payload);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("conversation_created", result.EventType);
    }

    [Fact]
    public async Task ProcessWebhookAsync_Unknown_Event_Should_Return_Success()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var provider = CreateProvider(httpClient);

        var payload = @"{""event"": ""unknown_event""}";

        // Act
        var result = await provider.ProcessWebhookAsync("unknown_event", payload);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("unknown_event", result.EventType);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task CreateContactAsync_Should_Throw_On_Error_Response()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(@"{""error"": ""Invalid email""}", Encoding.UTF8, "application/json")
        };

        var httpClient = CreateMockHttpClient(response);
        var provider = CreateProvider(httpClient);

        var request = new ChatContactCreateRequest
        {
            Email = "invalid-email"
        };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => provider.CreateContactAsync(request));
    }

    [Fact]
    public async Task SendMessageAsync_Should_Throw_On_Server_Error()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var httpClient = CreateMockHttpClient(response);
        var provider = CreateProvider(httpClient);

        var request = new ChatMessageCreateRequest
        {
            Content = "Test message"
        };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => provider.SendMessageAsync("789", request));
    }

    #endregion

    #region Configuration Validation Tests

    [Fact]
    public void Configuration_Validation_Should_Fail_Without_BaseUrl()
    {
        // Arrange
        var config = new ChatwootConfiguration
        {
            ApiKey = "test-key",
            AccountId = 1
        };

        // Act
        var (isValid, errors) = config.Validate();

        // Assert
        Assert.False(isValid);
        Assert.Contains("BaseUrl is required", errors);
    }

    [Fact]
    public void Configuration_Validation_Should_Fail_Without_ApiKey()
    {
        // Arrange
        var config = new ChatwootConfiguration
        {
            BaseUrl = "https://chatwoot.example.com"
        };

        // Act
        var (isValid, errors) = config.Validate();

        // Assert
        Assert.False(isValid);
        Assert.Contains("ApiKey is required", errors);
    }

    [Fact]
    public void Configuration_Validation_Should_Pass_With_Required_Fields()
    {
        // Arrange
        var config = new ChatwootConfiguration
        {
            BaseUrl = "https://chatwoot.example.com",
            ApiKey = "test-key",
            AccountId = 1
        };

        // Act
        var (isValid, errors) = config.Validate();

        // Assert
        Assert.True(isValid);
        Assert.Empty(errors);
    }

    #endregion
}
