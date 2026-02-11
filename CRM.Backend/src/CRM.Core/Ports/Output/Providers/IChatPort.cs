// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace CRM.Core.Ports.Output.Providers;

#region Chat Port Interface

/// <summary>
/// Output port for chat/messaging operations supporting pluggable chat providers.
/// Enables live chat integration with customer conversation sync to CRM timeline.
/// Implementations: BuiltIn (stub), Chatwoot, Intercom, Zendesk, Freshchat.
/// </summary>
public interface IChatPort
{
    /// <summary>
    /// Gets the unique identifier for this chat provider.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Checks if the chat provider is properly configured and available.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    #region Contact Management

    /// <summary>
    /// Creates a new contact in the external chat system.
    /// </summary>
    /// <param name="request">Contact creation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created external contact with provider-assigned ID.</returns>
    Task<ChatContact> CreateContactAsync(ChatContactCreateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a contact by their external provider ID.
    /// </summary>
    /// <param name="externalId">The provider-assigned contact ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The contact if found, null otherwise.</returns>
    Task<ChatContact?> GetContactAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a contact by email address.
    /// </summary>
    /// <param name="email">Email to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The contact if found, null otherwise.</returns>
    Task<ChatContact?> FindContactByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a contact by phone number.
    /// </summary>
    /// <param name="phone">Phone number to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The contact if found, null otherwise.</returns>
    Task<ChatContact?> FindContactByPhoneAsync(string phone, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates contact information in the external chat system.
    /// </summary>
    /// <param name="externalId">The provider-assigned contact ID.</param>
    /// <param name="request">Updated contact details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateContactAsync(string externalId, ChatContactUpdateRequest request, CancellationToken cancellationToken = default);

    #endregion

    #region Conversation Management

    /// <summary>
    /// Creates a new conversation with a contact.
    /// </summary>
    /// <param name="request">Conversation creation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created conversation.</returns>
    Task<ChatConversation> CreateConversationAsync(ChatConversationCreateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a conversation by ID.
    /// </summary>
    /// <param name="conversationId">The conversation ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The conversation if found.</returns>
    Task<ChatConversation?> GetConversationAsync(string conversationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all conversations for a contact.
    /// </summary>
    /// <param name="contactExternalId">The contact's external ID.</param>
    /// <param name="status">Optional status filter (open, resolved, pending).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of conversations.</returns>
    Task<IEnumerable<ChatConversation>> GetContactConversationsAsync(string contactExternalId, string? status = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a message in a conversation (typically from an agent).
    /// </summary>
    /// <param name="conversationId">The conversation ID.</param>
    /// <param name="request">Message details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The sent message.</returns>
    Task<ChatMessage> SendMessageAsync(string conversationId, ChatMessageCreateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets messages in a conversation.
    /// </summary>
    /// <param name="conversationId">The conversation ID.</param>
    /// <param name="afterMessageId">Optional: get messages after this ID.</param>
    /// <param name="limit">Maximum messages to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of messages.</returns>
    Task<IEnumerable<ChatMessage>> GetMessagesAsync(string conversationId, string? afterMessageId = null, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves/closes a conversation.
    /// </summary>
    /// <param name="conversationId">The conversation ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ResolveConversationAsync(string conversationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reopens a resolved conversation.
    /// </summary>
    /// <param name="conversationId">The conversation ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ReopenConversationAsync(string conversationId, CancellationToken cancellationToken = default);

    #endregion

    #region Agent Operations

    /// <summary>
    /// Assigns an agent to a conversation.
    /// </summary>
    /// <param name="conversationId">The conversation ID.</param>
    /// <param name="agentExternalId">The agent's external ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AssignAgentAsync(string conversationId, string agentExternalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available agents.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of available agents.</returns>
    Task<IEnumerable<ChatAgent>> GetAgentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets agent availability status.
    /// </summary>
    /// <param name="agentExternalId">The agent's external ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Agent with current status.</returns>
    Task<ChatAgent?> GetAgentStatusAsync(string agentExternalId, CancellationToken cancellationToken = default);

    #endregion

    #region Webhook Processing

    /// <summary>
    /// Processes an incoming webhook event from the chat provider.
    /// </summary>
    /// <param name="eventType">The type of webhook event.</param>
    /// <param name="payload">The raw JSON payload.</param>
    /// <param name="signature">Optional webhook signature for validation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Processed event that can be mapped to CRM Activity.</returns>
    Task<ChatWebhookResult> ProcessWebhookAsync(string eventType, string payload, string? signature = null, CancellationToken cancellationToken = default);

    #endregion

    /// <summary>
    /// Gets the health status of the chat provider.
    /// </summary>
    Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default);
}

#endregion

#region Chat DTOs

/// <summary>
/// A contact in the external chat system.
/// </summary>
public class ChatContact
{
    /// <summary>
    /// Provider-assigned external ID.
    /// </summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>
    /// CRM Contact ID if linked.
    /// </summary>
    public int? CrmContactId { get; set; }

    /// <summary>
    /// CRM Account ID if linked.
    /// </summary>
    public int? CrmAccountId { get; set; }

    /// <summary>
    /// Contact email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Contact phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Contact name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Avatar/profile image URL.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Custom attributes synced from CRM.
    /// </summary>
    public Dictionary<string, object>? CustomAttributes { get; set; }

    /// <summary>
    /// Contact creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last activity timestamp.
    /// </summary>
    public DateTime? LastActivityAt { get; set; }
}

/// <summary>
/// Request to create a chat contact.
/// </summary>
public class ChatContactCreateRequest
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? CrmContactId { get; set; }
    public int? CrmAccountId { get; set; }
    public Dictionary<string, object>? CustomAttributes { get; set; }
}

/// <summary>
/// Request to update a chat contact.
/// </summary>
public class ChatContactUpdateRequest
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Name { get; set; }
    public Dictionary<string, object>? CustomAttributes { get; set; }
}

/// <summary>
/// A conversation in the chat system.
/// </summary>
public class ChatConversation
{
    /// <summary>
    /// Provider-assigned conversation ID.
    /// </summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>
    /// The contact's external ID.
    /// </summary>
    public string ContactExternalId { get; set; } = string.Empty;

    /// <summary>
    /// Conversation status (open, resolved, pending, snoozed).
    /// </summary>
    public string Status { get; set; } = "open";

    /// <summary>
    /// The communication channel (web, whatsapp, facebook, twitter, email, sms).
    /// </summary>
    public string Channel { get; set; } = "web";

    /// <summary>
    /// Assigned agent ID if any.
    /// </summary>
    public string? AssignedAgentId { get; set; }

    /// <summary>
    /// Assigned agent name.
    /// </summary>
    public string? AssignedAgentName { get; set; }

    /// <summary>
    /// Conversation subject/topic if available.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Conversation creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last message timestamp.
    /// </summary>
    public DateTime? LastMessageAt { get; set; }

    /// <summary>
    /// Total message count.
    /// </summary>
    public int MessageCount { get; set; }

    /// <summary>
    /// Unread message count.
    /// </summary>
    public int UnreadCount { get; set; }

    /// <summary>
    /// Recent messages (optional, for summary).
    /// </summary>
    public IEnumerable<ChatMessage>? RecentMessages { get; set; }

    /// <summary>
    /// Custom labels/tags.
    /// </summary>
    public List<string>? Labels { get; set; }
}

/// <summary>
/// Request to create a conversation.
/// </summary>
public class ChatConversationCreateRequest
{
    public string ContactExternalId { get; set; } = string.Empty;
    public string Channel { get; set; } = "web";
    public string? Subject { get; set; }
    public string? InitialMessage { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// A chat message.
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// Provider-assigned message ID.
    /// </summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>
    /// Parent conversation ID.
    /// </summary>
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>
    /// Message content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Content type (text, template, input_select, etc.).
    /// </summary>
    public string ContentType { get; set; } = "text";

    /// <summary>
    /// Sender type: "contact", "agent", "bot".
    /// </summary>
    public string SenderType { get; set; } = "contact";

    /// <summary>
    /// Sender external ID.
    /// </summary>
    public string? SenderId { get; set; }

    /// <summary>
    /// Sender display name.
    /// </summary>
    public string? SenderName { get; set; }

    /// <summary>
    /// Whether this is a private/internal note.
    /// </summary>
    public bool IsPrivate { get; set; }

    /// <summary>
    /// Attachment URLs if any.
    /// </summary>
    public List<ChatAttachment>? Attachments { get; set; }

    /// <summary>
    /// Message timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request to create/send a message.
/// </summary>
public class ChatMessageCreateRequest
{
    public string Content { get; set; } = string.Empty;
    public string ContentType { get; set; } = "text";
    public bool IsPrivate { get; set; }
    public List<string>? AttachmentUrls { get; set; }
    public string? TemplateId { get; set; }
    public Dictionary<string, object>? TemplateParams { get; set; }
}

/// <summary>
/// A chat attachment.
/// </summary>
public class ChatAttachment
{
    public string Url { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
}

/// <summary>
/// A chat agent.
/// </summary>
public class ChatAgent
{
    public string ExternalId { get; set; } = string.Empty;
    public int? CrmUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public string Status { get; set; } = "offline"; // online, offline, busy
    public int? ActiveConversations { get; set; }
}

/// <summary>
/// Result of processing a webhook.
/// </summary>
public class ChatWebhookResult
{
    /// <summary>
    /// Whether the webhook was processed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The event type that was processed.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Mapped CRM Activity type if applicable.
    /// </summary>
    public string? CrmActivityType { get; set; }

    /// <summary>
    /// The contact external ID involved.
    /// </summary>
    public string? ContactExternalId { get; set; }

    /// <summary>
    /// The conversation external ID involved.
    /// </summary>
    public string? ConversationExternalId { get; set; }

    /// <summary>
    /// The message if this was a message event.
    /// </summary>
    public ChatMessage? Message { get; set; }

    /// <summary>
    /// CRM Activity to create (if applicable).
    /// </summary>
    public ChatActivityMapping? ActivityMapping { get; set; }

    /// <summary>
    /// Error message if processing failed.
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// Mapping data for creating a CRM Activity from a chat event.
/// </summary>
public class ChatActivityMapping
{
    /// <summary>
    /// The CRM Activity type to create.
    /// </summary>
    public string ActivityType { get; set; } = "ChatMessage";

    /// <summary>
    /// Activity title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Activity description/content.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The channel (whatsapp, facebook, web, etc.).
    /// </summary>
    public string Channel { get; set; } = "web";

    /// <summary>
    /// Direction: inbound or outbound.
    /// </summary>
    public string Direction { get; set; } = "inbound";

    /// <summary>
    /// External IDs for linking.
    /// </summary>
    public string ExternalId { get; set; } = string.Empty;
    public string ExternalSource { get; set; } = string.Empty;

    /// <summary>
    /// Matched CRM entity IDs.
    /// </summary>
    public int? ContactId { get; set; }
    public int? AccountId { get; set; }

    /// <summary>
    /// Activity timestamp.
    /// </summary>
    public DateTime ActivityDate { get; set; }

    /// <summary>
    /// Additional metadata.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

#endregion
