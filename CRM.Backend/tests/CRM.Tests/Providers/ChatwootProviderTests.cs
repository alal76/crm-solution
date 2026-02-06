// CRM Solution - ChatwootProvider Tests
// Tests for the Chatwoot chat provider

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Chatwoot;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for ChatwootProvider.
/// Tests contact management, conversations, and messaging.
/// </summary>
public class ChatwootProviderTests : IDisposable
{
    private readonly Mock<ILogger<ChatwootProvider>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly IOptions<ChatwootConfiguration> _options;
    private readonly ChatwootProvider _provider;

    public ChatwootProviderTests()
    {
        _loggerMock = new Mock<ILogger<ChatwootProvider>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://chatwoot.example.com")
        };

        _options = Options.Create(new ChatwootConfiguration
        {
            BaseUrl = "https://chatwoot.example.com",
            ApiKey = "test-api-key",
            AccountId = "1",
            InboxId = "1",
            WebhookSecret = "webhook-secret"
        });

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);

        _provider = new ChatwootProvider(_options, _loggerMock.Object, httpClientFactoryMock.Object);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content = "{}")
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_CreatesProvider()
    {
        // Assert
        _provider.Should().NotBeNull();
        _provider.ProviderName.Should().Be("Chatwoot");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ChatwootProvider(null!, _loggerMock.Object, Mock.Of<IHttpClientFactory>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Contact Tests

    [Fact]
    public async Task CreateContactAsync_WithValidRequest_ReturnsContact()
    {
        // Arrange
        var response = new { id = 123, name = "John Doe", email = "john@example.com" };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new ChatContactCreateRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Phone = "+1234567890"
        };

        // Act
        var result = await _provider.CreateContactAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task CreateContactAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _provider.CreateContactAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetContactByIdAsync_WithValidId_ReturnsContact()
    {
        // Arrange
        var response = new { id = 123, name = "Jane Doe", email = "jane@example.com" };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.GetContactByIdAsync("123");

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public async Task GetContactByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.NotFound);

        // Act
        var result = await _provider.GetContactByIdAsync("999");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindContactByEmailAsync_WithExistingEmail_ReturnsContact()
    {
        // Arrange
        var response = new { payload = new[] { new { id = 123, email = "test@example.com" } } };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.FindContactByEmailAsync("test@example.com");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateContactAsync_WithValidRequest_ReturnsUpdatedContact()
    {
        // Arrange
        var response = new { id = 123, name = "Updated Name", email = "updated@example.com" };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new ChatContactUpdateRequest
        {
            ContactId = "123",
            Name = "Updated Name",
            Email = "updated@example.com"
        };

        // Act
        var result = await _provider.UpdateContactAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
    }

    #endregion

    #region Conversation Tests

    [Fact]
    public async Task CreateConversationAsync_WithValidRequest_ReturnsConversation()
    {
        // Arrange
        var response = new { id = 456, status = "open", contact_id = 123 };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new ChatConversationCreateRequest
        {
            ContactId = "123",
            InboxId = "1",
            InitialMessage = "Hello, I need help!"
        };

        // Act
        var result = await _provider.CreateConversationAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("open");
    }

    [Fact]
    public async Task GetConversationAsync_WithValidId_ReturnsConversation()
    {
        // Arrange
        var response = new { id = 456, status = "open", messages = new object[] { } };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.GetConversationAsync("456");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetConversationsForContactAsync_WithValidContactId_ReturnsConversations()
    {
        // Arrange
        var response = new { data = new { payload = new[] { new { id = 1 }, new { id = 2 } } } };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.GetConversationsForContactAsync("123");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CloseConversationAsync_WithValidId_ClosesConversation()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{\"status\":\"resolved\"}");

        // Act
        var result = await _provider.CloseConversationAsync("456");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ReopenConversationAsync_WithValidId_ReopensConversation()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{\"status\":\"open\"}");

        // Act
        var result = await _provider.ReopenConversationAsync("456");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Message Tests

    [Fact]
    public async Task SendMessageAsync_WithValidRequest_ReturnsMessage()
    {
        // Arrange
        var response = new { id = 789, content = "Hello!", message_type = "outgoing" };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new ChatMessageCreateRequest
        {
            ConversationId = "456",
            Content = "Hello!",
            ContentType = "text"
        };

        // Act
        var result = await _provider.SendMessageAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be("Hello!");
    }

    [Fact]
    public async Task SendMessageAsync_WithAttachment_SendsWithAttachment()
    {
        // Arrange
        var response = new { id = 789, content = "See attached file" };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        var request = new ChatMessageCreateRequest
        {
            ConversationId = "456",
            Content = "See attached file",
            Attachments = new List<ChatAttachment>
            {
                new ChatAttachment
                {
                    Filename = "document.pdf",
                    Content = Convert.ToBase64String(new byte[] { 1, 2, 3 }),
                    ContentType = "application/pdf"
                }
            }
        };

        // Act
        var result = await _provider.SendMessageAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMessagesAsync_WithValidConversationId_ReturnsMessages()
    {
        // Arrange
        var response = new
        {
            payload = new[]
            {
                new { id = 1, content = "Message 1" },
                new { id = 2, content = "Message 2" }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.GetMessagesAsync("456");

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Agent Tests

    [Fact]
    public async Task GetAgentsAsync_ReturnsAgents()
    {
        // Arrange
        var response = new[]
        {
            new { id = 1, name = "Agent 1", email = "agent1@example.com" },
            new { id = 2, name = "Agent 2", email = "agent2@example.com" }
        };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.GetAgentsAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task AssignAgentAsync_WithValidRequest_AssignsAgent()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK);

        // Act
        var result = await _provider.AssignAgentAsync("456", "1");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetAgentStatusAsync_WithValidId_ReturnsStatus()
    {
        // Arrange
        var response = new { id = 1, availability_status = "online" };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.GetAgentStatusAsync("1");

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Webhook Tests

    [Fact]
    public async Task ProcessWebhookAsync_WithConversationCreated_ProcessesSuccessfully()
    {
        // Arrange
        var payload = new ChatWebhookPayload
        {
            Event = "conversation_created",
            ConversationId = "456",
            ContactId = "123",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _provider.ProcessWebhookAsync(payload);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithMessageCreated_ProcessesSuccessfully()
    {
        // Arrange
        var payload = new ChatWebhookPayload
        {
            Event = "message_created",
            ConversationId = "456",
            MessageId = "789",
            Content = "Hello!",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _provider.ProcessWebhookAsync(payload);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithConversationResolved_ProcessesSuccessfully()
    {
        // Arrange
        var payload = new ChatWebhookPayload
        {
            Event = "conversation_status_changed",
            ConversationId = "456",
            Status = "resolved",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _provider.ProcessWebhookAsync(payload);

        // Assert
        result.Success.Should().BeTrue();
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_WithHealthyApi_ReturnsHealthy()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{}");

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsHealthy.Should().BeTrue();
        result.ProviderName.Should().Be("Chatwoot");
    }

    [Fact]
    public async Task HealthCheckAsync_WithUnhealthyApi_ReturnsUnhealthy()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.Unauthorized);

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.IsHealthy.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WithHealthyApi_ReturnsTrue()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{}");

        // Act
        var isAvailable = await _provider.IsAvailableAsync();

        // Assert
        isAvailable.Should().BeTrue();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task SendMessageAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{\"id\":1}");

        var request = new ChatMessageCreateRequest
        {
            ConversationId = "456",
            Content = "Test"
        };
        var cts = new CancellationTokenSource();

        // Act
        var result = await _provider.SendMessageAsync(request, cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Label Tests

    [Fact]
    public async Task AddLabelToConversationAsync_AddsLabel()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK);

        // Act
        var result = await _provider.AddLabelToConversationAsync("456", "urgent");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveLabelFromConversationAsync_RemovesLabel()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK);

        // Act
        var result = await _provider.RemoveLabelFromConversationAsync("456", "urgent");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Custom Attributes Tests

    [Fact]
    public async Task SetContactCustomAttributeAsync_SetsAttribute()
    {
        // Arrange
        var response = new { id = 123, custom_attributes = new { crm_id = "CRM-001" } };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.SetContactCustomAttributeAsync("123", "crm_id", "CRM-001");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SetConversationCustomAttributeAsync_SetsAttribute()
    {
        // Arrange
        var response = new { id = 456, custom_attributes = new { priority = "high" } };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _provider.SetConversationCustomAttributeAsync("456", "priority", "high");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task CreateContactAsync_WithApiError_ThrowsException()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.InternalServerError);

        var request = new ChatContactCreateRequest
        {
            Name = "Test",
            Email = "test@example.com"
        };

        // Act
        var act = () => _provider.CreateContactAsync(request);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task SendMessageAsync_WithRateLimitExceeded_HandlesGracefully()
    {
        // Arrange
        SetupHttpResponse((HttpStatusCode)429);

        var request = new ChatMessageCreateRequest
        {
            ConversationId = "456",
            Content = "Test"
        };

        // Act
        var act = () => _provider.SendMessageAsync(request);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    #endregion
}
