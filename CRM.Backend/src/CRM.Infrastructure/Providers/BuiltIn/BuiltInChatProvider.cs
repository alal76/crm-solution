// CRM Solution - BuiltIn Chat Provider
// Phase 3 Week 11: Implements IChatPort as a stub/minimal implementation
// Part of the Pluggable Architecture implementation
//
// HEXAGONAL ARCHITECTURE NOTE:
// This is the BuiltIn adapter for the IChatPort output port.
// It provides a stub implementation that stores conversations in memory.
// For real chat functionality, use Chatwoot, Intercom, or Zendesk providers.

using Microsoft.Extensions.Logging;
using CRM.Core.Ports.Output.Providers;
using System.Collections.Concurrent;

namespace CRM.Infrastructure.Providers.BuiltIn;

/// <summary>
/// BuiltIn chat provider with in-memory storage for development/testing.
/// This is a stub implementation - for production use, configure Chatwoot or Intercom.
/// Does not provide real-time chat functionality or external channels.
/// </summary>
public class BuiltInChatProvider : IChatPort
{
    private readonly ILogger<BuiltInChatProvider> _logger;
    
    // In-memory storage for development/testing
    private readonly ConcurrentDictionary<string, ChatContact> _contacts = new();
    private readonly ConcurrentDictionary<string, ChatConversation> _conversations = new();
    private readonly ConcurrentDictionary<string, List<ChatMessage>> _messages = new();
    private readonly ConcurrentDictionary<string, ChatAgent> _agents = new();
    
    // Counters start at 0 because Interlocked.Increment returns the incremented value
    private int _contactIdCounter;
    private int _conversationIdCounter;
    private int _messageIdCounter;

    public BuiltInChatProvider(ILogger<BuiltInChatProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Initialize with a default agent
        var defaultAgent = new ChatAgent
        {
            ExternalId = "agent_1",
            Name = "System Agent",
            Email = "agent@crm.local",
            Status = "online"
        };
        _agents.TryAdd(defaultAgent.ExternalId, defaultAgent);
    }

    /// <inheritdoc />
    public string ProviderName => "BuiltIn";

    /// <inheritdoc />
    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        // BuiltIn provider is always available
        return Task.FromResult(true);
    }

    #region Contact Management

    /// <inheritdoc />
    public Task<ChatContact> CreateContactAsync(
        ChatContactCreateRequest request, 
        CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        
        var contactId = $"builtin_contact_{Interlocked.Increment(ref _contactIdCounter)}";
        
        var contact = new ChatContact
        {
            ExternalId = contactId,
            Email = request.Email,
            Phone = request.Phone,
            Name = request.Name,
            CrmContactId = request.CrmContactId,
            CrmAccountId = request.CrmAccountId,
            CustomAttributes = request.CustomAttributes,
            CreatedAt = DateTime.UtcNow
        };
        
        _contacts.TryAdd(contactId, contact);
        
        _logger.LogDebug("Created BuiltIn chat contact: {ContactId} for {Name}", contactId, request.Name);
        
        return Task.FromResult(contact);
    }

    /// <inheritdoc />
    public Task<ChatContact?> GetContactAsync(
        string externalId, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalId))
            throw new ArgumentException("External ID is required", nameof(externalId));
        
        _contacts.TryGetValue(externalId, out var contact);
        return Task.FromResult(contact);
    }

    /// <inheritdoc />
    public Task<ChatContact?> FindContactByEmailAsync(
        string email, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
        
        var contact = _contacts.Values
            .FirstOrDefault(c => c.Email?.Equals(email, StringComparison.OrdinalIgnoreCase) == true);
        
        return Task.FromResult(contact);
    }

    /// <inheritdoc />
    public Task<ChatContact?> FindContactByPhoneAsync(
        string phone, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone is required", nameof(phone));
        
        var normalizedPhone = NormalizePhone(phone);
        var contact = _contacts.Values
            .FirstOrDefault(c => NormalizePhone(c.Phone) == normalizedPhone);
        
        return Task.FromResult(contact);
    }

    /// <inheritdoc />
    public Task UpdateContactAsync(
        string externalId, 
        ChatContactUpdateRequest request, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalId))
            throw new ArgumentException("External ID is required", nameof(externalId));
        if (request == null) 
            throw new ArgumentNullException(nameof(request));
        
        if (_contacts.TryGetValue(externalId, out var contact))
        {
            if (request.Email != null) contact.Email = request.Email;
            if (request.Phone != null) contact.Phone = request.Phone;
            if (request.Name != null) contact.Name = request.Name;
            if (request.CustomAttributes != null)
            {
                contact.CustomAttributes = request.CustomAttributes;
            }
            contact.LastActivityAt = DateTime.UtcNow;
            
            _logger.LogDebug("Updated BuiltIn chat contact: {ContactId}", externalId);
        }
        else
        {
            _logger.LogWarning("Contact not found for update: {ContactId}", externalId);
        }
        
        return Task.CompletedTask;
    }

    #endregion

    #region Conversation Management

    /// <inheritdoc />
    public Task<ChatConversation> CreateConversationAsync(
        ChatConversationCreateRequest request, 
        CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.ContactExternalId))
            throw new ArgumentException("Contact external ID is required", nameof(request));
        
        var conversationId = $"builtin_conv_{Interlocked.Increment(ref _conversationIdCounter)}";
        
        var conversation = new ChatConversation
        {
            ExternalId = conversationId,
            ContactExternalId = request.ContactExternalId,
            Channel = request.Channel ?? "web",
            Subject = request.Subject,
            Status = "open",
            CreatedAt = DateTime.UtcNow,
            MessageCount = 0,
            UnreadCount = 0
        };
        
        _conversations.TryAdd(conversationId, conversation);
        _messages.TryAdd(conversationId, new List<ChatMessage>());
        
        // If there's an initial message, add it
        if (!string.IsNullOrWhiteSpace(request.InitialMessage))
        {
            var message = CreateMessage(conversationId, request.InitialMessage, "contact", null);
            _messages[conversationId].Add(message);
            conversation.MessageCount = 1;
            conversation.UnreadCount = 1;
            conversation.LastMessageAt = message.CreatedAt;
        }
        
        _logger.LogDebug("Created BuiltIn conversation: {ConversationId} for contact {ContactId}", 
            conversationId, request.ContactExternalId);
        
        return Task.FromResult(conversation);
    }

    /// <inheritdoc />
    public Task<ChatConversation?> GetConversationAsync(
        string conversationId, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
            throw new ArgumentException("Conversation ID is required", nameof(conversationId));
        
        _conversations.TryGetValue(conversationId, out var conversation);
        
        if (conversation != null && _messages.TryGetValue(conversationId, out var messages))
        {
            conversation.RecentMessages = messages.TakeLast(5).ToList();
        }
        
        return Task.FromResult(conversation);
    }

    /// <inheritdoc />
    public Task<IEnumerable<ChatConversation>> GetContactConversationsAsync(
        string contactExternalId, 
        string? status = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(contactExternalId))
            throw new ArgumentException("Contact external ID is required", nameof(contactExternalId));
        
        var conversations = _conversations.Values
            .Where(c => c.ContactExternalId == contactExternalId);
        
        if (!string.IsNullOrWhiteSpace(status))
        {
            conversations = conversations.Where(c => c.Status == status);
        }
        
        return Task.FromResult(conversations.OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt).AsEnumerable());
    }

    /// <inheritdoc />
    public Task<ChatMessage> SendMessageAsync(
        string conversationId, 
        ChatMessageCreateRequest request, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
            throw new ArgumentException("Conversation ID is required", nameof(conversationId));
        if (request == null) 
            throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException("Message content is required", nameof(request));
        
        if (!_conversations.TryGetValue(conversationId, out var conversation))
        {
            throw new InvalidOperationException($"Conversation not found: {conversationId}");
        }
        
        var message = CreateMessage(
            conversationId, 
            request.Content, 
            "agent", 
            "System Agent",
            request.IsPrivate,
            request.ContentType);
        
        if (!_messages.TryGetValue(conversationId, out var messages))
        {
            messages = new List<ChatMessage>();
            _messages.TryAdd(conversationId, messages);
        }
        messages.Add(message);
        
        // Update conversation stats
        conversation.MessageCount++;
        conversation.LastMessageAt = message.CreatedAt;
        
        _logger.LogDebug("Sent message in conversation {ConversationId}: {MessageId}", 
            conversationId, message.ExternalId);
        
        return Task.FromResult(message);
    }

    /// <inheritdoc />
    public Task<IEnumerable<ChatMessage>> GetMessagesAsync(
        string conversationId, 
        string? afterMessageId = null, 
        int limit = 50, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
            throw new ArgumentException("Conversation ID is required", nameof(conversationId));
        
        if (!_messages.TryGetValue(conversationId, out var messages))
        {
            return Task.FromResult(Enumerable.Empty<ChatMessage>());
        }
        
        IEnumerable<ChatMessage> result = messages;
        
        if (!string.IsNullOrWhiteSpace(afterMessageId))
        {
            var afterIndex = messages.FindIndex(m => m.ExternalId == afterMessageId);
            if (afterIndex >= 0)
            {
                result = messages.Skip(afterIndex + 1);
            }
        }
        
        return Task.FromResult(result.Take(limit).AsEnumerable());
    }

    /// <inheritdoc />
    public Task ResolveConversationAsync(
        string conversationId, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
            throw new ArgumentException("Conversation ID is required", nameof(conversationId));
        
        if (_conversations.TryGetValue(conversationId, out var conversation))
        {
            conversation.Status = "resolved";
            _logger.LogDebug("Resolved conversation: {ConversationId}", conversationId);
        }
        else
        {
            _logger.LogWarning("Conversation not found for resolution: {ConversationId}", conversationId);
        }
        
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ReopenConversationAsync(
        string conversationId, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
            throw new ArgumentException("Conversation ID is required", nameof(conversationId));
        
        if (_conversations.TryGetValue(conversationId, out var conversation))
        {
            conversation.Status = "open";
            _logger.LogDebug("Reopened conversation: {ConversationId}", conversationId);
        }
        else
        {
            _logger.LogWarning("Conversation not found for reopening: {ConversationId}", conversationId);
        }
        
        return Task.CompletedTask;
    }

    #endregion

    #region Agent Operations

    /// <inheritdoc />
    public Task AssignAgentAsync(
        string conversationId, 
        string agentExternalId, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
            throw new ArgumentException("Conversation ID is required", nameof(conversationId));
        if (string.IsNullOrWhiteSpace(agentExternalId))
            throw new ArgumentException("Agent external ID is required", nameof(agentExternalId));
        
        if (!_conversations.TryGetValue(conversationId, out var conversation))
        {
            throw new InvalidOperationException($"Conversation not found: {conversationId}");
        }
        
        if (_agents.TryGetValue(agentExternalId, out var agent))
        {
            conversation.AssignedAgentId = agentExternalId;
            conversation.AssignedAgentName = agent.Name;
            
            _logger.LogDebug("Assigned agent {AgentId} to conversation {ConversationId}", 
                agentExternalId, conversationId);
        }
        else
        {
            // Create agent on the fly
            conversation.AssignedAgentId = agentExternalId;
            _logger.LogDebug("Assigned unknown agent {AgentId} to conversation {ConversationId}", 
                agentExternalId, conversationId);
        }
        
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IEnumerable<ChatAgent>> GetAgentsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_agents.Values.AsEnumerable());
    }

    /// <inheritdoc />
    public Task<ChatAgent?> GetAgentStatusAsync(
        string agentExternalId, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(agentExternalId))
            throw new ArgumentException("Agent external ID is required", nameof(agentExternalId));
        
        _agents.TryGetValue(agentExternalId, out var agent);
        return Task.FromResult(agent);
    }

    #endregion

    #region Webhook Processing

    /// <inheritdoc />
    public Task<ChatWebhookResult> ProcessWebhookAsync(
        string eventType, 
        string payload, 
        string? signature = null, 
        CancellationToken cancellationToken = default)
    {
        // BuiltIn provider doesn't receive external webhooks
        _logger.LogWarning("BuiltIn chat provider does not support webhooks. Event: {EventType}", eventType);
        
        return Task.FromResult(new ChatWebhookResult
        {
            Success = false,
            EventType = eventType,
            Error = "BuiltIn provider does not support webhooks. Use Chatwoot or Intercom for webhook integration."
        });
    }

    #endregion

    /// <inheritdoc />
    public Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderHealthResult
        {
            IsHealthy = true,
            ProviderName = ProviderName,
            ResponseTimeMs = 0,
            Details = new Dictionary<string, object>
            {
                ["type"] = "builtin",
                ["contacts_count"] = _contacts.Count,
                ["conversations_count"] = _conversations.Count,
                ["agents_count"] = _agents.Count,
                ["note"] = "In-memory storage, for development only"
            }
        });
    }

    #region Helper Methods

    private ChatMessage CreateMessage(
        string conversationId, 
        string content, 
        string senderType, 
        string? senderName,
        bool isPrivate = false,
        string contentType = "text")
    {
        var messageId = $"builtin_msg_{Interlocked.Increment(ref _messageIdCounter)}";
        
        return new ChatMessage
        {
            ExternalId = messageId,
            ConversationId = conversationId,
            Content = content,
            ContentType = contentType,
            SenderType = senderType,
            SenderName = senderName,
            IsPrivate = isPrivate,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;
        return new string(phone.Where(char.IsDigit).ToArray());
    }

    #endregion

    #region Test Helpers (Internal use for development)

    /// <summary>
    /// Simulates a customer sending a message (for testing).
    /// </summary>
    internal ChatMessage SimulateCustomerMessage(string conversationId, string content)
    {
        if (!_conversations.TryGetValue(conversationId, out var conversation))
        {
            throw new InvalidOperationException($"Conversation not found: {conversationId}");
        }
        
        var message = CreateMessage(conversationId, content, "contact", null);
        
        if (!_messages.TryGetValue(conversationId, out var messages))
        {
            messages = new List<ChatMessage>();
            _messages.TryAdd(conversationId, messages);
        }
        messages.Add(message);
        
        conversation.MessageCount++;
        conversation.UnreadCount++;
        conversation.LastMessageAt = message.CreatedAt;
        
        return message;
    }

    /// <summary>
    /// Adds an agent to the system (for testing).
    /// </summary>
    internal void AddAgent(ChatAgent agent)
    {
        _agents.TryAdd(agent.ExternalId, agent);
    }

    /// <summary>
    /// Clears all in-memory data (for testing).
    /// </summary>
    internal void ClearAll()
    {
        _contacts.Clear();
        _conversations.Clear();
        _messages.Clear();
        _agents.Clear();
        _contactIdCounter = 0;
        _conversationIdCounter = 0;
        _messageIdCounter = 0;
    }

    #endregion
}
