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

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CRM.Core.Ports.Output.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Providers.Chatwoot;

/// <summary>
/// Chatwoot implementation of IChatPort.
/// Provides live chat capabilities via Chatwoot's API.
/// Supports web chat, WhatsApp, Facebook, Twitter, SMS, and email channels.
/// </summary>
public class ChatwootProvider : IChatPort
{
    private readonly HttpClient _httpClient;
    private readonly ChatwootConfiguration _config;
    private readonly ILogger<ChatwootProvider> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public string ProviderName => "Chatwoot";

    public ChatwootProvider(
        HttpClient httpClient,
        IOptions<ChatwootConfiguration> config,
        ILogger<ChatwootProvider> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Configure JSON serialization
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };

        // Configure HttpClient
        _httpClient.BaseAddress = new Uri(_config.BaseUrl.TrimEnd('/'));
        _httpClient.DefaultRequestHeaders.Add("api_access_token", _config.ApiKey);
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
    }

    #region Availability Check

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/v1/accounts/{_config.AccountId}/agents",
                cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Chatwoot availability check failed");
            return false;
        }
    }

    #endregion

    #region Contact Management

    /// <inheritdoc />
    public async Task<ChatContact> CreateContactAsync(
        ChatContactCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Creating Chatwoot contact: {Name}", request.Name);

        var chatwootRequest = new
        {
            inbox_id = _config.DefaultInboxId ?? _config.ApiInboxId,
            name = request.Name,
            email = request.Email,
            phone_number = request.Phone,
            custom_attributes = BuildCustomAttributes(request.CrmContactId, request.CrmAccountId, request.CustomAttributes)
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"/api/v1/accounts/{_config.AccountId}/contacts",
            chatwootRequest,
            _jsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, "CreateContact", cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ChatwootContactResponse>(_jsonOptions, cancellationToken);

        return MapToContact(result!.Payload!.Contact!);
    }

    /// <inheritdoc />
    public async Task<ChatContact?> GetContactAsync(
        string externalId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalId))
        {
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/v1/accounts/{_config.AccountId}/contacts/{externalId}",
                cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            await EnsureSuccessAsync(response, "GetContact", cancellationToken);

            var result = await response.Content.ReadFromJsonAsync<ChatwootContactPayload>(_jsonOptions, cancellationToken);
            return result != null ? MapToContact(result) : null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to get Chatwoot contact: {ExternalId}", externalId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<ChatContact?> FindContactByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/v1/accounts/{_config.AccountId}/contacts/search?q={Uri.EscapeDataString(email)}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ChatwootContactSearchResponse>(_jsonOptions, cancellationToken);

            var contact = result?.Payload?.FirstOrDefault(c =>
                string.Equals(c.Email, email, StringComparison.OrdinalIgnoreCase));

            return contact != null ? MapToContact(contact) : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to search Chatwoot contact by email: {Email}", email);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<ChatContact?> FindContactByPhoneAsync(
        string phone,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        try
        {
            var normalizedPhone = NormalizePhoneNumber(phone);
            var response = await _httpClient.GetAsync(
                $"/api/v1/accounts/{_config.AccountId}/contacts/search?q={Uri.EscapeDataString(normalizedPhone)}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ChatwootContactSearchResponse>(_jsonOptions, cancellationToken);

            var contact = result?.Payload?.FirstOrDefault(c =>
                NormalizePhoneNumber(c.PhoneNumber) == normalizedPhone);

            return contact != null ? MapToContact(contact) : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to search Chatwoot contact by phone: {Phone}", phone);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task UpdateContactAsync(
        string externalId,
        ChatContactUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(externalId);
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Updating Chatwoot contact: {ExternalId}", externalId);

        var chatwootRequest = new
        {
            name = request.Name,
            email = request.Email,
            phone_number = request.Phone,
            custom_attributes = request.CustomAttributes
        };

        var response = await _httpClient.PutAsJsonAsync(
            $"/api/v1/accounts/{_config.AccountId}/contacts/{externalId}",
            chatwootRequest,
            _jsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, "UpdateContact", cancellationToken);
    }

    #endregion

    #region Conversation Management

    /// <inheritdoc />
    public async Task<ChatConversation> CreateConversationAsync(
        ChatConversationCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ContactExternalId);

        _logger.LogInformation("Creating Chatwoot conversation for contact: {ContactId}", request.ContactExternalId);

        // Get the source_id (contact identifier) from the contact
        var contact = await GetContactAsync(request.ContactExternalId, cancellationToken);
        if (contact == null)
        {
            throw new InvalidOperationException($"Contact not found: {request.ContactExternalId}");
        }

        var inboxId = _config.ApiInboxId ?? _config.DefaultInboxId;
        if (!inboxId.HasValue)
        {
            throw new InvalidOperationException("No inbox ID configured for creating conversations");
        }

        var chatwootRequest = new
        {
            source_id = contact.ExternalId,
            inbox_id = inboxId.Value,
            contact_id = int.Parse(request.ContactExternalId),
            status = "open",
            custom_attributes = request.Metadata
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"/api/v1/accounts/{_config.AccountId}/conversations",
            chatwootRequest,
            _jsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, "CreateConversation", cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ChatwootConversationPayload>(_jsonOptions, cancellationToken);

        // Send initial message if provided
        if (!string.IsNullOrWhiteSpace(request.InitialMessage) && result != null)
        {
            await SendMessageAsync(
                result.Id.ToString(),
                new ChatMessageCreateRequest { Content = request.InitialMessage },
                cancellationToken);
        }

        return MapToConversation(result!);
    }

    /// <inheritdoc />
    public async Task<ChatConversation?> GetConversationAsync(
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/v1/accounts/{_config.AccountId}/conversations/{conversationId}",
                cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            await EnsureSuccessAsync(response, "GetConversation", cancellationToken);

            var result = await response.Content.ReadFromJsonAsync<ChatwootConversationPayload>(_jsonOptions, cancellationToken);
            return result != null ? MapToConversation(result) : null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to get Chatwoot conversation: {ConversationId}", conversationId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChatConversation>> GetContactConversationsAsync(
        string contactExternalId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(contactExternalId))
        {
            return Enumerable.Empty<ChatConversation>();
        }

        try
        {
            var url = $"/api/v1/accounts/{_config.AccountId}/contacts/{contactExternalId}/conversations";

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Enumerable.Empty<ChatConversation>();
            }

            var result = await response.Content.ReadFromJsonAsync<ChatwootConversationListResponse>(_jsonOptions, cancellationToken);

            var conversations = result?.Payload ?? Enumerable.Empty<ChatwootConversationPayload>();

            if (!string.IsNullOrWhiteSpace(status))
            {
                conversations = conversations.Where(c =>
                    string.Equals(c.Status, status, StringComparison.OrdinalIgnoreCase));
            }

            return conversations.Select(MapToConversation);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get conversations for contact: {ContactId}", contactExternalId);
            return Enumerable.Empty<ChatConversation>();
        }
    }

    #endregion

    #region Messaging

    /// <inheritdoc />
    public async Task<ChatMessage> SendMessageAsync(
        string conversationId,
        ChatMessageCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conversationId);
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Sending message to Chatwoot conversation: {ConversationId}", conversationId);

        var chatwootRequest = new
        {
            content = request.Content,
            message_type = request.IsPrivate ? "outgoing" : "outgoing",
            @private = request.IsPrivate,
            content_type = request.ContentType,
            content_attributes = request.TemplateParams
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"/api/v1/accounts/{_config.AccountId}/conversations/{conversationId}/messages",
            chatwootRequest,
            _jsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, "SendMessage", cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ChatwootMessagePayload>(_jsonOptions, cancellationToken);

        return MapToMessage(result!, conversationId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(
        string conversationId,
        string? afterMessageId = null,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            return Enumerable.Empty<ChatMessage>();
        }

        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/v1/accounts/{_config.AccountId}/conversations/{conversationId}/messages",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Enumerable.Empty<ChatMessage>();
            }

            var result = await response.Content.ReadFromJsonAsync<ChatwootMessageListResponse>(_jsonOptions, cancellationToken);

            IEnumerable<ChatwootMessagePayload> messages = result?.Payload ?? Enumerable.Empty<ChatwootMessagePayload>();

            // Filter by afterMessageId if specified
            if (!string.IsNullOrWhiteSpace(afterMessageId))
            {
                var afterId = long.TryParse(afterMessageId, out var id) ? id : 0;
                messages = messages.Where(m => m.Id > afterId);
            }

            return messages
                .Take(limit)
                .Select(m => MapToMessage(m, conversationId));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get messages for conversation: {ConversationId}", conversationId);
            return Enumerable.Empty<ChatMessage>();
        }
    }

    #endregion

    #region Conversation Status

    /// <inheritdoc />
    public async Task ResolveConversationAsync(
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conversationId);

        _logger.LogInformation("Resolving Chatwoot conversation: {ConversationId}", conversationId);

        var response = await _httpClient.PostAsJsonAsync(
            $"/api/v1/accounts/{_config.AccountId}/conversations/{conversationId}/toggle_status",
            new { status = "resolved" },
            _jsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, "ResolveConversation", cancellationToken);
    }

    /// <inheritdoc />
    public async Task ReopenConversationAsync(
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conversationId);

        _logger.LogInformation("Reopening Chatwoot conversation: {ConversationId}", conversationId);

        var response = await _httpClient.PostAsJsonAsync(
            $"/api/v1/accounts/{_config.AccountId}/conversations/{conversationId}/toggle_status",
            new { status = "open" },
            _jsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, "ReopenConversation", cancellationToken);
    }

    #endregion

    #region Agent Operations

    /// <inheritdoc />
    public async Task AssignAgentAsync(
        string conversationId,
        string agentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conversationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId);

        _logger.LogInformation("Assigning agent {AgentId} to conversation: {ConversationId}", agentId, conversationId);

        var response = await _httpClient.PostAsJsonAsync(
            $"/api/v1/accounts/{_config.AccountId}/conversations/{conversationId}/assignments",
            new { assignee_id = int.Parse(agentId) },
            _jsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, "AssignAgent", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChatAgent>> GetAgentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/v1/accounts/{_config.AccountId}/agents",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Enumerable.Empty<ChatAgent>();
            }

            var result = await response.Content.ReadFromJsonAsync<List<ChatwootAgentPayload>>(_jsonOptions, cancellationToken);

            return (result ?? new List<ChatwootAgentPayload>()).Select(MapToAgent);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get Chatwoot agents");
            return Enumerable.Empty<ChatAgent>();
        }
    }

    /// <inheritdoc />
    public async Task<ChatAgent?> GetAgentStatusAsync(
        string agentId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(agentId))
        {
            return null;
        }

        try
        {
            // Chatwoot doesn't have a direct agent status endpoint
            // We get agent info and return the agent with status
            var agents = await GetAgentsAsync(cancellationToken);
            return agents.FirstOrDefault(a => a.ExternalId == agentId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get agent status: {AgentId}", agentId);
            return null;
        }
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
        // Webhook processing is handled by ChatwootWebhookController
        // This method is for providers that need internal webhook handling
        _logger.LogDebug("Chatwoot webhook received: {EventType}", eventType);

        // Validate signature if configured
        if (!string.IsNullOrEmpty(_config.WebhookSecret) && !string.IsNullOrEmpty(signature))
        {
            // Chatwoot uses token-based validation, not HMAC
            if (signature != _config.WebhookSecret)
            {
                return Task.FromResult(new ChatWebhookResult
                {
                    Success = false,
                    EventType = eventType,
                    Error = "Invalid webhook signature"
                });
            }
        }

        try
        {
            var webhookData = JsonSerializer.Deserialize<ChatwootWebhookPayload>(payload, _jsonOptions);

            return Task.FromResult(new ChatWebhookResult
            {
                Success = true,
                EventType = eventType,
                ConversationExternalId = webhookData?.Conversation?.Id.ToString(),
                ContactExternalId = webhookData?.Sender?.Id.ToString(),
                Message = webhookData?.Id.HasValue == true ? new ChatMessage
                {
                    ExternalId = webhookData.Id.Value.ToString(),
                    Content = webhookData.Content ?? string.Empty
                } : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Chatwoot webhook: {EventType}", eventType);
            return Task.FromResult(new ChatWebhookResult
            {
                Success = false,
                EventType = eventType,
                Error = ex.Message
            });
        }
    }

    #endregion

    #region Health Check

    /// <inheritdoc />
    public async Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/v1/accounts/{_config.AccountId}/agents",
                cancellationToken);

            var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            if (response.IsSuccessStatusCode)
            {
                var agents = await response.Content.ReadFromJsonAsync<List<ChatwootAgentPayload>>(_jsonOptions, cancellationToken);

                return new ProviderHealthResult
                {
                    IsHealthy = true,
                    ProviderName = ProviderName,
                    ResponseTimeMs = (long)responseTime,
                    Details = new Dictionary<string, object>
                    {
                        ["base_url"] = _config.BaseUrl,
                        ["account_id"] = _config.AccountId,
                        ["agent_count"] = agents?.Count ?? 0,
                        ["default_inbox_id"] = _config.DefaultInboxId ?? 0,
                        ["is_self_hosted"] = _config.IsSelfHosted
                    }
                };
            }

            return new ProviderHealthResult
            {
                IsHealthy = false,
                ProviderName = ProviderName,
                ResponseTimeMs = (long)responseTime,
                Message = $"Chatwoot API returned {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            return new ProviderHealthResult
            {
                IsHealthy = false,
                ProviderName = ProviderName,
                ResponseTimeMs = (long)responseTime,
                Message = ex.Message
            };
        }
    }

    #endregion

    #region Helper Methods

    private Dictionary<string, object> BuildCustomAttributes(
        int? crmContactId,
        int? crmAccountId,
        Dictionary<string, object>? additionalAttributes)
    {
        var attributes = new Dictionary<string, object>();

        if (crmContactId.HasValue)
        {
            attributes["crm_contact_id"] = crmContactId.Value;
        }

        if (crmAccountId.HasValue)
        {
            attributes["crm_account_id"] = crmAccountId.Value;
        }

        if (additionalAttributes != null)
        {
            foreach (var attr in additionalAttributes)
            {
                attributes[attr.Key] = attr.Value;
            }
        }

        return attributes;
    }

    private static string NormalizePhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return string.Empty;
        }

        return new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, string operation, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Chatwoot {Operation} failed: {StatusCode} - {Content}",
                operation, response.StatusCode, content);
            throw new HttpRequestException($"Chatwoot {operation} failed: {response.StatusCode}");
        }
    }

    #endregion

    #region Mapping Methods

    private static ChatContact MapToContact(ChatwootContactPayload payload)
    {
        return new ChatContact
        {
            ExternalId = payload.Id.ToString(),
            Email = payload.Email,
            Phone = payload.PhoneNumber,
            Name = payload.Name,
            AvatarUrl = payload.Thumbnail,
            CustomAttributes = payload.CustomAttributes,
            CreatedAt = payload.CreatedAt,
            LastActivityAt = payload.LastActivityAt
        };
    }

    private static ChatConversation MapToConversation(ChatwootConversationPayload payload)
    {
        return new ChatConversation
        {
            ExternalId = payload.Id.ToString(),
            ContactExternalId = payload.Meta?.Sender?.Id.ToString() ?? string.Empty,
            Status = payload.Status ?? "open",
            Channel = payload.Channel ?? "web",
            AssignedAgentId = payload.Meta?.Assignee?.Id.ToString(),
            AssignedAgentName = payload.Meta?.Assignee?.Name,
            CreatedAt = payload.CreatedAt,
            LastMessageAt = payload.LastActivityAt,
            MessageCount = payload.MessagesCount,
            UnreadCount = payload.UnreadCount,
            Labels = payload.Labels
        };
    }

    private static ChatMessage MapToMessage(ChatwootMessagePayload payload, string conversationId)
    {
        return new ChatMessage
        {
            ExternalId = payload.Id.ToString(),
            ConversationId = conversationId,
            Content = payload.Content ?? string.Empty,
            ContentType = payload.ContentType ?? "text",
            SenderType = MapMessageType(payload.MessageType),
            SenderId = payload.Sender?.Id.ToString(),
            SenderName = payload.Sender?.Name,
            IsPrivate = payload.Private,
            CreatedAt = payload.CreatedAt,
            Attachments = payload.Attachments?.Select(a => new ChatAttachment
            {
                Url = a.DataUrl,
                FileName = a.FileName,
                ContentType = a.FileType,
                FileSize = a.FileSize
            }).ToList()
        };
    }

    private static string MapMessageType(int messageType)
    {
        // Chatwoot message types: 0 = incoming, 1 = outgoing, 2 = activity
        return messageType switch
        {
            0 => "contact",
            1 => "agent",
            2 => "system",
            _ => "contact"
        };
    }

    private static ChatAgent MapToAgent(ChatwootAgentPayload payload)
    {
        return new ChatAgent
        {
            ExternalId = payload.Id.ToString(),
            Name = payload.Name ?? payload.Email ?? "Unknown",
            Email = payload.Email,
            AvatarUrl = payload.Thumbnail,
            Status = payload.AvailabilityStatus ?? "offline",
            ActiveConversations = null // Not available in basic agent endpoint
        };
    }

    #endregion
}

#region Chatwoot API Response DTOs

internal class ChatwootContactResponse
{
    public ChatwootContactResponsePayload? Payload { get; set; }
}

internal class ChatwootContactResponsePayload
{
    public ChatwootContactPayload? Contact { get; set; }
}

internal class ChatwootContactSearchResponse
{
    public List<ChatwootContactPayload>? Payload { get; set; }
}

internal class ChatwootContactPayload
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Name { get; set; }
    public string? Thumbnail { get; set; }
    public Dictionary<string, object>? CustomAttributes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
}

internal class ChatwootConversationListResponse
{
    public IEnumerable<ChatwootConversationPayload>? Payload { get; set; }
}

internal class ChatwootConversationPayload
{
    public int Id { get; set; }
    public string? Status { get; set; }
    public string? Channel { get; set; }
    public int MessagesCount { get; set; }
    public int UnreadCount { get; set; }
    public List<string>? Labels { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public ChatwootConversationMeta? Meta { get; set; }
}

internal class ChatwootConversationMeta
{
    public ChatwootSenderPayload? Sender { get; set; }
    public ChatwootAgentPayload? Assignee { get; set; }
}

internal class ChatwootSenderPayload
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
}

internal class ChatwootMessageListResponse
{
    public IEnumerable<ChatwootMessagePayload>? Payload { get; set; }
}

internal class ChatwootMessagePayload
{
    public int Id { get; set; }
    public string? Content { get; set; }
    public string? ContentType { get; set; }
    public int MessageType { get; set; }
    public bool Private { get; set; }
    public DateTime CreatedAt { get; set; }
    public ChatwootSenderPayload? Sender { get; set; }
    public List<ChatwootAttachmentPayload>? Attachments { get; set; }
}

internal class ChatwootAttachmentPayload
{
    public string? DataUrl { get; set; }
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public long FileSize { get; set; }
}

internal class ChatwootAgentPayload
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Thumbnail { get; set; }
    public string? AvailabilityStatus { get; set; }
    public string? Role { get; set; }
}

internal class ChatwootWebhookPayload
{
    public int? Id { get; set; }
    public string? Event { get; set; }
    public string? Content { get; set; }
    public ChatwootConversationPayload? Conversation { get; set; }
    public ChatwootSenderPayload? Sender { get; set; }
}

#endregion
