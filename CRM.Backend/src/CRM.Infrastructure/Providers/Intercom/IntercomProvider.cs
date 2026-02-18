// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CRM.Core.Ports.Output.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Providers.Intercom;

/// <summary>
/// Intercom implementation of IChatPort.
/// Provides live chat capabilities via Intercom's REST API.
/// Supports conversations, contacts, companies, and message threading.
/// </summary>
public class IntercomProvider : IChatPort
{
    private readonly HttpClient _httpClient;
    private readonly IntercomConfiguration _config;
    private readonly ILogger<IntercomProvider> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public IntercomProvider(
        HttpClient httpClient,
        IOptions<IntercomConfiguration> config,
        ILogger<IntercomProvider> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Configure JSON serialization - Intercom uses snake_case
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };

        // Configure HttpClient
        _httpClient.BaseAddress = new Uri(_config.BaseUrl.TrimEnd('/'));
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.AccessToken}");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("Intercom-Version", _config.ApiVersion);
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
    }

    public string ProviderName => "Intercom";

    #region Availability Check

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Use the "me" endpoint to verify authentication
            var response = await _httpClient.GetAsync("/me", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Intercom availability check failed");
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

        _logger.LogInformation("Creating Intercom contact: {Name}", request.Name);

        // Intercom calls contacts "Contacts" (formerly "Leads" and "Users")
        var intercomRequest = new
        {
            role = "user", // or "lead" for anonymous visitors
            email = request.Email,
            phone = request.Phone,
            name = request.Name,
            custom_attributes = BuildCustomAttributes(request.CrmContactId, request.CrmAccountId, request.CustomAttributes)
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/contacts",
            intercomRequest,
            _jsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, "CreateContact", cancellationToken);

        var intercomContact = await response.Content.ReadFromJsonAsync<IntercomContact>(_jsonOptions, cancellationToken);
        return MapToContact(intercomContact!);
    }

    /// <inheritdoc />
    public async Task<ChatContact?> GetContactAsync(string externalId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId);

        try
        {
            var response = await _httpClient.GetAsync($"/contacts/{externalId}", cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            await EnsureSuccessAsync(response, "GetContact", cancellationToken);

            var intercomContact = await response.Content.ReadFromJsonAsync<IntercomContact>(_jsonOptions, cancellationToken);
            return MapToContact(intercomContact!);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<ChatContact?> FindContactByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        _logger.LogDebug("Searching Intercom contact by email: {Email}", email);

        // Intercom search API
        var searchRequest = new
        {
            query = new
            {
                field = "email",
                @operator = "=",
                value = email
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/contacts/search",
            searchRequest,
            _jsonOptions,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Intercom contact search failed for email: {Email}", email);
            return null;
        }

        var searchResult = await response.Content.ReadFromJsonAsync<IntercomSearchResult>(_jsonOptions, cancellationToken);

        if (searchResult?.Data == null || !searchResult.Data.Any())
        {
            return null;
        }

        return MapToContact(searchResult.Data.First());
    }

    /// <inheritdoc />
    public async Task<ChatContact?> FindContactByPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);

        _logger.LogDebug("Searching Intercom contact by phone: {Phone}", phone);

        var searchRequest = new
        {
            query = new
            {
                field = "phone",
                @operator = "=",
                value = phone
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/contacts/search",
            searchRequest,
            _jsonOptions,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Intercom contact search failed for phone: {Phone}", phone);
            return null;
        }

        var searchResult = await response.Content.ReadFromJsonAsync<IntercomSearchResult>(_jsonOptions, cancellationToken);

        if (searchResult?.Data == null || !searchResult.Data.Any())
        {
            return null;
        }

        return MapToContact(searchResult.Data.First());
    }

    /// <inheritdoc />
    public async Task UpdateContactAsync(
        string externalId,
        ChatContactUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId);
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Updating Intercom contact: {ExternalId}", externalId);

        var updateRequest = new Dictionary<string, object?>();

        if (!string.IsNullOrEmpty(request.Email))
            updateRequest["email"] = request.Email;

        if (!string.IsNullOrEmpty(request.Phone))
            updateRequest["phone"] = request.Phone;

        if (!string.IsNullOrEmpty(request.Name))
            updateRequest["name"] = request.Name;

        if (request.CustomAttributes != null && request.CustomAttributes.Count > 0)
            updateRequest["custom_attributes"] = request.CustomAttributes;

        var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"/contacts/{externalId}")
        {
            Content = JsonContent.Create(updateRequest, options: _jsonOptions)
        };

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
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

        _logger.LogInformation("Creating Intercom conversation for contact: {ContactId}", request.ContactExternalId);

        // In Intercom, conversations are typically created when a contact sends a message
        // We'll create a conversation by sending an initial message as admin
        var conversationRequest = new
        {
            from = new { type = "admin", id = _config.DefaultAdminId },
            to = new { type = "contact", id = request.ContactExternalId },
            message_type = "inapp",
            body = request.InitialMessage ?? "Hello! How can we help you today?"
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/conversations",
            conversationRequest,
            _jsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, "CreateConversation", cancellationToken);

        var intercomConversation = await response.Content.ReadFromJsonAsync<IntercomConversation>(_jsonOptions, cancellationToken);
        return MapToConversation(intercomConversation!);
    }

    /// <inheritdoc />
    public async Task<ChatConversation?> GetConversationAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conversationId);

        try
        {
            var response = await _httpClient.GetAsync($"/conversations/{conversationId}", cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            await EnsureSuccessAsync(response, "GetConversation", cancellationToken);

            var intercomConversation = await response.Content.ReadFromJsonAsync<IntercomConversation>(_jsonOptions, cancellationToken);
            return MapToConversation(intercomConversation!);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChatConversation>> GetContactConversationsAsync(
        string contactExternalId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contactExternalId);

        _logger.LogDebug("Getting conversations for Intercom contact: {ContactId}", contactExternalId);

        // Search conversations by contact ID
        var searchRequest = new
        {
            query = new
            {
                field = "contact_ids",
                @operator = "=",
                value = contactExternalId
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/conversations/search",
            searchRequest,
            _jsonOptions,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to get conversations for contact: {ContactId}", contactExternalId);
            return Enumerable.Empty<ChatConversation>();
        }

        var searchResult = await response.Content.ReadFromJsonAsync<IntercomConversationSearchResult>(_jsonOptions, cancellationToken);

        if (searchResult?.Conversations == null)
        {
            return Enumerable.Empty<ChatConversation>();
        }

        var conversations = searchResult.Conversations.Select(MapToConversation);

        // Filter by status if provided
        if (!string.IsNullOrEmpty(status))
        {
            conversations = conversations.Where(c => c.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        return conversations;
    }

    /// <inheritdoc />
    public async Task<ChatMessage> SendMessageAsync(
        string conversationId,
        ChatMessageCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conversationId);
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Sending message to Intercom conversation: {ConversationId}", conversationId);

        var messageRequest = new
        {
            message_type = request.IsPrivate ? "note" : "comment",
            type = "admin",
            admin_id = _config.DefaultAdminId,
            body = request.Content
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"/conversations/{conversationId}/reply",
            messageRequest,
            _jsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, "SendMessage", cancellationToken);

        var intercomMessage = await response.Content.ReadFromJsonAsync<IntercomConversationPart>(_jsonOptions, cancellationToken);
        return MapToMessage(intercomMessage!, conversationId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(
        string conversationId,
        string? afterMessageId = null,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conversationId);

        var conversation = await GetConversationAsync(conversationId, cancellationToken);

        if (conversation?.RecentMessages == null)
        {
            // Fetch full conversation with parts
            var response = await _httpClient.GetAsync($"/conversations/{conversationId}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Enumerable.Empty<ChatMessage>();
            }

            var intercomConversation = await response.Content.ReadFromJsonAsync<IntercomConversation>(_jsonOptions, cancellationToken);

            if (intercomConversation?.ConversationParts?.Parts == null)
            {
                return Enumerable.Empty<ChatMessage>();
            }

            var messages = intercomConversation.ConversationParts.Parts
                .Select(p => MapToMessage(p, conversationId))
                .OrderBy(m => m.CreatedAt)
                .ToList();

            // Apply pagination
            if (!string.IsNullOrEmpty(afterMessageId))
            {
                var afterIndex = messages.FindIndex(m => m.ExternalId == afterMessageId);
                if (afterIndex >= 0)
                {
                    messages = messages.Skip(afterIndex + 1).ToList();
                }
            }

            return messages.Take(limit);
        }

        return conversation.RecentMessages;
    }

    /// <inheritdoc />
    public async Task ResolveConversationAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conversationId);

        _logger.LogInformation("Resolving Intercom conversation: {ConversationId}", conversationId);

        var closeRequest = new
        {
            message_type = "close",
            type = "admin",
            admin_id = _config.DefaultAdminId,
            body = "Conversation resolved"
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"/conversations/{conversationId}/reply",
            closeRequest,
            _jsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, "ResolveConversation", cancellationToken);
    }

    /// <inheritdoc />
    public async Task ReopenConversationAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conversationId);

        _logger.LogInformation("Reopening Intercom conversation: {ConversationId}", conversationId);

        var openRequest = new
        {
            message_type = "open",
            type = "admin",
            admin_id = _config.DefaultAdminId
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"/conversations/{conversationId}/reply",
            openRequest,
            _jsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, "ReopenConversation", cancellationToken);
    }

    #endregion

    #region Agent Operations

    /// <inheritdoc />
    public async Task AssignAgentAsync(
        string conversationId,
        string agentExternalId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conversationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(agentExternalId);

        _logger.LogInformation("Assigning agent {AgentId} to Intercom conversation: {ConversationId}",
            agentExternalId, conversationId);

        var assignRequest = new
        {
            message_type = "assignment",
            type = "admin",
            admin_id = _config.DefaultAdminId,
            assignee_id = agentExternalId,
            body = "Assigned to agent"
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"/conversations/{conversationId}/reply",
            assignRequest,
            _jsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, "AssignAgent", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChatAgent>> GetAgentsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting Intercom admins");

        var response = await _httpClient.GetAsync("/admins", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to get Intercom admins");
            return Enumerable.Empty<ChatAgent>();
        }

        var adminList = await response.Content.ReadFromJsonAsync<IntercomAdminList>(_jsonOptions, cancellationToken);

        if (adminList?.Admins == null)
        {
            return Enumerable.Empty<ChatAgent>();
        }

        return adminList.Admins.Select(MapToAgent);
    }

    /// <inheritdoc />
    public async Task<ChatAgent?> GetAgentStatusAsync(string agentExternalId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentExternalId);

        try
        {
            var response = await _httpClient.GetAsync($"/admins/{agentExternalId}", cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            await EnsureSuccessAsync(response, "GetAgentStatus", cancellationToken);

            var admin = await response.Content.ReadFromJsonAsync<IntercomAdmin>(_jsonOptions, cancellationToken);
            return MapToAgent(admin!);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            return null;
        }
    }

    #endregion

    #region Webhook Processing

    /// <inheritdoc />
    public async Task<ChatWebhookResult> ProcessWebhookAsync(
        string eventType,
        string payload,
        string? signature = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        _logger.LogInformation("Processing Intercom webhook event: {EventType}", eventType);

        // Validate webhook signature if configured
        if (!string.IsNullOrEmpty(_config.WebhookSecret) && !string.IsNullOrEmpty(signature))
        {
            if (!ValidateWebhookSignature(payload, signature))
            {
                _logger.LogWarning("Invalid Intercom webhook signature");
                return new ChatWebhookResult
                {
                    Success = false,
                    EventType = eventType,
                    Error = "Invalid webhook signature"
                };
            }
        }

        try
        {
            var webhookEvent = JsonSerializer.Deserialize<IntercomWebhookEvent>(payload, _jsonOptions);

            if (webhookEvent == null)
            {
                return new ChatWebhookResult
                {
                    Success = false,
                    EventType = eventType,
                    Error = "Failed to deserialize webhook payload"
                };
            }

            return await ProcessEventAsync(eventType, webhookEvent, cancellationToken);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Intercom webhook payload");
            return new ChatWebhookResult
            {
                Success = false,
                EventType = eventType,
                Error = $"JSON parse error: {ex.Message}"
            };
        }
    }

    private async Task<ChatWebhookResult> ProcessEventAsync(
        string eventType,
        IntercomWebhookEvent webhookEvent,
        CancellationToken cancellationToken)
    {
        // Intercom event types: conversation.user.created, conversation.user.replied,
        // conversation.admin.replied, conversation.admin.closed, etc.
        return eventType switch
        {
            "conversation.user.created" => await ProcessConversationCreatedAsync(webhookEvent, cancellationToken),
            "conversation.user.replied" => await ProcessUserMessageAsync(webhookEvent, cancellationToken),
            "conversation.admin.replied" => await ProcessAdminMessageAsync(webhookEvent, cancellationToken),
            "conversation.admin.closed" => ProcessConversationClosed(webhookEvent),
            "contact.created" => ProcessContactCreated(webhookEvent),
            "contact.updated" => ProcessContactUpdated(webhookEvent),
            _ => new ChatWebhookResult
            {
                Success = true,
                EventType = eventType,
                CrmActivityType = null // Not all events need CRM activities
            }
        };
    }

    private Task<ChatWebhookResult> ProcessConversationCreatedAsync(
        IntercomWebhookEvent webhookEvent,
        CancellationToken cancellationToken)
    {
        var result = new ChatWebhookResult
        {
            Success = true,
            EventType = "conversation.user.created",
            CrmActivityType = "ChatMessage",
            ConversationExternalId = webhookEvent.Data?.Item?.Id,
            ContactExternalId = webhookEvent.Data?.Item?.Source?.Author?.Id
        };

        if (webhookEvent.Data?.Item != null)
        {
            result.ActivityMapping = new ChatActivityMapping
            {
                ActivityType = "ChatMessage",
                Title = "New Chat Conversation",
                Description = webhookEvent.Data.Item.Source?.Body ?? "New conversation started",
                Channel = "intercom",
                Direction = "inbound",
                ExternalId = $"intercom:{webhookEvent.Data.Item.Id}",
                ExternalSource = "Intercom",
                ActivityDate = DateTime.UtcNow
            };
        }

        return Task.FromResult(result);
    }

    private Task<ChatWebhookResult> ProcessUserMessageAsync(
        IntercomWebhookEvent webhookEvent,
        CancellationToken cancellationToken)
    {
        var result = new ChatWebhookResult
        {
            Success = true,
            EventType = "conversation.user.replied",
            CrmActivityType = "ChatMessage",
            ConversationExternalId = webhookEvent.Data?.Item?.Id
        };

        var lastPart = webhookEvent.Data?.Item?.ConversationParts?.Parts?.LastOrDefault();
        if (lastPart != null)
        {
            result.Message = MapToMessage(lastPart, webhookEvent.Data?.Item?.Id ?? "");
            result.ContactExternalId = lastPart.Author?.Id;

            result.ActivityMapping = new ChatActivityMapping
            {
                ActivityType = "ChatMessage",
                Title = $"Chat Message from Customer",
                Description = lastPart.Body?.Length > 200 ? lastPart.Body[..200] + "..." : lastPart.Body ?? "",
                Channel = "intercom",
                Direction = "inbound",
                ExternalId = $"intercom:{lastPart.Id}",
                ExternalSource = "Intercom",
                ActivityDate = lastPart.CreatedAt > 0
                    ? DateTimeOffset.FromUnixTimeSeconds(lastPart.CreatedAt).UtcDateTime
                    : DateTime.UtcNow
            };
        }

        return Task.FromResult(result);
    }

    private Task<ChatWebhookResult> ProcessAdminMessageAsync(
        IntercomWebhookEvent webhookEvent,
        CancellationToken cancellationToken)
    {
        var result = new ChatWebhookResult
        {
            Success = true,
            EventType = "conversation.admin.replied",
            CrmActivityType = "ChatMessage",
            ConversationExternalId = webhookEvent.Data?.Item?.Id
        };

        var lastPart = webhookEvent.Data?.Item?.ConversationParts?.Parts?.LastOrDefault();
        if (lastPart != null)
        {
            result.Message = MapToMessage(lastPart, webhookEvent.Data?.Item?.Id ?? "");

            result.ActivityMapping = new ChatActivityMapping
            {
                ActivityType = "ChatMessage",
                Title = $"Chat Reply from {lastPart.Author?.Name ?? "Agent"}",
                Description = lastPart.Body?.Length > 200 ? lastPart.Body[..200] + "..." : lastPart.Body ?? "",
                Channel = "intercom",
                Direction = "outbound",
                ExternalId = $"intercom:{lastPart.Id}",
                ExternalSource = "Intercom",
                ActivityDate = lastPart.CreatedAt > 0
                    ? DateTimeOffset.FromUnixTimeSeconds(lastPart.CreatedAt).UtcDateTime
                    : DateTime.UtcNow
            };
        }

        return Task.FromResult(result);
    }

    private ChatWebhookResult ProcessConversationClosed(IntercomWebhookEvent webhookEvent)
    {
        return new ChatWebhookResult
        {
            Success = true,
            EventType = "conversation.admin.closed",
            CrmActivityType = "ChatMessage",
            ConversationExternalId = webhookEvent.Data?.Item?.Id,
            ActivityMapping = new ChatActivityMapping
            {
                ActivityType = "ChatMessage",
                Title = "Chat Conversation Resolved",
                Description = "Conversation was closed",
                Channel = "intercom",
                Direction = "outbound",
                ExternalId = $"intercom:closed:{webhookEvent.Data?.Item?.Id}",
                ExternalSource = "Intercom",
                ActivityDate = DateTime.UtcNow
            }
        };
    }

    private ChatWebhookResult ProcessContactCreated(IntercomWebhookEvent webhookEvent)
    {
        return new ChatWebhookResult
        {
            Success = true,
            EventType = "contact.created",
            ContactExternalId = webhookEvent.Data?.Item?.Id
        };
    }

    private ChatWebhookResult ProcessContactUpdated(IntercomWebhookEvent webhookEvent)
    {
        return new ChatWebhookResult
        {
            Success = true,
            EventType = "contact.updated",
            ContactExternalId = webhookEvent.Data?.Item?.Id
        };
    }

    private bool ValidateWebhookSignature(string payload, string signature)
    {
        if (string.IsNullOrEmpty(_config.WebhookSecret))
            return true;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config.WebhookSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computedSignature = Convert.ToHexString(hash).ToLowerInvariant();

        return string.Equals(computedSignature, signature.Replace("sha256=", ""), StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Health Check

    /// <inheritdoc />
    public async Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await _httpClient.GetAsync("/me", cancellationToken);
            stopwatch.Stop();

            if (response.IsSuccessStatusCode)
            {
                return new ProviderHealthResult
                {
                    IsHealthy = true,
                    ProviderName = ProviderName,
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    Details = new Dictionary<string, object>
                    {
                        { "app_id", _config.AppId },
                        { "api_version", _config.ApiVersion }
                    }
                };
            }

            return new ProviderHealthResult
            {
                IsHealthy = false,
                ProviderName = ProviderName,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                Message = $"API returned {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new ProviderHealthResult
            {
                IsHealthy = false,
                ProviderName = ProviderName,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                Message = ex.Message
            };
        }
    }

    #endregion

    #region Private Helper Methods

    private Dictionary<string, object> BuildCustomAttributes(
        int? crmContactId,
        int? crmAccountId,
        Dictionary<string, object>? additionalAttributes)
    {
        var attributes = new Dictionary<string, object>();

        if (crmContactId.HasValue)
            attributes["crm_contact_id"] = crmContactId.Value;

        if (crmAccountId.HasValue)
            attributes["crm_account_id"] = crmAccountId.Value;

        if (additionalAttributes != null)
        {
            foreach (var kvp in additionalAttributes)
            {
                attributes[kvp.Key] = kvp.Value;
            }
        }

        return attributes;
    }

    private static ChatContact MapToContact(IntercomContact intercomContact)
    {
        return new ChatContact
        {
            ExternalId = intercomContact.Id,
            Email = intercomContact.Email,
            Phone = intercomContact.Phone,
            Name = intercomContact.Name,
            AvatarUrl = intercomContact.Avatar?.ImageUrl,
            CustomAttributes = intercomContact.CustomAttributes?.ToDictionary(
                kvp => kvp.Key,
                kvp => (object)kvp.Value.ToString()),
            CreatedAt = intercomContact.CreatedAt > 0
                ? DateTimeOffset.FromUnixTimeSeconds(intercomContact.CreatedAt).UtcDateTime
                : DateTime.UtcNow,
            LastActivityAt = intercomContact.LastSeenAt > 0
                ? DateTimeOffset.FromUnixTimeSeconds(intercomContact.LastSeenAt).UtcDateTime
                : null
        };
    }

    private static ChatConversation MapToConversation(IntercomConversation intercomConversation)
    {
        return new ChatConversation
        {
            ExternalId = intercomConversation.Id,
            ContactExternalId = intercomConversation.Contacts?.Contacts?.FirstOrDefault()?.Id ?? "",
            Status = intercomConversation.State ?? "open",
            Channel = "intercom",
            AssignedAgentId = intercomConversation.Assignee?.Id,
            AssignedAgentName = intercomConversation.Assignee?.Name,
            Subject = intercomConversation.Source?.Subject,
            CreatedAt = intercomConversation.CreatedAt > 0
                ? DateTimeOffset.FromUnixTimeSeconds(intercomConversation.CreatedAt).UtcDateTime
                : DateTime.UtcNow,
            LastMessageAt = intercomConversation.UpdatedAt > 0
                ? DateTimeOffset.FromUnixTimeSeconds(intercomConversation.UpdatedAt).UtcDateTime
                : null,
            MessageCount = intercomConversation.Statistics?.CountConversationParts ?? 0,
            UnreadCount = 0, // Intercom doesn't expose this directly
            RecentMessages = intercomConversation.ConversationParts?.Parts?
                .Select(p => MapToMessage(p, intercomConversation.Id))
                .ToList(),
            Labels = intercomConversation.Tags?.Tags?.Select(t => t.Name).ToList()
        };
    }

    private static ChatMessage MapToMessage(IntercomConversationPart part, string conversationId)
    {
        return new ChatMessage
        {
            ExternalId = part.Id,
            ConversationId = conversationId,
            Content = part.Body ?? "",
            ContentType = part.PartType ?? "text",
            SenderType = part.Author?.Type == "admin" ? "agent" :
                        part.Author?.Type == "bot" ? "bot" : "contact",
            SenderId = part.Author?.Id,
            SenderName = part.Author?.Name,
            IsPrivate = part.PartType == "note",
            Attachments = part.Attachments?.Select(a => new ChatAttachment
            {
                Url = a.Url,
                FileName = a.Name,
                ContentType = a.ContentType,
                FileSize = a.FileSize
            }).ToList(),
            CreatedAt = part.CreatedAt > 0
                ? DateTimeOffset.FromUnixTimeSeconds(part.CreatedAt).UtcDateTime
                : DateTime.UtcNow
        };
    }

    private static ChatAgent MapToAgent(IntercomAdmin admin)
    {
        return new ChatAgent
        {
            ExternalId = admin.Id,
            Name = admin.Name ?? "",
            Email = admin.Email,
            AvatarUrl = admin.Avatar?.ImageUrl,
            Status = admin.Away == true ? "busy" : "online"
        };
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, string operation, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Intercom {Operation} failed: {StatusCode} - {Error}",
                operation, response.StatusCode, errorContent);

            throw new HttpRequestException(
                $"Intercom {operation} failed with status {response.StatusCode}: {errorContent}");
        }
    }

    #endregion
}

#region Intercom API Models

internal class IntercomContact
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Name { get; set; }
    public string? Role { get; set; }
    public IntercomAvatar? Avatar { get; set; }
    public Dictionary<string, JsonElement>? CustomAttributes { get; set; }
    public long CreatedAt { get; set; }
    public long LastSeenAt { get; set; }
}

internal class IntercomAvatar
{
    public string? ImageUrl { get; set; }
}

internal class IntercomSearchResult
{
    public string? Type { get; set; }
    public List<IntercomContact>? Data { get; set; }
    public int TotalCount { get; set; }
}

internal class IntercomConversation
{
    public string Id { get; set; } = string.Empty;
    public string? State { get; set; }
    public IntercomSource? Source { get; set; }
    public IntercomContacts? Contacts { get; set; }
    public IntercomAssignee? Assignee { get; set; }
    public IntercomConversationParts? ConversationParts { get; set; }
    public IntercomTags? Tags { get; set; }
    public IntercomStatistics? Statistics { get; set; }
    public long CreatedAt { get; set; }
    public long UpdatedAt { get; set; }
}

internal class IntercomSource
{
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public IntercomAuthor? Author { get; set; }
}

internal class IntercomContacts
{
    public List<IntercomContactRef>? Contacts { get; set; }
}

internal class IntercomContactRef
{
    public string Id { get; set; } = string.Empty;
}

internal class IntercomAssignee
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}

internal class IntercomConversationParts
{
    public List<IntercomConversationPart>? Parts { get; set; }
}

internal class IntercomConversationPart
{
    public string Id { get; set; } = string.Empty;
    public string? PartType { get; set; }
    public string? Body { get; set; }
    public IntercomAuthor? Author { get; set; }
    public List<IntercomAttachment>? Attachments { get; set; }
    public long CreatedAt { get; set; }
}

internal class IntercomAuthor
{
    public string? Id { get; set; }
    public string? Type { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
}

internal class IntercomAttachment
{
    public string Url { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
}

internal class IntercomTags
{
    public List<IntercomTag>? Tags { get; set; }
}

internal class IntercomTag
{
    public string Name { get; set; } = string.Empty;
}

internal class IntercomStatistics
{
    public int CountConversationParts { get; set; }
}

internal class IntercomConversationSearchResult
{
    public List<IntercomConversation>? Conversations { get; set; }
}

internal class IntercomAdmin
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public IntercomAvatar? Avatar { get; set; }
    public bool? Away { get; set; }
}

internal class IntercomAdminList
{
    public List<IntercomAdmin>? Admins { get; set; }
}

internal class IntercomWebhookEvent
{
    public string? Type { get; set; }
    public string? Topic { get; set; }
    public IntercomWebhookData? Data { get; set; }
}

internal class IntercomWebhookData
{
    public IntercomConversation? Item { get; set; }
}

#endregion
