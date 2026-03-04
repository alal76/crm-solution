// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.BuiltIn;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for BuiltInChatProvider.
/// Covers all public operations: contact CRUD, conversation lifecycle,
/// messaging, agent assignment, webhook rejection, and health checks.
///
/// MANDATORY pre-write verification:
///   Class     : BuiltInChatProvider
///   Namespace : CRM.Infrastructure.Providers.BuiltIn
///   Constructor: (ILogger&lt;BuiltInChatProvider&gt; logger)
///   ProviderName: "BuiltIn"
///   Storage   : ConcurrentDictionary (in-memory, per-instance)
///   Source read: verified 2026-03-03
/// </summary>
public class BuiltInChatProviderTests
{
    // ── Factory helper ───────────────────────────────────────────────────────

    private static BuiltInChatProvider CreateProvider(
        Mock<ILogger<BuiltInChatProvider>>? loggerMock = null)
    {
        loggerMock ??= new Mock<ILogger<BuiltInChatProvider>>();
        return new BuiltInChatProvider(loggerMock.Object);
    }

    /// <summary>
    /// Convenience: creates a contact and returns both the provider and the created contact.
    /// </summary>
    private static async Task<(BuiltInChatProvider provider, ChatContact contact)> CreateProviderWithContactAsync(
        string name = "Alice Smith",
        string? email = "alice@example.com",
        string? phone = "+15550001111")
    {
        var provider = CreateProvider();
        var contact = await provider.CreateContactAsync(new ChatContactCreateRequest
        {
            Name = name,
            Email = email,
            Phone = phone
        });
        return (provider, contact);
    }

    // ── Constructor Guards ───────────────────────────────────────────────────

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange & Act
        var act = () => new BuiltInChatProvider(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_InitializesWithDefaultSystemAgent()
    {
        // Arrange & Act
        var provider = CreateProvider();

        // Assert - the provider initializes with a default "System Agent"
        // verified by GetAgentsAsync() returning it immediately
        var agents = provider.GetAgentsAsync().Result;
        agents.Should().NotBeEmpty();
        agents.Should().ContainSingle(a => a.Name == "System Agent");
    }

    // ── Provider Metadata ────────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsBuiltIn()
    {
        // Arrange
        var provider = CreateProvider();

        // Assert
        provider.ProviderName.Should().Be("BuiltIn");
    }

    [Fact]
    public async Task IsAvailableAsync_AlwaysReturnsTrue()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = await provider.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    // ── CreateContactAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task CreateContactAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = async () => await provider.CreateContactAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateContactAsync_ReturnsContactWithGeneratedExternalId()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new ChatContactCreateRequest
        {
            Name = "Bob Jones",
            Email = "bob@example.com",
            Phone = "+15550002222",
            CrmContactId = 101,
            CrmAccountId = 50
        };

        // Act
        var contact = await provider.CreateContactAsync(request);

        // Assert
        contact.Should().NotBeNull();
        contact.ExternalId.Should().StartWith("builtin_contact_");
        contact.Name.Should().Be("Bob Jones");
        contact.Email.Should().Be("bob@example.com");
        contact.Phone.Should().Be("+15550002222");
        contact.CrmContactId.Should().Be(101);
        contact.CrmAccountId.Should().Be(50);
        contact.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateContactAsync_GeneratesUniqueIds_ForMultipleContacts()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var c1 = await provider.CreateContactAsync(new ChatContactCreateRequest { Name = "One" });
        var c2 = await provider.CreateContactAsync(new ChatContactCreateRequest { Name = "Two" });

        // Assert
        c1.ExternalId.Should().NotBe(c2.ExternalId);
    }

    // ── GetContactAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetContactAsync_ThrowsArgumentException_WhenExternalIdIsEmpty()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = async () => await provider.GetContactAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("externalId");
    }

    [Fact]
    public async Task GetContactAsync_ReturnsContact_WhenContactExists()
    {
        // Arrange
        var (provider, created) = await CreateProviderWithContactAsync();

        // Act
        var found = await provider.GetContactAsync(created.ExternalId);

        // Assert
        found.Should().NotBeNull();
        found!.ExternalId.Should().Be(created.ExternalId);
        found.Name.Should().Be("Alice Smith");
    }

    [Fact]
    public async Task GetContactAsync_ReturnsNull_WhenContactDoesNotExist()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var found = await provider.GetContactAsync("builtin_contact_9999");

        // Assert
        found.Should().BeNull();
    }

    // ── FindContactByEmailAsync ──────────────────────────────────────────────

    [Fact]
    public async Task FindContactByEmailAsync_ThrowsArgumentException_WhenEmailIsEmpty()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = async () => await provider.FindContactByEmailAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("email");
    }

    [Fact]
    public async Task FindContactByEmailAsync_ReturnsContact_WhenEmailMatches()
    {
        // Arrange
        var (provider, created) = await CreateProviderWithContactAsync(email: "alice@example.com");

        // Act
        var found = await provider.FindContactByEmailAsync("alice@example.com");

        // Assert
        found.Should().NotBeNull();
        found!.ExternalId.Should().Be(created.ExternalId);
    }

    [Fact]
    public async Task FindContactByEmailAsync_IsCaseInsensitive()
    {
        // Arrange
        var (provider, _) = await CreateProviderWithContactAsync(email: "alice@example.com");

        // Act
        var found = await provider.FindContactByEmailAsync("ALICE@EXAMPLE.COM");

        // Assert
        found.Should().NotBeNull();
    }

    [Fact]
    public async Task FindContactByEmailAsync_ReturnsNull_WhenEmailNotFound()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var found = await provider.FindContactByEmailAsync("nobody@nowhere.com");

        // Assert
        found.Should().BeNull();
    }

    // ── FindContactByPhoneAsync ──────────────────────────────────────────────

    [Fact]
    public async Task FindContactByPhoneAsync_ThrowsArgumentException_WhenPhoneIsEmpty()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = async () => await provider.FindContactByPhoneAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("phone");
    }

    [Fact]
    public async Task FindContactByPhoneAsync_ReturnsContact_WhenPhoneMatchesNormalized()
    {
        // Arrange - stored as +15550001111, search with spaces/dashes
        var (provider, _) = await CreateProviderWithContactAsync(phone: "+15550001111");

        // Act - phone normalization strips non-digits, so these should match
        var found = await provider.FindContactByPhoneAsync("+1-555-000-1111");

        // Assert
        found.Should().NotBeNull();
    }

    // ── UpdateContactAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task UpdateContactAsync_ThrowsArgumentException_WhenExternalIdIsEmpty()
    {
        // Arrange
        var provider = CreateProvider();
        var update = new ChatContactUpdateRequest { Name = "New Name" };

        // Act
        var act = async () => await provider.UpdateContactAsync(string.Empty, update);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("externalId");
    }

    [Fact]
    public async Task UpdateContactAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var (provider, contact) = await CreateProviderWithContactAsync();

        // Act
        var act = async () => await provider.UpdateContactAsync(contact.ExternalId, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateContactAsync_UpdatesFields_WhenContactExists()
    {
        // Arrange
        var (provider, contact) = await CreateProviderWithContactAsync();
        var update = new ChatContactUpdateRequest
        {
            Name = "Alice Updated",
            Email = "updated@example.com"
        };

        // Act
        await provider.UpdateContactAsync(contact.ExternalId, update);
        var refreshed = await provider.GetContactAsync(contact.ExternalId);

        // Assert
        refreshed.Should().NotBeNull();
        refreshed!.Name.Should().Be("Alice Updated");
        refreshed.Email.Should().Be("updated@example.com");
        refreshed.LastActivityAt.Should().HaveValue();
    }

    [Fact]
    public async Task UpdateContactAsync_CompletesSuccessfully_WhenContactDoesNotExist()
    {
        // Arrange - update on missing contact logs warning but doesn't throw
        var provider = CreateProvider();
        var update = new ChatContactUpdateRequest { Name = "Ghost" };

        // Act
        var act = async () => await provider.UpdateContactAsync("builtin_contact_9999", update);

        // Assert
        await act.Should().NotThrowAsync();
    }

    // ── CreateConversationAsync ──────────────────────────────────────────────

    [Fact]
    public async Task CreateConversationAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = async () => await provider.CreateConversationAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateConversationAsync_ThrowsArgumentException_WhenContactExternalIdIsEmpty()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new ChatConversationCreateRequest
        {
            ContactExternalId = string.Empty,
            Channel = "web"
        };

        // Act
        var act = async () => await provider.CreateConversationAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("request");
    }

    [Fact]
    public async Task CreateConversationAsync_ReturnsConversation_WithStatusOpen()
    {
        // Arrange
        var (provider, contact) = await CreateProviderWithContactAsync();
        var request = new ChatConversationCreateRequest
        {
            ContactExternalId = contact.ExternalId,
            Channel = "web",
            Subject = "Support Query"
        };

        // Act
        var conversation = await provider.CreateConversationAsync(request);

        // Assert
        conversation.ExternalId.Should().StartWith("builtin_conv_");
        conversation.ContactExternalId.Should().Be(contact.ExternalId);
        conversation.Status.Should().Be("open");
        conversation.Channel.Should().Be("web");
        conversation.Subject.Should().Be("Support Query");
    }

    [Fact]
    public async Task CreateConversationAsync_SetsInitialMessage_WhenProvided()
    {
        // Arrange
        var (provider, contact) = await CreateProviderWithContactAsync();
        var request = new ChatConversationCreateRequest
        {
            ContactExternalId = contact.ExternalId,
            InitialMessage = "Hello, I need help"
        };

        // Act
        var conversation = await provider.CreateConversationAsync(request);

        // Assert
        conversation.MessageCount.Should().Be(1);
        conversation.UnreadCount.Should().Be(1);
        conversation.LastMessageAt.Should().HaveValue();
    }

    // ── GetConversationAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetConversationAsync_ThrowsArgumentException_WhenConversationIdIsEmpty()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = async () => await provider.GetConversationAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("conversationId");
    }

    [Fact]
    public async Task GetConversationAsync_ReturnsConversation_WhenExists()
    {
        // Arrange
        var (provider, contact) = await CreateProviderWithContactAsync();
        var conv = await provider.CreateConversationAsync(new ChatConversationCreateRequest
        {
            ContactExternalId = contact.ExternalId
        });

        // Act
        var found = await provider.GetConversationAsync(conv.ExternalId);

        // Assert
        found.Should().NotBeNull();
        found!.ExternalId.Should().Be(conv.ExternalId);
    }

    [Fact]
    public async Task GetConversationAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var found = await provider.GetConversationAsync("builtin_conv_9999");

        // Assert
        found.Should().BeNull();
    }

    // ── GetContactConversationsAsync ─────────────────────────────────────────

    [Fact]
    public async Task GetContactConversationsAsync_ThrowsArgumentException_WhenContactIdIsEmpty()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = async () => await provider.GetContactConversationsAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("contactExternalId");
    }

    [Fact]
    public async Task GetContactConversationsAsync_ReturnsAllConversations_ForContact()
    {
        // Arrange
        var (provider, contact) = await CreateProviderWithContactAsync();
        await provider.CreateConversationAsync(new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });
        await provider.CreateConversationAsync(new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });

        // Act
        var conversations = (await provider.GetContactConversationsAsync(contact.ExternalId)).ToList();

        // Assert
        conversations.Should().HaveCount(2);
        conversations.Should().AllSatisfy(c => c.ContactExternalId.Should().Be(contact.ExternalId));
    }

    [Fact]
    public async Task GetContactConversationsAsync_FiltersbyStatus_WhenStatusProvided()
    {
        // Arrange
        var (provider, contact) = await CreateProviderWithContactAsync();
        var conv = await provider.CreateConversationAsync(new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId });
        await provider.ResolveConversationAsync(conv.ExternalId);
        await provider.CreateConversationAsync(new ChatConversationCreateRequest { ContactExternalId = contact.ExternalId }); // open

        // Act
        var openConvs = (await provider.GetContactConversationsAsync(contact.ExternalId, status: "open")).ToList();
        var resolvedConvs = (await provider.GetContactConversationsAsync(contact.ExternalId, status: "resolved")).ToList();

        // Assert
        openConvs.Should().HaveCount(1);
        resolvedConvs.Should().HaveCount(1);
    }

    // ── SendMessageAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task SendMessageAsync_ThrowsArgumentException_WhenConversationIdIsEmpty()
    {
        // Arrange
        var provider = CreateProvider();
        var request = new ChatMessageCreateRequest { Content = "Hello" };

        // Act
        var act = async () => await provider.SendMessageAsync(string.Empty, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("conversationId");
    }

    [Fact]
    public async Task SendMessageAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = async () => await provider.SendMessageAsync("conv_1", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendMessageAsync_ThrowsArgumentException_WhenContentIsEmpty()
    {
        // Arrange
        var (provider, contact) = await CreateProviderWithContactAsync();
        var conv = await provider.CreateConversationAsync(new ChatConversationCreateRequest
        {
            ContactExternalId = contact.ExternalId
        });

        // Act
        var act = async () => await provider.SendMessageAsync(
            conv.ExternalId,
            new ChatMessageCreateRequest { Content = string.Empty });

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("request");
    }

    [Fact]
    public async Task SendMessageAsync_ThrowsInvalidOperationException_WhenConversationNotFound()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = async () => await provider.SendMessageAsync(
            "builtin_conv_9999",
            new ChatMessageCreateRequest { Content = "Hello" });

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*builtin_conv_9999*");
    }

    [Fact]
    public async Task SendMessageAsync_ReturnsMessage_WithAgentSenderType()
    {
        // Arrange
        var (provider, contact) = await CreateProviderWithContactAsync();
        var conv = await provider.CreateConversationAsync(new ChatConversationCreateRequest
        {
            ContactExternalId = contact.ExternalId
        });

        // Act
        var message = await provider.SendMessageAsync(
            conv.ExternalId,
            new ChatMessageCreateRequest { Content = "How can I help you?" });

        // Assert
        message.ExternalId.Should().StartWith("builtin_msg_");
        message.Content.Should().Be("How can I help you?");
        message.SenderType.Should().Be("agent");
        message.ConversationId.Should().Be(conv.ExternalId);
        message.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SendMessageAsync_UpdatesConversationStats()
    {
        // Arrange
        var (provider, contact) = await CreateProviderWithContactAsync();
        var conv = await provider.CreateConversationAsync(new ChatConversationCreateRequest
        {
            ContactExternalId = contact.ExternalId
        });
        var initialCount = conv.MessageCount;

        // Act
        await provider.SendMessageAsync(conv.ExternalId, new ChatMessageCreateRequest { Content = "First" });
        await provider.SendMessageAsync(conv.ExternalId, new ChatMessageCreateRequest { Content = "Second" });
        var refreshed = await provider.GetConversationAsync(conv.ExternalId);

        // Assert
        refreshed!.MessageCount.Should().Be(initialCount + 2);
        refreshed.LastMessageAt.Should().HaveValue();
    }

    // ── GetMessagesAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetMessagesAsync_ThrowsArgumentException_WhenConversationIdIsEmpty()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = async () => await provider.GetMessagesAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("conversationId");
    }

    [Fact]
    public async Task GetMessagesAsync_ReturnsEmptyList_WhenConversationHasNoMessages()
    {
        // Arrange - conversation created without initial message
        var (provider, contact) = await CreateProviderWithContactAsync();
        var conv = await provider.CreateConversationAsync(new ChatConversationCreateRequest
        {
            ContactExternalId = contact.ExternalId
        });

        // Act
        var messages = (await provider.GetMessagesAsync(conv.ExternalId)).ToList();

        // Assert
        messages.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMessagesAsync_ReturnsAllMessages_WhenNoAfterIdProvided()
    {
        // Arrange
        var (provider, contact) = await CreateProviderWithContactAsync();
        var conv = await provider.CreateConversationAsync(new ChatConversationCreateRequest
        {
            ContactExternalId = contact.ExternalId
        });
        await provider.SendMessageAsync(conv.ExternalId, new ChatMessageCreateRequest { Content = "Msg1" });
        await provider.SendMessageAsync(conv.ExternalId, new ChatMessageCreateRequest { Content = "Msg2" });
        await provider.SendMessageAsync(conv.ExternalId, new ChatMessageCreateRequest { Content = "Msg3" });

        // Act
        var messages = (await provider.GetMessagesAsync(conv.ExternalId)).ToList();

        // Assert
        messages.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetMessagesAsync_RespectsLimit_WhenLimitIsSet()
    {
        // Arrange
        var (provider, contact) = await CreateProviderWithContactAsync();
        var conv = await provider.CreateConversationAsync(new ChatConversationCreateRequest
        {
            ContactExternalId = contact.ExternalId
        });
        for (var i = 0; i < 10; i++)
        {
            await provider.SendMessageAsync(conv.ExternalId, new ChatMessageCreateRequest { Content = $"Msg{i}" });
        }

        // Act
        var messages = (await provider.GetMessagesAsync(conv.ExternalId, limit: 3)).ToList();

        // Assert
        messages.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetMessagesAsync_ReturnsEmptyEnumerable_WhenConversationDoesNotExist()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var messages = (await provider.GetMessagesAsync("builtin_conv_9999")).ToList();

        // Assert
        messages.Should().BeEmpty();
    }

    // ── ResolveConversationAsync ─────────────────────────────────────────────

    [Fact]
    public async Task ResolveConversationAsync_ThrowsArgumentException_WhenConversationIdIsEmpty()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = async () => await provider.ResolveConversationAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("conversationId");
    }

    [Fact]
    public async Task ResolveConversationAsync_SetsStatusToResolved()
    {
        // Arrange
        var (provider, contact) = await CreateProviderWithContactAsync();
        var conv = await provider.CreateConversationAsync(new ChatConversationCreateRequest
        {
            ContactExternalId = contact.ExternalId
        });

        // Act
        await provider.ResolveConversationAsync(conv.ExternalId);
        var resolved = await provider.GetConversationAsync(conv.ExternalId);

        // Assert
        resolved!.Status.Should().Be("resolved");
    }

    [Fact]
    public async Task ResolveConversationAsync_CompletesSuccessfully_WhenConversationNotFound()
    {
        // Arrange - logs warning, does not throw
        var provider = CreateProvider();

        // Act
        var act = async () => await provider.ResolveConversationAsync("builtin_conv_9999");

        // Assert
        await act.Should().NotThrowAsync();
    }

    // ── ReopenConversationAsync ──────────────────────────────────────────────

    [Fact]
    public async Task ReopenConversationAsync_ThrowsArgumentException_WhenConversationIdIsEmpty()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = async () => await provider.ReopenConversationAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("conversationId");
    }

    [Fact]
    public async Task ReopenConversationAsync_SetsStatusBackToOpen_AfterResolution()
    {
        // Arrange
        var (provider, contact) = await CreateProviderWithContactAsync();
        var conv = await provider.CreateConversationAsync(new ChatConversationCreateRequest
        {
            ContactExternalId = contact.ExternalId
        });
        await provider.ResolveConversationAsync(conv.ExternalId);

        // Act
        await provider.ReopenConversationAsync(conv.ExternalId);
        var reopened = await provider.GetConversationAsync(conv.ExternalId);

        // Assert
        reopened!.Status.Should().Be("open");
    }

    // ── AssignAgentAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task AssignAgentAsync_ThrowsArgumentException_WhenConversationIdIsEmpty()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = async () => await provider.AssignAgentAsync(string.Empty, "agent_1");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("conversationId");
    }

    [Fact]
    public async Task AssignAgentAsync_ThrowsArgumentException_WhenAgentExternalIdIsEmpty()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = async () => await provider.AssignAgentAsync("conv_1", string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("agentExternalId");
    }

    [Fact]
    public async Task AssignAgentAsync_ThrowsInvalidOperationException_WhenConversationNotFound()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = async () => await provider.AssignAgentAsync("builtin_conv_9999", "agent_1");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*builtin_conv_9999*");
    }

    [Fact]
    public async Task AssignAgentAsync_AssignsNamedAgent_WhenAgentExistsInSystem()
    {
        // Arrange - default agent "agent_1" added by constructor
        var (provider, contact) = await CreateProviderWithContactAsync();
        var conv = await provider.CreateConversationAsync(new ChatConversationCreateRequest
        {
            ContactExternalId = contact.ExternalId
        });

        // Act
        await provider.AssignAgentAsync(conv.ExternalId, "agent_1");
        var refreshed = await provider.GetConversationAsync(conv.ExternalId);

        // Assert
        refreshed!.AssignedAgentId.Should().Be("agent_1");
        refreshed.AssignedAgentName.Should().Be("System Agent");
    }

    [Fact]
    public async Task AssignAgentAsync_SetsAgentId_EvenWhenAgentIsUnknown()
    {
        // Arrange - agents not in dict are still assigned by id
        var (provider, contact) = await CreateProviderWithContactAsync();
        var conv = await provider.CreateConversationAsync(new ChatConversationCreateRequest
        {
            ContactExternalId = contact.ExternalId
        });

        // Act
        await provider.AssignAgentAsync(conv.ExternalId, "agent_unknown_99");
        var refreshed = await provider.GetConversationAsync(conv.ExternalId);

        // Assert
        refreshed!.AssignedAgentId.Should().Be("agent_unknown_99");
    }

    // ── GetAgentsAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetAgentsAsync_ReturnsDefaultSystemAgent_OnFreshProvider()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var agents = (await provider.GetAgentsAsync()).ToList();

        // Assert
        agents.Should().HaveCount(1);
        agents[0].ExternalId.Should().Be("agent_1");
        agents[0].Name.Should().Be("System Agent");
        agents[0].Email.Should().Be("agent@crm.local");
        agents[0].Status.Should().Be("online");
    }

    // ── GetAgentStatusAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetAgentStatusAsync_ThrowsArgumentException_WhenAgentIdIsEmpty()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var act = async () => await provider.GetAgentStatusAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("agentExternalId");
    }

    [Fact]
    public async Task GetAgentStatusAsync_ReturnsAgent_WhenAgentExists()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var agent = await provider.GetAgentStatusAsync("agent_1");

        // Assert
        agent.Should().NotBeNull();
        agent!.Name.Should().Be("System Agent");
    }

    [Fact]
    public async Task GetAgentStatusAsync_ReturnsNull_WhenAgentDoesNotExist()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var agent = await provider.GetAgentStatusAsync("agent_9999");

        // Assert
        agent.Should().BeNull();
    }

    // ── ProcessWebhookAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task ProcessWebhookAsync_ReturnsFailure_Always()
    {
        // Arrange - BuiltIn does not receive external webhooks
        var provider = CreateProvider();

        // Act
        var result = await provider.ProcessWebhookAsync(
            eventType: "conversation_created",
            payload: "{}",
            signature: "sha256=xxx");

        // Assert
        result.Success.Should().BeFalse();
        result.EventType.Should().Be("conversation_created");
        result.Error.Should().NotBeNullOrEmpty();
    }

    // ── HealthCheckAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task HealthCheckAsync_ReturnsHealthy_Always()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var health = await provider.HealthCheckAsync();

        // Assert
        health.IsHealthy.Should().BeTrue();
        health.ProviderName.Should().Be("BuiltIn");
    }

    [Fact]
    public async Task HealthCheckAsync_IncludesContactAndConversationCounts_InDetails()
    {
        // Arrange
        var (provider, contact) = await CreateProviderWithContactAsync();
        await provider.CreateConversationAsync(new ChatConversationCreateRequest
        {
            ContactExternalId = contact.ExternalId
        });

        // Act
        var health = await provider.HealthCheckAsync();

        // Assert
        health.Details.Should().ContainKey("contacts_count");
        health.Details.Should().ContainKey("conversations_count");
        ((int)health.Details["contacts_count"]).Should().Be(1);
        ((int)health.Details["conversations_count"]).Should().Be(1);
    }

    // ── Integration-style: full contact → conversation → message → resolve cycle ──

    [Fact]
    public async Task FullChatLifecycle_ContactConversationMessageResolve()
    {
        // Arrange
        var provider = CreateProvider();

        // Act - 1: create contact
        var contact = await provider.CreateContactAsync(new ChatContactCreateRequest
        {
            Name = "Charlie Brown",
            Email = "charlie@peanuts.com"
        });

        // Act - 2: create conversation with initial message
        var conversation = await provider.CreateConversationAsync(new ChatConversationCreateRequest
        {
            ContactExternalId = contact.ExternalId,
            Channel = "web",
            InitialMessage = "I need assistance"
        });
        // Assert initial state immediately after creation (before mutation)
        conversation.Status.Should().Be("open");

        // Act - 3: agent replies
        var agentMsg = await provider.SendMessageAsync(
            conversation.ExternalId,
            new ChatMessageCreateRequest { Content = "Sure, how can I help?" });

        // Act - 4: get all messages
        var messages = (await provider.GetMessagesAsync(conversation.ExternalId)).ToList();

        // Act - 5: resolve
        await provider.ResolveConversationAsync(conversation.ExternalId);
        var resolved = await provider.GetConversationAsync(conversation.ExternalId);

        // Assert final state
        contact.ExternalId.Should().StartWith("builtin_contact_");
        agentMsg.SenderType.Should().Be("agent");
        messages.Should().HaveCount(2); // initial contact message + agent reply
        resolved!.Status.Should().Be("resolved");
    }
}
