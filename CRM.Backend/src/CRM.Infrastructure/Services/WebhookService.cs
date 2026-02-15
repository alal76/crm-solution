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

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for handling incoming webhooks from external services
/// </summary>
public class WebhookService : IWebhookService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<WebhookService> _logger;
    private readonly IConfiguration _configuration;

    public WebhookService(
        ICrmDbContext dbContext,
        ILogger<WebhookService> logger,
        IConfiguration configuration)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc />
    public async Task<WebhookIngestResult> ProcessWebFormAsync(WebFormSubmission dto)
    {
        _logger.LogDebug("Processing web form submission: {Email}", dto.Email);

        try
        {
            // Try to find existing account/contact by email
            int? accountId = null;
            int? contactId = null;

            if (!string.IsNullOrEmpty(dto.Email))
            {
                var existingAccount = await _dbContext.Accounts
                    .FirstOrDefaultAsync(a => a.Email == dto.Email && !a.IsDeleted);

                if (existingAccount != null)
                {
                    accountId = existingAccount.Id;
                }

                var existingContact = await _dbContext.Contacts
                    .FirstOrDefaultAsync(c => c.EmailPrimary == dto.Email && c.Status == CRM.Core.Models.ContactStatus.Active);

                if (existingContact != null)
                {
                    contactId = existingContact.Id;
                }
            }

            // Create lead if no existing account found
            if (!accountId.HasValue && !string.IsNullOrEmpty(dto.Email))
            {
                var lead = new Lead
                {
                    FirstName = GetFirstName(dto.Name),
                    LastName = GetLastName(dto.Name),
                    Email = dto.Email,
                    Phone = dto.Phone,
                    Source = LeadSource.Web,
                    Status = LeadLifecycleStatus.New,
                    QualificationNotes = $"{dto.Subject}\n\n{dto.Message}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Leads.Add(lead);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Created lead {LeadId} from web form", lead.Id);
            }

            // Create interaction record
            var interaction = new Interaction
            {
                AccountId = accountId ?? 0,
                ContactId = contactId,
                InteractionType = InteractionType.WebForm,
                Direction = InteractionDirection.Inbound,
                Subject = dto.Subject ?? "Web Form Submission",
                Description = dto.Message ?? string.Empty,
                InteractionDate = DateTime.UtcNow,
                Outcome = InteractionOutcome.None,
                IsCompleted = false,
                FollowUpDate = DateTime.UtcNow.AddDays(1),
                MeetingNotes = dto.CustomFieldsJson,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Only add if we have a valid account ID
            if (accountId.HasValue)
            {
                _dbContext.Interactions.Add(interaction);
                await _dbContext.SaveChangesAsync();
            }

            return new WebhookIngestResult
            {
                Success = true,
                InteractionId = accountId.HasValue ? interaction.Id : null,
                AccountId = accountId,
                ContactId = contactId,
                Message = "Web form processed successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing web form submission");
            return new WebhookIngestResult
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    /// <inheritdoc />
    public async Task<WebhookIngestResult> ProcessInboundEmailAsync(InboundEmail dto)
    {
        _logger.LogDebug("Processing inbound email from: {From}", dto.From);

        try
        {
            // Find account/contact by email
            int? accountId = null;
            int? contactId = null;

            if (!string.IsNullOrEmpty(dto.From))
            {
                var existingAccount = await _dbContext.Accounts
                    .FirstOrDefaultAsync(a => a.Email == dto.From && !a.IsDeleted);

                if (existingAccount != null)
                {
                    accountId = existingAccount.Id;
                }

                var existingContact = await _dbContext.Contacts
                    .FirstOrDefaultAsync(c => c.EmailPrimary == dto.From && c.Status == CRM.Core.Models.ContactStatus.Active);

                if (existingContact != null)
                {
                    contactId = existingContact.Id;
                }
            }

            // Create message record - use correct CommunicationMessage properties
            var message = new CommunicationMessage
            {
                AccountId = accountId,
                ContactId = contactId,
                ChannelType = ChannelType.Email,
                Direction = MessageDirection.Inbound,
                Subject = dto.Subject,
                Body = dto.TextBody ?? dto.HtmlBody ?? string.Empty,
                HtmlBody = dto.HtmlBody,
                FromAddress = dto.From,
                FromName = dto.FromName,
                ToAddress = dto.To,
                ConversationId = dto.ConversationId,
                InReplyToExternalId = dto.InReplyTo,
                Status = MessageStatus.Delivered,
                ReceivedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Only add if we have a valid account ID
            if (accountId.HasValue)
            {
                _dbContext.CommunicationMessages.Add(message);
                await _dbContext.SaveChangesAsync();
            }

            // Create interaction record
            Interaction? interaction = null;
            if (accountId.HasValue)
            {
                interaction = new Interaction
                {
                    AccountId = accountId.Value,
                    ContactId = contactId,
                    InteractionType = InteractionType.Email,
                    Direction = InteractionDirection.Inbound,
                    Subject = dto.Subject ?? "Inbound Email",
                    Description = dto.TextBody ?? dto.HtmlBody ?? string.Empty,
                    InteractionDate = DateTime.UtcNow,
                    Outcome = InteractionOutcome.None,
                    IsCompleted = false,
                    FollowUpDate = DateTime.UtcNow.AddDays(1),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Interactions.Add(interaction);
                await _dbContext.SaveChangesAsync();
            }

            return new WebhookIngestResult
            {
                Success = true,
                InteractionId = interaction?.Id,
                MessageId = accountId.HasValue ? message.Id : null,
                AccountId = accountId,
                ContactId = contactId,
                Message = "Inbound email processed successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing inbound email");
            return new WebhookIngestResult
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    /// <inheritdoc />
    public async Task<WebhookIngestResult> ProcessWhatsAppWebhookAsync(string payload)
    {
        _logger.LogDebug("Processing WhatsApp webhook");

        try
        {
            var data = JsonSerializer.Deserialize<JsonElement>(payload);

            // Parse WhatsApp Cloud API webhook format
            // Entry -> Changes -> Value -> Messages
            if (!data.TryGetProperty("entry", out var entries))
            {
                return new WebhookIngestResult { Success = true, Message = "No entry in payload" };
            }

            foreach (var entry in entries.EnumerateArray())
            {
                if (!entry.TryGetProperty("changes", out var changes)) continue;

                foreach (var change in changes.EnumerateArray())
                {
                    if (!change.TryGetProperty("value", out var value)) continue;
                    if (!value.TryGetProperty("messages", out var messages)) continue;

                    foreach (var msg in messages.EnumerateArray())
                    {
                        var from = msg.TryGetProperty("from", out var fromProp) ? fromProp.GetString() : null;
                        var type = msg.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : "text";
                        var body = "";

                        if (type == "text" && msg.TryGetProperty("text", out var textProp))
                        {
                            body = textProp.TryGetProperty("body", out var bodyProp) ? bodyProp.GetString() : "";
                        }

                        // Find account by phone
                        int? accountId = null;
                        if (!string.IsNullOrEmpty(from))
                        {
                            var account = await _dbContext.Accounts
                                .FirstOrDefaultAsync(a => a.Phone != null && a.Phone.Contains(from) && !a.IsDeleted);

                            if (account != null)
                            {
                                accountId = account.Id;
                            }
                        }

                        // Create message record
                        if (accountId.HasValue)
                        {
                            var message = new CommunicationMessage
                            {
                                AccountId = accountId.Value,
                                ChannelType = ChannelType.WhatsApp,
                                Direction = MessageDirection.Inbound,
                                Body = body ?? string.Empty,
                                FromAddress = from,
                                Status = MessageStatus.Delivered,
                                ReceivedAt = DateTime.UtcNow,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            _dbContext.CommunicationMessages.Add(message);
                        }
                    }
                }
            }

            await _dbContext.SaveChangesAsync();

            return new WebhookIngestResult
            {
                Success = true,
                Message = "WhatsApp webhook processed"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing WhatsApp webhook");
            return new WebhookIngestResult
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    /// <inheritdoc />
    public async Task<WebhookIngestResult> ProcessFacebookWebhookAsync(string payload)
    {
        _logger.LogDebug("Processing Facebook webhook");

        try
        {
            var data = JsonSerializer.Deserialize<JsonElement>(payload);

            // Parse Facebook Messenger webhook format
            if (!data.TryGetProperty("entry", out var entries))
            {
                return new WebhookIngestResult { Success = true, Message = "No entry in payload" };
            }

            foreach (var entry in entries.EnumerateArray())
            {
                if (!entry.TryGetProperty("messaging", out var messagingEvents)) continue;

                foreach (var messagingEvent in messagingEvents.EnumerateArray())
                {
                    var senderId = messagingEvent.TryGetProperty("sender", out var sender)
                        ? (sender.TryGetProperty("id", out var idProp) ? idProp.GetString() : null)
                        : null;

                    if (messagingEvent.TryGetProperty("message", out var message))
                    {
                        var text = message.TryGetProperty("text", out var textProp) ? textProp.GetString() : "";

                        // Find customer by Facebook ID (stored in SocialAccountId)
                        if (!string.IsNullOrEmpty(senderId))
                        {
                            // Would need to query social links - simplified for now
                            _logger.LogDebug("Received Facebook message from {SenderId}: {Text}", senderId, text);
                        }
                    }
                }
            }

            await _dbContext.SaveChangesAsync();

            return new WebhookIngestResult
            {
                Success = true,
                Message = "Facebook webhook processed"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Facebook webhook");
            return new WebhookIngestResult
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    /// <inheritdoc />
    public Task<WebhookIngestResult> ProcessTwitterWebhookAsync(string payload)
    {
        _logger.LogDebug("Processing Twitter webhook");

        try
        {
            var data = JsonSerializer.Deserialize<JsonElement>(payload);

            // Parse Twitter webhook format (Account Activity API)
            if (data.TryGetProperty("direct_message_events", out var dmEvents))
            {
                foreach (var dmEvent in dmEvents.EnumerateArray())
                {
                    var senderId = dmEvent.TryGetProperty("message_create", out var msgCreate)
                        ? (msgCreate.TryGetProperty("sender_id", out var senderProp) ? senderProp.GetString() : null)
                        : null;

                    var text = msgCreate.TryGetProperty("message_data", out var msgData)
                        ? (msgData.TryGetProperty("text", out var textProp) ? textProp.GetString() : "")
                        : "";

                    _logger.LogDebug("Received Twitter DM from {SenderId}: {Text}", senderId, text);
                }
            }

            // Handle tweets/mentions
            if (data.TryGetProperty("tweet_create_events", out var tweetEvents))
            {
                foreach (var tweet in tweetEvents.EnumerateArray())
                {
                    var text = tweet.TryGetProperty("text", out var textProp) ? textProp.GetString() : "";
                    var userId = tweet.TryGetProperty("user", out var user)
                        ? (user.TryGetProperty("id_str", out var idProp) ? idProp.GetString() : null)
                        : null;

                    _logger.LogDebug("Received Twitter mention from {UserId}: {Text}", userId, text);
                }
            }

            return Task.FromResult(new WebhookIngestResult
            {
                Success = true,
                Message = "Twitter webhook processed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Twitter webhook");
            return Task.FromResult(new WebhookIngestResult
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    /// <inheritdoc />
    public Task<bool> VerifyWebhookAsync(string channelType, string signature, string payload)
    {
        _logger.LogDebug("Verifying webhook signature for {ChannelType}", channelType);

        try
        {
            var secret = channelType.ToLowerInvariant() switch
            {
                "whatsapp" => _configuration["Webhooks:WhatsApp:VerifyToken"],
                "facebook" => _configuration["Webhooks:Facebook:AppSecret"],
                "twitter" => _configuration["Webhooks:Twitter:ConsumerSecret"],
                _ => null
            };

            if (string.IsNullOrEmpty(secret))
            {
                _logger.LogWarning("No webhook secret configured for {ChannelType}", channelType);
                return Task.FromResult(false);
            }

            // Compute expected signature
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var expectedSignature = "sha256=" + BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            var isValid = string.Equals(signature, expectedSignature, StringComparison.OrdinalIgnoreCase);

            if (!isValid)
            {
                _logger.LogWarning("Webhook signature verification failed for {ChannelType}", channelType);
            }

            return Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying webhook signature for {ChannelType}", channelType);
            return Task.FromResult(false);
        }
    }

    #region Helper Methods

    private static string GetFirstName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "Unknown";
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : "Unknown";
    }

    private static string GetLastName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "Unknown";
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "Unknown";
    }

    #endregion
}
