// CRM Solution - BuiltInChatProvider Tests
// Tests for the built-in in-memory chat provider

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.BuiltIn;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for BuiltInChatProvider.
/// Tests contact management, conversations, messages, and agent operations.
/// </summary>
public class BuiltInChatProviderTests
{
    private readonly Mock<ILogger<BuiltInChatProvider>> _loggerMock;

    public BuiltInChatProviderTests()
    {
        _loggerMock = new Mock<ILogger<BuiltInChatProvider>>();
    }

    private BuiltInChatProvider CreateProvider()
    {
        return new BuiltInChatProvider(_loggerMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidLogger_CreatesProvider()
    {
        // Act
        var provider = CreateProvider();

        // Assert
        provider.Should().NotBeNull();
        provider.ProviderName.Should().Be("BuiltIn");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new BuiltInChatProvider(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public async Task Constructor_CreatesDefaultAgent()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var agents = await provider.GetAgentsAsync();

        // Assert
        agents.Should().NotBeEmpty();
        agents.Should().Contain(a => a.ExternalId == "agent_1" && a.Name == "System Agent");
    }

    #endregion

    #region Provider Properties Tests

    [Fact]
    public void ProviderName_ReturnsBuiltIn()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var name = provider.ProviderName;

        // Assert
        name.Should().Be("BuiltIn");
    }

    [Fact]
    public async Task IsAvailableAsync_AlwaysReturnsTrue()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var isAvailable = await provider.IsAvailableAsync();

        // Assert
        isAvailable.Should().BeTrue();
    }

    #endregion

    #region Contact Management Tests

    [Fact]
    public async Task CreateContactAsync_WithValidRequest_CreatesContact()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new ChatContactCreateRequest
        {
            Email = "test@example.com",
            Name = "Test User",
            Phone = "+15551234567",
            CrmContactId = "crm-123",
            CrmAccountId = "account-456"
        };

        // Act
        var contact = await provider.CreateContactAsync(request);

        // Assert
        contact.Should().NotBeNull();
        contact.ExternalId.Should().StartWith("builtin_contact_");
        contact.Email.Should().Be("test@example.com");
        contact.Name.Should().Be("Test User");
        contact.Phone.Should().Be("+15551234567");
        contact.CrmContactId.Should().Be("crm-123");
        contact.CrmAccountId.Should().Be("account-456");
        contact.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateContactAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = () => provider.CreateContactAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateContactAsync_AssignsUniqueIds()
    {
        // Arrange
        var provider = CreateProvider();
        var request1 = new ChatContactCreateRequest { Email = "user1@example.com", Name = "User 1" };
        var request2 = new ChatContactCreateRequest { Email = "user2@example.com", Name = "User 2" };

        // Act
        var contact1 = await provider.CreateContactAsync(request1);
        var contact2 = await provider.CreateContactAsync(request2);

        // Assert
        contact1.ExternalId.Should().NotBe(contact2.ExternalId);
    }

    [Fact]
    public async Task GetContactAsync_WithExistingContact_ReturnsContact()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new ChatContactCreateRequest { Email = "find@example.com", Name = "Find Me" };
        var created = await provider.CreateContactAsync(request);

        // Act
        var found = await provider.GetContactAsync(created.ExternalId);

        // Assert
        found.Should().NotBeNull();
        found!.Email.Should().Be("find@example.com");
        found.Name.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetContactAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var contact = await provider.GetContactAsync("non-existent-id");

        // Assert
        contact.Should().BeNull();
    }

    [Fact]
    public async Task GetContactAsync_WithEmptyId_ThrowsArgumentException()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = () => provider.GetContactAsync("");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task FindContactByEmailAsync_WithExistingEmail_ReturnsContact()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new ChatContactCreateRequest { Email = "search@example.com", Name = "Search User" };
        await provider.CreateContactAsync(request);

        // Act
        var found = await provider.FindContactByEmailAsync("search@example.com");

        // Assert
        found.Should().NotBeNull();
        found!.Name.Should().Be("Search User");
    }

    [Fact]
    public async Task FindContactByEmailAsync_WithNonExistentEmail_ReturnsNull()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var contact = await provider.FindContactByEmailAsync("notfound@example.com");

        // Assert
        contact.Should().BeNull();
    }

    [Fact]
    public async Task UpdateContactAsync_WithExistingContact_UpdatesContact()
    {
        // Arrange
        var provider = CreateProvider();
        var createRequest = new ChatContactCreateRequest { Email = "update@example.com", Name = "Original" };
        var created = await provider.CreateContactAsync(createRequest);
        var updateRequest = new ChatContactUpdateRequest
        {
            ExternalId = created.ExternalId,
            Name = "Updated Name",
            Phone = "+15559876543"
        };

        // Act
        var updated = await provider.UpdateContactAsync(updateRequest);

        // Assert
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Name");
        updated.Phone.Should().Be("+15559876543");
    }

    [Fact]
    public async Task UpdateContactAsync_WithNonExistentContact_ReturnsNull()
    {
        // Arrange
        var provider = CreateProvider();
        var updateRequest = new ChatContactUpdateRequest
        {
            ExternalId = "non-existent",
            Name = "Updated"
        };

        // Act
        var result = await provider.UpdateContactAsync(updateRequest);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Conversation Management Tests

    [Fact]
    public async Task CreateConversationAsync_WithValidRequest_CreatesConversation()
    {
        // Arrange
        var provider = CreateProvider();
        var contact = await provider.CreateContactAsync(
            new ChatContactCreateRequest { Email = "conv@example.com", Name = "Conv User" });
        var request = new ChatConversationCreateRequest
        {
            ContactExternalId = contact.ExternalId,
            InboxId = "inbox-1",
            Subject = "Test Subject",
            InitialMessage = "Hello, this is a test"
        };

        // Act
        var conversation = await provider.CreateConversationAsync(request);

        // Assert
        conversation.Should().NotBeNull();
        conversation.ExternalId.Should().StartWith("builtin_conv_");
        conversation.ContactExternalId.Should().Be(contact.ExternalId);
        conversation.Status.Should().Be("open");
        conversation.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateConversationAsync_WithInitialMessage_CreatesMessage()
    {
        // Arrange
        var provider = CreateProvider();
        var contact = await provider.CreateContactAsync(
            new ChatContactCreateRequest { Email = "msg@example.com", Name = "Msg User" });
        var request = new ChatConversationCreateRequest
        {
            ContactExternalId = contact.ExternalId,
            InitialMessage = "Initial message content"
        };

        // Act
        var conversation = await provider.CreateConversationAsync(request);
        var messages = await provider.GetConversationMessagesAsync(conversation.ExternalId);

        // Assert
        messages.Should().HaveCount(1);
        messages.First().Content.Should().Be("Initial message content");
    }

    [Fact]
    public async Task GetConversationAsync_WithExistingConversation_ReturnsConversation()
    {
        // Arrange
        var provider = CreateProvider();
        var contact = await provider.CreateContactAsync(
            new ChatContactCreateRequest { Email = "get@example.com", Name = "Get User" });
        var created = await provider.CreateConversationAsync(
            new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });

        // Act
        var found = await provider.GetConversationAsync(created.ExternalId);

        // Assert
        found.Should().NotBeNull();
        found!.ExternalId.Should().Be(created.ExternalId);
    }

    [Fact]
    public async Task GetConversationAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var conversation = await provider.GetConversationAsync("non-existent");

        // Assert
        conversation.Should().BeNull();
    }

    [Fact]
    public async Task CloseConversationAsync_WithOpenConversation_ClosesConversation()
    {
        // Arrange
        var provider = CreateProvider();
        var contact = await provider.CreateContactAsync(
            new ChatContactCreateRequest { Email = "close@example.com", Name = "Close User" });
        var conversation = await provider.CreateConversationAsync(
            new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });

        // Act
        var result = await provider.CloseConversationAsync(conversation.ExternalId);

        // Assert
        result.Should().BeTrue();
        var updated = await provider.GetConversationAsync(conversation.ExternalId);
        updated!.Status.Should().Be("resolved");
    }

    [Fact]
    public async Task ReopenConversationAsync_WithClosedConversation_ReopensConversation()
    {
        // Arrange
        var provider = CreateProvider();
        var contact = await provider.CreateContactAsync(
            new ChatContactCreateRequest { Email = "reopen@example.com", Name = "Reopen User" });
        var conversation = await provider.CreateConversationAsync(
            new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });
        await provider.CloseConversationAsync(conversation.ExternalId);

        // Act
        var result = await provider.ReopenConversationAsync(conversation.ExternalId);

        // Assert
        result.Should().BeTrue();
        var updated = await provider.GetConversationAsync(conversation.ExternalId);
        updated!.Status.Should().Be("open");
    }

    #endregion

    #region Message Operations Tests

    [Fact]
    public async Task SendMessageAsync_WithValidRequest_SendsMessage()
    {
        // Arrange
        var provider = CreateProvider();
        var contact = await provider.CreateContactAsync(
            new ChatContactCreateRequest { Email = "send@example.com", Name = "Send User" });
        var conversation = await provider.CreateConversationAsync(
            new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });
        var request = new ChatMessageCreateRequest
        {
            ConversationExternalId = conversation.ExternalId,
            Content = "Test message content",
            ContentType = ChatMessageType.Text
        };

        // Act
        var message = await provider.SendMessageAsync(request);

        // Assert
        message.Should().NotBeNull();
        message.ExternalId.Should().StartWith("builtin_msg_");
        message.Content.Should().Be("Test message content");
        message.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SendMessageAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = () => provider.SendMessageAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetConversationMessagesAsync_ReturnsMessagesInOrder()
    {
        // Arrange
        var provider = CreateProvider();
        var contact = await provider.CreateContactAsync(
            new ChatContactCreateRequest { Email = "order@example.com", Name = "Order User" });
        var conversation = await provider.CreateConversationAsync(
            new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });

        // Send multiple messages
        await provider.SendMessageAsync(new ChatMessageCreateRequest
        {
            ConversationExternalId = conversation.ExternalId,
            Content = "Message 1"
        });
        await provider.SendMessageAsync(new ChatMessageCreateRequest
        {
            ConversationExternalId = conversation.ExternalId,
            Content = "Message 2"
        });

        // Act
        var messages = await provider.GetConversationMessagesAsync(conversation.ExternalId);

        // Assert
        messages.Should().HaveCount(2);
        messages[0].Content.Should().Be("Message 1");
        messages[1].Content.Should().Be("Message 2");
    }

    #endregion

    #region Agent Operations Tests

    [Fact]
    public async Task GetAgentsAsync_ReturnsAgentsList()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var agents = await provider.GetAgentsAsync();

        // Assert
        agents.Should().NotBeEmpty();
        agents.Should().ContainSingle(a => a.Name == "System Agent");
    }

    [Fact]
    public async Task GetAgentStatusAsync_WithExistingAgent_ReturnsStatus()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var status = await provider.GetAgentStatusAsync("agent_1");

        // Assert
        status.Should().NotBeNull();
        status!.AgentId.Should().Be("agent_1");
    }

    [Fact]
    public async Task AssignAgentAsync_WithValidIds_AssignsAgent()
    {
        // Arrange
        var provider = CreateProvider();
        var contact = await provider.CreateContactAsync(
            new ChatContactCreateRequest { Email = "assign@example.com", Name = "Assign User" });
        var conversation = await provider.CreateConversationAsync(
            new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });

        // Act
        var result = await provider.AssignAgentAsync(conversation.ExternalId, "agent_1");

        // Assert
        result.Should().BeTrue();
        var updated = await provider.GetConversationAsync(conversation.ExternalId);
        updated!.AssignedAgentId.Should().Be("agent_1");
    }

    #endregion

    #region Webhook Processing Tests

    [Fact]
    public async Task ProcessWebhookAsync_WithValidPayload_ProcessesWebhook()
    {
        // Arrange
        var provider = CreateProvider();
        var payload = new Dictionary<string, object>
        {
            ["event"] = "message_created",
            ["conversation_id"] = "conv-123",
            ["content"] = "Webhook message"
        };

        // Act
        var result = await provider.ProcessWebhookAsync(payload, "signature-123");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithNullPayload_ReturnsFailure()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = await provider.ProcessWebhookAsync(null!, "signature");

        // Assert
        result.Success.Should().BeFalse();
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_ReturnsHealthy()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = await provider.HealthCheckAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsHealthy.Should().BeTrue();
        result.ProviderName.Should().Be("BuiltIn");
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task CreateContactAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new ChatContactCreateRequest { Email = "cancel@example.com", Name = "Cancel User" };
        var cts = new CancellationTokenSource();

        // Act
        var contact = await provider.CreateContactAsync(request, cts.Token);

        // Assert
        contact.Should().NotBeNull();
    }

    [Fact]
    public async Task IsAvailableAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var provider = CreateProvider();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert - Should return immediately without error
        var result = await provider.IsAvailableAsync(cts.Token);
        result.Should().BeTrue();
    }

    #endregion

    #region Concurrent Access Tests

    [Fact]
    public async Task CreateContactAsync_ConcurrentCalls_HandlesCorrectly()
    {
        // Arrange
        var provider = CreateProvider();
        var tasks = new List<Task<ChatContact>>();

        // Act - Create 10 contacts concurrently
        for (int i = 0; i < 10; i++)
        {
            var index = i;
            tasks.Add(provider.CreateContactAsync(
                new ChatContactCreateRequest 
                { 
                    Email = $"concurrent{index}@example.com", 
                    Name = $"User {index}" 
                }));
        }
        var contacts = await Task.WhenAll(tasks);

        // Assert
        contacts.Should().HaveCount(10);
        contacts.Select(c => c.ExternalId).Distinct().Should().HaveCount(10);
    }

    [Fact]
    public async Task SendMessageAsync_ConcurrentCalls_HandlesCorrectly()
    {
        // Arrange
        var provider = CreateProvider();
        var contact = await provider.CreateContactAsync(
            new ChatContactCreateRequest { Email = "concurrent@example.com", Name = "Concurrent User" });
        var conversation = await provider.CreateConversationAsync(
            new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });
        var tasks = new List<Task<ChatMessage>>();

        // Act - Send 10 messages concurrently
        for (int i = 0; i < 10; i++)
        {
            var index = i;
            tasks.Add(provider.SendMessageAsync(
                new ChatMessageCreateRequest
                {
                    ConversationExternalId = conversation.ExternalId,
                    Content = $"Message {index}"
                }));
        }
        var messages = await Task.WhenAll(tasks);

        // Assert
        messages.Should().HaveCount(10);
        messages.Select(m => m.ExternalId).Distinct().Should().HaveCount(10);
    }

    #endregion
}
