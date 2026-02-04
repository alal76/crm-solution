// CRM Solution - BuiltInChatProvider Tests
// Phase 3 Week 11: Unit tests for the BuiltIn Chat Provider
// Tests contact management, conversation handling, and messaging

using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CRM.Infrastructure.Providers.BuiltIn;
using CRM.Core.Ports.Output.Providers;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for BuiltInChatProvider.
/// Verifies in-memory storage operations and IChatPort contract compliance.
/// </summary>
public class BuiltInChatProviderTests
{
    private readonly Mock<ILogger<BuiltInChatProvider>> _mockLogger;
    private readonly BuiltInChatProvider _provider;

    public BuiltInChatProviderTests()
    {
        _mockLogger = new Mock<ILogger<BuiltInChatProvider>>();
        _provider = new BuiltInChatProvider(_mockLogger.Object);
    }

    #region Provider Configuration Tests

    [Fact]
    public void ProviderName_Should_Return_BuiltIn()
    {
        // Assert
        Assert.Equal("BuiltIn", _provider.ProviderName);
    }

    [Fact]
    public async Task IsAvailableAsync_Should_Return_True()
    {
        // Act
        var result = await _provider.IsAvailableAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HealthCheckAsync_Should_Return_Healthy()
    {
        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        Assert.True(result.IsHealthy);
        Assert.Equal("BuiltIn", result.ProviderName);
        Assert.Contains("type", result.Details.Keys);
        Assert.Equal("builtin", result.Details["type"]);
    }

    #endregion

    #region Contact Management Tests

    [Fact]
    public async Task CreateContactAsync_Should_Create_Contact_With_Generated_Id()
    {
        // Arrange
        var request = new ChatContactCreateRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Phone = "+1234567890",
            CrmContactId = 123
        };

        // Act
        var contact = await _provider.CreateContactAsync(request);

        // Assert
        Assert.NotNull(contact);
        Assert.StartsWith("builtin_contact_", contact.ExternalId);
        Assert.Equal("John Doe", contact.Name);
        Assert.Equal("john@example.com", contact.Email);
        Assert.Equal("+1234567890", contact.Phone);
        Assert.Equal(123, contact.CrmContactId);
    }

    [Fact]
    public async Task GetContactAsync_Should_Return_Created_Contact()
    {
        // Arrange
        var request = new ChatContactCreateRequest
        {
            Name = "Jane Doe",
            Email = "jane@example.com"
        };
        var createdContact = await _provider.CreateContactAsync(request);

        // Act
        var contact = await _provider.GetContactAsync(createdContact.ExternalId);

        // Assert
        Assert.NotNull(contact);
        Assert.Equal(createdContact.ExternalId, contact.ExternalId);
        Assert.Equal("Jane Doe", contact.Name);
    }

    [Fact]
    public async Task GetContactAsync_Should_Return_Null_For_Unknown_Id()
    {
        // Act
        var contact = await _provider.GetContactAsync("unknown_id");

        // Assert
        Assert.Null(contact);
    }

    [Fact]
    public async Task FindContactByEmailAsync_Should_Find_Contact_Case_Insensitive()
    {
        // Arrange
        var request = new ChatContactCreateRequest
        {
            Name = "Email Test",
            Email = "Test@Example.com"
        };
        await _provider.CreateContactAsync(request);

        // Act
        var contact = await _provider.FindContactByEmailAsync("test@example.com");

        // Assert
        Assert.NotNull(contact);
        Assert.Equal("Email Test", contact.Name);
    }

    [Fact]
    public async Task FindContactByPhoneAsync_Should_Find_Contact_With_Normalized_Phone()
    {
        // Arrange
        var request = new ChatContactCreateRequest
        {
            Name = "Phone Test",
            Phone = "+1 (234) 567-8901"
        };
        await _provider.CreateContactAsync(request);

        // Act
        var contact = await _provider.FindContactByPhoneAsync("12345678901");

        // Assert
        Assert.NotNull(contact);
        Assert.Equal("Phone Test", contact.Name);
    }

    [Fact]
    public async Task UpdateContactAsync_Should_Update_Contact_Fields()
    {
        // Arrange
        var createRequest = new ChatContactCreateRequest
        {
            Name = "Original Name",
            Email = "original@example.com"
        };
        var contact = await _provider.CreateContactAsync(createRequest);

        var updateRequest = new ChatContactUpdateRequest
        {
            Name = "Updated Name",
            Email = "updated@example.com"
        };

        // Act
        await _provider.UpdateContactAsync(contact.ExternalId, updateRequest);
        var updatedContact = await _provider.GetContactAsync(contact.ExternalId);

        // Assert
        Assert.NotNull(updatedContact);
        Assert.Equal("Updated Name", updatedContact.Name);
        Assert.Equal("updated@example.com", updatedContact.Email);
        Assert.NotNull(updatedContact.LastActivityAt);
    }

    #endregion

    #region Conversation Management Tests

    [Fact]
    public async Task CreateConversationAsync_Should_Create_Conversation()
    {
        // Arrange
        var contact = await _provider.CreateContactAsync(new ChatContactCreateRequest { Name = "Test User" });
        var request = new ChatConversationCreateRequest
        {
            ContactExternalId = contact.ExternalId,
            Channel = "web",
            Subject = "Help needed"
        };

        // Act
        var conversation = await _provider.CreateConversationAsync(request);

        // Assert
        Assert.NotNull(conversation);
        Assert.StartsWith("builtin_conv_", conversation.ExternalId);
        Assert.Equal(contact.ExternalId, conversation.ContactExternalId);
        Assert.Equal("web", conversation.Channel);
        Assert.Equal("Help needed", conversation.Subject);
        Assert.Equal("open", conversation.Status);
    }

    [Fact]
    public async Task CreateConversationAsync_With_InitialMessage_Should_Include_Message()
    {
        // Arrange
        var contact = await _provider.CreateContactAsync(new ChatContactCreateRequest { Name = "Test User" });
        var request = new ChatConversationCreateRequest
        {
            ContactExternalId = contact.ExternalId,
            InitialMessage = "Hello, I need help!"
        };

        // Act
        var conversation = await _provider.CreateConversationAsync(request);
        var messages = await _provider.GetMessagesAsync(conversation.ExternalId);

        // Assert
        Assert.Equal(1, conversation.MessageCount);
        Assert.Single(messages);
        Assert.Equal("Hello, I need help!", messages.First().Content);
        Assert.Equal("contact", messages.First().SenderType);
    }

    [Fact]
    public async Task GetConversationAsync_Should_Return_Conversation_With_Recent_Messages()
    {
        // Arrange
        var contact = await _provider.CreateContactAsync(new ChatContactCreateRequest { Name = "Test User" });
        var conv = await _provider.CreateConversationAsync(new ChatConversationCreateRequest
        {
            ContactExternalId = contact.ExternalId,
            InitialMessage = "Initial message"
        });

        // Act
        var conversation = await _provider.GetConversationAsync(conv.ExternalId);

        // Assert
        Assert.NotNull(conversation);
        Assert.NotNull(conversation.RecentMessages);
        Assert.Single(conversation.RecentMessages);
    }

    [Fact]
    public async Task GetContactConversationsAsync_Should_Return_Contact_Conversations()
    {
        // Arrange
        var contact = await _provider.CreateContactAsync(new ChatContactCreateRequest { Name = "Test User" });
        await _provider.CreateConversationAsync(new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });
        await _provider.CreateConversationAsync(new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });

        // Act
        var conversations = await _provider.GetContactConversationsAsync(contact.ExternalId);

        // Assert
        Assert.Equal(2, conversations.Count());
    }

    [Fact]
    public async Task GetContactConversationsAsync_With_Status_Filter_Should_Filter_Results()
    {
        // Arrange
        var contact = await _provider.CreateContactAsync(new ChatContactCreateRequest { Name = "Test User" });
        var conv1 = await _provider.CreateConversationAsync(new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });
        var conv2 = await _provider.CreateConversationAsync(new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });
        await _provider.ResolveConversationAsync(conv2.ExternalId);

        // Act
        var openConversations = await _provider.GetContactConversationsAsync(contact.ExternalId, "open");
        var resolvedConversations = await _provider.GetContactConversationsAsync(contact.ExternalId, "resolved");

        // Assert
        Assert.Single(openConversations);
        Assert.Single(resolvedConversations);
    }

    #endregion

    #region Messaging Tests

    [Fact]
    public async Task SendMessageAsync_Should_Send_Message()
    {
        // Arrange
        var contact = await _provider.CreateContactAsync(new ChatContactCreateRequest { Name = "Test User" });
        var conv = await _provider.CreateConversationAsync(new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });
        var request = new ChatMessageCreateRequest
        {
            Content = "Hello from agent!",
            ContentType = "text",
            IsPrivate = false
        };

        // Act
        var message = await _provider.SendMessageAsync(conv.ExternalId, request);

        // Assert
        Assert.NotNull(message);
        Assert.StartsWith("builtin_msg_", message.ExternalId);
        Assert.Equal("Hello from agent!", message.Content);
        Assert.Equal("agent", message.SenderType);
        Assert.Equal("System Agent", message.SenderName);
    }

    [Fact]
    public async Task SendMessageAsync_Should_Update_Conversation_Stats()
    {
        // Arrange
        var contact = await _provider.CreateContactAsync(new ChatContactCreateRequest { Name = "Test User" });
        var conv = await _provider.CreateConversationAsync(new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });

        // Act
        await _provider.SendMessageAsync(conv.ExternalId, new ChatMessageCreateRequest { Content = "Message 1" });
        await _provider.SendMessageAsync(conv.ExternalId, new ChatMessageCreateRequest { Content = "Message 2" });
        var conversation = await _provider.GetConversationAsync(conv.ExternalId);

        // Assert
        Assert.Equal(2, conversation!.MessageCount);
        Assert.NotNull(conversation.LastMessageAt);
    }

    [Fact]
    public async Task GetMessagesAsync_Should_Return_Messages_In_Order()
    {
        // Arrange
        var contact = await _provider.CreateContactAsync(new ChatContactCreateRequest { Name = "Test User" });
        var conv = await _provider.CreateConversationAsync(new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });
        
        await _provider.SendMessageAsync(conv.ExternalId, new ChatMessageCreateRequest { Content = "Message 1" });
        await _provider.SendMessageAsync(conv.ExternalId, new ChatMessageCreateRequest { Content = "Message 2" });
        await _provider.SendMessageAsync(conv.ExternalId, new ChatMessageCreateRequest { Content = "Message 3" });

        // Act
        var messages = (await _provider.GetMessagesAsync(conv.ExternalId)).ToList();

        // Assert
        Assert.Equal(3, messages.Count);
        Assert.Equal("Message 1", messages[0].Content);
        Assert.Equal("Message 2", messages[1].Content);
        Assert.Equal("Message 3", messages[2].Content);
    }

    [Fact]
    public async Task GetMessagesAsync_With_AfterMessageId_Should_Return_Subsequent_Messages()
    {
        // Arrange
        var contact = await _provider.CreateContactAsync(new ChatContactCreateRequest { Name = "Test User" });
        var conv = await _provider.CreateConversationAsync(new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });
        
        var msg1 = await _provider.SendMessageAsync(conv.ExternalId, new ChatMessageCreateRequest { Content = "Message 1" });
        await _provider.SendMessageAsync(conv.ExternalId, new ChatMessageCreateRequest { Content = "Message 2" });
        await _provider.SendMessageAsync(conv.ExternalId, new ChatMessageCreateRequest { Content = "Message 3" });

        // Act
        var messages = (await _provider.GetMessagesAsync(conv.ExternalId, msg1.ExternalId)).ToList();

        // Assert
        Assert.Equal(2, messages.Count);
        Assert.Equal("Message 2", messages[0].Content);
        Assert.Equal("Message 3", messages[1].Content);
    }

    [Fact]
    public async Task SendMessageAsync_With_Invalid_Conversation_Should_Throw()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _provider.SendMessageAsync("invalid_conv", new ChatMessageCreateRequest { Content = "Test" }));
    }

    #endregion

    #region Conversation Status Tests

    [Fact]
    public async Task ResolveConversationAsync_Should_Set_Status_To_Resolved()
    {
        // Arrange
        var contact = await _provider.CreateContactAsync(new ChatContactCreateRequest { Name = "Test User" });
        var conv = await _provider.CreateConversationAsync(new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });

        // Act
        await _provider.ResolveConversationAsync(conv.ExternalId);
        var conversation = await _provider.GetConversationAsync(conv.ExternalId);

        // Assert
        Assert.Equal("resolved", conversation!.Status);
    }

    [Fact]
    public async Task ReopenConversationAsync_Should_Set_Status_To_Open()
    {
        // Arrange
        var contact = await _provider.CreateContactAsync(new ChatContactCreateRequest { Name = "Test User" });
        var conv = await _provider.CreateConversationAsync(new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });
        await _provider.ResolveConversationAsync(conv.ExternalId);

        // Act
        await _provider.ReopenConversationAsync(conv.ExternalId);
        var conversation = await _provider.GetConversationAsync(conv.ExternalId);

        // Assert
        Assert.Equal("open", conversation!.Status);
    }

    #endregion

    #region Agent Operations Tests

    [Fact]
    public async Task GetAgentsAsync_Should_Return_Default_Agent()
    {
        // Act
        var agents = await _provider.GetAgentsAsync();

        // Assert
        Assert.Single(agents);
        Assert.Equal("agent_1", agents.First().ExternalId);
        Assert.Equal("System Agent", agents.First().Name);
        Assert.Equal("online", agents.First().Status);
    }

    [Fact]
    public async Task AssignAgentAsync_Should_Assign_Agent_To_Conversation()
    {
        // Arrange
        var contact = await _provider.CreateContactAsync(new ChatContactCreateRequest { Name = "Test User" });
        var conv = await _provider.CreateConversationAsync(new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });

        // Act
        await _provider.AssignAgentAsync(conv.ExternalId, "agent_1");
        var conversation = await _provider.GetConversationAsync(conv.ExternalId);

        // Assert
        Assert.Equal("agent_1", conversation!.AssignedAgentId);
        Assert.Equal("System Agent", conversation.AssignedAgentName);
    }

    [Fact]
    public async Task GetAgentStatusAsync_Should_Return_Agent_Status()
    {
        // Act
        var agent = await _provider.GetAgentStatusAsync("agent_1");

        // Assert
        Assert.NotNull(agent);
        Assert.Equal("online", agent.Status);
    }

    [Fact]
    public async Task GetAgentStatusAsync_Should_Return_Null_For_Unknown_Agent()
    {
        // Act
        var agent = await _provider.GetAgentStatusAsync("unknown_agent");

        // Assert
        Assert.Null(agent);
    }

    #endregion

    #region Webhook Tests

    [Fact]
    public async Task ProcessWebhookAsync_Should_Return_Not_Supported()
    {
        // Act
        var result = await _provider.ProcessWebhookAsync("message_created", "{}");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("does not support webhooks", result.Error);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task CreateContactAsync_Should_Throw_For_Null_Request()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _provider.CreateContactAsync(null!));
    }

    [Fact]
    public async Task GetContactAsync_Should_Throw_For_Empty_Id()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _provider.GetContactAsync(""));
    }

    [Fact]
    public async Task FindContactByEmailAsync_Should_Throw_For_Empty_Email()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _provider.FindContactByEmailAsync(""));
    }

    [Fact]
    public async Task CreateConversationAsync_Should_Throw_For_Empty_ContactId()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _provider.CreateConversationAsync(new ChatConversationCreateRequest { ContactExternalId = "" }));
    }

    [Fact]
    public async Task SendMessageAsync_Should_Throw_For_Empty_Content()
    {
        // Arrange
        var contact = await _provider.CreateContactAsync(new ChatContactCreateRequest { Name = "Test User" });
        var conv = await _provider.CreateConversationAsync(new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _provider.SendMessageAsync(conv.ExternalId, new ChatMessageCreateRequest { Content = "" }));
    }

    #endregion

    #region Internal Helper Tests

    [Fact]
    public void SimulateCustomerMessage_Should_Add_Message_With_Contact_Sender()
    {
        // Arrange
        var contact = _provider.CreateContactAsync(new ChatContactCreateRequest { Name = "Test User" }).Result;
        var conv = _provider.CreateConversationAsync(new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId }).Result;

        // Act
        var message = _provider.SimulateCustomerMessage(conv.ExternalId, "Hello from customer!");

        // Assert
        Assert.Equal("contact", message.SenderType);
        Assert.Equal("Hello from customer!", message.Content);
    }

    [Fact]
    public void ClearAll_Should_Remove_All_Data()
    {
        // Arrange
        _provider.CreateContactAsync(new ChatContactCreateRequest { Name = "Test User" }).Wait();
        _provider.CreateContactAsync(new ChatContactCreateRequest { Name = "Test User 2" }).Wait();

        // Act
        _provider.ClearAll();
        
        // Create new contact to verify counter reset
        var newContact = _provider.CreateContactAsync(new ChatContactCreateRequest { Name = "New User" }).Result;

        // Assert
        Assert.Equal("builtin_contact_1", newContact.ExternalId);
    }

    #endregion
}
